import bpy, os, sys
# argv: blender --background --python bake_one.py -- <glb_name>
glb = None
args = sys.argv
if "--" in args:
    glb = args[args.index("--") + 1]
    if args.index("--") + 2 < len(args):
        MM = args[args.index("--") + 2]
if not glb or not glb.endswith(".glb"):
    print("NO_GLB_ARG"); sys.exit(1)
ASSET_DIR = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d"
MM = r"E:\OpenMakaiRanch\.artifacts\material_maker"
WOOD = "w03_painted_wood_siding"
path = os.path.join(ASSET_DIR, glb)
if not os.path.exists(path):
    print(f"SKIP {glb}"); sys.exit(1)
# fresh scene
for o in list(bpy.data.objects):
    bpy.data.objects.remove(o, do_unlink=True)
bpy.ops.import_scene.gltf(filepath=path, merge_vertices=True)
mat = bpy.data.materials.new("WoodPBR"); mat.use_nodes = True
nt = mat.node_tree; nt.nodes.clear()
bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled"); out = nt.nodes.new("ShaderNodeOutputMaterial")
nt.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
bsdf.inputs["Metallic"].default_value = 0.0
def tex(p, loc):
    n = nt.nodes.new("ShaderNodeTexImage"); n.image = bpy.data.images.load(p); n.location = loc; return n
alb = tex(os.path.join(MM, f"{WOOD}_albedo.png"), (-600, 300))
nt.links.new(alb.outputs["Color"], bsdf.inputs["Base Color"])
nrm = tex(os.path.join(MM, f"{WOOD}_normal.png"), (-600, -150)); nrm.image.colorspace_settings.name = "Non-Color"
nm = nt.nodes.new("ShaderNodeNormalMap"); nm.location = (-300, -150)
nt.links.new(nrm.outputs["Color"], nm.inputs["Color"]); nt.links.new(nm.outputs["Normal"], bsdf.inputs["Normal"])
orm = tex(os.path.join(MM, f"{WOOD}_orm.png"), (-600, -500)); orm.image.colorspace_settings.name = "Non-Color"
sep = nt.nodes.new("ShaderNodeSeparateColor"); sep.location = (-300, -500)
nt.links.new(orm.outputs["Color"], sep.inputs["Color"])
nt.links.new(sep.outputs["Green"], bsdf.inputs["Roughness"]); nt.links.new(sep.outputs["Blue"], bsdf.inputs["Metallic"])
cnt = 0
for o in bpy.data.objects:
    if o.type == 'MESH' and o.data.materials:
        for i in range(len(o.data.materials)):
            o.data.materials[i] = mat
        cnt += 1
bpy.ops.export_scene.gltf(filepath=path, export_format='GLB', use_selection=False)
print(f"WOOD_BAKED {glb} meshes={cnt}")
