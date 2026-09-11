using System;
using Godot;
using GEnvironment = Godot.Environment;

namespace OpenMakaiRanch.Visuals;

/// <summary>Isolated natural-meadow style study, not production terrain/navigation or an authored AAA asset.</summary>
public partial class PastoralLookDev : Control
{
    public SubViewport View { get; private set; } = null!;
    public WorldEnvironment EnvironmentNode { get; private set; } = null!;
    public Camera3D Camera { get; private set; } = null!;
    public PastoralSky SkyView { get; } = new();
    public ManaStoneLantern3D Lantern { get; private set; } = null!;
    public StandardMaterial3D Grass { get; } = new() { AlbedoColor = new Color("699e43"), Roughness = .93f };
    public ShaderMaterial Water { get; private set; } = null!;
    public double VisualSeconds { get; private set; }
    public bool Animate { get; set; }
    public bool ReducedMotion { get; set; }
    public float Night { get; private set; }
    public float CloudCover { get; private set; } = .24f;
    public bool SecondBody { get; private set; } = true;
    public string Tier { get; private set; } = "High";
    private Node3D _world = null!;
    private DirectionalLight3D _sun = null!;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var column = new VBoxContainer(); AddChild(column); column.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        column.AddChild(new Label { Text = "GREEN RANCH / sky + mana-stone lighting study / original placeholder landscape" });
        var row = new HFlowContainer(); column.AddChild(row);
        var time = new OptionButton(); foreach (var text in new[] { "Day", "Evening", "Night", "Overcast night" }) time.AddItem(text);
        time.ItemSelected += i => SetView(i == 0 ? 0 : i == 1 ? .48f : 1, i == 3 ? 1 : .24f, SecondBody, Tier);
        row.AddChild(time);
        var planet = new CheckButton { Text = "Companion world", ButtonPressed = true };
        planet.Toggled += value => SetView(Night, CloudCover, value, Tier); row.AddChild(planet);
        var low = new CheckButton { Text = "Low detail" }; low.Toggled += value => SetView(Night, CloudCover, SecondBody, value ? "Low" : "High"); row.AddChild(low);
        var lamp = new CheckButton { Text = "Inspect lantern" }; lamp.Toggled += SetLanternCamera; row.AddChild(lamp);
        var motion = new CheckButton { Text = "Flowing water", ButtonPressed = false }; motion.Toggled += value => Animate = value; row.AddChild(motion);
        var reduce = new CheckButton { Text = "Reduced motion" }; reduce.Toggled += value => ReducedMotion = value; row.AddChild(reduce);
        var frame = new SubViewportContainer { Stretch = true, SizeFlagsVertical = SizeFlags.ExpandFill, CustomMinimumSize = new(320,240) };
        column.AddChild(frame);
        View = new SubViewport { OwnWorld3D = true, Size = new(1280,600), RenderTargetUpdateMode = SubViewport.UpdateMode.Always, Msaa3D = Viewport.Msaa.Msaa2X };
        frame.AddChild(View); _world = new Node3D(); View.AddChild(_world);
        EnvironmentNode = new WorldEnvironment { Environment = new GEnvironment {
            AmbientLightSource = GEnvironment.AmbientSource.Color, AmbientLightColor = new Color("c4d4e5"), AmbientLightEnergy = .65f,
            TonemapMode = GEnvironment.ToneMapper.Agx, TonemapExposure = 1f,
            ReflectedLightSource = GEnvironment.ReflectionSource.Sky, GlowEnabled = false
        } };
        _world.AddChild(EnvironmentNode); SkyView.TryBind(EnvironmentNode);
        _sun = new DirectionalLight3D { RotationDegrees = new(-42,-30,0), LightEnergy = 1.3f, ShadowEnabled = true, DirectionalShadowMaxDistance = 45f };
        _world.AddChild(_sun);
        Camera = new Camera3D { Current = true, Fov = 52, Near = .05f, Far = 250 };
        _world.AddChild(Camera); SetLanternCamera(false);
        BuildLandscape();
        Lantern = new ManaStoneLantern3D { Position = new(2.0f,0,1.0f) }; _world.AddChild(Lantern);
        SetView(0,.24f,true,"High");
    }

    public void SetView(float night, float clouds, bool secondBody, string tier)
    {
        Night = PresenceTemperament.Unit(night); CloudCover = PresenceTemperament.Unit(clouds); SecondBody = secondBody; Tier = tier;
        SkyView.Configure(Night, CloudCover, _sun.GlobalBasis.Z, SecondBody);
        _sun.LightEnergy = Mathf.Lerp(1.3f,.18f,Night) * Mathf.Lerp(1f,.45f,CloudCover);
        _sun.LightColor = new Color("fff1d7").Lerp(new Color("9db9e5"),Night);
        _sun.ShadowEnabled = tier != "Low";
        EnvironmentNode.Environment.AmbientLightEnergy = Mathf.Lerp(.65f,.25f,Night);
        Lantern.Configure(Night,tier,false,false);
    }
    public void SetLanternCamera(bool close)
    {
        Camera.Position = close ? new Vector3(2.8f,1.8f,3.2f) : new Vector3(0,2.6f,10);
        Camera.LookAt(close ? new Vector3(2,1.55f,1) : new Vector3(0,3,-10),Vector3.Up);
    }
    public override void _Process(double delta) { if (Animate) AdvanceWater(delta,false); }
    public void AdvanceWater(double delta, bool paused)
    {
        if (paused || ReducedMotion || !double.IsFinite(delta) || delta <= 0) return;
        VisualSeconds = (VisualSeconds + Math.Min(delta,.1)) % 3600;
        Water.SetShaderParameter("flow_phase", (float)VisualSeconds);
    }
    public override void _ExitTree() => SkyView.Dispose();

    private void BuildLandscape()
    {
        AddMesh("Grass",new PlaneMesh { Size = new(140,140) },Vector3.Zero,Grass);
        var bank = new StandardMaterial3D { AlbedoColor = new Color("a7aa73"), Roughness = .93f };
        AddMesh("RiverBank",River(1.8f,.007f),Vector3.Zero,bank);
        Water = new ShaderMaterial { Shader = new Shader { Code = """
            shader_type spatial;
            render_mode cull_disabled;
            uniform float flow_phase = 0.0;
            void fragment() {
                float ripple = sin(UV.y * 260.0 - flow_phase * 0.85 + sin(UV.x * 7.0)*0.4);
                ALBEDO = mix(vec3(0.07,0.23,0.24),vec3(0.19,0.39,0.39),ripple*.16+.45);
                ROUGHNESS = 0.27; SPECULAR = 0.45;
            }
            """ } };
        AddMesh("RiverWater",River(1.5f,.012f),Vector3.Zero,Water);
        var bark = new StandardMaterial3D { AlbedoColor = new Color("6c5037"), Roughness = .94f };
        for (var i = 0; i < 18; i++) {
            var side = i % 2 == 0 ? -1f : 1f; var z = -5f - (i/2) * 5.5f; var x = side * (10 + i%3 * 2f);
            var height = 3.2f + i%4 * .35f;
            AddMesh("TreeTrunk"+i,new CylinderMesh { TopRadius = .11f, BottomRadius = .23f, Height = height, RadialSegments = 10 },new(x,height*.5f,z),bark);
            var leaves = new StandardMaterial3D { AlbedoColor = new Color("51853d").Lightened((i%3)*.045f), Roughness = .9f };
            for (var j = 0; j < 4; j++) {
                var a = j * Mathf.Tau / 3f;
                var crown = AddMesh("LeafCrown",new SphereMesh { Radius = 1, Height = 2, RadialSegments = 20, Rings = 10 },
                    new Vector3(x + Mathf.Cos(a)*.55f,height+.45f+j*.13f,z+Mathf.Sin(a)*.55f),leaves);
                crown.Scale = new(1.3f,1.1f,1.3f);
            }
        }
        // Distant ordinary green hills; no floating terrain, lava, glowing roots or gravity anomalies.
        for (var i = 0; i < 7; i++) {
            var hill = AddMesh("DistantHill",new SphereMesh { Radius = 1, Height = 2, RadialSegments = 32, Rings = 16 },new(-54+i*18,-2,-61-i%2*8),Grass);
            hill.Scale = new(22,6+i%3*2,16);
        }
    }
    private MeshInstance3D AddMesh(string name, Mesh mesh, Vector3 position, Material material)
    {
        var node = new MeshInstance3D { Name = name, Mesh = mesh, Position = position, MaterialOverride = material };
        _world.AddChild(node); return node;
    }
    private static ArrayMesh River(float width, float y)
    {
        const int rows = 96;
        var positions = new Vector3[(rows+1)*2]; var normals = new Vector3[positions.Length];
        var uv = new Vector2[positions.Length]; var indices = new int[rows*6];
        for (var i = 0; i <= rows; i++) {
            var t = i/(float)rows; var z = -55f + t*70f; var center = -4.9f + 1.5f*Mathf.Sin(z*.105f);
            for (var j = 0; j < 2; j++) { var k = i*2+j; positions[k] = new(center+(j-.5f)*width*2,y,z); normals[k]=Vector3.Up; uv[k]=new(j,t); }
            if (i==rows) continue;
            var n=i*2; var o=i*6;
            indices[o]=n; indices[o+1]=n+1; indices[o+2]=n+2;
            indices[o+3]=n+1; indices[o+4]=n+3; indices[o+5]=n+2;
        }
        var arrays = new Godot.Collections.Array(); arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex]=positions; arrays[(int)Mesh.ArrayType.Normal]=normals;
        arrays[(int)Mesh.ArrayType.TexUV]=uv; arrays[(int)Mesh.ArrayType.Index]=indices;
        var mesh = new ArrayMesh(); mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles,arrays); return mesh;
    }
}
