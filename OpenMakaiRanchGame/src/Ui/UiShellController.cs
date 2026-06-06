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
    private Label _dayLabel = null!;
    private Label _phaseLabel = null!;
    private Label _goldLabel = null!;
    private Label _screenLabel = null!;
    private Label _statusLabel = null!;
    private Button _endDayButton = null!;
    private ScrollContainer _scroll = null!;
    private VBoxContainer _content = null!;
    private string _currentScreen = "title";
    private readonly Dictionary<string, Button> _navButtons = new();

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
        if (!BuildShell())
        {
            return;
        }

        ShowScreen(InitialScreen);
    }

    public override void _ExitTree()
    {
        if (GodotObject.IsInstanceValid(_game))
        {
            _game.StateChanged -= RefreshCurrentScreen;
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
        var rootPanel = GetNodeOrNull<PanelContainer>(RootPanelPath);
        var navigation = GetNodeOrNull<VBoxContainer>(NavigationPath);
        var navPanel = navigation?.GetParent() as PanelContainer;
        var contentPanel = GetNodeOrNull<PanelContainer>(ContentPanelPath);
        var titleLabel = GetNodeOrNull<Label>(TitleLabelPath);
        _screenLabel = GetNodeOrNull<Label>(ScreenLabelPath)!;
        _statusLabel = GetNodeOrNull<Label>(StatusLabelPath)!;
        _dayLabel = GetNodeOrNull<Label>(DayLabelPath)!;
        _phaseLabel = GetNodeOrNull<Label>(PhaseLabelPath)!;
        _goldLabel = GetNodeOrNull<Label>(GoldLabelPath)!;
        _endDayButton = GetNodeOrNull<Button>(EndDayButtonPath)!;
        _scroll = GetNodeOrNull<ScrollContainer>(ScrollPath)!;
        _content = GetNodeOrNull<VBoxContainer>(ContentPath)!;

        if (canvas is null || rootPanel is null || navigation is null || navPanel is null || contentPanel is null || titleLabel is null
            || _screenLabel is null || _statusLabel is null || _dayLabel is null || _phaseLabel is null || _goldLabel is null
            || _endDayButton is null || _scroll is null || _content is null)
        {
            GD.PushError("UiShellController scene is missing required shell nodes.");
            return false;
        }

        canvas.MouseFilter = MouseFilterEnum.Ignore;
        canvas.Color = Palette.Canvas;
        rootPanel.AddThemeStyleboxOverride("panel", CardStyle(Palette.RootPanelFill, Palette.RootPanelBorder, 2, 16));
        navPanel.AddThemeStyleboxOverride("panel", CardStyle(Palette.NavPanelFill, Palette.NavPanelBorder, 1, 12));
        contentPanel.AddThemeStyleboxOverride("panel", CardStyle(Palette.ContentPanelFill, Palette.ContentPanelBorder, 1, 14));
        rootPanel.Scale = new Vector2(_game.State.Settings.UiScale, _game.State.Settings.UiScale);

        ApplyHeaderLabelStyle(titleLabel);
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

        ApplySectionStyle(navigation, "CoreSection");
        ApplySectionStyle(navigation, "ProgressSection");
        ApplySectionStyle(navigation, "SystemSection");

        _navButtons.Clear();
        return BindNavButton(navigation, "ranch", "OverviewButton")
            && BindNavButton(navigation, "roster", "CharactersButton")
            && BindNavButton(navigation, "schedule", "ScheduleButton")
            && BindNavButton(navigation, "town", "TownButton")
            && BindNavButton(navigation, "shop", "ShopButton")
            && BindNavButton(navigation, "adventure", "AdventureButton")
            && BindNavButton(navigation, "combat", "CombatButton")
            && BindNavButton(navigation, "milestones", "MilestonesButton")
            && BindNavButton(navigation, "research", "ResearchButton")
            && BindNavButton(navigation, "bond", "BondButton")
            && BindNavButton(navigation, "pets", "PetsButton")
            && BindNavButton(navigation, "saveload", "SaveLoadButton")
            && BindNavButton(navigation, "settings", "SettingsButton");
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
    }

    private void SetStatus(string message, bool isError = false)
    {
        _statusLabel.Text = message;
        _statusLabel.AddThemeColorOverride("font_color", isError ? new Color("ff9f9f") : Palette.MutedText);
    }

    private void ClearContent()
    {
        foreach (var child in _content.GetChildren())
        {
            child.QueueFree();
        }
    }

}
