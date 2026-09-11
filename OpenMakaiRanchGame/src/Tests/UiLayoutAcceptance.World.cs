using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

public partial class UiLayoutAcceptance
{
    private async Task CheckWorldLayouts(WorldGameController world)
    {
        world.CloseManagement();
        await Frames(4);
        var pastoral = world.Ranch!.GetNode<RanchSkyAndLanterns>("PastoralSkyAndLamps");
        var ranchCamera = world.Ranch.CameraRig!.GetNode<Camera3D>("Camera");
        var sourceEnvironment = world.Ranch.GetNode<WorldEnvironment>("WorldEnvironment").Environment;
        Check(pastoral.SkyActive && pastoral.LampCount == 2, "ranch mounts the pastoral sky and exactly two lanterns");
        Check(ranchCamera.Environment != sourceEnvironment && sourceEnvironment.Sky is null,
            "ranch sky uses a camera-local copy, not the global canonical environment");
        pastoral.Enabled = false;
        await Frames(4);
        Check(!pastoral.SkyActive && ranchCamera.Environment is null, "disabling the pastoral layer restores the camera fallback");
        pastoral.Enabled = true;
        await Frames(4);
        Check(pastoral.SkyActive, "pastoral presentation can be explicitly re-enabled");
        foreach (var size in new[] { new Vector2I(640, 480), new Vector2I(480, 800), new Vector2I(960, 540) })
        {
            await Resize(size);
            var ranch = world.Ranch!;
            var hud = ranch.Hud!;
            var assist = Descendants(world).OfType<PanelContainer>().First(panel => panel.Name == "InteractionPanel");
            var interact = assist.GetNode<Button>("Row/ActionButton");
            // The authored primary Station is the unbuilt Dairy Barn, not an available facility.
            // Keep that real progression gate as a negative control; do not unlock it for layout tests.
            var locked = ranch.Stations.First(point => point.TargetId == "dairy_barn");
            ranch.Player!.GlobalPosition = locked.GlobalPosition + new Vector3(0, 0, 1.1f);
            await Frames(8);
            Check(!locked.IsAvailable && VisibleTarget(interact)
                && assist.GetNode<Label>("Row/PromptLabel").Text.Contains("not built"),
                $"{size}: the unbuilt dairy station retains its production lock while allowing inspection");
            // Staged proximity exercises a built facility's presentation, not a walking route.
            var station = ranch.Stations.First(point => point.TargetId == "pasture");
            Check(station.IsAvailable, $"{size}: the existing Pasture supplies the positive interaction fixture");
            ranch.Player.GlobalPosition = station.GlobalPosition + new Vector3(0, 0, 1.1f);
            await Frames(8);
            Check(VisibleTarget(hud.GetNode<Button>("ManagementButton")) && VisibleTarget(hud.GetNode<Button>("AdvanceTimeButton")),
                $"{size}: ranch world action buttons retain usable hit targets");
            var worker = hud.GetNode<Control>("WorkerPanel").GetGlobalRect();
            var alerts = hud.GetNode<Control>("AlertPanel").GetGlobalRect();
            Check(!worker.Intersects(alerts), $"{size}: worker and attention panels do not overlap");
            Check(assist.IsVisibleInTree() && Encloses(GetViewport().GetVisibleRect(), assist.GetGlobalRect()),
                $"{size}: interaction panel remains inside the viewport");
            Check(VisibleTarget(interact), $"{size}: staged nearby station exposes an unclipped interaction button");
            Check(!hud.GetNode<Label>("Prompt").Visible, $"{size}: the legacy prompt does not duplicate the interaction overlay");
            await Capture($"ranch-world-{size.X}x{size.Y}");
            await CheckWorldHelp(world, hud, $"ranch-{size.X}x{size.Y}");
            Check(world.TravelTo("town"), $"{size}: layout fixture can use the existing town travel boundary");
            world.Transition?.CompleteImmediately();
            await Frames(8);
            var townHud = world.Town!.Hud!;
            Check(VisibleTarget(townHud.GetNode<Button>("ReturnRanchButton")), $"{size}: town return remains a usable hit target");
            await Capture($"town-world-{size.X}x{size.Y}");
            await CheckWorldHelp(world, townHud, $"town-{size.X}x{size.Y}");
            Check(world.TravelTo("ranch"), $"{size}: return remains available after closing town help");
            world.Transition?.CompleteImmediately();
            await Frames(5);
        }
    }

    private async Task CheckWorldHelp(WorldGameController world, CanvasLayer hud, string tag)
    {
        var helpButton = hud.GetNode<Button>("TutorialOverlay/HelpButton");
        Check(VisibleTarget(helpButton), $"{tag}: Help has a visible hit target");
        await Click(helpButton);
        var help = hud.GetNode<ScrollContainer>("TutorialOverlay/HelpPanel");
        Check(world.WorldHelpVisible && help.Visible && !world.WorldActionsAvailable,
            $"{tag}: a real Help click takes input ownership");
        Check(Encloses(GetViewport().GetVisibleRect(), help.GetGlobalRect()), $"{tag}: expanded help fits the viewport");
        Check(help.GetNode<Label>("Inner/ContextDetails").Text.Contains("Stamina"), $"{tag}: expanded help retains full resource context");
        var expectedAlerts = WorldAlertEvaluator.Evaluate(GameRoot.Instance);
        Check(expectedAlerts.All(alert => help.GetNode<Label>("Inner/ContextDetails").Text.Contains(alert.Detail)),
            $"{tag}: expanded help retains every compacted alert detail");
        await Capture("help-top-" + tag);
        var close = help.GetNode<Button>("Inner/CloseButton");
        close.GrabFocus();
        await Frames(5);
        Check(VisibleTarget(close), $"{tag}: focus scrolling reaches the Help close button");
        await Click(close);
        Check(!world.WorldHelpVisible && world.WorldActionsAvailable, $"{tag}: closing Help restores world input");
    }
}
