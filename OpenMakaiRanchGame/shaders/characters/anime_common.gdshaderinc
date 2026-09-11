// Original project material inputs. Color textures are sRGB; normals/masks/flow are linear data.
// Opaque surfaces only. Do not add ALPHA = 1.0: writing ALPHA selects the transparent pipeline.
uniform vec4 base_color : source_color = vec4(0.82, 0.69, 0.60, 1.0);
uniform float roughness : hint_range(0.18, 1.0) = 0.46;
uniform float specular_strength : hint_range(0.0, 1.0) = 0.38;
uniform float rim_strength : hint_range(0.0, 0.3) = 0.035;
uniform float normal_strength : hint_range(0.0, 1.0) = 0.5;
uniform float occlusion_strength : hint_range(0.0, 1.0) = 0.5;
uniform vec2 uv_scale = vec2(1.0);
uniform vec2 uv_offset = vec2(0.0);
uniform bool use_tint_mask = false;
uniform sampler2D tint_mask_texture : hint_default_white, filter_linear_mipmap_anisotropic;
uniform bool use_albedo = false;
uniform bool use_normal = false;
uniform bool use_mask = false;
uniform sampler2D albedo_texture : source_color, hint_default_white, filter_linear_mipmap_anisotropic;
uniform sampler2D normal_texture : hint_normal, filter_linear_mipmap_anisotropic;
uniform sampler2D surface_mask : hint_default_white, filter_linear_mipmap_anisotropic;

vec2 material_uv(vec2 uv) {
    return uv * uv_scale + uv_offset;
}
vec3 read_albedo(vec2 uv) {
    float tint = use_tint_mask ? texture(tint_mask_texture, uv).r : 1.0;
    return mix(vec3(1.0), base_color.rgb, clamp(tint, 0.0, 1.0))
        * (use_albedo ? texture(albedo_texture, uv).rgb : vec3(1.0));
}
vec4 read_mask(vec2 uv) {
    return use_mask ? texture(surface_mask, uv) : vec4(1.0);
}
