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
    // Persisted layer indices depend on array order. Keep existing entries stable and append new ones.
    public static readonly PortraitLayerFrame[] RaceLayers =
    {
        new("res://assets/portrait_layers/race_00.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_01.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_02.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_03.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_04.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_05.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_06.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_07.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_08.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_09.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_10.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_11.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_20.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_21.png", 0, 0, 48, 48, 0, -16)
    };

    public static readonly PortraitLayerFrame[] HairLayers =
    {
        new("res://assets/portrait_layers/hair_00.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_01.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_02.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_03.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_04.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_05.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_06.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_07.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_08.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_09.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_10.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_11.png", 0, 32, 48, 112, 0, -2)
    };

    // First six cloth entries preserve the historical order used by existing save data.
    public static readonly PortraitLayerFrame[] ClothLayers =
    {
        new("res://assets/portrait_layers/cloth/cloth_320_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_321.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_322_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_324_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_329_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_364_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/accessory_01.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/accessory_02.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/accessory_02_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/accessory_03.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_1462.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_1462_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_302.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_302_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_320.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_322.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_324.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_329.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_363.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_364.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_380.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_381.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_382.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_383.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_401.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_401_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_402.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_402_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_403.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_403_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_404.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_405.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_406.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_407.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_407_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_408.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_408_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_409.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_409_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_410.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_410_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_411.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_411_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_412.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_412_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_413.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_413_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_414.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_414_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_415.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_415_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_437.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_437_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_439.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_439_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_460.png", 0, 0, 64, 112, -8, 0)
    };

    public static readonly PortraitLayerFrame[] BodyLayers =
    {
        new("res://assets/portrait_layers/body.png", 0, 0, 48, 112, 0, 0),
        new("res://assets/portrait_layers/body2.png", 0, 0, 80, 112, -25, 0)
    };

    public static readonly PortraitLayerFrame FaceLayer = new("res://assets/portrait_layers/face.png", 0, 0, 8, 16, 14, 14, 6f);
    // Legacy mouth frames use the same draw offset; their opaque pixels sit lower inside the transparent 16x16 crop.
    public static readonly PortraitLayerFrame MouthLayer = new("res://assets/portrait_layers/face.png", 0, 384, 16, 16, 14, 14, 5f);
    public const string BackgroundLayer = "res://assets/portrait_layers/bg.png";

    public static IEnumerable<string> AllLayerPaths()
    {
        return RaceLayers.Concat(HairLayers)
            .Concat(ClothLayers)
            .Concat(BodyLayers)
            .Append(FaceLayer)
            .Append(MouthLayer)
            .Select(frame => frame.Path)
            .Append(BackgroundLayer)
            .Distinct(StringComparer.Ordinal);
    }

    public static int ClampIndex(int index, int size)
    {
        if (size <= 0)
        {
            return 0;
        }

        return ((index % size) + size) % size;
    }
}

public readonly record struct PortraitLayerFrame(string Path, int X, int Y, int Width, int Height, int OffsetX, int OffsetY, float Scale = 1f);

public sealed class SaveStateFactory
{
    private readonly DataRegistry _data;
    private readonly Random _random;
    private const int MinimumGeneratedHp = 40;
    private const int MinimumGeneratedEnergy = 35;
    private const int HighCombatBodyThreshold = 6;
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

        // Discover first 3 local missions
        var localMissions = _data.Missions.Values.Where(m => m.Tier == MissionTier.Local).OrderBy(m => m.Difficulty).ToList();
        for (int i = 0; i < Math.Min(3, localMissions.Count); i++)
            state.Adventure.DiscoveredMissionIds.Add(localMissions[i].Id);

        state.Player = new PlayerState
        {
            Name = "Anon",
            Race = "Demonfolk",
            RanchName = "Okachi Ranch",
            Gender = "Male",
            Height = 1900,
            ApparentAge = 20,
            BodyShape = "Standard",
            SkinColor = "Standard",
            HairColor = "Black",
            HairStyle = "Short",
            HairFeature = "None",
            EyeColor = "Red",
            EyeShape = "Standard",
            Job = "Dairy Farmer",
            Personality = "Quiet",
            HasHorns = true,
            HasGlasses = false,
            BustSize = "Flat",
            BreastFirmness = "Firm",
            FirstPersonPronoun = "I"
        };

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
        var (givenName, familyName) = CharacterGenerationPools.GenerateName(_random);
        var bustSize = CharacterGenerationPools.PickBreastSize(_random);
        var heightMm = CharacterGenerationPools.GenerateHeight(_random);
        var apparentAge = CharacterGenerationPools.GenerateApparentAge(_random);
        var race = CharacterGenerationPools.PickRace(_random);
        var talents = CharacterGenerationPools.GenerateTalents(_random, 3);
        var bodyTypeLabel = CharacterGenerationPools.BodyTypes[_random.Next(CharacterGenerationPools.BodyTypes.Length)];
        var skinColor = CharacterGenerationPools.SkinColors[_random.Next(CharacterGenerationPools.SkinColors.Length)];
        var hairColor = CharacterGenerationPools.HairColors[_random.Next(CharacterGenerationPools.HairColors.Length)];
        var hairStyle = CharacterGenerationPools.HairStyles[_random.Next(CharacterGenerationPools.HairStyles.Length)];
        var eyeColor = CharacterGenerationPools.EyeColors[_random.Next(CharacterGenerationPools.EyeColors.Length)];
        var personality = CharacterGenerationPools.Personalities[_random.Next(CharacterGenerationPools.Personalities.Length)];
        return new CharacterState
        {
            Id = recruitId,
            DefinitionId = selectedArchetype.Id,
            IsGenerated = true,
            DisplayNameOverride = $"{givenName} {familyName}",
            PortraitPathOverride = portraitPath,
            BodyImagePathOverride = bodyImagePath,
            BodyTypeOverride = bodyType,
            BodyLayerIndex = bodyLayer,
            RaceLayerIndex = raceLayer,
            HairLayerIndex = hairLayer,
            ClothLayerIndex = clothLayer,
            FaceLayerIndex = 0,
            TraitOverride = personality,
            MaxHpOverride = maxHp,
            MaxEnergyOverride = maxEnergy,
            Hp = maxHp,
            Energy = maxEnergy,
            RanchSkill = ranchSkill,
            CraftSkill = craftSkill,
            CombatSkill = combatSkill,
            Morale = 55,
            Bond = _random.Next(0, 5),
            Race = race,
            ApparentAge = apparentAge,
            Height = heightMm,
            SkinColor = skinColor,
            HairColor = hairColor,
            HairStyle = hairStyle,
            EyeColor = eyeColor,
            JobClass = CharacterGenerationPools.PickJob(_random),
            Personality = personality,
            BustSize = bustSize,
            Talents = talents
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
        // Mix seed and skill values so expanded layer pools are sampled more evenly.
        var mixedSeed = (recruitIdSeed * 73) + (ranchSkill * 31) + (craftSkill * 53) + (combatSkill * 97);
        var raceLayer = PortraitLayerCatalog.ClampIndex(mixedSeed + (ranchSkill * 17), PortraitLayerCatalog.RaceLayers.Length);
        var hairLayer = PortraitLayerCatalog.ClampIndex((mixedSeed * 3) + (craftSkill * 19), PortraitLayerCatalog.HairLayers.Length);
        var clothLayer = PortraitLayerCatalog.ClampIndex((mixedSeed * 5) + (combatSkill * 23), PortraitLayerCatalog.ClothLayers.Length);
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

        foreach (var path in PortraitLayerCatalog.AllLayerPaths())
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

            character.BodyLayerIndex = PortraitLayerCatalog.ClampIndex(character.BodyLayerIndex, PortraitLayerCatalog.BodyLayers.Length);
            character.FaceLayerIndex = 0;
            character.RaceLayerIndex = PortraitLayerCatalog.ClampIndex(character.RaceLayerIndex, PortraitLayerCatalog.RaceLayers.Length);
            character.HairLayerIndex = PortraitLayerCatalog.ClampIndex(character.HairLayerIndex, PortraitLayerCatalog.HairLayers.Length);
            character.ClothLayerIndex = PortraitLayerCatalog.ClampIndex(character.ClothLayerIndex, PortraitLayerCatalog.ClothLayers.Length);

            // NSFW migration - initialize mature state if missing
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
    private readonly EquipmentService _equipment;
    private readonly TalentService _talents;

    public RanchService(SaveState state, DataRegistry data, EquipmentService equipment, TalentService talents)
    {
        _state = state;
        _data = data;
        _equipment = equipment;
        _talents = talents;
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
        var logistics = _state.Research.UnlockedSkillIds.Contains("logistics");
        foreach (var facility in _state.Ranch.Facilities)
        {
            if (_data.Facilities.TryGetValue(facility.Key, out var definition))
            {
                upkeep += definition.UpkeepGold * Math.Max(1, facility.Value);
            }
        }

        if (logistics && upkeep > 0)
        {
            int saved = Math.Max(1, upkeep / 4);
            upkeep -= saved;
        }

        return upkeep;
    }

    public void ApplyAutomation(DailyReport report)
    {
        if (!_state.Research.UnlockedSkillIds.Contains("ranch_automation"))
            return;

        int totalBonus = 0;
        foreach (var facility in _state.Ranch.Facilities)
        {
            if (facility.Value <= 0) continue;
            int bonus = facility.Value * 2;
            _state.Ranch.Stockpile["farm_goods"] = _state.Ranch.Stockpile.GetValueOrDefault("farm_goods") + bonus;
            totalBonus += bonus;
        }
        if (totalBonus > 0)
            report.Lines.Add($"Automated Feeding produced {totalBonus} bonus farm goods from facilities.");
    }

    public int ApplyJobOutput(CharacterState character, JobDefinition job, DailyReport report)
    {
        if (string.IsNullOrWhiteSpace(job.ResourceId))
        {
            report.Lines.Add($"{character.Id} rested and recovered.");
            return 0;
        }

        var equipRanch = _equipment.BonusRanchSkill(character.Id);
        var equipCombat = _equipment.BonusCombatSkill(character.Id);
        var talentRanch = _talents.BonusRanchSkill(character.Id);
        var talentCombat = _talents.BonusCombatSkill(character.Id);
        var effectiveRanch = character.RanchSkill + equipRanch + talentRanch;
        var effectiveCombat = character.CombatSkill + equipCombat + talentCombat;
        var skillBonus = job.Category == JobCategory.Adventure ? effectiveCombat : effectiveRanch;
        var researchBonus = _state.Research.UnlockedSkillIds.Contains("ranch_planning") && job.Category != JobCategory.Rest ? 2 : 0;
        var amount = Math.Max(0, job.ResourceAmount + skillBonus / 2) + researchBonus;
        var gold = job.GoldIncome + amount * 3;

        var talentMult = _talents.JobOutputMultiplier(character.Id);
        if (talentMult < 1f)
        {
            int penalty = amount - (int)(amount * talentMult);
            if (penalty > 0)
            {
                amount -= penalty;
                gold = job.GoldIncome + amount * 3;
            }
        }
        else if (talentMult > 1f)
        {
            int bonus = (int)(amount * (talentMult - 1f));
            if (bonus > 0)
            {
                amount += bonus;
                gold += bonus * 3;
            }
        }

        if (_state.Research.UnlockedSkillIds.Contains("dairy_science") && job.Category == JobCategory.Dairy)
        {
            int bonus = amount / 2;
            amount += bonus;
            gold += bonus * 4;
            report.Lines.Add($"Dairy Science: +{bonus} bonus farm goods from improved techniques.");
        }

        if (_state.Research.UnlockedSkillIds.Contains("culinary_arts") && job.Category == JobCategory.Cooking)
        {
            int bonus = amount / 3 + 1;
            amount += bonus;
            report.Lines.Add($"Culinary Arts: produced {bonus} extra meals from expert cooking.");
        }

        if (_state.Research.UnlockedSkillIds.Contains("herbalism") && job.Category == JobCategory.Pharmacy)
        {
            int bonus = amount / 3 + 1;
            amount += bonus;
            gold += bonus * 5;
            report.Lines.Add($"Herbalism: +{bonus} bonus supplies from herbal remedies.");
        }

        if (_state.Research.UnlockedSkillIds.Contains("hospitality") && job.Category == JobCategory.CustomerService)
        {
            int bonus = amount / 2 + 1;
            amount += bonus;
            gold += 15;
            report.Lines.Add($"Hospitality: superior service earned extra comfort ({bonus}) and tips (+15g).");
        }

        if (_state.Research.UnlockedSkillIds.Contains("craftsmanship") && job.Category == JobCategory.Chore)
        {
            int bonus = amount / 3 + 1;
            amount += bonus;
            gold += bonus * 3;
            report.Lines.Add($"Craftsmanship: +{bonus} bonus output from skilled workshop work.");
        }

        _state.Ranch.Stockpile.TryGetValue(job.ResourceId, out var currentAmount);
        _state.Ranch.Stockpile[job.ResourceId] = currentAmount + amount;
        var displayName = !string.IsNullOrWhiteSpace(character.DisplayNameOverride) ? character.DisplayNameOverride : character.Id;
        report.Lines.Add($"{displayName} completed {job.DisplayName}, adding {amount} {job.ResourceId}.");
        return gold;
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
    private readonly DailyEventService _events;
    private readonly CharacterGrowthService _growth;
    private readonly ResourceConsumptionService _resources;
    private readonly InventoryService _inventory;
    private readonly MilkEconomyService _milkEconomy;
    private readonly TalentService _talents;

    public DailySettlementService(SaveState state, DataRegistry data, ScheduleService schedule, RanchService ranch, EconomyService economy, DayCycleService dayCycle, MilestoneService milestones, InventoryService inventory, TalentService talents)
    {
        _state = state;
        _data = data;
        _schedule = schedule;
        _ranch = ranch;
        _economy = economy;
        _dayCycle = dayCycle;
        _milestones = milestones;
        _events = new DailyEventService(state, data, economy);
        _growth = new CharacterGrowthService(state, talents);
        _resources = new ResourceConsumptionService(state, data);
        _inventory = inventory;
        _milkEconomy = new MilkEconomyService(state);
        _talents = talents;
    }

    public DailyReport SettleDay()
    {
        var report = new DailyReport { Day = _state.Calendar.Day };
        var income = 0;

        // 1. Resource consumption (meals, supplies) before work
        _resources.ConsumeResources(report);

        // 2. Process each character's assigned job
        foreach (var character in _state.Roster.Characters)
        {
            var jobId = _schedule.GetAssignment(character.Id);
            var job = _data.Jobs.TryGetValue(jobId, out var foundJob) ? foundJob : _data.Job("rest");
            income += _ranch.ApplyJobOutput(character, job, report);
            var fatigueResistance = _talents.FatigueResistance(character.Id);
            character.Fatigue = Math.Clamp(character.Fatigue + Math.Max(0, job.FatigueDelta - fatigueResistance), 0, 100);
            character.Morale = Math.Clamp(character.Morale + job.MoraleDelta, 0, 100);
            character.Bond = Math.Clamp(character.Bond + job.BondDelta, 0, 100);
        }

        // 3. Calculate expenses
        var expenses = _ranch.FacilityUpkeep() + PetCareCost();
        _economy.ApplySettlement(income, expenses);
        report.Income = income;
        report.Expenses = expenses;
        report.NetGold = income - expenses;
        report.Lines.Add($"Facility upkeep cost {expenses} gold.");

        // 3b. Facility automation bonus
        _ranch.ApplyAutomation(report);

        // 4. Daily milk production (pre-shipment)
        int milkRevenue = 0;
        foreach (var character in _state.Roster.Characters)
        {
            _milkEconomy.ProduceMilk(character.Id);
            // Auto-ship milk each day
            milkRevenue += _milkEconomy.ShipMilk(character.Id);
        }
        if (milkRevenue > 0)
        {
            report.MilkRevenue = milkRevenue;
            report.NetGold += milkRevenue;
            report.Lines.Add($"Auto-shipped milk for {milkRevenue} gold.");
        }

        // 5. Daily random events
        _events.GenerateEvents(report);

        // 6. Character growth
        _growth.ApplyGrowth(report);

        // 7. Check milestones
        _milestones.CheckAfterSettlement(report);

        // 8. Advance calendar
        _dayCycle.AdvanceToNextDay();

        // 9. Discover missions every 3 days
        var discovered = _state.Adventure.DiscoveredMissionIds.Count;
        var total = _data.Missions.Count;
        if (discovered < total && _state.Calendar.Day % 3 == 0)
        {
            var next = _data.Missions.Values
                .Where(m => !_state.Adventure.DiscoveredMissionIds.Contains(m.Id))
                .OrderBy(m => m.Difficulty)
                .FirstOrDefault();
            if (next is not null)
            {
                _state.Adventure.DiscoveredMissionIds.Add(next.Id);
                report.Lines.Add($"Scouted a new mission location: {next.DisplayName}");
            }
        }

        // 10. Refresh mercenaries
        _state.Adventure.AvailableMercenaries.Clear();
        _state.Adventure.ActiveMercenaryHpBonus = 0;

        return report;
    }

    private int PetCareCost()
    {
        return _state.Pets.AdoptedPetIds.Sum(petId => _data.Pets.TryGetValue(petId, out var pet) ? pet.CareCost : 0);
    }
}

// Additional gameplay services were moved to ManagementServices.cs to keep this file focused.