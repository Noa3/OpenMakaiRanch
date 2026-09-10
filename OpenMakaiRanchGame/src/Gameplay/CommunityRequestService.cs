using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Gameplay;

public sealed record CommunityRequestOffer(
    string Id, string Title, string Description, string ResourceId, string ResourceName,
    int Day, int RequiredAmount, int AvailableAmount, int RewardGold,
    string ProductionHint, bool CanDeliver, string UnavailableReason);

/// <summary>
/// Optional, bounded courier orders using the existing ranch stockpile and gold ledger.
/// Reading the board never produces goods, pays rewards, advances time or changes assignments.
/// The four durable flag IDs below belong to this feature; no per-day history grows in saves.
/// </summary>
public sealed class CommunityRequestService
{
    public const int OpeningDay = 2;
    public const int MaximumDailyRewardGold = 60;
    public const int LastDeliveryDayFlag = 1_230_000;
    public const int CompletedDeliveriesFlag = 1_230_001;
    public const int LastDeliveryKindFlag = 1_230_002;
    public const int LastDeliveryRewardFlag = 1_230_003;

    private readonly SaveState _state;
    private readonly FlagService _flags;
    private readonly EconomyService _economy;

    public CommunityRequestService(SaveState state, FlagService flags, EconomyService economy)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _flags = flags ?? throw new ArgumentNullException(nameof(flags));
        _economy = economy ?? throw new ArgumentNullException(nameof(economy));
    }

    public int CompletedDeliveries => Math.Max(0, _flags.GetGlobalIntFlag(CompletedDeliveriesFlag));
    public bool HasDeliveredToday => _state.Calendar.Day >= OpeningDay
        && _flags.GetGlobalIntFlag(LastDeliveryDayFlag) >= _state.Calendar.Day;

    public IReadOnlyList<CommunityRequestOffer> GetOffers()
    {
        // Modulo before addition also keeps extreme/corrupt day values from overflowing.
        var day = Math.Max(1, _state.Calendar.Day);
        var farmVariation = day % 3;
        var mealVariation = day % 2;
        var supplyVariation = (day % 3 + 1) % 3;
        return Array.AsReadOnly(new[]
        {
            Offer("market_basket", "A basket for the market",
                "The market stalls need a small basket of ranch goods. A courier can collect it here.",
                "farm_goods", "Farm goods", 3 + farmVariation, 30 + farmVariation * 10,
                "Schedule Pasture Work. Goods are added at the end-of-day settlement, not when assigning a job."),
            Offer("kitchen_delivery", "Lunch for the town crew",
                "The town crew could use prepared meals. Keep any meals you need for other ranch activities.",
                "meals", "Meals", 2 + mealVariation, 35 + mealVariation * 10,
                "Schedule Kitchen Chores or Cooking, then finish the day. Check the daily report for production."),
            Offer("workshop_delivery", "Supplies for small repairs",
                "The town workshop is collecting supplies for repairs. There is no need to make a special trip.",
                "supplies", "Supplies", 2 + supplyVariation, 40 + supplyVariation * 10,
                "Schedule Workshop Crafting or Office Work, then finish the day. Existing stock also counts.")
        });
    }

    public bool TryDeliver(string? requestId, int expectedDay, out string message)
    {
        if (expectedDay != _state.Calendar.Day || expectedDay < OpeningDay)
        {
            message = expectedDay < OpeningDay
                ? "The courier board opens on Day 2. Finish your first day to begin."
                : "The day has changed. Review the current board before delivering.";
            return false;
        }

        var offers = GetOffers();
        var offer = offers.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, requestId, StringComparison.Ordinal));
        if (offer is null)
        {
            message = "That request is not on the current board.";
            return false;
        }
        if (!offer.CanDeliver)
        {
            message = offer.UnavailableReason;
            return false;
        }

        // All validation is complete. The GameRoot command calls this synchronously on the game
        // thread, and publishes StateChanged only after the receipt is durable in FlagService.
        _state.Ranch.Stockpile[offer.ResourceId] = offer.AvailableAmount - offer.RequiredAmount;
        _economy.AddGold(offer.RewardGold);
        _flags.SetGlobalIntFlag(LastDeliveryDayFlag, expectedDay);
        _flags.SetGlobalIntFlag(CompletedDeliveriesFlag,
            CompletedDeliveries == int.MaxValue ? int.MaxValue : CompletedDeliveries + 1);
        _flags.SetGlobalIntFlag(LastDeliveryKindFlag,
            offer.Id == "market_basket" ? 1 : offer.Id == "kitchen_delivery" ? 2 : 3);
        _flags.SetGlobalIntFlag(LastDeliveryRewardFlag, offer.RewardGold);
        message = $"Delivered {offer.RequiredAmount} {offer.ResourceName.ToLowerInvariant()}. +{offer.RewardGold} G paid. Today's courier delivery is complete; the rest of the day is yours.";
        return true;
    }

    private CommunityRequestOffer Offer(string id, string title, string description,
        string resourceId, string resourceName, int required, int reward, string hint)
    {
        var available = _state.Ranch.Stockpile.TryGetValue(resourceId, out var amount)
            ? Math.Max(0, amount) : 0;
        var reason = string.Empty;
        if (_state.Calendar.Day < OpeningDay)
            reason = "Opens on Day 2. Set up ranch work, then finish your first day.";
        else if (HasDeliveredToday)
            reason = "Today's courier delivery is complete. Other orders are optional on a later day.";
        else if (available < required)
            reason = $"Need {required - available} more {resourceName.ToLowerInvariant()}. {hint}";
        else if (_economy.Gold > int.MaxValue - reward)
            reason = "There is no room for the full gold reward. No goods will be taken.";

        return new CommunityRequestOffer(id, title, description, resourceId, resourceName,
            _state.Calendar.Day, required, available, reward, hint, reason.Length == 0, reason);
    }
}
