using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Gameplay;

public sealed record RanchProjectStep(string Id, string Title, string Detail, int Progress, int Target,
    string AreaId, string StationId, string Action, bool Complete);

/// <summary>Read-only, optional projects derived from existing progress. No second quest wallet or timers.</summary>
public static class RanchProjectService
{
    public static IReadOnlyList<RanchProjectStep> Evaluate(SaveState state, FlagService flags)
    {
        var supplies = Math.Max(0, state.Ranch.Stockpile.GetValueOrDefault("supplies"));
        var corner = flags.GetGlobalFlag(RanchLeisureService.RestoredFlag);
        var deliveries = Math.Max(0, flags.GetGlobalIntFlag(CommunityRequestService.CompletedDeliveriesFlag));
        var kitchen = state.Ranch.Facilities.GetValueOrDefault("kitchen") >= 2;

        var patrol = state.Milestones.CompletedIds.Contains("first_patrol");
        // Existing receipts, rather than current stock/wallet, determine permanent accomplishment.
        return Array.AsReadOnly(new[]
        {
            new RanchProjectStep("breathing_room", T("project.corner.title", "A place to breathe"),
                T("project.corner.detail", "Restore the quiet corner: {0} supplies and {1} G. It provides a daily break without upkeep.", RanchLeisureService.RestoreSupplyCost, RanchLeisureService.RestoreGoldCost),
                corner ? 1 : 0, 1, "ranch", supplies < RanchLeisureService.RestoreSupplyCost && !corner ? "office" : "POINT_QUIET_CORNER",
                supplies < RanchLeisureService.RestoreSupplyCost && !corner ? T("project.corner.produce", "Assign Office Work, then settle the day") : T("project.corner.restore", "Visit the quiet corner"), corner),
            new RanchProjectStep("first_kitchen", T("project.kitchen.title", "A better kitchen"),
                T("project.kitchen.detail", "Upgrade the kitchen to level two. Review the price and daily upkeep first; job output still depends on workers, condition and research."),
                kitchen ? 1 : 0, 1, "ranch", "kitchen", kitchen ? T("project.kitchen.work", "Assign Kitchen Chores, then settle the day") : T("project.kitchen.build", "Upgrade the kitchen at its station"), kitchen),
            new RanchProjectStep("town_supplier", T("project.supplier.title", "A dependable neighbour"),
                T("project.supplier.detail", "Complete three community deliveries at your own pace. Existing delivery rewards apply; there is no extra completion payment or deadline."),
                Math.Min(3, deliveries), 3, "ranch", "POINT_COMMUNITY_BOARD", T("project.supplier.action", "Check the community board"), deliveries >= 3),
            new RanchProjectStep("first_expedition", T("project.patrol.title", "Beyond the ranch"),
                T("project.patrol.detail", "Complete your first successful expedition. Prepare the party and check its health before entering battle."),
                patrol ? 1 : 0, 1, "town", "adventure_guild", T("project.patrol.action", "Prepare an expedition at the guild"), patrol)
        });
    }
}
