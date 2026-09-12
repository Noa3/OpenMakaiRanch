using System.Collections.Generic;
using Godot;

namespace OpenMakaiRanch.World;

/// <summary>Authored civic BASE adapter: geometry and services never imply an unlock or a public office.</summary>
public partial class OkachiCivicHouse : WalkInBuilding
{
    private readonly List<MeshInstance3D> _roofMeshes = new();
    public override Vector3 WorkLocal => new(-3.25f, 0.7f, 3);
    public override void Build() { }
    public override void SetFacilityLevel(int level) { }
    public override void SetBuiltColor(bool built) { }

    public override void _Ready()
    {
        BuildingId = "town_hall";
        Footprint = new(12, 10);
        Player = GetParent().GetNodeOrNull<Node3D>("Player");
        Collect(this);
    }

    private void Collect(Node node)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is MeshInstance3D mesh && mesh.Name.ToString().StartsWith("Roof_")) _roofMeshes.Add(mesh);
            if (child is StaticBody3D) CollisionBodyCount++;
            Collect(child);
        }
    }

    public override bool ContainsWorldPoint(Vector3 worldPoint)
    {
        if (!IsInsideTree()) return false;
        var p = ToLocal(worldPoint);
        return p.Y > -0.3f && p.Y < 3.15f && Mathf.Abs(p.X) < 5.9f && Mathf.Abs(p.Z) < 4.9f;
    }

    // Explicit same-room bounds prevent selecting these desks through the facade or spine wall.
    // This is an interaction eligibility guard, not a substitute for actor traversal tests.
    public bool CanApproachService(string serviceId, Vector3 worldPoint)
    {
        if (!ContainsWorldPoint(worldPoint)) return false;
        var p = ToLocal(worldPoint);
        return serviceId switch
        {
            "town_hall" => p.X > -5.54f && p.X < -1.22f && p.Z > -0.14f && p.Z < 4.59f,
            "planning_board" => p.X > 1.22f && p.X < 5.54f && p.Z > -4.78f && p.Z < -0.36f,
            _ => false
        };
    }

    public override void _Process(double delta)
    {
        if (!IsVisibleInTree() || !GodotObject.IsInstanceValid(Player) || !Player!.IsInsideTree()) return;
        IsCutaway = ContainsWorldPoint(Player.GlobalPosition);
        // Only roof visuals change; imported wall/cap/furniture collision and shelter remain intact.
        foreach (var mesh in _roofMeshes) mesh.Visible = !IsCutaway;
    }
}
