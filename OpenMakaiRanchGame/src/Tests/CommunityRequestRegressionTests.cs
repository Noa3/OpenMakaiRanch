using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Data;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

/// <summary>Run only in the existing disposable-profile smoke harness; root tests use slot 99.</summary>
public static class CommunityRequestRegressionTests
{
    public static void Run(SmokeTestResult result)
    {
        TestOffersAndTransactions(result);
        TestBounds(result);
        TestProductionLoop(result);
        TestRootRoundTrip(result);
        TestPauseIntegration(result);
        TestSchemaRejectionBeforeMutation(result);
        TestCurrentManaSupplyContract(result);
    }

    private static (SaveState state, FlagService flags, CommunityRequestService requests) Fixture(int day = 2)
    {
        var state = new SaveState();
        state.Calendar.Day = day;
        state.Economy.Gold = 100;
        var flags = new FlagService();
        return (state, flags, new CommunityRequestService(state, flags, new EconomyService(state)));
    }

    private static void Stock(SaveState state, int amount)
    {
        foreach (var id in new[] { "farm_goods", "meals", "supplies" })
            state.Ranch.Stockpile[id] = amount;
    }

    private static void TestOffersAndTransactions(SmokeTestResult result)
    {
        var (state, flags, requests) = Fixture(1);
        Stock(state, 100);
        Check(result, requests.GetOffers().All(offer => !offer.CanDeliver), "Day 1 previews orders without allowing delivery");
        Check(result, !requests.TryDeliver("market_basket", 1, out _), "Day 1 command cannot bypass the opening day");
        state.Calendar.Day = 2;
        var offers = requests.GetOffers();
        var flagCount = flags.GlobalFlagCount;
        Check(result, offers.Count == 3 && offers.Select(offer => offer.Id).Distinct().Count() == 3,
            "three distinct production choices are available");
        Check(result, offers.SequenceEqual(requests.GetOffers()), "reading or reopening the board cannot reroll offers");
        Check(result, offers.All(offer => offer.RequiredAmount > 0 && offer.RewardGold > 0
            && offer.RewardGold <= CommunityRequestService.MaximumDailyRewardGold), "quantities and rewards are positive and bounded");
        Check(result, flags.GlobalFlagCount == flagCount && state.Economy.Gold == 100
            && state.Ranch.Stockpile.Values.All(amount => amount == 100), "reading the board has no gameplay side effects");
        Check(result, !requests.TryDeliver(null, 2, out _) && !requests.TryDeliver("unknown", 2, out _),
            "unknown requests cannot pay rewards");
        Check(result, !requests.TryDeliver(offers[0].Id, 3, out _), "a stale day is rejected before consuming goods");
        Check(result, flags.GlobalFlagCount == flagCount && state.Economy.Gold == 100
            && state.Ranch.Stockpile.Values.All(amount => amount == 100), "rejected requests leave the ledger unchanged");

        var stamina = state.Player.Stamina;
        var mana = state.Player.Mana;
        var phase = state.Calendar.Phase;
        var income = state.Economy.LastIncome;
        var selected = offers[0];
        Check(result, requests.TryDeliver(selected.Id, 2, out var receipt) && !string.IsNullOrWhiteSpace(receipt),
            "a stocked order produces a delivery receipt");
        Check(result, state.Economy.Gold == 100 + selected.RewardGold
            && state.Ranch.Stockpile[selected.ResourceId] == 100 - selected.RequiredAmount,
            "delivery consumes exactly the stated goods and pays exactly the stated reward");
        Check(result, state.Ranch.Stockpile["meals"] == 100 && state.Ranch.Stockpile["supplies"] == 100,
            "unrelated stock is preserved");
        Check(result, state.Calendar.Day == 2 && state.Calendar.Phase == phase
            && state.Player.Stamina == stamina && state.Player.Mana == mana && state.Economy.LastIncome == income,
            "delivery does not advance time, drain exploration resources or replay settlement income");
        Check(result, requests.CompletedDeliveries == 1 && requests.HasDeliveredToday
            && flags.GetGlobalIntFlag(CommunityRequestService.LastDeliveryRewardFlag) == selected.RewardGold,
            "the bounded receipt records the paid day and amount");
        var paidGold = state.Economy.Gold;
        Check(result, !requests.TryDeliver(selected.Id, 2, out _) && !requests.TryDeliver(offers[1].Id, 2, out _)
            && state.Economy.Gold == paidGold, "double-clicking and switching orders cannot pay a second time");
        Check(result, requests.GetOffers().All(offer => !offer.CanDeliver), "all rows show the shared daily delivery limit");

        flags.SyncToStorage(state.Flags);
        var restoredFlags = new FlagService();
        restoredFlags.SyncFromStorage(JsonSerializer.Deserialize<FlagStorage>(JsonSerializer.Serialize(state.Flags))!);
        var restored = new CommunityRequestService(state, restoredFlags, new EconomyService(state));
        Check(result, restored.HasDeliveredToday && !restored.TryDeliver(offers[2].Id, 2, out _),
            "a serialized receipt prevents same-day replay after service reconstruction");
        state.Calendar.Day = 3;
        Check(result, restored.GetOffers().Any(offer => offer.CanDeliver)
            && restored.TryDeliver(offers[1].Id, 3, out _) && restored.CompletedDeliveries == 2,
            "the next in-game day unlocks one new delivery");
        Check(result, restoredFlags.GlobalFlagCount == flagCount + 4, "receipt storage stays at four fields rather than growing per day");
        state.Calendar.Day = 12;
        Check(result, restored.CompletedDeliveries == 2 && restored.GetOffers().All(offer => offer.CanDeliver),
            "skipping days causes no penalty and does not build a daily backlog");
    }

    private static void TestBounds(SmokeTestResult result)
    {
        var (state, flags, requests) = Fixture();
        Check(result, !requests.TryDeliver("market_basket", 2, out _) && state.Ranch.Stockpile.Count == 0,
            "missing stock is rejected without creating inventory entries");
        state.Ranch.Stockpile["farm_goods"] = -20;
        Check(result, requests.GetOffers()[0].AvailableAmount == 0 && !requests.TryDeliver("market_basket", 2, out _)
            && state.Ranch.Stockpile["farm_goods"] == -20, "negative stock cannot fund an order or be mutated by a preview");
        Stock(state, 100);
        state.Economy.Gold = int.MaxValue;
        Check(result, !requests.TryDeliver("market_basket", 2, out _)
            && state.Ranch.Stockpile["farm_goods"] == 100 && requests.CompletedDeliveries == 0,
            "wallet overflow rejects delivery without consuming goods or its daily slot");
        var offer = requests.GetOffers()[0];
        state.Economy.Gold = int.MaxValue - offer.RewardGold;
        Check(result, requests.TryDeliver(offer.Id, 2, out _) && state.Economy.Gold == int.MaxValue,
            "an exact-fit wallet receives the entire reward");
        state.Calendar.Day = int.MaxValue;
        state.Economy.Gold = 0;
        flags.SetGlobalIntFlag(CommunityRequestService.CompletedDeliveriesFlag, int.MaxValue);
        Check(result, requests.GetOffers().All(item => item.RequiredAmount is >= 2 and <= 5
            && item.RewardGold <= CommunityRequestService.MaximumDailyRewardGold), "extreme day values keep quantities bounded");
        Check(result, requests.TryDeliver("workshop_delivery", int.MaxValue, out _)
            && requests.CompletedDeliveries == int.MaxValue, "lifetime receipt count saturates instead of wrapping negative");
        state.Calendar.Day = 3;
        Check(result, !requests.TryDeliver("kitchen_delivery", 3, out _), "a future-dated receipt fails closed");
    }

    private static void TestProductionLoop(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var schedule = new ScheduleService(state, data);
        var equipment = new EquipmentService(state, data);
        var talents = new TalentService(state, data);
        var ranch = new RanchService(state, data, equipment, talents);
        var economy = new EconomyService(state);
        var milestones = new MilestoneService(state, data, economy);
        var settlement = new DailySettlementService(state, data, schedule, ranch, economy,
            new DayCycleService(state), milestones, new InventoryService(state), talents);
        schedule.AssignJob("rancher", "pasture");
        settlement.SettleDay();
        var requests = new CommunityRequestService(state, new FlagService(), economy);
        var offer = requests.GetOffers().First(item => item.Id == "market_basket");
        Check(result, state.Calendar.Day == 2 && offer.CanDeliver,
            "the actual first-day pasture settlement supplies the Day 2 market order");
        var gold = economy.Gold;
        Check(result, requests.TryDeliver(offer.Id, 2, out _) && economy.Gold == gold + offer.RewardGold,
            "ordinary production can complete a courier order without a second reward calculator");
        Check(result, requests.GetOffers().All(item => data.Jobs.Values.Any(job => job.Assignable
            && job.ResourceId == item.ResourceId && job.ResourceAmount > 0)), "every order has an existing playable production source");
    }

    private static void TestRootRoundTrip(SmokeTestResult result)
    {
        var game = GameRoot.Instance;
        game.NewGame();
        try
        {
            game.State.Calendar.Day = 2;
            Stock(game.State, 100);
            var generation = game.StateGeneration;
            var gold = game.Economy.Gold;
            var notifications = 0;
            var reentrantPaid = false;
            void OnChanged()
            {
                notifications++;
                if (notifications == 1)
                    reentrantPaid = game.TryDeliverCommunityRequest("kitchen_delivery", 2, generation, out _);
            }
            game.StateChanged += OnChanged;
            try
            {
                Check(result, !game.TryDeliverCommunityRequest("market_basket", 2, generation + 1, out _)
                    && notifications == 0 && game.Economy.Gold == gold, "root rejects a stale generation without notifying observers");
                Check(result, game.TryDeliverCommunityRequest("market_basket", 2, generation, out _), "root delivery command succeeds");
                Check(result, notifications == 1 && !reentrantPaid,
                    "receipt is complete before the single notification; reentrant delivery cannot duplicate rewards");
            }
            finally { game.StateChanged -= OnChanged; }
            var paidGold = game.Economy.Gold;
            var remaining = game.State.Ranch.Stockpile["farm_goods"];
            Check(result, game.SaveSlot(99) && game.LoadSlot(99), "root courier state saves and loads through the real save boundary");
            Check(result, game.CompletedCommunityDeliveries == 1 && game.Economy.Gold == paidGold
                && game.State.Ranch.Stockpile["farm_goods"] == remaining, "gold, stock and receipt round-trip together");
            Check(result, !game.TryDeliverCommunityRequest("kitchen_delivery", 2, game.StateGeneration, out _),
                "current-generation commands cannot replay a saved delivery");
            Check(result, !game.TryDeliverCommunityRequest("kitchen_delivery", 2, generation, out _),
                "a pre-load callback cannot target the rebuilt session");
            game.State.Calendar.Day = 3;
            Check(result, game.TryDeliverCommunityRequest("kitchen_delivery", 3, game.StateGeneration, out _)
                && game.CompletedCommunityDeliveries == 2, "new-day delivery remains playable after loading");
        }
        finally
        {
            game.Save.Delete(99);
            game.NewGame();
        }
    }

    private static void TestPauseIntegration(SmokeTestResult result)
    {
        var game = GameRoot.Instance;
        var tree = game.GetTree();
        var wasPaused = tree.Paused;
        var pause = new PauseMenuController { Name = "CommunityPauseFixture" };
        var center = new CenterContainer { Name = "Center" };
        var panel = new PanelContainer { Name = "Panel" };
        var content = new VBoxContainer { Name = "Content" };
        pause.AddChild(center);
        center.AddChild(panel);
        panel.AddChild(content);
        content.AddChild(new Label { Name = "ContextLabel" });
        foreach (var name in new[] { "ResumeButton", "SaveLoadButton", "SettingsButton", "MainMenuButton", "QuitButton" })
            content.AddChild(new Button { Name = name, Text = name });
        try
        {
            tree.Root.AddChild(pause);
            pause.Open("ranch");
            var open = content.GetNode<Button>("CommunityBoardButton");
            open.EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, pause.IsCommunityBoardOpen && tree.Paused && !center.Visible,
                "pause menu opens a usable board without resuming the world");
            var board = pause.GetNode<Control>("CommunityBoard");
            var first = board.GetNode<Button>("Margin/Layout/OrdersScroll/Orders/Order0/DeliverButton");
            Check(result, first.Disabled, "Day 1 delivery button mirrors the service lock");
            Check(result, board.GetNode<ScrollContainer>("Margin/Layout/OrdersScroll").FollowFocus,
                "scrolling follows controller and keyboard focus");
            pause._UnhandledInput(new InputEventAction { Action = "ui_cancel", Pressed = true });
            Check(result, pause.IsOpen && !pause.IsCommunityBoardOpen && tree.Paused && center.Visible,
                "Back closes only the board and retains pause ownership");
            pause._UnhandledInput(new InputEventAction { Action = "ui_cancel", Pressed = true });
            Check(result, !pause.IsOpen && !tree.Paused, "a second Back resumes the game");
            pause.Open("town");
            open.EmitSignal(BaseButton.SignalName.Pressed);
            var route = string.Empty;
            pause.ManagementScreenRequested += screen => route = screen;
            board.GetNode<Button>("Margin/Layout/Footer/PlanButton").EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, route == "schedule" && !pause.IsOpen && !tree.Paused,
                "Plan ranch work routes to the existing schedule screen and releases pause ownership");
            Check(result, game.CompletedCommunityDeliveries == 0 && game.State.Calendar.Day == 1,
                "browsing and planning do not create deliveries or advance the day");
        }
        finally
        {
            pause.Free();
            tree.Paused = wasPaused;
        }
    }

    private static void TestSchemaRejectionBeforeMutation(SmokeTestResult result)
    {
        var future = new SaveState { SchemaVersion = SaveState.CurrentSchemaVersion + 1, Roster = null! };
        future.Economy.Gold = 321;
        var version = future.SchemaVersion;
        var rejected = false;
        try { SaveMigrator.Migrate(future); }
        catch (InvalidOperationException) { rejected = true; }
        Check(result, rejected && future.SchemaVersion == version && future.Roster is null && future.Economy.Gold == 321,
            "future schemas are rejected before normalization mutates the supplied object");
        var malformed = new SaveState { SchemaVersion = 13 };
        malformed.Roster.Characters = new List<CharacterState> { null! };
        rejected = false;
        try { SaveMigrator.Migrate(malformed); }
        catch (InvalidOperationException) { rejected = true; }
        Check(result, rejected && malformed.SchemaVersion == 13,
            "null roster entries are rejected before any version migration");
    }

    private static void TestCurrentManaSupplyContract(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        state.Player.Mana = 20;
        state.Economy.ManaReservoir = 45;
        var magic = new MagicService(state, data);
        Check(result, magic.RechargePlayerManaFromStorage(30) == 0, "mana supply still requires its device");
        state.Inventory.Items["magic_supply_device"] = 1;
        Check(result, magic.RechargePlayerManaFromStorage(30) == 22 && state.Player.Mana == 42
            && state.Economy.ManaReservoir == 1, "45 stored MP restores 22 personal MP at the existing 2:1 cost");
        Check(result, magic.SpendPlayerMana(10) && state.Player.Mana == 32
            && state.Economy.ManaReservoir == 1, "subsequent personal MP spending leaves the ranch reserve separate");
    }

    private static void Check(SmokeTestResult result, bool condition, string description)
    {
        result.Passed &= condition;
        result.Lines.Add($"SMOKE {(condition ? "OK" : "FAIL")} community: {description}");
    }
}
