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

        _wired = true;
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
