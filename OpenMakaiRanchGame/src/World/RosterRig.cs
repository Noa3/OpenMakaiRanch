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
///
/// AI-001 navigation (opt-in): with <see cref="WalkSpeed"/> at 0 (default) a refresh snaps each
/// avatar straight to its logical anchor — the original, deterministic behavior the smoke suite
/// asserts. With <see cref="WalkSpeed"/> above 0, avatars instead *walk* toward their anchor each
/// frame (bounded by <see cref="ArrivalTolerance"/> and <see cref="MinProgress"/>, with bounded stuck
/// recovery via <see cref="StandInNavigationMath"/>). This is presentation only: it still reads the
/// shared assignment, computes no work, and never leaves the greybox bounds.
/// </summary>
public partial class RosterRig : Node3D
{
    private readonly Dictionary<string, CharacterAvatar3D> _avatars = new();
    private readonly Dictionary<string, Godot.Vector3> _targets = new();

    public int AvatarCount => _avatars.Count;

    /// <summary>The logical anchor a character's stand-in is steering to, or null if it has no target.</summary>
    public Godot.Vector3? GetTarget(string characterId)
    {
        return _targets.TryGetValue(characterId, out var target) ? target : null;
    }

    /// <summary>The stand-in avatar for a character, or null if not present.</summary>
    public CharacterAvatar3D? GetAvatar(string characterId)
    {
        if (_avatars.TryGetValue(characterId, out var avatar) && GodotObject.IsInstanceValid(avatar))
        {
            return avatar;
        }
        return null;
    }

    /// <summary>Stand-in walk speed (units/sec). 0 (default) = snap directly to the anchor (original behavior).</summary>
    [Export] public float WalkSpeed { get; set; } = 0f;

    /// <summary>Distance considered "arrived" at the anchor.</summary>
    [Export] public float ArrivalTolerance { get; set; } = 0.15f;

    /// <summary>Minimum progress a step must make or it is treated as stuck (and the avatar snaps to the anchor).</summary>
    [Export] public float MinProgress { get; set; } = 1e-3f;

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
            _targets[id] = placement.Position;
            if (WalkSpeed <= 0f)
            {
                // Original behavior: snap directly to the logical anchor (deterministic, what the
                // smoke suite asserts). No per-frame motion.
                avatar.GlobalPosition = placement.Position;
            }
            _avatars[id] = avatar;
        }

        return _avatars.Count;
    }

    /// <summary>
    /// AI-001: each frame, walk any avatar that still has to reach its anchor. No-op when
    /// <see cref="WalkSpeed"/> is 0 (snap mode) or every avatar is already arrived.
    /// </summary>
    public override void _Process(double delta)
    {
        if (WalkSpeed <= 0f)
        {
            return;
        }

        var maxStep = WalkSpeed * (float)delta;
        foreach (var (id, avatar) in _avatars)
        {
            if (GodotObject.IsInstanceValid(avatar) && _targets.TryGetValue(id, out var target))
            {
                var position = avatar.GlobalPosition;
                avatar.GlobalPosition = StandInNavigationMath.Tick(
                    position,
                    target,
                    maxStep,
                    ArrivalTolerance,
                    MinProgress);
            }
        }
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
        _targets.Remove(characterId);
    }
}
