using System;
using System.Reflection;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

// Bind only pause ownership, not heavy world composition. Explicit event callbacks remain real.
public partial class PauseInputHostFixture : WorldGameController
{
    public override void _Ready() { }
}

public static class PauseInputRegressionTests
{
    public static void Run(SmokeTestResult result)
    {
        // This synchronous suite is invoked deferred and may run inside a physics tick.
        // ActionPress's just-pressed timestamp is therefore not a valid simulated process frame.
        // Check the no-polling contract explicitly, then exercise actual event-routing methods.
        var source = Godot.FileAccess.GetFileAsString("res://src/World/WorldGameController.cs");
        Check(result, !string.IsNullOrWhiteSpace(source), "world input source is available for the ownership guard");
        Check(result, !source.Contains("Input.IsActionJustPressed(\"ui_cancel\")", StringComparison.Ordinal),
            "source guard: world updates do not poll Back after the event owner has handled it");

        var tree = GameRoot.Instance.GetTree();
        var wasPaused = tree.Paused;
        var pause = new PauseMenuController { Name = "PauseInputFixture" };
        var center = new CenterContainer { Name = "Center" };
        var panel = new PanelContainer { Name = "Panel" };
        var content = new VBoxContainer { Name = "Content" };
        pause.AddChild(center);
        center.AddChild(panel);
        panel.AddChild(content);
        content.AddChild(new Button { Name = "ResumeButton", Text = "Resume" });
        var host = new PauseInputHostFixture { ProcessMode = Node.ProcessModeEnum.Disabled };
        try
        {
            tree.Root.AddChild(pause);
            tree.Root.AddChild(host);
            var field = typeof(WorldGameController).GetField("_pauseMenu", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("World pause binding was not found.");
            field.SetValue(host, pause);

            foreach (var action in new[] { "pause_menu", "ui_cancel" })
            {
                pause.Open("ranch");
                content.GetNode<Button>("CommunityBoardButton").EmitSignal(BaseButton.SignalName.Pressed);
                Check(result, pause.IsCommunityBoardOpen && tree.Paused, $"{action}: board is open before host routing");
                host._UnhandledInput(new InputEventAction { Action = action, Pressed = true });
                Check(result, pause.IsOpen && !pause.IsCommunityBoardOpen && tree.Paused,
                    $"{action}: host Back closes only the board and keeps the world paused");
                host._UnhandledInput(new InputEventAction { Action = action, Pressed = true });
                Check(result, !pause.IsOpen && !tree.Paused, $"{action}: the next Back closes pause");
                pause.Close();
            }
        }
        finally
        {
            host.Free();
            pause.Close();
            pause.Free();
            tree.Paused = wasPaused;
        }
    }

    private static void Check(SmokeTestResult result, bool condition, string description)
    {
        result.Passed &= condition;
        result.Lines.Add($"SMOKE {(condition ? "OK" : "FAIL")} pause input: {description}");
    }
}
