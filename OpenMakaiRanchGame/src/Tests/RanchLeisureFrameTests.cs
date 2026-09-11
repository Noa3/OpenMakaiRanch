using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

/// <summary>
/// Extends the existing Day 2 walkthrough through ordinary supply production and Day 3 restoration.
/// Proximity is explicitly staged; no stock, money, day or stamina is injected into this progression.
/// </summary>
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
            await ProduceRestorationSupplies(game, world, leisure, result);
            var assignments = string.Join("|", game.State.Schedule.AssignedJobs.OrderBy(pair => pair.Key));
            var beforeStock = game.State.Ranch.Stockpile["supplies"];
            var beforeGold = game.Economy.Gold;
            var beforeStamina = game.State.Player.Stamina;
            var beforePhase = game.State.Calendar.Phase;
            player.GlobalPosition = leisure.BoardStation!.GlobalPosition;
            await Frames(game, 2);
            await KeyStroke(game, Key.F);
            Check(result, pause.IsCommunityBoardOpen && game.GetTree().Paused,
                "F at the physical noticeboard opens the existing paused courier board");
            Check(result, game.Economy.Gold == beforeGold && game.State.Ranch.Stockpile["supplies"] == beforeStock,
                "opening the physical board does not deliver or pay automatically");
            await KeyStroke(game, Key.Escape);
            Check(result, !pause.IsOpen && !pause.IsCommunityBoardOpen && !game.GetTree().Paused,
                "physical board returns directly to the world with no remote pause hub");

            player.GlobalPosition = leisure.CornerStation!.GlobalPosition;
            await Frames(game, 2);
            Check(result, ranch.GetInteractionPresentation().TargetNode == leisure.CornerStation,
                "quiet corner is selected by the existing nearest-target resolver");
            await KeyStroke(game, Key.F);
            Check(result, pause.IsRanchCornerOpen && game.GetTree().Paused, "F opens the quiet-corner surface with one pause owner");
            var panel = pause.GetNode<Control>("RanchCorner");
            var restore = panel.GetNode<Button>("Margin/Layout/Scroll/Content/Restore");
            var rest = panel.GetNode<Button>("Margin/Layout/Scroll/Content/Rest");
            var share = panel.GetNode<Button>("Margin/Layout/Scroll/Content/Share");
            Check(result, !restore.Disabled, "ordinary supply production and earned gold cover optional restoration");
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
            await KeyStroke(game, Key.Escape);
            Check(result, !pause.IsOpen && !pause.IsRanchCornerOpen && !game.GetTree().Paused,
                "Back closes the corner directly to the world without leaving a hidden pause owner");
            var oldGold = game.Economy.Gold;
            restore.EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, game.Economy.Gold == oldGold, "hidden panel callbacks cannot repeat construction");

            var resident = game.Roster.Characters.First(character => character.Id != "anon");
            Check(result, game.TryConductMentorship(resident.Id, game.StateGeneration),
                "an ordinary existing mentorship spends progression stamina before the break");
            var tiredStamina = game.State.Player.Stamina;
            await KeyStroke(game, Key.F);
            Check(result, pause.IsRanchCornerOpen && !rest.Disabled, "reopening exposes recovery after actual progression spending");
            rest.EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, game.State.Player.Stamina == tiredStamina + RanchLeisureService.DailyRecovery && rest.Disabled,
                "live break restores ten stamina and immediately disables repeat recovery");
            rest.EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, game.State.Player.Stamina == tiredStamina + RanchLeisureService.DailyRecovery,
                "repeated button signal cannot bypass the daily receipt");
            panel.GetNode<Button>("Margin/Layout/Scroll/Content/PlanSupplies").EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, !pause.IsOpen && !game.GetTree().Paused && world.IsStationPanelOpen
                && world.StationPanel!.ContextKind == "guide" && !world.Shell!.IsVisibleInTree(),
                "finding supplies opens the place guide without a remote work or pause surface");
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
                await KeyStroke(game, Key.F);
                Check(result, pause.IsRanchCornerOpen, "personal station opens even with no available roster worker");
                await KeyStroke(game, Key.Escape);
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
            await KeyStroke(game, Key.F);
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

    private static async Task ProduceRestorationSupplies(GameRoot game, WorldGameController world,
        RanchLeisureController leisure, SmokeTestResult result)
    {
        var ranch = world.Ranch!;
        var pause = world.PauseMenu!;
        // Day 1 consumes starting supplies for existing facility maintenance. The UI must explain
        // the resulting shortage and lead to real production, not assume those supplies survived.
        ranch.Player!.GlobalPosition = leisure.CornerStation!.GlobalPosition;
        await Frames(game, 2);
        await KeyStroke(game, Key.F);
        var panel = pause.GetNode<Control>("RanchCorner");
        var status = game.GetRanchCornerStatus();
        Check(result, game.State.Calendar.Day == 2 && !status.CanRestore
            && status.Supplies < RanchLeisureService.RestoreSupplyCost
            && status.RestoreReason.Contains("supplies", StringComparison.OrdinalIgnoreCase),
            "after real facility upkeep the corner explains the missing supplies instead of taking partial payment");
        panel.GetNode<Button>("Margin/Layout/Scroll/Content/PlanSupplies").EmitSignal(BaseButton.SignalName.Pressed);
        await Frames(game, 2);
        Check(result, world.IsStationPanelOpen && world.StationPanel!.ContextKind == "guide" && !game.GetTree().Paused,
            "the shortage's planning route opens the place guide without a leftover pause");
        var beforePlanningGold = game.Economy.Gold;
        var beforePlanningStock = game.State.Ranch.Stockpile["supplies"];
        PlayabilityRegressionTests.Buttons(world.StationPanel!).Single(button => button.Name == "Place_office")
            .EmitSignal(BaseButton.SignalName.Pressed);
        await Frames(game, 2);
        Check(result, !world.IsManagementVisible && world.NavigationGuide!.Target?.TargetId == "office"
            && game.Economy.Gold == beforePlanningGold && game.State.Ranch.Stockpile["supplies"] == beforePlanningStock,
            "the actual Office destination marks a place without teleporting, spending or paying production");
        var office = ranch.Stations.Single(station => station.TargetId == "office");
        ranch.Player!.GlobalPosition = office.GlobalPosition;
        await Frames(game, 3);
        await KeyStroke(game, Key.F);
        Check(result, world.IsStationPanelOpen && world.StationPanel!.ContextId == "office",
            "F at the physical Office opens its own local assignment surface");
        var officeLabel = game.Data.Jobs["office"].DisplayName;
        var worker = game.Roster.Characters.First(character => game.Schedule.GetAssignment(character.Id) != "dairy");
        var officeButton = PlayabilityRegressionTests.Buttons(world.StationPanel!)
            .Single(button => button.Name == "Assign_" + worker.Id);
        if (officeButton.Disabled) throw new InvalidOperationException("Supply-producing Office Work assignment is unavailable");
        officeButton.EmitSignal(BaseButton.SignalName.Pressed);
        await Frames(game, 3);
        Check(result, game.Schedule.GetAssignment(worker.Id) == "office" && game.Economy.Gold == beforePlanningGold
            && game.State.Ranch.Stockpile["supplies"] == beforePlanningStock,
            "a local Office assignment changes the shared schedule without paying production before settlement");
        PlayabilityRegressionTests.Buttons(world.StationPanel!).Single(button => button.Name == "StationClose")
            .EmitSignal(BaseButton.SignalName.Pressed);
        await Frames(game, 2);
        var dayBefore = game.State.Calendar.Day;
        var advance = ranch.GetNode<Button>("WorldHud/AdvanceTimeButton");
        for (var phase = 0; phase < 4 && game.State.Calendar.Day == dayBefore; phase++)
        {
            if (!advance.IsVisibleInTree() || advance.Disabled)
                throw new InvalidOperationException("Normal world phase advance is unavailable");
            advance.EmitSignal(BaseButton.SignalName.Pressed);
            await Frames(game, 2);
        }
        Check(result, game.State.Calendar.Day == dayBefore && !world.IsManagementVisible
            && string.IsNullOrWhiteSpace(game.State.Calendar.NightAction)
            && world.NavigationGuide!.Target?.TargetId == "ranch_house",
            "Plan Night marks the house and requires an explicit local workload choice before settlement");
        var house = ranch.Stations.Single(station => station.TargetId == "ranch_house");
        ranch.Player!.GlobalPosition = house.GlobalPosition;
        await Frames(game, 3);
        await KeyStroke(game, Key.F);
        Check(result, world.IsStationPanelOpen && world.StationPanel!.ContextId == "ranch_house",
            "the house opens through the same physical interaction used by the player");
        PlayabilityRegressionTests.Buttons(world.StationPanel!).Single(button => button.Name == "HousePlan_rest")
            .EmitSignal(BaseButton.SignalName.Pressed);
        await Frames(game, 3);
        Check(result, game.State.Calendar.NightAction == "rest", "the local night-choice button selects ordinary rest");
        var endDay = PlayabilityRegressionTests.Buttons(world.StationPanel!).Single(button => button.Name == "HouseSleep");
        if (!endDay.IsVisibleInTree() || endDay.Disabled)
            throw new InvalidOperationException("Sleep at the house is unavailable after planning the night");
        endDay.EmitSignal(BaseButton.SignalName.Pressed);
        await Frames(game, 3);
        Check(result, game.State.Calendar.Day == dayBefore + 1 && game.LastDailyReport?.Day == dayBefore
            && world.Shell.CurrentScreen == "report", "normal phase advancement reaches the Day 3 settlement report once");
        Check(result, game.State.Ranch.Stockpile["supplies"] >= RanchLeisureService.RestoreSupplyCost
            && game.LastDailyReport!.Lines.Any(line => line.Contains(officeLabel, StringComparison.Ordinal)),
            "the actual Office Work report accounts for the supplies used by restoration");
        world.GetNode<Button>("ManagementLayer/ManagementUi/UiShell/Margin/RootPanel/Root/TopBar/TopBarRow1/ReturnToWorldButton")
            .EmitSignal(BaseButton.SignalName.Pressed);
        await Frames(game, 2);
    }

    private static async Task Frames(GameRoot game, int count)
    {
        for (var i = 0; i < count; i++) await game.ToSignal(game.GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    private static async Task KeyStroke(GameRoot game, Key key)
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
