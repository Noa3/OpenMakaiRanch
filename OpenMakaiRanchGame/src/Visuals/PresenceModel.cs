using System;
using System.Collections.Generic;

namespace OpenMakaiRanch.Visuals;

/// <summary>Presentation preferences, not demographic categories or replacements for original traits.</summary>
public readonly record struct PresenceTemperament(
    float Sociability = .5f, float Expressiveness = .5f, float Composure = .5f,
    float Confidence = .5f, float Playfulness = .5f, float Curiosity = .5f,
    float Diligence = .5f, float Warmth = .5f)
{
    public PresenceTemperament() : this(.5f, .5f, .5f, .5f, .5f, .5f, .5f, .5f) { }
    public static float Unit(float value, float fallback = 0f) => float.IsFinite(value) ? Math.Clamp(value, 0f, 1f) : fallback;
    public PresenceTemperament Validated() => new(Unit(Sociability, .5f), Unit(Expressiveness, .5f),
        Unit(Composure, .5f), Unit(Confidence, .5f), Unit(Playfulness, .5f), Unit(Curiosity, .5f),
        Unit(Diligence, .5f), Unit(Warmth, .5f));

    public static PresenceTemperament Blend(PresenceTemperament a, PresenceTemperament b, float weight)
    {
        a = a.Validated(); b = b.Validated(); weight = Unit(weight);
        // Preserve exact authored endpoints instead of accumulating interpolation roundoff.
        if (weight <= 0f) return a;
        if (weight >= 1f) return b;
        float Mix(float x, float y) => x + (y - x) * weight;
        return new(Mix(a.Sociability, b.Sociability), Mix(a.Expressiveness, b.Expressiveness),
            Mix(a.Composure, b.Composure), Mix(a.Confidence, b.Confidence), Mix(a.Playfulness, b.Playfulness),
            Mix(a.Curiosity, b.Curiosity), Mix(a.Diligence, b.Diligence), Mix(a.Warmth, b.Warmth));
    }
}

/// <summary>Convenience starting points. Custom/blended profiles remain unrestricted by this catalogue.</summary>
public static class PresenceArchetypes
{
    public static IReadOnlyDictionary<string, PresenceTemperament> All { get; } =
        new System.Collections.ObjectModel.ReadOnlyDictionary<string, PresenceTemperament>(
            new Dictionary<string, PresenceTemperament>(StringComparer.Ordinal)
            {
                ["quiet_reserved"] = new(.2f, .3f, .6f, .3f, .2f, .6f, .6f, .6f),
                ["calm_stoic"] = new(.3f, .15f, .95f, .7f, .15f, .5f, .8f, .45f),
                ["proud_guarded"] = new(.4f, .6f, .45f, .8f, .4f, .5f, .7f, .4f),
                ["lively_social"] = new(.95f, .9f, .3f, .8f, .8f, .75f, .4f, .8f),
                ["playful_teaser"] = new(.8f, .75f, .55f, .8f, .95f, .7f, .4f, .65f),
                ["warm_caretaker"] = new(.75f, .6f, .75f, .65f, .4f, .5f, .8f, .95f),
                ["refined_formal"] = new(.5f, .4f, .9f, .8f, .25f, .5f, .8f, .6f),
                ["dutiful_perfectionist"] = new(.4f, .35f, .8f, .65f, .15f, .6f, .95f, .5f),
                ["curious_scholar"] = new(.45f, .5f, .7f, .5f, .4f, .98f, .75f, .6f),
                ["dreamy_artist"] = new(.35f, .65f, .5f, .45f, .65f, .9f, .3f, .75f),
                ["competitive_athlete"] = new(.7f, .75f, .6f, .9f, .55f, .65f, .85f, .6f),
                ["tired_realist"] = new(.3f, .3f, .7f, .6f, .3f, .35f, .55f, .45f)
            });

    public static PresenceTemperament Resolve(string? id) => id is not null && All.TryGetValue(id, out var value)
        ? value : new(.5f, .5f, .5f, .5f, .5f, .5f, .5f, .5f);
}

public enum PresencePhase { Morning, Afternoon, Evening, Night }
public enum PresenceEmotion { Neutral, Warm, Curious, Concerned, Irritated, Tired }
public enum PresenceIntent { Idle, Shelter, Rest, Meal, Wash, Duty, Follow, Listen, Company, Quiet, Observe }
[Flags]
public enum PresenceOpportunities
{
    None = 0, Shelter = 1, Rest = 2, Meal = 4, Wash = 8, Duty = 16,
    Follow = 32, Listen = 64, Company = 128, Quiet = 256, Observe = 512
}

/// <summary>
/// Immutable read model supplied by the simulation owner. Needs are normalized urgency, not inventory.
/// Unknown opportunities default closed. Neither this object nor its consumers advance the game clock.
/// </summary>
public sealed record PresenceSnapshot
{
    public string ActorId { get; init; } = "";
    public string SessionId { get; init; } = "";
    public long Revision { get; init; }
    public PresencePhase Phase { get; init; }
    public PresenceEmotion Emotion { get; init; }
    public PresenceOpportunities Available { get; init; }
    public float Fatigue { get; init; }
    public float Hunger { get; init; }
    public float HygieneNeed { get; init; }
    public float SocialNeed { get; init; }
    public float QuietNeed { get; init; }
    public float Trust { get; init; }
    public float SpeechEnvelope { get; init; }
    public bool UnsafeWeather { get; init; }
    public bool InConversation { get; init; }
    public bool DutyActive { get; init; }
    public bool CompanionAccepted { get; init; }
    public bool PhenomenonVisible { get; init; }
    public bool Moving { get; init; }
    public bool Paused { get; init; }
    public bool CinematicOwnsPose { get; init; }
    public bool ReducedMotion { get; init; }
}

public readonly record struct PresenceSuggestion(string ActorId, string SessionId, long Revision,
    PresenceIntent Intent, string Reason, float Score)
{
    public bool Matches(PresenceSnapshot current) => ActorId == current.ActorId && SessionId == current.SessionId
        && Revision == current.Revision && !string.IsNullOrWhiteSpace(SessionId) && !string.IsNullOrWhiteSpace(ActorId);
}

/// <summary>
/// Read-only utility recommendation. No movement, rewards, need recovery, relationship mutation or
/// forced invitations. The command owner must revalidate revision, availability, path and reservation.
/// </summary>
public static class PresenceIntentAdvisor
{
    public static PresenceSuggestion Recommend(PresenceSnapshot state, PresenceTemperament temperament,
        PresenceIntent previous = PresenceIntent.Idle, float heldSeconds = 0f)
    {
        ArgumentNullException.ThrowIfNull(state);
        var p = temperament.Validated();
        PresenceSuggestion Result(PresenceIntent intent, string reason, float score) =>
            new(state.ActorId, state.SessionId, state.Revision, intent, reason, score);
        bool Has(PresenceOpportunities opportunity) => (state.Available & opportunity) != 0;
        if (string.IsNullOrWhiteSpace(state.ActorId) || string.IsNullOrWhiteSpace(state.SessionId))
            return Result(PresenceIntent.Idle, "missing_identity", 0);
        if (state.Paused || state.CinematicOwnsPose) return Result(PresenceIntent.Idle, "presentation_locked", 0);
        if (state.UnsafeWeather)
            return Has(PresenceOpportunities.Shelter) ? Result(PresenceIntent.Shelter, "unsafe_weather", 100)
                : Result(PresenceIntent.Idle, "shelter_unavailable", 100);
        if (PresenceTemperament.Unit(state.Fatigue) >= .9f)
            return Has(PresenceOpportunities.Rest) ? Result(PresenceIntent.Rest, "urgent_fatigue", 95)
                : Result(PresenceIntent.Idle, "rest_unavailable", 95);
        if (PresenceTemperament.Unit(state.Hunger) >= .9f)
            return Has(PresenceOpportunities.Meal) ? Result(PresenceIntent.Meal, "urgent_hunger", 94)
                : Result(PresenceIntent.Idle, "meal_unavailable", 94);
        // A requested conversation or an already accepted outing owns attention, not inferred affection.
        if (state.InConversation && Has(PresenceOpportunities.Listen))
            return Result(PresenceIntent.Listen, "active_conversation", 90);
        if (state.CompanionAccepted && Has(PresenceOpportunities.Follow))
            return Result(PresenceIntent.Follow, "accepted_companionship", 85);
        if (state.DutyActive && Has(PresenceOpportunities.Duty))
            return Result(PresenceIntent.Duty, "scheduled_commitment", 80);

        var best = Result(PresenceIntent.Idle, "no_available_need", 5);
        void Offer(PresenceIntent intent, PresenceOpportunities opportunity, float score, string reason)
        {
            if (!Has(opportunity)) return;
            // Retention is bounded and only offered for a still valid candidate; emergencies bypass it.
            var retention = intent == previous && float.IsFinite(heldSeconds) && heldSeconds >= 0 && heldSeconds < 8 ? 8f : 0f;
            if (score + retention > best.Score) best = Result(intent, reason, score + retention);
        }
        var night = state.Phase == PresencePhase.Night;
        Offer(PresenceIntent.Rest, PresenceOpportunities.Rest, 55 * PresenceTemperament.Unit(state.Fatigue) + (night ? 18 : 0), "rest_need");
        Offer(PresenceIntent.Meal, PresenceOpportunities.Meal, 65 * PresenceTemperament.Unit(state.Hunger), "hunger");
        Offer(PresenceIntent.Wash, PresenceOpportunities.Wash, 45 * PresenceTemperament.Unit(state.HygieneNeed), "hygiene");
        Offer(PresenceIntent.Company, PresenceOpportunities.Company, PresenceTemperament.Unit(state.SocialNeed) * (35 + 20 * p.Sociability), "social_need");
        Offer(PresenceIntent.Quiet, PresenceOpportunities.Quiet, PresenceTemperament.Unit(state.QuietNeed) * (35 + 20 * (1 - p.Sociability)), "quiet_need");
        if (state.PhenomenonVisible)
            Offer(PresenceIntent.Observe, PresenceOpportunities.Observe, 10 + 24 * p.Curiosity, "visible_phenomenon");
        return best;
    }
}
