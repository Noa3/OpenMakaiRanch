import bpy, os, math

ASSET_DIR = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d"
MM = r"E:\OpenMakaiRanch\.artifacts\material_maker"
WOOD = "w03_painted_wood_siding"

def make_wood_material(albedo, normal, orm):
    mat = bpy.data.materials.new("WoodPBR")
    mat.use_nodes = True
    nt = mat.node_tree
    nt.nodes.clear()
    bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled")
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    out.location = (400, 0)
    nt.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    bsdf.inputs["Metallic"].default_value = 0.0

    def tex(path, loc):
        n = nt.nodes.new("ShaderNodeTexImage")
        n.image = bpy.data.images.load(path)
        n.location = loc
        return n

    alb = tex(os.path.join(MM, f"{WOOD}_albedo.png"), (-600, 300))
    nt.links.new(alb.outputs["Color"], bsdf.inputs["Base Color"])
    nrm = tex(os.path.join(MM, f"{WOOD}_normal.png"), (-600, -150))
    nrm.image.colorspace_settings.name = "Non-Color"
    nm = nt.nodes.new("ShaderNodeNormalMap")
    nm.location = (-300, -150)
    nt.links.new(nrm.outputs["Color"], nm.inputs["Color"])
    nt.links.new(nm.outputs["Normal"], bsdf.inputs["Normal"])
    ormimg = tex(os.path.join(MM, f"{WOOD}_orm.png"), (-600, -500))
    ormimg.image.colorspace_settings.name = "Non-Color"
    sep = nt.nodes.new("ShaderNodeSeparateColor")
    sep.location = (-300, -500)
    nt.links.new(ormimg.outputs["Color"], sep.inputs["Color"])
    nt.links.new(sep.outputs["Green"], bsdf.inputs["Roughness"])
    nt.links.new(sep.outputs["Blue"], bsdf.inputs["Metallic"])
    return mat

wood = make_wood_material(*[f"{WOOD}_{s}" for s in ("albedo","normal","orm")])

def apply_wood_to_glb(glb_name):
    path = os.path.join(ASSET_DIR, glb_name)
    if not os.path.exists(path):
        print(f"SKIP {glb_name} (missing)")
        return
    # clear all scene data
    for block in (bpy.data.meshes, bpy.data.materials, bpy.data.images, bpy.data.armatures):
        for b in list(block):
            if b.users == 0:
                block.remove(b)
    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o, do_unlink=True)
    bpy.ops.import_scene.gltf(filepath=path, merge_vertices=True)
    count = 0
    for obj in bpy.data.objects:
        if obj.type == 'MESH' and obj.data.materials:
            # replace every material slot with the wood PBR material
            for i in range(len(obj.data.materials)):
                obj.data.materials[i] = wood
            count += 1
    bpy.ops.export_scene.gltf(filepath=path, export_format='GLB', use_selection=False, export_apply=True)
    print(f"WOOD_BAKED {glb_name} meshes={count}")

for g in ("fence.glb", "signpost.glb", "crates_woodpile.glb", "pasture_boundary.glb"):
    apply_wood_to_glb(g)
print("ALL_WOOD_BAKED_OK")
