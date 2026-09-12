using System.Collections.Generic;
using System.Linq;
using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// Collision-free stylized placeholder presentation for Okachi Town.
/// Buildings are generated around authored service points so final CC0/custom meshes can replace
/// them without changing service IDs or screen routing.
/// </summary>
public partial class TownPresentationBuilder : Node3D
{
    private Node3D? _generated;
    private readonly Dictionary<string, WalkInBuilding> _serviceBuildings = new();
    private readonly Dictionary<string, Label3D> _serviceSigns = new();
    private readonly Dictionary<string, Node3D> _externalServiceModels = new();

    private const string VendorRoot = "res://assets/vendor/kaykit_medieval_hexagon/Assets/gltf";
    private static readonly Dictionary<string, string> ServiceModels = new()
    {
        ["general_store"] = $"{VendorRoot}/buildings/blue/building_market_blue.gltf",
        ["tavern"] = $"{VendorRoot}/buildings/blue/building_tavern_blue.gltf",
        ["research_office"] = $"{VendorRoot}/buildings/blue/building_blacksmith_blue.gltf"
    };

    private static readonly Color Road = new("b9a27a");
    private static readonly Color Plaza = new("a6a6a0");
    private static readonly Color WallA = new("d7c3a6");
    private static readonly Color WallB = new("b8c8cf");
    private static readonly Color RoofA = new("9e5963");
    private static readonly Color RoofB = new("526f86");
    private static readonly Color Wood = new("765b45");
    private static readonly Color Leaf = new("5f8f61");
    private static readonly Color Lamp = new("e7c56e");

    public int GeneratedNodeCount => _generated?.GetChildCount() ?? 0;

    public override void _Ready()
    {
        BuildOnce();
        Refresh(OpenMakaiRanch.App.GameRoot.Instance);
    }

    public void Refresh(OpenMakaiRanch.App.GameRoot? game)
    {
        if (game is null || !GodotObject.IsInstanceValid(game))
        {
            return;
        }

        var services = new List<TownServicePoint>();
        CollectServices(GetParent(), services);
        foreach (var service in services)
        {
            var available = service.IsAvailable;
            if (_serviceBuildings.TryGetValue(service.ServiceId, out var building) && GodotObject.IsInstanceValid(building))
            {
                building.SetBuiltColor(available);
            }

            if (_serviceSigns.TryGetValue(service.ServiceId, out var sign) && GodotObject.IsInstanceValid(sign))
            {
                sign.Modulate = available ? Colors.White : new Color(0.70f, 0.70f, 0.74f);
                sign.Text = available ? service.Label : $"{service.Label} [Locked]";
            }
        }
    }

    private void BuildOnce()
    {
        if (_generated is not null)
        {
            return;
        }

        _generated = new Node3D { Name = "GeneratedTownPlaceholders" };
        AddChild(_generated);

        // Market lanes and their approaches come from the same JSON as these service anchors.
        // Offset the fountain from the main walking line rather than using a radial crossroad hub.
        AddCylinder("FountainBase", new Vector3(-1, 0.35f, -0.8f), 0.8f, 0.55f, WallB);
        AddCylinder("FountainWater", new Vector3(-1, 0.66f, -0.8f), 0.65f, 0.05f, new Color("65a8c5"));

        var services = new List<TownServicePoint>();
        CollectServices(GetParent(), services);
        var index = 0;
        foreach (var service in services)
        {
            BuildServiceBuilding(service, index++);
        }

        foreach (var pos in new[]
        {
            new Vector3(-3.3f,0,-1.6f), new Vector3(4.4f,0,-0.7f),
            new Vector3(-2.4f,0,8.7f), new Vector3(5.4f,0,5.9f)
        })
        {
            AddLamp(pos);
        }

        AddGate();
    }

    private void BuildServiceBuilding(TownServicePoint service, int index)
    {
        var plot = OrganicWorldLayout.TownPlots.Single(p => p.Id == service.ServiceId);
        var center = new Vector3(plot.Center.X, 0, plot.Center.Y);
        // Keep the authored service node/ScreenId. Proximity lives outside its actual doorway.
        service.Position = OrganicWorldLayout.Entrance(plot);
        if (service.ServiceId == "planning_board")
        {
            AddBox("PlanningPost", center + Vector3.Up * 0.8f, new Vector3(0.14f, 1.6f, 0.14f), Wood);
            AddBox("PlanningBoard", center + Vector3.Up * 1.5f, new Vector3(2, 1.1f, 0.15f), WallA, plot.Yaw);
        }
        else
        {
            var building = new WalkInBuilding
            {
                Name = "Building_" + service.ServiceId, BuildingId = "town_" + service.ServiceId,
                Position = center, Rotation = new Vector3(0, plot.Yaw, 0), Footprint = plot.Footprint,
                WallColor = index % 2 == 0 ? WallA : WallB,
                RoofColor = index % 3 == 0 ? new Color("a7624d") : new Color("566e76"),
                Player = GetParent().GetNodeOrNull<ThirdPersonPlayerController>("Player")
            };
            _generated!.AddChild(building);
            building.SetFacilityLevel(1);
            _serviceBuildings[service.ServiceId] = building;
        }

        var label = new Label3D
        {
            Name = $"Sign_{service.ServiceId}",
            Text = service.Label,
            Position = OrganicWorldLayout.Entrance(plot, 2.5f),
            FontSize = 30,
            OutlineSize = 6
        };
        _generated!.AddChild(label);
        _serviceSigns[service.ServiceId] = label;
    }

    private void AddGate()
    {
        AddBox("GatePostL", new Vector3(-1.8f,1.4f,13.5f), new Vector3(0.3f,2.8f,0.3f), Wood);
        AddBox("GatePostR", new Vector3(1.8f,1.4f,13.5f), new Vector3(0.3f,2.8f,0.3f), Wood);
        AddBox("GateBeam", new Vector3(0,2.6f,13.5f), new Vector3(4.0f,0.35f,0.35f), RoofB);
        var label = new Label3D
        {
            Name = "TownGateSign",
            Text = "OKACHI TOWN",
            Position = new Vector3(0,2.55f,13.25f),
            FontSize = 32,
            OutlineSize = 6
        };
        _generated!.AddChild(label);
    }

    private void AddTree(string name, Vector3 pos, float scale)
    {
        var externalPath = $"{VendorRoot}/decoration/nature/tree_single_A.gltf";
        if (TryAddExternalScene(name, externalPath, pos, Vector3.One * (1.35f * scale), 0f, out _))
        {
            return;
        }

        AddCylinder(name+"_Trunk", pos+new Vector3(0,0.75f*scale,0), 0.16f*scale,1.5f*scale,Wood);
        AddSphere(name+"_Crown", pos+new Vector3(0,2.0f*scale,0),0.72f*scale,Leaf);
    }

    private void AddLamp(Vector3 pos)
    {
        AddCylinder("LampPost_"+pos.X+"_"+pos.Z, pos+new Vector3(0,1.1f,0),0.08f,2.2f,Wood);
        AddSphere("Lamp_"+pos.X+"_"+pos.Z, pos+new Vector3(0,2.25f,0),0.18f,Lamp);
    }

    private bool TryAddExternalScene(string name, string path, Vector3 position, Vector3 scale, float yaw, out Node3D? instance)
    {
        instance = null;
        if (_generated is null || string.IsNullOrWhiteSpace(path) || !ResourceLoader.Exists(path))
        {
            return false;
        }

        var packed = GD.Load<PackedScene>(path);
        if (packed is null)
        {
            return false;
        }

        instance = packed.Instantiate<Node3D>();
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

    private MeshInstance3D AddBox(string name, Vector3 pos, Vector3 size, Color color, float yaw=0)
    {
        var node=new MeshInstance3D
        {
            Name=name, Position=pos, Rotation=new Vector3(0,yaw,0),
            Mesh=new BoxMesh{Size=size}, MaterialOverride=Material(color)
        };
        _generated!.AddChild(node);
        return node;
    }

    private void AddCylinder(string name, Vector3 pos, float radius, float height, Color color)
    {
        var node=new MeshInstance3D
        {
            Name=name, Position=pos,
            Mesh=new CylinderMesh{TopRadius=radius,BottomRadius=radius,Height=height,RadialSegments=12},
            MaterialOverride=Material(color)
        };
        _generated!.AddChild(node);
    }

    private void AddSphere(string name, Vector3 pos, float radius, Color color)
    {
        var node=new MeshInstance3D
        {
            Name=name, Position=pos,
            Mesh=new SphereMesh{Radius=radius,Height=radius*2,RadialSegments=12,Rings=7},
            MaterialOverride=Material(color)
        };
        _generated!.AddChild(node);
    }

    private static StandardMaterial3D Material(Color color) => new()
    {
        AlbedoColor=color, Roughness=0.88f, Metallic=0
    };

    private static void CollectServices(Node? node, List<TownServicePoint> result)
    {
        if (node is null) return;
        foreach (var child in node.GetChildren())
        {
            if (child is TownServicePoint service) result.Add(service);
            CollectServices(child,result);
        }
    }
}
