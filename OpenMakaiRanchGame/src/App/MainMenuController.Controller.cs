using Godot;

namespace OpenMakaiRanch.App;

/// <summary>Ensures the main menu is immediately navigable by keyboard/controller without a mouse.</summary>
public partial class MainMenuController
{
    private bool _initialMenuFocusApplied;

    public override void _Process(double delta)
    {
        RefreshMenuLayout();
        if (_initialMenuFocusApplied || !IsVisibleInTree() || GetViewport().GuiGetFocusOwner() is not null)
        {
            return;
        }

        Button? target = null;
        if (GodotObject.IsInstanceValid(_continueButton) && _continueButton.Visible && !_continueButton.Disabled)
        {
            target = _continueButton;
        }
        else if (GodotObject.IsInstanceValid(_newGameButton) && _newGameButton.Visible && !_newGameButton.Disabled)
        {
            target = _newGameButton;
        }

        if (target is null)
        {
            return;
        }

        target.GrabFocus();
        _initialMenuFocusApplied = true;
    }
}
