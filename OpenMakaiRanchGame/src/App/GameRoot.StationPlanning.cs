using System;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.App;

public sealed record StationActionResult(bool Success, string Message);

public partial class GameRoot
{
    private bool _stationCommandBusy;

    private bool StationCommandMatches(ulong generation, int day, DayPhase phase) =>
        !_stationCommandBusy && !_residentCommandBusy && !_settlingDay && !_combatWorldTimeLocked
        && GetTree()?.Paused != true && generation == StateGeneration && day == State.Calendar.Day && phase == State.Calendar.Phase;

    private static StationActionResult ChangedStation() => new(false,
        T("world.station.changed", "The plan or price changed. Review this station again; nothing was charged."));

    public StationActionResult TryUpgradeFacility(FacilityUpgradeOffer? quote, ulong generation, int day, DayPhase phase)
    {
        if (!StationCommandMatches(generation, day, phase) || quote is null) return ChangedStation();
        var current = Ranch.InspectFacilityUpgrade(quote.FacilityId);
        if (!current.CanUpgrade) return new(false, current.Reason);
        if (current != quote) return ChangedStation(); // Reject a reused price or changed upkeep projection.
        _stationCommandBusy = true;
        try
        {
            if (!Ranch.UpgradeFacility(quote.FacilityId, Economy)) return ChangedStation();
            StateChanged?.Invoke();
            return new(true, T("world.station.upgrade.done", "Level {0} completed for {1} gold. Daily facility upkeep is now {2} gold; no work output was paid.",
                quote.NextLevel, quote.Cost, quote.RanchUpkeepAfter));
        }
        finally { _stationCommandBusy = false; }
    }

    // The world supplies its resolved station's real job/facility, never labels from a translated UI.
    // Raw simulation callers retain ScheduleService; this boundary is for a displayed work plan.
    public StationActionResult TryPlanStationJob(string residentId, string jobId, string requiredFacilityId,
        string previousJobId, bool release, ulong generation, int day, DayPhase phase)
    {
        if (!StationCommandMatches(generation, day, phase) || string.IsNullOrWhiteSpace(residentId)
            || State.Roster.Characters.Count(c => c.Id == residentId) != 1
            || string.IsNullOrWhiteSpace(jobId) || !Data.Jobs.TryGetValue(jobId, out var job) || !job.Assignable
            || Schedule.GetAssignment(residentId) != previousJobId) return ChangedStation();
        if (release && previousJobId != jobId) return ChangedStation();
        if (!release && !string.IsNullOrWhiteSpace(requiredFacilityId)
            && (!Data.Facilities.ContainsKey(requiredFacilityId)
                || !Ranch.Facilities.TryGetValue(requiredFacilityId, out var level) || level <= 0))
            return new(false, T("world.station.build_first", "Build this facility before assigning its team."));
        var next = release ? "rest" : jobId;
        if (!TryAssignJob(residentId, next, generation)) return ChangedStation();
        return new(true, release ? T("world.work.rest_success", "Rest assigned.")
            : T("world.station.assignment.done", "Work plan updated. Production and earnings are applied only at day end."));
    }
}
