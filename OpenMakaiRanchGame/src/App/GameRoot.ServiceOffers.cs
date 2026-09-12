using System;
using System.Collections.Generic;
using OpenMakaiRanch.Core.Models;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.App;

public sealed record PurchaseOffer(string ItemId, int Quantity, int UnitPrice, int TotalPrice, int Owned, int Gold, bool CanBuy);
public sealed record ResearchOffer(string SkillId, string ResourceId, int Cost, int Available, bool Unlocked, bool CanUnlock);

/// <summary>Displayed decisions validated before calling the existing shop/research owners.</summary>
public partial class GameRoot
{
    public PurchaseOffer? InspectPurchase(string? id, int quantity)
    {
        if (string.IsNullOrWhiteSpace(id) || !Data.Items.TryGetValue(id, out var item)
            || quantity <= 0 || item.Price <= 0) return null;
        var owned = State.Inventory.Items.GetValueOrDefault(id);
        var cost = (long)item.Price * quantity;
        var representable = cost <= int.MaxValue && owned >= 0 && (long)owned + quantity <= int.MaxValue;
        return new(id, quantity, item.Price, (int)Math.Min(int.MaxValue, cost), owned, Economy.Gold,
            representable && Economy.Gold >= cost);
    }

    public ResearchOffer? InspectResearch(string? id)
    {
        if (string.IsNullOrWhiteSpace(id) || !Data.Skills.TryGetValue(id, out var skill)) return null;
        var available = string.IsNullOrWhiteSpace(skill.CostResourceId) ? 0 : State.Ranch.Stockpile.GetValueOrDefault(skill.CostResourceId);
        var unlocked = State.Research.UnlockedSkillIds.Contains(id);
        return new(id, skill.CostResourceId, skill.CostAmount, available, unlocked,
            !unlocked && skill.CostAmount >= 0 && (string.IsNullOrWhiteSpace(skill.CostResourceId) || available >= skill.CostAmount));
    }

    public StationActionResult TryPurchaseOffer(PurchaseOffer? offer, ulong generation, int day, DayPhase phase)
    {
        if (!StationCommandMatches(generation, day, phase) || offer is null || !offer.CanBuy
            || InspectPurchase(offer.ItemId, offer.Quantity) != offer) return ChangedStation();
        _stationCommandBusy = true;
        try
        {
            // Preflight includes wide arithmetic and inventory capacity; Shop still owns payment/inventory.
            if (!Shop.Buy(offer.ItemId, offer.Quantity)) return ChangedStation();
            StateChanged?.Invoke();
            return new(true, T("panel.shop.bought", "Purchased {0} for {1} G. The item is now in your bag.", offer.Quantity, offer.TotalPrice));
        }
        finally { _stationCommandBusy = false; }
    }

    public StationActionResult TryResearchOffer(ResearchOffer? offer, ulong generation, int day, DayPhase phase)
    {
        if (!StationCommandMatches(generation, day, phase) || offer is null || !offer.CanUnlock
            || InspectResearch(offer.SkillId) != offer) return ChangedStation();
        _stationCommandBusy = true;
        try
        {
            // Use the actual stock-funded ResearchService, not the unused gold/cooldown research tree.
            if (!Research.Unlock(offer.SkillId)) return ChangedStation();
            StateChanged?.Invoke();
            return new(true, T("panel.research.done", "Research unlocked. Its existing effects apply immediately; no day was skipped."));
        }
        finally { _stationCommandBusy = false; }
    }
}
