using Godot;

namespace OpenMakaiRanch.World;

public partial class WorldStation
{
    /// <summary>Personal/inspection points must work even when the ranch has no assigned worker.</summary>
    [Export] public bool RequiresWorker { get; set; } = true;

    /// <summary>Presentation-only feedback for non-work stations; never a reward calculation.</summary>
    [Export] public string SuccessFeedback { get; set; } = "Opened.";
}
