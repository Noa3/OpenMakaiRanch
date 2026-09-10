import bpy, os, sys

ASSET_DIR = r"E:\OpenMakaiRanch\OpenMakaiRanchGame\assets\3d"
MM = r"E:\OpenMakaiRanch\.artifacts\material_maker"

def make_mat(mat, albedo, normal, orm):
    m = bpy.data.materials.new("PBR_" + mat)
    m.use_nodes = True
    nt = m.node_tree
    nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial"); out.location=(400,0)
    bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled"); bsdf.location=(100,0)
    bsdf.inputs["Metallic"].default_value=0.0
    bsdf.inputs["Roughness"].default_value=1.0
    nt.links.new(bsdf.outputs[0], out.inputs["Surface"])
    def tex(path, loc, name):
        img = bpy.data.images.load(path)
        n = nt.nodes.new("ShaderNodeTexImage"); n.location=loc; n.image=img; n.name=name
        return n
    a = tex(os.path.join(MM, albedo), (-500,300), "alb")
    n = tex(os.path.join(MM, normal), (-500,0), "nrm")
    o = tex(os.path.join(MM, orm), (-500,-300), "orm")
    nt.links.new(a.outputs[0], bsdf.inputs["Base Color"])
    nt.links.new(n.outputs[0], bsdf.inputs["Normal"])
    nt.links.new(o.outputs[0], bsdf.inputs["Roughness"])
    nt.links.new(o.outputs[0], bsdf.inputs["Metallic"])
    return m

def reset():
    for o in list(bpy.data.objects): bpy.data.objects.remove(o, do_unlink=True)
    for b in list(bpy.data.meshes): bpy.data.meshes.remove(b)
    for m in list(bpy.data.materials): bpy.data.materials.remove(m)
    for i in list(bpy.data.images): bpy.data.images.remove(i)

def bake(glb, mat, albedo, normal, orm, targets):
    reset()
    bpy.ops.import_scene.gltf(filepath=os.path.join(ASSET_DIR, glb))
    m = make_mat(mat, albedo, normal, orm)
    applied=0
    for o in bpy.data.objects:
        if o.type=="MESH" and o.name in targets:
            o.data.materials.clear()
            o.data.materials.append(m); applied+=1
    if applied==0:
        print("SKIP no target matched", glb, targets); return
    out = os.path.join(ASSET_DIR, glb)
    sel=[o for o in bpy.data.objects if o.type=="MESH"]
    for o in sel: o.select_set(True)
    bpy.context.view_layer.objects.active = sel[0]
    bpy.ops.export_scene.gltf(filepath=out, export_format='GLB', use_selection=True)
    print(f"BAKE_OK {glb} mat={mat} applied={applied}/{len(targets)}")

if "--" not in sys.argv: sys.exit()
a = sys.argv[sys.argv.index("--")+1:]
glb, mat, albedo, normal, orm = a[0], a[1], a[2], a[3], a[4]
targets = set(a[5].split(","))
bake(glb, mat, albedo, normal, orm, targets)