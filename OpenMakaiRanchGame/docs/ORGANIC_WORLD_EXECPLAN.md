# Organic ranch and town revision

> Superseded spatial scope: the user requested a larger connected coastal region. Preserve this interim work and its artifacts. Small-layout exports are paused. Continue with `execplan/COASTAL_REGION.md`; existing compact plot clusters are reusable cores, not final world bounds.

## Intent and approval boundary

The user's latest direction explicitly replaces the rigid greybox spatial arrangement: preserve the soft illustrated world style, but make ranch and town look naturally grown. The earlier exact-layout constraint no longer applies. Work remains on the user-assigned `astar` branch; existing uncommitted visual-target evidence is preserved. No commit/push is authorized.

This is a playable spatial/presentation revision, not final design approval. Character examples inform clean anime face shapes, sculpted hair locks, contours and cel shading only. Revised independent, fully clothed adult character proposals remain reference images, not approved roster identities or integrated rigs.

## Existing implementation

`RanchPresentationBuilder` generates straight box paths and points buildings toward one hub. `RanchBuildingPlots` supplies physical building footprints, rotations and clearance rectangles. `WalkInBuilding` owns genuine doorway collision and cutaway interiors. `TownPresentationBuilder` builds a crossroad and radial service facades, several still box proxies. `SimpleNavigationRegionBuilder` bakes real static collision. `WorldBoundaryBuilder` owns finite-map collision independently of dressing. Services and settlement remain under `GameRoot`.

## Implementation sequence

- [x] Inspect current branch, world composition, building/door/navigation contracts and user image references.
- [ ] Author asymmetric ranch plots and town service placements with explicit spatial data shared by tooling and runtime.
- [ ] Build editable Blender environment sources and GLB exports: curved variable-width paths, terrain/landscape silhouettes, clustered broadleaf vegetation and contextual details. Instantiate static scenery through authored scenes, not a new runtime world composer.
- [ ] Replace obsolete straight paths/radial service positioning while preserving stable station IDs, facility visibility/levels, entrance clearance, portals, current save/load and one simulation.
- [ ] Produce one revised character proposal locally; inspect actual output, preserve provenance and list visual shortcomings.
- [ ] Build/import, run spatial regressions and isolated smoke; capture both real locations and inspect images. Maximum three visual implementation/review rounds in this revision; no invented similarity scores.
- [ ] Record executed tests, actual screenshots, remaining quality/gameplay limitations and changed source paths.

## Acceptance and validation

1. Ranch and town have visibly non-grid building groupings and continuous curved approaches to their actual interaction positions.
2. No building or tall dressing blocks another doorway or the travel gate; real collision/navigation still reaches service/station destinations.
3. Authored terrain collision matches visible walkable terrain; distant backdrop stays outside finite gameplay boundaries.
4. State transitions, first-day flow, assignments, travel and current-version save/load continue using existing commands and services. No second time/reward path.
5. Local character draft visibly uses anime contour/cel-shading language; honest separation from a playable rig.
6. Real Forward+ screenshots, build output and isolated tests are retained. Historical UI acceptance already has failures; do not attribute them to this patch or claim them fixed without evidence.

## Risks and deferred scope

Full production assets, final character rigging, all town interiors, detailed terrain ecology and performance across target hardware are not implied by this revision. Existing seasonal/density behavior must not silently disappear when replacing decorative elements. Preserve user saves and prior evidence. If spatial expansion is unnecessary, retain finite map bounds to limit movement/navigation risk.

## Verification record

Pending implementation. Character generation and read-only dependency audit delegated independently with bounded attempts; parent verifies resulting artifacts.
