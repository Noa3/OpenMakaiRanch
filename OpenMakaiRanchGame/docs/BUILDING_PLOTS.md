# Building levels and reserved space

2026-09-11. Existing `RanchService.UpgradeFacility` already increments levels and prices. It did not define matching physical expansion stages. This continuation preserves the economic level progression rather than inventing an original-game level cap.

`RanchBuildingPlots` defines eight fixed metre-scale plots, a roof/clearance envelope and an entrance reservation for each. The town-gate approach is protected. Validation rejects overlapping reserved envelopes, a building covering another entrance, out-of-bounds/invalid dimensions and duplicate IDs. Conservative axis-aligned bounds include rotated roofs; harmless near overlaps are intentionally rejected rather than gambling with player clearance.

`RanchPresentationBuilder` now uses those shared plots, instead of calculating every building from a nearby station independently. Original station IDs remain. The work point stays inside its doorway. Existing approaches are reconnected to the moved work points. This is authored layout, not free placement.

`WalkInBuilding.SetFacilityLevel` has bounded visual grades 0–3. Grade 2/3 adds wall-mounted equipment details inside the existing footprint. Higher simulation levels retain grade 3. No shell scaling, new floor area, collision rebuilding around a player, or blocked aisle is introduced on upgrade. Doors remain 1.8 m wide and 2.55 m high by default, walls 3.2 m; these are game-design dimensions for the existing characters/camera, not claims of architectural compliance. Upper floors, annexes and bespoke tier models are NOT implemented.

Upgrade cost/level arithmetic now rejects unrepresentable transactions before spending gold. This is a narrow arithmetic guard, not an exhaustive overflow audit of upkeep, automation or every production formula.

## Future authored expansions

Reserve the largest desired plot before admitting a wider tier. Model side wings as explicit stage volumes inside that reserved plot, not arbitrary node scaling. Keep an entrance-to-work-point route and a public path route clear, test NPC navigation after rebaking, and preview occupied/pending areas before committing a build. Placement validation must precede resource spending. Never move a resident or player silently out of a collision created by an upgrade; defer installation or use a reviewed construction transition.

Current validation is a static layout/footprint contract, NOT proof of complete traversability. Furniture, world props, camera collision, navmesh connectivity, free building placement and every character size still need runtime acceptance. The existing physical doorway test is retained and expanded grade invariants are added.
