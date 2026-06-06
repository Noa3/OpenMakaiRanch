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
            new Color("0b1220"),
            new Color("182132"),
            new Color("2f425e"),
            new Color("101828"),
            new Color("2b3f5f"),
            new Color("0e1726"),
            new Color("2a3d58"),
            new Color("e5f2ff"),
            new Color("c2d7f2"),
            new Color("9db1cd"),
            new Color("7fa2cb"),
            new Color("12243a"),
            new Color("355071"),
            new Color("d9ebff"),
            new Color("1d4c73"),
            new Color("5fa4d9"),
            new Color("255b89"),
            new Color("163d5d"),
            new Color("18293f"),
            new Color("3d5574"),
            new Color("1f334e"),
            new Color("142337"),
            new Color("142237"),
            new Color("3a5678"),
            new Color("243345"),
            new Color("465d77")),
        ["sunrise"] = new UiThemePalette(
            "sunrise",
            "Sunrise",
            new Color("1e1411"),
            new Color("2f211c"),
            new Color("765447"),
            new Color("261915"),
            new Color("6d4d43"),
            new Color("2a1c18"),
            new Color("7d5c50"),
            new Color("ffe7d5"),
            new Color("f7d6c1"),
            new Color("d8b39a"),
            new Color("e8b896"),
            new Color("3c2721"),
            new Color("8f6352"),
            new Color("ffeede"),
            new Color("a95c3a"),
            new Color("f4ae84"),
            new Color("c06a45"),
            new Color("8b4f35"),
            new Color("4c2f27"),
            new Color("8b6759"),
            new Color("5a3a31"),
            new Color("432921"),
            new Color("33231c"),
            new Color("7d5f52"),
            new Color("3a2a22"),
            new Color("856556")),
        ["paperlight"] = new UiThemePalette(
            "paperlight",
            "Paperlight",
            new Color("edf2f7"),
            new Color("ffffff"),
            new Color("c5d0dc"),
            new Color("f6f9fc"),
            new Color("c7d3df"),
            new Color("fefefe"),
            new Color("c9d6e3"),
            new Color("233548"),
            new Color("2d4258"),
            new Color("576f86"),
            new Color("3b5d7d"),
            new Color("dce8f4"),
            new Color("9cb3ca"),
            new Color("233548"),
            new Color("2a7bb8"),
            new Color("4ca0db"),
            new Color("3990cd"),
            new Color("1f678f"),
            new Color("e4edf6"),
            new Color("9fb4c8"),
            new Color("d7e4f0"),
            new Color("c8d9ea"),
            new Color("ffffff"),
            new Color("c4d2df"),
            new Color("e8eef4"),
            new Color("b1c0ce"))
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
