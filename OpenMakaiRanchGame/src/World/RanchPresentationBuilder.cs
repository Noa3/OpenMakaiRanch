using System;
using System.Collections.Generic;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Lightweight stylized presentation pass for the ranch while final authored/CC0 assets are gated.
///
/// Dressing is decorative; WalkInBuilding supplies the physical building shells. Existing world stations, facility IDs,
/// schedule and settlement remain authoritative. This layer can therefore be replaced
/// piece-by-piece with real assets without changing gameplay.
/// </summary>
public partial class RanchPresentationBuilder : Node3D
{
    [Export] public bool BuildPlaceholderLandmarks { get; set; } = true;

    private readonly Dictionary<string, WalkInBuilding> _facilityLandmarks = new();
    public IReadOnlyDictionary<string, WalkInBuilding> Buildings => _facilityLandmarks;
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
                landmark.SetBuiltColor(built);
                var visualLevel = string.IsNullOrWhiteSpace(child.RequiredFacilityId) ? 1
                    : game.State.Ranch.Facilities.GetValueOrDefault(child.RequiredFacilityId);
                landmark.SetFacilityLevel(visualLevel);
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

        // Curved paths and landscape are authored offline from OrganicWorldLayout.SourcePath.
        // Do not overlay obsolete hub spokes or connect entrances to old greybox station positions.

        BuildEntryArch();
        BuildCentralLandmark();
        BuildFacilityLandmarks();

        // Small admitted CC0 prop accents. They remain decorative; collision and gameplay IDs stay authored.
        TryAddExternalScene("RanchBarrelA", $"{VendorRoot}/decoration/props/barrel.gltf",
            new Vector3(4.0f, 0.03f, 3.6f), Vector3.One * 1.1f, 0.2f);
        TryAddExternalScene("RanchBarrelB", $"{VendorRoot}/decoration/props/barrel.gltf",
            new Vector3(4.55f, 0.03f, 3.9f), Vector3.One * 0.95f, -0.25f);
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
        AddCylinder("WellBase", new Vector3(3f, 0.32f, 3.8f), 0.78f, 0.55f, WoodColor.Lightened(0.18f));
        AddCylinder("WellWater", new Vector3(3f, 0.61f, 3.8f), 0.60f, 0.05f, new Color("70a9c9"));

        AddBox("NoticeBoardPost", RanchLeisureController.BoardOrigin + Vector3.Up * 0.85f, new Vector3(0.18f, 1.7f, 0.18f), WoodColor);
        AddBox("NoticeBoard", RanchLeisureController.BoardOrigin + Vector3.Up * 1.45f, new Vector3(1.7f, 1.0f, 0.18f), AccentColor.Darkened(0.12f));
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
            AddTree($"Tree_{i:00}", new Vector3(positions[i].X * 1.34f, 0, positions[i].Z * 1.43f), 0.85f + (i % 3) * 0.12f);
        }
    }

    private void BuildFacilityLandmarks()
    {
        var ranch = GetParent();
        if (ranch is null) return;
        var plotErrors = RanchBuildingPlots.Validate(RanchBuildingPlots.All);
        if (plotErrors.Count > 0) throw new InvalidOperationException(string.Join("; ", plotErrors));
        // Created before the parent ranch collects physical stations. No additional reward authority.
        foreach (var (id, label, position) in new[]
        {

            ("pet_care", "Pet care", new Vector3(-6, 0.5f, -8))
        })
        {
            var station = new WorldStation { Name = "Station_" + id, TargetId = id, Label = label,
                CommandTargetId = "rest", Position = position };
            _generated!.AddChild(station);
            station.AddChild(new Label3D { Name = "Label", Text = label, Position = new Vector3(0, 1.5f, 0),
                FontSize = 28, OutlineSize = 5, Billboard = BaseMaterial3D.BillboardModeEnum.Enabled });
        }
        // Snapshot: adding a building must not expand the enumeration recursively.
        var stations = new List<WorldStation>(EnumerateStations(ranch));
        foreach (var station in stations)
        {
            if (!station.RequiresWorker) continue;
            if (station.TargetId is "ranch_house" or "kitchen" or "office")
            {
                if (ranch.GetNodeOrNull<RanchHome>("RanchHome") is { } home)
                {
                    _facilityLandmarks[station.TargetId] = home;
                    var anchor = station.TargetId == "kitchen" ? "KitchenWork" : station.TargetId == "office" ? "OfficeWork" : "HouseWork";
                    station.GlobalPosition = home.GetNode<Node3D>("Assets/" + anchor).GlobalPosition;
                    if (station.GetNodeOrNull<MeshInstance3D>("Mesh") is { } block) block.Visible = false;
                }
                continue;
            }
            var plot = RanchBuildingPlots.Find(station.TargetId);
            if (plot is null) continue; // New building types require an authored, validated plot.
            var building = new WalkInBuilding { Name = "Building_" + station.TargetId,
                BuildingId = station.TargetId, Footprint = plot.Footprint,
                Position = new Vector3(plot.Center.X, 0, plot.Center.Y),
                Rotation = new Vector3(0, plot.Yaw, 0),
                Player = ranch.GetNodeOrNull<ThirdPersonPlayerController>("Player"),
                WallColor = station.TargetId == "dairy_barn" ? new Color("abc0bb") : new Color("b9ac8a") };
            _generated!.AddChild(building);
            _facilityLandmarks[station.TargetId] = building;
            // Keep the stable station node and identifier, now just inside its actual doorway.
            station.Position = building.Position + new Basis(Vector3.Up, building.Rotation.Y) * building.WorkLocal;
            if (station.GetNodeOrNull<MeshInstance3D>("Mesh") is { } oldBlock) oldBlock.Visible = false;

        }
    }

    private void AddTree(string name, Vector3 basePosition, float scale)
    {
        if (TryAddExternalScene(name, $"{VendorRoot}/decoration/nature/tree_single_A.gltf",
                basePosition, Vector3.One * (1.35f * scale), 0f, requirePlotClearance: true))
        {
            return;
        }

        // The fallback crown must respect the same reserved plots as an imported mesh.
        var radius = scale + 0.12f;
        if (!RanchDressingClearance.Allows(new Rect2(new Vector2(basePosition.X, basePosition.Z)
            - Vector2.One * radius, Vector2.One * (radius * 2)))) return;
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

    private bool TryAddExternalScene(string name, string path, Vector3 position, Vector3 scale, float yaw, bool requirePlotClearance = false)
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
        if (requirePlotClearance && !RanchDressingClearance.AllowsMeshes(instance))
        {
            instance.Free();
            return false;
        }
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
