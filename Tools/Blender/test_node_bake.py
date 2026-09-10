import bpy, os
ASSET_DIR = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d"
MM = r"E:\OpenMakaiRanch\.artifacts\material_maker"
WOOD="w03_painted_wood_siding"
path = os.path.join(ASSET_DIR, "fence.glb")
for o in list(bpy.data.objects): bpy.data.objects.remove(o, do_unlink=True)
for b in list(bpy.data.meshes):
    if b.users==0: bpy.data.meshes.remove(b)
for b in list(bpy.data.materials):
    if b.users==0: bpy.data.materials.remove(b)
for b in list(bpy.data.images):
    if b.users==0: bpy.data.images.remove(b)
bpy.ops.import_scene.gltf(filepath=path, merge_vertices=True)
print("IMPORTED")
mat = bpy.data.materials.new("WoodPBR"); mat.use_nodes=True
nt=mat.node_tree; nt.nodes.clear()
bsdf=nt.nodes.new("ShaderNodeBsdfPrincipled"); out=nt.nodes.new("ShaderNodeOutputMaterial")
nt.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
bsdf.inputs["Metallic"].default_value=0.0
def tex(p,loc):
    n=nt.nodes.new("ShaderNodeTexImage"); n.image=bpy.data.images.load(p); n.location=loc; return n
alb=tex(os.path.join(MM,f"{WOOD}_albedo.png"),(-600,300))
nt.links.new(alb.outputs["Color"], bsdf.inputs["Base Color"])
print("ALBEDO_LINKED")
nrm=tex(os.path.join(MM,f"{WOOD}_normal.png"),(-600,-150))
nrm.image.colorspace_settings.name="Non-Color"
nm=nt.nodes.new("ShaderNodeNormalMap")
nt.links.new(nrm.outputs["Color"], nm.inputs["Color"]); nt.links.new(nm.outputs["Normal"], bsdf.inputs["Normal"])
print("NORMAL_LINKED")
orm=tex(os.path.join(MM,f"{WOOD}_orm.png"),(-600,-500))
orm.image.colorspace_settings.name="Non-Color"
sep=nt.nodes.new("ShaderNodeSeparateColor")
nt.links.new(orm.outputs["Color"], sep.inputs["Color"])
nt.links.new(sep.outputs["Green"], bsdf.inputs["Roughness"])
nt.links.new(sep.outputs["Blue"], bsdf.inputs["Metallic"])
print("ORM_LINKED")
for o in bpy.data.objects:
    if o.type=='MESH' and o.data.materials:
        for i in range(len(o.data.materials)): o.data.materials[i]=mat
print("ASSIGNED")
bpy.ops.export_scene.gltf(filepath=path, export_format='GLB', use_selection=False)
print("NODE_EXPORT_OK")
