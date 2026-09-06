using Godot;
using OpenMakaiRanch.Core.Resources;

namespace OpenMakaiRanch.Character;

/// <summary>
/// Presentation-only visual profile mapping a stable <see cref="DefinitionId"/> to render
/// parameters. Per the 3D_REMAKE_PLAN: "Separate mesh, skeleton, clothing, expressions and
/// morph presentation from gameplay state." This type carries no gameplay numbers — no HP,
/// skills, bond, or reward. It is a Resource so it can be authored, saved, and swapped without
/// touching simulation state.
///
/// <see cref="IsDebugStandIn"/> is the honesty flag: true means "this is a placeholder
/// geometry for scale/collision, not a character model." CHAR-001 ships only stand-ins.
/// </summary>
[GlobalClass]
public partial class CharacterVisualProfile : Resource
{
    /// <summary>Stable identity; the definition key (e.g. "slay"). Never a runtime index.</summary>
    public string DefinitionId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Fail-closed eligibility carried forward so downstream gates see the same value.</summary>
    public AdultEligibility AdultEligibility { get; set; } = AdultEligibility.Unknown;

    public CharacterProvenance Provenance { get; set; } = CharacterProvenance.Unknown;

    /// <summary>
    /// True when this profile renders honest placeholder geometry (CHAR-001 default).
    /// A false value implies real authored assets exist (CHAR-002/ART-002), which requires
    /// passing the fail-closed adult-eligibility + design gate first.
    /// </summary>
    public bool IsDebugStandIn { get; set; } = true;

    /// <summary>Body (capsule) tint. Neutral, derived from the definition's skin color.</summary>
    public Color BodyColor { get; set; } = new Color(0.8f, 0.72f, 0.65f);

    /// <summary>Head (sphere) tint. Neutral, derived from the definition's hair color.</summary>
    public Color HeadColor { get; set; } = new Color(0.4f, 0.3f, 0.2f);

    /// <summary>Approximate height in meters, for scale and camera clearance.</summary>
    public float Height { get; set; } = 1.7f;
}
