using Godot;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.App;

/// <summary>A small title diorama in its own world. It never advances or loads the game simulation.</summary>
public partial class MainMenuVista : SubViewportContainer
{
    private SubViewport _viewport = null!;
    private Camera3D _camera = null!;
    private double _clock;
    private Vector2 _lastSize;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Stretch = true;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _viewport = new SubViewport { Name = "VistaViewport", OwnWorld3D = true,
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always, Size = new Vector2I(960, 540) };
        AddChild(_viewport);
        var world = new Node3D { Name = "VistaWorld" }; _viewport.AddChild(world);
        world.AddChild(new WorldEnvironment { Environment = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Color, BackgroundColor = new Color("9bb7c4"),
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            AmbientLightColor = new Color("c8d5e4"), AmbientLightEnergy = 0.65f
        } });
        world.AddChild(new DirectionalLight3D { RotationDegrees = new Vector3(-38, -28, 0),
            LightColor = new Color("ffe2b1"), LightEnergy = 1.1f, ShadowEnabled = true });
        Box(world, new Vector3(0, -0.3f, 0), new Vector3(56, 0.6f, 42), "668366");
        Box(world, new Vector3(0, 0.015f, 7), new Vector3(44, 0.02f, 2.5f), "c3ac81");
        Box(world, new Vector3(5, 0.02f, 0), new Vector3(2.5f, 0.02f, 20), "c3ac81");
        Box(world, new Vector3(19, -0.04f, 0), new Vector3(4, 0.05f, 42), "537e94");
        var farm = new WalkInBuilding { Name = "Farmhouse", BuildingId = "ranch_house",
            Position = new Vector3(4, 0, -5), Footprint = new Vector2(7.2f, 6.4f), WallColor = new Color("e1d1af") };
        world.AddChild(farm);
        world.AddChild(new WalkInBuilding { Name = "Barn", BuildingId = "dairy_barn",
            Position = new Vector3(-7, 0, -3), Footprint = new Vector2(7.6f, 6.6f), WallColor = new Color("a7bdb7") });
        for (var i = 0; i < 14; i++)
        {
            var x = i < 7 ? -19 + i * 5 : 14 + i % 2 * 4;
            var z = i < 7 ? -15 : -15 + (i - 7) * 5;
            Box(world, new Vector3(x, 1.3f, z), new Vector3(0.35f, 2.6f, 0.35f), "795b42");
            world.AddChild(new MeshInstance3D { Position = new Vector3(x, 3.2f, z),
                Mesh = new SphereMesh { Radius = 1.6f, Height = 3.2f },
                MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(i % 3 == 0 ? "b8a76d" : "739766"), Roughness = 1 } });
        }
        for (var i = 0; i < 10; i++)
        {
            Box(world, new Vector3(-15 + i * 2.7f, 0.6f, 13), new Vector3(0.13f, 1.2f, 0.13f), "997757");
            if (i < 9) Box(world, new Vector3(-13.65f + i * 2.7f, 0.8f, 13), new Vector3(2.7f, 0.12f, 0.10f), "997757");
        }
        _camera = new Camera3D { Name = "VistaCamera", Current = true, Fov = 46 };
        world.AddChild(_camera);
        UpdateCamera();
    }

    public override void _Process(double delta)
    {
        if (_viewport is null || !IsVisibleInTree()) return;
        if (!_lastSize.IsEqualApprox(Size))
        {
            _lastSize = Size;
            // Capped rendering resolution; the presentation is not a second full game world.
            StretchShrink = Mathf.Max(1, Mathf.CeilToInt(Size.X / 960f));
        }
        if (GameRoot.Instance?.State.Settings.ReducedMotion != true)
        {
            _clock += System.Math.Clamp(delta, 0, 0.1);
            UpdateCamera();
        }
    }

    private void UpdateCamera()
    {
        _camera.Position = new Vector3(25 + Mathf.Sin((float)_clock * 0.07f) * 0.8f, 15, 25);
        _camera.LookAt(new Vector3(1, 1, -1), Vector3.Up);
    }

    private static void Box(Node parent, Vector3 position, Vector3 size, string color) => parent.AddChild(
        new MeshInstance3D { Position = position, Mesh = new BoxMesh { Size = size },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(color), Roughness = 0.9f } });
}
