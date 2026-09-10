using System;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// In-game pause menu. Owns presentation/routing only; save/settings/options stay in management UI.
/// </summary>
public partial class PauseMenuController : Control
{
    private Label? _contextLabel;
    private Button? _resumeButton;
    private Button? _saveLoadButton;
    private Button? _settingsButton;
    private Button? _optionsButton;
    private Button? _mainMenuButton;
    private Button? _quitButton;

    public bool IsOpen => Visible;
    public event Action<string>? ManagementScreenRequested;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        InputBindingService.EnsureApplied();

        _contextLabel = GetNodeOrNull<Label>("Center/Panel/Content/ContextLabel");
        _resumeButton = GetNodeOrNull<Button>("Center/Panel/Content/ResumeButton");
        _saveLoadButton = GetNodeOrNull<Button>("Center/Panel/Content/SaveLoadButton");
        _settingsButton = GetNodeOrNull<Button>("Center/Panel/Content/SettingsButton");
        _mainMenuButton = GetNodeOrNull<Button>("Center/Panel/Content/MainMenuButton");
        _quitButton = GetNodeOrNull<Button>("Center/Panel/Content/QuitButton");
        EnsureOptionsButton();

        if (_resumeButton is not null) _resumeButton.Pressed += Close;
        if (_saveLoadButton is not null) _saveLoadButton.Pressed += () => OpenManagement("saveload");
        if (_settingsButton is not null) _settingsButton.Pressed += () => OpenManagement("settings");
        if (_optionsButton is not null) _optionsButton.Pressed += () => OpenManagement("options");
        if (_mainMenuButton is not null) _mainMenuButton.Pressed += ReturnToMainMenu;
        if (_quitButton is not null) _quitButton.Pressed += QuitGame;
        Visible = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible || (!@event.IsActionPressed("ui_cancel") && !@event.IsActionPressed("pause_menu"))) return;
        Close();
        GetViewport().SetInputAsHandled();
    }

    public void Open(string areaId)
    {
        if (Visible) return;
        if (_contextLabel is not null && GameRoot.Instance is { } game)
        {
            var area = areaId == "town" ? "Okachi Town" : game.State.Player.RanchName;
            _contextLabel.Text = $"{area}  •  Day {game.State.Calendar.Day}  •  {game.State.Calendar.Phase}";
        }
        if (_resumeButton is not null)
            _resumeButton.TooltipText = $"Return to the game [{InputBindingService.GetCombinedLabel("pause_menu")} / Back]";

        Visible = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().Paused = true;
        CallDeferred(nameof(FocusResume));
    }

    public void Close()
    {
        if (!Visible && !GetTree().Paused) return;
        GetTree().Paused = false;
        Visible = false;
    }

    private void EnsureOptionsButton()
    {
        var content = GetNodeOrNull<VBoxContainer>("Center/Panel/Content");
        if (content is null) return;
        _optionsButton = content.GetNodeOrNull<Button>("OptionsButton");
        if (_optionsButton is not null) return;
        _optionsButton = new Button
        {
            Name = "OptionsButton",
            Text = "Options / Controls",
            TooltipText = "Open graphics, camera, accessibility and remappable controls."
        };
        content.AddChild(_optionsButton);
        if (_settingsButton is not null) content.MoveChild(_optionsButton, _settingsButton.GetIndex() + 1);
    }

    private void FocusResume()
    {
        if (Visible && _resumeButton is not null && GodotObject.IsInstanceValid(_resumeButton)) _resumeButton.GrabFocus();
    }

    private void OpenManagement(string screenId)
    {
        Close();
        ManagementScreenRequested?.Invoke(screenId);
    }

    private void ReturnToMainMenu()
    {
        Close();
        GameRoot.PendingInitialScreen = null;
        var error = GetTree().ChangeSceneToFile(GameRouteCatalog.MainMenu);
        if (error != Error.Ok) GD.PushError($"Pause menu failed to open MainMenu: {error}");
    }

    private void QuitGame()
    {
        Close();
        GetTree().Quit();
    }
}
