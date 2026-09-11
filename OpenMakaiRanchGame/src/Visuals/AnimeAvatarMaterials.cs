using System;
using Godot;

namespace OpenMakaiRanch.Visuals;

/// <summary>
/// Explicit preview for the project's generated player stand-in only. No recursive import traversal,
/// name inference on third-party models, saved setting, or simulation mutation. Unknown parts stay intact.
/// </summary>
public static class AnimeAvatarMaterials
{
    public static bool PreviewRequested => Array.Exists(OS.GetCmdlineUserArgs(), value => value == "--anime-pbr-preview");

    public static int ApplyPlayer(Node3D generatedRoot, string? quality, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(generatedRoot);
        if (!enabled) return 0;
        var applied = 0;
        foreach (var child in generatedRoot.GetChildren())
        {
            if (child is not MeshInstance3D mesh || mesh.MaterialOverride is not StandardMaterial3D source
                || source.Transparency != BaseMaterial3D.TransparencyEnum.Disabled)
                continue;
            AnimeSurfaceKind? kind = mesh.Name.ToString() switch
            {
                "Body" => AnimeSurfaceKind.Cloth,
                "Head" => AnimeSurfaceKind.Skin,
                "Hair" or "HairBack" => AnimeSurfaceKind.Hair,
                "EyeLeft" or "EyeRight" => AnimeSurfaceKind.Eye,
                _ => null
            };
            if (kind is null) continue;
            var profile = AnimeSurfaceProfile.Create(kind.Value, source.AlbedoColor);
            profile.AlbedoTexture = source.AlbedoTexture;
            profile.UvScale = new Vector2(source.Uv1Scale.X, source.Uv1Scale.Y);
            profile.UvOffset = new Vector2(source.Uv1Offset.X, source.Uv1Offset.Y);
            if (source.NormalEnabled) { profile.NormalTexture = source.NormalTexture; profile.NormalStrength = source.NormalScale; }
            if (kind == AnimeSurfaceKind.Eye && source.AlbedoTexture is null)
            {
                // Only the existing untextured spherical stand-in gets a procedural iris.
                // Authored eye art must not be painted over or lose its selected tint/UVs.
                profile.ProceduralIris = true;
                profile.IrisColor = source.AlbedoColor;
                profile.BaseColor = new Color(0.9f, 0.92f, 0.88f);
            }
            mesh.MaterialOverride = AnimeMaterialFactory.Create(profile, quality);
            applied++;
        }
        return applied;
    }
}
