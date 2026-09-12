"""Editable metre-scale ranch kit. Run Blender --background --python this_file.
Owned outputs only: assets/3d/ranch_furniture. No gameplay/scene modifications.
"""
import bpy, json, math, struct, hashlib
from pathlib import Path
from mathutils import Vector

OUT = Path(__file__).resolve().parents[2] / 'OpenMakaiRanchGame/assets/3d/ranch_furniture'
OUT.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.unit_settings.system = 'METRIC'
scene.unit_settings.scale_length = 1
materials = {}
for name, color, rough, metallic in [
    ('Oak_Honey', (.36,.18,.075,1), .72,0), ('Oak_Endgrain',(.23,.095,.035,1),.8,0),
    ('Paint_Sage',(.19,.31,.24,1),.7,0), ('Linen_Cream',(.78,.71,.54,1),.95,0),
    ('Blanket_Indigo',(.09,.18,.27,1),.95,0), ('Iron_Black',(.045,.052,.055,1),.35,.8),
    ('Ceramic_Stone',(.41,.43,.38,1),.84,0)]:
    m = bpy.data.materials.new(name); m.diffuse_color=color; m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=color
    p.inputs['Roughness'].default_value=rough; p.inputs['Metallic'].default_value=metallic
    materials[name]=m
assets=[]; current=None

def begin(name, purpose):
    global current
    coll=bpy.data.collections.new(name); scene.collection.children.link(coll)
    root=bpy.data.objects.new(name,None); coll.objects.link(root)
    root['units']='metres'; root['purpose']=purpose; root['forward_blender']='-Y'; root['forward_godot']='+Z'
    current={'name':name,'collection':coll,'root':root,'objects':[],'anchors':{},'purpose':purpose}
    assets.append(current)
    anchor('origin',(0,0,0),'Floor-centred placement pivot')

def anchor(name, pos, purpose):
    o=bpy.data.objects.new('anchor_'+name,None); current['collection'].objects.link(o)
    o.parent=current['root']; o.location=pos; o.empty_display_type='ARROWS'; o.empty_display_size=.12
    o['purpose']=purpose; current['objects'].append(o)
    current['anchors'][name]={'node_name':o.name,'blender_xyz_m':list(pos),'godot_xyz_m':[pos[0],pos[2],-pos[1]],'purpose':purpose}
    return o

def box(name,pos,size,mat='Oak_Honey',bevel=.015):
    bpy.ops.mesh.primitive_cube_add(size=1, location=pos)
    o=bpy.context.object; o.name=name; o.dimensions=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    for c in list(o.users_collection): c.objects.unlink(o)
    current['collection'].objects.link(o); o.parent=current['root']
    o.data.materials.append(materials[mat])
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(island_margin=.025); bpy.ops.object.mode_set(mode='OBJECT')
    mod=o.modifiers.new('Editable softened timber edges','BEVEL'); mod.width=min(bevel,min(size)*.2); mod.segments=2
    o['part']=name; current['objects'].append(o); return o

def beam(name,a,b,width,depth=None,mat='Oak_Honey'):
    a,b=Vector(a),Vector(b); o=box(name,(a+b)*.5,(width,depth or width,(b-a).length),mat)
    o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler(); return o

def legs(width,depth,height,thick=.075):
    for x in [-width/2+thick/2,width/2-thick/2]:
        for y in [-depth/2+thick/2,depth/2-thick/2]: box('leg',(x,y,height/2),(thick,thick,height),'Oak_Endgrain')

def planks(name,width,depth,z,thick,n=5,mat='Oak_Honey'):
    for i in range(n): box(name+str(i),(0,-depth/2+(i+.5)*depth/n,z),(width,depth/n-.008,thick),mat)

begin('bed_single','Single adult-scale bed; mattress 1.10 x 2.15 m; no animation supplied')
legs(1.24,2.3,.43,.10)
for x in [-.57,.57]: box('side_rail',(x,0,.35),(.1,2.3,.19))
for y in [-1.10,1.10]: box('end_rail',(0,y,.35),(1.24,.1,.19))
for i in range(9): box('support_slat',(0,-.95+i*.2375,.42),(1.12,.09,.055),'Oak_Endgrain')
box('mattress',(0,0,.535),(1.10,2.15,.18),'Linen_Cream',.055)
box('blanket',(0,-.32,.641),(1.115,1.48,.036),'Blanket_Indigo',.012)
box('pillow',(0,.77,.67),(.70,.40,.095),'Linen_Cream',.05)
for x in [-.57,.57]: box('head_post',(x,1.10,.58),(.10,.10,1.16),'Oak_Endgrain')
for z in [.70,.91,1.10]: box('headboard',(0,1.10,z),(1.09,.07,.15))
anchor('lie',(0,0,.625),'Supine mattress surface; head toward +Y Blender / -Z Godot')
anchor('approach',(1.05,0,0),'Right-side standing approach; allow .70 m unobstructed aisle')

begin('dining_table','Four-place dining table; top surface 0.76 m')
legs(1.7,.9,.70,.09); planks('top_',1.8,1.0,.725,.07)
for x in [-.77,.77]: box('apron',(x,0,.62),(.07,.80,.16))
for y in [-.37,.37]: box('apron',(0,y,.62),(1.56,.07,.16))
anchor('surface',(0,0,.76),'Tabletop surface')
for i,(x,y) in enumerate([(-.5,-.86),(.5,-.86),(-.5,.86),(.5,.86)]): anchor('seat_'+str(i),(x,y,.46),'Suggested chair-seat centre; chair is separate asset')

begin('chair','Dining chair; seat surface 0.46 m; open leg and back geometry')
legs(.48,.48,.42,.055); planks('seat_',.50,.50,.44,.04,4)
for x in [-.215,.215]: box('back_post',(x,.215,.65),(.055,.055,.70),'Oak_Endgrain')
for z in [.67,.81,.95]: box('back_slat',(0,.215,z),(.40,.045,.095))
for x in [-.215,.215]: box('stretcher',(x,0,.19),(.035,.40,.035))
anchor('sit',(0,0,.46),'Seat surface, facing -Y Blender / +Z Godot; not a rig pelvis calibration')
anchor('approach',(0,-.70,0),'Clear approach in front')

begin('bench','Two-place bench; seat surface 0.46 m')
legs(1.40,.43,.41,.085); planks('seat_',1.60,.48,.435,.05,3)
box('stretcher',(0,0,.19),(1.3,.065,.08),'Oak_Endgrain')
for x in [-.42,.42]: anchor('sit_'+str(x),(x,0,.46),'Seat surface; facing -Y Blender / +Z Godot')

begin('writing_desk','Desk with knee clearance and separate drawer meshes')
legs(1.3,.65,.72,.075); planks('desktop_',1.4,.75,.755,.05)
box('back_apron',(0,.28,.62),(1.22,.065,.18))
box('drawer_box',(.44,0,.58),(.35,.53,.26),'Oak_Endgrain')
box('drawer_front',(.44,-.29,.58),(.36,.045,.24),'Paint_Sage')
box('drawer_pull',(.44,-.33,.58),(.12,.04,.025),'Iron_Black')
anchor('work_surface',(0,0,.78),'Desktop surface')
anchor('sit',(-.22,-.64,.46),'Suggested separate chair-seat centre')

begin('wardrobe','Two-door wardrobe; separately editable panels, hinges, handles')
legs(1.05,.60,.12,.08)
for x in [-.495,.495]: box('side',(x,0,1.02),(.06,.60,1.8))
for z in [.15,1.93]: box('shelf',(0,0,z),(1.05,.60,.06))
box('back',(0,.278,1.04),(.95,.045,1.72),'Oak_Endgrain')
for x in [-.245,.245]:
    box('door',(x,-.305,1.04),(.48,.055,1.72),'Paint_Sage')
    for z in [.58,1.48]: box('raised_panel',(x,-.344,z),(.36,.025,.62),'Paint_Sage')
    box('handle',(x*.3,-.373,1.04),(.022,.04,.14),'Iron_Black')
anchor('approach',(0,-.95,0),'Door access; reserve 0.65 m plus door swing')
anchor('hinge_left',(-.49,-.305,.18),'Vertical door rotation axis; doors unrigged')
anchor('hinge_right',(.49,-.305,.18),'Vertical door rotation axis; doors unrigged')

begin('open_shelf','Four-tier freestanding storage shelf')
for x in [-.47,.47]:
    for y in [-.18,.18]: box('post',(x,y,.90),(.06,.06,1.80),'Oak_Endgrain')
for i,z in enumerate([.12,.62,1.12,1.62]):
    planks('shelf_'+str(i),1.0,.42,z,.05,3)
    anchor('storage_'+str(i),(0,0,z+.025),'Shelf storage surface')
beam('back_brace',(-.45,.18,.15),(.45,.18,1.72),.04)

begin('kitchen_counter','Counter with inset basin cavity, no opaque block in basin')
legs(1.4,.65,.12,.09)
for x in [-.67,.67]: box('cabinet_side',(x,0,.48),(.06,.65,.78),'Paint_Sage')
box('base',(0,0,.15),(1.4,.65,.06),'Oak_Endgrain')
box('back',(0,.30,.49),(1.4,.05,.72),'Paint_Sage')
for x in [-.34,.34]:
    box('door',(x,-.32,.49),(.66,.05,.64),'Paint_Sage')
    box('pull',(x,-.36,.73),(.12,.035,.025),'Iron_Black')
box('worktop_left',(-.42,0,.90),(.64,.72,.06),'Ceramic_Stone')
box('rim_right',(.68,0,.90),(.12,.72,.06),'Ceramic_Stone')
for y in [-.30,.30]: box('rim',(.27,y,.90),(.72,.12,.06),'Ceramic_Stone')
box('basin_bottom',(.27,0,.71),(.61,.48,.05),'Ceramic_Stone')
for x in [-.06,.60]: box('basin_side',(x,0,.81),(.04,.48,.20),'Ceramic_Stone')
for y in [-.23,.23]: box('basin_end',(.27,y,.81),(.64,.04,.20),'Ceramic_Stone')
beam('tap_riser',(.27,.26,.93),(.27,.26,1.15),.028,mat='Iron_Black')
beam('tap_spout',(.27,.26,1.15),(.27,.07,1.15),.028,mat='Iron_Black')
anchor('work_surface',(-.42,0,.93),'Counter work surface')
anchor('approach',(0,-.90,0),'Counter access')

begin('feed_trough','Open livestock trough; interior clear 1.66 x 0.42 m')
for x in [-.62,.62]: box('foot',(x,0,.10),(.13,.73,.20),'Oak_Endgrain')
box('bottom',(0,0,.25),(1.8,.56,.08))
for y in [-.26,.26]: box('long_wall',(0,y,.46),(1.8,.10,.42))
for x in [-.85,.85]: box('end_wall',(x,0,.46),(.10,.44,.42))
for x in [-.65,.65]:
    for y in [-.318,.318]: box('iron_strap',(x,y,.46),(.045,.02,.39),'Iron_Black')
anchor('fill',(0,0,.56),'Interior fill presentation centre')

# Fence modules use exact post-centre snapping; terminal posts overlap at shared sockets.
def fence_post(x,y):
    box('post',(x,y,.65),(.16,.16,1.30),'Oak_Endgrain')
    box('post_cap',(x,y,1.315),(.19,.19,.05))
def rails(a,b):
    for z in [.48,.94]: beam('rail',(a[0],a[1],z),(b[0],b[1],z),.095,.12)

begin('fence_straight_2m','2 m post-centre modular fence; shared terminal posts may overlap')
fence_post(-1,0); fence_post(1,0); rails((-1,0),(1,0))
anchor('snap_left',(-1,0,0),'Connect module post centre')
anchor('snap_right',(1,0,0),'Connect module post centre')

begin('fence_corner_2m','L corner; each arm 2 m post-centre')
for x,y in [(0,0),(2,0),(0,2)]: fence_post(x,y)
rails((0,0),(2,0)); rails((0,0),(0,2))
anchor('snap_x',(2,0,0),'X-arm terminal post centre')
anchor('snap_y',(0,2,0),'Y-arm terminal post centre')
anchor('corner',(0,0,0),'Corner post placement pivot')

begin('fence_gate_2m','2 m post-centre gate; 1.84 m post clearance; pivoted editable leaf')
fence_post(-1,0); fence_post(1,0)
leaf=anchor('hinge',(-.88,0,.12),'Rotate this empty about Blender Z / Godot Y to swing leaf')
parts_start=len(current['objects'])
for x in [-.83,.83]: box('gate_stile',(x,0,.68),(.09,.09,1.06))
for z in [.25,1.08]: box('gate_rail',(0,0,z),(1.67,.09,.10))
for x in [-.50,-.17,.17,.50]: box('gate_picket',(x,0,.67),(.075,.065,.83))
beam('gate_diagonal',(-.80,-.065,.29),(.80,-.065,1.04),.07,.045)
box('latch',(.83,-.09,.86),(.18,.055,.045),'Iron_Black')
for o in current['objects'][parts_start:]:
    p=o.location.copy(); o.parent=leaf; o.location=p-leaf.location
for z in [.30,1.03]: box('hinge_strap',(-.88,-.067,z),(.24,.035,.045),'Iron_Black')
anchor('snap_left',(-1,0,0),'Left post centre'); anchor('snap_right',(1,0,0),'Right post centre')
anchor('passage',(0,0,0),'1.84 m clear between posts when leaf swung open')

# Concrete brief refinements: shaped parts, two body sizes and complete use sockets.
def delete_asset(a):
    for o in list(a['collection'].objects): bpy.data.objects.remove(o,do_unlink=True)
    bpy.data.collections.remove(a['collection']); assets.remove(a)

def mesh_part(name, verts, faces, mat, smooth=False):
    mesh=bpy.data.meshes.new(name); mesh.from_pydata(verts,[],faces); mesh.update()
    o=bpy.data.objects.new(name,mesh); current['collection'].objects.link(o); o.parent=current['root']
    mesh.materials.append(materials[mat]); current['objects'].append(o)
    bpy.ops.object.select_all(action='DESELECT'); o.select_set(True); bpy.context.view_layer.objects.active=o
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.normals_make_consistent(inside=False); bpy.ops.uv.smart_project(island_margin=.02)
    bpy.ops.object.mode_set(mode='OBJECT')
    for p in mesh.polygons: p.use_smooth=smooth
    mod=o.modifiers.new('Editable edge finish','BEVEL'); mod.width=.002; mod.segments=2
    return o

def turned_leg(name, x,y,h,r):
    profile=[(0,.78),(.025,1),(.10,.72),(.30,.56),(.60,.78),(.70,1),(.78,.85),(.88,1),(1,1)]
    vs=[(x+r*s*math.cos(j*math.tau/12),y+r*s*math.sin(j*math.tau/12),z*h) for z,s in profile for j in range(12)]
    fs=[tuple(reversed(range(12))),tuple(range((len(profile)-1)*12,len(profile)*12))]
    for i in range(len(profile)-1):
        for j in range(12): fs.append((i*12+j,i*12+(j+1)%12,(i+1)*12+(j+1)%12,(i+1)*12+j))
    return mesh_part(name,vs,fs,'Oak_Endgrain',True)

def cushion(name,center,size,mat):
    # Superellipsoid: broad flat centre, genuinely rounded fabric perimeter.
    vs=[]; fs=[]; rings=16; seg=40
    def sp(v,p): return math.copysign(abs(v)**p,v)
    for i in range(rings+1):
        t=-math.pi/2+math.pi*i/rings
        for j in range(seg):
            u=math.tau*j/seg
            vs.append((center[0]+size[0]/2*sp(math.cos(t),.25)*sp(math.cos(u),.25),
                       center[1]+size[1]/2*sp(math.cos(t),.25)*sp(math.sin(u),.25),
                       center[2]+size[2]/2*sp(math.sin(t),.5)))
    for i in range(rings):
        for j in range(seg): fs.append((i*seg+j,i*seg+(j+1)%seg,(i+1)*seg+(j+1)%seg,(i+1)*seg+j))
    o=mesh_part(name,vs,fs,mat,True)
    # Weld pole duplicates for closed manifold geometry.
    bpy.context.view_layer.objects.active=o
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT'); bpy.ops.mesh.remove_doubles(threshold=.00001); bpy.ops.object.mode_set(mode='OBJECT')
    return o

def seam(name,w,l,z,mat='Linen_Cream'):
    points=[]
    for i in range(80):
        t=math.tau*i/80
        points.append((w/2*math.copysign(abs(math.cos(t))**.25,math.cos(t)),l/2*math.copysign(abs(math.sin(t))**.25,math.sin(t)),z))
    vs=[]; fs=[]
    for i,p in enumerate(points):
        tangent=(Vector(points[(i+1)%80])-Vector(points[(i-1)%80])).normalized()
        side=Vector((tangent.y,-tangent.x,0))
        for j in range(6): vs.append(tuple(Vector(p)+.004*(side*math.cos(j*math.tau/6)+Vector((0,0,1))*math.sin(j*math.tau/6))))
    for i in range(80):
        for j in range(6): fs.append((i*6+j,i*6+(j+1)%6,((i+1)%80)*6+(j+1)%6,((i+1)%80)*6+j))
    return mesh_part(name,vs,fs,mat,True)

def duvet(w,l,top):
    nx,ny=24,36; vs=[]
    for j in range(ny+1):
        y=-l/2+.045+(l*.72)*j/ny
        for i in range(nx+1):
            u=-1+2*i/nx; x=u*(w/2+.045)
            fold=.014*math.sin(i*.9+j*.28)+.008*math.sin(j*.82-i*.32)
            drape=.095*max(0,(abs(u)-.84)/.16)**2
            vs.append((x,y,top+.027+fold-drape))
    fs=[]
    for j in range(ny):
        for i in range(nx):
            a=j*(nx+1)+i; fs.append((a,a+1,a+nx+2,a+nx+1))
    o=mesh_part('duvet_sculpted_folds',vs,fs,'Blanket_Indigo',True)
    mod=o.modifiers.new('Editable textile thickness','SOLIDIFY'); mod.thickness=.012
    return o

old=next(a for a in assets if a['name']=='bed_single'); delete_asset(old)
for name,w,l,top in [('bed_standard',1.05,2.20,.48),('bed_large',1.30,2.65,.55)]:
    begin(name,'Bed with rounded mattress, piped seams, slatted support and modeled folded duvet')
    current['dimensions_contract']={'mattress_width_m':w,'mattress_length_m':l,'mattress_top_m':top}
    for x in [-w/2-.035,w/2+.035]:
        for y in [-l/2,l/2]: turned_leg('turned_bed_foot',x,y,top-.17,.065)
    for x in [-w/2-.03,w/2+.03]: box('mortised_side_rail',(x,0,top-.23),(.08,l+.14,.16))
    for y in [-l/2,l/2]: box('end_rail',(0,y,top-.23),(w+.14,.075,.16))
    for i in range(12): box('support_slat',(0,-l/2+.08+i*(l-.16)/11,top-.19),(w+.02,.095,.035),'Oak_Endgrain')
    cushion('mattress',(0,0,top-.085),(w,l,.17),'Linen_Cream')
    seam('mattress_upper_piping',w*.985,l*.985,top-.025)
    seam('mattress_lower_piping',w*.985,l*.985,top-.145)
    cushion('pillow',(0,l*.34,top+.075),(w*.72,.43,.14),'Linen_Cream')
    duvet(w,l,top)
    for x in [-w/2-.03,w/2+.03]: box('head_post',(x,l/2,.58),(.09,.09,1.16),'Oak_Endgrain')
    for z in [.65,.83,1.01]: box('head_slat',(0,l/2,z),(w+.03,.055,.12))
    for x in [-w/2-.03,w/2+.03]:
        for z in [.65,1.01]:
            o=box('joinery_peg',(x,l/2-.05,z),(.024,.018,.024),'Oak_Endgrain'); o.rotation_euler.y=math.pi/4

# Rename canonical exports, exact furniture heights.
for a in assets:
    names={'dining_table':'table','writing_desk':'desk','open_shelf':'pantry'}
    if a['name'] in names:
        a['name']=names[a['name']]; a['root'].name=a['name']; a['collection'].name=a['name']
    current=a
    if a['name'] in ['chair','bench','table','desk']:
        for o in list(a['objects']):
            if o.type=='MESH' and o.name.startswith('leg'):
                x,y,h=o.location.x,o.location.y,o.dimensions.z
                a['objects'].remove(o); bpy.data.objects.remove(o,do_unlink=True)
                turned_leg('turned_leg',x,y,h,.042 if a['name']!='chair' else .033)
    if a['name']=='table':
        for o in a['objects']:
            if o.type=='MESH' and o.name.startswith('top_'): o.location.z-=.01
    if a['name']=='desk':
        for o in a['objects']:
            if o.type=='MESH' and o.name.startswith('desktop_'): o.location.z-=.03
    if a['name']=='chair':
        # Replace flat seat planks with sculpted concave saddle, solid underside.
        for o in list(a['objects']):
            if o.type=='MESH' and o.name.startswith('seat_'):
                a['objects'].remove(o); bpy.data.objects.remove(o,do_unlink=True)
        vs=[]; fs=[]; n=12
        for j in range(n+1):
            for i in range(n+1):
                x=-.25+.5*i/n; y=-.25+.5*j/n
                vs.append((x,y,.46+.016*((x/.25)**2+(y/.25)**2)))
        for j in range(n):
            for i in range(n):
                q=j*(n+1)+i; fs.append((q,q+1,q+n+2,q+n+1))
        o=mesh_part('curved_seat',vs,fs,'Oak_Honey',True)
        mod=o.modifiers.new('Seat thickness','SOLIDIFY'); mod.thickness=.04
        for o in a['objects']:
            if o.type=='MESH' and o.name.startswith('back_slat'):
                # Subdivide before curvature so slats genuinely bow, not only translate.
                bpy.ops.object.select_all(action='DESELECT'); o.select_set(True); bpy.context.view_layer.objects.active=o
                bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT'); bpy.ops.mesh.subdivide(number_cuts=6); bpy.ops.object.mode_set(mode='OBJECT')
                for v in o.data.vertices: v.co.y+=.05*(1-(v.co.x/.20)**2)
    if a['name']=='pantry':
        for x in [-.245,.245]:
            box('lower_door',(x,-.23,.36),(.475,.04,.44),'Paint_Sage')
            box('door_pull',(x*.28,-.27,.45),(.025,.045,.105),'Iron_Black')

# Larger chair duplicates editable parts, not a scaled runtime instance.
base=next(a for a in assets if a['name']=='chair')
begin('chair_large','Wide armless chair for intended tall-body range; 0.65 m seat width, 0.55 m seat centre')
for src in base['objects']:
    if src.type!='MESH': continue
    o=src.copy(); o.data=src.data.copy(); current['collection'].objects.link(o); o.parent=current['root']
    o.location.x*=1.3; o.location.y*=1.2; o.location.z*=.55/.46
    for v in o.data.vertices: v.co.x*=1.3; v.co.y*=1.2; v.co.z*=.55/.46
    current['objects'].append(o)
current['dimensions_contract']={'seat_width_m':.65,'seat_centre_top_m':.55,'armrests':False}
base['dimensions_contract']={'seat_width_m':.5,'seat_centre_top_m':.46,'armrests':False}

# Fence total height is 1.20 m. Gate post inner faces precisely +/- 0.80 m.
for a in assets:
    if not a['name'].startswith('fence'): continue
    for o in a['objects']:
        o.location.z*=1.2/1.34
        if o.type=='MESH':
            for v in o.data.vertices: v.co.z*=1.2/1.34
        if a['name']=='fence_gate_2m':
            o.location.x*=.88
            if o.type=='MESH':
                # Keep posts .16 wide while reducing span; other pieces scale.
                if not o.name.startswith('post'):
                    for v in o.data.vertices: v.co.x*=.88
    a['dimensions_contract']={'height_m':1.2}
    if a['name']=='fence_gate_2m':
        a['purpose']='Gate: 1.60 m post-face walk opening, 1.76 m post-centre span; articulated leaf, no animation'
        a['dimensions_contract']['clear_opening_m']=1.6
        for k,d in a['anchors'].items():
            d['blender_xyz_m'][0]*=.88; d['blender_xyz_m'][2]*=1.2/1.34
            p=d['blender_xyz_m']; d['godot_xyz_m']=[p[0],p[2],-p[1]]
        a['anchors']['passage']['purpose']='1.60 m clear between posts with gate leaf open'
begin('fence_post','Standalone terminal post, 1.20 m height')
fence_post(0,0)
for o in current['objects']:
    o.location.z*=1.2/1.34
    if o.type=='MESH':
        for v in o.data.vertices: v.co.z*=1.2/1.34
current['dimensions_contract']={'height_m':1.2}

# Stable floor-centre origins, including asymmetric L module.
for a in assets:
    current=a
    if a['name']=='fence_corner_2m':
        for o in a['objects']: o.location-=Vector((1,1,0))
        for d in a['anchors'].values():
            p=d['blender_xyz_m']; p[0]-=1; p[1]-=1; d['godot_xyz_m']=[p[0],p[2],-p[1]]
    a['use_slots']=[]
    def slot(label,pose,approach,exit,rect,facing=(0,0,1)):
        for kind,p in [('Approach',approach),('Pose',pose),('Exit',exit)]:
            ob=anchor(label+'_'+kind,p,'Authoring socket only; no allocation, rig or animation guarantee')
            ob.name=a['name']+'_'+label+'_'+kind
            a['anchors'][label+'_'+kind]['node_name']=ob.name
            ob['facing_godot_xyz']=list(facing); ob['clear_space_blender_xywh_m']=list(rect)
        a['use_slots'].append({'id':label,'nodes':[a['name']+'_'+label+'_'+k for k in ['Approach','Pose','Exit']],
                               'facing_godot_xyz':list(facing),'clear_space_blender_xywh_m':list(rect),
                               'pose_semantics':'Contact/support target, not calibrated skeletal pelvis'})
    n=a['name']
    if n.startswith('bed_'):
        w=a['dimensions_contract']['mattress_width_m']; l=a['dimensions_contract']['mattress_length_m']; top=a['dimensions_contract']['mattress_top_m']
        slot('Sleep',(0,0,top),(w/2+.5,-.3,0),(w/2+.5,.45,0),(w/2+.12,-l/2,.85,l),(0,0,-1))
    elif n in ['chair','chair_large']:
        h=.46 if n=='chair' else .55
        slot('Seat',(0,0,h),(0,-.9,0),(.65,-.75,0),(-.5,-1.25,1,1))
    elif n=='bench':
        for i,x in enumerate([-.42,.42]): slot('Seat'+str(i+1),(x,0,.46),(x,-.85,0),(x+(-.25 if i==0 else .25),-.85,0),(x-.39,-1.2,.78,.9))
    elif n in ['desk','table']:
        slot('Work',(0,0,.75),(0,-1.1,0),(.9,-1.1,0),(-.8,-1.55,1.6,.9),(0,0,-1))
        if n=='table':
            for i,(x,y) in enumerate([(-.5,-.86),(.5,-.86),(-.5,.86),(.5,.86)]):
                side=1 if y>0 else -1
                slot('Dining'+str(i+1),(x,y,.46),(x,y+side*.65,0),(x+side*.35,y+side*.65,0),(x-.4,y+side*.4-.4,.8,.8),(0,0,side))
        a['dimensions_contract']={'worktop_m':.75}
    elif n in ['wardrobe','kitchen_counter','pantry','feed_trough']:
        slot('Use',(0,-.6,0),(0,-1.15,0),(.8,-1.15,0),(-.8,-1.6,1.6,1.0),(0,0,-1))
    elif n=='fence_gate_2m': slot('Passage',(0,0,0),(0,-1.1,0),(0,1.1,0),(-.8,-1.6,1.6,3.2),(0,0,-1))
    # Retire provisional sockets so only complete use triplets advertise use.
    for key in list(a['anchors']):
        if key in ['approach','lie','sit'] or key.startswith('seat_') or key.startswith('sit_'):
            node=bpy.data.objects.get(a['anchors'][key]['node_name'])
            if node: a['objects'].remove(node); bpy.data.objects.remove(node,do_unlink=True)
            del a['anchors'][key]
    if n in ['table','desk']:
        for key in ['surface','work_surface']:
            if key in a['anchors']: bpy.data.objects[a['anchors'][key]['node_name']].location.z=.75
    bpy.data.objects[a['anchors']['origin']['node_name']].location=(0,0,0)
    bpy.context.view_layer.update()
    for d in a['anchors'].values():
        ob=bpy.data.objects[d['node_name']]; p=a['root'].matrix_world.inverted()@ob.matrix_world.translation
        d['blender_xyz_m']=list(p); d['godot_xyz_m']=[p.x,p.z,-p.y]
    a['root']['use_slots_json']=json.dumps(a['use_slots'])
    a['root']['body_range_m']='1.30-2.25 intended; no mannequin/IK verification'
    a['root']['dimensions_contract_json']=json.dumps(a.get('dimensions_contract',{}))

# Export each asset at its own origin before arranging editable source gallery.
receipt={'units':'metres','blender_to_godot':'(x,y,z) -> (x,z,-y), standard glTF Y-up export',
 'material_policy':'Portable Principled base-color/roughness/metallic, no external textures; editable UV0 on every mesh',
 'scope':'Neutral props only. No character assets, occupancy mechanics, collision or game scene wiring.', 'assets':[]}
for idx,a in enumerate(assets):
    bpy.ops.object.select_all(action='DESELECT')
    for o in [a['root']]+a['objects']: o.select_set(True)
    bpy.context.view_layer.update()
    pts=[o.matrix_world@Vector(v) for o in a['objects'] if o.type=='MESH' for v in o.bound_box]
    lo=[min(p[i] for p in pts) for i in range(3)]; hi=[max(p[i] for p in pts) for i in range(3)]
    meshes=[o for o in a['objects'] if o.type=='MESH']
    assert all(o.data.uv_layers and len(o.data.uv_layers.active.data)==len(o.data.loops) for o in meshes)
    path=OUT/(a['name']+'.glb')
    bpy.ops.export_scene.gltf(filepath=str(path),export_format='GLB',use_selection=True,export_apply=True,export_yup=True,export_extras=True)
    raw=path.read_bytes(); length,kind=struct.unpack_from('<II',raw,12); gltf=json.loads(raw[20:20+length])
    primitives=[p for m in gltf['meshes'] for p in m['primitives']]
    assert all('TEXCOORD_0' in p['attributes'] and 'material' in p for p in primitives)
    expected={d['node_name'] for d in a['anchors'].values()}
    assert expected.issubset({n.get('name') for n in gltf['nodes']})
    triangles=sum(gltf['accessors'][p['indices']]['count']//3 for p in primitives)
    item={'name':a['name'],'purpose':a['purpose'],'glb':path.name,'bounds_blender_m':{'min':lo,'max':hi},
          'dimensions_blender_xyz_m':[round(hi[i]-lo[i],4) for i in range(3)],
          'source_mesh_objects':len(meshes),'glb_meshes':len(gltf['meshes']),'glb_triangles':triangles,
          'uv0_verified':True,'material_slots':sorted({m.name for o in meshes for m in o.data.materials}),
          'anchors':a['anchors'],'use_slots':a['use_slots'],'dimensions_contract':a.get('dimensions_contract',{}),'normals_exported_verified':all('NORMAL' in p['attributes'] for p in primitives),'sha256':hashlib.sha256(raw).hexdigest()}
    receipt['assets'].append(item)
    a['root'].location=((idx%4)*3.6,(idx//4)*3.9,0)
    a['root']['gallery_offset_only']=True
    print('EXPORT_OK',a['name'],len(meshes),triangles)

# Non-exported studio presentation in its own collection.
studio=bpy.data.collections.new('_Studio_NotExported'); scene.collection.children.link(studio)
def studio_obj(o):
    for c in list(o.users_collection): c.objects.unlink(o)
    studio.objects.link(o)
for a in assets:
    bpy.ops.object.text_add(location=a['root'].location+Vector((-.85,-1.52,.012)))
    o=bpy.context.object; o.name='Label_'+a['name']; o.data.body=a['name'].replace('_',' ').upper(); o.data.size=.15
    o.data.extrude=.001; o.data.materials.append(materials['Iron_Black']); studio_obj(o)
bpy.ops.mesh.primitive_plane_add(size=200,location=(5,4,-.012)); ground=bpy.context.object; studio_obj(ground)
m=bpy.data.materials.new('Studio_Sand'); m.diffuse_color=(.15,.17,.18,1); ground.data.materials.append(m)
world=bpy.data.worlds.new('StudioWorld'); scene.world=world; world.use_nodes=True
world.node_tree.nodes['Background'].inputs[0].default_value=(.30,.36,.43,1)
world.node_tree.nodes['Background'].inputs[1].default_value=.5
for loc,power,size in [((2,-4,12),2300,8),((12,5,9),1800,7),((-5,6,6),1200,5)]:
    bpy.ops.object.light_add(type='AREA',location=loc); o=bpy.context.object; o.data.energy=power; o.data.shape='DISK'; o.data.size=size
    o.rotation_euler=(Vector((5,4,0))-o.location).to_track_quat('-Z','Y').to_euler(); studio_obj(o)
bpy.ops.object.camera_add(location=(17,-20,24)); camera=bpy.context.object; studio_obj(camera)
camera.rotation_euler=(Vector((5.3,5.2,.4))-camera.location).to_track_quat('-Z','Y').to_euler()
camera.data.type='ORTHO'; camera.data.ortho_scale=23; scene.camera=camera
scene.render.engine='CYCLES'; scene.cycles.device='CPU'; scene.cycles.samples=24; scene.cycles.use_denoising=True
scene.render.resolution_x=1800; scene.render.resolution_y=1400; scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'; scene.render.filepath=str(OUT/'ranch_furniture_contact_sheet.png')
scene['asset_count']=len(assets); scene['editing_notes']='Each collection is one prop. Mesh parts and bevel modifiers remain editable. Gallery offsets are root-only; individual GLBs are origin-local.'
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'ranch_furniture.blend'))
(OUT/'receipts.json').write_text(json.dumps(receipt,indent=2),encoding='utf-8')
bpy.ops.render.render(write_still=True)
# Detail view makes folds, seam geometry, headboard joinery and turned feet inspectable.
for a in assets:
    a['collection'].hide_render=a['name'] not in ['bed_standard','bed_large']
    if a['name'] in ['bed_standard','bed_large']:
        a['root'].location=(0,0,0) if a['name']=='bed_standard' else (2.1,0,0)
for o in studio.objects:
    if o.type=='FONT': o.hide_render=True
camera.location=(5,-6,4.8); camera.rotation_euler=(Vector((1.0,0,.45))-camera.location).to_track_quat('-Z','Y').to_euler()
camera.data.ortho_scale=5.9; scene.render.resolution_x=1400; scene.render.resolution_y=1100
scene.render.filepath=str(OUT/'beds_detail.png'); bpy.ops.render.render(write_still=True)
# Reopen saved source and verify persisted editable data, not merely in-memory construction.
bpy.ops.wm.open_mainfile(filepath=str(OUT/'ranch_furniture.blend'))
checks=[]
for a in receipt['assets']:
    coll=bpy.data.collections[a['name']]; ms=[o for o in coll.objects if o.type=='MESH']
    assert len(ms)==a['source_mesh_objects']
    assert all(o.data.uv_layers and any(m.type=='BEVEL' for m in o.modifiers) for o in ms)
    checks.append({'asset':a['name'],'saved_mesh_count':len(ms),'editable_bevels_and_uv0':True})
(OUT/'source_verification.json').write_text(json.dumps({'blend_reopened':True,'asset_count':len(checks),'checks':checks},indent=2),encoding='utf-8')
print('KIT_VERIFIED',len(checks))
