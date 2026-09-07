# -*- coding: utf-8 -*-
"""Build low-poly ranch world assets: fence, tree, well. Export each as GLB.

All non-adult environment assets (replaces the pure-box world). Origin at each
asset's ground footprint centre; y-up in Godot (Blender z-up auto-converted by glTF).
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


def cone(name, r, depth, loc, material, verts=8):
    bpy.ops.mesh.primitive_cone_add(radius1=r, radius2=0.0, depth=depth, vertices=verts, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.data.materials.append(material)
    return o


def sphere(name, r, loc, material, seg=10, ring=8):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=r, location=loc, segments=seg, ring_count=ring)
    o = bpy.context.active_object
    o.name = name
    o.data.materials.append(material)
    bpy.ops.object.shade_smooth()
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


# ============================================================ FENCE
# One segment: 2 posts + 2 horizontal rails. ~1.6 m long, ~0.95 m tall.
reset_scene()
m_wood = mat("Fence_Wood", (0.35, 0.24, 0.13), rough=0.9)
seg_len = 1.6
post_h = 0.95
post_r = 0.05
rail_h = 0.05
objs = []
for px in (-seg_len / 2, seg_len / 2):
    objs.append(box(f"Fence_Post_{px:+.0f}", (0.09, 0.09, post_h), (px, 0, post_h / 2), m_wood))
for rh in (0.30, 0.62):
    objs.append(box(f"Fence_Rail_{rh:.2f}", (seg_len, 0.05, rail_h), (0, 0, rh), m_wood))
# small pointed post caps
for px in (-seg_len / 2, seg_len / 2):
    objs.append(cone(f"Fence_Cap_{px:+.0f}", 0.055, 0.12, (px, 0, post_h + 0.06), m_wood, verts=6))
export_glb(objs, "fence.glb")

# ============================================================ TREE
# Trunk (cylinder) + 3 stacked low-poly canopy cones. ~2.6 m tall.
reset_scene()
m_trunk = mat("Tree_Trunk", ((0.28, 0.19, 0.10)), rough=0.9)
m_leaf = mat("Tree_Leaf", ((0.16, 0.42, 0.14)), rough=0.9)
trunk_h = 1.1
objs = [cyl("Tree_Trunk", 0.14, trunk_h, (0, 0, trunk_h / 2), m_trunk, verts=8)]
cy = trunk_h
for i, (r, d) in enumerate([(0.85, 0.8), (0.62, 0.7), (0.40, 0.6)]):
    cy += d * 0.6
    objs.append(cone(f"Tree_Canopy_{i}", r, d, (0, 0, cy), m_leaf, verts=8))
export_glb(objs, "tree.glb")

# ============================================================ WELL
# Stone ring (cylinder) + inner dark + 2 posts + 2 rails + small gable roof.
reset_scene()
m_stone = mat("Well_Stone", ((0.45, 0.44, 0.40)), rough=0.95)
m_stone_dark = mat("Well_StoneDark", ((0.12, 0.11, 0.10)), rough=0.9)
m_roof = mat("Well_Roof", ((0.30, 0.18, 0.10)), rough=0.85)
ring_r = 0.75
ring_h = 0.5
objs = [cyl("Well_Ring", ring_r, ring_h, (0, 0, ring_h / 2), m_stone, verts=16)]
objs.append(cyl("Well_Water", ring_r - 0.12, 0.06, (0, 0, ring_h - 0.04), m_stone_dark, verts=16))
# posts at front/back of ring
post_h = 1.5
for py in (-ring_r, ring_r):
    objs.append(box(f"Well_Post_{py:+.0f}", (0.1, 0.1, post_h), (0, py, post_h / 2 + ring_h * 0.3), m_stone))
# cross rail near top
objs.append(box("Well_Rail", (0.1, 2 * ring_r, 0.08), (0, 0, post_h * 0.85 + ring_h * 0.3), m_stone))
# gable roof over the posts (ridge along Y, at x=0)
roof_w = 1.5
roof_d = 2 * ring_r + 0.6
roof_h = 0.55
ry = post_h * 0.85 + ring_h * 0.3 + 0.1
mesh = bpy.data.meshes.new("Well_Roof")
bm = bmesh.new()
w = roof_w / 2
d = roof_d / 2
h = roof_h
base = ry
e_fl = bm.verts.new((-w, -d, base))
e_fr = bm.verts.new(( w, -d, base))
e_bl = bm.verts.new((-w,  d, base))
e_br = bm.verts.new(( w,  d, base))
r_f  = bm.verts.new(( 0, -d, base + h))
r_b  = bm.verts.new(( 0,  d, base + h))
bm.faces.new((e_fl, e_bl, r_b, r_f))   # left slope (planar)
bm.faces.new((e_fr, e_br, r_b, r_f))   # right slope (planar)
bm.faces.new((e_fl, e_fr, r_f))       # front gable (triangle)
bm.faces.new((e_br, e_bl, r_b))       # back gable (triangle)
bm.to_mesh(mesh)
bm.free()
roof_obj = bpy.data.objects.new("Well_Roof", mesh)
bpy.context.collection.objects.link(roof_obj)
roof_obj.data.materials.append(m_roof)
# Consistent outward normals (a black gable face means inverted winding).
bpy.ops.object.select_all(action="DESELECT")
roof_obj.select_set(True)
bpy.context.view_layer.objects.active = roof_obj
bpy.ops.object.mode_set(mode="EDIT")
bpy.ops.mesh.select_all(action="SELECT")
bpy.ops.mesh.normals_make_consistent(inside=False)
bpy.ops.object.mode_set(mode="OBJECT")
objs.append(roof_obj)
export_glb(objs, "well.glb")

print("ALL_ASSETS_OK")
