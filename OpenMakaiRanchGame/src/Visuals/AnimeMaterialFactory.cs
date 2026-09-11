using System;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.Visuals;

/// <summary>
/// Main-thread resource creation. Godot caches shader resources; each call gets independent uniforms.
/// Quality derives from the existing GraphicsQualityProfile, not a second settings/save model.
/// </summary>
public static class AnimeMaterialFactory
{
    private const string Root = "res://shaders/characters/";

    public static string ResolveShader(AnimeSurfaceKind kind, string? quality, string renderer)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        var tier = GraphicsQualityProfile.Resolve(quality).Name;
        var forward = string.Equals(renderer, "forward_plus", StringComparison.Ordinal);
        if (kind == AnimeSurfaceKind.Eye) return Root + "anime_eye.gdshader";
        if (tier == "Low" || !forward) return Root + "anime_basic.gdshader";
        if (kind == AnimeSurfaceKind.Skin && tier is "High" or "Ultra") return Root + "anime_skin.gdshader";
        return Root + (kind == AnimeSurfaceKind.Hair ? "anime_hair.gdshader" : "anime_basic.gdshader");
    }

    public static ShaderMaterial Create(AnimeSurfaceProfile profile, string? quality = "Medium", string? renderer = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        renderer ??= RenderingServer.GetCurrentRenderingMethod().ToString();
        var path = ResolveShader(profile.Kind, quality, renderer);
        var shader = GD.Load<Shader>(path) ?? throw new InvalidOperationException($"Missing character shader: {path}");
        var material = new ShaderMaterial { Shader = shader, ResourceLocalToScene = true };
        material.SetShaderParameter("base_color", AnimeSurfaceProfile.SafeColor(profile.BaseColor));
        Set(material, "roughness", profile.Roughness, 0.46f, 0.18f, 1f);
        Set(material, "specular_strength", profile.Specular, 0.38f, 0f, 1f);
        Set(material, "rim_strength", profile.Rim, 0.035f, 0f, 0.3f);
        Set(material, "normal_strength", profile.NormalStrength, 0.5f, 0f, 1f);
        Set(material, "occlusion_strength", profile.OcclusionStrength, 0.5f, 0f, 1f);
        Texture(material, "albedo_texture", "use_albedo", profile.AlbedoTexture);
        Texture(material, "normal_texture", "use_normal", profile.NormalTexture);
        Texture(material, "surface_mask", "use_mask", profile.SurfaceMask);
        if (path.EndsWith("anime_skin.gdshader", StringComparison.Ordinal))
            Set(material, "scattering_strength", profile.Scattering, 0.1f, 0f, 0.3f);
        if (path.EndsWith("anime_hair.gdshader", StringComparison.Ordinal))
        {
            Set(material, "anisotropy_strength", profile.Anisotropy, 0.65f, 0f, 1f);
            Texture(material, "flow_texture", "use_flow", profile.HairFlowTexture);
        }
        if (profile.Kind == AnimeSurfaceKind.Eye)
        {
            material.SetShaderParameter("procedural_iris", profile.ProceduralIris);
            material.SetShaderParameter("iris_color", AnimeSurfaceProfile.SafeColor(profile.IrisColor));
            Set(material, "iris_radius", profile.IrisRadius, 0.52f, 0.15f, 0.8f);
            Set(material, "pupil_radius", profile.PupilRadius, 0.33f, 0.1f, 0.7f);
        }
        return material;
    }

    public static ShaderMaterial Create(AnimeSurfaceKind kind, Color color, string? quality = "Medium")
        => Create(AnimeSurfaceProfile.Create(kind, color), quality);

    private static void Set(ShaderMaterial material, string name, float value, float fallback, float min, float max)
        => material.SetShaderParameter(name, AnimeSurfaceProfile.Bounded(value, fallback, min, max));

    private static void Texture(ShaderMaterial material, string sampler, string enabled, Texture2D? texture)
    {
        material.SetShaderParameter(enabled, texture is not null);
        if (texture is not null) material.SetShaderParameter(sampler, texture);
    }
}
