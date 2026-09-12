using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Dev;

public partial class VisualTargetCapture
{
    private async Task CaptureWorldReview(WorldGameController world)
    {
        var checks = new List<object>();
        var failed = new List<string>();
        var shots = new List<object>();
        void Check(bool pass, string name)
        {
            checks.Add(new { name, passed = pass });
            if (!pass) failed.Add(name);
            GD.Print($"ORGANIC WORLD {(pass ? "OK" : "FAIL")}: {name}");
        }
        var game = GameRoot.Instance;
        var economy = (game.Economy.Gold, game.State.Player.Stamina, game.State.Calendar.Day, game.State.Calendar.Phase);
        world.ProcessMode = ProcessModeEnum.Inherit;
        await Frames(8);
        foreach (var area in new[] { "ranch", "town" })
        {
            if (world.ActiveAreaId != area)
            {
                Check(world.TravelTo(area), "travel to " + area);
                world.Transition?.CompleteImmediately();
            }
            world.CloseManagement();
            await Frames(15);
            for (var i = 0; i < 3; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            Node3D location = area == "ranch" ? world.Ranch! : world.Town!;
            var player = area == "ranch" ? world.Ranch!.Player! : world.Town!.Player!;
            var rig = area == "ranch" ? world.Ranch!.CameraRig! : world.Town!.CameraRig!;
            var nav = location.GetNode<SimpleNavigationRegionBuilder>("NavigationRegion");
            Check(nav.CollisionBakeComplete && nav.Enabled, area + " collision navigation baked and active");
            var plots = area == "ranch" ? RanchBuildingPlots.All : OrganicWorldLayout.TownPlots;
            var map = nav.GetNavigationMap();
            var start = location.ToGlobal(new Vector3(0, 0.2f, 10));
            var excludes = new Godot.Collections.Array<Rid>();
            foreach (var collider in Descendants(world).OfType<CollisionObject3D>().Where(c => c is not StaticBody3D))
                excludes.Add(collider.GetRid());
            foreach (var plot in plots)
            {
                var building = Descendants(location).OfType<WalkInBuilding>().SingleOrDefault(b =>
                    b.BuildingId == plot.Id || b.BuildingId == "town_" + plot.Id);
                var target = area == "ranch"
                    ? world.ResolveStation(plot.Id)!.GlobalPosition
                    : world.Town!.Services.Single(s => s.ServiceId == plot.Id).GlobalPosition;
                target.Y = 0.2f;
                var route = NavigationServer3D.MapGetPath(map, start, target, true);
                var reached = route.Length > 1 && new Vector2(route[^1].X - target.X, route[^1].Z - target.Z).Length() < 0.85f;
                Check(reached, area + ": navigable gate-to-" + plot.Id);
                if (building is null) continue;
                var ray = PhysicsRayQueryParameters3D.Create(
                    building.ToGlobal(new Vector3(0, 1, plot.Footprint.Y / 2 + 0.6f)),
                    building.ToGlobal(new Vector3(0, 1, 0)), 1);
                ray.Exclude = excludes;
                Check(player.GetWorld3D().DirectSpaceState.IntersectRay(ray).Count == 0, area + ": clear doorway " + plot.Id);
                ray.From = building.ToGlobal(new Vector3(plot.Footprint.X / 2 + 0.6f, 1, 0));
                ray.To = building.ToGlobal(new Vector3(plot.Footprint.X / 2 - 0.6f, 1, 0));
                Check(player.GetWorld3D().DirectSpaceState.IntersectRay(ray).Count > 0, area + ": physical wall " + plot.Id);
            }
            rig.ProcessMode = ProcessModeEnum.Disabled;
            var camera = rig.GetNode<Camera3D>("Camera");
            camera.Fov = 55;
            camera.MakeCurrent();
            var viewpoints = area == "ranch"
                ? new[] { ("ranch-overview", new Vector3(32, 22, 35), new Vector3(0, 0, -1)),
                          ("ranch-lane", new Vector3(2, 2.8f, 10), new Vector3(0, 2.1f, -4)) }
                : new[] { ("town-overview", new Vector3(29, 21, 32), new Vector3(0, 0, -1)),
                          ("town-market-lane", new Vector3(0, 2.8f, 9), new Vector3(0, 2.3f, -4)) };
            foreach (var (name, position, target) in viewpoints)
            {
                camera.LookAtFromPosition(position, target, Vector3.Up);
                await Frames(12);
                world.ProcessMode = ProcessModeEnum.Disabled;
                foreach (var node in Descendants(world))
                {
                    if (node is CanvasLayer layer) layer.Visible = false;
                    if (node is Control control) control.Visible = false;
                    if (node is Label3D label) label.Visible = false;
                    if (node is GpuParticles3D particles) { particles.Emitting = false; particles.Visible = false; }
                }
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                using var image = GetViewport().GetTexture().GetImage();
                var path = Path.Combine(_output, name + ".png");
                Check(image is not null && !image.IsEmpty() && image.SavePng(path) == Error.Ok, "render " + name);
                shots.Add(new { name, position = V(camera.GlobalPosition), rotation = V(camera.GlobalRotation), fov = camera.Fov });
                world.ProcessMode = ProcessModeEnum.Inherit;
            }
        }
        Check(world.TravelTo("ranch"), "return travel town-to-ranch");
        world.Transition?.CompleteImmediately();
        Check(economy == (game.Economy.Gold, game.State.Player.Stamina, game.State.Calendar.Day, game.State.Calendar.Phase),
            "spatial review/travel does not pay gold, consume stamina or advance time");
        File.WriteAllText(Path.Combine(_output, "organic-world-review.json"), JsonSerializer.Serialize(new
        {
            passed = failed.Count == 0, checks, shots,
            limits = "Actual static-collision route and doorway ray checks, not a full player-walk or performance benchmark. Fresh isolated profile; overlays and GPU particles hidden."
        }, new JsonSerializerOptions { WriteIndented = true }));
        if (failed.Count > 0) throw new InvalidOperationException("Organic world checks failed: " + string.Join("; ", failed));
    }
}
