using Godot;

namespace OpenMakaiRanch.Character;

/// <summary>
/// Godot 4 spatial soft-anime shader source. The GLSL mirrors
/// <see cref="SoftShadingMath"/> 1:1 so the C# test suite can verify the
/// shading math headlessly while the GPU uses the same formulas.
///
/// SOFT, NATURAL anime look — no hard corners, no cel banding, no hard
/// outline. A smooth lighting model:
///   albedo * ( soft_diffuse * light + hemi_ambient ) + soft_rim
/// </summary>
public static class SoftShaderSource
{
    /// <summary>The GLSL source for the soft-anime spatial shader.</summary>
    public const string Gdshader = @"
shader_type spatial;
render_mode cull_back;

uniform vec4 base_color : source_color = vec4(0.85, 0.78, 0.72, 1.0);
uniform vec4 light_color : source_color = vec4(1.0, 0.98, 0.94, 1.0);
uniform vec4 sky_color : source_color = vec4(0.55, 0.60, 0.72, 1.0);
uniform vec4 ground_color : source_color = vec4(0.30, 0.28, 0.26, 1.0);
uniform float ambient_strength : hint_range(0.0, 2.0) = 0.6;
uniform float diffuse_softness : hint_range(0.0, 1.0) = 0.9;
uniform vec4 rim_color : source_color = vec4(1.0, 0.96, 0.90, 1.0);
uniform float rim_strength : hint_range(0.0, 1.0) = 0.25;
uniform float rim_power : hint_range(0.5, 8.0) = 2.0;
uniform float light_energy : hint_range(0.0, 2.0) = 1.0;

void fragment() {
    vec3 n = normalize(NORMAL);
    vec3 l = normalize(-LIGHT);
    vec3 v = normalize(VIEW);

    // Soft diffuse: a smooth ramp over the lit half, no hard cel band.
    float ndotl = clamp(dot(n, l), 0.0, 1.0);
    float eased = ndotl * ndotl * (3.0 - 2.0 * ndotl); // smoothstep(0,1,ndotl)
    float diffuse = mix(ndotl, eased, clamp(diffuse_softness, 0.0, 1.0)) * light_energy;

    // Hemisphere ambient: sky on up-facing, ground on down-facing.
    float hemi = n.y * 0.5 + 0.5;
    vec3 hemi_color = mix(ground_color.rgb, sky_color.rgb, hemi);

    vec3 band = base_color.rgb * (light_color.rgb * diffuse + hemi_color * ambient_strength);

    // Soft rim edge glow (subtle, not a hard outline).
    float ndotv = clamp(dot(n, v), 0.0, 1.0);
    float rim = pow(1.0 - ndotv, max(rim_power, 0.01)) * rim_strength;
    band += rim_color.rgb * base_color.rgb * rim;

    ALBEDO = clamp(band, 0.0, 1.0);
    ALPHA = 1.0;
}
";

    /// <summary>Build a Godot <see cref="Shader"/> from the source string.</summary>
    public static Shader BuildShader()
    {
        return new Shader { Code = Gdshader };
    }
}
