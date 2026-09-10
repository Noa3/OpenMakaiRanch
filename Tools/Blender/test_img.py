import bpy, os
p = r"E:\OpenMakaiRanch\.artifacts\material_maker\w03_painted_wood_siding_albedo.png"
img = bpy.data.images.load(p)
print("IMG_LOADED", img.name, img.size[0], img.size[1])
img2 = bpy.data.images.load(r"E:\OpenMakaiRanch\.artifacts\material_maker\w03_painted_wood_siding_orm.png")
img2.colorspace_settings.name = "Non-Color"
print("IMG2_LOADED", img2.name)
img3 = bpy.data.images.load(r"E:\OpenMakaiRanch\.artifacts\material_maker\w03_painted_wood_siding_normal.png")
img3.colorspace_settings.name = "Non-Color"
print("ALL_IMAGES_OK")
