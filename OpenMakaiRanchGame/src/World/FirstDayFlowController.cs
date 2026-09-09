using System;
using System.Linq;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.World;

/// <summary>
/// Guided first playable day:
/// bedroom wake-up -> ranch tour -> work/schedule -> evening intruder combat tutorial ->
/// personal conversation -> night routine -> normal daily settlement.
///
/// It uses additive StoryProgressState for resume safety and delegates all real simulation changes to
/// existing GameRoot services.
/// </summary>
public partial class FirstDayFlowController : Control
{
    public const int StageWakeUp = 0;
    public const int StageLeaveBedroom = 1;
    public const int StageRanchWelcome = 2;
    public const int StagePastureAssignment = 3;
    public const int StageManagementDairy = 4;
    public const int StageInvestigateIntruder = 5;
    public const int StageIntruderCombat = 6;
    public const int StagePersonalEvening = 7;
    public const int StageNightRoutine = 8;
    public const int StageCompleted = 9;

    private WorldGameController? _host;
    private GameRoot? _game;
    private IntroHouseController? _intro;
    private RanchGreyboxController? _ranch;

    private PanelContainer? _objectivePanel;
    private Label? _objectiveLabel;
    private PanelContainer? _dialoguePanel;
    private Label? _speakerLabel;
    private Label? _bodyLabel;
    private HBoxContainer? _choiceRow;

    private bool _active;
    private bool _pendingPresentation;
    private bool _initialized;

    public bool IsActive => _active;
    public bool BlocksWorldInput => _active && _dialoguePanel?.Visible == true;
    public bool BlocksManagement => _active && CurrentStage < StageManagementDairy;
    public int CurrentStage => _game?.State.Story.FirstDayStage ?? StageCompleted;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        MouseFilter = MouseFilterEnum.Ignore;
        BuildUi();
        Visible = false;
        CallDeferred(nameof(InitializeDeferred));
    }

    public override void _ExitTree()
    {
        if (_intro is not null && GodotObject.IsInstanceValid(_intro))
        {
            _intro.ExitRequested -= OnIntroExitRequested;
        }
        if (_ranch is not null && GodotObject.IsInstanceValid(_ranch))
        {
            _ranch.StationInteractionSucceeded -= OnStationInteractionSucceeded;
        }
        if (_host?.Shell is { } shell && GodotObject.IsInstanceValid(shell))
        {
            shell.ScreenChanged -= OnScreenChanged;
        }
    }

    public override void _Process(double delta)
    {
        if (!_initialized)
        {
            return;
        }

        if (!_active)
        {
            TryStartOrResume();
            return;
        }

        if (_pendingPresentation && _host?.Transition?.IsTransitioning != true)
        {
            _pendingPresentation = false;
            PresentCurrentStage();
        }

        if (_dialoguePanel?.Visible == true || _host is null || _game is null)
        {
            return;
        }

        if (CurrentStage == StageManagementDairy)
        {
            var hasDairy = _game.Roster.Characters.Any(character =>
                string.Equals(_game.Schedule.GetAssignment(character.Id), "dairy", StringComparison.OrdinalIgnoreCase));

            if (hasDairy && !_host.IsManagementVisible)
            {
                FinishRanchTour();
            }
            else if (hasDairy)
            {
                SetObjective("Dairy Work is covered. Close Management / Return to World to continue the tour.");
            }

            return;
        }

        if (CurrentStage == StageInvestigateIntruder)
        {
            var intruder = GetIntruder();
            var player = _ranch?.Player;
            if (intruder is not null && player is not null
                && player.GlobalPosition.DistanceTo(intruder.GlobalPosition) <= 2.8f)
            {
                SetStage(StageIntruderCombat);
                ShowDialogue(
                    "Caught Intruder",
                    "You catch the thief with stolen ranch supplies in hand. This is a safe tutorial encounter: combat uses the same round-based CombatService as later missions.",
                    ("Review combat basics", ShowCombatBasics),
                    ("Confront her", StartIntruderCombat));
            }
        }
    }

    private void InitializeDeferred()
    {
        _host = GetParent() as WorldGameController
            ?? GetParent()?.GetParent() as WorldGameController;
        _game = GameRoot.Instance;
        _intro = _host?.IntroHouse;
        _ranch = _host?.Ranch;

        if (_host is null || _game is null || _intro is null || _ranch is null || _host.Shell is null)
        {
            GD.PushWarning("FirstDayFlowController could not bind first-day world composition.");
            return;
        }

        _intro.ExitRequested += OnIntroExitRequested;
        _ranch.StationInteractionSucceeded += OnStationInteractionSucceeded;
        _host.Shell.ScreenChanged += OnScreenChanged;
        _initialized = true;
        TryStartOrResume();
    }

    private void TryStartOrResume()
    {
        if (_host is null || _game is null || _host.Shell is null)
        {
            return;
        }

        if (_game.State.NgPlusActive || _game.State.Story.FirstDayCompleted || _game.State.Calendar.Day != 1)
        {
            _active = false;
            Visible = false;
            return;
        }

        // Character creation and the authored prologue still own the screen first.
        if (_host.FlowLocksUi || _host.Shell.CurrentScreen != "ranch")
        {
            return;
        }

        _active = true;
        Visible = true;
        ResumeStage();
    }

    private void ResumeStage()
    {
        if (_host is null || _game is null || _intro is null)
        {
            return;
        }

        var stage = CurrentStage;
        if (stage <= StageLeaveBedroom)
        {
            _host.ActivateStoryArea("intro", reposition: true, firstArrival: false);
            _intro.SetDoorEnabled(stage >= StageLeaveBedroom);
        }
        else
        {
            _host.ActivateStoryArea("ranch", reposition: stage <= StageRanchWelcome, firstArrival: stage == StageRanchWelcome);
            _intro.SetDoorEnabled(false);
        }

        SetIntruderVisible(stage is StageInvestigateIntruder or StageIntruderCombat);
        _pendingPresentation = true;
    }

    private void PresentCurrentStage()
    {
        switch (CurrentStage)
        {
            case StageWakeUp:
                ShowDialogue(
                    GuideName(),
                    $"Morning. {PlayerName()}, wake up. You said you wanted to handle the ranch yourself this time, remember? Get dressed, look around, and I'll show you what needs attention.",
                    ("Wake up", () =>
                    {
                        SetStage(StageLeaveBedroom);
                        HideDialogue();
                        _intro?.FinishWakeUp();
                        SetObjective("Get familiar with movement, then walk to the bedroom door and press F.");
                    }));
                break;

            case StageLeaveBedroom:
                HideDialogue();
                _intro?.SetDoorEnabled(true);
                SetObjective("Walk to the bedroom door and press F to follow your childhood friend outside.");
                break;

            case StageRanchWelcome:
                ShowDialogue(
                    GuideName(),
                    "Welcome outside. The HUD shows the day, resources, warnings and your selected worker. Workstations let you assign jobs physically; Management [M] contains the full schedule, facilities, town, inventory and other systems.",
                    ("Show me the ranch", () =>
                    {
                        SetStage(StagePastureAssignment);
                        HideDialogue();
                        SetObjective("Ranch tour: select a worker and assign someone to Pasture Work at the pasture station.");
                    }));
                break;

            case StagePastureAssignment:
                HideDialogue();
                SetObjective("Ranch tour: assign a resident to Pasture Work using the nearby pasture workstation.");
                break;

            case StageManagementDairy:
                ShowDialogue(
                    GuideName(),
                    "Good. Spatial stations are quick, but the full Schedule is in Management. One important existing settlement rule is Dairy Work: leaving it completely unstaffed adds maintenance cost and hurts morale.",
                    ("Open Schedule", () =>
                    {
                        HideDialogue();
                        _host?.OpenManagementScreen("schedule");
                        SetObjective("Management tutorial: assign at least one resident to Dairy Work, then Return to World.");
                    }));
                break;

            case StageInvestigateIntruder:
                HideDialogue();
                SetIntruderVisible(true);
                SetObjective("Evening: investigate the suspicious figure near the ranch event/storage side.");
                break;

            case StageIntruderCombat:
                ShowDialogue(
                    "Caught Intruder",
                    "The intruder refuses to drop the stolen goods. Combat is round-based: HP keeps combatants standing, Attack/Defense determine damage, Speed influences turn order, and some actions can defend or use skills.",
                    ("Start tutorial fight", StartIntruderCombat));
                break;

            case StagePersonalEvening:
                ShowPersonalEvening();
                break;

            case StageNightRoutine:
                ShowNightRoutine();
                break;

            default:
                CompleteFlowPresentation();
                break;
        }
    }

    private void OnScreenChanged(string screenId)
    {
        if (!_initialized)
        {
            return;
        }

        if (screenId == "ranch")
        {
            TryStartOrResume();
        }
    }

    private void OnIntroExitRequested()
    {
        if (!_active || CurrentStage != StageLeaveBedroom || _host is null)
        {
            return;
        }

        _intro?.SetDoorEnabled(false);
        SetStage(StageRanchWelcome);
        _host.ActivateStoryArea("ranch", reposition: true, firstArrival: true);
        _pendingPresentation = true;
    }

    private void OnStationInteractionSucceeded(string characterId, string jobId)
    {
        if (!_active || CurrentStage != StagePastureAssignment)
        {
            return;
        }

        if (!string.Equals(jobId, "pasture", StringComparison.OrdinalIgnoreCase))
        {
            SetObjective($"That assignment works, but for this tutorial try the Pasture station once. Current station: {jobId}.");
            return;
        }

        SetStage(StageManagementDairy);
        PresentCurrentStage();
    }

    private void FinishRanchTour()
    {
        if (_game is null)
        {
            return;
        }

        _game.State.Story.RanchTourCompleted = true;

        // The guided tour consumes the first working day up to evening, but never settles the day.
        var guard = 0;
        while (_game.State.Calendar.Phase != DayPhase.Evening && guard++ < 3)
        {
            if (_game.State.Calendar.Phase == DayPhase.Night)
            {
                break;
            }
            _game.AdvanceTime();
        }

        SetStage(StageInvestigateIntruder);
        SetIntruderVisible(true);
        ShowDialogue(
            GuideName(),
            "That covers the basics. It's already getting late—wait. Did you hear that? Someone is moving around where they shouldn't be.",
            ("I'll check it", () =>
            {
                HideDialogue();
                SetObjective("Evening: find the suspicious intruder on the ranch and approach her.");
            }));
    }

    private void ShowCombatBasics()
    {
        ShowDialogue(
            "Combat Tutorial",
            "Later battles can involve parties, skills, equipment, mercenaries and capture rules. This first encounter is deliberately small: the existing CombatService resolves the same Attack/Defense/Speed round logic without rewards or capture.",
            ("Fight the intruder", StartIntruderCombat),
            ("Back", () => PresentCurrentStage()));
    }

    private void StartIntruderCombat()
    {
        if (_game is null)
        {
            return;
        }

        HideDialogue();
        SetObjective("Combat tutorial resolving through the normal round-based CombatService...");

        _game.StartNewCombat();
        var report = _game.RunRoundBasedMission("tutorial_ranch_intruder", autoResolve: false);

        var outcomeText = report.Outcome switch
        {
            MissionOutcome.Success => "You win the short struggle and pin the intruder before she can escape.",
            MissionOutcome.PartialSuccess => "It is messy, but you manage to stop the intruder.",
            _ => "The intruder gets the better of the first exchange."
        };

        if (report.Outcome == MissionOutcome.Failure)
        {
            ShowDialogue(
                "Combat Tutorial",
                $"{outcomeText}

The tutorial opponent is intentionally weak; you can retry without changing the story outcome.",
                ("Retry", StartIntruderCombat));
            return;
        }

        var roundSummary = report.Rounds.Count > 0
            ? $"The fight lasted {report.Rounds.Count} round(s). Open the Adventure Guild later for the full mission/combat presentation."
            : "The encounter resolved immediately.";

        ShowDialogue(
            "Combat Tutorial",
            $"{outcomeText}

{roundSummary}",
            ("Detain the intruder", FinishIntruderEncounter));
    }

    private void FinishIntruderEncounter()
    {
        if (_game is null)
        {
            return;
        }

        _game.State.Story.IntruderEncounterCompleted = true;
        SetIntruderVisible(false);

        if (_game.State.Calendar.Phase == DayPhase.Evening)
        {
            _game.AdvanceTime();
        }

        SetStage(StagePersonalEvening);
        ShowDialogue(
            GuideName(),
            "She's secured for the night. That was more excitement than I planned for your first day. Come sit down for a moment before you decide how to finish the evening.",
            ("Sit and talk", ShowPersonalEvening));
    }

    private void ShowPersonalEvening()
    {
        ShowDialogue(
            GuideName(),
            "Personal time is for quieter conversations. The regular game already has Visit/Bond systems for ranch residents; this first-night conversation is story-only so it doesn't invent extra relationship rewards.",
            ("Ask about the ranch", () => ShowPersonalTopic(
                "We've known this place for a long time, but running it is different from growing up around it. Watch the workload and the people, not just the money.")),
            ("Ask about the intruder", () => ShowPersonalTopic(
                "She looked desperate more than dangerous. Tomorrow we can decide what the incident means. Tonight, locking the stores is enough.")),
            ("Talk about tomorrow", () => ShowPersonalTopic(
                "Tomorrow is yours. Assign work, visit town, take missions, spend time with people—or ignore my advice and learn the hard way.")));
    }

    private void ShowPersonalTopic(string text)
    {
        ShowDialogue(
            GuideName(),
            text,
            ("Finish the conversation", FinishPersonalEvening),
            ("Ask something else", ShowPersonalEvening));
    }

    private void FinishPersonalEvening()
    {
        if (_game is null)
        {
            return;
        }

        _game.State.Story.PersonalEveningCompleted = true;
        SetStage(StageNightRoutine);
        ShowNightRoutine();
    }

    private void ShowNightRoutine()
    {
        if (_game is null)
        {
            return;
        }

        ClearChoices();
        ShowDialogueBase(
            GuideName(),
            _game.State.Ranch.BathtubClean
                ? "You're exhausted. You can take a bath before sleeping, go straight to bed, or use the existing night workload choices for training/admin."
                : "You're exhausted. The bath is dirty, so either go straight to bed or choose one of the existing night workload options. Assign Cleaning on a later day to prepare the bath again.");

        AddChoice("Take a bath, then sleep", ChooseBathAndSleep, disabled: !_game.State.Ranch.BathtubClean);
        AddChoice("Go straight to bed", () => ChooseNight("rest"));
        AddChoice("Night training", () => ChooseNight("train"));
        AddChoice("Handle administration", () => ChooseNight("admin"));
    }

    private void ChooseBathAndSleep()
    {
        if (_game is null || !_game.UsePlayerBathForNight())
        {
            ShowNightRoutine();
            return;
        }

        FinishFirstDay("You take a quiet bath, then head to bed.");
    }

    private void ChooseNight(string action)
    {
        _game?.SetNightAction(action);
        FinishFirstDay(action switch
        {
            "train" => "You spend the last hours of the day training before turning in.",
            "admin" => "You finish the day's paperwork before calling it a night.",
            _ => "You decide the first day has been long enough and go straight to bed."
        });
    }

    private void FinishFirstDay(string closingLine)
    {
        if (_game is null || _host is null)
        {
            return;
        }

        _game.State.Story.FirstDayCompleted = true;
        _game.State.Story.FirstDayStage = StageCompleted;
        _game.State.WorldAreaId = "ranch";
        _game.NotifyStateChanged();
        _game.AutosaveCheckpoint("first-day story completed");

        ShowDialogue(
            "End of First Day",
            closingLine,
            ("End Day", () =>
            {
                HideDialogue();
                _active = false;
                Visible = false;
                _host.AdvanceWorldTime();
            }));
    }

    private void SetStage(int stage)
    {
        if (_game is null)
        {
            return;
        }

        _game.State.Story.FirstDayStage = stage;
        _game.NotifyStateChanged();
        _game.AutosaveCheckpoint($"first-day stage {stage}");
    }

    private void SetIntruderVisible(bool visible)
    {
        if (GetIntruder() is { } intruder)
        {
            intruder.Visible = visible;
        }
    }

    private Node3D? GetIntruder() => _ranch?.GetNodeOrNull<Node3D>("FirstDayIntruder");

    private string GuideName()
    {
        if (_game is null || string.IsNullOrWhiteSpace(_game.State.Story.ChildhoodFriendCharacterId))
        {
            return "Childhood Friend";
        }

        var character = _game.Roster.Find(_game.State.Story.ChildhoodFriendCharacterId);
        return character is null
            ? "Childhood Friend"
            : _game.Roster.DefinitionFor(character).DisplayName;
    }

    private string PlayerName() => _game?.State.Player.Name ?? "Rancher";

    private void CompleteFlowPresentation()
    {
        _active = false;
        Visible = false;
        HideDialogue();
        SetIntruderVisible(false);
    }

    private void BuildUi()
    {
        _objectivePanel = new PanelContainer
        {
            Name = "ObjectivePanel",
            Visible = false,
            MouseFilter = MouseFilterEnum.Ignore
        };
        _objectivePanel.SetAnchorsPreset(LayoutPreset.CenterTop);
        _objectivePanel.OffsetLeft = -430;
        _objectivePanel.OffsetTop = 26;
        _objectivePanel.OffsetRight = 430;
        _objectivePanel.OffsetBottom = 88;
        AddChild(_objectivePanel);

        _objectiveLabel = new Label
        {
            Name = "ObjectiveLabel",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _objectivePanel.AddChild(_objectiveLabel);

        _dialoguePanel = new PanelContainer
        {
            Name = "DialoguePanel",
            Visible = false,
            MouseFilter = MouseFilterEnum.Stop
        };
        _dialoguePanel.SetAnchorsPreset(LayoutPreset.CenterBottom);
        _dialoguePanel.OffsetLeft = -520;
        _dialoguePanel.OffsetTop = -285;
        _dialoguePanel.OffsetRight = 520;
        _dialoguePanel.OffsetBottom = -38;
        AddChild(_dialoguePanel);

        var content = new VBoxContainer
        {
            Name = "Content"
        };
        content.AddThemeConstantOverride("separation", 10);
        _dialoguePanel.AddChild(content);

        _speakerLabel = new Label
        {
            Name = "SpeakerLabel",
            Text = "Childhood Friend"
        };
        _speakerLabel.AddThemeFontSizeOverride("font_size", 22);
        content.AddChild(_speakerLabel);

        _bodyLabel = new Label
        {
            Name = "BodyLabel",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _bodyLabel.AddThemeFontSizeOverride("font_size", 17);
        content.AddChild(_bodyLabel);

        _choiceRow = new HBoxContainer
        {
            Name = "Choices",
            Alignment = BoxContainer.AlignmentMode.End
        };
        _choiceRow.AddThemeConstantOverride("separation", 8);
        content.AddChild(_choiceRow);
    }

    private void ShowDialogue(string speaker, string body, params (string Label, Action Action)[] choices)
    {
        ShowDialogueBase(speaker, body);
        ClearChoices();
        foreach (var choice in choices)
        {
            AddChoice(choice.Label, choice.Action);
        }
    }

    private void ShowDialogueBase(string speaker, string body)
    {
        Visible = true;
        if (_speakerLabel is not null) _speakerLabel.Text = speaker;
        if (_bodyLabel is not null) _bodyLabel.Text = body;
        if (_dialoguePanel is not null) _dialoguePanel.Visible = true;
        SetStoryInputLocked(true);
    }

    private void HideDialogue()
    {
        if (_dialoguePanel is not null) _dialoguePanel.Visible = false;
        SetStoryInputLocked(false);
    }

    private void SetObjective(string text)
    {
        Visible = true;
        if (_objectivePanel is not null) _objectivePanel.Visible = !string.IsNullOrWhiteSpace(text);
        if (_objectiveLabel is not null) _objectiveLabel.Text = text;
    }

    private void ClearChoices()
    {
        if (_choiceRow is null)
        {
            return;
        }

        foreach (var child in _choiceRow.GetChildren())
        {
            _choiceRow.RemoveChild(child);
            child.QueueFree();
        }
    }

    private void AddChoice(string label, Action action, bool disabled = false)
    {
        if (_choiceRow is null)
        {
            return;
        }

        var button = new Button
        {
            Text = label,
            Disabled = disabled,
            FocusMode = FocusModeEnum.All
        };
        button.Pressed += action;
        _choiceRow.AddChild(button);
    }

    private void SetStoryInputLocked(bool locked)
    {
        if (_host is null)
        {
            return;
        }

        if (_host.ActiveAreaId == "intro")
        {
            _intro?.InputGate.SetUiOwnsInput(locked);
        }
        else
        {
            _ranch?.InputGate.SetUiOwnsInput(locked || _host.IsManagementVisible);
        }

        _host.MobileControls?.SetBlocked(locked || _host.IsManagementVisible);
    }
}
