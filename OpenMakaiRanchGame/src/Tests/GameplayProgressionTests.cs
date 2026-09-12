using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Data;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.Tests;

/// <summary>Gameplay-only contracts and bounded policy comparisons, not navigation or optimality proofs.</summary>
public static class GameplayProgressionTests
{
    private static void Check(SmokeTestResult result, bool pass, string message)
    { result.Passed &= pass; result.Lines.Add($"SMOKE {(pass ? "OK" : "FAIL")} progression: {message}"); }

    public static void Run(SmokeTestResult result)
    {
        CheckFood(result);
        CheckBuildings(result);
        CheckRecognition(result);
        CheckRootSaving(result);
        var purchased = RunPolicy(result, "purchased_food", false, false);
        var kitchen = RunPolicy(result, "adaptive_kitchen", true, false);
        var upgraded = RunPolicy(result, "upgraded_kitchen", true, true);
        Check(result, kitchen.MealSpend < purchased.MealSpend && kitchen.FinalGold > purchased.FinalGold,
            "ordinary food production reduces purchases and improves the tested policy's final balance");
        Check(result, upgraded.UpgradeSpend > kitchen.UpgradeSpend && upgraded.HomeMeals > 0,
            "the upgrade policy actually paid for equipment and used its food, without assuming it is optimal");
    }

    private static void CheckFood(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = DaySettlementRegressionTests.Fixture(data, 3, "rest");
        state.Ranch.Stockpile["meals"] = 2;
        state.Inventory.Items["meal_box"] = 1;
        var service = new RanchProvisioningService(state, data);
        var before = JsonSerializer.Serialize(state);
        for (var i = 0; i < 20; i++) service.InspectLunch();
        Check(result, before == JsonSerializer.Serialize(state) && service.InspectLunch() == new LunchPlan(3, 2, 1, 0),
            "lunch previews are read-only and allocate pantry meals before portable boxes");
        state.Roster.Characters[0].Energy = 190; // An existing over-cap bonus must not be erased by lunch.
        var report = new DailyReport();
        var lunch = service.ConsumeLunch(report);
        Check(result, lunch.PantryMeals == 2 && lunch.MealBoxes == 1 && state.Ranch.Stockpile["meals"] == 0
            && state.Inventory.Items["meal_box"] == 0, "three lunches conserve exactly two ranch meals and one box");
        Check(result, state.Roster.Characters[0].Energy == 190 && state.Roster.Characters[1].Energy == 75,
            "lunch recovers normal energy without erasing an existing above-cap bonus");
        Check(result, service.InspectLunch().MissingMeals == 3, "using food cannot leave phantom servings");
        state.Ranch.Stockpile["meals"] = -2; state.Inventory.Items["meal_box"] = -3;
        before = JsonSerializer.Serialize(state);
        Check(result, service.InspectLunch() == new LunchPlan(3, 0, 0, 3) && JsonSerializer.Serialize(state) == before,
            "malformed negative food is not edible and inspection does not rewrite saves");
        state.Ranch.Stockpile["meals"] = int.MaxValue; state.Inventory.Items["meal_box"] = int.MaxValue;
        state.Schedule.AssignedJobs[state.Roster.Characters[2].Id] = "rest";
        Check(result, service.InspectLunch() == new LunchPlan(2, 2, 0, 0), "large stock cannot overflow and resting keeps the old zero-cost rule");
        service.ConsumeLunch(new DailyReport());
        Check(result, state.Ranch.Stockpile["meals"] == int.MaxValue - 2 && state.Inventory.Items["meal_box"] == int.MaxValue,
            "portable items remain available for gifts, recovery and outings when pantry food covers lunch");

        // No inventory grants during this production/consumption loop: output is available next day.
        state = DaySettlementRegressionTests.Fixture(data, 2, "rest");
        state.Roster.Characters.ForEach(c => { c.Hp = 100; c.Energy = 60; });
        state.Inventory.Items["meal_box"] = 0;
        foreach (var c in state.Roster.Characters) state.Schedule.AssignedJobs[c.Id] = "kitchen";
        var first = MakeSettlement(state, data); first.SettleDay();
        Check(result, first.LastWorkFacts!.Lunch.MissingMeals == 2 && state.Ranch.Stockpile["meals"] > 2,
            "today's cooking does not retroactively feed yesterday's shift");
        state.Calendar.NightAction = "rest";
        var second = MakeSettlement(state, data); second.SettleDay();
        Check(result, second.LastWorkFacts!.Lunch is { PantryMeals: 2, MealBoxes: 0, MissingMeals: 0 },
            "the next ordinary settlement uses the previous day's actual kitchen production");
    }

    private static void CheckBuildings(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = DaySettlementRegressionTests.Fixture(data, 1, "rest");
        var worker = state.Roster.Characters[0]; worker.Hp = 100;
        var ranch = new RanchService(state, data, new EquipmentService(state, data), new TalentService(state, data));
        foreach (var (jobId, facilityId) in new[] { ("kitchen", "kitchen"), ("cooking", "kitchen"),
            ("dairy", "dairy_barn"), ("pasture", "pasture"), ("workshop", "workshop"),
            ("pharmacy", "pharmacy_lab"), ("cleaning", "bathhouse"), ("customer_service", "guest_room") })
        {
            state.Ranch.Facilities.Clear();
            var job = data.Jobs[jobId];
            var baseline = ranch.PreviewJobOutput(worker, job);
            state.Ranch.Facilities[facilityId] = 1;
            Check(result, ranch.PreviewJobOutput(worker, job) == baseline, jobId + ": level one preserves the previous output and pay");
            state.Ranch.Facilities[facilityId] = 2;
            var before = JsonSerializer.Serialize(state);
            var forecast = ranch.PreviewJobOutput(worker, job);
            var benefit = ranch.InspectWorkBenefit(job);
            Check(result, forecast.FacilityBonus == data.Facilities[facilityId].OutputBonus
                && forecast.Amount == baseline.Amount + benefit.ExtraUnits && forecast.Gold == baseline.Gold
                && JsonSerializer.Serialize(state) == before, jobId + ": useful local equipment produces stock, not duplicate wages");
            var previous = state.Ranch.Stockpile.GetValueOrDefault(job.ResourceId);
            Check(result, ranch.ApplyJobOutput(worker, job, new DailyReport()) == forecast.Gold
                && state.Ranch.Stockpile[job.ResourceId] == previous + forecast.Amount,
                jobId + ": actual production uses the same tier contribution as its public preview");
            state.Ranch.Facilities[facilityId] = 5; var five = ranch.PreviewJobOutput(worker, job);
            state.Ranch.Facilities[facilityId] = int.MaxValue;
            Check(result, ranch.PreviewJobOutput(worker, job) == five, jobId + ": higher numerical levels cannot create unbounded tier bonuses");
        }
        state.Ranch.Facilities.Clear(); state.Ranch.Facilities["kitchen"] = 3;
        Check(result, ranch.InspectWorkBenefit(data.Jobs["office"]).ExtraUnits == 0,
            "kitchen equipment does not grant an unrelated office production bonus");
        worker.Fatigue = 85;
        Check(result, ranch.PreviewJobOutput(worker, data.Jobs["kitchen"]).FacilityBonus == 1,
            "existing fatigue also reduces the new equipment stock bonus");
        worker.Mature.FallState = FallState.Collapse;
        Check(result, ranch.PreviewJobOutput(worker, data.Jobs["kitchen"]).Amount == 0,
            "equipment is not unstaffed automation and does not make collapsed residents work");
        worker.Mature.FallState = default; worker.Fatigue = 0;
        state.Ranch.Stockpile["meals"] = int.MaxValue - 1;
        ranch.ApplyJobOutput(worker, data.Jobs["kitchen"], new DailyReport());
        Check(result, state.Ranch.Stockpile["meals"] == int.MaxValue, "extra output at inventory capacity never wraps stock negative");
    }

    private static void CheckRecognition(SmokeTestResult result)
    {
        var state = new SaveState(); state.Calendar.Day = 2;
        var flags = new FlagService();
        var service = new RanchProgressionService(state, flags);
        var before = JsonSerializer.Serialize(state);
        for (var i = 0; i < 30; i++) service.Inspect();
        Check(result, flags.GlobalFlagCount == 0 && before == JsonSerializer.Serialize(state)
            && service.Inspect().Count == 5, "optional goals have stable IDs and read-only untranslated-state inspection");
        SettledWorkFacts Facts(int day) => new(day, 3, 2, 1, 0, new(2, 2, 0, 0), true, 120, 70, 0,
            new Dictionary<string, long> { ["meals"] = 3, ["supplies"] = 2, ["farm_goods"] = 4 });
        var report = new DailyReport { Day = 1 };
        service.ObserveSettlement(Facts(1), report);
        Check(result, service.Inspect().Count(a => a.Complete) == 4 && report.Events.Count == 4
            && report.Events.All(e => e.GoldDelta == 0 && e.ItemAmount == 0),
            "earned goals may complete on day one and grant recognition, not economic snowball bonuses");
        var count = flags.GlobalFlagCount; before = JsonSerializer.Serialize(state);
        service.ObserveSettlement(Facts(1), report);
        Check(result, report.Events.Count == 4 && flags.GlobalFlagCount == count && before == JsonSerializer.Serialize(state),
            "replaying the same boundary cannot repeat an optional event or extend receipt storage");
        state.Calendar.Day = 3; state.VictoryDay = 2;
        flags.SetGlobalFlag(RanchLeisureService.RestoredFlag, true);
        flags.SetGlobalIntFlag(CommunityRequestService.CompletedDeliveriesFlag, 3);
        state.Ranch.Facilities["kitchen"] = 2; state.Milestones.CompletedIds.Add("first_patrol");
        var next = new DailyReport { Day = 2 };
        service.ObserveSettlement(Facts(2), next);
        Check(result, service.Inspect().Single(a => a.Id == "restored_home").CompletedDay == 2
            && next.Events.Count == 1 && state.VictoryDay == 2,
            "the restoration chapter can complete after victory without moving the existing finish line");
        state.Calendar.Day = 8;
        var denied = new DailyReport { Day = 3 };
        service.ObserveSettlement(Facts(3), denied);
        Check(result, denied.Events.Count == 0, "an unrelated day boundary cannot submit a stale completion");
        flags.SyncToStorage(state.Flags);
        var loaded = JsonSerializer.Deserialize<SaveState>(JsonSerializer.Serialize(state))!;
        var restored = new FlagService(); restored.SyncFromStorage(loaded.Flags);
        Check(result, new RanchProgressionService(loaded, restored).Inspect().SequenceEqual(service.Inspect()),
            "current-schema JSON preserves every first achievement day");
        foreach (var invalid in new[] { Facts(1) with { TeamWell = false }, Facts(1) with { AutomaticallyRested = 1 },
            Facts(1) with { Lunch = new(2, 1, 0, 1) }, Facts(1) with { WorkGold = 0 },
            Facts(1) with { PortableMealReplacementCost = 90 } })
        {
            var fresh = new SaveState(); fresh.Calendar.Day = 2;
            var check = new RanchProgressionService(fresh, new FlagService());
            check.ObserveSettlement(invalid, new DailyReport { Day = 1 });
            Check(result, !check.Inspect().Single(a => a.Id == "small_crew").Complete,
                "small-crew success requires real work, wellbeing, lunch and replacement-food cost coverage");
        }
    }

    private static void CheckRootSaving(SmokeTestResult result)
    {
        var game = GameRoot.Instance;
        var wrote = false;
        if (game.HasSaveSlot(99)) throw new InvalidOperationException("Progression test refuses an occupied isolated slot 99.");
        try
        {
            game.NewGame();
            game.State.Calendar.Day = 2; game.State.Calendar.Phase = DayPhase.Night;
            game.State.Calendar.NightAction = "rest"; game.State.Story.FirstDayCompleted = true;
            game.State.Ranch.Stockpile["meals"] = game.Roster.Characters.Count;
            foreach (var c in game.Roster.Characters) game.Schedule.AssignJob(c.Id, "kitchen");
            var before = game.StateGeneration;
            var day = game.State.Calendar.Day;
            var report = game.EndDay();
            Check(result, game.GetRanchAchievements().Single(a => a.Id == "home_cooking").CompletedDay == day
                && game.State.Calendar.Day == day + 1 && game.StateGeneration == before && !game.State.VictoryDay.HasValue,
                "the real root settlement records self-supply without replacing the session or completing the main campaign");
            wrote = game.SaveSlot(99);
            Check(result, wrote && game.LoadSlot(99), "the completed optional goal saves and loads through the ordinary root");
            var current = game.GetRanchAchievements().Single(a => a.Id == "home_cooking");
            var eventTitle = current.Title;
            game.State.Calendar.Phase = DayPhase.Night; game.State.Calendar.NightAction = "rest";
            var another = game.EndDay();
            Check(result, game.GetRanchAchievements().Single(a => a.Id == "home_cooking").CompletedDay == day
                && another.Events.All(e => e.Title != eventTitle) && !game.State.NgPlusActive,
                "continuing after load retains the first day and does not replay success or force a new run");
            game.StartNewGamePlus();
            Check(result, game.GetRanchAchievements().All(a => !a.Complete),
                "only explicit New Game+ starts new run-local achievement records");
        }
        finally { if (wrote) game.Save.Delete(99); game.NewGame(); }
    }

    private sealed record PolicyResult(int FinalGold, long MealSpend, long UpgradeSpend, int HomeMeals);

    private static PolicyResult RunPolicy(SmokeTestResult result, string id, bool useKitchen, bool upgrade)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data, new Random(41)).CreateNewGame();
        var initialGold = state.Economy.Gold;
        var money = new EconomyService(state);
        var ranch = new RanchService(state, data, new EquipmentService(state, data), new TalentService(state, data));
        var shop = new ShopService(data, money, new InventoryService(state));
        var schedule = new ScheduleService(state, data);
        var barnPrice = ranch.InspectFacilityUpgrade("dairy_barn").Cost;
        Check(result, ranch.UpgradeFacility("dairy_barn", money), id + ": ordinary starting money pays for the barn");
        long upgradeSpend = barnPrice, mealSpend = 0, settlements = 0;
        var home = 0;
        if (upgrade)
        {
            var cost = ranch.InspectFacilityUpgrade("kitchen").Cost;
            Check(result, ranch.UpgradeFacility("kitchen", money), id + ": kitchen level two is a real paid investment");
            upgradeSpend += cost;
        }
        var dairy = state.Roster.Characters.OrderByDescending(c => ranch.PreviewJobOutput(c, data.Jobs["dairy"]).Amount).First().Id;
        for (var index = 0; index < 14; index++)
        {
            // A fixed, documented policy, not an optimizer. No injected gold/skills or changed weather rules.
            foreach (var c in state.Roster.Characters)
                schedule.AssignJob(c.Id, c.Id == dairy ? "dairy" : useKitchen
                    && state.Ranch.Stockpile.GetValueOrDefault("meals") < state.Roster.Characters.Count * 2 ? "kitchen" : "office");
            var plan = new RanchProvisioningService(state, data).InspectLunch();
            if (plan.MissingMeals > 0)
            {
                var cost = (long)plan.MissingMeals * data.Items["meal_box"].Price;
                if (cost <= money.Gold && shop.Buy("meal_box", plan.MissingMeals)) mealSpend += cost;
            }
            state.Calendar.Phase = DayPhase.Night; state.Calendar.NightAction = "rest";
            var opening = money.Gold;
            var service = MakeSettlement(state, data); var report = service.SettleDay();
            var facts = service.LastWorkFacts!;
            home += facts.Lunch.PantryMeals;
            settlements += report.NetGold;
            Check(result, report.Day == index + 1 && state.Calendar.Day == index + 2
                && report.NetGold == (long)money.Gold - opening && state.Ranch.Stockpile.Values.All(v => v >= 0),
                id + $": day {report.Day} conserves stock and reconciles the actual wallet");
            result.Lines.Add($"BALANCE {id} day={report.Day} wallet={money.Gold} pantry={state.Ranch.Stockpile.GetValueOrDefault("meals")} home={facts.Lunch.PantryMeals} boxed={facts.Lunch.MealBoxes} missing={facts.Lunch.MissingMeals} work={facts.WorkGold} upkeep={facts.OperatingExpenses} full_net={report.NetGold}");
        }
        Check(result, (long)money.Gold == initialGold - upgradeSpend - mealSpend + settlements,
            id + ": all fourteen days include construction, purchased lunches and actual event/milestone payments");
        result.Lines.Add($"BALANCE TOTAL {id} wallet={money.Gold} meal_spend={mealSpend} upgrade_spend={upgradeSpend} home_meals={home}");
        return new(money.Gold, mealSpend, upgradeSpend, home);
    }

    private static DailySettlementService MakeSettlement(SaveState state, DataRegistry data)
    {
        var money = new EconomyService(state); var talents = new TalentService(state, data);
        return new(state, data, new ScheduleService(state, data),
            new RanchService(state, data, new EquipmentService(state, data), talents), money,
            new DayCycleService(state), new MilestoneService(state, data, money), new InventoryService(state), talents);
    }
}
