# Illustrated ranch home materials

Three shared, low-contrast materials, authored as editable Material Maker graphs and actually exported locally. No geometry or runtime changes.

## Assets
`OpenMakaiRanchGame/assets/materials/ranch_home/`

| Stem | One UV tile covers | Density at 512 px |
|---|---|---|
| warm_lime_plaster | 2 x 2 metres | 256 px/m |
| painted_warm_oak | 1 x 1 metre | 512 px/m |
| natural_linen | 0.25 x 0.25 metres | 2048 px/m |

Each stem has `_albedo.png`, `_normal.png`, `_roughness.png`, `_orm.png` and a Material Maker-exported `.tres`. Use UV = surface position in metres / tile metres. Oak grain follows V. Linen is deliberately exaggerated illustration-scale weave (32 threads per 0.25 m), not a physically microscopic textile. Scale metadata is an authoring contract, not measured scanned material.

Albedo: sRGB. Normal: linear tangent space, OpenGL +Y, outward +Z. Roughness: grayscale linear, extracted exactly from ORM green. ORM: linear R=unoccluded AO (255), G=roughness, B=nonmetal (0). Use repeat sampling and mipmaps. No baked lighting, cavity grime or geometry-specific AO. `.tres` uses relative map references; UV density must be applied by the mesh author. No runtime import/load claim is made.

## Editable sources and provenance
This directory holds one `.ptex` per material. Original graphs combine installed value-noise/weave/colorize nodes; the normal node embeds the installed analytic shader from `addons/material_maker/nodes/normal_map.mmg`, excluding its eager buffer. Schema checked against `material_maker/examples/wood.ptex`; no example textures copied.
Source: local `E:/material-maker-src`, git `ad19fcf`, upstream https://github.com/RodZill4/material-maker. MIT copyright Rodolphe Suescun and contributors: included `MATERIAL_MAKER_LICENSE.md`.
Installed Material Maker distribution: `E:/material_maker_1_7_windows` (1.7 directory; bundled engine reports Godot 4.7). Actual exports used source project with Godot `4.7.2.stable.mono.official.ed1daf0bf`, Vulkan RTX 5090.

`house_accent_palette.ora`: original three-layer OpenRaster palette constructed locally. **Krita 5.3.3 (858d352)** actually opened/converts it using supported `--export --export-filename` to `house_accent_palette.kra`; Krita then reopened that native file and exported `house_accent_palette.png`. KRA ZIP/maindoc validation found three paint layers. Teal RGB(76,113,109), terracotta RGB(172,105,78), cream RGB(220,206,174). These can be flat Blender colors; no extra texture duplicates required.

## Reproduce
From repo root:
```
python Tools/Materials/ranch_home/build_materials.py
"C:/Program Files/Krita (x64)/bin/krita.exe" --nosplash --export --export-filename "E:/OpenMakaiRanch/Tools/Materials/ranch_home/house_accent_palette.kra" "E:/OpenMakaiRanch/Tools/Materials/ranch_home/house_accent_palette.ora"
"C:/Program Files/Krita (x64)/bin/krita.exe" --nosplash --export --export-filename "E:/OpenMakaiRanch/Tools/Materials/ranch_home/house_accent_palette.png" "E:/OpenMakaiRanch/Tools/Materials/ranch_home/house_accent_palette.kra"
python Tools/Materials/ranch_home/verify_finish.py
```
Python requires Pillow and numpy (already available). MM's current CLI parses `--size` but passes fixed 2048 to export: finalizer resamples actual exported maps to 512, renormalizes normals and extracts roughness. The source remains procedural/full resolution editable.

## Verification and limits
`verification.json` / asset `material_manifest.json`: per-map dimensions, SHA256, channel ranges, edge deltas, metre scale. All 12 required maps passed. Actual contact sheet and Krita PNG inspected with vision: soft cream plaster, directional muted golden oak, fine natural weave, blue normals, nonmetal roughness maps; no obvious corruption. See `material_contact_sheet.png`.
Integer noise counts and even weave counts are periodic. Opposite-edge statistics are sampled-pixel differences, not a claim that endpoint texels must be identical: linen normal is approximately 12.6/255 due to weave boundary gradient. A final in-engine oblique-angle/mipmap look review remains the integrator's responsibility.

Initial buffered normal export produced invalid maps despite return code 0. Fixed by inlining installed analytic normal shader. MM's default normal convention must be used before Godot target conversion; choosing OpenGL inside that node double-converted Z. Final logs contain **no shader compilation/buffer failures**. Nonfatal Steam initialization, missing user export-target directory, and shutdown leak/thread warnings remain; exports succeeded and images were validated. No credentials or app configuration were modified. Krita emitted nonfatal fontconfig warnings; native and PNG exports succeeded.

## Subsequent parent integration — authored prototype only

- `Tools/Blender/materialize_ranch_asset.py --family <ranch_house|ranch_furniture> --asset <name>` runs through Blender's `--python ... --` arguments, one fresh process per asset. It reads original editable sources and writes separate `.blend`/GLB derivatives under `OpenMakaiRanchGame/assets/3d/ranch_home_textured/`. Packed images keep native derivative sources editable; original model/map sources remain unchanged.
- Putz/plaster, timber/oak and cream textile slots receive their corresponding maps. Roof, metal, ceramic and painted-color identities are preserved, not covered with one global wood override. UV0 uses source-local metre dimensions with the declared tile sizes; floor grain follows plank Y and rotated timber parts use their local long axis. A final evaluated triangulation modifier resolves tangent-export warnings without removing editable base meshes/bevels.
- Read-only check: `python Tools/Materials/ranch_home/validate_model_derivatives.py`. Latest executed result: **12 maps, 3 Krita paint layers, 18 model derivatives**, matching source/export hashes, world-space vertex positions within 0.00002 m, unchanged triangle counts, and **478 normal-mapped surfaces with unit tangents**. `verify_finish.py` is a mutating finalizer; do not rerun it merely to validate finished maps.
- `scenes/dev/RanchHomePrototype.tscn` now references textured derivatives. Existing isolated `OMR_RANCH_ASSET_REVIEW=1 python Tools/VisualTargets/capture.py` loaded all 18 models and all three `.tres` resources in Godot 4.7.2 Forward+. Material textures resolved at 512 × 512; all three roughness channels read **Green**. No normal-game scene was replaced.
- Final verified capture: `.dream-loop/capture-st5t70sy`; canonical evidence at `OpenMakaiRanchGame/docs/art/ranch_house/material-pass/`. All six camera contracts match the pre-material capture `capture-2qs1hmtx`; lighting setup was not altered. First attempted capture `capture-p9azs9ye` was rejected because editor import externalized additional texture PNGs during the source snapshot. That failed run is not acceptance evidence; its log is retained.
- Rendered review still shows dim interiors and coarse/mottled plaster. Material wiring is verified, **not final art approval**, lighting polish, articulated furniture use or production housing integration. Debug build passed with existing CS8602 on full rebuild; VisualTargets normal/optimized each ran 44 tests (43 passed, 1 skip), diff check passed.
- Godot-externalized PNGs beside derivative GLBs are runtime dependencies: preserve them. Roll back only this prototype's resource references using the archived `RanchHomePrototype.before.tscn`; do not delete unrelated or original assets.
