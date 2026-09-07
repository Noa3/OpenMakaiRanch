using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Character;
using OpenMakaiRanch.World;

/// <summary>
/// Throwaway capture node for visual inspection (AC #13 + AC #22).
/// Loads the RanchGreybox world, enables soft-anime shading on all NPC avatars,
/// positions a vantage camera, waits a few frames for rendering to stabilise,
/// then saves the viewport to a PNG and quits. Run as the main scene:
///   godot --path PROJECT res://scenes/dev/WorldCapture.tscn
/// </summary>
[GlobalClass]
public partial class WorldCapture : Node
{
    private int _frame;
    private bool _captured;

    public override void _Ready()
    {
        // Ensure a valid game state so the roster is populated.
        var game = GameRoot.Instance;
        if (game is not null && GodotObject.IsInstanceValid(game))
        {
            game.NewGame();
        }

        // Load and instantiate the 3D ranch world.
        var scene = GD.Load<PackedScene>("res://scenes/dev/RanchGreybox.tscn");
        if (scene is null)
        {
            GD.PrintErr("WorldCapture: failed to load RanchGreybox.tscn");
            GetTree().Quit(1);
            return;
        }

        var world = scene.Instantiate() as Node3D;
        if (world is null)
        {
            GD.PrintErr("WorldCapture: scene instance is not a Node3D");
            GetTree().Quit(1);
            return;
        }
        AddChild(world);

        // Enable soft-anime shading on every NPC avatar (AC #13 visual check).
        // Drive the RosterRig directly (authoritative avatar set) instead of FindChildren,
        // which is fragile against C# node type-name matching.
        var rosterRig = world.GetNodeOrNull<RosterRig>("RosterRig");
        int avatarCount = 0;
        if (rosterRig is not null && game is not null)
        {
            var placed = rosterRig.Refresh(game);
            foreach (var id in rosterRig.AvatarIds)
            {
                var avatar = rosterRig.GetAvatar(id);
                if (avatar is not null)
                {
                    avatar.UseSoftShading = true;
                    avatar.Rebuild();
                    avatarCount++;
                }
            }
        }
        GD.Print($"WorldCapture: roster={game?.Roster?.Characters.Count ?? -1} avatars={avatarCount} (rosterRig={rosterRig?.AvatarCount ?? -1})");

        // Vantage camera: TIGHT close-up on the first avatar so the gradient is
        // large enough to measure objectively (AC #13).
        var firstId = rosterRig is not null && rosterRig.AvatarIds.Any() ? rosterRig.AvatarIds.First() : null;
        Vector3 restArea = new(-12f, 0f, -6f);
        if (firstId is not null)
        {
            var a = rosterRig!.GetAvatar(firstId);
            if (a is not null) restArea = a.GlobalPosition;
        }
        var camera = new Camera3D
        {
            Name = "CaptureCamera",
            Current = true,
            Position = restArea + new Vector3(1.6f, 1.3f, 2.2f),
            Fov = 45f,
        };
        camera.LookAtFromPosition(camera.Position, restArea + new Vector3(0f, 0.9f, 0f), Vector3.Up);
        AddChild(camera);

        // Second vantage: the wide establishing shot of the full ranch (AC #22).
        var wide = new Camera3D
        {
            Name = "CaptureCameraWide",
            Current = false,
            Position = new Vector3(22f, 14f, 22f),
            Fov = 55f,
        };
        wide.LookAtFromPosition(wide.Position, new Vector3(0f, 0.5f, 0f), Vector3.Up);
        AddChild(wide);

        GD.Print("WorldCapture: ready, capturing after 8 frames...");
    }

    public override void _Process(double delta)
    {
        if (_captured) return;
        _frame++;
        if (_frame < 8) return;

        var viewport = GetViewport();
        if (viewport is null)
        {
            GD.PrintErr("WorldCapture: no viewport");
            GetTree().Quit(1);
            return;
        }

        var texture = viewport.GetTexture();
        var image = texture.GetImage();
        if (image is null)
        {
            GD.PrintErr("WorldCapture: GetImage() returned null (render not ready)");
            GetTree().Quit(1);
            return;
        }

        // Save to an absolute path (bypass user:// profile isolation).
        var outDir = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
            "OpenMakaiRanchCapture");
        System.IO.Directory.CreateDirectory(outDir);
        var outPath = System.IO.Path.Combine(outDir, "world_capture.png");

        var err = image.SavePng(outPath);
        if (err == Error.Ok)
        {
            GD.Print($"WorldCapture: SAVED {outPath}");
            GD.Print("WorldCapture: CAPTURE_OK");
        }
        else
        {
            GD.PrintErr($"WorldCapture: SavePng failed: {err}");
            GetTree().Quit(1);
            return;
        }

        _captured = true;
        GetTree().Quit(0);
    }
}
