using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// Lightweight weather shelter marker. Presentation systems use simple box containment instead of
/// physics queries so roof checks remain cheap even with many buildings.
/// </summary>
public partial class WorldShelterVolume : Node3D
{
    public const string ShelterGroup = "world_shelter";

    [Export] public Vector3 HalfExtents { get; set; } = new(2f, 2f, 2f);

    public override void _Ready()
    {
        AddToGroup(ShelterGroup);
    }

    public bool ContainsPoint(Vector3 globalPoint)
    {
        return ContainsOffset(ToLocal(globalPoint), HalfExtents);
    }

    public static bool ContainsOffset(Vector3 localOffset, Vector3 halfExtents)
    {
        return Mathf.Abs(localOffset.X) <= Mathf.Abs(halfExtents.X)
            && Mathf.Abs(localOffset.Y) <= Mathf.Abs(halfExtents.Y)
            && Mathf.Abs(localOffset.Z) <= Mathf.Abs(halfExtents.Z);
    }

    public static bool IsPointSheltered(SceneTree? tree, Vector3 globalPoint)
    {
        if (tree is null)
        {
            return false;
        }

        foreach (var node in tree.GetNodesInGroup(ShelterGroup))
        {
            if (node is WorldShelterVolume shelter && GodotObject.IsInstanceValid(shelter)
                && shelter.ContainsPoint(globalPoint))
            {
                return true;
            }
        }

        return false;
    }
}
