using System;
using System.Collections.Generic;
using Godot;
using OpenMakaiRanch.App;
using GEnvironment = Godot.Environment;

namespace OpenMakaiRanch.Visuals;

/// <summary>
/// Isolated, original material study. OwnWorld3D and viewport-local settings keep the ranch,
/// cameras, global render settings, clock and saves untouched. This is not final character art.
/// </summary>
public partial class AnimeLookDev : Control
{
    public static readonly string[] LightNames = { "Neutral", "Daylight", "Interior", "Sunset", "Night" };
    public SubViewport StudyViewport { get; private set; } = null!;
    public GEnvironment StudyEnvironment { get; private set; } = null!;
    public Camera3D StudyCamera { get; private set; } = null!;
    public Node3D SpecimenRoot { get; private set; } = null!;
    public string QualityName { get; private set; } = "High";
    public int LightPreset { get; private set; }
    public bool MatteComparison { get; private set; }
    public bool Portrait { get; private set; }
    public bool DepthOfField { get; private set; }
    public int SampleCount => _samples.Count;
    private readonly List<(MeshInstance3D Mesh, AnimeSurfaceProfile Profile)> _samples = new();
    private DirectionalLight3D _key = null!, _fill = null!, _rim = null!;
    private Label _status = null!;
    private Node3D _world = null!;
    private bool ForwardPlus => RenderingServer.GetCurrentRenderingMethod().ToString() == "forward_plus";

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var column = new VBoxContainer();
        AddChild(column);
        column.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var title = new Label { Text = "ANIME MATERIAL LAB  /  original test geometry, not final character art" };
        column.AddChild(title);
        var controls = new GridContainer { Columns = 3 };
        column.AddChild(controls);
        var light = new OptionButton { TooltipText = "Lighting only; never advances game time." };
        foreach (var name in LightNames) light.AddItem(name);
        light.ItemSelected += index => SetLighting((int)index);
        controls.AddChild(light);
        var quality = new OptionButton();
        foreach (var name in new[] { "Low", "Medium", "High", "Ultra" }) quality.AddItem(name);
        quality.Select(2);
        quality.ItemSelected += index => SetQuality(quality.GetItemText((int)index));
        controls.AddChild(quality);
        var matte = new CheckButton { Text = "Matte reference" };
        matte.Toggled += SetMatte;
        controls.AddChild(matte);
        var portrait = new CheckButton { Text = "Portrait camera" };
        portrait.Toggled += SetPortrait;
        controls.AddChild(portrait);
        var dof = new CheckButton { Text = "Portrait depth of field" };
        dof.Toggled += SetDepthOfField;
        controls.AddChild(dof);
        var rotate = new Button { Text = "Rotate subject 30 degrees" };
        rotate.Pressed += () => SpecimenRoot.RotateY(Mathf.DegToRad(30f));
        controls.AddChild(rotate);
        Resized += () => controls.Columns = Size.X < 700f ? 2 : 3;
        _status = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        column.AddChild(_status);
        var frame = new SubViewportContainer
        {
            Stretch = true, SizeFlagsVertical = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(320f, 240f), MouseFilter = MouseFilterEnum.Stop
        };
        column.AddChild(frame);
        StudyViewport = new SubViewport
        {
            Name = "MaterialStudyViewport", OwnWorld3D = true, Size = new Vector2I(1280, 600),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always, TransparentBg = false
        };
        frame.AddChild(StudyViewport);
        _world = new Node3D();
        StudyViewport.AddChild(_world);
        StudyEnvironment = new GEnvironment
        {
            BackgroundMode = GEnvironment.BGMode.Color, BackgroundColor = new Color(0.16f, 0.18f, 0.22f),
            AmbientLightSource = GEnvironment.AmbientSource.Color, AmbientLightColor = new Color(0.73f, 0.78f, 0.87f),
            AmbientLightEnergy = 0.35f, ReflectedLightSource = GEnvironment.ReflectionSource.Sky,
            Sky = new Sky { SkyMaterial = new ProceduralSkyMaterial
            {
                SkyTopColor = new Color(0.32f, 0.43f, 0.62f), SkyHorizonColor = new Color(0.73f, 0.77f, 0.82f),
                GroundBottomColor = new Color(0.12f, 0.13f, 0.16f), GroundHorizonColor = new Color(0.53f, 0.54f, 0.55f)
            } },
            TonemapMode = GEnvironment.ToneMapper.Agx, TonemapExposure = 1f,
            SsaoRadius = 0.3f, SsaoIntensity = 0.5f,
            GlowIntensity = 0.25f, GlowBloom = 0.02f
        };
        _world.AddChild(new WorldEnvironment { Environment = StudyEnvironment });
        _key = Light(new Vector3(-35f, -25f, 0f), true);
        _fill = Light(new Vector3(-12f, 65f, 0f), false);
        _rim = Light(new Vector3(-25f, 150f, 0f), false);
        StudyCamera = new Camera3D { Current = true, Fov = 32f, Near = 0.05f, Far = 40f };
        _world.AddChild(StudyCamera);
        var floor = new MeshInstance3D
        {
            Mesh = new PlaneMesh { Size = new Vector2(14f, 14f) },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.24f, 0.26f, 0.29f), Roughness = 0.9f }
        };
        _world.AddChild(floor);
        SpecimenRoot = new Node3D { Name = "OriginalClothedMannequin" };
        _world.AddChild(SpecimenRoot);
        BuildStudy();
        SetQuality("High");
        SetLighting(0);
        SetPortrait(false);
    }

    private DirectionalLight3D Light(Vector3 rotation, bool shadows)
    {
        var light = new DirectionalLight3D
        {
            RotationDegrees = rotation, ShadowEnabled = shadows, DirectionalShadowMaxDistance = 20f,
            LightAngularDistance = 2f
        };
        _world.AddChild(light);
        return light;
    }

    private void BuildStudy()
    {
        var skin = AnimeSurfaceProfile.Create(AnimeSurfaceKind.Skin, new Color(0.82f, 0.66f, 0.55f));
        var cloth = AnimeSurfaceProfile.Create(AnimeSurfaceKind.Cloth, new Color(0.17f, 0.30f, 0.28f));
        var hair = AnimeSurfaceProfile.Create(AnimeSurfaceKind.Hair, new Color(0.24f, 0.43f, 0.35f));
        var eye = AnimeSurfaceProfile.Create(AnimeSurfaceKind.Eye, new Color(0.9f, 0.92f, 0.88f));
        eye.ProceduralIris = true;
        eye.IrisColor = new Color(0.73f, 0.46f, 0.10f);
        // Fully clothed and deliberately geometric. No external identity, rig or design is implied.
        Capsule(SpecimenRoot, cloth, new Vector3(0f, 0.99f, 0f), 0.25f, 0.92f);
        Sphere(SpecimenRoot, skin, new Vector3(0f, 1.63f, 0f), new Vector3(0.20f, 0.24f, 0.19f));
        Sphere(SpecimenRoot, hair, new Vector3(0f, 1.78f, -0.03f), new Vector3(0.22f, 0.14f, 0.20f));
        Sphere(SpecimenRoot, skin, new Vector3(0f, 1.62f, 0.19f), new Vector3(0.023f, 0.033f, 0.034f));
        foreach (var sign in new[] { -1f, 1f })
        {
            Capsule(SpecimenRoot, cloth, new Vector3(sign * 0.14f, 0.38f, 0f), 0.11f, 0.65f);
            Capsule(SpecimenRoot, cloth, new Vector3(sign * 0.32f, 1.12f, 0f), 0.095f, 0.55f);
            Sphere(SpecimenRoot, skin, new Vector3(sign * 0.32f, 0.83f, 0f), new Vector3(0.074f, 0.095f, 0.074f));
            Sphere(SpecimenRoot, eye, new Vector3(sign * 0.072f, 1.665f, 0.174f), new Vector3(0.045f, 0.030f, 0.027f));
            Sphere(SpecimenRoot, hair, new Vector3(sign * 0.19f, 1.62f, -0.04f), new Vector3(0.055f, 0.19f, 0.12f));
        }
        var samples = new[]
        {
            (skin, new Vector3(-1.15f, 1.30f, 0f), "SKIN"),
            (AnimeSurfaceProfile.Create(AnimeSurfaceKind.Skin, new Color(0.28f, 0.17f, 0.12f)), new Vector3(-1.15f, 0.48f, 0f), "SKIN / DARK"),
            (hair, new Vector3(1.15f, 1.30f, 0f), "HAIR / FLOW"),
            (eye, new Vector3(1.15f, 0.48f, 0f), "EYE / OPAQUE")
        };
        foreach (var (profile, position, label) in samples)
        {
            Sphere(_world, profile, position, Vector3.One * 0.31f);
            _world.AddChild(new Label3D
            {
                Text = label, Position = position + new Vector3(0f, -0.40f, 0f),
                FontSize = 28, PixelSize = 0.003f, Billboard = BaseMaterial3D.BillboardModeEnum.Enabled
            });
        }
        // A fabric panel tests a broad highlight separately from the body silhouette.
        var panel = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = new Vector3(0.8f, 0.10f, 0.6f) }, Position = new Vector3(0f, 0.05f, 0.55f)
        };
        _world.AddChild(panel);
        _samples.Add((panel, cloth));
    }

    private void Sphere(Node3D parent, AnimeSurfaceProfile profile, Vector3 position, Vector3 scale)
    {
        var mesh = new MeshInstance3D
        {
            Mesh = new SphereMesh { Radius = 1f, Height = 2f, RadialSegments = 48, Rings = 24 },
            Position = position, Scale = scale
        };
        parent.AddChild(mesh);
        _samples.Add((mesh, profile));
    }

    private void Capsule(Node3D parent, AnimeSurfaceProfile profile, Vector3 position, float radius, float height)
    {
        var mesh = new MeshInstance3D { Mesh = new CapsuleMesh { Radius = radius, Height = height }, Position = position };
        parent.AddChild(mesh);
        _samples.Add((mesh, profile));
    }

    public void SetQuality(string name)
    {
        QualityName = GraphicsQualityProfile.Resolve(name).Name;
        var policy = GraphicsQualityProfile.Resolve(QualityName);
        // Native-size comparison on every tier: resolution is held constant to judge materials.
        StudyViewport.Scaling3DScale = 1f;
        StudyViewport.Msaa3D = QualityName == "Low" ? Viewport.Msaa.Msaa2X : Viewport.Msaa.Msaa4X;
        // Local temporal accumulation reduces fine highlight/soft-shadow shimmer in the static study.
        // Do not apply this globally: animated production characters need separate ghosting review.
        StudyViewport.UseTaa = ForwardPlus && (QualityName is "High" or "Ultra");
        StudyEnvironment.SsaoEnabled = ForwardPlus && policy.Ssao;
        StudyEnvironment.GlowEnabled = ForwardPlus && policy.Glow;
        // SSR/SSIL/GI/fog are not stacked blindly on a material test. World ownership stays elsewhere.
        StudyEnvironment.SsrEnabled = false;
        StudyEnvironment.SsilEnabled = false;
        _key.ShadowEnabled = policy.Shadows;
        ApplyMaterials();
        ApplyCamera();
        UpdateStatus();
    }

    public void SetMatte(bool enabled) { MatteComparison = enabled; ApplyMaterials(); UpdateStatus(); }

    private void ApplyMaterials()
    {
        var shared = new Dictionary<AnimeSurfaceProfile, Material>();
        foreach (var (mesh, profile) in _samples)
        {
            if (!shared.TryGetValue(profile, out var material))
            {
                material = MatteComparison
                    ? new StandardMaterial3D
                    {
                        AlbedoColor = profile.Kind == AnimeSurfaceKind.Eye && profile.ProceduralIris ? profile.IrisColor : profile.BaseColor,
                        AlbedoTexture = profile.AlbedoTexture, Roughness = 1f, MetallicSpecular = 0f
                    }
                    : AnimeMaterialFactory.Create(profile, QualityName);
                shared.Add(profile, material);
            }
            mesh.MaterialOverride = material;
        }
    }

    public void SetLighting(int index)
    {
        LightPreset = Math.Clamp(index, 0, LightNames.Length - 1);
        (_key.LightColor, _key.LightEnergy, _fill.LightEnergy, _rim.LightEnergy, StudyEnvironment.AmbientLightEnergy) = LightPreset switch
        {
            1 => (new Color(1f, 0.95f, 0.87f), 1.25f, 0.25f, 0.45f, 0.40f),
            2 => (new Color(1f, 0.83f, 0.68f), 0.85f, 0.18f, 0.28f, 0.24f),
            3 => (new Color(1f, 0.64f, 0.39f), 1.15f, 0.20f, 0.75f, 0.25f),
            4 => (new Color(0.48f, 0.65f, 1f), 0.50f, 0.14f, 0.60f, 0.14f),
            _ => (Colors.White, 1.00f, 0.22f, 0.40f, 0.35f)
        };
        _fill.LightColor = LightPreset == 3 ? new Color(0.55f, 0.66f, 1f) : new Color(0.75f, 0.84f, 1f);
        _rim.LightColor = LightPreset == 4 ? new Color(0.60f, 0.72f, 1f) : new Color(1f, 0.91f, 0.81f);
        UpdateStatus();
    }

    public void SetPortrait(bool enabled) { Portrait = enabled; ApplyCamera(); UpdateStatus(); }
    public void SetDepthOfField(bool enabled) { DepthOfField = enabled; ApplyCamera(); UpdateStatus(); }

    private void ApplyCamera()
    {
        var target = Portrait ? new Vector3(0f, 1.60f, 0f) : new Vector3(0f, 1.02f, 0f);
        StudyCamera.Position = Portrait ? new Vector3(0.15f, 1.75f, 2.0f) : new Vector3(0.25f, 2.05f, 6.6f);
        StudyCamera.LookAt(target, Vector3.Up);
        StudyCamera.Attributes = new CameraAttributesPractical
        {
            AutoExposureEnabled = false,
            DofBlurFarEnabled = ForwardPlus && Portrait && DepthOfField && QualityName != "Low",
            DofBlurNearEnabled = false, DofBlurFarDistance = StudyCamera.Position.DistanceTo(target) + 0.20f,
            DofBlurFarTransition = 1.5f, DofBlurAmount = 0.12f
        };
    }

    private void UpdateStatus()
    {
        _status.Text = $"{LightNames[LightPreset]} | {QualityName} | {RenderingServer.GetCurrentRenderingMethod()} | "
            + (MatteComparison ? "Matte reference" : "Anime PBR")
            + " | Native resolution, fixed exposure; no gameplay/settings/save changes.";
    }
}
