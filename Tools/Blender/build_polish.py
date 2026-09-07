# -*- coding: utf-8 -*-
"""ART-001e polish assets:
 1) ground_grass.png  — 1024x1024 procedural grass albedo (noise + colour variation)
 2) pasture_boundary.glb — fence loop (back + 2 sides, front left open) to enclose the field
Both non-adult. Exported for Godot."""
import bpy, os, math, bmesh

ASSETS = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d"

def clear_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o, do_unlink=True)

# =====================================================================
# 1) GRASS ALBEDO PNG (procedural, headless render of a textured plane)
# =====================================================================
clear_scene()
bpy.ops.mesh.primitive_plane_add(size=1, location=(0, 0, 0))
g = bpy.context.active_object
mat = bpy.data.materials.new("GrassTex")
mat.use_nodes = True
nt = mat.node_tree
bsdf = nt.nodes["Principled BSDF"]
# procedural grass: layered noise (broad patches + fine blade detail) + directional streaks
tex_coord = nt.nodes.new("ShaderNodeTexCoord")
mapping = nt.nodes.new("ShaderNodeMapping")
mapping.inputs["Scale"].default_value = (1, 3, 1)  # stretch -> directional blade streaks
# broad patches (dark/light grass)
noise = nt.nodes.new("ShaderNodeTexNoise")
noise.inputs["Scale"].default_value = 28.0
noise.inputs["Detail"].default_value = 10.0
noise.inputs["Roughness"].default_value = 0.55
ramp = nt.nodes.new("ShaderNodeValToRGB")
ramp.color_ramp.elements[0].position = 0.28
ramp.color_ramp.elements[0].color = (0.10, 0.20, 0.05, 1.0)   # dark shadowed grass
ramp.color_ramp.elements[1].position = 0.72
ramp.color_ramp.elements[1].color = (0.52, 0.66, 0.26, 1.0)   # sunlit grass
e_mid = ramp.color_ramp.elements.new(0.5)
e_mid.color = (0.30, 0.44, 0.16, 1.0)
# fine blade-level speckle
noise2 = nt.nodes.new("ShaderNodeTexNoise")
noise2.inputs["Scale"].default_value = 900.0
noise2.inputs["Detail"].default_value = 3.0
ramp2 = nt.nodes.new("ShaderNodeValToRGB")
ramp2.color_ramp.elements[0].position = 0.42
ramp2.color_ramp.elements[0].color = (0.6, 0.6, 0.6, 1.0)
ramp2.color_ramp.elements[1].position = 0.58
ramp2.color_ramp.elements[1].color = (1.0, 1.0, 1.0, 1.0)
# mix base colour with the fine detail (multiply-ish via Mix)
# NOTE: ShaderNodeMix has DUPLICATE "A"/"B"/"Result" socket names (float/vector/color)
# — name lookup returns the FLOAT socket (wrong). Explicit indices:
#   inputs 0=Factor(float) 1=A(float) 2=B(float) 3=A(vector) 4=B(vector)
#           5=A(color) 6=B(color)
#   outputs 0=Result(float) 1=Result(vector) 2=Result(color)
mix = nt.nodes.new("ShaderNodeMix")
mix.data_type = "RGBA"
mix.blend_type = "MULTIPLY"
mix.inputs[0].default_value = 0.45  # Factor (float)
# Pick the RGBA sockets by name+type (robust across Blender versions)
def rgba_sock(sockets):
    for s in sockets:
        if s.type == "RGBA" and s.name not in ("Factor",):
            return s
    return None
a_color = [s for s in mix.inputs if s.type == "RGBA" and s.name == "A"][0]
b_color = [s for s in mix.inputs if s.type == "RGBA" and s.name == "B"][0]
res_color = [s for s in mix.outputs if s.type == "RGBA"][0]
nt.links.new(tex_coord.outputs["Object"], mapping.inputs["Vector"])
nt.links.new(mapping.outputs["Vector"], noise.inputs["Vector"])
nt.links.new(noise.outputs["Fac"], ramp.inputs["Fac"])
nt.links.new(mapping.outputs["Vector"], noise2.inputs["Vector"])
nt.links.new(noise2.outputs["Fac"], ramp2.inputs["Fac"])
nt.links.new(ramp.outputs["Color"], a_color)
nt.links.new(ramp2.outputs["Color"], b_color)
nt.links.new(res_color, bsdf.inputs["Base Color"])
bsdf.inputs["Roughness"].default_value = 0.98
g.data.materials.append(mat)

# ortho top-down camera for a clean albedo capture
cam_d = bpy.data.cameras.new("Cam")
cam_d.type = "ORTHO"
cam_d.ortho_scale = 1.05
cam = bpy.data.objects.new("Cam", cam_d); bpy.context.collection.objects.link(cam)
cam.location = (0, 0, 3)
bpy.context.scene.camera = cam

# neutral light so the albedo reads true (no shading)
if not bpy.data.worlds:
    bpy.data.worlds.new("World")
world = bpy.data.worlds["World"] if "World" in bpy.data.worlds else bpy.data.worlds.new("World")
bpy.context.scene.world = world
world.use_nodes = True
bg = world.node_tree.nodes["Background"]
bg.inputs["Color"].default_value = (1, 1, 1, 1)
bg.inputs["Strength"].default_value = 1.0
# disable shadows where the property exists (version-safe)
for o in bpy.data.objects:
    if o.type == "MESH":
        try:
            o.visible_shadow = False
        except Exception:
            pass

sc = bpy.context.scene
sc.render.engine = "CYCLES"  # CYCLES is proven headless-safe (EEVEE can render empty)
sc.cycles.samples = 64
sc.render.resolution_x = 1024
sc.render.resolution_y = 1024
out = os.path.join(ASSETS, "ground_grass.png")
sc.render.filepath = out
bpy.ops.render.render(write_still=True)
print("TEXTURE_OK", out, os.path.getsize(out) if os.path.exists(out) else "MISSING")

# =====================================================================
# 2) PASTURE BOUNDARY FENCE (back + 2 sides, front open)
# =====================================================================
clear_scene()

def mat(name, color, rough=0.9):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    m.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (*color, 1.0)
    m.node_tree.nodes["Principled BSDF"].inputs["Roughness"].default_value = rough
    return m

m_wood = mat("Wood", (0.40, 0.28, 0.16))
m_dark = mat("DarkWood", (0.28, 0.19, 0.11))

def box(name, size, loc, material):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.scale = (size[0]/2, size[1]/2, size[2]/2)
    bpy.ops.object.transform_apply(scale=True)
    o.data.materials.append(material)
    return o

objs = []
# field bounds (Godot-ish): x in [-16,16], z in [-12,12]; front (z>0 side toward player) open
seg_len = 4.0
def fence_run(cx, cz, along, count, rot_y):
    """count fence segments centred at (cx,cz), laid along 'x' or 'z'."""
    for i in range(count):
        t = (i - (count - 1)/2) * seg_len
        if along == "x":
            px, pz = cx + t, cz
        else:
            px, pz = cx, cz + t
        # two posts + two rails per segment
        for side in (-seg_len/2, seg_len/2):
            if along == "x":
                ploc = (px + side, 0, pz); rloc = (px + side/2, 0, pz)
                post = box(f"Post_{px}_{side}", (0.14, 0.14, 1.1), ploc, m_dark)
                rail = box(f"Rail_{px}_{side}", (seg_len, 0.07, 0.12), rloc, m_wood)
            else:
                ploc = (px, 0, pz + side); rloc = (px, 0, pz + side/2)
                post = box(f"Postz_{px}_{side}", (0.14, 0.14, 1.1), ploc, m_dark)
                rail = box(f"Railz_{px}_{side}", (0.07, seg_len, 0.12), rloc, m_wood)
            objs.extend([post, rail])

# back edge (z = -12): run along x
fence_run(0, -12, "x", 8, 0)
# left edge (x = -16): run along z
fence_run(-16, 0, "z", 6, 0)
# right edge (x = 16): run along z
fence_run(16, 0, "z", 6, 0)

path = os.path.join(ASSETS, "pasture_boundary.glb")
for o in bpy.data.objects:
    o.select_set(False)
for o in objs:
    o.select_set(True)
bpy.context.view_layer.objects.active = objs[0]
bpy.ops.export_scene.gltf(filepath=path, export_format='GLB', use_selection=True,
                          export_apply=True, export_yup=True)
print("EXPORT_OK pasture_boundary.glb", os.path.getsize(path))
print("ALL_POLISH_OK")
