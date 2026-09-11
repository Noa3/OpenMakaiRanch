using System;
using OpenMakaiRanch.Core.Resources;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Gameplay;

/// <summary>A read-only price and upkeep quote. Never a pending purchase or save object.</summary>
public sealed record FacilityUpgradeOffer(string FacilityId, int Level, int NextLevel, int Cost,
    int BaseUpkeepBefore, int BaseUpkeepAfter, int RanchUpkeepBefore, int RanchUpkeepAfter,
    bool CanUpgrade, string Reason);

public sealed partial class RanchService
{
    public FacilityUpgradeOffer InspectFacilityUpgrade(string? facilityId)
    {
        var total = FacilityUpkeep();
        if (string.IsNullOrWhiteSpace(facilityId) || !_data.Facilities.TryGetValue(facilityId, out var definition))
            return new(facilityId ?? "", 0, 0, 0, 0, 0, total, total, false,
                T("world.station.upgrade.unknown", "This place has no equipment upgrade."));
        _state.Ranch.Facilities.TryGetValue(facilityId, out var level);
        var valid = level >= 0 && level < int.MaxValue && definition.BuildCost >= 0 && definition.UpkeepGold >= 0
            && (long)definition.BuildCost + (long)level * 75 <= int.MaxValue;
        var cost = FacilityUpgradeCost(definition, level);
        var next = valid ? level + 1 : level;
        var reason = !valid ? T("world.station.upgrade.limit", "No further upgrade is supported at this level.")
            : _state.Economy.Gold < cost ? T("world.station.upgrade.shortfall", "You need {0} more gold before buying this upgrade.", (long)cost - _state.Economy.Gold)
            : "";
        return new(facilityId, level, next, cost, FacilityBaseUpkeep(definition, level),
            FacilityBaseUpkeep(definition, next), total, CalculateFacilityUpkeep(facilityId, next), reason.Length == 0, reason);
    }

    public bool UpgradeFacility(string facilityId, EconomyService economy)
    {
        var offer = InspectFacilityUpgrade(facilityId);
        if (!offer.CanUpgrade || !economy.Spend(offer.Cost)) return false;
        _state.Ranch.Facilities[facilityId] = offer.NextLevel;
        return true;
    }

    public int FacilityUpgradeCost(FacilityDefinition definition, int currentLevel) =>
        (int)Math.Clamp((long)definition.BuildCost + Math.Max(0L, currentLevel) * 75, 0L, int.MaxValue);

    public static int FacilityBaseUpkeep(FacilityDefinition definition, int level) =>
        (int)Math.Min(int.MaxValue, Math.Max(0L, definition.UpkeepGold) * Math.Max(0L, level));

    public int FacilityUpkeep() => CalculateFacilityUpkeep(null, 0);

    private int CalculateFacilityUpkeep(string? replacementId, int replacementLevel)
    {
        long total = 0;
        void Include(FacilityDefinition definition, int level)
        {
            if (level <= 0) return; // An unbuilt entry is not a maintained building.
            var amount = Math.Max(0L, definition.UpkeepGold) * level;
            total = total > long.MaxValue - amount ? long.MaxValue : total + amount;
        }
        foreach (var entry in _state.Ranch.Facilities)
            if (entry.Key != replacementId && _data.Facilities.TryGetValue(entry.Key, out var definition))
                Include(definition, entry.Value);
        if (replacementId is not null && _data.Facilities.TryGetValue(replacementId, out var replacement))
            Include(replacement, replacementLevel);
        // Logistics is rounded once on the whole ranch bill, never once per individual building.
        if (total > 0 && _state.Research.UnlockedSkillIds.Contains("logistics"))
            total -= Math.Max(1L, total / 4);
        return (int)Math.Min(int.MaxValue, total);
    }
}
