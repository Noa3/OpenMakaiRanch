using Godot;
using OpenMakaiRanch.Character;

namespace OpenMakaiRanch.Character;

/// <summary>
/// Builds Godot <see cref="ShaderMaterial"/> instances that render with the
/// soft-anime shader (<see cref="SoftShaderSource"/>). The factory is the single
/// place where the C# <see cref="SoftShadingMath.SoftParameters"/> are mapped
/// onto shader uniforms, so the C# math and the GPU shader always agree.
///
/// Node-free where possible: the material is built without a scene tree, so
/// the parameter mapping is verifiable in headless smoke tests.
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

        material.SetShaderParameter("base_color", p.BaseColor);
        material.SetShaderParameter("light_color", p.LightColor);
        material.SetShaderParameter("sky_color", p.SkyColor);
        material.SetShaderParameter("ground_color", p.GroundColor);
        material.SetShaderParameter("ambient_strength", p.AmbientStrength);
        material.SetShaderParameter("diffuse_softness", p.DiffuseSoftness);
        material.SetShaderParameter("rim_color", p.RimColor);
        material.SetShaderParameter("rim_strength", p.RimStrength);
        material.SetShaderParameter("rim_power", p.RimPower);
        material.SetShaderParameter("light_energy", p.LightEnergy);
    }

    /// <summary>Read back a soft-shading parameter from a material as a <see cref="float"/>.</summary>
    public static float GetFloat(ShaderMaterial material, string name)
        => (float)material.GetShaderParameter(name);

    /// <summary>Read back a soft-shading color parameter from a material.</summary>
    public static Color GetColor(ShaderMaterial material, string name)
        => (Color)material.GetShaderParameter(name);
}
