using Godot;

namespace OpenMakaiRanch.World;

/// <summary>Single event-owned route for the configurable pause shortcut and standard UI Back.</summary>
public partial class WorldGameController
{
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsEcho() || _flowLocksUi
            || (!@event.IsActionPressed("pause_menu") && !@event.IsActionPressed("ui_cancel")))
            return;

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

        if (_transition?.IsTransitioning == true || _firstDayFlow?.BlocksWorldInput == true)
            return;

        _pauseMenu?.Open(_activeAreaId);
        GetViewport().SetInputAsHandled();
    }
}
