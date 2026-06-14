using System;
using System.Collections.Generic;
using Godot;

namespace OpenMakaiRanch.Ui;

public sealed record UiThemePalette(
    string Id,
    string DisplayName,
    Color Canvas,
    Color RootPanelFill,
    Color RootPanelBorder,
    Color NavPanelFill,
    Color NavPanelBorder,
    Color ContentPanelFill,
    Color ContentPanelBorder,
    Color HeaderText,
    Color BodyText,
    Color MutedText,
    Color SectionText,
    Color ChipFill,
    Color ChipBorder,
    Color ChipText,
    Color PrimaryFill,
    Color PrimaryBorder,
    Color PrimaryHover,
    Color PrimaryPressed,
    Color SecondaryFill,
    Color SecondaryBorder,
    Color SecondaryHover,
    Color SecondaryPressed,
    Color CardFill,
    Color CardBorder,
    Color StatBarBackground,
    Color StatBarBorder);

public static class UiThemeCatalog
{
    private static readonly Dictionary<string, UiThemePalette> Palettes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["midnight"] = new UiThemePalette(
            "midnight",
            "Midnight",
            new Color("121216"),
            new Color("1c1c24"),
            new Color("2e2e3e"),
            new Color("16161c"),
            new Color("2a2a3a"),
            new Color("1a1a22"),
            new Color("34344a"),
            new Color("eeeef0"),
            new Color("c8c8d0"),
            new Color("8888a0"),
            new Color("a8a8c0"),
            new Color("1e1e2e"),
            new Color("3e3e56"),
            new Color("e0e0e8"),
            new Color("4a6fa5"),
            new Color("7faee6"),
            new Color("5a85bf"),
            new Color("3a5590"),
            new Color("242438"),
            new Color("3e3e56"),
            new Color("2e2e48"),
            new Color("1a1a2e"),
            new Color("1e1e2a"),
            new Color("3a3a50"),
            new Color("14141c"),
            new Color("2a2a3e")),
        ["sunrise"] = new UiThemePalette(
            "sunrise",
            "Sunrise",
            new Color("1a1412"),
            new Color("28201c"),
            new Color("504038"),
            new Color("201815"),
            new Color("4a3c34"),
            new Color("221a16"),
            new Color("584840"),
            new Color("f0e6dc"),
            new Color("d8c8bc"),
            new Color("a89080"),
            new Color("c8b0a0"),
            new Color("382821"),
            new Color("6a5448"),
            new Color("f0e0d4"),
            new Color("8a5c44"),
            new Color("d4a088"),
            new Color("a87058"),
            new Color("704c3c"),
            new Color("3c2c24"),
            new Color("6a584c"),
            new Color("4c382e"),
            new Color("34241c"),
            new Color("2c2018"),
            new Color("5c4c40"),
            new Color("28201a"),
            new Color("5c4c44")),
        ["paperlight"] = new UiThemePalette(
            "paperlight",
            "Paperlight",
            new Color("eef0f4"),
            new Color("ffffff"),
            new Color("d0d4dc"),
            new Color("f4f6f8"),
            new Color("ccd0d8"),
            new Color("f8fafc"),
            new Color("d4d8e0"),
            new Color("202834"),
            new Color("303848"),
            new Color("687088"),
            new Color("485068"),
            new Color("e4e8f0"),
            new Color("a8b0c0"),
            new Color("283040"),
            new Color("3870a8"),
            new Color("5898d0"),
            new Color("4888c0"),
            new Color("285888"),
            new Color("e0e8f0"),
            new Color("a0b0c0"),
            new Color("d0dce8"),
            new Color("c4d0e0"),
            new Color("ffffff"),
            new Color("bcc8d4"),
            new Color("e4e8f0"),
            new Color("b0bcc8"))
    };

    public static IReadOnlyCollection<UiThemePalette> All => Palettes.Values;

    public static UiThemePalette Resolve(string? id)
    {
        if (!string.IsNullOrWhiteSpace(id) && Palettes.TryGetValue(id, out var palette))
        {
            return palette;
        }

        return Palettes["midnight"];
    }
}
