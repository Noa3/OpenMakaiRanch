import bpy, os, sys
args = sys.argv
glb = args[args.index("--")+1] if "--" in args else None
if not glb: sys.exit()
AD = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d"
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=os.path.join(AD, glb))
print("=== ALL OBJECTS in", glb, "total=", len(bpy.data.objects))
for o in bpy.data.objects:
    nmat = 0
    if o.type=="MESH" and o.data and o.data.materials:
        nmat = len([m for m in o.data.materials if m is not None])
    print(f"  {o.name:28} type={o.type:8} mats={nmat}")