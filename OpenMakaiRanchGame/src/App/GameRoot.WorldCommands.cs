using System;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.App;

/// <summary>
/// Generation-guarded world command entry points recovered from the dev branch after the manual
/// merge kept the older GameRoot conflict side. Each method delegates to the existing canonical
/// simulation service; no duplicate shop, recruitment, mission or reward calculation is created.
/// </summary>
public partial class GameRoot
{
    public bool TryBuyItem(string? itemId, int quantity, ulong expectedGeneration)
    {
        if (expectedGeneration != StateGeneration || string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
            return false;

        if (!Shop.Buy(itemId, quantity))
            return false;

        StateChanged?.Invoke();
        return true;
    }

    public bool TryRecruit(ulong expectedGeneration)
    {
        if (expectedGeneration != StateGeneration)
            return false;

        if (!Recruitment.HireOffer())
            return false;

        StateChanged?.Invoke();
        return true;
    }

    public bool TryRunMission(string? missionId, ulong expectedGeneration)
    {
        if (expectedGeneration != StateGeneration || string.IsNullOrWhiteSpace(missionId))
            return false;

        var report = RunMission(missionId);
        return report.Outcome != MissionOutcome.None;
    }
}