using System;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Gameplay;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Ui;

/// <summary>
/// Styling and reusable visual composition helpers for UiShellController.
/// </summary>
public partial class UiShellController
{
    private static readonly Vector2 PortraitDisplaySize = new(112, 112);
    private const float PortraitBodyOriginX = 32f;
    private UiThemePalette Palette => _game.Theme;

    private void ApplyHeaderLabelStyle(Label label)
    {
        label.CustomMinimumSize = new Vector2(220, 0);
        label.ThemeTypeVariation = "HeaderSmall";
        label.AddThemeColorOverride("font_color", Palette.HeaderText);
        ConfigureReadableLabel(label);
    }

    private void ApplyMutedLabelStyle(Label label)
    {
        label.AddThemeColorOverride("font_color", Palette.MutedText);
        ConfigureReadableLabel(label);
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

        panel.CustomMinimumSize = new Vector2(70, 24);
        panel.AddThemeStyleboxOverride("panel", CardStyle(Palette.ChipFill, Palette.ChipBorder, 1, 4));
    }

    private void ApplySectionLabelStyle(Label label)
    {
        label.CustomMinimumSize = new Vector2(0, 28);
        label.AddThemeColorOverride("font_color", Palette.SectionText);
        ConfigureReadableLabel(label);
    }

    private void ApplyPrimaryButtonStyle(Button button)
    {
        button.CustomMinimumSize = button.CustomMinimumSize == Vector2.Zero ? new Vector2(120, 36) : button.CustomMinimumSize;
        ConfigureReadableButton(button);
        foreach (var state in new[] { "font_color", "font_hover_color", "font_pressed_color", "font_disabled_color", "font_focus_color" })
            button.AddThemeColorOverride(state, Palette.HeaderText);
        button.AddThemeFontSizeOverride("font_size", 15);
        button.AddThemeStyleboxOverride("normal", CardStyle(Palette.PrimaryFill, Palette.PrimaryBorder, 1, 4));
        button.AddThemeStyleboxOverride("pressed", CardStyle(Palette.PrimaryPressed, Palette.PrimaryBorder, 1, 4));
        button.AddThemeStyleboxOverride("hover", CardStyle(Palette.PrimaryHover, Palette.PrimaryBorder, 1, 4));
        button.AddThemeStyleboxOverride("disabled", CardStyle(Palette.SecondaryFill, Palette.SecondaryBorder, 1, 4));
        button.SizeFlagsHorizontal = SizeFlags.ExpandFill;
    }

    private void ApplySecondaryButtonStyle(Button button)
    {
        button.CustomMinimumSize = button.CustomMinimumSize == Vector2.Zero ? new Vector2(120, 34) : button.CustomMinimumSize;
        ConfigureReadableButton(button);
        foreach (var state in new[] { "font_color", "font_hover_color", "font_pressed_color", "font_disabled_color", "font_focus_color" })
            button.AddThemeColorOverride(state, Palette.BodyText);
        button.AddThemeFontSizeOverride("font_size", 15);
        button.AddThemeStyleboxOverride("normal", CardStyle(Palette.SecondaryFill, Palette.SecondaryBorder, 1, 4));
        button.AddThemeStyleboxOverride("pressed", CardStyle(Palette.SecondaryPressed, Palette.SecondaryBorder, 1, 4));
        button.AddThemeStyleboxOverride("hover", CardStyle(Palette.SecondaryHover, Palette.SecondaryBorder, 1, 4));
        button.AddThemeStyleboxOverride("disabled", CardStyle(Palette.SecondaryFill.Darkened(0.2f), Palette.SecondaryBorder.Darkened(0.2f), 1, 4));
        button.SizeFlagsHorizontal = SizeFlags.ExpandFill;
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
        ConfigureReadableLabel(label);
        return label;
    }

    private Label SubtitleLabel(string text)
    {
        var label = new Label { Text = text, CustomMinimumSize = new Vector2(0, 24) };
        label.AddThemeColorOverride("font_color", Palette.BodyText);
        ConfigureReadableLabel(label);
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
        label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
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
        ConfigureReadableLabel(line);
        if (fill)
        {
            line.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        }

        return line;
    }

    private Label BodyLabel(string text)
    {
        var label = new Label { Text = text, VerticalAlignment = VerticalAlignment.Center };
        label.AddThemeColorOverride("font_color", Palette.BodyText);
        ConfigureReadableLabel(label);
        return label;
    }

    private Button PrimaryButton(string text)
    {
        return PrimaryButton(text, "");
    }

    private Button PrimaryButton(string text, string tooltip)
    {
        var button = new Button { Text = text, TooltipText = tooltip, CustomMinimumSize = new Vector2(0, 36) };
        ApplyPrimaryButtonStyle(button);
        return button;
    }

    private Button SecondaryButton(string text)
    {
        return SecondaryButton(text, "");
    }

    private Button SecondaryButton(string text, string tooltip)
    {
        var button = new Button { Text = text, TooltipText = tooltip, CustomMinimumSize = new Vector2(0, 34) };
        ApplySecondaryButtonStyle(button);
        return button;
    }

    private Button SmallButton(string text)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(92, 26) };
        ConfigureReadableButton(button);
        button.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        return button;
    }

    private OptionButton StyledPicker(float minWidth = 200)
    {
        var picker = new OptionButton
        {
            CustomMinimumSize = new Vector2(minWidth, 34),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
            FitToLongestItem = false,
            TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
        };
        return picker;
    }

    private HFlowContainer FlowRow(int separation = 8)
    {
        var row = new HFlowContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        row.AddThemeConstantOverride("h_separation", separation);
        row.AddThemeConstantOverride("v_separation", 6);
        return row;
    }

    private void AddFlowButton(HFlowContainer row, Button button, float minWidth = 132)
    {
        button.CustomMinimumSize = new Vector2(Math.Max(button.CustomMinimumSize.X, minWidth), Math.Max(button.CustomMinimumSize.Y, 34));
        button.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        row.AddChild(button);
    }

    private Button DestinationButton(string text, string screenId, bool primary = false, string tooltip = "")
    {
        var canEnter = CanEnterScreen(screenId, out var requirement);
        var label = canEnter ? text : $"{text} {LockedSuffix()}";
        var button = primary ? PrimaryButton(label, canEnter ? tooltip : requirement) : SecondaryButton(label, canEnter ? tooltip : requirement);
        button.Disabled = !canEnter;
        button.Pressed += () =>
        {
            _game.Feedback.PlayNavigate();
            ShowScreen(screenId);
        };
        return button;
    }

    private Label RequirementLabel(string text)
    {
        var label = MutedLabel(text);
        label.AddThemeColorOverride("font_color", new Color("e9c46a"));
        return label;
    }

    private static void ConfigureReadableLabel(Label label)
    {
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
    }

    private static void ConfigureReadableButton(Button button)
    {
        button.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        button.ClipText = true;
    }

    private PanelContainer CardContainer()
    {
        var card = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        card.AddThemeStyleboxOverride("panel", CardStyle(Palette.CardFill, Palette.CardBorder, 1, 8));
        return card;
    }

    private VBoxContainer CardContent()
    {
        var inner = new VBoxContainer();
        inner.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        inner.AddThemeConstantOverride("separation", 6);
        return inner;
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
                CustomMinimumSize = PortraitDisplaySize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize
            };
        }

        return new ColorRect
        {
            Color = new Color("24364f"),
            CustomMinimumSize = PortraitDisplaySize
        };
    }

    private static Control BuildCharacterVisual(CharacterState character, CharacterDefinition definition)
    {
        var wrap = new VBoxContainer { CustomMinimumSize = PortraitDisplaySize };
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
            || PortraitLayerCatalog.BodyBaseLayers.Length == 0
            || PortraitLayerCatalog.BreastLayers.Length == 0
            || PortraitLayerCatalog.ClothLayers.Length == 0)
        {
            return null;
        }

        var bg = LoadTexture(PortraitLayerCatalog.BackgroundLayer);
        if (bg is null)
        {
            return null;
        }

        var bodyShape = PortraitLayerCatalog.ClampIndex(character.BodyLayerIndex, PortraitLayerCatalog.BodyTypeCount);
        var skinColor = PortraitLayerCatalog.ClampIndex(character.SkinColorIndex, PortraitLayerCatalog.SkinColorCount);
        var breastSize = PortraitLayerCatalog.ClampIndex(character.BreastSizeIndex, PortraitLayerCatalog.BreastSizeCount);

        var bodyBase = LayerRect(PortraitLayerCatalog.BodyBaseLayers[PortraitLayerCatalog.BodyBaseIndex(bodyShape, skinColor)]);
        var breast = LayerRect(PortraitLayerCatalog.BreastLayers[PortraitLayerCatalog.BreastIndex(bodyShape, breastSize, skinColor)]);
        var race = LayerRect(PortraitLayerCatalog.RaceLayers[PortraitLayerCatalog.ClampIndex(character.RaceLayerIndex, PortraitLayerCatalog.RaceLayers.Length)]);
        var face = LayerRect(PortraitLayerCatalog.FaceLayer);
        var mouth = LayerRect(PortraitLayerCatalog.MouthLayer);
        var hair = LayerRect(PortraitLayerCatalog.HairLayers[PortraitLayerCatalog.ClampIndex(character.HairLayerIndex, PortraitLayerCatalog.HairLayers.Length)]);
        var cloth = LayerRect(PortraitLayerCatalog.ClothLayers[PortraitLayerCatalog.ClampIndex(character.ClothLayerIndex, PortraitLayerCatalog.ClothLayers.Length)]);
        if (bodyBase is null || breast is null || race is null || face is null || mouth is null || hair is null || cloth is null)
        {
            return null;
        }

        var stack = new Control { CustomMinimumSize = PortraitDisplaySize };
        stack.AddChild(LayerRect(bg, new Vector2(24, 0), new Vector2(64, 112)));
        stack.AddChild(bodyBase);
        stack.AddChild(race);
        stack.AddChild(breast);
        stack.AddChild(face);
        stack.AddChild(mouth);
        stack.AddChild(hair);
        stack.AddChild(cloth);
        return stack;
    }

    private static TextureRect? LayerRect(PortraitLayerFrame frame)
    {
        var texture = LoadTexture(frame.Path);
        if (texture is null)
        {
            return null;
        }

        var atlas = new AtlasTexture
        {
            Atlas = texture,
            Region = new Rect2(frame.X, frame.Y, frame.Width, frame.Height)
        };
        var scaledW = (int)(frame.Width * frame.Scale);
        var scaledH = (int)(frame.Height * frame.Scale);
        return LayerRect(atlas, new Vector2(PortraitBodyOriginX + frame.OffsetX, frame.OffsetY), new Vector2(scaledW, scaledH));
    }

    private static TextureRect LayerRect(Texture2D texture, Vector2 position, Vector2 size)
    {
        return new TextureRect
        {
            Texture = texture,
            Position = position,
            Size = size,
            CustomMinimumSize = size,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            MouseFilter = Control.MouseFilterEnum.Ignore
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
        bar.AddThemeStyleboxOverride("background", CardStyle(Palette.StatBarBackground, Palette.StatBarBorder, 1, 4));
        bar.AddThemeStyleboxOverride("fill", CardStyle(fillColor, fillColor, 0, 4));
        wrap.AddChild(bar);

        return wrap;
    }

    private void BuildCompactStatBar(PanelContainer chip, out Label label, out ProgressBar bar, Color fillColor)
    {
        chip.CustomMinimumSize = new Vector2(80, 34);
        chip.AddThemeStyleboxOverride("panel", CardStyle(Palette.StatBarBackground, Palette.StatBarBorder, 1, 4));

        var wrap = new VBoxContainer();
        wrap.AddThemeConstantOverride("separation", 1);
        wrap.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        wrap.SizeFlagsVertical = SizeFlags.ExpandFill;

        label = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = "..."
        };
        label.AddThemeFontSizeOverride("font_size", 11);
        label.AddThemeColorOverride("font_color", Palette.ChipText);
        wrap.AddChild(label);

        bar = new ProgressBar
        {
            MinValue = 0,
            MaxValue = 100,
            Value = 50,
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(0, 6)
        };
        bar.AddThemeStyleboxOverride("background", CardStyle(Palette.StatBarBackground, Palette.StatBarBorder, 1, 2));
        bar.AddThemeStyleboxOverride("fill", CardStyle(fillColor, fillColor, 0, 2));
        wrap.AddChild(bar);

        chip.AddChild(wrap);
    }

    private Control StatChip(string text)
    {
        var panel = new PanelContainer();
        panel.AddThemeStyleboxOverride("panel", CardStyle(Palette.ChipFill, Palette.ChipBorder, 1, 4));
        panel.AddChild(MutedLabel(text));
        return panel;
    }

    private static string ScreenTitle(string screenId)
    {
        return screenId switch
        {
            "title" => T("screen.title", "Main Menu"),
            "ranch" => T("screen.ranch", "Ranch Overview"),
            "roster" => T("screen.roster", "Characters"),
            "schedule" => T("screen.schedule", "Daily Schedule"),
            "town" => T("screen.town", "Town Hub"),
            "shop" => T("screen.shop", "General Store"),
            "adventure" => T("screen.adventure", "Adventure Guild"),
            "combat" => T("screen.combat", "Combat And Mission Result"),
            "milestones" => T("screen.milestones", "Milestones"),
            "research" => T("screen.research", "Research"),
            "bond" => T("screen.bond", "Bond And Mentorship"),
            "pets" => T("screen.pets", "Pets"),
            "training" => T("screen.training", "Training Room"),
            "milk" => T("screen.milk", "Milk Processing"),
            "mental" => T("screen.mental", "Mental State Overview"),
            "character_creation" => T("screen.character_creation", "Character Creation"),
            "prologue" => T("screen.prologue", "Opening"),
            "victory" => T("screen.victory", "Victory!"),
            "saveload" => T("screen.saveload", "Save And Load"),
            "settings" => T("screen.settings", "Settings"),
            _ => T("screen.ranch", "Ranch Overview")
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
