using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

public partial class WorldGameController
{
    /// <summary>Null for ordinary WorldGame. Regional integration is selected only by the dev scene's root script.</summary>
    public CoastalRegionController? CoastalRegion => this as CoastalRegionController;
    public ThirdPersonPlayerController? GetActivePlayer() => ActiveAreaId switch
    {
        "intro" => IntroHouse?.Player,
        "town" => Town?.Player,
        _ => Ranch?.Player
    };

    internal bool ActivateCoastalArea(string destination)
    {
        if (CoastalRegion?.RegionReady != true || !WorldActionsAvailable) return false;
        if (!SetActiveArea(destination, reposition: false)) return false;
        GameRoot.Instance?.SetWorldArea(destination);
        return true;
    }
}
