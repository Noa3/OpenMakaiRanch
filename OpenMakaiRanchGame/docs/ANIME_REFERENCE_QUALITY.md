# Anime reference quality and material fidelity

Research/implementation checkpoint: **2026-09-11**. This is a follow-up to [ANIME_VISUAL_PIPELINE.md](ANIME_VISUAL_PIPELINE.md), not a second rendering architecture or a new gameplay roadmap.

## Branch ownership and scope

Work is isolated on `feature/anime-material-quality-20260911`, stacked on the fixed `7aa7f0dbe197aa8df77f20b2b3464490d0ec41f8` snapshot of `feature/anime-lookdev-pipeline-20260911` (PR #16). The follow-up is PR #17. Neither the base branch, main, nor the concurrent world/stations/interiors branch is modified. Review/merge #16 first; then retarget/revalidate #17 against main. Do not automatically merge or force-reset concurrent work.

The parent already supplied the anime material factory, explicit bindings, generated-player opt-in, quality routing, lighting lab and rendered CI. This pass **extends those features**. It does not claim to have created the entire graphics pipeline or to have reached the reference artists' finished production quality.

## What the artist research actually establishes

| Claim | Evidence and limit |
| --- | --- |
| BlobCG uses personally revised anime shaders | The public text of **Future Redeemed**, 19 April 2023 [1], discusses remaking the XC3 shader around the then-current XC2 shader. The public preview of **Future Redeemed models**, 2 May 2023 [2], also describes that relationship. This supports custom look development, not a particular game engine. |
| BlobCG's exact current renderer | **Not verified in this research pass.** Mirrored biographies have described Blender, but the primary social profile was not readable here. The sources above do not establish Eevee versus Cycles, exact nodes, sample counts, or a current 2026 toolchain. Do not convert an older or mirrored biography into a certainty about every supplied image. |
| Kyoko3D's workflow | The creator's own Patreon profile [3] identifies 3D animation. It does not supply a reliably accessible renderer, software list, shader graph or settings. Unrelated artists/models called Kyoko are not evidence. |
| A screenshot proves anisotropy, SSS, HDRI or AgX | **No.** Those are possible implementation choices; similar visible results can come from textures, geometry, normals, lighting or compositing. We choose our own real-time techniques and test them. |

Research used public creator statements/previews and Godot's primary documentation. The three user-supplied images were the detailed visual references. Full locked animations and private production files were not inspected, purchased, downloaded, or redistributed. No image is assigned to a particular artist without a source establishing that attribution. The references are not a license to reuse characters, textures, animation files or branding.

## Visible targets from the three supplied references

These are visual observations and our art-direction decisions, not claims about either artist's shader internals.

**Pink-haired portrait:** the face has deliberately simplified planes, clean eyes/lashes, local line accents, broad hair shapes, controlled bright hair highlights and strong warm/cool separation from the background. The camera and framing contribute substantially. Transfer the controlled contrast and material hierarchy, not the specific identity, outfit or composition.

**Bright outdoor full-body view:** the body/garment surfaces have different highlight shapes; the light is broad, the background has atmospheric softness, and the subject remains readable against a bright environment. Glossiness is local, not one roughness value applied to every surface. A sharp gameplay camera must remain usable even when the presentation camera has shallow depth of field.

**Green-haired close-up:** face silhouette, eyelid contours, restrained nose/mouth details and the arrangement of hair clumps carry much of the anime appearance. Fine linework exists at lashes and clothing edges despite the generally soft shading. This does not justify a thick full-screen outline or hard two-band cel shading everywhere.

Our target remains **soft, stylized anime materials with physically informed reflections**, not unrestricted realism and not a blanket toon filter. 'Soft anime PBR' is a descriptive project label, not an identified commercial shader product.

| Area | Production direction | Avoid |
| --- | --- | --- |
| Face | Author cheek/chin/forehead silhouette, eyelid geometry and controlled normals; review front, three-quarter and profile under moving light. | Assuming a sphere with a nose and glossy skin is a finished anime face. |
| Eyes | Preserve painted iris/pupil/catchlight artwork; tint only a dedicated iris region; keep an opaque depth-stable baseline. | Tinting the sclera blue when changing iris color; replacing textured eyes with the spherical test iris. |
| Hair | Large authored clumps, coherent root-to-tip UV/tangents, low-frequency color variation, masked roughness/specular and restrained anisotropic response. | Treating every lock as identical polished plastic, noisy micro-normal detail, transparent cards without sorting review. |
| Skin | Smooth geometry, soft diffuse response, restrained local color variation, controlled highlights; SSS is optional by renderer/tier. | Default varnish/clearcoat, globally wet-looking skin, excessive AO or baking all scene shadows into albedo. |
| Cloth/accessories | Separate fabric and harder-surface response; retain painted seams and trim. | Applying the skin shader to the complete character or silently making metal non-metallic. |
| Lighting/camera | Exposure-consistent tests, readable fill and limited rim, authored interior/outdoor transitions. | Enabling every expensive effect simultaneously or using bloom/blur to conceal bad assets. |

## Implemented material fidelity changes

`AnimeSurfaceProfile` now has `TintMaskTexture`, `UvScale` and `UvOffset`. All four shader families use the same UV1 transform before sampling their maps. Defaults preserve the parent branch's behavior.

Tint is multiplicative and selective:

```text
surface_uv = UV1 * UvScale + UvOffset
final_color = albedo(surface_uv) * lerp(white, BaseColor, tint_mask_R(surface_uv))
```

The tint mask is **linear data**: R=0 preserves authored albedo, R=1 applies the selected palette. A missing mask intentionally tints everything as before. Intermediate values allow soft transitions. This is not a replacement for the existing surface mask:

| Map | Interpretation |
| --- | --- |
| Albedo | Color/sRGB, including optional original painted detail. |
| Tint mask | Linear R, controls recoloring only. No alpha transparency. |
| Surface mask | Linear RGBA = AO / roughness multiplier / specular multiplier / SSS multiplier. **Not glTF ORM.** |
| Normal | Tangent-space normal data; correct UVs and tangents remain the asset author's responsibility. |
| Hair flow | Linear RG direction; follows the transformed UV lookup. |

UV scales are finite, bounded to [-64,64], with near-zero or non-finite components replaced by 1. Offsets are bounded to [-64,64], with non-finite components replaced by 0. Factory validation does not modify caller resources. These safety bounds are not a general lossless converter for every imported material feature. UV2, triplanar maps, per-map transforms, metallic materials and transparency still require deliberate authored handling.

The existing generated-player `--anime-pbr-preview` path preserves its simple source UV1 scale/offset and retains textured-eye artwork. Only an **untextured spherical stand-in eye** receives the procedural iris. Unrecognized, nested and transparent objects remain untouched. There is no recursive material replacement, new save setting or default visual-mode switch.

The matte comparison now preserves the same authored albedo, tint masks and UVs while removing gloss/rim/scattering. The opaque eye's clearcoat can be disabled for that comparison. This is a material-response A/B, not a claim that all illumination contributions are perfectly isolated.

## Improved look-development specimen

The lab replaces the round head/protruding nose/eyeball assembly with **original, sampled calibration surfaces**: a shaped face with integrated nose, almond-shaped eye patches, local eyelid/brow/mouth accents, a scalp shell and curved volumetric hair locks. Explicit normals, UVs, tangent vectors and handedness accompany the mesh. Related locks share one material/mesh surface rather than allocating one material per strand.

Small deterministic 256x128 or 128x256 maps provide eye artwork, iris-only tint, restrained skin color variation and hair color/material variation, with mipmaps. They are generated from project code, not copied from artist images. The body remains a fully clothed geometric stand-in. The specimen is **not a rigged production character**, not an approved identity/design, and not a substitute for artist-authored topology, animation or corrective shapes.

Portrait mode hides the large peripheral material swatches; returning to full framing restores them. The acceptance sequence adds iris recolor, intentionally unmasked control, UV-shift, near-profile and night portrait views. It compares actual projected iris and sclera pixel neighborhoods, rather than accepting only a successful shader compile.

## Authoring the next production character

Before replacing ranch residents, finish one original/licensed hero character in a Blender-to-glTF-to-Godot loop. Godot supports glTF import [6]; importing a mesh is not importing arbitrary Blender shader-node behavior. Recreate the intended material response through these explicit Godot profiles.

1. **Approve the asset and silhouette.** Record provenance/redistribution permission and required character metadata through existing project procedures. Prepare front/side/three-quarter views, neutral expression, real-world scale and recognizable non-copied design. Do not use this calibration specimen to bypass identity/content gates.
2. **Author deformation-ready geometry.** Face/eyelid loops, closed mouth interior where needed, shoulder/elbow/hand topology, skeleton, skin weights, blink and expression shapes. Preserve shapes at actual dialogue distance. Hair/clothing motion and corrective poses need separate animation tests, not shader settings.
3. **Supply deliberate maps.** Stable UV1, no accidental flipped normal convention, texture-color/data separation, explicit tint regions, readable hair flow, and roughness appropriate to each material. Keep original textures and shader profiles outside imported resources so reimport is repeatable.
4. **Bind explicitly and compare.** Use `AnimeSurfaceBinding3D` per approved surface. Validate at fixed exposure in all five light presets, both renderers, native-resolution portrait/full views, and rotating light/subject tests. Then review in the real ranch with the world agent's environments. Keep a rollback to authored/original materials.
5. **Measure before rollout.** Define the minimum PC, display resolution, intended resident count and a repeatable ranch route. Measure frame time, VRAM, shadow passes, draw calls and animation cost. Add LODs and texture budgets from those measurements. Software-rendered CI cannot establish a target-PC FPS claim.

Prioritize a finished face/eyes/hair/rig over expanding the number of placeholder characters. A production model should pass silhouette and expression review with post-processing disabled before adding presentation lighting. The desired reference quality remains an art-and-animation deliverable, not just a renderer checkbox.

## Running and reviewing

Existing engine/framework contracts remain unchanged: Godot 4.7.2 Mono, C# 12, project target net8.0 and SDK 10.0.401. Use the installed engine path; the lab launcher does not install/download anything.

```bash
python -m unittest discover -s Tools/Godot -p 'test_anime_lookdev.py' -v
python Tools/Godot/anime_lookdev.py --godot /path/to/godot-mono --renderer forward_plus
python Tools/Godot/anime_lookdev.py --godot /path/to/godot-mono --renderer gl_compatibility
python Tools/Godot/anime_lookdev.py --godot /path/to/godot-mono --renderer forward_plus --interactive
```

Linux rendered execution needs a display, or `xvfb-run -a` before the command. The autoload-free disposable host remains isolated from the ranch clock, gameplay and personal saves. Never substitute a headless dummy-renderer run for visual evidence. The 16 required screenshots, results, complete logs and hashes must agree with the tested source commit. See the follow-up validation receipt for actual results; a planned check is not a passed check.

## Sources

Accessed 11 September 2026. Creator posts are historical statements, not a current toolchain guarantee.

1. BlobCG, **Future Redeemed**, 19 April 2023: https://www.patreon.com/BlobCG/posts/future-redeemed-81763916
2. BlobCG, **Future Redeemed models**, public preview, 2 May 2023: https://www.patreon.com/BlobCG/posts/future-redeemed-82379104
3. Kyoko3D, creator profile: https://www.patreon.com/cw/KYOKO3D
4. Godot 4.7, spatial shader reference (outputs, opaque/alpha behavior, material response): https://docs.godotengine.org/en/4.7/tutorials/shaders/shader_reference/spatial_shader.html
5. Godot 4.7, ArrayMesh (attribute arrays and clockwise front-face winding): https://docs.godotengine.org/en/4.7/classes/class_arraymesh.html
6. Godot 4.7, 3D import formats: https://docs.godotengine.org/en/4.7/tutorials/assets_pipeline/importing_3d_scenes/available_formats.html
7. Godot 4.7, environment and post-processing (AgX, reflections and camera effects): https://docs.godotengine.org/en/4.7/tutorials/3d/environment_and_post_processing.html
8. Godot 4.7, BaseMaterial3D (source texture/UV properties): https://docs.godotengine.org/en/4.7/classes/class_basematerial3d.html
