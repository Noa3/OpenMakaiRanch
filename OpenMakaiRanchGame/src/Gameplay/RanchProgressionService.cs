using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Gameplay;

public sealed record RanchAchievement(string Id, string Title, string Requirement, int CompletedDay)
{
    public bool Complete => CompletedDay > 0;
}

/// <summary>Optional, run-local recognition. No currency, production multipliers, deadlines or win gates.</summary>
public sealed class RanchProgressionService(SaveState state, FlagService flags)
{
    public const int LastObservedDayFlag = 1_230_500;
    private static readonly string[] Ids = { "restored_home", "home_cooking", "balanced_shift", "small_crew", "time_to_breathe" };
    private static readonly string[] Titles = { "A home worth returning to", "From our own kitchen", "A balanced shift", "Small crew, steady ranch", "Room for a day off" };
    private static readonly string[] Requirements = {
        "Finish the four starting ranch projects at your own pace.",
        "Feed at least two workers entirely from ranch meals and cook enough to cover them again.",
        "Produce meals, supplies and farm goods in one profitable, fully fed shift with a well-rested team.",
        "With at most three residents, cover operating costs from work alone while everybody working is fed and the team is well.",
        "Give at least one resident a day off while a fed, well team covers operating costs from work alone."
    };

    public IReadOnlyList<RanchAchievement> Inspect() => Array.AsReadOnly(Ids.Select((id, index) => new RanchAchievement(
        id, T("gameplay.achievement." + id + ".title", Titles[index]),
        T("gameplay.achievement." + id + ".requirement", Requirements[index]),
        Math.Max(0, flags.GetGlobalIntFlag(LastObservedDayFlag + index + 1)))).ToArray());

    internal void ObserveSettlement(SettledWorkFacts facts, DailyReport report)
    {
        // Only the actual day boundary commits receipts. Queries never grant anything.
        if (report.Day < 1 || facts.Day != report.Day || (long)state.Calendar.Day != (long)report.Day + 1
            || flags.GetGlobalIntFlag(LastObservedDayFlag) >= report.Day) return;
        flags.SetGlobalIntFlag(LastObservedDayFlag, report.Day);
        var fed = facts.Workers > 0 && facts.Lunch.MissingMeals == 0;
        var sound = fed && facts.TeamWell && facts.AutomaticallyRested == 0 && facts.OperatingNet >= 0;
        var criteria = new[] {
            RanchProjectService.Evaluate(state, flags).All(project => project.Complete),
            facts.Workers >= 2 && facts.Lunch.PantryMeals == facts.Workers && facts.Lunch.MealBoxes == 0
                && facts.AutomaticallyRested == 0 && facts.Units("meals") >= facts.Workers && facts.TeamWell,
            sound && facts.Units("meals") > 0 && facts.Units("supplies") > 0 && facts.Units("farm_goods") > 0,
            sound && facts.Residents is >= 1 and <= 3,
            sound && facts.RestingResidents > 0
        };
        for (var i = 0; i < Ids.Length; i++)
        {
            var flag = LastObservedDayFlag + i + 1;
            if (!criteria[i] || flags.GetGlobalIntFlag(flag) > 0) continue;
            flags.SetGlobalIntFlag(flag, report.Day);
            report.Events.Add(new DailyEvent {
                Title = T("gameplay.achievement." + Ids[i] + ".title", Titles[i]),
                Description = T("gameplay.achievement.recorded", "Achieved on day {0}. Your ranch continues; this optional record adds no chores or stat bonuses.", report.Day),
                IsPositive = true
            });
        }
    }
}
