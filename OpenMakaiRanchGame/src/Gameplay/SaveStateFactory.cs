using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

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
        var startingIds = new[] { "anon", "rancher" };
        foreach (var definition in _data.CharacterList().Where(d => startingIds.Contains(d.Id)))
        {
            var idSeed = definition.Id.Sum(value => value);
            var (bodyLayer, raceLayer, hairLayer, clothLayer, skinColorIdx, breastSizeIdx) = BuildLayerSelection(definition.RanchSkill, definition.CraftSkill, definition.CombatSkill, idSeed);
            state.Roster.Characters.Add(new CharacterState
            {
                Id = definition.Id,
                DefinitionId = definition.Id,
                Hp = definition.MaxHp,
                Energy = definition.MaxEnergy,
                RanchSkill = definition.RanchSkill,
                CraftSkill = definition.CraftSkill,
                CombatSkill = definition.CombatSkill,
                MagicPower = definition.MagicPower,
                BodyLayerIndex = bodyLayer,
                SkinColorIndex = skinColorIdx,
                BreastSizeIndex = breastSizeIdx,
                RaceLayerIndex = raceLayer,
                HairLayerIndex = hairLayer,
                ClothLayerIndex = clothLayer,
                FaceLayerIndex = 0
            });
            state.Schedule.AssignedJobs[definition.Id] = "rest";
        }

        var existingIds = state.Roster.Characters.Select(character => character.Id).ToHashSet();
        state.Ranch.Facilities["pasture"] = 1;
        state.Ranch.Facilities["kitchen"] = 1;
        state.Ranch.Stockpile["farm_goods"] = 0;
        state.Ranch.Stockpile["meals"] = 0;
        state.Ranch.Stockpile["supplies"] = 3;
        state.Adventure.SelectedPartyIds.AddRange(state.Roster.Characters.Take(state.Roster.Characters.Count).Select(character => character.Id));
        state.Inventory.Items["meal_box"] = 2;
        state.Recruitment.CurrentOffer = CreateGeneratedRecruit(state);

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

        // Adopt starting pet
        if (!string.IsNullOrWhiteSpace(state.Player.StartingPetId) && state.Player.StartingPetId != "none" &&
            _data.Pets.ContainsKey(state.Player.StartingPetId))
        {
            state.Pets.AdoptedPetIds.Add(state.Player.StartingPetId);
            state.Pets.Entries[state.Player.StartingPetId] = new PetEntryState();
        }

        // Adopt starting mount
        if (!string.IsNullOrWhiteSpace(state.Player.StartingMountId) && state.Player.StartingMountId != "none" &&
            _data.Pets.ContainsKey(state.Player.StartingMountId) &&
            !state.Pets.AdoptedPetIds.Contains(state.Player.StartingMountId))
        {
            state.Pets.AdoptedPetIds.Add(state.Player.StartingMountId);
            state.Pets.Entries[state.Player.StartingMountId] = new PetEntryState();
        }

        state.Calendar.CurrentWeather = _random.NextDouble() switch
        {
            < 0.40 => Weather.Clear,
            < 0.70 => Weather.Cloudy,
            < 0.90 => Weather.Rain,
            _ => Weather.Storm
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
        var (bodyLayer, raceLayer, hairLayer, clothLayer, _, _) = BuildLayerSelection(ranchSkill, craftSkill, combatSkill, recruitIdSeed: _random.Next());

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
        var magicPower = selectedArchetype.MagicPower + _random.Next(0, 4);
        var genSkinIdx = PortraitLayerCatalog.MapSkinColorToIndex(skinColor);
        var genBreastIdx = PortraitLayerCatalog.MapBustSizeToBreastIndex(bustSize);
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
            SkinColorIndex = genSkinIdx,
            BreastSizeIndex = genBreastIdx,
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
            MagicPower = magicPower,
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

    private static (int BodyLayer, int RaceLayer, int HairLayer, int ClothLayer, int SkinColorIndex, int BreastSizeIndex) BuildLayerSelection(int ranchSkill, int craftSkill, int combatSkill, int recruitIdSeed)
    {
        var baseBodyTier = combatSkill >= HighCombatBodyThreshold ? 1 : 0;
        var bodyLayer = PortraitLayerCatalog.ClampIndex(baseBodyTier + recruitIdSeed, PortraitLayerCatalog.BodyTypeCount);
        var mixedSeed = (recruitIdSeed * 73) + (ranchSkill * 31) + (craftSkill * 53) + (combatSkill * 97);
        var raceLayer = PortraitLayerCatalog.ClampIndex(mixedSeed + (ranchSkill * 17), PortraitLayerCatalog.RaceLayers.Length);
        var hairLayer = PortraitLayerCatalog.ClampIndex((mixedSeed * 3) + (craftSkill * 19), PortraitLayerCatalog.HairLayers.Length);
        var clothLayer = PortraitLayerCatalog.ClampIndex((mixedSeed * 5) + (combatSkill * 23), PortraitLayerCatalog.ClothLayers.Length);
        var skinColorIdx = PortraitLayerCatalog.ClampIndex((mixedSeed * 7) + recruitIdSeed, PortraitLayerCatalog.SkinColorCount);
        var breastSizeIdx = PortraitLayerCatalog.ClampIndex((mixedSeed * 11) + (craftSkill * 13), PortraitLayerCatalog.BreastSizeCount);
        return (bodyLayer, raceLayer, hairLayer, clothLayer, skinColorIdx, breastSizeIdx);
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
