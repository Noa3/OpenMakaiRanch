using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

public static class PortraitLayerCatalog
{
    public static readonly string[] RaceLayers =
    {
        "res://assets/portrait_layers/race_00.png",
        "res://assets/portrait_layers/race_01.png",
        "res://assets/portrait_layers/race_02.png",
        "res://assets/portrait_layers/race_03.png",
        "res://assets/portrait_layers/race_04.png",
        "res://assets/portrait_layers/race_05.png"
    };

    public static readonly string[] HairLayers =
    {
        "res://assets/portrait_layers/hair_00.png",
        "res://assets/portrait_layers/hair_01.png",
        "res://assets/portrait_layers/hair_02.png",
        "res://assets/portrait_layers/hair_03.png",
        "res://assets/portrait_layers/hair_04.png",
        "res://assets/portrait_layers/hair_05.png"
    };

    public static readonly string[] ClothLayers =
    {
        "res://assets/portrait_layers/cloth/cloth_320_00.png",
        "res://assets/portrait_layers/cloth/cloth_321.png",
        "res://assets/portrait_layers/cloth/cloth_322_00.png",
        "res://assets/portrait_layers/cloth/cloth_324_00.png",
        "res://assets/portrait_layers/cloth/cloth_329_00.png",
        "res://assets/portrait_layers/cloth/cloth_364_00.png"
    };

    public static readonly string[] BodyLayers =
    {
        "res://assets/portrait_layers/body.png",
        "res://assets/portrait_layers/body2.png"
    };

    public const string FaceLayer = "res://assets/portrait_layers/face.png";
    public const string BackgroundLayer = "res://assets/portrait_layers/bg.png";

    public static int ClampIndex(int index, int size)
    {
        if (size <= 0)
        {
            return 0;
        }

        return ((index % size) + size) % size;
    }
}

public sealed class SaveStateFactory
{
    private readonly DataRegistry _data;
    private readonly Random _random;
    private const int MinimumGeneratedHp = 40;
    private const int MinimumGeneratedEnergy = 35;
    private const int HighCombatBodyThreshold = 6;
    private static readonly string[] RecruitNamePrefixes = { "Aster", "Bramble", "Cinder", "Dawn", "Ember", "Fable", "Gale", "Hollow", "Iris", "Juniper", "Kite", "Lumen" };
    private static readonly string[] RecruitNameSuffixes = { "Vale", "Moor", "Hearth", "Run", "Bloom", "Reed", "March", "Wisp", "Bell", "Cross", "Field", "Song" };
    private static readonly string[] RecruitTraits = { "Inventive", "Tender", "Swift", "Patient", "Plucky", "Observant", "Steadfast", "Cheerful" };
    private static readonly string[] CombatPortraitPool = { "res://assets/portraits/slay.png", "res://assets/portraits/kagura.png", "res://assets/portraits/noir.png" };
    private static readonly string[] CraftPortraitPool = { "res://assets/portraits/maria.png", "res://assets/portraits/noir.png", "res://assets/portraits/sampleprt.png" };
    private static readonly string[] RanchPortraitPool = { "res://assets/portraits/sharon.png", "res://assets/portraits/slay.png", "res://assets/portraits/sampleprt.png" };
    private static readonly string[] BalancedPortraitPool = { "res://assets/portraits/sampleprt.png", "res://assets/portraits/kagura.png", "res://assets/portraits/maria.png" };
    private static bool _visualPoolsValidated;

    public SaveStateFactory(DataRegistry data, Random? random = null)
    {
        _data = data;
        _random = random ?? Random.Shared;
        ValidateVisualPools();
    }

    public SaveState CreateNewGame()
    {
        var state = new SaveState();
        foreach (var definition in _data.CharacterList())
        {
            var idSeed = definition.Id.Sum(value => value);
            var (bodyLayer, raceLayer, hairLayer, clothLayer) = BuildLayerSelection(definition.RanchSkill, definition.CraftSkill, definition.CombatSkill, idSeed);
            state.Roster.Characters.Add(new CharacterState
            {
                Id = definition.Id,
                DefinitionId = definition.Id,
                Hp = definition.MaxHp,
                Energy = definition.MaxEnergy,
                RanchSkill = definition.RanchSkill,
                CraftSkill = definition.CraftSkill,
                CombatSkill = definition.CombatSkill,
                BodyLayerIndex = bodyLayer,
                RaceLayerIndex = raceLayer,
                HairLayerIndex = hairLayer,
                ClothLayerIndex = clothLayer,
                FaceLayerIndex = 0
            });
            state.Schedule.AssignedJobs[definition.Id] = "rest";
        }

        var existingIds = state.Roster.Characters.Select(character => character.Id).ToHashSet();
        foreach (var recruit in CreateGeneratedRecruits(existingIds))
        {
            state.Roster.Characters.Add(recruit);
            state.Schedule.AssignedJobs[recruit.Id] = "rest";
        }

        state.Ranch.Facilities["pasture"] = 1;
        state.Ranch.Facilities["kitchen"] = 1;
        state.Ranch.Stockpile["farm_goods"] = 0;
        state.Ranch.Stockpile["meals"] = 0;
        state.Ranch.Stockpile["supplies"] = 3;
        state.Adventure.SelectedPartyIds.AddRange(state.Roster.Characters.Take(3).Select(character => character.Id));
        state.Inventory.Items["meal_box"] = 2;
        state.Recruitment.CurrentOffer = CreateGeneratedRecruit(state);
        return state;
    }

    public void RerollGeneratedRecruits(SaveState state)
    {
        var removedIds = state.Roster.Characters.Where(character => character.IsStartingRecruit).Select(character => character.Id).ToHashSet();
        state.Roster.Characters.RemoveAll(character => character.IsStartingRecruit);
        foreach (var removedId in removedIds)
        {
            state.Schedule.AssignedJobs.Remove(removedId);
            state.Adventure.SelectedPartyIds.Remove(removedId);
        }

        var existingIds = state.Roster.Characters.Select(character => character.Id).ToHashSet();
        foreach (var recruit in CreateGeneratedRecruits(existingIds))
        {
            state.Roster.Characters.Add(recruit);
            state.Schedule.AssignedJobs[recruit.Id] = "rest";
        }

        state.Recruitment.CurrentOffer = CreateGeneratedRecruit(state);

        while (state.Adventure.SelectedPartyIds.Count < Math.Min(3, state.Roster.Characters.Count))
        {
            var recruitId = state.Roster.Characters.Select(character => character.Id).FirstOrDefault(id => !state.Adventure.SelectedPartyIds.Contains(id));
            if (string.IsNullOrWhiteSpace(recruitId))
            {
                break;
            }

            state.Adventure.SelectedPartyIds.Add(recruitId);
        }
    }

    public CharacterState CreateGeneratedRecruit(SaveState state, IEnumerable<string>? reservedIds = null)
    {
        var existingIds = state.Roster.Characters.Select(character => character.Id).ToHashSet();
        if (reservedIds is not null)
        {
            foreach (var reservedId in reservedIds)
            {
                if (!string.IsNullOrWhiteSpace(reservedId))
                {
                    existingIds.Add(reservedId);
                }
            }
        }

        return CreateGeneratedRecruit(existingIds);
    }

    private IEnumerable<CharacterState> CreateGeneratedRecruits(ISet<string> existingIds)
    {
        var archetypes = _data.CharacterList().OrderBy(_ => _random.Next()).Take(2).ToList();
        foreach (var archetype in archetypes)
        {
            var recruit = CreateGeneratedRecruit(existingIds, archetype);
            recruit.IsStartingRecruit = true;
            yield return recruit;
        }
    }

    private CharacterState CreateGeneratedRecruit(ISet<string> existingIds, CharacterDefinition? archetype = null)
    {
        var allArchetypes = _data.CharacterList();
        if (allArchetypes.Count == 0)
        {
            throw new InvalidOperationException("No character archetypes are available for recruitment.");
        }

        var selectedArchetype = archetype ?? allArchetypes[_random.Next(allArchetypes.Count)];
        var bonusRanch = _random.Next(0, 3);
        var bonusCraft = _random.Next(0, 3);
        var bonusCombat = _random.Next(0, 3);
        var ranchSkill = Math.Clamp(selectedArchetype.RanchSkill + bonusRanch - 1, 1, 9);
        var craftSkill = Math.Clamp(selectedArchetype.CraftSkill + bonusCraft - 1, 1, 9);
        var combatSkill = Math.Clamp(selectedArchetype.CombatSkill + bonusCombat - 1, 1, 9);
        var maxHp = Math.Max(MinimumGeneratedHp, selectedArchetype.MaxHp + _random.Next(-10, 16));
        var maxEnergy = Math.Max(MinimumGeneratedEnergy, selectedArchetype.MaxEnergy + _random.Next(-8, 14));
        var (portraitPath, bodyImagePath, bodyType) = PickGeneratedVisuals(ranchSkill, craftSkill, combatSkill, selectedArchetype);
        var (bodyLayer, raceLayer, hairLayer, clothLayer) = BuildLayerSelection(ranchSkill, craftSkill, combatSkill, recruitIdSeed: _random.Next());

        var recruitId = $"recruit_{Guid.NewGuid():N}"[..20];
        while (existingIds.Contains(recruitId))
        {
            recruitId = $"recruit_{Guid.NewGuid():N}"[..20];
        }

        existingIds.Add(recruitId);
        return new CharacterState
        {
            Id = recruitId,
            DefinitionId = selectedArchetype.Id,
            IsGenerated = true,
            DisplayNameOverride = $"{RecruitNamePrefixes[_random.Next(RecruitNamePrefixes.Length)]} {RecruitNameSuffixes[_random.Next(RecruitNameSuffixes.Length)]}",
            PortraitPathOverride = portraitPath,
            BodyImagePathOverride = bodyImagePath,
            BodyTypeOverride = bodyType,
            BodyLayerIndex = bodyLayer,
            RaceLayerIndex = raceLayer,
            HairLayerIndex = hairLayer,
            ClothLayerIndex = clothLayer,
            FaceLayerIndex = 0,
            TraitOverride = RecruitTraits[_random.Next(RecruitTraits.Length)],
            MaxHpOverride = maxHp,
            MaxEnergyOverride = maxEnergy,
            Hp = maxHp,
            Energy = maxEnergy,
            RanchSkill = ranchSkill,
            CraftSkill = craftSkill,
            CombatSkill = combatSkill,
            Morale = 55,
            Bond = _random.Next(0, 5)
        };
    }

    private (string PortraitPath, string BodyImagePath, string BodyType) PickGeneratedVisuals(int ranchSkill, int craftSkill, int combatSkill, CharacterDefinition archetype)
    {
        var dominant = "balanced";
        if (Math.Abs(ranchSkill - craftSkill) > 1 || Math.Abs(craftSkill - combatSkill) > 1 || Math.Abs(ranchSkill - combatSkill) > 1)
        {
            if (combatSkill >= ranchSkill && combatSkill >= craftSkill)
            {
                dominant = "combat";
            }
            else if (craftSkill >= ranchSkill && craftSkill >= combatSkill)
            {
                dominant = "craft";
            }
            else if (ranchSkill >= craftSkill && ranchSkill >= combatSkill)
            {
                dominant = "ranch";
            }
        }

        var pool = dominant switch
        {
            "combat" => CombatPortraitPool,
            "craft" => CraftPortraitPool,
            "ranch" => RanchPortraitPool,
            _ => BalancedPortraitPool
        };
        var selectedPortrait = pool[_random.Next(pool.Length)];
        var bodyPool = pool.Where(path => !string.Equals(path, selectedPortrait, StringComparison.Ordinal)).ToArray();
        var selectedBody = bodyPool.Length > 0 ? bodyPool[_random.Next(bodyPool.Length)] : selectedPortrait;
        var bodyType = dominant switch
        {
            "combat" => "Athletic",
            "craft" => "Refined",
            "ranch" => "Sturdy",
            _ => string.IsNullOrWhiteSpace(archetype.BodyType) ? "Balanced" : archetype.BodyType
        };

        return (selectedPortrait, selectedBody, bodyType);
    }

    private static (int BodyLayer, int RaceLayer, int HairLayer, int ClothLayer) BuildLayerSelection(int ranchSkill, int craftSkill, int combatSkill, int recruitIdSeed)
    {
        var baseBodyTier = combatSkill >= HighCombatBodyThreshold ? 1 : 0;
        var bodyLayer = PortraitLayerCatalog.ClampIndex(baseBodyTier + recruitIdSeed, PortraitLayerCatalog.BodyLayers.Length);
        var raceLayer = PortraitLayerCatalog.ClampIndex(ranchSkill + recruitIdSeed, PortraitLayerCatalog.RaceLayers.Length);
        var hairLayer = PortraitLayerCatalog.ClampIndex(craftSkill * 2 + recruitIdSeed, PortraitLayerCatalog.HairLayers.Length);
        var clothLayer = PortraitLayerCatalog.ClampIndex(combatSkill + craftSkill + recruitIdSeed, PortraitLayerCatalog.ClothLayers.Length);
        return (bodyLayer, raceLayer, hairLayer, clothLayer);
    }

    private static void ValidateVisualPools()
    {
        if (_visualPoolsValidated)
        {
            return;
        }

        _visualPoolsValidated = true;
        foreach (var path in CombatPortraitPool.Concat(CraftPortraitPool).Concat(RanchPortraitPool).Concat(BalancedPortraitPool).Distinct())
        {
            if (!Godot.FileAccess.FileExists(path))
            {
                GD.PushWarning($"Generated visual path missing: {path}");
            }
        }

        foreach (var path in PortraitLayerCatalog.RaceLayers
                     .Concat(PortraitLayerCatalog.HairLayers)
                     .Concat(PortraitLayerCatalog.ClothLayers)
                     .Concat(PortraitLayerCatalog.BodyLayers)
                     .Append(PortraitLayerCatalog.FaceLayer)
                     .Append(PortraitLayerCatalog.BackgroundLayer))
        {
            if (!Godot.FileAccess.FileExists(path))
            {
                GD.PushWarning($"Layered portrait asset missing: {path}");
            }
        }
    }
}

public sealed class SaveService
{
    private const long MaxSaveBytes = 4 * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        MaxDepth = 32,
        Converters = { new JsonStringEnumConverter() }
    };

    public bool Save(SaveState state, int slot)
    {
        var path = SavePath(slot);
        EnsureSaveDirectory();
        var json = JsonSerializer.Serialize(state, JsonOptions);
        if (Encoding.UTF8.GetByteCount(json) > MaxSaveBytes)
        {
            GD.PushError($"Save slot {slot} exceeds the maximum supported size.");
            return false;
        }

        var absolutePath = ProjectSettings.GlobalizePath(path);
        var temporaryPath = absolutePath + ".tmp";
        try
        {
            File.WriteAllText(temporaryPath, json, Encoding.UTF8);
            File.Move(temporaryPath, absolutePath, true);
            return true;
        }
        catch (Exception exception)
        {
            GD.PushError($"Could not write save slot {slot}: {exception.Message}");
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            return false;
        }
    }

    public bool HasSave(int slot) => Godot.FileAccess.FileExists(SavePath(slot));

    public SaveState? Load(int slot)
    {
        var path = SavePath(slot);
        if (!Godot.FileAccess.FileExists(path))
        {
            return null;
        }

        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
        if (file is null)
        {
            GD.PushError($"Could not open save slot {slot} for reading.");
            return null;
        }

        if ((long)file.GetLength() > MaxSaveBytes)
        {
            GD.PushError($"Save slot {slot} exceeds the maximum supported size.");
            return null;
        }

        try
        {
            var state = JsonSerializer.Deserialize<SaveState>(file.GetAsText(), JsonOptions);
            if (state is null)
            {
                GD.PushError($"Save slot {slot} was empty or invalid.");
                return null;
            }

            state = SaveMigrator.Migrate(state);
            if (state.SchemaVersion != SaveState.CurrentSchemaVersion)
            {
                GD.PushError($"Save slot {slot} schema {state.SchemaVersion} is not supported by schema {SaveState.CurrentSchemaVersion}.");
                return null;
            }

            return state;
        }
        catch (Exception exception)
        {
            GD.PushError($"Save slot {slot} could not be parsed: {exception.Message}");
            return null;
        }
    }

    public void Delete(int slot)
    {
        var path = SavePath(slot);
        if (!Godot.FileAccess.FileExists(path))
        {
            return;
        }

        DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(path));
    }

    private static string SavePath(int slot) => $"user://saves/slot{slot}.json";

    private static void EnsureSaveDirectory()
    {
        var absolutePath = ProjectSettings.GlobalizePath("user://saves");
        Directory.CreateDirectory(absolutePath);
    }
}

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
        state.Milestones.CompletedIds ??= new List<string>();
        state.Research.UnlockedSkillIds ??= new List<string>();
        state.Pets.AdoptedPetIds ??= new List<string>();
        state.Bond.CompletedEventIds ??= new List<string>();
        state.Adventure.LastCaptureSummary ??= string.Empty;
        foreach (var character in state.Roster.Characters)
        {
            if (string.IsNullOrWhiteSpace(character.BodyImagePathOverride) && !string.IsNullOrWhiteSpace(character.PortraitPathOverride))
            {
                character.BodyImagePathOverride = character.PortraitPathOverride;
            }

            if (string.IsNullOrWhiteSpace(character.BodyTypeOverride))
            {
                character.BodyTypeOverride = "Balanced";
            }

            character.BodyLayerIndex = PortraitLayerCatalog.ClampIndex(character.BodyLayerIndex, PortraitLayerCatalog.BodyLayers.Length);
            character.FaceLayerIndex = 0;
            character.RaceLayerIndex = PortraitLayerCatalog.ClampIndex(character.RaceLayerIndex, PortraitLayerCatalog.RaceLayers.Length);
            character.HairLayerIndex = PortraitLayerCatalog.ClampIndex(character.HairLayerIndex, PortraitLayerCatalog.HairLayers.Length);
            character.ClothLayerIndex = PortraitLayerCatalog.ClampIndex(character.ClothLayerIndex, PortraitLayerCatalog.ClothLayers.Length);
        }

        return state;
    }
}

public sealed class RosterService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;

    public RosterService(SaveState state, DataRegistry data)
    {
        _state = state;
        _data = data;
    }

    public IReadOnlyList<CharacterState> Characters => _state.Roster.Characters;

    public CharacterDefinition DefinitionFor(CharacterState character)
    {
        var baseDefinition = _data.Characters.TryGetValue(character.DefinitionId, out var found)
            ? found
            : new CharacterDefinition { Id = character.DefinitionId, DisplayName = character.DefinitionId, Trait = "Uncatalogued", MaxHp = Math.Max(character.Hp, 1), MaxEnergy = Math.Max(character.Energy, 1) };

        return new CharacterDefinition
        {
            Id = baseDefinition.Id,
            DisplayName = string.IsNullOrWhiteSpace(character.DisplayNameOverride) ? baseDefinition.DisplayName : character.DisplayNameOverride,
            PortraitPath = string.IsNullOrWhiteSpace(character.PortraitPathOverride) ? baseDefinition.PortraitPath : character.PortraitPathOverride,
            BodyImagePath = string.IsNullOrWhiteSpace(character.BodyImagePathOverride)
                ? (string.IsNullOrWhiteSpace(baseDefinition.BodyImagePath) ? baseDefinition.PortraitPath : baseDefinition.BodyImagePath)
                : character.BodyImagePathOverride,
            BodyType = string.IsNullOrWhiteSpace(character.BodyTypeOverride)
                ? (string.IsNullOrWhiteSpace(baseDefinition.BodyType) ? "Balanced" : baseDefinition.BodyType)
                : character.BodyTypeOverride,
            Trait = string.IsNullOrWhiteSpace(character.TraitOverride) ? baseDefinition.Trait : character.TraitOverride,
            MaxHp = character.MaxHpOverride ?? baseDefinition.MaxHp,
            MaxEnergy = character.MaxEnergyOverride ?? baseDefinition.MaxEnergy,
            RanchSkill = character.RanchSkill,
            CraftSkill = character.CraftSkill,
            CombatSkill = character.CombatSkill
        };
    }

    public CharacterState? Find(string characterId) => _state.Roster.Characters.FirstOrDefault(character => character.Id == characterId);
}

public sealed class ScheduleService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;

    public ScheduleService(SaveState state, DataRegistry data)
    {
        _state = state;
        _data = data;
    }

    public IReadOnlyList<JobDefinition> AssignableJobs => _data.AssignableJobs();

    public string GetAssignment(string characterId)
    {
        return _state.Schedule.AssignedJobs.TryGetValue(characterId, out var jobId) ? jobId : "rest";
    }

    public void AssignJob(string characterId, string jobId)
    {
        if (!_data.Jobs.ContainsKey(jobId))
        {
            return;
        }

        _state.Schedule.AssignedJobs[characterId] = jobId;
    }
}

public sealed class RanchService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;

    public RanchService(SaveState state, DataRegistry data)
    {
        _state = state;
        _data = data;
    }

    public IReadOnlyDictionary<string, int> Stockpile => _state.Ranch.Stockpile;
    public IReadOnlyDictionary<string, int> Facilities => _state.Ranch.Facilities;

    public bool UpgradeFacility(string facilityId, EconomyService economy)
    {
        if (!_data.Facilities.TryGetValue(facilityId, out var definition))
        {
            return false;
        }

        _state.Ranch.Facilities.TryGetValue(facilityId, out var currentLevel);
        var nextLevel = currentLevel + 1;
        var cost = FacilityUpgradeCost(definition, currentLevel);
        if (!economy.Spend(cost))
        {
            return false;
        }

        _state.Ranch.Facilities[facilityId] = nextLevel;
        return true;
    }

    public int FacilityUpgradeCost(FacilityDefinition definition, int currentLevel)
    {
        return definition.BuildCost + Math.Max(0, currentLevel) * 75;
    }

    public int FacilityUpkeep()
    {
        var upkeep = 0;
        foreach (var facility in _state.Ranch.Facilities)
        {
            if (_data.Facilities.TryGetValue(facility.Key, out var definition))
            {
                upkeep += definition.UpkeepGold * Math.Max(1, facility.Value);
            }
        }

        return upkeep;
    }

    public int ApplyJobOutput(CharacterState character, JobDefinition job, DailyReport report)
    {
        if (string.IsNullOrWhiteSpace(job.ResourceId))
        {
            report.Lines.Add($"{character.Id} rested and recovered.");
            return 0;
        }

        var skillBonus = job.Category == JobCategory.Adventure ? character.CombatSkill : character.RanchSkill;
        var researchBonus = _state.Research.UnlockedSkillIds.Contains("ranch_planning") && job.Category != JobCategory.Rest ? 2 : 0;
        var amount = Math.Max(0, job.ResourceAmount + skillBonus / 2) + researchBonus;
        _state.Ranch.Stockpile.TryGetValue(job.ResourceId, out var currentAmount);
        _state.Ranch.Stockpile[job.ResourceId] = currentAmount + amount;
        report.Lines.Add($"{character.Id} completed {job.DisplayName}, adding {amount} {job.ResourceId}.");
        return job.GoldIncome + amount * 3;
    }
}

public sealed class EconomyService
{
    private readonly SaveState _state;

    public EconomyService(SaveState state)
    {
        _state = state;
    }

    public int Gold => _state.Economy.Gold;

    public bool Spend(int amount)
    {
        if (amount < 0 || _state.Economy.Gold < amount)
        {
            return false;
        }

        _state.Economy.Gold -= amount;
        return true;
    }

    public void AddGold(int amount)
    {
        _state.Economy.Gold += Math.Max(0, amount);
    }

    public void ApplySettlement(int income, int expenses)
    {
        _state.Economy.LastIncome = income;
        _state.Economy.LastExpenses = expenses;
        _state.Economy.Gold += income - expenses;
    }
}

public sealed class DayCycleService
{
    private readonly SaveState _state;

    public DayCycleService(SaveState state)
    {
        _state = state;
    }

    public void AdvanceToNextDay()
    {
        _state.Calendar.Day += 1;
        _state.Calendar.Phase = DayPhase.Morning;
    }

    public bool AdvancePhase()
    {
        if (_state.Calendar.Phase == DayPhase.Night)
        {
            return false;
        }

        _state.Calendar.Phase = _state.Calendar.Phase switch
        {
            DayPhase.Morning => DayPhase.Afternoon,
            DayPhase.Afternoon => DayPhase.Evening,
            DayPhase.Evening => DayPhase.Night,
            _ => DayPhase.Night
        };
        return true;
    }
}

public sealed class DailySettlementService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private readonly ScheduleService _schedule;
    private readonly RanchService _ranch;
    private readonly EconomyService _economy;
    private readonly DayCycleService _dayCycle;
    private readonly MilestoneService _milestones;

    public DailySettlementService(SaveState state, DataRegistry data, ScheduleService schedule, RanchService ranch, EconomyService economy, DayCycleService dayCycle, MilestoneService milestones)
    {
        _state = state;
        _data = data;
        _schedule = schedule;
        _ranch = ranch;
        _economy = economy;
        _dayCycle = dayCycle;
        _milestones = milestones;
    }

    public DailyReport SettleDay()
    {
        var report = new DailyReport { Day = _state.Calendar.Day };
        var income = 0;

        foreach (var character in _state.Roster.Characters)
        {
            var jobId = _schedule.GetAssignment(character.Id);
            var job = _data.Jobs.TryGetValue(jobId, out var foundJob) ? foundJob : _data.Job("rest");
            income += _ranch.ApplyJobOutput(character, job, report);
            character.Fatigue = Math.Clamp(character.Fatigue + job.FatigueDelta, 0, 100);
            character.Morale = Math.Clamp(character.Morale + job.MoraleDelta, 0, 100);
            character.Bond = Math.Clamp(character.Bond + job.BondDelta, 0, 100);
        }

        var expenses = _ranch.FacilityUpkeep() + PetCareCost();
        _economy.ApplySettlement(income, expenses);
        report.Income = income;
        report.Expenses = expenses;
        report.NetGold = income - expenses;
        report.Lines.Add($"Facility upkeep cost {expenses} gold.");
        _milestones.CheckAfterSettlement(report);
        _dayCycle.AdvanceToNextDay();
        return report;
    }

    private int PetCareCost()
    {
        return _state.Pets.AdoptedPetIds.Sum(petId => _data.Pets.TryGetValue(petId, out var pet) ? pet.CareCost : 0);
    }
}

// Additional gameplay services were moved to ManagementServices.cs to keep this file focused.