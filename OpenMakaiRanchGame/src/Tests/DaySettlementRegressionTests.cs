using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Data;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.Tests;

/// <summary>Isolated numeric fixtures, not earned progression or approval of character designs.</summary>
public static class DaySettlementRegressionTests
{
    public static void Run(SmokeTestResult result)
    {
        RanchDesignRegressionTests.Run(result);
        ResidentInteractionRegressionTests.Run(result);
        StationPlanningRegressionTests.Run(result);
        var data = DataRegistry.CreateSeeded();
        foreach (var count in new[] { 1, 4, 8 })
        {
            var state = Fixture(data, count, "train");
            var report = Settle(state, data);
            Check(result, state.Roster.Characters.All(character => character.SkillXp.GetValueOrDefault("ranch") == 10),
                $"{count} workers: training grants one extra growth pass per worker, not one pass per resident");
            Check(result, state.Calendar.NightAction == string.Empty && state.Calendar.Phase == DayPhase.Morning,
                $"{count} workers: the chosen night is consumed and reaches Morning");
            Check(result, report.Day + 1 == state.Calendar.Day, $"{count} workers: settlement advances exactly one day");
        }

        var levelState = Fixture(data, 1, "train");
        var trainee = levelState.Roster.Characters[0];
        trainee.RanchSkill = 1;
        trainee.SkillXp["ranch"] = 14;
        var levelReport = Settle(levelState, data);
        Check(result, levelReport.CharacterGrowth.Any(entry => entry.CharacterId == trainee.Id) && trainee.HasGrownToday,
            "a training level-up remains marked after the ordinary daily growth pass");

        var positive = false;
        var negative = false;
        var reconciled = true;
        var totalsAgree = true;
        var examples = new List<string>();
        for (var day = 1; day <= 96; day++)
        {
            var state = Fixture(data, 0, "rest");
            state.Calendar.Day = day;
            var before = state.Economy.Gold;
            var report = Settle(state, data);
            positive |= report.Events.Any(entry => entry.GoldDelta > 0);
            negative |= report.Events.Any(entry => entry.GoldDelta < 0);
            var actual = (long)state.Economy.Gold - before;
            if (actual != report.NetGold)
            {
                reconciled = false;
                if (examples.Count < 3) examples.Add($"day {day}: report {report.NetGold}, wallet {actual}");
            }
            totalsAgree &= report.Income - report.Expenses == report.NetGold
                && state.Economy.LastIncome == report.Income && state.Economy.LastExpenses == report.Expenses;
        }
        Check(result, positive && negative, "96 seeded days actually exercise positive and paid negative gold events");
        Check(result, reconciled, "daily net includes actual event payments: " + string.Join("; ", examples));
        Check(result, totalsAgree, "daily report and overview share the same complete income and expense totals");

        var windfallDay = FindGoldEventDay(data, positive: true);
        var capped = Fixture(data, 0, "rest");
        capped.Calendar.Day = windfallDay;
        capped.Economy.Gold = int.MaxValue;
        var cappedReport = new DailyReport();
        new DailyEventService(capped, data, new EconomyService(capped)).GenerateEvents(cappedReport);
        Check(result, cappedReport.Events.Count == 1 && cappedReport.Events[0].GoldDelta == 0 && capped.Economy.Gold == int.MaxValue,
            "a capped event reports credited gold, not an amount the wallet did not receive");

        var poor = Fixture(data, 0, "rest");
        poor.Calendar.Day = FindGoldEventDay(data, positive: false);
        poor.Economy.Gold = 0;
        var poorReport = new DailyReport();
        new DailyEventService(poor, data, new EconomyService(poor)).GenerateEvents(poorReport);
        Check(result, poor.Economy.Gold == 0 && poorReport.Events.Single().GoldDelta == 0,
            "an unaffordable event does not report a payment or deduct gold");

        CheckRootCommands(result);
        DayCommandRegressionTests.Run(result, data);
    }

    private static void CheckRootCommands(SmokeTestResult result)
    {
        var game = GameRoot.Instance;
        try
        {
            game.NewGame();
            game.State.Calendar.Day = 2;
            game.State.Calendar.Phase = DayPhase.Night;
            game.State.Calendar.NightAction = string.Empty;
            var before = game.Economy.Gold;
            var accepted = game.AdvanceTime();
            Check(result, !accepted && game.State.Calendar.Day == 2 && game.Economy.Gold == before
                && game.State.Reports.Count == 0, "the root time command cannot bypass an unplanned night");

            game.NewGame();
            game.SetNightAction("train");
            Check(result, string.IsNullOrEmpty(game.State.Calendar.NightAction), "night selection cannot be preloaded during Morning");

            game.NewGame();
            game.State.Calendar.Day = 2;
            game.State.Calendar.Phase = DayPhase.Night;
            game.SetNightAction("rest");
            var notifications = 0;
            var advancedDuringNotification = false;
            DailyReport? nested = null;
            void Reenter(DailyReport report)
            {
                notifications++;
                if (notifications != 1) return; // Bound the pre-fix reproduction, never recurse indefinitely.
                nested = game.EndDay();
                advancedDuringNotification = game.AdvanceTime();
            }
            game.DaySettled += Reenter;
            try
            {
                var outer = game.EndDay();
                Check(result, notifications == 1 && game.State.Calendar.Day == 3 && game.State.Calendar.Phase == DayPhase.Morning
                    && game.State.Reports.Count == 1, "day-settled observers cannot recursively settle tomorrow");
                Check(result, !advancedDuringNotification && ReferenceEquals(nested, outer),
                    "reentrant completion returns the current report and cannot advance the new Morning");
            }
            finally { game.DaySettled -= Reenter; }
        }
        finally { game.NewGame(); }
    }

    internal static SaveState Fixture(DataRegistry data, int count, string night)
    {
        var state = new SaveState();
        state.Calendar.Day = Enumerable.Range(2, 100).First(day => new Random(day * 31 + count * 7).NextDouble() > 0.4);
        state.Calendar.Phase = DayPhase.Night;
        state.Calendar.NightAction = night;
        state.Economy.Gold = 1000;
        state.Inventory.Items["meal_box"] = 100;
        state.Milestones.CompletedIds.AddRange(data.Milestones.Keys); // Isolate event/work accounting from reward unlocks.
        for (var i = 0; i < count; i++)
        {
            var character = new CharacterState
            {
                Id = $"day_contract_{i}", DisplayNameOverride = $"Worker {i}",
                RanchSkill = 10, Energy = 60, MaxEnergyOverride = 150, Fatigue = 0, Morale = 50, Bond = 10
            };
            state.Roster.Characters.Add(character);
            state.Schedule.AssignedJobs[character.Id] = "pasture";
        }
        return state;
    }

    internal static DailyReport Settle(SaveState state, DataRegistry data)
    {
        var economy = new EconomyService(state);
        var talents = new TalentService(state, data);
        var equipment = new EquipmentService(state, data);
        var ranch = new RanchService(state, data, equipment, talents);
        return new DailySettlementService(state, data, new ScheduleService(state, data), ranch, economy,
            new DayCycleService(state), new MilestoneService(state, data, economy), new InventoryService(state), talents).SettleDay();
    }

    private static int FindGoldEventDay(DataRegistry data, bool positive)
    {
        for (var day = 1; day <= 300; day++)
        {
            var state = Fixture(data, 0, "rest");
            state.Calendar.Day = day;
            var report = new DailyReport();
            new DailyEventService(state, data, new EconomyService(state)).GenerateEvents(report);
            if (report.Events.Any(entry => positive ? entry.GoldDelta > 0 : entry.GoldDelta < 0)) return day;
        }
        throw new InvalidOperationException("No gold-event fixture found within bounded seed search.");
    }

    internal static void Check(SmokeTestResult result, bool passed, string message)
    {
        result.Passed &= passed;
        result.Lines.Add($"SMOKE {(passed ? "OK" : "FAIL")} day contract: {message}");
    }
}
