using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.World;

/// <summary>
/// Applies phase/weather lighting and renderer-compatible atmosphere to the world.
/// Pure presentation: no clock, weather or gameplay state is mutated here.
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

    public DaylightState Apply(DayPhase phase)
    {
        return Apply(phase, Weather.Clear);
    }

    public DaylightState Apply(DayPhase phase, Weather weather)
    {
        var state = DaylightMath.For(phase);
        var game = GameRoot.Instance;
        var settings = game?.State.Settings;

        if (_sun is not null)
        {
            var weatherSunMultiplier = weather switch
            {
                Weather.Cloudy => 0.78f,
                Weather.Rain => 0.62f,
                Weather.Storm => 0.42f,
                _ => 1.0f
            };

            _sun.LightEnergy = state.SunEnergy * weatherSunMultiplier;
            _sun.LightColor = weather switch
            {
                Weather.Storm => state.SunColor.Lerp(new Color(0.58f, 0.65f, 0.76f), 0.55f),
                Weather.Rain => state.SunColor.Lerp(new Color(0.68f, 0.74f, 0.82f), 0.38f),
                Weather.Cloudy => state.SunColor.Lerp(new Color(0.78f, 0.81f, 0.86f), 0.22f),
                _ => state.SunColor
            };
            _sun.ShadowEnabled = game?.RuntimeSettings?.EffectiveShadowsEnabled ?? true;
            _sun.RotationDegrees = new Vector3(-state.SunElevationDegrees, -state.SunAzimuthDegrees, 0f);
        }

        if (_worldEnvironment?.Environment is { } environment)
        {
            environment.AmbientLightColor = WeatherAmbient(state.AmbientColor, weather);
            environment.AmbientLightEnergy = state.AmbientEnergy * WeatherAmbientMultiplier(weather);
            environment.TonemapExposure = state.TonemapExposure * WeatherExposureMultiplier(weather);

            ApplyAtmosphere(environment, phase, weather, settings);
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

    private static void ApplyAtmosphere(Environment environment, DayPhase phase, Weather weather, SettingsState? settings)
    {
        var atmosphereEnabled = settings?.AtmosphereEffectsEnabled ?? true;
        var weatherEnabled = settings?.WeatherEffectsEnabled ?? true;
        var quality = settings?.GraphicsQuality ?? "Medium";
        var lowQuality = string.Equals(quality, "Low", System.StringComparison.OrdinalIgnoreCase);

        if (!atmosphereEnabled || lowQuality)
        {
            environment.AdjustmentEnabled = false;
            environment.GlowEnabled = false;
            environment.FogEnabled = false;
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
            switch (weather)
            {
                case Weather.Cloudy:
                    brightness *= 0.98f;
                    saturation *= 0.94f;
                    break;
                case Weather.Rain:
                    brightness *= 0.94f;
                    saturation *= 0.86f;
                    contrast *= 0.98f;
                    break;
                case Weather.Storm:
                    brightness *= 0.86f;
                    saturation *= 0.76f;
                    contrast *= 1.05f;
                    break;
            }
        }

        environment.AdjustmentEnabled = true;
        environment.AdjustmentBrightness = brightness;
        environment.AdjustmentContrast = contrast;
        environment.AdjustmentSaturation = saturation;

        var allowGlow = quality is "High" or "Ultra" or "Custom";
        environment.GlowEnabled = allowGlow;
        environment.GlowBloom = phase switch
        {
            DayPhase.Evening => 0.08f,
            DayPhase.Night => 0.14f,
            _ => 0.035f
        };

        var fogNeeded = weatherEnabled && weather is Weather.Cloudy or Weather.Rain or Weather.Storm;
        environment.FogEnabled = fogNeeded;
        environment.FogDensity = weather switch
        {
            Weather.Cloudy => 0.0045f,
            Weather.Rain => 0.010f,
            Weather.Storm => 0.020f,
            _ => 0.0f
        };
        environment.FogLightColor = weather switch
        {
            Weather.Storm => new Color(0.35f, 0.40f, 0.48f),
            Weather.Rain => new Color(0.48f, 0.54f, 0.62f),
            _ => new Color(0.64f, 0.68f, 0.72f)
        };
        environment.FogLightEnergy = phase == DayPhase.Night ? 0.55f : 0.82f;
        environment.FogSunScatter = weather == Weather.Storm ? 0.08f : 0.18f;
        environment.FogSkyAffect = weather == Weather.Storm ? 0.86f : 0.68f;
    }

    private static Color WeatherAmbient(Color source, Weather weather)
    {
        return weather switch
        {
            Weather.Cloudy => source.Lerp(new Color(0.55f, 0.60f, 0.68f), 0.22f),
            Weather.Rain => source.Lerp(new Color(0.42f, 0.49f, 0.59f), 0.36f),
            Weather.Storm => source.Lerp(new Color(0.28f, 0.33f, 0.42f), 0.55f),
            _ => source
        };
    }

    private static float WeatherAmbientMultiplier(Weather weather) => weather switch
    {
        Weather.Cloudy => 0.92f,
        Weather.Rain => 0.82f,
        Weather.Storm => 0.68f,
        _ => 1.0f
    };

    private static float WeatherExposureMultiplier(Weather weather) => weather switch
    {
        Weather.Cloudy => 0.98f,
        Weather.Rain => 0.94f,
        Weather.Storm => 0.88f,
        _ => 1.0f
    };
}
