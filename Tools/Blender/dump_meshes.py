import bpy, os, sys
args = sys.argv
glb = args[args.index("--")+1] if "--" in args else None
if not glb: sys.exit()
AD = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d"
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=os.path.join(AD, glb))
print("=== MESHES in", glb)
for o in bpy.data.objects:
    if o.type=="MESH" and o.data and o.data.materials and o.data.materials[0]:
        m=o.data.materials[0]
        has_alb = any(n.node_type=="TEX_IMAGE" and "albedo" in n.image.name.lower() for n in m.node_tree.nodes) if m.node_tree else False
        print(f"  {o.name:24} mat={m.name:12} baked_pbr={has_alb}")
    elif o.type=="MESH":
        print(f"  {o.name:24} (no material)")