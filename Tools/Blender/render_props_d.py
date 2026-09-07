# -*- coding: utf-8 -*-
"""Preview ART-001d props by importing the built GLBs (preview == deliverable)."""
import bpy, os
ASSETS = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d"

# clean
for o in list(bpy.data.objects):
    bpy.data.objects.remove(o, do_unlink=True)

placements = (
    ("signpost.glb", (-3.0, 0.0, 0.0)),
    ("crates_woodpile.glb", (-0.5, 0.0, 0.0)),
    ("grass_tufts.glb", (2.5, 0.0, 0.0)),
    ("hedge.glb", (0.0, 2.0, 0.0)),
)
for fname, off in placements:
    before = set(o.name for o in bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=os.path.join(ASSETS, fname))
    for o in bpy.data.objects:
        if o.name not in before and o.type == "MESH":
            o.location = (o.location[0] + off[0], o.location[1] + off[1], o.location[2] + off[2])

# ground
bpy.ops.mesh.primitive_plane_add(size=40, location=(0, 1.0, 0))
g = bpy.context.active_object
gm = bpy.data.materials.new("G"); gm.use_nodes = True
gm.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0.30, 0.42, 0.20, 1.0)
g.data.materials.append(gm)

# sun
sun_d = bpy.data.lights.new("Sun", type="SUN"); sun_d.energy = 3.0
sun = bpy.data.objects.new("Sun", sun_d); bpy.context.collection.objects.link(sun)
sun.rotation_euler = (0.9, 0.15, 0.5)

# camera aimed at the props row
cam_d = bpy.data.cameras.new("Cam")
cam_d.lens = 28  # wide, to frame the full props row
cam = bpy.data.objects.new("Cam", cam_d); bpy.context.collection.objects.link(cam)
cam.location = (0, 5.5, 2.4)
tgt = bpy.data.objects.new("CamTarget", None); bpy.context.collection.objects.link(tgt)
tgt.location = (0, 0.5, 0.5)
tc = cam.constraints.new("TRACK_TO"); tc.target = tgt
tc.track_axis = "TRACK_NEGATIVE_Z"; tc.up_axis = "UP_Y"
bpy.context.scene.camera = cam

sc = bpy.context.scene
sc.render.engine = "CYCLES"
sc.cycles.samples = 64
sc.render.resolution_x = 1280; sc.render.resolution_y = 720
out = r"E:\OpenMakaiRanch\Tools\Blender\props_d_preview.png"
sc.render.filepath = out
bpy.ops.render.render(write_still=True)
print("RENDER_OK", out, os.path.getsize(out) if os.path.exists(out) else "MISSING")
