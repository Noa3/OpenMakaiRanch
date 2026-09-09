using System;
using System.IO;
using System.Text.Json;
using Godot;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.App;

public sealed class SettingsStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public SettingsState Load()
    {
        var absolutePath = ProjectSettings.GlobalizePath("user://settings.json");
        if (!File.Exists(absolutePath))
        {
            return new SettingsState();
        }

        try
        {
            var json = File.ReadAllText(absolutePath);
            return EnsureDefaults(JsonSerializer.Deserialize<SettingsState>(json, JsonOptions) ?? new SettingsState());
        }
        catch (Exception exception)
        {
            GD.PushWarning($"Could not load local settings: {exception.Message}");
            return new SettingsState();
        }
    }

    public void Save(SettingsState settings)
    {
        var absolutePath = ProjectSettings.GlobalizePath("user://settings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath) ?? ProjectSettings.GlobalizePath("user://"));
        var temporaryPath = absolutePath + ".tmp";
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings, JsonOptions));
            File.Move(temporaryPath, absolutePath, true);
        }
        catch (Exception exception)
        {
            GD.PushError($"Could not save local settings: {exception.Message}");
            try
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
            catch (Exception cleanupException)
            {
                GD.PushWarning($"Could not remove temporary settings file: {cleanupException.Message}");
            }
        }
    }

    private static SettingsState EnsureDefaults(SettingsState settings)
    {
        settings.ThemeId = string.IsNullOrWhiteSpace(settings.ThemeId) ? "midnight" : settings.ThemeId;
        settings.Locale = string.IsNullOrWhiteSpace(settings.Locale) ? "en" : settings.Locale;
        settings.UiScale = Mathf.Clamp(settings.UiScale <= 0 ? 1.0f : settings.UiScale, 0.80f, 1.50f);

        settings.MasterVolume = Mathf.Clamp(settings.MasterVolume, 0f, 1f);
        settings.MusicVolume = Mathf.Clamp(settings.MusicVolume, 0f, 1f);
        settings.SfxVolume = Mathf.Clamp(settings.SfxVolume, 0f, 1f);
        settings.UiVolume = Mathf.Clamp(settings.UiVolume, 0f, 1f);

        settings.GraphicsQuality = NormalizeQuality(settings.GraphicsQuality);
        settings.RenderScale = Mathf.Clamp(settings.RenderScale <= 0f ? 0.85f : settings.RenderScale, 0.50f, 1.00f);
        settings.FrameRateLimit = NormalizeFrameLimit(settings.FrameRateLimit);
        settings.WindowWidth = Mathf.Clamp(settings.WindowWidth <= 0 ? 1920 : settings.WindowWidth, 960, 7680);
        settings.WindowHeight = Mathf.Clamp(settings.WindowHeight <= 0 ? 1080 : settings.WindowHeight, 540, 4320);

        settings.CameraSensitivity = Mathf.Clamp(settings.CameraSensitivity <= 0f ? 1.0f : settings.CameraSensitivity, 0.35f, 2.50f);
        settings.CameraFov = Mathf.Clamp(settings.CameraFov <= 0f ? 70f : settings.CameraFov, 55f, 95f);
        settings.TouchControlScale = Mathf.Clamp(settings.TouchControlScale <= 0f ? 1.0f : settings.TouchControlScale, 0.75f, 1.50f);

        settings.SeenTutorialIds ??= new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);

        return settings;
    }

    private static string NormalizeQuality(string? quality)
    {
        return (quality ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "low" => "Low",
            "high" => "High",
            "ultra" => "Ultra",
            "custom" => "Custom",
            _ => "Medium"
        };
    }

    private static int NormalizeFrameLimit(int value)
    {
        if (value <= 0)
        {
            return 0;
        }

        return value switch
        {
            <= 30 => 30,
            <= 45 => 45,
            <= 60 => 60,
            <= 90 => 90,
            <= 120 => 120,
            _ => 144
        };
    }
}