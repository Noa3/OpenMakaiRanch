using Godot;
using OpenMakaiRanch.Character;

/// <summary>
/// A/B shading diagnostic: two capsules side by side under one directional
/// light — left = StandardMaterial3D (engine baseline), right = soft-anime
/// ShaderMaterial. If left shows a gradient and right is flat, the soft
/// shader is unlit; if both show gradient, the earlier flat avatars were in
/// cast shadow (placement, not a shader bug).
/// </summary>
[GlobalClass]
public partial class ShadingAB : Node
{
    public override void _Ready()
    {
        var light = new DirectionalLight3D
        {
            ShadowEnabled = false,
            LightEnergy = 1.2f,
        };
        light.RotationDegrees = new Vector3(-55f, -35f, 0f);
        AddChild(light);

        var env = new WorldEnvironment { Environment = new Godot.Environment { BackgroundMode = Godot.Environment.BGMode.Color, BackgroundColor = new Color(0.1f, 0.1f, 0.14f) } };
        AddChild(env);

        // Left capsule: Standard baseline.
        var stdMat = new StandardMaterial3D { AlbedoColor = new Color(0.85f, 0.78f, 0.72f), Roughness = 0.9f };
        MakeCapsule(new Vector3(-1f, 1.2f, 0f), stdMat);

        // Right capsule: soft-anime shader (default subtle rim = 0.12).
        var softMat = SoftMaterialFactory.Create(new Color(0.85f, 0.78f, 0.72f));
        MakeCapsule(new Vector3(1f, 1.2f, 0f), softMat);

        var camera = new Camera3D { Current = true, Position = new Vector3(0f, 1.2f, 5f), Fov = 45f };
        camera.LookAtFromPosition(camera.Position, new Vector3(0f, 1.2f, 0f), Vector3.Up);
        AddChild(camera);

        GD.Print("ShadingAB: ready");
    }

    private void MakeCapsule(Vector3 pos, Material mat)
    {
        var mesh = new MeshInstance3D
        {
            Position = pos,
            MaterialOverride = mat,
            Mesh = new CapsuleMesh { Radius = 0.4f, Height = 1.2f },
        };
        AddChild(mesh);
    }

    public override void _Process(double delta)
    {
        if (_cap) return;
        _f++;
        if (_f < 8) return;
        _cap = true;
        var img = GetViewport().GetTexture().GetImage();
        var dir = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
            "OpenMakaiRanchCapture");
        System.IO.Directory.CreateDirectory(dir);
        var path = System.IO.Path.Combine(dir, "shading_ab.png");
        GD.Print($"ShadingAB: {(img.SavePng(path) == Error.Ok ? "SAVED " + path : "SAVE FAIL")}");
        GetTree().Quit(0);
    }

    private int _f;
    private bool _cap;
}
