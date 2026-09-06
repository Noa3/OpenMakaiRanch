using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.World;

/// <summary>
/// A fully resolved lighting state for one time of day — every value a renderer needs, computed
/// once and deterministically from the shared <see cref="DayPhase"/>.
///
/// This is the presentation half of WORLD-003 ("lighting"). It is deliberately pure: it reads the
/// existing day/phase from the shared simulation and produces numbers. It owns no clock, advances
/// no day, and has no second source of truth — the world and the management UI both derive from the
/// same <see cref="DayPhase"/>, so the sky can never disagree with the settlement.
/// </summary>
public readonly record struct DaylightState(
    float SunEnergy,
    float SunElevationDegrees,
    float SunAzimuthDegrees,
    Godot.Color SunColor,
    Godot.Color AmbientColor,
    float AmbientEnergy,
    float TonemapExposure)
{
    /// <summary>True when the sun is below the horizon (night). Drives whether interior lamps show.</summary>
    public bool IsNight => SunElevationDegrees <= 0f;
}

/// <summary>
/// Maps the shared <see cref="DayPhase"/> (Morning / Afternoon / Evening / Night) to a
/// <see cref="DaylightState"/>. Deterministic, allocation-free, and Node-free so it can be verified
/// headlessly and unit-tested without a scene tree.
///
/// The mapping is intentionally explicit (a hand-tuned table, not a formula over a hidden clock) so
/// the four phases are stable, testable, and easy to art-direct later (ART-002) without touching the
/// simulation.
/// </summary>
public static class DaylightMath
{
    /// <summary>Warm morning sun, low on the east.</summary>
    public const float MorningElevation = 28f;
    public const float MorningAzimuth = 95f;
    /// <summary>High neutral-midday sun, near due south.</summary>
    public const float AfternoonElevation = 62f;
    public const float AfternoonAzimuth = 185f;
    /// <summary>Low warm evening sun, west.</summary>
    public const float EveningElevation = 14f;
    public const float EveningAzimuth = 262f;

    public static DaylightState For(DayPhase phase)
    {
        switch (phase)
        {
            case DayPhase.Afternoon:
                return new DaylightState(
                    SunEnergy: 1.0f,
                    SunElevationDegrees: AfternoonElevation,
                    SunAzimuthDegrees: AfternoonAzimuth,
                    SunColor: new Godot.Color(1.0f, 0.98f, 0.92f),
                    AmbientColor: new Godot.Color(0.62f, 0.68f, 0.78f),
                    AmbientEnergy: 0.55f,
                    TonemapExposure: 1.0f);

            case DayPhase.Evening:
                return new DaylightState(
                    SunEnergy: 0.55f,
                    SunElevationDegrees: EveningElevation,
                    SunAzimuthDegrees: EveningAzimuth,
                    SunColor: new Godot.Color(1.0f, 0.62f, 0.34f),
                    AmbientColor: new Godot.Color(0.42f, 0.34f, 0.42f),
                    AmbientEnergy: 0.4f,
                    TonemapExposure: 0.95f);

            case DayPhase.Night:
                // No direct sun; a cool low ambient keeps the scene legible. Interior lamps carry it.
                return new DaylightState(
                    SunEnergy: 0.0f,
                    SunElevationDegrees: -18f,
                    SunAzimuthDegrees: 185f,
                    SunColor: new Godot.Color(0.5f, 0.6f, 0.85f),
                    AmbientColor: new Godot.Color(0.18f, 0.2f, 0.3f),
                    AmbientEnergy: 0.35f,
                    TonemapExposure: 0.9f);

            case DayPhase.Morning:
            default:
                return new DaylightState(
                    SunEnergy: 0.8f,
                    SunElevationDegrees: MorningElevation,
                    SunAzimuthDegrees: MorningAzimuth,
                    SunColor: new Godot.Color(1.0f, 0.88f, 0.7f),
                    AmbientColor: new Godot.Color(0.5f, 0.6f, 0.72f),
                    AmbientEnergy: 0.45f,
                    TonemapExposure: 0.98f);
        }
    }
}
