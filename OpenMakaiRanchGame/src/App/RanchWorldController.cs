using Godot;
using OpenMakaiRanch.Ui;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.App;

/// <summary>
/// Composes the 3D ranch world and the existing 2D management UI into one boot world, both on the
/// shared <see cref="GameRoot"/> (autoload — a single simulation).
///
/// Design (no second economy / clock / job path):
///   - The 3D world (<c>scenes/dev/RanchGreybox.tscn</c>) is the primary view: player, camera,
///     stations, day-phase lighting, and CHAR-001 roster stand-ins, all derived from the shared state.
///   - The management UI is the *existing* <c>scenes/Game.tscn</c> (the tested 2D game: economy,
///     clock, jobs, save/load) — not a duplicate. It opens as a full-viewport overlay over the world.
///   - World input ownership stays in the greybox controller's single <see cref="WorldInputGate"/>.
///     This controller drives it through the greybox's tested <c>EnterManagementUi</c> /
///     <c>LeaveManagementUi</c>, so opening management suspends world input (movement, camera,
///     interaction) and closing resumes it safely — one gate, never left dangling.
///
/// Presentation only: this controller holds no economy/clock/job state — it only switches which
/// presentation is active and routes input. The simulation stays in <see cref="GameRoot"/>.
/// </summary>
public partial class RanchWorldController : Node3D
{
    [Export] public NodePath WorldPath { get; set; } = "RanchWorld3D";
    [Export] public NodePath ManagementOverlayPath { get; set; } = "ManagementCanvas/Game";

    public enum Mode { World, Management }

    public Mode CurrentMode { get; private set; } = Mode.World;
    public bool ManagementOpen => CurrentMode == Mode.Management;

    /// <summary>The single world-input gate (owned by the greybox controller) — exposed for tests/UI.</summary>
    public WorldInputGate? InputGate => _greybox?.InputGate;

    private RanchGreyboxController? _greybox;
    private CanvasItem? _overlay;
    private UiShellController? _uiShell;

    /// <summary>Captured before the UiShell consumes it: a pending initial screen (e.g. character
    /// creation) means the player must start in management mode, not the 3D world.</summary>
    private bool _bootInManagement;

    public override void _EnterTree()
    {
        // Parent _EnterTree runs before children, so the pending screen is still set here.
        // UiShell._Ready consumes it afterwards; we only record the boot intent.
        _bootInManagement = GameRoot.PendingInitialScreen is not null;
    }

    public override void _Ready()
    {
        _greybox = GetNodeOrNull<RanchGreyboxController>(WorldPath);
        _overlay = GetNodeOrNull<CanvasItem>(ManagementOverlayPath);
        _uiShell = GetNodeOrNull<UiShellController>(ManagementOverlayPath + "/UiShell");

        if (_greybox is null)
        {
            GD.PushError("RanchWorldController: 3D world not found at " + WorldPath);
        }
        else
        {
            // The in-world "Open Management UI" button (greybox TSCN connection) requests the
            // overlay through this composition; the greybox already flipped the shared gate.
            _greybox.ManagementUiRequested += HandleManagementUiRequested;
        }

        // Boot in world mode: 3D visible, management overlay hidden, world owns input —
        // except when the player must complete a setup screen first (new game / character creation).
        if (_overlay is not null)
        {
            _overlay.Visible = _bootInManagement;
        }
        if (_bootInManagement)
        {
            _greybox?.EnterManagementUi();
            CurrentMode = Mode.Management;
        }
        else
        {
            CurrentMode = Mode.World;
        }
    }

    /// <summary>Open the management UI over the 3D world (world input suspended via the shared gate).</summary>
    public bool EnterManagement()
    {
        if (CurrentMode == Mode.Management)
        {
            return true; // already open
        }
        if (_greybox is null)
        {
            return false;
        }

        _greybox.EnterManagementUi(); // shared gate -> UI owns input (movement + camera + interaction stop)
        if (_overlay is not null)
        {
            _overlay.Visible = true;
        }
        _uiShell?.ShowScreen("ranch"); // the ranch overview is the management home
        CurrentMode = Mode.Management;
        return true;
    }

    /// <summary>Return to the 3D world (world input resumed safely via the shared gate).</summary>
    public bool ReturnToWorld()
    {
        if (CurrentMode == Mode.World)
        {
            return true; // already in world
        }
        _greybox?.LeaveManagementUi(); // shared gate -> world owns input again (never left UI-owned)
        if (_overlay is not null)
        {
            _overlay.Visible = false;
        }
        CurrentMode = Mode.World;
        return true;
    }

    /// <summary>Toggle between the 3D world and the management UI.</summary>
    public bool ToggleManagement()
    {
        return CurrentMode == Mode.World ? EnterManagement() : ReturnToWorld();
    }

    private void HandleManagementUiRequested()
    {
        // The greybox already flipped the shared gate (EnterManagementUi). Reveal the overlay and
        // record the mode. No double gate flip — EnterManagement would be a no-op on the gate.
        if (CurrentMode == Mode.Management)
        {
            return;
        }
        if (_overlay is not null)
        {
            _overlay.Visible = true;
        }
        _uiShell?.ShowScreen("ranch");
        CurrentMode = Mode.Management;
    }

    public override void _ExitTree()
    {
        if (_greybox is not null)
        {
            _greybox.ManagementUiRequested -= HandleManagementUiRequested;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Esc returns from management to the world. (The 2D UI keeps its own navigation while open.)
        if (@event.IsActionPressed("ui_cancel"))
        {
            _ = ReturnToWorld();
            GetViewport().SetInputAsHandled();
        }
    }
}
