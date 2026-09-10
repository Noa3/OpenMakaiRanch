using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Gameplay;

/// <summary>
/// Real-time companionship/date layer.
///
/// This does not replace the source game's mental model. It feeds Bond/Morale and the existing
/// Resistance/Dignity/Aversion/Favorability/etc. values, while persisting only date history and
/// the currently accompanying ranch resident.
/// </summary>
public sealed class DatingService
{
    private readonly SaveState _state;
    private readonly PlayerStaminaService _stamina;

    public DatingService(SaveState state, PlayerStaminaService stamina)
    {
        _state = state;
        _stamina = stamina;
        _state.Dating ??= new DatingState();
        _state.Dating.Partners ??= new Dictionary<string, DatingPartnerState>();
    }

    public string ActivePartnerId => _state.Dating.ActivePartnerId;
    public bool HasActivePartner => !string.IsNullOrWhiteSpace(ActivePartnerId);

    public IReadOnlyList<CharacterState> EligiblePartners() =>
        _state.Roster.Characters
            .Where(IsEligiblePartner)
            .ToList();

    public bool IsEligiblePartner(CharacterState character) =>
        character.Id != "anon"
        && AdultEligibilityGate.IsEligibleForAdult(character)
        && character.Mature.FallState != FallState.Collapse
        && !character.Mature.IsCollapsed;

    public int ActivityCost(DateActivityKind kind) => kind switch
    {
        DateActivityKind.RanchWalk => 8,
        DateActivityKind.WorkTogether => 10,
        DateActivityKind.SharedMeal => 8,
        DateActivityKind.TownOuting => 15,
        DateActivityKind.QuietRest => 8,
        _ => 10
    };

    public RelationshipStage StageFor(string characterId)
    {
        var character = FindCharacter(characterId);
        if (character is null)
            return RelationshipStage.Distant;

        var progress = ProgressFor(characterId);
        var m = character.Mature;

        if (progress.PositiveMoments >= 8
            && progress.ForcedMoments == 0
            && character.Bond >= 75
            && m.Favorability >= 10000
            && EffectiveAversion(character, progress) <= 500)
            return RelationshipStage.DeeplyAttached;

        if (progress.PositiveMoments >= 4
            && progress.ForcedMoments <= 1
            && character.Bond >= 50
            && m.Favorability >= 5000
            && EffectiveAversion(character, progress) <= 1500)
            return RelationshipStage.Romantic;

        if (progress.PositiveMoments >= 2
            && character.Bond >= 30
            && m.Favorability >= 2000
            && EffectiveAversion(character, progress) <= 3000)
            return RelationshipStage.Close;

        return progress.DatesStarted > 0 || character.Bond >= 10
            ? RelationshipStage.Familiar
            : RelationshipStage.Distant;
    }

    public DatingResult StartDate(string characterId, DateInviteApproach approach)
    {
        var character = FindCharacter(characterId);
        if (character is null || !IsEligiblePartner(character))
            return DatingResult.Fail("This resident is not available for dating/companionship.");

        if (HasActivePartner && !string.Equals(ActivePartnerId, characterId, StringComparison.Ordinal))
            return DatingResult.Fail("End the current outing before inviting someone else.");

        var progress = ProgressFor(characterId);
        if (string.Equals(ActivePartnerId, characterId, StringComparison.Ordinal))
            return DatingResult.Ok($"{DisplayName(character)} is already accompanying you.");

        var willingness = WillingnessScore(character, progress);
        if (approach == DateInviteApproach.Respectful && willingness < 250)
            return DatingResult.Fail($"{DisplayName(character)} declines the invitation. Give them space or improve the relationship first.");

        _state.Dating.ActivePartnerId = characterId;
        _state.Dating.ActiveApproach = approach;
        _state.Dating.StartedDay = _state.Calendar.Day;
        _state.Dating.StartedPhase = _state.Calendar.Phase;

        if (progress.LastDateDay != _state.Calendar.Day)
        {
            progress.DatesStarted++;
            progress.LastDateDay = _state.Calendar.Day;
        }

        switch (approach)
        {
            case DateInviteApproach.Respectful:
                if (willingness >= 700)
                {
                    character.Morale = Clamp100(character.Morale + 2);
                    character.Bond = Clamp100(character.Bond + 1);
                }
                break;

            case DateInviteApproach.Pressured:
                progress.PressuredMoments++;
                character.Morale = Clamp100(character.Morale - 4);
                character.Bond = Clamp100(character.Bond - 2);
                AdjustMind(character, dignity: -20, aversion: 120, antipathy: 80, fear: 20, favorability: -40);
                break;

            case DateInviteApproach.Forced:
                progress.ForcedMoments++;
                character.Morale = Clamp100(character.Morale - 10);
                character.Bond = Clamp100(character.Bond - 5);
                // Pressure may erode dignity, but it also makes the social route substantially worse.
                // This prevents forced outings from becoming an optimal shortcut to positive romance.
                AdjustMind(character, resistance: 120, dignity: -100, aversion: 320, antipathy: 260, fear: 120, favorability: -180);
                break;
        }

        return DatingResult.Ok(approach switch
        {
            DateInviteApproach.Respectful => $"{DisplayName(character)} agrees to spend time with you and will accompany you.",
            DateInviteApproach.Pressured => $"{DisplayName(character)} comes along reluctantly. Their discomfort will affect the outing.",
            _ => $"{DisplayName(character)} is forced to accompany you. This seriously damages trust and mood."
        });
    }

    public DatingResult EndDate()
    {
        if (!HasActivePartner)
            return DatingResult.Fail("No resident is currently accompanying you.");

        var character = FindCharacter(ActivePartnerId);
        var name = character is null ? "Your companion" : DisplayName(character);
        _state.Dating.ActivePartnerId = string.Empty;
        _state.Dating.ActiveApproach = DateInviteApproach.Respectful;
        return DatingResult.Ok($"{name} returns to their normal ranch routine.");
    }

    public bool CanPerformActivity(DateActivityKind kind, out string reason)
    {
        reason = string.Empty;
        if (!HasActivePartner)
        {
            reason = "Invite a resident to accompany you first.";
            return false;
        }

        var character = FindCharacter(ActivePartnerId);
        if (character is null || !IsEligiblePartner(character))
        {
            reason = "The current companion is no longer available.";
            return false;
        }

        if (_state.Dating.LastActivityDay == _state.Calendar.Day
            && _state.Dating.LastActivityPhase == _state.Calendar.Phase)
        {
            reason = "You already completed a meaningful companion activity this phase.";
            return false;
        }

        if (!_stamina.CanSpend(ActivityCost(kind)))
        {
            reason = $"Not enough player stamina ({ActivityCost(kind)} required).";
            return false;
        }

        if (kind == DateActivityKind.SharedMeal
            && (!_state.Inventory.Items.TryGetValue("meal_box", out var meals) || meals <= 0))
        {
            reason = "A shared meal requires one meal_box.";
            return false;
        }

        return true;
    }

    public DatingResult PerformActivity(DateActivityKind kind)
    {
        if (!CanPerformActivity(kind, out var reason))
            return DatingResult.Fail(reason);

        var character = FindCharacter(ActivePartnerId)!;
        var progress = ProgressFor(character.Id);
        var cost = ActivityCost(kind);

        if (kind == DateActivityKind.SharedMeal)
        {
            _state.Inventory.Items["meal_box"]--;
            if (_state.Inventory.Items["meal_box"] <= 0)
                _state.Inventory.Items.Remove("meal_box");
        }

        _stamina.Spend(cost);
        _state.Dating.LastActivityDay = _state.Calendar.Day;
        _state.Dating.LastActivityPhase = _state.Calendar.Phase;
        progress.SharedActivities++;
        progress.LastActivityId = kind.ToString();

        var compatibility = Compatibility(character, kind);
        var lowMood = character.Morale < 30 || character.Fatigue >= 75;
        var approach = _state.Dating.ActiveApproach;

        if (approach == DateInviteApproach.Forced)
        {
            ApplyForcedActivity(character, progress, kind);
            return DatingResult.Ok($"{DisplayName(character)} goes through with {ActivityName(kind)}, but the forced outing worsens trust and mood.");
        }

        if (approach == DateInviteApproach.Pressured && (compatibility < 0 || lowMood))
        {
            progress.PressuredMoments++;
            character.Morale = Clamp100(character.Morale - 3);
            character.Bond = Clamp100(character.Bond - 1);
            AdjustMind(character, dignity: -15, aversion: 90, antipathy: 70, fear: 10, favorability: -25);
            return DatingResult.Ok($"{DisplayName(character)} clearly isn't enjoying {ActivityName(kind)}. Pressing on creates distance.");
        }

        ApplyPositiveActivity(character, progress, kind, compatibility, lowMood, approach == DateInviteApproach.Pressured);
        return DatingResult.Ok($"{DisplayName(character)} spends time with you: {ActivityName(kind)}. {RelationshipSummary(character.Id)}");
    }

    public IReadOnlyList<string> ThoughtsFor(string characterId)
    {
        var character = FindCharacter(characterId);
        if (character is null)
            return Array.Empty<string>();

        var progress = ProgressFor(characterId);
        var thoughts = new List<string>();
        var active = string.Equals(ActivePartnerId, characterId, StringComparison.Ordinal);

        if (active && _state.Dating.ActiveApproach == DateInviteApproach.Forced)
        {
            thoughts.Add("I want to go back...");
            thoughts.Add("Why am I being made to do this?");
        }
        else if (active && _state.Dating.ActiveApproach == DateInviteApproach.Pressured)
        {
            thoughts.Add("I wish they'd listen when I hesitate...");
        }

        if (character.Fatigue >= 75)
            thoughts.Add("I'm exhausted. Somewhere quiet would be nice...");
        if (character.Morale < 30)
            thoughts.Add("I'm not really in the mood for this...");
        if (EffectiveAversion(character, progress) >= 6000)
            thoughts.Add("I still don't trust them.");

        if (OriginalCalendarRules.IsRain(_state.Calendar.CurrentWeather))
            thoughts.Add("We should find somewhere dry.");
        else if (OriginalCalendarRules.IsSnow(_state.Calendar.CurrentWeather))
            thoughts.Add("The snow makes the ranch look different today.");

        var stage = StageFor(characterId);
        if ((int)stage >= (int)RelationshipStage.Romantic)
            thoughts.Add("I like being together like this.");
        else if ((int)stage >= (int)RelationshipStage.Close)
            thoughts.Add("This is actually rather nice.");
        else if (active)
            thoughts.Add("I wonder where we're going.");

        return thoughts.Count > 0 ? thoughts : new[] { "A quiet day on the ranch..." };
    }

    public string RelationshipSummary(string characterId)
    {
        var character = FindCharacter(characterId);
        if (character is null)
            return "Relationship unavailable.";

        var stage = StageFor(characterId);
        return $"{stage} · Bond {character.Bond} · Morale {character.Morale} · Favorability {character.Mature.Favorability} · Aversion {EffectiveAversion(character, ProgressFor(characterId))}";
    }

    private void ApplyPositiveActivity(
        CharacterState character,
        DatingPartnerState progress,
        DateActivityKind kind,
        int compatibility,
        bool lowMood,
        bool pressured)
    {
        var scalePercent = lowMood ? 60 : 100;
        if (compatibility > 0) scalePercent += 20;
        if (compatibility < 0) scalePercent -= 20;
        if (pressured) scalePercent -= 25;
        scalePercent = Math.Clamp(scalePercent, 30, 130);

        int Scale(int value) => value * scalePercent / 100;

        switch (kind)
        {
            case DateActivityKind.WorkTogether:
            {
                // Source parity: working alongside the player first lowers aversion; once it is gone,
                // the same shared work begins increasing favorability.
                var roll = StableRoll(character.Id, _state.Calendar.Day, (int)_state.Calendar.Phase, 50);
                if (EffectiveAversion(character, progress) > 0)
                    character.Mature.Aversion = ClampMind(character.Mature.Aversion - Scale(100 + roll));
                else
                    character.Mature.Favorability = ClampFavorability(character.Mature.Favorability + Scale(30 + roll % 30));
                character.Bond = Clamp100(character.Bond + 2);
                character.Morale = Clamp100(character.Morale + 3);
                break;
            }

            case DateActivityKind.RanchWalk:
                character.Bond = Clamp100(character.Bond + 2);
                character.Morale = Clamp100(character.Morale + Scale(4));
                ApplyFriendlyMind(character, progress, Scale(120), Scale(80));
                break;

            case DateActivityKind.SharedMeal:
                character.Bond = Clamp100(character.Bond + 3);
                character.Morale = Clamp100(character.Morale + Scale(8));
                character.Fatigue = Clamp100(character.Fatigue - 3);
                ApplyFriendlyMind(character, progress, Scale(170), Scale(90));
                break;

            case DateActivityKind.TownOuting:
                character.Bond = Clamp100(character.Bond + 4);
                character.Morale = Clamp100(character.Morale + Scale(6));
                ApplyFriendlyMind(character, progress, Scale(220), Scale(120));
                break;

            case DateActivityKind.QuietRest:
                character.Bond = Clamp100(character.Bond + 2);
                character.Morale = Clamp100(character.Morale + Scale(6));
                character.Fatigue = Clamp100(character.Fatigue - 10);
                character.Energy = Math.Min(character.MaxEnergyOverride ?? 150, character.Energy + 15);
                ApplyFriendlyMind(character, progress, Scale(90), Scale(50));
                break;
        }

        progress.PositiveMoments++;
    }

    private void ApplyFriendlyMind(CharacterState character, DatingPartnerState progress, int favorability, int aversionReduction)
    {
        if (EffectiveAversion(character, progress) > 0)
        {
            character.Mature.Aversion = ClampMind(character.Mature.Aversion - aversionReduction);
        }
        else
        {
            character.Mature.Favorability = ClampFavorability(character.Mature.Favorability + favorability);
        }

        character.Mature.Antipathy = ClampMind(character.Mature.Antipathy - Math.Max(10, aversionReduction / 3));
        character.Mature.Fear = ClampMind(character.Mature.Fear - Math.Max(5, aversionReduction / 6));
    }

    private void ApplyForcedActivity(CharacterState character, DatingPartnerState progress, DateActivityKind kind)
    {
        progress.ForcedMoments++;
        var severe = kind == DateActivityKind.TownOuting || kind == DateActivityKind.WorkTogether;
        character.Bond = Clamp100(character.Bond - (severe ? 4 : 3));
        character.Morale = Clamp100(character.Morale - (severe ? 9 : 6));
        AdjustMind(
            character,
            resistance: severe ? 100 : 60,
            dignity: severe ? -70 : -40,
            aversion: severe ? 220 : 160,
            antipathy: severe ? 180 : 120,
            fear: severe ? 90 : 60,
            favorability: severe ? -120 : -80);
    }

    private int WillingnessScore(CharacterState character, DatingPartnerState progress)
    {
        // Older remake saves leave untouched non-captured residents at the generic 10000 mental
        // defaults. Do not treat that uninitialized placeholder as established hatred. Captured or
        // previously pressured residents always use their actual aversion.
        var aversion = EffectiveAversion(character, progress);
        return character.Bond * 30
            + character.Morale * 10
            + character.Mature.Favorability / 20
            - aversion / 20
            - character.Mature.Fear / 40
            - character.Mature.Antipathy / 40;
    }

    private static int EffectiveAversion(CharacterState character, DatingPartnerState progress)
    {
        var established = character.IsCaptured
            || progress.PressuredMoments > 0
            || progress.ForcedMoments > 0
            || character.Bond > 0
            || character.Mature.Favorability > 0
            || character.Mature.Aversion < 10000;
        return established ? character.Mature.Aversion : 0;
    }

    private static int Compatibility(CharacterState character, DateActivityKind kind)
    {
        var personality = character.Personality ?? string.Empty;
        var traits = character.Talents ?? new List<string>();

        bool Has(params string[] values) =>
            values.Any(value => personality.Contains(value, StringComparison.OrdinalIgnoreCase)
                || traits.Any(trait => trait.Contains(value, StringComparison.OrdinalIgnoreCase)));

        return kind switch
        {
            DateActivityKind.RanchWalk when Has("Playful", "Cheerful", "Outgoing", "Animal", "Relaxed") => 1,
            DateActivityKind.WorkTogether when Has("Diligent", "Serious", "Disciplined", "Dutiful", "Practical") => 1,
            DateActivityKind.SharedMeal when Has("Big Eater", "Cooking", "Warm", "Social") => 1,
            DateActivityKind.TownOuting when Has("Outgoing", "Playful", "Charming", "Curious") => 1,
            DateActivityKind.QuietRest when Has("Lazy", "Calm", "Shy", "Quiet", "Relaxed", "Anxious") => 1,
            DateActivityKind.TownOuting when Has("Shy", "Anxious", "Stoic") => -1,
            DateActivityKind.WorkTogether when Has("Lazy") => -1,
            _ => 0
        };
    }

    private void AdjustMind(
        CharacterState character,
        int resistance = 0,
        int dignity = 0,
        int aversion = 0,
        int antipathy = 0,
        int fear = 0,
        int favorability = 0)
    {
        var m = character.Mature;
        m.Resistance = ClampMind(m.Resistance + resistance);
        m.Dignity = ClampMind(m.Dignity + dignity);
        m.Aversion = ClampMind(m.Aversion + aversion);
        m.Antipathy = ClampMind(m.Antipathy + antipathy);
        m.Fear = ClampMind(m.Fear + fear);
        m.Favorability = ClampFavorability(m.Favorability + favorability);
    }

    private DatingPartnerState ProgressFor(string characterId)
    {
        if (!_state.Dating.Partners.TryGetValue(characterId, out var progress))
        {
            progress = new DatingPartnerState();
            _state.Dating.Partners[characterId] = progress;
        }

        return progress;
    }

    private CharacterState? FindCharacter(string characterId) =>
        _state.Roster.Characters.FirstOrDefault(character => string.Equals(character.Id, characterId, StringComparison.Ordinal));

    private static string DisplayName(CharacterState character) =>
        string.IsNullOrWhiteSpace(character.DisplayNameOverride) ? character.Id : character.DisplayNameOverride;

    private static string ActivityName(DateActivityKind kind) => kind switch
    {
        DateActivityKind.RanchWalk => "a walk around the ranch",
        DateActivityKind.WorkTogether => "working together",
        DateActivityKind.SharedMeal => "a shared meal",
        DateActivityKind.TownOuting => "an outing in Okachi Town",
        DateActivityKind.QuietRest => "quiet time together",
        _ => "time together"
    };

    private static int StableRoll(string id, int day, int phase, int range)
    {
        unchecked
        {
            var hash = 17;
            foreach (var ch in id)
                hash = hash * 31 + ch;
            hash = hash * 31 + day;
            hash = hash * 31 + phase;
            return Math.Abs(hash % Math.Max(1, range));
        }
    }

    private static int Clamp100(int value) => Math.Clamp(value, 0, 100);
    private static int ClampMind(int value) => Math.Clamp(value, 0, 10000);
    private static int ClampFavorability(int value) => Math.Clamp(value, 0, 20000);
}

public readonly record struct DatingResult(bool Success, string Message)
{
    public static DatingResult Ok(string message) => new(true, message);
    public static DatingResult Fail(string message) => new(false, message);
}
