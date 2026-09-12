# Okachi supply house — recovered models and engine review

## Actual status

Modeled, saved-source verified, imported, composed into reusable Godot scenes and inspected in real Forward+. **Not placed in the production town; no public-service binding or gameplay construction project yet.** The worker timed out after its final saved-asset verification, not before exporting. Parent preserved its files and independently repeated verification without regenerating or modifying the Blender asset.

## Deliverables and dimensions

Paths below are relative to `OpenMakaiRanchGame/` unless stated otherwise.

| Export in `assets/3d/okachi_supply_house/` | Meshes | Triangles | Collision shapes verified in Godot |
|---|---:|---:|---:|
| `base_shell.glb` | 144 | 1706 | 63 |
| `rear_annex.glb` | 48 | 565 | 18 |
| `rear_connection_cap.glb` | 2 | 24 | 1 |

- Main floor: **12×10 m**, local X[-6,6], Z[-5,5]. Rear frame extends to Z=-5.04; use measured bounds, not just floor size.
- Annex: **12×6 m**, Z[-11,-5], already authored at its connection coordinates. Do not apply another -6 m placement offset.
- Combined floor envelope: **12×16 m**. Main roof reaches Y5.255; annex roof Y4.705. Floor thickness occupies Y[-0.2,0]. All dimensions are metres, not scaled scene instances.
- Four main rooms: reception, meeting, archive and supply office; **2 m clear central corridor**. Wall/design clear-height reference: 3.15 m.
- Eight architectural openings: clear widths **1.4 / 1.6 / 1.8 m**, clear height **2.78 m** after frames. See `geometry_receipts.json` for each aperture and collision-volume probe. These are geometric measurements, not completed actor traversal.
- Editable source: `assets/3d/okachi_supply_house/okachi_supply_house.blend`; generator: repository-root `Tools/Blender/build_okachi_supply_house.py`.
- Source uses existing project material textures, packed into Blender; furniture references the existing `ranch_furniture.blend` library. Furniture is **not** embedded into architecture GLBs. No new external asset download or character design.

## Godot composition

`scenes/world/OkachiSupplyHouseCore.tscn` shares the actual shell, separately instantiated existing textured furniture, and room lights. `OkachiSupplyHouseBase.tscn` adds the closing cap. `OkachiSupplyHouseExpanded.tscn` adds the annex, extra shelves and lights, with **no cap node or cap collider**. These scenes are reusable asset variants, not an implemented persisted build state.

Base has 64 architectural collision shapes; expanded has 81. Furniture is dressing only: its collision, occupancy, service actions and seating remain open. No roof collision or automatic weather shelter is supplied by this asset composition. No mayor/landlord identity or new treasury was introduced.

## Independent verification and evidence

- Reopened saved `.blend`, checked **82 collision-object vertex bounds**, decoded every GLB's transformed vertices, matched export hashes and names, and verified linked furniture source exists. `saved_asset_verification.json` and `godot-review/blender-parent-verify.log` hold actual output. Blender source remained unchanged.
- Opt-in engine check: `OMR_OKACHI_ASSET_REVIEW=1 python Tools/VisualTargets/capture.py` from repository root. Existing isolated capture/build/profile pipeline reused; no personal saves touched.
- Final successful run: `.dream-loop/capture-3qliv4xo`. Six real images, base/expanded exterior, cutaway and reception, use matching cameras/FOV and lighting. Full source snapshot, assembly hash, logs, context and images preserved under `godot-review/`.
- Actual renderer: Godot 4.7.2 Mono, Vulkan Forward+, RTX 5090, **1600×900**, one clear-morning configuration. Audio driver Dummy. No FPS, audio or low/high/night/rain acceptance.
- Real physics center rays: front doorway clear in both variants; four room-door center rays clear; rear connection blocked in base and clear in expanded. These detect a retained invisible cap, **not** full capsule clearance, route navigation or walking. The standing existing `noir` profile measures 1.62 m in both variants; it is a scale mannequin, not an integrated clerk or approved production rig.
- Build passed, existing CS8602 warning remains. Capture Python suite: **47 tests, 46 passed and one skipped**, normal and `python -O`; new wrapper tests reject an incomplete image set or a retained cap. Mocked wrapper fixtures are separate from the real engine evidence. Full gameplay smoke was not rerun for these opt-in capture/asset-only additions; prior regional smoke remains evidence for its earlier scope.
- First successful capture `.dream-loop/capture-_qkeq_tt` is preserved: its reception view was obscured by the mannequin. Final pass moves the scale reference aside and tightens overview framing, without changing models.

## Recovery archive

`.dream-loop/okachi-recovery-49w3056r/worker-deliverables.zip` preserves 102 worker files; every archived member was hash-compared. SHA-256: `381b26a6aab7d5dfa3f6d96f52dd7ed617b3fef08ec97f4cf26515cb6655b7fb`.

Saved Blender SHA-256: `0f3f4632242c5f83c8320ab7dd1a7ef5441e6bbbe63dda0c4b959f2bc0131c78`.

Archive is a recovery subset, not a standalone game or self-contained furniture library. Existing `iteration_1` and all original project assets remain. No stage, commit, pull, merge or push performed.

## Remaining work

Reorganize conflicting town plots before production placement. Bind real local services with existing guards; add furniture collision/use and actual route/companion tests. Model/assemble a real construction phase, then connect only a verified progression rule or the explicit isolated test contract. Market shelter/construction kit is a separate in-progress asset task.

Art review: room structure and rear growth are readable; frontage identity remains stable. Plaster is still too mottled, outside is subdued, windows are opaque blue-grey, rooms/annex are sparsely dressed, and lantern fixtures/weather/audio are unfinished. Do not label these technical import checks final art approval or a completed inhabited-town slice.
