using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Character;
using OpenMakaiRanch.World;

/// <summary>
/// Dream-loop capture node: loads the RanchGreybox world, applies a golden-hour
/// environment (sky, fog, SSAO, glow, tonemap) + warm low sun, then captures
/// a wide establishing shot (primary, for dream-loop judging) and a close-up
/// (secondary, for AC #13 avatar shading). Run as the main scene:
///   godot --path PROJECT res://scenes/dev/WorldCapture.tscn
/// </summary>
[GlobalClass]
public partial class WorldCapture : Node
{
    private int _frame;
    private bool _capturedWide;
    private bool _capturedClose;
    private const int WarmupFrames = 12;

    public override void _Ready()
    {
        var game = GameRoot.Instance;
        if (game is not null && GodotObject.IsInstanceValid(game))
        {
            game.NewGame();
        }

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

        // Roster + soft shading (AC #13).
        var rosterRig = world.GetNodeOrNull<RosterRig>("RosterRig");
        int avatarCount = 0;
        if (rosterRig is not null && game is not null)
        {
            rosterRig.Refresh(game);
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
        GD.Print($"WorldCapture: roster={game?.Roster?.Characters.Count ?? -1} avatars={avatarCount}");

        // ---- Golden-hour environment (overrides DaylightRig Morning values) ----
        ApplyGoldenHourEnvironment(world);

        // ---- Cameras ----
        // Wide establishing shot (primary — dream-loop judging target).
        var firstId = rosterRig is not null && rosterRig.AvatarIds.Any() ? rosterRig.AvatarIds.First() : null;
        Vector3 restArea = new(-12f, 0f, -6f);
        if (firstId is not null)
        {
            var a = rosterRig!.GetAvatar(firstId);
            if (a is not null) restArea = a.GlobalPosition;
        }

        var wide = new Camera3D
        {
            Name = "CaptureCameraWide",
            Current = true,
            Position = new Vector3(18f, 10f, 22f),
            Fov = 50f,
        };
        wide.LookAtFromPosition(wide.Position, new Vector3(-2f, 1f, -4f), Vector3.Up);
        AddChild(wide);

        var close = new Camera3D
        {
            Name = "CaptureCameraClose",
            Current = false,
            Position = restArea + new Vector3(1.6f, 1.3f, 2.2f),
            Fov = 45f,
        };
        close.LookAtFromPosition(close.Position, restArea + new Vector3(0f, 0.9f, 0f), Vector3.Up);
        AddChild(close);

        GD.Print("WorldCapture: ready, capturing after warmup...");
    }

    /// <summary>
    /// Apply a warm golden-hour environment: procedural sky with a warm horizon,
    /// exponential fog, filmic tonemap, subtle SSAO, and a soft warm glow.
    /// This overrides the DaylightRig's Morning values for the capture.
    /// </summary>
    private static void ApplyGoldenHourEnvironment(Node3D world)
    {
        // --- Sky (saturated, reads as atmosphere not void) ---
        var skyMaterial = new ProceduralSkyMaterial
        {
            SkyTopColor = new Color(0.20f, 0.40f, 0.66f),
            SkyHorizonColor = new Color(0.86f, 0.82f, 0.72f),
            GroundBottomColor = new Color(0.18f, 0.26f, 0.18f),
            GroundHorizonColor = new Color(0.72f, 0.66f, 0.52f),
            SunAngleMax = 0.9f,
            EnergyMultiplier = 1.2f,
            SkyEnergyMultiplier = 1.0f,
        };
        var sky = new Sky { SkyMaterial = skyMaterial };

        // --- Environment ---
        var env = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Sky,
            Sky = sky,
            TonemapMode = Godot.Environment.ToneMapper.Filmic,
            TonemapExposure = 0.9f,
            // SSAO: strong contact darkening — the #1 "flat/plastic" unlock.
            SsaoEnabled = true,
            SsaoRadius = 0.45f,
            SsaoIntensity = 0.85f,
            SsaoSharpness = 0.8f,
            SsaoDetail = 0.4f,
            // Fog: very soft cool haze, near zero (no wash).
            FogEnabled = true,
            FogDensity = 0.003f,
            FogLightColor = new Color(0.80f, 0.86f, 0.95f),
            FogSunScatter = 0.0f,
            // Glow: off (no washout).
            GlowEnabled = false,
            // Ambient: low, so key:ambient ≈ 2.2:1 (contrast).
            AmbientLightSource = Godot.Environment.AmbientSource.Sky,
            AmbientLightEnergy = 0.4f,
        };

        var worldEnv = world.GetNodeOrNull<WorldEnvironment>("WorldEnvironment");
        if (worldEnv is not null)
        {
            worldEnv.Environment = env;
            GD.Print("WorldCapture: environment applied (contrast + SSAO)");
        }
        else
        {
            GD.PrintErr("WorldCapture: WorldEnvironment node not found");
            return;
        }

        // --- Sun: strong warm key (golden hour), deep shadows ---
        var sun = world.GetNodeOrNull<DirectionalLight3D>("Sun");
        if (sun is not null)
        {
            sun.LightEnergy = 2.0f;
            sun.LightColor = new Color(1.0f, 0.85f, 0.64f);
            // 20° elevation, 210° azimuth (warm key from upper-right-back).
            sun.RotationDegrees = new Vector3(-20f, -210f, 0f);
            sun.ShadowEnabled = true;
            sun.ShadowBias = 0.12f;
            sun.ShadowNormalBias = 0.4f;
            GD.Print("WorldCapture: key sun applied (energy 2.0)");
        }

        // --- Rim / fill: cool light from behind-left, separates silhouettes off the sky ---
        var rim = new DirectionalLight3D
        {
            Name = "RimFillLight",
            LightEnergy = 0.55f,
            LightColor = new Color(0.62f, 0.72f, 0.92f),
            ShadowEnabled = false,
        };
        // From behind-left, low angle: back-lights the barn/trees for a soft rim.
        rim.RotationDegrees = new Vector3(-12f, 120f, 0f);
        world.AddChild(rim);
        GD.Print("WorldCapture: cool rim fill applied");
    }

    public override void _Process(double delta)
    {
        if (_capturedWide && _capturedClose) return;
        _frame++;
        if (_frame < WarmupFrames) return;

        var viewport = GetViewport();
        if (viewport is null)
        {
            GD.PrintErr("WorldCapture: no viewport");
            GetTree().Quit(1);
            return;
        }

        var outDir = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
            "OpenMakaiRanchCapture");
        System.IO.Directory.CreateDirectory(outDir);

        // Deterministic: Wide in the first post-warmup frame (it is Current by default),
        // then switch to Close and capture in the next frame so it has rendered once.
        if (!_capturedWide && _frame == WarmupFrames)
        {
            if (CaptureTo(viewport, outDir, "world_capture_wide.png"))
            {
                _capturedWide = true;
                SetCurrentCamera("CaptureCameraClose");
            }
            else
            {
                GetTree().Quit(1);
                return;
            }
        }
        else if (!_capturedClose && _capturedWide && _frame == WarmupFrames + 1)
        {
            if (CaptureTo(viewport, outDir, "world_capture_closeup.png"))
            {
                _capturedClose = true;
            }
            else
            {
                GetTree().Quit(1);
                return;
            }
        }
        else
        {
            return; // not yet our frame
        }

        // Only quit once BOTH captures are saved.
        if (_capturedWide && _capturedClose)
        {
            GetTree().Quit(0);
        }
    }

    private void SetCurrentCamera(string name)
    {
        foreach (var child in GetChildren())
        {
            if (child is Camera3D cam)
            {
                cam.Current = cam.Name == name;
            }
        }
    }

    private bool CaptureTo(Viewport viewport, string outDir, string fileName)
    {
        var texture = viewport.GetTexture();
        var image = texture?.GetImage();
        if (image is null)
        {
            GD.PrintErr($"WorldCapture: GetImage() returned null for {fileName}");
            return false;
        }
        var outPath = System.IO.Path.Combine(outDir, fileName);
        var err = image.SavePng(outPath);
        if (err == Error.Ok)
        {
            GD.Print($"WorldCapture: SAVED {outPath}");
            return true;
        }
        GD.PrintErr($"WorldCapture: SavePng failed: {err}");
        return false;
    }
}
