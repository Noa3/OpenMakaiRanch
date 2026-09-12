using System.Collections.Generic;
using Godot;

namespace OpenMakaiRanch.World;

/// <summary>Compatibility adapter for the authored home; never generates geometry or grants facilities.</summary>
public partial class RanchHome : WalkInBuilding
{
    private readonly List<MeshInstance3D> _roofMeshes = new();
    public override Vector3 WorkLocal => new(-7.15f, 0.5f, -0.4f);
    public override void Build() { } // All geometry, collision and lights belong to RanchHome.tscn.
    public override void SetFacilityLevel(int level) { } // Room existence is not an equipment unlock.
    public override void SetBuiltColor(bool built) { } // Preserve authored materials.
    public override void _Ready()
    {
        BuildingId = "ranch_house";
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
        var p = ToLocal(worldPoint) - new Vector3(0, 0, 6);
        return p.Y > -0.3f && p.Y < 3.1f &&
            ((Mathf.Abs(p.X) < 9.9f && p.Z > -8 && p.Z < 8.18f)
            || (Mathf.Abs(p.X) < 5.9f && p.Z >= -20 && p.Z <= -8));
    }
    public override void _Process(double delta)
    {
        if (!IsVisibleInTree() || !GodotObject.IsInstanceValid(Player) || !Player!.IsInsideTree()) return;
        IsCutaway = ContainsWorldPoint(Player.GlobalPosition);
        foreach (var mesh in _roofMeshes) mesh.Visible = !IsCutaway;
    }
}
