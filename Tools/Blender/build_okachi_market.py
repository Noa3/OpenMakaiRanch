"""Editable local-space Okachi market kit. No engine/layout/economy writes.
Run Blender --background --threads 2 --python-exit-code 1 --python this_file.
Coordinates below are Godot XYZ metres, +Z front. No imported generators.
"""
import bpy, json, math, struct, hashlib, shutil, uuid
from pathlib import Path
from datetime import datetime, timezone
from mathutils import Vector, Matrix, Quaternion
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'OpenMakaiRanchGame/assets/3d/okachi_market'
DOC=ROOT/'OpenMakaiRanchGame/docs/art/okachi_market'
OUT.mkdir(parents=True,exist_ok=True); DOC.mkdir(parents=True,exist_ok=True)
# Every rerun preserves complete earlier outputs in a unique generation archive.
if any(OUT.glob('*.glb')) or (OUT/'okachi_market.blend').exists():
    arc=OUT/'history'/(datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%SZ')+'_'+uuid.uuid4().hex[:8])
    arc.mkdir(parents=True)
    for base,label in [(OUT,'assets'),(DOC,'evidence')]:
        (arc/label).mkdir()
        for f in base.iterdir():
            if f.is_file(): shutil.copy2(f,arc/label/f.name)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene; scene.unit_settings.system='METRIC'; scene.unit_settings.scale_length=1
materials={}
for name,color,metal in [('Timber',(.19,.10,.055),0),('Oak_Honey',(.36,.18,.075),0),('Sage',(.24,.37,.28),0),('Slate',(.15,.23,.24),0),('Stone',(.37,.38,.33),0),('Iron',(.045,.052,.055),.8),('Lime',(.79,.72,.58),0)]:
    m=bpy.data.materials.new(name); m.diffuse_color=(*color,1); m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=(*color,1); p.inputs['Roughness'].default_value=.78; p.inputs['Metallic'].default_value=metal
    materials[name]=m
modules={}; current=None

def xyz(p): return Vector((p[0],-p[2],p[1]))
def begin(name):
    global current
    current=bpy.data.collections.new(name); scene.collection.children.link(current); modules[name]=current
    current['units']='metres'; current['forward']='+Z Godot / -Y Blender'
def move(o):
    for c in list(o.users_collection): c.objects.unlink(o)
    current.objects.link(o)
def box(name,p,size,mat='Timber',collision=True):
    bpy.ops.mesh.primitive_cube_add(size=1,location=xyz(p)); o=bpy.context.object
    o.name=current.name+'_'+name+'_'+str(len(current.objects))+('-col' if collision else ''); o.dimensions=(size[0],size[2],size[1]); move(o)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True); o.data.materials.append(materials[mat]); return o
def beam(name,a,b,width=.16,mat='Timber',collision=True):
    aa,bb=xyz(a),xyz(b); o=box(name,(0,0,0),(width,(bb-aa).length,width),mat,collision)
    o.location=(aa+bb)/2; o.rotation_euler=(bb-aa).to_track_quat('Z','Y').to_euler(); return o
def socket(name,p):
    o=bpy.data.objects.new('Socket_'+current.name+'_'+name,None); current.objects.link(o); o.location=xyz(p); o.empty_display_type='ARROWS'; o.empty_display_size=.25
    o['purpose']='Neutral authoring hint only; not interaction, IK, reservation or navigation'; return o

begin('canopy')
# Roof is exactly 6 x 4m. Four edge posts, no floor or middle supports.
for x in [-2.7,2.7]:
    for z in [-1.7,1.7]:
        box('Pad',(x,.09,z),(.42,.18,.42),'Stone')
        box('Post',(x,1.69,z),(.24,3.02,.24))
        box('Shoe',(x,.28,z),(.27,.18,.27),'Iron')
        # Knee braces confined to perimeter; lowest point >2.8m.
        beam('Knee_X',(x,2.92,z),(x-math.copysign(.60,x),3.45,z),.16)
        beam('Knee_Z',(x,2.92,z),(x,3.45,z-math.copysign(.54,z)),.16)
for z in [-1.7,1.7]: box('Tie',(0,3.40,z),(5.65,.24,.22))
for x in [-2.7,2.7]: box('Eave',(x,3.47,0),(.24,.24,3.65))
# Gable trusses, kingposts and longitudinal ridge carry roof planes.
for z in [-1.7,1.7]:
    for x in [-2.7,2.7]: beam('PrincipalRafter',(x,3.54,z),(0,4.25,z),.18)
    beam('Kingpost',(0,3.52,z),(0,4.25,z),.18)
box('Ridge',(0,4.23,0),(.18,.20,3.85))
for z in [-1.12,0,1.12]:
    for x in [-2.85,2.85]: beam('CommonRafter',(x,3.54,z),(0,4.29,z),.12)
# Explicit solid roof panels, with outward normals recalculated below.
for side in [-1,1]:
    vs=[xyz((x,y+off,z)) for off in [0,.10] for x,y,z in [(0,4.35,-2),(side*3,3.55,-2),(side*3,3.55,2),(0,4.35,2)]]
    mesh=bpy.data.meshes.new('EditableSlatePanel'); mesh.from_pydata(vs,[],[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]); mesh.update()
    o=bpy.data.objects.new('Roof_Canopy_Panel_'+str(side),mesh); current.objects.link(o); mesh.materials.append(materials['Slate'])
    for z in [-1.94,1.94]:
        o=beam('Fascia',(0,4.36,z),(side*2.98,3.565,z),.11,'Sage',False); o.name='Roof_'+o.name
    for z in [-1.3,-.65,0,.65,1.3]:
        o=beam('StandingSeam',(0,4.46,z),(side*2.99,3.6627,z),.025,'Slate',False); o.name='Roof_'+o.name
r=box('Cap',(0,4.46,0),(.18,.10,4),'Sage',False); r.name='Roof_'+r.name
socket('Approach',(0,0,2.7)); socket('Loading',(0,0,-2.7)); socket('Accessory_Left',(-2,0,0)); socket('Accessory_Right',(2,0,0))

begin('counter')
# Portable open-backed 1.8 x .8m counter; no merchandise. Front +Z.
for x in [-.77,.77]:
    for z in [-.27,.27]: box('Leg',(x,.45,z),(.12,.90,.12))
for z in [-.27,.27]: box('Apron',(0,.77,z),(1.66,.22,.10))
for x in [-.77,.77]: box('SideRail',(x,.40,0),(.10,.13,.62))
box('FrontPanel',(0,.50,.30),(1.48,.48,.08),'Sage')
for i in range(4): box('TopPlank',(0,.95,-.3+i*.2),(1.8,.10,.196),'Oak_Honey')
box('LowerShelf',(0,.23,-.025),(1.42,.09,.49),'Oak_Honey')
socket('Approach',(0,0,1)); socket('Staff',(0,0,-.9)); socket('Accessory',(0,1,0))

begin('scaffold_bay')
for x in [-1.1,1.1]:
    for z in [-.48,.48]:
        box('Foot',(x,.07,z),(.36,.14,.36),'Stone')
        box('Upright',(x,1.48,z),(.16,2.82,.16))
for z in [-.48,.48]:
    for y in [.40,1.67,2.78]: box('Ledger',(0,y,z),(2.36,.14,.14))
for x in [-1.1,1.1]:
    for y in [.4,1.67,2.78]: box('Transom',(x,y,0),(.16,.14,1.12))
    beam('EndBrace',(x,.46,-.48),(x,2.68,.48),.14)
beam('BackBrace',(-1.1,.46,-.48),(1.1,2.68,-.48),.14)
for i in range(5): box('Deck',(0,1.80,-.44+i*.22),(2.38,.12,.212),'Oak_Honey')
box('BackToeBoard',(0,1.98,-.52),(2.38,.24,.09),'Sage')
socket('Loading',(0,0,1.1))

begin('material_stack')
for x in [-.75,.75]: box('Dunnage',(x,.08,0),(.18,.16,.86))
for level in range(4):
    for i in range(4): box('Lumber',(0,.23+level*.145,-.30+i*.20),(2.05,.13,.18),'Oak_Honey')
for x in [-.67,.67]: box('Band',(x,.45,0),(.09,.61,.81),'Iron',False)
socket('Loading',(0,0,1.1))

begin('barrier')
# Solid broad rails and feet; no wire-thin collision needles.
for x in [-.82,.82]:
    box('Foot',(x,.08,0),(.30,.16,.78))
    box('Upright',(x,.60,0),(.14,1.04,.14))
for y in [.47,.96]: box('Rail',(0,y,0),(2,.23,.13),'Sage')
for x in [-.66,0,.66]: box('ContrastPanel',(x,.96,.071),(.23,.19,.02),'Lime',False)
socket('Approach',(0,0,1.1))

# Bake all object transforms into editable vertices; rootless mesh nodes are identity.
for c in modules.values():
    for o in c.objects:
        if o.type=='MESH':
            bpy.ops.object.select_all(action='DESELECT'); o.select_set(True); bpy.context.view_layer.objects.active=o
            bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
            bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT'); bpy.ops.mesh.normals_make_consistent(inside=False); bpy.ops.object.mode_set(mode='OBJECT')
bpy.context.view_layer.update()
def bounds(c):
    pts=[]
    for o in c.objects:
        if o.type!='MESH': continue
        ev=o.evaluated_get(bpy.context.evaluated_depsgraph_get()); me=ev.to_mesh()
        pts.extend([(v.x,v.z,-v.y) for v in [ev.matrix_world@v.co for v in me.vertices]]); ev.to_mesh_clear()
    return {'min':[min(p[i] for p in pts) for i in range(3)],'max':[max(p[i] for p in pts) for i in range(3)]}
# Conservative actual-mesh AABB checks prove central lane, not actor traversal.
lane_min=(-1,0.001,-2); lane_max=(1,2.8,2)
hits=[]; lower=[]
for o in modules['canopy'].objects:
    if o.type!='MESH':continue
    pp=[(v.co.x,v.co.z,-v.co.y) for v in o.data.vertices]
    lo=[min(p[i] for p in pp) for i in range(3)]; hi=[max(p[i] for p in pp) for i in range(3)]
    if all(min(hi[i],lane_max[i])-max(lo[i],lane_min[i])>1e-5 for i in range(3)): hits.append(o.name)
    if any(t in o.name for t in ['Knee','Tie','Eave']): lower.append(lo[1])
assert not hits,hits
assert min(lower)>=2.8,min(lower)

def decode(path):
    raw=path.read_bytes(); n,typ=struct.unpack_from('<II',raw,12); d=json.loads(raw[20:20+n]); bn,bt=struct.unpack_from('<II',raw,20+n); blob=raw[28+n:28+n+bn]
    pts=[]; triangles=0
    for node in d['nodes']:
        assert 'children' not in node,'Unexpected hierarchy'
        assert node.get('scale',[1,1,1])==[1,1,1]
        if 'mesh' not in node:continue
        assert not any(k in node for k in ['matrix','rotation','translation']),node
        for p in d['meshes'][node['mesh']]['primitives']:
            a=d['accessors'][p['attributes']['POSITION']]; v=d['bufferViews'][a['bufferView']]
            assert a['componentType']==5126 and a['type']=='VEC3'
            start=v.get('byteOffset',0)+a.get('byteOffset',0); stride=v.get('byteStride',12)
            pts.extend(struct.unpack_from('<fff',blob,start+i*stride) for i in range(a['count']))
            triangles+=d['accessors'][p['indices']]['count']//3
    names=[n.get('name','') for n in d['nodes']]
    return {'file':path.name,'sha256':hashlib.sha256(raw).hexdigest(),'bytes':len(raw),'mesh_count':len(d['meshes']),'triangles':triangles,'collision_mesh_count':sum(n.endswith('-col') for n in names),'collision_nodes':[n for n in names if n.endswith('-col')],'roof_nodes':[n for n in names if n.startswith('Roof_')],'sockets':[n for n in names if n.startswith('Socket_')],'decoded_bounds_m':{'min':[min(p[i] for p in pts) for i in range(3)],'max':[max(p[i] for p in pts) for i in range(3)]},'mesh_node_transforms':'identity'}
exports=[]
for name,c in modules.items():
    bpy.ops.object.select_all(action='DESELECT')
    for o in c.objects:o.select_set(True)
    path=OUT/(name+'.glb'); bpy.ops.export_scene.gltf(filepath=str(path),export_format='GLB',use_selection=True,export_yup=True,export_extras=True)
    r=decode(path); r['evaluated_bounds_m']=bounds(c)
    assert r['mesh_count']==sum(o.type=='MESH' for o in c.objects)
    assert all(abs(r['decoded_bounds_m'][k][i]-r['evaluated_bounds_m'][k][i])<1e-5 for k in ['min','max'] for i in range(3)),r
    exports.append(r); print('EXPORT_OK',name,r['mesh_count'],r['triangles'])
# Saved sources remain overlapping LOCAL origins. Toggle named module collections.
bpy.context.preferences.filepaths.save_version=0
blend=OUT/'okachi_market.blend'; bpy.ops.wm.save_as_mainfile(filepath=str(blend))
# Reopen exact saved source before creating transient catalogue staging.
bpy.ops.wm.open_mainfile(filepath=str(blend)); scene=bpy.context.scene
assert not bpy.data.libraries and not bpy.data.images
modules={name:bpy.data.collections[name] for name in modules}
materials={name:bpy.data.materials[name] for name in materials}
for r in exports: assert bounds(modules[Path(r['file']).stem])==r['evaluated_bounds_m']
for name,offset in {'canopy':(-2.4,0,0),'counter':(2.8,0,2),'scaffold_bay':(3.4,0,-1.2),'material_stack':(.1,0,3.5),'barrier':(-3,0,3.5)}.items():
    for o in modules[name].objects:o.location+=xyz(offset)
begin('Presentation_NOT_EXPORTED')
box('Ground',(0,-.08,0),(16,.12,13),'Lime',False)
world=bpy.data.worlds.new('Daylight');scene.world=world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.63,.73,.85,1);world.node_tree.nodes['Background'].inputs[1].default_value=.7
bpy.ops.object.light_add(type='AREA',location=(-3,-5,11));bpy.context.object.data.energy=2200;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=8
bpy.ops.object.light_add(type='SUN');bpy.context.object.rotation_euler=(.3,-.5,-.4);bpy.context.object.data.energy=1.5
bpy.ops.object.camera_add();cam=bpy.context.object;scene.camera=cam;cam.location=xyz((12,10,17));cam.rotation_euler=(xyz((0,1.4,.3))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=15
scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=20;scene.cycles.use_denoising=True
scene.render.resolution_x=1400;scene.render.resolution_y=1000;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX'
scene.render.filepath=str(DOC/'contact_sheet.png');bpy.ops.render.render(write_still=True)
receipt={'units':'metres; Godot XYZ +Z front, Blender (x,-z,y)','source_sha256':hashlib.sha256(blend.read_bytes()).hexdigest(),'generator_sha256':hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),'source_reopened_verified':True,'external_libraries':[],'external_images':[],'texture_attribution':'No textures used. Flat palette values adapted read-only from build_okachi_supply_house.py and build_ranch_furniture.py; fence source build_world_assets.py inspected. No generator imported or executed.','canopy_footprint_m':[6,4],'minimum_frame_underside_m':min(lower),'clear_lane_probe_min':lane_min,'clear_lane_probe_max':lane_max,'clear_lane_blocking_meshes':hits,'counter_top_height_m':1.0,'reserved_court_m':[14,12],'court_authored':False,'exports':exports,'render':{'file':'contact_sheet.png','engine':'Cycles','device':'CPU','samples':20,'sha256':hashlib.sha256((DOC/'contact_sheet.png').read_bytes()).hexdigest()},'limitations':['No Godot import or -col importer verification.','No navigation, physical traversal, use/IK, stage persistence, finance or town placement claims.','Roof has no collision; solid frame/furniture use visible -col meshes.','Scaffold is a static construction prop, not certified climbing equipment; no ladder or climbing behavior.','Contact sheet uses transient exploded catalogue placements, not a market court layout.','Saved source collections share local origins; solo desired collection for editing.']}
(DOC/'geometry_receipts.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8')
print('MARKET_VERIFIED_FINISHED',len(exports),'GLBs; saved source reopened; CPU contact sheet complete')
