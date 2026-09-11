# Anime visual pipeline — evidence, direction and implementation

Checkpoint: 2026-09-11. Branch: `feature/anime-lookdev-pipeline-20260911`, PR #16.
Base: `3209f4c94a43d9823baf94e0f75365e141f3aeb1`. This is a **material/look-development foundation**, not a claim to have delivered production anime characters or matched an artist's finished film.

## Research: what is actually established

Public first-party pages were researched again on 2026-09-11. Some posts expose only a search-indexed public excerpt; locked articles/project files were not accessed. The user's three supplied stills were examined as visual references, not as evidence of software settings or image authorship.

| Source | Supported conclusion | Does not establish |
| --- | --- | --- |
| BlobCG, [About](https://www.patreon.com/BlobCG/about) | Artist describes predominantly stylized rather than realistic anime animation. | Exact engine, render backend or current shader implementation. |
| BlobCG, [WIP Blog #9 and new XC2 shader](https://www.patreon.com/BlobCG/posts/wip-blog-9-and-72034837), 2022-09-15, public excerpt | Material/shader development is part of the artist's work. | Complete node graph; unavailable paid content was not inspected. |
| BlobCG, [Future Redeemed](https://www.patreon.com/posts/future-redeemed-81763916), 2023-04-19 | Artist explicitly describes revising their XC3 shader based on their XC2 shader. | The actual proprietary game shader, or the exact shader behind the user's newer stills. |
| BlobCG, [Future Redeemed models](https://www.patreon.com/BlobCG/posts/future-redeemed-82379104), 2023-05-02, public excerpt | Further revised XC3/XC2 material work. | A current 2026 workflow or a license to use models. |
| [KYOKO3D's own Patreon](https://www.patreon.com/cw/KYOKO3D), public listing | Creator publishes 3D animation. | Blender/Maya/Unreal, Eevee/Cycles, light rig, shader, samples or hardware. |

Blender appears in mirrored versions of BlobCG's social bio; the directly accessed first-party pages above did not independently confirm that bio. Therefore the previous conversation's categorical software attribution is not used as a build requirement. **Neither artist's precise renderer or material settings have been verified.** Blender is our recommended authoring tool, not a reverse-engineered claim about every reference. No paid files, artist images, ripped game models or shader packages were copied into this repository.

## Reading the three references

These are observations about the user-supplied images, not additional facts about their authors:

**A — pink hair, cool background:** strong silhouette separation, graphic hair shapes and compact stylized highlights; smooth face gradients; dark garment contrast and selective line detail. The visible player overlay/compression makes exact antialiasing and post-effect attribution unreliable.

**B — bright beach:** much softer background detail and stronger broad surface highlights; warmer foreground against cyan surroundings. Its very shallow focus and high-key lighting are useful portrait references, not appropriate defaults for navigating the ranch.

**C — green hair, neutral dark background:** more restrained lighting, warm eye colors and controlled material contrast. Fine dark boundaries and stylized hair grouping matter as much as reflectivity. This is a more useful starting point for readable dialogue portraits than reproducing B's shine everywhere.

Common direction: soft, deliberately shaped diffuse gradients; legible anime silhouettes; separate material responses; controlled highlights; restrained edge lighting. This is **not simply glossy PBR everywhere**, nor hard two-band cel shading. We use “soft anime PBR” as a project shorthand, not an official shader category. A still cannot prove anisotropy, SSS, ray tracing, a particular tonemapper or a particular compositing stack.

## Corrections to the preliminary recommendation

1. `SoftShaderSource` is a matte primitive-fallback option, not the shader of every character. `CharacterAvatar3D` retains imported placeholder materials. `PlayerAvatar3D` uses its own StandardMaterial3D instances. Replacing SoftShader alone cannot produce the requested result.
2. Quality profiles already exist in `src/App/GraphicsQualityProfile.cs`. This work consumes that policy rather than creating another settings/save authority. The `.85` project render-scale value is not proof that runtime High/Ultra always run at that scale.
3. AgX better preserves **hue** as brightness increases, while still desaturating highlights. It does not preserve full saturation indefinitely. Its exposure response must be judged using skin, hair and eye references together.
4. SDFGI, SSIL, SSR, AO, glow and depth of field are not a mandatory ordered checklist. Godot owns render ordering. Effects need independent visual/performance justification. SSR cannot replace off-screen reflection information. A material lab does not need a complete GI/fog stack.
5. Numeric material values below are our bounded starting points, not values recovered from either artist. Anisotropic hair requires coherent UVs/tangents or an authored flow map. A good face silhouette, normals, textures and deformation remain essential.

## Implemented architecture

`src/Visuals/AnimeSurfaceProfile.cs` defines presentation-only resources; no character eligibility, stats, rewards, clock or save fields. `AnimeMaterialFactory` builds local material instances while Godot reuses shader resources. Shared source resources are never edited.

| Surface | Current implementation | Default study values | Production limitation |
| --- | --- | --- | --- |
| Skin | Non-metallic soft PBR, bounded specular/roughness, subtle rim; skin-mode SSS on High/Ultra Forward+ | Roughness .46, specular .38, SSS .10 | No authored face normal field, blush/face shadow mask or final skin textures yet. |
| Hair | Separate opaque anisotropic material, normalized RG flow with a defined degenerate-map fallback | Roughness .38, anisotropy .65, rim .07 | Opaque clumps only; no alpha-card importer/conversion and no claim of automatic tangent repair. |
| Eyes | Opaque surface and restrained clearcoat; optional derivative-smoothed procedural iris for original spherical stand-ins | Roughness .20, specular .38 | Not physically refractive layered cornea. Procedural iris must stay off for imported textured eyes. |
| Cloth | Separate high-roughness non-metallic material | Roughness .80, specular .22 | No cloth simulation or textile-specific authored normal maps in this patch. |

All four use the engine's material lighting rather than a global full-screen toon filter. Skin does not receive a default varnish/clearcoat layer. No forced emission keeps faces or eyes bright at night. Materials remain opaque: writing `ALPHA`, even as 1, would move them into the transparent pipeline.

### Texture contract

Albedo and color uniforms are sRGB inputs (`source_color`). Normal, flow and surface-mask textures are linear data. `SurfaceMask` is **RGBA = ambient occlusion, roughness multiplier, specular multiplier, scattering multiplier**. It is **not glTF's ORM packing**; convert/repack explicitly. Missing maps have defined defaults, numeric NaN/infinite/out-of-range inputs are sanitized without mutating the profile. Normal maps require correct handedness/tangents; do not mistake a single test sphere for a complete import qualification.

### Capability and cost policy

Low, non-Forward+ and unknown renderer skin/hair select a basic shader **without SSS/anisotropy outputs**, instead of merely setting expensive-path uniforms to zero. Medium/Custom Forward+ hair can use anisotropy; only High/Ultra Forward+ skin uses SSS. Unknown quality resolves through the existing Medium policy. Eye shading is opaque across both tested desktop backends. Mobile has conservative material selection but is not device-certified by desktop tests.

The study intentionally renders all tiers at **native scale 1.0** so material comparisons are not confounded by resolution. This does not change the game's user-selected render scale or establish a performance preset. Ultra initially shares the same material algorithm with High; more cost is not automatically more quality.

### Safe adoption in actual scenes

`AnimeSurfaceBinding3D` is an explicit per-surface component. Assign a target MeshInstance3D, surface index and profile. It never recursively overrides a model, never mutates imported Mesh/Material resources and refuses an existing whole-mesh MaterialOverride, transparent BaseMaterial3D, or an unknown ShaderMaterial. It restores only the surface override it still owns; later edits by another system are preserved. Assign textures in the new profile explicitly: this is not an automatic Blender shader translator.

The generated player can opt in for a session with **`--anime-pbr-preview` after Godot's `--` separator**. Only recognized direct children of PlayerAvatar3D's generated stand-in are rematerialized: body, head, hair and eyes. Unknown accessories, transparent parts and nested/imported models remain untouched. The existing player colors and existing quality selection are reused. No new saved setting is introduced. Settings/state rebuilds reapply the session preview through the existing RefreshFrom path. Imported ranch NPC models retain their authored materials until individual surface conversions are reviewed.

The old SoftShader and ordinary default player appearance are preserved. Do not merge a global replacement into the concurrent world/station work without comparing authored materials first.

## Look-development laboratory

Open `scenes/dev/AnimeLookDev.tscn` in the Godot editor and run the current scene (F6), or use the standalone command below to avoid all game autoloads. The scene has its own World3D/SubViewport, fixed exposure, an original fully clothed geometric mannequin, skin-tone swatches, hair/eye spheres and a fabric panel. It deliberately is **not** a finished anime character.

Controls: Neutral/Daylight/Interior/Sunset/Night lighting; Low/Medium/High/Ultra; matte comparison; portrait camera; optional portrait-only depth of field; 30-degree subject rotation. Lighting presets change only the study, never the game calendar. No global RenderingServer quality or ProjectSettings writes occur.

Matte A/B preserves geometry, camera and light/environment instances. It compares a simplified matte palette to the new material treatment, including procedural eye appearance; it is not a pixel-exact capture of the previous entire game, and it does not isolate specular as the only changed variable.

Depth of field is opt-in, far-only, portrait-only, non-Low and Forward+-only. Navigation framing remains sharp. Bloom/AO are conservative and capability-gated. The lab deliberately disables SSR/SSIL and does not enable GI or volumetric fog. Test representative world interiors and moving faces before changing those decisions.

```bash
# From repository root; existing Godot 4.7 stable Mono and .NET SDK must be installed.
# Set GODOT_BIN to the actual non-console-stub executable when it is not on PATH.
python Tools/Godot/anime_lookdev.py --interactive --renderer forward_plus --timeout 3600

# Automated, isolated rendered validation; needs a display (or xvfb-run on Linux).
python Tools/Godot/anime_lookdev.py --renderer forward_plus
python Tools/Godot/anime_lookdev.py --renderer gl_compatibility
python -m unittest discover -s Tools/Godot -p 'test_anime_lookdev.py' -v
```

`AnimeLookDevChecks.tscn` is **not** a personal-save test entry point. The launcher copies a rendering-source whitelist and the existing quality-policy file into `.artifacts/anime-lookdev/<unique-run>/project`. This host has no GameRoot, SceneRouter, MCP or other game autoloads. Runtime data/config paths and evidence are disposable; the runner never executes ordinary save-slot tests. Do not replace it with raw smoke flags.

## Next production passes, in order

**1. One original master character.** Author a non-explicit reference sheet with front/side/three-quarter views, adult design review where required by the project's existing policy, neutral expression and practical clothing. Preserve `.blend`, unit scale, rest pose, UVs, custom normals, material separation and original design provenance. Export GLB with test animations; keep Blender shader graphs separate from Godot runtime shader implementation.

**2. Face and hair art, not more bloom.** Establish head silhouette, eyelids/lashes, iris placement, mouth shapes and manually controlled face shading under rotating light. Evaluate a face-shadow mask or authored normal field only on that qualified model. Author opaque hair clumps, coherent tangent flow and controlled highlight breakup. Avoid identical plastic sheen across skin, hair and clothes.

**3. Deformation/animation.** Validate shoulders, elbows, hips, knees, blink, eye aim and speech shapes at ordinary gameplay and portrait distance. Maintain matching clothing deformations, limit intersections and test extreme supported combinations. Secondary hair/clothing motion needs bounded joints and distance-based updates. This patch adds no body-physics or final rig.

**4. A real ranch lighting slice.** One exterior plus one traversable interior, with day/night and overcast comparison, coherent palette and reflection-probe strategy. Keep existing DaylightRig and gameplay camera as authorities. Portrait fill/rim lights must be explicitly scoped and cleaned up; never attach unrestricted shadow-casting photo lights to every resident.

**5. Performance and motion acceptance.** Record GPU/CPU frame times on representative low/high hardware at chosen resolution; assess hair shimmer, eye stability, AO/SSS halos, TAA ghosting and texture/LOD transitions while walking. Use software CI for regressions, not performance promises. Choose numeric budgets after measurements rather than inventing universal triangle limits.

A production visual milestone requires reviewed real-model turntables and walking/dialogue screenshots in those lighting conditions, with graphics settings/hardware recorded. Passing compilation, shader tests or a mannequin screenshot alone does not satisfy that milestone.

## Technical primary references

- [Godot 4.7 spatial shader reference](https://docs.godotengine.org/en/4.7/tutorials/shaders/shader_reference/spatial_shader.html): material outputs, skin mode, anisotropy, opaque/transparent behavior; use 4.7 `CLEARCOAT_ROUGHNESS` rather than an older shader API spelling.
- [Godot 4.7 environment and post-processing](https://docs.godotengine.org/en/4.7/tutorials/3d/environment_and_post_processing.html): tonemapping, hue/highlights, AO, reflection and renderer tradeoffs.
- [Godot 4.7 renderers](https://docs.godotengine.org/en/4.7/tutorials/rendering/renderers.html): capability distinctions; do not equate a renderer name with identical visual output across devices.
- [CameraAttributesPractical](https://docs.godotengine.org/en/4.7/classes/class_cameraattributespractical.html): depth-of-field controls and exposure ownership.
- [Godot import formats](https://docs.godotengine.org/en/4.7/tutorials/assets_pipeline/importing_3d_scenes/available_formats.html): glTF/Blender pipeline and export constraints.

See `ANIME_LOOKDEV_VALIDATION.md` for executed evidence and remaining acceptance; source and result receipts take precedence over aspirations in this document.
