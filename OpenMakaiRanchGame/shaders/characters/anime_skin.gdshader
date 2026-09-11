shader_type spatial;
render_mode cull_back, diffuse_burley, specular_schlick_ggx, sss_mode_skin;
#include "anime_common.gdshaderinc"
uniform float scattering_strength : hint_range(0.0, 0.3) = 0.1;

// Forward+ High/Ultra only. No default clearcoat: ordinary skin must not look varnished.
void fragment() {
    vec4 mask = read_mask(UV);
    ALBEDO = read_albedo(UV);
    METALLIC = 0.0;
    ROUGHNESS = clamp(roughness * mask.g, 0.18, 1.0);
    SPECULAR = specular_strength * mask.b;
    AO = mix(1.0, mask.r, occlusion_strength);
    AO_LIGHT_AFFECT = 0.0;
    RIM = rim_strength;
    RIM_TINT = 0.45;
    NORMAL_MAP = use_normal ? texture(normal_texture, UV).rgb : vec3(0.5, 0.5, 1.0);
    NORMAL_MAP_DEPTH = normal_strength;
    SSS_STRENGTH = scattering_strength * mask.a;
}
