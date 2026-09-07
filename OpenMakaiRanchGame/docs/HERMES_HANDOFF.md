# HERMES_HANDOFF — OpenMakaiRanchGame

> A new session can continue from this file alone. State verified 2026-09-07.

## Current Objective
Production-quality **first 3D anime vertical slice** of the eraMakaiRanch remake.
Standing direction (user): **soft, natural anime shading — NO hard toon/cel band, NO hard
corners.** Preserve the existing C# simulation, services, save system, and ERA-imported data;
3D is *presentation* over the same simulation, not a second economy/clock.

## Current Task
SOFT-ANIME-001 **GPU-verified + fixed** (BUG-001) and NPC 3D interaction (AC #10) shipped.
Next unblocked: **PERF-001** (GPU frame-time baseline) and the AC #8/#12 character pipeline
(both currently gate-blocked — see Known Problems).

## Current Git Branch
`dev`

## Current Commit
`1a43379` — `feat: NPC 3D interaction (AC #10)` (latest committed).
Uncommitted working tree: SOFT-ANIME-001 shader fix + GPU verification + dev capture tools
(see Files Changed).

## Completed Recently
1. **NPC 3D interaction (AC #10 + AC #11 + AC #7 player-driven)** — `CharacterAvatar3D` now
   implements `IWorldInteractable` (same contract as `WorldStation`); `RosterRig` stamps
   id/name/dispatcher on every placed avatar; `RanchGreyboxController.HandleInteract` picks the
   nearest interactable (station OR avatar) → `F` near an NPC dispatches `Mentorship` through
   the shared `IWorldCommandDispatcher` → `GameRoot.TryConductMentorship`. Committed `1a43379`.
2. **SOFT-ANIME-001 GPU fix (BUG-001)** — the hand-rolled `dot(NORMAL,-LIGHT)` shader failed
   Godot 4 GPU compile (avatars flat unlit gray). Rewrote to idiomatic `ALBEDO`+`SPECULAR=0`+
   `RIM`; rim 0.25→0.12. A/B render objectively measured + vision-confirmed the soft look.
3. **GPU render verification pipeline** — `WorldCapture` (whole-world capture) + `ShadingAB`
   (controlled Standard-vs-soft A/B), both `src/Dev/` + `scenes/dev/`.

## Systems Currently Working
- C# simulation: RanchService, BondService, ScheduleService, EconomyService, DayCycle, Save
  (SchemaVersion 14), DataRegistry, ERA import — all intact.
- 3D world: `RanchGreybox.tscn` (barn, fences, trees, well, ground, walls, PBR, sun,
  world environment), `WorldCameraRig`, `ThirdPersonPlayerController` (WASD + camera-relative +
  sprint + F-interact), `WorldInputBootstrap` (runtime input map).
- NPCs: roster avatars placed via `RosterRig` (roster=2 in a fresh game), now **interactable**.
- Soft-anime shading (opt-in per avatar), GPU-verified soft + no hard corners.
- Management UI (UiShellController) available in-world ("Open Management UI" button) — same
  simulation, efficient path.

## Systems Currently Broken
None known. (See Known Problems for gate-blocked + unverified items.)

## Files Changed (uncommitted)
- `src/Character/SoftShaderSource.cs` — idiomatic GLSL (ALBEDO/SPECULAR/RIM), no `-LIGHT`.
- `src/Character/SoftMaterialFactory.cs` — `base_color` + `rim_strength` uniforms; accessors.
- `src/Character/SoftShadingMath.cs` — rim default 0.25→0.12 (C# reference stays headless-verified).
- `src/Tests/SmokeTestRunner.cs` — soft-shader assertions updated (18), NPC interaction (14).
- `src/Dev/WorldCapture.cs` (new) — whole-world GPU capture for visual inspection.
- `src/Dev/ShadingAB.cs` (new) — controlled Standard-vs-soft A/B render.
- `scenes/dev/WorldCapture.tscn` (new), `scenes/dev/ShadingAB.tscn` (new).
- `docs/KANBAN.md` — SOFT-ANIME-001 GPU-verified, BUG-001, WORLD-004, AC #10/#11/#13/#22 done.
- `docs/HERMES_HANDOFF.md` (this file, new).

## Assets In Progress
- `assets/3d/` — 14 GLB props + PBR maps (barn, fence, tree, well, hay, trough, path stones,
  pasture boundary, grass field, wood bakes, bark/cobblestone bakes). All committed.
- Soft-shader materials (runtime, built by `SoftMaterialFactory`).

## Tests Performed
- `dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj` → **0 warnings / 0 errors**.
- `python Tools/Godot/launch.py --mode smoke` → **SMOKE PASS, 1311 assertions** (was 1298;
  +14 NPC interaction, soft-shader assertions updated to the fixed idiom).
- 18 soft-shader assertions (math contract, shader-source guards incl. "no invalid LIGHT",
  factory uniform mapping, avatar opt-in + rebuild idempotency).
- 14 NPC-interaction assertions (IWorldInteractable contract, guard double-activation, stub
  dispatcher receives Mentorship with the avatar id, nearest-target selection, bound/unbound).

## Visual Tests Performed
- **AC #22 (actually launched + visually inspected):** `WorldCapture.tscn` ran the real GPU
  render of the composed ranch; saved PNG; vision-inspected — barn/fences/trees/well/ground/
  walls/sun present, no black screen/clipping.
- **AC #13 (soft shading visually inspected):** `ShadingAB.tscn` A/B render under one
  directional light. **Objectively measured** (pixel variance) + **vision-confirmed**:
  Standard = strong lit→shadow gradient; soft shader (rim 0.12) = **properly lit, soft
  low-contrast gradient, no cel band, no hard outline, no hard corners** — a credible
  soft-anime match. This caught BUG-001 (the original shader was flat/unlit).
- Note: in the *full-world* close-up the avatars read somewhat flat because they sit near the
  tree's cast shadow (placement), not a shading-model defect — the A/B test isolates the shader
  and proves it is lit + soft.

## Known Problems
1. **PERF-001 (unblocked, needs GPU)** — 3D frame-time/draw-call/triangle baseline not yet
   measured. Not verifiable in the headless smoke path; needs the real-GPU render harness
   (now available via `WorldCapture`).
2. **CHAR-002 / ART-002 (AC #8, AC #12) — gate-blocked.** `data/characters.json` has **no
   `AdultEligibility` field**, no ConfirmedAdult character, no independent design review.
   Roster contains minors (Slay 13, Maria 15, minor-coded Ayaka). **Will not generate
   explicit character art/text for this roster** until a clearly-adult character (candidate
   Noir, 26) clears independent human design review (`ADULT_CHARACTER_VALIDATION.md`).
   Non-adult world art + fail-closed gates + gate-safe data model are all in place.
3. **MORPH-001 / ANIM-001** — blocked on CHAR-002/ART-002 (no rigged/morphed character yet).
4. **Visual end-quality of soft shading** — technically GPU-verified; final aesthetic sign-off
   (whether the soft look is "anime-natural" enough at gameplay camera) still needs a human
   design review, especially once a real character model exists.
5. **Avatar grounding in the full world** — avatars near the tree read as floating (no contact
   shadow) in the close-up; a contact-shadow / grounding pass is a candidate ART polish item.

## Important Decisions
- **No hard toon/cel shader** (user: "dont use a toon shader... i dont want to have it hard
  corners"). Soft, natural anime shading instead — implemented as SOFT-ANIME-001.
- **Soft shader = idiomatic Godot 4 spatial path** (`ALBEDO` + engine diffuse/hemi + `RIM`),
  NOT a hand-rolled `LIGHT` dot product (invalid in `fragment()`, fails GPU compile).
- **Rim 0.12** (was 0.25) — 0.25 washed the surface flat against built-in diffuse.
- **NPC interaction reuses the shared command dispatcher** — no second simulation; physical +
  management both call `GameRoot.TryConductMentorship`.
- **Headless smoke cannot verify runtime shaders** — a real-GPU render step is now part of the
  verification path (BUG-001 lesson).
- **Pre-release: fresh games are expected.** No old-save migration effort unless requested.
  Never delete user files implicitly.
- ERA source `eraMakaiRanch-game-eng-translation/` stays read-only.

## Next Exact Action
1. **Commit** the SOFT-ANIME-001 GPU fix + BUG-001 + GPU verification tools (currently
   uncommitted) — a verified green slice (build 0/0, smoke 1311 PASS).
2. Then **PERF-001**: measure representative 3D frame-time / draw calls / triangles / memory at
   the `RanchGreybox.tscn` composition using the real-GPU `WorldCapture` harness (add a
   `RenderingServer` stats read before the quit). Establish the baseline before LOD/instancing.

## Next Recommended Tasks
- **PERF-001** — GPU frame-time / draw-call / triangle baseline (unblocked, next).
- **AC #8 / CHAR-002 / ART-002** — ONE clearly-adult master character (candidate Noir) through
  the full production pipeline, **only after** independent design review clears
  `ConfirmedAdult` + `VISUALLY_UNAMBIGUOUSLY_ADULT`. Gate-safe fail-closed state is already in
  place.
- **MORPH-001 / ANIM-001** — gameplay→visual morph curves + shared rig/animation, once CHAR-002
  is unblocked.
- **AVATAR-001 (polish)** — grounding/contact shadow for placed avatars in the full world.
- **UI-002 (polish)** — in-world interaction prompt (show "Press F — {name}" when near an NPC).

## Useful Commands
- **Build:** `cd OpenMakaiRanchGame && dotnet build OpenMakaiRanchGame.csproj` (expect 0/0).
- **Smoke (headless):** `python Tools/Godot/launch.py --mode smoke` (NEVER run raw smoke against
  personal saves — slot 99 is overwritten).
- **GPU render capture (whole world):**
  `E:\GodotEditor\Godot_v4.7.2-stable_mono_win64.exe --path OpenMakaiRanchGame res://scenes/dev/WorldCapture.tscn`
  → saves `C:\Users\noa3\AppData\Local\OpenMakaiRanchCapture\world_capture.png`.
- **A/B shading test:** `... res://scenes/dev/ShadingAB.tscn` → `shading_ab.png`.
  (Isolated-profile env: set `APPDATA`/`LOCALAPPDATA`/`OMR_EXPECTED_USER_ROOT` to a temp dir so
  the personal save profile is untouched.)
- **Editor:** `python Tools/Godot/launch.py --mode editor`. **Isolated playtest:**
  `python Tools/Godot/launch.py --mode runtime --isolated`.
- **Blender (headless):** `D:\SteamLibrary\steamapps\common\Blender\blender.exe --background --python <script>`
  → `gltf` GLB export (ART-001a path).

## Resume Instructions
1. `git status` — confirm the SOFT-ANIME-001 GPU fix + GPU tools are still uncommitted (or
   already committed if a later session landed it).
2. Build (0/0) + smoke (expect ~1311 PASS).
3. If the user wants to continue: commit the green slice, then **PERF-001** (unblocked) or
   **CHAR-002** (only after design-review clearance — do NOT generate explicit adult art for the
   current roster; see Known Problems #2).
4. Do NOT re-introduce a hard toon/cel shader or a hand-rolled `LIGHT` dot product — the user's
   direction is soft/natural, and `LIGHT` in `fragment()` fails GPU compile.
5. Keep the ERA source read-only; never delete user save files; keep the simulation as the single
   source of truth (3D is presentation).
