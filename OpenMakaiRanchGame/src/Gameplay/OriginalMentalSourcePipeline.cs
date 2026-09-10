using System;
using System.Collections.Generic;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Gameplay;

/// <summary>
/// Adapts the original era mental SOURCE pipeline to the real-time remake.
///
/// The source game calculates short-lived SOURCE values first, redistributes and scales them by
/// traits/state, then applies the result to persistent BASE values. The remake previously skipped
/// that layer and derived every persistent value directly from TrainingActionDefinition.MentalEffect.
/// Keeping the transient layer here makes the result deterministic, testable and much closer to
/// the original without adding transient fields to the save format.
///
/// Source references:
/// - 3_振り分け前SOURCE倍率.ERB
/// - 4_精神SOURCE振り分け2.ERB
/// - 5_精神SOURCE倍率補正.ERB
/// - 調教コマンド後精神増減.ERB
/// - 精神増減関数.ERB
/// </summary>
public static class OriginalMentalSourcePipeline
{
    private const int FirstMax = 10_000;
    private const int FavorabilityLoveMax = 20_000;
    private const int MentalEndure = 3_000;
    private const int MentalLimiter = 1_000;

    private sealed class Sources
    {
        // Original final mental SOURCE band, represented with neutral English identifiers.
        public int Cattle;
        public int Lust;
        public int Obedience;
        public int Surrender;
        public int Pain;
        public int Fear;
        public int Unpleasant;
        public int Antipathy;
        public int Despair;

        // Intermediate sources used by the original redistribution stages.
        public int Shame;
        public int Humiliation;
        public int Dominance;
        public int Uncleanliness;

        public int FinalSum =>
            Cattle + Lust + Obedience + Surrender + Pain + Fear + Unpleasant + Antipathy + Despair;
    }

    /// <summary>
    /// Resolve one command into persistent-state deltas. Positive MentalEffect means supportive
    /// treatment; negative MentalEffect means pressure/distress. This fixes the former inverted
    /// sign behavior where friendly actions directly reduced Resistance and Dignity.
    /// </summary>
    public static MentalStateEffects Resolve(TrainingActionDefinition action, CharacterState character)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(character);

        var mind = character.Mature;
        if (mind.IsBrainwashed)
            return new MentalStateEffects();

        var support = Math.Max(0, action.MentalEffect);
        var pressure = Math.Max(0, -action.MentalEffect);
        var pain = Math.Max(0, action.BasePain);
        var pleasure = Math.Max(0, action.BasePleasure);

        var sources = BuildInitialSources(action, character, support, pressure, pain, pleasure);
        ApplyPreDistributionTraitMultipliers(sources, character);
        RedistributeSources(sources, character);
        ApplyFinalSourceMultipliers(sources, character);

        // The original collapse state suppresses every mental source except despair.
        if (mind.IsCollapsed || mind.FallState == FallState.Collapse)
        {
            var despair = sources.Despair;
            sources = new Sources { Despair = despair };
        }

        var effects = ResolvePersistentEffects(sources, action, character, support, pressure);
        ApplySupportiveRecovery(effects, character, support, pleasure);
        return effects;
    }

    private static Sources BuildInitialSources(
        TrainingActionDefinition action,
        CharacterState character,
        int support,
        int pressure,
        int pain,
        int pleasure)
    {
        var s = new Sources
        {
            // Values intentionally use SOURCE-like magnitudes. Persistent BASE conversion later
            // follows the original /20 and /30 conversion, including the threshold soft-cap.
            Lust = pleasure * 55,
            Obedience = Math.Max(0, pleasure * 28 + support * 55 - pressure * 12),
            Surrender = pressure * 55,
            Pain = pain * 100,
            Shame = pressure * 35,
            Humiliation = pressure * 45,
            Dominance = pressure * 45,
            Uncleanliness = action.SensationTypes.Contains(SensationType.Disgust)
                ? (pressure + pain + 1) * 45
                : 0
        };

        if (action.SensationTypes.Contains(SensationType.Fear))
            s.Fear += (pressure * 80) + (pain * 55) + 80;
        if (action.SensationTypes.Contains(SensationType.Disgust))
            s.Unpleasant += (pressure * 70) + (pain * 45) + 60;
        if (action.SensationTypes.Contains(SensationType.Antipathy))
            s.Antipathy += (pressure * 75) + (pain * 40) + 60;
        if (action.SensationTypes.Contains(SensationType.Despair))
            s.Despair += (pressure * 90) + (pain * 55) + 100;
        if (action.SensationTypes.Contains(SensationType.Lust))
            s.Lust += (pleasure + pressure + 1) * 45;

        // Milk/cattle progression in the original is a dedicated mental SOURCE. Restrict it to
        // explicitly milk-related actions instead of treating all PleasureB actions as cattle gain.
        if (IsMilkProgressAction(action.Id))
            s.Cattle += (pleasure * 32) + (pressure * 12);

        // A supportive action can calm negative emotions, but must not itself count as pressure
        // against Resistance/Dignity. That distinction is enforced again during BASE conversion.
        if (support > 0)
        {
            s.Antipathy = Math.Max(0, s.Antipathy - support * 40);
            s.Fear = Math.Max(0, s.Fear - support * 35);
            s.Unpleasant = Math.Max(0, s.Unpleasant - support * 30);
            s.Despair = Math.Max(0, s.Despair - support * 30);
        }

        return s;
    }

    /// <summary>
    /// Mirrors the source game's pre-distribution talent multipliers for shame/humiliation/
    /// dominance. Alias matching accepts both remake ids and translated display-style names.
    /// </summary>
    private static void ApplyPreDistributionTraitMultipliers(Sources s, CharacterState character)
    {
        if (HasTalent(character, "shy", "bashful", "embarrassed"))
            s.Shame = Scale(s.Shame, 150);
        else if (HasTalent(character, "shameless", "low shame"))
            s.Shame = Scale(s.Shame, 50);

        if (HasTalent(character, "prideful", "high pride"))
        {
            s.Humiliation = Scale(s.Humiliation, 130);
            s.Dominance = Scale(s.Dominance, 130);
        }
        else if (HasTalent(character, "arrogant"))
        {
            s.Humiliation = Scale(s.Humiliation, 160);
            s.Dominance = Scale(s.Dominance, 160);
        }
        else if (HasTalent(character, "low pride"))
        {
            s.Humiliation = Scale(s.Humiliation, 80);
            s.Dominance = Scale(s.Dominance, 90);
        }
        else if (HasTalent(character, "submissive", "servile"))
        {
            s.Humiliation = Scale(s.Humiliation, 50);
            s.Dominance = Scale(s.Dominance, 80);
        }
    }

    /// <summary>
    /// Recreates the important conversions in 4_精神SOURCE振り分け2.ERB.
    /// </summary>
    private static void RedistributeSources(Sources s, CharacterState character)
    {
        var masochism = Math.Clamp(character.Addictions.Masochism, 0, 100);
        if (masochism >= 25)
        {
            var factor = Math.Clamp((masochism - 25) / 5, 0, 15);
            s.Lust += (s.Humiliation + s.Dominance + s.Shame) * factor / 10;
        }

        var cowardly = HasTalent(character, "cowardly", "coward");
        var weakWilled = HasTalent(character, "weak-willed", "weak willed", "weak_willed");
        var pessimistic = HasTalent(character, "pessimistic");
        var quiet = HasTalent(character, "docile", "quiet");
        var indifferent = HasTalent(character, "indifferent", "apathetic");

        if ((HasTalent(character, "shy", "bashful") || quiet) && !indifferent)
            s.Surrender += s.Shame;

        if (cowardly || weakWilled || pessimistic)
            s.Surrender += s.Pain + s.Fear;

        if (cowardly)
            s.Surrender += s.Dominance;

        if (masochism >= 25)
        {
            var factor = Math.Clamp((masochism - 20) / 5, 0, 20);
            s.Surrender += (s.Shame + s.Humiliation + s.Dominance) * factor / 10;
        }

        if (cowardly && masochism < 50)
            s.Fear += (s.Shame + s.Humiliation + s.Dominance) / 2;

        s.Unpleasant += s.Uncleanliness;
        if (HasTalent(character, "prideful", "high pride", "arrogant"))
            s.Unpleasant += s.Humiliation;

        // Original: while reason remains, degrading/dominating sources also generate antipathy.
        if (character.Mature.Reason > 0)
            s.Antipathy += (s.Shame + s.Humiliation + s.Dominance) / 2;

        if (HasTalent(character, "denial of pleasure", "pleasure denial", "denial_of_pleasure")
            && character.Mature.Reason > 0)
        {
            s.Antipathy += s.Lust / 2;
        }

        // Loss of stamina/health is the remake equivalent of the original depleted physical/
        // mental-energy condition that converts pain/fear into despair.
        if (character.Energy <= 0)
            s.Despair += s.Pain + s.Fear;
    }

    private static void ApplyFinalSourceMultipliers(Sources s, CharacterState character)
    {
        // Obedience / compliance source.
        var obediencePct = 150;
        if (character.Energy <= 0) obediencePct -= 30;
        if (HasTalent(character, "chaste", "pure", "clean")) obediencePct += 10;
        if (HasTalent(character, "brave", "strong-willed", "strong willed")) obediencePct -= 20;
        if (HasTalent(character, "prideful", "high pride")) obediencePct -= 10;
        if (HasTalent(character, "arrogant")) obediencePct -= 20;
        if (HasTalent(character, "rebellious", "defiant")) obediencePct -= 20;
        if (HasTalent(character, "self-control", "self control", "self_control")) obediencePct -= 10;
        if (HasTalent(character, "indifferent", "apathetic")) obediencePct -= 10;
        if (character.Mature.Resistance > 0) obediencePct -= 20;
        if (character.Mature.Aversion > 0) obediencePct -= 20;
        s.Obedience = Scale(s.Obedience, Math.Clamp(obediencePct, 10, 300));

        // Surrender source.
        var surrenderPct = 150;
        if (character.Hp <= 0) surrenderPct += 50;
        if (character.Energy <= 0) surrenderPct += 50;
        if (HasTalent(character, "self-control", "self control", "self_control")) surrenderPct -= 10;
        if (HasTalent(character, "indifferent", "apathetic")) surrenderPct -= 10;
        s.Surrender = Scale(s.Surrender, Math.Clamp(surrenderPct, 10, 300));

        // Pain source.
        var painPct = 100;
        if (character.Energy <= 0) painPct += 30;
        if (HasTalent(character, "pain sensitive", "weak to pain", "pain_sensitive")) painPct += 30;
        if (HasTalent(character, "cowardly", "coward")) painPct += 20;
        if (HasTalent(character, "pain resistant", "strong against pain", "pain_resistant")) painPct -= 30;
        if (HasTalent(character, "emotionless", "low emotion")) painPct -= 20;
        if (HasTalent(character, "brave", "strong-willed", "strong willed")) painPct -= 20;
        s.Pain = Scale(s.Pain, Math.Clamp(painPct, 10, 300));

        // Fear source.
        var fearPct = 100;
        if (HasTalent(character, "cowardly", "coward")) fearPct += 50;
        if (HasTalent(character, "weak-willed", "weak willed", "weak_willed")) fearPct += 20;
        if (HasTalent(character, "pessimistic")) fearPct += 30;
        if (HasTalent(character, "emotionless", "low emotion")) fearPct -= 30;
        if (HasTalent(character, "optimistic")) fearPct -= 30;
        if (HasTalent(character, "brave", "strong-willed", "strong willed")) fearPct -= 20;
        if (HasTalent(character, "rough")) fearPct -= 20;
        if (HasTalent(character, "noble", "graceful")) fearPct -= 10;
        if (HasTalent(character, "righteous", "justice")) fearPct -= 30;
        s.Fear = Scale(s.Fear, Math.Clamp(fearPct, 10, 300));

        // Cattle-specialized characters are much less affected by fear in the original.
        if (character.Mature.FallState == FallState.MilkCow)
            s.Fear /= 5;

        if (HasTalent(character, "emotionless", "low emotion"))
            s.Unpleasant = Scale(s.Unpleasant, 70);

        // Antipathy source.
        var antipathyPct = 100;
        if (HasTalent(character, "sassy")) antipathyPct += 20;
        if (HasTalent(character, "rebellious", "defiant")) antipathyPct += 30;
        if (HasTalent(character, "brave", "strong-willed", "strong willed")) antipathyPct += 30;
        if (HasTalent(character, "prideful", "high pride")) antipathyPct += 30;
        if (HasTalent(character, "arrogant")) antipathyPct += 50;
        if (HasTalent(character, "cowardly", "coward")) antipathyPct -= 10;
        if (HasTalent(character, "docile", "quiet")) antipathyPct -= 10;
        if (HasTalent(character, "obedient", "compliant")) antipathyPct -= 20;
        s.Antipathy = Scale(s.Antipathy, Math.Clamp(antipathyPct, 10, 300));

        if (character.Mature.FallState is FallState.Slave or FallState.MilkCow)
            s.Antipathy = 0;

        // Despair source.
        var despairPct = 100;
        if (character.Mature.Dignity >= 5_000) despairPct -= 30;
        if (character.Hp <= 0) despairPct += 50;
        if (HasTalent(character, "weak-willed", "weak willed", "weak_willed")) despairPct += 10;
        if (HasTalent(character, "cowardly", "coward")) despairPct += 30;
        if (HasTalent(character, "pessimistic")) despairPct += 20;
        if (HasTalent(character, "submissive", "servile")) despairPct += 50;
        if (HasTalent(character, "optimistic")) despairPct -= 20;
        if (HasTalent(character, "brave", "strong-willed", "strong willed")) despairPct -= 20;
        if (HasTalent(character, "noble", "graceful")) despairPct -= 20;
        s.Despair = Scale(s.Despair, Math.Clamp(despairPct, 10, 300));
    }

    private static MentalStateEffects ResolvePersistentEffects(
        Sources s,
        TrainingActionDefinition action,
        CharacterState character,
        int support,
        int pressure)
    {
        var m = character.Mature;
        var effects = new MentalStateEffects
        {
            // Keep emotional SOURCE history useful for AI/thought bubbles/UI. These are slower
            // persistent memories than the raw per-command source magnitudes.
            LustDelta = Math.Max(0, s.Lust / 45),
            ObedienceDelta = Math.Max(0, s.Obedience / 45),
            SubmissionDelta = Math.Max(0, s.Surrender / 45),
            MilkCowDelta = Math.Max(0, s.Cattle / 45),
            PainDelta = Math.Max(0, s.Pain / 55),
            FearDelta = Math.Max(0, s.Fear / 55),
            DisgustDelta = Math.Max(0, s.Unpleasant / 55),
            AntipathyDelta = Math.Max(0, s.Antipathy / 55),
            DespairDelta = Math.Max(0, s.Despair / 55)
        };

        // Resistance and dignity are progression barriers in the source game. Supportive social
        // actions no longer directly damage them; pressure is required to move these BASE values.
        if (pressure > 0)
        {
            var resistanceRaw = Math.Max(0, s.FinalSum + s.Surrender - s.Unpleasant - s.Antipathy);
            effects.ResistanceDelta = -BaseMinus(resistanceRaw, 200);

            var dignityRaw = Math.Max(0, s.FinalSum + s.Despair - s.Antipathy);
            if (m.Resistance > 0)
                dignityRaw = Math.Max(0, dignityRaw - 2_000);
            effects.DignityDelta = -BaseMinus(dignityRaw, 100);
        }

        // Aversion only starts clearing after resistance/dignity have yielded, matching the source
        // game's rejection -> succumb -> falling ladder. Negative emotions actively oppose it.
        if (m.Resistance <= 0 && m.Dignity <= 0)
        {
            var negativeTail = s.Fear + s.Unpleasant + s.Antipathy + s.Despair;
            var aversionRaw = Math.Max(0, s.Cattle + (s.Obedience * 2) - (negativeTail / 2));
            effects.AversionDelta = -BaseMinus(aversionRaw, 100);
        }

        // Reason erosion in the source depends on cattle/lust sources and is blocked early by
        // resistance. Supportive non-intimate actions therefore do not erode it.
        if (m.Resistance <= 0 && (pressure > 0 || action.IsMatureOnly))
        {
            var reasonRaw = Math.Max(0, (s.Cattle * 2) + s.Lust - (support * 50));
            effects.ReasonDelta = -BaseMinus(reasonRaw, 100);
        }

        // Original rule: MentalStrength only falls from despair once both HP and mental energy are
        // depleted. This prevents normal interactions from shortcutting straight to Collapse.
        if (character.Hp <= 0 && character.Energy <= 0)
        {
            var mentalRaw = Math.Max(0, s.Despair - s.Obedience);
            var damage = BaseMinus(mentalRaw, 100);
            if (m.MentalStrength < MentalEndure)
                damage /= 2;
            effects.MentalStrengthDelta = -damage;
        }

        // Favorability is locked behind cleared resistance+aversion in the source. Use BASE_PLUS's
        // /30 conversion and soft-cap rather than giving direct pleasure -> love at any stage.
        if (m.Resistance <= 0 && m.Aversion <= 0)
        {
            var negativeTail = s.Fear + s.Unpleasant + s.Antipathy + s.Despair;
            var favorRaw = Math.Max(0, s.Obedience - (negativeTail / 2));
            effects.FavorabilityDelta = BasePlus(favorRaw, 100);
        }

        return effects;
    }

    private static void ApplySupportiveRecovery(
        MentalStateEffects effects,
        CharacterState character,
        int support,
        int pleasure)
    {
        if (support <= 0)
            return;

        // Real-time remake quality-of-life layer: friendly routines calm accumulated negative
        // emotional memory and restore some mental stamina, but do not rebuild already-broken
        // Resistance/Dignity progression gates.
        effects.FearDelta -= support * 8;
        effects.DisgustDelta -= support * 6;
        effects.AntipathyDelta -= support * 10;
        effects.DespairDelta -= support * 8;
        effects.PainDelta -= support * 4;

        if (!character.Mature.IsCollapsed)
            effects.MentalStrengthDelta += support * 3;

        // Before the original favorability gate opens, supportive interaction still matters through
        // Bond/Morale elsewhere. Once it is open, provide a small relationship-quality bonus.
        if (character.Mature.Resistance <= 0 && character.Mature.Aversion <= 0)
            effects.FavorabilityDelta += (support * 3) + (pleasure / 3);
    }

    /// <summary>
    /// Apply source game's BASE_MINUS conversion: divide by 20, then soften values above cap to
    /// cap + overflow/5, and finally enforce the original cap*10 safety ceiling.
    /// </summary>
    private static int BaseMinus(int rawSource, int cap)
    {
        if (rawSource <= 0)
            return 0;
        var value = rawSource / 20;
        return ApplyThresholdSoftCap(value, cap);
    }

    /// <summary>Apply source game's BASE_PLUS conversion (/30) with the same soft-cap.</summary>
    private static int BasePlus(int rawSource, int cap)
    {
        if (rawSource <= 0)
            return 0;
        var value = rawSource / 30;
        return ApplyThresholdSoftCap(value, cap);
    }

    public static int ApplyThresholdSoftCap(int value, int threshold)
    {
        if (value <= 0 || threshold <= 0)
            return 0;
        if (value > threshold)
            value = threshold + ((value - threshold) / 5);
        return Math.Clamp(value, 0, threshold * 10);
    }

    /// <summary>
    /// Helper used by MentalStateService when it needs a non-training mental-strength reduction.
    /// The original limiter prevents ordinary outside-training damage from crossing 10% in one go.
    /// </summary>
    public static int ApplyMentalStrengthLimiter(int current, int requestedDelta, bool allowCollapse)
    {
        current = Math.Clamp(current, 0, FirstMax);
        if (requestedDelta >= 0 || allowCollapse)
            return requestedDelta;

        var target = current + requestedDelta;
        if (target >= MentalLimiter)
            return requestedDelta;
        if (current <= MentalLimiter)
            return 0;
        return MentalLimiter - current;
    }

    public static bool CanGainFavorability(CharacterState character)
    {
        ArgumentNullException.ThrowIfNull(character);
        return character.Mature.Resistance <= 0 && character.Mature.Aversion <= 0;
    }

    public static int FavorabilityStageCap(CharacterState character)
    {
        ArgumentNullException.ThrowIfNull(character);
        return character.Mature.FallState switch
        {
            FallState.Slave or FallState.MilkCow => 99_900,
            FallState.Love or FallState.Devotion => FavorabilityLoveMax,
            _ => FirstMax
        };
    }

    private static bool IsMilkProgressAction(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;
        var normalized = Normalize(id);
        return normalized.Contains("milk", StringComparison.Ordinal)
            || normalized.Contains("milker", StringComparison.Ordinal)
            || normalized.Contains("lact", StringComparison.Ordinal);
    }

    private static int Scale(int value, int percent)
    {
        if (value <= 0 || percent <= 0)
            return 0;
        return value * percent / 100;
    }

    private static bool HasTalent(CharacterState character, params string[] aliases)
    {
        if (character.Talents is null || character.Talents.Count == 0)
            return false;

        foreach (var talent in character.Talents)
        {
            var normalizedTalent = Normalize(talent);
            foreach (var alias in aliases)
            {
                var normalizedAlias = Normalize(alias);
                if (normalizedTalent == normalizedAlias || normalizedTalent.EndsWith(normalizedAlias, StringComparison.Ordinal))
                    return true;
            }
        }
        return false;
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var chars = new char[value.Length];
        var count = 0;
        foreach (var c in value)
        {
            if (!char.IsLetterOrDigit(c))
                continue;
            chars[count++] = char.ToLowerInvariant(c);
        }
        return new string(chars, 0, count);
    }
}