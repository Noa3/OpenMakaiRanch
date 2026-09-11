shader_type spatial;
render_mode cull_back, diffuse_burley, specular_schlick_ggx;
#include "anime_common.gdshaderinc"
uniform float anisotropy_strength : hint_range(0.0, 1.0) = 0.65;
uniform bool use_flow = false;
uniform sampler2D flow_texture : hint_default_white, filter_linear_mipmap_anisotropic;

// Requires authored UVs/tangents. A neutral/degenerate flow map has a defined fallback.
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
    vec2 flow = use_flow ? texture(flow_texture, UV).rg * 2.0 - 1.0 : vec2(0.0, 1.0);
    float magnitude = dot(flow, flow);
    ANISOTROPY_FLOW = magnitude > 0.0001 ? flow * inversesqrt(max(magnitude, 0.0001)) : vec2(0.0, 1.0);
    ANISOTROPY = anisotropy_strength;
}
