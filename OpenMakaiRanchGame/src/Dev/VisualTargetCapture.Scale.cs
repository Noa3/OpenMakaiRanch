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
    private async Task CaptureRanchScale(WorldGameController world)
    {
        var game = GameRoot.Instance!;
        var ranch = world.Ranch!;
        var player = ranch.Player!;
        var playerVisual = player.GetNode<PlayerAvatar3D>("Visual");
        var playerBefore = MeasureActor(playerVisual, "PLAYER", game.State.Player.Height / 1000f);
        var collider = player.GetNode<CollisionShape3D>("Collision");
        var physics = new
        {
            shape = collider.Shape.GetClass().ToString(),
            size = collider.Shape is BoxShape3D box ? V(box.Size)
                : collider.Shape is CapsuleShape3D capsule ? new[] { capsule.Radius * 2, capsule.Height, capsule.Radius * 2 } : null,
            local_position = V(collider.Position), world_scale = V(collider.GlobalBasis.Scale),
            player.MaxWalkSpeed, player.Acceleration, player.HeadHeight,
            visual_offset = V(playerVisual.Position), playerVisual.VisualScale
        };
        var yard = GetNode<Node3D>("ScaleYard");
        yard.Visible = true;
        player.GlobalPosition = yard.ToGlobal(new Vector3(-6, 0.8f, 0));
        player.Rotation = Vector3.Zero;
        var records = new List<object> { playerBefore };
        var profiles = game.Data.Characters.Values.Select(CharacterAvatarFactory.CreateProfile).OrderBy(p => p.Height).ToArray();
        var chosen = new[] { profiles.First(), profiles.First(p => p.DefinitionId == "noir"), profiles.Last() }
            .DistinctBy(p => p.DefinitionId).ToArray();
        for (var index = 0; index < chosen.Length; index++)
        {
            var avatar = CharacterAvatarFactory.BuildAvatar(chosen[index]);
            avatar.Name = "ScaleResident_" + chosen[index].DefinitionId;
            avatar.Position = new Vector3(-3 + index * 3, 0, 0);
            yard.AddChild(avatar);
            await Frames(2);
            var report = MeasureActor(avatar, chosen[index].DefinitionId, chosen[index].Height);
            records.Add(report);
            var bounds = ActorBounds(avatar, true);
            yard.AddChild(new Label3D { Name = "ScaleName_" + index,
                Text = chosen[index].DisplayName + " / " + chosen[index].Height.ToString("0.00") + " m target\nbody " + bounds.Size.Y.ToString("0.00") + " m",
                Position = avatar.Position + new Vector3(0, 3.35f, 0), FontSize = 38, PixelSize = 0.004f,
                Billboard = BaseMaterial3D.BillboardModeEnum.Enabled });
        }
        for (var mark = 0; mark <= 3; mark++)
            yard.AddChild(new Label3D { Name = "MetreLabel" + mark, Text = mark + " m",
                Position = new Vector3(-6.5f, mark, -1.35f), FontSize = 44, PixelSize = 0.004f,
                Billboard = BaseMaterial3D.BillboardModeEnum.Enabled });
        var camera = ranch.CameraRig!.GetNode<Camera3D>("Camera");
        camera.Fov = 55;
        var shots = new List<object>();
        foreach (var (name, from, look) in new[]
        {
            ("scale-front", new Vector3(0, 4.2f, 16), new Vector3(0, 1.1f, 0)),
            ("scale-player-eye", new Vector3(-5, 1.7f, 7), new Vector3(0, 1.1f, 1))
        })
        {
            camera.LookAtFromPosition(yard.ToGlobal(from), yard.ToGlobal(look), Vector3.Up);
            camera.MakeCurrent();
            await Frames(4);
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            using var image = GetViewport().GetTexture().GetImage();
            if (image.SavePng(Path.Combine(_output, name + ".png")) != Error.Ok) throw new IOException("Scale screenshot failed");
            shots.Add(new { name, camera_position = V(camera.GlobalPosition), camera.Fov });
        }
        File.WriteAllText(Path.Combine(_output, "ranch-scale.json"), JsonSerializer.Serialize(new
        {
            kind = "live geometry measurement; calibration props are temporary, not final artwork",
            renderer = RenderingServer.GetCurrentRenderingMethod().ToString(),
            actors = records, actual_player_physics = physics, shots,
            source_catalog_profiles = profiles.Select(p => new { p.DefinitionId, p.Height }).ToArray(),
            limits = "Body bounds exclude hair, horns, eyes/glasses and debug marker. Separate complete-visual bounds retained. Player is a capsule stand-in without anatomical feet. Fixture placement is NOT proof of walking or furniture use. No gameplay height, identity or speed changed."
        }, new JsonSerializerOptions { WriteIndented = true }));
        GD.Print("RANCH SCALE MEASURED");
    }

    private static object MeasureActor(Node3D root, string id, float targetHeight)
    {
        var body = ActorBounds(root, true);
        var all = ActorBounds(root, false);
        return new
        {
            id, target_body_height = targetHeight,
            body_height = body.Size.Y, body_width = body.Size.X, body_depth = body.Size.Z,
            bodily_bottom_relative_to_origin = body.Position.Y - root.GlobalPosition.Y,
            bodily_crown_relative_to_origin = body.End.Y - root.GlobalPosition.Y,
            complete_visual_height = all.Size.Y,
            accessory_above_crown = all.End.Y - body.End.Y,
            debug_marker_above_crown = ActorBounds(root, false, true).End.Y - body.End.Y,
            root_local_scale = V(root.Scale), root_world_scale = V(root.GlobalBasis.Scale),
            skeleton_count = Descendants(root).OfType<Skeleton3D>().Count(),
            animation_players = Descendants(root).OfType<AnimationPlayer>().Select(p => p.GetAnimationList().Select(s => s.ToString()).ToArray()).ToArray(),
            meshes = Descendants(root).OfType<MeshInstance3D>().Where(m => m.IsVisibleInTree() && m.Mesh is not null)
                .Select(m => new { name = m.Name.ToString(), scale = V(m.GlobalBasis.Scale) }).ToArray()
        };
    }

    private static Aabb ActorBounds(Node3D root, bool bodilyOnly, bool includeDebugMarker = false)
    {
        var minimum = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        var maximum = -minimum;
        var count = 0;
        foreach (var mesh in Descendants(root).OfType<MeshInstance3D>())
        {
            var name = mesh.Name.ToString();
            if (!includeDebugMarker && name.Contains("DebugMarker", StringComparison.OrdinalIgnoreCase)) continue;
            if (!mesh.IsVisibleInTree() || mesh.Mesh is null || (bodilyOnly && new[] { "Hair", "Horn", "Eye", "Glass", "DebugMarker" }.Any(s => name.Contains(s, StringComparison.OrdinalIgnoreCase)))) continue;
            var bounds = mesh.GetAabb();
            for (var corner = 0; corner < 8; corner++)
            {
                var point = bounds.Position + new Vector3((corner & 1) == 0 ? 0 : bounds.Size.X,
                    (corner & 2) == 0 ? 0 : bounds.Size.Y, (corner & 4) == 0 ? 0 : bounds.Size.Z);
                point = mesh.GlobalTransform * point;
                minimum = minimum.Min(point); maximum = maximum.Max(point); count++;
            }
        }
        if (count == 0) throw new InvalidOperationException("Actor has no measurable visible body: " + root.Name);
        return new Aabb(minimum, maximum - minimum);
    }
}
