# Bounded real regional traversal — blocked at valley bridge approach

## Result

**Failed route; verified recording; `accepted:false`.** One graphical engine attempt, no retry and no production repair. Run `.dream-loop/capture-pyefrgv4` used Godot 4.7.2 Mono, Forward+, RTX 5090, 1600×900 and render scale 1. Existing isolated capture launcher built the current dirty sources successfully (0 errors, existing CS8602 warning), checked disposable `user://`, and verified unchanged source/DLL provenance across execution. No engine ERROR occurred; existing fractional `region_min_size` warnings remain. Launcher exited **1**, correctly distinguishing a recorded failure from a passed route.

The real player started at regional ranch spawn `(0,12.8,10.5)`, approached the home front at ranch-local `(-10,4)`, returned to the lane, crossed the meadow footbridge to the west endpoint and returned, then followed the authored valley road through point 4 `(61,4.4,165)`. Player stalled approaching point 5, the start of `ranch_valley_bridge.glb`. The valley bridge crossing, regional seam, civic room and market were **not reached**.

## First blocker / repair handoff

- Segment: `valley road 5`, authored `region.connection.points[4] → [5]`.
- Target, regional XYZ: `(68.53518,4.22,169.27055)`.
- Actual player body origin: `(66.55552,5.075163,168.13174)`.
- First rail contact: recorded frame **3449**, engine physics frame **3520**. Repeated through driving frame **3868**; fixture stopped after seven seconds without material progress.
- Solid: `/root/RegionalTraversalCapture/WorldGame/RanchWorld/CoastalModules/ranch_valley_bridge/valley_rail_1/StaticBody3D`.
- Contact point: `(66.685005,5.52,168.6107)`; normal `(-0.82580084,0,-0.56396186)`.
- Input remained nonzero `(-0.86680925,0.49863988)` through ordinary `MobileMovementInput`; world-input gate was true, body velocity zero, body on floor. This is an actual physical rail contact, not an input lock or a synthetic overlap rejection.
- Hypothesis: the authored approach arrives nearly across the bridge axis and strikes the full-length side rail just behind its open start plane. Inspect approach/deck/rail geometry and adequate turning clearance together. Current generation source builds both side rails over the whole deck length. The blocker screenshot visibly places the player against the near rail; trace supplies the actual collider and normal.
- This proves **the tested authored centreline approach is blocked**, not that every possible player approach to this bridge is impossible. No invented detour, rail disabling, radius reduction, teleport, asset rewrite or production fix was used to hide it. Parent should repair or explicitly approve a genuine approach fixture correction before further walking attempts.

The follower remained moving, not frozen: final position `(18.078814,9.062276,80.639915)`, well behind the player on the long road. Its last 400 recorded frames contain about 16 m movement. The fixture waits for follower arrival at the home/meadow and intended bridge/seam/town destinations, not every intermediate road waypoint. Therefore no follower arrival at the valley bridge is claimed, and its lag here is **not evidence of a navigation stall**.

## Evidence and exact scope

- `evidence/regional-trace.jsonl`: **3,870 consecutive physics frames**, engine frames 72–3941, recorded from `SceneTree.PhysicsFrame`, including screenshot waits. JSONL is flushed each frame so a timeout retains preceding motion evidence.
- Global positions, active area, player/escort instance IDs, velocity, input, floor state, player slide contacts, follower navigation target, seam count/blocker and sampled capsule results are recorded. Active area stayed `ranch`, player and escort each retained one instance ID, transition count stayed zero. Seam handoff continuity is implemented in the recorder but **not exercised** by this failed run.
- Approximate accumulated movement: player **280.42 m**, follower **154.76 m**. Maximum per-frame displacement: player **0.10868 m**, follower **0.04024 m**; no recorded spatial jump.
- **967** follower-capsule samples, **zero** overlap samples. Radius 0.35 m, actual prepared character height; checks every fourth physics frame, **sampled and not swept**. Follower is the existing bounded nav-path mover, not a CharacterBody3D solver. These samples do not certify every surface/contact between them.
- Four actual PNGs: `regional-start`, `regional-home`, `regional-meadow`, `regional-blocker`. All dimensions and hashes validated. Active regional gameplay cameras/HUD remain visible; these are not the ordinary ranch P01 camera or clean art-review shots. Images were visually inspected; the home camera is close to the doorway and is not an interior-access tour.
- Home test means front approach only at `(-10,4)`; no full interior route, furniture interaction, sleep or indoor service acceptance. Existing rancher was prepared as escort, not invited through earned gameplay. Fresh isolated state bypassed first-day story presentation; the independent ranch-control tutorial overlay remains visible in the screenshots.
- Player/follower reached the western meadow endpoint together. Player crossed the meadow bridge outward and back; follower followed its own navigation route continuously. No claim that it exactly retraced all player waypoints or crossed both bridges.
- No `TravelTo`, player/follower position assignments, speed/radius/navigation tuning or transactions occur in the recorded fixture route. Production seam logic may perform a same-pose actor handoff; no handoff occurred here. Real regional spawn comes from the existing regional host during setup.
- No new economy, costs, construction state, save schema, clock/day/stamina changes, ordinary gameplay activation or broad acceptance. Personal saves, original game and existing editor were not touched. No commit/push/merge/reset.

## Files and reproduction

```bash
OMR_REGIONAL_TRAVERSAL=1 python Tools/VisualTargets/capture.py
```

- New authored `scenes/dev/RegionalTraversalCapture.tscn` instances the existing opt-in regional scene.
- New `src/Dev/VisualTargetCapture.RegionalTraversal.cs` owns movement, continuous recording, checkpoint screenshots and bounded failure output.
- Minimal flag dispatch in existing `VisualTargetCapture.cs` and `Tools/VisualTargets/capture.py`; other modes retain their 120-second engine timeout, regional mode uses 240 seconds with a 190-second in-fixture budget and bounded segment/setup waits.
- New `Tools/VisualTargets/regional_traversal.py` validates recording/provenance/PNGs and keeps failed route status separate from evidence integrity.
- New `test_regional_traversal.py` uses explicitly synthetic/mocked validation data, **not engine evidence**. Entire VisualTargets suite ran normally and with `python -O`: **58 tests, 1 skipped, no failures** (57 executed tests passed). Optimized output preserved in `wrapper-tests.log`.
- Build, engine, console, profile-isolation logs and exact `source-state.json` preserved beside this report; actual JSON/PNG evidence copied unchanged under `evidence/`. `summary.json` includes the first rail contact and last driving frame.

The test module and this report were added after the engine capture; no capture/production source was repaired afterward. Snapshot describes exactly the code/assets built for the recorded run. The existing town-only green result is separate evidence and is not reused as regional proof.
