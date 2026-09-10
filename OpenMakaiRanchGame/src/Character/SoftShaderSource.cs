using Godot;

namespace OpenMakaiRanch.Character;

/// <summary>
/// Godot 4 spatial soft-anime shader source. Per the user's explicit direction
/// this is a SOFT, NATURAL look — no hard corners, no cel banding, no hard
/// outline.
///
/// Godot 4 spatial-shader idiom (verified against docs.godotengine.org):
///   - ALBEDO: base color. Set from a <c>base_color</c> uniform (ShaderMaterial
///     carries no StandardMaterial AlbedoColor property). The engine then applies
///     soft diffuse + hemisphere ambient (via WorldEnvironment) automatically.
///   - SPECULAR: 0.0 for a soft matte finish (no hard specular highlight).
///   - RIM: engine computes a soft fresnel edge glow (subtle, not a hard
///     outline). Set via the rim_strength uniform.
///
/// Note: <c>LIGHT</c> is only valid inside a <c>light()</c> function, not
/// <c>fragment()</c> — hand-rolled dot(N, -L) would fail GPU compilation. The
/// engine's built-in lighting is the correct soft-diffuse path.
///
/// The C# <see cref="SoftShadingMath"/> remains the headless-verified reference
/// model for the intended composite (soft diffuse + hemisphere ambient + gentle
/// rim). The GPU path uses Godot's built-in lighting, which is the idiomatic,
/// correct approach.
/// </summary>
public static class SoftShaderSource
{
    /// <summary>The GLSL source for the soft-anime spatial shader.</summary>
    public const string Gdshader = @"
shader_type spatial;
render_mode cull_back;

uniform vec4 base_color : source_color = vec4(0.85, 0.78, 0.72, 1.0);
uniform float rim_strength : hint_range(0.0, 1.0) = 0.12;

void fragment() {
    // Base albedo: the engine applies soft diffuse + hemisphere ambient on top.
    ALBEDO = base_color.rgb;

    // Soft matte: no hard specular highlight.
    SPECULAR = 0.0;

    // Soft rim edge glow (subtle, not a hard outline). The engine computes
    // the fresnel factor; we only set the strength.
    RIM = rim_strength;
    RIM_TINT = 0.4;
}
";

    /// <summary>Build a Godot <see cref="Shader"/> from the source string.</summary>
    public static Shader BuildShader()
    {
        return new Shader { Code = Gdshader };
    }
}
