using System;

namespace OpenMakaiRanch.Gameplay;

public enum DevelopmentTrackState { Available, Complete, SkillLimit, CapacityLimit, Invalid }

/// <summary>A long-term progression preview, not permission to take a lesson right now.</summary>
public sealed record DevelopmentTrack(string Focus, DevelopmentField CapacityField, int CurrentSkill,
    int HighestRecordedSkill, int Points, int PointsPerStage, int MaximumStages, long TargetSkill,
    int NextStageCapacityGain, DevelopmentTrackState State)
{
    public int Stage => Points / PointsPerStage;
    public long SkillGainNeeded => Math.Max(0L, TargetSkill - CurrentSkill);
}

public sealed partial class CharacterDevelopmentService
{
    public DevelopmentTrack? InspectPracticeTrack(string? id, string? focus)
    {
        var c = Find(id);
        if (c is null || c.Id == "anon" || focus is not ("combat" or "magic")) return null;
        var combat = focus == "combat";
        var skill = combat ? c.CombatSkill : c.MagicPower;
        var step = combat ? ConditioningStep : AttunementStep;
        var limit = combat ? ConditioningLimit : AttunementLimit;
        var points = Math.Clamp(Get(c, combat ? ConditioningPoints : AttunementPoints), 0, limit);
        var capacity = combat ? _roster.DefinitionFor(c).MaxHp : c.MaxMana;
        var high = Get(c, BaselineDay) > 0 ? Math.Max(skill, Get(c, HighWaterStart + (combat ? 2 : 3))) : skill;
        var field = combat ? DevelopmentField.MaxHp : DevelopmentField.MaxMana;
        DevelopmentTrack Result(DevelopmentTrackState status, long target, int bonus) =>
            new(focus, field, skill, high, points, step, MaximumStages, target, bonus, status);
        if (skill < 0 || capacity < (combat ? 1 : 0))
            return Result(DevelopmentTrackState.Invalid, skill, 0);
        if (points >= limit) return Result(DevelopmentTrackState.Complete, skill, 0);
        var gain = (int)Math.Min(CapacityPerStage, (long)int.MaxValue - capacity);
        var target = (long)high + step - points % step;
        if (gain <= 0) return Result(DevelopmentTrackState.CapacityLimit, target, 0);
        // Returning to a lost old skill high grants no extra points. Forecast the same high-water rule.
        // Magic lessons add two, except the existing zero->three normalization; parity matters near int.MaxValue.
        var magicBase = Math.Max(1L, skill);
        var reachable = combat ? 10L : skill > int.MaxValue - 2 ? skill
            : magicBase + (int.MaxValue - magicBase) / 2 * 2;
        return Result(target <= reachable ? DevelopmentTrackState.Available : DevelopmentTrackState.SkillLimit,
            target, gain);
    }
}
