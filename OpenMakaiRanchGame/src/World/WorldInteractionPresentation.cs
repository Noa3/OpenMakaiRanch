using System;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Character;

namespace OpenMakaiRanch.World;

/// <summary>
/// Presentation-only snapshot of the closest thing the player can interact with. It deliberately
/// carries no gameplay callback; activation remains owned by RanchGreyboxController/TownWorldController.
/// </summary>
public readonly record struct WorldInteractionPresentation(
    Node3D? TargetNode,
    string Label,
    float Distance,
    float InteractionRange,
    bool Available,
    string UnavailableReason)
{
    public bool HasTarget => TargetNode is not null && GodotObject.IsInstanceValid(TargetNode);
    public bool InRange => HasTarget && Distance <= InteractionRange;

    public static WorldInteractionPresentation None(float interactionRange) =>
        new(null, string.Empty, float.PositiveInfinity, interactionRange, false, string.Empty);
}

public partial class RanchGreyboxController
{
    /// <summary>Returns the same nearest-target decision used by the actual ranch interaction.</summary>
    public WorldInteractionPresentation GetInteractionPresentation()
    {
        UpdateNearbyStation();

        CharacterAvatar3D? npc = null;
        var npcValid = !string.IsNullOrWhiteSpace(_nearbyCharacterId)
            && Roster is not null
            && Roster.TryGetAvatar(_nearbyCharacterId, out npc)
            && npc is not null;
        var stationValid = _nearbyStation is not null && GodotObject.IsInstanceValid(_nearbyStation);
        var travelValid = _travelPortal is not null && GodotObject.IsInstanceValid(_travelPortal);

        if (travelValid
            && (!npcValid || _travelPortalDistance <= _nearbyCharacterDistance)
            && (!stationValid || _travelPortalDistance <= _nearbyDistance))
        {
            return new WorldInteractionPresentation(
                _travelPortal,
                _travelPortal!.Prompt,
                _travelPortalDistance,
                InteractionRange,
                true,
                string.Empty);
        }

        if (npcValid && (!stationValid || _nearbyCharacterDistance < _nearbyDistance))
        {
            return new WorldInteractionPresentation(
                npc,
                $"Talk to {ResolveCharacterName(_nearbyCharacterId)}",
                _nearbyCharacterDistance,
                InteractionRange,
                true,
                string.Empty);
        }

        if (stationValid)
        {
            return new WorldInteractionPresentation(
                _nearbyStation,
                _nearbyStation!.Label,
                _nearbyDistance,
                InteractionRange,
                _nearbyStation.IsAvailable,
                _nearbyStation.UnavailableReason ?? string.Empty);
        }

        return WorldInteractionPresentation.None(InteractionRange);
    }
}

public partial class TownWorldController
{
    /// <summary>Returns the same nearest-target decision used by the actual town interaction.</summary>
    public WorldInteractionPresentation GetInteractionPresentation()
    {
        UpdateNearbyTargets();

        CharacterAvatar3D? companion = null;
        var companionValid = !string.IsNullOrWhiteSpace(_nearbyCompanionId)
            && Companion is not null
            && Companion.TryGetAvatar(_nearbyCompanionId, out companion)
            && companion is not null;
        var serviceValid = _nearbyService is not null && GodotObject.IsInstanceValid(_nearbyService);
        var portalValid = _returnPortal is not null && GodotObject.IsInstanceValid(_returnPortal);

        if (companionValid
            && (!portalValid || _nearbyCompanionDistance < _returnPortalDistance)
            && (!serviceValid || _nearbyCompanionDistance < _nearbyServiceDistance))
        {
            var character = GameRoot.Instance?.Roster.Find(_nearbyCompanionId);
            var name = character is null
                ? _nearbyCompanionId
                : (!string.IsNullOrWhiteSpace(character.DisplayNameOverride)
                    ? character.DisplayNameOverride
                    : GameRoot.Instance!.Roster.DefinitionFor(character).DisplayName);
            return new WorldInteractionPresentation(
                companion,
                $"Talk to {name}",
                _nearbyCompanionDistance,
                InteractionRange,
                true,
                string.Empty);
        }

        if (portalValid && (!serviceValid || _returnPortalDistance <= _nearbyServiceDistance))
        {
            return new WorldInteractionPresentation(
                _returnPortal,
                _returnPortal!.Prompt,
                _returnPortalDistance,
                InteractionRange,
                true,
                string.Empty);
        }

        if (serviceValid)
        {
            return new WorldInteractionPresentation(
                _nearbyService,
                _nearbyService!.Label,
                _nearbyServiceDistance,
                InteractionRange,
                _nearbyService.IsAvailable,
                _nearbyService.UnavailableReason ?? string.Empty);
        }

        return WorldInteractionPresentation.None(InteractionRange);
    }
}
