using System;
using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// Spatial doorway/service marker in Okachi Town. Activating it only requests an existing UI screen;
/// purchases, missions, research, bonds and recruitment remain owned by GameRoot/services.
/// </summary>
public partial class TownServicePoint : Area3D
{
    [Export] public string ServiceId { get; set; } = string.Empty;
    [Export] public string ScreenId { get; set; } = string.Empty;
    [Export] public string Label { get; set; } = "Service";
    [Export] public string Description { get; set; } = string.Empty;
    [Export] public string RequiredFacilityId { get; set; } = string.Empty;

    public Func<(bool Available, string Reason)>? AvailabilityResolver { get; set; }

    public bool IsAvailable => ResolveAvailability().Available;
    public string UnavailableReason => ResolveAvailability().Reason;

    private (bool Available, string Reason) ResolveAvailability()
    {
        if (AvailabilityResolver is null)
        {
            return (true, string.Empty);
        }

        var value = AvailabilityResolver();
        return (value.Available, value.Reason ?? string.Empty);
    }
}
