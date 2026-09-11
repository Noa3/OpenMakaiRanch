using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

/// <summary>Layout-boundary regressions in the existing live HUD/menu fixture; no gameplay mutations.</summary>
public static class HudViewStateFrameTests
{
    public static async Task Run(GameRoot game, WorldGameController world, SmokeTestResult result)
    {
        var shell = world.Shell!;
        var scroll = shell.GetNode<ScrollContainer>(shell.ScrollPath);
        var content = shell.GetNode<Control>(shell.ContentPath);
        shell.ShowScreen("options");
        await Frames(game, 3);
        game.GetViewport().GuiGetFocusOwner()?.ReleaseFocus();
        scroll.ScrollVertical = 250;
        var expectedScroll = scroll.ScrollVertical;
        for (var i = 0; i < 4; i++) game.NotifyStateChanged();
        await Frames(game, 3);
        Check(result, expectedScroll > 0 && Math.Abs(scroll.ScrollVertical - expectedScroll) <= 1,
            "a burst of same-view notifications retains the exact requested scroll position");

        var binding = PlayabilityRegressionTests.Buttons(content).First(button => button.Name == "Keyboard_interact");
        binding.GrabFocus();
        scroll.EnsureControlVisible(binding);
        await Frames(game, 2);
        expectedScroll = scroll.ScrollVertical;
        game.NotifyStateChanged();
        await Frames(game, 3);
        var restored = game.GetViewport().GuiGetFocusOwner();
        Check(result, restored is Button { Disabled: false } && restored.Name == "Keyboard_interact"
            && restored.IsVisibleInTree() && content.IsAncestorOf(restored),
            "a rebuilt options view restores the logical binding control, not a retired node");
        Check(result, Math.Abs(scroll.ScrollVertical - expectedScroll) <= 1,
            "focus restoration does not overwrite the previously visible options position");
        Check(result, scroll.FollowFocus, "normal focus-following remains enabled after restoration");

        // An exact key must respect a deliberate manual scroll away from the focused input.
        scroll.ScrollVertical = 0;
        game.NotifyStateChanged();
        await Frames(game, 3);
        restored = game.GetViewport().GuiGetFocusOwner();
        Check(result, restored?.Name == "Keyboard_interact" && scroll.ScrollVertical == 0,
            "an unchanged focus key preserves deliberate manual scrolling away from the control");

        // Synthetic presentation-only phase-change fixture: the old key disappears during
        // composition, so its index fallback must become visible instead of inheriting zero.
        if (restored is not null) restored.Name = "RemovedCommandFixture";
        game.NotifyStateChanged();
        await Frames(game, 3);
        restored = game.GetViewport().GuiGetFocusOwner();
        Check(result, restored?.Name == "Keyboard_interact" && scroll.ScrollVertical > 0
            && restored.GetGlobalRect().Position.Y >= scroll.GetGlobalRect().Position.Y - 1
            && restored.GetGlobalRect().End.Y <= scroll.GetGlobalRect().End.Y + 1,
            "a removed command's replacement focus is scrolled into view after layout");

        // Queue a restore and leave before it runs. It must not scroll/focus the destination.
        game.NotifyStateChanged();
        shell.ShowScreen("schedule");
        await Frames(game, 3);
        Check(result, shell.CurrentScreen == "schedule" && scroll.ScrollVertical == 0,
            "a pending options restore cannot scroll a newly selected screen");
        Check(result, game.GetViewport().GuiGetFocusOwner()?.Name != "Keyboard_interact",
            "a pending options restore cannot focus a retired binding after routing");

        shell.ShowScreen("options");
        await Frames(game, 3);
        binding = PlayabilityRegressionTests.Buttons(content).First(button => button.Name == "Keyboard_interact");
        binding.GrabFocus();
        game.NotifyStateChanged();
        world.CloseManagement();
        await Frames(game, 3);
        Check(result, !world.IsManagementVisible && world.Ranch!.InputGate.WorldInputEnabled,
            "hiding a pending view leaves the world in control");
        restored = game.GetViewport().GuiGetFocusOwner();
        Check(result, restored is null || !shell.IsAncestorOf(restored),
            "a hidden view cannot reclaim GUI focus on the following frame");
        world.OpenManagementScreen("schedule");
        await Frames(game, 3);
        Check(result, scroll.ScrollVertical == 0,
            "reopening management does not inherit the discarded options restore");

        // Leave the caller in its expected options view; no cached Control references escape.
        shell.ShowScreen("options");
        await Frames(game, 3);
    }

    private static async Task Frames(GameRoot game, int count)
    {
        for (var i = 0; i < count; i++) await game.ToSignal(game.GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    private static void Check(SmokeTestResult result, bool passed, string message)
    {
        result.Passed &= passed;
        result.Lines.Add($"SMOKE {(passed ? "OK" : "FAIL")} HUD view state: {message}");
    }
}
