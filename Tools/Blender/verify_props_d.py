# -*- coding: utf-8 -*-
"""Verify each ART-001d GLB's mesh object count (the deliverable, not a preview)."""
import bpy, os
ASSETS = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d"
for name in ("signpost.glb", "crates_woodpile.glb", "grass_tufts.glb", "hedge.glb"):
    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o, do_unlink=True)
    before = set(o.name for o in bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=os.path.join(ASSETS, name))
    meshes = [o for o in bpy.data.objects if o.name not in before and o.type == "MESH"]
    # total triangle count
    tris = sum(len(o.data.polygons) for o in meshes if o.data)
    print("CHECK", name, "meshes=", len(meshes), "tris=", tris,
          "names=", [o.name for o in meshes][:8])
print("COUNT_DONE")
