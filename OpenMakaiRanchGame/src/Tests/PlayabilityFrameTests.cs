using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

/// <summary>
/// A bounded frame-driven walkthrough from the fresh world entry to Day 2, production, delivery
/// and save/load. Keyboard events pass through Godot's InputMap and real process frames. Positions
/// near interactables are staged explicitly; this is not a navigation, art or physical-pad playtest.
/// </summary>
public static class PlayabilityFrameTests
{
    public static async Task Run(GameRoot game, SmokeTestResult result)
    {
        var settings = game.State.Settings.Clone();
        var tree = game.GetTree();
        Node? priorScene = null;
        var priorMode = Node.ProcessModeEnum.Inherit;
        var priorVisible = false;
        WorldGameController? world = null;
        try
        {
            tree.Paused = false;
            // Earlier synchronous tests queue temporary nodes for deletion. Let the real tree flush.
            await Frames(game, 3);
            // Bootstrap can route to MainMenu during those frames. Capture the actual current
            // scene now, not its already-freed predecessor, before isolating the playable world.
            priorScene = tree.CurrentScene;
            priorMode = priorScene?.ProcessMode ?? Node.ProcessModeEnum.Inherit;
            priorVisible = priorScene is CanvasItem canvas && canvas.Visible;
            if (priorScene is not null && GodotObject.IsInstanceValid(priorScene))
            {
                priorScene.ProcessMode = Node.ProcessModeEnum.Disabled;
                if (priorScene is CanvasItem existingUi) existingUi.Visible = false;
            }
            game.NewGame();
            game.State.Settings.ReducedMotion = true;
            world = PlayabilityRegressionTests.CreateWorld(game);
            var flow = world.FirstDayFlow ?? throw new InvalidOperationException("First-day controller missing");
            await Until(game, () => flow.IsActive && flow.BlocksWorldInput, "first wake-up dialogue");
            Check(result, game.State.Calendar.Day == 1 && flow.CurrentStage == FirstDayFlowController.StageWakeUp,
                "fresh world entry starts the guided first day");
            PlayabilityRegressionTests.Press(flow, "Wake up");
            await Frames(game, 3);

            var intro = world.IntroHouse ?? throw new InvalidOperationException("Intro house missing");
            var player = intro.Player ?? throw new InvalidOperationException("Intro player missing");
            var stamina = game.State.Player.Stamina;
            var position = player.GlobalPosition;
            await KeyStroke(game, Key.W, 12);
            var displacement = player.GlobalPosition - position;
            Check(result, new Vector2(displacement.X, displacement.Z).Length() > 0.02f,
                "a real W InputMap event moves the CharacterBody across physics frames");
            Check(result, game.State.Player.Stamina == stamina,
                "ordinary exploration remains free of daily stamina charges");

            player.GlobalPosition = intro.GetNode<Node3D>("ExitDoorPoint").GlobalPosition;
            await Frames(game, 2);
            await KeyStroke(game, Key.F);
            await Until(game, () => flow.CurrentStage == FirstDayFlowController.StageRanchWelcome && flow.BlocksWorldInput,
                "ranch welcome after the bedroom door interaction");
            Check(result, world.ActiveAreaId == "ranch", "real door interaction enters the ranch instead of leaving the introduction stuck");
            PlayabilityRegressionTests.Press(flow, "Show me the ranch");
            await Frames(game, 2);

            var ranch = world.Ranch ?? throw new InvalidOperationException("Ranch missing");
            var pasture = ranch.Stations.First(station => station.CommandTargetId == "pasture");
            var pastureWorker = ranch.SelectedCharacterId;
            ranch.Player!.GlobalPosition = pasture.GlobalPosition;
            await Frames(game, 2);
            await KeyStroke(game, Key.F);
            await Until(game, () => flow.CurrentStage == FirstDayFlowController.StageManagementDairy, "pasture tutorial assignment");
            Check(result, game.Schedule.GetAssignment(pastureWorker) == "pasture",
                "physical workstation input assigns work in the shared schedule");
            PlayabilityRegressionTests.Press(flow, "Open Schedule");
            await Frames(game, 2);
            var dairyLabel = game.Data.Jobs["dairy"].DisplayName;
            var dairyButtons = PlayabilityRegressionTests.Buttons(world.Shell!).Where(button => button.Text == dairyLabel).ToArray();
            var dairyIndex = game.Roster.Characters.ToList().FindIndex(character => character.Id != pastureWorker);
            if (dairyIndex < 0 || dairyIndex >= dairyButtons.Length || dairyButtons[dairyIndex].Disabled)
                throw new InvalidOperationException("Second worker's dairy assignment button unavailable");
            dairyButtons[dairyIndex].EmitSignal(BaseButton.SignalName.Pressed);
            await Frames(game, 2);
            ReturnToWorld(world);
            await Until(game, () => flow.CurrentStage == FirstDayFlowController.StageInvestigateIntruder && flow.BlocksWorldInput,
                "evening ranch-tour completion");
            Check(result, game.State.Calendar.Phase == DayPhase.Evening && game.State.Story.RanchTourCompleted,
                "pasture plus a separate dairy assignment complete the tour through its real UI");

            PlayabilityRegressionTests.Press(flow, "I'll check it");
            ranch.Player.GlobalPosition = ranch.GetNode<Node3D>("FirstDayIntruder").GlobalPosition;
            await Until(game, () => flow.CurrentStage == FirstDayFlowController.StageIntruderCombat && flow.BlocksWorldInput,
                "intruder proximity encounter");
            PlayabilityRegressionTests.Press(flow, "Confront her");
            await Frames(game, 2);
            Check(result, game.LastCombatReport is not null && game.LastCombatReport.Outcome != MissionOutcome.Failure,
                "the fresh starting party can finish the mandatory tutorial fight without stat cheats");
            PlayabilityRegressionTests.Press(flow, "Detain the intruder");
            PlayabilityRegressionTests.Press(flow, "Sit and talk");
            PlayabilityRegressionTests.Press(flow, "Ask about the ranch");
            PlayabilityRegressionTests.Press(flow, "Finish the conversation");
            Check(result, game.State.Calendar.Phase == DayPhase.Night && !game.CombatWorldTimeLocked,
                "tutorial results release the combat clock before the normal night routine");
            PlayabilityRegressionTests.Press(flow, "Take a hot bath, then sleep");
            PlayabilityRegressionTests.Press(flow, "End Day");
            await Frames(game, 3);
            Check(result, game.State.Calendar.Day == 2 && game.State.Calendar.Phase == DayPhase.Morning
                && game.LastDailyReport?.Day == 1 && world.Shell!.CurrentScreen == "report",
                "the complete first-day story reaches one ordinary settlement and its report");
            Check(result, game.State.Player.Stamina == game.State.Player.MaxStamina + PlayerStaminaService.HotBathNextDayBonus,
                "the prepared evening bath supplies extra starting stamina on Day 2");
            ReturnToWorld(world);
            await Frames(game, 2);

            // Exercise both pause routing and a real production-backed order, not injected stock.
            var pauseEntry = $"handler={world.IsProcessingUnhandledInput()}, process={world.CanProcess()}, "
                + $"management={world.IsManagementVisible}, flow={world.FlowLocksUi}, "
                + $"story={flow.BlocksWorldInput}, transition={world.Transition?.IsTransitioning}, "
                + $"focus={game.GetViewport().GuiGetFocusOwner()?.GetPath()}, scene={tree.CurrentScene?.SceneFilePath}";
            await KeyStroke(game, Key.Escape);
            Check(result, world.PauseMenu!.IsOpen && tree.Paused,
                $"real Escape input opens pause (before: {pauseEntry}; after open={world.PauseMenu.IsOpen}, paused={tree.Paused})");
            PlayabilityRegressionTests.Press(world.PauseMenu, "Community Board");
            var offer = game.GetCommunityRequests().First(value => value.Id == "market_basket");
            var gold = game.Economy.Gold;
            var stock = game.State.Ranch.Stockpile.GetValueOrDefault(offer.ResourceId);
            Check(result, offer.CanDeliver, "actual tutorial pasture production supplies the Day 2 market basket");
            var delivery = world.PauseMenu.GetNode<Button>("CommunityBoard/Margin/Layout/OrdersScroll/Orders/Order0/DeliverButton");
            if (delivery.Disabled) throw new InvalidOperationException("Production-backed delivery button is disabled");
            delivery.EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, game.Economy.Gold == gold + offer.RewardGold
                && game.State.Ranch.Stockpile[offer.ResourceId] == stock - offer.RequiredAmount,
                "the board consumes the displayed stock and pays the displayed gold exactly once");
            await KeyStroke(game, Key.Escape);
            Check(result, world.PauseMenu.IsOpen && !world.PauseMenu.IsCommunityBoardOpen,
                "first real Back closes only the nested courier board");
            await KeyStroke(game, Key.Escape);
            Check(result, !world.PauseMenu.IsOpen && !tree.Paused && ranch.InputGate.WorldInputEnabled,
                "second real Back resumes the world without reopening pause in the same frame");

            Check(result, game.SaveSlot(99), "completed first-day session writes the isolated smoke slot");
            var savedGold = game.Economy.Gold;
            var savedStock = game.State.Ranch.Stockpile[offer.ResourceId];
            game.NewGame();
            Check(result, game.LoadSlot(99), "the completed session loads through the real root save boundary");
            Check(result, game.State.Calendar.Day == 2 && game.State.Story.FirstDayCompleted
                && game.Economy.Gold == savedGold && game.State.Ranch.Stockpile[offer.ResourceId] == savedStock
                && game.GetCommunityRequests().All(value => !value.CanDeliver),
                "story, day, stock, gold and one-per-day receipt survive load together");
        }
        catch (Exception exception)
        {
            Check(result, false, $"frame walkthrough stopped: {exception.Message}");
        }
        finally
        {
            ReleaseKey(Key.W);
            ReleaseKey(Key.F);
            ReleaseKey(Key.Escape);
            tree.Paused = false;
            if (world is not null && GodotObject.IsInstanceValid(world)) world.Free();
            game.Save.Delete(99); // Same disposable slot as the launcher-isolated suite, never a personal profile.
            game.State.Settings = settings;
            GameRoot.PendingInitialScreen = null;
            game.NewGame();
            if (priorScene is not null && GodotObject.IsInstanceValid(priorScene))
            {
                priorScene.ProcessMode = priorMode;
                if (priorScene is CanvasItem existingUi) existingUi.Visible = priorVisible;
            }
            await Frames(game, 2);
        }
    }

    private static void ReturnToWorld(WorldGameController world)
    {
        var button = world.GetNode<Button>("ManagementLayer/ManagementUi/UiShell/Margin/RootPanel/Root/TopBar/TopBarRow1/ReturnToWorldButton");
        if (!button.IsVisibleInTree() || button.Disabled) throw new InvalidOperationException("Return-to-world button unavailable");
        button.EmitSignal(BaseButton.SignalName.Pressed);
    }

    private static async Task Frames(GameRoot game, int count)
    {
        for (var i = 0; i < count; i++)
            await game.ToSignal(game.GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    private static async Task Until(GameRoot game, Func<bool> predicate, string step)
    {
        for (var frame = 0; frame < 180; frame++)
        {
            if (predicate()) return;
            await Frames(game, 1);
        }
        throw new InvalidOperationException($"No progress after 180 process frames: {step}");
    }

    private static async Task KeyStroke(GameRoot game, Key key, int holdFrames = 2)
    {
        Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = key, Keycode = key, Pressed = true });
        Input.FlushBufferedEvents();
        // Headless render frames may run faster than physics. Movement must span actual ticks.
        if (key == Key.W)
        {
            for (var i = 0; i < holdFrames; i++)
                await game.ToSignal(game.GetTree(), SceneTree.SignalName.PhysicsFrame);
        }
        else
        {
            await Frames(game, holdFrames);
        }
        ReleaseKey(key);
        await Frames(game, 2);
    }

    private static void ReleaseKey(Key key)
    {
        Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = key, Keycode = key, Pressed = false });
        Input.FlushBufferedEvents();
    }

    private static void Check(SmokeTestResult result, bool passed, string message)
    {
        result.Passed &= passed;
        result.Lines.Add($"SMOKE {(passed ? "OK" : "FAIL")} frame playthrough: {message}");
    }
}
