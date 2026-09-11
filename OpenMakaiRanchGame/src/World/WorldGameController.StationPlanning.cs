using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.World;

public partial class WorldGameController
{
    private bool MatchingWorkStation(string id, out WorldStation? station)
    {
        station = ResolveStation(id);
        return IsStationPanelOpen && _stationPanel?.ContextKind == "station" && _stationPanel.ContextId == id
            && station is not null && station.CommandKind == WorldCommandKind.AssignJob && CanUseStationHere(station);
    }

    private static StationActionResult StationNotNearby() => new(false,
        T("world.station.nearby", "Open this station nearby before changing its team or equipment."));

    public StationActionResult TryUpgradeAtStation(string id, FacilityUpgradeOffer quote, ulong generation, int day, DayPhase phase)
    {
        if (!MatchingWorkStation(id, out var station) || station!.RequiredFacilityId != quote.FacilityId)
            return StationNotNearby();
        return GameRoot.Instance.TryUpgradeFacility(quote, generation, day, phase);
    }

    public StationActionResult TryAssignAtStation(string id, string residentId, string previousJobId, bool release,
        ulong generation, int day, DayPhase phase)
    {
        if (!MatchingWorkStation(id, out var station) || (!release && !station!.IsAvailable)) return StationNotNearby();
        var game = GameRoot.Instance;
        var result = game.TryPlanStationJob(residentId, station!.CommandTargetId, station.RequiredFacilityId,
            previousJobId, release, generation, day, phase);
        if (result.Success && !release && generation == game.StateGeneration && game.State.Calendar.Day == day
            && game.State.Calendar.Phase == phase && game.Schedule.GetAssignment(residentId) == station.CommandTargetId)
        {
            if (IsGuidedOpening) _stationPanel!.Close();
            _ranch?.NotifyStationAssignment(residentId, station.CommandTargetId);
        }
        return result;
    }
}
