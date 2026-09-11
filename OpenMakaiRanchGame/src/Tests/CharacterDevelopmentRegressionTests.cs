using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Data;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.Tests;

public static class CharacterDevelopmentRegressionTests
{
    private static void Check(SmokeTestResult result, bool pass, string message)
    { result.Passed &= pass; result.Lines.Add($"SMOKE {(pass ? "OK" : "FAIL")} character development: {message}"); }

    public static void Run(SmokeTestResult result)
    {
        CheckSourceRules(result);
        CheckObservations(result);
        CheckPractice(result);
        CheckRoot(result);
    }

    private static void CheckSourceRules(SmokeTestResult result)
    {
        var c = new CharacterState { Id = "ward_fixture", Hp = 100, Spirit = 51, MaxSpirit = 101, Mana = 5, MaxMana = 1000 };
        c.Talents.Add("2");
        var before = JsonSerializer.Serialize(c);
        var ward = CharacterProtectionService.Inspect(c);
        Check(result, ward.WardReserveReady && JsonSerializer.Serialize(c) == before,
            "exact original talent ID reads EP above half without modifying the resident");
        c.Mana = 0;
        Check(result, CharacterProtectionService.Inspect(c).WardReserveReady, "zero MP does not exhaust a ward's EP reserve");
        c.Spirit = 50; c.Mana = 1000;
        Check(result, CharacterProtectionService.Inspect(c).Reserve == SpiritReserveState.Low,
            "EP at floor(maximum / 2) is low even with abundant mana");
        c.Spirit = 0;
        Check(result, CharacterProtectionService.Inspect(c).Reserve == SpiritReserveState.Depleted
            && c.Talents.Contains("2"), "depletion neither deletes the protection trait nor grants a bypass");
        foreach (var maximum in new[] { 0, -1 })
        {
            c.MaxSpirit = maximum;
            Check(result, CharacterProtectionService.Inspect(c).Reserve == SpiritReserveState.Unknown,
                "missing or malformed EP capacity is unknown, not permission");
        }
        c.MaxSpirit = int.MaxValue; c.Spirit = int.MaxValue;
        Check(result, CharacterProtectionService.Inspect(c).WardReserveReady, "large EP uses division, without doubled-resource overflow");
        c.Talents.Clear(); c.Talents.Add("talent_2");
        Check(result, CharacterProtectionService.Inspect(c).HasOriginalWardTrait, "the exact older numeric talent alias is recognized");
        c.Talents.Clear(); c.Talents.Add("talent_20"); c.Race = "Elf";
        Check(result, !CharacterProtectionService.Inspect(c).HasOriginalWardTrait,
            "neighbouring IDs, race and appearance do not invent protection traits");
        c.Hp = 0;
        Check(result, CharacterProtectionService.NeedsRecovery(c), "incapacity remains a recovery condition, not an unlock");
        c.Hp = 101; c.Spirit = 60; c.MaxSpirit = 100; c.Mana = 600; c.MaxMana = 1000; c.Level = 29;
        var power = OriginalCharacterRules.InspectCombatPower(c, 300, 83);
        Check(result, power == new OriginalCombatPower(true, 1100, 32, 60, true),
            "source power discounts mana at equality below level 30 and preserves both integer roundings");
        c.Level = 30;
        Check(result, OriginalCharacterRules.InspectCombatPower(c, 300, 83) == new OriginalCombatPower(true, 1100, 180, 600, false),
            "source level 30 uses full mana, without guessing an aptitude from CombatSkill");
        c.Level = 29; c.Mana = 599;
        Check(result, !OriginalCharacterRules.InspectCombatPower(c, 300, 83).ExcessManaDiscounted,
            "just below the ten-to-one ratio no mana discount applies");
        foreach (var aptitude in new[] { -1, 101 })
            Check(result, !OriginalCharacterRules.InspectCombatPower(c, 300, aptitude).Valid,
                "reference calculation rejects an unsupported aptitude percentage");
        Check(result, !OriginalCharacterRules.InspectCombatPower(c, 0, 80).Valid, "zero health capacity cannot divide by zero");
        c.Hp = int.MaxValue; c.Spirit = int.MaxValue; c.MaxSpirit = int.MaxValue;
        c.Mana = int.MaxValue; c.MaxMana = int.MaxValue; c.Level = 30;
        before = JsonSerializer.Serialize(c);
        Check(result, OriginalCharacterRules.InspectCombatPower(c, int.MaxValue, 100).CurrentPower == 4294967294L
            && before == JsonSerializer.Serialize(c), "wide source arithmetic remains exact and never rewrites the tactical character");
    }

    private static (SaveState state, CharacterState character, FlagService flags, DataRegistry data) Fixture()
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveState(); state.Calendar.Day = 2; state.Story.FirstDayCompleted = true;
        var c = new CharacterState { Id = "development_fixture", DisplayNameOverride = "Resident", Hp = 80, MaxHpOverride = 100,
            Energy = 80, MaxEnergyOverride = 100, Morale = 60, RanchSkill = 1, CraftSkill = 1, CombatSkill = 1,
            MagicPower = 2, MaxMana = 50, Mana = 17, MaxSpirit = 100, Spirit = 60, Height = 1650 };
        state.Roster.Characters.Add(c); state.Player.Stamina = 100;
        return (state, c, new FlagService(), data);
    }

    private static string Snapshot(SaveState state, FlagService flags)
    {
        var storage = new FlagStorage(); flags.SyncToStorage(storage);
        return JsonSerializer.Serialize(new { State = state, Flags = storage });
    }

    private static void CheckObservations(SmokeTestResult result)
    {
        var (state, c, flags, data) = Fixture();
        var service = new CharacterDevelopmentService(state, data, flags);
        var before = Snapshot(state, flags);
        for (var i = 0; i < 100; i++) { service.Inspect(c.Id); service.Capture(c.Id, DevelopmentCause.Practice); }
        Check(result, before == Snapshot(state, flags) && service.Inspect(c.Id)!.FirstObservedDay is null,
            "previews and captures allocate no saved baseline, journal or rewards");
        Check(result, service.Inspect(null) is null && service.Inspect("missing") is null, "unknown residents have no invented development");
        var token = service.Capture(c.Id, DevelopmentCause.Practice);
        c.CombatSkill += 2; c.MagicPower += 4;
        var hp = c.Hp; var mana = c.Mana; var spirit = c.Spirit; var energy = c.Energy; var stamina = state.Player.Stamina;
        var changes = service.Complete(token);
        var snapshot = service.Inspect(c.Id)!;
        Check(result, c.MaxHpOverride == 105 && c.MaxMana == 55 && c.Hp == hp && c.Mana == mana
            && c.Spirit == spirit && c.Energy == energy && state.Player.Stamina == stamina,
            "genuine skill gains grow bounded capacity without a free HP/MP/EP/SP/STA refill");
        Check(result, changes.Count == 6 && snapshot.Values.Single(v => v.Field == DevelopmentField.Combat).Baseline == 1
            && snapshot.Morph.Conditioning == 2 / 6.0 && snapshot.Morph.Attunement == 4 / 12.0,
            "one observation records actual stats, capacity and continuous non-graphic development channels");
        before = Snapshot(state, flags);
        Check(result, service.Complete(token).Count == 0 && before == Snapshot(state, flags), "the same capture cannot award a rank twice");
        var foreign = new CharacterDevelopmentService(state, data, new FlagService());
        token = service.Capture(c.Id, DevelopmentCause.Practice);
        Check(result, foreign.Complete(token).Count == 0 && before == Snapshot(state, flags), "another flag owner cannot accept a capture");
        c.CombatSkill++;
        Check(result, service.Complete(token).Count > 0, "a rejected foreign submission does not consume the real owner's capture");
        token = service.Capture(c.Id, DevelopmentCause.Practice);
        var duplicate = service.Capture(c.Id, DevelopmentCause.Practice);
        c.CombatSkill++;
        service.Complete(token); before = Snapshot(state, flags);
        Check(result, service.Complete(duplicate).Count == 0 && before == Snapshot(state, flags), "revision receipts reject two overlapping observations");
        // Losing and recovering a previous skill does not farm repeated capacity.
        token = service.Capture(c.Id, DevelopmentCause.Practice); c.CombatSkill -= 2; service.Complete(token);
        var capacity = c.MaxHpOverride;
        token = service.Capture(c.Id, DevelopmentCause.Practice); c.CombatSkill += 2; service.Complete(token);
        Check(result, c.MaxHpOverride == capacity, "returning to a previous skill high does not award another permanent benefit");
        for (var i = 0; i < 40; i++)
        {
            token = service.Capture(c.Id, DevelopmentCause.Practice);
            c.MagicPower += 2; service.Complete(token);
        }
        snapshot = service.Inspect(c.Id)!;
        Check(result, c.MaxMana == 65 && snapshot.Morph.Attunement == 1 && snapshot.RecentChanges.Count == 12
            && flags.TotalCharFlagCount <= 81, "repeated training saturates its benefit and keeps exactly bounded journal storage");
        Check(result, snapshot.RecentChanges.All(x => x.Day == 2) && c.Height == 1650 && c.BodyTypeOverride == ""
            && c.BustSize == 3 && c.AdultEligibility == AdultEligibility.Unknown,
            "non-explicit conditioning does not overwrite identity, age eligibility or anatomical content");
        before = Snapshot(state, flags);
        for (var i = 0; i < 100; i++) service.Inspect(c.Id);
        Check(result, before == Snapshot(state, flags), "reading a populated journal does not change its cursor or baseline");
        flags.SyncToStorage(state.Flags);
        var loaded = JsonSerializer.Deserialize<SaveState>(JsonSerializer.Serialize(state))!;
        var restored = new FlagService(); restored.SyncFromStorage(loaded.Flags);
        var resumed = new CharacterDevelopmentService(loaded, data, restored).Inspect(c.Id)!;
        Check(result, JsonSerializer.Serialize(snapshot) == JsonSerializer.Serialize(resumed),
            "current-schema JSON retains baseline, capped channels, ordered history and source protection");
        token = service.Capture(c.Id, DevelopmentCause.Practice);
        state.Calendar.Day++; c.RanchSkill++; before = Snapshot(state, flags);
        Check(result, service.Complete(token).Count == 0 && before == Snapshot(state, flags), "a stale daytime observation cannot commit in another day");
        token = service.Capture(c.Id, DevelopmentCause.Practice);
        state.Roster.Characters.Add(c); before = Snapshot(state, flags);
        Check(result, service.Complete(token).Count == 0 && before == Snapshot(state, flags), "ambiguous resident identity rejects completion");
        state.Roster.Characters.RemoveAt(1);
        c.Hp = 0; token = service.Capture(c.Id, DevelopmentCause.Practice); c.CombatSkill += 2;
        service.Complete(token);
        Check(result, c.MaxHpOverride == capacity, "incapacity cannot be used to earn a new conditioning benefit");
    }

    private static void CheckPractice(SmokeTestResult result)
    {
        var (state, c, flags, data) = Fixture();
        var residents = new ResidentInteractionService(state, data, flags, new VisitService(state, data),
            new BondService(state, data, new MilestoneService(state, data, new EconomyService(state))),
            new TrainingService(state, new TalentService(state, data)), new PlayerStaminaService(state));
        var development = new CharacterDevelopmentService(state, data, flags);
        var stamina = state.Player.Stamina;
        Check(result, residents.Execute(c.Id, ResidentAction.CombatPractice).Success && c.CombatSkill == 2
            && c.Energy == 70 && state.Calendar.TrainedToday == 1
            && state.Player.Stamina == stamina - residents.Cost(ResidentAction.CombatPractice),
            "real resident practice applies the old lesson effects and costs exactly once");
        Check(result, development.Inspect(c.Id)!.RecentChanges.Any(x => x.Field == DevelopmentField.Combat),
            "ordinary resident actions feed the development journal without a debug command");
        var before = Snapshot(state, flags);
        Check(result, !residents.Execute(c.Id, ResidentAction.CombatPractice).Success && before == Snapshot(state, flags),
            "repeated practice cannot farm history, capacity or another lesson");
        state.Calendar.Day++; state.Calendar.TrainedToday = 0; state.Player.Stamina = 100;
        Check(result, residents.Execute(c.Id, ResidentAction.CombatPractice).Success && c.MaxHpOverride == 105 && c.Hp == 80,
            "a second real combat lesson crosses the conditioning stage, without healing the resident");
        c.Hp = 0; state.Calendar.Day++; state.Calendar.TrainedToday = 0; before = Snapshot(state, flags);
        Check(result, !residents.Execute(c.Id, ResidentAction.MagicPractice).Success && before == Snapshot(state, flags),
            "the shared recovery guard rejects an incapacitated resident before costs or development flags");
    }

    private static void CheckRoot(SmokeTestResult result)
    {
        var game = GameRoot.Instance;
        var wrote = false;
        if (game.HasSaveSlot(99)) throw new InvalidOperationException("Development test refuses an occupied isolated slot 99.");
        try
        {
            game.NewGame(); game.State.Calendar.Day = 2; game.State.Story.FirstDayCompleted = true;
            game.State.Calendar.Phase = DayPhase.Night; game.State.Calendar.NightAction = "rest";
            var c = game.State.Roster.Characters.First(x => x.Id != "anon");
            c.Hp = 100; c.Energy = 80; c.Fatigue = 0; c.CombatSkill = 2; c.SkillXp["combat"] = 19;
            game.State.Schedule.AssignedJobs[c.Id] = "patrol";
            var day = game.State.Calendar.Day;
            var report = game.EndDay();
            var snapshot = game.GetCharacterDevelopment(c.Id)!;
            Check(result, c.CombatSkill == 3 && snapshot.FirstObservedDay == day
                && snapshot.RecentChanges.Any(x => x.Field == DevelopmentField.Combat && x.Cause == DevelopmentCause.Workday)
                && report.Lines.Any(x => x.Contains("→")), "the ordinary root day records an actual existing work level-up in its report");
            wrote = game.SaveSlot(99);
            Check(result, wrote && game.LoadSlot(99), "the ordinary root saves and reloads development through its canonical FlagService");
            Check(result, JsonSerializer.Serialize(game.GetCharacterDevelopment(c.Id)) == JsonSerializer.Serialize(snapshot),
                "root reload does not reconstruct a new baseline or replay a reward");
            game.StartNewGamePlus();
            Check(result, game.State.Roster.Characters.All(x => game.GetCharacterDevelopment(x.Id)!.FirstObservedDay is null),
                "only an explicit new run starts fresh character development records");
        }
        finally { if (wrote) game.Save.Delete(99); game.NewGame(); }
    }
}
