# HERMES_HANDOFF — OpenMakaiRanchGame

> A new session can continue from this file alone. State verified 2026-09-08.

## Current Objective
Production-quality **first 3D anime vertical slice** of the eraMakaiRanch remake.
Standing direction (user): **soft, natural anime shading — NO hard toon/cel band, NO hard
corners.** Preserve the existing C# simulation, services, save system, and ERA-imported data;
3D is *presentation* over the same simulation, not a second economy/clock.

## Current Task
Landing **WORLD-TOWN-001** — the first original area beyond the ranch made physically present:
a walkable **town 3D scene** (`scenes/Town.tscn`) with a **shop counter** wired to the *existing*
`ShopService` (single economy, no second shop). This is the top READY world-design card from
`docs/WORLD_DESIGN.md` (the area-inventory + world Kanban authored earlier this branch). This
session also shipped WORLD-CAMERA (mouse/keyboard camera control — AC #6) and the WORLD_DESIGN
area inventory/plan. Prior: NSFW-GATE-002 (gate negative tests), WORLD-INPUT (gamepad), UI-002,
PERF-001, SOFT-ANIME-001 + BUG-001 (GPU-verified), AC #10/#11/#13/#22, dream-loop pipeline
(scene 4.5/10, **Tier-2 ceiling — blocked by greybox asset fidelity, NOT lighting**).

## Current Git Branch
`dev`

## Current Commit
`6409eaf` — `feat: WORLD-CAMERA mouse/keyboard camera control (look signs verified)` (latest pre-town).
`dca6a34` — `docs: WORLD_DESIGN area inventory + design + world Kanban`.
`c46c43d` — `test: NSFW-GATE-002 ...`. `427ff5d` — WORLD-INPUT. `67f4e19` dream-loop. `590cf73` UI-002.
`454c7fc` PERF-001. `c7417cb` BUG-001. `1a43379` NPC AC #10. `87400b6` Wood-PBR.
WORLD-TOWN-001 (this slice) is staged below once committed.

## Completed Recently
0. **WORLD-TOWN-001 (town + shop counter → existing economy)** — `scenes/Town.tscn` + new
   `WorldCommandKind.ShopBuy` → `GameRootCommandDispatcher` → `GameRoot.TryBuyItem` (generation-
   guarded, calls the single `Shop.Buy`). 16 new smoke assertions incl. fail-closed (broke = no
   mutation) + stale-generation rejection + scene-liveness. Build 0/0, full smoke **1373 PASS**.
1. **WORLD-CAMERA (AC #6)** — mouse (RMB-hold relative look, **signs verified**) + Q/E + wheel zoom
   + right-stick look; camera rotation was previously **dead code** (actions registered, never bound).
   Committed `6409eaf`.
2. **WORLD_DESIGN** — `docs/WORLD_DESIGN.md`: original area inventory (9 facilities + Town +
   Adventure + Training/Bath) → remake equivalents + status; world Kanban (WORLD-TOWN-### etc.).
   Key finding: original Town/Adventure exist only as management UI, not 3D areas. Committed `dca6a34`.

## Systems Currently Working
- C# simulation: RanchService, BondService, ScheduleService, EconomyService, DayCycle, Save
  (SchemaVersion 14), DataRegistry, ERA import — all intact.
- 3D world: `RanchGreybox.tscn` (barn, fences, trees, well, ground, walls, PBR, sun,
  world environment), `WorldCameraRig`, `ThirdPersonPlayerController` (keyboard **and
  gamepad left stick** + camera-relative + F/A-interact), `WorldInputBootstrap` (runtime input
  map — keyboard + gamepad buttons), `WorldCameraRig` (right-stick look + zoom).
- NPCs: roster avatars placed via `RosterRig` (roster=2 in a fresh game), now **interactable**.
- Soft-anime shading (opt-in per avatar), GPU-verified soft + no hard corners.
- Management UI (UiShellController) available in-world ("Open Management UI" button) — same
  simulation, efficient path.

## Systems Currently Broken
None known. (See Known Problems for gate-blocked + unverified items.)

## Files Changed (uncommitted)
None — working tree clean (all slices committed; see Current Commit).

(Recent commits: `427ff5d` WORLD-INPUT gamepad; `c46c43d` NSFW-GATE-002 dispatch negative tests;
`67f4e19` dream-loop capture+judge pipeline; `590cf73` UI-002 prompt; `454c7fc` PERF-001;
`c7417cb` SOFT-ANIME-001 + BUG-001 + GPU tools; `1a43379` NPC AC #10; `87400b6` Wood-PBR.)

## Assets In Progress
- `assets/3d/` — 14 GLB props + PBR maps (barn, fence, tree, well, hay, trough, path stones,
  pasture boundary, grass field, wood bakes, bark/cobblestone bakes). All committed.
- Soft-shader materials (runtime, built by `SoftMaterialFactory`).

## Tests Performed
- `dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj` → **0 warnings / 0 errors**
  (verified after WORLD-INPUT — the compiler confirmed `JoyAxis`/`JoyButton`/`Input.GetJoyAxis`
  exist in the GodotSharp 4.7.0 binding; `JoypadAxis`/`JoypadButton` do NOT).
- `python Tools/Godot/launch.py --mode smoke` → **SMOKE PASS, 1354 assertions** (1341 after
  NSFW-GATE-002; +13 gamepad input assertions for WORLD-INPUT).
- **NSFW-GATE-002** — 26 dispatch-boundary negative assertions (Minor/Unknown/Ambiguous deny
  `PerformAction`; ConfirmedAdult passes the eligibility gate) in `TestAdultEligibilityGate`.
- **WORLD-INPUT** — 13 assertions: 8 world actions registered + 5 gamepad button bindings present
  in the InputMap (A/Start→interact, B→recenter, L1/R1→zoom).
- **PERF-001 real-GPU baseline** — `bash run_perf.sh` → 120 sampled frames: 4,143 draw calls,
  393,403 primitives, 5,553 objects, 2.44 GB video memory, 2,833 nodes, ~320 FPS engine cap.
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
1. **CHAR-002 / ART-002 (AC #8, AC #12) — gate-blocked.** `data/characters.json` has **no
   `AdultEligibility` field**, no ConfirmedAdult character, no independent design review.
   Roster contains minors (Slay 13, Maria 15, minor-coded Ayaka). **Will not generate
   explicit character art/text for this roster** until a clearly-adult character (candidate
   Noir, 26) clears independent human design review (`ADULT_CHARACTER_VALIDATION.md`).
   Non-adult world art + fail-closed gates + gate-safe data model are all in place.
2. **MORPH-001 / ANIM-001** — blocked on CHAR-002/ART-002 (no rigged/morphed character yet).
3. **Visual end-quality of soft shading** — technically GPU-verified (A/B measured +
   vision-confirmed soft, no hard corners); final aesthetic sign-off (whether the soft look is
   "anime-natural" enough at gameplay camera) still needs a human design review, especially once
   a real character model exists.
4. **Avatar grounding in the full world** — avatars near the tree read as floating (no contact
   shadow) in the close-up; a contact-shadow / grounding pass is a candidate ART polish item.
5. **PERF-001 frame-delta caveat** — the baseline's frame-delta (72 ms) is OS-compositor-throttled
   (isolated capture window is not foreground). The stable, decision-relevant signals are the
   GPU-cost metrics (draw calls / tris / video mem). Re-measure in a foreground window before
   any LOD/instancing decision. See `docs/PERF_BASELINE.md`.
6. **WORLD-INPUT analog-stick sign is live-only** — the gamepad left/right stick direction signs
   (`-LeftY` = forward, `-RightY` = look up) follow the standard Godot convention but **cannot be
   verified headlessly** (no physical joypad in CI; this binding lacks `Input.PushEvent`/
   `ParseInputEvent`). The **button** mapping is headless-verified (13 smoke assertions). Confirm
   stick direction with a real controller before marking controller support fully verified.
7. **Dream-loop visual ceiling** — the ranch world scores **4.5/10 (Tier 2)** under the
   `vision_analyze` judge. Tier 1 (shape) PASS, Tier 2 (light/color) near-ceiling, **Tier 3
   (asset fidelity) is the blocker**. Real on-disk state (verified): **8 of 13 GLBs already
   carry baked PBR** (fence, well, tree, tree_broadleaf, signpost, pasture_boundary, path_stones,
   crates — each ~1.8–4 MB with albedo+normal+ORM maps) + the grass field uses `grass_field.tres`
   PBR. The texture-less ones are the small props: **barn (15 KB, 0 textures)**, hay_bale, hedge,
   water_trough, grass_tufts. All are low-poly (68–240 tris) so even the PBR'd ones read flat in
   the wide capture. Lighting alone cannot push past this (anti-loop stopped after 3 rounds
   4.0→4.5→4.5). Next lever = **barn texture + higher-poly hero props**, or a cleared concept
   reference — the barn being the single most visible texture-less element.

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
1. **Verify the gamepad stick direction live** (only thing that can't be checked headlessly) —
   attach a controller, confirm left-stick = forward/strafe and right-stick = look feel correct;
   flip the sign in `ThirdPersonPlayerController`/`WorldCameraRig` if inverted. (Button mapping
   is already headless-verified.)
2. Then the next unblocked value by priority: **barn texture** (the single most visible
   texture-less element — the 15 KB barn is the anchor of the whole ranch; it has zero PBR while
   8 sibling props already do) to break the dream-loop 4.5/10 Tier-3 ceiling, OR a P0/P1 system
   card if one is higher priority than art.
   **CHAR-002 / ART-002** (AC #8/#12) is the big remaining gap but **gate-blocked** until a
   clearly-adult character clears independent design review — do NOT generate explicit adult
   art for the current roster (see Known Problems #1).

## Next Recommended Tasks
- **BARN-001 (art, unblocked)** — author a painted-wood-siding PBR map for the barn (the 15 KB
  anchor prop has zero textures while 8 sibling props do) and bake it into `barn.glb`; the single
  highest-impact fix toward the dream-loop 4.5/10 Tier-3 ceiling. Non-adult asset.
- **WORLD-INPUT live-verify** — confirm gamepad stick direction with a real controller (buttons
  already headless-verified); flip signs if inverted.
- **AVATAR-001 (polish)** — contact shadow / grounding for placed avatars (read as floating near
  the tree in the close-up).
- **AC #8 / CHAR-002 / ART-002** — ONE clearly-adult master character (candidate Noir) through
  the full production pipeline, **only after** independent design review clears
  `ConfirmedAdult` + `VISUALLY_UNAMBIGUOUSLY_ADULT`. Gate-safe fail-closed state is already in
  place. (Stop condition: this requires a human design-review decision the agent cannot make.)
- **MORPH-001 / ANIM-001** — gameplay→visual morph curves + shared rig/animation, once CHAR-002
  is unblocked.

## Useful Commands
- **Build:** `cd OpenMakaiRanchGame && dotnet build OpenMakaiRanchGame.csproj` (expect 0/0).
- **Smoke (headless):** `python Tools/Godot/launch.py --mode smoke` (NEVER run raw smoke against
  personal saves — slot 99 is overwritten).
- **GPU render capture (whole world):**
  `E:\GodotEditor\Godot_v4.7.2-stable_mono_win64.exe --path OpenMakaiRanchGame res://scenes/dev/WorldCapture.tscn`
  → saves `C:\Users\noa3\AppData\Local\OpenMakaiRanchCapture\world_capture.png`.
- **A/B shading test:** `... res://scenes/dev/ShadingAB.tscn` → `shading_ab.png`.
- **PERF-001 GPU baseline:** `bash run_perf.sh` (isolated profile) → `PERF_STATS` lines +
  `PERF_STATS_OK`; or `... res://scenes/dev/PerfCapture.tscn` directly.
  (Isolated-profile env: set `APPDATA`/`LOCALAPPDATA`/`OMR_EXPECTED_USER_ROOT` to a temp dir so
  the personal save profile is untouched.)
- **Editor:** `python Tools/Godot/launch.py --mode editor`. **Isolated playtest:**
  `python Tools/Godot/launch.py --mode runtime --isolated`.
- **Blender (headless):** `D:\SteamLibrary\steamapps\common\Blender\blender.exe --background --python <script>`
  → `gltf` GLB export (ART-001a path).

## Resume Instructions
1. `git status` — working tree should be **clean** (WORLD-INPUT + NSFW-GATE-002 committed).
2. Build (0/0) + smoke (expect **1354 PASS**).
3. Continue: **BARN-001** (author a barn PBR map — the highest-impact unblocked art fix) and/or
   **WORLD-INPUT live-verify** (confirm gamepad stick direction on a real controller).
   **CHAR-002 / ART-002** stays **gate-blocked** — do NOT generate explicit adult art for the
   current roster until independent design review clears a clearly-adult character (see
   Known Problems #1).
4. Do NOT re-introduce a hard toon/cel shader or a hand-rolled `LIGHT` dot product — the user's
   direction is soft/natural, and `LIGHT` in `fragment()` fails GPU compile.
5. GodotSharp 4.7.0 binding: use `JoyAxis`/`JoyButton`/`Input.GetJoyAxis` (NOT
   `JoypadAxis`/`JoypadButton` — those do not exist in this version).
6. Keep the ERA source read-only; never delete user save files; keep the simulation as the single
   source of truth (3D is presentation).
