using Godot;
using OpenMakaiRanch.Ui;

namespace OpenMakaiRanch.App;

/// <summary>
/// Gameplay scene host. Keeps scene-level concerns separate from the main menu and bootstrap.
/// </summary>
public partial class GameSceneController : Control
{
    [Export]
    public NodePath UiShellPath { get; set; } = "UiShell";

    public override void _Ready()
    {
        if (GetNodeOrNull<UiShellController>(UiShellPath) is not { })
        {
            GD.PushError($"Game scene could not find UiShell at '{UiShellPath}'.");
        }
    }
}
