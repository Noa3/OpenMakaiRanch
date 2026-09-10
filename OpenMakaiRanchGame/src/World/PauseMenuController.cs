using System;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Ui;

namespace OpenMakaiRanch.World;

/// <summary>
/// In-game pause menu. It pauses the SceneTree and owns only presentation/routing.
/// Save/settings/options operations are delegated back to the existing management UI.
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
    private Button? _communityButton;
    private Control? _menuCenter;
    private CommunityBoardPanel? _communityBoard;

    public bool IsOpen => Visible;
    public bool IsCommunityBoardOpen => Visible && _communityBoard?.Visible == true;
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
        _menuCenter = GetNodeOrNull<Control>("Center");
        EnsureOptionsButton();
        EnsureCommunityBoard();

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
        if (!Visible || @event.IsEcho()
            || (!@event.IsActionPressed("ui_cancel") && !@event.IsActionPressed("pause_menu")))
            return;

        // Back leaves only the innermost panel. Browsing or closing the board never resumes time.
        if (IsCommunityBoardOpen) CloseCommunityBoard();
        else Close();
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

        _communityBoard?.Close();
        if (_menuCenter is not null) _menuCenter.Visible = true;
        Visible = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().Paused = true;
        CallDeferred(nameof(FocusResume));
    }

    public void Close()
    {
        _communityBoard?.Close();
        if (_menuCenter is not null) _menuCenter.Visible = true;
        if (!Visible && !GetTree().Paused) return;
        GetTree().Paused = false;
        Visible = false;
    }

    private void EnsureCommunityBoard()
    {
        var content = GetNodeOrNull<VBoxContainer>("Center/Panel/Content");
        if (content is null || _menuCenter is null) return;

        _communityButton = new Button
        {
            Name = "CommunityBoardButton", Text = "Community Board",
            TooltipText = "Optional courier orders: trade ranch goods for gold. One delivery per day; no penalties for skipping."
        };
        content.AddChild(_communityButton);
        if (_resumeButton is not null)
            content.MoveChild(_communityButton, _resumeButton.GetIndex() + 1);
        _communityButton.Pressed += OpenCommunityBoard;

        _communityBoard = new CommunityBoardPanel { Name = "CommunityBoard" };
        AddChild(_communityBoard);
        _communityBoard.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _communityBoard.OffsetLeft = 24;
        _communityBoard.OffsetTop = 24;
        _communityBoard.OffsetRight = -24;
        _communityBoard.OffsetBottom = -24;
        _communityBoard.BackRequested += CloseCommunityBoard;
        _communityBoard.PlanningRequested += () => OpenManagement("schedule");
    }

    private void OpenCommunityBoard()
    {
        if (!Visible || _communityBoard is null || _menuCenter is null
            || GameRoot.Instance is not { } game || !GodotObject.IsInstanceValid(game)) return;
        _menuCenter.Visible = false;
        _communityBoard.Open(game);
    }

    private void CloseCommunityBoard()
    {
        _communityBoard?.Close();
        if (_menuCenter is not null) _menuCenter.Visible = true;
        if (Visible) _communityButton?.GrabFocus();
    }

    private void EnsureOptionsButton()
    {
        var content = GetNodeOrNull<VBoxContainer>("Center/Panel/Content");
        if (content is null) return;
        _optionsButton = content.GetNodeOrNull<Button>("OptionsButton");
        if (_optionsButton is not null) return;

        _optionsButton = new Button
        {
            Name = "OptionsButton", Text = "Options / Controls",
            TooltipText = "Open graphics, camera, accessibility and remappable controls."
        };
        content.AddChild(_optionsButton);
        if (_settingsButton is not null)
            content.MoveChild(_optionsButton, _settingsButton.GetIndex() + 1);
    }

    private void FocusResume()
    {
        if (Visible && !IsCommunityBoardOpen && _resumeButton is not null
            && GodotObject.IsInstanceValid(_resumeButton))
            _resumeButton.GrabFocus();
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
        if (error != Error.Ok)
            GD.PushError($"Pause menu failed to open MainMenu: {error}");
    }

    private void QuitGame()
    {
        Close();
        GetTree().Quit();
    }
}
