shader_type spatial;
render_mode cull_back, diffuse_burley, specular_schlick_ggx;
#include "anime_common.gdshaderinc"

// Deliberately no SSS or anisotropy outputs: lower tiers avoid those shader paths.
void fragment() {
    vec2 surface_uv = material_uv(UV);
    vec4 mask = read_mask(surface_uv);
    ALBEDO = read_albedo(surface_uv);
    METALLIC = 0.0;
    ROUGHNESS = clamp(roughness * mask.g, 0.18, 1.0);
    SPECULAR = specular_strength * mask.b;
    AO = mix(1.0, mask.r, occlusion_strength);
    AO_LIGHT_AFFECT = 0.0;
    RIM = rim_strength;
    RIM_TINT = 0.45;
    NORMAL_MAP = use_normal ? texture(normal_texture, surface_uv).rgb : vec3(0.5, 0.5, 1.0);
    NORMAL_MAP_DEPTH = normal_strength;
}
