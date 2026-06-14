using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Gameplay;
using static OpenMakaiRanch.Locale.LocaleCatalog;

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
	public NodePath NavigationPath { get; set; } = "Margin/RootPanel/Root/Body/NavPanel/NavScroll/Navigation";

	[Export]
	public NodePath ContentPanelPath { get; set; } = "Margin/RootPanel/Root/Body/ContentPanel";

	[Export]
	public NodePath ScrollPath { get; set; } = "Margin/RootPanel/Root/Body/ContentPanel/Scroll";

	[Export]
	public NodePath ContentPath { get; set; } = "Margin/RootPanel/Root/Body/ContentPanel/Scroll/Content";

	[Export]
	public NodePath CompactNavigationScrollPath { get; set; } = "Margin/RootPanel/Root/CompactNavigationScroll";

	[Export]
	public NodePath CompactNavigationPath { get; set; } = "Margin/RootPanel/Root/CompactNavigationScroll/CompactNavigation";

	[Export]
	public NodePath TitleLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow1/TitleBox/TitleLabel";

	[Export]
	public NodePath ScreenLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow1/TitleBox/ScreenLabel";

	[Export]
	public NodePath StatusLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow1/TitleBox/StatusLabel";

	[Export]
	public NodePath DayLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow1/DayChip/DayLabel";

	[Export]
	public NodePath PhaseLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow1/PhaseChip/PhaseLabel";

	[Export]
	public NodePath WeatherLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow1/WeatherChip/WeatherLabel";

	[Export]
	public NodePath PlayerNameLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow1/PlayerNameChip/PlayerNameLabel";

	[Export]
	public NodePath GoldLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow2/GoldChip/GoldLabel";

	[Export]
	public NodePath HpLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow1/HpChip/HpLabel";

	[Export]
	public NodePath SpiritLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow2/SpiritChip/SpiritLabel";

	[Export]
	public NodePath ManaLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow2/ManaChip/ManaLabel";

	[Export]
	public NodePath HealthLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow2/HealthChip/HealthLabel";

	[Export]
	public NodePath WorkloadLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow1/WorkloadChip/WorkloadLabel";

	[Export]
	public NodePath BathtubLabelPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow2/BathtubChip/BathtubLabel";

	[Export]
	public NodePath EndDayButtonPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow2/EndDayButton";

	[Export]
	public NodePath MenuButtonPath { get; set; } = "Margin/RootPanel/Root/TopBar/TopBarRow1/MenuButton";

	private GameRoot _game = null!;
	private MarginContainer _margin = null!;
	private PanelContainer _rootPanel = null!;
	private VBoxContainer _topBar = null!;
	private HBoxContainer _topBarRow1 = null!;
	private HBoxContainer _topBarRow2 = null!;
	private Label _titleLabel = null!;
	private Label _dayLabel = null!;
	private Label _phaseLabel = null!;
	private Label _weatherLabel = null!;
	private Label _playerNameLabel = null!;
	private Label _goldLabel = null!;
	private Label _hpLabel = null!;
	private Label _spiritLabel = null!;
	private Label _manaLabel = null!;
	private Label _healthLabel = null!;
	private Label _workloadLabel = null!;
	private Label _bathtubLabel = null!;
	private PanelContainer _dayChip = null!;
	private PanelContainer _phaseChip = null!;
	private PanelContainer _weatherChip = null!;
	private PanelContainer _playerNameChip = null!;
	private PanelContainer _goldChip = null!;
	private PanelContainer _hpChip = null!;
	private PanelContainer _spiritChip = null!;
	private PanelContainer _manaChip = null!;
	private PanelContainer _healthChip = null!;
	private PanelContainer _workloadChip = null!;
	private PanelContainer _bathtubChip = null!;
	private Label _screenLabel = null!;
	private Label _statusLabel = null!;
	private Button _endDayButton = null!;
	private Button _menuButton = null!;
	private bool _navCollapsed;
	private HBoxContainer _body = null!;
	private PanelContainer _navPanel = null!;
	private VBoxContainer _navigation = null!;
	private ScrollContainer _compactNavigationScroll = null!;
	private HBoxContainer _compactNavigation = null!;
	private ScrollContainer _scroll = null!;
	private VBoxContainer _content = null!;
	private bool _shellReady;
	private bool _fullScreenMode;
	private string _currentScreen = "title";
	private string _detailCharacterId = string.Empty;
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
		_game.StateChanged += RefreshCurrentScreen;
		_game.GameComplete += OnGameComplete;
		if (!BuildShell())
		{
			_game.StateChanged -= RefreshCurrentScreen;
			_game.GameComplete -= OnGameComplete;
			return;
		}

		router.RegisterShell(this);

		var pending = GameRoot.PendingInitialScreen;
		GameRoot.PendingInitialScreen = null;
		ShowScreen(pending ?? InitialScreen);
	}

	public override void _Notification(int what)
	{
		if (what == NotificationResized && _shellReady)
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
		if (!CanEnterScreen(screenId, out var blockedReason))
		{
			_game.Feedback.PlayError();
			SetStatus(blockedReason, true);
			screenId = "ranch";
		}

		var sameScreenRefresh = _currentScreen == screenId;
		var previousScroll = sameScreenRefresh && IsInstanceValid(_scroll) ? _scroll.ScrollVertical : 0;
		var wasFullScreen = _fullScreenMode;
		var nowFullScreen = screenId is "character_creation" or "prologue" or "victory" or "title";
		_fullScreenMode = nowFullScreen;
		_currentScreen = screenId;

		if (nowFullScreen != wasFullScreen)
		{
			if (nowFullScreen)
			{
				_navPanel.Visible = false;
				_compactNavigationScroll.Visible = false;
				_topBar.Visible = false;
				_margin.OffsetLeft = _margin.OffsetTop = _margin.OffsetRight = _margin.OffsetBottom = 0;
				_rootPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = Colors.Transparent });
			}
			else
			{
				_topBar.Visible = true;
				_rootPanel.AddThemeStyleboxOverride("panel", CardStyle(Palette.RootPanelFill, Palette.RootPanelBorder, 1, 10));
				ApplyResponsiveLayout();
			}
		}

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
			case "character_detail": RenderCharacterDetail(); break;
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
		_topBar = GetNodeOrNull<VBoxContainer>("Margin/RootPanel/Root/TopBar")!;
		_topBarRow1 = GetNodeOrNull<HBoxContainer>("Margin/RootPanel/Root/TopBar/TopBarRow1")!;
		_topBarRow2 = GetNodeOrNull<HBoxContainer>("Margin/RootPanel/Root/TopBar/TopBarRow2")!;
		_navigation = GetNodeOrNull<VBoxContainer>(NavigationPath)!;
		_navPanel = _navigation?.GetParent()?.GetParent() as PanelContainer ?? null!;
		_body = GetNodeOrNull<HBoxContainer>("Margin/RootPanel/Root/Body")!;
		var contentPanel = GetNodeOrNull<PanelContainer>(ContentPanelPath);
		_titleLabel = GetNodeOrNull<Label>(TitleLabelPath)!;
		_screenLabel = GetNodeOrNull<Label>(ScreenLabelPath)!;
		_statusLabel = GetNodeOrNull<Label>(StatusLabelPath)!;
		_dayLabel = GetNodeOrNull<Label>(DayLabelPath)!;
		_phaseLabel = GetNodeOrNull<Label>(PhaseLabelPath)!;
		_weatherLabel = GetNodeOrNull<Label>(WeatherLabelPath)!;
		_playerNameLabel = GetNodeOrNull<Label>(PlayerNameLabelPath)!;
		_goldLabel = GetNodeOrNull<Label>(GoldLabelPath)!;
		_hpLabel = GetNodeOrNull<Label>(HpLabelPath)!;
		_spiritLabel = GetNodeOrNull<Label>(SpiritLabelPath)!;
		_manaLabel = GetNodeOrNull<Label>(ManaLabelPath)!;
		_healthLabel = GetNodeOrNull<Label>(HealthLabelPath)!;
		_workloadLabel = GetNodeOrNull<Label>(WorkloadLabelPath)!;
		_bathtubLabel = GetNodeOrNull<Label>(BathtubLabelPath)!;
		_dayChip = _dayLabel?.GetParent() as PanelContainer ?? null!;
		_phaseChip = _phaseLabel?.GetParent() as PanelContainer ?? null!;
		_weatherChip = _weatherLabel?.GetParent() as PanelContainer ?? null!;
		_playerNameChip = _playerNameLabel?.GetParent() as PanelContainer ?? null!;
		_goldChip = _goldLabel?.GetParent() as PanelContainer ?? null!;
		_hpChip = _hpLabel?.GetParent() as PanelContainer ?? null!;
		_spiritChip = _spiritLabel?.GetParent() as PanelContainer ?? null!;
		_manaChip = _manaLabel?.GetParent() as PanelContainer ?? null!;
		_healthChip = _healthLabel?.GetParent() as PanelContainer ?? null!;
		_workloadChip = _workloadLabel?.GetParent() as PanelContainer ?? null!;
		_bathtubChip = _bathtubLabel?.GetParent() as PanelContainer ?? null!;
		_endDayButton = GetNodeOrNull<Button>(EndDayButtonPath)!;
		_menuButton = GetNodeOrNull<Button>(MenuButtonPath)!;
		_scroll = GetNodeOrNull<ScrollContainer>(ScrollPath)!;
		_content = GetNodeOrNull<VBoxContainer>(ContentPath)!;
		_compactNavigationScroll = GetNodeOrNull<ScrollContainer>(CompactNavigationScrollPath)!;
		_compactNavigation = GetNodeOrNull<HBoxContainer>(CompactNavigationPath)!;

		if (canvas is null || _margin is null || _rootPanel is null || root is null || _topBar is null || _topBarRow1 is null || _topBarRow2 is null
			|| _navigation is null || _navPanel is null || _body is null || contentPanel is null || _titleLabel is null
			|| _screenLabel is null || _statusLabel is null || _dayLabel is null || _phaseLabel is null || _weatherLabel is null || _playerNameLabel is null
			|| _goldLabel is null || _hpLabel is null || _spiritLabel is null || _manaLabel is null || _healthLabel is null || _workloadLabel is null || _bathtubLabel is null
			|| _dayChip is null || _phaseChip is null || _weatherChip is null || _playerNameChip is null
			|| _goldChip is null || _hpChip is null || _spiritChip is null || _manaChip is null || _healthChip is null || _workloadChip is null || _bathtubChip is null
			|| _endDayButton is null || _scroll is null || _content is null || _compactNavigationScroll is null || _compactNavigation is null || _menuButton is null)
		{
			GD.PushError($"UiShellController scene is missing shell node: canvas={canvas is not null} margin={_margin is not null} rootPanel={_rootPanel is not null} root={root is not null} topBar={_topBar is not null} topBarRow1={_topBarRow1 is not null} topBarRow2={_topBarRow2 is not null} nav={_navigation is not null} navPanel={_navPanel is not null} body={_body is not null} contentPanel={contentPanel is not null} titleLabel={_titleLabel is not null} screenLabel={_screenLabel is not null} statusLabel={_statusLabel is not null} dayLabel={_dayLabel is not null} phaseLabel={_phaseLabel is not null} weatherLabel={_weatherLabel is not null} playerNameLabel={_playerNameLabel is not null} goldLabel={_goldLabel is not null} hpLabel={_hpLabel is not null} spiritLabel={_spiritLabel is not null} manaLabels={_manaLabel is not null} healthLabel={_healthLabel is not null} workloadLabel={_workloadLabel is not null} bathtubLabel={_bathtubLabel is not null} dayChip={_dayChip is not null} phaseChip={_phaseChip is not null} weatherChip={_weatherChip is not null} playerNameChip={_playerNameChip is not null} goldChip={_goldChip is not null} hpChip={_hpChip is not null} spiritChip={_spiritChip is not null} manaChip={_manaChip is not null} healthChip={_healthChip is not null} workloadChip={_workloadChip is not null} bathtubChip={_bathtubChip is not null} endDayButton={_endDayButton is not null} menuButton={_menuButton is not null} scroll={_scroll is not null} content={_content is not null} compactNavScroll={_compactNavigationScroll is not null} compactNav={_compactNavigation is not null}");
			return false;
		}

		canvas.MouseFilter = MouseFilterEnum.Ignore;
		canvas.Color = Palette.Canvas;
	_rootPanel.AddThemeStyleboxOverride("panel", CardStyle(Palette.RootPanelFill, Palette.RootPanelBorder, 1, 10));
	_navPanel.AddThemeStyleboxOverride("panel", CardStyle(Palette.NavPanelFill, Palette.NavPanelBorder, 1, 8));
		contentPanel.AddThemeStyleboxOverride("panel", CardStyle(Palette.ContentPanelFill, Palette.ContentPanelBorder, 1, 10));
	_rootPanel.Scale = Vector2.One * _game.State.Settings.UiScale;

	ApplyHeaderLabelStyle(_titleLabel);
		ApplyMutedLabelStyle(_screenLabel);
		ApplyMutedLabelStyle(_statusLabel);
		ApplyChipLabelStyle(_dayLabel);
		ApplyChipLabelStyle(_phaseLabel);
		ApplyChipLabelStyle(_weatherLabel);
		ApplyChipLabelStyle(_playerNameLabel);
		ApplyChipLabelStyle(_goldLabel);
		ApplyChipLabelStyle(_hpLabel);
		ApplyChipLabelStyle(_spiritLabel);
		ApplyChipLabelStyle(_manaLabel);
		ApplyChipLabelStyle(_healthLabel);
		ApplyChipLabelStyle(_workloadLabel);
		ApplyChipLabelStyle(_bathtubLabel);
		ApplyChipPanelStyle(_dayChip);
		ApplyChipPanelStyle(_phaseChip);
		ApplyChipPanelStyle(_weatherChip);
		ApplyChipPanelStyle(_playerNameChip);
		ApplyChipPanelStyle(_goldChip);
		ApplyChipPanelStyle(_hpChip);
		ApplyChipPanelStyle(_spiritChip);
		ApplyChipPanelStyle(_manaChip);
		ApplyChipPanelStyle(_healthChip);
		ApplyChipPanelStyle(_workloadChip);
		ApplyChipPanelStyle(_bathtubChip);
		ApplyPrimaryButtonStyle(_endDayButton);
		_endDayButton.Pressed += () => ExecuteUiAction(() => _game.AdvanceTime(), true);

		if (_menuButton is not null)
		{
			_menuButton.Text = T("common.menu", "Menu");
			_menuButton.Pressed += ToggleNavCollapse;
		}

		ApplySectionStyle(_navigation, "CoreSection");
		ApplySectionStyle(_navigation, "ProgressSection");
		ApplySectionStyle(_navigation, "AdventureSection");
		ApplySectionStyle(_navigation, "SystemSection");

		_navButtons.Clear();
		foreach (var button in _navigation.GetChildren().OfType<Button>())
		{
			if (!BindNavButton(button))
			{
				return false;
			}
		}

		if (!BindCompactNavigation())
		{
			return false;
		}

		_shellReady = true;
		ApplyResponsiveLayout();
		return true;
	}

	private bool BindCompactNavigation()
	{
		_compactNavigationScroll.CustomMinimumSize = new Vector2(0, 44);
		_compactNavigationScroll.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		_compactNavigationScroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Auto;
		_compactNavigationScroll.VerticalScrollMode = ScrollContainer.ScrollMode.Disabled;
		_compactNavigationScroll.Visible = false;
		_compactNavigation.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		_compactNavigation.AddThemeConstantOverride("separation", 6);

		_compactNavButtons.Clear();
		foreach (var button in _compactNavigation.GetChildren().OfType<Button>())
		{
			if (!TryResolveScreenId(button, "CompactButton", out var screenId))
			{
				GD.PushError($"UiShellController could not map compact navigation button '{button.Name}' to a screen id.");
				return false;
			}

			if (string.IsNullOrWhiteSpace(button.TooltipText))
			{
				button.TooltipText = ScreenTitle(screenId);
			}

			ApplySecondaryButtonStyle(button);
			button.CustomMinimumSize = new Vector2(76, 34);
			button.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
			button.Pressed += () =>
			{
				_game.Feedback.PlayNavigate();
				ShowScreen(screenId);
			};
			_compactNavButtons[screenId] = button;
		}

		return true;
	}

	private void ApplyResponsiveLayout()
	{
		var viewportSize = GetViewportRect().Size;
		var compact = _navCollapsed || viewportSize.X <= 900 || viewportSize.Y <= 520;
		var tightWidth = viewportSize.X <= 680;
		var ultraTight = viewportSize.X <= 560;
		var margin = compact ? 8 : 18;

		if (_fullScreenMode)
		{
			_margin.OffsetLeft = _margin.OffsetTop = _margin.OffsetRight = _margin.OffsetBottom = 0;
			_topBar.Visible = false;
			_navPanel.Visible = false;
			_compactNavigationScroll.Visible = false;
		}
		else
		{
			_margin.OffsetLeft = margin;
			_margin.OffsetTop = compact ? 8 : 16;
			_margin.OffsetRight = -margin;
			_margin.OffsetBottom = compact ? -8 : -16;

			_topBar.Visible = true;
			_navPanel.Visible = !compact;
			_compactNavigationScroll.Visible = compact;
		}

		_rootPanel.Scale = Vector2.One * _game.State.Settings.UiScale;
		_body.AddThemeConstantOverride("separation", compact ? 8 : 12);
		_topBar.AddThemeConstantOverride("separation", compact ? 2 : 4);
		_topBarRow1.AddThemeConstantOverride("separation", compact ? 4 : 8);
		_topBarRow2.AddThemeConstantOverride("separation", compact ? 4 : 8);
		_topBar.CustomMinimumSize = new Vector2(0, compact ? 50 : 64);
		_titleLabel.CustomMinimumSize = new Vector2(tightWidth ? 0 : compact ? 100 : 180, 0);
		_titleLabel.Visible = !tightWidth;
		_statusLabel.Visible = !compact || !string.IsNullOrWhiteSpace(_statusLabel.Text);
		_screenLabel.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;

		_dayChip.Visible = true;
		_phaseChip.Visible = !tightWidth;
		_weatherChip.Visible = !tightWidth;
		_playerNameChip.Visible = !ultraTight;

		_goldChip.Visible = true;
		_hpChip.Visible = !ultraTight;
		_spiritChip.Visible = !tightWidth;
		_manaChip.Visible = !tightWidth;
		_healthChip.Visible = !tightWidth;
		_workloadChip.Visible = !ultraTight;
		_bathtubChip.Visible = !tightWidth;

		ApplyChipMinimum(_dayChip, compact);
		ApplyChipMinimum(_phaseChip, compact);
		ApplyChipMinimum(_weatherChip, compact);
		ApplyChipMinimum(_playerNameChip, compact);
		ApplyChipMinimum(_goldChip, compact);
		ApplyChipMinimum(_hpChip, compact);
		ApplyChipMinimum(_spiritChip, compact);
		ApplyChipMinimum(_manaChip, compact);
		ApplyChipMinimum(_healthChip, compact);
		ApplyChipMinimum(_workloadChip, compact);
		ApplyChipMinimum(_bathtubChip, compact);
		_endDayButton.CustomMinimumSize = new Vector2(compact ? 128 : 140, compact ? 26 : 30);
		_menuButton.CustomMinimumSize = new Vector2(compact ? 58 : 64, compact ? 26 : 30);
	}

	private void ToggleNavCollapse()
	{
		_navCollapsed = !_navCollapsed;
		_game.Feedback.PlayConfirm();
		ApplyResponsiveLayout();
	}

	private static void ApplyChipMinimum(PanelContainer? panel, bool compact)
	{
		if (panel is not null)
		{
			panel.CustomMinimumSize = compact ? new Vector2(72, 22) : new Vector2(88, 26);
		}
	}

	private void ApplySectionStyle(Node navigation, string nodeName)
	{
		if (navigation.GetNodeOrNull<Label>(nodeName) is { } label)
		{
			ApplySectionLabelStyle(label);
		}
	}

	private bool BindNavButton(Button button)
	{
		if (!TryResolveScreenId(button, "Button", out var screenId))
		{
			GD.PushError($"UiShellController could not map navigation button '{button.Name}' to a screen id.");
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

	private static bool TryResolveScreenId(Button button, string suffix, out string screenId)
	{
		if (button.HasMeta("screen_id"))
		{
			var meta = button.GetMeta("screen_id").AsString();
			if (!string.IsNullOrWhiteSpace(meta))
			{
				screenId = meta;
				return true;
			}
		}

		var nodeName = button.Name.ToString();
		if (!nodeName.EndsWith(suffix, StringComparison.Ordinal))
		{
			screenId = string.Empty;
			return false;
		}

		var token = nodeName[..^suffix.Length];
		screenId = token switch
		{
			"Overview" => "ranch",
			"Characters" => "roster",
			"SaveLoad" => "saveload",
			_ => token.ToLowerInvariant()
		};

		return !string.IsNullOrWhiteSpace(screenId);
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
		var cal = _game.State.Calendar;
		var eco = _game.State.Economy;
		var ranch = _game.State.Ranch;
		var player = _game.State.Player;

		_dayLabel.Text = $"{cal.Season} / Day {cal.Day}";
		_phaseLabel.Text = cal.Phase.ToString();
		_weatherLabel.Text = WeatherSymbol(cal.CurrentWeather);
		_playerNameLabel.Text = player.Name;

		_goldLabel.Text = $"Gold {eco.Gold}";
		_hpLabel.Text = $"HP {PlayerHp()}";
		_spiritLabel.Text = $"Energy {eco.SpiritEnergy}";
		_manaLabel.Text = $"Mana {eco.ManaReservoir}";
		_healthLabel.Text = $"Health {ranch.CattleHealth}%";
		_workloadLabel.Text = $"Load {ranch.Workload}%";
		_bathtubLabel.Text = ranch.BathtubClean ? "Bath: Clean" : "Bath: Dirty";

		_endDayButton.Text = cal.Phase == DayPhase.Night ? "End Day" : "Advance Phase";
		_screenLabel.Text = ScreenTitle(_currentScreen);
		_endDayButton.Disabled = _currentScreen == "title";
	}

	private string WeatherSymbol(Weather w) => w switch
	{
		Weather.Clear => "☀ Clear",
		Weather.Cloudy => "☁ Cloudy",
		Weather.Rain => "🌧 Rain",
		Weather.Storm => "⛈ Storm",
		_ => "☀ Clear"
	};

	private string PlayerHp()
	{
		if (_game.State.Roster.Characters.Count == 0) return "--";
		var pc = _game.State.Roster.Characters[0];
		return pc.MaxHpOverride.HasValue ? $"{pc.Hp}/{pc.MaxHpOverride.Value}" : $"{pc.Hp}";
	}

	private void UpdateNavigationState()
	{
		var hiddenScreens = new HashSet<string> { "training", "milk", "mental" };
		foreach (var pair in _navButtons)
		{
			UpdateNavigationButton(pair.Value, pair.Key, hiddenScreens.Contains(pair.Key));
		}

		foreach (var pair in _compactNavButtons)
		{
			UpdateNavigationButton(pair.Value, pair.Key, hiddenScreens.Contains(pair.Key));
		}
	}

	private void UpdateNavigationButton(Button button, string screenId, bool hidden)
	{
		button.Visible = !hidden;
		if (hidden)
		{
			button.Disabled = true;
			return;
		}

		if (!button.HasMeta("base_text"))
		{
			button.SetMeta("base_text", button.Text);
		}

		var baseText = button.GetMeta("base_text").AsString();
		var canEnter = CanEnterScreen(screenId, out var requirement);
		button.Text = canEnter ? baseText : $"{baseText} {LockedSuffix()}";
		button.TooltipText = canEnter ? ScreenTitle(screenId) : requirement;
		button.Disabled = screenId == _currentScreen || !canEnter;
	}

	private string LockedSuffix() => T("label.locked_suffix", "(Locked)");

	private bool CanEnterScreen(string screenId, out string requirement)
	{
		requirement = string.Empty;
		return screenId switch
		{
			"combat" => CanEnterCombat(out requirement),
			"research" => HasBuiltFacility("workshop", out requirement),
			"milk" => HasBuiltFacility("dairy_barn", out requirement),
			_ => true
		};
	}

	private bool CanEnterCombat(out string requirement)
	{
		if (_game.LastCombatReport is not null)
		{
			requirement = string.Empty;
			return true;
		}

		requirement = T("screen.combat.locked", "Choose a mission from Adventure before entering Combat.");
		return false;
	}

	private bool HasBuiltFacility(string facilityId, out string requirement)
	{
		var level = FacilityLevel(facilityId);
		if (level > 0)
		{
			requirement = string.Empty;
			return true;
		}

		var name = FacilityName(facilityId);
		requirement = T("screen.facility.locked", "Build {0} in Town before entering.", name);
		return false;
	}

	private int FacilityLevel(string facilityId)
	{
		return _game.Ranch.Facilities.TryGetValue(facilityId, out var level) ? level : 0;
	}

	private string FacilityName(string facilityId)
	{
		return _game.Data.Facilities.TryGetValue(facilityId, out var definition) ? definition.DisplayName : facilityId;
	}

	private void SetStatus(string message, bool isError = false)
	{
		_statusLabel.Text = message;
		_statusLabel.AddThemeColorOverride("font_color", isError ? new Color("ff9f9f") : Palette.MutedText);
		ApplyResponsiveLayout();
	}

	private void ClearContent()
	{
		var children = System.Linq.Enumerable.ToList(_content.GetChildren());
		foreach (var child in children)
		{
			_content.RemoveChild(child);
			child.QueueFree();
		}
	}

}
