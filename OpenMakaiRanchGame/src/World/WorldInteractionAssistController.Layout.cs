using Godot;

namespace OpenMakaiRanch.World;

public partial class WorldInteractionAssistController
{
    private void RefreshHudLayout()
    {
        if (_panel is null || _label is null || _actionButton is null) return;
        // The host binds its area controllers after this child's _Ready. Hide the legacy
        // prompts once those bindings exist, rather than leaving duplicate instructions.
        HideLegacyPrompts();
        var metrics = ScreenLayout.Calculate(GetViewport());
        var width = Mathf.Min(780, Mathf.Max(1, metrics.ContentWidth - 108));
        var narrow = width < 600;
        var row = _panel.GetNode<BoxContainer>("Row");
        row.Vertical = narrow;
        _label.MaxLinesVisible = 2;
        _label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        _actionButton.ClipText = true;
        _actionButton.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        var height = narrow && _actionButton.Visible ? 112 : 68;
        var shift = metrics.HorizontalCenter - metrics.ViewportSize.X / 2 - (metrics.ContentWidth < 1000 ? 38 : 0);
        _panel.OffsetLeft = shift - width / 2;
        _panel.OffsetRight = shift + width / 2;
        _panel.OffsetTop = -metrics.SafeBottom - 38 - height;
        _panel.OffsetBottom = -metrics.SafeBottom - 38;
    }
}
