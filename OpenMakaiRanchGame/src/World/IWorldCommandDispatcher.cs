using System;

namespace OpenMakaiRanch.World;

/// <summary>
/// The single boundary through which a world station dispatches its effect.
/// Implementations MUST route to the <c>OpenMakaiRanch.App.GameRoot</c> commands
/// (TryAssignJob / TryConductMentorship / TryCompleteBondEvent), guarded by StateGeneration.
///
/// This is the "no second reward calculator" rule from the 3D_REMAKE_PLAN: the station
/// never computes income, exp or bond deltas itself. It names a command and a target; the
/// dispatcher routes it to the one simulation that already owns those numbers.
///
/// Kept as an interface so the station's availability + double-activation logic can be
/// verified headlessly with a stub dispatcher, and the production binding can be swapped
/// in without the station knowing about Godot nodes.
/// </summary>
public interface IWorldCommandDispatcher
{
    /// <summary>
    /// Dispatch a world command. Returns true on success. The dispatcher owns the
    /// StateGeneration guard and the actual simulation call.
    /// </summary>
    bool Dispatch(WorldCommand command, WorldInteractionContext context);
}

/// <summary>
/// A named world command. The kind + target fully describe the effect without carrying
/// any simulation state, so it is safe to hold and compare.
/// </summary>
public sealed record WorldCommand(WorldCommandKind Kind, string? TargetId);

public enum WorldCommandKind
{
    /// <summary>Assign <see cref="WorldCommand.TargetId"/> (a job id) to the player.</summary>
    AssignJob,
    /// <summary>Conduct a mentorship with <see cref="WorldCommand.TargetId"/> (a character id).</summary>
    Mentorship,
    /// <summary>Complete <see cref="WorldCommand.TargetId"/> (a bond event id).</summary>
    BondEvent
}
