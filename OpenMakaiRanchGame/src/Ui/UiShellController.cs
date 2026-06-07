using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.Ui;

public partial class UiShellController : Control
{
    private static readonly (string ScreenId, string NodeName, string CompactText)[] NavigationItems =
    {
        ("ranch", "OverviewButton", "Overview"),
        ("roster", "CharactersButton", "Roster"),
        ("schedule", "ScheduleButton", "Schedule"),
        ("town", "TownButton", "Town"),
        ("shop", "ShopButton", "Shop"),
        ("adventure", "AdventureButton", "Adventure"),
        ("combat", "CombatButton", "Combat"),
        ("milestones", "MilestonesButton", "Goals"),
        ("research", "ResearchButton", "Research"),
        ("bond", "BondButton", "Bond"),
        ("pets", "PetsButton", "Pets"),
        ("training", "TrainingButton", "Training"),
        ("milk", "MilkButton", "Milk"),
        ("mental", "MentalButton", "Mental"),
        ("saveload", "SaveLoadButton", "Save"),
        ("settings", "SettingsButton", "Settings")
    };

    [Export]
    public string InitialScreen { get; set; } = "title";

    [ExportGroup("Shell Nodes")]
    [Export]
    public NodePath CanvasPath { get; set; } = "Canvas";

    [Export]
    public NodePath RootPanelPath { get; set; } = "Margin/RootPanel";

    [Export]
    public NodePath NavigationPath { get; set; } = "Margin/RootPanel/Root/Body/NavPanel/Navigation";

    [Export]
    public NodePath ContentPanelPath { get; set; } = "Margin/RootPanel/Root/Body/ContentPanel";

    [Export]
    public NodePath ScrollPath { get; set; } = "Margin/RootPanel/Root/Body/ContentPanel/Scroll";

    [Export]
    public NodePath ContentPath { get; set; } = "Margin/RootPanel/Root/Body/ContentPanel/Scroll/Content";

    [Export]
    public NodePath TitleLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TitleBox/TitleLabel";

    [Export]
    public NodePath ScreenLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TitleBox/ScreenLabel";

    [Export]
    public NodePath StatusLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TitleBox/StatusLabel";

    [Export]
    public NodePath DayLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/DayChip/DayLabel";

    [Export]
    public NodePath PhaseLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/PhaseChip/PhaseLabel";

    [Export]
    public NodePath GoldLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/GoldChip/GoldLabel";

    [Export]
    public NodePath EndDayButtonPath { get; set; } = "Margin/RootPanel/Root/TopBar/EndDayButton";

    private GameRoot _game = null!;
    private MarginContainer _margin = null!;
    private PanelContainer _rootPanel = null!;
    private HBoxContainer _topBar = null!;
    private Label _titleLabel = null!;
    private Label _dayLabel = null!;
    private Label _phaseLabel = null!;
    private Label _goldLabel = null!;
    private Label _screenLabel = null!;
    private Label _statusLabel = null!;
    private Button _endDayButton = null!;
    private HBoxContainer _body = null!;
    private PanelContainer _navPanel = null!;
    private VBoxContainer _navigation = null!;
    private ScrollContainer _compactNavigationScroll = null!;
    private HBoxContainer _compactNavigation = null!;
    private ScrollContainer _scroll = null!;
    private VBoxContainer _content = null!;
    private string _currentScreen = "title";
    private readonly Dictionary<string, Button> _navButtons = new();
    private readonly Dictionary<string, Button> _compactNavButtons = new();

    public override void _Ready()
    {
        if (GameRoot.Instance is not { } game || !GodotObject.IsInstanceValid(game))
        {
            GD.PushError("UiShellController could not find a valid GameRoot instance.");
            return;
        }

        if (SceneRouter.Instance is not { } router || !GodotObject.IsInstanceValid(router))
        {
            GD.PushError("UiShellController could not find a valid SceneRouter instance.");
            return;
        }

        _game = game;
        router.RegisterShell(this);
        _game.StateChanged += RefreshCurrentScreen;
        _game.GameComplete += OnGameComplete;
        if (!BuildShell())
        {
            return;
        }

        ShowScreen(InitialScreen);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized && IsInstanceValid(_rootPanel))
        {
            ApplyResponsiveLayout();
        }
    }

    public override void _ExitTree()
    {
        if (GodotObject.IsInstanceValid(_game))
        {
            _game.StateChanged -= RefreshCurrentScreen;
            _game.GameComplete -= OnGameComplete;
        }

        if (SceneRouter.Instance is { } router && GodotObject.IsInstanceValid(router))
        {
            router.UnregisterShell(this);
        }
    }

    public void ShowScreen(string screenId)
    {
        var sameScreenRefresh = _currentScreen == screenId;
        var previousScroll = sameScreenRefresh && IsInstanceValid(_scroll) ? _scroll.ScrollVertical : 0;

        _currentScreen = screenId;
        ClearContent();
        UpdateTopBar();
        UpdateNavigationState();

        switch (screenId)
        {
            case "title": RenderTitle(); break;
            case "ranch": RenderRanch(); break;
            case "roster": RenderRoster(); break;
            case "schedule": RenderSchedule(); break;
            case "town": RenderTown(); break;
            case "shop": RenderShop(); break;
            case "adventure": RenderAdventure(); break;
            case "combat": RenderCombat(); break;
            case "milestones": RenderMilestones(); break;
            case "research": RenderResearch(); break;
            case "bond": RenderBond(); break;
            case "pets": RenderPets(); break;
            case "saveload": RenderSaveLoad(); break;
            case "settings": RenderSettings(); break;
            case "training": RenderTraining(); break;
            case "milk": RenderMilkEconomy(); break;
            case "mental": RenderMentalState(); break;
            case "character_creation": RenderCharacterCreation(); break;
            case "prologue": RenderPrologue(); break;
            case "victory": RenderVictory(); break;
            default: RenderRanch(); break;
        }

        if (sameScreenRefresh && IsInstanceValid(_scroll))
        {
            _scroll.ScrollVertical = previousScroll;
        }
    }

    private bool BuildShell()
    {
        AnchorRight = 1;
        AnchorBottom = 1;

        var canvas = GetNodeOrNull<ColorRect>(CanvasPath);
        _margin = GetNodeOrNull<MarginContainer>("Margin")!;
        _rootPanel = GetNodeOrNull<PanelContainer>(RootPanelPath)!;
        var root = _rootPanel?.GetNodeOrNull<VBoxContainer>("Root");
        _topBar = GetNodeOrNull<HBoxContainer>("Margin/RootPanel/Root/TopBar")!;
        _navigation = GetNodeOrNull<VBoxContainer>(NavigationPath)!;
        _navPanel = _navigation?.GetParent() as PanelContainer ?? null!;
        _body = GetNodeOrNull<HBoxContainer>("Margin/RootPanel/Root/Body")!;
        var contentPanel = GetNodeOrNull<PanelContainer>(ContentPanelPath);
        _titleLabel = GetNodeOrNull<Label>(TitleLabelPath)!;
        _screenLabel = GetNodeOrNull<Label>(ScreenLabelPath)!;
        _statusLabel = GetNodeOrNull<Label>(StatusLabelPath)!;
        _dayLabel = GetNodeOrNull<Label>(DayLabelPath)!;
        _phaseLabel = GetNodeOrNull<Label>(PhaseLabelPath)!;
        _goldLabel = GetNodeOrNull<Label>(GoldLabelPath)!;
        _endDayButton = GetNodeOrNull<Button>(EndDayButtonPath)!;
        _scroll = GetNodeOrNull<ScrollContainer>(ScrollPath)!;
        _content = GetNodeOrNull<VBoxContainer>(ContentPath)!;

        if (canvas is null || _margin is null || _rootPanel is null || root is null || _topBar is null || _navigation is null || _navPanel is null || _body is null || contentPanel is null || _titleLabel is null
            || _screenLabel is null || _statusLabel is null || _dayLabel is null || _phaseLabel is null || _goldLabel is null
            || _endDayButton is null || _scroll is null || _content is null)
        {
            GD.PushError("UiShellController scene is missing required shell nodes.");
            return false;
        }

        canvas.MouseFilter = MouseFilterEnum.Ignore;
        canvas.Color = Palette.Canvas;
    _rootPanel.AddThemeStyleboxOverride("panel", CardStyle(Palette.RootPanelFill, Palette.RootPanelBorder, 2, 16));
    _navPanel.AddThemeStyleboxOverride("panel", CardStyle(Palette.NavPanelFill, Palette.NavPanelBorder, 1, 12));
        contentPanel.AddThemeStyleboxOverride("panel", CardStyle(Palette.ContentPanelFill, Palette.ContentPanelBorder, 1, 14));
    _rootPanel.Scale = Vector2.One;

    ApplyHeaderLabelStyle(_titleLabel);
        ApplyMutedLabelStyle(_screenLabel);
        ApplyMutedLabelStyle(_statusLabel);
        ApplyChipLabelStyle(_dayLabel);
        ApplyChipLabelStyle(_phaseLabel);
        ApplyChipLabelStyle(_goldLabel);
        ApplyChipPanelStyle(_dayLabel.GetParent() as PanelContainer);
        ApplyChipPanelStyle(_phaseLabel.GetParent() as PanelContainer);
        ApplyChipPanelStyle(_goldLabel.GetParent() as PanelContainer);
        ApplyPrimaryButtonStyle(_endDayButton);
        _endDayButton.Pressed += () => ExecuteUiAction(() => _game.AdvanceTime(), true);

        ApplySectionStyle(_navigation, "CoreSection");
        ApplySectionStyle(_navigation, "ProgressSection");
        ApplySectionStyle(_navigation, "NsfwSection");
        ApplySectionStyle(_navigation, "SystemSection");

        _navButtons.Clear();
        foreach (var item in NavigationItems)
        {
            if (!BindNavButton(_navigation, item.ScreenId, item.NodeName))
            {
                return false;
            }
        }

        BuildCompactNavigation(root);
        ApplyResponsiveLayout();
        return true;
    }

    private void BuildCompactNavigation(VBoxContainer root)
    {
        _compactNavigationScroll = new ScrollContainer
        {
            CustomMinimumSize = new Vector2(0, 44),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Auto,
            VerticalScrollMode = ScrollContainer.ScrollMode.Disabled,
            Visible = false
        };
        _compactNavigation = new HBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _compactNavigation.AddThemeConstantOverride("separation", 6);
        _compactNavigationScroll.AddChild(_compactNavigation);
        root.AddChild(_compactNavigationScroll);
        root.MoveChild(_compactNavigationScroll, 1);

        _compactNavButtons.Clear();
        foreach (var item in NavigationItems)
        {
            var button = SecondaryButton(item.CompactText, ScreenTitle(item.ScreenId));
            button.CustomMinimumSize = new Vector2(76, 34);
            button.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            button.Pressed += () =>
            {
                _game.Feedback.PlayNavigate();
                ShowScreen(item.ScreenId);
            };
            _compactNavigation.AddChild(button);
            _compactNavButtons[item.ScreenId] = button;
        }
    }

    private void ApplyResponsiveLayout()
    {
        var viewportSize = GetViewportRect().Size;
        var compact = viewportSize.X <= 900 || viewportSize.Y <= 520;
        var tightWidth = viewportSize.X <= 680;
        var margin = compact ? 8 : 18;
        _margin.OffsetLeft = margin;
        _margin.OffsetTop = compact ? 8 : 16;
        _margin.OffsetRight = -margin;
        _margin.OffsetBottom = compact ? -8 : -16;

        _rootPanel.Scale = Vector2.One;
        _navPanel.Visible = !compact;
        _compactNavigationScroll.Visible = compact;
        _body.AddThemeConstantOverride("separation", compact ? 8 : 12);
        _topBar.AddThemeConstantOverride("separation", compact ? 6 : 10);
        _topBar.CustomMinimumSize = new Vector2(0, compact ? 48 : 58);
        _titleLabel.CustomMinimumSize = new Vector2(tightWidth ? 0 : compact ? 120 : 220, 0);
        _titleLabel.Visible = !tightWidth;
        _statusLabel.Visible = !compact || !string.IsNullOrWhiteSpace(_statusLabel.Text);
        _screenLabel.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;

        ApplyChipMinimum(_dayLabel.GetParent() as PanelContainer, compact);
        ApplyChipMinimum(_phaseLabel.GetParent() as PanelContainer, compact);
        ApplyChipMinimum(_goldLabel.GetParent() as PanelContainer, compact);
        _endDayButton.CustomMinimumSize = new Vector2(compact ? 96 : 118, compact ? 34 : 38);
    }

    private static void ApplyChipMinimum(PanelContainer? panel, bool compact)
    {
        if (panel is not null)
        {
            panel.CustomMinimumSize = compact ? new Vector2(78, 30) : new Vector2(104, 32);
        }
    }

    private void ApplySectionStyle(Node navigation, string nodeName)
    {
        if (navigation.GetNodeOrNull<Label>(nodeName) is { } label)
        {
            ApplySectionLabelStyle(label);
        }
    }

    private bool BindNavButton(Node navigation, string screenId, string nodeName)
    {
        var button = navigation.GetNodeOrNull<Button>(nodeName);
        if (button is null)
        {
            GD.PushError($"UiShellController scene is missing navigation button '{nodeName}'.");
            return false;
        }

        ApplySecondaryButtonStyle(button);
        button.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        button.Pressed += () =>
        {
            _game.Feedback.PlayNavigate();
            ShowScreen(screenId);
        };
        _navButtons[screenId] = button;
        return true;
    }

    private void ExecuteUiAction(Func<bool> action, bool stateEventExpected, string? goToScreen = null)
    {
        var previousScreen = _currentScreen;
        if (goToScreen is not null && stateEventExpected)
        {
            _currentScreen = goToScreen;
        }

        if (!action())
        {
            _currentScreen = previousScreen;
            _game.Feedback.PlayError();
            SetStatus("Action failed. Check save slot/state and try again.", true);
            RefreshCurrentScreen();
            return;
        }

        _game.Feedback.PlayConfirm();
        SetStatus(string.Empty);
        if (goToScreen is not null)
        {
            if (!stateEventExpected)
            {
                ShowScreen(goToScreen);
            }

            return;
        }

        if (!stateEventExpected)
        {
            RefreshCurrentScreen();
        }
    }

    private void ExecuteUiAction(Action action, bool stateEventExpected, string? goToScreen = null)
    {
        var previousScreen = _currentScreen;
        if (goToScreen is not null && stateEventExpected)
        {
            _currentScreen = goToScreen;
        }

        try
        {
            action();
        }
        catch (Exception exception)
        {
            GD.PushError($"UiShellController action failed: {exception}");
            _currentScreen = previousScreen;
            _game.Feedback.PlayError();
            SetStatus($"Action failed: {exception.Message}", true);
            RefreshCurrentScreen();
            return;
        }

        _game.Feedback.PlayConfirm();
        SetStatus(string.Empty);
        if (goToScreen is not null)
        {
            if (!stateEventExpected)
            {
                ShowScreen(goToScreen);
            }

            return;
        }

        if (!stateEventExpected)
        {
            RefreshCurrentScreen();
        }
    }

    private void RefreshCurrentScreen()
    {
        ShowScreen(_currentScreen);
    }

    private void OnGameComplete()
    {
        ShowScreen("victory");
    }

    private void UpdateTopBar()
    {
        _dayLabel.Text = $"Day {_game.State.Calendar.Day}";
        _phaseLabel.Text = _game.State.Calendar.Phase.ToString();
        _goldLabel.Text = $"Gold {_game.State.Economy.Gold}";
        _endDayButton.Text = _game.State.Calendar.Phase == DayPhase.Night ? "End Day" : "Advance Phase";
        _screenLabel.Text = ScreenTitle(_currentScreen);
        _endDayButton.Disabled = _currentScreen == "title";
    }

    private void UpdateNavigationState()
    {
        foreach (var pair in _navButtons)
        {
            pair.Value.Disabled = pair.Key == _currentScreen;
        }

        foreach (var pair in _compactNavButtons)
        {
            pair.Value.Disabled = pair.Key == _currentScreen;
        }
    }

    private void SetStatus(string message, bool isError = false)
    {
        _statusLabel.Text = message;
        _statusLabel.AddThemeColorOverride("font_color", isError ? new Color("ff9f9f") : Palette.MutedText);
        ApplyResponsiveLayout();
    }

    private void ClearContent()
    {
        foreach (var child in _content.GetChildren())
        {
            child.QueueFree();
        }
    }

}
