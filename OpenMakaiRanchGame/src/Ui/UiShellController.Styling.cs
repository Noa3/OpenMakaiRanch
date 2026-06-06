using System;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.Ui;

/// <summary>
/// Styling and reusable visual composition helpers for UiShellController.
/// </summary>
public partial class UiShellController
{
    private UiThemePalette Palette => _game.Theme;

    private void ApplyHeaderLabelStyle(Label label)
    {
        label.CustomMinimumSize = new Vector2(220, 0);
        label.ThemeTypeVariation = "HeaderSmall";
        label.AddThemeColorOverride("font_color", Palette.HeaderText);
    }

    private void ApplyMutedLabelStyle(Label label)
    {
        label.AddThemeColorOverride("font_color", Palette.MutedText);
    }

    private void ApplyChipLabelStyle(Label label)
    {
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.AddThemeColorOverride("font_color", Palette.ChipText);
    }

    private void ApplyChipPanelStyle(PanelContainer? panel)
    {
        if (panel is null)
        {
            return;
        }

        panel.CustomMinimumSize = new Vector2(104, 32);
        panel.AddThemeStyleboxOverride("panel", CardStyle(Palette.ChipFill, Palette.ChipBorder, 1, 8));
    }

    private void ApplySectionLabelStyle(Label label)
    {
        label.CustomMinimumSize = new Vector2(0, 28);
        label.AddThemeColorOverride("font_color", Palette.SectionText);
    }

    private void ApplyPrimaryButtonStyle(Button button)
    {
        button.CustomMinimumSize = button.CustomMinimumSize == Vector2.Zero ? new Vector2(0, 36) : button.CustomMinimumSize;
        button.AddThemeColorOverride("font_color", Palette.HeaderText);
        button.AddThemeStyleboxOverride("normal", CardStyle(Palette.PrimaryFill, Palette.PrimaryBorder, 1, 8));
        button.AddThemeStyleboxOverride("pressed", CardStyle(Palette.PrimaryPressed, Palette.PrimaryBorder, 1, 8));
        button.AddThemeStyleboxOverride("hover", CardStyle(Palette.PrimaryHover, Palette.PrimaryBorder, 1, 8));
        button.AddThemeStyleboxOverride("disabled", CardStyle(Palette.SecondaryFill, Palette.SecondaryBorder, 1, 8));
    }

    private void ApplySecondaryButtonStyle(Button button)
    {
        button.CustomMinimumSize = button.CustomMinimumSize == Vector2.Zero ? new Vector2(0, 34) : button.CustomMinimumSize;
        button.AddThemeColorOverride("font_color", Palette.BodyText);
        button.AddThemeStyleboxOverride("normal", CardStyle(Palette.SecondaryFill, Palette.SecondaryBorder, 1, 8));
        button.AddThemeStyleboxOverride("pressed", CardStyle(Palette.SecondaryPressed, Palette.SecondaryBorder, 1, 8));
        button.AddThemeStyleboxOverride("hover", CardStyle(Palette.SecondaryHover, Palette.SecondaryBorder, 1, 8));
        button.AddThemeStyleboxOverride("disabled", CardStyle(Palette.SecondaryFill.Darkened(0.2f), Palette.SecondaryBorder.Darkened(0.2f), 1, 8));
    }

    private void AddTitle(string text)
    {
        _content.AddChild(TitleLabel(text));
    }

    private Label HeaderLabel(string text)
    {
        var label = new Label
        {
            Text = text,
            CustomMinimumSize = new Vector2(220, 0),
            ThemeTypeVariation = "HeaderSmall"
        };
        ApplyHeaderLabelStyle(label);
        return label;
    }

    private Label TitleLabel(string text)
    {
        var label = new Label { Text = text, CustomMinimumSize = new Vector2(0, 30) };
        label.AddThemeColorOverride("font_color", Palette.HeaderText);
        return label;
    }

    private Label SubtitleLabel(string text)
    {
        var label = new Label { Text = text, CustomMinimumSize = new Vector2(0, 24) };
        label.AddThemeColorOverride("font_color", Palette.BodyText);
        return label;
    }

    private Label MutedLabel(string text)
    {
        var label = new Label { Text = text };
        ApplyMutedLabelStyle(label);
        return label;
    }

    private Label ChipLabel(string text)
    {
        var label = new Label { Text = text, CustomMinimumSize = new Vector2(104, 32), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        ApplyChipLabelStyle(label);
        return label;
    }

    private Label SectionLabel(string text)
    {
        var label = new Label { Text = text.ToUpperInvariant(), CustomMinimumSize = new Vector2(0, 28) };
        ApplySectionLabelStyle(label);
        return label;
    }

    private Label AddStyledLine(string text, bool fill = false)
    {
        var line = new Label { Text = text };
        line.AddThemeColorOverride("font_color", Palette.BodyText);
        if (fill)
        {
            line.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        }

        return line;
    }

    private Button PrimaryButton(string text)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(0, 36) };
        ApplyPrimaryButtonStyle(button);
        return button;
    }

    private Button SecondaryButton(string text)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(0, 34) };
        ApplySecondaryButtonStyle(button);
        return button;
    }

    private PanelContainer CardContainer()
    {
        var card = new PanelContainer();
        card.AddThemeStyleboxOverride("panel", CardStyle(Palette.CardFill, Palette.CardBorder, 1, 12));
        return card;
    }

    private static StyleBoxFlat CardStyle(Color fill, Color border, int borderWidth, int cornerRadius)
    {
        var style = new StyleBoxFlat
        {
            BgColor = fill,
            BorderColor = border,
            CornerRadiusTopLeft = cornerRadius,
            CornerRadiusTopRight = cornerRadius,
            CornerRadiusBottomRight = cornerRadius,
            CornerRadiusBottomLeft = cornerRadius,
            ContentMarginLeft = 12,
            ContentMarginTop = 10,
            ContentMarginRight = 12,
            ContentMarginBottom = 10
        };

        style.BorderWidthLeft = borderWidth;
        style.BorderWidthTop = borderWidth;
        style.BorderWidthRight = borderWidth;
        style.BorderWidthBottom = borderWidth;
        return style;
    }

    private static Control BuildPortrait(string path)
    {
        var texture = LoadTexture(path);
        if (texture is not null)
        {
            return new TextureRect
            {
                Texture = texture,
                CustomMinimumSize = new Vector2(112, 112),
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize
            };
        }

        return new ColorRect
        {
            Color = new Color("24364f"),
            CustomMinimumSize = new Vector2(112, 112)
        };
    }

    private static Control BuildCharacterVisual(CharacterState character, CharacterDefinition definition)
    {
        var wrap = new VBoxContainer { CustomMinimumSize = new Vector2(112, 112) };
        wrap.AddThemeConstantOverride("separation", 6);

        var layered = BuildLayeredPortrait(character);
        if (layered is not null)
        {
            wrap.AddChild(layered);
        }
        else
        {
            wrap.AddChild(BuildPortrait(definition.PortraitPath));
            wrap.AddChild(BuildPortrait(string.IsNullOrWhiteSpace(definition.BodyImagePath) ? definition.PortraitPath : definition.BodyImagePath));
        }

        return wrap;
    }

    private static Control? BuildLayeredPortrait(CharacterState character)
    {
        if (PortraitLayerCatalog.RaceLayers.Length == 0
            || PortraitLayerCatalog.HairLayers.Length == 0
            || PortraitLayerCatalog.BodyLayers.Length == 0
            || PortraitLayerCatalog.ClothLayers.Length == 0)
        {
            return null;
        }

        var bg = LoadTexture(PortraitLayerCatalog.BackgroundLayer);
        var race = LoadTexture(PortraitLayerCatalog.RaceLayers[PortraitLayerCatalog.ClampIndex(character.RaceLayerIndex, PortraitLayerCatalog.RaceLayers.Length)]);
        var body = LoadTexture(PortraitLayerCatalog.BodyLayers[PortraitLayerCatalog.ClampIndex(character.BodyLayerIndex, PortraitLayerCatalog.BodyLayers.Length)]);
        var face = LoadTexture(PortraitLayerCatalog.FaceLayer);
        var hair = LoadTexture(PortraitLayerCatalog.HairLayers[PortraitLayerCatalog.ClampIndex(character.HairLayerIndex, PortraitLayerCatalog.HairLayers.Length)]);
        var cloth = LoadTexture(PortraitLayerCatalog.ClothLayers[PortraitLayerCatalog.ClampIndex(character.ClothLayerIndex, PortraitLayerCatalog.ClothLayers.Length)]);
        if (bg is null || face is null)
        {
            return null;
        }

        if (race is null || body is null || hair is null || cloth is null)
        {
            return null;
        }

        var stack = new Control { CustomMinimumSize = new Vector2(112, 112) };
        stack.AddChild(LayerRect(bg));
        stack.AddChild(LayerRect(race));
        stack.AddChild(LayerRect(body));
        stack.AddChild(LayerRect(face));
        stack.AddChild(LayerRect(hair));
        stack.AddChild(LayerRect(cloth));
        return stack;
    }

    private static TextureRect LayerRect(Texture2D? texture)
    {
        return new TextureRect
        {
            Texture = texture,
            AnchorRight = 1,
            AnchorBottom = 1,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize
        };
    }

    private Control StatBar(string label, int value, int max, Color fillColor)
    {
        var wrap = new VBoxContainer();
        wrap.AddThemeConstantOverride("separation", 2);

        var safeMax = Math.Max(max, 1);
        var header = new Label { Text = $"{label}: {value}/{safeMax}" };
        header.AddThemeColorOverride("font_color", Palette.BodyText);
        wrap.AddChild(header);

        var bar = new ProgressBar
        {
            MinValue = 0,
            MaxValue = safeMax,
            Value = Math.Clamp(value, 0, safeMax),
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(0, 14)
        };
        bar.AddThemeStyleboxOverride("background", CardStyle(Palette.StatBarBackground, Palette.StatBarBorder, 1, 6));
        bar.AddThemeStyleboxOverride("fill", CardStyle(fillColor, fillColor, 0, 6));
        wrap.AddChild(bar);

        return wrap;
    }

    private Control StatChip(string text)
    {
        var panel = new PanelContainer();
        panel.AddThemeStyleboxOverride("panel", CardStyle(Palette.ChipFill, Palette.ChipBorder, 1, 8));
        panel.AddChild(MutedLabel(text));
        return panel;
    }

    private static string ScreenTitle(string screenId)
    {
        return screenId switch
        {
            "title" => "Main Menu",
            "ranch" => "Ranch Overview",
            "roster" => "Characters",
            "schedule" => "Daily Schedule",
            "town" => "Town Hub",
            "shop" => "General Store",
            "adventure" => "Adventure Guild",
            "combat" => "Combat And Mission Result",
            "milestones" => "Milestones",
            "research" => "Research",
            "bond" => "Bond And Mentorship",
            "pets" => "Pets",
            "saveload" => "Save And Load",
            "settings" => "Settings",
            _ => "Ranch Overview"
        };
    }

    private static Texture2D? LoadTexture(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !ResourceLoader.Exists(path))
        {
            return null;
        }

        return GD.Load<Texture2D>(path);
    }
}
