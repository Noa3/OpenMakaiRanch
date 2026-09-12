using System;
using OpenMakaiRanch.Core.Resources;

namespace OpenMakaiRanch.Gameplay;

public sealed record FacilityWorkBenefit(string FacilityId, int Level, int UsefulLevel, int ExtraUnits);

public sealed partial class RanchService
{
    public const int MaximumProductiveFacilityLevel = 5;

    /// <summary>Bounded extra stock from a staffed building, not extra wages or another worker.</summary>
    public FacilityWorkBenefit InspectWorkBenefit(JobDefinition job)
    {
        ArgumentNullException.ThrowIfNull(job);
        var id = job.Id switch
        {
            "dairy" => "dairy_barn",
            "pasture" => "pasture",
            "kitchen" or "cooking" => "kitchen",
            "workshop" => "workshop",
            "pharmacy" => "pharmacy_lab",
            "customer_service" => "guest_room",
            "cleaning" => "bathhouse",
            _ => ""
        };
        _state.Ranch.Facilities.TryGetValue(id, out var level);
        level = Math.Max(0, level);
        var useful = Math.Min(MaximumProductiveFacilityLevel, level);
        var amount = 0;
        if (id.Length > 0 && _data.Jobs.ContainsKey(job.Id)
            && _data.Facilities.TryGetValue(id, out var facility)
            && !string.IsNullOrEmpty(job.ResourceId) && facility.OutputResourceId == job.ResourceId)
            amount = (int)Math.Min(int.MaxValue, (long)Math.Max(0, useful - 1) * Math.Max(0, facility.OutputBonus));
        return new(id, level, useful, amount);
    }
}
