using Godot;

namespace OpenMakaiRanch.World;

/// <summary>Event-owned shortcuts respect text editing and close only the innermost UI.</summary>
public partial class WorldGameController
{
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsEcho() || _flowLocksUi) return;
        if (@event.IsActionPressed("toggle_management"))
        {
            ToggleManagement();
            GetViewport().SetInputAsHandled();
            return;
        }
        if (!@event.IsActionPressed("pause_menu") && !@event.IsActionPressed("ui_cancel")) return;

        if (_pauseMenu?.IsOpen == true)
        {
            _pauseMenu.GoBack();
            GetViewport().SetInputAsHandled();
            return;
        }
        if (IsManagementVisible)
        {
            CloseManagement();
            GetViewport().SetInputAsHandled();
            return;
        }
        if (WorldHelpVisible)
        {
            CloseWorldHelp();
            GetViewport().SetInputAsHandled();
            return;
        }
        if (_transition?.IsTransitioning == true || _firstDayFlow?.BlocksWorldInput == true) return;
        _pauseMenu?.Open(_activeAreaId);
        GetViewport().SetInputAsHandled();
    }
}
