using Godot;
using OpenMakaiRanch.Character;

namespace OpenMakaiRanch.Character;

/// <summary>
/// Builds Godot <see cref="ShaderMaterial"/> instances that render with the
/// soft-anime shader (<see cref="SoftShaderSource"/>). The factory is the single
/// place where the C# <see cref="SoftShadingMath.SoftParameters"/> are mapped
/// onto the material, so the C# math and the GPU material always agree.
///
/// Node-free where possible: the material is built without a scene tree, so
/// the parameter mapping is verifiable in headless smoke tests.
///
/// Godot 4 spatial-shader idiom:
///   - base_color: shader uniform driving ALBEDO (engine applies soft diffuse
///     + hemisphere ambient via WorldEnvironment).
///   - SPECULAR: 0.0 for a soft matte finish.
///   - rim_strength: shader uniform driving RIM (soft fresnel edge glow).
/// </summary>
public static class SoftMaterialFactory
{
    /// <summary>
    /// Build a soft-anime material for a surface albedo (e.g. skin or hair tint).
    /// The shading parameters default to <see cref="SoftShadingMath.SoftParameters.Default"/>.
    /// </summary>
    public static ShaderMaterial Create(Color baseColor, SoftShadingMath.SoftParameters? p = null)
    {
        p ??= SoftShadingMath.SoftParameters.Default;
        p.BaseColor = baseColor;
        var material = new ShaderMaterial { Shader = SoftShaderSource.BuildShader() };
        ApplyParameters(material, p);
        return material;
    }

    /// <summary>
    /// Apply soft-shading parameters + base color onto an existing material.
    /// Exposed for tests and for re-tinting a material without rebuilding.
    /// </summary>
    public static void ApplyParameters(ShaderMaterial material, SoftShadingMath.SoftParameters p)
    {
        if (material is null) throw new System.ArgumentNullException(nameof(material));
        if (material.Shader is null) material.Shader = SoftShaderSource.BuildShader();

        // base_color drives ALBEDO (engine does soft diffuse + hemisphere ambient).
        material.SetShaderParameter("base_color", p.BaseColor);

        // rim_strength drives RIM (soft fresnel edge glow).
        material.SetShaderParameter("rim_strength", p.RimStrength);
    }

    /// <summary>Read back the rim_strength uniform from a material as a <see cref="float"/>.</summary>
    public static float GetRimStrength(ShaderMaterial material)
        => (float)material.GetShaderParameter("rim_strength");

    /// <summary>Read back the base_color uniform (ALBEDO) from a material.</summary>
    public static Color GetAlbedo(ShaderMaterial material)
        => (Color)material.GetShaderParameter("base_color");
}
