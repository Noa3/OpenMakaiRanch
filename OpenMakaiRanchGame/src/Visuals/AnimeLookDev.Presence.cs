using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace OpenMakaiRanch.Visuals;

public partial class AnimeLookDev
{
    public PresenceRig3D PresenceRig { get; private set; } = null!;
    public Node3D PresenceHead { get; private set; } = null!;
    public Node3D PresenceBody { get; private set; } = null!;
    public MeshInstance3D PresenceEye { get; private set; } = null!;
    public MakaiPhenomenon3D Phenomenon { get; private set; } = null!;
    public PresenceTimeline PresenceClock { get; private set; } = null!;
    private readonly List<PresenceMorphSlot> _presenceSlots = new();
    private PresenceTemperament _temperament = PresenceArchetypes.Resolve("warm_caretaker");
    private bool _presencePlaying, _showMakai;
    private (int Light, string Quality, bool Reduced)? _phenomenonSettings;
    private Label _intentStatus = null!;

    private void BuildPresenceControls(VBoxContainer column)
    {
        PresenceBody = new Node3D { Name = "PresenceBodyPivot" };
        var children = SpecimenRoot.GetChildren().OfType<Node3D>().ToArray();
        SpecimenRoot.AddChild(PresenceBody);
        foreach (var node in children) node.Reparent(PresenceBody);
        PresenceHead = new Node3D { Name = "PresenceHeadPivot", Position = AnimeCalibrationGeometry.HeadOrigin };
        PresenceBody.AddChild(PresenceHead);
        foreach (var (mesh, _) in _samples)
        {
            var name = mesh.Name.ToString();
            if (!name.StartsWith("Calibration", StringComparison.Ordinal)) continue;
            mesh.Reparent(PresenceHead);
            var family = name.Contains("Eye", StringComparison.Ordinal) ? "eye" : name == "CalibrationLines" ? "lines"
                : name == "CalibrationMouth" ? "mouth" : "none";
            mesh.Mesh = PresenceCalibrationShapes.Add((ArrayMesh)mesh.Mesh, family);
            void Slot(string shape, PresenceChannel channel) => _presenceSlots.Add(new(mesh, shape, channel));
            if (family is "eye" or "lines") Slot("blink", PresenceChannel.Blink);
            if (family == "eye") PresenceEye = mesh;
            if (family == "lines") { Slot("brow_raise", PresenceChannel.BrowRaise); Slot("brow_pinch", PresenceChannel.BrowPinch); }
            if (family == "mouth") { Slot("smile", PresenceChannel.Smile); Slot("jaw_open", PresenceChannel.JawOpen); }
        }
        PresenceRig = new PresenceRig3D(); AddChild(PresenceRig);
        PresenceClock = new PresenceTimeline(new PresenceSnapshot { ActorId = "lookdev-resident", SessionId = "isolated-lab", Revision = 1,
            Emotion = PresenceEmotion.Warm, InConversation = true, Trust = .6f, Available = PresenceOpportunities.Listen });
        Phenomenon = new MakaiPhenomenon3D { Name = "OriginalMakaiVeil" }; _world.AddChild(Phenomenon);
        var row = new HFlowContainer(); column.AddChild(row); column.MoveChild(row, 2);
        var play = new CheckButton { Text = "Microgestures" }; row.AddChild(play);
        play.Toggled += value => { _presencePlaying = value; if (value) BindPresence(); else ResetPresence(); };
        var archetype = new OptionButton(); row.AddChild(archetype);
        foreach (var id in PresenceArchetypes.All.Keys) archetype.AddItem(id);
        archetype.Select(5);
        archetype.ItemSelected += index => _temperament = PresenceArchetypes.Resolve(archetype.GetItemText((int)index));
        var acknowledge = new Button { Text = "Acknowledge" }; row.AddChild(acknowledge);
        acknowledge.Pressed += () => PresenceClock.Acknowledge();
        var context = new OptionButton { TooltipText = "Synthetic needs, never actual resident state." };
        foreach (var text in new[] { "Conversation", "Tired", "Unsafe weather", "Free time" }) context.AddItem(text);
        row.AddChild(context);
        context.ItemSelected += index => SetPresenceContext((int)index);
        var reduce = new CheckButton { Text = "Reduced motion" }; row.AddChild(reduce);
        reduce.Toggled += value => PresenceClock.TryUpdate(PresenceClock.Snapshot with
            { Revision = PresenceClock.Snapshot.Revision + 1, ReducedMotion = value });
        var veil = new CheckButton { Text = "Makai veil (evening/night)" }; row.AddChild(veil);
        veil.Toggled += value => { _showMakai = value; RefreshPhenomenon(); };
        _intentStatus = new Label { Text = "Presentation prototype / no needs, jobs or relationship values changed", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        column.AddChild(_intentStatus); column.MoveChild(_intentStatus, 3);
    }

    public bool BindPresence() => PresenceRig.Bind(PresenceHead, PresenceBody, _presenceSlots.ToArray());
    public void ResetPresence() { _presencePlaying = false; PresenceRig.Release(); }
    public void ApplyPresencePose(PresencePose pose)
    {
        if (!PresenceRig.Bound && !BindPresence()) throw new InvalidOperationException(PresenceRig.Diagnostic);
        if (!PresenceRig.Apply(pose)) throw new InvalidOperationException(PresenceRig.Diagnostic);
    }
    public void SetMakaiPreview(bool enabled) { _showMakai = enabled; RefreshPhenomenon(); }
    public void SetPresenceContext(int index)
    {
        var old = PresenceClock.Snapshot;
        var state = new PresenceSnapshot { ActorId = old.ActorId, SessionId = old.SessionId, Revision = old.Revision + 1,
            ReducedMotion = old.ReducedMotion, Trust = .6f };
        state = index switch
        {
            1 => state with { Fatigue = .95f, Emotion = PresenceEmotion.Tired, Available = PresenceOpportunities.Rest },
            2 => state with { UnsafeWeather = true, Emotion = PresenceEmotion.Concerned, Available = PresenceOpportunities.Shelter },
            3 => state with { SocialNeed = .6f, QuietNeed = .6f, PhenomenonVisible = true, Emotion = PresenceEmotion.Curious,
                Available = PresenceOpportunities.Company | PresenceOpportunities.Quiet | PresenceOpportunities.Observe },
            _ => state with { InConversation = true, Emotion = PresenceEmotion.Warm, Available = PresenceOpportunities.Listen }
        };
        PresenceClock.TryUpdate(state);
    }
    private void RefreshPhenomenon()
    {
        _phenomenonSettings = (LightPreset, QualityName, PresenceClock.Snapshot.ReducedMotion);
        Phenomenon.Configure(_showMakai,
            LightPreset == 4 ? PresencePhase.Night : LightPreset == 3 ? PresencePhase.Evening : PresencePhase.Afternoon,
            QualityName, false, PresenceClock.Snapshot.ReducedMotion);
    }

    public override void _Process(double delta)
    {
        if (PresenceClock is null) return;
        if (_presencePlaying)
        {
            if (PresenceClock.Snapshot.CinematicOwnsPose) PresenceRig.Release();
            else if (PresenceRig.Bound) PresenceRig.Apply(PresenceClock.Advance(delta, _temperament, IsVisibleInTree()));
            var suggestion = PresenceIntentAdvisor.Recommend(PresenceClock.Snapshot, _temperament);
            var text = $"Read-only intent: {suggestion.Intent} / {suggestion.Reason}. No simulation changes.";
            if (_intentStatus.Text != text) _intentStatus.Text = text;
        }
        if (_showMakai)
        {
            if (_phenomenonSettings != (LightPreset, QualityName, PresenceClock.Snapshot.ReducedMotion)) RefreshPhenomenon();
            Phenomenon.Advance(delta, PresenceClock.Snapshot.Paused || !IsVisibleInTree());
        }
    }
}
