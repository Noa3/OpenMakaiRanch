using Godot;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Ui;

public partial class UiShellController
{
    private WorldGameController? WorldHost()
    {
        for (Node? node = GetParent(); node is not null; node = node.GetParent())
            if (node is WorldGameController host) return host;
        return null;
    }

    private void ApplyOpeningUtilityLayout()
    {
        if (!_shellReady || _fullScreenMode) return;
        var restricted = WorldHost()?.IsGuidedOpening == true;
        if (restricted)
        {
            _navPanel.Visible = false;
            _compactNavigationScroll.Visible = false;
        }
        _endDayButton.Visible = !restricted;
        _menuButton.Visible = !restricted;
    }
}
