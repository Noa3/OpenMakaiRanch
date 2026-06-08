using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

public sealed class TalentService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;

    public TalentService(SaveState state, DataRegistry data)
    {
        _state = state;
        _data = data;
    }

    public int BonusRanchSkill(string characterId)
    {
        return SumBonuses(characterId, t => t.BonusRanchSkill);
    }

    public int BonusCraftSkill(string characterId)
    {
        return SumBonuses(characterId, t => t.BonusCraftSkill);
    }

    public int BonusCombatSkill(string characterId)
    {
        return SumBonuses(characterId, t => t.BonusCombatSkill);
    }

    public int BonusMaxHp(string characterId)
    {
        return SumBonuses(characterId, t => t.BonusMaxHp);
    }

    public int BonusMaxEnergy(string characterId)
    {
        return SumBonuses(characterId, t => t.BonusMaxEnergy);
    }

    public float GrowthMultiplier(string characterId)
    {
        return ProductBonuses(characterId, t => t.GrowthMultiplier);
    }

    public float JobOutputMultiplier(string characterId)
    {
        return ProductBonuses(characterId, t => t.JobOutputMultiplier);
    }

    public float TrainingEfficiency(string characterId)
    {
        return ProductBonuses(characterId, t => t.TrainingEfficiency);
    }

    public int MoraleCapBonus(string characterId)
    {
        return SumBonuses(characterId, t => t.MoraleCapBonus);
    }

    public int FatigueResistance(string characterId)
    {
        return SumBonuses(characterId, t => t.FatigueResistance);
    }

    private int SumBonuses(string characterId, System.Func<TalentDefinition, int> selector)
    {
        var character = _state.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null || character.Talents is null) return 0;

        int total = 0;
        foreach (var talentId in character.Talents)
        {
            if (_data.Talents.TryGetValue(talentId, out var talent))
                total += selector(talent);
        }
        return total;
    }

    private float ProductBonuses(string characterId, System.Func<TalentDefinition, float> selector)
    {
        var character = _state.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null || character.Talents is null) return 1f;

        float product = 1f;
        foreach (var talentId in character.Talents)
        {
            if (_data.Talents.TryGetValue(talentId, out var talent))
                product *= selector(talent);
        }
        return product;
    }
}
