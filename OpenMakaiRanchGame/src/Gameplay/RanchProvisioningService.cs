using System;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Data;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Gameplay;

public sealed record LunchPlan(int Workers, int PantryMeals, int MealBoxes, int MissingMeals);

/// <summary>One existing workday lunch, using ranch food before portable items. Never creates food.</summary>
public sealed class RanchProvisioningService(SaveState state, DataRegistry data)
{
    // Production happens later in settlement: today's cooking supplies tomorrow's lunch.
    // Resting residents keep the existing no-lunch-cost rule.
    public LunchPlan InspectLunch()
    {
        var workers = state.Roster.Characters.Count(c =>
            state.Schedule.AssignedJobs.TryGetValue(c.Id, out var job) && job != "rest");
        state.Ranch.Stockpile.TryGetValue("meals", out var pantry);
        state.Inventory.Items.TryGetValue("meal_box", out var boxes);
        var home = Math.Min(workers, Math.Max(0, pantry));
        var portable = Math.Min(workers - home, Math.Max(0, boxes));
        return new(workers, home, portable, workers - home - portable);
    }

    internal LunchPlan ConsumeLunch(DailyReport report)
    {
        var plan = InspectLunch();
        var home = plan.PantryMeals;
        var boxes = plan.MealBoxes;
        foreach (var c in state.Roster.Characters)
        {
            if (!state.Schedule.AssignedJobs.TryGetValue(c.Id, out var job) || job == "rest") continue;
            if (home > 0 || boxes > 0)
            {
                if (home > 0) { state.Ranch.Stockpile["meals"]--; home--; }
                else { state.Inventory.Items["meal_box"]--; boxes--; }
                var capacity = Math.Max(0, c.MaxEnergyOverride ??
                    (data.Characters.TryGetValue(c.DefinitionId ?? c.Id, out var definition) ? definition.MaxEnergy : 100));
                if (c.Energy < capacity) c.Energy = (int)Math.Min(capacity, (long)c.Energy + 15);
                c.Morale = (int)Math.Clamp((long)c.Morale + 3, 0, 100);
            }
            else c.Morale = (int)Math.Clamp((long)c.Morale - 2, 0, 100);
        }
        if (plan.Workers > 0)
            report.Lines.Add(T("gameplay.food.lunch", "Lunch: {0} ranch meals, {1} meal boxes; {2} workers without a meal.",
                plan.PantryMeals, plan.MealBoxes, plan.MissingMeals));
        return plan;
    }
}
