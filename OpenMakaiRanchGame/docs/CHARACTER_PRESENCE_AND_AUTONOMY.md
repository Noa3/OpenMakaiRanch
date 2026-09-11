# Character presence, combinable archetypes and bounded autonomy

Checkpoint: 2026-09-12. Companion documents: [MAKAI_WORLD_STYLE_AND_LORE.md](MAKAI_WORLD_STYLE_AND_LORE.md), [ANIME_REFERENCE_QUALITY.md](ANIME_REFERENCE_QUALITY.md).

## Branch ownership / handoff to parallel agents

Branch `feature/makai-presence-and-worldstyle-20260911` starts at main `1e7f90936849443f1605e573a37d3e43e0676245`. The earlier material pipeline is already merged there. New runtime/library code is confined to `src/Visuals/`; only the existing isolated lab and its evidence capture list are extended. No new GitHub workflow, package dependency, save schema, engine or language version is introduced.

**Logic agents retain ownership** of `GameRoot`, `ScheduleService`, `DatingService`, `RanchService`, settlement, needs/resources, traits/relationship changes, save/load and the canonical calendar. The world/stations agent retains `RosterRig`, `CharacterAvatar3D`, movement, collision, navigation, station availability and `WorldAtmosphereController`. Do not merge this by replacing their files or implementing a competing daily scheduler.

This pass delivers usable, tested building blocks and an interactive lab, not autonomous production residents or final facial animation assets. Existing residents and the player's controls remain unchanged.

## Archetypes as starting points, not permanent boxes

Separate four concerns: **appearance/species/body and voice**, **long-term personality preferences**, **temporary state**, and **relationship/context**. None implies the others. A stern-looking character can be warm; a reserved person can be confident in familiar work. Do not derive personality or moral worth from gender, body shape, skin tone or species.

`PresenceTemperament` contains eight normalized, mixable axes: sociability, expressiveness, composure, confidence, playfulness, curiosity, diligence and warmth. Twelve convenience presets are provided: quiet/reserved, calm/stoic, proud/guarded, lively/social, playful/teasing, warm/caring, refined/formal, dutiful/perfectionist, curious/scholarly, dreamy/artistic, competitive/athletic and tired/pragmatic. `PresenceTemperament.Blend` and the exported `PresenceStyleProfile` resource support custom values; the catalogue is not an enumeration of every possible character.

This is a presentation vocabulary, not psychological measurement. Diligence is reserved for later authored activity preferences; it does not currently alter duties or grant productivity. No new character-creator interface or universal body/voice generator is delivered. Player-character presentation could use the same profiles on explicit user selection, without automatically moving the player, answering dialogue, choosing a relationship or changing input response.

Original source already separates some generic speech categories, including strong-willed, refined and quiet dialogue files. Preserve original trait IDs and conditions in the logic owner's model; do not silently map those categories into invented persistent stats. Upstream dialogue headers have their own attribution/noncommercial notices. All new presets, microgesture curves and lab shapes here are original code rather than copied dialogue or artist assets.

## Implemented expression and microgesture model

`PresenceMotion` samples deterministic per-character visual motion. Its time and seed are independent of gameplay randomness and the world clock. It supplies blink, smile, raised/contracted brows, simple mouth opening, restrained head orientation and millimetre-scale torso movement. Warmth, confidence, composure, expressiveness, playfulness, supplied fatigue/trust/context affect the relevant output. Actor IDs desynchronize residents.

`PresenceTimeline` owns only a cosmetic time accumulator. Hidden/paused actors freeze; invalid deltas are ignored; long frame catch-up is capped. Explicit identity/session resets clear transient state. Older revisions or another actor/session cannot replace its snapshot implicitly. An acknowledgement cue produces a brief nod; callers should trigger this on a meaningful dialogue beat rather than looping nods at every text character.

Reduced motion suppresses head/body drift; functional facial expression and blinking remain. A cinematic ownership flag suppresses generated pose. The lab releases its rig binding when a cinematic takes over. Resuming requires an explicit bind; a lost binding is not automatically fought for every frame.

**Limits:** the head motion is not independent anatomically solved eye gaze; the torso offset is not hand animation or a locomotion rig. `SpeechEnvelope` is a caller-supplied amplitude, not phoneme alignment or audio analysis. The test mouth is a graphic shape, not a production mouth cavity. The timing is artist-tunable, not a claim about clinical human blink/saccade norms.

## Explicit rig binding, not animation ownership theft

`PresenceRig3D.Bind(headPivot, bodyPivot, mappings)` accepts **dedicated unkeyed additive pivots** and named blendshape slots. The adapter validates names, duplicate mapping, current mesh and finite weights before taking a lease. Shape weights are instance-local; source mesh data stays unchanged. Baseline pose/weights are restored only while still owned and unchanged by another writer. Reimport, an external pose/shape write or another lease causes a safe release rather than an animation tug-of-war.

Production rigs must intentionally route channels into their AnimationTree or suitable post-animation rig modifiers. Do not bind this node to an AnimationPlayer-keyed skeleton root and assume update order will solve blending. Godot's `SkeletonModifier3D` is the relevant post-animation bone path [1]; with an AnimationTree, keep animation playback ownership there [2]. MeshInstance blendshape methods are the explicit per-instance interface [3]. The lab's ArrayMesh targets follow the vertex/normal/tangent contract [4].

The original calibration specimen now has actual named shape targets, but uses approximate normal treatment and schematic topology. This is not a high-quality character replacement. A production face needs authored neutral/expression shapes, eyelid closure over eyeballs, asymmetric poses, gaze limits, lips/teeth/mouth interior, corrective shapes, hair/clothing motion and transitions verified from multiple angles. Hand gestures should be authored clips with contact/IK and priority masks, not random arm jitter.

## Read-only autonomy contract

`PresenceSnapshot` is immutable input supplied by the simulation owner. Values are normalized urgency, not new resource inventories. Unknown opportunities are **closed by default**. Hunger, hygiene, social and quiet needs may be supplied when the owning logic actually supports them; absent systems do not get fabricated persistent values or passive decay in this branch.

`PresenceIntentAdvisor.Recommend` returns a stamped `PresenceSuggestion` containing actor, session, revision, intent, reason and score. It issues **no command**, changes no stat and supplies no path. Priority currently is: locked presentation -> no suggestion; unsafe weather -> shelter (or explicit unavailable reason); urgent fatigue/hunger -> available recovery (or unavailable reason); active conversation; explicitly accepted companionship; scheduled duty; otherwise available needs/leisure/visible phenomenon. A bounded score-retention bonus reduces ordinary indecision without retaining unavailable activities. This is a reviewable starting policy, not final balance.

Same state may produce different free-time preferences: a social resident can prefer company; a reserved resident can choose quiet; curiosity can make a visible accessible phenomenon interesting. High trust alone never authorizes following or an invitation. A smile, blush, archetype or nod is not consent, a promise, a trait change or evidence of a hidden decision.

### Logic-agent integration sequence

1. Construct an immutable snapshot from canonical state. Use stable actor/session IDs and increment revision when action-relevant context changes. Keep style preferences separate from original traits unless an explicit mapping is reviewed.
2. Derive `Available` from actual enabled facilities, permissions, reservations, reachability and supported needs. Opportunity bits are not new station IDs. Closed/inaccessible food does not become a meal because a score is high.
3. Evaluate at a bounded decision cadence or meaningful event, not a fresh random action every frame. Retain the accepted intent and held time in the owning controller, not in global static state.
4. Before dispatch, revalidate actor/session/revision, resource availability, actor willingness, facility access and navigation. `Matches` is a stale-snapshot guard, not authorization. Reserve a destination with an expiry; resolve competing residents fairly.
5. Execute through the **existing** command/service owner, then present the accepted action using `RosterRig`/animation. Display a short reason when a resident declines, waits or cannot find shelter. Never silently teleport or pay a second job reward.
6. Settle consequences once through canonical services. Resource recovery, relationship changes, lost duties and trait evolution belong there. The visual adapter only displays accepted states, and releases before a conversation/cinematic/locomotion owner requires its channels.

A small C# example for the ownership boundary (not a complete command adapter):

```csharp
var suggestion = PresenceIntentAdvisor.Recommend(snapshot, style.Values, previousIntent, heldSeconds);
if (!suggestion.Matches(currentSnapshot)) return;
// Owner must now revalidate availability, permission, path and reservations.
// Only an accepted canonical command may move an NPC or change needs/resources.
```

No polling of a supposedly read-only service that creates progress/save records is added. No separate real-time needs simulator, game calendar or save migration is hidden in presentation code.

## Interactive lab

Use the existing isolated host:

```bash
python Tools/Godot/anime_lookdev.py --renderer forward_plus --interactive --timeout 3600
```

Provide the installed Godot Mono executable through `--godot` or `GODOT_BIN` as before. Linux requires a display or `xvfb-run -a`. Enable **Microgestures**, choose a preset, use **Acknowledge**, then try synthetic **Conversation / Tired / Unsafe weather / Free time** contexts. **Reduced motion** removes ambient head/body motion. Select **Night** or **Sunset** and enable **Makai veil** to view the environment prototype. These are lab fixtures, not real resident state or game-time commands.

Automatic rendered acceptance retains the previous 16 material captures and adds five pose/phenomenon views. It exercises actual instance weights and changed rendered pixels, plus finite inputs, stale snapshot rejection, pause/visibility ownership, available-action priorities and visual-effect budgets. Passing these does not certify natural animation or reference-artist/AAA quality. See [PRESENCE_VALIDATION.md](PRESENCE_VALIDATION.md) for actual run status rather than treating this planned test description as a pass receipt.

## Next production gate

One original/licensed character, fully clothed for neutral rig validation, in a small real ranch exterior and interior. Review acknowledgement, listening, refusal, interruption, fatigue and a quiet shared moment. Add look-at targets with limits, facial asymmetry, appropriate authored hand gestures, pose matching and sparse reactions to the world. Measure motion quality, style coherence, frame cost and interference with existing gameplay. Only then roll out to multiple residents and creator presets.

## Technical sources

[1] https://docs.godotengine.org/en/stable/classes/class_skeletonmodifier3d.html

[2] https://docs.godotengine.org/en/stable/classes/class_animationtree.html

[3] https://docs.godotengine.org/en/stable/classes/class_meshinstance3d.html

[4] https://docs.godotengine.org/en/stable/classes/class_arraymesh.html

Original source provenance and exact inspected hashes are in `MAKAI_WORLD_STYLE_AND_LORE.md`. Current engine/framework/language settings are retained, not inferred from the moving `stable` documentation alias.
