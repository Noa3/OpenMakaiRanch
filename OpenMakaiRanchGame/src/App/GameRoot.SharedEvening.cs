using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.App;

public partial class GameRoot
{
    private SharedEveningService SharedEvening => new(State, Data, Flags, Dating);
    public SharedEveningStatus GetSharedEveningStatus() => SharedEvening.Inspect();

    public bool TryPlanSharedEvening(ulong generation, int day, out string message)
    {
        if (_settlingDay || _combatWorldTimeLocked || generation != StateGeneration || day != State.Calendar.Day)
        { message = T("evening.reason.changed", "The situation changed. Review tonight's plan again."); return false; }
        if (!SharedEvening.TryPlan(out message)) return false;
        StateChanged?.Invoke(); return true;
    }

    public bool TryCancelSharedEvening(ulong generation, int day)
    {
        if (_settlingDay || _combatWorldTimeLocked || generation != StateGeneration || day != State.Calendar.Day) return false;
        SharedEvening.Cancel(); StateChanged?.Invoke(); return true;
    }

    public bool HadSharedEvening(int day) => day > 0 && Flags.GetGlobalIntFlag(SharedEveningService.LastSettledDayFlag) == day;
}
