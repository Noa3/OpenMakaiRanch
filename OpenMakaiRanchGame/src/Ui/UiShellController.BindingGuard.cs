using Godot;

namespace OpenMakaiRanch.Ui;

public partial class UiShellController
{
    private bool IsLiveBindingControl(Button? button) => GodotObject.IsInstanceValid(button)
        && button!.IsInsideTree() && !button.IsQueuedForDeletion()
        && _content.GetNodeOrNull<Control>(ControlsExtensionName) is { } card
        && !card.IsQueuedForDeletion() && card.IsVisibleInTree() && card.IsAncestorOf(button);

    private bool TryCancelBindingInput(InputEvent input)
    {
        // Back remains a safety fallback for both capture devices, including the arming delay.
        var cancel = input is InputEventKey { Pressed: true, Echo: false } key
            && (key.Keycode == Key.Escape || key.PhysicalKeycode == Key.Escape)
            || input is InputEventJoypadButton { Pressed: true, ButtonIndex: JoyButton.B };
        if (!cancel) return false;
        CancelBindingCapture("Binding cancelled.");
        GetViewport().SetInputAsHandled();
        return true;
    }
}
