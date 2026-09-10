using System.Text;
using Godot;

namespace OpenMakaiRanch.Tests;

public partial class UiLayoutAcceptance
{
    private string _lastFocusGeometry = string.Empty;

    public override void _Process(double delta)
    {
        if (string.IsNullOrEmpty(_evidence)) return;
        var focus = GetViewport().GuiGetFocusOwner();
        if (!GodotObject.IsInstanceValid(focus) || !focus!.IsInsideTree()) return;
        var text = new StringBuilder($"{GetTree().Root.Size}: {focus.Name} rect={focus.GetGlobalRect()}");
        for (var node = focus.GetParent(); node is not null; node = node.GetParent())
            if (node is Control { ClipContents: true } clip)
                text.Append($" | clip {clip.Name}={clip.GetGlobalRect()}");
        var geometry = text.ToString();
        if (geometry == _lastFocusGeometry) return;
        _lastFocusGeometry = geometry;
        GD.Print("UI FOCUS " + geometry);
    }
}
