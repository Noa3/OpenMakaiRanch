"""Material-only, metre-UV derivatives; run one Blender process per asset.
Original sources/exports remain untouched. Does not generate game geometry.
"""
import argparse
import hashlib
import json
import struct
import sys
from pathlib import Path

import bpy

ROOT = Path(__file__).resolve().parents[2]
GAME = ROOT / 'OpenMakaiRanchGame'
MAPS = GAME / 'assets/materials/ranch_home'
OUTPUT = GAME / 'assets/3d/ranch_home_textured'
parser = argparse.ArgumentParser()
parser.add_argument('--family', required=True, choices=['ranch_house', 'ranch_furniture'])
parser.add_argument('--asset', required=True)
args = parser.parse_args(sys.argv[sys.argv.index('--') + 1:])
source_dir = GAME / 'assets/3d' / args.family
if args.family == 'ranch_house':
    allowed = {'main_house', 'residential_wing', 'north_connector_cap'}
    source = source_dir / 'ranch_house_editable.blend'
else:
    allowed = {a['name'] for a in json.loads((source_dir / 'receipts.json').read_text())['assets']}
    source = source_dir / 'ranch_furniture.blend'
if args.asset not in allowed:
    raise ValueError('Unknown source asset: ' + args.asset)
out = OUTPUT / args.family
out.mkdir(parents=True, exist_ok=True)


def glb_info(path):
    raw = path.read_bytes()
    length, kind = struct.unpack_from('<II', raw, 12)
    if raw[:4] != b'glTF' or kind != 0x4e4f534a:
        raise ValueError('Invalid GLB: ' + str(path))
    doc = json.loads(raw[20:20 + length])
    triangles = sum(doc['accessors'][p['indices']]['count'] // 3
                    for mesh in doc['meshes'] for p in mesh['primitives'])
    return raw, doc, triangles


original_bytes, original_doc, original_triangles = glb_info(source_dir / (args.asset + '.glb'))
bpy.ops.wm.open_mainfile(filepath=str(source))
collection = bpy.data.collections[args.asset]
keep = set(collection.objects)
for obj in list(bpy.data.objects):
    if obj not in keep:
        bpy.data.objects.remove(obj, do_unlink=True)
if args.family == 'ranch_furniture':
    bpy.data.objects[args.asset].location = (0, 0, 0)
elif args.asset == 'residential_wing':
    for obj in keep:
        obj.location.y -= 14
for obj in keep:
    obj.hide_set(False)
    obj.hide_render = False
collection.hide_render = False
bpy.context.view_layer.update()

# Per-material metre tiles; UVs use local surface lengths, not packed 0..1 islands.
# Preserve existing painted, metal, roof and ceramic material identities.
specs = {
    'Plaster': ('warm_lime_plaster', 2.0, (1, 1, 1, 1)),
    'Floor': ('painted_warm_oak', 1.0, (1, 1, 1, 1)),
    'Timber': ('painted_warm_oak', 1.0, (.48, .40, .33, 1)),
    'Oak_Honey': ('painted_warm_oak', 1.0, (1, 1, 1, 1)),
    'Oak_Endgrain': ('painted_warm_oak', 1.0, (.55, .45, .36, 1)),
    'Linen_Cream': ('natural_linen', .25, (1, 1, 1, 1)),
}
used = {}
texture_hashes = {}
for obj in keep:
    if obj.type != 'MESH':
        continue
    uv = obj.data.uv_layers.active
    if uv is None:
        raise ValueError('Missing source UV layer: ' + obj.name)
    # Coordinates below are mesh-local: world-axis dimensions misorient rotated braces.
    lengths = tuple(max(v.co[axis] for v in obj.data.vertices)
                     - min(v.co[axis] for v in obj.data.vertices) for axis in range(3))
    for face in obj.data.polygons:
        material = obj.data.materials[face.material_index]
        if material.name not in specs:
            continue
        stem, tile, tint = specs[material.name]
        used[material.name] = (stem, tile, tint)
        normal_axis = max(range(3), key=lambda axis: abs(face.normal[axis]))
        axes = [axis for axis in range(3) if axis != normal_axis]
        # Put grain V along the timber part's long axis. Floors follow authored Y planks.
        v_axis = max(axes, key=lambda axis: lengths[axis])
        if material.name == 'Floor' and 1 in axes:
            v_axis = 1
        u_axis = next(axis for axis in axes if axis != v_axis)
        for loop_index in face.loop_indices:
            co = obj.data.vertices[obj.data.loops[loop_index].vertex_index].co
            uv.data[loop_index].uv = (co[u_axis] * obj.scale[u_axis] / tile,
                                       co[v_axis] * obj.scale[v_axis] / tile)
    # Tangent generation rejects remaining n-gons in beveled/deformed furniture.
    # Keep the editable source mesh; triangulate only the evaluated derivative.
    obj.modifiers.new('Tangent export triangulation', 'TRIANGULATE')

for name, (stem, tile, tint) in used.items():
    material = bpy.data.materials[name]
    material.use_nodes = True
    tree = material.node_tree
    tree.nodes.clear()
    output = tree.nodes.new('ShaderNodeOutputMaterial')
    shader = tree.nodes.new('ShaderNodeBsdfPrincipled')
    shader.inputs['Metallic'].default_value = 0
    tree.links.new(shader.outputs['BSDF'], output.inputs['Surface'])
    nodes = {}
    for channel in ['albedo', 'normal', 'roughness']:
        path = MAPS / (stem + '_' + channel + '.png')
        texture_hashes[path.name] = hashlib.sha256(path.read_bytes()).hexdigest()
        image = bpy.data.images.load(str(path), check_existing=True)
        image.colorspace_settings.name = 'sRGB' if channel == 'albedo' else 'Non-Color'
        image.pack()
        node = tree.nodes.new('ShaderNodeTexImage')
        node.image = image
        node.extension = 'REPEAT'
        nodes[channel] = node
    color = nodes['albedo'].outputs['Color']
    if tint != (1, 1, 1, 1):
        multiply = tree.nodes.new('ShaderNodeMixRGB')
        multiply.blend_type = 'MULTIPLY'
        multiply.inputs[0].default_value = 1
        multiply.inputs[2].default_value = tint
        tree.links.new(color, multiply.inputs[1])
        color = multiply.outputs[0]
    tree.links.new(color, shader.inputs['Base Color'])
    tree.links.new(nodes['roughness'].outputs['Color'], shader.inputs['Roughness'])
    normal = tree.nodes.new('ShaderNodeNormalMap')
    tree.links.new(nodes['normal'].outputs['Color'], normal.inputs['Color'])
    tree.links.new(normal.outputs['Normal'], shader.inputs['Normal'])
    material['tile_metres'] = tile
    material['material_maker_source'] = stem + '.ptex'

bpy.ops.object.select_all(action='SELECT')
bpy.context.scene.unit_settings.system = 'METRIC'
bpy.context.scene.unit_settings.scale_length = 1
bpy.context.preferences.filepaths.save_version = 0
blend = out / (args.asset + '.blend')
bpy.ops.wm.save_as_mainfile(filepath=str(blend))
path = out / (args.asset + '.glb')
bpy.ops.export_scene.gltf(filepath=str(path), export_format='GLB', use_selection=True,
                          export_apply=True, export_yup=True, export_extras=True,
                          export_normals=True, export_texcoords=True, export_tangents=True)
raw, doc, triangles = glb_info(path)
if triangles != original_triangles:
    raise ValueError('Material pass changed triangle count')
original_colliders = {n['name'] for n in original_doc['nodes'] if n.get('name', '').endswith('-col')}
colliders = {n['name'] for n in doc['nodes'] if n.get('name', '').endswith('-col')}
if colliders != original_colliders:
    raise ValueError('Material pass changed collision mesh names')
textured = [m for m in doc.get('materials', []) if m.get('pbrMetallicRoughness', {}).get('baseColorTexture')]
if used and not textured:
    raise ValueError('No material textures survived GLB export')
for mesh in doc['meshes']:
    for primitive in mesh['primitives']:
        material = doc['materials'][primitive['material']]
        if 'normalTexture' in material and 'TANGENT' not in primitive['attributes']:
            raise ValueError('Normal-mapped surface exported without tangents: ' + mesh['name'])
receipt = {'family': args.family, 'asset': args.asset, 'source_blend': str(source.relative_to(ROOT)),
           'source_blend_sha256': hashlib.sha256(source.read_bytes()).hexdigest(),
           'source_glb_sha256': hashlib.sha256(original_bytes).hexdigest(),
           'glb_sha256': hashlib.sha256(raw).hexdigest(), 'blend_sha256': hashlib.sha256(blend.read_bytes()).hexdigest(),
           'triangles': triangles, 'collision_mesh_names': sorted(colliders), 'texture_source_hashes': texture_hashes,
           'metre_uv_materials': {k: {'stem': v[0], 'tile_metres': v[1]} for k, v in used.items()},
           'embedded_image_count': len(doc.get('images', [])), 'textured_material_count': len(textured),
           'scope': 'Material-only derivative, geometry and collision-name counts preserved. Original sources untouched.'}
(out / (args.asset + '.json')).write_text(json.dumps(receipt, indent=2))
print('TEXTURED_ASSET_PASS', args.family, args.asset, triangles, len(textured))
