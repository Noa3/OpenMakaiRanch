using Godot;

namespace OpenMakaiRanch.Visuals;

/// <summary>Artist-authored .tres preferences. Not a saved second personality/needs simulation.</summary>
[GlobalClass]
public partial class PresenceStyleProfile : Resource
{
    [Export(PropertyHint.Range, "0,1")] public float Sociability { get; set; } = .5f;
    [Export(PropertyHint.Range, "0,1")] public float Expressiveness { get; set; } = .5f;
    [Export(PropertyHint.Range, "0,1")] public float Composure { get; set; } = .5f;
    [Export(PropertyHint.Range, "0,1")] public float Confidence { get; set; } = .5f;
    [Export(PropertyHint.Range, "0,1")] public float Playfulness { get; set; } = .5f;
    [Export(PropertyHint.Range, "0,1")] public float Curiosity { get; set; } = .5f;
    [Export(PropertyHint.Range, "0,1")] public float Diligence { get; set; } = .5f;
    [Export(PropertyHint.Range, "0,1")] public float Warmth { get; set; } = .5f;
    public PresenceTemperament Values => new PresenceTemperament(Sociability, Expressiveness, Composure,
        Confidence, Playfulness, Curiosity, Diligence, Warmth).Validated();
}
