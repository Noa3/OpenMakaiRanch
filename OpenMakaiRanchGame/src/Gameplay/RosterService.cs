using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

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
