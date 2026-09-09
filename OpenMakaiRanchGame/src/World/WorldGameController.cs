using Godot;
using OpenMakaiRanch.Ui;

namespace OpenMakaiRanch.World;

/// <summary>
/// Composes the playable 3D ranch with the existing management Game.tscn as a full-viewport
/// overlay. Both views share the same GameRoot; this host owns presentation/input visibility only.
///
/// Mandatory full-screen flows (character creation, prologue, victory/title) lock the overlay open.
/// Leaving such a flow for the ranch automatically reveals the 3D world. During ordinary play the
/// player may toggle management with the mapped action or the HUD button.
/// </summary>
public partial class WorldGameController : Node
{
    [Export] public NodePath RanchPath { get; set; } = "RanchWorld";
    [Export] public NodePath ManagementRootPath { get; set; } = "ManagementLayer/ManagementUi";
    [Export] public NodePath UiShellPath { get; set; } = "ManagementLayer/ManagementUi/UiShell";
    [Export] public NodePath ManagementButtonPath { get; set; } = "RanchWorld/WorldHud/ManagementButton";

    private RanchGreyboxController? _ranch;
    private Control? _managementRoot;
    private UiShellController? _shell;
    private Button? _managementButton;
    private bool _flowLocksUi;

    public bool IsManagementVisible => _managementRoot?.Visible == true;
    public bool FlowLocksUi => _flowLocksUi;
    public RanchGreyboxController? Ranch => _ranch;
    public UiShellController? Shell => _shell;

    public override void _Ready()
    {
        _ranch = GetNodeOrNull<RanchGreyboxController>(RanchPath);
        _managementRoot = GetNodeOrNull<Control>(ManagementRootPath);
        _shell = GetNodeOrNull<UiShellController>(UiShellPath);
        _managementButton = GetNodeOrNull<Button>(ManagementButtonPath);

        if (_ranch is null || _managementRoot is null || _shell is null)
        {
            GD.PushError("WorldGameController could not bind RanchWorld + management UI composition.");
            return;
        }

        _shell.ScreenChanged += OnShellScreenChanged;
        if (_managementButton is not null)
        {
            _managementButton.Pressed += ToggleManagement;
        }

        _flowLocksUi = RequiresFullScreenUi(_shell.CurrentScreen);
        ApplyManagementVisibility(_flowLocksUi);
    }

    public override void _ExitTree()
    {
        if (_shell is not null && GodotObject.IsInstanceValid(_shell))
        {
            _shell.ScreenChanged -= OnShellScreenChanged;
        }

        if (_managementButton is not null && GodotObject.IsInstanceValid(_managementButton))
        {
            _managementButton.Pressed -= ToggleManagement;
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("toggle_management"))
        {
            ToggleManagement();
        }
    }

    /// <summary>Open the existing management shell and suspend 3D-world input.</summary>
    public bool OpenManagement()
    {
        return ApplyManagementVisibility(true);
    }

    /// <summary>
    /// Close management and return control to the 3D world. Mandatory full-screen flows cannot be
    /// hidden because that would strand character creation/prologue/victory behind the world.
    /// </summary>
    public bool CloseManagement()
    {
        if (_flowLocksUi)
        {
            return false;
        }

        return ApplyManagementVisibility(false);
    }

    public void ToggleManagement()
    {
        if (IsManagementVisible)
        {
            CloseManagement();
        }
        else
        {
            OpenManagement();
        }
    }

    private void OnShellScreenChanged(string screenId)
    {
        var wasLocked = _flowLocksUi;
        _flowLocksUi = RequiresFullScreenUi(screenId);

        if (_flowLocksUi)
        {
            ApplyManagementVisibility(true);
            return;
        }

        // New game: character creation -> prologue -> ranch. When that mandatory UI flow finishes,
        // automatically reveal the world instead of making the user close a redundant overlay.
        if (wasLocked && screenId == "ranch")
        {
            ApplyManagementVisibility(false);
        }
    }

    private bool ApplyManagementVisibility(bool visible)
    {
        if (_managementRoot is null || _ranch is null)
        {
            return false;
        }

        if (!visible && _flowLocksUi)
        {
            return false;
        }

        _managementRoot.Visible = visible;
        if (visible)
        {
            _ranch.EnterManagementUi();
        }
        else
        {
            _ranch.LeaveManagementUi();
            _ranch.RefreshLiveWorld();
        }

        return true;
    }

    private static bool RequiresFullScreenUi(string screenId)
    {
        return screenId is "character_creation" or "prologue" or "victory" or "title";
    }
}
