using System;
using System.Collections.Generic;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Lightweight stylized presentation pass for the ranch while final authored/CC0 assets are gated.
///
/// Everything created here is decorative and collision-free. Existing world stations, facility IDs,
/// collision, schedule and settlement remain authoritative. This layer can therefore be replaced
/// piece-by-piece with real assets without changing gameplay.
/// </summary>
public partial class RanchPresentationBuilder : Node3D
{
    [Export] public bool BuildPlaceholderLandmarks { get; set; } = true;

    private readonly Dictionary<string, MeshInstance3D> _facilityLandmarks = new();
    private Node3D? _generated;
    private Label3D? _entrySign;

    private const string VendorRoot = "res://assets/vendor/kaykit_medieval_hexagon/Assets/gltf";

    private static readonly Color GroundColor = new("7ea66a");
    private static readonly Color PathColor = new("cbb98d");
    private static readonly Color WoodColor = new("795b43");
    private static readonly Color RoofColor = new("a65353");
    private static readonly Color LeafColor = new("5d8d57");
    private static readonly Color AccentColor = new("ddb76d");
    private static readonly Color BuiltColor = new("7aa8c9");
    private static readonly Color LockedColor = new("777983");

    public int GeneratedNodeCount => _generated?.GetChildCount() ?? 0;

    public override void _Ready()
    {
        ApplyBaseMaterials();

        if (BuildPlaceholderLandmarks)
        {
            BuildOnce();
        }

        Refresh(GameRoot.Instance);
    }

    /// <summary>Refresh facility landmark colors from the shared RanchService only.</summary>
    public void Refresh(GameRoot? game)
    {
        if (game is null || !GodotObject.IsInstanceValid(game))
        {
            return;
        }

        if (_entrySign is not null)
        {
            _entrySign.Text = string.IsNullOrWhiteSpace(game.State.Player.RanchName)
                ? "RANCH"
                : game.State.Player.RanchName.ToUpperInvariant();
        }

        var ranch = GetParent();
        if (ranch is null)
        {
            return;
        }

        foreach (var child in EnumerateStations(ranch))
        {
            var built = string.IsNullOrWhiteSpace(child.RequiredFacilityId)
                || (game.Ranch.Facilities.TryGetValue(child.RequiredFacilityId, out var level) && level > 0);

            if (child.GetNodeOrNull<MeshInstance3D>("Mesh") is { } marker)
            {
                marker.MaterialOverride = CreateMaterial(built ? AccentColor : LockedColor);
            }

            if (_facilityLandmarks.TryGetValue(child.TargetId, out var landmark)
                && GodotObject.IsInstanceValid(landmark))
            {
                landmark.MaterialOverride = CreateMaterial(built ? BuiltColor : LockedColor);
            }

            if (child.GetNodeOrNull<Label3D>("Label") is { } label)
            {
                label.Modulate = built ? Colors.White : new Color(0.72f, 0.72f, 0.76f);
                label.Text = built ? child.Label : $"{child.Label}  [Locked]";
            }
        }
    }

    private void ApplyBaseMaterials()
    {
        if (GetNodeOrNull<MeshInstance3D>("../Ground/Mesh") is { } ground)
        {
            ground.MaterialOverride = CreateMaterial(GroundColor);
        }

        foreach (var path in new[] { "../Wall1/Mesh", "../Wall2/Mesh", "../InteriorWall/Mesh" })
        {
            if (GetNodeOrNull<MeshInstance3D>(path) is { } wall)
            {
                wall.MaterialOverride = CreateMaterial(WoodColor.Darkened(0.15f));
            }
        }
    }

    private void BuildOnce()
    {
        if (_generated is not null && GodotObject.IsInstanceValid(_generated))
        {
            return;
        }

        _generated = new Node3D { Name = "GeneratedStylizedPlaceholders" };
        AddChild(_generated);

        // Main readable loop: entry -> hub, then hub branches to the authored work stations.
        AddPath("EntryPath", new Vector3(0f, 0.03f, 10f), new Vector3(0f, 0.03f, 1.5f), 2.4f);
        AddPath("PasturePath", new Vector3(0f, 0.03f, 1.5f), new Vector3(-12f, 0.03f, -8f), 1.8f);
        AddPath("KitchenPath", new Vector3(0f, 0.03f, 1.5f), new Vector3(-12f, 0.03f, 6f), 1.8f);
        AddPath("WorkshopPath", new Vector3(0f, 0.03f, 1.5f), new Vector3(12f, 0.03f, 6f), 1.8f);
        AddPath("PharmacyPath", new Vector3(0f, 0.03f, 1.5f), new Vector3(12f, 0.03f, -1f), 1.8f);
        AddPath("DairyPath", new Vector3(0f, 0.03f, 1.5f), new Vector3(10f, 0.03f, -8f), 1.8f);

        BuildEntryArch();
        BuildCentralLandmark();
        BuildBoundaryNature();
        BuildFacilityLandmarks();

        // Small admitted CC0 prop accents. They remain decorative; collision and gameplay IDs stay authored.
        TryAddExternalScene("RanchBarrelA", $"{VendorRoot}/decoration/props/barrel.gltf",
            new Vector3(-3.0f, 0.03f, 3.6f), Vector3.One * 1.1f, 0.2f);
        TryAddExternalScene("RanchBarrelB", $"{VendorRoot}/decoration/props/barrel.gltf",
            new Vector3(-3.55f, 0.03f, 3.9f), Vector3.One * 0.95f, -0.25f);
    }

    private void BuildEntryArch()
    {
        AddBox("EntryPostL", new Vector3(-1.65f, 1.25f, 12.2f), new Vector3(0.28f, 2.5f, 0.28f), WoodColor);
        AddBox("EntryPostR", new Vector3(1.65f, 1.25f, 12.2f), new Vector3(0.28f, 2.5f, 0.28f), WoodColor);
        AddBox("EntryBeam", new Vector3(0f, 2.35f, 12.2f), new Vector3(3.6f, 0.32f, 0.34f), AccentColor);

        _entrySign = new Label3D
        {
            Name = "RanchEntrySign",
            Text = "RANCH",
            Position = new Vector3(0f, 2.35f, 12.0f),
            FontSize = 34,
            OutlineSize = 6
        };
        _generated!.AddChild(_entrySign);
    }

    private void BuildCentralLandmark()
    {
        // Small open plaza / well proxy. It is deliberately low so camera/player sight lines remain clear.
        AddCylinder("WellBase", new Vector3(0f, 0.32f, 3.8f), 0.78f, 0.55f, WoodColor.Lightened(0.18f));
        AddCylinder("WellWater", new Vector3(0f, 0.61f, 3.8f), 0.60f, 0.05f, new Color("70a9c9"));

        AddBox("NoticeBoardPost", new Vector3(-2.25f, 0.85f, 2.9f), new Vector3(0.18f, 1.7f, 0.18f), WoodColor);
        AddBox("NoticeBoard", new Vector3(-2.25f, 1.45f, 2.9f), new Vector3(1.7f, 1.0f, 0.18f), AccentColor.Darkened(0.12f));
    }

    private void BuildBoundaryNature()
    {
        var positions = new[]
        {
            new Vector3(-17.2f, 0f, -11.8f), new Vector3(-14.6f, 0f, -12.4f),
            new Vector3(-10.5f, 0f, -12.6f), new Vector3(-6.2f, 0f, -12.4f),
            new Vector3(6.0f, 0f, -12.4f), new Vector3(14.5f, 0f, -12.2f),
            new Vector3(17.0f, 0f, -9.8f), new Vector3(17.1f, 0f, -5.0f),
            new Vector3(17.0f, 0f, 7.0f), new Vector3(15.8f, 0f, 11.8f),
            new Vector3(-16.0f, 0f, 11.8f), new Vector3(-17.0f, 0f, 7.0f),
            new Vector3(-17.0f, 0f, 2.0f), new Vector3(-17.0f, 0f, -5.0f)
        };

        for (var i = 0; i < positions.Length; i++)
        {
            AddTree($"Tree_{i:00}", positions[i], 0.85f + (i % 3) * 0.12f);
        }
    }

    private void BuildFacilityLandmarks()
    {
        var ranch = GetParent();
        if (ranch is null)
        {
            return;
        }

        foreach (var station in EnumerateStations(ranch))
        {
            var stationPos = station.Position;
            var outward = new Vector3(stationPos.X, 0f, stationPos.Z);
            if (outward.LengthSquared() < 0.01f)
            {
                outward = Vector3.Back;
            }
            outward = outward.Normalized();

            var buildingPos = stationPos + outward * 2.15f;
            buildingPos.Y = 1.05f;

            var shell = AddBox(
                $"Landmark_{station.TargetId}",
                buildingPos,
                new Vector3(3.4f, 2.1f, 2.8f),
                BuiltColor);
            _facilityLandmarks[station.TargetId] = shell;

            AddBox(
                $"Roof_{station.TargetId}",
                buildingPos + new Vector3(0f, 1.28f, 0f),
                new Vector3(3.8f, 0.48f, 3.2f),
                RoofColor);

            // A bright front marker helps identify the approach side even before final art exists.
            var towardHub = -outward;
            var front = buildingPos + towardHub * 1.45f + new Vector3(0f, -0.2f, 0f);
            AddBox(
                $"Door_{station.TargetId}",
                front,
                new Vector3(0.85f, 1.45f, 0.16f),
                WoodColor,
                Mathf.Atan2(towardHub.X, towardHub.Z));
        }
    }

    private void AddTree(string name, Vector3 basePosition, float scale)
    {
        if (TryAddExternalScene(name, $"{VendorRoot}/decoration/nature/tree_single_A.gltf",
                basePosition, Vector3.One * (1.35f * scale), 0f))
        {
            return;
        }

        AddCylinder($"{name}_Trunk", basePosition + new Vector3(0f, 0.75f * scale, 0f),
            0.18f * scale, 1.5f * scale, WoodColor);
        AddSphere($"{name}_Crown", basePosition + new Vector3(0f, 2.05f * scale, 0f),
            0.78f * scale, LeafColor);
        AddSphere($"{name}_Crown2", basePosition + new Vector3(0.42f * scale, 1.9f * scale, 0.12f),
            0.48f * scale, LeafColor.Lightened(0.08f));
    }

    private void AddPath(string name, Vector3 from, Vector3 to, float width)
    {
        var delta = to - from;
        var length = new Vector2(delta.X, delta.Z).Length();
        if (length <= 0.01f)
        {
            return;
        }

        var midpoint = (from + to) * 0.5f;
        var yaw = Mathf.Atan2(delta.X, delta.Z);
        AddBox(name, midpoint, new Vector3(width, 0.055f, length), PathColor, yaw);
    }

    private bool TryAddExternalScene(string name, string path, Vector3 position, Vector3 scale, float yaw)
    {
        if (_generated is null || string.IsNullOrWhiteSpace(path) || !ResourceLoader.Exists(path))
        {
            return false;
        }

        var packed = GD.Load<PackedScene>(path);
        if (packed is null)
        {
            return false;
        }

        var instance = packed.Instantiate<Node3D>();
        if (instance is null)
        {
            return false;
        }

        instance.Name = name;
        instance.Position = position;
        instance.Scale = scale;
        instance.Rotation = new Vector3(0f, yaw, 0f);
        _generated.AddChild(instance);
        return true;
    }

    private MeshInstance3D AddBox(string name, Vector3 position, Vector3 size, Color color, float yaw = 0f)
    {
        var node = new MeshInstance3D
        {
            Name = name,
            Position = position,
            Rotation = new Vector3(0f, yaw, 0f),
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = CreateMaterial(color)
        };
        _generated!.AddChild(node);
        return node;
    }

    private void AddCylinder(string name, Vector3 position, float radius, float height, Color color)
    {
        var node = new MeshInstance3D
        {
            Name = name,
            Position = position,
            Mesh = new CylinderMesh
            {
                TopRadius = radius,
                BottomRadius = radius,
                Height = height,
                RadialSegments = 12
            },
            MaterialOverride = CreateMaterial(color)
        };
        _generated!.AddChild(node);
    }

    private void AddSphere(string name, Vector3 position, float radius, Color color)
    {
        var node = new MeshInstance3D
        {
            Name = name,
            Position = position,
            Mesh = new SphereMesh
            {
                Radius = radius,
                Height = radius * 2f,
                RadialSegments = 12,
                Rings = 7
            },
            MaterialOverride = CreateMaterial(color)
        };
        _generated!.AddChild(node);
    }

    private static StandardMaterial3D CreateMaterial(Color color)
    {
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            Roughness = 0.88f,
            Metallic = 0f
        };
    }

    private static IEnumerable<WorldStation> EnumerateStations(Node node)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is WorldStation station)
            {
                yield return station;
            }

            foreach (var nested in EnumerateStations(child))
            {
                yield return nested;
            }
        }
    }
}
