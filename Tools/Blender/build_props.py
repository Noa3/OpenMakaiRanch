# -*- coding: utf-8 -*-
"""ART-001c: low-poly ranch props — hay bales, water trough, broadleaf tree, path stones.

All non-adult environment assets. Origin at each asset's ground footprint centre.
"""
import bpy, os, bmesh

OUT = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d"
os.makedirs(OUT, exist_ok=True)


def reset_scene():
    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o, do_unlink=True)
    for block in (bpy.data.meshes, bpy.data.materials):
        for b in list(block):
            if b.users == 0:
                block.remove(b)


def mat(name, color, rough=0.85, metal=0.0):
    m = bpy.data.materials.get(name)
    if m:
        return m
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    b = m.node_tree.nodes.get("Principled BSDF")
    if b:
        b.inputs["Base Color"].default_value = (*color, 1.0)
        b.inputs["Roughness"].default_value = rough
        b.inputs["Metallic"].default_value = metal
    return m


def box(name, size, loc, material):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.scale = (size[0], size[1], size[2])
    bpy.ops.object.transform_apply(scale=True)
    o.data.materials.append(material)
    return o


def cyl(name, r, depth, loc, material, verts=12):
    bpy.ops.mesh.primitive_cylinder_add(radius=r, depth=depth, vertices=verts, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.data.materials.append(material)
    return o


def sphere(name, r, loc, material, seg=10, ring=8):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=r, location=loc, segments=seg, ring_count=ring)
    o = bpy.context.active_object
    o.name = name
    o.data.materials.append(material)
    return o


def export_glb(objects, name):
    for o in bpy.data.objects:
        o.select_set(o in objects)
    if objects:
        bpy.context.view_layer.objects.active = objects[0]
    path = os.path.join(OUT, name)
    bpy.ops.export_scene.gltf(filepath=path, use_selection=True, export_format="GLB")
    for o in bpy.data.objects:
        o.select_set(False)
    print("EXPORT_OK", name, os.path.getsize(path))


# ============================================================ HAY BALE
# One rectangular bale (~1.2 x 0.9 x 0.9) with a lighter top "cut face".
reset_scene()
m_hay = mat("Hay", (0.72, 0.58, 0.25), rough=0.95)
m_hay_light = mat("HayLight", (0.80, 0.68, 0.34), rough=0.95)
objs = [box("Hay_Body", (1.2, 0.9, 0.9), (0, 0, 0.45), m_hay)]
# lighter top face strip (the cut end)
objs.append(box("Hay_Top", (0.12, 0.86, 0.86), (0.55, 0, 0.45), m_hay_light))
# a few thin band lines (darker straps)
m_strap = mat("HayStrap", (0.35, 0.28, 0.12), rough=0.9)
for sx in (-0.3, 0.3):
    objs.append(box(f"Hay_Strap_{sx:+.1f}", (0.04, 0.92, 0.92), (sx, 0, 0.45), m_strap))
export_glb(objs, "hay_bale.glb")

# ============================================================ WATER TROUGH
# Long open-top trough: base + 2 long side walls + 2 end walls. ~2.4 m long.
reset_scene()
m_wood = mat("Trough_Wood", (0.30, 0.21, 0.12), rough=0.9)
m_water = mat("Trough_Water", (0.25, 0.42, 0.55), rough=0.25)
L = 2.4
W = 0.6
H = 0.5
t = 0.06  # wall thickness
objs = [box("Trough_Base", (L, W, t), (0, 0, t / 2), m_wood)]
# long side walls
for sy in (-W / 2 + t / 2, W / 2 - t / 2):
    objs.append(box(f"Trough_Side_{sy:+.1f}", (L, t, H), (0, sy, H / 2), m_wood))
# end walls
for sx in (-L / 2 + t / 2, L / 2 - t / 2):
    objs.append(box(f"Trough_End_{sx:+.1f}", (t, W - 2 * t, H), (sx, 0, H / 2), m_wood))
# water surface (slightly inset, near the top)
objs.append(box("Trough_Water", (L - 2 * t, W - 2 * t, 0.03), (0, 0, H * 0.8), m_water))
export_glb(objs, "water_trough.glb")

# ============================================================ BROADLEAF TREE
# Trunk + 3 overlapping round canopy blobs (spheres), distinct from the conifer.
reset_scene()
m_trunk = mat("BLeaf_Trunk", (0.30, 0.20, 0.11), rough=0.9)
m_leaf = mat("BLeaf_Leaf", (0.20, 0.48, 0.18), rough=0.9)
trunk_h = 1.4
objs = [cyl("BLeaf_Trunk", 0.16, trunk_h, (0, 0, trunk_h / 2), m_trunk, verts=8)]
# canopy: 3 offset spheres
canopy_base = trunk_h + 0.2
objs.append(sphere("BLeaf_Canopy_A", 0.95, (0, 0, canopy_base + 0.5), m_leaf, seg=10, ring=8))
objs.append(sphere("BLeaf_Canopy_B", 0.7, (0.55, 0.25, canopy_base + 0.1), m_leaf, seg=10, ring=8))
objs.append(sphere("BLeaf_Canopy_C", 0.65, (-0.5, -0.3, canopy_base + 0.25), m_leaf, seg=10, ring=8))
export_glb(objs, "tree_broadleaf.glb")

# ============================================================ PATH STONES
# 3 flat irregular stepping stones in a loose line.
reset_scene()
m_stone = mat("PathStone", (0.48, 0.46, 0.42), rough=0.95)
m_stone2 = mat("PathStone2", (0.42, 0.40, 0.37), rough=0.95)
import math
positions = [(0, 0), (0.9, 0.25), (1.9, -0.1)]
objs = []
for i, (px, py) in enumerate(positions):
    # slightly irregular: two overlapping flattened cylinders
    r = 0.45 - i * 0.05
    objs.append(cyl(f"Stone_{i}_A", r, 0.08, (px, py, 0.04), m_stone if i % 2 == 0 else m_stone2, verts=7))
    objs.append(cyl(f"Stone_{i}_B", r * 0.7, 0.08, (px + 0.12, py + 0.08, 0.04), m_stone2 if i % 2 == 0 else m_stone, verts=7))
export_glb(objs, "path_stones.glb")

print("ALL_PROPS_OK")
