using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Scene controller for the opt-in <c>scenes/dev/RanchGreybox.tscn</c>. It wires the shared
/// input gate, the command dispatcher, and the interact prompt, and handles the single
/// "interact" action: find the nearest in-range station and dispatch through the guard.
///
/// This node owns no simulation. It only routes input to the controller / camera / station
/// and surfaces a prompt. All game effects flow through <see cref="GameRoot"/>.
/// </summary>
public partial class RanchGreyboxController : Node3D
{
    [Export] public float InteractionRange { get; set; } = 2.5f;

    public WorldInputGate InputGate { get; private set; } = new();

    private ThirdPersonPlayerController? _player;
    private WorldStation? _station;
    private Label? _prompt;
    private bool _wired;

    public ThirdPersonPlayerController? Player => _player;
    public WorldStation? Station => _station;
    public bool Wired => _wired;

    /// <summary>Applies the shared phase to the scene's sun + environment (WORLD-003 lighting).</summary>
    public DaylightRig? Daylight { get; private set; }

    /// <summary>Places CHAR-001 stand-ins for the live roster (WORLD-003 / AI-001 placement).</summary>
    public RosterRig? Roster { get; private set; }

    public override void _Ready()
    {
        _player = GetNodeOrNull<ThirdPersonPlayerController>("Player");
        _station = GetNodeOrNull<WorldStation>("Station");

        // Shared input gate: the player reads it, the controller drives it.
        if (_player is not null)
        {
            _player.InputGate = InputGate;
        }

        // Production dispatcher binding the station to GameRoot.
        if (_station is not null && _station.Dispatcher is null)
        {
            _station.Dispatcher = new GameRootCommandDispatcher();
        }

        // Prompt (optional; the scene may author it).
        var promptLayer = GetNodeOrNull<CanvasLayer>("PromptLayer");
        if (promptLayer is not null)
        {
            _prompt = promptLayer.GetNodeOrNull<Label>("Prompt");
        }

        // WORLD-003: the scene is a live view of the shared simulation, not a static greybox.
        // Lighting derives from the current DayPhase; roster placement derives from assignments.
        WireLiveWorld();

        _wired = true;
    }

    /// <summary>
    /// Bind the day-phase lighting + roster placement to the shared <see cref="GameRoot"/> and
    /// apply them for the current state. Safe when a rig or the GameRoot is missing (headless,
    /// pre-boot) — the scene then simply stays in its authored state.
    /// </summary>
    private void WireLiveWorld()
    {
        var game = GameRoot.Instance;
        if (game is null || !GodotObject.IsInstanceValid(game))
        {
            return;
        }

        var dayRig = GetNodeOrNull<DaylightRig>("DaylightRig");
        if (dayRig is not null)
        {
            dayRig.Bind(GetNodeOrNull<DirectionalLight3D>("Sun"), GetNodeOrNull<WorldEnvironment>("WorldEnvironment"));
            dayRig.ApplyFrom(game);
            Daylight = dayRig;
        }

        var rosterRig = GetNodeOrNull<RosterRig>("RosterRig");
        if (rosterRig is not null)
        {
            rosterRig.Refresh(game);
            Roster = rosterRig;
        }
    }

    /// <summary>
    /// Re-derive the live world from the shared simulation — call when the phase or assignments
    /// change (e.g. after End Day, after a world interaction, after loading).
    /// </summary>
    public void RefreshLiveWorld()
    {
        var game = GameRoot.Instance;
        if (game is null || !GodotObject.IsInstanceValid(game))
        {
            return;
        }

        Daylight?.ApplyFrom(game);
        Roster?.Refresh(game);
    }

    /// <summary>
    /// Open the management UI: the world loses input ownership. Called by the scene's
    /// "Open Management UI" button.
    /// </summary>
    public void EnterManagementUi()
    {
        InputGate.SetUiOwnsInput(true);
    }

    /// <summary>
    /// Leave the management UI: the world regains input ownership.
    /// </summary>
    public void LeaveManagementUi()
    {
        InputGate.SetUiOwnsInput(false);
    }

    public override void _Input(InputEvent @event)
    {
        if (!InputGate.WorldInputEnabled)
        {
            return;
        }

        if (@event is InputEventAction action && action.Pressed && action.Action == "interact")
        {
            HandleInteract();
        }
    }

    private void HandleInteract()
    {
        if (_player is null || _station is null)
        {
            return;
        }

        var distance = _player.GlobalPosition.DistanceTo(_station.GlobalPosition);
        if (distance > InteractionRange || !_station.IsAvailable)
        {
            return;
        }

        var generation = ResolveGeneration();
        var context = new WorldInteractionContext(_station.TargetId, generation);
        var ok = _station.Activate(context);
        if (_prompt is not null)
        {
            _prompt.Text = ok ? $"{_station.Label}: done" : $"{_station.Label}: {_station.UnavailableReason}";
        }
    }

    private ulong ResolveGeneration()
    {
        return GameRoot.Instance?.StateGeneration ?? 0UL;
    }
}
