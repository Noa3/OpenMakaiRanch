using System;

namespace OpenMakaiRanch.World;

/// <summary>
/// The world-facing contract for anything the player can approach and use from the 3D ranch.
///
/// Per the 3D_REMAKE_PLAN: a stable target ID, a label, an availability/reason, and an action
/// dispatched through the command boundary. Interaction range is spatial presentation only —
/// this type must never compute a second reward, economy or job outcome. All game effects flow
/// through <see cref="OpenMakaiRanch.App.GameRoot"/> commands (TryAssignJob / TryConductMentorship /
/// TryCompleteBondEvent), guarded by StateGeneration.
///
/// Kept as a pure interface so availability logic and double-activation protection can be verified
/// headlessly.
/// </summary>
public interface IWorldInteractable
{
    /// <summary>Stable identity; survives reloads and scene swaps. Never a runtime index.</summary>
    string TargetId { get; }

    /// <summary>Human-readable label shown on the prompt; not an ID.</summary>
    string Label { get; }

    /// <summary>
    /// True when the player can use this target right now. When false, <see cref="UnavailableReason"/>
    /// must explain why.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>Why the target is currently unavailable, or null when available.</summary>
    string? UnavailableReason { get; }

    /// <summary>
    /// Dispatch the interaction through the command boundary. Returns the command result (true = success).
    /// This method must NOT be a reward calculator — it routes to GameRoot.
    /// </summary>
    bool Activate(WorldInteractionContext context);
}

/// <summary>
/// The data a world interaction needs to dispatch its command, without owning simulation state.
/// </summary>
public readonly struct WorldInteractionContext
{
    /// <summary>The roster character id the command acts on (mentorship target, job assignee, etc.).</summary>
    public readonly string CharacterId;
    public readonly ulong ExpectedGeneration;

    public WorldInteractionContext(string characterId, ulong expectedGeneration)
    {
        CharacterId = characterId;
        ExpectedGeneration = expectedGeneration;
    }
}
