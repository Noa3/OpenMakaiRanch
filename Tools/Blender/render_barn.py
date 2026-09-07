# -*- coding: utf-8 -*-
"""Render the barn from an angled view to verify geometry (non-blocking visual check)."""
import bpy, os
# Re-run the build so the scene is populated deterministically.
exec(open(r"E:\OpenMakaiRanch\Tools\Blender\build_barn.py", encoding="utf-8").read())

out = r"E:\OpenMakaiRanch\Tools\Blender\barn_preview.png"

# Ground plane so the barn sits on a surface
bpy.ops.mesh.primitive_plane_add(size=60, location=(0, 0, 0))
ground = bpy.context.active_object
ground.name = "Ground"
gmat = bpy.data.materials.new("GroundMat")
gmat.use_nodes = True
gb = gmat.node_tree.nodes.get("Principled BSDF")
gb.inputs["Base Color"].default_value = (0.32, 0.42, 0.24, 1.0)
gb.inputs["Roughness"].default_value = 1.0
ground.data.materials.append(gmat)

# World environment (sky-like fill)
world = bpy.data.worlds.new("World")
bpy.context.scene.world = world
world.use_nodes = True
bg = world.node_tree.nodes.get("Background")
bg.inputs[0].default_value = (0.55, 0.65, 0.8, 1.0)
bg.inputs[1].default_value = 1.0

# Sun
sun_data = bpy.data.lights.new("Sun", type="SUN")
sun_data.energy = 4.0
sun = bpy.data.objects.new("Sun", sun_data)
bpy.context.collection.objects.link(sun)
sun.rotation_euler = (0.9, 0.2, 0.6)

# Track target at the barn's visual centre
target = bpy.data.objects.new("CamTarget", None)
target.location = (1.0, 0.0, 3.0)
bpy.context.collection.objects.link(target)

# Camera
cam_data = bpy.data.cameras.new("Cam")
cam = bpy.data.objects.new("Cam", cam_data)
bpy.context.collection.objects.link(cam)
cam.location = (16, 18, 10)
tc = cam.constraints.new("TRACK_TO")
tc.target = target
tc.track_axis = "TRACK_NEGATIVE_Z"
tc.up_axis = "UP_Y"
bpy.context.scene.camera = cam

bpy.context.scene.render.engine = "CYCLES"
bpy.context.scene.cycles.device = "CPU"
bpy.context.scene.cycles.samples = 32
bpy.context.scene.render.resolution_x = 960
bpy.context.scene.render.resolution_y = 720
bpy.context.scene.render.filepath = out
bpy.ops.render.render(write_still=True)
print("RENDER_OK", out, os.path.getsize(out) if os.path.exists(out) else "MISSING")
