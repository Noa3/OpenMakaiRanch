using System;
using System.Linq;
using System.Text.Json;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.Tests;

public static class RanchLeisureRegressionTests
{
    public static void Run(SmokeTestResult result)
    {
        TestTransactions(result);
        TestRecoveryBounds(result);
        TestRootCommands(result);
    }

    private static (SaveState state, FlagService flags, RanchLeisureService service) Fixture()
    {
        var state = new SaveState();
        state.Calendar.Day = 2;
        state.Story.FirstDayCompleted = true;
        state.WorldAreaId = "ranch";
        state.Economy.Gold = 100;
        state.Ranch.Stockpile["supplies"] = 3;
        var flags = new FlagService();
        var service = new RanchLeisureService(state, flags, new EconomyService(state), new PlayerStaminaService(state));
        return (state, flags, service);
    }

    private static void TestTransactions(SmokeTestResult r)
    {
        var (s, f, service) = Fixture();
        var before = JsonSerializer.Serialize(s);
        var status = service.GetStatus();
        Check(r, status.CanRestore && !status.Restored && !status.CanRest, "unrestored corner offers construction but no recovery");
        Check(r, service.GetStatus() == status && before == JsonSerializer.Serialize(s) && f.GlobalFlagCount == 0,
            "inspecting the corner is deterministic and changes no state or flags");
        s.Calendar.Day = 1;
        Check(r, !service.TryRestore(1, out _) && !service.TryRest(1, out _), "first-day actions are rejected");
        s.Calendar.Day = 2;
        s.Story.FirstDayCompleted = false;
        Check(r, !service.TryRestore(2, out _), "unfinished introduction cannot be bypassed by a day value");
        s.Story.FirstDayCompleted = true;
        s.WorldAreaId = "town";
        Check(r, !service.TryRestore(2, out _), "corner cannot be restored from another area");
        s.WorldAreaId = "ranch";
        Check(r, !service.TryRestore(3, out _), "stale day rejects construction");
        s.Ranch.Stockpile["supplies"] = 2;
        Check(r, !service.TryRestore(2, out _) && s.Economy.Gold == 100 && s.Ranch.Stockpile["supplies"] == 2,
            "supply shortage does not deduct gold or partial stock");
        s.Ranch.Stockpile["supplies"] = 3;
        s.Economy.Gold = 39;
        Check(r, !service.TryRestore(2, out _) && s.Ranch.Stockpile["supplies"] == 3 && !service.IsRestored,
            "gold shortage preserves supplies and construction state");
        s.Economy.Gold = 100;
        var stamina = s.Player.Stamina;
        var phase = s.Calendar.Phase;
        var income = s.Economy.LastIncome;
        Check(r, service.TryRestore(2, out var message) && message.Length > 0, "restoration returns a receipt");
        Check(r, service.IsRestored && s.Economy.Gold == 60 && s.Ranch.Stockpile["supplies"] == 0,
            "construction charges exactly 40 G and three supplies once");
        Check(r, s.Player.Stamina == stamina && s.Calendar.Day == 2 && s.Calendar.Phase == phase && s.Economy.LastIncome == income,
            "restoration does not charge stamina, advance time or alter settlement income");
        Check(r, !service.TryRestore(2, out _) && s.Economy.Gold == 60, "repeat restoration cannot charge a second time");
        s.Player.Stamina = s.Player.MaxStamina;
        Check(r, !service.TryRest(2, out _) && f.GetGlobalIntFlag(RanchLeisureService.LastRestDayFlag) == 0,
            "full stamina does not consume the daily break");
        s.Player.Stamina = 80;
        Check(r, service.TryRest(2, out _) && s.Player.Stamina == 90, "a break restores ten stamina");
        Check(r, !service.TryRest(2, out _) && s.Player.Stamina == 90, "a second break cannot refill again");
        s.Calendar.Phase = DayPhase.Evening;
        Check(r, !service.TryRest(2, out _), "advancing a phase does not reset daily recovery");
        f.SyncToStorage(s.Flags);
        var restoredFlags = new FlagService();
        restoredFlags.SyncFromStorage(JsonSerializer.Deserialize<FlagStorage>(JsonSerializer.Serialize(s.Flags))!);
        var reloaded = new RanchLeisureService(s, restoredFlags, new EconomyService(s), new PlayerStaminaService(s));
        Check(r, reloaded.IsRestored && !reloaded.TryRest(2, out _), "serialized flags retain improvement and daily receipt");
        Check(r, f.GlobalFlagCount == 2, "storage uses exactly two bounded flags");
    }

    private static void TestRecoveryBounds(SmokeTestResult r)
    {
        var (s, f, service) = Fixture();
        service.TryRestore(2, out _);
        s.Player.Stamina = 99;
        Check(r, service.TryRest(2, out _) && s.Player.Stamina == 100, "partial refill stops at the current capacity");
        s.Player.Stamina = 90;
        Check(r, !service.TryRest(2, out _), "a partial refill still uses one daily break");
        s.Calendar.Day = 3;
        s.Player.DailyStaminaBonus = 25;
        s.Player.NextDayStaminaBonus = 25;
        s.Player.BathedToday = true;
        s.Player.Stamina = 120;
        Check(r, service.TryRest(3, out _) && s.Player.Stamina == 125, "recovery respects the existing Well Rested capacity");
        Check(r, s.Player.NextDayStaminaBonus == 25 && s.Player.DailyStaminaBonus == 25 && s.Player.BathedToday,
            "corner recovery neither grants nor consumes the separate bath bonus");
        s.Calendar.Day = 20;
        s.Player.Stamina = 0;
        Check(r, service.TryRest(20, out _) && s.Player.Stamina == 10, "skipped days accumulate no recovery backlog");
        Check(r, f.GlobalFlagCount == 2 && s.Economy.Gold == 60, "later days add neither upkeep nor growing history");
        f.SetGlobalIntFlag(RanchLeisureService.LastRestDayFlag, 30);
        Check(r, !service.TryRest(20, out _), "a future receipt fails closed instead of paying again");
        s.Calendar.Day = 31;
        s.Player.MaxStamina = int.MaxValue;
        Check(r, !service.GetStatus().CanRest && !service.TryRest(31, out _), "invalid overflowing stamina capacity rejects without normalization");
    }

    private static void PrepareRoot(GameRoot game)
    {
        game.NewGame();
        game.State.Calendar.Day = 2;
        game.State.Story.FirstDayCompleted = true;
        game.State.WorldAreaId = "ranch";
        game.State.Economy.Gold = 100;
        game.State.Ranch.Stockpile["supplies"] = 3;
        game.State.Player.Stamina = 70;
    }

    private static void TestRootCommands(SmokeTestResult r)
    {
        var game = GameRoot.Instance;
        var settings = game.State.Settings.Clone();
        try
        {
            PrepareRoot(game);
            var g = game.StateGeneration;
            var phase = game.State.Calendar.Phase;
            Check(r, !game.TryRestoreRanchCorner(2, phase, g - 1, out _), "root rejects stale generation");
            Check(r, !game.TryRestoreRanchCorner(2, DayPhase.Night, g, out _), "root rejects a stale phase");
            Check(r, !game.TryRestoreRanchCorner(3, phase, g, out _), "root rejects a stale day");
            game.BeginCombatSession();
            Check(r, !game.TryRestoreRanchCorner(2, phase, g, out _), "root rejects construction during combat");
            game.EndCombatSession();
            Check(r, !game.IsRanchCornerRestored && game.Economy.Gold == 100 && game.State.Ranch.Stockpile["supplies"] == 3,
                "rejected root commands leave all costs and completion untouched");
            var notifications = 0;
            var duplicate = true;
            void OnRestore()
            {
                notifications++;
                duplicate = game.TryRestoreRanchCorner(2, phase, g, out _);
            }
            game.StateChanged += OnRestore;
            try { Check(r, game.TryRestoreRanchCorner(2, phase, g, out _), "root construction succeeds"); }
            finally { game.StateChanged -= OnRestore; }
            Check(r, notifications == 1 && !duplicate && game.Economy.Gold == 60,
                "completion is recorded before notification, preventing reentrant construction");
            notifications = 0;
            void OnRest()
            {
                notifications++;
                duplicate = game.TryRestAtRanchCorner(2, phase, g, out _);
            }
            game.StateChanged += OnRest;
            try { Check(r, game.TryRestAtRanchCorner(2, phase, g, out _), "root recovery succeeds"); }
            finally { game.StateChanged -= OnRest; }
            Check(r, notifications == 1 && !duplicate && game.State.Player.Stamina == 80,
                "recovery receipt prevents a reentrant second refill");
            Check(r, !game.CanShareRanchCorner(out _), "solo corner is usable without enabling unavailable companionship");

            // Isolated synthetic character for a numeric, non-explicit activity contract. This is
            // not an eligibility/design approval of any shipped or original character.
            var partner = new CharacterState
            {
                Id = "synthetic_corner_adult", DisplayNameOverride = "Contract companion", ApparentAge = 30,
                AdultEligibility = AdultEligibility.ConfirmedAdult, Bond = 30, Morale = 60, Fatigue = 20
            };
            game.State.Roster.Characters.Add(partner);
            Check(r, game.StartDate(partner.Id, DateInviteApproach.Respectful).Success, "synthetic willing companion can start the existing outing");
            var stamina = game.State.Player.Stamina;
            Check(r, !game.TryShareRanchCorner("stale_partner", 2, phase, g, out _) && game.State.Player.Stamina == stamina,
                "stale companion cannot spend stamina or modify a different relationship");
            game.State.Dating.ActiveApproach = DateInviteApproach.Pressured;
            Check(r, !game.TryShareRanchCorner(partner.Id, 2, phase, g, out _), "quiet-corner interaction requires a voluntary outing");
            game.State.Dating.ActiveApproach = DateInviteApproach.Respectful;
            var bond = partner.Bond;
            Check(r, game.TryShareRanchCorner(partner.Id, 2, phase, g, out _)
                && game.State.Player.Stamina == stamina - game.Dating.ActivityCost(DateActivityKind.QuietRest)
                && partner.Bond > bond, "shared moment delegates to the existing activity cost and positive outcome");
            Check(r, !game.TryShareRanchCorner(partner.Id, 2, phase, g, out _)
                && !game.Dating.CanPerformActivity(DateActivityKind.QuietRest, out _),
                "world and management share the same one-activity-per-phase limit");
            game.State.Calendar.Phase = DayPhase.Afternoon;
            partner.AdultEligibility = AdultEligibility.Unknown;
            Check(r, !game.TryShareRanchCorner(partner.Id, 2, DayPhase.Afternoon, g, out _),
                "existing eligibility denial remains effective at the new entry point");
            game.EndDate();
            game.State.Roster.Characters.Remove(partner);
            Check(r, game.SaveSlot(99), "real root save writes restoration and daily receipt");
            var gold = game.Economy.Gold;
            var savedStamina = game.State.Player.Stamina;
            game.NewGame();
            Check(r, !game.IsRanchCornerRestored, "fresh session does not inherit the prior improvement");
            Check(r, game.LoadSlot(99) && game.IsRanchCornerRestored && game.Economy.Gold == gold
                && game.State.Player.Stamina == savedStamina, "current-version root load restores the complete transaction");
            Check(r, !game.TryRestAtRanchCorner(2, game.State.Calendar.Phase, game.StateGeneration, out _),
                "loading a completed daily recovery cannot grant another");
            Check(r, !game.TryRestoreRanchCorner(2, game.State.Calendar.Phase, g, out _),
                "pre-load callbacks cannot affect the loaded session");
        }
        finally
        {
            game.EndCombatSession();
            game.Save.Delete(99);
            game.State.Settings = settings;
            game.NewGame();
        }
    }

    internal static void Check(SmokeTestResult result, bool passed, string message)
    {
        result.Passed &= passed;
        result.Lines.Add($"SMOKE {(passed ? "OK" : "FAIL")} ranch leisure: {message}");
    }
}
