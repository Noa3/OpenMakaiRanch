"""Read-only Blender geometry verification; writes scoped asset evidence only."""
import bpy,json,math,hashlib,struct
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]; OUT=ROOT/'OpenMakaiRanchGame/assets/3d/organic'
sha=hashlib.sha256((ROOT/'OpenMakaiRanchGame/data/world/organic_layout.json').read_bytes()).hexdigest()
report={'source_sha256':sha,'assets':{}}
for name in ['ranch_landscape','town_landscape','broadleaf']:
 bpy.ops.wm.open_mainfile(filepath=str(OUT/(name+'.blend')))
 evidence=json.loads((OUT/(name+'_evidence.json')).read_text()); assert evidence['source_sha256']==sha
 binary=(OUT/(name+'.glb')).read_bytes(); length,kind=struct.unpack_from('<II',binary,12); gltf=json.loads(binary[20:20+length])
 result={'glb_bytes':len(binary),'glb_sha256':hashlib.sha256(binary).hexdigest(),'glb_meshes':len(gltf['meshes']),'glb_primitives':sum(len(m['primitives']) for m in gltf['meshes']),'glb_triangles':sum(gltf['accessors'][p['indices']]['count']//3 for m in gltf['meshes'] for p in m['primitives']),'source_mesh_vertices':sum(len(o.data.vertices) for o in bpy.context.scene.objects if o.type=='MESH')}
 assert result['glb_triangles']==evidence['triangles']; assert result['glb_meshes']==evidence['mesh_objects']
 if name!='broadleaf':
  terrain=bpy.data.objects['Terrain-col']; path=bpy.data.objects['Path']; max_gap=0
  for v in path.data.vertices:
   hit,location,normal,index=terrain.closest_point_on_mesh(Vector((v.co.x,v.co.y,0)))
   assert hit
   max_gap=max(max_gap,abs(v.co.z-location.z))
  result['max_path_ground_gap_m']=max_gap;assert max_gap<=.015
  trees=[p for p in evidence['placements'] if p['kind']=='tree']; counts=[sum((k+j)%3==idx for k in range(7) for j in range(7))*63 for idx in range(3)]
  crown_failures=[]; actual=[]
  for i,p in enumerate(trees):
   radius=0
   for idx,count in enumerate(counts):
    mesh=bpy.data.objects['Leaf_'+str(idx)].data
    for v in list(mesh.vertices)[i*count:(i+1)*count]:radius=max(radius,math.hypot(v.co.x-p['x'],-v.co.y-p['z']))
   actual.append(radius)
   if p['clearance']<radius+1:crown_failures.append({'tree_index':i,'actual_radius':radius,'clearance':p['clearance']})
  result['actual_crown_clearance_failures']=crown_failures;result['actual_crown_radii']=actual; result['terrain_vertices']=len(terrain.data.vertices);result['terrain_triangles']=sum(len(p.vertices)-2 for p in terrain.data.polygons)
  result['placement_counts']=evidence['counts'];result['bounds']=evidence['godot_bounds'];result['protected_ground_failures']=evidence['nonzero_protected_ground_vertices'];result['documented_collider_suffix_present']=any(n.get('name')=='Terrain-col' for n in gltf['nodes'])
 report['assets'][name]=result
 print('VERIFY',name,json.dumps({k:v for k,v in result.items() if k!='actual_crown_radii'}),flush=True)
(OUT/'verification.json').write_text(json.dumps(report,indent=2))
assert not any(a.get('actual_crown_clearance_failures') for a in report['assets'].values())
print('VERIFIED_LAYOUT_SHA',sha)
