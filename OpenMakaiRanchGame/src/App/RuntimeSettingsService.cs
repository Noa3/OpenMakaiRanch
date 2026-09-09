using System;
using Godot;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.App;

/// <summary>
/// Central runtime application of local user preferences. Gameplay state never depends on these
/// values, so changing quality, audio or controls cannot alter simulation results or save parity.
/// </summary>
public partial class RuntimeSettingsService : Node
{
    public const string MasterBus = "Master";
    public const string MusicBus = "Music";
    public const string SfxBus = "SFX";
    public const string UiBus = "UI";

    private SettingsState? _current;
    private bool _focusMuted;

    public bool IsMobilePlatform =>
        OS.HasFeature("mobile") || OS.HasFeature("android") || OS.HasFeature("ios");

    public bool IsDesktopPlatform =>
        OS.HasFeature("pc") || OS.HasFeature("windows") || OS.HasFeature("linuxbsd") || OS.HasFeature("macos");

    public bool IsWebPlatform => OS.HasFeature("web");

    public string CurrentRenderingMethod => RenderingServer.GetCurrentRenderingMethod();

    public bool IsForwardPlus => string.Equals(CurrentRenderingMethod, "forward_plus", StringComparison.OrdinalIgnoreCase);
    public bool IsMobileRenderer => string.Equals(CurrentRenderingMethod, "mobile", StringComparison.OrdinalIgnoreCase);
    public bool IsCompatibilityRenderer => string.Equals(CurrentRenderingMethod, "gl_compatibility", StringComparison.OrdinalIgnoreCase);

    public GraphicsQualityProfile QualityProfile => GraphicsQualityProfile.Resolve(_current?.GraphicsQuality);

    public float DecorationDensity =>
        QualityProfile.DecorationDensity * Mathf.Clamp(_current?.WorldDetailScale ?? 1.0f, 0.35f, 1.25f);

    public float ParticleDensity => QualityProfile.ParticleDensity;

    public bool EffectiveAdvancedLightingEnabled =>
        _current?.AdvancedLightingEnabled == true && IsForwardPlus && QualityProfile.AdvancedLighting;

    public bool EffectiveWorldParticlesEnabled =>
        _current?.WorldParticlesEnabled == true && _current.WeatherEffectsEnabled;

    public bool EffectiveShadowsEnabled =>
        _current?.ShadowsEnabled == true && QualityProfile.Shadows;

    public bool ShouldShowTouchControls =>
        _current?.TouchControlsEnabled == true || IsMobilePlatform;

    public override void _Ready()
    {
        EnsureAudioBuses();
    }

    public override void _Notification(int what)
    {
        if (_current is null)
        {
            return;
        }

        if (what == NotificationApplicationFocusOut && _current.MuteWhenUnfocused)
        {
            _focusMuted = true;
            ApplyMasterMute();
        }
        else if (what == NotificationApplicationFocusIn)
        {
            _focusMuted = false;
            ApplyMasterMute();
        }
    }

    public void Apply(SettingsState settings)
    {
        settings.WorldDetailScale = Mathf.Clamp(settings.WorldDetailScale, 0.35f, 1.25f);
        _current = settings;
        EnsureAudioBuses();
        ApplyAudio(settings);
        ApplyDisplay(settings);
    }

    public static void ApplyQualityPreset(SettingsState settings, string quality)
    {
        var profile = GraphicsQualityProfile.Resolve(quality);
        settings.GraphicsQuality = profile.Name;
        settings.RenderScale = profile.RenderScale;
        settings.ShadowsEnabled = profile.Shadows;
        settings.AtmosphereEffectsEnabled = profile.Atmosphere;
        settings.WeatherEffectsEnabled = true;
        settings.AdvancedLightingEnabled = profile.AdvancedLighting;
        settings.WorldParticlesEnabled = true;
        settings.WorldDetailScale = profile.WorldDetailScale;
        settings.FrameRateLimit = profile.DefaultFrameRateLimit;
    }

    private void ApplyAudio(SettingsState settings)
    {
        SetBusLinearVolume(MasterBus, settings.MasterVolume);
        SetBusLinearVolume(MusicBus, settings.MusicVolume);
        SetBusLinearVolume(SfxBus, settings.SfxVolume);
        SetBusLinearVolume(UiBus, settings.UiVolume);
        ApplyMasterMute();
    }

    private void ApplyDisplay(SettingsState settings)
    {
        Engine.MaxFps = Math.Max(0, settings.FrameRateLimit);

        DisplayServer.WindowSetVsyncMode(settings.VSyncEnabled
            ? DisplayServer.VSyncMode.Enabled
            : DisplayServer.VSyncMode.Disabled);

        var root = GetTree()?.Root;
        if (root is not null)
        {
            root.ContentScaleFactor = Mathf.Clamp(settings.UiScale, 0.80f, 1.50f);
            root.Scaling3DScale = Mathf.Clamp(settings.RenderScale, 0.50f, 1.00f);
        }

        // Fullscreen is a desktop preference. Mobile/web own their surface dimensions.
        if (IsDesktopPlatform && !IsWebPlatform)
        {
            var window = GetWindow();
            if (window is not null)
            {
                window.Mode = settings.Fullscreen
                    ? Window.ModeEnum.ExclusiveFullscreen
                    : Window.ModeEnum.Windowed;

                if (!settings.Fullscreen)
                {
                    var desired = new Vector2(
                        Mathf.Clamp(settings.WindowWidth, 960, 7680),
                        Mathf.Clamp(settings.WindowHeight, 540, 4320));
                    var usable = DisplayServer.ScreenGetUsableRect();
                    if (usable.Size.X > 0 && usable.Size.Y > 0)
                    {
                        var max = new Vector2(usable.Size.X * 0.94f, usable.Size.Y * 0.92f);
                        var fit = Mathf.Min(1f, Mathf.Min(max.X / desired.X, max.Y / desired.Y));
                        desired *= fit;
                    }

                    window.Size = new Vector2I(
                        Mathf.Max(640, Mathf.RoundToInt(desired.X)),
                        Mathf.Max(360, Mathf.RoundToInt(desired.Y)));

                    if (usable.Size.X > window.Size.X && usable.Size.Y > window.Size.Y)
                    {
                        window.Position = usable.Position + (usable.Size - window.Size) / 2;
                    }
                }
            }
        }
    }

    private void ApplyMasterMute()
    {
        if (_current is null)
        {
            return;
        }

        var master = AudioServer.GetBusIndex(MasterBus);
        if (master >= 0)
        {
            AudioServer.SetBusMute(master, !_current.AudioEnabled || _focusMuted);
        }
    }

    private static void EnsureAudioBuses()
    {
        EnsureBus(MusicBus);
        EnsureBus(SfxBus);
        EnsureBus(UiBus);
    }

    private static void EnsureBus(string name)
    {
        if (AudioServer.GetBusIndex(name) >= 0)
        {
            return;
        }

        AudioServer.AddBus();
        var index = AudioServer.BusCount - 1;
        AudioServer.SetBusName(index, name);
        AudioServer.SetBusSend(index, MasterBus);
    }

    private static void SetBusLinearVolume(string busName, float linear)
    {
        var index = AudioServer.GetBusIndex(busName);
        if (index < 0)
        {
            return;
        }

        var clamped = Mathf.Clamp(linear, 0f, 1f);
        var db = clamped <= 0.0001f ? -80f : 20f * MathF.Log10(clamped);
        AudioServer.SetBusVolumeDb(index, db);
    }
}
