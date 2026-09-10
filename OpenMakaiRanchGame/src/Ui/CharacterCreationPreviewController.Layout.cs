using Godot;

namespace OpenMakaiRanch.Ui;

public partial class CharacterCreationPreviewController
{
    private float _creationLayoutWidth = -1;

    public override void _Process(double delta)
    {
        // A child's old minimum can temporarily make Size wider than the viewport. Do not let
        // that old minimum select the desktop layout again while shrinking the window.
        var width = Mathf.Max(1, Mathf.Min(Size.X, GetViewport().GetVisibleRect().Size.X - 48));
        if (Mathf.IsEqualApprox(width, _creationLayoutWidth)) return;
        _creationLayoutWidth = width;
        var narrow = width < 920;
        var preview = GetNode<Control>("CreationBody/PreviewCard");
        var frame = GetNode<Control>("CreationBody/PreviewCard/PreviewInner/PreviewFrame");
        var settings = GetNode<Control>("CreationBody/SettingsColumn");
        preview.CustomMinimumSize = new Vector2(Mathf.Min(340, width), narrow ? 330 : 540);
        frame.CustomMinimumSize = new Vector2(Mathf.Min(320, Mathf.Max(1, width - 24)), narrow ? 230 : 410);
        settings.CustomMinimumSize = new Vector2(Mathf.Min(560, width), 0);
        foreach (var section in new[] { "Basic", "Body", "Appearance", "Accessories", "PetMount" })
        {
            var grid = GetNode<GridContainer>($"CreationBody/SettingsColumn/{section}Card/{section}Inner/{section}Grid");
            grid.Columns = width < 520 ? 1 : 2;
            foreach (var node in grid.GetChildren())
                if (node is LineEdit or OptionButton)
                {
                    var control = (Control)node;
                    control.CustomMinimumSize = new Vector2(Mathf.Min(220, Mathf.Max(1, width - 24)), 34);
                }
        }
    }
}
