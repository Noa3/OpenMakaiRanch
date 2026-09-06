using System.Collections.Generic;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Character;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.World;

/// <summary>
/// Places CHAR-001 stand-in avatars at the logical greybox anchors for the current roster. Pure
/// presentation: it reads the shared simulation (each character's job + the phase) and positions
/// avatars via <see cref="RosterPlacementMath"/>. It moves no simulation state, assigns no work, and
/// owns no schedules — the avatars are honest stand-ins (CHAR-001), not a second work economy.
///
/// <see cref="Refresh"/> is idempotent: it reuses existing avatar nodes by character id and only adds
/// or removes what changed, so it is safe to call from the UI when assignments change.
/// </summary>
public partial class RosterRig : Node3D
{
    private readonly Dictionary<string, CharacterAvatar3D> _avatars = new();

    public int AvatarCount => _avatars.Count;

    /// <summary>
    /// Rebuild the avatar set for the current roster. Stable per-character id; deterministic spread.
    /// Returns the number of avatars placed.
    /// </summary>
    public int Refresh(GameRoot game)
    {
        var roster = game.Roster;
        var schedule = game.Schedule;
        var data = game.Data;

        // Group roster order by anchor so mates stand side by side (stable ordinal = roster index).
        var seenPerAnchor = new Dictionary<string, int>();
        var desired = new Dictionary<string, (RosterPlacement placement, CharacterDefinition definition)>();

        foreach (var character in roster.Characters)
        {
            var assignment = schedule.GetAssignment(character.Id);
            var category = JobCategory.Rest;
            if (data.Jobs.TryGetValue(assignment, out var job))
            {
                category = job.Category;
            }

            var anchorId = RosterPlacementMath.AnchorForJob(category).AnchorId;
            var ordinal = seenPerAnchor.TryGetValue(anchorId, out var count) ? count : 0;
            seenPerAnchor[anchorId] = ordinal + 1;

            var placement = RosterPlacementMath.Place(character.Id, category, ordinal);
            desired[character.Id] = (placement, roster.DefinitionFor(character));
        }

        // Remove avatars no longer in the roster.
        foreach (var stale in _avatars.Keys.Where(id => !desired.ContainsKey(id)).ToList())
        {
            RemoveAvatar(stale);
        }

        // Add or reposition avatars for the current roster.
        foreach (var (id, (placement, definition)) in desired)
        {
            var avatar = _avatars.TryGetValue(id, out var existing)
                ? existing
                : CreateAvatar(id, definition);
            avatar.GlobalPosition = placement.Position;
            _avatars[id] = avatar;
        }

        return _avatars.Count;
    }

    private CharacterAvatar3D CreateAvatar(string characterId, CharacterDefinition definition)
    {
        var profile = CharacterAvatarFactory.CreateProfile(definition);
        var avatar = CharacterAvatarFactory.BuildAvatar(profile);
        avatar.Name = $"Avatar_{characterId}";
        AddChild(avatar);
        return avatar;
    }

    private void RemoveAvatar(string characterId)
    {
        if (_avatars.TryGetValue(characterId, out var avatar) && avatar is not null)
        {
            avatar.QueueFree();
        }
        _avatars.Remove(characterId);
    }
}
