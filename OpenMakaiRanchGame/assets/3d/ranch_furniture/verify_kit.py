"""Independent saved-source and binary GLB verification; run using Blender -b -P."""
import bpy, json, struct, math, hashlib
import numpy as np
from pathlib import Path
from mathutils import Vector, Quaternion, Matrix
P=Path(__file__).resolve().parent
r=json.loads((P/'receipts.json').read_text())
required={'bed_standard','bed_large','chair','chair_large','bench','table','desk','wardrobe','kitchen_counter','pantry','fence_straight_2m','fence_corner_2m','fence_gate_2m','fence_post'}
assert required.issubset({a['name'] for a in r['assets']})
bpy.ops.wm.open_mainfile(filepath=str(P/'ranch_furniture.blend'))
results=[]
for a in r['assets']:
    raw=(P/a['glb']).read_bytes(); assert hashlib.sha256(raw).hexdigest()==a['sha256']
    size,typ=struct.unpack_from('<II',raw,12); g=json.loads(raw[20:20+size]); binary=raw[28+size:]
    def read_accessor(index):
        ac=g['accessors'][index]; v=g['bufferViews'][ac['bufferView']]
        dtype={5126:'<f4',5125:'<u4',5123:'<u2',5121:'u1'}[ac['componentType']]
        n={'SCALAR':1,'VEC2':2,'VEC3':3,'VEC4':4}[ac['type']]
        offset=v.get('byteOffset',0)+ac.get('byteOffset',0)
        stride=v.get('byteStride',np.dtype(dtype).itemsize*n)
        return np.ndarray((ac['count'],n),dtype=dtype,buffer=binary,offset=offset,strides=(stride,np.dtype(dtype).itemsize)).copy()
    all_pts=[]; n_bad=0; uv_bad=0; degen=0; tris=0
    def visit(i,parent):
        nonlocal_dummy=None
        node=g['nodes'][i]
        if 'matrix' in node: local=Matrix(np.array(node['matrix']).reshape(4,4).T.tolist())
        else:
            q=node.get('rotation',[0,0,0,1]); local=Matrix.LocRotScale(Vector(node.get('translation',[0,0,0])),Quaternion((q[3],q[0],q[1],q[2])),Vector(node.get('scale',[1,1,1])))
        world=parent@local
        if 'mesh' in node:
            for prim in g['meshes'][node['mesh']]['primitives']:
                pos=read_accessor(prim['attributes']['POSITION'])
                all_pts.extend([tuple(world@Vector(p)) for p in pos])
        for child in node.get('children',[]): visit(child,world)
    for node in g['scenes'][g.get('scene',0)]['nodes']: visit(node,Matrix.Identity(4))
    for mesh in g['meshes']:
        for p in mesh['primitives']:
            ns=read_accessor(p['attributes']['NORMAL']); uv=read_accessor(p['attributes']['TEXCOORD_0'])
            n_bad+=int(np.count_nonzero(~np.isfinite(ns).all(axis=1) | (abs(np.linalg.norm(ns,axis=1)-1)>.001)))
            uv_bad+=int(np.count_nonzero(~np.isfinite(uv).all(axis=1)))
            ps=read_accessor(p['attributes']['POSITION']); ix=read_accessor(p['indices']).reshape(-1,3)
            area=np.linalg.norm(np.cross(ps[ix[:,1]]-ps[ix[:,0]],ps[ix[:,2]]-ps[ix[:,0]]),axis=1)
            degen+=int(np.count_nonzero(area<1e-12)); tris+=len(ix)
    assert n_bad==0 and uv_bad==0,(a['name'],n_bad,uv_bad)
    points=np.array(all_pts); lo=points.min(axis=0); hi=points.max(axis=0)
    assert abs(lo[1])<.005,(a['name'],'not on floor',lo.tolist())
    coll=bpy.data.collections[a['name']]; root=bpy.data.objects[a['name']]
    assert all(abs(s-1)<1e-6 for s in root.scale)
    measured={}
    if a['name'].startswith('bed_'):
        mattress=next(o for o in coll.objects if o.type=='MESH' and o.name.startswith('mattress') and 'piping' not in o.name)
        verts=[root.matrix_world.inverted()@mattress.matrix_world@v.co for v in mattress.data.vertices]
        measured={'mattress_width_m':max(v.x for v in verts)-min(v.x for v in verts),'mattress_length_m':max(v.y for v in verts)-min(v.y for v in verts),'mattress_top_m':max(v.z for v in verts)}
        assert all(abs(measured[k]-value)<.002 for k,value in a['dimensions_contract'].items())
    if a['name'].startswith('fence'):
        measured['height_m']=float(hi[1]-lo[1]); assert abs(measured['height_m']-1.2)<.002
    if a['name']=='fence_gate_2m':
        posts=[o for o in coll.objects if o.type=='MESH' and o.name.startswith('post') and not o.name.startswith('post_cap')]
        boxes=sorted([(min((o.matrix_world@Vector(v)).x for v in o.bound_box),max((o.matrix_world@Vector(v)).x for v in o.bound_box)) for o in posts])
        measured['clear_opening_m']=boxes[1][0]-boxes[0][1]; assert abs(measured['clear_opening_m']-1.6)<.002
    if a['name'] in ['table','desk']:
        measured['worktop_m']=float(hi[1]); assert abs(hi[1]-.75)<.002
    names={n.get('name') for n in g['nodes']}
    for slot in a['use_slots']:
        assert len(set(slot['nodes']))==3 and set(slot['nodes']).issubset(names)
        locs=[bpy.data.objects[n].location[:] for n in slot['nodes']]
        assert len(set(locs))==3
    results.append({'asset':a['name'],'glb_bounds_godot_m':{'min':lo.tolist(),'max':hi.tolist()},'triangles':tris,'degenerate_triangle_count':degen,'non_unit_or_invalid_normals':n_bad,'invalid_uv_values':uv_bad,'measurements':measured,'complete_use_slots':len(a['use_slots'])})
assert len(results)==len(list(P.glob('*.glb')))
report={'verified_asset_count':len(results),'required_names_present':True,'source_reopened':True,'binary_glb_decoded':True,'results':results}
(P/'independent_verification.json').write_text(json.dumps(report,indent=2))
print('INDEPENDENT_KIT_PASS',len(results),'TRIANGLES',sum(x['triangles'] for x in results),'DEGENERATES',sum(x['degenerate_triangle_count'] for x in results))
