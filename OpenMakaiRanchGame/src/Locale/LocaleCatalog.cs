using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace OpenMakaiRanch.Locale;

public static class LocaleCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new() { MaxDepth = 8 };
    private static Dictionary<string, string> _overrides = new();
    private static string _currentLocale = "en";

    public static string CurrentLocale => _currentLocale;

    public static string[] AvailableLocales { get; } = { "en", "ja" };

    public static string LocaleDisplayName(string locale) => locale switch
    {
        "en" => "English",
        "ja" => "日本語",
        _ => locale
    };

    public static void SetLocale(string locale)
    {
        var normalizedLocale = NormalizeLocale(locale);
        if (normalizedLocale == _currentLocale) return;
        LoadLocale(normalizedLocale);
    }

    public static string NormalizeLocale(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return "en";
        }

        return Array.IndexOf(AvailableLocales, locale) >= 0 ? locale : "en";
    }

    public static void LoadLocale(string locale)
    {
        locale = NormalizeLocale(locale);
        _currentLocale = locale;
        _overrides.Clear();
        if (locale == "en") return;

        var path = $"res://locale/{locale}.json";
        if (!Godot.FileAccess.FileExists(path))
        {
            return;
        }

        try
        {
            using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
            if (file is null)
            {
                return;
            }

            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(file.GetAsText(), JsonOptions);
            _overrides = data ?? new Dictionary<string, string>();
        }
        catch (Exception exception)
        {
            GD.PushWarning($"Locale '{locale}' could not be loaded: {exception.Message}");
        }
    }

    public static string T(string key, string defaultText)
    {
        if (_currentLocale == "en")
            return defaultText;

        if (_overrides.TryGetValue(key, out var translated))
            return translated;

        return defaultText;
    }

    public static string T(string key, string defaultText, params object[] args)
    {
        var text = T(key, defaultText);
        return args.Length > 0 ? string.Format(text, args) : text;
    }

    public static string EnumDisplayName<TEnum>(TEnum value) where TEnum : Enum
    {
        var key = $"enum.{typeof(TEnum).Name}.{value}";
        var def = value.ToString();
        return T(key, def);
    }
}
