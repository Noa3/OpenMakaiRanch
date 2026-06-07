using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

public sealed class DailyEventService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private readonly EconomyService _economy;

    public DailyEventService(SaveState state, DataRegistry data, EconomyService economy)
    {
        _state = state;
        _data = data;
        _economy = economy;
    }

    public void GenerateEvents(DailyReport report)
    {
        int seed = _state.Calendar.Day * 31 + _state.Roster.Characters.Count * 7;
        var rng = new Random(seed);
        double roll = rng.NextDouble();

        // 40% chance of an event each day
        if (roll > 0.4) return;

        double eventRoll = rng.NextDouble();
        if (eventRoll < 0.35)
        {
            // Good event
            var goodEvents = new List<(string title, string desc, int goldMin, int goldMax, string item, int itemAmt)>
            {
                ("Traveling Merchant", "A traveling merchant passes through and offers a discount!", 10, 30, "meal_box", 1),
                ("Good Harvest", "The ranch yields an unexpected bounty today.", 15, 40, "feed_bundle", 2),
                ("Helping Hands", "A neighbor sends extra hands for the day!", 0, 0, "farm_goods", 3),
                ("Windfall", "You find a pouch of gold near the fence line.", 25, 50, "", 0),
                ("Lucky Day", "A stroke of luck boosts morale across the ranch!", 0, 0, "", 0),
            };
            var evt = goodEvents[rng.Next(goodEvents.Count)];
            int gold = evt.goldMin > 0 ? rng.Next(evt.goldMin, evt.goldMax + 1) : 0;
            if (gold > 0) _economy.AddGold(gold);
            if (!string.IsNullOrEmpty(evt.item))
                _state.Inventory.Items[evt.item] = _state.Inventory.Items.GetValueOrDefault(evt.item) + evt.itemAmt;
            if (evt.title == "Lucky Day")
                foreach (var c in _state.Roster.Characters) c.Morale = Math.Clamp(c.Morale + 8, 0, 100);

            report.Events.Add(new DailyEvent
            {
                Title = evt.title, Description = evt.desc, IsPositive = true,
                GoldDelta = gold, ItemId = evt.item, ItemAmount = evt.itemAmt
            });
        }
        else if (eventRoll < 0.65)
        {
            // Bad event
            var badEvents = new List<(string title, string desc, int goldCost, int fatigueAll, int moraleAll)>
            {
                ("Fence Broken", "A section of fence collapses. Ranch hands spend the morning repairing it.", 15, 5, -3),
                ("Supply Mold", "Damp weather ruins some of your stored supplies.", 0, 0, -5),
                ("Night Prowlers", "Wild creatures disturb the livestock. Everyone loses sleep.", 0, 10, -8),
                ("Tool Breakage", "A crucial tool breaks and needs replacement.", 20, 0, -2),
                ("Bad Weather", "Heavy rain makes everything harder today.", 0, 8, -5),
            };
            var evt = badEvents[rng.Next(badEvents.Count)];
            if (evt.goldCost > 0 && !_economy.Spend(evt.goldCost))
            {
                report.Events.Add(new DailyEvent { Title = evt.title, Description = $"${{evt.desc}} (could not afford repair)", IsPositive = false });
                return;
            }
            foreach (var c in _state.Roster.Characters)
            {
                c.Fatigue = Math.Clamp(c.Fatigue + evt.fatigueAll, 0, 100);
                c.Morale = Math.Clamp(c.Morale + evt.moraleAll, 0, 100);
            }
            report.Events.Add(new DailyEvent
            {
                Title = evt.title, Description = evt.desc, IsPositive = false,
                GoldDelta = -evt.goldCost
            });
        }
        else
        {
            // Neutral event
            var neutralEvents = new List<(string title, string desc, string item, int amt)>
            {
                ("Strange Tracks", "You find unusual tracks near the eastern pasture. Could be worth investigating.", "", 0),
                ("Rumor Mill", "Travelers share rumors of a new dungeon entrance to the north.", "", 0),
                ("Migrating Birds", "A flock of brilliant Makai birds passes overhead. A beautiful sight.", "", 0),
                ("Curious Pet", "Your pet brings you a small trinket they found.", "fabric_patch", 1),
            };
            var evt = neutralEvents[rng.Next(neutralEvents.Count)];
            if (!string.IsNullOrEmpty(evt.item))
                _state.Inventory.Items[evt.item] = _state.Inventory.Items.GetValueOrDefault(evt.item) + evt.amt;
            report.Events.Add(new DailyEvent
            {
                Title = evt.title, Description = evt.desc, IsPositive = true,
                ItemId = evt.item, ItemAmount = evt.amt
            });
        }
    }
}

public sealed class CharacterGrowthService
{
    private readonly SaveState _state;

    public CharacterGrowthService(SaveState state)
    {
        _state = state;
    }

    public void ApplyGrowth(DailyReport report)
    {
        foreach (var character in _state.Roster.Characters)
        {
            character.HasGrownToday = false;
            var jobId = _state.Schedule.AssignedJobs.GetValueOrDefault(character.Id) ?? "rest";
            if (jobId == "rest") continue;

            string skillKey = jobId switch
            {
                "pasture" or "kitchen" or "workshop" or "mentorship" => "ranch",
                "patrol" => "combat",
                _ => "ranch"
            };

            character.SkillXp ??= new Dictionary<string, int>();
            int xp = character.SkillXp.GetValueOrDefault(skillKey);
            int gain = 3 + (character.Fatigue < 50 ? 2 : 0);
            xp += gain;
            character.SkillXp[skillKey] = xp;

            int currentSkill = skillKey switch
            {
                "ranch" => character.RanchSkill,
                "craft" => character.CraftSkill,
                "combat" => character.CombatSkill,
                _ => 0
            };

            int threshold = currentSkill * 5 + 10;
            if (xp >= threshold && currentSkill < 10)
            {
                xp -= threshold;
                character.SkillXp[skillKey] = xp;

                switch (skillKey)
                {
                    case "ranch": character.RanchSkill = Math.Clamp(character.RanchSkill + 1, 1, 10); break;
                    case "craft": character.CraftSkill = Math.Clamp(character.CraftSkill + 1, 1, 10); break;
                    case "combat": character.CombatSkill = Math.Clamp(character.CombatSkill + 1, 1, 10); break;
                }
                character.HasGrownToday = true;
                report.CharacterGrowth.Add(new CharacterGrowthEntry
                {
                    CharacterId = character.Id,
                    DisplayName = character.DisplayNameOverride,
                    SkillGained = skillKey,
                    Amount = 1
                });
                report.SkillGains++;
            }
        }
    }
}

public sealed class ResourceConsumptionService
{
    private readonly SaveState _state;

    public ResourceConsumptionService(SaveState state)
    {
        _state = state;
    }

    public void ConsumeResources(DailyReport report)
    {
        // Each non-resting character consumes 1 meal if available -> morale bonus
        int mealsConsumed = 0;
        foreach (var character in _state.Roster.Characters)
        {
            var jobId = _state.Schedule.AssignedJobs.GetValueOrDefault(character.Id) ?? "rest";
            if (jobId == "rest") continue;

            if (_state.Inventory.Items.TryGetValue("meal_box", out var meals) && meals > 0)
            {
                _state.Inventory.Items["meal_box"] = meals - 1;
                mealsConsumed++;
                character.Morale = Math.Clamp(character.Morale + 3, 0, 100);
                character.Energy = Math.Min(character.Energy + 15, character.MaxEnergyOverride ?? 100);
            }
            else
            {
                character.Morale = Math.Clamp(character.Morale - 2, 0, 100);
            }
        }
        if (mealsConsumed > 0)
            report.Lines.Add($"Consumed {mealsConsumed} meal box(es) for lunch. Morale and energy improved.");

        // Supplies consumed by facilities
        int suppliesConsumed = 0;
        foreach (var facility in _state.Ranch.Facilities)
        {
            if (facility.Value <= 0) continue;
            if (_state.Ranch.Stockpile.TryGetValue("supplies", out var supplies) && supplies >= facility.Value)
            {
                _state.Ranch.Stockpile["supplies"] = supplies - facility.Value;
                suppliesConsumed += facility.Value;
            }
        }
        if (suppliesConsumed > 0)
            report.Lines.Add($"Consumed {suppliesConsumed} supplies for facility maintenance.");

        // Auto-rest for high fatigue characters
        int autoRested = 0;
        foreach (var character in _state.Roster.Characters)
        {
            if (character.Fatigue >= 70)
            {
                _state.Schedule.AssignedJobs[character.Id] = "rest";
                autoRested++;
                report.Lines.Add($"{character.DisplayNameOverride} was too exhausted and forced to rest.");
            }
        }
    }
}

public sealed class WinConditionService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;

    public WinConditionService(SaveState state, DataRegistry data)
    {
        _state = state;
        _data = data;
    }

    public bool IsGameComplete()
    {
        int totalMissions = _data.Missions.Count;
        int discovered = _state.Adventure.DiscoveredMissionIds.Count;

        int totalBonds = _data.BondEvents.Values
            .Select(e => e.CharacterId)
            .Distinct()
            .Count();
        int maxBonds = _state.Roster.Characters.Count(c => c.Bond >= 40);

        int facilitiesMaxed = _state.Ranch.Facilities.Count(f => f.Value >= 5);
        int totalFacilities = _data.Facilities.Count;

        int researches = _state.Research.UnlockedSkillIds.Count;
        int totalResearch = _data.Skills.Count;

        return discovered >= totalMissions
            && maxBonds >= Math.Min(totalBonds, _state.Roster.Characters.Count)
            && facilitiesMaxed >= totalFacilities
            && researches >= totalResearch;
    }

    public string ProgressSummary()
    {
        int missions = _state.Adventure.DiscoveredMissionIds.Count;
        int totalMissions = _data.Missions.Count;
        int bonds = _state.Roster.Characters.Count(c => c.Bond >= 40);
        int totalChars = _state.Roster.Characters.Count;
        int facMaxed = _state.Ranch.Facilities.Count(f => f.Value >= 5);
        int totalFac = _data.Facilities.Count;
        int research = _state.Research.UnlockedSkillIds.Count;
        int totalRes = _data.Skills.Count;

        return $"Missions: {missions}/{totalMissions} | Max Bonds: {bonds}/{totalChars} | Facilities: {facMaxed}/{totalFac} | Research: {research}/{totalRes}";
    }
}
