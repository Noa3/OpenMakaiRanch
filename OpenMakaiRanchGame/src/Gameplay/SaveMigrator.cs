using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;

namespace OpenMakaiRanch.Gameplay;

public static class SaveMigrator
{
    public static SaveState Migrate(SaveState state)
    {
        if (state.SchemaVersion <= 0)
        {
            state.SchemaVersion = 1;
        }

        if (state.SchemaVersion == 1) state.SchemaVersion = 2;
        if (state.SchemaVersion == 2) state.SchemaVersion = 3;
        if (state.SchemaVersion == 3) state.SchemaVersion = 4;
        if (state.SchemaVersion == 4) state.SchemaVersion = 5;
        if (state.SchemaVersion == 5) state.SchemaVersion = 6;
        if (state.SchemaVersion == 6) state.SchemaVersion = 7;
        if (state.SchemaVersion == 7) state.SchemaVersion = 8;
        if (state.SchemaVersion == 8) state.SchemaVersion = 9;
        if (state.SchemaVersion == 9) state.SchemaVersion = 10;
        if (state.SchemaVersion == 10) state.SchemaVersion = 11;
        if (state.SchemaVersion == 11) state.SchemaVersion = 12;
        if (state.SchemaVersion == 12) state.SchemaVersion = 13;

        state.Calendar ??= new CalendarState();
        state.Economy ??= new EconomyState();
        state.Ranch ??= new RanchState();
        state.Roster ??= new RosterState();
        state.Schedule ??= new ScheduleState();
        state.Inventory ??= new InventoryState();
        state.Adventure ??= new AdventureState();
        state.Milestones ??= new MilestoneState();
        state.Research ??= new ResearchState();
        state.Pets ??= new PetState();
        state.Bond ??= new BondState();
        state.Recruitment ??= new RecruitmentState();
        state.Settings ??= new SettingsState();
        state.Reports ??= new List<DailyReport>();
        state.Flags ??= new FlagStorage();
        state.Flags.GlobalBoolFlags ??= new Dictionary<int, bool>();
        state.Flags.GlobalIntFlags ??= new Dictionary<int, int>();
        state.Flags.TempBoolFlags ??= new Dictionary<int, bool>();
        state.Flags.TempIntFlags ??= new Dictionary<int, int>();
        state.Flags.CharBoolFlags ??= new Dictionary<string, Dictionary<int, bool>>();
        state.Flags.CharIntFlags ??= new Dictionary<string, Dictionary<int, int>>();
        foreach (var characterId in state.Flags.CharBoolFlags.Keys.ToArray())
            state.Flags.CharBoolFlags[characterId] ??= new Dictionary<int, bool>();
        foreach (var characterId in state.Flags.CharIntFlags.Keys.ToArray())
            state.Flags.CharIntFlags[characterId] ??= new Dictionary<int, int>();

        state.Settings.ThemeId = string.IsNullOrWhiteSpace(state.Settings.ThemeId) ? "midnight" : state.Settings.ThemeId;
        state.Settings.Locale = string.IsNullOrWhiteSpace(state.Settings.Locale) ? "en" : state.Settings.Locale;
        state.Settings.UiScale = Math.Clamp(state.Settings.UiScale <= 0 ? 1.0f : state.Settings.UiScale, 0.85f, 1.35f);

        state.Ranch.Stockpile ??= new Dictionary<string, int>();
        state.Ranch.Facilities ??= new Dictionary<string, int>();
        state.Roster.Characters ??= new List<CharacterState>();

        // Explicit JSON nulls must be normalized before migration reads the roster.
        if (state.SchemaVersion == 13)
        {
            foreach (var character in state.Roster.Characters)
            {
                character.SkinColorIndex = PortraitLayerCatalog.MapSkinColorToIndex(character.SkinColor);
                character.BreastSizeIndex = PortraitLayerCatalog.MapBustSizeToBreastIndex(character.BustSize);
            }
            state.SchemaVersion = 14;
        }

        if (state.SchemaVersion == 14)
            state.SchemaVersion = 15;

        // Schema 16 adds original-style player vitals and Money.csv resource buckets. The fields
        // all have safe initializers, so migration only advances the version and normalizes data.
        if (state.SchemaVersion == 15)
            state.SchemaVersion = 16;

        state.Schedule.AssignedJobs ??= new Dictionary<string, string>();
        state.Inventory.Items ??= new Dictionary<string, int>();
        state.Adventure.SelectedPartyIds ??= new List<string>();
        state.Adventure.DiscoveredMissionIds ??= new List<string>();
        state.Adventure.AvailableMercenaries ??= new List<MercenaryOffer>();
        state.Milestones.CompletedIds ??= new List<string>();
        state.Research.UnlockedSkillIds ??= new List<string>();
        state.Pets.AdoptedPetIds ??= new List<string>();
        state.Pets.Entries ??= new Dictionary<string, PetEntryState>();
        foreach (var petId in state.Pets.AdoptedPetIds)
        {
            if (!state.Pets.Entries.ContainsKey(petId))
                state.Pets.Entries[petId] = new PetEntryState();
        }

        state.Bond.CompletedEventIds ??= new List<string>();
        state.Dating ??= new DatingState();
        state.Dating.Partners ??= new Dictionary<string, DatingPartnerState>();
        state.Dating.ActivePartnerId ??= string.Empty;
        foreach (var partnerId in state.Dating.Partners.Keys.ToList())
        {
            var relationship = state.Dating.Partners[partnerId];
            if (relationship is null)
            {
                state.Dating.Partners[partnerId] = new DatingPartnerState();
                continue;
            }

            relationship.DatesStarted = Math.Max(0, relationship.DatesStarted);
            relationship.SharedActivities = Math.Max(0, relationship.SharedActivities);
            relationship.PositiveMoments = Math.Max(0, relationship.PositiveMoments);
            relationship.PressuredMoments = Math.Max(0, relationship.PressuredMoments);
            relationship.ForcedMoments = Math.Max(0, relationship.ForcedMoments);
            relationship.TrustDamage = Math.Clamp(relationship.TrustDamage, 0, 100);
            relationship.LastActivityId ??= string.Empty;
        }

        state.Adventure.LastCaptureSummary ??= string.Empty;
        foreach (var character in state.Roster.Characters)
        {
            character.SkillXp ??= new Dictionary<string, int>();
            if (string.IsNullOrWhiteSpace(character.BodyImagePathOverride) && !string.IsNullOrWhiteSpace(character.PortraitPathOverride))
                character.BodyImagePathOverride = character.PortraitPathOverride;

            if (string.IsNullOrWhiteSpace(character.BodyTypeOverride))
                character.BodyTypeOverride = "Balanced";

            character.BodyLayerIndex = PortraitLayerCatalog.ClampIndex(character.BodyLayerIndex, PortraitLayerCatalog.BodyTypeCount);
            character.SkinColorIndex = PortraitLayerCatalog.ClampIndex(character.SkinColorIndex, PortraitLayerCatalog.SkinColorCount);
            character.BreastSizeIndex = PortraitLayerCatalog.ClampIndex(character.BreastSizeIndex, PortraitLayerCatalog.BreastSizeCount);
            character.FaceLayerIndex = 0;
            character.RaceLayerIndex = PortraitLayerCatalog.ClampIndex(character.RaceLayerIndex, PortraitLayerCatalog.RaceLayers.Length);
            character.HairLayerIndex = PortraitLayerCatalog.ClampIndex(character.HairLayerIndex, PortraitLayerCatalog.HairLayers.Length);
            character.ClothLayerIndex = PortraitLayerCatalog.ClampIndex(character.ClothLayerIndex, PortraitLayerCatalog.ClothLayers.Length);

            character.Mature ??= new MentalState();
            character.Milk ??= new MilkState();
            character.Addictions ??= new AddictionState();
            character.Equipment ??= new EquipmentState();
            character.EquippedItems ??= new Dictionary<string, string>();
            character.Talents ??= new List<string>();

            // Original-style personal resources. Zero mana/spirit are valid for characters, so
            // absent old-save values remain zero instead of being guessed from MagicPower.
            character.MaxMana = Math.Max(0, character.MaxMana);
            character.Mana = Math.Clamp(character.Mana, 0, character.MaxMana);
            character.ManaRecoveryPercent = Math.Clamp(character.ManaRecoveryPercent, 0, 100);
            character.MaxSpirit = Math.Max(0, character.MaxSpirit);
            character.Spirit = Math.Clamp(character.Spirit, 0, character.MaxSpirit);
            character.Level = Math.Max(1, character.Level);
            character.UltimateCharges = Math.Max(0, character.UltimateCharges);
            character.RecoveryCharges = Math.Max(0, character.RecoveryCharges);

            // Adult eligibility migration (fail-closed)
            if (character.AdultEligibility == AdultEligibility.Unknown)
                AdultEligibilityGate.ValidateAndSetEligibility(character, character.ApparentAge, character.AgeContextNote);
            if (character.Provenance == CharacterProvenance.Unknown)
                character.Provenance = character.IsGenerated ? CharacterProvenance.Generated : CharacterProvenance.RemakeFallback;
        }

        state.Mature ??= new MatureState();
        state.Mature.TrainingHistory ??= new List<TrainingRecord>();

        state.Player ??= new PlayerState();
        if (string.IsNullOrWhiteSpace(state.Player.Name)) state.Player.Name = "Anon";
        if (string.IsNullOrWhiteSpace(state.Player.Race)) state.Player.Race = "Demonfolk";
        if (string.IsNullOrWhiteSpace(state.Player.RanchName)) state.Player.RanchName = "Okachi Ranch";
        if (string.IsNullOrWhiteSpace(state.Player.Gender)) state.Player.Gender = "Male";
        if (string.IsNullOrWhiteSpace(state.Player.BodyShape)) state.Player.BodyShape = "Standard";
        if (string.IsNullOrWhiteSpace(state.Player.SkinColor)) state.Player.SkinColor = "Standard";
        if (string.IsNullOrWhiteSpace(state.Player.HairColor)) state.Player.HairColor = "Black";
        if (string.IsNullOrWhiteSpace(state.Player.HairStyle)) state.Player.HairStyle = "Short";
        if (string.IsNullOrWhiteSpace(state.Player.EyeColor)) state.Player.EyeColor = "Red";
        if (string.IsNullOrWhiteSpace(state.Player.EyeShape)) state.Player.EyeShape = "Standard";

        state.Player.MaxHp = Math.Max(1, state.Player.MaxHp);
        state.Player.Hp = Math.Clamp(state.Player.Hp, 0, state.Player.MaxHp);
        state.Player.MaxSp = Math.Max(1, state.Player.MaxSp);
        state.Player.Sp = Math.Clamp(state.Player.Sp, 0, state.Player.MaxSp);
        state.Player.MaxSpirit = Math.Max(0, state.Player.MaxSpirit);
        state.Player.Spirit = Math.Clamp(state.Player.Spirit, 0, state.Player.MaxSpirit);
        state.Player.Level = Math.Max(1, state.Player.Level);
        state.Player.CombatPower = Math.Max(0, state.Player.CombatPower);
        state.Player.UltimateCharges = Math.Max(0, state.Player.UltimateCharges);
        state.Player.RecoveryCharges = Math.Max(0, state.Player.RecoveryCharges);
        state.Player.MaxMana = Math.Max(0, state.Player.MaxMana);
        state.Player.Mana = Math.Clamp(state.Player.Mana, 0, state.Player.MaxMana);
        state.Player.ManaRecoveryPercent = Math.Clamp(state.Player.ManaRecoveryPercent, 0, 100);
        state.Player.MaxStamina = Math.Max(1, state.Player.MaxStamina);
        state.Player.DailyStaminaBonus = Math.Clamp(state.Player.DailyStaminaBonus, 0, PlayerStaminaService.MaxRestedBonus);
        state.Player.NextDayStaminaBonus = Math.Clamp(state.Player.NextDayStaminaBonus, 0, PlayerStaminaService.MaxRestedBonus);
        state.Player.Stamina = Math.Clamp(state.Player.Stamina, 0, state.Player.MaxStamina + state.Player.DailyStaminaBonus);

        // Money.csv resource state. Cash is intentionally not clamped here because settlement
        // currently permits temporary negative balances; debt/loan behavior is handled separately.
        state.Economy.ExpenseAccount = Math.Max(0, state.Economy.ExpenseAccount);
        state.Economy.LoanBalance = Math.Max(0, state.Economy.LoanBalance);
        state.Economy.BatterySurcharge = Math.Max(0, state.Economy.BatterySurcharge);
        state.Economy.PendingMilkRevenue = Math.Max(0, state.Economy.PendingMilkRevenue);
        state.Economy.SpiritEnergy = Math.Max(0, state.Economy.SpiritEnergy);
        state.Economy.ManaReservoir = Math.Max(0, state.Economy.ManaReservoir);
        state.Economy.ContributionPoints = Math.Max(0, state.Economy.ContributionPoints);
        state.Economy.CompoundingProgress = Math.Max(0, state.Economy.CompoundingProgress);
        state.Economy.BathhouseCleaningProgress = Math.Max(0, state.Economy.BathhouseCleaningProgress);
        state.Economy.ActionPoints ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        state.Economy.ActionPointProgress ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        state.Economy.LifetimeActionPoints ??= new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        NormalizeNonNegative(state.Economy.ActionPoints);
        NormalizeNonNegative(state.Economy.ActionPointProgress);
        NormalizeNonNegative(state.Economy.LifetimeActionPoints);

        // Never return a partially migrated version after normalization.
        state.SchemaVersion = SaveState.CurrentSchemaVersion;
        return state;
    }

    private static void NormalizeNonNegative(Dictionary<string, int> values)
    {
        foreach (var key in values.Keys.ToArray())
            values[key] = Math.Max(0, values[key]);
    }

    private static void NormalizeNonNegative(Dictionary<string, long> values)
    {
        foreach (var key in values.Keys.ToArray())
            values[key] = Math.Max(0L, values[key]);
    }
}