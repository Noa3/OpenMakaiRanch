using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Character;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Dev;

public partial class VisualTargetCapture
{
    private async Task CaptureMarketAssets(WorldGameController world)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal)
        { ["canopy.glb"] = 37, ["counter.glb"] = 14, ["scaffold_bay.glb"] = 29, ["material_stack.glb"] = 18, ["barrier.glb"] = 6 };
        var sources = new HashSet<string>(StringComparer.Ordinal);
        var variants = new List<object>();
        var shots = new List<object>();
        var camera = world.Ranch!.CameraRig!.GetNode<Camera3D>("Camera");
        foreach (var state in new[] { "Base", "Work", "Finished" })
        {
            var kit = GD.Load<PackedScene>("res://scenes/world/OkachiMarket" + state + ".tscn").Instantiate<Node3D>();
            kit.Position = new Vector3(1000, 0, 1000);
            AddChild(kit);
            try
            {
                await Frames(3);
                var props = Descendants(kit).OfType<Node3D>()
                    .Where(n => n.SceneFilePath.StartsWith("res://assets/3d/okachi_market/", StringComparison.Ordinal)).ToArray();
                var imports = new List<object>();
                foreach (var prop in props)
                {
                    var shapes = Descendants(prop).OfType<CollisionShape3D>().Count(c => c.Shape is not null && !c.Disabled);
                    var name = Path.GetFileName(prop.SceneFilePath);
                    var bounds = ActorBounds(prop, false);
                    var local = bounds.Position - kit.GlobalPosition;
                    if (shapes != counts[name] || !prop.GlobalBasis.Scale.IsEqualApprox(Vector3.One)
                        || local.X < -7 || local.Z < -6 || local.X + bounds.Size.X > 7 || local.Z + bounds.Size.Z > 6)
                        throw new InvalidOperationException("Market import/count/scale/court bounds mismatch: " + prop.Name);
                    sources.Add(name);
                    imports.Add(new { name, source = prop.SceneFilePath, collision_shapes = shapes,
                        world_scale = V(prop.GlobalBasis.Scale), bounds_min = V(local), bounds_size = V(bounds.Size) });
                }
                var visibleRoof = Descendants(kit).OfType<MeshInstance3D>().Count(m => m.IsVisibleInTree()
                    && m.Name.ToString().StartsWith("Roof_", StringComparison.Ordinal));
                if (visibleRoof != (state == "Finished" ? 17 : 0))
                    throw new InvalidOperationException("Market authored roof-state override failed: " + state);
                await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
                var space = kit.GetWorld3D().DirectSpaceState;
                bool VolumeClear(Vector3 center, Vector3 size)
                {
                    using var shape = new BoxShape3D { Size = size };
                    using var query = new PhysicsShapeQueryParameters3D
                    {
                        Shape = shape, Transform = new Transform3D(Basis.Identity, kit.ToGlobal(center)),
                        CollisionMask = 1, CollideWithBodies = true, CollideWithAreas = false, Margin = 0
                    };
                    return space.IntersectShape(query, 1).Count == 0;
                }
                var bypassClear = VolumeClear(new Vector3(-5, 1.42f, 0), new Vector3(2, 2.8f, 11.5f));
                var aisleClear = VolumeClear(new Vector3(0, 1.42f, 0), new Vector3(2, 2.8f, 4));
                using var frontQuery = PhysicsRayQueryParameters3D.Create(kit.ToGlobal(new Vector3(0, 1, 4)), kit.ToGlobal(new Vector3(0, 1, 1.8f)), 1);
                var frontBlocked = space.IntersectRay(frontQuery).Count > 0;
                if (!bypassClear || !aisleClear || frontBlocked != (state == "Work"))
                    throw new InvalidOperationException("Market collision-volume/work barrier mismatch: " + state);
                var profile = CharacterAvatarFactory.CreateProfile(GameRoot.Instance!.Data.Characters["noir"]);
                var actor = CharacterAvatarFactory.BuildAvatar(profile);
                actor.Position = new Vector3(-2, 0, state == "Work" ? 3.7f : -.9f);
                kit.AddChild(actor);
                await Frames(2);
                variants.Add(new { state = state.ToLowerInvariant(), imports, visible_roof_meshes = visibleRoof,
                    bypass_clear_volume = bypassClear, central_clear_volume = aisleClear, front_work_barrier_ray = frontBlocked,
                    actor = MeasureActor(actor, profile.DefinitionId, profile.Height) });
                foreach (var (view, from, target, fov) in new[]
                {
                    ("overview", new Vector3(12, 12, 15), new Vector3(0, .6f, 0), 45f),
                    ("eye", new Vector3(0, 1.75f, 5.7f), new Vector3(0, 1.0f, -1), 60f)
                })
                {
                    camera.Fov = fov;
                    camera.LookAtFromPosition(kit.ToGlobal(from), kit.ToGlobal(target), Vector3.Up);
                    camera.MakeCurrent();
                    await Frames(4);
                    await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                    using var image = GetViewport().GetTexture().GetImage();
                    var name = "market-" + state.ToLowerInvariant() + "-" + view;
                    if (image.SavePng(Path.Combine(_output, name + ".png")) != Error.Ok) throw new IOException(name);
                    shots.Add(new { name, camera_position = V(from), camera_target = V(target), camera.Fov });
                }
            }
            finally { kit.QueueFree(); await Frames(3); }
        }
        if (!sources.SetEquals(counts.Keys)) throw new InvalidOperationException("Missing market-kit module.");
        File.WriteAllText(Path.Combine(_output, "market-assets.json"), JsonSerializer.Serialize(new
        {
            passed = true, renderer = RenderingServer.GetCurrentRenderingMethod().ToString(),
            unique_assets = sources.OrderBy(s => s).ToArray(), variants, shots,
            limits = "Authored asset variants only; separate fixture instances, not a live/persisted upgrade. Stationary clear-volume queries and barrier rays are not player/companion walks or navigation. No service binding, transactions, construction animation/audio or weather shelter behavior tested. Standing mannequin is a scale reference, not a clerk."
        }, new JsonSerializerOptions { WriteIndented = true }));
        GD.Print("MARKET ASSET IMPORT/CAPTURE PASS");
    }
}
