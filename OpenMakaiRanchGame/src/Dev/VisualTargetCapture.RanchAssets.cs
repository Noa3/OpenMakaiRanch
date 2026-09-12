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
    private async Task CaptureRanchAssets(WorldGameController world)
    {
        // Test-only instantiation of an authored scene; never replace production plots here.
        var scene = GD.Load<PackedScene>("res://scenes/dev/RanchHomePrototype.tscn")
            ?? throw new IOException("Ranch home prototype did not import.");
        var kit = scene.Instantiate<Node3D>();
        kit.Position = new Vector3(1000, 0, 1000);
        AddChild(kit);
        try
        {
            await Frames(3);
            var materialReports = new List<object>();
            foreach (var stem in new[] { "warm_lime_plaster", "painted_warm_oak", "natural_linen" })
            {
                var material = GD.Load<StandardMaterial3D>($"res://assets/materials/ranch_home/{stem}.tres")
                    ?? throw new IOException("Material resource did not load: " + stem);
                if (material.AlbedoTexture is null || material.NormalTexture is null || material.RoughnessTexture is null
                    || !material.NormalEnabled || material.Metallic != 0)
                    throw new InvalidOperationException("Incomplete PBR material resource: " + stem);
                var textures = new[] { material.AlbedoTexture, material.NormalTexture, material.RoughnessTexture };
                if (textures.Any(t => t.GetWidth() != 512 || t.GetHeight() != 512))
                    throw new InvalidOperationException("Unexpected imported material dimensions: " + stem);
                materialReports.Add(new { stem, roughness_channel = material.RoughnessTextureChannel.ToString(),
                    textures = textures.Select(t => new { path = t.ResourcePath, width = t.GetWidth(), height = t.GetHeight() }).ToArray() });
            }
            var instances = kit.GetNode<Node3D>("Assets").GetChildren().OfType<Node3D>().ToArray();
            var sources = instances.Select(n => n.SceneFilePath).Distinct().OrderBy(p => p).ToArray();
            if (sources.Length != 18 || sources.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("Expected all 18 distinct exported asset sources in prototype.");
            var reports = new List<object>();
            foreach (var instance in instances)
            {
                var descendants = Descendants(instance).ToArray();
                var meshes = descendants.OfType<MeshInstance3D>().Count(m => m.Mesh is not null);
                var bodies = descendants.OfType<StaticBody3D>().Count();
                var shapes = descendants.OfType<CollisionShape3D>().Count(c => c.Shape is not null && !c.Disabled);
                if (meshes == 0 || !instance.GlobalBasis.Scale.IsEqualApprox(Vector3.One))
                    throw new InvalidOperationException("Missing mesh or non-metre instance scale: " + instance.Name);
                if (instance.SceneFilePath.Contains("/ranch_house/", StringComparison.Ordinal) && shapes == 0)
                    throw new InvalidOperationException("House collision suffix did not import: " + instance.Name);
                var bounds = ActorBounds(instance, false);
                reports.Add(new { name = instance.Name.ToString(), source = instance.SceneFilePath,
                    mesh_count = meshes, static_body_count = bodies, collision_shape_count = shapes,
                    local_position = V(instance.Position), world_scale = V(instance.GlobalBasis.Scale),
                    bounds_min_relative_to_kit = V(bounds.Position - kit.GlobalPosition), bounds_size = V(bounds.Size) });
            }
            var game = GameRoot.Instance!;
            var profile = CharacterAvatarFactory.CreateProfile(game.Data.Characters["noir"]);
            var mannequin = CharacterAvatarFactory.BuildAvatar(profile);
            mannequin.Position = new Vector3(1.8f, 0, 0);
            kit.AddChild(mannequin);
            await Frames(2);
            var actor = MeasureActor(mannequin, profile.DefinitionId, profile.Height);
            var roofs = Descendants(kit).OfType<MeshInstance3D>()
                .Where(m => m.Name.ToString().StartsWith("Roof_", StringComparison.Ordinal)).ToArray();
            var camera = world.Ranch!.CameraRig!.GetNode<Camera3D>("Camera");
            var shots = new List<object>();
            foreach (var (name, from, target, cutaway) in new[]
            {
                ("home-exterior", new Vector3(31, 22, 31), new Vector3(0, 1, -5), false),
                ("home-furnished-cutaway", new Vector3(25, 29, 25), new Vector3(0, 0, -5), true),
                ("home-office", new Vector3(-8.8f, 2.1f, 3.4f), new Vector3(-6.4f, .85f, 5.5f), false),
                ("home-kitchen", new Vector3(3.5f, 2, 3.5f), new Vector3(7.7f, .85f, 5.8f), false),
                ("home-resident-room", new Vector3(4.95f, 2, -3.6f), new Vector3(2.8f, .75f, -6), false),
                ("home-common-scale", new Vector3(5, 1.7f, 2.3f), new Vector3(-2, 1, 0), false)
            })
            {
                foreach (var roof in roofs) roof.Visible = !cutaway;
                camera.LookAtFromPosition(kit.ToGlobal(from), kit.ToGlobal(target), Vector3.Up);
                camera.MakeCurrent();
                await Frames(4);
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                using var image = GetViewport().GetTexture().GetImage();
                if (image.SavePng(Path.Combine(_output, name + ".png")) != Error.Ok)
                    throw new IOException("Ranch asset screenshot failed: " + name);
                shots.Add(new { name, camera_local_position = V(from), target = V(target), cutaway, camera.Fov });
            }
            File.WriteAllText(Path.Combine(_output, "ranch-assets.json"), JsonSerializer.Serialize(new
            {
                scene = "res://scenes/dev/RanchHomePrototype.tscn", renderer = RenderingServer.GetCurrentRenderingMethod().ToString(),
                verified_unique_asset_count = sources.Length, sources, instances = reports, scale_mannequin = actor, shots,
                material_resources = materialReports,
                limits = "Authored furnished prototype only; not the production ranch or upgrade system. Imported house collider nodes checked, not physical walking or doorway passage. Furniture has no collision or use controller. Sockets are support targets, not validated pelvis/IK poses. Example beds are not resident allocations or maximum capacity. Bath remains unfurnished. Roof junction and production materials need another art pass. No navigation, sitting, lying, save/load or performance acceptance claimed."
            }, new JsonSerializerOptions { WriteIndented = true }));
            GD.Print("RANCH ASSET IMPORT/CAPTURE PASS: " + sources.Length + " sources");
        }
        finally
        {
            kit.QueueFree();
            await Frames(2);
        }
    }
}
