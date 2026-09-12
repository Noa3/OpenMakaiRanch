"""Bounded editable civic supply asset. Headless Blender, no engine/shared-data writes.
All helper coordinates are Godot XYZ metres; Blender receives (x,-z,y).
Furniture is linked for preview only; exports contain architecture and sockets.
"""
import bpy, json, math, struct, hashlib, shutil
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]; GAME=ROOT/'OpenMakaiRanchGame'
OUT=GAME/'assets/3d/okachi_supply_house'; DOC=GAME/'docs/art/okachi_supply_house'
OUT.mkdir(parents=True,exist_ok=True); DOC.mkdir(parents=True,exist_ok=True)
if (OUT/'okachi_supply_house.blend').exists():
    archive=OUT/'iteration_1'; archive.mkdir(exist_ok=True)
    for p in list(OUT.glob('*'))+list(DOC.glob('*.png')):
        if p.is_file(): shutil.copy2(p,archive/p.name)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene; scene.unit_settings.system='METRIC'; scene.unit_settings.scale_length=1
collections={}
for n in ['base_shell','rear_annex','rear_connection_cap','Preview_Furniture','Presentation']:
    c=bpy.data.collections.new(n); scene.collection.children.link(c); collections[n]=c
current='base_shell'; bodies=[]; doors=[]; windows=[]
def xyz(v): return (v[0],-v[2],v[1])
def mat(name,color,stem=None):
    m=bpy.data.materials.new(name); m.diffuse_color=(*color,1); m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=(*color,1); p.inputs['Roughness'].default_value=.78
    if stem:
        for chan in ['albedo','roughness','normal']:
            f=GAME/'assets/materials/ranch_home'/f'{stem}_{chan}.png'
            image=bpy.data.images.load(str(f),check_existing=True); image.pack()
            if chan!='albedo': image.colorspace_settings.name='Non-Color'
            t=m.node_tree.nodes.new('ShaderNodeTexImage'); t.image=image
            if chan=='normal':
                normal=m.node_tree.nodes.new('ShaderNodeNormalMap'); normal.inputs['Strength'].default_value=.22
                m.node_tree.links.new(t.outputs['Color'],normal.inputs['Color']); m.node_tree.links.new(normal.outputs[0],p.inputs['Normal'])
            else: m.node_tree.links.new(t.outputs['Color'],p.inputs['Base Color' if chan=='albedo' else 'Roughness'])
    return m
plaster=mat('Plaster',(.79,.72,.58),'warm_lime_plaster'); oak=mat('Floor',(.48,.29,.13),'painted_warm_oak')
timber=mat('Timber',(.19,.10,.055)); roofmat=mat('Roof_Slate',(.15,.23,.24)); trim=mat('Paint_Supply_Sage',(.24,.37,.28)); glass=mat('Window_BlueGrey',(.25,.45,.49)); stone=mat('Foundation_Stone',(.37,.38,.33)); brass=mat('Hardware_Brass',(.52,.37,.12))
def move(o):
    for c in list(o.users_collection): c.objects.unlink(o)
    collections[current].objects.link(o)
def box(name,center,size,material,col=False):
    bpy.ops.mesh.primitive_cube_add(size=1,location=xyz(center)); o=bpy.context.object; o.name=name+('-col' if col else '')
    o.dimensions=(size[0],size[2],size[1]); bpy.ops.object.transform_apply(location=False,rotation=False,scale=True); move(o); o.data.materials.append(material)
    for poly in o.data.polygons:
        axes=[a for a in range(3) if a!=max(range(3),key=lambda a:abs(poly.normal[a]))]
        for li in poly.loop_indices:
            co=o.data.vertices[o.data.loops[li].vertex_index].co; o.data.uv_layers.active.data[li].uv=(co[axes[0]],co[axes[1]])
    if col: bodies.append({'name':o.name,'module':current,'min':[center[i]-size[i]/2 for i in range(3)],'max':[center[i]+size[i]/2 for i in range(3)]})
    return o
def socket(name,p):
    o=bpy.data.objects.new(name,None); collections[current].objects.link(o); o.location=xyz(p); o.empty_display_type='ARROWS'; o.empty_display_size=.35; o['purpose']='Authoring approach only; no gameplay binding'; return o
# Segmented solid walls. Holes described in wall-local horizontal and height coordinates.
def wall(name,axis,fixed,lo,hi,holes=(),height=3.15,thick=.22):
    xs=sorted(set([lo,hi]+[v for h in holes for v in h[:2]]))
    for i,(a,b) in enumerate(zip(xs,xs[1:])):
        cut=next((h for h in holes if h[0]<=(a+b)/2<=h[1]),None)
        spans=[(0,height)] if cut is None else [(0,cut[2]),(cut[3],height)]
        for j,(bottom,top) in enumerate(spans):
            if top-bottom<.001: continue
            pos=[(a+b)/2,(top+bottom)/2,fixed] if axis=='x' else [fixed,(top+bottom)/2,(a+b)/2]
            size=[b-a,top-bottom,thick] if axis=='x' else [thick,top-bottom,b-a]
            box(f'{name}_{i}_{j}',pos,size,plaster,True)
def doorway(name,axis,fixed,center,width,module=None):
    # Width is clear BETWEEN frame inner faces. Frame extends away from hole.
    for s in [-1,1]:
        p=[center+s*(width/2+.06),1.39,fixed] if axis=='x' else [fixed,1.39,center+s*(width/2+.06)]
        size=[.12,2.78,.30] if axis=='x' else [.22,2.78,.12]
        box(name+'_Jamb'+str(s),p,size,timber,True)
    p=[center,2.84,fixed] if axis=='x' else [fixed,2.84,center]
    box(name+'_Header',p,[width+.24,.12,.30] if axis=='x' else [.22,.12,width+.24],timber,True)
    d={'name':name,'module':current,'axis':axis,'fixed':fixed,'center':center,'clear_width_m':width,'clear_height_m':2.78}
    doors.append(d)
def window(name,axis,fixed,center):
    w=1.7; bottom=1.1; top=2.5
    def part(suffix,h,y,sw,sh,depth,material,col=False):
        return box(name+suffix,[h,y,fixed] if axis=='x' else [fixed,y,h],[sw,sh,depth] if axis=='x' else [depth,sh,sw],material,col)
    part('_Glazing',center,1.8,w,1.4,.035,glass,True)
    for s in [-1,1]:part('_Frame'+str(s),center+s*.85,1.8,.09,1.49,.29,timber)
    part('_Sill',center,1.07,1.92,.10,.39,timber);part('_Lintel',center,2.53,1.9,.1,.30,timber)
    part('_Mullion',center,1.8,.055,1.4,.12,trim);part('_Rail',center,1.8,w,.055,.12,trim)
    windows.append({'name':name,'module':current,'axis':axis,'wall_plane':fixed,'horizontal_center':center,'opening':[w,1.4],'same_mesh_inside_outside':True})
def roof(name,z0,z1,eave=3.25,ridge=5.0):
    # Editable sloping roof solids; all roof geometry starts Roof_.
    for side in [-1,1]:
        xa,xb=sorted([0,6*side]); ya=eave if xa else ridge; yb=eave if xb else ridge
        verts=[xyz((x,y,z)) for off in [0,.14] for x,y,z in [(xa,ya+off,z0),(xb,yb+off,z0),(xb,yb+off,z1),(xa,ya+off,z1)]]
        mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]);mesh.update()
        o=bpy.data.objects.new(f'Roof_{name}_{side}',mesh);collections[current].objects.link(o);mesh.materials.append(roofmat)
        # Narrow standing seams give a readable roof rather than a flat slab.
        for k in range(int((z1-z0)/.65)+1):
            z=z0+.1+k*.65
            if z>z1-.1:continue
            beam=box(f'Roof_{name}_Seam_{side}_{k}',(side*3,(eave+ridge)/2+.16,z),(math.sqrt(36+(ridge-eave)**2)-.02,.035,.045),trim)
            beam.rotation_euler[1]=side*math.atan((ridge-eave)/6)
    box('Roof_'+name+'_Ridge',(0,ridge+.19,(z0+z1)/2),(.18,.13,z1-z0),timber)
# Outer shell inside 12x10, roof eaves remain within reserved envelope.
box('Main_Floor',(0,-.1,0),(12,.2,10),oak,True)
wall('Front','x',4.7,-5.65,5.65,[(-.92,.92,0,2.9),(-4.65,-2.95,1.1,2.5),(2.95,4.65,1.1,2.5)])
doorway('Front_Entrance','x',4.7,0,1.6)
for x in [-3.8,3.8]:window('Front_Window'+str(x),'x',4.7,x)
wall('Rear','x',-4.89,-5.65,5.65,[(-.92,.92,0,2.9)])
doorway('Rear_Connection','x',-4.89,0,1.6)
for x in [-5.65,5.65]:
    holes=[(-3.65,-1.95,1.1,2.5),(1.65,3.35,1.1,2.5)]
    wall('Side'+str(x),'z',x,-4.78,4.78,holes)
    for z in [-2.8,2.5]:window('SideWindow'+str(x)+str(z),'z',x,z)
# Spine inner faces exactly +/-1.0, widened holes leave 1.4m after frames.
for x in [-1.11,1.11]:
    wall('Spine'+str(x),'z',x,-4.78,4.59,[(-3.52,-1.88,0,2.9),(1.38,3.02,0,2.9)])
    for z in [-2.7,2.2]:doorway('Internal'+str(x)+str(z),'z',x,z,1.4)
    wall('Room_Divider'+str(x),'x',-.25,-5.54 if x<0 else 1.22,-1.22 if x<0 else 5.54)
for x in [-5.65,-1.3,1.3,5.65]:
    box('Facade_Post'+str(x),(x,1.56,4.85),(.16,3.12,.16),timber)
box('Facade_Beam',(0,3.08,4.85),(11.45,.18,.17),timber)
# Gable infill belongs to cutaway, leaving genuine 3.15m room volume.
for z in [-4.89,4.7]:
    mesh=bpy.data.meshes.new('Gable');mesh.from_pydata([xyz(v) for v in [(-5.65,3.15,z),(5.65,3.15,z),(0,4.88,z)]],[],[(0,1,2)]);mesh.materials.append(plaster)
    ob=bpy.data.objects.new('Roof_Main_Gable'+str(z),mesh);collections[current].objects.link(ob)
roof('Main',-5,5)
# Blank emblem board, no invented civic title or visible text.
box('Supply_Blank_Signboard',(2.05,2.27,4.88),(1.02,.68,.12),trim)
box('Supply_Parcel_Emblem',(2.05,2.27,4.97),(.32,.27,.035),oak)
box('Supply_Parcel_Band',(2.05,2.27,4.99),(.035,.29,.018),brass)
rooms={'reception':[-5.54,-1.22,-.14,4.59],'meeting':[1.22,5.54,-.14,4.59],'archive':[-5.54,-1.22,-4.78,-.36],'supply_office':[1.22,5.54,-4.78,-.36]}
for n,p in {'Reception_Desk_Approach':(-3.1,0,2.7),'Reception_Staff':(-3.1,0,.9),'Meeting':(3.2,0,3.6),'Archive':(-3.1,0,-2),'Supply_Office':(3.2,0,-2),'Rear_Connection':(0,0,-5)}.items():socket('Socket_'+n,p)
current='rear_annex'
box('Annex_Floor',(0,-.1,-8),(12,.2,6),oak,True)
wall('Annex_Rear','x',-10.78,-5.65,5.65,[(-.92,.92,0,2.9)])
doorway('Annex_Rear_Exit','x',-10.78,0,1.6)
wall('Annex_West','z',-5.65,-10.89,-5,[(-9.85,-8.15,1.1,2.5)])
window('Annex_West_Window','z',-5.65,-9)
wall('Annex_East','z',5.65,-10.89,-5,[(-8.62,-6.58,0,2.9)])
doorway('Loading_Access','z',5.65,-7.6,1.8)
roof('Annex',-11,-5,3.25,4.45)
for x in [-5.65,5.65]:box('Annex_Timber'+str(x),(x,3.08,-8),(.17,.16,5.8),timber)
socket('Socket_Loading_Access',(5.2,0,-7.6));socket('Socket_Annex_Storage',(-3.6,0,-8));socket('Socket_Annex_Connection',(0,0,-5.3))
current='rear_connection_cap'
box('Removable_Rear_Cap',(0,1.39,-4.89),(1.60,2.78,.15),trim,True)
box('Cap_Crossbar',(0,1.6,-5.0),(1.52,.12,.12),timber)
# Linked collection instances: actual existing furniture, never duplicated in exports.
current='Preview_Furniture'
source=GAME/'assets/3d/ranch_furniture/ranch_furniture.blend'
with bpy.data.libraries.load(str(source),link=True) as (src,dst):dst.collections=[n for n in ['desk','chair','table','pantry','wardrobe'] if n in src.collections]
linked={c.name:c for c in dst.collections}
placements=[('desk',(-3.25,0,1.65),0),('chair',(-3.25,0,.55),0),('table',(3.25,0,2.15),0),('chair',(3.25,0,3.35),math.pi),('chair',(3.25,0,.95),0),('wardrobe',(-4.5,0,-3.7),0),('pantry',(-2.3,0,-3.9),0),('desk',(3.35,0,-3.5),0),('chair',(3.35,0,-4.15),0),('pantry',(-4.3,0,-9.7),0),('pantry',(-2.2,0,-9.7),0)]
for i,(n,p,r) in enumerate(placements):
    o=bpy.data.objects.new('Reference_'+n+'_'+str(i),None);o.instance_type='COLLECTION';o.instance_collection=linked[n];collections[current].objects.link(o);o.rotation_euler.z=r
    # Source furniture collections are laid out on a catalogue grid. Remove that
    # authored root offset without modifying linked library data.
    root=next(ob for ob in linked[n].objects if ob.parent is None and ob.name==n)
    from mathutils import Matrix
    o.location=Vector(xyz(p)) - Matrix.Rotation(r,3,'Z') @ root.location
# Close the rear annex gable; cutaway naming removes it with roof.
current='rear_annex'
mesh=bpy.data.meshes.new('Annex_Rear_Gable');mesh.from_pydata([xyz(v) for v in [(-5.65,3.15,-10.78),(5.65,3.15,-10.78),(0,4.38,-10.78)]],[],[(0,1,2)]);mesh.materials.append(plaster)
ob=bpy.data.objects.new('Roof_Annex_Rear_Gable',mesh);collections[current].objects.link(ob)
# Validate actual architectural vertex AABBs and all open doorway volumes against colliders.
bpy.context.view_layer.update()
def bounds(objects):
    vv=[o.matrix_world@v.co for o in objects if o.type=='MESH' for v in o.data.vertices]
    g=[(v.x,v.z,-v.y) for v in vv]
    return {'min':[min(v[i] for v in g) for i in range(3)],'max':[max(v[i] for v in g) for i in range(3)]}
checks=[]
for d in doors:
    h=d['clear_width_m']/2-.002; f=d['fixed']; c=d['center']
    low=[c-h,.005,f-.16] if d['axis']=='x' else [f-.16,.005,c-h]
    high=[c+h,2.775,f+.16] if d['axis']=='x' else [f+.16,2.775,c+h]
    hits=[b['name'] for b in bodies if b['module']!='rear_connection_cap' and all(min(high[i],b['max'][i])-max(low[i],b['min'][i])>1e-5 for i in range(3))]
    assert not hits,(d,hits)
    checks.append(dict(d,probe_min=low,probe_max=high,blocking_bodies=hits))
spine_hits=[b['name'] for b in bodies if b['module']=='base_shell' and all(min([.998,2.99,4.5][i],b['max'][i])-max([-.998,.005,-4.7][i],b['min'][i])>1e-5 for i in range(3))]
assert not spine_hits,spine_hits
exports=[]
for name in ['base_shell','rear_annex','rear_connection_cap']:
    bpy.ops.object.select_all(action='DESELECT')
    obs=list(collections[name].objects)
    for o in obs:o.select_set(True)
    path=OUT/(name+'.glb');bpy.ops.export_scene.gltf(filepath=str(path),export_format='GLB',use_selection=True,export_yup=True,export_extras=True)
    raw=path.read_bytes();size,kind=struct.unpack_from('<II',raw,12);doc=json.loads(raw[20:20+size]);names=[n['name'] for n in doc.get('nodes',[])]
    assert len([o for o in obs if o.type=='MESH'])==len(doc['meshes'])
    assert all(tuple(o.scale)==(1,1,1) for o in obs)
    exports.append({'file':path.name,'sha256':hashlib.sha256(raw).hexdigest(),'bytes':len(raw),'meshes':len(doc['meshes']),'nodes':len(names),'collision_nodes':[n for n in names if n.endswith('-col')],'roof_nodes':[n for n in names if n.startswith('Roof_')],'sockets':[n for n in names if n.startswith('Socket_')],'bounds_godot_m':bounds(obs),'node_names':names,'triangles':sum(doc['accessors'][p['indices']]['count']//3 for m in doc['meshes'] for p in m['primitives'])})
    print('EXPORT_OK',name,len(doc['meshes']))
# Cap is deliberately absent in connected preview, but remains independent export.
collections['rear_connection_cap'].hide_render=True;collections['rear_connection_cap'].hide_viewport=True
current='Presentation'
world=bpy.data.worlds.new('Warm_Daylight');scene.world=world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.63,.73,.85,1);world.node_tree.nodes['Background'].inputs[1].default_value=.55
bpy.ops.object.light_add(type='AREA',location=(0,-8,15));light=bpy.context.object;move(light);light.data.energy=2600;light.data.shape='DISK';light.data.size=12
bpy.ops.object.light_add(type='SUN',location=(5,-6,12));sun=bpy.context.object;move(sun);sun.rotation_euler=(.35,-.4,-.3);sun.data.energy=2
box('Preview_Ground',(0,-.24,-3),(30,.08,32),stone)
bpy.ops.object.camera_add();cam=bpy.context.object;move(cam);scene.camera=cam;cam.data.type='ORTHO';cam.data.ortho_scale=24
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True;scene.render.resolution_x=1200;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
def camera(pos,target):cam.location=xyz(pos);cam.rotation_euler=(Vector(xyz(target))-cam.location).to_track_quat('-Z','Y').to_euler()
camera((19,16,22),(0,1.5,-2.6))
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'okachi_supply_house.blend'))
scene.render.filepath=str(DOC/'exterior.png');bpy.ops.render.render(write_still=True)
for o in bpy.data.objects:
    if o.name.startswith('Roof_'):o.hide_render=True
camera((17,25,14),(0,0,-3));scene.render.filepath=str(DOC/'roof_cutaway.png');bpy.ops.render.render(write_still=True)
receipt={'units':'metres','godot_axes':'X right, Y up, front +Z; Blender (x,-z,y)','root_scaling':'none; every exported object scale = 1','room_clear_height_m':3.15,'reserved_envelope_xz':[-6,6,-11,5],'rooms_xmin_xmax_zmin_zmax':rooms,'spine_clear_width_m':2,'spine_collision_hits':spine_hits,'doors':checks,'windows':windows,'collision_bodies':bodies,'exports':exports,'furniture_preview':{'source':str(source.relative_to(GAME)),'linked_instances':len(placements),'exported':False},'limitations':['No Godot import, navmesh or actor traversal run: parent owns integration.','Door frames have open apertures; no animated door leaves. Rear cap physically closes rear connection only in base state.','Furniture linked for preview only; integrate existing furniture GLBs separately.','Roof uses no collision; imported -col behavior still requires engine verification.','Sockets are authoring hints, not interactions or calibrated actor poses.']}
(DOC/'geometry_receipts.json').write_text(json.dumps(receipt,indent=2))
print('VERIFIED_DOORS',len(checks),'SPINE',spine_hits,'FINISHED')
