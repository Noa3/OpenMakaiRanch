using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Data;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.Tests;

public static class DayCommandRegressionTests
{
    public static void Run(SmokeTestResult result, DataRegistry data)
    {
        var game = GameRoot.Instance;
        try
        {
            game.NewGame();
            game.State.Calendar.Day = 2;
            game.State.Calendar.Phase = DayPhase.Night;
            var generation = game.StateGeneration;
            var gold = game.Economy.Gold;
            var stamina = game.State.Player.Stamina;
            var workload = game.State.Ranch.Workload;
            var before = game.Roster.Characters.Select(c => (c.Energy, c.Fatigue, c.Morale, c.Bond)).ToArray();
            var notifications = 0;
            void OnChanged() => notifications++;
            game.StateChanged += OnChanged;
            try
            {
                Check(result, game.TrySelectNightAction("rest", generation, 2)
                    && game.TrySelectNightAction("train", generation, 2)
                    && game.TrySelectNightAction("admin", generation, 2),
                    "a current nightly plan can be revised through all three choices before settlement");
                Check(result, notifications == 3 && game.State.Calendar.NightAction == "admin"
                    && gold == game.Economy.Gold && stamina == game.State.Player.Stamina && workload == game.State.Ranch.Workload
                    && before.SequenceEqual(game.Roster.Characters.Select(c => (c.Energy, c.Fatigue, c.Morale, c.Bond))),
                    "planning notifies once per change without applying recovery, training, costs or workload twice");
                Check(result, !game.TrySelectNightAction("admin", generation, 2)
                    && !game.TrySelectNightAction("unknown", generation, 2)
                    && !game.TrySelectNightAction("rest", generation, 1)
                    && !game.TrySelectNightAction("rest", generation + 1, 2) && notifications == 3,
                    "identical, unknown, stale-day and stale-session choices do not mutate or notify");
            }
            finally { game.StateChanged -= OnChanged; }
            game.BeginCombatSession();
            Check(result, !game.TrySelectNightAction("rest", generation, 2)
                && !game.TryAdvanceTime(generation, 2, DayPhase.Night), "combat keeps night choices and time blocked");
            game.EndCombatSession();
            var bath = game.UsePlayerBath();
            Check(result, bath.Used && bath.UsedCleanBath && game.State.Calendar.NightAction == "admin"
                && game.State.Player.NextDayStaminaBonus == PlayerStaminaService.HotBathNextDayBonus,
                "a prepared night bath preserves the selected Admin plan and its separate next-morning bonus");
            var chosen = game.State.Calendar.NightAction;
            Check(result, !game.TryAdvanceTime(generation, 1, DayPhase.Night)
                && !game.TryAdvanceTime(generation, 2, DayPhase.Evening)
                && !game.TryAdvanceTime(generation + 1, 2, DayPhase.Night)
                && game.State.Calendar.NightAction == chosen, "time commands reject the wrong day, phase or session");
            Check(result, game.TryAdvanceTime(generation, 2, DayPhase.Night)
                && !game.TryAdvanceTime(generation, 2, DayPhase.Night)
                && game.State.Calendar.Day == 3 && game.State.Calendar.Phase == DayPhase.Morning,
                "replaying the accepted night command cannot advance tomorrow or pay again");

            game.NewGame();
            game.State.Calendar.Day = 2;
            game.State.Calendar.Phase = DayPhase.Night;
            Check(result, !game.TrySelectNightAction("rest", generation, 2)
                && !game.TryAdvanceTime(generation, 2, DayPhase.Night),
                "a new session on the same displayed day still rejects the old command");
            game.SetNightAction("rest");
            var oldState = game.State;
            void ReplaceSession(DailyReport _) => game.NewGame();
            game.DaySettled += ReplaceSession;
            DailyReport completed;
            try { completed = game.EndDay(); }
            finally { game.DaySettled -= ReplaceSession; }
            Check(result, !ReferenceEquals(game.State, oldState) && completed.Day == 2
                && oldState.Reports.Contains(completed) && game.State.Calendar.Day == 1
                && game.State.Reports.Count == 0 && game.LastDailyReport is null,
                "a completion observer replacing the session cannot attach the old report to the new game");
            Check(result, game.AdvanceTime(), "the settlement guard releases after an observer replaces the session");
        }
        finally { game.NewGame(); }

        var state = DaySettlementRegressionTests.Fixture(data, 1, "rest");
        state.Roster.Characters[0].HasGrownToday = true;
        state.Schedule.AssignedJobs[state.Roster.Characters[0].Id] = "rest";
        DaySettlementRegressionTests.Settle(state, data);
        Check(result, !state.Roster.Characters[0].HasGrownToday, "a new day clears yesterday's growth mark even without work");

        var trainee = DaySettlementRegressionTests.Fixture(data, 4, "train");
        trainee.Schedule.AssignedJobs[trainee.Roster.Characters[0].Id] = "rest";
        DaySettlementRegressionTests.Settle(trainee, data);
        Check(result, trainee.Roster.Characters[0].SkillXp.GetValueOrDefault("ranch") == 0
            && trainee.Roster.Characters.Skip(1).All(c => c.SkillXp.GetValueOrDefault("ranch") == 10),
            "night training retains ordinary rest-job exclusion rather than granting every resident XP");

        foreach (var opening in new[] { 100, int.MaxValue, int.MinValue })
        {
            var wallet = new SaveState();
            wallet.Economy.Gold = opening;
            var economy = new EconomyService(wallet);
            var ledger = new DailyGoldLedger(opening);
            economy.ApplySettlement(opening == int.MinValue ? 0 : 70, 15);
            ledger.RecordWorkAndUpkeep(opening == int.MinValue ? 0 : 70, 15, economy.Gold);
            var start = economy.Gold;
            economy.AddGold(40);
            ledger.RecordChange(start, economy.Gold);
            var report = new DailyReport();
            ledger.Complete(report, wallet.Economy);
            Check(result, report.NetGold == (long)economy.Gold - opening && report.Income >= 0 && report.Expenses >= 0
                && (long)report.Income - report.Expenses == report.NetGold,
                $"ledger at opening {opening} reports applied bounded payments without changing the wallet");
        }

        var milestoneState = DaySettlementRegressionTests.Fixture(data, 0, "rest");
        milestoneState.Calendar.Day = 300;
        milestoneState.Milestones.CompletedIds.Clear();
        milestoneState.Economy.Gold = int.MaxValue;
        var milestoneEconomy = new EconomyService(milestoneState);
        var milestoneReport = new DailyReport();
        new MilestoneService(milestoneState, data, milestoneEconomy).CheckAfterSettlement(milestoneReport);
        Check(result, milestoneState.Milestones.CompletedIds.Count > 0 && milestoneReport.Income == 0 && milestoneReport.NetGold == 0
            && milestoneEconomy.Gold == int.MaxValue, "capped milestone receipts contain only credited gold, not phantom income");
        Check(result, milestoneReport.Lines.All(line => !line.Contains("gold") || line.Contains("(+0 gold credited)")),
            "milestone messages agree with the actual credit at the wallet limit");
    }

    private static void Check(SmokeTestResult result, bool passed, string message) =>
        DaySettlementRegressionTests.Check(result, passed, message);
}
