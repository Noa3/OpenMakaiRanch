using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace OpenMakaiRanch.Visuals;

public partial class AnimeLookDevChecks
{
    private async Task CheckPresenceAndMakai()
    {
        var neutral = new PresenceTemperament();
        Check("presence parameterless preferences are neutral", neutral.Sociability == .5f && neutral.Warmth == .5f);
        Check("presence catalogue has twelve editable starting points", PresenceArchetypes.All.Count == 12);
        Check("presence unknown archetype has neutral fallback", PresenceArchetypes.Resolve("unknown") == neutral);
        var social = PresenceArchetypes.Resolve("lively_social");
        var quiet = PresenceArchetypes.Resolve("quiet_reserved");
        Check("presence blend endpoints preserve authored profiles", PresenceTemperament.Blend(social, quiet, 0) == social
            && PresenceTemperament.Blend(social, quiet, 1) == quiet);
        Check("presence mixtures are not locked to an archetype", PresenceTemperament.Blend(social, quiet, .5f) != social
            && PresenceTemperament.Blend(social, quiet, .5f) != quiet);
        var invalid = neutral with { Sociability = float.NaN, Warmth = 40, Curiosity = -1 };
        Check("presence input validation is finite and nonmutating", invalid.Validated().Sociability == .5f
            && invalid.Validated().Warmth == 1 && invalid.Validated().Curiosity == 0 && float.IsNaN(invalid.Sociability));
        var state = new PresenceSnapshot { ActorId = "resident-a", SessionId = "session-a", Revision = 7 };
        PresenceSuggestion Decide(PresenceSnapshot s) => PresenceIntentAdvisor.Recommend(s, neutral);
        Check("presence unknown availability defaults closed", Decide(state with { Hunger = .8f }).Intent == PresenceIntent.Idle);
        Check("presence missing identity refuses advice", Decide(state with { SessionId = "" }).Reason == "missing_identity");
        Check("presence urgent fatigue requests available rest", Decide(state with { Fatigue = .95f,
            Available = PresenceOpportunities.Rest }).Intent == PresenceIntent.Rest);
        Check("presence urgent unavailable rest does not invent a facility", Decide(state with { Fatigue = .95f }).Reason == "rest_unavailable");
        Check("presence hunger only suggests a known meal opportunity", Decide(state with { Hunger = 1,
            Available = PresenceOpportunities.Meal }).Intent == PresenceIntent.Meal);
        Check("presence bad weather outranks duty and companions", Decide(state with { UnsafeWeather = true, DutyActive = true,
            CompanionAccepted = true, Available = PresenceOpportunities.Shelter | PresenceOpportunities.Duty | PresenceOpportunities.Follow }).Intent == PresenceIntent.Shelter);
        Check("presence unavailable shelter is reported rather than teleported", Decide(state with { UnsafeWeather = true }).Reason == "shelter_unavailable");
        Check("presence active conversation owns ordinary attention", Decide(state with { InConversation = true, DutyActive = true,
            Available = PresenceOpportunities.Listen | PresenceOpportunities.Duty }).Intent == PresenceIntent.Listen);
        Check("presence accepted outing can recommend following", Decide(state with { CompanionAccepted = true,
            Available = PresenceOpportunities.Follow }).Intent == PresenceIntent.Follow);
        Check("presence trust alone never authorizes following", Decide(state with { Trust = 1,
            Available = PresenceOpportunities.Follow }).Intent == PresenceIntent.Idle);
        Check("presence scheduled duty is preserved", Decide(state with { DutyActive = true, SocialNeed = 1,
            Available = PresenceOpportunities.Duty | PresenceOpportunities.Company }).Intent == PresenceIntent.Duty);
        Check("presence night influences available rest without advancing a clock", Decide(state with { Phase = PresencePhase.Night,
            Available = PresenceOpportunities.Rest }).Intent == PresenceIntent.Rest && state.Phase == PresencePhase.Morning);
        var leisure = state with { SocialNeed = .6f, QuietNeed = .6f,
            Available = PresenceOpportunities.Company | PresenceOpportunities.Quiet };
        Check("presence social and reserved styles choose differently in free time",
            PresenceIntentAdvisor.Recommend(leisure, social).Intent == PresenceIntent.Company
            && PresenceIntentAdvisor.Recommend(leisure, quiet).Intent == PresenceIntent.Quiet);
        Check("presence curiosity requires a visible accessible phenomenon", Decide(state with { PhenomenonVisible = true,
            Available = PresenceOpportunities.Observe }).Intent == PresenceIntent.Observe
            && Decide(state with { Available = PresenceOpportunities.Observe }).Intent == PresenceIntent.Idle);
        Check("presence retention never resurrects unavailable targets", PresenceIntentAdvisor.Recommend(state, neutral,
            PresenceIntent.Company, 1).Intent == PresenceIntent.Idle);
        var retained = state with { Hunger = .5f, SocialNeed = .7f,
            Available = PresenceOpportunities.Meal | PresenceOpportunities.Company };
        Check("presence hysteresis is bounded in duration", PresenceIntentAdvisor.Recommend(retained, neutral,
            PresenceIntent.Meal, 1).Score > PresenceIntentAdvisor.Recommend(retained, neutral, PresenceIntent.Meal, 10).Score);
        var proposal = Decide(leisure);
        Check("presence proposals carry session actor and revision", proposal.Matches(leisure)
            && !proposal.Matches(leisure with { Revision = 8 }) && !proposal.Matches(leisure with { SessionId = "replacement" })
            && !proposal.Matches(leisure with { ActorId = "other" }));
        Check("presence pause refuses new intents", Decide(leisure with { Paused = true }).Reason == "presentation_locked");
        Check("presence nonfinite unknown needs stay harmless", Decide(state with { Hunger = float.NaN, Fatigue = float.NaN }).Intent == PresenceIntent.Idle);

        var warm = state with { Emotion = PresenceEmotion.Warm, Trust = .8f, InConversation = true, SpeechEnvelope = .7f };
        var pose = PresenceMotion.Sample(2.3, state.ActorId, social, warm);
        Check("presence motion is reproducible without global random", pose == PresenceMotion.Sample(2.3, state.ActorId, social, warm));
        Check("presence stable identifiers desynchronize residents", PresenceMotion.StableSeed("resident-a") != PresenceMotion.StableSeed("resident-b")
            && pose != PresenceMotion.Sample(2.3, "resident-b", social, warm));
        Check("presence reduced motion removes head and body drift", PresenceMotion.Sample(2.3, state.ActorId, social,
            warm with { ReducedMotion = true }).HeadDegrees == Vector3.Zero
            && PresenceMotion.Sample(2.3, state.ActorId, social, warm with { ReducedMotion = true }).BodyOffset == Vector3.Zero);
        Check("presence expressive channels remain bounded", pose.Smile > 0 && pose.Smile <= 1 && pose.JawOpen > 0
            && pose.JawOpen <= 1 && pose.HeadDegrees.Length() < 8 && pose.BodyOffset.Length() < .015f);
        var peak = 0f;
        for (var i = 0; i < 2800; i++) peak = Math.Max(peak, PresenceMotion.Sample(i * .005, state.ActorId, quiet, state).Blink);
        Check("presence timeline actually contains a closing blink", peak > .95f);
        Check("presence acknowledgement changes head response", PresenceMotion.Sample(2.3, state.ActorId, social, warm, .2).HeadDegrees != pose.HeadDegrees);
        var clock = new PresenceTimeline(warm);
        clock.Advance(.05, social); var frozen = clock.Pose; var time = clock.Seconds;
        clock.TryUpdate(warm with { Paused = true });
        Check("presence pause freezes timeline and pose", clock.Advance(1, social) == frozen && clock.Seconds == time);
        clock.TryUpdate(warm);
        Check("presence hidden actors do not advance cosmetic time", clock.Advance(1, social, false) == frozen && clock.Seconds == time);
        Check("presence invalid delta does not advance time", clock.Advance(double.NaN, social) == frozen && clock.Seconds == time);
        clock.Advance(30, social);
        Check("presence resume catchup is bounded", clock.Seconds - time <= .100001);
        Check("presence stale updates cannot replace a session", !clock.TryUpdate(warm with { Revision = 6 })
            && !clock.TryUpdate(warm with { SessionId = "other" }) && !clock.TryUpdate(warm with { ActorId = "other" }));
        clock.TryUpdate(warm with { CinematicOwnsPose = true });
        Check("presence cinematic ownership suppresses pose", clock.Advance(.1, social) == default);
        clock.Reset(warm with { SessionId = "new-session" });
        Check("presence explicit reset clears timing and pose", clock.Seconds == 0 && clock.Pose == default && clock.Snapshot.SessionId == "new-session");

        var study = _study!;
        study.ResetPresence(); study.SetPortrait(true); study.SetLighting(0); study.SetDepthOfField(false);
        var headBase = study.PresenceHead.Transform; var bodyBase = study.PresenceBody.Transform;
        var sourceMesh = study.PresenceEye.Mesh;
        var blinkIndex = study.PresenceEye.FindBlendShapeByName("blink");
        Check("presence calibration has actual named shape targets", blinkIndex >= 0);
        var mouth = study.PresenceHead.GetChildren().OfType<MeshInstance3D>().Single(m => m.Name == "CalibrationMouth");
        var mouthMesh = (ArrayMesh)mouth.Mesh;
        var original = mouthMesh.SurfaceGetArrays(0);
        var vertices = original[(int)Mesh.ArrayType.Vertex].AsVector3Array();
        var indices = original[(int)Mesh.ArrayType.Index].AsInt32Array();
        var orientationOk = true;
        foreach (var target in mouthMesh.SurfaceGetBlendShapeArrays(0))
        {
            var changed = target[(int)Mesh.ArrayType.Vertex].AsVector3Array();
            for (var i = 0; i < indices.Length; i += 3)
            {
                var a = indices[i]; var b = indices[i + 1]; var c = indices[i + 2];
                var before = (vertices[b] - vertices[a]).Cross(vertices[c] - vertices[a]);
                var after = (changed[b] - changed[a]).Cross(changed[c] - changed[a]);
                if (before.LengthSquared() > 1e-18f && before.Dot(after) <= 0) orientationOk = false;
            }
        }
        Check("presence mouth targets never invert front-face winding", orientationOk);
        Check("presence missing shapes fail without taking ownership", !study.PresenceRig.Bind(study.PresenceHead, study.PresenceBody,
            new PresenceMorphSlot(study.PresenceEye, "not-authored", PresenceChannel.Blink)) && !study.PresenceRig.Bound);
        Check("presence invalid mapping preserves local transforms", study.PresenceHead.Transform == headBase && study.PresenceBody.Transform == bodyBase);
        Check("presence explicit binding succeeds", study.BindPresence());
        var rival = new PresenceRig3D(); AddChild(rival);
        Check("presence competing bind is refused", !rival.Bind(study.PresenceHead, study.PresenceBody,
            new PresenceMorphSlot(study.PresenceEye, "blink", PresenceChannel.Blink)));
        study.ApplyPresencePose(default);
        var open = await Capture("presence-neutral");
        study.ApplyPresencePose(new PresencePose(1, 0, 0, 0, 0, Vector3.Zero, Vector3.Zero));
        var blinkImage = await Capture("presence-blink");
        Check("presence blink changes actual rendered pixels", Difference(open, blinkImage) > .00001);
        var creasePoint = AnimeCalibrationGeometry.HeadOrigin + new Vector3(.078f, .037f,
            AnimeCalibrationGeometry.Front(.078f, .037f) + .006f);
        var lidPoint = AnimeCalibrationGeometry.HeadOrigin + new Vector3(.078f, .046f,
            AnimeCalibrationGeometry.Front(.078f, .046f));
        var creaseColor = Sample(blinkImage, study.StudyCamera.UnprojectPosition(creasePoint));
        var lidColor = Sample(blinkImage, study.StudyCamera.UnprojectPosition(lidPoint));
        Check("presence closed lids retain a visible dark crease", (lidColor.R + lidColor.G + lidColor.B)
            - (creaseColor.R + creaseColor.G + creaseColor.B) > .05f);
        Check("presence blink writes instance weight not shared source", Mathf.IsEqualApprox(study.PresenceEye.GetBlendShapeValue(blinkIndex), 1)
            && study.PresenceEye.Mesh == sourceMesh);
        study.ApplyPresencePose(new PresencePose(0, .7f, .5f, 0, .4f, new Vector3(2, 4, 1), new Vector3(.002f, .002f, 0)));
        var expressive = await Capture("presence-warm");
        Check("presence expression and posture visibly differ", Difference(open, expressive) > .0001);
        var mouthPoint = new Vector3(0, -.0926f, AnimeCalibrationGeometry.Front(0, -.0926f) + .002f);
        var cheekPoint = new Vector3(.05f, -.0926f, AnimeCalibrationGeometry.Front(.05f, -.0926f) + .002f);
        var mouthColor = Sample(expressive, study.StudyCamera.UnprojectPosition(mouth.GlobalTransform * mouthPoint));
        var cheekColor = Sample(expressive, study.StudyCamera.UnprojectPosition(mouth.GlobalTransform * cheekPoint));
        Check("presence open mouth remains visible on the face", (cheekColor.R + cheekColor.G + cheekColor.B)
            - (mouthColor.R + mouthColor.G + mouthColor.B) > .05f);
        study.ResetPresence();
        Check("presence release restores original weights and pivots", study.PresenceHead.Transform == headBase
            && study.PresenceBody.Transform == bodyBase && study.PresenceEye.GetBlendShapeValue(blinkIndex) == 0);
        study.BindPresence(); study.ApplyPresencePose(new PresencePose(.8f, 0, 0, 0, 0, Vector3.Zero, Vector3.Zero));
        study.PresenceEye.SetBlendShapeValue(blinkIndex, .25f);
        Check("presence competing morph writer revokes binding", !study.PresenceRig.Apply(default) && !study.PresenceRig.Bound
            && Mathf.IsEqualApprox(study.PresenceEye.GetBlendShapeValue(blinkIndex), .25f));
        study.PresenceEye.SetBlendShapeValue(blinkIndex, 0);
        study.BindPresence(); study.PresenceEye.Mesh = PresenceCalibrationShapes.Add(AnimeCalibrationGeometry.Eye(1), "eye");
        Check("presence reimport invalidates binding safely", !study.PresenceRig.Apply(default) && !study.PresenceRig.Bound);
        study.PresenceEye.Mesh = sourceMesh;
        study.BindPresence(); study.PresenceHead.Position += new Vector3(.01f, 0, 0); var external = study.PresenceHead.Transform;
        Check("presence external pivot movement is never undone", !study.PresenceRig.Apply(default) && study.PresenceHead.Transform == external);
        study.PresenceHead.Transform = headBase;
        rival.QueueFree();

        var fx = study.Phenomenon;
        var environment = study.StudyEnvironment; var camera = study.StudyCamera;
        fx.Configure(true, PresencePhase.Night, "Ultra", false, false);
        Check("makai instancing has a hard upper bound", fx.ActiveMotes == MakaiPhenomenon3D.MaximumMotes);
        fx.Configure(true, PresencePhase.Night, "High", false, false);
        Check("makai high tier has bounded detail", fx.ActiveMotes == 64);
        fx.Advance(.05, false); var fxTime = fx.VisualSeconds;
        fx.Advance(1, true);
        Check("makai pause freezes shader and particle timeline", fx.VisualSeconds == fxTime);
        fx.Configure(true, PresencePhase.Night, "High", false, true); fx.Advance(1, false);
        Check("makai reduced motion retains a static veil without motes", fx.EnabledForView && fx.ActiveMotes == 0 && fx.VisualSeconds == fxTime);
        fx.Configure(true, PresencePhase.Night, "Low", false, false); fx.Advance(1, false);
        Check("makai low tier preserves static style without animated detail", fx.EnabledForView && fx.ActiveMotes == 0 && fx.VisualSeconds == fxTime);
        fx.Configure(true, PresencePhase.Night, "High", true, false);
        Check("makai interior shelter suppresses exterior effects", !fx.Visible && fx.ActiveMotes == 0);
        fx.Configure(true, PresencePhase.Morning, "High", false, false);
        Check("makai effect respects caller supplied daytime", !fx.Visible && fx.ActiveMotes == 0);
        study.SetPortrait(false); study.SetLighting(4); study.SetMakaiPreview(false);
        var off = await Capture("makai-off");
        study.PresenceClock.TryUpdate(study.PresenceClock.Snapshot with { Paused = true });
        study.SetMakaiPreview(true);
        var night = await Capture("makai-night");
        Check("makai prototype changes actual rendered sky area", Difference(off, night) > .0001);
        Check("makai keeps existing environment and camera ownership", study.StudyEnvironment == environment && study.StudyCamera == camera);
        study.SetMakaiPreview(false);
        Check("makai disable clears visibility and visible particles", !fx.Visible && fx.ActiveMotes == 0);
    }
}
