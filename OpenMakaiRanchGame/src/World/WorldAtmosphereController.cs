using System;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.World;

/// <summary>
/// Presentation-only environmental life layer. It follows the active player with bounded particle
/// volumes instead of filling the entire map: rain/snow, seasonal leaves/pollen and subtle night
/// motes. Gameplay weather/season remains owned by CalendarState.
/// </summary>
public partial class WorldAtmosphereController : Node3D
{
    [Export] public NodePath PlayerPath { get; set; } = "../Player";
    [Export] public float FollowHeight { get; set; } = 8.0f;
    [Export] public float FollowRadius { get; set; } = 13.0f;

    private ThirdPersonPlayerController? _player;
    private GPUParticles3D? _weather;
    private GPUParticles3D? _seasonal;
    private GPUParticles3D? _nightMotes;
    private Weather _lastWeather = (Weather)(-1);
    private Season _lastSeason = (Season)(-1);
    private DayPhase _lastPhase = (DayPhase)(-1);
    private string _lastQuality = string.Empty;
    private bool _lastParticlesEnabled;

    public int WeatherParticleAmount => _weather?.Amount ?? 0;
    public int SeasonalParticleAmount => _seasonal?.Amount ?? 0;
    public bool WeatherEmitting => _weather?.Emitting == true;
    public bool SeasonalEmitting => _seasonal?.Emitting == true;

    public override void _Ready()
    {
        _player = GetNodeOrNull<ThirdPersonPlayerController>(PlayerPath);
        BuildEmitters();

        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged += Refresh;
        }

        Refresh();
    }

    public override void _ExitTree()
    {
        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged -= Refresh;
        }
    }

    public override void _Process(double delta)
    {
        if (_player is not null && GodotObject.IsInstanceValid(_player))
        {
            GlobalPosition = _player.GlobalPosition + Vector3.Up * FollowHeight;
        }

        // StateChanged normally handles this. This cheap guard also catches direct dev/test changes.
        if (GameRoot.Instance is { } game)
        {
            var cal = game.State.Calendar;
            var quality = game.State.Settings.GraphicsQuality;
            var enabled = game.RuntimeSettings.EffectiveWorldParticlesEnabled;
            if (cal.CurrentWeather != _lastWeather || cal.Season != _lastSeason || cal.Phase != _lastPhase
                || !string.Equals(quality, _lastQuality, StringComparison.Ordinal) || enabled != _lastParticlesEnabled)
            {
                Refresh();
            }
        }
    }

    public void Refresh()
    {
        var game = GameRoot.Instance;
        if (game is null || _weather is null || _seasonal is null || _nightMotes is null)
        {
            return;
        }

        var cal = game.State.Calendar;
        var settings = game.State.Settings;
        var particlesEnabled = game.RuntimeSettings.EffectiveWorldParticlesEnabled;
        var density = Mathf.Clamp(game.RuntimeSettings.ParticleDensity, 0.15f, 1.3f);

        ConfigureWeather(_weather, cal.CurrentWeather, particlesEnabled, density);
        ConfigureSeasonal(_seasonal, cal.Season, cal.CurrentWeather, particlesEnabled, density);
        ConfigureNightMotes(_nightMotes, cal.Season, cal.Phase, cal.CurrentWeather, particlesEnabled, density);

        _lastWeather = cal.CurrentWeather;
        _lastSeason = cal.Season;
        _lastPhase = cal.Phase;
        _lastQuality = settings.GraphicsQuality;
        _lastParticlesEnabled = particlesEnabled;
    }

    private void BuildEmitters()
    {
        _weather = CreateEmitter("WeatherParticles");
        _seasonal = CreateEmitter("SeasonalParticles");
        _nightMotes = CreateEmitter("NightMotes");

        AddChild(_weather);
        AddChild(_seasonal);
        AddChild(_nightMotes);
    }

    private GPUParticles3D CreateEmitter(string name)
    {
        var particles = new GPUParticles3D
        {
            Name = name,
            Emitting = false,
            Amount = 64,
            Lifetime = 2.0f,
            Randomness = 0.35f,
            FixedFps = 30,
            Interpolate = true
        };
        particles.Set("local_coords", false);
        particles.Set("visibility_aabb", new Aabb(
            new Vector3(-FollowRadius - 4f, -FollowHeight - 8f, -FollowRadius - 4f),
            new Vector3((FollowRadius + 4f) * 2f, FollowHeight + 18f, (FollowRadius + 4f) * 2f)));
        return particles;
    }

    private void ConfigureWeather(GPUParticles3D particles, Weather weather, bool enabled, float density)
    {
        if (!enabled || (!OriginalCalendarRules.IsRain(weather) && !OriginalCalendarRules.IsSnow(weather)))
        {
            particles.Emitting = false;
            return;
        }

        var snow = OriginalCalendarRules.IsSnow(weather);
        var severity = WeatherSeverity(weather);
        particles.Amount = Math.Max(32, Mathf.RoundToInt((snow ? 260f : 520f) * severity * density));
        particles.Lifetime = snow ? 4.2f : 1.8f;
        particles.Randomness = snow ? 0.65f : 0.30f;

        var wind = OriginalCalendarRules.IsStrongWind(weather) ? 3.2f : 0.8f;
        var process = new ParticleProcessMaterial
        {
            Direction = snow ? new Vector3(0.25f, -1f, 0.10f).Normalized() : new Vector3(0.08f, -1f, 0.03f).Normalized(),
            Spread = snow ? 28f : 7f,
            Gravity = new Vector3(wind, snow ? -0.9f : -7.5f, 0.35f),
            InitialVelocityMin = snow ? 1.4f : 10f,
            InitialVelocityMax = snow ? 3.2f : 16f,
            EmissionBoxExtents = new Vector3(FollowRadius, 1.4f, FollowRadius),
            Color = snow ? new Color(0.93f, 0.97f, 1f, 0.88f) : new Color(0.62f, 0.78f, 1f, 0.72f)
        };
        process.Set("emission_shape", 3);
        particles.ProcessMaterial = process;
        particles.DrawPass1 = ParticleBox(
            snow ? new Vector3(0.055f, 0.055f, 0.055f) : new Vector3(0.018f, 0.55f, 0.018f),
            snow ? new Color(0.95f, 0.98f, 1f, 0.9f) : new Color(0.55f, 0.72f, 1f, 0.68f));
        particles.Emitting = true;
    }

    private void ConfigureSeasonal(GPUParticles3D particles, Season season, Weather weather, bool enabled, float density)
    {
        // Severe precipitation already provides enough motion/readability.
        if (!enabled || OriginalCalendarRules.IsSevere(weather) || weather == Weather.Storm
            || season is Season.Winter)
        {
            particles.Emitting = false;
            return;
        }

        var autumn = season == Season.Autumn;
        var spring = season == Season.Spring;
        if (!autumn && !spring)
        {
            particles.Emitting = false;
            return;
        }

        particles.Amount = Math.Max(16, Mathf.RoundToInt((autumn ? 135f : 70f) * density));
        particles.Lifetime = autumn ? 6.5f : 5.0f;
        particles.Randomness = 0.75f;

        var wind = OriginalCalendarRules.IsStrongWind(weather) ? 4.2f : 1.2f;
        var process = new ParticleProcessMaterial
        {
            Direction = new Vector3(0.35f, -0.8f, 0.12f).Normalized(),
            Spread = autumn ? 55f : 70f,
            Gravity = new Vector3(wind, autumn ? -0.55f : -0.25f, 0.4f),
            InitialVelocityMin = 0.4f,
            InitialVelocityMax = autumn ? 2.3f : 1.2f,
            EmissionBoxExtents = new Vector3(FollowRadius, 3.0f, FollowRadius),
            Color = autumn ? new Color(0.92f, 0.50f, 0.16f, 0.88f) : new Color(1f, 0.78f, 0.88f, 0.62f)
        };
        process.Set("emission_shape", 3);
        particles.ProcessMaterial = process;
        particles.DrawPass1 = ParticleBox(
            autumn ? new Vector3(0.085f, 0.025f, 0.14f) : new Vector3(0.035f, 0.015f, 0.055f),
            autumn ? new Color(0.95f, 0.48f, 0.12f, 0.86f) : new Color(1f, 0.76f, 0.86f, 0.60f));
        particles.Emitting = true;
    }

    private void ConfigureNightMotes(
        GPUParticles3D particles,
        Season season,
        DayPhase phase,
        Weather weather,
        bool enabled,
        float density)
    {
        var calmNight = phase is DayPhase.Evening or DayPhase.Night
            && weather is Weather.Clear or Weather.Cloudy
            && season is Season.Spring or Season.Summer;

        if (!enabled || !calmNight)
        {
            particles.Emitting = false;
            return;
        }

        particles.Amount = Math.Max(10, Mathf.RoundToInt(42f * density));
        particles.Lifetime = 4.5f;
        particles.Randomness = 0.8f;
        var process = new ParticleProcessMaterial
        {
            Direction = Vector3.Up,
            Spread = 85f,
            Gravity = new Vector3(0.1f, 0.12f, 0.05f),
            InitialVelocityMin = 0.08f,
            InitialVelocityMax = 0.45f,
            EmissionBoxExtents = new Vector3(FollowRadius * 0.8f, 2.0f, FollowRadius * 0.8f),
            Color = new Color(0.92f, 1f, 0.58f, 0.72f)
        };
        process.Set("emission_shape", 3);
        particles.ProcessMaterial = process;
        particles.DrawPass1 = ParticleBox(new Vector3(0.035f, 0.035f, 0.035f), new Color(0.92f, 1f, 0.55f, 0.75f), emission: true);
        particles.Emitting = true;
    }

    private static BoxMesh ParticleBox(Vector3 size, Color color, bool emission = false)
    {
        var material = new StandardMaterial3D
        {
            AlbedoColor = color,
            Roughness = 0.65f,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            VertexColorUseAsAlbedo = true,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha
        };
        if (emission)
        {
            material.EmissionEnabled = true;
            material.Emission = new Color(color.R, color.G, color.B);
            material.EmissionEnergyMultiplier = 1.5f;
        }

        return new BoxMesh
        {
            Size = size,
            Material = material
        };
    }

    private static float WeatherSeverity(Weather weather) => weather switch
    {
        Weather.Drizzle => 0.35f,
        Weather.Rain or Weather.Snow => 0.60f,
        Weather.HeavyRain or Weather.HeavySnow => 0.85f,
        Weather.TorrentialRain or Weather.Storm or Weather.Blizzard => 1.15f,
        _ => 0.50f
    };
}
