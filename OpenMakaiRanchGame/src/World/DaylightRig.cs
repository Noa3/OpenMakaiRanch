using System;
using Godot;
using GodotEnvironment = Godot.Environment;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.World;

/// <summary>
/// Applies shared phase/weather state to lighting and renderer-aware atmosphere.
/// Desktop Forward+ can add SSAO/SSIL/SSR/volumetric fog, while Mobile/Compatibility keep the same
/// gameplay weather with cheaper fog/glow/color treatment.
/// </summary>
public partial class DaylightRig : Node3D
{
    private DirectionalLight3D? _sun;
    private WorldEnvironment? _worldEnvironment;
    private bool _wired;

    public DaylightState LastApplied { get; private set; }
    public Weather LastWeather { get; private set; } = Weather.Clear;

    public void Bind(DirectionalLight3D? sun, WorldEnvironment? worldEnvironment)
    {
        _sun = sun;
        _worldEnvironment = worldEnvironment;
        _wired = sun is not null || worldEnvironment is not null;
    }

    public bool Wired => _wired;

    public DaylightState Apply(DayPhase phase) => Apply(phase, Weather.Clear);

    public DaylightState Apply(DayPhase phase, Weather weather)
    {
        var state = DaylightMath.For(phase);
        var game = GameRoot.Instance;
        var settings = game?.State.Settings;

        if (_sun is not null)
        {
            _sun.LightEnergy = state.SunEnergy * WeatherSunMultiplier(weather);
            _sun.LightColor = WeatherSunColor(state.SunColor, weather);
            _sun.ShadowEnabled = game?.RuntimeSettings?.EffectiveShadowsEnabled ?? true;
            _sun.RotationDegrees = new Vector3(-state.SunElevationDegrees, -state.SunAzimuthDegrees, 0f);
            _sun.LightVolumetricFogEnergy = IsSevereAtmosphere(weather) ? 1.35f : 1.0f;
        }

        if (_worldEnvironment?.Environment is { } environment)
        {
            environment.AmbientLightColor = WeatherAmbient(state.AmbientColor, weather);
            environment.AmbientLightEnergy = state.AmbientEnergy * WeatherAmbientMultiplier(weather);
            environment.TonemapExposure = state.TonemapExposure * WeatherExposureMultiplier(weather);
            ApplyAtmosphere(environment, phase, weather, settings, game?.RuntimeSettings);
        }

        LastApplied = state;
        LastWeather = weather;
        return state;
    }

    public DaylightState ApplyFrom(GameRoot? game)
    {
        var phase = game?.State.Calendar.Phase ?? DayPhase.Morning;
        var weather = game?.State.Calendar.CurrentWeather ?? Weather.Clear;
        return Apply(phase, weather);
    }

    private static void ApplyAtmosphere(
        GodotEnvironment environment,
        DayPhase phase,
        Weather weather,
        SettingsState? settings,
        RuntimeSettingsService? runtime)
    {
        var atmosphereEnabled = settings?.AtmosphereEffectsEnabled ?? true;
        var weatherEnabled = settings?.WeatherEffectsEnabled ?? true;
        var quality = settings?.GraphicsQuality ?? "Medium";
        var profile = GraphicsQualityProfile.Resolve(quality);

        if (!atmosphereEnabled || !profile.Atmosphere)
        {
            DisableOptionalEnvironmentEffects(environment);
            return;
        }

        var brightness = phase switch
        {
            DayPhase.Morning => 1.02f,
            DayPhase.Afternoon => 1.00f,
            DayPhase.Evening => 0.98f,
            DayPhase.Night => 0.92f,
            _ => 1.00f
        };
        var contrast = phase switch
        {
            DayPhase.Evening => 1.04f,
            DayPhase.Night => 1.07f,
            _ => 1.01f
        };
        var saturation = phase switch
        {
            DayPhase.Morning => 1.04f,
            DayPhase.Evening => 1.08f,
            DayPhase.Night => 0.92f,
            _ => 1.00f
        };

        if (weatherEnabled)
        {
            var severity = WeatherVisualSeverity(weather);
            brightness *= Mathf.Lerp(1.0f, 0.84f, severity);
            saturation *= Mathf.Lerp(1.0f, OriginalCalendarRules.IsSnow(weather) ? 0.84f : 0.76f, severity);
            contrast *= OriginalCalendarRules.IsSevere(weather) ? 1.04f : 0.99f;
        }

        environment.AdjustmentEnabled = true;
        environment.AdjustmentBrightness = brightness;
        environment.AdjustmentContrast = contrast;
        environment.AdjustmentSaturation = saturation;

        environment.GlowEnabled = profile.Glow;
        environment.GlowBloom = phase switch
        {
            DayPhase.Evening => 0.08f,
            DayPhase.Night => 0.14f,
            _ => 0.035f
        };

        var fogNeeded = weatherEnabled && WeatherNeedsFog(weather);
        environment.FogEnabled = fogNeeded;
        environment.FogDensity = FogDensity(weather);
        environment.FogLightColor = WeatherFogColor(weather);
        environment.FogLightEnergy = phase == DayPhase.Night ? 0.55f : 0.82f;
        environment.FogSunScatter = OriginalCalendarRules.IsSevere(weather) ? 0.08f : 0.18f;
        environment.FogSkyAffect = OriginalCalendarRules.IsSevere(weather) ? 0.86f : 0.68f;

        ApplyForwardPlusEffects(environment, phase, weather, profile, runtime);
    }

    private static void ApplyForwardPlusEffects(
        GodotEnvironment environment,
        DayPhase phase,
        Weather weather,
        GraphicsQualityProfile profile,
        RuntimeSettingsService? runtime)
    {
        var forwardPlus = runtime?.IsForwardPlus == true;
        var advanced = runtime?.EffectiveAdvancedLightingEnabled == true;
        if (!forwardPlus || !advanced)
        {
            environment.SsaoEnabled = false;
            environment.SsilEnabled = false;
            environment.SsrEnabled = false;
            environment.VolumetricFogEnabled = false;
            return;
        }

        var high = profile.Name is "High" or "Ultra";
        var ultra = profile.Name == "Ultra";

        environment.SsaoEnabled = profile.Ssao;
        environment.SsaoRadius = high ? 1.25f : 0.9f;
        environment.SsaoIntensity = high ? 2.1f : 1.6f;

        environment.SsilEnabled = profile.Ssil;
        environment.SsilRadius = ultra ? 7.0f : 4.5f;
        environment.SsilIntensity = ultra ? 1.15f : 0.8f;

        // Screen-space reflections matter most when the scene is visually wet/icy or at night.
        var reflectiveSituation = OriginalCalendarRules.IsRain(weather)
            || OriginalCalendarRules.IsSnow(weather)
            || phase == DayPhase.Night;
        environment.SsrEnabled = profile.Ssr && reflectiveSituation;
        environment.SsrMaxSteps = profile.SsrMaxSteps;

        var volumetricSituation = WeatherNeedsFog(weather)
            || phase == DayPhase.Night && weather != Weather.Clear;
        environment.VolumetricFogEnabled = profile.VolumetricFog && volumetricSituation;
        environment.VolumetricFogDensity = weather switch
        {
            Weather.Blizzard => 0.030f,
            Weather.TorrentialRain or Weather.Storm => 0.022f,
            Weather.HeavySnow or Weather.HeavyRain => 0.016f,
            Weather.Snow or Weather.Rain => 0.010f,
            Weather.Drizzle or Weather.Cloudy => 0.006f,
            _ => phase == DayPhase.Night ? 0.0035f : 0.0f
        };
        environment.VolumetricFogLength = profile.VolumetricFogLength;
        environment.VolumetricFogAmbientInject = phase == DayPhase.Night ? 0.45f : 0.25f;
        environment.VolumetricFogAnisotropy = OriginalCalendarRules.IsRain(weather) ? 0.42f : 0.24f;
        environment.VolumetricFogSkyAffect = OriginalCalendarRules.IsSevere(weather) ? 0.92f : 0.70f;
    }

    private static void DisableOptionalEnvironmentEffects(GodotEnvironment environment)
    {
        environment.AdjustmentEnabled = false;
        environment.GlowEnabled = false;
        environment.FogEnabled = false;
        environment.SsaoEnabled = false;
        environment.SsilEnabled = false;
        environment.SsrEnabled = false;
        environment.VolumetricFogEnabled = false;
    }

    private static float WeatherSunMultiplier(Weather weather) => weather switch
    {
        Weather.Cloudy => 0.78f,
        Weather.Drizzle => 0.72f,
        Weather.Rain => 0.62f,
        Weather.HeavyRain => 0.52f,
        Weather.TorrentialRain or Weather.Storm => 0.40f,
        Weather.StrongWind => 0.88f,
        Weather.Snow => 0.72f,
        Weather.HeavySnow => 0.58f,
        Weather.Blizzard => 0.38f,
        _ => 1.0f
    };

    private static Color WeatherSunColor(Color source, Weather weather) => weather switch
    {
        Weather.TorrentialRain or Weather.Storm or Weather.Blizzard =>
            source.Lerp(new Color(0.58f, 0.65f, 0.76f), 0.58f),
        Weather.HeavyRain or Weather.HeavySnow =>
            source.Lerp(new Color(0.64f, 0.70f, 0.80f), 0.48f),
        Weather.Rain or Weather.Snow =>
            source.Lerp(new Color(0.68f, 0.74f, 0.82f), 0.38f),
        Weather.Drizzle or Weather.Cloudy =>
            source.Lerp(new Color(0.78f, 0.81f, 0.86f), 0.22f),
        _ => source
    };

    private static Color WeatherAmbient(Color source, Weather weather) => weather switch
    {
        Weather.Cloudy or Weather.Drizzle => source.Lerp(new Color(0.55f, 0.60f, 0.68f), 0.22f),
        Weather.Rain or Weather.Snow => source.Lerp(new Color(0.42f, 0.49f, 0.59f), 0.36f),
        Weather.HeavyRain or Weather.HeavySnow => source.Lerp(new Color(0.34f, 0.40f, 0.50f), 0.46f),
        Weather.TorrentialRain or Weather.Storm or Weather.Blizzard =>
            source.Lerp(new Color(0.28f, 0.33f, 0.42f), 0.58f),
        _ => source
    };

    private static float WeatherAmbientMultiplier(Weather weather) => weather switch
    {
        Weather.Cloudy or Weather.Drizzle => 0.92f,
        Weather.Rain or Weather.Snow => 0.82f,
        Weather.HeavyRain or Weather.HeavySnow => 0.74f,
        Weather.TorrentialRain or Weather.Storm or Weather.Blizzard => 0.64f,
        _ => 1.0f
    };

    private static float WeatherExposureMultiplier(Weather weather) => weather switch
    {
        Weather.Cloudy or Weather.Drizzle => 0.98f,
        Weather.Rain or Weather.Snow => 0.94f,
        Weather.HeavyRain or Weather.HeavySnow => 0.91f,
        Weather.TorrentialRain or Weather.Storm or Weather.Blizzard => 0.86f,
        _ => 1.0f
    };

    private static bool WeatherNeedsFog(Weather weather) => weather is not
        (Weather.Clear or Weather.StrongWind);

    private static float FogDensity(Weather weather) => weather switch
    {
        Weather.Cloudy => 0.0045f,
        Weather.Drizzle => 0.006f,
        Weather.Rain => 0.010f,
        Weather.HeavyRain => 0.014f,
        Weather.TorrentialRain or Weather.Storm => 0.022f,
        Weather.Snow => 0.009f,
        Weather.HeavySnow => 0.016f,
        Weather.Blizzard => 0.026f,
        _ => 0.0f
    };

    private static Color WeatherFogColor(Weather weather) => weather switch
    {
        Weather.Blizzard or Weather.HeavySnow => new Color(0.70f, 0.76f, 0.82f),
        Weather.Snow => new Color(0.76f, 0.80f, 0.86f),
        Weather.TorrentialRain or Weather.Storm => new Color(0.35f, 0.40f, 0.48f),
        Weather.HeavyRain or Weather.Rain => new Color(0.48f, 0.54f, 0.62f),
        _ => new Color(0.64f, 0.68f, 0.72f)
    };

    private static float WeatherVisualSeverity(Weather weather) => weather switch
    {
        Weather.Cloudy => 0.12f,
        Weather.Drizzle => 0.25f,
        Weather.Rain or Weather.Snow => 0.45f,
        Weather.StrongWind => 0.28f,
        Weather.HeavyRain or Weather.HeavySnow => 0.68f,
        Weather.TorrentialRain or Weather.Storm or Weather.Blizzard => 1.0f,
        _ => 0.0f
    };

    private static bool IsSevereAtmosphere(Weather weather) =>
        OriginalCalendarRules.IsSevere(weather) || weather == Weather.Storm;
}
