# -*- coding: utf-8 -*-
"""Preview ART-001c props by importing the built GLBs (preview == deliverable)."""
import bpy, os
ASSETS = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d"

for o in list(bpy.data.objects):
    bpy.data.objects.remove(o, do_unlink=True)
for block in (bpy.data.meshes, bpy.data.materials):
    for b in list(block):
        if b.users == 0:
            block.remove(b)

bpy.ops.mesh.primitive_plane_add(size=40, location=(0, 0, 0))
ground = bpy.context.active_object
gmat = bpy.data.materials.new("Ground")
gmat.use_nodes = True
gb = gmat.node_tree.nodes.get("Principled BSDF")
gb.inputs["Base Color"].default_value = (0.22, 0.40, 0.16, 1.0)
ground.data.materials.append(gmat)


def import_glb(path, loc):
    bpy.ops.import_scene.gltf(filepath=path)
    objs = [o for o in bpy.context.selected_objects]
    for o in objs:
        o.location = (o.location.x + loc[0], o.location.y + loc[1], o.location.z + loc[2])


import_glb(os.path.join(ASSETS, "hay_bale.glb"), (-3.0, 0.5, 0.0))
import_glb(os.path.join(ASSETS, "water_trough.glb"), (-0.5, -0.5, 0.0))
import_glb(os.path.join(ASSETS, "tree_broadleaf.glb"), (2.0, 0.5, 0.0))
import_glb(os.path.join(ASSETS, "path_stones.glb"), (4.5, -0.3, 0.0))

sun = bpy.data.lights.new("Sun", type="SUN"); sun.energy = 3.5
s = bpy.data.objects.new("Sun", sun); bpy.context.collection.objects.link(s)
s.rotation_euler = (0.9, 0.2, 0.6)
world = bpy.data.worlds.get("World") or bpy.data.worlds.new("World")
bpy.context.scene.world = world
world.use_nodes = True
bg = world.node_tree.nodes.get("Background")
if bg:
    bg.inputs["Color"].default_value = (0.55, 0.65, 0.85, 1.0)
    bg.inputs["Strength"].default_value = 0.6

cam_data = bpy.data.cameras.new("Cam")
cam = bpy.data.objects.new("Cam", cam_data)
bpy.context.collection.objects.link(cam)
cam.location = (5.0, 7.5, 3.8)
tc = bpy.data.objects.new("CamTarget", None)
bpy.context.collection.objects.link(tc)
tc.location = (0.5, 0, 0.9)
con = cam.constraints.new("TRACK_TO"); con.target = tc
bpy.context.scene.camera = cam

bpy.context.scene.render.engine = "CYCLES"
bpy.context.scene.cycles.samples = 32
bpy.context.scene.render.resolution_x = 1000
bpy.context.scene.render.resolution_y = 620
out = r"E:\OpenMakaiRanch\Tools\Blender\props_preview.png"
bpy.context.scene.render.filepath = out
bpy.ops.render.render(write_still=True)
print("RENDER_OK", out, os.path.getsize(out))
