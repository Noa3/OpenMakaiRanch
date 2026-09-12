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
    private async Task CaptureOkachiAssets(WorldGameController world)
    {
        var variants = new List<object>();
        var shots = new List<object>();
        var architecture = new HashSet<string>(StringComparer.Ordinal);
        var camera = world.Ranch!.CameraRig!.GetNode<Camera3D>("Camera");
        foreach (var expanded in new[] { false, true })
        {
            var state = expanded ? "expanded" : "base";
            var scenePath = "res://scenes/world/OkachiSupplyHouse" + (expanded ? "Expanded" : "Base") + ".tscn";
            var kit = GD.Load<PackedScene>(scenePath).Instantiate<Node3D>();
            kit.Position = new Vector3(1000, 0, 1000);
            AddChild(kit);
            try
            {
                await Frames(3);
                var sources = new[] { kit.GetNode<Node3D>("Core/Architecture"),
                    kit.GetNode<Node3D>(expanded ? "Annex" : "RearCap") };
                var imported = new List<object>();
                foreach (var instance in sources)
                {
                    var nodes = Descendants(instance).ToArray();
                    var expectedShapes = instance.Name == "Architecture" ? 63 : expanded ? 18 : 1;
                    var shapeCount = nodes.OfType<CollisionShape3D>().Count(c => c.Shape is not null && !c.Disabled);
                    if (shapeCount != expectedShapes || !instance.GlobalBasis.Scale.IsEqualApprox(Vector3.One))
                        throw new InvalidOperationException("Civic import collision/scale mismatch: " + instance.Name);
                    var bounds = ActorBounds(instance, false);
                    architecture.Add(instance.SceneFilePath);
                    imported.Add(new { source = instance.SceneFilePath, collision_shapes = shapeCount,
                        meshes = nodes.OfType<MeshInstance3D>().Count(m => m.Mesh is not null),
                        world_scale = V(instance.GlobalBasis.Scale), bounds_min = V(bounds.Position - kit.GlobalPosition), bounds_size = V(bounds.Size) });
                }
                await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
                var space = kit.GetWorld3D().DirectSpaceState;
                bool Blocked(Vector3 from, Vector3 to)
                {
                    using var ray = PhysicsRayQueryParameters3D.Create(kit.ToGlobal(from), kit.ToGlobal(to), 1);
                    return space.IntersectRay(ray).Count > 0;
                }
                var frontClear = !Blocked(new Vector3(0, 1, 5.5f), new Vector3(0, 1, 3.9f));
                var rearBlocked = Blocked(new Vector3(0, 1, -4.4f), new Vector3(0, 1, -5.5f));
                var roomsClear = new[] { -1f, 1f }.All(side => new[] { -2.7f, 2.2f }.All(z =>
                    !Blocked(new Vector3(side * .7f, 1, z), new Vector3(side * 1.6f, 1, z))));
                if (!frontClear || !roomsClear || rearBlocked == expanded)
                    throw new InvalidOperationException("Civic doorway/rear-cap ray mismatch in " + state);
                var game = GameRoot.Instance!;
                var profile = CharacterAvatarFactory.CreateProfile(game.Data.Characters["noir"]);
                var actor = CharacterAvatarFactory.BuildAvatar(profile);
                actor.Position = new Vector3(-4.65f, 0, 1.4f);
                kit.AddChild(actor);
                await Frames(2);
                variants.Add(new { state, scene = scenePath, imported,
                    front_clear_ray = frontClear, four_room_clear_rays = roomsClear, rear_blocked_ray = rearBlocked,
                    actor = MeasureActor(actor, profile.DefinitionId, profile.Height) });
                var roofs = Descendants(kit).OfType<MeshInstance3D>()
                    .Where(m => m.Name.ToString().StartsWith("Roof_", StringComparison.Ordinal)).ToArray();
                foreach (var (view, from, target, cutaway) in new[]
                {
                    ("exterior", new Vector3(14, 12, 16), new Vector3(0, 1.5f, -2.6f), false),
                    ("cutaway", new Vector3(12, 18, 10), new Vector3(0, 0, -3), true),
                    ("reception", new Vector3(-1.65f, 1.75f, 4.05f), new Vector3(-3.25f, 1.0f, 1.55f), false)
                })
                {
                    foreach (var roof in roofs) roof.Visible = !cutaway;
                    camera.Fov = view == "reception" ? 55 : 45;
                    camera.LookAtFromPosition(kit.ToGlobal(from), kit.ToGlobal(target), Vector3.Up);
                    camera.MakeCurrent();
                    await Frames(4);
                    await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                    using var image = GetViewport().GetTexture().GetImage();
                    var name = "okachi-" + state + "-" + view;
                    if (image.SavePng(Path.Combine(_output, name + ".png")) != Error.Ok)
                        throw new IOException("Civic screenshot failed: " + name);
                    shots.Add(new { name, state, camera_position = V(from), camera_target = V(target), cutaway, camera.Fov });
                }
            }
            finally
            {
                kit.QueueFree();
                await Frames(3);
            }
        }
        if (architecture.Count != 3) throw new InvalidOperationException("Missing civic architectural module.");
        File.WriteAllText(Path.Combine(_output, "okachi-assets.json"), JsonSerializer.Serialize(new
        {
            renderer = RenderingServer.GetCurrentRenderingMethod().ToString(),
            passed = true, unique_architecture = architecture.OrderBy(s => s).ToArray(), variants, shots,
            limits = "Asset review only. Same cameras/lighting for separate base/expanded scenes; no in-world upgrade or finance. Door center rays are not actor passage, furniture collision, navigation or occupancy proof. Roster mannequin is a standing scale reference, not clerk/service integration. No construction phase, animation, weather cycle, audio, persistent project or town placement tested."
        }, new JsonSerializerOptions { WriteIndented = true }));
        GD.Print("OKACHI ASSET IMPORT/CAPTURE PASS");
    }
}
