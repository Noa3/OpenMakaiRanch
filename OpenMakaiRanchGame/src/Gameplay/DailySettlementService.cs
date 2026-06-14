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

        _resources.ConsumeResources(report);

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

        var expenses = _ranch.FacilityUpkeep() + PetCareCost();
        _economy.ApplySettlement(income, expenses);
        report.Income = income;
        report.Expenses = expenses;
        report.NetGold = income - expenses;
        report.Lines.Add($"Facility upkeep cost {expenses} gold.");

        _ranch.ApplyAutomation(report);

        int milkRevenue = 0;
        foreach (var character in _state.Roster.Characters)
        {
            _milkEconomy.ProduceMilk(character.Id);
            milkRevenue += _milkEconomy.ShipMilk(character.Id);
        }
        if (milkRevenue > 0)
        {
            report.MilkRevenue = milkRevenue;
            report.NetGold += milkRevenue;
            report.Lines.Add($"Auto-shipped milk for {milkRevenue} gold.");
        }

        _events.GenerateEvents(report);
        _growth.ApplyGrowth(report);
        _milestones.CheckAfterSettlement(report);
        _dayCycle.AdvanceToNextDay();

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

    private int PetCareCost()
    {
        return _state.Pets.AdoptedPetIds.Sum(petId => _data.Pets.TryGetValue(petId, out var pet) ? pet.CareCost : 0);
    }
}
