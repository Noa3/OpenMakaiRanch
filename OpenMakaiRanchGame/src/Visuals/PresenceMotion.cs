using System;
using Godot;

namespace OpenMakaiRanch.Visuals;

public readonly record struct PresencePose(float Blink, float Smile, float BrowRaise, float BrowPinch,
    float JawOpen, Vector3 HeadDegrees, Vector3 BodyOffset);

/// <summary>
/// Deterministic, bounded presentation sampling. Seconds are a caller-owned visual timeline; no
/// Random.Shared, game ticks or wall clock. Artist-tunable timing here is not a clinical human model.
/// </summary>
public static class PresenceMotion
{
    public static uint StableSeed(string actorId)
    {
        uint hash = 2166136261;
        foreach (var c in actorId) hash = unchecked((hash ^ c) * 16777619);
        return hash;
    }

    private static float Noise(uint seed, uint cycle)
    {
        var n = unchecked(seed ^ (cycle * 747796405u + 2891336453u));
        n = unchecked((n ^ (n >> 16)) * 2246822519u);
        return (n & 0x00ffffff) / 16777215f;
    }
    private static float Smooth(float x) { x = Math.Clamp(x, 0, 1); return x * x * (3 - 2 * x); }
    private static float BlinkPulse(float age)
    {
        if (age < 0 || age > .20f) return 0;
        return age < .065f ? Smooth(age / .065f) : 1 - Smooth((age - .065f) / .135f);
    }

    public static PresencePose Sample(double seconds, string actorId, PresenceTemperament temperament,
        PresenceSnapshot state, double reactionAge = -1)
    {
        ArgumentNullException.ThrowIfNull(state);
        var p = temperament.Validated();
        var t = (float)(double.IsFinite(seconds) ? Math.Clamp(seconds, 0, 86400) : 0);
        var seed = StableSeed(actorId ?? "");
        var cycle = (uint)(t / 7f);
        var local = t % 7f;
        var start = 1f + 4.8f * Noise(seed, cycle);
        var blink = BlinkPulse(local - start);
        if (Noise(seed ^ 981u, cycle) > .85f) blink = Math.Max(blink, BlinkPulse(local - start - .31f));
        var fatigue = PresenceTemperament.Unit(state.Fatigue);
        var smile = state.Emotion == PresenceEmotion.Warm ? .16f + .35f * p.Warmth : 0;
        var brow = state.Emotion == PresenceEmotion.Curious ? .25f + .25f * p.Expressiveness : 0;
        var pinch = state.Emotion is PresenceEmotion.Irritated or PresenceEmotion.Concerned ? .25f + .25f * p.Expressiveness : 0;
        var jaw = .45f * PresenceTemperament.Unit(state.SpeechEnvelope);
        var movement = state.ReducedMotion ? 0 : (.35f + .65f * p.Expressiveness) * (1f - .3f * p.Composure);
        var glanceCycle = (uint)(t / 3.5f);
        var transition = Smooth((t % 3.5f) / .22f);
        var from = Noise(seed ^ 83u, glanceCycle == 0 ? 0 : glanceCycle - 1) * 2 - 1;
        var to = Noise(seed ^ 83u, glanceCycle) * 2 - 1;
        var attention = state.InConversation ? .85f - .35f * PresenceTemperament.Unit(state.Trust) : 1f;
        var glance = Mathf.Lerp(from, to, transition) * attention * (1.1f - .25f * p.Confidence);
        var acknowledgement = double.IsFinite(reactionAge) && reactionAge >= 0 && reactionAge <= .85
            ? Mathf.Sin((float)reactionAge / .85f * Mathf.Tau) * Mathf.Sin((float)reactionAge / .85f * Mathf.Pi) : 0f;
        var shift = state.Moving ? 0f : Mathf.Sin(t * .65f + Noise(seed, 33) * Mathf.Tau) * .003f;
        var head = new Vector3(fatigue * 2f + acknowledgement * 2.2f, glance * 3f,
            Mathf.Sin(t * .41f + Noise(seed, 14) * Mathf.Tau) * (.5f + .5f * p.Playfulness)) * movement;
        var body = new Vector3(shift, Mathf.Sin(t * 1.5f + Noise(seed, 19) * Mathf.Tau) * .0025f, 0) * movement;
        if (state.CinematicOwnsPose) return default;
        return new(Math.Clamp(Math.Max(blink, fatigue * .16f), 0, 1), smile, brow, pinch, jaw, head, body);
    }
}
