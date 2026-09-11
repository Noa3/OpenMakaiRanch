using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.App;

public partial class GameRoot
{
    private bool _residentCommandBusy;
    // Resolve current state-bound services after NewGame/Load; never keep an old resident reference in UI.
    public ResidentInteractionService Residents => new(State, Data, Flags, Visit, Bond, Training, PlayerStamina);

    public ResidentActionResult TryResidentAction(string? id, ResidentAction action, ulong generation,
        int day, DayPhase phase, string? itemId = null)
    {
        if (_residentCommandBusy || _settlingDay || _combatWorldTimeLocked || generation != StateGeneration
            || day != State.Calendar.Day || phase != State.Calendar.Phase || GetTree()?.Paused == true)
            return new(false, false, T("world.resident.reason.changed", "The situation changed. Review this resident's options again."));
        _residentCommandBusy = true;
        try
        {
            var result = Residents.Execute(id, action, itemId);
            // The command remains locked during observers; a redraw cannot recursively buy another action.
            if (result.Changed) StateChanged?.Invoke();
            return result;
        }
        finally { _residentCommandBusy = false; }
    }
}
