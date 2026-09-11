shader_type spatial;
render_mode cull_back, diffuse_burley, specular_schlick_ggx;
#include "anime_common.gdshaderinc"
uniform float clearcoat_strength : hint_range(0.0, 1.0) = 0.35;
uniform bool procedural_iris = false;
uniform vec4 iris_color : source_color = vec4(0.3, 0.55, 0.25, 1.0);
uniform float iris_radius : hint_range(0.15, 0.8) = 0.52;
uniform float pupil_radius : hint_range(0.1, 0.7) = 0.33;
varying vec3 local_direction;

void vertex() {
    local_direction = VERTEX;
}

// Opaque eye surface: stable depth/SSAO/SSR behavior; no screen-space refraction or fake emission.
// Procedural iris is a test-geometry convenience, NOT a replacement for an authored eye asset.
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
    if (procedural_iris) {
        vec3 direction = normalize(local_direction + vec3(0.0, 0.0, 0.00001));
        float radius = length(direction.xy) / max(iris_radius, 0.15);
        float edge = max(fwidth(radius), 0.01);
        float front = smoothstep(0.1, 0.3, direction.z);
        float iris = (1.0 - smoothstep(1.0 - edge, 1.0 + edge, radius)) * front;
        float pupil = 1.0 - smoothstep(pupil_radius - edge, pupil_radius + edge, radius);
        float angle = atan(direction.y, direction.x + 0.00001);
        float fiber = 0.06 * (1.0 - smoothstep(0.15, 0.65, fwidth(angle) * 48.0));
        vec3 pigment = iris_color.rgb * (0.94 + fiber * sin(angle * 48.0 + radius * 22.0));
        pigment *= 1.0 - 0.65 * smoothstep(0.8, 1.0, radius);
        pigment = mix(pigment, vec3(0.008), pupil);
        ALBEDO = mix(ALBEDO, pigment, iris);
    }
    CLEARCOAT = clearcoat_strength;
    CLEARCOAT_ROUGHNESS = 0.18;
}
