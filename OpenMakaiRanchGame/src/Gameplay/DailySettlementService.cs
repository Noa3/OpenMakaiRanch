using System;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

public sealed class DailySettlementService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private readonly ScheduleService _schedule;
    private readonly RanchService _ranch;
    private readonly EconomyService _economy;
    private readonly DayCycleService _dayCycle;
    private readonly MilestoneService _milestones;
    private readonly DailyEventService _events;
    private readonly CharacterGrowthService _growth;
    private readonly ResourceConsumptionService _resources;
    private readonly InventoryService _inventory;
    private readonly MilkEconomyService _milkEconomy;
    private readonly TalentService _talents;

    public DailySettlementService(SaveState state, DataRegistry data, ScheduleService schedule, RanchService ranch, EconomyService economy, DayCycleService dayCycle, MilestoneService milestones, InventoryService inventory, TalentService talents)
    {
        _state = state;
        _data = data;
        _schedule = schedule;
        _ranch = ranch;
        _economy = economy;
        _dayCycle = dayCycle;
        _milestones = milestones;
        _events = new DailyEventService(state, data, economy);
        _growth = new CharacterGrowthService(state, talents);
        _resources = new ResourceConsumptionService(state, data);
        _inventory = inventory;
        _milkEconomy = new MilkEconomyService(state);
        _talents = talents;
    }

    public DailyReport SettleDay()
    {
        var report = new DailyReport { Day = _state.Calendar.Day };
        var income = 0;
        var ledger = new DailyGoldLedger(_economy.Gold);
        // Clear once per day, not between night training and ordinary growth.
        foreach (var character in _state.Roster.Characters) character.HasGrownToday = false;

        _resources.ConsumeResources(report);
        ApplyNightAction(report);

        foreach (var character in _state.Roster.Characters)
        {
            var jobId = _schedule.GetAssignment(character.Id);
            var job = _data.Jobs.TryGetValue(jobId, out var foundJob) ? foundJob : _data.Job("rest");
            income += _ranch.ApplyJobOutput(character, job, report);
            var fatigueResistance = _talents.FatigueResistance(character.Id);
            var fatigueDelta = job.FatigueDelta >= 0
                ? Math.Max(0, job.FatigueDelta - fatigueResistance)
                : job.FatigueDelta - fatigueResistance;
            character.Fatigue = Math.Clamp(character.Fatigue + fatigueDelta, 0, 100);
            character.Morale = Math.Clamp(character.Morale + job.MoraleDelta, 0, 100);
            character.Bond = Math.Clamp(character.Bond + job.BondDelta, 0, 100);
        }

        var expenses = (int)Math.Min(int.MaxValue, (long)_ranch.FacilityUpkeep() + PetCareCost());

        // Original-game rule: at least one slave must be assigned to Dairy
        // to keep the farm maintained. Without it the herd degrades and upkeep costs more.
        var hasDairyWorker = _state.Roster.Characters.Any(character => _schedule.GetAssignment(character.Id) == "dairy");
        if (!hasDairyWorker)
        {
            expenses = (int)Math.Min(int.MaxValue, (long)expenses + 15);
            report.Lines.Add("No one was assigned to Dairy work. Farm maintenance suffers (+15g upkeep).");
            foreach (var character in _state.Roster.Characters)
            {
                character.Morale = Math.Clamp(character.Morale - 1, 0, 100);
            }
        }

        _economy.ApplySettlement(income, expenses);
        ledger.RecordWorkAndUpkeep(income, expenses, _economy.Gold);
        report.Income = income;
        report.Expenses = expenses;
        report.NetGold = income - expenses;
        report.Lines.Add($"Facility upkeep cost {expenses} gold.");

        _ranch.ApplyAutomation(report);

        var beforeShipping = _economy.Gold;
        foreach (var character in _state.Roster.Characters)
        {
            _milkEconomy.ProduceMilk(character.Id);
            _milkEconomy.ShipMilk(character.Id);
        }
        var milkRevenue = ledger.RecordChange(beforeShipping, _economy.Gold);
        if (milkRevenue > 0)
        {
            report.MilkRevenue = milkRevenue;
            report.NetGold += milkRevenue;
            report.Lines.Add($"Auto-shipped milk for {milkRevenue} gold.");
        }

        var beforeEvents = _economy.Gold;
        _events.GenerateEvents(report);
        ledger.RecordChange(beforeEvents, _economy.Gold);
        _growth.ApplyGrowth(report);
        var beforeMilestones = _economy.Gold;
        _milestones.CheckAfterSettlement(report);
        ledger.RecordChange(beforeMilestones, _economy.Gold);
        ledger.Complete(report, _state.Economy);
        _dayCycle.AdvanceToNextDay();

        // Original ANNIVERSARY_MESSAGE parity: announce the first day of a new season/year after
        // DATE_CALC/weather rollover. This modernizes presentation without changing settlement.
        if (_state.Calendar.IsSeasonStart)
        {
            var seasonLine = _state.Calendar.Season == Season.Spring
                ? $"Year {_state.Calendar.Year} begins. Spring has come to the ranch."
                : $"The season changes. {_state.Calendar.Season} begins.";
            report.Lines.Add(seasonLine);
        }

        var discovered = _state.Adventure.DiscoveredMissionIds.Count;
        var total = _data.Missions.Count;
        if (discovered < total && _state.Calendar.Day % 3 == 0)
        {
            var next = _data.Missions.Values
                .Where(m => !_state.Adventure.DiscoveredMissionIds.Contains(m.Id))
                .OrderBy(m => m.Difficulty)
                .FirstOrDefault();
            if (next is not null)
            {
                _state.Adventure.DiscoveredMissionIds.Add(next.Id);
                report.Lines.Add($"Scouted a new mission location: {next.DisplayName}");
            }
        }

        _state.Adventure.AvailableMercenaries.Clear();
        _state.Adventure.ActiveMercenaryHpBonus = 0;

        return report;
    }

    /// <summary>
    /// Original-game night phase: players pick one nightly workload —
    /// rest, training, or administrative duties — before the day settles.
    /// </summary>
    private void ApplyNightAction(DailyReport report)
    {
        var action = _state.Calendar.NightAction;
        switch (action)
        {
            case "train":
            {
                // One extra ranch-wide pass, independent of resident count.
                foreach (var character in _state.Roster.Characters)
                {
                    character.Bond = Math.Clamp(character.Bond + 1, 0, 100);
                    character.Morale = Math.Clamp(character.Morale + 2, 0, 100);

                }

                _growth.ApplyGrowth(report);
                report.Lines.Add("Night training: one extra growth pass for working residents; bond +1 and morale +2.");
                break;
            }
            case "admin":
            {
                // Reduces workload, not upkeep or gold costs.
                _state.Ranch.Workload = Math.Max(0, _state.Ranch.Workload - 10);
                report.Lines.Add("Night administrative work: paperwork handled, workload reduced.");
                break;
            }
            case "rest":
            default:
            {
                foreach (var character in _state.Roster.Characters)
                {
                    character.Fatigue = Math.Clamp(character.Fatigue - 20, 0, 100);
                    character.Morale = Math.Clamp(character.Morale + 4, 0, 100);
                    character.Energy = Math.Clamp(character.Energy + 15, 0, (character.MaxEnergyOverride ?? 150) + 0);
                }

                report.Lines.Add("The ranch rested at night. Energy restored.");
                break;
            }
        }

        _state.Calendar.NightAction = string.Empty;
    }

    private int PetCareCost()
    {
        return (int)Math.Min(int.MaxValue, _state.Pets.AdoptedPetIds.Sum(petId =>
            _data.Pets.TryGetValue(petId, out var pet) ? Math.Max(0L, pet.CareCost) : 0L));
    }
}
