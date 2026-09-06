using Godot;
using OpenMakaiRanch.Core.Resources;

namespace OpenMakaiRanch.Character;

/// <summary>
/// Pure, Node-free factory for <see cref="CharacterVisualProfile"/> and <see cref="CharacterAvatar3D"/>.
/// Kept static so profile construction, color mapping, and the adult-eligibility gate can be
/// verified headlessly without a Godot tree.
///
/// Per the 3D_REMAKE_PLAN: "Missing art gets an honest debug stand-in, not a random hero model."
/// and "Adult-specific presentation has a separate fail-closed identity/design gate; this plan
/// does not grant content approval."
///
/// CHAR-001 ships only stand-ins: <see cref="CreateProfile"/> always sets <c>IsDebugStandIn = true</c>.
/// The gate (<see cref="CanUseRealAvatar"/>) is the gate CHAR-002/ART-002 must pass before a
/// stand-in may be promoted to a real character model.
/// </summary>
public static class CharacterAvatarFactory
{
    /// <summary>
    /// Build a presentation-only profile from a <see cref="CharacterDefinition"/>.
    /// The definition is read; it is never mutated. All gameplay state stays in the definition/state.
    /// </summary>
    public static CharacterVisualProfile CreateProfile(CharacterDefinition? definition)
    {
        if (definition is null)
        {
            throw new System.ArgumentNullException(nameof(definition));
        }

        return new CharacterVisualProfile
        {
            DefinitionId = definition.Id,
            DisplayName = definition.DisplayName,
            AdultEligibility = definition.AdultEligibility,
            Provenance = definition.Provenance,
            // CHAR-001: no real assets exist yet. Honest stand-in, not a hero model.
            IsDebugStandIn = true,
            BodyColor = MapSkinColor(definition.SkinColor),
            HeadColor = MapHairColor(definition.HairColor),
        };
    }

    /// <summary>
    /// Fail-closed gate: a real (non-stand-in) avatar requires confirmed adulthood.
    /// Unknown/Minor/Ambiguous all return false. This is the gate CHAR-002/ART-002 must pass
    /// before promoting a stand-in to a real character model. CHAR-001 does not ship real
    /// assets, so a true result here does not unlock any real model yet.
    /// </summary>
    public static bool CanUseRealAvatar(CharacterDefinition? definition)
    {
        return definition is not null
            && definition.AdultEligibility == AdultEligibility.ConfirmedAdult;
    }

    /// <summary>
    /// Build a <see cref="CharacterAvatar3D"/> node from a profile. The node is not in the
    /// tree; call <c>AddChild</c> to attach it, then <see cref="CharacterAvatar3D.Rebuild"/>
    /// runs on <c>_Ready</c> to generate the stand-in geometry.
    /// </summary>
    public static CharacterAvatar3D BuildAvatar(CharacterVisualProfile profile)
    {
        if (profile is null)
        {
            throw new System.ArgumentNullException(nameof(profile));
        }

        return new CharacterAvatar3D { Profile = profile };
    }

    /// <summary>Map a neutral skin-color name to a stand-in body tint. Unknown → default.</summary>
    public static Color MapSkinColor(string skinColor)
    {
        return (skinColor ?? string.Empty).ToLowerInvariant() switch
        {
            "pale" => new Color(0.92f, 0.88f, 0.85f),
            "dark" => new Color(0.55f, 0.42f, 0.35f),
            _ => new Color(0.8f, 0.72f, 0.65f),
        };
    }

    /// <summary>Map a neutral hair-color name to a stand-in head tint. Unknown → default.</summary>
    public static Color MapHairColor(string hairColor)
    {
        return (hairColor ?? string.Empty).ToLowerInvariant() switch
        {
            "black" => new Color(0.1f, 0.1f, 0.1f),
            "blonde" => new Color(0.85f, 0.75f, 0.4f),
            "red" => new Color(0.7f, 0.3f, 0.2f),
            "silver" => new Color(0.85f, 0.85f, 0.9f),
            "pink" => new Color(0.9f, 0.5f, 0.7f),
            _ => new Color(0.4f, 0.3f, 0.2f),
        };
    }
}
