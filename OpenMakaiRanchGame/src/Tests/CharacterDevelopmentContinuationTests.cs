using System;
using System.Linq;
using System.Text.Json;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Data;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.Locale;

namespace OpenMakaiRanch.Tests;

/// <summary>Isolated mechanics/command fixtures, not a full campaign or model-morph playtest.</summary>
public static class CharacterDevelopmentContinuationTests
{
    private static void Check(SmokeTestResult result, bool pass, string message)
    { result.Passed &= pass; result.Lines.Add($"SMOKE {(pass ? "OK" : "FAIL")} development continuation: {message}"); }

    public static void Run(SmokeTestResult result)
    {
        var locale = LocaleCatalog.CurrentLocale;
        try
        {
            LocaleCatalog.LoadLocale("en");
            CheckTracks(result);
            CheckFeedback(result);
            CheckRoot(result);
        }
        finally { LocaleCatalog.LoadLocale(locale); }
    }

    private static CharacterState Resident(string id) => new()
    {
        Id = id, DefinitionId = "rancher", DisplayNameOverride = "Resident", Hp = 80, MaxHpOverride = 100,
        Energy = 100, MaxEnergyOverride = 100, Morale = 60, CombatSkill = 3, MagicPower = 2,
        MaxMana = 50, Mana = 17, MaxSpirit = 100, Spirit = 60, Height = 1650
    };

    private static (SaveState state, CharacterState resident, DataRegistry data, FlagService flags) Fixture()
    {
        var state = new SaveState(); state.Calendar.Day = 2; state.Story.FirstDayCompleted = true;
        state.Player.Stamina = 100;
        var c = Resident("development_continuation"); state.Roster.Characters.Add(c);
        return (state, c, DataRegistry.CreateSeeded(), new FlagService());
    }

    private static string Snapshot(SaveState state, FlagService flags)
    {
        var storage = new FlagStorage(); flags.SyncToStorage(storage);
        return JsonSerializer.Serialize(new { State = state, Flags = storage });
    }

    private static void CheckTracks(SmokeTestResult result)
    {
        var (state, c, data, flags) = Fixture();
        var development = new CharacterDevelopmentService(state, data, flags);
        var before = Snapshot(state, flags);
        for (var i = 0; i < 100; i++)
            foreach (var focus in new[] { "combat", "magic", "ranch", "craft", "unknown" })
                CharacterDevelopmentFeedback.Explain(development.InspectPracticeTrack(c.Id, focus));
        Check(result, before == Snapshot(state, flags), "opening forecasts never creates a baseline, stage or journal entry");
        var track = development.InspectPracticeTrack(c.Id, "combat")!;
        Check(result, track.Stage == 0 && track.TargetSkill == 5 && track.SkillGainNeeded == 2
            && track.NextStageCapacityGain == 5 && track.State == DevelopmentTrackState.Available,
            "initial conditioning forecasts the actual two-point threshold, without retrospective rewards");
        Check(result, development.InspectPracticeTrack(c.Id, "magic")!.TargetSkill == 6
            && development.InspectPracticeTrack(c.Id, "ranch") is null
            && development.InspectPracticeTrack(null, "combat") is null,
            "only the two authored capacity tracks have forecasts; unsupported or missing targets do not");
        var token = development.Capture(c.Id, DevelopmentCause.Practice); c.CombatSkill++; development.Complete(token);
        track = development.InspectPracticeTrack(c.Id, "combat")!;
        Check(result, track.Points == 1 && track.TargetSkill == 5 && track.SkillGainNeeded == 1,
            "one genuine point reduces the distance to the next stage");
        token = development.Capture(c.Id, DevelopmentCause.Practice); c.CombatSkill = 2; development.Complete(token);
        track = development.InspectPracticeTrack(c.Id, "combat")!;
        Check(result, track.TargetSkill == 5 && track.SkillGainNeeded == 3 && track.HighestRecordedSkill == 4,
            "a setback forecasts catch-up to the old high as well as new progress");
        token = development.Capture(c.Id, DevelopmentCause.Practice); c.CombatSkill = 4; development.Complete(token);
        Check(result, c.MaxHpOverride == 100 && development.InspectPracticeTrack(c.Id, "combat")!.Points == 1,
            "recovering a lost skill does not receive another conditioning point");
        token = development.Capture(c.Id, DevelopmentCause.Practice); c.CombatSkill++; development.Complete(token);
        Check(result, c.MaxHpOverride == 105 && c.Hp == 80 && development.InspectPracticeTrack(c.Id, "combat")!.Stage == 1,
            "reaching the forecast threshold adds the predicted capacity, not healing");
        token = development.Capture(c.Id, DevelopmentCause.Practice); c.MagicPower += 100; development.Complete(token);
        track = development.InspectPracticeTrack(c.Id, "magic")!;
        Check(result, track.State == DevelopmentTrackState.Complete && track.Stage == 3
            && track.SkillGainNeeded == 0 && track.NextStageCapacityGain == 0,
            "completed tracks stop promising a fourth capacity reward");

        var fresh = Resident("limited"); fresh.CombatSkill = 9; state.Roster.Characters.Add(fresh);
        track = development.InspectPracticeTrack(fresh.Id, "combat")!;
        Check(result, track.State == DevelopmentTrackState.SkillLimit && track.TargetSkill == 11,
            "an imported nine-point skill does not promise a stage beyond the ten-point lesson limit");
        fresh.CombatSkill = 1; fresh.MaxHpOverride = int.MaxValue - 2;
        track = development.InspectPracticeTrack(fresh.Id, "combat")!;
        token = development.Capture(fresh.Id, DevelopmentCause.Practice); fresh.CombatSkill += 2; development.Complete(token);
        Check(result, track.NextStageCapacityGain == 2 && fresh.MaxHpOverride == int.MaxValue && fresh.Hp == 80,
            "preview and committed capacity both saturate at the same remaining two points");
        Check(result, development.InspectPracticeTrack(fresh.Id, "combat")!.State == DevelopmentTrackState.CapacityLimit,
            "a full integer capacity explains why another bonus cannot fit");
        fresh.MagicPower = int.MaxValue - 4;
        Check(result, development.InspectPracticeTrack(fresh.Id, "magic")!.State == DevelopmentTrackState.Available,
            "the last reachable odd magic threshold remains available");
        token = development.Capture(fresh.Id, DevelopmentCause.Practice); fresh.MagicPower += 3; development.Complete(token);
        Check(result, development.InspectPracticeTrack(fresh.Id, "magic")!.State == DevelopmentTrackState.SkillLimit,
            "a one-point gap cannot promise an overflowing two-point magic lesson");
        fresh.MaxHpOverride = -1;
        Check(result, development.InspectPracticeTrack(fresh.Id, "combat")!.State == DevelopmentTrackState.Invalid,
            "invalid capacity is not described as a training opportunity");
        state.Roster.Characters.Add(c);
        Check(result, development.InspectPracticeTrack(c.Id, "combat") is null, "duplicate identities cannot produce a forecast");
    }

    private static void CheckFeedback(SmokeTestResult result)
    {
        var (state, c, data, flags) = Fixture();
        var development = new CharacterDevelopmentService(state, data, flags);
        var service = new ResidentInteractionService(state, data, flags, new VisitService(state, data),
            new BondService(state, data, new MilestoneService(state, data, new EconomyService(state))),
            new TrainingService(state, new TalentService(state, data)), new PlayerStaminaService(state));
        Check(result, CharacterDevelopmentFeedback.Reflect(development.Inspect(c.Id)) == "",
            "a resident without recorded changes gets no invented development story");
        var offer = service.Inspect(c.Id, ResidentAction.CombatPractice);
        Check(result, offer.Available && offer.Reason.Contains("3 → 5") && offer.Reason.Contains("+5"),
            "the existing available-action tooltip includes the real next conditioning target");
        var action = service.Execute(c.Id, ResidentAction.CombatPractice);
        Check(result, action.Success && action.Message.Contains("4 → 5") && c.MaxHpOverride == 100,
            "the actual lesson result explains remaining progress rather than granting a bonus early");
        state.Player.Stamina = 0;
        var before = Snapshot(state, flags);
        var chat = service.Execute(c.Id, ResidentAction.Chat);
        for (var i = 0; i < 50; i++) service.Execute(c.Id, ResidentAction.Chat);
        Check(result, chat.Success && !chat.Changed && chat.Message.Contains("3 → 4") && before == Snapshot(state, flags),
            "free conversation reflects a real recorded improvement without costs, rewards or memory writes");
        c.Fatigue = 70;
        Check(result, service.Execute(c.Id, ResidentAction.Chat).Message.Contains("quiet break"),
            "immediate fatigue takes precedence over a development reflection");
        c.Fatigue = 0; c.Morale = 30;
        Check(result, service.Execute(c.Id, ResidentAction.Chat).Message.Contains("Today feels difficult"),
            "low morale is not hidden behind celebratory progress dialogue");
        c.Morale = 60;
        var token = development.Capture(c.Id, DevelopmentCause.Practice); c.CombatSkill = 2; development.Complete(token);
        Check(result, service.Execute(c.Id, ResidentAction.Chat).Message.Contains("earlier form"),
            "a recorded loss is acknowledged rather than described as an improvement");
        c.CombatSkill = 8;
        Check(result, CharacterDevelopmentFeedback.Reflect(development.Inspect(c.Id)) == "",
            "an untracked edit does not leave a misleading obsolete reflection");
        var en = CharacterDevelopmentFeedback.Explain(development.InspectPracticeTrack(c.Id, "combat"));
        before = Snapshot(state, flags);
        LocaleCatalog.LoadLocale("de");
        var de = CharacterDevelopmentFeedback.Explain(development.InspectPracticeTrack(c.Id, "combat"));
        Check(result, de.Contains("Konditionierung") && de.Contains("Kampffertigkeit") && de != en
            && !de.Contains("{0}") && before == Snapshot(state, flags), "German feedback formats at read time without gameplay mutation");
        LocaleCatalog.LoadLocale("en");
    }

    private static void CheckRoot(SmokeTestResult result)
    {
        var game = GameRoot.Instance;
        var wrote = false;
        var paused = game.GetTree().Paused;
        if (game.HasSaveSlot(99)) throw new InvalidOperationException("Continuation test refuses an occupied isolated slot 99.");
        try
        {
            game.NewGame(); game.State.Calendar.Day = 2; game.State.Story.FirstDayCompleted = true;
            var c = Resident("root_practice"); c.CombatSkill = 1;
            var other = Resident("root_practice_other");
            game.State.Roster.Characters.Add(c); game.State.Roster.Characters.Add(other);
            var day = game.State.Calendar.Day; var phase = game.State.Calendar.Phase; var generation = game.StateGeneration;
            string Current() => Snapshot(game.State, game.Flags);
            var before = Current();
            foreach (var focus in new string?[] { null, "", "unknown", "Combat" })
                Check(result, !game.TryTrainCharacter(c.Id, focus, generation, day, phase).Success && before == Current(),
                    "an unmapped legacy focus rejects without mutation: " + focus);
            Check(result, !game.TryTrainCharacter(c.Id, "combat", generation + 1, day, phase).Success
                && !game.TryTrainCharacter(c.Id, "combat", generation, day + 1, phase).Success
                && !game.TryTrainCharacter(c.Id, "combat", generation, day, DayPhase.Night).Success && before == Current(),
                "the stamped training entry rejects stale session, day and phase");
            game.State.Story.FirstDayCompleted = false; before = Current();
            Check(result, !game.TrainCharacter(c.Id, "combat") && before == Current(), "legacy training cannot skip the guided introduction");
            game.State.Story.FirstDayCompleted = true; game.State.Calendar.Phase = DayPhase.Night; before = Current();
            Check(result, !game.TrainCharacter(c.Id, "combat") && before == Current(), "legacy training cannot double the night plan");
            game.State.Calendar.Phase = phase; game.GetTree().Paused = true; before = Current();
            Check(result, !game.TrainCharacter(c.Id, "combat") && before == Current(), "legacy training is blocked while paused");
            game.GetTree().Paused = false; game.BeginCombatSession(); before = Current();
            Check(result, !game.TrainCharacter(c.Id, "combat") && before == Current(), "legacy training cannot run during combat preparation");
            game.EndCombatSession();
            game.State.Player.Stamina = 0; before = Current();
            Check(result, !game.TrainCharacter(c.Id, "combat") && before == Current(), "legacy training cannot bypass player stamina");
            game.State.Player.Stamina = 100;
            var notifications = 0; var reentered = false; var advanced = false;
            void Reenter()
            {
                notifications++;
                if (notifications != 1) return;
                reentered |= game.TrainCharacter(other.Id, "combat");
                reentered |= game.TryTrainCharacter(other.Id, "magic", generation, day, phase).Success;
                advanced |= game.AdvanceTime();
            }
            game.StateChanged += Reenter;
            try
            {
                Check(result, game.TrainCharacter(c.Id, "combat") && c.CombatSkill == 2 && c.Energy == 90
                    && game.State.Player.Stamina == 80 && game.State.Calendar.TrainedToday == 1,
                    "the legacy root entry now pays the same single lesson and stamina costs");
            }
            finally { game.StateChanged -= Reenter; }
            Check(result, notifications == 1 && !reentered && !advanced && other.CombatSkill == 3,
                "one notification stays command-locked against reentrant practice and time advance");
            Check(result, game.GetCharacterDevelopment(c.Id)!.RecentChanges.Any(x => x.Field == DevelopmentField.Combat)
                && game.GetCharacterDevelopmentTrack(c.Id, "combat")!.Points == 1,
                "legacy root training now records actual development exactly once");
            before = Current();
            Check(result, !game.TrainCharacter(c.Id, "magic")
                && !game.TryResidentAction(c.Id, ResidentAction.CraftPractice, generation, day, phase).Success && before == Current(),
                "switching old/new entry points or focus cannot evade the same resident's daily receipt");
            Check(result, game.TryTrainCharacter(other.Id, "magic", generation, day, phase).Success
                && game.State.Calendar.TrainedToday == 2 && game.State.Player.Stamina == 60,
                "another resident can use the second shared ranch lesson");
            var third = Resident("root_practice_third"); game.State.Roster.Characters.Add(third); before = Current();
            Check(result, !game.TrainCharacter(third.Id, "combat") && before == Current(),
                "the legacy entry cannot create a third ranch-wide lesson");
            var track = game.GetCharacterDevelopmentTrack(c.Id, "combat");
            var id = c.Id; wrote = game.SaveSlot(99);
            Check(result, wrote && game.LoadSlot(99) && game.GetCharacterDevelopmentTrack(id, "combat") == track,
                "ordinary root save/load retains the forecast and underlying high-water progress");
            before = Current();
            Check(result, !game.TrainCharacter(id, "combat") && before == Current(), "loading cannot erase a legacy-entry daily lesson receipt");
            game.State.Calendar.Phase = DayPhase.Night; game.State.Calendar.NightAction = "rest";
            foreach (var resident in game.State.Roster.Characters) game.State.Schedule.AssignedJobs[resident.Id] = "rest";
            var nested = false;
            void DuringSettlement(DailyReport _) { nested |= game.TrainCharacter(third.Id, "combat"); }
            game.DaySettled += DuringSettlement;
            try { game.EndDay(); }
            finally { game.DaySettled -= DuringSettlement; }
            Check(result, !nested && game.State.Calendar.Day == day + 1 && game.State.Calendar.TrainedToday == 0,
                "day settlement rejects nested practice and resets the canonical lesson budget once");
            c = game.Roster.Find(id)!; c.Energy = 100; c.Fatigue = 0; c.Morale = 60; var hp = c.Hp;
            Check(result, game.TrainCharacter(id, "combat") && c.CombatSkill == 3 && c.MaxHpOverride == 105 && c.Hp == hp,
                "a real next-day legacy lesson reaches the forecast capacity stage without a refill");
        }
        finally
        {
            game.GetTree().Paused = paused;
            if (wrote) game.Save.Delete(99);
            game.NewGame();
        }
    }
}
