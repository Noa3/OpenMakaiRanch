using Godot;

namespace OpenMakaiRanch.World;

/// <summary>Checks decorative mesh bounds before they can obscure reserved buildings or doors.</summary>
public static class RanchDressingClearance
{
    public static bool Allows(Rect2 bounds)
    {
        if (!bounds.Position.IsFinite() || !bounds.Size.IsFinite()
            || bounds.Size.X <= 0 || bounds.Size.Y <= 0 || !RanchBuildingPlots.WorldBounds.Encloses(bounds)
            || bounds.Intersects(RanchBuildingPlots.GateApproach)) return false;
        foreach (var plot in RanchBuildingPlots.All)
            if (bounds.Intersects(plot.ReservedBounds) || bounds.Intersects(plot.EntranceBounds)) return false;
        return true;
    }

    // Compose local transforms, rather than reading GlobalTransform before a node enters the tree.
    public static bool AllowsMeshes(Node3D root)
    {
        Rect2? footprint = null;
        var valid = true;
        void Visit(Node node, Transform3D parent)
        {
            var transform = node is Node3D spatial ? parent * spatial.Transform : parent;
            if (node is MeshInstance3D { Mesh: not null } mesh)
            {
                var box = mesh.GetAabb();
                for (var corner = 0; corner < 8; corner++)
                {
                    var point = transform * box.GetEndpoint(corner);
                    if (!point.IsFinite()) { valid = false; continue; }
                    var ground = new Vector2(point.X, point.Z);
                    footprint = footprint is { } existing ? existing.Expand(ground) : new Rect2(ground, Vector2.Zero);
                }
            }
            foreach (var child in node.GetChildren()) Visit(child, transform);
        }
        Visit(root, Transform3D.Identity);
        return valid && footprint is { } bounds && Allows(bounds);
    }
}
