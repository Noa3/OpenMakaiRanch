using System;
using System.Reflection;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

public static class PauseInputRegressionTests
{
    public static void Run(SmokeTestResult result)
    {
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
        var host = new WorldGameController();
        try
        {
            tree.Root.AddChild(pause);
            // Isolate the input-owning host from heavy world composition. No production test hook
            // is added, and the actual _Process implementation is exercised unchanged.
            var field = typeof(WorldGameController).GetField("_pauseMenu", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("World pause binding was not found.");
            field.SetValue(host, pause);
            pause.Open("ranch");
            Input.ActionPress("ui_cancel");
            Check(result, Input.IsActionJustPressed("ui_cancel"), "Back remains just-pressed during the current process frame");
            pause._UnhandledInput(new InputEventAction { Action = "ui_cancel", Pressed = true });
            Check(result, !pause.IsOpen && !tree.Paused, "Back closes the pause menu before world processing resumes");
            host._Process(0.0);
            Check(result, !pause.IsOpen && !tree.Paused,
                "the same Back press cannot reopen pause in the following world _Process call");
        }
        finally
        {
            Input.ActionRelease("ui_cancel");
            pause.Close();
            pause.Free();
            host.Free();
            tree.Paused = wasPaused;
        }
    }

    private static void Check(SmokeTestResult result, bool condition, string description)
    {
        result.Passed &= condition;
        result.Lines.Add($"SMOKE {(condition ? "OK" : "FAIL")} pause input: {description}");
    }
}
