# Building levels and reserved space

2026-09-11. Existing `RanchService.UpgradeFacility` already increments levels and prices. It did not define matching physical expansion stages. This continuation preserves the economic level progression rather than inventing an original-game level cap.

`RanchBuildingPlots` defines eight fixed metre-scale plots, a roof/clearance envelope and an entrance reservation for each. The town-gate approach is protected. Validation rejects overlapping reserved envelopes, a building covering another entrance, out-of-bounds/invalid dimensions and duplicate IDs. Conservative axis-aligned bounds include rotated roofs; harmless near overlaps are intentionally rejected rather than gambling with player clearance.

`RanchPresentationBuilder` now uses those shared plots, instead of calculating every building from a nearby station independently. Original station IDs remain. The work point stays inside its doorway. Existing approaches are reconnected to the moved work points. This is authored layout, not free placement.

`WalkInBuilding.SetFacilityLevel` has bounded visual grades0-3. Grade2/3 adds wall-mounted equipment details inside the existing footprint. Higher simulation levels retain grade3. No shell scaling, new floor area, collision rebuilding around a player or blocked aisle is introduced on upgrade. Doors remain1.8m wide and2.55m high by default, walls3.2m; these are game-design dimensions for the existing characters/camera, not architectural-compliance claims. Upper floors, annexes and bespoke tier models are NOT implemented.

`RanchDressingClearance` checks imported tree MeshInstance3D bounds by composing local transforms before tree entry. It also checks conservative placeholder crown bounds. Trees crossing reserved roofs, entrance corridors, the town-gate approach or world bounds are not added; there is no out-of-tree GlobalTransform read. This covers the static tree geometry, not shader/skeleton expansion or every other prop.

Upgrade cost/level arithmetic rejects unrepresentable transactions before spending gold. This is a narrow arithmetic guard, not an exhaustive overflow audit of upkeep, automation or every production formula.

## Future authored expansions

Reserve the largest desired plot before admitting a wider tier. Model side wings as explicit stage volumes inside that reserved plot, not arbitrary node scaling. Keep an entrance-to-work-point route and a public path route clear, test NPC navigation after rebaking, and preview occupied/pending areas before committing a build. Placement validation must precede resource spending. Never silently move a resident/player out of collision created by an upgrade; defer installation or use a reviewed construction transition.

Current validation is a static layout/footprint contract, NOT proof of complete traversability. Furniture/other world props, camera collision, navmesh connectivity, free placement and every character size still need acceptance. RANCH_DESIGN_VALIDATION.md records successful exact-head tests, including selected physical doorway walking and additional grade/tree invariants, without declaring all routes complete.
