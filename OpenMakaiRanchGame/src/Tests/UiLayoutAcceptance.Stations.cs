using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Ui;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

public partial class UiLayoutAcceptance
{
    private async Task CheckStationJourney(WorldGameController world)
    {
        var game = GameRoot.Instance;
        world.CloseManagement();
        world.Transition?.CompleteImmediately();
        if (world.ActiveAreaId != "ranch") world.TravelTo("ranch");
        world.Transition?.CompleteImmediately();
        await Resize(new Vector2I(960, 540));
        await Frames(10);
        var gold = game.Economy.Gold; var stamina = game.State.Player.Stamina;
        var day = game.State.Calendar.Day; var phase = game.State.Calendar.Phase;
        var ranch = world.Ranch!; var player = ranch.Player!;
        Check(WorldDestinationCatalog.DirectionArrow(Vector3.Zero, Vector3.Forward, Basis.Identity) == "↑"
            && WorldDestinationCatalog.DirectionArrow(Vector3.Zero, Vector3.Back, Basis.Identity) == "↓"
            && WorldDestinationCatalog.DirectionArrow(Vector3.Zero, Vector3.Right, Basis.Identity) == "→"
            && WorldDestinationCatalog.DirectionArrow(Vector3.Zero, Vector3.Left, Basis.Identity) == "←",
            "wayfinding: all four camera-relative cardinal directions are correct");
        Check(WorldDestinationCatalog.DirectionArrow(Vector3.Zero, Vector3.Left, new Basis(Vector3.Up, Mathf.Pi / 2)) == "↑",
            "wayfinding: directions rotate with the camera, not fixed world axes");
        Check(world.NavigationGuide!.MarkerCount is > 0 and <= 12, "wayfinding: actual warnings produce a bounded set of station markers");
        world.ToggleManagement();
        await Frames(6);
        Check(world.IsStationPanelOpen && world.StationPanel!.ContextKind == "guide" && !world.Shell!.IsVisibleInTree(),
            "world-first: the old Management shortcut opens a read-only places guide, not the global service hub");
        var guide = world.StationPanel!;
        var place = Descendants(guide).OfType<Button>().Single(b => b.Name == "Place_dairy_barn");
        await ClickStationButton(place);
        Check(!world.IsManagementVisible && world.NavigationGuide.Target?.TargetId == "dairy_barn"
            && game.Economy.Gold == gold && game.State.Player.Stamina == stamina,
            "wayfinding: choosing a destination marks the barn without a purchase, assignment or teleport");

        var station = world.ResolveStation("dairy_barn")!;
        var building = ranch.Presentation!.Buildings[station.TargetId];
        Check(building.DoorWidth >= 1.5f && building.DoorHeight >= 2.45f && building.WallHeight >= 3f
            && building.Footprint.X >= 6f && building.CollisionBodyCount >= 7,
            "interior: the barn has human-scale clearance and real segmented walls, not a solid visual box");
        var nav = ranch.GetNode<SimpleNavigationRegionBuilder>("NavigationRegion");
        Check(nav.CollisionBakeComplete && nav.NavigationMesh!.GetPolygonCount() > 1,
            "interior: the ranch navigation mesh is baked from real collision instead of one open rectangle");
        var start = building.ToGlobal(building.EntryLocal);
        player.GlobalPosition = start; player.Velocity = Vector3.Zero;
        var front = building.GlobalBasis.Z;
        ranch.CameraRig!.SetOrbit(Mathf.Atan2(front.Z, front.X), Mathf.DegToRad(34), 8);
        await Frames(8);
        var ray = PhysicsRayQueryParameters3D.Create(building.ToGlobal(new Vector3(0, 1, building.Footprint.Y / 2 + 0.5f)),
            building.ToGlobal(new Vector3(0, 1, 0)), 1);
        ray.Exclude = new Godot.Collections.Array<Rid> { player.GetRid() };
        Check(player.GetWorld3D().DirectSpaceState.IntersectRay(ray).Count == 0,
            "interior: a physical ray through the doorway is unobstructed");
        ray.From = building.ToGlobal(new Vector3(building.Footprint.X / 2 + 1, 1, 0));
        ray.To = building.ToGlobal(new Vector3(building.Footprint.X / 2 - 0.5f, 1, 0));
        Check(player.GetWorld3D().DirectSpaceState.IntersectRay(ray).Count > 0,
            "interior: the adjacent side wall physically blocks a ray");
        try
        {
            Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = Key.W, Keycode = Key.W, Pressed = true });
            Input.FlushBufferedEvents();
            var keyEvent = InputMap.ActionGetEvents("move_forward").OfType<InputEventKey>().First();
            for (var i = 0; i < 10; i++) InputBindingService.GetCombinedLabel("interact");
            Check(ReferenceEquals(keyEvent, InputMap.ActionGetEvents("move_forward").OfType<InputEventKey>().First())
                && Input.IsActionPressed("move_forward"),
                "input: reading live HUD bindings leaves the held movement event and pressed state intact");
            Check(player.InputGate.WorldInputEnabled && player.ReadMovementInput().Y > 0,
                $"interior: actual W reaches movement (gate={player.InputGate.WorldInputEnabled}, focus={player.InputGate.WindowFocused}, input={player.ReadMovementInput()}, camera={GetViewport().GetCamera3D()?.GetPath()}, process={player.CanProcess()})");
            for (var i = 0; i < 27; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        }
        finally { Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = Key.W, Keycode = Key.W, Pressed = false }); }
        await Frames(8);
        Check(building.ContainsWorldPoint(player.GlobalPosition) && building.IsCutaway,
            $"interior: actual movement walks through the door and reveals the roof cutaway without changing scenes (start={start}, end={player.GlobalPosition}, local={building.ToLocal(player.GlobalPosition)}, gate={player.InputGate.WorldInputEnabled})");
        Check(player.GlobalBasis.Z.Dot(-front) > 0.94f, $"movement: the character's visible +Z face follows travel, not backwards (dot={player.GlobalBasis.Z.Dot(-front)})");
        Check(game.Economy.Gold == gold && game.State.Player.Stamina == stamina && game.State.Calendar.Day == day,
            "interior: walking and cutaway do not tax stamina, pay income or settle the day");
        await Capture("walk-in-dairy-960x540");
        // Stage proximity explicitly for the interface check; the separate step above tested walking.
        player.GlobalPosition = station.GlobalPosition + new Vector3(0, 0.3f, 0); player.Velocity = Vector3.Zero;
        await Frames(6);
        Check(world.OpenStation(station), "station: the physical barn opens its dedicated work surface in range");
        await Resize(new Vector2I(640, 480));
        await Frames(6);
        var panel = world.StationPanel!;
        Check(Encloses(GetViewport().GetVisibleRect(), panel.GetNode<Control>("StationCard").GetGlobalRect())
            && VisibleTarget(Descendants(panel).OfType<Button>().Single(b => b.Name == "StationClose")),
            "station: dedicated work and stable Back fit the small viewport");
        Check(!Descendants(panel).OfType<Button>().Any(b => b.Name.ToString().Contains("Shop") || b.Text == "Auto Battle"),
            "station: Dairy Barn does not expose unrelated town or combat management");
        await ClickStationButton(Descendants(panel).OfType<Button>().Single(b => b.Name == "StationTab_team"));
        if (!station.IsAvailable)
        {
            Check(Descendants(panel).OfType<Button>().Where(b => b.Name.ToString().StartsWith("Assign_", StringComparison.Ordinal)).All(b => b.Disabled),
                "station: inspection of the unfinished barn cannot assign dairy production");
            await ClickStationButton(Descendants(panel).OfType<Button>().Single(b => b.Name == "StationTab_upgrade"));
            var build = Descendants(panel).OfType<Button>().Single(b => b.Name == "FacilityUpgrade");
            var facility = game.Data.Facilities[station.RequiredFacilityId];
            var cost = game.Ranch.FacilityUpgradeCost(facility, 0);
            await ClickStationButton(build);
            Check(station.IsAvailable && game.Economy.Gold == gold - cost,
                "station: the actual Build button spends the canonical facility price and unlocks work once");
            gold = game.Economy.Gold;
            await ClickStationButton(Descendants(panel).OfType<Button>().Single(b => b.Name == "StationTab_team"));
        }
        var assign = Descendants(panel).OfType<Button>().FirstOrDefault(b => b.Name.ToString().StartsWith("Assign_", StringComparison.Ordinal) && !b.Disabled);
        if (assign is null) throw new InvalidOperationException("The continuing station fixture needs one resident not already assigned to Dairy.");
        var id = assign.Name.ToString()["Assign_".Length..];
        await ClickStationButton(assign);
        Check(game.Schedule.GetAssignment(id) == "dairy" && game.Economy.Gold == gold && game.State.Player.Stamina == stamina,
            "station: an actual Assign click changes the shared schedule with no early output or extra stamina fee");
        Check(!WorldAlertEvaluator.Evaluate(game).Any(a => a.Id == "dairy_unstaffed"),
            "wayfinding: staffing resolves the authoritative dairy warning");
        await Capture("dairy-station-640x480");
        var retired = Descendants(panel).OfType<Button>().Single(b => b.Name == "Rest_" + id);
        await ClickStationButton(Descendants(panel).OfType<Button>().Single(b => b.Name == "StationClose"));
        retired.EmitSignal(BaseButton.SignalName.Pressed);
        Check(game.Schedule.GetAssignment(id) == "dairy" && !world.IsManagementVisible && ranch.InputGate.WorldInputEnabled,
            "station: closing restores world input and a retired callback cannot alter work");
        player.GlobalPosition = new Vector3(0, 0.8f, 10);
        Check(!world.OpenStation(station), "station: a remote station cannot be opened from across the ranch");
        Check(world.OpenDedicatedService("options"), "service: one dedicated utility options page remains available");
        await Frames(8);
        var shell = world.Shell!;
        Check(!shell.GetNode<Control>(shell.NavigationPath).IsVisibleInTree()
            && !shell.GetNode<Button>(shell.EndDayButtonPath).IsVisibleInTree(),
            "service: Options hides global navigation and daily settlement controls");
        shell.ShowScreen("shop");
        Check(shell.CurrentScreen == "options", "service: a dedicated options route cannot expose the store");
        Check(!world.OpenDedicatedService("unknown_service") && shell.CurrentScreen == "options",
            "service: an unknown service cannot fall through to the old global hub");
        world.CloseManagement();
        Check(game.State.Calendar.Day == day && game.State.Calendar.Phase == phase,
            "station: the complete guidance/work/options journey keeps the same day and phase");
    }

    private async Task ClickStationButton(Button button)
    {
        button.GrabFocus(); await Frames(6);
        Check(!button.Disabled && VisibleTarget(button), $"station click: {button.Name} is enabled and visible");
        await Click(button); await Frames(6);
    }
}
