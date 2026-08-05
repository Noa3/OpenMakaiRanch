using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

/// <summary>
/// Research skill tree with dependencies, costs, and cooldowns.
/// Maps to the 12+ research skills defined in data/skills.json.
/// Extends the basic ResearchState in SaveModels.cs with full tree logic.
/// </summary>
public sealed class ResearchTreeService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private readonly EconomyService _economy;
    private readonly Random _random;

    public ResearchTreeService(SaveState state, DataRegistry data, EconomyService economy, Random? random = null)
    {
        _state = state;
        _data = data;
        _economy = economy;
        _random = random ?? Random.Shared;
    }

    public IReadOnlyList<string> UnlockedSkills => _state.Research.UnlockedSkillIds;

    public bool IsUnlocked(string skillId) => _state.Research.UnlockedSkillIds.Contains(skillId);

    public bool CanUnlock(string skillId)
    {
        if (!_data.Skills.TryGetValue(skillId, out var skill))
            return false;

        if (_state.Research.UnlockedSkillIds.Contains(skillId))
            return false;

        // Check cost
        if (_economy.Gold < skill.CostAmount)
            return false;

        // Check cooldown
        if (_state.Research.LastResearchDay + 3 > _state.Calendar.Day + 1)
            return false;

        return true;
    }

    public bool UnlockSkill(string skillId)
    {
        if (!CanUnlock(skillId))
            return false;

        var skill = _data.Skills[skillId];
        _economy.Spend(skill.CostAmount);
        _state.Research.UnlockedSkillIds.Add(skillId);
        _state.Research.LastResearchDay = _state.Calendar.Day;

        return true;
    }

    public List<SkillDefinition> AvailableSkills()
    {
        return _data.Skills.Values
            .Where(s => !IsUnlocked(s.Id))
            .OrderBy(s => s.CostAmount)
            .ToList();
    }

    public List<SkillDefinition> LockedSkills()
    {
        return _data.Skills.Values
            .Where(s => !IsUnlocked(s.Id))
            .ToList();
    }

    public (int daysLeft, int cost) GetCooldownInfo(string skillId)
    {
        if (!_data.Skills.TryGetValue(skillId, out var skill))
            return (0, 0);

        var daysLeft = Math.Max(0, _state.Research.LastResearchDay + 3 - _state.Calendar.Day - 1);
        return (daysLeft, skill.CostAmount);
    }
}
