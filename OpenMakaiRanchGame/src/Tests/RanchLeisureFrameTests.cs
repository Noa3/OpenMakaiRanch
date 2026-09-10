using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

/// <summary>Extends the existing production-backed Day 2 walkthrough. Proximity is explicitly staged.</summary>
public static class RanchLeisureFrameTests
{
    public static async Task Run(GameRoot game, WorldGameController world, SmokeTestResult result)
    {
        var ranch = world.Ranch ?? throw new InvalidOperationException("Ranch missing for leisure walkthrough");
        var leisure = ranch.GetNode<RanchLeisureController>("Leisure");
        var player = ranch.Player!;
        var pause = world.PauseMenu!;
        try
        {
            Check(result, leisure.CornerStation is not null && leisure.BoardStation is not null
                && ranch.Stations.Contains(leisure.CornerStation) && ranch.Stations.Contains(leisure.BoardStation),
                "authored live world registers both personal points in the existing interaction list");
            var assignments = string.Join("|", game.State.Schedule.AssignedJobs.OrderBy(pair => pair.Key));
            var beforeStock = game.State.Ranch.Stockpile["supplies"];
            var beforeGold = game.Economy.Gold;
            var beforeStamina = game.State.Player.Stamina;
            var beforePhase = game.State.Calendar.Phase;
            player.GlobalPosition = leisure.BoardStation!.GlobalPosition;
            await Frames(game, 2);
            await Key(game, Key.F);
            Check(result, pause.IsCommunityBoardOpen && game.GetTree().Paused,
                "F at the physical noticeboard opens the existing paused courier board");
            Check(result, game.Economy.Gold == beforeGold && game.State.Ranch.Stockpile["supplies"] == beforeStock,
                "opening the physical board does not deliver or pay automatically");
            await Key(game, Key.Escape);
            Check(result, pause.IsOpen && !pause.IsCommunityBoardOpen, "physical board keeps existing nested Back-to-pause behavior");
            await Key(game, Key.Escape);
            Check(result, !pause.IsOpen && !game.GetTree().Paused, "second Back returns from the board to the world");

            player.GlobalPosition = leisure.CornerStation!.GlobalPosition;
            await Frames(game, 2);
            Check(result, ranch.GetInteractionPresentation().TargetNode == leisure.CornerStation,
                "quiet corner is selected by the existing nearest-target resolver");
            await Key(game, Key.F);
            Check(result, pause.IsRanchCornerOpen && game.GetTree().Paused, "F opens the quiet-corner surface with one pause owner");
            var panel = pause.GetNode<Control>("RanchCorner");
            var restore = panel.GetNode<Button>("Margin/Layout/Scroll/Content/Restore");
            var rest = panel.GetNode<Button>("Margin/Layout/Scroll/Content/Rest");
            var share = panel.GetNode<Button>("Margin/Layout/Scroll/Content/Share");
            Check(result, !restore.Disabled, "actual Day 2 starting supplies and earned gold cover optional restoration");
            restore.EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, game.IsRanchCornerRestored && leisure.ShowsRestoredCorner,
                "restoration changes the visible greybox bench immediately through shared-state notification");
            Check(result, game.Economy.Gold == beforeGold - RanchLeisureService.RestoreGoldCost
                && game.State.Ranch.Stockpile["supplies"] == beforeStock - RanchLeisureService.RestoreSupplyCost,
                "live restoration consumes exactly the displayed resources without test stock injection");
            Check(result, restore.Disabled && rest.Disabled && share.Disabled,
                "restored/full-stamina/no-companion conditions disable unavailable actions");
            Check(result, game.State.Player.Stamina == beforeStamina && game.State.Calendar.Phase == beforePhase
                && assignments == string.Join("|", game.State.Schedule.AssignedJobs.OrderBy(pair => pair.Key)),
                "personal point changes no assignments, time or stamina during inspection/construction");
            var visualCount = leisure.GetChildCount();
            for (var i = 0; i < 20; i++) leisure.RefreshFromGame();
            Check(result, visualCount == leisure.GetChildCount(), "repeated visual refreshes do not accumulate props or labels");
            await Key(game, Key.Escape);
            Check(result, !pause.IsOpen && !pause.IsRanchCornerOpen && !game.GetTree().Paused,
                "Back closes the corner directly to the world without leaving a hidden pause owner");
            var oldGold = game.Economy.Gold;
            restore.EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, game.Economy.Gold == oldGold, "hidden panel callbacks cannot repeat construction");

            var resident = game.Roster.Characters.First(character => character.Id != "anon");
            Check(result, game.TryConductMentorship(resident.Id, game.StateGeneration),
                "an ordinary existing mentorship spends progression stamina before the break");
            var tiredStamina = game.State.Player.Stamina;
            await Key(game, Key.F);
            Check(result, pause.IsRanchCornerOpen && !rest.Disabled, "reopening exposes recovery after actual progression spending");
            rest.EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, game.State.Player.Stamina == tiredStamina + RanchLeisureService.DailyRecovery && rest.Disabled,
                "live break restores ten stamina and immediately disables repeat recovery");
            rest.EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, game.State.Player.Stamina == tiredStamina + RanchLeisureService.DailyRecovery,
                "repeated button signal cannot bypass the daily receipt");
            panel.GetNode<Button>("Margin/Layout/Scroll/Content/PlanSupplies").EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, !pause.IsOpen && !game.GetTree().Paused && world.IsManagementVisible && world.Shell!.CurrentScreen == "schedule",
                "planning supplies hands ownership to the existing Schedule screen");
            world.CloseManagement();
            await Frames(game, 2);
            Check(result, ranch.InputGate.WorldInputEnabled, "returning from planning restores world controls");

            // Separate composition edge case: a personal point needs no roster worker. Preserve all
            // production characters, then restore them before saving this otherwise ordinary session.
            var residents = game.State.Roster.Characters.ToArray();
            game.State.Roster.Characters.Clear();
            ranch.RefreshLiveWorld();
            try
            {
                await Key(game, Key.F);
                Check(result, pause.IsRanchCornerOpen, "personal station opens even with no available roster worker");
                await Key(game, Key.Escape);
            }
            finally
            {
                game.State.Roster.Characters.AddRange(residents);
                ranch.RefreshLiveWorld();
            }
            Check(result, game.SaveSlot(99), "completed corner transaction writes the isolated current-version save");
            var savedGold = game.Economy.Gold;
            var savedStamina = game.State.Player.Stamina;
            var savedSupplies = game.State.Ranch.Stockpile["supplies"];
            await Key(game, Key.F);
            Check(result, pause.IsRanchCornerOpen, "corner can reopen after roster presentation is restored");
            game.NewGame();
            Check(result, !pause.IsOpen && !game.GetTree().Paused && !leisure.ShowsRestoredCorner,
                "replacing the session closes stale UI and removes the prior improvement's presentation");
            restore.EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, !game.IsRanchCornerRestored, "old UI cannot build in the new session");
            Check(result, game.LoadSlot(99) && leisure.ShowsRestoredCorner,
                "loading restores the existing visual rather than spawning duplicate improvement nodes");
            Check(result, game.Economy.Gold == savedGold && game.State.Player.Stamina == savedStamina
                && game.State.Ranch.Stockpile["supplies"] == savedSupplies && !game.GetRanchCornerStatus().CanRest,
                "gold, supplies, recovery receipt and stamina survive the same root save boundary");
        }
        finally
        {
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.F, PhysicalKeycode = Key.F, Pressed = false });
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, PhysicalKeycode = Key.Escape, Pressed = false });
            Input.FlushBufferedEvents();
            pause.Close();
        }
    }

    private static async Task Frames(GameRoot game, int count)
    {
        for (var i = 0; i < count; i++) await game.ToSignal(game.GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    private static async Task Key(GameRoot game, Key key)
    {
        Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = key, Keycode = key, Pressed = true });
        Input.FlushBufferedEvents();
        await Frames(game, 2);
        Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = key, Keycode = key, Pressed = false });
        Input.FlushBufferedEvents();
        await Frames(game, 2);
    }

    private static void Check(SmokeTestResult result, bool passed, string message)
    {
        result.Passed &= passed;
        result.Lines.Add($"SMOKE {(passed ? "OK" : "FAIL")} leisure frames: {message}");
    }
}
