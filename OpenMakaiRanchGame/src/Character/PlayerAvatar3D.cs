using System;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Character;

/// <summary>
/// Safe, presentation-only 3D stand-in for the player. It is shared by character creation and
/// the playable ranch so both views reflect the same <see cref="PlayerState"/>.
///
/// This deliberately does not render adult-specific body parameters. The stand-in visualizes
/// ordinary identity/customization only: approximate height, skin/hair/eye colors, horns and
/// glasses. Real authored player models can replace this node later without changing save state.
/// </summary>
public partial class PlayerAvatar3D : Node3D
{
    [Export] public bool AutoBindToGameRoot { get; set; } = true;
    [Export] public bool AnimateMovement { get; set; } = true;
    [Export] public float VisualScale { get; set; } = 1.0f;

    public MeshInstance3D? Body { get; private set; }
    public MeshInstance3D? Head { get; private set; }
    public int RebuildCount { get; private set; }

    private Node3D? _generated;
    private double _walkClock;

    public override void _Ready()
    {
        EnsureGeneratedRoot();

        if (AutoBindToGameRoot && GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged += OnStateChanged;
            RefreshFrom(game.State.Player);
        }
    }

    public override void _ExitTree()
    {
        if (AutoBindToGameRoot && GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged -= OnStateChanged;
        }
    }

    public override void _Process(double delta)
    {
        if (!AnimateMovement || _generated is null)
        {
            return;
        }

        if (GetParent() is CharacterBody3D body)
        {
            var planar = new Vector2(body.Velocity.X, body.Velocity.Z).Length();
            if (planar > 0.15f)
            {
                _walkClock += delta * Math.Clamp(planar, 1.0f, 8.0f);
                _generated.Position = new Vector3(0f, Mathf.Sin((float)_walkClock * 7f) * 0.025f, 0f);
                return;
            }
        }

        _walkClock = 0.0;
        _generated.Position = Vector3.Zero;
    }

    public void RefreshFrom(PlayerState player)
    {
        if (player is null)
        {
            return;
        }

        var root = EnsureGeneratedRoot();
        foreach (var child in root.GetChildren())
        {
            root.RemoveChild(child);
            child.Free();
        }

        Body = null;
        Head = null;

        var heightMeters = Mathf.Clamp(player.Height / 1000f, 1.45f, 2.25f);
        var scale = (heightMeters / 1.80f) * Mathf.Max(0.25f, VisualScale);

        var clothing = new StandardMaterial3D
        {
            AlbedoColor = ClothingColorFor(player.Race),
            Roughness = 0.82f
        };
        var skin = new StandardMaterial3D
        {
            AlbedoColor = SkinColorFor(player.SkinColor),
            Roughness = 0.88f
        };
        var hair = new StandardMaterial3D
        {
            AlbedoColor = HairColorFor(player.HairColor),
            Roughness = 0.72f
        };
        var eye = new StandardMaterial3D
        {
            AlbedoColor = EyeColorFor(player.EyeColor),
            Roughness = 0.38f
        };
        var dark = new StandardMaterial3D
        {
            AlbedoColor = new Color(0.08f, 0.09f, 0.12f),
            Roughness = 0.55f
        };

        Body = AddMesh(root, "Body",
            new CapsuleMesh { Radius = 0.31f * scale, Height = 1.28f * scale },
            clothing,
            new Vector3(0f, 0.79f * scale, 0f));

        Head = AddMesh(root, "Head",
            new SphereMesh { Radius = 0.23f * scale, Height = 0.46f * scale },
            skin,
            new Vector3(0f, 1.60f * scale, 0f));

        // Hair cap + a restrained rear mass for long styles. This is a silhouette preview, not
        // final hairstyle modeling.
        AddMesh(root, "Hair",
            new SphereMesh { Radius = 0.238f * scale, Height = 0.40f * scale },
            hair,
            new Vector3(0f, 1.69f * scale, -0.015f * scale),
            new Vector3(1.04f, 0.78f, 1.04f));

        if (IsLongHair(player.HairStyle))
        {
            AddMesh(root, "HairBack",
                new CapsuleMesh { Radius = 0.16f * scale, Height = 0.72f * scale },
                hair,
                new Vector3(0f, 1.34f * scale, -0.13f * scale),
                new Vector3(1.0f, 1.0f, 0.72f));
        }

        AddMesh(root, "EyeLeft",
            new SphereMesh { Radius = 0.033f * scale, Height = 0.066f * scale },
            eye,
            new Vector3(-0.078f * scale, 1.62f * scale, 0.205f * scale),
            new Vector3(1f, 0.72f, 0.55f));
        AddMesh(root, "EyeRight",
            new SphereMesh { Radius = 0.033f * scale, Height = 0.066f * scale },
            eye,
            new Vector3(0.078f * scale, 1.62f * scale, 0.205f * scale),
            new Vector3(1f, 0.72f, 0.55f));

        if (player.HasHorns)
        {
            AddHorn(root, "HornLeft", -0.13f * scale, scale, dark);
            AddHorn(root, "HornRight", 0.13f * scale, scale, dark);
        }

        if (player.HasGlasses)
        {
            AddGlasses(root, scale, dark);
        }

        RebuildCount += 1;
    }

    private void OnStateChanged()
    {
        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            RefreshFrom(game.State.Player);
        }
    }

    private Node3D EnsureGeneratedRoot()
    {
        if (_generated is not null && GodotObject.IsInstanceValid(_generated))
        {
            return _generated;
        }

        _generated = new Node3D { Name = "GeneratedVisual" };
        AddChild(_generated);
        return _generated;
    }

    private static MeshInstance3D AddMesh(
        Node3D parent,
        string name,
        Mesh mesh,
        Material material,
        Vector3 position,
        Vector3? scale = null,
        Vector3? rotationDegrees = null)
    {
        var node = new MeshInstance3D
        {
            Name = name,
            Mesh = mesh,
            MaterialOverride = material,
            Position = position,
            Scale = scale ?? Vector3.One
        };
        if (rotationDegrees.HasValue)
        {
            node.RotationDegrees = rotationDegrees.Value;
        }

        parent.AddChild(node);
        return node;
    }

    private static void AddHorn(Node3D root, string name, float x, float scale, Material material)
    {
        AddMesh(root, name,
            new BoxMesh { Size = new Vector3(0.075f, 0.28f, 0.075f) * scale },
            material,
            new Vector3(x, 1.91f * scale, -0.015f * scale),
            rotationDegrees: new Vector3(0f, 0f, x < 0f ? 18f : -18f));
    }

    private static void AddGlasses(Node3D root, float scale, Material material)
    {
        AddMesh(root, "GlassesBridge",
            new BoxMesh { Size = new Vector3(0.09f, 0.018f, 0.018f) * scale },
            material,
            new Vector3(0f, 1.61f * scale, 0.227f * scale));

        AddMesh(root, "GlassesLeft",
            new BoxMesh { Size = new Vector3(0.105f, 0.075f, 0.012f) * scale },
            material,
            new Vector3(-0.09f * scale, 1.61f * scale, 0.226f * scale));
        AddMesh(root, "GlassesRight",
            new BoxMesh { Size = new Vector3(0.105f, 0.075f, 0.012f) * scale },
            material,
            new Vector3(0.09f * scale, 1.61f * scale, 0.226f * scale));
    }

    private static bool IsLongHair(string style)
    {
        var value = (style ?? string.Empty).ToLowerInvariant();
        return value.Contains("long")
            || value.Contains("tail")
            || value.Contains("braid")
            || value.Contains("bun")
            || value.Contains("updo")
            || value.Contains("dread");
    }

    private static Color SkinColorFor(string value)
    {
        return (value ?? string.Empty).ToLowerInvariant() switch
        {
            "pale" or "porcelain" or "white" => new Color(0.93f, 0.86f, 0.80f),
            "fair" or "light" => new Color(0.88f, 0.76f, 0.67f),
            "tan" or "wheat" or "golden" => new Color(0.73f, 0.53f, 0.37f),
            "olive" or "caramel" => new Color(0.58f, 0.40f, 0.29f),
            "brown" or "dark" or "chocolate" => new Color(0.38f, 0.25f, 0.19f),
            "ebony" => new Color(0.24f, 0.16f, 0.13f),
            _ => new Color(0.82f, 0.69f, 0.60f)
        };
    }

    private static Color HairColorFor(string value)
    {
        return (value ?? string.Empty).ToLowerInvariant() switch
        {
            "black" => new Color(0.07f, 0.07f, 0.09f),
            "blonde" or "gold" => new Color(0.86f, 0.70f, 0.30f),
            "red" or "crimson" or "auburn" => new Color(0.58f, 0.18f, 0.13f),
            "pink" or "rose" => new Color(0.86f, 0.38f, 0.60f),
            "blue" or "azure" => new Color(0.18f, 0.35f, 0.72f),
            "green" or "mint" => new Color(0.23f, 0.58f, 0.38f),
            "silver" or "platinum" or "white" or "gray" => new Color(0.78f, 0.80f, 0.86f),
            "purple" => new Color(0.45f, 0.25f, 0.62f),
            "orange" => new Color(0.82f, 0.38f, 0.12f),
            "cyan" or "teal" or "sky blue" => new Color(0.20f, 0.65f, 0.72f),
            "brown" or "chestnut" => new Color(0.32f, 0.18f, 0.11f),
            _ => new Color(0.32f, 0.22f, 0.18f)
        };
    }

    private static Color EyeColorFor(string value)
    {
        return (value ?? string.Empty).ToLowerInvariant() switch
        {
            "red" or "crimson" => new Color(0.80f, 0.16f, 0.15f),
            "blue" or "sky blue" => new Color(0.20f, 0.50f, 0.88f),
            "green" => new Color(0.20f, 0.70f, 0.38f),
            "purple" or "violet" => new Color(0.56f, 0.30f, 0.82f),
            "gold" or "amber" or "orange" => new Color(0.90f, 0.62f, 0.16f),
            "pink" => new Color(0.92f, 0.38f, 0.62f),
            "silver" or "white" or "gray" => new Color(0.72f, 0.78f, 0.86f),
            "brown" or "hazel" => new Color(0.40f, 0.25f, 0.13f),
            "teal" => new Color(0.12f, 0.68f, 0.67f),
            _ => new Color(0.10f, 0.10f, 0.12f)
        };
    }

    private static Color ClothingColorFor(string race)
    {
        var hash = (race ?? string.Empty).GetHashCode(StringComparison.Ordinal);
        var variants = new[]
        {
            new Color(0.18f, 0.29f, 0.42f),
            new Color(0.24f, 0.35f, 0.28f),
            new Color(0.38f, 0.24f, 0.32f),
            new Color(0.28f, 0.26f, 0.42f)
        };
        return variants[Math.Abs(hash % variants.Length)];
    }
}
