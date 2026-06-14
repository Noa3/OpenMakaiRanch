using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

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

        if (character.Mature.FallState == FallState.Collapse)
        {
            report.Lines.Add($"{character.Id} is collapsed and unable to work.");
            return 0;
        }

        var fatiguePenalty = character.Fatigue switch
        {
            >= 80 => 0.5f,
            >= 60 => 0.75f,
            >= 40 => 0.9f,
            _ => 1.0f
        };

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

        if (fatiguePenalty < 1f)
        {
            int lost = amount - (int)(amount * fatiguePenalty);
            if (lost > 0)
            {
                amount -= lost;
                gold = job.GoldIncome + amount * 3;
                report.Lines.Add($"{character.DisplayNameOverride}'s fatigue reduced output by {lost} {job.ResourceId}.");
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
