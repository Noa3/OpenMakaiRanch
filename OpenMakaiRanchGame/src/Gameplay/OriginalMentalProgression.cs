using System;
using System.Collections.Generic;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Gameplay;

/// <summary>
/// Read-only interpretation of the original era mental progression thresholds.
/// This intentionally does not replace/save a second FallState enum: existing saves keep their
/// current FallState representation while relationship/world presentation can still react to the
/// source game's Resistance -> Dignity -> Aversion -> Will progression.
/// </summary>
public enum OriginalMentalProgressStage
{
    Guarded,
    ResistanceBroken,
    DignityBroken,
    AversionCleared,
    WillBroken
}

public static class OriginalMentalProgression
{
    private static readonly string[] CollapseLostTraits =
    {
        "prideful", "arrogant", "stubborn", "defiant", "sassy",
        "optimistic", "charismatic", "righteous", "composure"
    };

    public static OriginalMentalProgressStage StageFor(CharacterState character)
    {
        ArgumentNullException.ThrowIfNull(character);
        var mind = character.Mature;

        // Mirrors the source MIND_CHANGE_CALC ordering: later zeroed parameters represent deeper
        // progression. MentalStrength/Will is the final irreversible collapse boundary.
        if (mind.MentalStrength <= 0)
            return OriginalMentalProgressStage.WillBroken;
        if (mind.Aversion <= 0)
            return OriginalMentalProgressStage.AversionCleared;
        if (mind.Dignity <= 0)
            return OriginalMentalProgressStage.DignityBroken;
        if (mind.Resistance <= 0)
            return OriginalMentalProgressStage.ResistanceBroken;
        return OriginalMentalProgressStage.Guarded;
    }

    public static string Describe(CharacterState character) => StageFor(character) switch
    {
        OriginalMentalProgressStage.WillBroken => "Will broken",
        OriginalMentalProgressStage.AversionCleared => "Aversion cleared",
        OriginalMentalProgressStage.DignityBroken => "Dignity broken",
        OriginalMentalProgressStage.ResistanceBroken => "Resistance broken",
        _ => "Guarded"
    };

    public static bool IsCollapseSensitiveTrait(string trait)
    {
        if (string.IsNullOrWhiteSpace(trait))
            return false;

        foreach (var candidate in CollapseLostTraits)
        {
            if (string.Equals(trait, candidate, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    public static IReadOnlyList<string> CollapseTraits => CollapseLostTraits;
}
