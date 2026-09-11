using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace OpenMakaiRanch.Visuals;

public enum PresenceChannel { Blink, Smile, BrowRaise, BrowPinch, JawOpen }
public readonly record struct PresenceMorphSlot(MeshInstance3D Mesh, StringName Shape, PresenceChannel Channel);

/// <summary>
/// Explicit, opt-in presentation binding. Use dedicated unkeyed pivots, not a locomotion root or
/// animation-owned bone. Shape weights are instance-local. A competing writer revokes the binding.
/// Production AnimationTree/SkeletonModifier integration remains the rig author's responsibility.
/// </summary>
public partial class PresenceRig3D : Node
{
    private const string Lease = "_omr_presence_owner";
    private sealed class Slot
    {
        public required MeshInstance3D Target;
        public required Mesh Source;
        public required int Index;
        public required PresenceChannel Channel;
        public float Baseline, Last;
    }
    private readonly List<Slot> _slots = new();
    private readonly List<Node3D> _leased = new();
    private Node3D? _head, _body;
    private Transform3D _headBase, _bodyBase, _headLast, _bodyLast;
    public bool Bound => _head is not null;
    public string Diagnostic { get; private set; } = "not_bound";

    public bool Bind(Node3D headPivot, Node3D bodyPivot, params PresenceMorphSlot[] mappings)
    {
        ArgumentNullException.ThrowIfNull(mappings);
        Release();
        if (!GodotObject.IsInstanceValid(headPivot) || !GodotObject.IsInstanceValid(bodyPivot) || headPivot == bodyPivot)
            return Fail("invalid_pivots");
        var targets = new HashSet<Node3D> { headPivot, bodyPivot };
        var slots = new List<Slot>();
        var unique = new HashSet<(ulong, int)>();
        foreach (var mapping in mappings)
        {
            if (!GodotObject.IsInstanceValid(mapping.Mesh) || mapping.Mesh.Mesh is null || !Enum.IsDefined(mapping.Channel))
                return Fail("invalid_morph_target");
            var index = mapping.Mesh.FindBlendShapeByName(mapping.Shape);
            if (index < 0) return Fail("missing_morph:" + mapping.Shape);
            if (!unique.Add((mapping.Mesh.GetInstanceId(), index))) return Fail("duplicate_morph_mapping");
            var initial = mapping.Mesh.GetBlendShapeValue(index);
            if (!float.IsFinite(initial)) return Fail("invalid_initial_weight");
            slots.Add(new Slot { Target = mapping.Mesh, Source = mapping.Mesh.Mesh, Index = index,
                Channel = mapping.Channel, Baseline = initial, Last = initial });
            targets.Add(mapping.Mesh);
        }
        if (targets.Any(node => node.HasMeta(Lease))) return Fail("already_owned");
        _head = headPivot; _body = bodyPivot;
        _headBase = _headLast = headPivot.Transform; _bodyBase = _bodyLast = bodyPivot.Transform;
        _slots.AddRange(slots);
        foreach (var node in targets) { node.SetMeta(Lease, GetInstanceId()); _leased.Add(node); }
        Diagnostic = "bound";
        return true;
    }

    private bool Fail(string reason) { Diagnostic = reason; return false; }
    private bool Owns(Node3D node) => GodotObject.IsInstanceValid(node) && node.HasMeta(Lease)
        && unchecked((ulong)node.GetMeta(Lease).AsInt64()) == GetInstanceId();
    private bool Unchanged(Slot s) => GodotObject.IsInstanceValid(s.Target) && s.Target.Mesh == s.Source
        && Owns(s.Target) && Mathf.IsEqualApprox(s.Target.GetBlendShapeValue(s.Index), s.Last);

    public bool Apply(PresencePose pose)
    {
        if (!Bound) return false;
        if (!Owns(_head!) || !Owns(_body!) || _head!.Transform != _headLast || _body!.Transform != _bodyLast
            || _slots.Any(s => !Unchanged(s)))
        {
            Release(); return Fail("external_writer_or_reimport");
        }
        var angles = FiniteVector(pose.HeadDegrees, 8f) * (Mathf.Pi / 180f);
        var offset = FiniteVector(pose.BodyOffset, .015f);
        _headLast = _headBase * new Transform3D(Basis.FromEuler(angles), Vector3.Zero);
        _bodyLast = new Transform3D(_bodyBase.Basis, _bodyBase.Origin + offset);
        _head.Transform = _headLast; _body.Transform = _bodyLast;
        foreach (var slot in _slots)
        {
            var value = slot.Channel switch
            {
                PresenceChannel.Blink => pose.Blink, PresenceChannel.Smile => pose.Smile,
                PresenceChannel.BrowRaise => pose.BrowRaise, PresenceChannel.BrowPinch => pose.BrowPinch,
                PresenceChannel.JawOpen => pose.JawOpen, _ => 0f
            };
            slot.Last = Math.Clamp(slot.Baseline + PresenceTemperament.Unit(value), 0f, 1f);
            slot.Target.SetBlendShapeValue(slot.Index, slot.Last);
        }
        return true;
    }

    private static Vector3 FiniteVector(Vector3 v, float limit) => new(
        float.IsFinite(v.X) ? Math.Clamp(v.X, -limit, limit) : 0,
        float.IsFinite(v.Y) ? Math.Clamp(v.Y, -limit, limit) : 0,
        float.IsFinite(v.Z) ? Math.Clamp(v.Z, -limit, limit) : 0);

    public void Release()
    {
        if (_head is not null && Owns(_head) && _head.Transform == _headLast) _head.Transform = _headBase;
        if (_body is not null && Owns(_body) && _body.Transform == _bodyLast) _body.Transform = _bodyBase;
        foreach (var slot in _slots) if (Unchanged(slot)) slot.Target.SetBlendShapeValue(slot.Index, slot.Baseline);
        foreach (var node in _leased) if (Owns(node)) node.RemoveMeta(Lease);
        _slots.Clear(); _leased.Clear(); _head = null; _body = null;
        Diagnostic = "released";
    }
    public override void _ExitTree() => Release();
}
