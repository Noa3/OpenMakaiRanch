using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.Tests;

/// <summary>Rule parity and non-blocking completion fixtures, not earned campaign pacing evidence.</summary>
public static class OptimizationRegressionTests
{
    public static void Run(SmokeTestResult result)
    {
        void Check(bool value, string text)
        { result.Passed &= value; result.Lines.Add($"SMOKE {(value ? "OK" : "FAIL")} optimization: {text}"); }
        var data = new DataRegistry();
        var state = new SaveState();
        var worker = new CharacterState { Id = "forecast_worker", RanchSkill = 6, CombatSkill = 10, Fatigue = 60,
            Hp = 100, Energy = 100, EquippedItems = new Dictionary<string, string> { ["weapon"] = "tool" },
            Talents = new List<string> { "artisan" } };
        state.Roster.Characters.Add(worker);
        data.Items["tool"] = new ItemDefinition { Id = "tool", Category = ItemCategory.Equipment,
            Slot = EquipmentSlot.Weapon, BonusRanchSkill = 2, BonusCombatSkill = 4 };
        data.Items["replacement"] = new ItemDefinition { Id = "replacement", Category = ItemCategory.Equipment, Slot = EquipmentSlot.Weapon };
        data.Talents["artisan"] = new TalentDefinition { Id = "artisan", BonusRanchSkill = 3,
            BonusCombatSkill = 5, JobOutputMultiplier = 1.25f };
        state.Research.UnlockedSkillIds.AddRange(new[] { "ranch_planning", "dairy_science", "culinary_arts", "herbalism", "hospitality", "craftsmanship" });
        var equipment = new EquipmentService(state, data);
        var ranch = new RanchService(state, data, equipment, new TalentService(state, data));
        // Hand-calculated from the pre-refactor rule: (4 + floor(11/2) + 2) -> talent13
        // -> fatigue9; gross37, then each category's separate specialist rule. Adventure uses19/2.
        foreach (var (category, amount, gold) in new[] {
            (JobCategory.RanchWork, 9, 37), (JobCategory.Dairy, 13, 53), (JobCategory.Cooking, 13, 37),
            (JobCategory.Pharmacy, 13, 57), (JobCategory.CustomerService, 14, 52),
            (JobCategory.Chore, 13, 49), (JobCategory.Adventure, 13, 49) })
        {
            var job = new JobDefinition { Id = "preview", Category = category, ResourceId = "output", ResourceAmount = 4, GoldIncome = 10 };
            var before = JsonSerializer.Serialize(state);
            var estimate = ranch.PreviewJobOutput(worker, job);
            Check(estimate.Amount == amount && estimate.Gold == gold && before == JsonSerializer.Serialize(state),
                category + ": read-only estimate preserves the hand-calculated original output and gross pay");
            var existing = state.Ranch.Stockpile.GetValueOrDefault("output");
            Check(ranch.ApplyJobOutput(worker, job, new DailyReport()) == gold
                && state.Ranch.Stockpile["output"] == existing + amount, category + ": committed work agrees with the unchanged rule");
        }
        var ordinary = new JobDefinition { Id = "ordinary", Category = JobCategory.RanchWork, ResourceId = "output", ResourceAmount = 4, GoldIncome = 10 };
        foreach (var (fatigue, expected) in new[] { (0, 13), (39, 13), (40, 11), (59, 11), (60, 9), (79, 9), (80, 6), (100, 6) })
        {
            worker.Fatigue = fatigue;
            Check(ranch.PreviewJobOutput(worker, ordinary).Amount == expected, $"fatigue {fatigue}: the existing rounding/condition threshold remains transparent");
        }
        worker.Fatigue = 0;
        var low = ranch.PreviewJobOutput(worker, ordinary);
        worker.RanchSkill = 12;
        var high = ranch.PreviewJobOutput(worker, ordinary);
        Check(high.Amount > low.Amount && high.Gold > low.Gold, "better practical skill really improves output, without pace-based penalties");
        state.Calendar.Day = 500;
        Check(high == ranch.PreviewJobOutput(worker, ordinary), "the same plan has the same return early or late in the calendar");
        worker.Mature.FallState = FallState.Collapse;
        var snap = JsonSerializer.Serialize(state);
        Check(ranch.PreviewJobOutput(worker, ordinary) is { IsCollapsed: true, Amount: 0, Gold: 0 }
            && snap == JsonSerializer.Serialize(state), "collapsed-worker inspection does not pay or heal");
        worker.Mature.FallState = default;
        Check(ranch.PreviewJobOutput(worker, new JobDefinition()) is { IsRest: true, Amount: 0, Gold: 0 }, "rest is not an invented income source");
        worker.EquippedItems = null!; snap = JsonSerializer.Serialize(state);
        for (var i = 0; i < 20; i++) ranch.PreviewJobOutput(worker, ordinary);
        Check(snap == JsonSerializer.Serialize(state) && worker.EquippedItems is null, "previewing a missing equipment map never repairs or mutates a save");
        worker.EquippedItems = new Dictionary<string, string> { ["weapon"] = "tool" };
        snap = JsonSerializer.Serialize(state);
        var denied = true;
        for (var i = 0; i < 20; i++) denied &= !equipment.Equip(worker.Id, "replacement");
        Check(denied, "twenty unavailable replacement requests are all rejected");
        Check(snap == JsonSerializer.Serialize(state) && !state.Inventory.Items.ContainsKey("tool"), "failed equipment swaps cannot duplicate the equipped tool");
        state.Inventory.Items["replacement"] = 1;
        Check(equipment.Equip(worker.Id, "replacement") && worker.EquippedItems["weapon"] == "replacement"
            && !state.Inventory.Items.ContainsKey("replacement") && state.Inventory.Items["tool"] == 1,
            "a real owned replacement swaps once and returns exactly one old tool");
        state.Inventory.Items["tool"] = 1; state.Inventory.Items["replacement"] = int.MaxValue;
        snap = JsonSerializer.Serialize(state);
        Check(!equipment.Equip(worker.Id, "tool") && snap == JsonSerializer.Serialize(state), "a swap that cannot return its old item is rejected before consumption");

        // Economy-only baseline: normal starting resources, real purchases/construction and
        // seven settlements, without tutorial staging, battle rewards or injected gold/stats.
        var weekData = DataRegistry.CreateSeeded();
        var week = new SaveStateFactory(weekData, new Random(41)).CreateNewGame();
        var weekMoney = new EconomyService(week);
        var weekRanch = new RanchService(week, weekData, new EquipmentService(week, weekData), new TalentService(week, weekData));
        if (week.Ranch.Facilities.GetValueOrDefault("dairy_barn") == 0)
            Check(weekRanch.UpgradeFacility("dairy_barn", weekMoney), "starting funds build the ordinary dairy before the economic baseline");
        var schedule = new ScheduleService(week, weekData);
        var dairyWorker = week.Roster.Characters.OrderByDescending(c => weekRanch.PreviewJobOutput(c, weekData.Jobs["dairy"]).Amount).First();
        foreach (var resident in week.Roster.Characters) schedule.AssignJob(resident.Id, resident.Id == dairyWorker.Id ? "dairy" : "office");
        var shop = new ShopService(weekData, weekMoney, new InventoryService(week));
        var firstDay = week.Calendar.Day;
        for (var i = 0; i < 7; i++)
        {
            var missingMeals = Math.Max(0, week.Roster.Characters.Count - week.Inventory.Items.GetValueOrDefault("meal_box"));
            if (missingMeals > 0 && (long)missingMeals * weekData.Items["meal_box"].Price <= weekMoney.Gold)
                Check(shop.Buy("meal_box", missingMeals), "the baseline pays the canonical meal price, without a grant");
            week.Calendar.Phase = DayPhase.Night; week.Calendar.NightAction = "rest";
            var balance = weekMoney.Gold;
            var report = DaySettlementRegressionTests.Settle(week, weekData);
            Check(report.Day == firstDay + i && week.Calendar.Day == firstDay + i + 1
                && report.NetGold == (long)weekMoney.Gold - balance,
                $"baseline day{report.Day}: one settlement reconciles wallet (income={report.Income}, expenses={report.Expenses}, wallet={weekMoney.Gold})");
        }
        Check(weekMoney.Gold >= 0 && !new WinConditionService(week, weekData).IsGameComplete(),
            "the seven-day dairy/office baseline stays solvent without exhausting the completion goals");

        var game = GameRoot.Instance;
        try
        {
            game.NewGame(); game.State.Calendar.Day = 2; game.State.Calendar.Phase = DayPhase.Night;
            game.State.Calendar.NightAction = "rest"; game.State.Story.FirstDayCompleted = true;
            var progress = game.WinCondition.InspectProgress();
            snap = JsonSerializer.Serialize(game.State);
            for (var i = 0; i < 20; i++) game.WinCondition.InspectProgress();
            Check(snap == JsonSerializer.Serialize(game.State), "completion inspection is read-only");
            foreach (var key in game.Data.Missions.Keys) if (!game.State.Adventure.DiscoveredMissionIds.Contains(key)) game.State.Adventure.DiscoveredMissionIds.Add(key);
            foreach (var key in game.Data.Skills.Keys) if (!game.State.Research.UnlockedSkillIds.Contains(key)) game.State.Research.UnlockedSkillIds.Add(key);
            foreach (var key in game.Data.Facilities.Keys) game.State.Ranch.Facilities[key] = 5;
            foreach (var resident in game.Roster.Characters) resident.Bond = 80;
            Check(game.WinCondition.InspectProgress().Complete && game.WinCondition.IsGameComplete(), "meeting existing goals on day2 is accepted without adding a time gate");
            var catalogId = game.Data.Missions.Keys.First();
            game.State.Adventure.DiscoveredMissionIds.Remove(catalogId);
            game.State.Adventure.DiscoveredMissionIds.Add("not_a_catalog_mission");
            Check(!game.WinCondition.IsGameComplete(), "an unknown mission ID cannot substitute for actual discovery");
            game.State.Adventure.DiscoveredMissionIds.Remove("not_a_catalog_mission"); game.State.Adventure.DiscoveredMissionIds.Add(catalogId);
            var generation = game.StateGeneration;
            var notifications = 0;
            void Completed() => notifications++;
            game.GameComplete += Completed;
            try
            {
                game.EndDay();
                var recorded = game.State.VictoryDay;
                Check(recorded == 3 && notifications == 1 && !game.State.NgPlusActive && game.StateGeneration == generation,
                    "successful early completion records the new morning without restarting the run");
                for (var i = 0; i < 3; i++)
                {
                    for (var step = 0; step < 4 && game.State.Calendar.Phase != DayPhase.Night; step++) game.AdvanceTime();
                    game.TrySelectNightAction("rest", generation, game.State.Calendar.Day);
                    Check(game.AdvanceTime(), "ordinary phase/night commands remain usable after completion");
                }
                Check(game.State.Calendar.Day == 6 && game.State.VictoryDay == recorded && notifications == 1
                    && game.StateGeneration == generation && !game.State.NgPlusActive,
                    "later days neither reset the completion record nor repeatedly announce victory or force New Game+");
                var loaded = JsonSerializer.Deserialize<SaveState>(JsonSerializer.Serialize(game.State))!;
                Check(loaded.VictoryDay == recorded && loaded.Calendar.Day == 6 && !loaded.NgPlusActive,
                    "the existing save fields retain completion and the continued run together");
            }
            finally { game.GameComplete -= Completed; }
        }
        finally { game.NewGame(); }
    }
}
