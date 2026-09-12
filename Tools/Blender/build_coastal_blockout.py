"""Offline modular coastal blockout. Run Blender -b --threads 2 --python this_file."""
import sys, json, math, hashlib, struct
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'Tools/WorldDesign'))
from coastal_geometry import *
import bpy
OUT=GAME/'assets/3d/coastal_region'
OUT.mkdir(parents=True,exist_ok=True)
data,sha=load()
validate_export_routes(data)  # Must pass before touching existing candidate exports.
grids=build_grids(data,sha); checks=validate(data,grids)
(GAME/'data/world/coastal_heightfields.json').write_text(json.dumps(grids,separators=(',',':')))
bpy.ops.wm.read_factory_settings(use_empty=True)
materials={}
for name,color in {'grass':(.25,.37,.19,1),'path':(.48,.36,.22,1),'water':(.055,.28,.39,1),'sand':(.65,.57,.38,1),'wood':(.29,.19,.10,1),'safety':(.6,.1,.1,1)}.items():
    m=bpy.data.materials.new(name);m.diffuse_color=color;m.use_nodes=True;m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=color;m.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.85;materials[name]=m
receipts=[]; groups={}
def mesh(name,verts,faces,mat,group):
    if not faces:return
    # Godot (x,y,z) -> Blender (x,-z,y); exporter Y-up reverses mapping.
    me=bpy.data.meshes.new(name);me.from_pydata([(x,-z,y) for x,y,z in verts],[],faces);me.update()
    obj=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(obj);me.materials.append(materials[mat]);groups.setdefault(group,[]).append(obj)
    obj['source_sha256']=sha;return obj

def box(name,x,y,z,w,h,d,group,mat='wood'):
    vs=[(x+dx*w/2,y+dy*h/2,z+dz*d/2) for dx,dy,dz in [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]]
    return mesh(name,vs,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(3,7,6,2),(0,4,7,3),(1,2,6,5)],mat,group)

def barrier(name,a,b,grid,group,height=2.3):
    # Vertical double-sided collision wall, bottom anchored below terrain.
    ya=sample(grid,*a)-.6;yb=sample(grid,*b)-.6
    mesh(name,[(a[0],ya,a[1]),(b[0],yb,b[1]),(b[0],yb+height,b[1]),(a[0],ya+height,a[1])],[(0,1,2),(0,2,3),(2,1,0),(3,2,0)],'safety',group)

def clip(poly,a,b):
    def side(p):return (b[0]-a[0])*(p[2]-a[1])-(b[1]-a[1])*(p[0]-a[0])
    out=[]
    for p,q in zip(poly,poly[1:]+poly[:1]):
        sp,sq=side(p),side(q)
        if sp>=-1e-9:out.append(p)
        if (sp>=0)!=(sq>=0):
            t=sp/(sp-sq);out.append(tuple(p[k]+t*(q[k]-p[k]) for k in range(3)))
    return out

for area,g in grids['areas'].items():
    ox,oz,w,d=g['bounds'];c=g['columns'];h=g['heights']; rr=routes(area,data)
    # Tiles retain independently editable names and triangle collision.
    for tz in range(0,d,24):
        for tx in range(0,w,24):
            tw=min(24,w-tx);td=min(24,d-tz);vs=[];fs=[]
            for j in range(td+1):
                for i in range(tw+1):vs.append((ox+tx+i,h[(tz+j)*c+tx+i],oz+tz+j))
            for j in range(td):
                for i in range(tw):
                    a=j*(tw+1)+i;b=a+1;cc=a+tw+1;dd=cc+1;fs.extend([(a,dd,b),(a,cc,dd)])
            mesh(f'{area}_tile_{tx:03}_{tz:03}-col',vs,fs,'grass',area+'_terrain')
    # Clip each path rectangle against exact grid triangles: all overlay vertices
    # lie on the shared triangulated ground, including full-width edges.
    for rn,width,pts in rr:
        vs=[];fs=[]
        for segment,(a,b) in enumerate(zip(pts,pts[1:])):
            dx,dz=b[0]-a[0],b[2]-a[2];length=math.hypot(dx,dz)
            if length<1e-8:continue
            nx,nz=-dz/length*width/2,dx/length*width/2
            poly=[(a[0]-nx,a[2]-nz),(b[0]-nx,b[2]-nz),(b[0]+nx,b[2]+nz),(a[0]+nx,a[2]+nz)]
            for j in range(max(0,math.floor(min(p[1] for p in poly)-oz)),min(d,math.ceil(max(p[1] for p in poly)-oz))):
                for i in range(max(0,math.floor(min(p[0] for p in poly)-ox)),min(w,math.ceil(max(p[0] for p in poly)-ox))):
                    p=[(ox+i,h[j*c+i],oz+j),(ox+i+1,h[j*c+i+1],oz+j),(ox+i+1,h[(j+1)*c+i+1],oz+j+1),(ox+i,h[(j+1)*c+i],oz+j+1)]
                    for tri in [[p[0],p[1],p[2]],[p[0],p[2],p[3]]]:
                        clipped=tri
                        for aa,bb in zip(poly,poly[1:]+poly[:1]):
                            if clipped:clipped=clip(clipped,aa,bb)
                        if len(clipped)>=3:
                            start=len(vs);vs.extend((x,y+.018,z) for x,y,z in clipped)
                            fs.extend((start,start+k+1,start+k) for k in range(1,len(clipped)-1))
        mesh(area+'_'+rn,vs,fs,'path',area+'_paths')
    # Exact authored walkable polygon walls, densely ground-following.
    poly=data['region']['areas'][area]['walkable_boundary']
    for edge,(a,b) in enumerate(zip(poly,poly[1:]+poly[:1])):
        n=max(1,math.ceil(math.dist(a,b)))
        for k in range(n):
            aa=[a[i]+(b[i]-a[i])*k/n for i in range(2)];bb=[a[i]+(b[i]-a[i])*(k+1)/n for i in range(2)]
            barrier(f'{area}_boundary_{edge}_{k}-colonly',aa,bb,g,area+'_safety')
    stream=[local(area,p,data) for p in data['region']['stream']['points']];widths=data['region']['stream']['widths'];vs=[];fs=[]
    for si,(a,b) in enumerate(zip(stream,stream[1:])):
        dx,dz=b[0]-a[0],b[2]-a[2];length=math.hypot(dx,dz);nx,nz=-dz/length,dx/length;n=math.ceil(length/.5)
        for k in range(n):
            ts=[k/n,(k+1)/n];pp=[]
            for t in ts:
                x,y,z=[a[j]+t*(b[j]-a[j]) for j in range(3)];half=(widths[si]*(1-t)+widths[si+1]*t)/2
                pp.append([(x-nx*half,y,z-nz*half),(x+nx*half,y,z+nz*half)])
            mid=[sum(p[j] for pair in pp for p in pair)/4 for j in range(3)]
            if not(ox<=mid[0]<=ox+w and oz<=mid[2]<=oz+d):continue
            q=len(vs);vs.extend([pp[0][0],pp[1][0],pp[1][1],pp[0][1]]);fs.extend([(q,q+2,q+1),(q,q+3,q+2)])
            for side in [0,1]:
                aa,bb=pp[0][side],pp[1][side]
                # Actual bridge opening in both stream bank colliders.
                if area=='ranch' and 22.2-.4<=mid[2]<=25.8+.4:continue
                barrier(f'{area}_bank_{si}_{k}_{side}-colonly',[aa[0],aa[2]],[bb[0],bb[2]],g,area+'_safety',3.2)
    mesh(area+'_stream',vs,fs,'water',area+'_water')
    if area=='town':
        shore=data['region']['coast']['shoreline'];vs=[];fs=[]
        for a,b in zip(shore,shore[1:]):
            q=len(vs);vs.extend([(-500,-4,a[2]),a,b,(-500,-4,b[2])]);fs.extend([(q,q+2,q+1),(q,q+3,q+2)])
        mesh('town_bay_water',vs,fs,'water','town_water')
        # Shore barrier except the guarded fixed pier opening.
        for si,(a,b) in enumerate(zip(shore,shore[1:])):
            n=math.ceil(math.dist(a,b))
            for k in range(n):
                aa=[a[j]+(b[j]-a[j])*k/n for j in range(3)];bb=[a[j]+(b[j]-a[j])*(k+1)/n for j in range(3)]
                if -26.2<=aa[2]<=-21.8:continue
                barrier(f'town_shore_{si}_{k}-colonly',[aa[0]+2,aa[2]],[bb[0]+2,bb[2]],g,'town_safety',3.5)

bridge=data['region']['ranch_side_route']['bridge'];x,z,w,d=bridge['deck_bounds'];y=bridge['deck_elevation']-12
box('ranch_footbridge_deck-col',x+w/2,y-.15,z+d/2,w,.3,d,'ranch_bridge')
for side in [z,z+d]:box('bridge_rail_'+str(side)+'-col',x+w/2,y+.65,side,w,1.3,.15,'ranch_bridge')
pier=data['region']['coast']['pier'];x,z,w,d=pier['deck_bounds'];y=pier['deck_elevation']
box('town_pier_deck-col',x+w/2,y-.15,z+d/2,w,.3,d,'town_pier')
# Short approach deck spans the specified ramp-start to fixed pier.
box('town_pier_landing-col',-52,y-.15,-24,4,.3,4,'town_pier')
for side in [z,z+d]:box('pier_rail_'+str(side)+'-col',x+w/2,y+.65,side,w,1.3,.15,'town_pier')
box('pier_seaward_guard-col',x,y+.65,z+d/2,.18,1.3,d,'town_pier')
for px in [-70,-64,-58]:
    for pz in [-25.5,-22.5]:box('pier_pile_'+str(px)+'_'+str(pz),px,-3.9,pz,.3,2.8,.3,'town_pier')

# Source combines modular collections; exported GLBs only contain their group.
for group,objects in groups.items():
    collection=bpy.data.collections.new(group);bpy.context.scene.collection.children.link(collection)
    for o in objects:
        for old in list(o.users_collection):old.objects.unlink(o)
        collection.objects.link(o)
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects:o.select_set(True)
    path=OUT/(group+'.glb')
    bpy.ops.export_scene.gltf(filepath=str(path),export_format='GLB',use_selection=True,export_yup=True,export_extras=True)
    raw=path.read_bytes();size,kind=struct.unpack_from('<II',raw,12);doc=json.loads(raw[20:20+size]);names=[n.get('name','') for n in doc['nodes']]
    assert len(doc.get('meshes',[]))==len(objects),(group,'mesh count')
    if group.endswith('terrain'):assert all(n.endswith('-col') for n in names)
    if group.endswith('safety'):assert all(n.endswith('-colonly') for n in names)
    receipts.append({'module':group+'.glb','sha256':hashlib.sha256(raw).hexdigest(),'bytes':len(raw),'meshes':len(doc['meshes']),'vertices':sum(len(o.data.vertices) for o in objects),'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in objects)})
    print('EXPORT_OK',group,len(objects),flush=True)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'coastal_region.blend'))
receipt={'version':1,'stage':'blockout','source_sha256':sha,'checks':checks,'modules':receipts,'coordinate_mapping':'Godot XYZ -> Blender X,-Z,Y; GLB Y-up','path_surface_offset':.018,'limitations':['No player traversal or Godot import verified by generator','No art preview: parent live Godot capture required','Terrain grid extends beyond exact polygon; collision-only polygon walls constrain play','Blockout straight-segment paths, not final curved landscaping']}
(OUT/'generation_receipt.json').write_text(json.dumps(receipt,indent=2))
print('COASTAL_BLOCKOUT_OK',len(receipts),flush=True)
