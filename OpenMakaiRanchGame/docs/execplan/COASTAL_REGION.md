# Okachi coastal region: connected terrain before final landscape art

## Purpose and current stage

Implement the user's larger coherent region without discarding the organic-core work: wooded hinterland → raised ranch terrace and meadows → optional valley footpath → market settlement → accessible bay beach and small pier. First validate a playable terrain/building blockout, then refine only a ranch reference area and a contiguous coast slice. No streaming, fishing, vessels, tide simulation, new quests, economy, clock, weather or progression systems.

This is a living execution plan. The numerical blockout targets below are proposals, not approved final art. Do not present partial checks as completed acceptance.

## Paused handoff — rejected coastal candidate

**Superseded priority:** the newer user request authorizes completing the connected ranch/public-market slice. `GROWING_REGION.md` is the active execution plan. The rejection below remains historical evidence until the strict bridge/route checks actually pass; it is not permission to activate an unverified region.

Ranch house, residential modules, furniture and their integration now have priority. Preserve this work; do not activate the region or start another landscape export during the ranch-center slice.

- Parent verified the delayed worker handoff on `astar`: 10 modular GLBs and `assets/3d/coastal_region/coastal_region.blend` exist. Module/source hashes passed the real geometry suite. `generation_receipt.json` explicitly records `accepted:false` / `rejected_blockout_candidate`.
- Fresh `python -m unittest discover -s Tools/WorldDesign -p test_coastal_geometry.py -v`: **6 pass, 1 fail; exit 1**. `test_connection_width_polygon_and_grade` rejects ground deviation `0.1394483787870735` against limit `0.12`. Do not weaken the threshold to hide missing road ground.
- Independently recomputed centerline crossing: regional **X65.15140845070422, Z174.22535211267606**, lane Y4.215492957746479, water Y3.176056338028169. Stream carving cuts the valley lane; a deliberate reroute or planned second crossing is required. Full-width export guard already rejects near X61.818/Z166.818 and executes before any existing export/heightfield write. Parent invoked this guard read-only and confirmed rejection; no regeneration performed.
- Source hash remains `df2a7fc867768b91cc5a82747f8f098fdaa7b7de3ada07b01b7face69fa00ccf`.
- Regional runtime files and shared-code changes are preserved. Scene reference search finds the coastal controller only in `scenes/dev/CoastalRegionBlockout.tscn`; `CoastalRegionWorldHost.CoastalRegion` is a subtype cast, null in ordinary WorldGame. Parent re-read the shared diffs and ran Debug build: **exit 0, 0 errors**, no warnings in that incremental run. The earlier full build's CS8602 remains separately documented.
- **Acceptance metadata is not a runtime lock:** `CoastalRegionTerrain.Load/Parse` checks source/grid/module contracts, not `generation_receipt.json`'s `accepted` field. Do not interpret the rejection receipt as proof that manually opening the dev scene is prevented. No coastal runtime, seam traversal, follower journey, save/load or performance acceptance was executed.
- Existing smoke passes do **not** resolve the separate historical UI-acceptance failures and do not prove walking. Quick travel and fixture teleports remain excluded from route evidence. No rollback, commit, push or production activation performed.

## Preservation and baseline

- Branch `astar`; no commit, push, reset or foreign-file rollback authorized.
- Verified working-state checkpoint `.dream-loop/checkpoints/pre-coastal-replan-20260912T025115Z.zip`, SHA-256 `93dbba5b307c089f479ea5d170086ad0c7874935be23547e8c9a8b2100d87645`. Archive CRC and per-file hashes verified; source manifest, binary diff and untracked files preserved. This checkpoint includes the initial regional planning additions; retained earlier assets are not overwritten.
- Old assets `assets/3d/organic/*`, Blender sources, references and test evidence remain. They are prototypes, not final terrain. `build_organic_world.py` now rejects new exports while `export_policy.legacy_small_landscape_paused` is true.
- Previous workers had finished when correction arrived. No worker remains authorized to export small landscapes. Completed character proposal is separate and does not gate this plan.
- Previous intermediate smoke: `SMOKE PASS` from `proc_ef1826d5dc67`; last rendered UI suite already had 22 failures (Help/focus, town journeys, small station panels). Preserve this baseline separately from new failures.

## Confirmed constraints and scale

`docs/MAKAI_WORLD_STYLE_AND_LORE.md:58-59` identifies civilian institutions and says Okachi/Makkai Plains naming is conditional on joke display, not mandatory serious geography. `docs/art/OKACHI_TOWN_WORLD.md:11-19` documents both South Gate connections and free travel; its lines 53-79 describe the old local square, not a mandatory coastline. No fixed coast cardinal direction was found in these canonical documents. The regional east coast is therefore a documented new layout decision, not claimed original canon.

Both present portals use local south: ranch departure at positive local Z, town return at positive local Z. Preserve local labels and rotate the town's regional coordinate frame by 180 degrees. Do not silently rename story exits. Confirm the intro arrival still uses the existing portal flow.

`ThirdPersonPlayerController.cs:12-17` defaults to MaxWalkSpeed 5, SprintMultiplier 1.65, Acceleration 40 and HeadHeight 1.6. Scene overrides and actual capsule/visual heights must be measured live before timing claims. Existing shell footprints remain 4.6–7.6 units. Adopt one unit ≈ one metre; do not scale actors/buildings or alter movement, phase duration or stamina costs. Analytical lengths are planning evidence only; real traversal needs timestamps and positions from the running player.

## Shared spatial source and datums

Keep `data/world/organic_layout.json` as source. Its top-level ranch/town plots are reusable compact cores; `region` adds explicit regional position, local orientation, elevations, boundaries, water and transitions.

- North = regional -Z, east = +X. Sea level = regional Y 0.
- Ranch root = (0,12,0), yaw 0. Nominal core/rand area 120 × 100 m. Compact current services retain positions and footprint sizes on the 12 m terrace.
- Town root = (70,4,220), yaw π. Nominal town/coast area 180 × 130 m. Market/services remain on the 4 m terrace; sea is town-local west, regional east.
- Optional connection belongs to the ranch scene; it extends beyond the core rectangle in a named explicit corridor. Its six XYZ controls descend from 12 m at the farm exit to 4 m at the town seam. Never leave an undocumented boundary hole.
- Scene seam: ranch local (70,-7.2,185) and town local (0,0.8,35) are the same regional player centre (70,4.8,185). Only swap active scene/actor at this shared point. Quick travel remains separate and cannot count as walking evidence.
- Stream water surface: 31 → 15 → 10 → 7 → 4 → 2.8 → 1.4 → 0 m. Interpolate heights monotonically even when smoothing plan-view curves. Bed below water, banks above it; no terrain ridge crossing or arbitrary decorative hills.
- Core plateaus cover complete rotated building, doorway and approach envelopes. Use full-width sampled path ribbons, not only control-point/centreline clearance.
- Coherent western valley shoulder and source hills frame the region. Extra ranch area serves meadows/optional exploration. A short fixed footbridge connects the west meadow across the stream.
- Bay shoreline and land boundary are separate: beach remains at least 0.75 m above sea level; services stay on terrace. A descent south of the mouth reaches the beach and guarded pier (deck 1.4 m above sea). A single southern headland orients the bay.
- No swimming. Explicit stream-bank, shore and pier-edge colliders keep dry routes safe; display water but never treat visible water as playable land. Boundaries and terrain collision must agree.

## Runtime/asset handoff contract

Use opt-in `scenes/dev/CoastalRegionBlockout.tscn` first, based on existing WorldGame/services; production activation follows actual verification. Do not write a second game simulation.

Offline modular outputs under `assets/3d/coastal_region/`: `{ranch,town}_terrain.glb` (named terrain tiles with collision), `{ranch,town}_paths.glb`, `{ranch,town}_water.glb`, `{ranch,town}_safety.glb` (collision-only bank/shore barriers), `ranch_bridge.glb`, `town_pier.glb`. Separate land, paths, water, safety, vegetation and buildings. `.blend` source and generation receipt retained. Do not integrate old all-in-one landscapes or bake buildings into terrain.

`data/world/coastal_heightfields.json` is generated from the same source bytes; schema: `{version:1,source_sha256:"...",areas:{ranch:{bounds:[x,z,width,depth],step:1.0,columns:N,rows:M,heights:[row-major local Y],modules:["res://..."]},town:{...}}}`. Grid cell faces use diagonal (i,j)→(i+1,j+1); CPU queries must match that triangulation, not invent a different bilinear surface. Pier/bridge are separate dry collision surfaces; physical raycasts or navigation height may take precedence over base heightfield. Generator must emit validation evidence and reject inconsistent downhill/clearance/seam geometry before export.

Runtime adapters read the region; position/rotate roots, deactivate old flat ground and small greybox walls only in region mode, use true boundary polygons and new navigation AABBs. WorldBoundaryBuilder must not reintroduce old extents. Preserve ordinary quality/season settings; final vegetation must remain separate and respond to existing controllers.

Fix all flat-height assumptions in player spawns, NPC targets/follow positions, RosterRig navigation steps and local/global conversion. `Entrance` height is an actor offset relative to a plot surface/local area datum, not an unconditional world Y. Do not let companion use a through-wall direct-move fallback or teleport to hide navigation failure.

## Sequence and acceptance

- [x] Preserve checkpoint and old assets; pause legacy exporter.
- [x] Inspect canonical geography, current portal topology and player movement source.
- [x] Define joint spatial input, datums, water course, compact cores and explicit transition corridor.
- [ ] Produce overview with elevations, stream, bay, routes and real walkable boundary; validate widths/clearance and source geometry.
- [ ] Build modular playable terrain/building blockout; add opt-in scene using current game services and existing input.
- [ ] Verify actual player travel: ranch → required stations → optional valley route → town services → beach/pier → return. Camera and collision must work at real heights.
- [ ] Repeat relevant routes with existing companion; record following distance/stalls, route continuity and height error. No teleports as route proof.
- [ ] Measure capsule/visual scale and stopwatch/physics elapsed traversal lengths at unchanged movement parameters; report time per leg, not only estimates.
- [ ] Verify current-schema save/load, tutorial/unlocks, facilities and quick travel; run smoke and targeted integration tests. Keep pre-existing UI failures distinct.
- [ ] Verify Forward+ and affected quality/reduced-motion/season settings with real screenshots.
- [ ] Only then refine the existing ranch core and small continuous bay section; rest may remain readable blockout.
- [ ] Activate verified region in ordinary play, or leave opt-in and state precise blockers. No false completion.

## Validation strategy

Run `dotnet build` in the game directory, Python unit suites, isolated Godot launcher/import, then a rendered isolated regional traversal fixture using real Input actions and physics. A fixture may initialize state once and use matching-anchor scene transitions; it must not reposition the player to skip route legs. Save positions/time as a trajectory and verify maximum inter-frame displacement. Include companion in the same recording. Compare semantic gameplay state across travel and current-schema save/load.

Use terrain-source hashes, generated-module hashes, route logs and PNGs together. A Blender export, navigation path, overhead image, analytical speed calculation or one teleport does not establish playable walking.

## Risks/open questions

Known flat assumptions include `RosterRig` setting assignment Y to zero and discarding navigation-path Y. Earlier capture's fixed camera coordinates and old boundaries do not test slopes. Town rotation means all transforms must use actual local/global conversions. Water barriers can split navigation; bridge openings need matching gaps. Full path ribbons may overshoot concave boundaries after smoothing. Separate area nav maps/disabled collisions must not leave stale bodies at seams. Seasonal and low-quality variants need testing; newly authored fixed green trees are not automatically season-compatible.

This document separates plan from implementation. Update checks and evidence only after actual execution; do not turn these proposed values into unsupported final approval.
