using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

public partial class UiLayoutAcceptance
{
    private async Task CheckProjectsJourney(WorldGameController world)
    {
        var game = GameRoot.Instance;
        world.CloseManagement(); world.Transition?.CompleteImmediately(); await Frames(6);
        var state = game.State; var before = JsonSerializer.Serialize(state);
        var generation = game.StateGeneration;
        world.OpenWorldGuide(); await Resize(new Vector2I(640, 480)); await Frames(8);
        await ClickStationButton(Descendants(world.StationPanel!).OfType<Button>().Single(button => button.Name == "OpenProjects"));
        Check(world.StationPanel!.ContextId == "projects" && world.IsStationPanelOpen,
            "projects: a real Places button opens optional project guidance without the old global hub");
        CheckLocalizedPanelGeometry(world.StationPanel, "project journal 640x480");
        await Capture("ranch-projects-640x480");
        var next = Descendants(world.StationPanel).OfType<Button>().First(button => button.Name.ToString().StartsWith("Project_"));
        await ClickStationButton(next);
        Check(!world.IsStationPanelOpen && world.NavigationGuide?.Target is not null,
            "projects: choosing a next step marks one physical destination and returns to the world");
        Check(ReferenceEquals(state, game.State) && generation == game.StateGeneration && JsonSerializer.Serialize(state) == before,
            "projects: reading and tracking objectives does not mutate gameplay, awards or save progress");
        world.NavigationGuide?.ClearTarget();
    }
}
