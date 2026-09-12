# Finish the inhabited ranch–Okachi market connection

This is the active execution plan for the user's growing ranch/coastal-region request. Progress, discoveries, decisions and outcomes remain evidence-based and are updated during work. It supersedes the pause in COASTAL_REGION.md, not the preserved assets or safety constraints.

## Purpose / Big Picture

Make one useful, connected place: the existing furnished ranch home, the valley road, a modest public administration/supply house and a small market court. The player enters rooms without an automatic menu, approaches recognizable furnishings/people for existing actions, and can see and use one completed public improvement. Later town districts are reserved spatially, not mass-modeled or individually simulated now. The long-term architecture should suggest roughly 300 residents without 300 persistent background NPC records.

## Progress

- [x] (2026-09-12 19:47+08:00) Live repository confirmed: astar, clean starting HEAD 1a65868, user-uploaded prior asset work present. No branch switch, merge, stage, commit or push authorized or performed.
- [x] Read AGENTS, current handoff/decisions/issues/kanban, existing coastal layout/runtime, home/material evidence and actual station/plot implementation.
- [x] Verify executable versions: Blender 5.2.1 LTS, Godot 4.7.2 Mono, Material Maker file/product 1.7.0.0, Krita 5.3.3, dotnet SDK 10.0.401; retain net8.0, Godot.NET.Sdk4.7.0, C#12 and existing save architecture.
- [x] Add first-slice groups, later residential/workshop reservations and permanent open spaces to the existing organic_layout.json development section. These are design targets, not implemented unlocks.
- [x] Record the user's selection of the shown C01 duo with its exact image/provenance in C01_ANIME_DESIGN_SELECTION.md and the atlas. Previous manifest, image/review/history retained; no new generation or roster identity reassignment. Atlas validates with this one approved target.
- [x] Resolve full-width valley-road/stream geometry with an explicit bridge and dry approaches; preserve old candidates, strict geometry checks and existing meadow bridge. Eleven modules re-exported against final dry-terrace house placement; nineteen WorldDesign tests pass. Physical traversal remains separate.
- [x] Audit original/remake tax/resource/lore authority. Paid community deliveries are earned participation, not donations or proven construction finance; document test-only construction contract in `docs/art/REGIONAL_DEVELOPMENT_FACTS.md`. No charges added.
- [x] Integrate existing authored ranch home into the actual ranch scene: shared kitchen/office/house room anchors, imported metre-scale geometry, 28 furniture bodies, 14 room lights, two shelter volumes and roof-only cutaway. Parent corrected gate overlap and outdoor activity anchors; isolated Forward+ startup/capture passes. This is integration evidence, not full interior-use acceptance.
- [ ] Verify home room access with player/companion and actual indoor actions. Tutorial/night/convenience smoke passes at final placement; explicit traversal/furniture use remains open.
- [x] Recover public house/annex/cap after worker timeout: three real GLBs and editable Blender source verified, then composed as shared Core plus Base/Expanded Godot scenes with existing furniture. Six actual Forward+ views and cap/door-center collision rays pass; full evidence in `docs/art/okachi_supply_house/README.md`. This is asset/variant verification, not production placement or a gameplay expansion.
- [x] Model and independently verify five market/construction GLBs and their unchanged Blender source. Compose shared court plus Base/Work/Finished scenes; actual Forward+ capture verifies imports, scale, work-stage roof overrides, obstruction and free bypass volumes. Evidence: `docs/art/okachi_market/README.md`; no live/persisted project claimed.
- [x] Recover `deleg_cf879fcf` integration into the ordinary town: authored civic rooms and Base market, six spatial entries retaining seven service IDs; town-only layout and regional exports synchronized. Actual local player/follower movement and existing reception/market actions verified in `art/town-core-integration/README.md`. Full usable construction project remains open.
- [x] Reproduce and fix follower navigation defects exposed by real movement: raster/foot-height mismatch, doorway raster resolution, premature corner advancement and invalid rejection of long valid path segments. Record continuously through UI/captures; both actors reach the two civic rooms, market and western exit with unchanged movement settings and no transactions.
- [ ] Wire a meaningful existing public action and a usable completed shelter/market space; construction animation/audio/day-night/weather presentation through existing state authority, no fabricated financial transactions.
- [ ] Verify player and actual companion movement continuously through the slice, including doors/bridge, without teleporting for route proof.
- [ ] Verify base/work/finished conditions under day/evening/night/rain, low/high Forward+, save/load and repeated events/menus; separate actual transactions from synthetic fixtures and slow/efficient policies.
- [ ] Promote only verified slice to ordinary play; preserve original assets and useful test scenes, update canonical handoff/issues with actual evidence.

## Surprises & Discoveries

Actual town traversal caught defects that source/GLB tests could not: stationary followers above raised nav waypoints, doors erased by coarse voxels, corner clipping and an eight-metre segment cutoff. The corrected run `capture-un6uxw5a` records 1,498 physics frames and five real PNGs. The player uses ordinary physics; the follower uses bounded nav-path movement with sampled capsule checks every fourth frame. Scene entry/escort selection are prepared, not regional walking or an earned invitation. The broader smoke initially found one obsolete assertion equating seven services with seven separate parcels. That assertion now checks six parcels plus seven actual loaded services. Rerun `proc_a5a41e4133a8` passed with exit 0 and `SMOKE PASS` (2,118 SMOKE OK, zero SMOKE FAIL); its exact log is archived under `art/town-core-integration/smoke-console.log`. Independent read-only review `deleg_c70b79da` is complete: no new evidenced must-fix regressions in the focused scope; report hash and eight source fingerprints rechecked by the parent. See `art/town-core-integration/REVIEW.md` for explicit limits. The planning room is reachable but its dedicated action remains blocked. No fake duplicate building was added to satisfy a count assertion.

The canonical handoff still names a former feature branch. Live Git and current user instruction select astar instead. Its historical green test counts are not this task's validation.

At task start the coastal candidate failed a full-width route check (six passes, one failure): the valley lane crossed the stream near regional X65.151408/Z174.225352. The explicit bridge and matching bank openings now pass strict geometry checks. Rejection metadata is not a runtime launch lock; the scene remains opt-in and awaits physical traversal/navigation acceptance.

At task start house/furniture sources and a textured furnished prototype existed, but ordinary ranch still used independent kitchen/office/house shells and obsolete 49x39 bounds. The integrated ranch now uses one authored home and 120x100 bounds. Physical plots are not service IDs: consolidation preserves kitchen/office/house identifiers and their facility requirements.

No resident upper bound or room/bed allocation was implemented by previous asset work. Seven sample beds are not a population guarantee. Active mannequins are rigid stand-ins, not articulated production rigs. Prior C01 images are designs, not meshes or animations.

The earlier material pass loaded 18 assets and three materials in Forward+ with stable source bytes, but showed dark/mottled interiors. Improve authored lighting and material restraint; do not call technical map wiring final art quality. Existing compiled UI/smoke evidence is not all-route or furniture-use evidence.

The first parent graphical run rejected the worker's otherwise compilable placement: `ranch_house: blocks town gate approach` in BuildFacilityLandmarks. Godot continued a partially constructed scene and printed VISUAL CAPTURE PASS, but the wrapper rejected ERROR lines. A westward correction passed the flat-ranch boot, then failed regional envelope heights at the carved stream bank. A combined gate/parcel/full-envelope probe selected a dry northern position without weakening either guard. Initial quiet-corner proposal at Z=11 occupied the home lane; presentation now uses Z=14 and approach Z=15.

## Decision Log

Decision: retain existing region datums, stream and both local South Gate names; use one intentionally planned main-lane footbridge rather than casually relocating the river/settlement. Reason: least coherent terrain change and a visible explanation for the crossing. Bridge endpoints/width must be measured and verified, not guessed into acceptance.

Decision: use one shared spatial source, data/world/organic_layout.json. Its development section holds shared master targets and green reservations, while existing ranch/town/region sections remain actual imported layout authority. Unbuilt district records do not create runtime services or progression levels.

Decision: final main-house mesh origin is ranch-local **(-10,0,-5)**, facing +Z, with its 20x28m envelope centered **(-10,-11)**. Pasture **(-2,-34)** and pet care **(-13,-34)** leave the north wing and approaches clear. This northern placement satisfies both unchanged gate reservation and regional dry/flat envelope checks; the earlier westward trial is rejected because it reached the stream bank. Main 20x16m shell and separate north wing keep metre scale. Kitchen and office are internal services; their authored markers and outdoor paths move with the house. Quiet corner stays (-6.5,0,14), off the main home lane, with front approach at Z=15.

Decision: public house main origin is town-local (12,0,-2), yaw0, initial 12x10m shell with a 6m rear addition reserved inside a 12x16m envelope centered (12,-5). Supply yard center (23,4), 10x10m; market court center (-1,8), 14x12m. Local +Z faces the arriving gate/market. Existing guild/tavern/bathhouse positions must be coherently relocated where they conflict, not overlapped with the new house. Final paths/plots need clearance checks before model detailing.

Decision: preserve all currently available services in smaller/shared premises through design stages A–D. A is small supply settlement, B a busier market, C added workshops/public rooms/shore promenade, D connected neighborhoods and a modest larger civic/harbor area. These are visible design descriptions, not newly invented original unlocks, fees or minimum-day requirements.

Decision: source audit identifies `GameRoot.CompletedCommunityDeliveries > 0` after successful `TryDeliverCommunityRequest` as earned participation, but it represents paid trade. No active remake port of original municipal tax-funded development was established. Prepare complete construction states and the explicit isolated test contract; do not invent a treasury, donation, double debit or contribution-point conversion. Original land is inherited, not an invented municipal lease. Town courier access also needs guarded routing: `planning_board` targets `town`, which is absent from `IsKnownService`.

Decision: anonymous ambience is bounded presentation with suitable real destinations and off-camera spawning; named/service NPC identity and stocks must remain stable. Real ranch residents require distinct sleep assignments, whereas implied town population does not require an individual census. No new series of character designs.

## Context and Orientation

Repository root E:/OpenMakaiRanch contains Tools and OpenMakaiRanchGame. AGENTS.md is authoritative for commands. All canonical documents stay in OpenMakaiRanchGame/docs. User's original reference directory and personal user:// saves are protected. Existing editor PID77332 owns this project; do not kill it. Import through Tools/Godot/bridge.mjs editor.filesystem_scan and verify resource loading afterwards, not merely scanning:true.

GameRoot owns live state/services and replaces them on NewGame/LoadSlot. WorldGameController owns area/UI/input transitions. CoastalRegionController is an opt-in derived host with actual datums, grids, seam handoff and grounding; runtime remains unverified until exercised. RosterRig follows real NavigationAgent paths and shared roster state; do not alter walk speed/time/stamina to compensate for geometry. Author static hierarchy in TSCN, models in Blender, and add only focused integration adapters.

Existing textured source family is assets/3d/ranch_home_textured/{ranch_house,ranch_furniture}; original models remain assets/3d/ranch_house and ranch_furniture. Material sources are Tools/Materials/ranch_home and assets/materials/ranch_home. Existing dev scene is scenes/dev/RanchHomePrototype.tscn. Coastal modules/heightfields are generated by Tools/Blender/build_coastal_blockout.py and Tools/WorldDesign/coastal_geometry.py from shared layout.

## Plan of Work

First finish strict bridge geometry and the original/remake resource/lore audit while integrating the existing ranch home in a separately owned code/scene slice. Parent owns cross-area activation, public-building modeling/composition, town layout and final route tests. Do not run competing broad writers or replace files modified by another worker.

Once the connection and town envelopes are validated, model only the public house, its rear addition and supply/market improvement. Reuse furniture and restrained material family. Create separate scaffold/material-stack/barrier/canopy/light modules; do not duplicate the town for every stage. Main entrance, main lane and green buffer remain open during work. Completion must remove construction collision and activate new physical use at a safe unoccupied transition.

Bind public interactions to existing stable service IDs: general_store/shop, adventure_guild/adventure, research_office/research with existing workshop gate, tavern/roster, bathhouse/bond, town_hall/milestones, planning_board/town. Actual specific finance actions and the clerk's role await factual audit; these routing names alone do not prove an accounting command exists. Never silently turn a planning board into an automatic purchase.

Add actual localized nearby actions and positional audio/environment animation only for this section. Keep management overview accessible and avoid daily furniture clicking. Architecture for final town districts stays lightweight reservation data and reusable house forms until the first complete slice works.

## Concrete Steps

From repository root, use dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj --configuration Debug, Python unittest discovery under Tools/WorldDesign and Tools/VisualTargets, and the existing isolated Tools/Godot/launch.py --mode smoke. Invoke Blender --background --threads 2 --python-exit-code 1 --python on the inspected asset script; check traceback absence and the completion marker, and preserve sources/receipts. No new build pipeline or dependencies.

Use the existing VisualTargetCapture/capture.py and rendered acceptance machinery for targeted new scene/route checks. Stop GPU particle simulation only in static fixtures; day/rain behavior needs separate live checks. Existing shell frame tests can guide real held-input traversal, not serve as evidence for untouched doors.

## Validation and Acceptance

A player starts at the real home entrance, walks into the office/kitchen, uses an existing action, walks the valley connection and bridge to the public desk/market, and returns with an eligible existing companion. Record physics time, actual displacement, active area, companion gap and collisions; assert no discontinuous position catch-up or quick-travel substitution. Measure doorway free volume after frames/furniture and actual imported actors including parent scale. Closed/locked functions remain locked.

For the first expansion, record base, active works and finished use with the same cameras. Required evidence includes actual trigger/receipt, visible and audible works, accessible finished space/action, unchanged paid resources on repeated menus/loads/event processing, and save/load before/during/after. Compare efficient and slow real policies only against authoritative transactions; synthetic preconditions must be labeled separately. Do not gate fast play on arbitrary elapsed days.

Capture low/high Forward+, day/evening/night and existing rain with hardware/renderer/resolution/source metadata. Verify consistent geometry/collision/navigation/audio/light and no trapped occupants when changing stage. Keep ranch/day/night/tutorial/free travel and existing UI authority working. A rendered showcase alone is not completion.

## Idempotence and Recovery

Preserve prior assets, rejected exports and evidence in scoped archives; never delete personal files or reset user work. A source-hash change during generation/capture invalidates that evidence and requires re-generation/re-capture after inputs settle. Geometry acceptance never comes from return code alone. Mark partially integrated pieces honestly; ordinary activation is a separate gate after physical verification. No stage/commit/push/merge without permission.

Current Git configuration warns of LF-to-CRLF normalization for the layout. Source hashes bind raw bytes: a later checkout that changes line endings requires a checked re-export even if JSON meaning is unchanged. No Git normalization settings were changed during this task.

## Artifacts and Notes

Current baseline is committed user work at 1a65868, not a parent-created checkpoint. Old .dream-loop captures and canonical docs/art/ranch_house/{godot-preview,material-pass} remain. New factual handoff is assigned to docs/art/REGIONAL_DEVELOPMENT_FACTS.md and bridge evidence to docs/art/coastal-region-plan/BRIDGE_FIX.md; their mere presence will not certify their contents.

Parent house integration evidence: `docs/art/ranch_house/production-integration/capture-1zlpmmfw/` preserves build/engine/profile-isolation logs, full source snapshot and W01 PNG/context. `rejected-gate-overlap/` retains the failing log/source snapshot. Actual renderer: Godot 4.7.2 Mono, Vulkan Forward+, NVIDIA GeForce RTX 5090, 1600x900, clear morning, captured configured preset/render scale in W01-context.json. Audio uses Dummy; no audio acceptance or FPS benchmark. Image inspected: the long authored home is present alongside existing utility buildings; surroundings remain plain greybox lawn/boundary and need landscape/path dressing. This overview cannot prove interior light quality or doors.

Parent checks: `dotnet build` succeeds (existing CS8602 at RanchLeisureFrameTests.cs:218); VisualTargets ran 44 tests, 43 pass and one skip; `git diff --check` passes. Graphical capture passed at the earlier westward placement, not the final northern placement. Both isolated smokes `proc_502d8be55d44` and final-placement/deck-query `proc_dba73bf9ad68` completed with exit 0 and `SMOKE PASS`. Final run `.artifacts/godot/smoke-ib45o_8l/console.log` contains fourteen verified coastal-surface checks, including the real Godot import at unit scale with seven collision shapes. Navigation rounding warnings remain; no physical route or baked-navigation acceptance is inferred.

Regional resynchronization evidence: `docs/art/coastal-region-plan/parent-resync/` contains exact layout/receipt, nineteen-test log, successful and rejected Blender logs, candidate-probe source snapshot and full final smoke log. Current layout/heightfields/export SHA-256 is `3cb6fa1be0408f148666281f54938e98b8cbe1a11600981b83a25567f3c494bf`; all eleven GLB hashes match. Valley bridge remains 84 triangles/seven collision meshes. Original archive checksum verified; additional pre-sync archive preserved under `.dream-loop/coastal-sync-yagl98ae/`. Earlier failed Blender execution returned OS exit 0 despite a traceback; it is preserved as failure, not success.

Runtime module allowlist includes `ranch_valley_bridge.glb`. Separate walking surfaces cover valley/meadow bridges, pier and landing for queries without physics; raw terrain still represents the carved stream. Real engine smoke checks passed for surface elevations, endpoints, NaN rejection, stale-source rejection and imported collision shapes. Physical player/companion walking, deck-aware baked navigation, seam crossing and final-position Forward+ captures remain open. No normal-play coastal activation or financial change was made.

## Interfaces and Dependencies

Keep GameRoot.State/StateChanged and current service properties authoritative; reacquire after load. Preserve WorldStation.TargetId, CommandKind, CommandTargetId, RequiredFacilityId and contextual dispatch guards. Keep TownServicePoint IDs/ScreenIds and availability intact. Existing CoastalRegionController exposes Terrain, RegionReady/RegionError, NavigationReady, GetAreaRoot, GetSeamWorld, GroundWorldPosition and TryWalkingTransition; verify their actual behavior, not only compiled signatures. Furniture use and persistent unique ranch bed assignment remain explicit missing interfaces until bounded implementation is exercised.

## Outcomes & Retrospective

Market asset slice: all five worker exports and saved source independently rechecked without regeneration. Shared authored court and three variants loaded in actual Forward+ run `capture-xmzp8x50`; source/assembly provenance and six matching-camera views are preserved under `docs/art/okachi_market/godot-review/`. A real 2x2.8x11.5m collision volume verifies the permanent side bypass, and a front ray hits the work barrier only during Work. Roof visuals are absent in Work and present in Finished; solid framing remains. This proves local geometry/variant assembly, not walking, rain shelter, service behavior, finances or a live upgrade. Build and fifty Python tests (49 pass/one skip) pass; no new complete gameplay smoke run for this opt-in asset review. Actual civic/Base-market town integration is now delegated separately.

Public asset recovery: worker `deleg_784642d7` timed out after its saved-source checks, not before asset creation. Parent hash-archived all 102 worker files, independently reopened/decoded sources, and retained the original generation. `OkachiSupplyHouse{Core,Base,Expanded}.tscn` now load real models and separately reuse the existing furniture family. Final isolated capture `capture-3qliv4xo` verifies 64/81 architectural collision shapes and blocked/open rear cap by state. Six matching-camera images are archived under `docs/art/okachi_supply_house/godot-review/`; this run also captures the current northern ranch placement in `W01-current.png`. No town placement, construction phase, public interaction, furniture-use or walk-route acceptance is claimed. Plaster contrast, exterior lighting and sparse dressing remain art issues. Capture suite now runs 47 tests (46 pass, one skip), normal and optimized; build/capture pass with existing warnings. No new full gameplay smoke was run for these opt-in additions.

The authored home is now integrated and graphically boot-verified after parent corrections. No finished region, public expansion, earned transaction, bed allocation or complete walking acceptance is claimed. Resource/lore and bridge reports are separate handoffs. Public asset modeling proceeds after the route/parcel prerequisite. This plan is the current continuity anchor rather than the older paused coastal handoff.

Revision note: created for the new growing-region request; records existing approved design direction, uploaded astar baseline, shared end-state reservations, narrow delivery boundary and truthful acceptance gates.
