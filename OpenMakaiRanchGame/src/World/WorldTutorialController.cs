using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.World;

/// <summary>
/// First-run world onboarding plus an always-available help surface.
///
/// The tutorial observes presentation/game state but does not own gameplay. Completion records live
/// in SettingsState so the same player is not forced through basic controls for every save slot.
/// </summary>
public partial class WorldTutorialController : Control
{
    private sealed record TutorialStep(string Id, string Title, string Body, string KeyHint);

    private static readonly TutorialStep[] Steps =
    {
        new("world_move", "Move around the ranch",
            "Use WASD or the arrow keys to walk. Hold Shift when you want to move faster.",
            "WASD / Arrows  •  Shift = sprint"),
        new("world_camera", "Look around",
            "Hold the right mouse button and move the mouse to orbit the camera. Use the mouse wheel to zoom.",
            "Hold RMB = look  •  Wheel = zoom  •  R = recenter"),
        new("world_assign", "Give a worker a job",
            "Tab changes the selected worker. Walk to a built work station such as the Pasture or Kitchen and press F to assign that worker.",
            "Tab = worker  •  F = use nearby station"),
        new("world_management", "Open ranch management",
            "Management contains the full schedule, facilities, roster, town, research, save/load and other detailed systems. Open it whenever you need a deeper decision.",
            "M = management  •  Esc / Return to World = close"),
        new("world_town", "Visit Okachi Town",
            "Follow the south road to the town gate and press F. Town services use the same shared shop, research, adventure, roster, bond and milestone systems.",
            "South gate → F = travel to town"),
        new("world_time", "Advance the day",
            "Use Advance Phase in the world HUD when you are ready. At Night, choose a night plan before ending the day. Settlement then opens the Daily Report.",
            "Advance Phase → Night plan → End Day → Daily Report")
    };

    private RanchGreyboxController? _ranch;
    private WorldGameController? _worldGame;
    private PanelContainer? _hintCard;
    private Label? _stepLabel;
    private Label? _titleLabel;
    private Label? _bodyLabel;
    private Label? _keyLabel;
    private Button? _dismissButton;
    private Button? _skipButton;
    private Button? _helpButton;
    private PanelContainer? _helpPanel;
    private Button? _closeHelpButton;
    private CheckButton? _tutorialToggle;
    private Button? _resetTutorialButton;

    private Vector3 _startPosition;
    private DayPhase _initialPhase;
    private int _initialDay;
    private bool _cameraUsed;
    private bool _stationUsed;
    private bool _managementOpened;
    private bool _townTravelUsed;
    private bool _timeAdvanced;
    private int _visibleStepIndex = -1;

    public int VisibleStepIndex => _visibleStepIndex;
    public bool HelpVisible => _helpPanel?.Visible == true;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;

        _ranch = GetNodeOrNull<RanchGreyboxController>("../..");
        _worldGame = GetNodeOrNull<WorldGameController>("../../..");

        _hintCard = GetNodeOrNull<PanelContainer>("HintCard");
        _stepLabel = GetNodeOrNull<Label>("HintCard/Inner/StepLabel");
        _titleLabel = GetNodeOrNull<Label>("HintCard/Inner/TitleLabel");
        _bodyLabel = GetNodeOrNull<Label>("HintCard/Inner/BodyLabel");
        _keyLabel = GetNodeOrNull<Label>("HintCard/Inner/KeyLabel");
        _dismissButton = GetNodeOrNull<Button>("HintCard/Inner/Actions/DismissButton");
        _skipButton = GetNodeOrNull<Button>("HintCard/Inner/Actions/SkipButton");
        _helpButton = GetNodeOrNull<Button>("HelpButton");
        _helpPanel = GetNodeOrNull<PanelContainer>("HelpPanel");
        _closeHelpButton = GetNodeOrNull<Button>("HelpPanel/Inner/CloseButton");
        _tutorialToggle = GetNodeOrNull<CheckButton>("HelpPanel/Inner/TutorialToggle");
        _resetTutorialButton = GetNodeOrNull<Button>("HelpPanel/Inner/ResetTutorialButton");

        if (_helpPanel is not null)
        {
            _helpPanel.Visible = false;
        }

        if (_ranch?.Player is not null)
        {
            _startPosition = _ranch.Player.GlobalPosition;
        }

        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            _initialPhase = game.State.Calendar.Phase;
            _initialDay = game.State.Calendar.Day;
            if (_tutorialToggle is not null)
            {
                _tutorialToggle.ButtonPressed = game.State.Settings.TutorialHintsEnabled;
            }

            game.StateChanged += OnSharedStateChanged;
        }

        if (_ranch is not null)
        {
            _ranch.StationInteractionSucceeded += OnStationInteractionSucceeded;
            _ranch.TravelRequested += OnTravelRequested;
        }

        if (_dismissButton is not null)
        {
            _dismissButton.Pressed += DismissCurrentStep;
            _dismissButton.TooltipText = "Mark this tutorial step complete and continue.";
        }
        if (_skipButton is not null)
        {
            _skipButton.Pressed += SkipTutorial;
            _skipButton.TooltipText = "Mark the basic world tutorial complete. Help remains available with F1.";
        }
        if (_helpButton is not null)
        {
            _helpButton.Pressed += ToggleHelp;
            _helpButton.TooltipText = "Controls, gameplay loop and tutorial options (F1).";
        }
        if (_closeHelpButton is not null)
        {
            _closeHelpButton.Pressed += CloseHelp;
        }
        if (_tutorialToggle is not null)
        {
            _tutorialToggle.Toggled += SetTutorialHints;
        }
        if (_resetTutorialButton is not null)
        {
            _resetTutorialButton.Pressed += ResetTutorial;
            _resetTutorialButton.TooltipText = "Show the basic ranch tutorial again.";
        }

        RefreshTutorial();
    }

    public override void _ExitTree()
    {
        if (_ranch is not null && GodotObject.IsInstanceValid(_ranch))
        {
            _ranch.StationInteractionSucceeded -= OnStationInteractionSucceeded;
            _ranch.TravelRequested -= OnTravelRequested;
        }

        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged -= OnSharedStateChanged;
        }

        if (_ranch is not null && GodotObject.IsInstanceValid(_ranch)
            && _helpPanel?.Visible == true && _worldGame?.IsManagementVisible != true)
        {
            _ranch.InputGate.SetUiOwnsInput(false);
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("open_help"))
        {
            ToggleHelp();
        }

        if (_worldGame?.IsManagementVisible == true)
        {
            _managementOpened = true;
            if (_helpPanel?.Visible == true)
            {
                CloseHelpInternal(releaseWorldInput: false);
            }
        }

        if (!_cameraUsed && _ranch?.InputGate.WorldInputEnabled == true)
        {
            _cameraUsed = Input.IsMouseButtonPressed(MouseButton.Right)
                || Input.IsActionPressed("camera_look_left")
                || Input.IsActionPressed("camera_look_right")
                || Input.IsActionPressed("camera_look_up")
                || Input.IsActionPressed("camera_look_down")
                || Input.IsActionJustPressed("camera_zoom_in")
                || Input.IsActionJustPressed("camera_zoom_out")
                || Input.IsActionJustPressed("camera_recenter");
        }

        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            _timeAdvanced |= game.State.Calendar.Day != _initialDay || game.State.Calendar.Phase != _initialPhase;
        }

        RefreshTutorial();
    }

    public void ToggleHelp()
    {
        if (_helpPanel is null || _worldGame?.IsManagementVisible == true)
        {
            return;
        }

        if (_helpPanel.Visible)
        {
            CloseHelp();
            return;
        }

        _helpPanel.Visible = true;
        _ranch?.InputGate.SetUiOwnsInput(true);
    }

    public void CloseHelp()
    {
        CloseHelpInternal(releaseWorldInput: true);
    }

    public void DismissCurrentStep()
    {
        var game = GameRoot.Instance;
        if (game is null || _visibleStepIndex < 0 || _visibleStepIndex >= Steps.Length)
        {
            return;
        }

        game.MarkTutorialSeen(Steps[_visibleStepIndex].Id);
        RefreshTutorial();
    }

    public void SkipTutorial()
    {
        var game = GameRoot.Instance;
        if (game is null)
        {
            return;
        }

        foreach (var step in Steps)
        {
            game.MarkTutorialSeen(step.Id);
        }
        game.MarkTutorialSeen("world_tutorial_complete");
        _ranch?.Hud?.SetStatus("World tutorial skipped. Press F1 whenever you need help.");
        RefreshTutorial();
    }

    private void ResetTutorial()
    {
        if (GameRoot.Instance is not { } game)
        {
            return;
        }

        game.ResetTutorialProgress();
        game.SetTutorialHintsEnabled(true);
        _cameraUsed = false;
        _stationUsed = false;
        _managementOpened = false;
        _townTravelUsed = false;
        _timeAdvanced = false;
        _startPosition = _ranch?.Player?.GlobalPosition ?? Vector3.Zero;
        _initialPhase = game.State.Calendar.Phase;
        _initialDay = game.State.Calendar.Day;
        if (_tutorialToggle is not null)
        {
            _tutorialToggle.ButtonPressed = true;
        }
        CloseHelp();
        RefreshTutorial();
    }

    private void SetTutorialHints(bool enabled)
    {
        GameRoot.Instance?.SetTutorialHintsEnabled(enabled);
        RefreshTutorial();
    }

    private void OnStationInteractionSucceeded(string characterId, string targetId)
    {
        _stationUsed = true;
        RefreshTutorial();
    }

    private void OnTravelRequested(string destinationId)
    {
        if (destinationId == "town")
        {
            _townTravelUsed = true;
            GameRoot.Instance?.MarkTutorialSeen("world_town");
            RefreshTutorial();
        }
    }

    private void OnSharedStateChanged()
    {
        if (GameRoot.Instance is { } game && _tutorialToggle is not null)
        {
            _tutorialToggle.SetPressedNoSignal(game.State.Settings.TutorialHintsEnabled);
        }
        RefreshTutorial();
    }

    private void RefreshTutorial()
    {
        var game = GameRoot.Instance;
        if (_hintCard is null || game is null || !GodotObject.IsInstanceValid(game))
        {
            return;
        }

        AutoCompleteObservedSteps(game);

        if (!game.State.Settings.TutorialHintsEnabled || game.HasSeenTutorial("world_tutorial_complete"))
        {
            _visibleStepIndex = -1;
            _hintCard.Visible = false;
            return;
        }

        var next = Array.FindIndex(Steps, step => !game.HasSeenTutorial(step.Id));
        if (next < 0)
        {
            game.MarkTutorialSeen("world_tutorial_complete");
            _visibleStepIndex = -1;
            _hintCard.Visible = false;
            _ranch?.Hud?.SetStatus("Tutorial complete. Press F1 to reopen controls and help.");
            return;
        }

        _visibleStepIndex = next;
        _hintCard.Visible = _worldGame?.IsManagementVisible != true;
        if (!_hintCard.Visible)
        {
            return;
        }

        var step = Steps[next];
        if (_stepLabel is not null)
        {
            _stepLabel.Text = $"Ranch Tutorial  {next + 1}/{Steps.Length}";
        }
        if (_titleLabel is not null)
        {
            _titleLabel.Text = step.Title;
        }
        if (_bodyLabel is not null)
        {
            _bodyLabel.Text = step.Body;
        }
        if (_keyLabel is not null)
        {
            _keyLabel.Text = step.KeyHint;
        }
        if (_dismissButton is not null)
        {
            _dismissButton.Text = next == Steps.Length - 1 ? "Got it" : "Next / Skip Step";
        }
    }

    private void AutoCompleteObservedSteps(GameRoot game)
    {
        if (!game.HasSeenTutorial("world_move") && _ranch?.Player is { } player
            && player.GlobalPosition.DistanceTo(_startPosition) >= 1.5f)
        {
            game.MarkTutorialSeen("world_move");
        }

        if (!game.HasSeenTutorial("world_camera") && _cameraUsed)
        {
            game.MarkTutorialSeen("world_camera");
        }

        if (!game.HasSeenTutorial("world_assign") && _stationUsed)
        {
            game.MarkTutorialSeen("world_assign");
        }

        if (!game.HasSeenTutorial("world_management") && _managementOpened)
        {
            game.MarkTutorialSeen("world_management");
        }

        if (!game.HasSeenTutorial("world_town") && _townTravelUsed)
        {
            game.MarkTutorialSeen("world_town");
        }

        if (!game.HasSeenTutorial("world_time") && _timeAdvanced)
        {
            game.MarkTutorialSeen("world_time");
        }
    }

    private void CloseHelpInternal(bool releaseWorldInput)
    {
        if (_helpPanel is null)
        {
            return;
        }

        _helpPanel.Visible = false;
        if (releaseWorldInput && _worldGame?.IsManagementVisible != true)
        {
            _ranch?.InputGate.SetUiOwnsInput(false);
        }
    }
}
