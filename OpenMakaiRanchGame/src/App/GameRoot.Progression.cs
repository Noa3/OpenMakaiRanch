using System.Collections.Generic;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.App;

public partial class GameRoot
{
    // A new value snapshot each time; never retain another session's state-bound service in UI.
    public IReadOnlyList<RanchAchievement> GetRanchAchievements() => new RanchProgressionService(State, Flags).Inspect();
    public LunchPlan InspectNextLunch() => new RanchProvisioningService(State, Data).InspectLunch();
}
