using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Character;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.World;

/// <summary>
/// Places CHAR-001 stand-in avatars at the logical ranch anchors for the current roster.
///
/// This remains presentation-only: assignments are read from the shared ScheduleService and
/// movement only changes transient Node3D transforms. Walking/arrival never pays rewards, changes
/// jobs, advances time, or affects settlement.
///
/// New avatars spawn at their current logical anchor. When an existing character's assignment
/// changes, its target changes and the stand-in walks toward that target instead of teleporting.
/// The current greybox has open traversal space; obstacle-aware NavigationAgent3D remains a later
/// environment slice once authored navigation geometry exists.
/// </summary>
public partial class RosterRig : Node3D
{
    [Export] public bool AnimateTravel { get; set; } = true;
    [Export] public float TravelSpeed { get; set; } = 2.4f;
    [Export] public float ArrivalDistance { get; set; } = 0.08f;

    private readonly Dictionary<string, CharacterAvatar3D> _avatars = new();
    private readonly Dictionary<string, Vector3> _targets = new();

    public int AvatarCount => _avatars.Count;

    public int TravelingCount => _avatars.Count(pair =>
        _targets.TryGetValue(pair.Key, out var target)
        && pair.Value.GlobalPosition.DistanceTo(target) > ArrivalDistance);

    public bool TryGetTarget(string characterId, out Vector3 target)
    {
        return _targets.TryGetValue(characterId, out target);
    }

    public bool TryFindNearest(Vector3 worldPosition, float maxDistance, out string characterId, out CharacterAvatar3D? avatar, out float distance)
    {
        characterId = string.Empty;
        avatar = null;
        distance = float.PositiveInfinity;

        foreach (var (id, candidate) in _avatars)
        {
            if (!GodotObject.IsInstanceValid(candidate))
            {
                continue;
            }

            var candidateDistance = worldPosition.DistanceTo(candidate.GlobalPosition);
            if (candidateDistance > maxDistance || candidateDistance >= distance)
            {
                continue;
            }

            characterId = id;
            avatar = candidate;
            distance = candidateDistance;
        }

        return avatar is not null;
    }

    public override void _Process(double delta)
    {
        if (!AnimateTravel || _avatars.Count == 0)
        {
            return;
        }

        var step = Mathf.Max(0f, TravelSpeed) * (float)delta;
        foreach (var (id, avatar) in _avatars)
        {
            if (!_targets.TryGetValue(id, out var target) || !GodotObject.IsInstanceValid(avatar))
            {
                continue;
            }

            var current = avatar.GlobalPosition;
            var distance = current.DistanceTo(target);
            if (distance <= ArrivalDistance)
            {
                avatar.GlobalPosition = target;
                continue;
            }

            var next = current.MoveToward(target, step);
            var travel = target - current;
            travel.Y = 0f;
            if (travel.LengthSquared() > 0.0001f)
            {
                var yaw = Mathf.Atan2(-travel.X, -travel.Z);
                avatar.Rotation = new Vector3(avatar.Rotation.X, yaw, avatar.Rotation.Z);
            }

            avatar.GlobalPosition = next;
        }
    }

    /// <summary>
    /// Re-derive targets from the current roster/schedule. Stable per-character id and deterministic
    /// spread. Existing avatars keep their current transform and walk toward changed targets.
    /// </summary>
    public int Refresh(GameRoot game)
    {
        var roster = game.Roster;
        var schedule = game.Schedule;
        var data = game.Data;

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

        foreach (var stale in _avatars.Keys.Where(id => !desired.ContainsKey(id)).ToList())
        {
            RemoveAvatar(stale);
        }

        foreach (var (id, (placement, definition)) in desired)
        {
            if (_avatars.TryGetValue(id, out var existing))
            {
                _targets[id] = placement.Position;
                if (!AnimateTravel)
                {
                    existing.GlobalPosition = placement.Position;
                }
                continue;
            }

            var avatar = CreateAvatar(id, definition);
            avatar.GlobalPosition = placement.Position;
            _avatars[id] = avatar;
            _targets[id] = placement.Position;
        }

        return _avatars.Count;
    }

    private CharacterAvatar3D CreateAvatar(string characterId, CharacterDefinition definition)
    {
        var profile = CharacterAvatarFactory.CreateProfile(definition);
        var avatar = CharacterAvatarFactory.BuildAvatar(profile);
        avatar.Name = $"Avatar_{characterId}";

        var nameplate = new Label3D
        {
            Name = "Nameplate",
            Text = definition.DisplayName,
            Position = new Vector3(0f, 2.05f, 0f),
            FontSize = 28,
            OutlineSize = 6
        };
        avatar.AddChild(nameplate);

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
        _targets.Remove(characterId);
    }
}
