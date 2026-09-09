using System.Collections.Generic;
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
    private readonly Dictionary<string, MeshInstance3D> _serviceBuildings = new();
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
                building.MaterialOverride = Material(available ? WallA : new Color("777983"));
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

        AddBox("MainRoad", new Vector3(0, 0.02f, 1), new Vector3(4.2f, 0.05f, 27f), Road);
        AddBox("CrossRoad", new Vector3(0, 0.025f, -2), new Vector3(29f, 0.05f, 4.2f), Road);
        AddCylinder("CentralPlaza", new Vector3(0, 0.035f, -2), 4.0f, 0.06f, Plaza);
        AddCylinder("FountainBase", new Vector3(0, 0.35f, -2), 1.15f, 0.55f, WallB);
        AddCylinder("FountainWater", new Vector3(0, 0.66f, -2), 0.88f, 0.05f, new Color("65a8c5"));

        var services = new List<TownServicePoint>();
        CollectServices(GetParent(), services);
        var index = 0;
        foreach (var service in services)
        {
            BuildServiceBuilding(service, index++);
        }

        var treePositions = new[]
        {
            new Vector3(-15f,0,-11f), new Vector3(-10f,0,-11.5f), new Vector3(10f,0,-11.5f),
            new Vector3(15f,0,-11f), new Vector3(-15f,0,9f), new Vector3(15f,0,9f),
            new Vector3(-8f,0,10.5f), new Vector3(8f,0,10.5f)
        };
        for (var i=0; i<treePositions.Length; i++)
        {
            AddTree($"TownTree_{i}", treePositions[i], 0.9f + (i%2)*0.1f);
        }

        foreach (var pos in new[]
        {
            new Vector3(-4.5f,0,-6f), new Vector3(4.5f,0,-6f),
            new Vector3(-4.5f,0,2f), new Vector3(4.5f,0,2f)
        })
        {
            AddLamp(pos);
        }

        AddGate();
    }

    private void BuildServiceBuilding(TownServicePoint service, int index)
    {
        var position = service.Position;
        var radial = new Vector3(position.X, 0, position.Z + 2f);
        if (radial.LengthSquared() < 0.1f)
        {
            radial = Vector3.Right;
        }
        radial = radial.Normalized();

        var center = position + radial * 2.2f;
        center.Y = 1.25f;

        var wall = index % 2 == 0 ? WallA : WallB;
        var roof = index % 2 == 0 ? RoofA : RoofB;

        Node3D? externalModel = null;
        var externalLoaded = ServiceModels.TryGetValue(service.ServiceId, out var externalPath)
            && TryAddExternalScene(
                $"External_{service.ServiceId}",
                externalPath,
                new Vector3(center.X, 0.03f, center.Z),
                Vector3.One * 2.0f,
                0f,
                out externalModel);

        if (externalLoaded && externalModel is not null)
        {
            _externalServiceModels[service.ServiceId] = externalModel;
        }

        var building = AddBox($"Building_{service.ServiceId}", center, new Vector3(4.2f, 2.5f, 3.4f), wall);
        building.Visible = !externalLoaded;
        _serviceBuildings[service.ServiceId] = building;
        var roofProxy = AddBox($"Roof_{service.ServiceId}", center + new Vector3(0,1.55f,0), new Vector3(4.6f,0.55f,3.8f), roof);
        roofProxy.Visible = !externalLoaded;

        _generated!.AddChild(new WorldShelterVolume
        {
            Name = $"Shelter_{service.ServiceId}",
            Position = center + new Vector3(0f, 0.65f, 0f),
            HalfExtents = new Vector3(2.25f, 1.75f, 1.95f)
        });

        var towardPlaza = -radial;
        var door = center + towardPlaza * 1.78f + new Vector3(0,-0.35f,0);
        AddBox($"Door_{service.ServiceId}", door, new Vector3(1.0f,1.65f,0.18f), Wood, Mathf.Atan2(towardPlaza.X,towardPlaza.Z));

        var label = new Label3D
        {
            Name = $"Sign_{service.ServiceId}",
            Text = service.Label,
            Position = center + new Vector3(0, 1.95f, 0),
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
