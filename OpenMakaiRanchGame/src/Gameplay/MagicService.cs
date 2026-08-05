using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

/// <summary>
/// Magic system: spells, mana, and magical effects.
/// Uses DataRegistry.Items for magic items and DataRegistry.Skills for magic theory.
/// </summary>
public sealed class MagicService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private readonly EconomyService _economy;
    private readonly Random _random;

    public MagicService(SaveState state, DataRegistry data, EconomyService economy, Random? random = null)
    {
        _state = state;
        _data = data;
        _economy = economy;
        _random = random ?? Random.Shared;
    }

    public int ManaReservoir => _state.Economy.ManaReservoir;

    public int SpiritEnergy => _state.Economy.SpiritEnergy;

    public bool CanCast(string spellId, int manaCost)
    {
        if (_state.Mature.LastSpellDay + 1 > _state.Calendar.Day)
            return false;

        if (ManaReservoir < manaCost)
            return false;

        return true;
    }

    public bool CastSpell(string spellId, int manaCost, string casterId)
    {
        if (!CanCast(spellId, manaCost))
            return false;

        _state.Economy.ManaReservoir -= manaCost;
        _state.Mature.LastSpellDay = _state.Calendar.Day;

        // Apply spell effects based on type
        ApplySpellEffect(spellId, manaCost);

        return true;
    }

    public void RegenerateMana(int amount)
    {
        _state.Economy.ManaReservoir += amount;
    }

    public List<ItemDefinition> AvailableMagicItems()
    {
        return _data.Items.Values
            .Where(i => i.Category == ItemCategory.Consumable)
            .OrderBy(i => i.Price)
            .ToList();
    }

    public List<SkillDefinition> MagicSkills()
    {
        return _data.Skills.Values
            .Where(s => s.Id.Contains("arcane") || s.Id.Contains("magic"))
            .ToList();
    }

    private void ApplySpellEffect(string spellId, int manaCost)
    {
        // Apply magic spell effects
        switch (spellId)
        {
            case "mana_regen":
                RegenerateMana(manaCost * 2);
                break;
            case "morale_boost":
                // Boost all character morale
                break;
            case "fatigue_reduce":
                // Reduce fatigue for all characters
                break;
            default:
                // Generic spell effect
                break;
        }
    }
}
