using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Dev;

/// <summary>Opt-in reference capture only. Does not change production scenes or facility ownership.</summary>
public partial class VisualTargetCapture : Node
{
    private string _output = string.Empty;
    private bool _authorized;

    public override void _EnterTree()
    {
        try
        {
            var root = OS.GetEnvironment("OMR_EXPECTED_USER_ROOT");
            _output = OS.GetEnvironment("OMR_VISUAL_TARGET_OUTPUT");
            if (!OS.IsDebugBuild() || !OS.GetCmdlineUserArgs().Contains("--visual-target-capture")
                || string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(_output)
                || DisplayServer.GetName() == "headless")
                throw new InvalidOperationException("Use Tools/VisualTargets/capture.py; a real renderer and isolated profile are required.");
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!Path.GetFullPath(OS.GetUserDataDir()).StartsWith(prefix, comparison)
                || !Path.GetFullPath(_output).StartsWith(prefix, comparison)
                || !File.Exists(Path.Combine(root, "visual-target.marker")))
                throw new InvalidOperationException("Reference capture isolation was not verified.");
            Directory.CreateDirectory(_output);
            _authorized = true;
        }
        catch (Exception error)
        {
            GD.PrintErr("VISUAL CAPTURE REFUSED: " + error.Message);
            GetTree().Quit(1);
        }
    }

    public override async void _Ready()
    {
        if (!_authorized) return;
        try
        {
            await Frames(3);
            var game = GameRoot.Instance ?? throw new InvalidOperationException("GameRoot unavailable.");
            game.NewGame();
            // Autoload initialization precedes this setup; never access services in _EnterTree.
            // Only bypass presentation onboarding. Preserve fresh facilities, wallet and calendar.
            game.State.Story.FirstDayCompleted = true;
            game.State.Story.FirstDayStage = FirstDayFlowController.StageCompleted;
            game.State.Settings.AutosaveEnabled = false;
            game.State.Settings.ReducedMotion = true;
            game.State.Settings.RenderScale = 1.0f;
            game.State.Calendar.CurrentWeather = OpenMakaiRanch.Core.Models.Weather.Clear;
            var world = GetNode<WorldGameController>("WorldGame");
            await Frames(12);
            world.Shell!.ShowScreen("ranch");
            world.ActivateStoryArea("ranch", reposition: false, firstArrival: false);
            world.Transition?.CompleteImmediately();
            world.CloseManagement();
            game.NotifyStateChanged();
            await Frames(12);
            var ranch = world.Ranch ?? throw new InvalidOperationException("Ranch missing.");
            if (OS.GetEnvironment("OMR_REGIONAL_TRAVERSAL") == "1")
            {
                await CaptureRegionalTraversal((CoastalRegionController)world);
                return;
            }
            if (OS.GetEnvironment("OMR_TOWN_CORE_REVIEW") == "1") await CaptureTownCore(world);
            var rig = ranch.CameraRig ?? throw new InvalidOperationException("Camera rig missing.");
            var camera = rig.GetNode<Camera3D>("Camera");
            rig.ProcessMode = ProcessModeEnum.Disabled;
            foreach (var node in Descendants(world))
            {
                if (node is CanvasLayer layer) layer.Visible = false;
                if (node is Label3D label) label.Visible = false;
            }
            GetWindow().Size = new Vector2I(1600, 900);
            GetViewport().Scaling3DScale = 1.0f;
            camera.Fov = 55;
            camera.LookAtFromPosition(new Vector3(38, 23, 42), new Vector3(0, 0, -1), Vector3.Up);
            camera.MakeCurrent();
            await Frames(20);
            // HUD controllers restore visibility while processing. Freeze this presentation fixture,
            // then hide overlays on the final frame rather than racing their normal refresh handlers.
            world.ProcessMode = ProcessModeEnum.Disabled;
            // GPU simulation does not stop with Node processing. Exclude particles from this fixture.
            var excludedParticles = Descendants(world).OfType<GpuParticles3D>().ToArray();
            foreach (var particles in excludedParticles)
            {
                particles.Emitting = false;
                particles.Visible = false;
            }
            foreach (var node in Descendants(world))
            {
                if (node is CanvasLayer layer) layer.Visible = false;
                if (node is Control control) control.Visible = false;
                if (node is Label3D label) label.Visible = false;
            }
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            if (excludedParticles.Any(particles => particles.Emitting || particles.Visible))
                throw new InvalidOperationException("Capture particle exclusion was overridden.");
            using var image = GetViewport().GetTexture().GetImage();
            if (image is null || image.IsEmpty() || image.GetWidth() != 1600 || image.GetHeight() != 900
                || image.SavePng(Path.Combine(_output, "W01-current.png")) != Error.Ok)
                throw new IOException("Rendered W01 capture failed or viewport differs.");
            var context = new
            {
                source_commit = OS.GetEnvironment("OMR_VISUAL_SOURCE_COMMIT"),
                scene = "res://scenes/WorldGame.tscn",
                camera = new { id = "P01", projection = camera.Projection.ToString(), fov = camera.Fov,
                    position = V(camera.GlobalPosition), rotation_radians = V(camera.GlobalRotation), near = camera.Near, far = camera.Far },
                viewport = new[] { image.GetWidth(), image.GetHeight() },
                renderer = RenderingServer.GetCurrentRenderingMethod().ToString(),
                quality = new { preset = game.State.Settings.GraphicsQuality, render_scale = GetViewport().Scaling3DScale,
                    gpu_particles = "hidden_for_reference_fixture" },
                excluded_gpu_particles = excludedParticles.Select(particles => particles.GetPath().ToString()).ToArray(),
                phase = game.State.Calendar.Phase.ToString(),
                weather = game.State.Calendar.CurrentWeather.ToString(),
                progression = new { fixture = "fresh facilities; clear-morning visual fixture; onboarding presentation bypassed; autosave disabled", day = game.State.Calendar.Day,
                    facilities = game.State.Ranch.Facilities.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value) },
                identities = game.State.Roster.Characters.Select(c => new { c.Id, c.DefinitionId }).ToArray(),
                locale = game.State.Settings.Locale,
                plots = RanchBuildingPlots.All.Select(p => new { p.Id, center = new[] { p.Center.X, p.Center.Y },
                    footprint = new[] { p.Footprint.X, p.Footprint.Y }, yaw = p.Yaw }).ToArray(),
                limits = "Fresh source capture, not generated or approved artwork. HUD, labels and GPU particles hidden for reference editing. No frame-rate benchmark or pixel-exact temporal determinism claim."
            };
            File.WriteAllText(Path.Combine(_output, "W01-context.json"), JsonSerializer.Serialize(context, new JsonSerializerOptions { WriteIndented = true }));
            if (OS.GetEnvironment("OMR_VISUAL_WORLD_REVIEW") == "1") await CaptureWorldReview(world);
            if (OS.GetEnvironment("OMR_RANCH_SCALE_REVIEW") == "1") await CaptureRanchScale(world);
            if (OS.GetEnvironment("OMR_RANCH_ASSET_REVIEW") == "1") await CaptureRanchAssets(world);
            if (OS.GetEnvironment("OMR_OKACHI_ASSET_REVIEW") == "1") await CaptureOkachiAssets(world);
            if (OS.GetEnvironment("OMR_MARKET_ASSET_REVIEW") == "1") await CaptureMarketAssets(world);
            GD.Print("VISUAL CAPTURE PASS");
            world.QueueFree();
            await Frames(4);
            GetTree().Quit(0);
        }
        catch (Exception error)
        {
            GD.PrintErr("VISUAL CAPTURE FAIL: " + error);
            GetTree().Quit(1);
        }
    }

    private static float[] V(Vector3 value) => new[] { value.X, value.Y, value.Z };
    private static System.Collections.Generic.IEnumerable<Node> Descendants(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }
    private async Task Frames(int count)
    {
        for (var i = 0; i < count; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }
}
