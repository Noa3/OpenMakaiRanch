using System.Linq;
using Godot;

namespace OpenMakaiRanch.Ui;

public partial class UiShellController
{
    private void FinalizeSequentialManagementCards()
    {
        // These legacy renderers use PanelContainer as a vertical list. Godot instead
        // gives every direct Control child the same rectangle. Normalize once, during
        // composition, before focus restoration/container layout. Do not touch authored
        // overlays, character previews or other screens that may intentionally overlap.
        if (_currentScreen is "adventure" or "combat")
            StackSequentialCards(_content);
    }

    internal static void StackSequentialCards(Node root)
    {
        // Work bottom-up on a stable snapshot so nested mission/result cards are handled
        // without reparenting the same control twice. Existing single-content cards stay
        // untouched, preserving their nodes, signals, focus keys and explicit layout.
        foreach (var child in root.GetChildren())
            StackSequentialCards(child);
        if (root is not PanelContainer panel) return;
        var children = panel.GetChildren().OfType<Control>().ToArray();
        if (children.Length <= 1) return;
        var stack = new VBoxContainer
        {
            Name = "SequentialCardContent",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        stack.AddThemeConstantOverride("separation", 6);
        panel.AddChild(stack);
        foreach (var child in children)
            child.Reparent(stack, false);
    }
}
