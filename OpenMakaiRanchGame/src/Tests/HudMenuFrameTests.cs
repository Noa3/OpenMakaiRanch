using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.Ui;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

/// <summary>
/// Live UI integration, using explicitly synthetic Day-2 state and injected input. This is not
/// progression funding, rendered hit-target acceptance, or a physical-controller certification.
/// </summary>
public static class HudMenuFrameTests
{
    public static async Task Run(GameRoot game, SmokeTestResult result)
    {
        var settings = game.State.Settings.Clone();
        var tree = game.GetTree();
        await Frames(game, 3);
        var prior = tree.CurrentScene;
        var priorMode = prior?.ProcessMode ?? Node.ProcessModeEnum.Inherit;
        var priorVisible = prior is CanvasItem canvas && canvas.Visible;
        WorldGameController? world = null;
        try
        {
            if (prior is not null)
            {
                prior.ProcessMode = Node.ProcessModeEnum.Disabled;
                if (prior is CanvasItem item) item.Hide();
            }
            game.NewGame();
            game.State.Calendar.Day = 2;
            game.State.Story.FirstDayCompleted = true;
            game.State.Story.FirstDayStage = FirstDayFlowController.StageCompleted;
            game.State.Settings.ReducedMotion = true;
            world = PlayabilityRegressionTests.CreateWorld(game);
            await Frames(game, 4);
            await CheckHudOwnership(game, world, result);
            await CheckMenus(game, world, result);
            await CheckCombat(game, world, result);
        }
        catch (Exception exception)
        {
            Check(result, false, $"integration harness stopped: {exception}");
        }
        finally
        {
            foreach (var key in new[] { Key.Tab, Key.Escape, Key.T }) Release(key);
            tree.Paused = false;
            if (GodotObject.IsInstanceValid(world)) world!.Free();
            game.EndCombatSession();
            game.State.Settings = settings;
            game.NewGame();
            GameRoot.PendingInitialScreen = null;
            if (GodotObject.IsInstanceValid(prior))
            {
                prior!.ProcessMode = priorMode;
                if (prior is CanvasItem item) item.Visible = priorVisible;
            }
            await Frames(game, 2);
        }
    }

    private static async Task CheckHudOwnership(GameRoot game, WorldGameController world, SmokeTestResult r)
    {
        var ranch = world.Ranch!;
        var town = world.Town!;
        Check(r, ranch.Hud!.Visible && !town.Hud!.Visible, "only ranch HUD is visible in ranch");
        world.OpenManagementScreen("schedule");
        Check(r, !ranch.Hud.Visible && !town.Hud!.Visible, "management hides both area HUD layers");
        var phase = game.State.Calendar.Phase;
        ranch.Hud.GetNode<Button>("AdvanceTimeButton").EmitSignal(BaseButton.SignalName.Pressed);
        Check(r, phase == game.State.Calendar.Phase, "a hidden world advance callback cannot advance behind management");
        world.CloseManagement();
        Check(r, ranch.Hud.Visible && !town.Hud!.Visible, "closing management restores only active HUD");
        Check(r, world.TravelTo("town"), "ordinary ranch-to-town travel remains available");
        world.Transition?.CompleteImmediately();
        Check(r, !ranch.Hud.Visible && town.Hud!.Visible, "only town HUD is visible after travel");
        phase = game.State.Calendar.Phase;
        ranch.Hud.GetNode<Button>("AdvanceTimeButton").EmitSignal(BaseButton.SignalName.Pressed);
        Check(r, phase == game.State.Calendar.Phase, "inactive ranch HUD cannot advance the town session");
        ranch.Hud.GetNode<Button>("ManagementButton").EmitSignal(BaseButton.SignalName.Pressed);
        Check(r, !world.IsManagementVisible, "inactive ranch HUD cannot open town management");
        world.CloseManagement();

        var help = town.GetNode<TownTutorialController>("TownHud/TutorialOverlay");
        help.ToggleHelp();
        Check(r, help.HelpVisible && !town.InputGate.WorldInputEnabled, "town help owns input");
        await Stroke(game, Key.Escape);
        Check(r, !help.HelpVisible && world.PauseMenu?.IsOpen != true && town.InputGate.WorldInputEnabled,
            "Back closes help instead of opening pause above it");
        world.PauseMenu!.Close();
        help.CloseHelp();
        help.ToggleHelp();
        world.OpenManagementScreen("shop");
        Check(r, !help.HelpVisible && world.IsManagementVisible && !town.InputGate.WorldInputEnabled,
            "opening management closes help while retaining the management input lock");
        help.ToggleHelp();
        Check(r, !help.HelpVisible, "help cannot open behind management");
        town.Hud!.GetNode<Button>("ReturnRanchButton").EmitSignal(BaseButton.SignalName.Pressed);
        Check(r, world.ActiveAreaId == "town", "hidden town-return button cannot travel behind management");
        help.CloseHelp();
        world.CloseManagement();
        Check(r, world.TravelTo("ranch"), "return from town remains available after help and management");
        world.Transition?.CompleteImmediately();
        await Frames(game, 2);
        world.PauseMenu.Open("ranch");
        phase = game.State.Calendar.Phase;
        world.AdvanceWorldTime();
        Check(r, phase == game.State.Calendar.Phase, "world command rejects phase advance while paused");
        Check(r, !world.TravelTo("town"), "travel is rejected while pause owns the interface");
        world.PauseMenu.Close();
        if (world.ActiveAreaId != "ranch") world.TravelTo("ranch");
        world.Transition?.CompleteImmediately();
    }

    private static async Task CheckMenus(GameRoot game, WorldGameController world, SmokeTestResult r)
    {
        var shell = world.Shell!;
        world.OpenManagementScreen("ranch");
        var gold = game.Economy.Gold;
        var stamina = game.State.Player.Stamina;
        var day = game.State.Calendar.Day;
        foreach (var route in new[] { "ranch", "report", "roster", "schedule", "town", "shop", "adventure",
            "milestones", "bond", "pets", "saveload", "settings", "options", "room_assign", "ability" })
        {
            try
            {
                shell.ShowScreen(route);
                await Frames(game, 2);
                Check(r, shell.CurrentScreen == route && shell.GetNode<Control>(shell.ContentPath).GetChildCount() > 0,
                    $"ordinary {route} screen has live content");
            }
            catch (Exception exception) { Check(r, false, $"{route} screen: {exception.Message}"); }
        }
        Check(r, game.Economy.Gold == gold && game.State.Player.Stamina == stamina && game.State.Calendar.Day == day,
            "browsing ordinary menus does not pay, consume stamina or settle days");
        shell.ShowScreen("research");
        Check(r, shell.CurrentScreen != "research", "research access still requires the existing workshop");
        var residents = game.State.Roster.Characters.ToArray();
        game.State.Player.Hp = 73;
        game.State.Player.MaxHp = 150;
        try
        {
            game.State.Roster.Characters.Clear();
            shell.ShowScreen("schedule");
            var hp = shell.GetNode<Label>(shell.HpLabelPath);
            Check(r, hp.Text == "HP 73/150", "empty roster shows canonical player HP immediately without indexing a resident");
        }
        catch (Exception exception) { Check(r, false, $"empty-roster top bar: {exception.Message}"); }
        finally { game.State.Roster.Characters.AddRange(residents); }
        shell.ShowScreen("schedule");
        Check(r, shell.GetNode<Label>(shell.HpLabelPath).Text == "HP 73/150", "player HP is correct before the next process frame");

        shell.ShowScreen("options");
        await Frames(game, 3);
        var content = shell.GetNode<Control>(shell.ContentPath);
        var scroll = shell.GetNode<ScrollContainer>(shell.ScrollPath);
        GetFocus(game)?.ReleaseFocus();
        scroll.ScrollVertical = 250;
        var beforeScroll = scroll.ScrollVertical;
        game.NotifyStateChanged();
        await Frames(game, 3);
        Check(r, beforeScroll > 0 && scroll.ScrollVertical > 0, "same-screen state refresh preserves a scrolled options view");
        Check(r, scroll.FollowFocus, "management scrolling follows keyboard and controller focus");
        // Force compact navigation without requiring a particular display backend/window size.
        shell.GetNode<Button>(shell.MenuButtonPath).EmitSignal(BaseButton.SignalName.Pressed);
        GetFocus(game)?.ReleaseFocus();
        await Frames(game, 3);
        Check(r, GetFocus(game) is { } focus && focus.IsVisibleInTree() && shell.IsAncestorOf(focus),
            "compact menu obtains an enabled visible keyboard focus target");

        var binding = PlayabilityRegressionTests.Buttons(content).First(button => button.Name == "Keyboard_interact");
        var originalBinding = InputBindingService.GetKeyboardLabel("interact");
        binding.EmitSignal(BaseButton.SignalName.Pressed);
        await game.ToSignal(game.GetTree().CreateTimer(0.22), SceneTreeTimer.SignalName.Timeout);
        shell.ShowScreen("schedule");
        // An old queued press must not arm a capture belonging to a discarded screen.
        binding.EmitSignal(BaseButton.SignalName.Pressed);
        await Frames(game, 20);
        world.CloseManagement();
        shell._Input(new InputEventKey { Keycode = Key.T, PhysicalKeycode = Key.T, Pressed = true });
        Check(r, originalBinding == InputBindingService.GetKeyboardLabel("interact"),
            "retired or hidden options controls cannot capture the next gameplay key");

        world.OpenManagementScreen("ranch");
        game.State.Calendar.Phase = DayPhase.Night;
        game.State.Calendar.NightAction = string.Empty;
        game.NotifyStateChanged();
        var beforeDay = game.State.Calendar.Day;
        var endDay = shell.GetNode<Button>(shell.EndDayButtonPath);
        Check(r, endDay.Text == "Plan Night", "management exposes Plan Night until a night action is chosen");
        endDay.EmitSignal(BaseButton.SignalName.Pressed);
        Check(r, beforeDay == game.State.Calendar.Day && game.State.Calendar.Phase == DayPhase.Night,
            "management cannot silently settle a night with no selected action");
        game.State.Calendar.Phase = DayPhase.Morning;
        game.NotifyStateChanged();
        world.CloseManagement();
    }

    private static async Task CheckCombat(GameRoot game, WorldGameController world, SmokeTestResult r)
    {
        var shell = world.Shell!;
        game.EndCombatSession();
        world.OpenManagementScreen("ranch");
        shell.ShowScreen("combat");
        Check(r, !game.CombatWorldTimeLocked && shell.CurrentScreen != "combat",
            "rejected combat navigation creates no clock lock");
        world.OpenManagementScreen("adventure");
        var fight = PlayabilityRegressionTests.Buttons(shell).First(button => button.Text == "Fight" && !button.Disabled);
        fight.EmitSignal(BaseButton.SignalName.Pressed);
        Check(r, shell.CurrentScreen == "combat" && game.CombatWorldTimeLocked && game.LastCombatReport is not null,
            "Fight opens a prepared mission with a stable clock owner");
        var cost = game.AdventureStaminaCost(game.LastCombatReport!.MissionId);
        var stamina = game.State.Player.Stamina;
        var tactical = PlayabilityRegressionTests.Buttons(shell).First(button => button.Text.StartsWith("Tactical Battle") && !button.Disabled);
        tactical.EmitSignal(BaseButton.SignalName.Pressed);
        await Frames(game, 2);
        var session = game.ActiveCombatSession;
        Check(r, session is not null && shell.CurrentScreen == "combat" && game.CombatWorldTimeLocked,
            "Tactical Battle stays in the live combat menu after state notification");
        Check(r, game.State.Player.Stamina == stamina - cost, "live tactical entry charges the displayed stamina once");
        var endDay = shell.GetNode<Button>(shell.EndDayButtonPath);
        Check(r, endDay.Disabled, "phase advance is visibly disabled during combat");
        if (session is { IsFinished: false })
        {
            Check(r, !world.CloseManagement(), "Return to World cannot abandon an unresolved tactical session");
            shell.ShowScreen("shop");
            Check(r, shell.CurrentScreen == "combat" && ReferenceEquals(game.ActiveCombatSession, session),
                "another menu cannot discard an unresolved battle");
            Check(r, !game.BeginInteractiveCombat(session.Report.MissionId) && ReferenceEquals(game.ActiveCombatSession, session),
                "repeated combat-start commands do not reset the current encounter");
            game.AutoFinishInteractiveCombat();
        }
        Check(r, game.CurrentCombatPhase == CombatPhase.BattleResults && shell.CurrentScreen == "combat",
            "finished tactical combat reaches the actual results menu");
        world.CloseManagement();
        Check(r, !world.IsManagementVisible && !game.CombatWorldTimeLocked && game.ActiveCombatSession is null,
            "leaving results releases both management and the combat clock");
        var phase = game.State.Calendar.Phase;
        world.AdvanceWorldTime();
        Check(r, phase != game.State.Calendar.Phase, "world time advances again after results close");
    }

    private static Control? GetFocus(GameRoot game) => game.GetViewport().GuiGetFocusOwner();
    private static async Task Frames(GameRoot game, int count)
    {
        for (var i = 0; i < count; i++) await game.ToSignal(game.GetTree(), SceneTree.SignalName.ProcessFrame);
    }
    private static async Task Stroke(GameRoot game, Key key)
    {
        Input.ParseInputEvent(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = true });
        Input.FlushBufferedEvents();
        await Frames(game, 2);
        Release(key);
        await Frames(game, 2);
    }
    private static void Release(Key key)
    {
        Input.ParseInputEvent(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = false });
        Input.FlushBufferedEvents();
    }
    private static void Check(SmokeTestResult r, bool passed, string message)
    {
        r.Passed &= passed;
        r.Lines.Add($"SMOKE {(passed ? "OK" : "FAIL")} HUD/menu: {message}");
    }
}
