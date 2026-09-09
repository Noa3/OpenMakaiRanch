# External Asset Admission — CC0 Candidates

Last reviewed: 2026-09-09

This file is a provenance gate, not an asset dump. No external binary is considered admitted to
OpenMakaiRanch until the exact package/file, source page, license and destination are recorded here
(or in a later lock/receipt) and the imported copy is verified.

## Default policy

- Prefer assets with an explicit **CC0 / public-domain dedication** from the original publisher.
- Do not use search-engine mirrors, reuploads, Pinterest boards, asset scrapes or license-unknown files.
- Keep the upstream package name/version/date and original source page.
- Preserve an upstream LICENSE/readme when the package includes one.
- Record modifications separately; do not imply that OpenMakaiRanch authored the upstream asset.
- Do not use provider logos or promotional renders as game assets merely because the downloadable
  asset pack itself is CC0.
- When an asset's license differs from the publisher's usual license, the individual asset page wins.
- Until exact files are admitted, the game must continue to work with its authored/procedural placeholders.

## Approved source families for evaluation

### Quaternius — Farm Buildings Pack

Source: https://quaternius.com/packs/farmbuildings.html

Publisher page states:
- 13 models;
- FBX / OBJ / Blend;
- CC0;
- free for personal and commercial projects.

Potential use:
- replace greybox barn/workshop/kitchen/farm-building shells after scale/style review;
- use as modular reference or prototype assets rather than redesigning gameplay around the pack.

Status: **candidate only — not yet vendored**.

### Quaternius — Ultimate Stylized Nature Pack

Source: https://quaternius.com/packs/ultimatestylizednature.html

Publisher page states:
- 60+ stylized nature assets;
- FBX / OBJ / glTF / Blend;
- CC0;
- free for personal and commercial projects.

Potential use:
- trees, shrubs, rocks, small landscape props;
- strongest current candidate for the readable stylized/anime-adjacent ranch exterior.

Status: **candidate only — not yet vendored**.

### Kenney — Nature Kit

Source: https://kenney.nl/assets/nature-kit

Publisher page states:
- 330 3D files;
- Creative Commons CC0.

Kenney support documentation also states that asset-page game assets are public-domain/CC0 and can
be used commercially without required attribution.

Potential use:
- fallback foliage/rocks/environment props;
- especially useful when Quaternius silhouettes do not fit a gameplay landmark.

Status: **candidate only — not yet vendored**.

### Poly Haven

License: https://polyhaven.com/license

Poly Haven states that its downloadable HDRIs, textures and 3D models are CC0 and may be used for
commercial work without required attribution.

Potential use:
- selectively source neutral PBR surfaces or an HDRI when it improves lighting;
- avoid mixing photoreal scanned props into the stylized ranch unless deliberately simplified.

Important:
- Poly Haven's site/logo/promotional content is not automatically the same thing as a downloadable
  CC0 asset; use the actual asset files and their recorded source pages.

Status: **approved source family for evaluation; no exact asset admitted yet**.

### Kenney — Fantasy Town Kit

Source: https://kenney.nl/assets/fantasy-town-kit

Publisher page states:
- 160 3D files;
- Creative Commons CC0.

Potential use:
- Okachi Town building shells, walls and town props;
- strong candidate for replacing the current generated service-building placeholders while keeping
  the authored service positions/IDs.

Status: **candidate only — not yet vendored**.

### Quaternius — Medieval Village Pack

Source: https://quaternius.com/packs/medievalvillage.html

Publisher page states:
- 44 models;
- FBX / OBJ / Blend;
- CC0;
- free for personal and commercial projects.

Potential use:
- alternate/secondary Okachi Town buildings and props;
- useful when a more anime-fantasy/rural silhouette fits better than Kenney's kit.

Status: **candidate only — not yet vendored**.

## First recommended admission batch

Before importing, inspect the exact downloaded archives and record the package filenames/hashes.

1. Quaternius Ultimate Stylized Nature Pack
   - trees;
   - shrubs;
   - rocks;
   - grass/ground-detail props.
2. Quaternius Farm Buildings Pack
   - one barn/farm-building candidate;
   - one utility/workshop candidate if silhouette and scale fit.
3. Kenney Nature Kit
   - only fill gaps that remain after the first two packs.
4. Poly Haven
   - material/HDRI only when the stylized scene still needs it.

## Godot integration rules

- External originals live under a dedicated provenance-preserving source directory before derived
  Godot-ready assets are created.
- Derived/imported assets should use stable project paths and must not replace simulation IDs.
- Scene landmarks keep existing facility IDs (pasture, kitchen, workshop, pharmacy_lab, dairy_barn, etc.)
  even when their meshes change.
- Collisions/navigation are authored for gameplay, not blindly inherited from decorative meshes.
- LOD/material/texture choices are reviewed at the actual third-person camera.
- Placeholder/procedural visuals remain available until the replacement passes runtime review.

## Not approved by this document

- license-unknown anime character models;
- models ripped from commercial games;
- marketplace assets whose redistribution/use terms have not been individually reviewed;
- AI-generated assets where the generator/output rights are unclear;
- any downloaded asset merely because a search result labels it "free".