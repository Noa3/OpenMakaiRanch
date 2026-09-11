using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Godot;

namespace OpenMakaiRanch.Locale;

/// <summary>One presentation-only catalog for both retained menus and world interfaces.</summary>
public static class LocaleCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new() { MaxDepth = 8 };
    private static Dictionary<string, string> _english = new(StringComparer.Ordinal);
    private static Dictionary<string, string> _overrides = new(StringComparer.Ordinal);
    private static string _currentLocale = "en";
    private static CultureInfo _culture = CultureInfo.GetCultureInfo("en");

    public static string CurrentLocale => _currentLocale;
    public static string[] AvailableLocales { get; } = { "en", "de", "ja" };

    // Native language names stay recognizable even when the player cannot read the active locale.
    public static string LocaleDisplayName(string locale) => locale switch
    {
        "en" => "English",
        "de" => "Deutsch",
        "ja" => "日本語",
        _ => locale
    };

    public static void SetLocale(string locale)
    {
        var normalized = NormalizeLocale(locale);
        if (normalized != _currentLocale) LoadLocale(normalized);
    }

    public static string NormalizeLocale(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale)) return "en";
        var language = locale.Trim().Replace('-', '_').ToLowerInvariant().Split('_')[0];
        return Array.IndexOf(AvailableLocales, language) >= 0 ? language : "en";
    }

    public static void LoadLocale(string locale)
    {
        var normalized = NormalizeLocale(locale);
        // Build replacements first. A missing/invalid optional file leaves a usable English UI.
        var english = ReadCatalog("res://locale/en.json");
        Merge(english, ReadCatalog("res://locale/ui/en.json"));
        var translated = new Dictionary<string, string>(StringComparer.Ordinal);
        if (normalized != "en")
        {
            Merge(translated, ReadCatalog($"res://locale/{normalized}.json"));
            Merge(translated, ReadCatalog($"res://locale/ui/{normalized}.json"));
        }
        _english = english;
        _overrides = translated;
        _culture = CultureInfo.GetCultureInfo(normalized);
        _currentLocale = normalized;
    }

    private static Dictionary<string, string> ReadCatalog(string path)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!Godot.FileAccess.FileExists(path)) return result;
        try
        {
            using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
            if (file is null) return result;
            if (file.GetLength() > 512 * 1024)
            {
                GD.PushWarning($"Locale catalog is too large: {path}");
                return result;
            }
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(file.GetAsText(), JsonOptions);
            if (data is not null) Merge(result, data);
        }
        catch (Exception exception)
        {
            GD.PushWarning($"Locale catalog '{path}' could not be loaded: {exception.Message}");
        }
        return result;
    }

    private static void Merge(Dictionary<string, string> target, Dictionary<string, string> source)
    {
        foreach (var entry in source)
            if (!string.IsNullOrWhiteSpace(entry.Key) && !string.IsNullOrWhiteSpace(entry.Value))
                target[entry.Key] = entry.Value;
    }

    public static string T(string key, string defaultText)
    {
        if (_overrides.TryGetValue(key, out var translated)) return translated;
        return _english.TryGetValue(key, out var english) ? english : defaultText;
    }

    public static string T(string key, string defaultText, params object[] args)
    {
        var english = _english.TryGetValue(key, out var baseline) ? baseline : defaultText;
        return FormatForDisplay(T(key, defaultText), english, _culture, args);
    }

    // Never change CurrentCulture: parsing saves, IDs, controls and simulation is not localization.
    // A translator's malformed format string must not break an action or swallow its callback.
    internal static string FormatForDisplay(string translated, string english, IFormatProvider culture, params object[] args)
    {
        if (args.Length == 0) return translated;
        try { return string.Format(culture, translated, args); }
        catch (FormatException)
        {
            try { return string.Format(CultureInfo.GetCultureInfo("en"), english, args); }
            catch (FormatException) { return english; }
        }
    }

    public static string WorldName(string id, string fallback) => T("world.name." + id, fallback);
    public static string JobName(string id, string fallback) => T("world.job." + id, fallback);
    public static string ResourceName(string id) => T("world.resource." + id, id);
    public static string EnumDisplayName<TEnum>(TEnum value) where TEnum : Enum =>
        T($"enum.{typeof(TEnum).Name}.{value}", value.ToString());
}
