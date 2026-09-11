using Godot;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.App;

public partial class MainMenuController
{
    private Label? _vistaCaption;

    private void SetupTitleVista()
    {
        // A separate, input-transparent diorama, not the live ranch or an auto-loaded save.
        _background.AddChild(new MainMenuVista { Name = "TitleVista" });
        var veil = new ColorRect { Color = new Color(0.015f, 0.035f, 0.055f, 0.20f), MouseFilter = MouseFilterEnum.Ignore };
        _background.AddChild(veil);
        veil.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var title = GetNode<Label>("Root/Center/Panel/Content/TitleLabel");
        title.AddThemeFontSizeOverride("font_size", 32);
        title.AddThemeColorOverride("font_color", new Color("f0dfb3"));
        title.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _vistaCaption = new Label
        {
            Name = "TitleCaption", HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = MouseFilterEnum.Ignore, CustomMinimumSize = new Vector2(0, 38)
        };
        _vistaCaption.AddThemeColorOverride("font_color", new Color("d1dcd6"));
        title.GetParent().AddChild(_vistaCaption);
        title.GetParent().MoveChild(_vistaCaption, 1);
        foreach (var button in new[] { _continueButton, _newGameButton, _newGamePlusButton, _quitButton })
        {
            button.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            button.CustomMinimumSize = new Vector2(0, 42);
            var normal = new StyleBoxFlat { BgColor = new Color("263e46"),
                CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6, CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
                ContentMarginLeft = 14, ContentMarginRight = 14, ContentMarginTop = 8, ContentMarginBottom = 8 };
            button.AddThemeStyleboxOverride("normal", normal);
            var hover = (StyleBoxFlat)normal.Duplicate(); hover.BgColor = new Color("3a555a");
            button.AddThemeStyleboxOverride("hover", hover);
            var pressed = (StyleBoxFlat)normal.Duplicate(); pressed.BgColor = new Color("192e35");
            button.AddThemeStyleboxOverride("pressed", pressed);
            button.AddThemeStyleboxOverride("focus", new StyleBoxFlat { DrawCenter = false,
                BorderColor = new Color("e4cb8f"), BorderWidthLeft = 2, BorderWidthRight = 2,
                BorderWidthTop = 2, BorderWidthBottom = 2, CornerRadiusTopLeft = 6,
                CornerRadiusTopRight = 6, CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6 });
        }
    }

    private void RefreshTitleVista()
    {
        if (_vistaCaption is not null)
            _vistaCaption.Text = T("mainmenu.caption", "A home to restore. A world to discover.");
        var index = System.Array.IndexOf(AvailableLocales, CurrentLocale);
        if (index >= 0) _langPicker.Selected = index;
    }
}
