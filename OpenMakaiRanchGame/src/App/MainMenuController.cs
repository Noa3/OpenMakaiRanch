using Godot;

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
    public NodePath QuitButtonPath { get; set; } = "Root/Center/Panel/Content/QuitButton";

    [Export]
    public NodePath StatusLabelPath { get; set; } = "Root/Center/Panel/Content/StatusLabel";

    private Button _continueButton = null!;
    private Button _newGameButton = null!;
    private Button _quitButton = null!;
    private Label _statusLabel = null!;
    private ColorRect _background = null!;
    private PanelContainer _panel = null!;

    public override void _Ready()
    {
        if (!TryBindUi())
        {
            GD.PushError("MainMenuController failed to bind required scene nodes.");
            return;
        }

        _continueButton.Pressed += () => HandleStart(loadExisting: true);
        _newGameButton.Pressed += () => HandleStart(loadExisting: false);
        _quitButton.Pressed += () => GetTree().Quit();

        ApplyTheme();
        RefreshState();
    }

    private bool TryBindUi()
    {
        var continueButton = GetNodeOrNull<Button>(ContinueButtonPath);
        var newGameButton = GetNodeOrNull<Button>(NewGameButtonPath);
        var quitButton = GetNodeOrNull<Button>(QuitButtonPath);
        var statusLabel = GetNodeOrNull<Label>(StatusLabelPath);
        var background = GetNodeOrNull<ColorRect>(BackgroundPath);
        var panel = GetNodeOrNull<PanelContainer>(PanelPath);

        if (continueButton is null || newGameButton is null || quitButton is null || statusLabel is null || background is null || panel is null)
        {
            return false;
        }

        _continueButton = continueButton;
        _newGameButton = newGameButton;
        _quitButton = quitButton;
        _statusLabel = statusLabel;
        _background = background;
        _panel = panel;
        return true;
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

        _statusLabel.AddThemeColorOverride("font_color", palette.MutedText);
        _continueButton.AddThemeColorOverride("font_color", palette.HeaderText);
        _newGameButton.AddThemeColorOverride("font_color", palette.HeaderText);
        _quitButton.AddThemeColorOverride("font_color", palette.BodyText);
    }

    private void RefreshState()
    {
        var canContinue = GameRoot.Instance is not null && GameRoot.Instance.HasSaveSlot(1);
        _continueButton.Disabled = !canContinue;
        _continueButton.Text = canContinue ? "Continue Slot 1" : "No Save In Slot 1";
        _statusLabel.Text = canContinue ? "Ready." : "No save found in slot 1. Start a new game.";
    }

    private void HandleStart(bool loadExisting)
    {
        if (GameRoot.Instance is not { } game)
        {
            GD.PushError("MainMenu could not resolve GameRoot autoload.");
            _statusLabel.Text = "GameRoot autoload missing.";
            return;
        }

        if (loadExisting)
        {
            ContinueFromSlot(game);
            return;
        }

        StartNewGame(game);
    }

    private void ContinueFromSlot(GameRoot game)
    {
        if (!game.LoadSlot(1))
        {
            GD.PushWarning("Continue requested but save slot 1 was unavailable.");
            _statusLabel.Text = "Continue failed: slot 1 unavailable.";
            RefreshState();
            return;
        }

        GoToGameScene();
    }

    private void StartNewGame(GameRoot game)
    {
        game.NewGame();
        GoToGameScene();
    }

    private void GoToGameScene()
    {
        var error = GetTree().ChangeSceneToFile("res://scenes/Game.tscn");
        if (error != Error.Ok)
        {
            GD.PushError($"MainMenu failed to open Game scene: {error}");
            _statusLabel.Text = $"Could not open game scene: {error}";
        }
    }
}
