using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Tests;

/// <summary>Quotes and command boundaries, not a market-balance or full UI certification.</summary>
public static class GameplayPanelRegressionTests
{
    public static void Run(SmokeTestResult result)
    {
        var game = GameRoot.Instance;
        void Check(bool pass, string message)
        { result.Passed &= pass; result.Lines.Add($"SMOKE {(pass ? "OK" : "FAIL")} gameplay panels: {message}"); }
        string Snapshot()
        {
            var storage = new FlagStorage(); game.Flags.SyncToStorage(storage);
            return JsonSerializer.Serialize(new { game.State, Flags = storage });
        }
        var paused = game.GetTree().Paused;
        try
        {
            game.NewGame(); game.State.Calendar.Day = 2; game.State.Story.FirstDayCompleted = true;
            var generation = game.StateGeneration; var day = game.State.Calendar.Day; var phase = game.State.Calendar.Phase;
            var before = Snapshot();
            for (var i = 0; i < 50; i++)
            {
                game.InspectPurchase("meal_box", 1);
                foreach (var id in game.Data.Skills.Keys) game.InspectResearch(id);
                foreach (var definition in game.Data.Jobs.Values) game.Ranch.InspectWorkBenefitAtLevel(definition, 5);
            }
            Check(before == Snapshot(), "repeated food, purchase, research and tier previews are read-only");
            Check(game.InspectPurchase(null, 1) is null && game.InspectPurchase("meal_box", 0) is null
                && game.InspectPurchase("missing", 1) is null, "unknown or zero-quantity purchases do not create an offer");
            var quote = game.InspectPurchase("meal_box", 1)!;
            Check(quote.CanBuy && quote.TotalPrice == game.Data.Items["meal_box"].Price,
                "normal purchase uses the existing catalog price and starting wallet");
            Check(!game.InspectPurchase("meal_box", int.MaxValue)!.CanBuy,
                "wide price multiplication rejects an unrepresentable bulk purchase");
            foreach (var invalid in new[] { quote with { Quantity = 0 }, quote with { UnitPrice = 1 }, quote with { TotalPrice = 0 }, quote with { Gold = quote.Gold + 1 } })
                Check(!game.TryPurchaseOffer(invalid, generation, day, phase).Success && before == Snapshot(),
                    "altered quotes are rejected before payment or inventory mutation");
            Check(!game.TryPurchaseOffer(quote, generation + 1, day, phase).Success
                && !game.TryPurchaseOffer(quote, generation, day + 1, phase).Success
                && !game.TryPurchaseOffer(quote, generation, day, DayPhase.Night).Success && before == Snapshot(),
                "stale generation, day and phase cannot commit a displayed purchase");
            game.GetTree().Paused = true;
            Check(!game.TryPurchaseOffer(quote, generation, day, phase).Success && before == Snapshot(), "paused purchase is inert");
            game.GetTree().Paused = false;
            game.BeginCombatSession(); before = Snapshot();
            Check(!game.TryPurchaseOffer(quote, generation, day, phase).Success && before == Snapshot(), "combat preparation rejects unrelated shop actions");
            game.EndCombatSession();
            var price = game.Data.Items["meal_box"].Price;
            try
            {
                game.Data.Items["meal_box"].Price++;
                before = Snapshot();
                Check(!game.TryPurchaseOffer(quote, generation, day, phase).Success && before == Snapshot(),
                    "a catalog-price change invalidates the already displayed offer");
            }
            finally { game.Data.Items["meal_box"].Price = price; }
            game.State.Inventory.Items["meal_box"] = int.MaxValue;
            before = Snapshot();
            Check(!game.InspectPurchase("meal_box", 1)!.CanBuy
                && !game.TryPurchaseOffer(quote, generation, day, phase).Success && before == Snapshot(),
                "full inventory stacks cannot wrap negative or be charged by an old quote");
            game.State.Inventory.Items["meal_box"] = 2;
            quote = game.InspectPurchase("meal_box", 1)!;
            var notifications = 0; var nested = false; var advanced = false;
            void Reenter()
            {
                notifications++;
                if (notifications != 1) return;
                nested = game.TryPurchaseOffer(game.InspectPurchase("meal_box", 1), generation, day, phase).Success;
                advanced = game.AdvanceTime();
            }
            game.StateChanged += Reenter;
            try
            {
                Check(game.TryPurchaseOffer(quote, generation, day, phase).Success
                    && game.Economy.Gold == quote.Gold - quote.TotalPrice
                    && game.State.Inventory.Items["meal_box"] == quote.Owned + 1,
                    "one accepted purchase consumes exactly its price and adds exactly one item");
            }
            finally { game.StateChanged -= Reenter; }
            Check(notifications == 1 && !nested && !advanced, "purchase publication cannot reenter another purchase or advance time");
            before = Snapshot();
            Check(!game.TryPurchaseOffer(quote, generation, day, phase).Success && before == Snapshot(),
                "replaying the successful quote cannot buy another item");

            var skill = game.Data.Skills.Values.First(s => s.CostAmount > 0 && !string.IsNullOrWhiteSpace(s.CostResourceId));
            game.State.Ranch.Stockpile[skill.CostResourceId] = skill.CostAmount;
            var research = game.InspectResearch(skill.Id)!;
            Check(research.CanUnlock && !research.Unlocked && research.Cost == skill.CostAmount,
                "research quote uses stock-funded canonical research, not the separate unused gold tree");
            before = Snapshot();
            Check(!game.TryResearchOffer(research with { Cost = 0 }, generation, day, phase).Success && before == Snapshot(),
                "changing the displayed research cost rejects before unlocking");
            game.State.Ranch.Stockpile[skill.CostResourceId]--;
            before = Snapshot();
            Check(!game.TryResearchOffer(research, generation, day, phase).Success && before == Snapshot(),
                "missing research resources invalidate the old quote without a partial charge");
            game.State.Ranch.Stockpile[skill.CostResourceId] = skill.CostAmount;
            var gold = game.Economy.Gold;
            Check(game.TryResearchOffer(research, generation, day, phase).Success
                && game.State.Research.UnlockedSkillIds.Count(id => id == skill.Id) == 1
                && game.State.Ranch.Stockpile[skill.CostResourceId] == 0
                && game.State.Calendar.Day == day && game.State.Calendar.Phase == phase,
                "research spends the listed stock exactly once without advancing the calendar");
            // Existing milestone rewards may change gold. They are not a second purchase cost.
            Check(game.Economy.Gold >= gold, "stock-funded research does not secretly charge gold");
            before = Snapshot();
            Check(!game.TryResearchOffer(research, generation, day, phase).Success && before == Snapshot(),
                "completed research cannot repeat its cost or milestone effects");
            var job = game.Data.Jobs["kitchen"];
            var currentLevel = game.Ranch.Facilities.GetValueOrDefault("kitchen");
            Check(game.Ranch.InspectWorkBenefit(job) == game.Ranch.InspectWorkBenefitAtLevel(job, currentLevel)
                && game.Ranch.InspectWorkBenefitAtLevel(job, 6).ExtraUnits == game.Ranch.InspectWorkBenefitAtLevel(job, 5).ExtraUnits,
                "upgrade previews and current production share the same bounded equipment contribution");
            quote = game.InspectPurchase("meal_box", 1)!;
            game.NewGame(); before = Snapshot();
            Check(!game.TryPurchaseOffer(quote, generation, day, phase).Success && before == Snapshot(),
                "a replaced session cannot accept an earlier shop decision");
        }
        finally { game.GetTree().Paused = paused; game.NewGame(); }
    }
}
