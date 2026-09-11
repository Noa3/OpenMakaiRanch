using System;
using Godot;

namespace OpenMakaiRanch.Visuals;

public enum AnimeSurfaceKind { Skin, Hair, Eye, Cloth }

/// <summary>Presentation only. Textures and authored resources are never modified by the factory.</summary>
[GlobalClass]
public partial class AnimeSurfaceProfile : Resource
{
    [Export] public AnimeSurfaceKind Kind { get; set; } = AnimeSurfaceKind.Skin;
    [Export] public Color BaseColor { get; set; } = new(0.82f, 0.69f, 0.60f);
    [Export(PropertyHint.Range, "0.18,1,0.01")] public float Roughness { get; set; } = 0.46f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float Specular { get; set; } = 0.38f;
    [Export(PropertyHint.Range, "0,0.3,0.01")] public float Rim { get; set; } = 0.035f;
    [Export(PropertyHint.Range, "0,0.3,0.01")] public float Scattering { get; set; } = 0.10f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float Anisotropy { get; set; } = 0.65f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float NormalStrength { get; set; } = 0.5f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float OcclusionStrength { get; set; } = 0.5f;
    [Export] public Texture2D? AlbedoTexture { get; set; }
    /// <summary>Linear R: 0 preserves painted color, 1 applies BaseColor. Missing mask tints everything.</summary>
    [Export] public Texture2D? TintMaskTexture { get; set; }
    /// <summary>Shared UV1 transform for all maps. Zero/non-finite scale components fall back to 1.</summary>
    [Export] public Vector2 UvScale { get; set; } = Vector2.One;
    [Export] public Vector2 UvOffset { get; set; } = Vector2.Zero;
    [Export] public Texture2D? NormalTexture { get; set; }
    /// <summary>Linear RGBA: AO, roughness multiplier, specular multiplier, SSS multiplier. NOT glTF ORM.</summary>
    [Export] public Texture2D? SurfaceMask { get; set; }
    /// <summary>Linear RG encodes a tangent-space direction from [0,1] to [-1,1].</summary>
    [Export] public Texture2D? HairFlowTexture { get; set; }
    /// <summary>Only for original spherical test eyes. Leave disabled on imported textured eyes.</summary>
    [Export] public bool ProceduralIris { get; set; }
    [Export] public Color IrisColor { get; set; } = new(0.3f, 0.55f, 0.25f);
    [Export(PropertyHint.Range, "0.15,0.8,0.01")] public float IrisRadius { get; set; } = 0.52f;
    [Export(PropertyHint.Range, "0.1,0.7,0.01")] public float PupilRadius { get; set; } = 0.33f;

    public static AnimeSurfaceProfile Create(AnimeSurfaceKind kind, Color color)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        return new AnimeSurfaceProfile
        {
            Kind = kind, BaseColor = color,
            Roughness = kind switch { AnimeSurfaceKind.Hair => 0.38f, AnimeSurfaceKind.Eye => 0.20f, AnimeSurfaceKind.Cloth => 0.80f, _ => 0.46f },
            Specular = kind == AnimeSurfaceKind.Cloth ? 0.22f : 0.38f,
            Rim = kind == AnimeSurfaceKind.Hair ? 0.07f : 0.025f
        };
    }

    internal static float Bounded(float value, float fallback, float min, float max)
        => float.IsFinite(value) ? Math.Clamp(value, min, max) : fallback;

    internal static Vector2 SafeUvScale(Vector2 value)
        => new(Scale(value.X), Scale(value.Y));

    private static float Scale(float value)
        => !float.IsFinite(value) || Math.Abs(value) < 0.001f ? 1f : Math.Clamp(value, -64f, 64f);

    internal static Vector2 SafeUvOffset(Vector2 value)
        => new(Bounded(value.X, 0f, -64f, 64f), Bounded(value.Y, 0f, -64f, 64f));

    internal static Color SafeColor(Color color)
        => new(Bounded(color.R, 0.5f, 0f, 1f), Bounded(color.G, 0.5f, 0f, 1f),
            Bounded(color.B, 0.5f, 0f, 1f), 1f);
}
