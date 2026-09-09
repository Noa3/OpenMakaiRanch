using System;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Gameplay;

/// <summary>
/// Calendar/weather rules traced from the original eraMakaiRanch sources:
/// CSV/Day.csv, CSV/Time.csv, ERB/表示関数/日付表示.ERB and
/// ERB/内部計算・ステータス増減/天気変更.ERB.
///
/// The remake keeps the original 7-day week, 28-day season, 112-day year and season-weighted
/// forecast behavior. Presentation may modernize how the result looks, but these values remain the
/// shared simulation source of truth.
/// </summary>
public static class OriginalCalendarRules
{
    public static Weather RollWeather(Season season, Random? random = null)
    {
        random ??= Random.Shared;
        var roll = random.Next(0, 100);

        return season switch
        {
            Season.Spring => roll switch
            {
                <= 59 => Weather.Cloudy,
                <= 74 => Weather.Drizzle,
                <= 79 => Weather.Rain,
                <= 82 => Weather.HeavyRain,
                <= 85 => Weather.StrongWind,
                _ => Weather.Clear
            },
            Season.Summer => roll switch
            {
                <= 39 => Weather.Cloudy,
                <= 44 => Weather.Drizzle,
                <= 54 => Weather.Rain,
                <= 74 => Weather.HeavyRain,
                <= 77 => Weather.StrongWind,
                <= 80 => Weather.TorrentialRain,
                _ => Weather.Clear
            },
            Season.Autumn => roll switch
            {
                <= 69 => Weather.Cloudy,
                <= 72 => Weather.Drizzle,
                <= 84 => Weather.Rain,
                <= 87 => Weather.HeavyRain,
                <= 89 => Weather.StrongWind,
                <= 91 => Weather.TorrentialRain,
                _ => Weather.Clear
            },
            Season.Winter => roll switch
            {
                <= 49 => Weather.Cloudy,
                <= 69 => Weather.Snow,
                <= 79 => Weather.HeavySnow,
                <= 84 => Weather.Blizzard,
                _ => Weather.Clear
            },
            _ => Weather.Clear
        };
    }

    /// <summary>
    /// In the original, the last day of every season forces tomorrow's forecast to overcast so the
    /// season transition starts from a controlled atmosphere.
    /// </summary>
    public static Weather RollTomorrow(CalendarState calendar, Random? random = null)
    {
        return calendar.IsSeasonEnd
            ? Weather.Cloudy
            : RollWeather(calendar.Season, random);
    }

    public static bool IsRain(Weather weather) => weather is
        Weather.Drizzle or Weather.Rain or Weather.HeavyRain or Weather.TorrentialRain or Weather.Storm;

    public static bool IsSnow(Weather weather) => weather is
        Weather.Snow or Weather.HeavySnow or Weather.Blizzard;

    public static bool IsStrongWind(Weather weather) => weather is
        Weather.StrongWind or Weather.Storm or Weather.Blizzard;

    public static bool IsSevere(Weather weather) => weather is
        Weather.HeavyRain or Weather.TorrentialRain or Weather.Storm or Weather.HeavySnow or Weather.Blizzard;
}
