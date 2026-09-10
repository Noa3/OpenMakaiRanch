namespace OpenMakaiRanch.App;

/// <summary>
/// Single source of truth for scalable 3D presentation budgets.
/// Gameplay never reads these values; they only control rendering/presentation cost.
/// </summary>
public readonly record struct GraphicsQualityProfile(
    string Name,
    float RenderScale,
    float WorldDetailScale,
    float DecorationDensity,
    float ParticleDensity,
    int SurfaceMarkBudget,
    int PuddleBudget,
    int DefaultFrameRateLimit,
    bool Shadows,
    bool Atmosphere,
    bool AdvancedLighting,
    bool Glow,
    bool Ssao,
    bool Ssil,
    bool Ssr,
    bool VolumetricFog,
    int SsrMaxSteps,
    float VolumetricFogLength)
{
    public static GraphicsQualityProfile Resolve(string? quality)
    {
        return (quality ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "low" => new(
                "Low", 0.60f, 0.65f, 0.45f, 0.30f,
                16, 3, 45,
                Shadows: false, Atmosphere: false, AdvancedLighting: false,
                Glow: false, Ssao: false, Ssil: false, Ssr: false, VolumetricFog: false,
                SsrMaxSteps: 0, VolumetricFogLength: 0f),

            "high" => new(
                "High", 1.00f, 1.00f, 0.95f, 0.90f,
                64, 10, 60,
                Shadows: true, Atmosphere: true, AdvancedLighting: true,
                Glow: true, Ssao: true, Ssil: true, Ssr: true, VolumetricFog: true,
                SsrMaxSteps: 56, VolumetricFogLength: 64f),

            "ultra" => new(
                "Ultra", 1.00f, 1.15f, 1.15f, 1.20f,
                96, 14, 120,
                Shadows: true, Atmosphere: true, AdvancedLighting: true,
                Glow: true, Ssao: true, Ssil: true, Ssr: true, VolumetricFog: true,
                SsrMaxSteps: 96, VolumetricFogLength: 96f),

            // Custom starts from Medium cost ceilings; individual SettingsState toggles still decide
            // which effects are enabled. This avoids a "Custom" toggle accidentally implying Ultra.
            "custom" => new(
                "Custom", 0.80f, 0.85f, 0.70f, 0.60f,
                36, 6, 60,
                Shadows: true, Atmosphere: true, AdvancedLighting: true,
                Glow: true, Ssao: true, Ssil: false, Ssr: false, VolumetricFog: false,
                SsrMaxSteps: 56, VolumetricFogLength: 64f),

            _ => new(
                "Medium", 0.80f, 0.85f, 0.70f, 0.60f,
                36, 6, 60,
                Shadows: true, Atmosphere: true, AdvancedLighting: true,
                Glow: false, Ssao: true, Ssil: false, Ssr: false, VolumetricFog: false,
                SsrMaxSteps: 56, VolumetricFogLength: 64f)
        };
    }
}
