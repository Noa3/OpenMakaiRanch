using Godot;

namespace OpenMakaiRanch.App;

public partial class MainMenuController
{
    private Vector2 _menuLayoutSize = new(-1, -1);

    private void RefreshMenuLayout()
    {
        if (!GodotObject.IsInstanceValid(_panel)) return;
        var size = GetViewport().GetVisibleRect().Size;
        if (_menuLayoutSize.IsEqualApprox(size)) return;
        _menuLayoutSize = size;
        _panel.CustomMinimumSize = new Vector2(Mathf.Min(560, Mathf.Max(1, size.X - 48)),
            Mathf.Min(360, Mathf.Max(1, size.Y - 48)));
    }
}
