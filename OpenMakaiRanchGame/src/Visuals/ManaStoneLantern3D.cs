using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.Visuals;

/// <summary>Original opaque mana-stone lamp: readable housing, warm light, no flame/flicker or fuel simulation.</summary>
public partial class ManaStoneLantern3D : Node3D
{
    public OmniLight3D LampLight { get; private set; } = null!;
    public StandardMaterial3D StoneMaterial { get; private set; } = null!;
    public MeshInstance3D Stone { get; private set; } = null!;
    public float LightLevel { get; private set; }
    public override void _Ready()
    {
        var wood = new StandardMaterial3D { AlbedoColor = new Color("70563b"), Roughness = .86f };
        var metal = new StandardMaterial3D { AlbedoColor = new Color("35413b"), Metallic = .65f, Roughness = .42f };
        StoneMaterial = new StandardMaterial3D { AlbedoColor = new Color("f7de9e"), Roughness = .48f,
            EmissionEnabled = true, Emission = new Color("ffcf79"), EmissionEnergyMultiplier = .04f };
        Part("Post", new CylinderMesh { TopRadius = .065f, BottomRadius = .085f, Height = 1.35f }, new(0,.675f,0), wood);
        Part("Base", new CylinderMesh { TopRadius = .23f, BottomRadius = .19f, Height = .075f, RadialSegments = 12 }, new(0,1.36f,0), metal);
        Part("Cap", new CylinderMesh { TopRadius = .08f, BottomRadius = .27f, Height = .17f, RadialSegments = 12 }, new(0,1.87f,0), metal);
        for (var i = 0; i < 4; i++) {
            var a = Mathf.Pi * (.25f + i * .5f);
            Part("Frame" + i, new BoxMesh { Size = new Vector3(.027f,.43f,.027f) },
                new Vector3(Mathf.Cos(a)*.17f,1.61f,Mathf.Sin(a)*.17f), metal);
        }
        Stone = Part("ManaStone", new SphereMesh { Radius = .12f, Height = .32f, RadialSegments = 6, Rings = 3 }, new(0,1.59f,0), StoneMaterial);
        LampLight = new OmniLight3D { Name = "LocalLight", Position = new(0,1.65f,0),
            LightColor = new Color("ffe0a8"), LightEnergy = 0f, OmniRange = 4.5f,
            ShadowEnabled = false, DistanceFadeEnabled = true, DistanceFadeBegin = 12f, DistanceFadeLength = 6f };
        AddChild(LampLight);
    }
    public void Configure(float night, string? quality, bool shadowsAllowed, bool viewerSheltered, bool enabled = true)
    {
        if (LampLight is null) return;
        var level = enabled ? Mathf.SmoothStep(.1f,.75f,PresenceTemperament.Unit(night)) : 0f;
        LightLevel = level;
        StoneMaterial.EmissionEnergyMultiplier = .035f + 1.4f * level;
        var tier = GraphicsQualityProfile.Resolve(quality).Name;
        LampLight.LightEnergy = tier == "Low" || viewerSheltered ? 0f : level * .85f;
        LampLight.Visible = LampLight.LightEnergy > .001f;
        LampLight.ShadowEnabled = shadowsAllowed && (tier is "High" or "Ultra");
    }
    private MeshInstance3D Part(string name, Mesh mesh, Vector3 position, Material material)
    {
        var part = new MeshInstance3D { Name = name, Mesh = mesh, Position = position, MaterialOverride = material };
        AddChild(part); return part;
    }
}
