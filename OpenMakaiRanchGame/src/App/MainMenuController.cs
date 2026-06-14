using Godot;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.App;

/// <summary>
/// Standalone main menu controller using scene-authored UI nodes.
/// </summary>
public partial class MainMenuController : Control
{
	[Export]
	public NodePath ContinueButtonPath { get; set; } = "Root/Center/Panel/Content/ContinueButton";

	[Export]
	public NodePath BackgroundPath { get; set; } = "Root/Background";

	[Export]
	public NodePath PanelPath { get; set; } = "Root/Center/Panel";

	[Export]
	public NodePath NewGameButtonPath { get; set; } = "Root/Center/Panel/Content/NewGameButton";

	[Export]
	public NodePath NewGamePlusButtonPath { get; set; } = "Root/Center/Panel/Content/NewGamePlusButton";

	[Export]
	public NodePath QuitButtonPath { get; set; } = "Root/Center/Panel/Content/QuitButton";

	[Export]
	public NodePath LangPickerPath { get; set; } = "Root/Center/Panel/Content/LangRow/LangPicker";

	[Export]
	public NodePath LangLabelPath { get; set; } = "Root/Center/Panel/Content/LangRow/LangLabel";

	private Button _continueButton = null!;
	private Button _newGameButton = null!;
	private Button _newGamePlusButton = null!;
	private Button _quitButton = null!;
	private ColorRect _background = null!;
	private PanelContainer _panel = null!;
	private OptionButton _langPicker = null!;
	private Label _langLabel = null!;

	public override void _Ready()
	{
		if (!TryBindUi())
		{
			GD.PushError("MainMenuController failed to bind required scene nodes.");
			return;
		}

		_continueButton.Pressed += () => HandleStart(loadExisting: true);
		_newGameButton.Pressed += () => HandleStart(loadExisting: false);
		_newGamePlusButton.Pressed += () => HandleStart(loadExisting: false, newGamePlus: true);
		_quitButton.Pressed += () => GetTree().Quit();

		if (GameRoot.Instance is { } game)
		{
			game.StateChanged += RefreshState;
		}

		ApplyTheme();
		SetupLangPicker();
		RefreshState();
	}

	private bool TryBindUi()
	{
		var continueButton = GetNodeOrNull<Button>(ContinueButtonPath);
		var newGameButton = GetNodeOrNull<Button>(NewGameButtonPath);
		var newGamePlusButton = GetNodeOrNull<Button>(NewGamePlusButtonPath);
		var quitButton = GetNodeOrNull<Button>(QuitButtonPath);
		var background = GetNodeOrNull<ColorRect>(BackgroundPath);
		var panel = GetNodeOrNull<PanelContainer>(PanelPath);
		var langPicker = GetNodeOrNull<OptionButton>(LangPickerPath);
		var langLabel = GetNodeOrNull<Label>(LangLabelPath);

		if (continueButton is null || newGameButton is null || newGamePlusButton is null || quitButton is null || background is null || panel is null || langPicker is null || langLabel is null)
		{
			return false;
		}

		_continueButton = continueButton;
		_newGameButton = newGameButton;
		_newGamePlusButton = newGamePlusButton;
		_quitButton = quitButton;
		_background = background;
		_panel = panel;
		_langPicker = langPicker;
		_langLabel = langLabel;
		return true;
	}

	private void SetupLangPicker()
	{
		var game = GameRoot.Instance;
		if (game is null) return;

		_langPicker.Clear();
		foreach (var locale in AvailableLocales)
		{
			_langPicker.AddItem(LocaleDisplayName(locale));
		}

		var currentIdx = System.Array.IndexOf(AvailableLocales, game.State.Settings.Locale);
		if (currentIdx >= 0)
			_langPicker.Selected = currentIdx;

		_langPicker.ItemSelected += idx =>
		{
			var locale = AvailableLocales[(int)idx];
			game.SetLocale(locale);
		};
	}

	private void ApplyTheme()
	{
		if (GameRoot.Instance is not { } game)
		{
			return;
		}

		var palette = game.Theme;
		_background.Color = palette.Canvas;
		_panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
		{
			BgColor = palette.RootPanelFill,
			BorderColor = palette.RootPanelBorder,
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			CornerRadiusTopLeft = 12,
			CornerRadiusTopRight = 12,
			CornerRadiusBottomLeft = 12,
			CornerRadiusBottomRight = 12,
			ContentMarginLeft = 14,
			ContentMarginTop = 12,
			ContentMarginRight = 14,
			ContentMarginBottom = 12
		});

		_langLabel.AddThemeColorOverride("font_color", palette.BodyText);
		_continueButton.AddThemeColorOverride("font_color", palette.HeaderText);
		_newGameButton.AddThemeColorOverride("font_color", palette.HeaderText);
		_newGamePlusButton.AddThemeColorOverride("font_color", palette.HeaderText);
		_quitButton.AddThemeColorOverride("font_color", palette.BodyText);
	}

	private void RefreshState()
	{
		var game = GameRoot.Instance;
		if (game is null) return;

		GetNode<Label>("Root/Center/Panel/Content/TitleLabel").Text = T("mainmenu.title", "Open Makai Ranch");
		GetNode<Label>("Root/Center/Panel/Content/LangRow/LangLabel").Text = T("mainmenu.language", "Language:");

		var canContinue = game.HasSaveSlot(0) || game.HasSaveSlot(1);
		_continueButton.Visible = canContinue;
		_continueButton.Text = T("mainmenu.continue", "Continue");

		_newGameButton.Text = T("mainmenu.new_game", "New Game");

		var hasVictorySave = game.HasVictorySave();
		_newGamePlusButton.Visible = hasVictorySave;
		_newGamePlusButton.Disabled = !hasVictorySave;
		_newGamePlusButton.Text = T("mainmenu.new_game_plus", "New Game+");

		_quitButton.Text = T("mainmenu.quit", "Quit");
	}

	private void HandleStart(bool loadExisting, bool newGamePlus = false)
	{
		if (GameRoot.Instance is not { } game)
		{
			GD.PushError("MainMenu could not resolve GameRoot autoload.");
			return;
		}

		if (loadExisting)
		{
			ContinueFromSlot(game);
			return;
		}

		if (newGamePlus)
		{
			StartNewGamePlusFromMenu(game);
			return;
		}

		StartNewGame(game);
	}

	private void ContinueFromSlot(GameRoot game)
	{
		if (game.LoadSlot(1))
		{
			GoToGameScene();
			return;
		}

		if (game.LoadSlot(0))
		{
			GoToGameScene();
			return;
		}

		GD.PushWarning("Continue requested but no save slots were available.");
		RefreshState();
	}

	private void StartNewGame(GameRoot game)
	{
		game.NewGame();
		GameRoot.PendingInitialScreen = "character_creation";
		GoToGameScene();
	}

	private void StartNewGamePlusFromMenu(GameRoot game)
	{
		if (!game.LoadSlot(1))
		{
			GD.PushWarning("New Game+ failed: could not load save.");
			return;
		}
		game.StartNewGamePlus();
		GoToGameScene();
	}

	private void GoToGameScene()
	{
		var error = GetTree().ChangeSceneToFile("res://scenes/Game.tscn");
		if (error != Error.Ok)
		{
			GD.PushError($"MainMenu failed to open Game scene: {error}");
		}
	}
}
