"""Read-only saved-source/export verification; never executes the generator."""
import bpy, hashlib, json, struct
from pathlib import Path
ROOT = Path(__file__).resolve().parents[4]
OUT = ROOT / 'OpenMakaiRanchGame/assets/3d/okachi_market'
DOC = ROOT / 'OpenMakaiRanchGame/docs/art/okachi_market'
r = json.loads((DOC / 'geometry_receipts.json').read_text())
source = OUT / 'okachi_market.blend'
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
assert sha(source) == r['source_sha256']
assert sha(ROOT/'Tools/Blender/build_okachi_market.py') == r['generator_sha256']
assert {e['file'] for e in r['exports']} == {'canopy.glb','counter.glb','scaffold_bay.glb','material_stack.glb','barrier.glb'}
bpy.ops.wm.open_mainfile(filepath=str(source))
assert not bpy.data.libraries and not bpy.data.images
verified = []
for e in r['exports']:
    raw = (OUT / e['file']).read_bytes()
    assert hashlib.sha256(raw).hexdigest() == e['sha256']
    magic, version, length = struct.unpack_from('<III', raw)
    assert magic == 0x46546C67 and version == 2 and length == len(raw)
    n, kind = struct.unpack_from('<II', raw, 12)
    assert kind == 0x4E4F534A
    d = json.loads(raw[20:20+n]); bn, bt = struct.unpack_from('<II', raw, 20+n)
    assert bt == 0x004E4942
    binary = raw[28+n:28+n+bn]; points = []; triangles = 0
    for node in d['nodes']:
        if 'mesh' not in node: continue
        assert not any(k in node for k in ('matrix','rotation','translation'))
        assert node.get('scale',[1,1,1]) == [1,1,1]
        for prim in d['meshes'][node['mesh']]['primitives']:
            a = d['accessors'][prim['attributes']['POSITION']]; view = d['bufferViews'][a['bufferView']]
            assert a['type'] == 'VEC3' and a['componentType'] == 5126
            start = view.get('byteOffset',0) + a.get('byteOffset',0)
            points.extend(struct.unpack_from('<fff',binary,start+i*view.get('byteStride',12)) for i in range(a['count']))
            triangles += d['accessors'][prim['indices']]['count']//3
    collection = bpy.data.collections[Path(e['file']).stem]
    objects = [o for o in collection.objects if o.type == 'MESH']; saved = []
    for obj in objects:
        evaluated = obj.evaluated_get(bpy.context.evaluated_depsgraph_get()); mesh = evaluated.to_mesh()
        try:
            for v in mesh.vertices:
                p = evaluated.matrix_world @ v.co; saved.append((p.x,p.z,-p.y))
        finally: evaluated.to_mesh_clear()
    count = sum(n.get('name','').endswith('-col') for n in d['nodes'])
    assert len(objects) == len(d['meshes']) == e['mesh_count']
    assert count == e['collision_mesh_count'] and triangles == e['triangles']
    bounds = {k:[f(p[i] for p in points) for i in range(3)] for k,f in [('min',min),('max',max)]}
    for key, fn in [('min',min),('max',max)]:
        for i in range(3):
            assert abs(bounds[key][i]-fn(p[i] for p in saved)) < 1e-5
            assert abs(bounds[key][i]-e['decoded_bounds_m'][key][i]) < 1e-5
    verified.append({'file':e['file'],'sha256':e['sha256'],'bounds':bounds,'triangles':triangles,'collision_meshes':count})
assert sha(source) == r['source_sha256']
(DOC/'parent-verification.json').write_text(json.dumps({'source_sha256':sha(source),'source_unchanged':True,'verified':verified},indent=2)+'\n')
print('MARKET_PARENT_SOURCE_AND_EXPORT_PASS',len(verified))
