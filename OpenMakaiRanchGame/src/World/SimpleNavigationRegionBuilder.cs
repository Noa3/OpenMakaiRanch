using Godot;

namespace OpenMakaiRanch.World;

/// <summary>Bakes the bounded area's real static collision once, after procedural shells are ready.</summary>
public partial class SimpleNavigationRegionBuilder : NavigationRegion3D
{
    [Export] public Vector2 Size { get; set; } = new(36f, 26f);
    [Export] public float Y { get; set; } = 0.02f;
    public bool CollisionBakeComplete { get; private set; }
    public Aabb? RegionalBakeBounds { get; set; }

    public override void _Ready()
    {
        // Until the deferred bake, there is deliberately no pretend obstacle-free rectangle.
        CallDeferred(nameof(BakeCollision));
    }

    private void BakeCollision()
    {
        if (!IsInsideTree() || CollisionBakeComplete) return;
        var mesh = new NavigationMesh
        {
            GeometryParsedGeometryType = NavigationMesh.ParsedGeometryType.StaticColliders,
            GeometryCollisionMask = 1,
            AgentRadius = 0.4f, AgentHeight = 2.25f, AgentMaxClimb = 0.25f,
            CellSize = 0.25f, CellHeight = 0.25f, RegionMinSize = 0.5f,
            FilterWalkableLowHeightSpans = true, FilterLedgeSpans = true,
            FilterBakingAabb = RegionalBakeBounds ?? new Aabb(new Vector3(-Size.X / 2, -0.3f, -Size.Y / 2), new Vector3(Size.X, 3.2f, Size.Y))
        };
        using var geometry = new NavigationMeshSourceGeometryData3D();
        NavigationServer3D.ParseSourceGeometryData(mesh, geometry, GetParent());
        NavigationServer3D.BakeFromSourceGeometryData(mesh, geometry);
        NavigationMesh = mesh;
        CollisionBakeComplete = mesh.GetPolygonCount() > 0;
        if (!CollisionBakeComplete) GD.PushError($"No walkable collision was baked for {GetParent().Name}.");
    }
}
