using System;
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
/// </summary>
public partial class RosterRig : Node3D
{
    [Export] public bool AnimateTravel { get; set; } = true;
    [Export] public float TravelSpeed { get; set; } = 2.4f;
    [Export] public float ArrivalDistance { get; set; } = 0.08f;
    [Export] public bool CompanionOnly { get; set; }
    [Export] public float CompanionSideOffset { get; set; } = 1.15f;
    [Export] public float CompanionBackOffset { get; set; } = 1.35f;

    private readonly Dictionary<string, CharacterAvatar3D> _avatars = new();
    private readonly Dictionary<string, Label3D> _thoughtBubbles = new();
    private readonly Dictionary<string, NavigationAgent3D> _agents = new();
    private readonly Dictionary<string, Vector3> _targets = new();
    private CoastalRegionController? _coastalRegion;
    private string _coastalAreaId = "";
    public void BindCoastalSurface(CoastalRegionController region, string areaId)
    {
        _coastalRegion = region;
        _coastalAreaId = areaId;
    }
    private Vector3 CoastalGround(Vector3 position)
        {
            if (_coastalRegion is null) return position;
            var root = _coastalRegion.GetAreaRoot(_coastalAreaId);
            var area = _coastalRegion.Terrain!.Areas[_coastalAreaId];
            var local = root.ToLocal(position);
            // A side-offset follow target can extend beyond the finite raster while its player stays inside.
            // Clamp the requested target, never the actor. The navigation path still decides reachability.
            local.X = Mathf.Clamp(local.X, area.Bounds.Position.X + 0.5f, area.Bounds.End.X - 0.5f);
            local.Z = Mathf.Clamp(local.Z, area.Bounds.Position.Y + 0.5f, area.Bounds.End.Y - 0.5f);
            return _coastalRegion.GroundWorldPosition(_coastalAreaId, root.ToGlobal(local));
        }

    private Node3D? _followTarget;
    private CollisionShape3D? _followBodyShape;
    private GameRoot? _game;
    private string _activeCompanionId = string.Empty;

    // Presentation-only: talking pauses this avatar, never its scheduled production.
    public string ConversationFocusId { get; set; } = string.Empty;
    public int AvatarCount => _avatars.Count;
    public string ActiveCompanionId => _activeCompanionId;

    internal static float AlignReachedWaypointHeight(Vector3 current, Vector3 waypoint, float previousOffset, float planarTolerance)
    {
        // On banks, the nearest polygon can differ in height from the active waypoint.
        // A physically grounded avatar must not wait forever directly beneath that point.
        // Do not skip horizontal corners or compensate for a different floor/storey.
        if (!current.IsFinite() || !waypoint.IsFinite() || !float.IsFinite(previousOffset)) return previousOffset;
        var horizontal = new Vector2(waypoint.X - current.X, waypoint.Z - current.Z);
        var vertical = waypoint.Y - current.Y;
        return horizontal.Length() <= planarTolerance && Mathf.Abs(vertical) <= SimpleNavigationRegionBuilder.MaximumAgentClimb
            ? previousOffset + vertical : previousOffset;
    }

    public void BindFollowTarget(Node3D? target)
    {
        _followTarget = target;
        _followBodyShape = target is CharacterBody3D
            ? target.GetChildren().OfType<CollisionShape3D>().FirstOrDefault(c => c.Shape is CapsuleShape3D)
            : null;
    }

    public bool TryGetAvatar(string characterId, out CharacterAvatar3D? avatar)
    {
        if (_avatars.TryGetValue(characterId, out var found) && GodotObject.IsInstanceValid(found))
        {
            avatar = found;
            return true;
        }

        avatar = null;
        return false;
    }

    public int TravelingCount => _avatars.Count(pair =>
        _targets.TryGetValue(pair.Key, out var target)
        && GodotObject.IsInstanceValid(pair.Value)
        && PositionOf(pair.Value).DistanceTo(target) > ArrivalDistance);

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

            var candidateDistance = worldPosition.DistanceTo(PositionOf(candidate));
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

    public override void _PhysicsProcess(double delta)
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

            if (id == ConversationFocusId) { avatar.PlayLocomotion(0f, false); continue; }

            if (string.Equals(id, _activeCompanionId, StringComparison.Ordinal)
                && _followTarget is not null
                && GodotObject.IsInstanceValid(_followTarget))
            {
                target = CompanionFollowTarget(_followTarget);
                _targets[id] = target;
                RefreshThoughtBubble(id);
            }
            else if (_thoughtBubbles.TryGetValue(id, out var idleBubble) && GodotObject.IsInstanceValid(idleBubble))
            {
                idleBubble.Visible = false;
            }

            var current = PositionOf(avatar);
            var distance = current.DistanceTo(target);
            if (distance <= ArrivalDistance)
            {
                SetPosition(avatar, target);
                avatar.PlayLocomotion(0f, false);
                continue;
            }

            var travelTarget = target;
            // Standalone visual previews can move directly. Actual areas must obey their baked
            // collision map; missing/unreachable paths never fall back to walking through walls.
            if (GetParent() is RanchGreyboxController or TownWorldController)
            {
                if (!_agents.TryGetValue(id, out var agent) || !GodotObject.IsInstanceValid(agent) || !agent.IsInsideTree()
                    || NavigationServer3D.MapGetIterationId(agent.GetNavigationMap()) == 0)
                { avatar.PlayLocomotion(0, false); continue; }
                if (agent.TargetPosition.DistanceSquaredTo(target) > 0.0025f) agent.TargetPosition = target;
                // Preserve the active path's correction across physics frames. Replacing it
                // with the nearest polygon's height can select the bank below a bridge and
                // also undo the correction before NavigationAgent consumes the waypoint.
                var nextPath = agent.GetNextPathPosition();
                var alignedOffset = AlignReachedWaypointHeight(current, nextPath, agent.PathHeightOffset, agent.PathDesiredDistance);
                if (!Mathf.IsEqualApprox(alignedOffset, agent.PathHeightOffset))
                {
                    agent.PathHeightOffset = alignedOffset;
                    nextPath = agent.GetNextPathPosition();
                }
                if (agent.IsNavigationFinished())
                                {
                                    // Only the final, sub-step arrival may finish directly; never bypass an unreachable path.
                                    var planarRemaining = new Vector2(target.X - current.X, target.Z - current.Z).Length();
                                    if (_coastalRegion is null || !agent.IsTargetReachable() || planarRemaining > 0.4f)
                                    { avatar.PlayLocomotion(0, false); continue; }
                                    travelTarget = target;
                                }
                                else
                                {
                                    // A valid path may cross a large plaza in one long segment.
                                    // MoveToward still limits displacement to the unchanged per-frame step.
                                    travelTarget = _coastalRegion is null ? new Vector3(nextPath.X, current.Y, nextPath.Z) : CoastalGround(nextPath);
                                }
            }

            var next = current.MoveToward(travelTarget, step);
            if (_coastalRegion is not null) next = CoastalGround(next);
            var travel = travelTarget - current;
            travel.Y = 0f;
            if (travel.LengthSquared() > 0.0001f)
            {
                var yaw = Mathf.Atan2(-travel.X, -travel.Z);
                if (_coastalRegion is not null && avatar.IsInsideTree())
                    avatar.GlobalRotation = new Vector3(avatar.GlobalRotation.X, yaw, avatar.GlobalRotation.Z);
                else avatar.Rotation = new Vector3(avatar.Rotation.X, yaw, avatar.Rotation.Z);
            }

            SetPosition(avatar, next);
            avatar.PlayLocomotion(TravelSpeed, false);
        }
    }

    /// <summary>
    /// Re-derive targets from the current roster/schedule. Stable per-character id and deterministic
    /// spread. Existing avatars keep their current transform and walk toward changed targets.
    /// </summary>
    public int Refresh(GameRoot game)
    {
        _game = game;
        _activeCompanionId = game.Dating.ActivePartnerId;

        var roster = game.Roster;
        var schedule = game.Schedule;
        var data = game.Data;

        var seenPerAnchor = new Dictionary<string, int>();
        var desired = new Dictionary<string, (RosterPlacement placement, CharacterDefinition definition, float height)>();

        foreach (var character in roster.Characters)
        {
            if (CompanionOnly && !string.Equals(character.Id, _activeCompanionId, StringComparison.Ordinal))
            {
                continue;
            }

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
            if (_coastalRegion is not null)
                placement = placement with { Position = CoastalGround(ToGlobal(placement.Position)) };
            if (GetParent() is RanchGreyboxController ranch && assignment != "rest"
                && ranch.Stations.FirstOrDefault(station => station.CommandTargetId == assignment && station.RequiresWorker) is { } station)
            {
                var target = station.GlobalPosition + new Vector3(Mathf.Clamp((ordinal % 3 - 1) * 0.75f, -0.75f, 0.75f), 0, -(ordinal / 3) * 0.55f);
                if (_coastalRegion is null) target.Y = 0;
                else target = CoastalGround(target);
                placement = placement with { Position = target };
            }
            desired[character.Id] = (placement, roster.DefinitionFor(character), character.Height / 1000f);
        }

        foreach (var stale in _avatars.Keys.Where(id => !desired.ContainsKey(id)).ToList())
        {
            RemoveAvatar(stale);
        }

        foreach (var (id, (placement, definition, height)) in desired)
        {
            var targetPosition = string.Equals(id, _activeCompanionId, StringComparison.Ordinal)
                && _followTarget is not null
                && GodotObject.IsInstanceValid(_followTarget)
                    ? CompanionFollowTarget(_followTarget)
                    : placement.Position;

            if (_avatars.TryGetValue(id, out var existing))
            {
                if (existing.Profile is { } existingProfile && height > 0
                    && !Mathf.IsEqualApprox(existingProfile.Height, height))
                {
                    existingProfile.Height = height;
                    existing.Rebuild();
                }
                _targets[id] = targetPosition;
                if (!AnimateTravel && id != ConversationFocusId)
                {
                    SetPosition(existing, targetPosition);
                }
                RefreshThoughtBubble(id);
                continue;
            }

            var avatar = CreateAvatar(id, definition, height);
            SetPosition(avatar, targetPosition);
            _avatars[id] = avatar;
            _targets[id] = targetPosition;
            RefreshThoughtBubble(id);
        }

        return _avatars.Count;
    }

    private CharacterAvatar3D CreateAvatar(string characterId, CharacterDefinition definition, float height)
    {
        var profile = CharacterAvatarFactory.CreateProfile(definition);
        // DefinitionFor omits presentation metadata. The persisted instance's millimetres
        // remain authoritative, including generated heights which are not whole centimetres.
        if (height > 0) profile.Height = height;
        var avatar = CharacterAvatarFactory.BuildAvatar(profile);
        avatar.Name = $"Avatar_{characterId}";

        var agent = new NavigationAgent3D
        {
            Name = "NavigationAgent",
            // Do not skip tight doorway corners before the avatar reaches their safe centreline.
            PathDesiredDistance = 0.05f,
            TargetDesiredDistance = ArrivalDistance,
            Radius = 0.35f,
            Height = 1.7f,
            AvoidanceEnabled = false
        };
        avatar.AddChild(agent);
        _agents[characterId] = agent;

        var character = _game?.Roster.Find(characterId);
        var displayName = character is not null && !string.IsNullOrWhiteSpace(character.DisplayNameOverride)
            ? character.DisplayNameOverride
            : definition.DisplayName;

        var nameplate = new Label3D
        {
            Name = "Nameplate",
            Text = displayName,
            Position = new Vector3(0f, 2.05f, 0f),
            FontSize = 28,
            OutlineSize = 6
        };
        avatar.AddChild(nameplate);

        var thoughtBubble = new Label3D
        {
            Name = "ThoughtBubble",
            Text = string.Empty,
            Position = new Vector3(0f, 2.55f, 0f),
            FontSize = 20,
            OutlineSize = 5,
            Visible = false
        };
        avatar.AddChild(thoughtBubble);
        _thoughtBubbles[characterId] = thoughtBubble;

        AddChild(avatar);
        return avatar;
    }

    private Vector3 CompanionFollowTarget(Node3D target)
    {
        if (!target.IsInsideTree())
        {
            var basis = target.Transform.Basis;
            return target.Position - FlatForward(basis) * CompanionBackOffset + FlatRight(basis) * CompanionSideOffset;
        }

        var globalBasis = target.GlobalTransform.Basis;
        var follow = target.GlobalPosition - FlatForward(globalBasis) * CompanionBackOffset + FlatRight(globalBasis) * CompanionSideOffset;
        if (_coastalRegion is null && GodotObject.IsInstanceValid(_followBodyShape)
            && _followBodyShape?.Shape is CapsuleShape3D capsule)
        {
            // A player's origin is its capsule centre, not the companion's foot anchor.
            follow.Y = (_followBodyShape.GlobalTransform * new Vector3(0, -capsule.Height * 0.5f, 0)).Y;
        }
        return CoastalGround(follow);
    }

    private static Vector3 FlatRight(Basis basis)
    {
        var right = basis.X;
        right.Y = 0f;
        return right.LengthSquared() < 0.001f ? Vector3.Right : right.Normalized();
    }

    private static Vector3 FlatForward(Basis basis)
    {
        var forward = -basis.Z;
        forward.Y = 0f;
        return forward.LengthSquared() < 0.001f ? Vector3.Forward : forward.Normalized();
    }

    private static Vector3 PositionOf(Node3D node) => node.IsInsideTree() ? node.GlobalPosition : node.Position;

    private static void SetPosition(Node3D node, Vector3 position)
    {
        if (node.IsInsideTree())
            node.GlobalPosition = position;
        else
            node.Position = position;
    }

    private void RefreshThoughtBubble(string characterId)
    {
        if (!_thoughtBubbles.TryGetValue(characterId, out var bubble) || !GodotObject.IsInstanceValid(bubble))
            return;

        if (_game is null
            || !string.Equals(characterId, _activeCompanionId, StringComparison.Ordinal))
        {
            bubble.Visible = false;
            return;
        }

        var thoughts = _game.Dating.ThoughtsFor(characterId);
        if (thoughts.Count == 0)
        {
            bubble.Visible = false;
            return;
        }

        var stableOffset = 0;
        foreach (var ch in characterId)
            stableOffset += ch;

        var interval = (long)(Time.GetTicksMsec() / 7000UL);
        var index = (int)((interval + stableOffset) % thoughts.Count);
        bubble.Text = thoughts[index];
        bubble.Visible = true;
    }

    private void RemoveAvatar(string characterId)
    {
        if (_avatars.TryGetValue(characterId, out var avatar) && avatar is not null)
        {
            avatar.QueueFree();
        }

        _avatars.Remove(characterId);
        _agents.Remove(characterId);
        _targets.Remove(characterId);
        _thoughtBubbles.Remove(characterId);
    }
}