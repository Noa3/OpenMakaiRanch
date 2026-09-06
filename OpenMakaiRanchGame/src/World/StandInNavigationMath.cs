namespace OpenMakaiRanch.World;

/// <summary>
/// Pure, Node-free steering for AI-001 stand-in navigation: move a point toward a target,
/// decide when it has arrived, detect a stuck state (no progress), and recover by snapping to
/// the target.
///
/// This is the "nearby navigation + bounded stuck recovery" half of AI-001. It is a pure
/// function of its inputs — deterministic, headless-testable, and it owns no simulation state,
/// no schedules, and no work economy. It only answers: "given where I am and where I should be,
/// where do I go next, and do I give up and snap?"
/// </summary>
public static class StandInNavigationMath
{
    /// <summary>Step a position toward <paramref name="target"/> by at most <paramref name="maxStep"/>.</summary>
    public static Godot.Vector3 StepToward(Godot.Vector3 position, Godot.Vector3 target, float maxStep)
    {
        var delta = target - position;
        var distance = delta.Length();
        if (distance <= 1e-4f || maxStep <= 0f)
        {
            return target; // already there (or no movement allowed) -> snap to target
        }

        var step = delta.Normalized() * System.Math.Min(maxStep, distance);
        return position + step;
    }

    /// <summary>True when <paramref name="position"/> is within <paramref name="tolerance"/> of the target.</summary>
    public static bool IsArrived(Godot.Vector3 position, Godot.Vector3 target, float tolerance)
    {
        return (position - target).Length() <= tolerance;
    }

    /// <summary>
    /// True when a step made no meaningful progress toward the target (blocked/stuck).
    /// <paramref name="previous"/> is the position before the step, <paramref name="after"/> after.
    /// Progress is the reduction in distance-to-target.
    /// </summary>
    public static bool IsStuck(Godot.Vector3 previous, Godot.Vector3 after, Godot.Vector3 target, float minProgress)
    {
        var before = (previous - target).Length();
        var afterDist = (after - target).Length();
        return (before - afterDist) < minProgress;
    }

    /// <summary>
    /// Bounded stuck recovery: give up steering and snap straight to the target. This keeps a
    /// stand-in from oscillating or wedging against an obstacle — it lands at the logical anchor
    /// (in-bounds) and moves on.
    /// </summary>
    public static Godot.Vector3 Recover(Godot.Vector3 _position, Godot.Vector3 target)
    {
        return target;
    }

    /// <summary>
    /// A full deterministic tick: if already within <paramref name="arrivalTolerance"/> snap to the
    /// target; otherwise step toward it by at most <paramref name="maxStep"/>; and if that step made
    /// less than <paramref name="minProgress"/> progress (blocked/stuck) recover by snapping to the
    /// target. Returns the resulting position.
    /// </summary>
    public static Godot.Vector3 Tick(
        Godot.Vector3 position,
        Godot.Vector3 target,
        float maxStep,
        float arrivalTolerance,
        float minProgress)
    {
        if (IsArrived(position, target, arrivalTolerance))
        {
            return target;
        }

        var next = StepToward(position, target, maxStep);
        return IsStuck(position, next, target, minProgress) ? Recover(position, target) : next;
    }
}
