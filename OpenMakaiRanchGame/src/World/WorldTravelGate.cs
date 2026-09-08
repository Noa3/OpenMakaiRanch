using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// The presentation boundary a travel gate routes to. Travel is a *scene/presentation* concern —
/// it moves the player between authored areas and never touches gold, the clock, jobs, bonds or
/// any simulation number. The composition (<c>RanchWorldController</c>) owns this; the gate only
/// names a destination and hands it off. This keeps "no second simulation" intact: a gate is a
/// door, not a reward calculator.
/// </summary>
public interface ITravelHandler
{
    /// <summary>The area id the player is currently in (e.g. "ranch" / "town").</summary>
    string? ActiveAreaId { get; }

    /// <summary>
    /// Move the player to <paramref name="destinationAreaId"/>. Returns true on success.
    /// The handler owns visibility, process state, input ownership and the player/camera pose.
    /// </summary>
    bool TravelTo(string destinationAreaId);
}

/// <summary>
/// A world smart object the player approaches and "uses" to travel between authored areas
/// (ranch ↔ town). It carries a stable id, a label (prompt), a destination area id, and one
/// reserved interaction slot guarded against double activation.
///
/// It reuses the same <see cref="IWorldInteractable"/> contract and <see cref="WorldInteractionGuard"/>
/// as a service station, but dispatches to an <see cref="ITravelHandler"/> (presentation) instead of
/// the <see cref="IWorldCommandDispatcher"/> (simulation). That separation is deliberate: activating
/// a gate must never move a simulation number.
/// </summary>
public partial class WorldTravelGate : Area3D, IWorldInteractable
{
    /// <summary>Stable identity (e.g. "GATE_RANCH_TO_TOWN"); never a runtime index.</summary>
    [Export] public string TargetId { get; set; } = "GATE_UNKNOWN";

    /// <summary>Human-readable prompt label ("Head to Town", "Return to Ranch").</summary>
    [Export] public string Label { get; set; } = "Travel";

    /// <summary>The destination area id the handler should travel to (e.g. "town").</summary>
    [Export] public string Destination { get; set; } = string.Empty;

    /// <summary>The composition that performs the area swap. Injected by the scene/tests.</summary>
    public ITravelHandler? Handler { get; set; }

    private readonly WorldInteractionGuard _guard = new();

    public string? UnavailableReason =>
        Handler is null ? "no travel handler bound"
        : string.IsNullOrEmpty(Destination) ? "no destination area"
        : string.Empty;

    public bool IsAvailable =>
        _guard.CanInteract && Handler is not null && !string.IsNullOrEmpty(Destination);

    /// <summary>
    /// Travel through the handler. Returns true on success. Rejects double activation and a
    /// missing handler/destination. Never computes a reward.
    /// </summary>
    public bool Activate(WorldInteractionContext context)
    {
        if (Handler is null || string.IsNullOrEmpty(Destination) || !_guard.CanInteract)
        {
            return false;
        }

        if (!_guard.BeginCommand())
        {
            return false;
        }

        try
        {
            return Handler.TravelTo(Destination);
        }
        finally
        {
            _guard.EndCommand();
        }
    }

    /// <summary>Mark the gate present/absent (scene exit, load/new game, error).</summary>
    public void SetTargetPresent(bool present) => _guard.SetTargetPresent(present);

    public WorldInteractionGuard Guard => _guard;
}
