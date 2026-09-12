"""Offline organic landscapes. Blender --background --threads 2 --python this_file.
Godot coordinates x,y,z map to Blender x,-z,y. No buildings are baked.
"""
import bpy, math, random, json, hashlib, struct
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'OpenMakaiRanchGame/data/world/organic_layout.json'
OUT=ROOT/'OpenMakaiRanchGame/assets/3d/organic'
OUT.mkdir(parents=True,exist_ok=True)
RAW=SOURCE.read_bytes(); SHA=hashlib.sha256(RAW).hexdigest(); LAYOUT=json.loads(RAW)
if LAYOUT.get('export_policy', {}).get('legacy_small_landscape_paused'):
 raise RuntimeError('Legacy landscape exports are paused by the coastal-region course correction. Preserve existing assets; use the reviewed regional blockout pipeline instead.')
R=random.Random(78129)
B={}; M={}
def material(name,color):
 m=bpy.data.materials.new(name); m.diffuse_color=(*color,1); m.use_nodes=True
 m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(*color,1)
 m.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.88
 M[name]=m

def add(name,verts,faces):
 v,f=B.setdefault(name,([],[])); n=len(v); v.extend((x,-z,y) for x,y,z in verts); f.extend(tuple(n+i for i in face) for face in faces)
def blob(name,x,y,z,sx,sy,sz,seed):
 # Unequal scalloped lobes, not primitive sphere instances.
 rr=random.Random(seed); n=9; rings=6; vv=[]
 phases=[rr.uniform(0,6.28) for _ in range(3)]
 for j in range(rings+1):
  phi=math.pi*(j+.06)/(rings+.12)
  for i in range(n):
   a=2*math.pi*i/n; ripple=1+.14*math.sin(3*a+phases[0])*math.sin(phi)+.1*math.cos(5*a+2*phi+phases[1])
   vv.append((x+sx*math.sin(phi)*math.cos(a)*ripple,y+sy*math.cos(phi)*ripple,z+sz*math.sin(phi)*math.sin(a)*ripple))
 ff=[]
 for j in range(rings):
  for i in range(n): ff.append((j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i))
 ff.extend([tuple(range(n-1,-1,-1)),tuple(rings*n+i for i in range(n))]); add(name,vv,ff)
def branch(a,b,r1,r2):
 a=Vector(a); b=Vector(b); d=(b-a).normalized(); u=d.cross(Vector((0,0,1)))
 if u.length<.01:u=d.cross(Vector((1,0,0)))
 u.normalize(); v=d.cross(u); verts=[]
 for p,r in [(a,r1),(b,r2)]:
  for i in range(7):verts.append(tuple(p+r*(u*math.cos(i*math.tau/7)+v*math.sin(i*math.tau/7))))
 add('Bark',verts,[(i,(i+1)%7,(i+1)%7+7,i+7) for i in range(7)]+[tuple(range(6,-1,-1)),tuple(range(7,14))])
def tree(x,z,h,base):
 lean=R.uniform(-.32,.32); branch((x,base,z),(x+lean,base+h*.69,z+.13),h*.062,h*.02)
 for k in range(7):
  a=k*2.399+R.random()*.4; reach=h*R.uniform(.22,.34); cy=base+h*R.uniform(.66,.94)
  cx=x+math.cos(a)*reach; cz=z+math.sin(a)*reach
  branch((x+lean*.5,base+h*.42,z),(cx,cy-.2,cz),h*.026,.025)
  for j in range(7):
   aa=j*2.399; rad=h*.105*math.sqrt(j/6)
   blob('Leaf_'+str((k+j)%3),cx+math.cos(aa)*rad,cy+R.uniform(-.18,.2)*h*.3,cz+math.sin(aa)*rad,h*R.uniform(.095,.145),h*R.uniform(.10,.17),h*R.uniform(.095,.15),R.randrange(999999))
def sample(points):
 pts=[points[0]]+points+[points[-1]]; out=[]
 for k in range(1,len(pts)-2):
  a,b,c,d=pts[k-1:k+3]; steps=max(4,int(math.dist(b,c)/.3))
  for i in range(steps):
   t=i/steps
   out.append([.5*((2*b[q])+(-a[q]+c[q])*t+(2*a[q]-5*b[q]+4*c[q]-d[q])*t*t+(-a[q]+3*b[q]-3*c[q]+d[q])*t*t*t) for q in range(2)])
 return out+[points[-1]]
def reset():
 global B,M
 bpy.ops.wm.read_factory_settings(use_empty=True); B={};M={}
 for n,c in {'Ground':(.31,.43,.20),'Path':(.60,.48,.32),'Bark':(.25,.17,.105),'Leaf_0':(.20,.37,.17),'Leaf_1':(.33,.48,.21),'Leaf_2':(.43,.55,.26),'Stone':(.48,.47,.36),'Grass':(.34,.46,.21),'Flower':(.80,.61,.37),'FlowerPink':(.70,.40,.45)}.items():material(n,c)
def finish_meshes():
 for name,(verts,faces) in B.items():
  if not verts:continue
  mesh=bpy.data.meshes.new(name); mesh.from_pydata(verts,[],faces); mesh.update()
  obj=bpy.data.objects.new('Terrain-col' if name=='Ground' else name,mesh); bpy.context.collection.objects.link(obj); mesh.materials.append(M[name])
  if name!='Path':
   for p in mesh.polygons:p.use_smooth=True

def setup_camera(name,pos,target):
 data=bpy.data.cameras.new(name); obj=bpy.data.objects.new(name,data); bpy.context.collection.objects.link(obj)
 obj.location=(pos[0],-pos[2],pos[1]); dest=Vector((target[0],-target[2],target[1])); obj.rotation_euler=(dest-obj.location).to_track_quat('-Z','Y').to_euler(); data.lens=24; bpy.context.scene.camera=obj; return obj

def export(name,evidence):
 finish_meshes(); scene=bpy.context.scene
 meshes=[o for o in scene.objects if o.type=='MESH']; pts=[o.matrix_world@Vector(v) for o in meshes for v in o.bound_box]
 evidence['mesh_objects']=len(meshes); evidence['vertices']=sum(len(o.data.vertices) for o in meshes); evidence['triangles']=sum(len(p.vertices)-2 for o in meshes for p in o.data.polygons)
 evidence['godot_bounds']={'min':[min(p.x for p in pts),min(p.z for p in pts),min(-p.y for p in pts)],'max':[max(p.x for p in pts),max(p.z for p in pts),max(-p.y for p in pts)]}
 bpy.ops.object.select_all(action='DESELECT')
 for o in meshes:o.select_set(True)
 bpy.ops.export_scene.gltf(filepath=str(OUT/(name+'.glb')),export_format='GLB',use_selection=True,export_cameras=False,export_lights=False,export_yup=True)
 world=bpy.data.worlds.new('Warm open sky'); world.use_nodes=True; world.node_tree.nodes['Background'].inputs[0].default_value=(.63,.74,.85,1); world.node_tree.nodes['Background'].inputs[1].default_value=.7; scene.world=world
 sun_data=bpy.data.lights.new('Afternoon sun','SUN'); sun_data.energy=2.2; sun_data.angle=.15; sun=bpy.data.objects.new('Afternoon sun',sun_data); scene.collection.objects.link(sun); sun.rotation_euler=(.5,-.35,-.55)
 scene.render.engine='CYCLES'; scene.cycles.device='CPU'; scene.cycles.samples=12; scene.cycles.use_denoising=True; scene.render.threads_mode='FIXED';scene.render.threads=2
 scene.render.resolution_x=960;scene.render.resolution_y=600;scene.render.resolution_percentage=100
 scene.view_settings.view_transform='AgX'; scene.render.image_settings.file_format='PNG'
 if name=='broadleaf':views=[('preview',(8,5,10),(0,2.7,0))]
 else:views=[('player',(0,2.05,13),(0,2,-7)),('overview',(34,38,43),(0,0,0))]
 for suffix,pos,target in views:
  setup_camera(suffix,pos,target); scene.render.filepath=str(OUT/(name+'_'+suffix+'.png')); bpy.ops.render.render(write_still=True)
 bpy.ops.wm.save_as_mainfile(filepath=str(OUT/(name+'.blend')))
 evidence['source_sha256']=SHA; (OUT/(name+'_evidence.json')).write_text(json.dumps(evidence,indent=2))
 print('EXPORT_OK',name,json.dumps({k:evidence[k] for k in ['mesh_objects','vertices','triangles','godot_bounds']}),flush=True)

def build(name,data):
 reset(); boxes=[]; paths=[]; placements=[]; failures=[]; approach_evidence={}
 for p in data['plots']:
  x,z=p['center']; w,d=p['footprint']; a=p['yaw']; f=(math.sin(a),math.cos(a)); ex=abs(math.cos(a))*w/2+abs(math.sin(a))*d/2+1; ez=abs(math.sin(a))*w/2+abs(math.cos(a))*d/2+1
  boxes.append((x-ex,z-ez,x+ex,z+ez))
  outside=[x+f[0]*(d/2+1.3),z+f[1]*(d/2+1.3)]; step=[x+f[0]*(d/2+.1),z+f[1]*(d/2+.1)]
  boxes.append((min(outside[0],step[0])-1.25,min(outside[1],step[1])-1.25,max(outside[0],step[0])+1.25,max(outside[1],step[1])+1.25))
  chain=data['approaches'][p['id']]+[outside,step]; paths.append((p['id'],1.65,sample(chain))); approach_evidence[p['id']]={'outside':outside,'doorstep':step,'control_points':chain}
 for p in data['paths']:paths.append((p['id'],p['width'],sample(p['points'])))
 # Spatial sample index allows conservative flat/canopy clearance queries.
 cells={}
 for pid,w,ps in paths:
  for x,z in ps:
   for ix in range(math.floor(x-5),math.ceil(x+5)+1):
    for iz in range(math.floor(z-5),math.ceil(z+5)+1):cells.setdefault((ix,iz),[]).append((x,z,w/2))
 gate=(-1.6,7,1.6,19.4) if name=='ranch' else (-3.2,9,3.2,15)
 def distance_box(x,z,b):return math.hypot(max(b[0]-x,0,x-b[2]),max(b[1]-z,0,z-b[3]))
 def distance(x,z,with_gate=True):
  ds=[distance_box(x,z,b) for b in boxes]
  ds.extend(math.hypot(x-a,z-b)-r for a,b,r in data['clear_points'])
  ds.extend(math.hypot(x-a,z-b)-r-.2 for a,b,r in cells.get((math.floor(x),math.floor(z)),[]))
  if with_gate:ds.append(distance_box(x,z,gate))
  return min(ds)
 hx,hz=data['half_extents']
 def ground(x,z):
  dist=distance(x,z); blend=max(0,min(1,(dist-1.2)/3)); blend=blend*blend*(3-2*blend)
  interior=.18+.17*math.sin(x*.18+z*.12)+.12*math.cos(z*.29-x*.1)
  outside=max(abs(x)-hx,abs(z)-hz,0); hills=(1-math.exp(-outside*.13))*(2.2+1.6*math.sin(x*.095+z*.067)**2)
  return blend*(max(0,interior)+hills)
 # Broad continuous terrain extends far beyond collision limits; regular large triangles.
 sx,sz=hx+24,hz+24; nx,nz=math.ceil(2*sx/.65),math.ceil(2*sz/.65)
 verts=[(-sx+2*sx*i/nx,ground(-sx+2*sx*i/nx,-sz+2*sz*j/nz),-sz+2*sz*j/nz) for j in range(nz+1) for i in range(nx+1)]
 faces=[]
 for j in range(nz):
  for i in range(nx):
   a=j*(nx+1)+i; faces.extend([(a,a+nx+2,a+1),(a,a+nx+1,a+nx+2)])
 add('Ground',verts,faces)
 for path_index,(pid,w,ps) in enumerate(paths):
  vv=[]
  relief=.003+path_index*.0008
  for i,(x,z) in enumerate(ps):
   a=ps[max(0,i-1)];b=ps[min(len(ps)-1,i+1)];dx,dz=b[0]-a[0],b[1]-a[1];length=max(.0001,math.hypot(dx,dz)); width=w/2*(1+.055*math.sin(i*.65)+.045*math.sin(i*.21))
   for side in [-1,1]:
    px=x+side*(-dz)/length*width;pz=z+side*dx/length*width;vv.append((px,relief+i*1e-7,pz))
   if i%3==0:
    for side in [-1,1]:
     px=x+side*(-dz)/length*(width+.13);pz=z+side*dx/length*(width+.13)
     # Edge grit stays low, never tall rocks on approach ribbons.
     if all(distance_box(px,pz,box)>.12 for box in boxes):blob('Stone',px,.025,pz,R.uniform(.045,.11),.025,R.uniform(.04,.09),R.randrange(99999))
  add('Path',vv,[(2*i,2*i+1,2*i+3,2*i+2) for i in range(len(ps)-1)])
 for gx,gz in data['groves']:
  accepted=0
  for attempt in range(85):
   x=gx+R.gauss(0,3.2);z=gz+R.gauss(0,3.2);h=R.uniform(3.8,6.7);radius=h*.58
   if distance(x,z)<radius+1:continue
   if any(math.hypot(x-p['x'],z-p['z'])<2.2 for p in placements if p['kind']=='tree'):continue
   tree(x,z,h,ground(x,z));placements.append({'kind':'tree','x':x,'z':z,'radius':radius,'height':h,'clearance':distance(x,z)});accepted+=1
   if accepted==5:break
 # Irregular distant groves, not rows, cover the horizon without enclosing the gate.
 for _ in range(85):
  x=R.uniform(-sx+4,sx-4);z=R.uniform(-sz+4,sz-4)
  if abs(x)<hx+4 and abs(z)<hz+4:continue
  h=R.uniform(4,7.5);radius=h*.58
  if distance(x,z)<radius+1:continue
  tree(x,z,h,ground(x,z));placements.append({'kind':'tree','x':x,'z':z,'radius':radius,'height':h,'clearance':distance(x,z)})
 for _ in range(320):
  x=R.uniform(-hx-4,hx+4);z=R.uniform(-hz-4,hz+4)
  if distance(x,z)<1.8:continue
  y=ground(x,z);radius=R.uniform(.28,.55)
  blob('Leaf_0',x,y+.22,z,radius,.29,radius,R.randrange(99999));placements.append({'kind':'shrub','x':x,'z':z,'radius':radius,'clearance':distance(x,z)})
  for k in range(6):
   px=x+R.uniform(-.8,.8);pz=z+R.uniform(-.8,.8)
   if distance(px,pz)<1.1:continue
   yy=ground(px,pz);blob('Flower' if k%2 else 'FlowerPink',px,yy+.11,pz,.055,.04,.055,R.randrange(99999))
   add('Grass',[(px-.04,yy,pz),(px,yy+.19,pz),(px+.04,yy,pz),(px,yy,pz-.04),(px,yy+.16,pz),(px,yy,pz+.04)],[(0,1,2),(3,4,5)])
 for p in placements:
  if p['clearance']<p['radius']+1:failures.append(p)
 flat_bad=sum(1 for x,y,z in verts if distance(x,z)<=1.2 and abs(y)>1e-8)
 export(name+'_landscape',{'layout_version':LAYOUT['version'],'playable_half_extents':[hx,hz],'placements':placements,'counts':{kind:sum(p['kind']==kind for p in placements) for kind in ['tree','shrub']},'approaches':approach_evidence,'paths':[{'id':pid,'width':w,'samples':len(ps)} for pid,w,ps in paths],'clearance_failures':failures,'nonzero_protected_ground_vertices':flat_bad,'path_relief_m':{'min':.003,'max':max(v[2] for v in B['Path'][0])},'collision':{'mesh':'Terrain-col','policy':'Godot documented -col: retains visual mesh, adds child StaticBody3D triangle collision','source':'https://raw.githubusercontent.com/godotengine/godot-docs/master/tutorials/assets_pipeline/importing_3d_scenes/node_type_customization.rst'},'limitations':['Godot integration/import verification belongs to parent; no buildings in these scenery assets.','Terrain extends outside existing gameplay collision boundaries.','Leaf meshes merged per material; source editable mesh islands, no armatures.']})

for name in ['ranch','town']:build(name,LAYOUT[name])
reset();tree(0,0,5.6,0);export('broadleaf',{'counts':{'tree':1},'seasonal_mesh_prefix':'Leaf_'})
assert hashlib.sha256(SOURCE.read_bytes()).hexdigest()==SHA,'Layout changed during generation'
print('LAYOUT_HASH_STABLE',SHA,flush=True)
