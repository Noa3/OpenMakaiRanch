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
    // Tests the real town after an explicit scene-entry setup, NOT the ranch-to-town connection.
    // Locomotion thereafter uses normal player physics and the existing touch-input path only.
    private async Task CaptureTownCore(WorldGameController world)
    {
        var game = GameRoot.Instance;
        var partner = game.Roster.Find("rancher") ?? throw new InvalidOperationException("Existing rancher required.");
        var previousPartner = game.State.Dating.ActivePartnerId;
        var started = Time.GetTicksMsec();
        var checks = new List<object>(); var trace = new List<object>(); var shots = new List<object>();
        static float[] Vec(Vector3 p) => new[] { p.X, p.Y, p.Z };
        var passed = false; string? error = null;
        Action? physicsRecorder = null; Exception? recordingError = null;
        ThirdPersonPlayerController? player = null;
        var frame = 0;
        void Check(bool ok, string label) { checks.Add(new { ok, label }); if (!ok) throw new InvalidOperationException(label); }
        try
        {
            game.State.Dating.ActivePartnerId = partner.Id; // Synthetic escort setup; no invitation/payment claimed.
            game.NotifyStateChanged();
            Check(world.TravelTo("town"), "existing scene entry accepts town");
            world.Transition?.CompleteImmediately(); world.CloseManagement();
            await Frames(24);
            var town = world.Town ?? throw new InvalidOperationException("Actual town missing.");
            player = town.Player ?? throw new InvalidOperationException("Town player missing.");
            var rig = town.Companion ?? throw new InvalidOperationException("Existing companion rig missing.");
            Check(rig.TryGetAvatar(partner.Id, out var escort) && escort is not null, "existing escort spawned");
            var companion = escort!;
            GetWindow().Size = new Vector2I(1600, 900);
            GetViewport().Scaling3DScale = 1.0f;
            var civic = town.GetNode<OkachiCivicHouse>("CivicHouse");
            var camRig = town.CameraRig ?? throw new InvalidOperationException("Town camera missing.");
            var camera = camRig.GetNode<Camera3D>("Camera");
            camRig.ProcessMode = ProcessModeEnum.Disabled;
            camera.Fov = 55;
            camera.LookAtFromPosition(town.ToGlobal(new Vector3(30, 23, 30)), town.ToGlobal(new Vector3(0, 0, -3)), Vector3.Up);
            camera.MakeCurrent();
            var speed = player.MaxWalkSpeed;
            var gold = game.Economy.Gold; var deliveries = game.CompletedCommunityDeliveries;
            var before = player.GlobalPosition; var beforeEscort = companion.GlobalPosition;
            var elapsed = 0d;
            using var escortShape = new CapsuleShape3D { Radius = 0.35f, Height = partner.Height / 1000f };
            void RecordPhysicsFrame()
            {
                var dt = GetPhysicsProcessDeltaTime(); elapsed += dt; frame++;
                var now = player.GlobalPosition; var following = companion.GlobalPosition;
                Check(now.DistanceTo(before) < 0.65f, "player trajectory has no jump");
                Check(following.DistanceTo(beforeEscort) < 0.65f, "escort trajectory has no jump");
                if (frame % 4 == 0)
                {
                    using var query = new PhysicsShapeQueryParameters3D
                    {
                        Shape = escortShape, CollisionMask = 1, CollideWithBodies = true, CollideWithAreas = false,
                        Transform = new Transform3D(Basis.Identity, following + Vector3.Up * (escortShape.Height / 2 + 0.02f)),
                        Exclude = new Godot.Collections.Array<Rid> { player.GetRid() }
                    };
                    Check(town.GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count == 0,
                        $"escort capsule avoids actual solid at {town.ToLocal(following)}");
                }
                trace.Add(new { frame, elapsed, player = Vec(town.ToLocal(now)), escort = Vec(town.ToLocal(following)),
                    input = new[] { player.MobileMovementInput.X, player.MobileMovementInput.Y } });
                before = now; beforeEscort = following;
            }
            physicsRecorder = () =>
            {
                if (recordingError is not null) return;
                try { RecordPhysicsFrame(); }
                catch (Exception ex) { recordingError = ex; }
            };
            // Keep recording during UI rendering and screenshots, not only movement awaits.
            GetTree().PhysicsFrame += physicsRecorder;
            async Task Tick()
            {
                await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
                if (recordingError is not null) throw recordingError;
            }
            async Task Walk(float x, float z)
            {
                var target = town.ToGlobal(new Vector3(x, 0, z));
                var start = Time.GetTicksMsec();
                while (true)
                {
                    var difference = target - player.GlobalPosition; difference.Y = 0;
                    if (difference.Length() <= 0.16f) break;
                    if (Time.GetTicksMsec() - start > 12000)
                        throw new InvalidOperationException($"Walk stalled toward ({x},{z}); actual={town.ToLocal(player.GlobalPosition)}, input={player.ReadMovementInput()}, gate={player.InputGate.WorldInputEnabled}");
                    var right = camera.GlobalBasis.X; right.Y = 0; right = right.Normalized();
                    var forward = -camera.GlobalBasis.Z; forward.Y = 0; forward = forward.Normalized();
                    var direction = difference.Normalized() * Mathf.Min(1, difference.Length() / 0.6f);
                    player.MobileMovementInput = new Vector2(direction.Dot(right), direction.Dot(forward));
                    player.MobileSprintHeld = false;
                    await Tick();
                }
                player.MobileMovementInput = Vector2.Zero;
                for (var i = 0; i < 4; i++) await Tick();
                checks.Add(new { ok = true, label = $"walk reached ({x},{z})" });
            }
            async Task AwaitEscort(string room)
            {
                var start = Time.GetTicksMsec();
                while (!civic.CanApproachService(room, companion.GlobalPosition + Vector3.Up * 0.8f))
                {
                    if (Time.GetTicksMsec() - start > 18000)
                    {
                        var agent = companion.GetNodeOrNull<NavigationAgent3D>("NavigationAgent");
                        var nav = Descendants(town).OfType<SimpleNavigationRegionBuilder>().FirstOrDefault();
                        throw new InvalidOperationException($"Escort did not enter {room}; actual={town.ToLocal(companion.GlobalPosition)}; "
                            + $"origin={town.GlobalPosition}; process={rig.IsPhysicsProcessing()}/{rig.CanProcess()}; animate={rig.AnimateTravel}; "
                            + $"polygons={nav?.NavigationMesh?.GetPolygonCount()}; agent={agent is not null}; "
                            + $"target={agent?.TargetPosition}; next={agent?.GetNextPathPosition()}; finished={agent?.IsNavigationFinished()}; reachable={agent?.IsTargetReachable()}");
                    }
                    await Tick();
                }
                Check(true, $"escort entered actual {room} room");
            }
            async Task AwaitEscortNear(string label)
            {
                var start = Time.GetTicksMsec();
                while (new Vector2(companion.GlobalPosition.X - player.GlobalPosition.X, companion.GlobalPosition.Z - player.GlobalPosition.Z).Length() > 2.8f
                    || (label == "market bypass exit" && town.ToLocal(companion.GlobalPosition).Z >= 2f))
                {
                    if (Time.GetTicksMsec() - start > 30000) throw new InvalidOperationException($"Escort did not reach {label}");
                    await Tick();
                }
                Check(true, $"escort reached {label} on foot");
            }
            async Task Shot(string name, Vector3 from, Vector3 target)
            {
                camera.LookAtFromPosition(town.ToGlobal(from), town.ToGlobal(target), Vector3.Up); camera.MakeCurrent();
                await Frames(3); await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                var png = name + ".png";
                using var image = GetViewport().GetTexture().GetImage();
                if (image.SavePng(Path.Combine(_output, png)) != Error.Ok) throw new IOException(png);
                shots.Add(new { name, png, camera_position = Vec(camera.GlobalPosition), camera_target = Vec(town.ToGlobal(target)) });

            }
            Check(civic.GetNodeOrNull<Node3D>("Base/Core/Architecture") is not null, "authored civic shell is in actual town");
            Check(town.GetNodeOrNull<Node3D>("Market/CounterLeft") is not null, "base market is in actual town");
            await Shot("town-core-overview", new Vector3(30, 23, 30), new Vector3(0, 0, -3));
            await Walk(0, 6); await Walk(12, 6); await Walk(12, 4); await Walk(12, 0.2f);
            await Walk(10, 0.2f); await Walk(10, 1); await Walk(8.75f, 1);
            Check(!world.IsManagementVisible && !world.IsStationPanelOpen, "entering building opens no menu");
            await AwaitEscort("town_hall");
            Check(civic.CanApproachService("town_hall", player.GlobalPosition), "player reached reception on foot");
            Check(town.TryInteract() && world.IsManagementVisible && world.Shell?.CurrentScreen == "milestones"
                && world.Shell.IsVisibleInTree(), "actual reception interaction opens visible existing milestones");
            await Shot("town-core-milestones", new Vector3(22, 15, 14), new Vector3(12, 0, -2));
            Check(world.CloseManagement(), "reception closes normally"); await Frames(4);
            await Shot("town-core-reception", new Vector3(22, 15, 14), new Vector3(12, 1, -2));
            await Walk(10, 1); await Walk(10, 0.2f); await Walk(12, 0.2f); await Walk(12, -4.7f);
            await Walk(14, -4.7f); await Walk(14, -4.3f); await Walk(15.35f, -4.3f);
            await AwaitEscort("planning_board");
            Check(civic.CanApproachService("planning_board", player.GlobalPosition), "player reached planning office on foot");
            await Walk(14, -4.3f); await Walk(14, -4.7f); await Walk(12, -4.7f); await Walk(12, 4);
            await Walk(12, 6); await Walk(0, 6); await Walk(0, 12); await Walk(-11, 12); await Walk(-11, 9);
            await AwaitEscortNear("market counter");
            Check(town.TryInteract() && world.IsManagementVisible && world.Shell?.CurrentScreen == "shop"
                && world.Shell.IsVisibleInTree(), "market counter opens existing visible shop");
            await Shot("town-core-shop", new Vector3(-19, 9, 19), new Vector3(-9, 0, 8));
            Check(world.CloseManagement(), "shop closes normally"); await Frames(4);
            await Shot("town-core-market", new Vector3(-19, 9, 19), new Vector3(-9, 0, 8));
            await Walk(-11, 12); await Walk(-14, 12); await Walk(-14, 4); await Walk(-14, 0);
            await AwaitEscortNear("market bypass exit");
            Check(player.MaxWalkSpeed == speed && game.Economy.Gold == gold && game.CompletedCommunityDeliveries == deliveries,
                "walk speed, gold and delivery count unchanged; no transaction performed");
            passed = true;
        }
        catch (Exception ex) { error = ex.Message; throw; }
        finally
        {
            if (physicsRecorder is not null) GetTree().PhysicsFrame -= physicsRecorder;
            error ??= recordingError?.Message;
            passed &= recordingError is null;
            if (player is not null) { player.MobileMovementInput = Vector2.Zero; player.MobileSprintHeld = false; }
            File.WriteAllText(Path.Combine(_output, "town-core-walk.json"), JsonSerializer.Serialize(new
            {
                passed, error, wall_clock_seconds = (Time.GetTicksMsec() - started) / 1000.0,
                scope = "actual town only; existing TravelTo is setup, not a ranch/valley walking proof",
                setup = "existing rancher selected as escort; onboarding bypassed by isolated fixture; no invitation or transaction",
                checks, trace, shots, pipeline = "real mobile input, unchanged movement settings, normal _PhysicsProcess/MoveAndSlide"
            }, new JsonSerializerOptions { WriteIndented = true }));
            world.CloseManagement(); game.State.Dating.ActivePartnerId = previousPartner; game.NotifyStateChanged();
            world.TravelTo("ranch"); world.Transition?.CompleteImmediately();
            await Frames(4);
        }
    }
}
