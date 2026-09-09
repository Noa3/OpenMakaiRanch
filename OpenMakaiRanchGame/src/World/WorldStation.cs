using System;
using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// A world smart object: an approach point, a stable target ID, a label, and one reserved
/// interaction slot. Per the 3D_REMAKE_PLAN, a station is "a world prop offering an approach
/// point, facing/animation anchor and reservable interaction slot."
///
/// Interaction is dispatched through the <see cref="IWorldCommandDispatcher"/> (which routes to
/// GameRoot), so the station never computes a reward. Double activation and missing/despawned
/// targets are rejected by <see cref="WorldInteractionGuard"/>.
/// </summary>
public partial class WorldStation : Area3D, IWorldInteractable
{
    /// <summary>Stable identity (e.g. "STATION_MILK"); never a runtime index.</summary>
    [Export] public string TargetId { get; set; } = "STATION_UNKNOWN";

    /// <summary>Human-readable prompt label.</summary>
    [Export] public string Label { get; set; } = "Station";

    /// <summary>The command this station dispatches when activated.</summary>
    [Export] public WorldCommandKind CommandKind { get; set; } = WorldCommandKind.AssignJob;
    [Export] public string CommandTargetId { get; set; } = string.Empty;

    /// <summary>
    /// Optional facility gate. Empty means the station is part of the always-available ranch
    /// baseline. RanchGreyboxController resolves this against the shared RanchService.
    /// </summary>
    [Export] public string RequiredFacilityId { get; set; } = string.Empty;

    /// <summary>
    /// The dispatcher routing the command to GameRoot. The scene (RanchGreybox controller)
    /// injects the production binding; headless tests inject a stub.
    /// </summary>
    public IWorldCommandDispatcher? Dispatcher { get; set; }

    /// <summary>
    /// Optional external progression check injected by the world controller. Tests and isolated
    /// stations remain available when no resolver is supplied.
    /// </summary>
    public Func<(bool Available, string Reason)>? AvailabilityResolver { get; set; }

    private readonly WorldInteractionGuard _guard = new();

    public string? UnavailableReason
    {
        get
        {
            if (Dispatcher is null)
            {
                return "no command dispatcher bound";
            }
            if (!_guard.CanInteract)
            {
                return "station is busy or unavailable";
            }

            var availability = ResolveAvailability();
            return availability.Available ? string.Empty : availability.Reason;
        }
    }

    public bool IsAvailable
    {
        get
        {
            if (!_guard.CanInteract || Dispatcher is null)
            {
                return false;
            }

            return ResolveAvailability().Available;
        }
    }

    private (bool Available, string Reason) ResolveAvailability()
    {
        if (AvailabilityResolver is null)
        {
            return (true, string.Empty);
        }

        var resolved = AvailabilityResolver.Invoke();
        return (resolved.Available, resolved.Reason ?? string.Empty);
    }

    /// <summary>
    /// Activate through the guard + dispatcher. Returns the command result (true = success).
    /// Rejects double activation and a missing target.
    /// </summary>
    public bool Activate(WorldInteractionContext context)
    {
        if (!IsAvailable || Dispatcher is null)
        {
            return false;
        }

        if (!_guard.BeginCommand())
        {
            return false;
        }

        try
        {
            var command = new WorldCommand(CommandKind, CommandTargetId);
            return Dispatcher.Dispatch(command, context);
        }
        finally
        {
            _guard.EndCommand();
        }
    }

    /// <summary>Mark the station as present/absent (scene exit, load/new game, error).</summary>
    public void SetTargetPresent(bool present) => _guard.SetTargetPresent(present);

    public WorldInteractionGuard Guard => _guard;
}
