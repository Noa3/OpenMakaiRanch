# Okachi market assets — local-space handoff

Editable source: `assets/3d/okachi_market/okachi_market.blend`. Five separate GLBs alongside it. Generator: `Tools/Blender/build_okachi_market.py`.

## Verified geometry

Godot XYZ metres, +Z front; source Blender mapping `(x,-z,y)`. All exported mesh-node transforms identity. Saved source reopened successfully; evaluated vertex bounds match decoded GLB binary POSITION bounds within 1e-5m. Source collections intentionally share local origins: solo a collection to edit. No court geometry or stage duplication.

| GLB | Bounds size X × Y × Z (m) | Meshes | `-col` meshes | Triangles |
|---|---|---:|---:|---:|
| `canopy.glb` | 6.000 × 4.510 × 4.000 | 54 | 37 | 648 |
| `counter.glb` | 1.800 × 1.000 × 0.796 | 14 | 14 | 168 |
| `scaffold_bay.glb` | 2.560 × 2.890 × 1.320 | 29 | 29 | 348 |
| `material_stack.glb` | 2.050 × 0.755 × 0.860 | 20 | 18 | 240 |
| `barrier.glb` | 2.000 × 1.120 × 0.780 | 9 | 6 | 108 |

Canopy exact roof footprint 6 × 4m, four edge posts, pads 0.42m square. Minimum measured perimeter knee/beam underside 2.860042m. Actual mesh AABB intersection probe X[-1,1], Y[0.001,2.8], Z[-2,2] returns zero blocking meshes: 2m central aisle, no floor lip. This is geometry clearance, not physics traversal. Counter top 1.0m, approximately 1.8 × 0.8m, open staff side and front approach hint; no goods. Barrier height 1.12m with broad feet/rails. Scaffold uses 0.14–0.16m framing, deck, braces, guardrails; static construction prop, not climbable behavior.

Roof meshes use `Roof_`; solid frame/furniture meshes use exact `-col` suffixes. Roof has no collision. Sockets are neutral approach/loading/accessory hints, not actor poses, IK, interactions or reservations. Canonical 14 × 12m market court remains unbaked and unchanged.

## Evidence and attribution

`geometry_receipts.json`: exact bounds, node names, counts and hashes. `corrective_generation.log`: Blender exit 0, five EXPORT_OK markers, MARKET_VERIFIED_FINISHED, no traceback. `contact_sheet.png`: 1400 × 1000 CPU Cycles, 20 samples, visually inspected. All five forms readable, roof/perimeter supports and open lane visible. Scaffold joints show dark intersection patches: cosmetic cleanup remains; not a false final-art polish claim. Catalogue ground and exploded arrangement are render-only, absent from saved source and GLBs.

No external images or linked libraries: verified after reopening source. No textures used. Flat material colors adapted from read-only civic/furniture generators; existing fence source inspected for continuity. No other generator executed/imported.

Initial run exported assets but failed after reopening source because Python material references became stale. One corrective generation refreshed datablock references and made repeated collider names unique before `-col`. Initial files and failure evidence preserved in a unique `assets/3d/okachi_market/history/` generation archive; no further exports made.

## SHA-256

- Source `.blend`: `fb3df879f337bb0defae9ce408531f5e659b78f5d1c3faff77cc9f2962e5ad78`
- Generator: `e5a591deacc06fb72b1d69c8525fc0291ec801787a7925284adaa54f3a4f049a`
- `canopy.glb`: `f69e1d8e26637dedba3b65e6f7c31cae0ed49afaa221a5970d3718cbd4080ab3`
- `counter.glb`: `89c462d4c53cdc552b0a6025e30c07973217c7c7d73f7c46577e7793aa67a972`
- `scaffold_bay.glb`: `b681604f98bed4d6423d118e7fd97b5672100b88b468f0ab47b75fb3ab8352eb`
- `material_stack.glb`: `baaab4475ada7b71bc0c6c8799d2aa0a6adc79b560dbdff78b84d1c6263ccc6f`
- `barrier.glb`: `42e2425cb5ec2fcfb2c77d60108930b4b2a2cd3780d1e92d8e712ecb385251ab`
- Contact sheet: `0fa105ef14db5875159f1f466ff37be7dbf3e22681c9b0655451a9ac6763eecb`

## Explicit limits

No Godot import, importer collision validation, navigation, actor traversal, actual use, stage persistence, economics/finance or town placement tested. No shared layout, scenes, C#, civic/material sources or Git index changed. Interactive Blender/Godot untouched. Parent owns assembly and runtime acceptance.
