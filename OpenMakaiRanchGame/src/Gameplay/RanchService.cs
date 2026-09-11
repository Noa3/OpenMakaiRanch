using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

public sealed partial class RanchService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private readonly EquipmentService _equipment;
    private readonly TalentService _talents;

    public RanchService(SaveState state, DataRegistry data, EquipmentService equipment, TalentService talents)
    {
        _state = state;
        _data = data;
        _equipment = equipment;
        _talents = talents;
    }

    public IReadOnlyDictionary<string, int> Stockpile => _state.Ranch.Stockpile;
    public IReadOnlyDictionary<string, int> Facilities => _state.Ranch.Facilities;

    public void ApplyAutomation(DailyReport report)
    {
        if (!_state.Research.UnlockedSkillIds.Contains("ranch_automation"))
            return;

        int totalBonus = 0;
        foreach (var facility in _state.Ranch.Facilities)
        {
            if (facility.Value <= 0) continue;
            int bonus = facility.Value * 2;
            _state.Ranch.Stockpile["farm_goods"] = _state.Ranch.Stockpile.GetValueOrDefault("farm_goods") + bonus;
            totalBonus += bonus;
        }
        if (totalBonus > 0)
            report.Lines.Add($"Automated Feeding produced {totalBonus} bonus farm goods from facilities.");
    }

    public int ApplyJobOutput(CharacterState character, JobDefinition job, DailyReport report)
    {
        if (string.IsNullOrWhiteSpace(job.ResourceId))
        {
            report.Lines.Add($"{character.Id} rested and recovered.");
            return 0;
        }

        if (character.Mature.FallState == FallState.Collapse)
        {
            report.Lines.Add($"{character.Id} is collapsed and unable to work.");
            return 0;
        }

        var forecast = PreviewJobOutput(character, job);
        var amount = forecast.Amount;
        var gold = forecast.Gold;
        if (forecast.FatigueLost > 0)
            report.Lines.Add($"{character.DisplayNameOverride}'s fatigue reduced output by {forecast.FatigueLost} {job.ResourceId}.");
        if (_state.Research.UnlockedSkillIds.Contains("dairy_science") && job.Category == JobCategory.Dairy)
            report.Lines.Add($"Dairy Science: +{forecast.DairyBonus} bonus farm goods from improved techniques.");
        if (_state.Research.UnlockedSkillIds.Contains("culinary_arts") && job.Category == JobCategory.Cooking)
            report.Lines.Add($"Culinary Arts: produced {forecast.CulinaryBonus} extra meals from expert cooking.");
        if (_state.Research.UnlockedSkillIds.Contains("herbalism") && job.Category == JobCategory.Pharmacy)
            report.Lines.Add($"Herbalism: +{forecast.HerbalBonus} bonus supplies from herbal remedies.");
        if (_state.Research.UnlockedSkillIds.Contains("hospitality") && job.Category == JobCategory.CustomerService)
            report.Lines.Add($"Hospitality: superior service earned extra comfort ({forecast.HospitalityBonus}) and tips (+15g).");
        if (_state.Research.UnlockedSkillIds.Contains("craftsmanship") && job.Category == JobCategory.Chore)
            report.Lines.Add($"Craftsmanship: +{forecast.CraftBonus} bonus output from skilled workshop work.");

        if (string.Equals(job.Id, "cleaning", StringComparison.OrdinalIgnoreCase) && !_state.Ranch.BathtubClean)
        {
            _state.Ranch.BathtubClean = true;
            report.Lines.Add($"{character.DisplayNameOverride} cleaned and prepared the ranch bath.");
        }

        _state.Ranch.Stockpile.TryGetValue(job.ResourceId, out var currentAmount);
        _state.Ranch.Stockpile[job.ResourceId] = currentAmount + amount;
        var displayName = !string.IsNullOrWhiteSpace(character.DisplayNameOverride) ? character.DisplayNameOverride : character.Id;
        report.Lines.Add($"{displayName} completed {job.DisplayName}, adding {amount} {job.ResourceId}.");
        return gold;
    }
}
