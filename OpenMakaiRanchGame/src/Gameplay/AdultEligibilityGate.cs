using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;

namespace OpenMakaiRanch.Gameplay;

/// <summary>
/// Fail-closed adult eligibility gates. No implicit approval.
/// All paths must explicitly verify eligibility before adult content presentation.
/// </summary>
public static class AdultEligibilityGate
{
    /// <summary>
    /// Checks if a character definition is eligible for adult presentation.
    /// Fail-closed: returns false for Unknown, Minor, Ambiguous.
    /// </summary>
    public static bool IsEligibleForAdult(CharacterDefinition definition)
    {
        if (definition is null) return false;
        return definition.AdultEligibility == AdultEligibility.ConfirmedAdult;
    }

    /// <summary>
    /// Checks if a character state is eligible for adult presentation.
    /// Fail-closed: returns false for Unknown, Minor, Ambiguous.
    /// </summary>
    public static bool IsEligibleForAdult(CharacterState character)
    {
        if (character is null) return false;
        return character.AdultEligibility == AdultEligibility.ConfirmedAdult;
    }

    /// <summary>
    /// Checks if a character is eligible for adult training/actions.
    /// Fail-closed: returns false for Unknown, Minor, Ambiguous.
    /// </summary>
    public static bool CanPerformAdultAction(CharacterState character)
    {
        if (character is null) return false;
        return character.AdultEligibility == AdultEligibility.ConfirmedAdult;
    }

    /// <summary>
    /// Checks if a character is eligible for adult portrait/visual presentation.
    /// Fail-closed: returns false for Unknown, Minor, Ambiguous.
    /// </summary>
    public static bool CanRenderAdultPortrait(CharacterState character, CharacterDefinition definition)
    {
        if (character is null || definition is null) return false;
        return character.AdultEligibility == AdultEligibility.ConfirmedAdult
            && definition.AdultEligibility == AdultEligibility.ConfirmedAdult;
    }

    /// <summary>
    /// Validates and sets eligibility on a character definition during import/creation.
    /// This is the SINGLE source of truth for eligibility assignment.
    /// </summary>
    public static void ValidateAndSetEligibility(CharacterDefinition definition, int apparentAge, string? ageContextNote = null)
    {
        if (definition is null) return;

        definition.ApparentAge = apparentAge;
        definition.AgeContextNote = ageContextNote ?? string.Empty;

        // Fail-closed: minor apparent ages are never eligible
        if (apparentAge < 18)
        {
            definition.AdultEligibility = AdultEligibility.Minor;
            return;
        }

        // Ambiguous: apparent age 18+ but with concerning context (school markers, baby-face traits, etc.)
        // These require explicit human review before approval
        if (!string.IsNullOrWhiteSpace(ageContextNote))
        {
            var note = ageContextNote.ToLowerInvariant();
            if (note.Contains("school") || note.Contains("jk") || note.Contains("baby") 
                || note.Contains("child") || note.Contains("minor") || note.Contains("童顔")
                || note.Contains("student") || note.Contains("high school"))
            {
                definition.AdultEligibility = AdultEligibility.Ambiguous;
                return;
            }
        }

        // Default: Unknown - requires explicit human review and approval
        definition.AdultEligibility = AdultEligibility.Unknown;
    }

    /// <summary>
    /// Validates and sets eligibility on a character state (runtime).
    /// </summary>
    public static void ValidateAndSetEligibility(CharacterState character, int apparentAge, string? ageContextNote = null)
    {
        if (character is null) return;

        character.ApparentAge = apparentAge;
        character.AgeContextNote = ageContextNote ?? string.Empty;

        if (apparentAge < 18)
        {
            character.AdultEligibility = AdultEligibility.Minor;
            return;
        }

        if (!string.IsNullOrWhiteSpace(ageContextNote))
        {
            var note = ageContextNote.ToLowerInvariant();
            if (note.Contains("school") || note.Contains("jk") || note.Contains("baby") 
                || note.Contains("child") || note.Contains("minor") || note.Contains("童顔")
                || note.Contains("student") || note.Contains("high school"))
            {
                character.AdultEligibility = AdultEligibility.Ambiguous;
                return;
            }
        }

        character.AdultEligibility = AdultEligibility.Unknown;
    }

    /// <summary>
    /// Gets the eligibility denial reason for debugging/logging.
    /// </summary>
    public static string GetDenialReason(CharacterState character, CharacterDefinition? definition = null)
    {
        if (character is null) return "Character is null";

        return character.AdultEligibility switch
        {
            AdultEligibility.Minor => $"Minor apparent age ({character.ApparentAge})",
            AdultEligibility.Ambiguous => $"Ambiguous design/context: {character.AgeContextNote}",
            AdultEligibility.Unknown => "Eligibility not reviewed (Unknown)",
            _ => "Eligible"
        };
    }

    /// <summary>
    /// Gets the eligibility denial reason for a definition.
    /// </summary>
    public static string GetDenialReason(CharacterDefinition definition)
    {
        if (definition is null) return "Definition is null";

        return definition.AdultEligibility switch
        {
            AdultEligibility.Minor => $"Minor apparent age ({definition.ApparentAge})",
            AdultEligibility.Ambiguous => $"Ambiguous design/context: {definition.AgeContextNote}",
            AdultEligibility.Unknown => "Eligibility not reviewed (Unknown)",
            _ => "Eligible"
        };
    }
}