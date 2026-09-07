# -*- coding: utf-8 -*-
"""Build a low-poly ranch barn and export it as a GLB for Godot.

Non-adult, world-building asset (replaces the greybox box).
Dimensions in metres, origin at the barn's ground-centre.
"""
import bpy
import os

OUT_GLTF = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d\barn.glb"

# ---------------------------------------------------------------- clear scene
for ob in list(bpy.data.objects):
    bpy.data.objects.remove(ob, do_unlink=True)
for coll in (bpy.data.meshes, bpy.data.materials, bpy.data.lights, bpy.data.cameras):
    for d in list(coll):
        if d.users == 0:
            coll.remove(d)

# ---------------------------------------------------------------- materials
def flat_mat(name, color, rough=0.9, metal=0.0):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    b = m.node_tree.nodes.get("Principled BSDF")
    b.inputs["Base Color"].default_value = (*color, 1.0)
    b.inputs["Roughness"].default_value = rough
    b.inputs["Metallic"].default_value = metal
    return m

m_body   = flat_mat("BarnBody",   (0.42, 0.09, 0.07))   # dark barn red
m_roof   = flat_mat("BarnRoof",   (0.12, 0.11, 0.11), 0.85)
m_door   = flat_mat("BarnDoor",   (0.28, 0.18, 0.10))
m_trim   = flat_mat("BarnTrim",   (0.90, 0.88, 0.82), 0.7)
m_silo   = flat_mat("Silo",       (0.78, 0.80, 0.82), 0.6, 0.1)

# ---------------------------------------------------------------- helpers
def box(name, size, loc, mat):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    ob = bpy.context.active_object
    ob.name = name
    ob.scale = (size[0] / 2, size[1] / 2, size[2] / 2)
    bpy.ops.object.transform_apply(scale=True)
    if mat is not None:
        ob.data.materials.append(mat)
    bpy.ops.object.shade_flat()
    return ob

# ---------------------------------------------------------------- barn body
W, D, H = 8.0, 6.0, 4.0   # width (x), depth (y), wall height (z)
body = box("Barn_Body", (W, D, H), (0, 0, H / 2), m_body)

# ---------------------------------------------------------------- gable roof:
# ridge runs along Y at x=0; two slopes down to the wall tops; gable triangles front/back.
import bmesh
roof_mesh = bpy.data.meshes.new("Barn_Roof")
bm = bmesh.new()
o = 0.8          # roof overhang beyond the walls
rh = 2.2         # ridge height above wall top
bl = bm.verts.new((-W / 2 - o, -D / 2 - o, H))
fl = bm.verts.new((-W / 2 - o,  D / 2 + o, H))
br = bm.verts.new(( W / 2 + o, -D / 2 - o, H))
fr = bm.verts.new(( W / 2 + o,  D / 2 + o, H))
rs = bm.verts.new((0, -D / 2 - o, H + rh))   # ridge start (front)
re = bm.verts.new((0,  D / 2 + o, H + rh))   # ridge end (back)
bm.faces.new((bl, br, rs))          # front gable (triangle)
bm.faces.new((fr, fl, re))          # back gable (triangle)
bm.faces.new((bl, rs, re, fl))      # left slope
bm.faces.new((br, fr, re, rs))      # right slope
bm.to_mesh(roof_mesh)
bm.free()
roof = bpy.data.objects.new("Barn_Roof", roof_mesh)
bpy.context.collection.objects.link(roof)
roof.data.materials.append(m_roof)
bpy.ops.object.select_all(action="DESELECT")
roof.select_set(True); bpy.context.view_layer.objects.active = roof
bpy.ops.object.mode_set(mode="EDIT")
bpy.ops.mesh.select_all(action="SELECT")
bpy.ops.mesh.normals_make_consistent(inside=False)
bpy.ops.object.mode_set(mode="OBJECT")
bpy.ops.object.shade_flat()

# ---------------------------------------------------------------- door + trim (front, -Y)
door = box("Barn_Door", (2.2, 0.12, 3.0), (0, -D / 2 - 0.02, 1.5), m_door)
door_frame = box("Barn_DoorFrame", (2.7, 0.08, 3.5), (0, -D / 2 - 0.01, 1.6), m_trim)
# small loft window (front gable)
loft = box("Barn_LoftWindow", (1.0, 0.08, 0.9), (0, -D / 2 - 0.03, H + rh * 0.45), m_trim)

# ---------------------------------------------------------------- silo beside barn
silo = box("Barn_Silo", (1.8, 1.8, 7.0), (W / 2 + 1.6, -D / 2 + 1.2, 3.5), m_silo)
bpy.ops.object.select_all(action="DESELECT")
silo.select_set(True); bpy.context.view_layer.objects.active = silo
bpy.ops.mesh.primitive_cylinder_add(radius=0.9, depth=1.2, location=(W / 2 + 1.6, -D / 2 + 1.2, 7.0 + 0.6))
silo_top = bpy.context.active_object
silo_top.name = "Barn_SiloTop"
silo_top.data.materials.append(m_roof)
bpy.ops.object.shade_smooth()

# ---------------------------------------------------------------- export GLB
os.makedirs(os.path.dirname(OUT_GLTF), exist_ok=True)
bpy.ops.object.select_all(action="SELECT")
bpy.ops.export_scene.gltf(
    filepath=OUT_GLTF,
    use_selection=True,
    export_format="GLB",
    export_apply=True,
    export_yup=True,
)
print("EXPORT_OK", OUT_GLTF, os.path.getsize(OUT_GLTF) if os.path.exists(OUT_GLTF) else "MISSING")
print("OBJECTS", [o.name for o in bpy.data.objects])
