using Godot;

namespace OpenMakaiRanch.World;

/// <summary>Handles the configurable pause shortcut in addition to the standard UI Back fallback.</summary>
public partial class WorldGameController
{
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!@event.IsActionPressed("pause_menu") || _flowLocksUi) return;

        if (_pauseMenu?.IsOpen == true)
        {
            _pauseMenu.Close();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (IsManagementVisible)
        {
            CloseManagement();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (_transition?.IsTransitioning == true || _firstDayFlow?.BlocksWorldInput == true) return;
        _pauseMenu?.Open(_activeAreaId);
        GetViewport().SetInputAsHandled();
    }
}
