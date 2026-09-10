# -*- coding: utf-8 -*-
"""ART-001d: signpost, crates + woodpile, grass tufts, low hedge. Non-adult world assets."""
import bpy, bmesh, math
from mathutils import Vector

ASSET_DIR = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d"


def clear_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o, do_unlink=True)
    for c in list(bpy.data.collections):
        bpy.data.collections.remove(c)


def mat(name, color, rough=0.85):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    b = m.node_tree.nodes["Principled BSDF"]
    b.inputs["Base Color"].default_value = (*color, 1.0)
    b.inputs["Roughness"].default_value = rough
    return m


def box(name, size, loc, material):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.scale = (size[0] / 2, size[1] / 2, size[2] / 2)
    bpy.ops.object.transform_apply(scale=True)
    o.data.materials.append(material)
    return o


def cyl(name, r, depth, loc, material, verts=16):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=depth, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.data.materials.append(material)
    return o


def cone(name, r, depth, loc, material, verts=16):
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r, radius2=0.0, depth=depth, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.data.materials.append(material)
    return o


def export_glb(objs, fname):
    for o in bpy.data.objects:
        o.select_set(False)
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    path = f"{ASSET_DIR}/{fname}"
    bpy.ops.export_scene.gltf(filepath=path, export_format='GLB', use_selection=True,
                              export_apply=True, export_yup=True)
    print("EXPORT_OK", fname, __import__("os").path.getsize(path))


def make_materials():
    # (Re)create the material set. clear_scene()'s read_factory_settings() removes
    # data blocks, so this must run after every clear_scene().
    return {
        "wood": mat("Wood", (0.42, 0.30, 0.17), 0.85),
        "darkwood": mat("DarkWood", (0.30, 0.20, 0.12), 0.9),
        "grass": mat("GrassTuft", (0.22, 0.45, 0.15), 0.95),
        "flower": mat("Flower", (0.85, 0.55, 0.8), 0.8),
        "hedge": mat("Hedge", (0.18, 0.42, 0.20), 0.95),
        "plank": mat("Plank", (0.50, 0.36, 0.22), 0.85),
    }


def new_asset():
    clear_scene()
    return make_materials()


# ------------------------------------------------------------------ signpost
M = new_asset()
m_wood = M["wood"]; m_darkwood = M["darkwood"]; m_plank = M["plank"]
objs = []
post = cyl("Sign_Post", 0.05, 1.4, (0, 0, 0.7), m_darkwood, 10)
plank1 = box("Sign_Plank1", (0.7, 0.05, 0.28), (0.1, 0, 1.15), m_wood)
plank2 = box("Sign_Plank2", (0.6, 0.05, 0.24), (-0.05, 0, 0.82), m_wood)
for o in (plank1, plank2):
    o.rotation_euler = (0, 0, 0.3)
cap = cyl("Sign_Cap", 0.07, 0.06, (0, 0, 1.43), m_darkwood, 10)
objs += [post, plank1, plank2, cap]
export_glb(objs, "signpost.glb")

# ------------------------------------------------------------------ crates + woodpile
M = new_asset()
m_wood = M["wood"]; m_darkwood = M["darkwood"]
objs = []
# two stacked crates (slightly rotated for natural look)
c1 = box("Crate_Bottom", (0.5, 0.5, 0.5), (0, 0, 0.25), m_wood)
c2 = box("Crate_Top", (0.42, 0.42, 0.42), (0.03, 0.02, 0.71), m_wood)
c2.rotation_euler = (0, 0, 0.4)
# woodpile: 6 logs in a 2-3-1 pyramid beside the crates
log_r = 0.09
log_l = 1.0
positions = [
    (-0.75, 0.25, log_r), (-0.75, 0.25 + 2 * log_r * 0.9, log_r),
    (-0.75, 0.25 - 2 * log_r * 0.9, log_r),
    (-0.75, 0.25 + log_r, log_r + log_r * 1.6),
    (-0.75, 0.25 - log_r, log_r + log_r * 1.6),
    (-0.75, 0.25, log_r + 2 * log_r * 1.6),
]
for i, p in enumerate(positions):
    log = cyl(f"Log_{i}", log_r, log_l, p, m_darkwood, 10)
    log.rotation_euler = (0, math.radians(90), 0)  # rotate Z-axis -> X (lay on side)
    objs.append(log)
objs += [c1, c2]
export_glb(objs, "crates_woodpile.glb")

# ------------------------------------------------------------------ grass tufts (instanced patch)
M = new_asset()
m_grass = M["grass"]; m_flower = M["flower"]
objs = []
import random
random.seed(7)
tuft_positions = [(random.uniform(-0.6, 0.6), random.uniform(-0.6, 0.6)) for _ in range(7)]
for i, (tx, tz) in enumerate(tuft_positions):
    # each tuft = 3 thin cones fanned out
    for j in range(3):
        ang = random.uniform(0, 6.28)
        lean = random.uniform(0.15, 0.35)
        h = random.uniform(0.12, 0.22)
        blade = cone(f"Tuft_{i}_{j}", 0.03, h,
                     (tx + math.cos(ang) * 0.03, tz + math.sin(ang) * 0.03, h / 2),
                     m_grass, 6)
        blade.rotation_euler = (lean * math.cos(ang), lean * math.sin(ang), 0)
        objs.append(blade)
    # one flower on some tufts
    if i % 2 == 0:
        f = cyl(f"FlowerStem_{i}", 0.008, 0.14, (tx, tz, 0.07), m_grass, 6)
        objs.append(f)
        p = cyl(f"Flower_{i}", 0.035, 0.03, (tx, tz, 0.16), m_flower, 8)
        objs.append(p)
export_glb(objs, "grass_tufts.glb")

# ------------------------------------------------------------------ low hedge (long soft box)
M = new_asset()
m_hedge = M["hedge"]
objs = []
hedge = box("Hedge_Body", (2.2, 0.5, 0.55), (0, 0, 0.27), m_hedge)
# rounded top: scale a cylinder to an ellipse and squash
bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=1, depth=2.2, location=(0, 0, 0.4))
top = bpy.context.active_object
top.name = "Hedge_Top"
top.scale = (1, 0.25, 0.2)
bpy.ops.object.transform_apply(scale=True)
top.data.materials.append(m_hedge)
objs = [hedge, top]
export_glb(objs, "hedge.glb")

print("ALL_PROPS_D_OK")
