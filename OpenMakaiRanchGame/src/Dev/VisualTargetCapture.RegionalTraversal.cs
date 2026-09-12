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
    // No route transforms, TravelTo, speed overrides or navigation overrides in this fixture.
    private async Task CaptureRegionalTraversal(CoastalRegionController world)
    {
        var game = GameRoot.Instance;
        var started = Time.GetTicksMsec();
        var shots = new List<object>(); var reached = new List<string>();
        string segment = "setup", error = "";
        var frame = 0; var passed = false; Exception? recorderError = null;
        Action? recorder = null;
        Vector3? before = null, beforeEscort = null;
        var partner = game.Roster.Find("rancher") ?? throw new InvalidOperationException("Existing rancher missing");
        using var stream = new StreamWriter(Path.Combine(_output, "regional-trace.jsonl")) { AutoFlush = true };
        using var shape = new CapsuleShape3D { Radius = 0.35f, Height = partner.Height / 1000f };
        ThirdPersonPlayerController Player() => world.GetActivePlayer() ?? throw new InvalidOperationException("Active player missing");
        Node3D Escort()
        {
            var roster = world.ActiveAreaId == "town" ? world.Town?.Companion : world.Ranch?.Roster;
            if (roster is null || !roster.TryGetAvatar(partner.Id, out var avatar) || avatar is null)
                throw new InvalidOperationException("Active escort missing in " + world.ActiveAreaId);
            return avatar;
        }
        Camera3D Camera() => (world.ActiveAreaId == "town" ? world.Town!.CameraRig! : world.Ranch!.CameraRig!).GetNode<Camera3D>("Camera");
        async Task Shot(string name)
        {
            Player().MobileMovementInput = Vector2.Zero;
            await Frames(3); await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            using var image = GetViewport().GetTexture().GetImage();
            if (image.SavePng(Path.Combine(_output, name + ".png")) != Error.Ok) throw new IOException(name);
            shots.Add(new { name, frame, area = world.ActiveAreaId, camera_position = V(Camera().GlobalPosition), camera_rotation = V(Camera().GlobalRotation) });
        }
        async Task Tick()
        {
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            if (recorderError is not null) throw recorderError;
            if (Time.GetTicksMsec() - started > 190000) throw new InvalidOperationException("190s fixture budget exhausted");
        }
        async Task Walk(string label, Vector3 target)
        {
            segment = label; var start = Time.GetTicksMsec(); var progressAt = start;
            var best = float.PositiveInfinity;
            while (true)
            {
                var player = Player(); var delta = target - player.GlobalPosition; delta.Y = 0;
                if (delta.Length() <= 0.16f) break;
                if (delta.Length() < best - 0.15f) { best = delta.Length(); progressAt = Time.GetTicksMsec(); }
                if (Time.GetTicksMsec() - progressAt > 7000 || Time.GetTicksMsec() - start > 35000)
                    throw new InvalidOperationException($"Stalled {label}; target={target}; actual={player.GlobalPosition}; input={player.ReadMovementInput()}; gate={player.InputGate.WorldInputEnabled}; seam={world.LastTransitionBlocker}");
                var camera = Camera();
                var right = camera.GlobalBasis.X; right.Y = 0; right = right.Normalized();
                var forward = -camera.GlobalBasis.Z; forward.Y = 0; forward = forward.Normalized();
                var direction = delta.Normalized() * Mathf.Min(1, delta.Length() / 0.6f);
                player.MobileMovementInput = new Vector2(direction.Dot(right), direction.Dot(forward));
                player.MobileSprintHeld = false;
                await Tick();
                if (player != Player()) player.MobileMovementInput = Vector2.Zero;
            }
            Player().MobileMovementInput = Vector2.Zero;
            reached.Add(label);
        }
        async Task EscortNear(string label)
        {
            segment = label + " escort"; var start = Time.GetTicksMsec();
            while (new Vector2(Escort().GlobalPosition.X - Player().GlobalPosition.X, Escort().GlobalPosition.Z - Player().GlobalPosition.Z).Length() > 2.8f)
            {
                if (Time.GetTicksMsec() - start > 14000)
                {
                    var escort = Escort();
                    var agent = escort.GetNode<NavigationAgent3D>("NavigationAgent");
                    throw new InvalidOperationException($"Escort stalled at {label}; actual={escort.GlobalPosition}; next={agent.GetNextPathPosition()}; heightOffset={agent.PathHeightOffset}; target={agent.TargetPosition}; reachable={agent.IsTargetReachable()}; finished={agent.IsNavigationFinished()}");
                }
                await Tick();
            }
            reached.Add(segment);
        }
        Vector3 Local(string area, float x, float z) => world.GetAreaRoot(area).ToGlobal(new Vector3(x, 0, z));
        try
        {
            GetWindow().Size = new Vector2I(1600, 900); GetViewport().Scaling3DScale = 1;
            game.State.Dating.ActivePartnerId = partner.Id; game.NotifyStateChanged();
            while (!world.NavigationReady)
            {
                if (Time.GetTicksMsec() - started > 30000) throw new InvalidOperationException("Regional navigation not ready: " + world.RegionError);
                await Frames(1);
            }
            await Frames(12);
            var initialSpeed = Player().MaxWalkSpeed;
            var initialGold = game.Economy.Gold;
            recorder = () =>
            {
                if (recorderError is not null) return;
                try
                {
                    frame++;
                    var player = Player(); var escort = Escort();
                    var p = player.GlobalPosition; var e = escort.GlobalPosition;
                    var collisions = new List<string>();
                    if (frame % 4 == 0)
                    {
                        using var query = new PhysicsShapeQueryParameters3D { Shape = shape, CollisionMask = 1,
                            CollideWithBodies = true, CollideWithAreas = false,
                            Transform = new Transform3D(Basis.Identity, e + Vector3.Up * (shape.Height / 2 + 0.02f)),
                            Exclude = new Godot.Collections.Array<Rid> { player.GetRid() } };
                        foreach (var hit in player.GetWorld3D().DirectSpaceState.IntersectShape(query, 8))
                            collisions.Add(hit["collider"].AsGodotObject() is Node node ? node.GetPath().ToString() : hit["collider_id"].ToString());
                    }
                    var slides = new List<object>();
                    for (var i = 0; i < player.GetSlideCollisionCount(); i++)
                    {
                        var hit = player.GetSlideCollision(i);
                        slides.Add(new { collider = (hit.GetCollider() as Node)?.GetPath().ToString(), position = V(hit.GetPosition()), normal = V(hit.GetNormal()) });
                    }
                    var agent = escort.GetNodeOrNull<NavigationAgent3D>("NavigationAgent");
                    stream.WriteLine(JsonSerializer.Serialize(new { frame, physics_frame = Engine.GetPhysicsFrames(), segment,
                        area = world.ActiveAreaId, player_id = player.GetInstanceId(), escort_id = escort.GetInstanceId(),
                        player = V(p), escort = V(e), velocity = V(player.Velocity), on_floor = player.IsOnFloor(),
                        input = new[] { player.MobileMovementInput.X, player.MobileMovementInput.Y },
                        transitions = world.WalkingTransitionCount, seam_blocker = world.LastTransitionBlocker,
                        capsule_sampled = frame % 4 == 0, collisions, slides,
                        nav_target = agent is null ? null : V(agent.TargetPosition), nav_finished = agent?.IsNavigationFinished() }));
                    if (before is { } old && old.DistanceTo(p) > 0.65f) throw new InvalidOperationException("Player spatial jump");
                    if (beforeEscort is { } oldE && oldE.DistanceTo(e) > 0.65f) throw new InvalidOperationException("Escort spatial jump");
                    before = p; beforeEscort = e;
                    if (collisions.Count > 0) throw new InvalidOperationException("Sampled escort capsule overlaps " + string.Join(",", collisions));
                }
                catch (Exception ex) { recorderError = ex; }
            };
            GetTree().PhysicsFrame += recorder;
            segment = "real ranch spawn"; await Shot("regional-start"); await Tick();
            await Walk("home front approach", Local("ranch", -10, 4)); await EscortNear("home front approach"); await Shot("regional-home");
            await Walk("home return lane", Local("ranch", 0, 10.5f));
            using var layout = JsonDocument.Parse(Godot.FileAccess.GetFileAsString(OrganicWorldLayout.SourcePath));
            var region = layout.RootElement.GetProperty("region");
            static Vector3 Read(JsonElement a) => new(a[0].GetSingle(), a[1].GetSingle(), a[2].GetSingle());
            var side = region.GetProperty("ranch_side_route").GetProperty("points").EnumerateArray().Select(Read).ToArray();
            for (var i = 0; i < side.Length; i++) await Walk("meadow outward " + i, side[i]);
            await EscortNear("meadow west"); await Shot("regional-meadow");
            for (var i = side.Length - 2; i >= 0; i--) await Walk("meadow return " + i, side[i]);
            var road = region.GetProperty("connection").GetProperty("points").EnumerateArray().Select(Read).ToArray();
            var bridgeExit = region.GetProperty("connection").GetProperty("bridge").GetProperty("segment_index").GetInt32() + 1;
            for (var i = 0; i < road.Length; i++)
            {
                await Walk("valley road " + i, road[i]);
                // Stop at route controls; do not accelerate or relocate the slower companion.
                await EscortNear("valley road " + i);
                if (i == bridgeExit) await Shot("regional-valley-bridge");
            }
            await EscortNear("seam approach");
            await Walk("automatic walking seam", Local("town", 0, 32));
            if (world.ActiveAreaId != "town" || world.WalkingTransitionCount != 1) throw new InvalidOperationException("Automatic seam did not hand off");
            await Shot("regional-seam");
            foreach (var point in new[] { new Vector2(0, 20), new Vector2(0, 6), new Vector2(12, 6), new Vector2(12, 4), new Vector2(12, 0.2f), new Vector2(10, 0.2f), new Vector2(10, 1), new Vector2(8.75f, 1) })
                await Walk("civic " + point, Local("town", point.X, point.Y));
            await EscortNear("reception"); await Shot("regional-civic");
            foreach (var point in new[] { new Vector2(10, 1), new Vector2(10, 0.2f), new Vector2(12, 0.2f), new Vector2(12, 6), new Vector2(0, 6), new Vector2(0, 12), new Vector2(-11, 12), new Vector2(-11, 9) })
                await Walk("market " + point, Local("town", point.X, point.Y));
            await EscortNear("market"); await Shot("regional-market"); await Tick();
            if (initialSpeed != Player().MaxWalkSpeed || initialGold != game.Economy.Gold) throw new InvalidOperationException("Movement speed or gold changed");
            passed = true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            Player().MobileMovementInput = Vector2.Zero;
            try { await Shot("regional-blocker"); } catch (Exception shotError) { error += "; screenshot: " + shotError.Message; }
        }
        finally
        {
            if (recorder is not null) GetTree().PhysicsFrame -= recorder;
            Player().MobileMovementInput = Vector2.Zero;
            File.WriteAllText(Path.Combine(_output, "regional-traversal.json"), JsonSerializer.Serialize(new {
                passed, accepted = false, error, segment, frames = frame, reached, shots,
                source_commit = OS.GetEnvironment("OMR_VISUAL_SOURCE_COMMIT"),
                scene = "res://scenes/dev/CoastalRegionBlockout.tscn", camera = "active regional gameplay camera; per-shot global poses",
                renderer = RenderingServer.GetCurrentRenderingMethod().ToString(), viewport = new[] { 1600, 900 }, render_scale = GetViewport().Scaling3DScale,
                elapsed_seconds = (Time.GetTicksMsec() - started) / 1000.0,
                setup = "fresh isolated game; onboarding bypassed; existing rancher prepared as escort, not earned invitation; actual regional ranch spawn; no route teleport",
                limits = "Home exterior front approach only, no interior use. Player normal MobileMovementInput and MoveAndSlide. Follower bounded nav-path motion, not CharacterBody3D; capsule checks every fourth frame are sampled, not swept. No financial action, construction, weather matrix or ordinary gameplay activation."
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        GD.Print("REGIONAL TRAVERSAL RECORDED");
        GetTree().Quit(0); // Launcher validates evidence and returns failure for a failed route.
    }
}
