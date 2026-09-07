# -*- coding: utf-8 -*-
"""Verify each built GLB's object/mesh count (the deliverable, not a preview angle)."""
import bpy, os
ASSETS = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d"
for name in ("hay_bale", "water_trough", "tree_broadleaf", "path_stones"):
    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o, do_unlink=True)
    bpy.ops.import_scene.gltf(filepath=os.path.join(ASSETS, name + ".glb"))
    meshes = [o for o in bpy.data.objects if o.type == "MESH"]
    print(f"CHECK {name}: mesh_objects={len(meshes)} names={[m.name for m in meshes]}")
print("COUNT_DONE")
