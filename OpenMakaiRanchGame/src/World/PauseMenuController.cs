using System;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// In-game ESC menu. It pauses the SceneTree and owns only presentation/routing.
/// Save/settings operations are delegated back to the existing management UI.
/// </summary>
public partial class PauseMenuController : Control
{
    private Label? _contextLabel;
    private Button? _resumeButton;
    private Button? _saveLoadButton;
    private Button? _settingsButton;
    private Button? _mainMenuButton;
    private Button? _quitButton;

    public bool IsOpen => Visible;
    public event Action<string>? ManagementScreenRequested;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _contextLabel = GetNodeOrNull<Label>("Center/Panel/Content/ContextLabel");
        _resumeButton = GetNodeOrNull<Button>("Center/Panel/Content/ResumeButton");
        _saveLoadButton = GetNodeOrNull<Button>("Center/Panel/Content/SaveLoadButton");
        _settingsButton = GetNodeOrNull<Button>("Center/Panel/Content/SettingsButton");
        _mainMenuButton = GetNodeOrNull<Button>("Center/Panel/Content/MainMenuButton");
        _quitButton = GetNodeOrNull<Button>("Center/Panel/Content/QuitButton");

        if (_resumeButton is not null) _resumeButton.Pressed += Close;
        if (_saveLoadButton is not null) _saveLoadButton.Pressed += () => OpenManagement("saveload");
        if (_settingsButton is not null) _settingsButton.Pressed += () => OpenManagement("settings");
        if (_mainMenuButton is not null) _mainMenuButton.Pressed += ReturnToMainMenu;
        if (_quitButton is not null) _quitButton.Pressed += QuitGame;

        Visible = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible || !@event.IsActionPressed("ui_cancel"))
        {
            return;
        }

        Close();
        GetViewport().SetInputAsHandled();
    }

    public void Open(string areaId)
    {
        if (Visible)
        {
            return;
        }

        if (_contextLabel is not null && GameRoot.Instance is { } game)
        {
            var area = areaId == "town" ? "Okachi Town" : game.State.Player.RanchName;
            _contextLabel.Text = $"{area}  •  Day {game.State.Calendar.Day}  •  {game.State.Calendar.Phase}";
        }

        Visible = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().Paused = true;
    }

    public void Close()
    {
        if (!Visible && !GetTree().Paused)
        {
            return;
        }

        GetTree().Paused = false;
        Visible = false;
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
        var error = GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
        if (error != Error.Ok)
        {
            GD.PushError($"Pause menu failed to open MainMenu: {error}");
        }
    }

    private void QuitGame()
    {
        Close();
        GetTree().Quit();
    }
}
