# Art Asset Manifest

Baseline 2026-09-05. **No new 3D assets or generated concepts exist. No asset is visually approved.** Existing portraits are retained; this manifest does not certify their rights, age presentation or artistic suitability.

| Asset ID | Type | Source reference | Concept | Model / rig / texture | Godot import | In-game validation | LOD | Known issues |
|---|---|---|---|---|---|---|---|---|
| ranch_slice_01 | environment, planned | Current facility/job data; original layout audit pending | REFERENCE_NEEDED | Not started | None | None | Unspecified until measured | Floor plan and selected direction needed |
| ranch_interior_01 | interior, planned | Building/function selection pending | REFERENCE_NEEDED | Not started | None | None | Unspecified | Must share ranch visual/material language |
| hero_master_candidate | character, unselected | audit/character-metadata.json; Noir is candidate only | REFERENCE_NEEDED | Not started | None | None | Unspecified | No confirmed adult identity/design clearance; non-explicit reference audit pending |
| workstation_01 | interactive prop, planned | RanchService + ScheduleService contract | REFERENCE_NEEDED | Not started | None | None | Unspecified | Assignment-only interaction initially; no duplicate production |
| existing_portrait_set | retained 2D assets | assets/portraits/ and portrait_layers/ | Existing, not newly approved | Not a 3D pipeline | Existing texture imports | Full visual audit not performed | N/A | Four duplicate UID sidecars regenerated; pixel files unchanged |

## Status vocabulary

Use REFERENCE_NEEDED, REFERENCE_APPROVED, BLOCKOUT, MODELED, RETOPO_DONE, RIGGED, MORPHS_DONE, TEXTURED, GODOT_IMPORTED, VISUAL_TESTED and GAMEPLAY_APPROVED only with matching evidence. A generated image is a reference, not a rigged game asset. A loaded GLB is not gameplay approval.

Each produced asset must add exact source/reference/.blend/GLB/material/scene paths, author/tool provenance, review renders, measured triangles/materials/textures/bones/morphs, supported morph/animation ranges and current defects. Do not populate a runtime visual manifest with nonexistent paths merely to satisfy documentation.
