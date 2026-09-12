# Actual town core integration and local traversal

## Status

Recovered and completed the timed-out town integration. The normal `TownGreybox.tscn`, reached by `WorldGame.tscn`, now contains `OkachiCivicHouse` and `OkachiMarketBase`. This is not just an asset-review scene. Only the **BASE** market is enabled in normal play; no development state machine, new finance, or new save schema is introduced.

The civic shell is at town-local `(12,0,-2)`, with a reserved rear-annex envelope. Reception uses the existing `town_hall → milestones` identity; the planning-office approach preserves `planning_board → town`. The court is at `(-9,0,8)`, with the existing `general_store → shop` interaction at `(-11,0.7,9)`. The initial general store is a counter, **not yet an authored enclosed shop/storage building**. Other functions keep their identities and smaller physical buildings. The worker's town-only spatial edits passed the full corridor/dry-parcel checks after a coordinated regional re-export.

## Reproduced defects and fixes

1. The follower's next navigation point shared its X/Z but used the raised raster-surface Y. Keeping the avatar Y fixed prevented consuming that point. `RosterRig` now uses `NavigationAgent3D.PathHeightOffset` to align the surface with its origin. Capsule-follow targets use the player's feet rather than capsule centre; the shape reference is cached at binding.
2. The 0.25 m navigation raster erased real 1.4 m doors after erosion. The mesh and map now agree on 0.1 m horizontal / 0.05 m vertical cells. Configured bake radius 0.4 m, height 2.25 m, climb 0.25 m, avatar radius and travel speeds are unchanged.
3. Advancing waypoints 0.35 m early cut door-frame corners. The path-waypoint tolerance is now 0.05 m; actual collision probes remain strict.
4. An old non-coastal guard rejected valid navigation segments longer than 8 m, freezing the follower in the larger plaza. This guard was removed; displacement still uses the existing bounded `MoveToward` step, with no straight-line fallback around navigation.

Rejected runtime reports are retained under `rejected/`. Original integration/export inputs are preserved in `.dream-loop/town-integration-recovery-3cwxg6ny/pre-resync.zip`.

## Executed evidence

Canonical run: `.dream-loop/capture-un6uxw5a`; preserved source snapshot, build/engine/profile logs and `evidence/` here. `summary.json` aggregates the actual trace:

- 1,498 continuously recorded physics frames; 25.468 s wall-clock including setup, waits and UI rendering.
- Approximately 100.77 m of player movement and 74.60 m of follower movement. Maximum sampled per-frame displacement: player 0.10834 m, follower 0.05001 m.
- Player uses the existing `MobileMovementInput` path and normal `_PhysicsProcess` / `MoveAndSlide`, not transform repositioning along the route.
- Both entered the actual reception and planning-office rooms, reached the market, and crossed its western exit. The existing follower navigates independently rather than exactly retracing player waypoints.
- A capsule matching the follower's 0.35 m radius and recorded character height checked actual solids every fourth physics frame. No overlap was reported; this is sampled collision evidence, not continuous swept-volume certification.
- Entering the house did not open a menu. Interacting at reception and market opened the visible existing milestones and shop screens. Both closed normally; no purchase occurred. Gold, community-delivery count and configured movement speed remained unchanged.
- Five real Forward+ PNGs at 1600×900 include both actual UI screens. Recorded environment: RTX 5090, Medium, clear morning. Native benchmark/performance and low/high/night/rain matrices are not claimed.

Reproduce through the existing isolated wrapper:

```bash
OMR_TOWN_CORE_REVIEW=1 python Tools/VisualTargets/capture.py
```

The wrapper rejects failed/missing room-arrival or usage assertions, absent follower market/exit arrival, discontinuous trace frames/large jumps, and missing/wrong-size screenshots. Its Python tests use explicitly synthetic validator data, not simulated engine proof.

`Tools/WorldDesign`: 29 tests passed. `Tools/VisualTargets`: 52 passed, one skip, both normal and optimized Python. Build/capture passed; existing CS8602 remains. The previous raster-mismatch/height/climb warnings are resolved; the pre-existing fractional `region_min_size` warning remains. Full post-fix smoke `proc_e5b40d435aa4` initially found exactly one obsolete assertion equating seven service IDs with seven physical parcels. The corrected test checks six parcels and all seven service IDs from an instantiated town scene. Rerun `proc_a5a41e4133a8` completed with exit 0 and `SMOKE PASS`: 2,118 `SMOKE OK` lines and zero `SMOKE FAIL` lines, including the actual seven-service instance assertion. Source log: `.artifacts/godot/smoke-g_ai98hm/console.log`; preserved here as `smoke-console.log`. Independent read-only review `deleg_c70b79da` is now complete with no new evidenced must-fix regressions in the focused scope; see `REVIEW.md`. The parent independently matched the report hash and all eight reviewed source fingerprints. The reviewer did not run engines, certify the separate smoke or perform visual acceptance. The capture precedes only the assertion/error-message correction, not a later behavioral fix.

## Deliberate limits

- `TravelTo("town")` is fixture setup, with an existing character selected as escort. Neither is claimed as an earned invitation or a walked ranch/valley connection. No route transforms are assigned between local checkpoints.
- No permanent clerk identity, new billing, contribution payment, construction state/persistence, or earned market upgrade exists yet. The `planning_board → town` dedicated-menu routing blocker remains; its room was reached, but that action is not claimed functional.
- Sitting/sleeping, a full enclosed general store, current save/load across construction states, continuous coastal traversal, evening/night/rain and low/high quality remain open.
- Much of the surrounding town is still greybox. Plaster, sparse dressing, grass-only surroundings and distant camera visibility need visual work. A screenshot alone is not the traversal evidence; use the trace and collision/interaction results.
- Current regional exports are raw-byte-bound to the updated shared layout. Further layout or line-ending changes require coordinated re-export; Git settings were not changed.
