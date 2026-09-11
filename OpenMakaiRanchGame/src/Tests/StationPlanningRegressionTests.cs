using System;
using System.Linq;
using System.Text.Json;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.Tests;

/// <summary>Numeric and command-boundary checks; no earned balance or navigation claim.</summary>
public static class StationPlanningRegressionTests
{
    public static void Run(SmokeTestResult result)
    {
        void Check(bool pass, string message)
        { result.Passed &= pass; result.Lines.Add($"SMOKE {(pass ? "OK" : "FAIL")} station planning: {message}"); }
        var data = DataRegistry.CreateSeeded();
        var state = new SaveState();
        var ranch = new RanchService(state, data, new EquipmentService(state, data), new TalentService(state, data));
        var economy = new EconomyService(state);
        var definition = data.Facilities["kitchen"];
        state.Ranch.Facilities.Clear(); state.Ranch.Facilities["kitchen"] = 0;
        Check(ranch.FacilityUpkeep() == 0, "an explicitly unbuilt level-zero entry charges no facility upkeep");
        state.Ranch.Facilities["kitchen"] = -1;
        Check(ranch.FacilityUpkeep() == 0, "invalid non-positive level never becomes a phantom maintained building");
        state.Ranch.Facilities["kitchen"] = 2;
        Check(ranch.FacilityUpkeep() == definition.UpkeepGold * 2, "upkeep uses the actual positive building level");
        state.Ranch.Facilities["workshop"] = 1;
        state.Research.UnlockedSkillIds.Add("logistics");
        var raw = definition.UpkeepGold * 2 + data.Facilities["workshop"].UpkeepGold;
        Check(ranch.FacilityUpkeep() == raw - Math.Max(1, raw / 4), "logistics discount is rounded once on the entire facility bill");
        state.Economy.Gold = 10000;
        var snapshot = JsonSerializer.Serialize(state);
        for (var i = 0; i < 50; i++) ranch.InspectFacilityUpgrade("kitchen");
        Check(JsonSerializer.Serialize(state) == snapshot, "repeated price and upkeep previews are read-only");
        var quote = ranch.InspectFacilityUpgrade("kitchen");
        Check(quote.Level == 2 && quote.NextLevel == 3 && quote.Cost == definition.BuildCost + 150
            && quote.BaseUpkeepBefore == definition.UpkeepGold * 2 && quote.BaseUpkeepAfter == definition.UpkeepGold * 3,
            "quote exposes exact level, one-time cost and local before/after upkeep");
        Check(ranch.UpgradeFacility("kitchen", economy) && ranch.FacilityUpkeep() == quote.RanchUpkeepAfter
            && state.Economy.Gold == 10000 - quote.Cost, "successful upgrade matches the previewed discounted ranch bill and listed payment");
        state.Economy.Gold = 0; snapshot = JsonSerializer.Serialize(state);
        Check(!ranch.InspectFacilityUpgrade("kitchen").CanUpgrade && !ranch.UpgradeFacility("kitchen", economy)
            && JsonSerializer.Serialize(state) == snapshot, "unaffordable upgrade cannot make a partial payment or level change");
        Check(!ranch.InspectFacilityUpgrade(null).CanUpgrade && !ranch.InspectFacilityUpgrade("missing").CanUpgrade,
            "unknown upgrade targets return an unavailable offer");
        state.Economy.Gold = int.MaxValue; state.Ranch.Facilities["kitchen"] = int.MaxValue;
        Check(!ranch.InspectFacilityUpgrade("kitchen").CanUpgrade, "extreme level is disabled even with a full wallet");
        Check(ranch.FacilityUpkeep() == int.MaxValue, "extreme multiplied upkeep saturates rather than wrapping negative");
        state.Ranch.Facilities["workshop"] = int.MaxValue;
        Check(ranch.FacilityUpkeep() == int.MaxValue, "multiple extreme buildings cannot wrap the total after logistics");
        // Independent settlement fixture: no workers or automation, isolate only the expense total.
        var settlementState = DaySettlementRegressionTests.Fixture(data, 0, "rest");
        settlementState.Ranch.Facilities.Clear(); settlementState.Ranch.Facilities["kitchen"] = int.MaxValue;
        settlementState.Economy.Gold = 1000;
        var report = DaySettlementRegressionTests.Settle(settlementState, data);
        Check(settlementState.Economy.Gold < 0 && report.Expenses == int.MaxValue,
            "adding the missing-dairy penalty to saturated upkeep cannot turn the entire day free");

        var game = GameRoot.Instance;
        try
        {
            game.NewGame(); game.State.Calendar.Day = 2; game.State.Calendar.Phase = DayPhase.Morning;
            game.State.Story.FirstDayCompleted = true;
            game.State.Economy.Gold = 10000;
            var generation = game.StateGeneration; var day = game.State.Calendar.Day; var phase = game.State.Calendar.Phase;
            var rootQuote = game.Ranch.InspectFacilityUpgrade("kitchen");
            var before = JsonSerializer.Serialize(game.State);
            Check(!game.TryUpgradeFacility(rootQuote, generation - 1, day, phase).Success
                && !game.TryUpgradeFacility(rootQuote, generation, day - 1, phase).Success
                && !game.TryUpgradeFacility(rootQuote, generation, day, DayPhase.Night).Success
                && before == JsonSerializer.Serialize(game.State), "stale generation, day and phase cannot purchase an upgrade");
            var resident = game.Roster.Characters.First().Id;
            var oldJob = game.Schedule.GetAssignment(resident);
            Check(!game.TryPlanStationJob(resident, "office", "", "not_the_old_job", false, generation, day, phase).Success
                && before == JsonSerializer.Serialize(game.State), "an assignment preview cannot overwrite a job changed since it was shown");
            game.State.Ranch.Facilities["workshop"] = 0; before = JsonSerializer.Serialize(game.State);
            Check(!game.TryPlanStationJob(resident, "workshop", "workshop", oldJob, false, generation, day, phase).Success
                && before == JsonSerializer.Serialize(game.State), "unbuilt work cannot be assigned through the root planning boundary");
            var reentrant = false; var advanced = false; var jobChanged = false; var notifications = 0;
            void Reenter()
            {
                if (++notifications > 1) return; // Bound a regression rather than recursing indefinitely.
                reentrant |= game.TryUpgradeFacility(game.Ranch.InspectFacilityUpgrade("kitchen"), generation, day, phase).Success;
                advanced |= game.AdvanceTime();
                jobChanged |= game.TryAssignJob(resident, oldJob == "office" ? "rest" : "office", generation);
            }
            rootQuote = game.Ranch.InspectFacilityUpgrade("kitchen");
            game.StateChanged += Reenter;
            try
            {
                Check(game.TryUpgradeFacility(rootQuote, generation, day, phase).Success,
                    "current displayed upgrade succeeds through the root boundary");
            }
            finally { game.StateChanged -= Reenter; }
            Check(!reentrant && !advanced && !jobChanged && game.State.Calendar.Day == day && game.State.Calendar.Phase == phase,
                "upgrade observers cannot immediately buy another level, change a job or advance time");
            before = JsonSerializer.Serialize(game.State);
            Check(!game.TryUpgradeFacility(rootQuote, generation, day, phase).Success && before == JsonSerializer.Serialize(game.State),
                "replaying the successful quote cannot purchase the next, more expensive level");
            rootQuote = game.Ranch.InspectFacilityUpgrade("kitchen");
            game.State.Research.UnlockedSkillIds.Add("logistics"); before = JsonSerializer.Serialize(game.State);
            Check(!game.TryUpgradeFacility(rootQuote, generation, day, phase).Success && before == JsonSerializer.Serialize(game.State),
                "a research change invalidates the previously displayed running-cost projection");
            var money = game.Economy.Gold; var stock = JsonSerializer.Serialize(game.State.Ranch.Stockpile);
            var stamina = game.State.Player.Stamina;
            Check(game.TryPlanStationJob(resident, "office", "", oldJob, false, generation, day, phase).Success
                && game.Schedule.GetAssignment(resident) == "office" && game.Economy.Gold == money
                && game.State.Player.Stamina == stamina && stock == JsonSerializer.Serialize(game.State.Ranch.Stockpile),
                "work planning changes only the canonical job, not output or stamina");
            before = JsonSerializer.Serialize(game.State);
            Check(!game.TryPlanStationJob(resident, "pasture", "", "office", true, generation, day, phase).Success
                && before == JsonSerializer.Serialize(game.State), "a station cannot release a worker assigned somewhere else");
            Check(game.TryPlanStationJob(resident, "office", "", "office", true, generation, day, phase).Success
                && game.Schedule.GetAssignment(resident) == "rest", "a current worker can be sent to rest without extra payment");
        }
        finally { game.NewGame(); }
    }
}
