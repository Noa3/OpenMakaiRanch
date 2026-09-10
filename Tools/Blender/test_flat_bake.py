import bpy, os
ASSET_DIR = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d"
def reset():
    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o, do_unlink=True)
    for b in list(bpy.data.meshes):
        if b.users == 0: bpy.data.meshes.remove(b)
    for b in list(bpy.data.materials):
        if b.users == 0: bpy.data.materials.remove(b)
    for b in list(bpy.data.images):
        if b.users == 0: bpy.data.images.remove(b)
path = os.path.join(ASSET_DIR, "fence.glb")
reset()
bpy.ops.import_scene.gltf(filepath=path, merge_vertices=True)
m = bpy.data.materials.new("FlatWood"); m.use_nodes = True
b = m.node_tree.nodes["Principled BSDF"]
b.inputs["Base Color"].default_value = (0.5, 0.3, 0.15, 1.0)
n=0
for o in bpy.data.objects:
    if o.type=='MESH' and o.data.materials:
        for i in range(len(o.data.materials)): o.data.materials[i]=m
        n+=1
print("FLAT_ASSIGN meshes", n)
bpy.ops.export_scene.gltf(filepath=path, export_format='GLB', use_selection=False)
print("FLAT_EXPORT_OK")
