using System;
using static OpenMakaiRanch.Locale.LocaleCatalog;
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
                T("world.alert.night_plan_missing.title", "Night plan missing"),
                T("world.alert.night_plan_missing.detail", "Choose Rest, Train or Admin before ending the day."),
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
                T("world.alert.season_transition_tomorrow.title", "Season changes tomorrow"),
                T("world.alert.season_transition_tomorrow.detail", "Today is {0} {1}. Tomorrow begins {2}, Year {3}. The world dressing and seasonal weather table will change with it.", EnumDisplayName(calendar.Season), calendar.DayOfSeason, EnumDisplayName(nextSeason), nextYear),
                "ranch"));
        }
        else if (calendar.DayOfSeason >= CalendarState.DaysPerSeason - 2)
        {
            alerts.Add(new WorldAlert(
                "season_transition_soon",
                WorldAlertSeverity.Info,
                T("world.alert.season_transition_soon.title", "Season change approaching"),
                T("world.alert.season_transition_soon.detail", "{0} day(s) remain in {1}.", CalendarState.DaysPerSeason - calendar.DayOfSeason, EnumDisplayName(calendar.Season)),
                "ranch"));
        }

        var tomorrow = calendar.TomorrowWeather;
        if (tomorrow == Weather.Blizzard)
        {
            alerts.Add(new WorldAlert(
                "forecast_blizzard",
                WorldAlertSeverity.Warning,
                T("world.alert.forecast_blizzard.title", "Blizzard forecast tomorrow"),
                T("world.alert.forecast_blizzard.detail", "Expect heavy snow, strong wind and reduced visibility. Finish important outdoor planning before the day ends."),
                "schedule"));
        }
        else if (tomorrow is Weather.TorrentialRain or Weather.Storm)
        {
            alerts.Add(new WorldAlert(
                "forecast_storm",
                WorldAlertSeverity.Warning,
                T("world.alert.forecast_storm.title", "Severe rain forecast tomorrow"),
                T("world.alert.forecast_storm.detail", "Tomorrow's forecast calls for severe rain. Review staffing and supplies before settling the day."),
                "schedule"));
        }
        else if (tomorrow is Weather.HeavySnow or Weather.HeavyRain or Weather.StrongWind)
        {
            alerts.Add(new WorldAlert(
                "forecast_rough_weather",
                WorldAlertSeverity.Info,
                T("world.alert.forecast_rough_weather.title", "Rough weather tomorrow"),
                T("world.alert.forecast_rough_weather.detail", "Forecast: {0}. The world presentation will become more severe next morning.", EnumDisplayName(tomorrow)),
                "ranch"));
        }
        else if (OriginalCalendarRules.IsSnow(tomorrow) && calendar.Season == Season.Winter)
        {
            alerts.Add(new WorldAlert(
                "forecast_snow",
                WorldAlertSeverity.Info,
                T("world.alert.forecast_snow.title", "Snow forecast tomorrow"),
                T("world.alert.forecast_snow.detail", "Forecast: {0}. Winter ground and particle presentation will react after rollover.", EnumDisplayName(tomorrow)),
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
                T("world.alert.cattle_health_critical.title", "Cattle health critical"),
                T("world.alert.cattle_health_critical.detail", "Cattle health is {0}/100. Assign ranch care and review supplies before advancing the day.", health),
                "schedule"));
        }
        else if (health <= 50)
        {
            alerts.Add(new WorldAlert(
                "cattle_health_low",
                WorldAlertSeverity.Warning,
                T("world.alert.cattle_health_low.title", "Cattle health is low"),
                T("world.alert.cattle_health_low.detail", "Cattle health is {0}/100. Check ranch staffing and resources.", health),
                "schedule"));
        }

        if (dairyWorkers == 0)
        {
            alerts.Add(new WorldAlert(
                "dairy_unstaffed",
                health <= 50 ? WorldAlertSeverity.Critical : WorldAlertSeverity.Warning,
                T("world.alert.dairy_unstaffed.title", "No one assigned to Dairy Work"),
                T("world.alert.dairy_unstaffed.detail", "Daily settlement applies +15g maintenance and a morale penalty when Dairy Work has no assigned resident."),
                "schedule"));
        }

        if (pastureWorkers == 0)
        {
            alerts.Add(new WorldAlert(
                "pasture_unstaffed",
                WorldAlertSeverity.Info,
                T("world.alert.pasture_unstaffed.title", "Pasture is unstaffed"),
                T("world.alert.pasture_unstaffed.detail", "Nobody is assigned to Pasture Work. This mainly means no pasture output for the day."),
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
                T("world.alert.meals_empty.title", "No meal output stockpiled"),
                T("world.alert.meals_empty.detail", "The meals production stockpile is empty. This is not automatic starvation, but Kitchen/Cooking will produce more."),
                "schedule"));
        }

        if (mealBoxes <= 0 && game.Roster.Characters.Any(character => character.Fatigue >= 70 || character.Energy <= 20))
        {
            alerts.Add(new WorldAlert(
                "meal_box_missing",
                WorldAlertSeverity.Warning,
                T("world.alert.meal_box_missing.title", "No meal boxes for tired residents"),
                T("world.alert.meal_box_missing.detail", "Care feeding uses meal_box items, but none are in inventory while residents need recovery."),
                "shop"));
        }

        if (supplies <= 0)
        {
            alerts.Add(new WorldAlert(
                "supplies_empty",
                WorldAlertSeverity.Warning,
                T("world.alert.supplies_empty.title", "Supplies depleted"),
                T("world.alert.supplies_empty.detail", "Ranch supplies are at zero. Visit the General Store or assign a supply-producing job."),
                "shop"));
        }
        else if (supplies == 1)
        {
            alerts.Add(new WorldAlert(
                "supplies_low",
                WorldAlertSeverity.Info,
                T("world.alert.supplies_low.title", "Supplies are low"),
                T("world.alert.supplies_low.detail", "Only one supply unit remains."),
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
                T("world.alert.roster_hp_critical.title", "Injured ranch residents"),
                T("world.alert.roster_hp_critical.detail", "{0} resident(s) are at or below 25% HP.", criticalHp),
                "roster"));
        }

        if (criticalEnergy > 0 || criticalFatigue > 0)
        {
            alerts.Add(new WorldAlert(
                "roster_exhausted",
                criticalEnergy > 0 ? WorldAlertSeverity.Critical : WorldAlertSeverity.Warning,
                T("world.alert.roster_exhausted.title", "Residents need rest"),
                T("world.alert.roster_exhausted.detail", "{0} have critically low energy; {1} have very high fatigue.", criticalEnergy, criticalFatigue),
                "schedule"));
        }

        if (lowMorale > 0)
        {
            alerts.Add(new WorldAlert(
                "roster_morale_low",
                WorldAlertSeverity.Warning,
                T("world.alert.roster_morale_low.title", "Morale problems"),
                T("world.alert.roster_morale_low.detail", "{0} resident(s) have morale at or below 20.", lowMorale),
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
                T("world.alert.pets_hungry.title", "Pet needs food"),
                T("world.alert.pets_hungry.detail", "{0} adopted pet(s) have hunger at or below 20.", hungry),
                "pets"));
        }
        else if (gettingHungry > 0)
        {
            alerts.Add(new WorldAlert(
                "pets_getting_hungry",
                WorldAlertSeverity.Warning,
                T("world.alert.pets_getting_hungry.title", "Pet getting hungry"),
                T("world.alert.pets_getting_hungry.detail", "{0} adopted pet(s) are getting hungry.", gettingHungry),
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
                T("world.alert.workload_high.title", "Ranch workload is very high"),
                T("world.alert.workload_high.detail", "Workload is {0}/100. Night Admin can reduce it.", game.State.Ranch.Workload),
                "ranch"));
        }

        if (!game.State.Ranch.BathtubClean)
        {
            alerts.Add(new WorldAlert(
                "bath_dirty",
                WorldAlertSeverity.Info,
                T("world.alert.bath_dirty.title", "Bath needs cleaning"),
                T("world.alert.bath_dirty.detail", "The ranch bath is currently marked dirty."),
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
                T("world.alert.upkeep_risk.title", "Cannot cover facility upkeep"),
                T("world.alert.upkeep_risk.detail", "Current gold: {0}g; facility upkeep: {1}g.", gold, upkeep),
                "ranch"));
        }
        else if (upkeep > 0 && gold < upkeep * 2)
        {
            alerts.Add(new WorldAlert(
                "upkeep_low_buffer",
                WorldAlertSeverity.Warning,
                T("world.alert.upkeep_low_buffer.title", "Low gold buffer"),
                T("world.alert.upkeep_low_buffer.detail", "Only {0}g available against {1}g facility upkeep.", gold, upkeep),
                "ranch"));
        }
    }

    private static int CountJob(GameRoot game, string jobId)
    {
        return game.Roster.Characters.Count(character =>
            string.Equals(game.Schedule.GetAssignment(character.Id), jobId, StringComparison.OrdinalIgnoreCase));
    }
}
