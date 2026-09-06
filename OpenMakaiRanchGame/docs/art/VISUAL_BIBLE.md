# Visual Bible — Draft 0

Status: **DRAFT / REFERENCE_NEEDED**. No visual direction selected by the user, no golden images approved, no hero model produced. This is a production brief derived from the requested target, not evidence of completed art or original visual canon.

## Intent

A readable third-person anime ranch/life simulation, not a generic farming reskin. Architecture, props and character roles must follow source-grounded facilities/actions. Keep practical management UI available. Detailed art follows the functional ranch floor plan and greybox.

## Character style

Use clean silhouettes, intentionally simplified surfaces and controlled shading. Distinguish characters through face, posture, hair volume, garment construction and source-grounded role, not palette swaps on one body. Measure height and proportions in Blender; distinguish exact source measurements from explicit art-direction proposals. No unsupported canonical measurements.

All initial concepts are non-explicit. Identity, chronological age, apparent age, context and visual review are separate. No character currently passes CONFIRMED_ADULT and VISUALLY_UNAMBIGUOUSLY_ADULT. Never use an age number to clear a minor-coded design. Consult `../ADULT_CHARACTER_VALIDATION.md` before any character-specific production.

## Face and eyes

Test front, profile and three-quarter views at gameplay camera distance. Use controlled cheek/jaw planes, modest nose projection and readable eyelids/iris. Eyes should express gaze and blink without constant perfect player tracking. Keep head/eye look-at within anatomical limits. Custom normals or face-light masks are optional experiments, not mandatory complexity before a simple Godot light test.

## Hair

Use a small number of deliberate large clumps establishing front, side and rear volume before small strands. Preserve selected identity across generated views. Avoid transparency-heavy detail until performance and outline interaction are measured. Secondary motion follows a stable shared rig, not early noisy physics.

## Clothing

Build plausible garment layers with clear seams, attachments and footwear. Body, clothing and accessories remain modular. Supported body morphs need mapped clothing shapes; no static outfit that clips throughout the advertised range. Use non-explicit outfits in the first pipeline tests. Validate sitting, walking, running, bending and work poses, not only a neutral render.

## Architecture and environment shapes

Proposed direction for concept comparison: practical timber/stone ranch structures with restrained Makai-world accents. This is not claimed original canon. Establish structural masses, entrances, roof language, repeatable wall/floor/fence modules and gameplay landmarks. Broad readable shapes before dense decoration. Buildings remain editable modules, not one fused generated mesh.

Floor plan must mark player/NPC routes, work/event space, entrance clearance, camera clearance and interaction anchors. Concept art cannot override workable scale/navigation. Interior and exterior must share materials/palette rather than becoming unrelated styles.

## Props and vegetation

Props should reveal the actual work they support: storage, counters, stations and tools mapped to current data. Use real-world scale as a starting point, then verify reach/animation anchors. Keep material count and collision complexity bounded. Vegetation frames routes without hiding targets; large shape groups before alpha-card density.

## Palette and materials

Palette direction is provisional: warm neutral structural surfaces, cooler shaded values, limited character/facility accent colors. Establish exact swatches only after comparing concepts. Avoid high-frequency PBR noise and plastic-looking skin. Create consistent wood, stone, painted wood, metal, fabric, soil, grass and water families. Do not set arbitrary final texture or triangle budgets before a representative asset is measured.

## Lighting and shadows

Prototype Day, Evening, Night and Interior profiles. Map them to existing Calendar phases; do not invent a second running clock. Preserve face and path readability at night. Start with controlled light/shadow bands and shadow tint. Add face-specific lighting or rim/specular masks only when a visual test demonstrates need.

Final toon materials belong to Godot, with separate face, skin, eyes, hair, cloth and metal categories. Blender preview graphs are reference only; glTF does not transfer arbitrary shader graphs.

## Outlines and VFX

Compare inverted-hull, screen-space and no-outline baselines in Godot on face/hair/clothing at multiple distances. Record artifacts around transparencies and intersections before selecting. VFX should communicate actions/state without obscuring faces, prompts or paths. No full-screen polish before interactions work.

## UI relationship to world

Use real Godot Controls, not flattened generated UI images. Keep existing functionality; later improve hierarchy, gamepad focus, tooltips and shortcuts. World input pauses while a panel owns focus. World interaction and management shortcut dispatch the same command. Do not require repeated walking for every routine action.

## Reference workflow

Keep root `art/reference/` separated into characters, architecture, props, vegetation, UI, lighting and materials. Major asset workflow: audit source, define experience, produce candidates, select direction, establish one identity master, derive views sequentially, model editable source, render comparisons, integrate and inspect actual gameplay.

For hero references, use `art/reference/characters/<id>/` with identity_master, front, front_3q, side, back_3q, back, face_closeup, hair/outfit/expression references, palette and notes as the work becomes available. Do not create empty images or imply all views already exist. Reject identity drift; archive rejected candidates outside the active identity set without deleting original references.

## Authoring and runtime sources

Blender source: root `art/blender/characters`, `environments`, `props`. Reusable automation: root `Tools/Blender`. Runtime: `OpenMakaiRanchGame/assets/3d/...` using GLB plus Godot scenes/materials. These directories are planned, not populated by this baseline.

Record per asset: triangles, materials, textures, bones, morphs, collision complexity and measured runtime cost. Check deformation, normals, UVs, rig, scale and morphs. AI-generated meshes are drafts until validated. Use legally usable references/animations and record provenance; do not clone commercial assets.

## Golden targets and visual acceptance

Planned benchmarks: ranch day/evening, character close-up/gameplay and one interaction scene. None exists or is approved yet. Store selected images and comparison renders with exact engine/camera/light settings. CharacterLab/MorphLab/AnimationLab/EnvironmentLab are future tools, not present scenes.

Acceptance requires a real Godot screenshot, inspection against the selected target, recorded largest mismatch and an iterative correction. Loading successfully is not visual approval. Only one complete master-character and environment pipeline should precede mass asset conversion.
