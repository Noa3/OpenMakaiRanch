using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// Spatial travel point between world areas. Travel itself is presentation/location state only;
/// it does not advance time, spend gold or change gameplay unless a future shared rule explicitly
/// adds such a cost.
/// </summary>
public partial class WorldTravelPortal : Area3D
{
    [Export] public string DestinationId { get; set; } = string.Empty;
    [Export] public string Label { get; set; } = "Travel";
    [Export] public string Prompt { get; set; } = "Travel";
    [Export] public string RequiredFacilityId { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;
    public string UnavailableReason { get; set; } = string.Empty;
}
