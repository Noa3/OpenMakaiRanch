using System;
using OpenMakaiRanch.Core.Models;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Gameplay;

public enum SpiritReserveState { Unknown, Depleted, Low, Stable }
public sealed record CharacterProtectionSnapshot(bool HasOriginalWardTrait, SpiritReserveState Reserve,
    int Spirit, int MaxSpirit, int Mana, int MaxMana, bool NeedsRecovery, string Explanation)
{
    // This describes the RESOURCE prerequisite, not action permission or consent.
    public bool WardReserveReady => HasOriginalWardTrait && Reserve == SpiritReserveState.Stable;
}

/// <summary>
/// The original ward is specific, not universal immunity. Its resource is EP, not MP.
/// This non-explicit adapter never removes it or turns depletion/incapacity into permission.
/// </summary>
public static class CharacterProtectionService
{
    public static bool NeedsRecovery(CharacterState character) => character.Hp <= 0
        || character.Mature.IsCollapsed || character.Mature.FallState == FallState.Collapse;

    public static CharacterProtectionSnapshot Inspect(CharacterState character)
    {
        ArgumentNullException.ThrowIfNull(character);
        // Exact catalog IDs only. Do not infer a ward from race, body, age or a display name.
        var ward = character.Talents.Contains("2") || character.Talents.Contains("talent_2")
            || character.Talents.Contains("virginity_barrier");
        var reserve = character.MaxSpirit <= 0 || character.Spirit < 0
            ? SpiritReserveState.Unknown : character.Spirit == 0 ? SpiritReserveState.Depleted
            : character.Spirit <= character.MaxSpirit / 2 ? SpiritReserveState.Low : SpiritReserveState.Stable;
        var explanation = !ward ? T("character.protection.absent", "No resource-backed ward trait is recorded.")
            : reserve == SpiritReserveState.Unknown ? T("character.protection.unknown", "Ward trait recorded; its spirit capacity is unavailable.")
            : reserve == SpiritReserveState.Stable ? T("character.protection.ready", "Ward reserve: spirit energy is above half capacity. Mana is a separate resource.")
            : T("character.protection.low", "Ward reserve: spirit energy is at or below half capacity. Mana does not sustain this ward.");
        return new(ward, reserve, character.Spirit, character.MaxSpirit, character.Mana, character.MaxMana,
            NeedsRecovery(character), explanation);
    }
}

public sealed record OriginalCombatPower(bool Valid, long MaximumPotential, long CurrentPower,
    long EffectiveMana, bool ExcessManaDiscounted);

/// <summary>
/// Read-only reference for CALC_BATTLE_POWER, NOT a replacement for the tactical battle engine.
/// Aptitude is an explicit input: modern CombatSkill is not the original percentage.
/// </summary>
public static class OriginalCharacterRules
{
    public static OriginalCombatPower InspectCombatPower(CharacterState character, int maxHp, int aptitudePercent)
    {
        ArgumentNullException.ThrowIfNull(character);
        if (maxHp <= 0 || character.Hp < 0 || character.Hp > maxHp || character.Level < 1
            || character.Spirit < 0 || character.MaxSpirit < character.Spirit
            || character.Mana < 0 || character.MaxMana < character.Mana
            || aptitudePercent is < 0 or > 100) return new(false, 0, 0, 0, false);
        var discount = (long)character.Spirit * 10 <= character.Mana && character.Level < 30;
        var mana = discount ? character.Mana / 10L : character.Mana;
        // Preserve the original sequential INTEGER rounding, not a merged float multiplier.
        var healthPercent = (long)character.Hp * 100 / maxHp;
        var current = ((long)character.Spirit + mana) * healthPercent / 100;
        current = current * aptitudePercent / 100;
        return new(true, (long)character.MaxSpirit + character.MaxMana, current, mana, discount);
    }
}
