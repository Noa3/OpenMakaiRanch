"""Reusable metre-scale calibration geometry; not final house/furniture artwork."""
from pathlib import Path
import bpy
import json

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'OpenMakaiRanchGame/assets/3d/scale_yard'
OUT.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.scene.unit_settings.system = 'METRIC'
bpy.context.scene.unit_settings.scale_length = 1.0
materials = {}
for name, rgba in {'Ivory':(.88,.85,.72,1),'Dark':(.10,.17,.20,1),'Ochre':(.82,.52,.18,1),'Wood':(.40,.26,.16,1),'Blue':(.23,.43,.55,1)}.items():
    material = bpy.data.materials.new(name)
    material.diffuse_color = rgba
    material.use_nodes = True
    material.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value = rgba
    material.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value = .82
    materials[name] = material


def box(name, position, size, material):
    # Convert Godot XYZ to Blender X,-Z,Y. Object scale stays one after construction.
    x,y,z=position; sx,sy,sz=size
    bpy.ops.mesh.primitive_cube_add(size=1,location=(x,-z,y))
    obj=bpy.context.object;obj.name=name;obj.dimensions=(sx,sz,sy)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    obj.data.materials.append(materials[material])
    return obj


box('CalibrationFloor-col',(0,-.05,0),(18,.1,8),'Ivory')
for x in (-6,-3,0,3,6):
    box('MetreStaff',(x,1.5,-1.4),(.075,3,.075),'Dark')
    for tick in range(31):
        major=tick%10==0
        box('Tick_%02d'%tick,(x+.15,tick/10,-1.36),(.35 if major else .16,.014,.025),'Ochre' if major else 'Dark')
# Temporary measuring samples with explicit CLEAR dimensions, no architecture claims.
for x in (4.34,5.66):box('DoorJamb',(x,1.4,1.1),(.12,2.8,.2),'Wood')
box('DoorHead',(5,2.76,1.1),(1.44,.12,.2),'Wood')
box('WallSample',(7,1.475,1.1),(1.2,2.95,.22),'Ivory')
for x in (-7,-4.5):box('FencePost',(x,.65,1.3),(.13,1.3,.13),'Wood')
for y in (.42,.98):box('FenceRail',(-5.75,y,1.3),(2.5,.13,.09),'Wood')
box('TableTop',(-2.8,.75-.035,1.8),(1.5,.07,.85),'Wood')
for x in (-3.43,-2.17):
    for z in (1.48,2.12):box('TableLeg',(x,.34,z),(.08,.68,.08),'Wood')
box('ChairSeat',(-2.8,.46-.025,2.7),(.50,.05,.48),'Blue')
for x in (-3,-2.6):
    for z in (2.5,2.9):box('ChairLeg',(x,.21,z),(.06,.42,.06),'Wood')
box('ChairBack',(-2.8,.78,2.92),(.5,.54,.06),'Wood')
box('BedMattress',(0,.48-.09,2),(1.05,.18,2.2),'Blue')
box('BedFrame',(0,.24,2),(1.16,.13,2.32),'Wood')
box('BedPillow',(0,.60,1.15),(.62,.10,.4),'Ivory')
for x in (-.48,.48):
    for z in (.98,3.02):box('BedLeg',(x,.1,z),(.09,.2,.09),'Wood')
# Mesh UVs come from actual primitive construction; no import/root scaling.
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'ranch_scale_yard.blend'))
bpy.ops.export_scene.gltf(filepath=str(OUT/'ranch_scale_yard.glb'),export_format='GLB',export_yup=True,export_cameras=False,export_lights=False)
receipt={'purpose':'temporary calibration, not finished modelling','metres_per_blender_unit':1,'door_clear_width':1.2,'door_clear_height':2.7,'wall_clear_height':2.95,'chair_seat_height':.46,'table_top_height':.75,'mattress_size':[1.05,2.2],'mattress_top':.48,'fence_height':1.3,'mesh_objects':len([o for o in bpy.context.scene.objects if o.type=='MESH']),'non_unit_object_scales':[o.name for o in bpy.context.scene.objects if tuple(o.scale)!=(1,1,1)]}
(OUT/'dimensions.json').write_text(json.dumps(receipt,indent=2),encoding='utf-8')
print('SCALE_YARD_EXPORTED',json.dumps(receipt),flush=True)
