using System;
using System.Collections.Generic;
using Godot;

namespace OpenMakaiRanch.World;

/// <summary>Replaceable, metre-scale building shell. Cutaway changes visuals, never collision or saves.</summary>
public partial class WalkInBuilding : Node3D
{
    [Export] public Vector2 Footprint { get; set; } = new(6.2f, 5.6f);
    [Export] public float WallHeight { get; set; } = 3.2f;
    [Export] public float DoorWidth { get; set; } = 1.8f;
    [Export] public float DoorHeight { get; set; } = 2.55f;
    public string BuildingId { get; set; } = string.Empty;
    public Color WallColor { get; set; } = new("b7ad8c");
    public Color RoofColor { get; set; } = new("9c6753");
    public Node3D? Player { get; set; }
    public bool IsCutaway { get; private set; }
    public int CollisionBodyCount { get; private set; }
    public Vector3 EntryLocal => new(0, 0.8f, Footprint.Y / 2 + 1);
    public Vector3 WorkLocal => new(0, 0.5f, Footprint.Y / 2 - 1.15f);
    private Node3D _roof = null!;
    private readonly List<(Node3D Wall, Vector3 Normal)> _wallVisuals = new();
    private readonly List<MeshInstance3D> _plaster = new();
    private bool _built;
    public int VisualGrade { get; private set; } = -1;
    private Node3D? _upgrades;

    public void SetFacilityLevel(int level)
    {
        var grade = RanchBuildingPlots.VisualGrade(level);
        if (!_built || grade == VisualGrade) return;
        VisualGrade = grade;
        if (_upgrades is not null) { RemoveChild(_upgrades); _upgrades.QueueFree(); }
        _upgrades = new Node3D { Name = "UpgradeDetails" }; AddChild(_upgrades);
        // Wall-mounted details stay inside the original collision envelope and away from the aisle.
        if (grade >= 2)
            Mesh(_upgrades, "WallShelf", new(0, 1.9f, -Footprint.Y / 2 + 0.28f), new(1.8f, 0.14f, 0.4f), new Color("a68a59"));
        if (grade >= 3)
            Mesh(_upgrades, "CraftedWallPanel", new(0, 2.35f, -Footprint.Y / 2 + 0.15f), new(1.5f, 0.55f, 0.12f), new Color("c7ad74"));
    }

    public override void _Ready() => Build();

    public void Build()
    {
        if (_built) return;
        _built = true;
        Footprint = new Vector2(Mathf.Max(4.6f, Footprint.X), Mathf.Max(4.4f, Footprint.Y));
        WallHeight = Mathf.Max(3, WallHeight);
        DoorWidth = Mathf.Clamp(DoorWidth, 1.5f, Footprint.X - 2);
        DoorHeight = Mathf.Clamp(DoorHeight, 2.45f, WallHeight - 0.3f);
        const float thickness = 0.22f;
        var x = Footprint.X / 2; var z = Footprint.Y / 2;
        var wood = new Color("75553d");
        Solid("Floor", new(0, -0.06f, 0), new(Footprint.X, 0.12f, Footprint.Y), new Color("a28a65"));
        Wall("BackWall", new(0, WallHeight / 2, -z), new(Footprint.X, WallHeight, thickness), Vector3.Forward);
        Wall("LeftWall", new(-x, WallHeight / 2, 0), new(thickness, WallHeight, Footprint.Y), Vector3.Left);
        Wall("RightWall", new(x, WallHeight / 2, 0), new(thickness, WallHeight, Footprint.Y), Vector3.Right);
        var side = (Footprint.X - DoorWidth) / 2;
        Wall("EntryLeft", new(-(DoorWidth + side) / 2, WallHeight / 2, z), new(side, WallHeight, thickness), Vector3.Back);
        Wall("EntryRight", new((DoorWidth + side) / 2, WallHeight / 2, z), new(side, WallHeight, thickness), Vector3.Back);
        Wall("DoorLintel", new(0, (WallHeight + DoorHeight) / 2, z), new(DoorWidth, WallHeight - DoorHeight, thickness), Vector3.Back);
        // The opening is genuinely empty: no invisible door slab or teleport trigger.
        foreach (var dx in new[] { -DoorWidth / 2 - 0.06f, DoorWidth / 2 + 0.06f })
            Mesh(this, "DoorFrame", new(dx, DoorHeight / 2, z + 0.14f), new(0.12f, DoorHeight, 0.13f), wood);
        Mesh(this, "DoorFrameTop", new(0, DoorHeight + 0.04f, z + 0.14f), new(DoorWidth + 0.26f, 0.16f, 0.13f), wood);
        for (var i = 0; i <= 8; i++)
            Mesh(this, "FloorJoint", new(-x + Footprint.X * i / 8, 0.007f, 0), new(0.018f, 0.01f, Footprint.Y - 0.3f), wood.Darkened(0.1f));
        _roof = new Node3D { Name = "CutawayRoof" }; AddChild(_roof);
        var angle = Mathf.DegToRad(32);
        var rise = (x + 0.3f) * Mathf.Tan(angle);
        for (var sign = -1; sign <= 1; sign += 2)
        {
            var slope = Mesh(_roof, "RoofSlope", new(sign * (x + 0.3f) / 2, WallHeight + rise / 2, 0),
                new((x + 0.3f) / Mathf.Cos(angle), 0.14f, Footprint.Y + 0.7f), RoofColor);
            slope.Rotation = new Vector3(0, 0, -sign * angle);
            for (var row = 1; row < 6; row++)
                Mesh(slope, "TileCourse" + row, new((row / 6f - 0.5f) * (x + 0.3f) / Mathf.Cos(angle), 0.078f, 0),
                    new(0.035f, 0.022f, Footprint.Y + 0.68f), RoofColor.Darkened(0.10f));
        }
        Mesh(_roof, "Ridge", new(0, WallHeight + rise, 0), new(0.23f, 0.18f, Footprint.Y + 0.75f), RoofColor.Darkened(0.15f));
        foreach (var sign in new[] { -1, 1 })
        {
            var arrays = new Godot.Collections.Array(); arrays.Resize((int)Godot.Mesh.ArrayType.Max);
            arrays[(int)Godot.Mesh.ArrayType.Vertex] = new Vector3[]
            {
                new(-x, WallHeight, sign * z), new(x, WallHeight, sign * z),
                new(0, WallHeight + x * Mathf.Tan(angle), sign * z)
            };
            arrays[(int)Godot.Mesh.ArrayType.Normal] = new Vector3[] { Vector3.Back * sign, Vector3.Back * sign, Vector3.Back * sign };
            var mesh = new ArrayMesh(); mesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, arrays);
            var gable = new MeshInstance3D { Name = "Gable" + sign, Mesh = mesh,
                MaterialOverride = new StandardMaterial3D { AlbedoColor = WallColor, Roughness = 0.9f, CullMode = BaseMaterial3D.CullModeEnum.Disabled } };
            _roof.AddChild(gable);
        }
        AddChild(new WorldShelterVolume { Name = "InteriorShelter", Position = new(0, 1.6f, 0), HalfExtents = new(x - 0.12f, 1.65f, z - 0.12f) });
        var lamp = new OmniLight3D { Name = "InteriorLamp", Position = new(0, 2.6f, 0), LightColor = new Color("ffe5ba"),
            LightEnergy = 0.7f, OmniRange = Mathf.Max(Footprint.X, Footprint.Y), ShadowEnabled = false };
        AddChild(lamp);
        Mesh(this, "LampShade", new(0, 2.8f, 0), new(0.38f, 0.16f, 0.38f), new Color("e9bd74"));
        // Keep the doorway and central turning aisle clear. Different station roles get distinct props.
        Solid("WorkSurface", new(-x + 0.8f, 0.48f, -z + 0.8f), new(1.1f, 0.96f, 1.1f), wood);
        Solid("Storage", new(x - 0.65f, 0.7f, -z + 0.55f), new(0.85f, 1.4f, 0.65f), wood.Lightened(0.13f));
        if (BuildingId == "ranch_house")
        {
            Solid("Bed", new(-x + 1.1f, 0.3f, -0.3f), new(1.4f, 0.6f, 2.15f), new Color("667d91"));
            Mesh(this, "Pillow", new(-x + 1.1f, 0.65f, -1.05f), new(1.12f, 0.16f, 0.38f), new Color("e5dfcd"));
            Solid("BathStandIn", new(x - 0.95f, 0.3f, -0.15f), new(1.3f, 0.6f, 1.95f), new Color("c6ced1"));
        }
        else if (BuildingId == "dairy_barn")
        {
            Solid("CareBench", new(-x + 0.8f, 0.4f, 0), new(0.9f, 0.8f, 2), wood);
            Mesh(this, "MilkChurn", new(x - 0.7f, 0.65f, 0), new(0.55f, 1.3f, 0.55f), new Color("a7b8c1"));
        }
        else if (BuildingId == "kitchen")
        {
            Solid("KitchenCounter", new(-x + 0.7f, 0.48f, 0), new(0.9f, 0.96f, 1.9f), new Color("d5c49f"));
            Mesh(this, "Stove", new(-x + 0.7f, 1, 0), new(0.72f, 0.12f, 0.8f), new Color("454d51"));
        }
    }

    public void SetBuiltColor(bool built)
    {
        foreach (var mesh in _plaster)
            mesh.MaterialOverride = new StandardMaterial3D { AlbedoColor = built ? WallColor : new Color("86878a"), Roughness = 0.9f };
    }

    public bool ContainsWorldPoint(Vector3 worldPoint)
    {
        if (!IsInsideTree()) return false;
        var local = ToLocal(worldPoint);
        return Mathf.Abs(local.X) < Footprint.X / 2 - 0.1f && Mathf.Abs(local.Z) < Footprint.Y / 2 + 0.18f
            && local.Y > -0.3f && local.Y < WallHeight;
    }

    public override void _Process(double delta)
    {
        if (!IsVisibleInTree() || !GodotObject.IsInstanceValid(Player) || !Player!.IsInsideTree()) return;
        IsCutaway = ContainsWorldPoint(Player.GlobalPosition);
        _roof.Visible = !IsCutaway;
        var camera = GetViewport().GetCamera3D();
        var towardCamera = camera is null ? Vector3.Back : ToLocal(camera.GlobalPosition);
        foreach (var (wall, normal) in _wallVisuals)
            wall.Visible = !IsCutaway || normal.Dot(towardCamera) <= 0;
    }

    private void Wall(string name, Vector3 position, Vector3 size, Vector3 normal)
    {
        var body = Solid(name, position, size, WallColor);
        var visual = body.GetNode<MeshInstance3D>("Visual");
        _plaster.Add(visual); _wallVisuals.Add((visual, normal));
        // Details inherit the wall's cutaway visibility; they never become floating trim indoors.
        var sideWall = Mathf.Abs(normal.X) > 0.5f;
        var length = sideWall ? size.Z : size.X;
        var wood = new Color("705541");
        if (length > 1.4f && size.Y > 2.5f)
        {
            var offset = normal * 0.14f;
            var trimSize = sideWall ? new Vector3(0.13f, 0.16f, length) : new Vector3(length, 0.16f, 0.13f);
            Mesh(visual, "FoundationTrim", offset + Vector3.Down * (WallHeight / 2 - 0.2f), trimSize, wood);
            Mesh(visual, "EavesTrim", offset + Vector3.Up * (WallHeight / 2 - 0.12f), trimSize, wood);
            foreach (var sign in new[] { -1, 1 })
            {
                var along = (sideWall ? Vector3.Back : Vector3.Right) * (sign * (length / 2 - 0.09f));
                Mesh(visual, "TimberUpright", offset + along, new Vector3(0.15f, WallHeight, 0.15f), wood);
            }
            if (length > 3.5f)
            {
                var windowSize = sideWall ? new Vector3(0.08f, 1.05f, 1.0f) : new Vector3(1.0f, 1.05f, 0.08f);
                Mesh(visual, "WindowRecess", offset, windowSize + new Vector3(0.12f, 0.12f, 0.12f), wood);
                Mesh(visual, "WindowGlass", offset + normal * 0.085f, windowSize, new Color("789a9b"));
                Mesh(visual, "WindowMullion", offset + normal * 0.14f,
                    sideWall ? new Vector3(0.08f, 1.04f, 0.06f) : new Vector3(0.06f, 1.04f, 0.08f), wood);
            }
        }
    }

    private StaticBody3D Solid(string name, Vector3 position, Vector3 size, Color color)
    {
        var body = new StaticBody3D { Name = name, Position = position, CollisionLayer = 1, CollisionMask = 1 };
        body.AddChild(new CollisionShape3D { Name = "Collision", Shape = new BoxShape3D { Size = size } });
        Mesh(body, "Visual", Vector3.Zero, size, color);
        AddChild(body); CollisionBodyCount++; return body;
    }

    private static MeshInstance3D Mesh(Node parent, string name, Vector3 position, Vector3 size, Color color)
    {
        var mesh = new MeshInstance3D { Name = name, Position = position, Mesh = new BoxMesh { Size = size },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = color, Roughness = 0.88f } };
        parent.AddChild(mesh); return mesh;
    }
}
