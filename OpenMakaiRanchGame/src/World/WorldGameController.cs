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
    [Export] public NodePath IntroHousePath { get; set; } = "IntroHouse";
    [Export] public NodePath RanchPath { get; set; } = "RanchWorld";
    [Export] public NodePath TownPath { get; set; } = "TownWorld";
    [Export] public NodePath ManagementRootPath { get; set; } = "ManagementLayer/ManagementUi";
    [Export] public NodePath UiShellPath { get; set; } = "ManagementLayer/ManagementUi/UiShell";
    [Export] public NodePath ManagementButtonPath { get; set; } = "RanchWorld/WorldHud/ManagementButton";
    [Export] public NodePath AdvanceTimeButtonPath { get; set; } = "RanchWorld/WorldHud/AdvanceTimeButton";
    [Export] public NodePath ReturnToWorldButtonPath { get; set; } = "ManagementLayer/ManagementUi/UiShell/Margin/RootPanel/Root/TopBar/TopBarRow1/ReturnToWorldButton";
    [Export] public NodePath TransitionPath { get; set; } = "TransitionLayer/Transition";
    [Export] public NodePath PauseMenuPath { get; set; } = "PauseLayer/PauseMenu";
    [Export] public NodePath MobileControlsPath { get; set; } = "MobileControlsLayer/MobileControls";
    [Export] public NodePath FirstDayFlowPath { get; set; } = "StoryLayer/FirstDayFlow";

    private IntroHouseController? _introHouse;
    private RanchGreyboxController? _ranch;
    private TownWorldController? _town;
    private Control? _managementRoot;
    private UiShellController? _shell;
    private Button? _managementButton;
    private Button? _advanceTimeButton;
    private Button? _returnToWorldButton;
    private WorldTransitionController? _transition;
    private PauseMenuController? _pauseMenu;
    private MobileWorldControls? _mobileControls;
    private FirstDayFlowController? _firstDayFlow;
    private bool _flowLocksUi;
    private string _activeAreaId = "ranch";

    public bool IsManagementVisible => _managementRoot?.Visible == true;
    public bool FlowLocksUi => _flowLocksUi;
    public string ActiveAreaId => _activeAreaId;
    public IntroHouseController? IntroHouse => _introHouse;
    public RanchGreyboxController? Ranch => _ranch;
    public TownWorldController? Town => _town;
    public UiShellController? Shell => _shell;
    public PauseMenuController? PauseMenu => _pauseMenu;
    public WorldTransitionController? Transition => _transition;
    public MobileWorldControls? MobileControls => _mobileControls;
    public FirstDayFlowController? FirstDayFlow => _firstDayFlow;

    public override void _Ready()
    {
        _introHouse = GetNodeOrNull<IntroHouseController>(IntroHousePath);
        _ranch = GetNodeOrNull<RanchGreyboxController>(RanchPath);
        _town = GetNodeOrNull<TownWorldController>(TownPath);
        _managementRoot = GetNodeOrNull<Control>(ManagementRootPath);
        _shell = GetNodeOrNull<UiShellController>(UiShellPath);
        _managementButton = GetNodeOrNull<Button>(ManagementButtonPath);
        _advanceTimeButton = GetNodeOrNull<Button>(AdvanceTimeButtonPath);
        _returnToWorldButton = GetNodeOrNull<Button>(ReturnToWorldButtonPath);
        _transition = GetNodeOrNull<WorldTransitionController>(TransitionPath);
        _pauseMenu = GetNodeOrNull<PauseMenuController>(PauseMenuPath);
        _mobileControls = GetNodeOrNull<MobileWorldControls>(MobileControlsPath);
        _firstDayFlow = GetNodeOrNull<FirstDayFlowController>(FirstDayFlowPath);

        if (_introHouse is null || _ranch is null || _town is null || _managementRoot is null || _shell is null)
        {
            GD.PushError("WorldGameController could not bind IntroHouse + RanchWorld + TownWorld + management UI composition.");
            return;
        }

        _shell.ScreenChanged += OnShellScreenChanged;
        _shell.WorldTravelRequested += OnUiWorldTravelRequested;
        _ranch.CharacterInteractionRequested += OnCharacterInteractionRequested;
        _ranch.TravelRequested += OnTravelRequested;
        _town.TravelRequested += OnTravelRequested;
        _town.ServiceScreenRequested += OnTownServiceRequested;
        _town.CharacterInteractionRequested += OnCharacterInteractionRequested;
        if (_transition is not null)
        {
            _transition.Completed += OnTransitionCompleted;
        }
        if (_pauseMenu is not null)
        {
            _pauseMenu.ManagementScreenRequested += OnPauseManagementRequested;
        }
        if (_mobileControls is not null)
        {
            _mobileControls.InteractPressed += OnMobileInteractPressed;
            _mobileControls.ManagementPressed += ToggleManagement;
            _mobileControls.CycleWorkerPressed += OnMobileCycleWorker;
        }

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
        var savedArea = GameRoot.Instance is { } game && game.State.WorldAreaId is "ranch" or "town"
            ? game.State.WorldAreaId
            : "ranch";
        SetActiveArea(_flowLocksUi ? "ranch" : savedArea, reposition: false);
        ApplyManagementVisibility(_flowLocksUi);

        if (_flowLocksUi)
        {
            _transition?.HideImmediately();
        }
        else
        {
            _transition?.CoverInstant();
            RevealInitialWorld();
        }
    }

    public override void _ExitTree()
    {
        if (_shell is not null && GodotObject.IsInstanceValid(_shell))
        {
            _shell.ScreenChanged -= OnShellScreenChanged;
            _shell.WorldTravelRequested -= OnUiWorldTravelRequested;
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
            _town.CharacterInteractionRequested -= OnCharacterInteractionRequested;
        }
        if (_transition is not null && GodotObject.IsInstanceValid(_transition))
        {
            _transition.Completed -= OnTransitionCompleted;
        }
        if (_mobileControls is not null && GodotObject.IsInstanceValid(_mobileControls))
        {
            _mobileControls.InteractPressed -= OnMobileInteractPressed;
            _mobileControls.ManagementPressed -= ToggleManagement;
            _mobileControls.CycleWorkerPressed -= OnMobileCycleWorker;
        }
        if (_pauseMenu is not null && GodotObject.IsInstanceValid(_pauseMenu))
        {
            _pauseMenu.ManagementScreenRequested -= OnPauseManagementRequested;
            if (_pauseMenu.IsOpen)
            {
                _pauseMenu.Close();
            }
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
        RefreshMobileControls();
        if (Input.IsActionJustPressed("toggle_management") && _pauseMenu?.IsOpen != true)
        {
            ToggleManagement();
            return;
        }

        if (!Input.IsActionJustPressed("ui_cancel") || _flowLocksUi)
        {
            return;
        }

        if (IsManagementVisible)
        {
            CloseManagement();
            return;
        }

        if (_transition?.IsTransitioning == true || _firstDayFlow?.BlocksWorldInput == true)
        {
            return;
        }

        _pauseMenu?.Open(_activeAreaId);
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
        if (_firstDayFlow?.BlocksManagement == true)
        {
            ActiveStatus("Finish the current first-day tutorial step before opening Management.");
            return;
        }

        if (_activeAreaId == "intro")
        {
            ActiveStatus("Finish getting ready and head outside first.");
            return;
        }

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
    /// Story-only area switch. Intro is never persisted as a normal travel destination; the
    /// FirstDayStage determines whether a loaded game resumes there.
    /// </summary>
    public bool ActivateStoryArea(string areaId, bool reposition, bool firstArrival)
    {
        if (areaId is not ("intro" or "ranch"))
        {
            return false;
        }

        _transition?.CoverInstant();
        if (!SetActiveArea(areaId, reposition))
        {
            _transition?.HideImmediately();
            return false;
        }

        SetTransitionInputLock(true);
        RevealArea(areaId, firstArrival);
        return true;
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

        _transition?.CoverInstant();
        if (!SetActiveArea(destinationId, reposition: true))
        {
            _transition?.HideImmediately();
            SetTransitionInputLock(false);
            return false;
        }

        SetTransitionInputLock(true);
        GameRoot.Instance?.SetWorldArea(destinationId);
        RevealArea(destinationId, firstArrival: false);
        return true;
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

    private void OnMobileInteractPressed()
    {
        if (IsManagementVisible || _pauseMenu?.IsOpen == true || _transition?.IsTransitioning == true)
        {
            return;
        }

        if (_activeAreaId == "town")
        {
            _town?.TryInteract();
        }
        else
        {
            _ranch?.TryInteractWithNearestWorldTarget();
        }
    }

    private void OnMobileCycleWorker()
    {
        if (_activeAreaId == "ranch" && !IsManagementVisible)
        {
            _ranch?.CycleSelectedCharacter();
        }
    }

    private void OnPauseManagementRequested(string screenId)
    {
        OpenManagementScreen(screenId);
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

    private void OnUiWorldTravelRequested(string destinationId)
    {
        if (IsManagementVisible && !_flowLocksUi)
        {
            CloseManagement();
        }

        TravelTo(destinationId);
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
            _transition?.CoverInstant();
            SetActiveArea("ranch", reposition: false);
            GameRoot.Instance?.SetWorldArea("ranch");
            ApplyManagementVisibility(false);

            var game = GameRoot.Instance;
            var firstDayPending = game is not null
                && !game.State.NgPlusActive
                && game.State.Calendar.Day == 1
                && !game.State.Story.FirstDayCompleted;

            if (firstDayPending)
            {
                // ScreenChanged subscribers are not ordered by ownership. Re-evaluate the story
                // only after this host has released its full-screen UI lock.
                _firstDayFlow?.RefreshFromCurrentState();
            }
            else
            {
                SetTransitionInputLock(true);
                RevealArea("ranch", firstArrival: true);
            }
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
            var savedArea = GameRoot.Instance?.State.WorldAreaId;
            if (savedArea is "ranch" or "town" && savedArea != _activeAreaId)
            {
                SetActiveArea(savedArea, reposition: false);
            }

            ActiveLeaveManagement();
            _ranch.RefreshLiveWorld();
            _town.Refresh();
        }

        return true;
    }

    private bool SetActiveArea(string destinationId, bool reposition)
    {
        if (_introHouse is null || _ranch is null || _town is null)
        {
            return false;
        }

        if (destinationId is not ("intro" or "ranch" or "town"))
        {
            return false;
        }

        _activeAreaId = destinationId;
        var introActive = destinationId == "intro";
        var ranchActive = destinationId == "ranch";
        var townActive = destinationId == "town";

        _introHouse.Visible = introActive;
        _introHouse.ProcessMode = introActive ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
        _ranch.Visible = ranchActive;
        _ranch.ProcessMode = ranchActive ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
        _town.Visible = townActive;
        _town.ProcessMode = townActive ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;

        if (_introHouse.CameraRig?.GetNodeOrNull<Camera3D>("Camera") is { } introCamera)
        {
            introCamera.Current = introActive;
        }
        if (_ranch.CameraRig?.GetNodeOrNull<Camera3D>("Camera") is { } ranchCamera)
        {
            ranchCamera.Current = ranchActive;
        }
        if (_town.CameraRig?.GetNodeOrNull<Camera3D>("Camera") is { } townCamera)
        {
            townCamera.Current = townActive;
        }

        _introHouse.InputGate.Reset();
        _ranch.InputGate.Reset();
        _town.InputGate.Reset();

        _mobileControls?.Bind(
            introActive ? _introHouse.Player : ranchActive ? _ranch.Player : _town.Player,
            introActive ? _introHouse.CameraRig : ranchActive ? _ranch.CameraRig : _town.CameraRig,
            showCycle: ranchActive);

        if (reposition)
        {
            if (introActive && _introHouse.Player is not null)
            {
                _introHouse.Player.GlobalPosition = new Vector3(-1.2f, 0.8f, 0.4f);
            }
            else if (ranchActive && _ranch.Player is not null)
            {
                _ranch.Player.GlobalPosition = new Vector3(0f, 0.8f, 10.5f);
                _ranch.Hud?.SetStatus("Entered the ranch grounds.");
            }
            else if (townActive && _town.Player is not null)
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
        if (_activeAreaId == "town") _town?.EnterManagementUi();
        else if (_activeAreaId == "intro") _introHouse?.EnterStoryUi();
        else _ranch?.EnterManagementUi();
    }

    private void ActiveLeaveManagement()
    {
        if (_activeAreaId == "town") _town?.LeaveManagementUi();
        else if (_activeAreaId == "intro") _introHouse?.LeaveStoryUi();
        else _ranch?.LeaveManagementUi();
    }

    private void OnTransitionCompleted()
    {
        SetTransitionInputLock(false);
    }

    private void SetTransitionInputLock(bool locked)
    {
        if (_activeAreaId == "town") _town?.InputGate.SetUiOwnsInput(locked || IsManagementVisible);
        else if (_activeAreaId == "intro") _introHouse?.InputGate.SetUiOwnsInput(locked || IsManagementVisible);
        else _ranch?.InputGate.SetUiOwnsInput(locked || IsManagementVisible);
    }

    private void RevealInitialWorld()
    {
        SetTransitionInputLock(true);
        RevealArea(_activeAreaId, firstArrival: false);
    }

    private void RevealArea(string areaId, bool firstArrival)
    {
        if (_transition is null || GameRoot.Instance is not { } game)
        {
            return;
        }

        var location = areaId switch
        {
            "intro" => "Ranch House",
            "town" => "Okachi Town",
            _ => string.IsNullOrWhiteSpace(game.State.Player.RanchName) ? "Okachi Ranch" : game.State.Player.RanchName
        };

        var subtitle = areaId == "intro"
            ? "Day 1 • Morning — A familiar voice is trying to wake you."
            : firstArrival
                ? $"Day {game.State.Calendar.Day} • {game.State.Calendar.Phase} — Your first guided day on the ranch begins."
                : $"Day {game.State.Calendar.Day} • {game.State.Calendar.Phase} — {(areaId == "town" ? "Town services are open." : "Welcome back to the ranch.")}";

        _transition.Reveal(location, subtitle, firstArrival ? 1.15 : 0.55, firstArrival ? 1.05 : 0.65);
        if (game.State.Settings.ReducedMotion)
        {
            _transition.CompleteImmediately();
        }
    }

    private void RefreshMobileControls()
    {
        if (_mobileControls is null)
        {
            return;
        }

        var blocked = _flowLocksUi
            || IsManagementVisible
            || _pauseMenu?.IsOpen == true
            || _transition?.IsTransitioning == true
            || _firstDayFlow?.BlocksWorldInput == true;
        _mobileControls.SetBlocked(blocked);
    }

    private void ActiveStatus(string message)
    {
        if (_activeAreaId == "town") _town?.Hud?.SetStatus(message);
        else if (_activeAreaId == "intro") GD.Print($"Intro: {message}");
        else _ranch?.Hud?.SetStatus(message);
    }

    private static bool RequiresFullScreenUi(string screenId)
    {
        return screenId is "character_creation" or "prologue" or "victory" or "title";
    }
}
