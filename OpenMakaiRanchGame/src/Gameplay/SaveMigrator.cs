using System.Collections.Generic;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Gameplay;

public static class SaveMigrator
{
    public static SaveState Migrate(SaveState state)
    {
        if (state.SchemaVersion <= 0)
        {
            state.SchemaVersion = 1;
        }

        if (state.SchemaVersion == 1)
        {
            state.SchemaVersion = 2;
        }

        if (state.SchemaVersion == 2)
        {
            state.SchemaVersion = 3;
        }

        if (state.SchemaVersion == 3)
        {
            state.SchemaVersion = 4;
        }

        if (state.SchemaVersion == 4)
        {
            state.SchemaVersion = 5;
        }

        if (state.SchemaVersion == 5)
        {
            state.SchemaVersion = 6;
        }

        if (state.SchemaVersion == 6)
        {
            state.SchemaVersion = 7;
        }

        if (state.SchemaVersion == 7)
        {
            state.SchemaVersion = 8;
        }

        if (state.SchemaVersion == 8)
        {
            state.SchemaVersion = 9;
        }

        if (state.SchemaVersion == 9)
        {
            state.SchemaVersion = 10;
        }

        if (state.SchemaVersion == 10)
        {
            state.SchemaVersion = 11;
        }

        if (state.SchemaVersion == 11)
        {
            state.SchemaVersion = 12;
        }

        if (state.SchemaVersion == 12)
        {
            state.SchemaVersion = 13;
        }

        if (state.SchemaVersion == 13)
        {
            foreach (var character in state.Roster.Characters)
            {
                character.SkinColorIndex = PortraitLayerCatalog.MapSkinColorToIndex(character.SkinColor);
                character.BreastSizeIndex = PortraitLayerCatalog.MapBustSizeToBreastIndex(character.BustSize);
            }
            state.SchemaVersion = 14;
        }

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
        state.Settings.ThemeId = string.IsNullOrWhiteSpace(state.Settings.ThemeId) ? "midnight" : state.Settings.ThemeId;
        state.Settings.Locale = string.IsNullOrWhiteSpace(state.Settings.Locale) ? "en" : state.Settings.Locale;
        state.Settings.UiScale = Math.Clamp(state.Settings.UiScale <= 0 ? 1.0f : state.Settings.UiScale, 0.85f, 1.35f);

        state.Ranch.Stockpile ??= new Dictionary<string, int>();
        state.Ranch.Facilities ??= new Dictionary<string, int>();
        state.Roster.Characters ??= new List<CharacterState>();
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
        state.Adventure.LastCaptureSummary ??= string.Empty;
        foreach (var character in state.Roster.Characters)
        {
            character.SkillXp ??= new Dictionary<string, int>();
            if (string.IsNullOrWhiteSpace(character.BodyImagePathOverride) && !string.IsNullOrWhiteSpace(character.PortraitPathOverride))
            {
                character.BodyImagePathOverride = character.PortraitPathOverride;
            }

            if (string.IsNullOrWhiteSpace(character.BodyTypeOverride))
            {
                character.BodyTypeOverride = "Balanced";
            }

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

        return state;
    }
}
