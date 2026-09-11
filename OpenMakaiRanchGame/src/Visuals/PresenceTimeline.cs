using System;
using Godot;

namespace OpenMakaiRanch.Visuals;

/// <summary>Per-actor presentation lifetime, explicitly reset on session/identity replacement.</summary>
public sealed class PresenceTimeline
{
    public PresenceSnapshot Snapshot { get; private set; }
    public PresencePose Pose { get; private set; }
    public double Seconds { get; private set; }
    private double _reactionStarted = double.NegativeInfinity;
    public PresenceTimeline(PresenceSnapshot snapshot) { Snapshot = snapshot; Reset(snapshot); }
    public void Reset(PresenceSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Snapshot = snapshot; Seconds = 0; Pose = default; _reactionStarted = double.NegativeInfinity;
    }
    public bool TryUpdate(PresenceSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.ActorId != Snapshot.ActorId || snapshot.SessionId != Snapshot.SessionId || snapshot.Revision < Snapshot.Revision) return false;
        Snapshot = snapshot;
        return true;
    }
    public void Acknowledge()
    {
        if (!Snapshot.Paused && !Snapshot.CinematicOwnsPose) _reactionStarted = Seconds;
    }
    public PresencePose Advance(double delta, PresenceTemperament temperament, bool visible = true)
    {
        if (Snapshot.CinematicOwnsPose) return Pose = default;
        if (!visible || Snapshot.Paused || !double.IsFinite(delta) || delta <= 0) return Pose;
        var dt = Math.Min(delta, .1);
        Seconds += dt;
        var target = PresenceMotion.Sample(Seconds, Snapshot.ActorId, temperament, Snapshot, Seconds - _reactionStarted);
        var blend = (float)(1 - Math.Exp(-dt * 10));
        float Mix(float a, float b) => Mathf.Lerp(a, b, blend);
        Pose = new(target.Blink, Mix(Pose.Smile, target.Smile), Mix(Pose.BrowRaise, target.BrowRaise),
            Mix(Pose.BrowPinch, target.BrowPinch), Mix(Pose.JawOpen, target.JawOpen),
            Pose.HeadDegrees.Lerp(target.HeadDegrees, blend), Pose.BodyOffset.Lerp(target.BodyOffset, blend));
        return Pose;
    }
}
