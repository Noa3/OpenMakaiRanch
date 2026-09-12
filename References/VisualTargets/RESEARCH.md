# Research and the adapted Dream Loop

Research date: 2026-09-12. Primary sources only for technical conclusions. This is an **adaptation** for an existing Godot project and concurrent agents, not a claim to have executed the original skill or its independent judge.

## Dream Loop, pinned rather than search-cache instructions

Read `achimala/dream-loop` at **9bddb901f7d071cfefdd21e264267c757177a9df**: SKILL.md, the Pro workflow and its 3D-assets reference. The live GitHub files differ from the older SKILL.md body still returned by web search. Use the pinned files, not a cached instruction that forbids stylization: the user's soft-anime direction is authoritative. No subscription tier is inferred here; the Pro document is used as a process reference.

The reusable structure is: capture current work, generate a target from that baseline, implement, capture again, obtain independent visual criticism, correct and repeat. It separates visual quality from acceptable frame rate. The source explicitly requires image-generation access or a supplied target. Neither a prompt nor a generic screenshot is a substitute for the missing generated image. [1–3]

For this project, target images are versioned in `References/VisualTargets`, as explicitly requested. Only disposable run context belongs in ignored `.dream-loop/`. Do not install third-party skills or execute their helper code blindly. The referenced asset strategy does not authorize spending credits, reading secrets, uploading personal data or importing assets with unknown rights. This pack uses original code/wording and links to the MIT source rather than vendoring it. [4]

## Five adaptations needed for a real game

**One image is not a world specification.** Use a locked camera across three facility states, plus reverse views, exterior/interior agreement and dimensioned building sheets. A second doorway cannot appear just because another generated image needs a composition. Verify all eight existing footprints and entrances; annexes need a separately approved layout change. This is our engineering adaptation, not a guarantee made by Dream Loop.

**Artwork and interface are separate.** Let the image model refine background/asset appearance, while labels, inventory values, buttons and subtitles remain real adjustable UI. XAG 102 specifically advises keeping text separate from images. The SVG studies are reference designs, not textures to place in front of the player. [5]

**Use capture context, not visual guesswork.** Record scene, camera transform/projection, viewport, quality, renderer, phase, weather, fixture progress, character identities and locale. Godot documents waiting for `frame_post_draw` before reading the viewport texture; otherwise a capture can be black or stale. The existing repository capture harness already provides isolated runs. [6]

**A beautiful still cannot pass animation.** Blink closure, hand contact, gaze, interruption, expression blending and locomotion must be reviewed in short moving sequences with their real rig. Godot's AnimationTree blends/coordinates animation; arbitrary competing writers are not a replacement. Keep the existing presence lease and command ownership boundaries. [7]

**A visual score is not performance evidence.** Match composition, materials and lighting, then measure real frame time on a declared GPU/resolution/quality and resident count. Software-rendered CI can test correctness but cannot stand in for player hardware. Godot's Visual Profiler measures rendering work on CPU/GPU; scripting and physics require the separate standard Profiler. Compare equal viewport sizes. [8]

## UI acceptance standards

As a design target, use at least readable PC text equivalent to 18px body height at 1080p, and support scaling to 200% without lost actions or mandatory two-direction scrolling. This is based on XAG 101, not a claim that the current static SVGs pass runtime accessibility. Our full-size mockups use 22–28px body text at 1600x900; the overview atlas intentionally scales them down for indexing only. [9]

Important normal text should meet 4.5:1 contrast and must be tested over the lowest-contrast background region; large elements have different guidance. Do not encode unavailable/selected/error states by color alone. Actual options, focused controls and background opacity need runtime checks. [5]

Stable focus, consistent back behavior and predictable navigation are part of the menu design, following XAG 112. Test keyboard and controller, long German/Japanese strings, narrow viewports, changing state, cancelled purchases and lost proximity. The 35-route map prevents a cosmetic redesign from silently dropping existing functionality. [10]

## Asset production implications

Prefer original/authored or appropriately licensed assets, with source/provenance records. Godot supports glTF and Blender workflows; import settings and inherited scenes should preserve a repeatable pipeline. Image-to-3D output is a candidate mesh: inspect scale, topology, UVs, tangents, materials, LODs, rigging and collision separately. A screenshot cannot certify any of those. Do not produce a fake normal map by treating a painted image as validated surface data. [11–12]

The existing character reference artists are a rendering/material direction, not an asset license or a source of production identities. This research pass does not assert either artist's renderer, exact shader graph or current production software.

## Source ledger

[1] https://github.com/achimala/dream-loop/blob/9bddb901f7d071cfefdd21e264267c757177a9df/SKILL.md

[2] https://github.com/achimala/dream-loop/blob/9bddb901f7d071cfefdd21e264267c757177a9df/references/pro-mode/workflow.md

[3] https://github.com/achimala/dream-loop/blob/9bddb901f7d071cfefdd21e264267c757177a9df/references/pro-mode/assets-3d.md

[4] https://github.com/achimala/dream-loop/blob/9bddb901f7d071cfefdd21e264267c757177a9df/LICENSE

[5] https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/102

[6] https://docs.godotengine.org/en/stable/classes/class_viewport.html#class-viewport-method-get-texture

[7] https://docs.godotengine.org/en/stable/tutorials/animation/animation_tree.html

[8] https://docs.godotengine.org/en/stable/tutorials/scripting/debug/debugger_panel.html#visual-profiler

[9] https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/101

[10] https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/112

[11] https://docs.godotengine.org/en/stable/tutorials/assets_pipeline/importing_3d_scenes/available_formats.html

[12] https://docs.godotengine.org/en/stable/tutorials/assets_pipeline/importing_3d_scenes/import_configuration.html

The moving stable documentation alias is reference material, not permission to change the project's pinned engine/SDK/C# contract. Original project facts come from the pinned local source snapshot: `RanchBuildingPlots.cs`, `WorldStationPanel*.cs`, `UiShellController.cs`, `BUILDING_PLOTS.md`, `STATION_PLANNING.md` and the user-approved revised `MAKAI_WORLD_STYLE_AND_LORE.md`.
