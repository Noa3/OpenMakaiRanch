"""Read-only saved Blender/GLB verification. Run through headless Blender."""
import bpy,json,struct,hashlib
from pathlib import Path
from mathutils import Vector,Matrix,Quaternion
GAME=Path(__file__).resolve().parents[3]; OUT=GAME/'assets/3d/okachi_supply_house'; DOC=Path(__file__).parent
receipt=json.loads((DOC/'geometry_receipts.json').read_text())
bpy.ops.wm.open_mainfile(filepath=str(OUT/'okachi_supply_house.blend'))
for c in bpy.data.collections:
    if not c.library:c.hide_viewport=False
bpy.context.view_layer.update()
checked=[]
for b in receipt['collision_bodies']:
    o=bpy.data.objects[b['name']]; verts=[o.matrix_world@v.co for v in o.data.vertices]; vv=[(v.x,v.z,-v.y) for v in verts]
    low=[min(v[i] for v in vv) for i in range(3)]; high=[max(v[i] for v in vv) for i in range(3)]
    assert max(abs(a-c) for a,c in zip(low+high,b['min']+b['max']))<1e-5,b['name']
    checked.append(b['name'])
exports=[]
for e in receipt['exports']:
    raw=(OUT/e['file']).read_bytes(); assert hashlib.sha256(raw).hexdigest()==e['sha256']
    size=struct.unpack_from('<I',raw,12)[0]; d=json.loads(raw[20:20+size]); binary_start=20+size+8; binary=raw[binary_start:]
    assert [n['name'] for n in d['nodes']]==e['node_names']
    points=[]
    def visit(idx,parent):
        n=d['nodes'][idx]
        if 'matrix' in n:local=Matrix([n['matrix'][i::4] for i in range(4)])
        else:
            q=n.get('rotation',[0,0,0,1]);local=Matrix.LocRotScale(Vector(n.get('translation',[0,0,0])),Quaternion((q[3],q[0],q[1],q[2])),Vector(n.get('scale',[1,1,1])))
        world=parent@local
        if 'mesh' in n:
            for prim in d['meshes'][n['mesh']]['primitives']:
                a=d['accessors'][prim['attributes']['POSITION']]; view=d['bufferViews'][a['bufferView']]; assert a['componentType']==5126
                offset=view.get('byteOffset',0)+a.get('byteOffset',0);stride=view.get('byteStride',12)
                points.extend(world@Vector(struct.unpack_from('<fff',binary,offset+i*stride)) for i in range(a['count']))
        for child in n.get('children',[]):visit(child,world)
    for idx in d['scenes'][d.get('scene',0)]['nodes']:visit(idx,Matrix.Identity(4))
    low=[min(v[i] for v in points) for i in range(3)]; high=[max(v[i] for v in points) for i in range(3)]
    expected=e['bounds_godot_m'];assert max(abs(a-b) for a,b in zip(low+high,expected['min']+expected['max']))<1e-4
    exports.append({'file':e['file'],'decoded_vertices':len(points),'actual_bounds':{'min':low,'max':high},'hash_matches':True,'names_match':True})
assert all(lib.filepath and Path(bpy.path.abspath(lib.filepath)).exists() for lib in bpy.data.libraries)
result={'saved_blend_reopened':True,'collider_vertex_bounds_verified':len(checked),'glb_binary_verified':exports,'linked_furniture_sources_exist':True,'runtime_collision_tested':False}
(DOC/'saved_asset_verification.json').write_text(json.dumps(result,indent=2));print('SAVED_ASSET_VERIFIED',json.dumps(result))
