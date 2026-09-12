"""Read-only comparison of original GLBs and material-only derivatives."""
from pathlib import Path
import hashlib
import json
import struct
import zipfile
import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
GAME = ROOT / 'OpenMakaiRanchGame'
BASE = GAME / 'assets/3d'
OUT = BASE / 'ranch_home_textured'


def decode(path):
    raw = path.read_bytes()
    length, kind = struct.unpack_from('<II', raw, 12)
    if raw[:4] != b'glTF' or kind != 0x4e4f534a:
        raise ValueError('Invalid GLB: ' + str(path))
    doc = json.loads(raw[20:20 + length])
    binary = raw[28 + length:]

    def accessor(index):
        a = doc['accessors'][index]
        v = doc['bufferViews'][a['bufferView']]
        width = {'SCALAR': 1, 'VEC2': 2, 'VEC3': 3, 'VEC4': 4}[a['type']]
        dtype = np.dtype({5126: '<f4', 5125: '<u4', 5123: '<u2', 5121: 'u1'}[a['componentType']])
        return np.ndarray((a['count'], width), dtype=dtype, buffer=binary,
                          offset=v.get('byteOffset', 0) + a.get('byteOffset', 0),
                          strides=(v.get('byteStride', dtype.itemsize * width), dtype.itemsize)).copy()

    geometry = {}

    def visit(index, parent):
        node = doc['nodes'][index]
        if 'matrix' in node:
            local = np.array(node['matrix']).reshape(4, 4).T
        else:
            x, y, z, w = node.get('rotation', [0, 0, 0, 1])
            local = np.eye(4)
            local[:3, :3] = np.array([
                [1 - 2*y*y - 2*z*z, 2*x*y - 2*z*w, 2*x*z + 2*y*w],
                [2*x*y + 2*z*w, 1 - 2*x*x - 2*z*z, 2*y*z - 2*x*w],
                [2*x*z - 2*y*w, 2*y*z + 2*x*w, 1 - 2*x*x - 2*y*y],
            ]) @ np.diag(node.get('scale', [1, 1, 1]))
            local[:3, 3] = node.get('translation', [0, 0, 0])
        world = parent @ local
        if 'mesh' in node:
            points = []
            for primitive in doc['meshes'][node['mesh']]['primitives']:
                p = accessor(primitive['attributes']['POSITION'])
                points.extend((world @ np.column_stack([p, np.ones(len(p))]).T).T[:, :3])
            geometry[node['name']] = np.unique(np.round(np.array(points), 5), axis=0)
        for child in node.get('children', []):
            visit(child, world)

    for root in doc['scenes'][doc.get('scene', 0)]['nodes']:
        visit(root, np.eye(4))
    return doc, geometry, accessor


def verify():
    manifest = json.loads((GAME / 'assets/materials/ranch_home/material_manifest.json').read_text())
    verified_maps = 0
    for material in manifest['materials'].values():
        channels = {}
        for channel, entry in material['maps'].items():
            path = GAME / 'assets/materials/ranch_home' / entry['file']
            if hashlib.sha256(path.read_bytes()).hexdigest() != entry['sha256']:
                raise ValueError('Material map hash mismatch: ' + str(path))
            with Image.open(path) as image:
                if image.size != (512, 512):
                    raise ValueError('Material map dimensions differ: ' + str(path))
                channels[channel] = np.array(image)
            verified_maps += 1
        orm = channels['orm']
        normals = channels['normal'].astype(float) / 255 * 2 - 1
        if (not np.all(orm[:, :, 0] == 255) or not np.all(orm[:, :, 2] == 0)
                or not np.array_equal(orm[:, :, 1], channels['roughness'])
                or not np.isfinite(normals).all()
                or np.max(abs(np.linalg.norm(normals, axis=2) - 1)) > .02):
            raise ValueError('Invalid material channel contract')
    with zipfile.ZipFile(Path(__file__).parent / 'house_accent_palette.kra') as archive:
        paint_layers = archive.read('maindoc.xml').decode().count('nodetype="paintlayer"')
        if archive.testzip() is not None or paint_layers != 3:
            raise ValueError('Invalid Krita palette source')
    expected = [('ranch_house', n) for n in ['main_house', 'residential_wing', 'north_connector_cap']]
    expected += [('ranch_furniture', a['name']) for a in json.loads((BASE / 'ranch_furniture/receipts.json').read_text())['assets']]
    reports = []
    for family, name in expected:
        path = OUT / family / (name + '.glb')
        receipt = json.loads(path.with_suffix('.json').read_text())
        for source, key in [(path, 'glb_sha256'), (path.with_suffix('.blend'), 'blend_sha256'),
                            (ROOT / receipt['source_blend'], 'source_blend_sha256'),
                            (BASE / family / path.name, 'source_glb_sha256')]:
            if hashlib.sha256(source.read_bytes()).hexdigest() != receipt[key]:
                raise ValueError('Stale artifact: ' + str(source))
        for texture, digest in receipt['texture_source_hashes'].items():
            if hashlib.sha256((GAME / 'assets/materials/ranch_home' / texture).read_bytes()).hexdigest() != digest:
                raise ValueError('Texture changed after material export: ' + texture)
        _, old_geometry, _ = decode(BASE / family / path.name)
        doc, geometry, accessor = decode(path)
        if geometry.keys() != old_geometry.keys():
            raise ValueError('Mesh node set changed: ' + name)
        for node, points in geometry.items():
            previous = old_geometry[node]
            if points.shape != previous.shape or not np.allclose(points, previous, atol=.00002, rtol=0):
                raise ValueError('World-space vertex positions changed: ' + name + '/' + node)
        normal_surfaces = 0
        for mesh in doc['meshes']:
            for p in mesh['primitives']:
                material = doc['materials'][p['material']]
                if 'normalTexture' not in material:
                    continue
                normal_surfaces += 1
                tangents = accessor(p['attributes']['TANGENT'])
                normals = accessor(p['attributes']['NORMAL'])
                if (not np.isfinite(tangents).all() or not np.isfinite(normals).all()
                        or np.max(abs(np.linalg.norm(tangents[:, :3], axis=1) - 1)) > .002
                        or np.max(abs(np.linalg.norm(normals, axis=1) - 1)) > .002):
                    raise ValueError('Invalid tangent/normal basis: ' + name)
        for material in receipt['metre_uv_materials'].values():
            if material['tile_metres'] != manifest['materials'][material['stem']]['tile_metres'][0]:
                raise ValueError('Material density contract changed: ' + name)
        reports.append({'family': family, 'asset': name, 'mesh_nodes': len(geometry),
                        'normal_mapped_surfaces_with_unit_tangents': normal_surfaces,
                        'world_vertices_unchanged_20_micrometres': True})
    return {'verified_map_count': verified_maps, 'krita_paint_layer_count': paint_layers,
            'verified_asset_count': len(reports), 'checks': reports}


if __name__ == '__main__':
    print(json.dumps(verify(), indent=2))
