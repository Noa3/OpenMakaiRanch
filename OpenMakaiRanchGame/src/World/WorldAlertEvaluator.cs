using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;

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

        AddCalendarAlerts(game, alerts);
        AddCattleAlerts(game, alerts);
        AddStockpileAlerts(game, alerts);
        AddRosterAlerts(game, alerts);
        AddPetAlerts(game, alerts);
        AddRanchConditionAlerts(game, alerts);
        AddEconomyAlerts(game, alerts);

        if (state.Calendar.Phase == DayPhase.Night
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

    private static void AddCalendarAlerts(GameRoot game, List<WorldAlert> alerts)
    {
        var calendar = game.State.Calendar;

        if (calendar.IsSeasonEnd)
        {
            var nextSeason = calendar.Season == Season.Winter
                ? Season.Spring
                : (Season)((int)calendar.Season + 1);
            var nextYear = calendar.Season == Season.Winter ? calendar.Year + 1 : calendar.Year;
            alerts.Add(new WorldAlert(
                "season_transition_tomorrow",
                WorldAlertSeverity.Info,
                "Season changes tomorrow",
                $"Today is {calendar.Season} {calendar.DayOfSeason}. Tomorrow begins {nextSeason}, Year {nextYear}. The world dressing and seasonal weather table will change with it.",
                "ranch"));
        }
        else if (calendar.DayOfSeason >= CalendarState.DaysPerSeason - 2)
        {
            alerts.Add(new WorldAlert(
                "season_transition_soon",
                WorldAlertSeverity.Info,
                "Season change approaching",
                $"{CalendarState.DaysPerSeason - calendar.DayOfSeason} day(s) remain in {calendar.Season}.",
                "ranch"));
        }

        var tomorrow = calendar.TomorrowWeather;
        if (tomorrow == Weather.Blizzard)
        {
            alerts.Add(new WorldAlert(
                "forecast_blizzard",
                WorldAlertSeverity.Warning,
                "Blizzard forecast tomorrow",
                "Expect heavy snow, strong wind and reduced visibility. Finish important outdoor planning before the day ends.",
                "schedule"));
        }
        else if (tomorrow is Weather.TorrentialRain or Weather.Storm)
        {
            alerts.Add(new WorldAlert(
                "forecast_storm",
                WorldAlertSeverity.Warning,
                "Severe rain forecast tomorrow",
                "Tomorrow's forecast calls for severe rain. Review staffing and supplies before settling the day.",
                "schedule"));
        }
        else if (tomorrow is Weather.HeavySnow or Weather.HeavyRain or Weather.StrongWind)
        {
            alerts.Add(new WorldAlert(
                "forecast_rough_weather",
                WorldAlertSeverity.Info,
                "Rough weather tomorrow",
                $"Forecast: {tomorrow}. The world presentation will become more severe next morning.",
                "ranch"));
        }
        else if (OriginalCalendarRules.IsSnow(tomorrow) && calendar.Season == Season.Winter)
        {
            alerts.Add(new WorldAlert(
                "forecast_snow",
                WorldAlertSeverity.Info,
                "Snow forecast tomorrow",
                $"Forecast: {tomorrow}. Winter ground and particle presentation will react after rollover.",
                "ranch"));
        }
    }

    private static void AddCattleAlerts(GameRoot game, List<WorldAlert> alerts)
    {
        var health = game.State.Ranch.CattleHealth;
        var pastureWorkers = CountJob(game, "pasture");
        var dairyWorkers = CountJob(game, "dairy");

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

        if (dairyWorkers == 0)
        {
            alerts.Add(new WorldAlert(
                "dairy_unstaffed",
                health <= 50 ? WorldAlertSeverity.Critical : WorldAlertSeverity.Warning,
                "No one assigned to Dairy Work",
                "Daily settlement applies +15g maintenance and a morale penalty when Dairy Work has no assigned resident.",
                "schedule"));
        }

        if (pastureWorkers == 0)
        {
            alerts.Add(new WorldAlert(
                "pasture_unstaffed",
                WorldAlertSeverity.Info,
                "Pasture is unstaffed",
                "Nobody is assigned to Pasture Work. This mainly means no pasture output for the day.",
                "schedule"));
        }
    }

    private static void AddStockpileAlerts(GameRoot game, List<WorldAlert> alerts)
    {
        var stockpile = game.State.Ranch.Stockpile;
        var meals = stockpile.GetValueOrDefault("meals");
        var supplies = stockpile.GetValueOrDefault("supplies");
        var mealBoxes = game.State.Inventory.Items.GetValueOrDefault("meal_box");

        if (meals <= 0)
        {
            alerts.Add(new WorldAlert(
                "meals_empty",
                WorldAlertSeverity.Info,
                "No meal output stockpiled",
                "The meals production stockpile is empty. This is not automatic starvation, but Kitchen/Cooking will produce more.",
                "schedule"));
        }

        if (mealBoxes <= 0 && game.Roster.Characters.Any(character => character.Fatigue >= 70 || character.Energy <= 20))
        {
            alerts.Add(new WorldAlert(
                "meal_box_missing",
                WorldAlertSeverity.Warning,
                "No meal boxes for tired residents",
                "Care feeding uses meal_box items, but none are in inventory while residents need recovery.",
                "shop"));
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

    private static void AddRanchConditionAlerts(GameRoot game, List<WorldAlert> alerts)
    {
        if (game.State.Ranch.Workload >= 80)
        {
            alerts.Add(new WorldAlert(
                "workload_high",
                WorldAlertSeverity.Warning,
                "Ranch workload is very high",
                $"Workload is {game.State.Ranch.Workload}/100. Night Admin can reduce it.",
                "ranch"));
        }

        if (!game.State.Ranch.BathtubClean)
        {
            alerts.Add(new WorldAlert(
                "bath_dirty",
                WorldAlertSeverity.Info,
                "Bath needs cleaning",
                "The ranch bath is currently marked dirty.",
                "ranch"));
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
