using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

public enum WorldAlertSeverity
{
    Info,
    Warning,
    Critical
}

public sealed record WorldAlert(
    string Id,
    WorldAlertSeverity Severity,
    string Title,
    string Detail,
    string SuggestedScreen = "ranch");

/// <summary>
/// Presentation-only health/attention evaluator. It derives warnings from existing GameRoot state
/// and never mutates needs, jobs, economy, rewards or progression.
/// </summary>
public static class WorldAlertEvaluator
{
    public static IReadOnlyList<WorldAlert> Evaluate(GameRoot? game)
    {
        if (game is null)
        {
            return Array.Empty<WorldAlert>();
        }

        var alerts = new List<WorldAlert>();
        var state = game.State;

        AddCattleAlerts(game, alerts);
        AddStockpileAlerts(game, alerts);
        AddRosterAlerts(game, alerts);
        AddPetAlerts(game, alerts);
        AddEconomyAlerts(game, alerts);

        if (state.Calendar.Phase == OpenMakaiRanch.Core.Models.DayPhase.Night
            && state.Calendar.NightAction is not ("rest" or "train" or "admin"))
        {
            alerts.Add(new WorldAlert(
                "night_plan_missing",
                WorldAlertSeverity.Warning,
                "Night plan missing",
                "Choose Rest, Train or Admin before ending the day.",
                "ranch"));
        }

        return alerts
            .OrderByDescending(alert => alert.Severity)
            .ThenBy(alert => alert.Id, StringComparer.Ordinal)
            .ToArray();
    }

    private static void AddCattleAlerts(GameRoot game, List<WorldAlert> alerts)
    {
        var health = game.State.Ranch.CattleHealth;
        var pastureWorkers = CountJob(game, "pasture");
        var dairyWorkers = CountJob(game, "dairy");
        var dairyBuilt = game.Ranch.Facilities.TryGetValue("dairy_barn", out var dairyLevel) && dairyLevel > 0;

        if (health <= 25)
        {
            alerts.Add(new WorldAlert(
                "cattle_health_critical",
                WorldAlertSeverity.Critical,
                "Cattle health critical",
                $"Cattle health is {health}/100. Assign ranch care and review supplies before advancing the day.",
                "schedule"));
        }
        else if (health <= 50)
        {
            alerts.Add(new WorldAlert(
                "cattle_health_low",
                WorldAlertSeverity.Warning,
                "Cattle health is low",
                $"Cattle health is {health}/100. Check ranch staffing and resources.",
                "schedule"));
        }

        if (pastureWorkers == 0)
        {
            alerts.Add(new WorldAlert(
                "pasture_unstaffed",
                health <= 50 ? WorldAlertSeverity.Critical : WorldAlertSeverity.Warning,
                "Pasture has no worker",
                health <= 50
                    ? "Cattle health is already low and nobody is assigned to Pasture Work."
                    : "Nobody is assigned to Pasture Work. Rest is valid, but the pasture will produce nothing.",
                "schedule"));
        }

        if (dairyBuilt && dairyWorkers == 0)
        {
            alerts.Add(new WorldAlert(
                "dairy_unstaffed",
                WorldAlertSeverity.Warning,
                "Dairy Barn has no worker",
                "The Dairy Barn is built, but nobody is assigned to Dairy Work.",
                "schedule"));
        }
    }

    private static void AddStockpileAlerts(GameRoot game, List<WorldAlert> alerts)
    {
        var stockpile = game.State.Ranch.Stockpile;
        var meals = stockpile.GetValueOrDefault("meals");
        var supplies = stockpile.GetValueOrDefault("supplies");

        if (meals <= 0)
        {
            alerts.Add(new WorldAlert(
                "meals_empty",
                WorldAlertSeverity.Critical,
                "No prepared meals",
                "Meal stock is empty. Assign Kitchen/Cooking work or buy useful supplies before the situation worsens.",
                "schedule"));
        }
        else if (meals <= 2)
        {
            alerts.Add(new WorldAlert(
                "meals_low",
                WorldAlertSeverity.Warning,
                "Meals are running low",
                $"Only {meals} prepared meal(s) remain.",
                "schedule"));
        }

        if (supplies <= 0)
        {
            alerts.Add(new WorldAlert(
                "supplies_empty",
                WorldAlertSeverity.Warning,
                "Supplies depleted",
                "Ranch supplies are at zero. Visit the General Store or assign a supply-producing job.",
                "shop"));
        }
        else if (supplies == 1)
        {
            alerts.Add(new WorldAlert(
                "supplies_low",
                WorldAlertSeverity.Info,
                "Supplies are low",
                "Only one supply unit remains.",
                "shop"));
        }
    }

    private static void AddRosterAlerts(GameRoot game, List<WorldAlert> alerts)
    {
        var criticalFatigue = 0;
        var criticalEnergy = 0;
        var criticalHp = 0;
        var lowMorale = 0;

        foreach (var character in game.Roster.Characters)
        {
            var definition = game.Roster.DefinitionFor(character);
            var maxHp = Math.Max(1, character.MaxHpOverride ?? definition.MaxHp);

            if (character.Fatigue >= 85)
            {
                criticalFatigue++;
            }
            if (character.Energy <= 10)
            {
                criticalEnergy++;
            }
            if (character.Hp * 4 <= maxHp)
            {
                criticalHp++;
            }
            if (character.Morale <= 20)
            {
                lowMorale++;
            }
        }

        if (criticalHp > 0)
        {
            alerts.Add(new WorldAlert(
                "roster_hp_critical",
                WorldAlertSeverity.Critical,
                "Injured ranch residents",
                $"{criticalHp} resident(s) are at or below 25% HP.",
                "roster"));
        }

        if (criticalEnergy > 0 || criticalFatigue > 0)
        {
            alerts.Add(new WorldAlert(
                "roster_exhausted",
                criticalEnergy > 0 ? WorldAlertSeverity.Critical : WorldAlertSeverity.Warning,
                "Residents need rest",
                $"{criticalEnergy} have critically low energy; {criticalFatigue} have very high fatigue.",
                "schedule"));
        }

        if (lowMorale > 0)
        {
            alerts.Add(new WorldAlert(
                "roster_morale_low",
                WorldAlertSeverity.Warning,
                "Morale problems",
                $"{lowMorale} resident(s) have morale at or below 20.",
                "bond"));
        }
    }

    private static void AddPetAlerts(GameRoot game, List<WorldAlert> alerts)
    {
        var hungry = game.State.Pets.Entries.Values.Count(entry => entry.Hunger <= 20);
        var gettingHungry = game.State.Pets.Entries.Values.Count(entry => entry.Hunger is > 20 and <= 35);

        if (hungry > 0)
        {
            alerts.Add(new WorldAlert(
                "pets_hungry",
                WorldAlertSeverity.Critical,
                "Pet needs food",
                $"{hungry} adopted pet(s) have hunger at or below 20.",
                "pets"));
        }
        else if (gettingHungry > 0)
        {
            alerts.Add(new WorldAlert(
                "pets_getting_hungry",
                WorldAlertSeverity.Warning,
                "Pet getting hungry",
                $"{gettingHungry} adopted pet(s) are getting hungry.",
                "pets"));
        }
    }

    private static void AddEconomyAlerts(GameRoot game, List<WorldAlert> alerts)
    {
        var upkeep = game.Ranch.FacilityUpkeep();
        var gold = game.State.Economy.Gold;
        if (upkeep > 0 && gold < upkeep)
        {
            alerts.Add(new WorldAlert(
                "upkeep_risk",
                WorldAlertSeverity.Critical,
                "Cannot cover facility upkeep",
                $"Current gold: {gold}g; facility upkeep: {upkeep}g.",
                "ranch"));
        }
        else if (upkeep > 0 && gold < upkeep * 2)
        {
            alerts.Add(new WorldAlert(
                "upkeep_low_buffer",
                WorldAlertSeverity.Warning,
                "Low gold buffer",
                $"Only {gold}g available against {upkeep}g facility upkeep.",
                "ranch"));
        }
    }

    private static int CountJob(GameRoot game, string jobId)
    {
        return game.Roster.Characters.Count(character =>
            string.Equals(game.Schedule.GetAssignment(character.Id), jobId, StringComparison.OrdinalIgnoreCase));
    }
}
