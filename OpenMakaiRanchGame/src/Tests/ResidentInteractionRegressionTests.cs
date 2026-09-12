using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Data;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.Tests;

/// <summary>Isolated non-explicit mechanics fixtures, not character approvals or a balance certification.</summary>
public static class ResidentInteractionRegressionTests
{
    public static void Run(SmokeTestResult result)
    {
        void Check(bool pass, string message)
        { result.Passed &= pass; result.Lines.Add($"SMOKE {(pass ? "OK" : "FAIL")} resident interactions: {message}"); }
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data, new Random(91011)).CreateNewGame();
        state.Calendar.Day = 2; state.Story.FirstDayCompleted = true;
        var resident = state.Roster.Characters.First(c => c.Id != "anon");
        resident.Energy = 80; resident.Hp = 80; resident.Fatigue = 10; resident.Morale = 50;
        resident.RanchSkill = 2; resident.CraftSkill = 2; resident.CombatSkill = 2; resident.MaxEnergyOverride = 100;
        var flags = new FlagService();
        ResidentInteractionService Service(SaveState s, FlagService f) => new(s, data, f,
            new VisitService(s, data), new BondService(s, data, new MilestoneService(s, data, new EconomyService(s))),
            new TrainingService(s, new TalentService(s, data)), new PlayerStaminaService(s));
        var service = Service(state, flags);
        string Snapshot()
        {
            var storage = new FlagStorage(); flags.SyncToStorage(storage);
            return JsonSerializer.Serialize(new { State = state, Flags = storage });
        }
        var before = Snapshot();
        for (var i = 0; i < 50; i++)
            foreach (var action in Enum.GetValues<ResidentAction>()) service.Inspect(resident.Id, action, "gift_journal");
        Check(before == Snapshot(), "repeated previews do not change state or allocate saved daily receipts");
        state.Player.Stamina = 0; before = Snapshot();
        for (var i = 0; i < 20; i++)
        {
            var chat = service.Execute(resident.Id, ResidentAction.Chat);
            if (!chat.Success || chat.Changed) throw new InvalidOperationException("Ordinary conversation was not free/read-only.");
        }
        Check(before == Snapshot(), "ordinary conversation remains usable at zero stamina without farmable relationship rewards");
        Check(!service.Execute(resident.Id, ResidentAction.Encourage).Success && before == Snapshot(), "insufficient stamina rejects encouragement before effects");
        state.Player.Stamina = 100; before = Snapshot();
        Check(!service.Execute(resident.Id, (ResidentAction)999).Success && before == Snapshot(), "unknown commands reject without a cost or receipt");
        foreach (var id in new string?[] { null, "", "missing", "anon" })
            Check(!service.Execute(id, ResidentAction.Gift, "gift_journal").Success && before == Snapshot(), "invalid/self target rejects atomically: " + id);
        state.Roster.Characters.Add(resident);
        before = Snapshot();
        Check(!service.Execute(resident.Id, ResidentAction.Meal).Success && before == Snapshot(), "ambiguous duplicate resident IDs are not valid targets");
        state.Roster.Characters.RemoveAt(state.Roster.Characters.Count - 1);
        // A new game includes meals. Remove only this isolated fixture's starting stock so
        // the missing-item assertion actually exercises a missing item.
        state.Inventory.Items.Remove("meal_box");
        before = Snapshot();
        Check(!service.Execute(resident.Id, ResidentAction.Meal).Success && before == Snapshot(), "missing meal does not consume player energy");
        state.Inventory.Items["meal_box"] = 2;
        var energy = resident.Energy; var stamina = state.Player.Stamina;
        Check(service.Execute(resident.Id, ResidentAction.Meal).Success && state.Inventory.Items["meal_box"] == 1
            && resident.Energy == energy + 10 && state.Player.Stamina == stamina - service.Cost(ResidentAction.Meal), "one meal consumes one item and its declared stamina once");
        before = Snapshot();
        Check(!service.Execute(resident.Id, ResidentAction.Meal).Success && before == Snapshot(), "duplicate meal reward cannot be farmed on the same day");
        state.Inventory.Items["gift_journal"] = 2; state.Inventory.Items["dragon_egg"] = 1;
        before = Snapshot();
        Check(!service.Execute(resident.Id, ResidentAction.Gift, "dragon_egg").Success && before == Snapshot(), "special keepsakes are not silently consumed as ordinary gifts");
        var bond = resident.Bond; stamina = state.Player.Stamina;
        Check(service.Execute(resident.Id, ResidentAction.Gift, "gift_journal").Success && state.Inventory.Items["gift_journal"] == 1
            && resident.Bond == Math.Min(100, bond + 8) && state.Player.Stamina == stamina - service.Cost(ResidentAction.Gift), "ordinary gift uses the existing care effect with exact inventory cost");
        state.Inventory.Items["gift_ribbon"] = 1; before = Snapshot();
        Check(!service.Execute(resident.Id, ResidentAction.Gift, "gift_ribbon").Success && before == Snapshot(), "switching gift types cannot bypass the shared daily gift receipt");
        stamina = state.Player.Stamina; bond = resident.Bond;
        Check(service.Execute(resident.Id, ResidentAction.Encourage).Success && resident.Bond == Math.Min(100, bond + 3)
            && state.Player.Stamina == stamina - service.Cost(ResidentAction.Encourage), "encouragement is separate from free conversation and pays once");
        before = Snapshot();
        Check(!service.Execute(resident.Id, ResidentAction.Encourage).Success && before == Snapshot(), "encouragement has a durable daily limit");
        resident.Energy = 125; resident.Fatigue = 20;
        Check(service.Execute(resident.Id, ResidentAction.Recovery).Success && resident.Energy == 125 && resident.Fatigue == 10,
            "recovery never reduces an existing energy boost above the ordinary cap");
        // The raw service regression reproduces the pre-switch payment bug independently of the new UI.
        var training = new TrainingService(state, new TalentService(state, data));
        before = Snapshot();
        Check(!training.Train(resident.Id, "unknown_focus") && before == Snapshot(), "invalid training focus does not spend resident energy, fatigue or a training slot");
        resident.RanchSkill = 10; before = Snapshot();
        Check(!training.Train(resident.Id, "ranch") && before == Snapshot(), "capped training rejects before paying costs");
        resident.RanchSkill = 2; resident.MagicPower = int.MaxValue; before = Snapshot();
        Check(!training.Train(resident.Id, "magic") && before == Snapshot(), "magic training rejects integer overflow without spending resources");
        resident.Hp = 0; before = Snapshot();
        Check(!service.Execute(resident.Id, ResidentAction.CraftPractice).Success && !training.Train(resident.Id, "craft")
            && before == Snapshot(), "incapacitated residents cannot train through either boundary");
        resident.Hp = 80; resident.Morale = 24; before = Snapshot();
        Check(!service.Execute(resident.Id, ResidentAction.CraftPractice).Success && before == Snapshot(), "low morale blocks a new lesson without a penalty");
        resident.Morale = 50; resident.Fatigue = 80; before = Snapshot();
        Check(!service.Execute(resident.Id, ResidentAction.CraftPractice).Success && before == Snapshot(), "high fatigue blocks lessons without a cost");
        resident.Fatigue = 10; state.Player.Stamina = 100; state.Calendar.Phase = DayPhase.Night; before = Snapshot();
        Check(!service.Execute(resident.Id, ResidentAction.CraftPractice).Success && before == Snapshot(), "night lessons cannot silently double the chosen nightly growth plan");
        state.Calendar.Phase = DayPhase.Morning; state.Story.FirstDayCompleted = false; before = Snapshot();
        Check(!service.Execute(resident.Id, ResidentAction.CraftPractice).Success && before == Snapshot(), "new interactions cannot bypass the guided first day");
        state.Story.FirstDayCompleted = true; energy = resident.Energy; stamina = state.Player.Stamina;
        var assignment = state.Schedule.AssignedJobs.GetValueOrDefault(resident.Id, "rest");
        Check(service.Execute(resident.Id, ResidentAction.CraftPractice).Success && resident.CraftSkill == 3
            && resident.Energy == energy - 10 && state.Calendar.TrainedToday == 1
            && state.Player.Stamina == stamina - service.Cost(ResidentAction.CraftPractice)
            && state.Schedule.AssignedJobs.GetValueOrDefault(resident.Id, "rest") == assignment,
            "practice uses the shared training slot/effects, charges player stamina once and preserves the work plan");
        before = Snapshot();
        Check(!service.Execute(resident.Id, ResidentAction.CombatPractice).Success && before == Snapshot(), "changing practice focus does not bypass this resident's daily lesson");
        for (var i = 0; i < 2; i++)
        {
            var other = JsonSerializer.Deserialize<CharacterState>(JsonSerializer.Serialize(resident))!;
            other.Id = "practice_fixture_" + i; other.Energy = 80; other.Fatigue = 0;
            state.Roster.Characters.Add(other);
        }
        Check(service.Execute("practice_fixture_0", ResidentAction.RanchPractice).Success && state.Calendar.TrainedToday == 2,
            "another resident can use the second ranch-wide session");
        before = Snapshot();
        Check(!service.Execute("practice_fixture_1", ResidentAction.RanchPractice).Success && before == Snapshot(), "a third resident cannot exceed the existing two-session daily budget");
        flags.SyncToStorage(state.Flags);
        var loaded = JsonSerializer.Deserialize<SaveState>(JsonSerializer.Serialize(state))!;
        var loadedFlags = new FlagService(); loadedFlags.SyncFromStorage(loaded.Flags);
        var loadedService = Service(loaded, loadedFlags);
        Check(!loadedService.Inspect(resident.Id, ResidentAction.Meal).Available
            && !loadedService.Inspect(resident.Id, ResidentAction.CraftPractice).Available,
            "current-schema saving retains daily care and practice receipts");
        new DayCycleService(loaded).AdvanceToNextDay();
        Check(loadedService.Inspect(resident.Id, ResidentAction.CraftPractice).Available && loaded.Calendar.TrainedToday == 0
            && loadedService.Inspect(resident.Id, ResidentAction.Meal).Available, "the canonical next-day reset makes actions available without growing a per-day history");
        var count = loaded.Flags.CharIntFlags.Sum(pair => pair.Value.Count(entry =>
            entry.Key >= ResidentInteractionService.EncourageDay && entry.Key <= ResidentInteractionService.PracticeDay));
        Check(count <= 7, "the original daily-care receipt count remains bounded independently of development history");
    }
}
