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
        return state;
    }

    // ... (rest of the original SaveStateFactory implementation)
}

public readonly record struct PortraitLayerFrame(string Path, int X, int Y, int Width, int Height, int OffsetX, int OffsetY);