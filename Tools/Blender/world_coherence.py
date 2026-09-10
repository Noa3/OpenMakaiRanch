# -*- coding: utf-8 -*-
"""ART-001e: reconstruct the full Godot scene (Godot coords -> Blender) for a
whole-world coherence render. Placements mirror RanchGreybox.tscn exactly.
Godot (x,y,z) Y-up  ->  Blender (x, z, y) Z-up."""
import bpy, os
ASSETS = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d"

for o in list(bpy.data.objects):
    bpy.data.objects.remove(o, do_unlink=True)

def g2b(x, y, z):
    return (x, z, y)  # Godot Y-up -> Blender Z-up

def place(fname, loc, rot_z_godot=0.0):
    before = set(o.name for o in bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=os.path.join(ASSETS, fname))
    off = g2b(*loc)
    import math
    for o in bpy.data.objects:
        if o.name not in before and o.type == "MESH":
            o.location = (o.location[0] + off[0], o.location[1] + off[1], o.location[2] + off[2])
            if rot_z_godot:
                o.rotation_euler = (o.rotation_euler[0], o.rotation_euler[1], o.rotation_euler[2] + math.radians(rot_z_godot))

# ground 40 x 30 (Godot X,Z) -> horizontal Blender plane (already lies in XY)
bpy.ops.mesh.primitive_plane_add(size=1, location=(0, 0, -0.5))
g = bpy.context.active_object
g.scale = (40, 30, 1)
gm = bpy.data.materials.new("G"); gm.use_nodes = True
gm.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0.34, 0.46, 0.22, 1.0)
gm.node_tree.nodes["Principled BSDF"].inputs["Roughness"].default_value = 0.95
g.data.materials.append(gm)

# station box (Godot) -> Blender cube
bpy.ops.mesh.primitive_cube_add(size=1, location=g2b(0, 0.5, 0))
st = bpy.context.active_object
st.scale = (1.5, 1.0, 0.6)
sm = bpy.data.materials.new("Station"); sm.use_nodes = True
sm.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0.55, 0.42, 0.30, 1.0)
st.data.materials.append(sm)

# all Scenery instances + barn, exact TSCN placements
place("barn.glb", (0, 0, -9))
place("well.glb", (-6, 0, -4))
place("tree.glb", (8, 0, 2))
place("tree.glb", (-10, 0, 6))
place("tree.glb", (12, 0, -8))
place("fence.glb", (2, 0, 8))
place("fence.glb", (10, 0, 4), rot_z_godot=90)
place("hay_bale.glb", (3.5, 0, -6))
place("hay_bale.glb", (4.7, 0, -5.4))
place("water_trough.glb", (7, 0, 6))
place("tree_broadleaf.glb", (-14, 0, -6))
place("tree_broadleaf.glb", (15, 0, 9))
place("path_stones.glb", (0.6, 0, 5.5), rot_z_godot=90)
place("signpost.glb", (2.5, 0, 12))
place("crates_woodpile.glb", (3.5, 0, -10.5))
place("grass_tufts.glb", (-8, 0, 6))
place("grass_tufts.glb", (6, 0, -3))
place("hedge.glb", (-4, 0, -14))

# sun
sun_d = bpy.data.lights.new("Sun", type="SUN"); sun_d.energy = 3.0
sun = bpy.data.objects.new("Sun", sun_d); bpy.context.collection.objects.link(sun)
sun.rotation_euler = (0.7, 0.2, 0.5)

# wide camera covering the 40 x 30 field
cam_d = bpy.data.cameras.new("Cam"); cam_d.lens = 35
cam = bpy.data.objects.new("Cam", cam_d); bpy.context.collection.objects.link(cam)
cam.location = g2b(0, 26, 14)  # behind, elevated
tgt = bpy.data.objects.new("T", None); bpy.context.collection.objects.link(tgt)
tgt.location = g2b(0, -2, 0)
tc = cam.constraints.new("TRACK_TO"); tc.target = tgt
tc.track_axis = "TRACK_NEGATIVE_Z"; tc.up_axis = "UP_Y"
bpy.context.scene.camera = cam

sc = bpy.context.scene
sc.render.engine = "CYCLES"
sc.cycles.samples = 96
sc.render.resolution_x = 1600; sc.render.resolution_y = 900
out = r"E:\OpenMakaiRanch\Tools\Blender\world_coherence.png"
sc.render.filepath = out
bpy.ops.render.render(write_still=True)
print("RENDER_OK", out, os.path.getsize(out) if os.path.exists(out) else "MISSING")
