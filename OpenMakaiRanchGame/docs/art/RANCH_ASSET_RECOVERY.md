# Ranch house and furniture — recovered asset batch

Status: **editable assets and furnished Godot authoring prototype verified; production integration incomplete**. Branch `astar`. Both modeling delegates timed out without summaries. Their files survived. No user/editor process was killed; no reset, stage, commit, push or merge was performed during recovery.

**Material follow-up:** the baseline evidence below intentionally remains unchanged. The prototype now uses separate textured derivatives from `assets/3d/ranch_home_textured/`, with metre-scale UVs and the verified Material Maker plaster/oak/linen family. Original models remain intact. Final Forward+ capture `capture-st5t70sy`, comparison images, source provenance and read-only 18-model/12-map validation are archived under `ranch_house/material-pass/`. All six camera contracts match the baseline; interior lighting remains dim and plaster still looks coarse/mottled. This closes prototype material wiring, not production visual approval or the habitation/functionality gaps below. Reproduction: repository-root `Tools/Materials/ranch_home/README.md`.

## Deliverables

- `assets/3d/ranch_house/ranch_house_editable.blend`: editable main shell and attached wing. Main footprint 20 × 16 m; wing 12 × 12 m; source door contract 1.25 × 2.70 m after frames; clear ceiling height 2.95 m. `main_house.glb`, `residential_wing.glb`, `north_connector_cap.glb` are separate exports. Source geometry receipt: `ranch_house/geometry_receipt.json` beside this document's parent directory.
- `assets/3d/ranch_furniture/ranch_furniture.blend`: editable furniture gallery with separate mesh parts and bevel modifiers. **15 individual GLBs:** table, chair, bench, desk, wardrobe, pantry, kitchen counter, feed trough, straight fence, corner fence, gate, standard bed, large bed, large chair, terminal post. Receipts and reopened-source/binary verification live beside the assets.
- `scenes/dev/RanchHomePrototype.tscn`: authored composition with office, kitchen, common dining area, example player/resident rooms and four wing rooms. Seven example beds are **not** allocations, a recruit limit or proof of sufficient capacity. See `RANCH_HABITATION_FACTS.md`. This scene is **not referenced by the production ranch**; the capture loads it only under an explicit review flag.
- `src/Dev/VisualTargetCapture.RanchAssets.cs`: opt-in import/scale/collision-node inspection and rendered review through the existing isolated capture runner. No service, economy, upgrade or save-system changes.

## Executed validation

The furniture generator had a final patch after its earlier export. Recovery first archived both workers' outputs, then reran the final generator rather than treating stale exports as current.

```bash
"D:/SteamLibrary/steamapps/common/Blender/blender.exe" --background --threads 2 --python Tools/Blender/build_ranch_furniture.py
"D:/SteamLibrary/steamapps/common/Blender/blender.exe" --background --threads 2 --python OpenMakaiRanchGame/assets/3d/ranch_furniture/verify_kit.py
# Import through the existing editor's verified editor.filesystem_scan bridge;
# do not launch a conflicting editor or kill the user's session.
OMR_RANCH_ASSET_REVIEW=1 python Tools/VisualTargets/capture.py
python -m unittest discover -s Tools/VisualTargets -p 'test_*.py'
python -O -m unittest discover -s Tools/VisualTargets -p 'test_*.py'
git diff --check
```

- Blender **5.2.1 LTS**: `INDEPENDENT_KIT_PASS 15 TRIANGLES 65885 DEGENERATES 0`; saved source reopened, GLB bytes decoded, source dimensions/normals/UVs/complete socket names checked. Exit 0.
- Parent independently matched all three house GLB hashes, mesh/triangle totals and the saved `.blend` hash to the house receipt. Main: 309 meshes / 19,052 triangles; wing: 245 / 15,980; cap: 1 / 108. The source receipt records 16 unobstructed door point-probes; this is not a walking test.
- Actual **Godot 4.7.2 Mono / Forward+ / RTX 5090** run: `RANCH ASSET IMPORT/CAPTURE PASS: 18 sources`, `VISUAL CAPTURE PASS`. **36 instances**, all root world scales `(1,1,1)`. Imported house collision shapes: main 106, wing 82, cap 1. **Furniture collision shapes: 0.** Existing neutral stand-in body height measured **1.62 m**, matching its profile target.
- Build passed. Full build emitted existing `CS8602` at `src/Tests/RanchLeisureFrameTests.cs:218`; subsequent incremental capture build emitted none. Existing navigation-voxel rounding warnings remain in the capture log; no runtime ERROR/SCRIPT ERROR in this capture.
- VisualTargets: 44 tests each normal and optimized, **43 passed / 1 skipped**. `git diff --check` passed. Earlier full smoke passes predate this prototype; no new full-smoke claim for this asset-recovery slice.

Six real 1600 × 900 engine captures, JSON report, source/assembly provenance and logs are archived under **`ranch_house/godot-preview/`**. Corresponding transient run: `.dream-loop/capture-2qs1hmtx`. Cutaway, kitchen and common-room images inspected directly. Imported geometry is visible, but interior lighting remains dim and the building remains sparsely dressed. This is not a finished visual target approval.

## Remaining work — do not mark completed

- Production ranch positioning, everyday routes, station integration and existing unlock/upgrade presentation. No new housing rules were invented.
- Furniture/yard collision authoring, real player and follower door traversal, approach/exit clearances and camera behavior. Imported collider **existence** is not collision-behavior acceptance.
- Resident bed allocation, reservations and capacity expansion policy. Invisible residents are not considered housed by these example placements.
- Articulated sit/lie transitions and body-range/IK calibration. Current socket `Pose` nodes are support targets, **not validated pelvis transforms**. Existing stand-in has no working articulated furniture animation.
- Bath furnishings, more interior dressing, roof junction refinement, material/lighting polish. This batch uses portable flat Principled materials; no claim that the separate Material Maker/Krita set is integrated.
- Production save/load, navigation and performance validation; known UI-acceptance failures remain a separate issue.

Preserved pre-recovery archive: `.dream-loop/asset-recovery/worker-deliverables-ed0f5fb1e940.zip` (35 files; ZIP integrity checked), SHA-256 `164400a4c4536c65edcfbfe101514fb0f6356df42c0a77dfaaf0380c37033df4`. Existing `.blend1` and worker evidence remain untouched.
