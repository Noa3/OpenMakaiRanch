using System;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;

namespace OpenMakaiRanch.Gameplay;

/// <summary>Current-condition gross work output, not a promise of a complete day's net balance.</summary>
public sealed record JobOutputPreview
{
    public int Amount { get; init; }
    public int Gold { get; init; }
    public int SkillContribution { get; init; }
    public int PlanningBonus { get; init; }
    public float TalentMultiplier { get; init; } = 1;
    public float FatigueMultiplier { get; init; } = 1;
    public int FatigueLost { get; init; }
    public int DairyBonus { get; init; }
    public int CulinaryBonus { get; init; }
    public int HerbalBonus { get; init; }
    public int HospitalityBonus { get; init; }
    public int CraftBonus { get; init; }
    public bool IsRest { get; init; }
    public bool IsCollapsed { get; init; }
    public int SpecialistBonus => DairyBonus + CulinaryBonus + HerbalBonus + HospitalityBonus + CraftBonus;
}

public sealed partial class RanchService
{
    // ApplyJobOutput calls this same calculator immediately before committing stock and payment.
    // Planning never normalizes saves, pays rewards, cleans the bath or advances the clock.
    public JobOutputPreview PreviewJobOutput(CharacterState character, JobDefinition job)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(job);
        if (string.IsNullOrWhiteSpace(job.ResourceId)) return new() { IsRest = true };
        if (character.Mature.FallState == FallState.Collapse) return new() { IsCollapsed = true };
        var fatigueLost = 0;
        var dairyBonus = 0; var culinaryBonus = 0; var herbalBonus = 0;
        var hospitalityBonus = 0; var craftBonus = 0;
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
                fatigueLost = lost;
                amount -= lost;
                gold = job.GoldIncome + amount * 3;
            }
        }

        if (_state.Research.UnlockedSkillIds.Contains("dairy_science") && job.Category == JobCategory.Dairy)
        {
            int bonus = amount / 2;
            dairyBonus = bonus;
            amount += bonus;
            gold += bonus * 4;
        }

        if (_state.Research.UnlockedSkillIds.Contains("culinary_arts") && job.Category == JobCategory.Cooking)
        {
            int bonus = amount / 3 + 1;
            culinaryBonus = bonus;
            amount += bonus;
        }

        if (_state.Research.UnlockedSkillIds.Contains("herbalism") && job.Category == JobCategory.Pharmacy)
        {
            int bonus = amount / 3 + 1;
            herbalBonus = bonus;
            amount += bonus;
            gold += bonus * 5;
        }

        if (_state.Research.UnlockedSkillIds.Contains("hospitality") && job.Category == JobCategory.CustomerService)
        {
            int bonus = amount / 2 + 1;
            hospitalityBonus = bonus;
            amount += bonus;
            gold += 15;
        }

        if (_state.Research.UnlockedSkillIds.Contains("craftsmanship") && job.Category == JobCategory.Chore)
        {
            int bonus = amount / 3 + 1;
            craftBonus = bonus;
            amount += bonus;
            gold += bonus * 3;
        }

        return new JobOutputPreview
        {
            Amount = amount, Gold = gold, SkillContribution = skillBonus / 2,
            PlanningBonus = researchBonus, TalentMultiplier = talentMult, FatigueMultiplier = fatiguePenalty,
            FatigueLost = fatigueLost, DairyBonus = dairyBonus, CulinaryBonus = culinaryBonus,
            HerbalBonus = herbalBonus, HospitalityBonus = hospitalityBonus, CraftBonus = craftBonus
        };
    }
}
