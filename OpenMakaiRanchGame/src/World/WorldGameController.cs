using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Ui;

namespace OpenMakaiRanch.World;

/// <summary>
/// Composes the persistent 3D world areas (ranch + Okachi Town) with the existing management
/// Game.tscn overlay on one GameRoot. This host owns only area visibility, camera/input ownership,
/// spatial travel and UI routing; it never duplicates shop, research, adventure or settlement logic.
/// </summary>
public partial class WorldGameController : Node
{
    [Export] public NodePath RanchPath { get; set; } = "RanchWorld";
    [Export] public NodePath TownPath { get; set; } = "TownWorld";
    [Export] public NodePath ManagementRootPath { get; set; } = "ManagementLayer/ManagementUi";
    [Export] public NodePath UiShellPath { get; set; } = "ManagementLayer/ManagementUi/UiShell";
    [Export] public NodePath ManagementButtonPath { get; set; } = "RanchWorld/WorldHud/ManagementButton";
    [Export] public NodePath AdvanceTimeButtonPath { get; set; } = "RanchWorld/WorldHud/AdvanceTimeButton";
    [Export] public NodePath ReturnToWorldButtonPath { get; set; } = "ManagementLayer/ManagementUi/UiShell/Margin/RootPanel/Root/TopBar/TopBarRow1/ReturnToWorldButton";

    private RanchGreyboxController? _ranch;
    private TownWorldController? _town;
    private Control? _managementRoot;
    private UiShellController? _shell;
    private Button? _managementButton;
    private Button? _advanceTimeButton;
    private Button? _returnToWorldButton;
    private bool _flowLocksUi;
    private string _activeAreaId = "ranch";

    public bool IsManagementVisible => _managementRoot?.Visible == true;
    public bool FlowLocksUi => _flowLocksUi;
    public string ActiveAreaId => _activeAreaId;
    public RanchGreyboxController? Ranch => _ranch;
    public TownWorldController? Town => _town;
    public UiShellController? Shell => _shell;

    public override void _Ready()
    {
        _ranch = GetNodeOrNull<RanchGreyboxController>(RanchPath);
        _town = GetNodeOrNull<TownWorldController>(TownPath);
        _managementRoot = GetNodeOrNull<Control>(ManagementRootPath);
        _shell = GetNodeOrNull<UiShellController>(UiShellPath);
        _managementButton = GetNodeOrNull<Button>(ManagementButtonPath);
        _advanceTimeButton = GetNodeOrNull<Button>(AdvanceTimeButtonPath);
        _returnToWorldButton = GetNodeOrNull<Button>(ReturnToWorldButtonPath);

        if (_ranch is null || _town is null || _managementRoot is null || _shell is null)
        {
            GD.PushError("WorldGameController could not bind RanchWorld + TownWorld + management UI composition.");
            return;
        }

        _shell.ScreenChanged += OnShellScreenChanged;
        _ranch.CharacterInteractionRequested += OnCharacterInteractionRequested;
        _ranch.TravelRequested += OnTravelRequested;
        _town.TravelRequested += OnTravelRequested;
        _town.ServiceScreenRequested += OnTownServiceRequested;

        if (_managementButton is not null)
        {
            _managementButton.Pressed += ToggleManagement;
        }
        if (_advanceTimeButton is not null)
        {
            _advanceTimeButton.Pressed += AdvanceWorldTime;
        }
        if (_returnToWorldButton is not null)
        {
            _returnToWorldButton.Pressed += CloseManagementFromUi;
        }

        _flowLocksUi = RequiresFullScreenUi(_shell.CurrentScreen);
        SetActiveArea("ranch", reposition: false);
        ApplyManagementVisibility(_flowLocksUi);
    }

    public override void _ExitTree()
    {
        if (_shell is not null && GodotObject.IsInstanceValid(_shell))
        {
            _shell.ScreenChanged -= OnShellScreenChanged;
        }

        if (_ranch is not null && GodotObject.IsInstanceValid(_ranch))
        {
            _ranch.CharacterInteractionRequested -= OnCharacterInteractionRequested;
            _ranch.TravelRequested -= OnTravelRequested;
        }

        if (_town is not null && GodotObject.IsInstanceValid(_town))
        {
            _town.TravelRequested -= OnTravelRequested;
            _town.ServiceScreenRequested -= OnTownServiceRequested;
        }

        if (_managementButton is not null && GodotObject.IsInstanceValid(_managementButton))
        {
            _managementButton.Pressed -= ToggleManagement;
        }
        if (_advanceTimeButton is not null && GodotObject.IsInstanceValid(_advanceTimeButton))
        {
            _advanceTimeButton.Pressed -= AdvanceWorldTime;
        }
        if (_returnToWorldButton is not null && GodotObject.IsInstanceValid(_returnToWorldButton))
        {
            _returnToWorldButton.Pressed -= CloseManagementFromUi;
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("toggle_management"))
        {
            ToggleManagement();
            return;
        }

        if (IsManagementVisible && !_flowLocksUi && Input.IsActionJustPressed("ui_cancel"))
        {
            CloseManagement();
        }
    }

    public bool OpenManagement()
    {
        return ApplyManagementVisibility(true);
    }

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
            return;
        }

        if (_shell is not null)
        {
            _shell.ShowScreen(_activeAreaId == "town" ? "town" : "ranch");
        }
        OpenManagement();
    }

    private void CloseManagementFromUi()
    {
        CloseManagement();
    }

    /// <summary>
    /// Travel between world areas without inventing a cost that the shared simulation does not own.
    /// </summary>
    public bool TravelTo(string destinationId)
    {
        if (IsManagementVisible || _flowLocksUi)
        {
            return false;
        }

        return SetActiveArea(destinationId, reposition: true);
    }

    public void AdvanceWorldTime()
    {
        var game = GameRoot.Instance;
        if (game is null || !GodotObject.IsInstanceValid(game))
        {
            return;
        }

        if (game.State.Calendar.Phase == OpenMakaiRanch.Core.Models.DayPhase.Night
            && game.State.Calendar.NightAction is not ("rest" or "train" or "admin"))
        {
            OpenManagementScreen("ranch");
            _ranch?.Hud?.SetStatus("Choose tonight's work in management before ending the day.");
            return;
        }

        var dayBefore = game.State.Calendar.Day;
        if (!game.AdvanceTime())
        {
            ActiveStatus("Time could not be advanced.");
            return;
        }

        if (game.State.Calendar.Day != dayBefore
            && game.State.Calendar.Phase == OpenMakaiRanch.Core.Models.DayPhase.Morning
            && game.LastDailyReport is not null)
        {
            OpenManagementScreen("report");
            return;
        }

        _ranch?.RefreshLiveWorld();
        _town?.Refresh();
        ActiveStatus($"Advanced to {game.State.Calendar.Phase}.");
    }

    public bool OpenManagementScreen(string screenId)
    {
        if (_shell is null)
        {
            return false;
        }

        _shell.ShowScreen(screenId);
        return OpenManagement();
    }

    private void OnCharacterInteractionRequested(string characterId)
    {
        if (_shell is null || !_shell.ShowCharacterDetailFromWorld(characterId))
        {
            ActiveStatus("Character details are unavailable.");
            return;
        }

        OpenManagement();
    }

    private void OnTownServiceRequested(string screenId)
    {
        if (string.IsNullOrWhiteSpace(screenId) || _shell is null)
        {
            _town?.Hud?.SetStatus("This town service is unavailable.");
            return;
        }

        _shell.ShowScreen(screenId);
        OpenManagement();
    }

    private void OnTravelRequested(string destinationId)
    {
        if (!TravelTo(destinationId))
        {
            ActiveStatus("Travel is unavailable while another interface owns input.");
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

        // Mandatory new-game flow always begins at the ranch.
        if (wasLocked && screenId == "ranch")
        {
            SetActiveArea("ranch", reposition: false);
            ApplyManagementVisibility(false);
        }
    }

    private bool ApplyManagementVisibility(bool visible)
    {
        if (_managementRoot is null || _ranch is null || _town is null)
        {
            return false;
        }

        if (!visible && _flowLocksUi)
        {
            return false;
        }

        _managementRoot.Visible = visible;
        if (_returnToWorldButton is not null)
        {
            _returnToWorldButton.Visible = visible && !_flowLocksUi;
            _returnToWorldButton.Disabled = _flowLocksUi;
            _returnToWorldButton.Text = _activeAreaId == "town" ? "Return to Town" : "Return to World";
        }

        if (visible)
        {
            ActiveEnterManagement();
        }
        else
        {
            ActiveLeaveManagement();
            _ranch.RefreshLiveWorld();
            _town.Refresh();
        }

        return true;
    }

    private bool SetActiveArea(string destinationId, bool reposition)
    {
        if (_ranch is null || _town is null)
        {
            return false;
        }

        if (destinationId is not ("ranch" or "town"))
        {
            return false;
        }

        _activeAreaId = destinationId;
        var ranchActive = destinationId == "ranch";

        _ranch.Visible = ranchActive;
        _ranch.ProcessMode = ranchActive ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
        _town.Visible = !ranchActive;
        _town.ProcessMode = ranchActive ? ProcessModeEnum.Disabled : ProcessModeEnum.Inherit;

        if (_ranch.CameraRig?.GetNodeOrNull<Camera3D>("Camera") is { } ranchCamera)
        {
            ranchCamera.Current = ranchActive;
        }
        if (_town.CameraRig?.GetNodeOrNull<Camera3D>("Camera") is { } townCamera)
        {
            townCamera.Current = !ranchActive;
        }

        // Inactive areas never retain UI ownership.
        _ranch.InputGate.Reset();
        _town.InputGate.Reset();

        if (reposition)
        {
            if (ranchActive && _ranch.Player is not null)
            {
                _ranch.Player.GlobalPosition = new Vector3(0f, 0.8f, 10.5f);
                _ranch.Hud?.SetStatus("Returned to the ranch.");
            }
            else if (!ranchActive && _town.Player is not null)
            {
                _town.Player.GlobalPosition = new Vector3(0f, 0.8f, 10.2f);
                var game = GameRoot.Instance;
                if (game is not null && !game.HasSeenTutorial("town_arrival"))
                {
                    _town.Hud?.SetStatus("Welcome to Okachi Town. Walk to a building and press F; the south gate returns to the ranch.", 6.0);
                    game.MarkTutorialSeen("town_arrival");
                }
                else
                {
                    _town.Hud?.SetStatus("Arrived in Okachi Town.");
                }
            }
        }

        _ranch.RefreshLiveWorld();
        _town.Refresh();
        return true;
    }

    private void ActiveEnterManagement()
    {
        if (_activeAreaId == "town")
        {
            _town?.EnterManagementUi();
        }
        else
        {
            _ranch?.EnterManagementUi();
        }
    }

    private void ActiveLeaveManagement()
    {
        if (_activeAreaId == "town")
        {
            _town?.LeaveManagementUi();
        }
        else
        {
            _ranch?.LeaveManagementUi();
        }
    }

    private void ActiveStatus(string message)
    {
        if (_activeAreaId == "town")
        {
            _town?.Hud?.SetStatus(message);
        }
        else
        {
            _ranch?.Hud?.SetStatus(message);
        }
    }

    private static bool RequiresFullScreenUi(string screenId)
    {
        return screenId is "character_creation" or "prologue" or "victory" or "title";
    }
}
