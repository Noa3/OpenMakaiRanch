using Godot;

namespace OpenMakaiRanch.Character;

/// <summary>
/// Pure, Node-free soft anime shading math. Mirrors the GLSL in
/// <see cref="SoftShaderSource"/> 1:1 so the shader parameters can be verified
/// headlessly (no scene tree, no GPU).
///
/// Design intent (per user direction): a SOFT, NATURAL anime look — no hard
/// corners, no cel banding, no hard shadow edge. This is a smooth lighting
/// model, not a two-tone toon band:
///   - soft diffuse: a smooth ramp (no threshold band)
///   - hemisphere ambient: natural sky/ground color bleed
///   - gentle rim: subtle fresnel edge glow, not a hard outline
/// </summary>
public static class SoftShadingMath
{
    /// <summary>
    /// The soft shading parameter set. These map 1:1 onto shader uniforms
    /// (see <see cref="SoftMaterialFactory"/>) so the C# math and the GPU
    /// shader always agree on the same numbers.
    /// </summary>
    public class SoftParameters
    {
        /// <summary>Albedo (surface base color — profile skin/hair tint).</summary>
        public Color BaseColor { get; set; } = new(0.85f, 0.78f, 0.72f);

        /// <summary>Direct light color (multiplies the diffuse term).</summary>
        public Color LightColor { get; set; } = new(1f, 0.98f, 0.94f);

        /// <summary>Sky hemisphere ambient (upper hemispheres pick this up).</summary>
        public Color SkyColor { get; set; } = new(0.55f, 0.60f, 0.72f);

        /// <summary>Ground hemisphere ambient (lower hemispheres pick this up).</summary>
        public Color GroundColor { get; set; } = new(0.30f, 0.28f, 0.26f);

        /// <summary>
        /// Ambient overall strength. Keeps the model readable in shadow without
        /// a hard dark band. 0.0 = pure diffuse; 1.0 = full hemisphere fill.
        /// </summary>
        public float AmbientStrength { get; set; } = 0.6f;

        /// <summary>
        /// Diffuse softness: how much the lit-to-shadow transition is spread
        /// out. Higher = softer, more natural falloff (closer to linear). The
        /// default is high enough that there is NO visible hard cel band.
        /// </summary>
        public float DiffuseSoftness { get; set; } = 0.9f;

        /// <summary>Rim (fresnel) color — the soft edge glow.</summary>
        public Color RimColor { get; set; } = new(1f, 0.96f, 0.9f);

        /// <summary>
        /// Rim strength. Kept LOW for a subtle, natural edge — not a hard
        /// outline. 0.0 = no rim.
        /// </summary>
        public float RimStrength { get; set; } = 0.25f;

        /// <summary>
        /// Rim falloff power: rim = (1 - dot(N,V))^power. Higher = tighter rim
        /// hugging the silhouette; lower = wider soft glow.
        /// </summary>
        public float RimPower { get; set; } = 2.0f;

        /// <summary>Diffuse energy multiplier (overall light response).</summary>
        public float LightEnergy { get; set; } = 1.0f;

        public static SoftParameters Default => new();
    }

    /// <summary>
    /// Soft diffuse factor in [0,1] from the Godot 4 spatial convention
    /// dot(N, -L). A smooth ramp — deliberately NOT a cel band, so there is no
    /// hard shadow corner. <paramref name="softness"/> spreads the transition;
    /// high values approach a linear, natural falloff.
    /// </summary>
    public static float SoftDiffuse(float lightDot, float softness)
    {
        // lightDot is clamped to [0,1] (lit half). A smoothstep over a wide
        // window gives a soft, natural ramp with no hard edge.
        float x = Mathf.Clamp(lightDot, 0f, 1f);
        if (softness <= 0f)
        {
            return x; // linear
        }
        // Blend between linear and a soft eased ramp. softness near 1 = eased.
        float eased = x * x * (3f - 2f * x); // smoothstep(0,1,x)
        return Mathf.Lerp(x, eased, Mathf.Clamp(softness, 0f, 1f));
    }

    /// <summary>
    /// Hemisphere ambient factor: 1 for up-facing surfaces (sky), 0 for
    /// down-facing (ground). Mirrors the GLSL <c>normal.y*0.5+0.5</c>.
    /// </summary>
    public static float HemisphereMix(float normalY)
        => Mathf.Clamp(normalY * 0.5f + 0.5f, 0f, 1f);

    /// <summary>
    /// Rim (fresnel) factor: 0 head-on, grows toward the silhouette. Mirrors
    /// the GLSL <c>pow(1 - dot(N,V), power)</c>.
    /// </summary>
    public static float RimFactor(float viewDot, float power)
        => Mathf.Pow(Mathf.Max(0f, 1f - Mathf.Clamp(viewDot, 0f, 1f)), Mathf.Max(0.01f, power));

    /// <summary>
    /// Composite per-channel shading, mirroring the GLSL fragment math 1:1:
    ///   albedo * (soft_diffuse * light + hemi_ambient) + rim
    /// Everything smooth — no hard band, no hard corner.
    /// </summary>
    public static Color Shade(float lightDot, float normalY, float viewDot, SoftParameters p)
    {
        float diffuse = SoftDiffuse(lightDot, p.DiffuseSoftness) * p.LightEnergy;
        float hemi = HemisphereMix(normalY);

        // Diffuse term: light color. Ambient: sky/ground blend, scaled.
        Color diffuseTerm = p.LightColor * p.BaseColor * diffuse;
        Color hemiColor = p.GroundColor.Lerp(p.SkyColor, hemi);
        Color ambientTerm = hemiColor * p.BaseColor * p.AmbientStrength;

        Color band = diffuseTerm + ambientTerm;

        // Soft rim edge glow, added at grazing angles.
        float rim = RimFactor(viewDot, p.RimPower) * p.RimStrength;
        Color rimColor = p.RimColor * p.BaseColor * rim;

        Color result = band + rimColor;
        return new Color(
            Mathf.Min(1f, result.R),
            Mathf.Min(1f, result.G),
            Mathf.Min(1f, result.B),
            1f);
    }
}
