"""Build brief-exact editable habitable shells. Run with headless Blender CPU.
Blender +Y is north, exported Godot -Z. No furniture, gameplay or layout edits.
"""
import bpy, bmesh, math, json, struct, hashlib
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'OpenMakaiRanchGame/assets/3d/ranch_house'
PROOF=ROOT/'OpenMakaiRanchGame/docs/art/ranch_house'
OUT.mkdir(parents=True,exist_ok=True);PROOF.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
mats={}
for name,col in {'Plaster':(.80,.72,.57),'Timber':(.26,.135,.065),'Floor':(.49,.33,.18),'Roof':(.15,.27,.28),'Trim':(.55,.64,.48),'Tile':(.60,.65,.60),'Brass':(.61,.41,.17)}.items():
    m=bpy.data.materials.new(name);m.diffuse_color=(*col,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*col,1);p.inputs['Roughness'].default_value=.76;mats[name]=m
assets={};current=None;doors=[];windows=[];name_counts={}
def group(name):
    global current
    current=bpy.data.collections.new(name);scene.collection.children.link(current);assets[name]=current

def box(name,loc,size,mat='Plaster',collision=True,bevel=.015):
    name_counts[name]=name_counts.get(name,0)+1
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc);o=bpy.context.object;o.name=f'{name}_{name_counts[name]:03d}'+('-col' if collision else '')
    o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    for c in list(o.users_collection):c.objects.unlink(o)
    current.objects.link(o);o.data.materials.append(mats[mat])
    uv=o.data.uv_layers.active
    for p in o.data.polygons:
        axis=max(range(3),key=lambda k:abs(p.normal[k]));axes=[k for k in range(3) if k!=axis]
        for li in p.loop_indices:
            co=o.data.vertices[o.data.loops[li].vertex_index].co;uv.data[li].uv=(co[axes[0]]/2,co[axes[1]]/2)
    if bevel:
        mod=o.modifiers.new('Crafted_edge_bevel','BEVEL');mod.width=bevel;mod.segments=2
    return o

def part(name,axis,fixed,a,b,z0,z1,t=.2,mat='Plaster',collision=True):
    if b-a<.0001 or z1-z0<.0001:return
    loc=((a+b)/2,fixed,(z0+z1)/2) if axis=='X' else (fixed,(a+b)/2,(z0+z1)/2)
    size=(b-a,t,z1-z0) if axis=='X' else (t,b-a,z1-z0)
    return box(name,loc,size,mat,collision)

def wall(name,axis,fixed,start,end,holes=(),t=.2):
    cursor=start
    for centre,width,bottom,top,kind in sorted(holes):
        a,b=centre-width/2,centre+width/2
        part(name+'_pier',axis,fixed,cursor,a,0,2.95,t)
        part(name+'_below',axis,fixed,a,b,0,bottom,t)
        part(name+'_lintel',axis,fixed,a,b,top,2.95,t)
        if kind=='door':
            # Raw aperture 1.41 x 2.78 -> 1.25 x 2.70 after 80mm frames.
            part(name+'_DoorFrame_L',axis,fixed,a,a+.08,0,top,.28,'Timber')
            part(name+'_DoorFrame_R',axis,fixed,b-.08,b,0,top,.28,'Timber')
            part(name+'_DoorFrame_Top',axis,fixed,a+.08,b-.08,top-.08,top,.28,'Timber')
            doors.append({'asset':current.name,'name':name,'axis':axis,'fixed_blender':fixed,'center':centre,'clear_width':width-.16,'clear_height':top-.08,'clear_interval':[a+.08,b-.08]})
        else:
            part(name+'_WindowFrame_L',axis,fixed,a,a+.065,bottom,top,.28,'Trim',False)
            part(name+'_WindowFrame_R',axis,fixed,b-.065,b,bottom,top,.28,'Trim',False)
            part(name+'_WindowSill',axis,fixed,a-.12,b+.12,bottom-.07,bottom,.4,'Timber',False)
            part(name+'_WindowHead',axis,fixed,a,b,top-.065,top,.28,'Trim',False)
            part(name+'_WindowMullion',axis,fixed,centre-.025,centre+.025,bottom,top,.13,'Timber',False)
            windows.append({'asset':current.name,'name':name,'axis':axis,'center':centre,'fixed_blender':fixed,'raw_width':width,'sill':bottom,'top':top})
        cursor=b
    part(name+'_pier',axis,fixed,cursor,end,0,2.95,t)

def door(c):return(c,1.41,0,2.78,'door')
def win(c,w=1.8):return(c,w,1.0,2.55,'window')

def shell(prefix,w,d):
    box(prefix+'_Floor',(0,0,-.12),(w,d,.24),'Floor',True,.0)
    wall(prefix+'_SouthExterior','X',-d/2+.1,-w/2,w/2,[win(-w/2+2),door(0),win(w/2-2)])
    wall(prefix+'_NorthExterior','X',d/2-.1,-w/2,w/2,[win(-w/2+2),door(0),win(w/2-2)])
    for side in [-1,1]:
        wall(prefix+'_SideExterior','Y',side*(w/2-.1),-d/2+.2,d/2-.2,[win(-d/2+2),win(0),win(d/2-2)])
    for x in [-w/2+.08,w/2-.08]:
        for y in [-d/2+.08,d/2-.08]:box(prefix+'_CornerTimber',(x,y,1.475),(.18,.18,2.95),'Timber',False)
    for y in [-d/2+.07,d/2-.07]:box(prefix+'_HeaderTimber',(0,y,2.87),(w,.18,.16),'Timber',False)
    # Floor plank joints are authored thin dark inlays, not a procedural runtime material.
    for i in range(1,int(w/.5)):
        box(prefix+'_FloorJoint',(-w/2+i*.5,0,.002),(.012,d-.3,.004),'Timber',False,0)

def roof(prefix,w,d,centres,rise):
    box('Roof_'+prefix+'_Ceiling',(0,0,3.0),(w-.4,d-.4,.1),'Plaster',True,0)
    span=w/len(centres);half=span/2+.24;angle=math.atan2(rise,half);eave=3.12
    for ix,cx in enumerate(centres):
        for iy,y in enumerate([-d/4-.11,d/4+.11]):
            for sign in [-1,1]:
                ob=box(f'Roof_{prefix}_Section_{ix}_{iy}_{sign}',(cx+sign*half/2,y,eave+rise/2),(math.hypot(half,rise),d/2+.22,.14),'Roof');ob.rotation_euler.y=sign*angle
                for j in range(int((d/2+.4)/.7)+1):
                    yy=y-d/4-.15+j*.7
                    rib=box(f'Roof_{prefix}_Seam',(cx+sign*half/2,yy,eave+rise/2+.082),(math.hypot(half,rise),.025,.025),'Roof',False,.0);rib.rotation_euler.y=sign*angle
            box('Roof_'+prefix+'_Ridge',(cx,y,eave+rise+.08),(.17,d/2+.22,.14),'Brass',False)
        for y in [-d/2+.1,d/2-.1]:
            # Solid gable prism with explicit recalculated outward normals.
            verts=[]
            for yy in [y-.07,y+.07]:verts.extend([(cx-span/2,yy,2.95),(cx+span/2,yy,2.95),(cx,yy,eave+rise-.08)])
            mesh=bpy.data.meshes.new('Gable');mesh.from_pydata(verts,[],[(0,2,1),(3,4,5),(0,1,4,3),(1,2,5,4),(2,0,3,5)]);mesh.update()
            bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(mesh);bm.free()
            ob=bpy.data.objects.new(f'Roof_{prefix}_Gable_{ix}_{y}-col',mesh);current.objects.link(ob);mesh.materials.append(mats['Plaster']);uv=mesh.uv_layers.new()
            for p in mesh.polygons:
                axis=max(range(3),key=lambda k:abs(p.normal[k]));axes=[k for k in range(3)if k!=axis]
                for li in p.loop_indices:
                    co=mesh.vertices[mesh.loops[li].vertex_index].co;uv.data[li].uv=(co[axes[0]]/2,co[axes[1]]/2)

group('main_house');shell('Main',20,16)
# Front (Godot +Z = Blender -Y): office, entry, kitchen.
for x in [-3,3]:wall('FrontRoomPartition','Y',x,-7.8,-3)
wall('FrontCommunalBoundary','X',-3,-9.8,9.8,[door(-6.5),door(0),door(6.5)])
# Rear private rooms each open directly to communal living; no through bedrooms.
wall('RearCommunalBoundary','X',3,-9.8,9.8,[door(-7),door(-2.5),door(0),door(3.25),door(7.75)])
for x in [-4,-1,1,5.5]:wall('RearRoomPartition','Y',x,3.1,7.8)
# Flush continuous floor; furniture/material worker can zone finish surfaces.
roof('Main',20,16,[-5,5],2.0)
# Wing is separate, local origin at its centre.
group('residential_wing');shell('Wing',12,12)
for x in [-1,1]:wall('WingHallPartition','Y',x,-5.8,5.8,[door(-3),door(3)])
for a,b in [(-5.8,-1.1),(1.1,5.8)]:wall('WingRoomPartition','X',0,a,b)
roof('Wing',12,12,[-3,3],1.7)
group('north_connector_cap');box('RemovableNorthConnectorCap',(0,7.9,1.35),(1.25,.18,2.7),'Plaster')
# Validate source mesh normals, UV areas, aperture clearance against actual collider boxes.
receipt={'units':'metres','blender_north':'+Y','godot_front':'+Z','main_footprint_m':[20,16],'main_bounds_godot':{'x':[-10,10],'z':[-8,8]},'wing_footprint_m':[12,12],'wing_mount_godot':[0,0,-14],'north_connector_godot':[0,0,-8],'wing_south_connector_local_godot':[0,0,6],'clear_room_height_m':2.95,'door_clear_after_frames_m':[1.25,2.7],'wing_hall_partition_centrelines_x':[-1,1],'wing_hall_clear_width_m':1.8,'room_layout_godot':{'office':[-10,-3,3,8],'kitchen':[3,10,3,8],'entrance':[-3,3,3,8],'communal':[-10,10,-3,3],'player':[-10,-4,-8,-3],'bath':[-4,-1,-8,-3],'connector':[-1,1,-8,-3],'resident_A':[1,5.5,-8,-3],'resident_B':[5.5,10,-8,-3]},'notes':['Rooms are nominal planning extents; wall thickness reduces clear area.','Four wing private modules; base player room + two resident rooms do not cover unlimited recruits.','No furniture, articulated pose, resident assignment, gameplay unlock or game integration claim.','Open window apertures with mullions, no glass panes.','All structural mesh nodes use Godot -col; hide visible MeshInstance3D for cutaway, retain collider children.'],'doors':doors,'windows':windows,'assets':{}}
for name,col in assets.items():
    bad_volume=[];bad_uv=[];objects=[]
    for o in col.objects:
        bm=bmesh.new();bm.from_mesh(o.data)
        if bm.calc_volume(signed=True)<=0:bad_volume.append(o.name)
        bm.free();uv=o.data.uv_layers.active
        for p in o.data.polygons:
            coords=[uv.data[i].uv for i in p.loop_indices];area=abs(sum(a.x*b.y-b.x*a.y for a,b in zip(coords,coords[1:]+coords[:1])))/2
            if area<1e-9:bad_uv.append(o.name)
        points=[o.matrix_world@Vector(v) for v in o.bound_box]
        objects.append({'name':o.name,'bounds_blender_m':[[round(min(p[i] for p in points),5) for i in range(3)],[round(max(p[i] for p in points),5) for i in range(3)]],'collision':o.name.endswith('-col')})
    assert not bad_volume and not bad_uv,(bad_volume,bad_uv)
    bpy.ops.object.select_all(action='DESELECT')
    for o in col.objects:o.select_set(True)
    path=OUT/f'{name}.glb';bpy.ops.export_scene.gltf(filepath=str(path),export_format='GLB',use_selection=True,export_apply=True,export_normals=True,export_texcoords=True,export_yup=True)
    raw=path.read_bytes();length,kind=struct.unpack_from('<II',raw,12);g=json.loads(raw[20:20+length]);prims=[p for m in g['meshes']for p in m['primitives']]
    assert all('NORMAL'in p['attributes']and'TEXCOORD_0'in p['attributes']for p in prims)
    receipt['assets'][name]={'sha256':hashlib.sha256(raw).hexdigest(),'bytes':len(raw),'mesh_count':len(g['meshes']),'triangles':sum(g['accessors'][p['indices']]['count']//3 for p in prims),'collision_mesh_nodes':sum(n.get('name','').endswith('-col')for n in g['nodes']),'missing_or_degenerate_uv_faces':len(bad_uv),'nonpositive_mesh_volumes':len(bad_volume),'all_glb_primitives_normal_uv':True,'objects':objects}
    print('EXPORT_OK',name,len(g['meshes']))
# Probe all final door apertures: test multiple points against actual source collision AABBs.
checks=[]
for d in doors:
    bounds=receipt['assets'][d['asset']]['objects'];blocked=[]
    for lateral in [-.615,0,.615]:
        for z in [.02,1.4,2.69]:
            p=[d['center']+lateral,d['fixed_blender'],z]if d['axis']=='X'else[d['fixed_blender'],d['center']+lateral,z]
            for ob in bounds:
                if not ob['collision']or ob['name'].startswith('Roof_'):continue
                lo,hi=ob['bounds_blender_m']
                if all(lo[i]+1e-5<p[i]<hi[i]-1e-5 for i in range(3)):blocked.append(ob['name'])
    checks.append({'door':d['name'],'asset':d['asset'],'center':d['center'],'probe_count':9,'blocked':blocked});assert not blocked,(d,blocked)
receipt['door_collision_clearance_checks']=checks
# Assembled editable source, wing moved north 14m. Cap parked hidden, not blocking passage.
for o in assets['residential_wing'].objects:o.location.y+=14
for o in assets['north_connector_cap'].objects:o.hide_render=True;o.hide_set(True)
world=bpy.data.worlds.new('Daylight');scene.world=world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.55,.65,.78,1);world.node_tree.nodes['Background'].inputs[1].default_value=.65
bpy.ops.object.light_add(type='AREA',location=(-10,-10,20));bpy.context.object.data.energy=3000;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=14
bpy.ops.object.light_add(type='SUN');bpy.context.object.rotation_euler=(.45,-.5,-.35);bpy.context.object.data.energy=2
bpy.ops.object.camera_add(location=(31,-37,35));cam=bpy.context.object;scene.camera=cam;cam.data.type='ORTHO';cam.data.ortho_scale=44
cam.rotation_euler=(Vector((0,5,1))-cam.location).to_track_quat('-Z','Y').to_euler()
scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=20;scene.cycles.use_denoising=True
scene.render.resolution_x=1440;scene.render.resolution_y=1080;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX';scene.render.image_settings.file_format='PNG'
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'ranch_house_editable.blend'))
scene.render.filepath=str(PROOF/'exterior.png');bpy.ops.render.render(write_still=True)
for o in bpy.data.objects:
    if o.name.startswith('Roof_'):o.hide_render=True
cam.location=(26,-30,40);cam.rotation_euler=(Vector((0,5,0))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=41
scene.render.filepath=str(PROOF/'cutaway.png');bpy.ops.render.render(write_still=True)
cam.location=(0,6,45);cam.rotation_euler=(0,0,0);cam.data.ortho_scale=35
scene.render.resolution_x=1200;scene.render.resolution_y=1400;scene.render.filepath=str(PROOF/'floorplan.png');bpy.ops.render.render(write_still=True)
receipt['blend_sha256']=hashlib.sha256((OUT/'ranch_house_editable.blend').read_bytes()).hexdigest()
(PROOF/'geometry_receipt.json').write_text(json.dumps(receipt,indent=2))
print('BUILD_COMPLETE',OUT)
