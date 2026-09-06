using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;

namespace OpenMakaiRanch.World;

/// <summary>One logical place in the greybox a character stands for their assigned work.</summary>
public readonly record struct WorkAnchor(string AnchorId, string DisplayName, Godot.Vector3 Position, Godot.Vector3 Facing);

/// <summary>
/// A fully resolved standing order for one roster character this phase.
/// </summary>
public readonly record struct RosterPlacement(string CharacterId, WorkAnchor Anchor, Godot.Vector3 Position, int IndexInAnchor);

/// <summary>
/// Deterministic, Node-free placement of roster characters at logical greybox anchors, derived from
/// the <b>existing</b> per-character job assignments and the shared <see cref="DayPhase"/>.
///
/// This is the presentation half of AI-001 / WORLD-003 ("derive logical NPC locations from existing
/// assignments/phases"). It is a pure function of the shared simulation: it reads each character's
/// current job and the current phase, and returns where to put a stand-in avatar. It owns no
/// schedules, moves no characters, and computes no work — there is no second work economy. The
/// anchors live inside the existing greybox bounds (x ∈ [-15, 15], z ∈ [-10, 10]) so they compose
/// with the authored walls and the milk station.
/// </summary>
public static class RosterPlacementMath
{
    /// <summary>Logical anchors inside the existing greybox (x ∈ [-15,15], z ∈ [-10,10]).</summary>
    public static readonly WorkAnchor Pasture = new("PASTURE", "Pasture", new Godot.Vector3(8f, 0f, -4f), Godot.Vector3.Forward);
    public static readonly WorkAnchor Workshop = new("WORKSHOP", "Workshop", new Godot.Vector3(-10f, 0f, 4f), Godot.Vector3.Right);
    public static readonly WorkAnchor Kitchen = new("KITCHEN", "Kitchen", new Godot.Vector3(-6f, 0f, 6f), Godot.Vector3.Right);
    public static readonly WorkAnchor Office = new("OFFICE", "Office", new Godot.Vector3(2f, 0f, 7f), Godot.Vector3.Back);
    public static readonly WorkAnchor Pharmacy = new("PHARMACY", "Pharmacy", new Godot.Vector3(10f, 0f, 6f), Godot.Vector3.Right);
    public static readonly WorkAnchor Dairy = new("DAIRY", "Dairy (milk station)", new Godot.Vector3(12f, 0f, 0f), Godot.Vector3.Right);
    public static readonly WorkAnchor MentorCircle = new("MENTOR_CIRCLE", "Mentor circle", new Godot.Vector3(-2f, 0f, 0f), Godot.Vector3.Back);
    public static readonly WorkAnchor RestArea = new("REST_AREA", "Rest area", new Godot.Vector3(-12f, 0f, -6f), Godot.Vector3.Back);
    public static readonly WorkAnchor PatrolPath = new("PATROL_PATH", "Patrol path", new Godot.Vector3(0f, 0f, -8f), Godot.Vector3.Back);
    public static readonly WorkAnchor Customer = new("CUSTOMER", "Customer desk", new Godot.Vector3(4f, 0f, 8f), Godot.Vector3.Back);

    /// <summary>
    /// The anchor a job category maps to. Unknown/unassignable categories fall back to the rest area,
    /// so a character always has a logical, in-bounds place.
    /// </summary>
    public static WorkAnchor AnchorForJob(JobCategory category)
    {
        return category switch
        {
            JobCategory.RanchWork => Pasture,
            JobCategory.Chore => Workshop,
            JobCategory.Mentorship => MentorCircle,
            JobCategory.Adventure => PatrolPath,
            JobCategory.Dairy => Dairy,
            JobCategory.Office => Office,
            JobCategory.Cleaning => Kitchen,
            JobCategory.Cooking => Kitchen,
            JobCategory.Pharmacy => Pharmacy,
            JobCategory.CustomerService => Customer,
            JobCategory.Rest => RestArea,
            _ => RestArea
        };
    }

    /// <summary>
    /// Deterministic per-character lateral offset so several characters assigned to the same job stand
    /// side by side instead of overlapping. Bounded and stable (pure function of index).
    /// </summary>
    public static Godot.Vector3 SpreadOffset(WorkAnchor anchor, int indexInAnchor)
    {
        if (indexInAnchor <= 0)
        {
            return Godot.Vector3.Zero;
        }

        // Alternate left/right of the anchor, stepping out. Keeps every offset small and in-bounds.
        var side = indexInAnchor % 2 == 0 ? 1f : -1f;
        var step = (indexInAnchor + 1) / 2;
        return new Godot.Vector3(anchor.Facing.Z * side * step, 0f, -anchor.Facing.X * side * step);
    }

    /// <summary>
    /// Resolve the standing order for one character. <paramref name="indexInAnchor"/> is that
    /// character's ordinal among its anchor-mates (callers compute it stably, e.g. roster order).
    /// </summary>
    public static RosterPlacement Place(string characterId, JobCategory category, int indexInAnchor)
    {
        var anchor = AnchorForJob(category);
        var position = anchor.Position + SpreadOffset(anchor, indexInAnchor);
        return new RosterPlacement(characterId, anchor, position, indexInAnchor);
    }
}
