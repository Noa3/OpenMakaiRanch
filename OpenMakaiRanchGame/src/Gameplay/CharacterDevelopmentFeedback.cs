using System.Linq;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Gameplay;

/// <summary>Translate derived progress at read time; neither dialogue nor tooltips write gameplay state.</summary>
public static class CharacterDevelopmentFeedback
{
    public static string Explain(DevelopmentTrack? track)
    {
        if (track is null) return "";
        var name = track.Focus == "combat" ? T("development.track.conditioning", "Conditioning")
            : T("development.track.attunement", "Magical development");
        return track.State switch
        {
            DevelopmentTrackState.Complete => T("development.track.complete",
                "{0}: all {1} capacity stages reached. Skill progression is separate.", name, track.MaximumStages),
            DevelopmentTrackState.SkillLimit => T("development.track.skill_limit",
                "{0} {1}/{2}: current lessons cannot reach the next stage at this skill limit. Earned progress is kept.",
                name, track.Stage, track.MaximumStages),
            DevelopmentTrackState.CapacityLimit => T("development.track.capacity_limit",
                "{0}: no additional capacity fits at the current maximum. Earned progress is kept.", name),
            DevelopmentTrackState.Invalid => T("development.track.invalid", "{0}: a forecast is unavailable for the current values.", name),
            _ => T("development.track.next",
                "{0} {1}/{2}: {3} {4} → {5} for the next +{6} maximum {7}. Current resources are not refilled.",
                name, track.Stage, track.MaximumStages,
                SkillName(track.Focus == "combat" ? DevelopmentField.Combat : DevelopmentField.Magic),
                track.CurrentSkill, track.TargetSkill, track.NextStageCapacityGain,
                track.CapacityField == DevelopmentField.MaxHp ? T("development.capacity.hp", "HP") : T("development.capacity.mp", "MP"))
        };
    }

    public static string Reflect(CharacterDevelopmentSnapshot? snapshot)
    {
        if (snapshot is null) return "";
        // Do not invent development for a high imported skill, or repeat a stale entry after an untracked edit.
        var change = snapshot.RecentChanges.LastOrDefault(entry => (entry.Field is DevelopmentField.Ranch
            or DevelopmentField.Craft or DevelopmentField.Combat or DevelopmentField.Magic)
            && entry.Before != entry.After && snapshot.Values.Any(v => v.Field == entry.Field && v.Current == entry.After));
        if (change is null) return "";
        var thought = change.After < change.Before ? T("development.reflection.loss", "“I'm not quite back to my earlier form yet.”")
            : change.Field switch
            {
                DevelopmentField.Ranch => T("development.reflection.ranch", "“The ranch work feels more familiar now.”"),
                DevelopmentField.Craft => T("development.reflection.craft", "“I'm getting more confident with the tools.”"),
                DevelopmentField.Combat => T("development.reflection.combat", "“I feel more confident about my fighting skills.”"),
                _ => T("development.reflection.magic", "“Working with magic is starting to make more sense.”")
            };
        return T("development.reflection.summary", "{0} {1}: {2} → {3} (day {4}).", thought,
            SkillName(change.Field), change.Before, change.After, change.Day);
    }

    private static string SkillName(DevelopmentField field) => field switch
    {
        DevelopmentField.Ranch => T("development.skill.ranch", "ranch skill"),
        DevelopmentField.Craft => T("development.skill.craft", "craft skill"),
        DevelopmentField.Combat => T("development.skill.combat", "combat skill"),
        _ => T("development.skill.magic", "magical aptitude")
    };
}
