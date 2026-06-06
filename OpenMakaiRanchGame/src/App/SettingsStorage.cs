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
        settings.UiScale = Mathf.Clamp(settings.UiScale <= 0 ? 1.0f : settings.UiScale, 0.85f, 1.35f);
        if (settings.ContentMode == ContentMode.MatureSkeleton && !settings.MatureContentAgeConfirmed)
        {
            settings.ContentMode = ContentMode.Sfw;
        }

        return settings;
    }
}