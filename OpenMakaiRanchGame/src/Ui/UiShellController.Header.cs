using Godot;

namespace OpenMakaiRanch.Ui;

public partial class UiShellController
{
    private void BoundHeaderText(bool compact, float availableWidth)
    {
        // A FlowContainer sizes its children from their minimums. Wrapped status text with a
        // zero-width minimum can grow vertically one word/letter at a time and consume the
        // entire content viewport. Give the title column a real width and bound header lines,
        // without changing natural wrapping in the scrollable body.
        var titleBox = _statusLabel.GetParent<Control>();
        titleBox.CustomMinimumSize = new Vector2(
            Mathf.Min(compact ? 128f : 220f, Mathf.Max(64f, availableWidth - 64f)), 0f);
        _screenLabel.AutowrapMode = TextServer.AutowrapMode.Off;
        _screenLabel.MaxLinesVisible = 1;
        _screenLabel.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        _screenLabel.TooltipText = _screenLabel.Text;
        _statusLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _statusLabel.MaxLinesVisible = 2;
        _statusLabel.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        _statusLabel.TooltipText = _statusLabel.Text;
    }
}
