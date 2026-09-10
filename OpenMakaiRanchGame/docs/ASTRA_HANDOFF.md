# ASTRA Handoff

Checkpoint: **2026-09-10**, code **`40daf7560a53b66d8210989f695bfb3e3a8ec692`**. Canonical documentation lives in `OpenMakaiRanchGame/docs/`. The isolated full suite is now green; this is not a finished-game, rendered-playtest or enjoyment certification.

## Current objective and branch

The user explicitly requested **C# 12** and continued work on the game. This continuation improves actual movement, camera and first-day input behavior, closes the three stale smoke failures, and adds a frame-driven first-day walkthrough rather than another disconnected feature menu.

- Working branch: `playability/csharp12-controls-20260910`, PR **#11**, base `main`.
- Base checkpoint: `24f17171fca1f634fa6be5b9b399621408711885`. PRs #8 and #10 were merged before this continuation; older notes describing them as pending are historical.
- Verified code: `40daf7560a53b66d8210989f695bfb3e3a8ec692`. Documentation commits can follow without changing the tested code. Inspect the live branch before writing; do not force-reset concurrent work or merge without review.
- **Source language: C# 12 (`LangVersion=12.0`)**. Root Directory.Build.props supplies the default; Directory.Build.targets rejects other effective language versions before CoreCompile. The game project explicitly pins 12.0. Never use latest/preview to bypass compilation errors.
- Existing SDK **10.0.401**, game target **net8.0**, Godot package configuration and verified engine **4.7.2 Mono** remain unchanged. SDK, runtime target and C# language version are separate choices.
- Save schema remains **16**. D-011 still applies: fresh games are expected; no new legacy-save compatibility work. D-012 records the C# 12 requirement.

## Implemented and tested

**Movement:** configured circular InputMap deadzones, continuous analog/touch magnitude and bounded keyboard diagonals. Normalizing direction no longer turns a small stick tilt into full-speed movement. Movement takes its basis from the current main viewport camera, not a recursive search that can select a preview SubViewport or retain another area's camera. UI ownership, pause, disabling and focus loss immediately clear horizontal drift and held touch/sprint state while preserving vertical gravity. A gate reset cannot override actual application focus loss.

**Camera:** the active, visible, processing camera owns first-person mouse capture. Pause, management, focus loss, hidden/disabled areas and scene exit release that ownership. Resume checks the actual mouse mode rather than trusting a stale captured latch. An inactive ranch camera cannot release or recapture the town camera's mouse. Analog look has a dedicated angular rate, mouse-up follows the normal view convention, and Invert Y is explicit. One recenter request completes behind the player's facing direction, preserves zoom and can be cancelled by manual look; Reduced Motion uses an immediate recenter. Detached/freed targets are ignored rather than mixing local coordinates into live global transforms.

**Overlapping first-day UI:** finishing a scene fade retains an open story dialogue's input lock, and closing a dialogue retains any pending fade/management lock. The tutorial-skip route reaches the normal Night routine without paying rewards, advancing the day or draining stamina. Bath then End Day reaches the existing Day 2 report and next-day stamina bonus.

**Existing smoke corrections:** the positive gated-production fixture is now a separate synthetic age-30 character in an isolated state, with a separate Unknown-denial fixture. No production character receives approval and no runtime gate changes. Mana expectations now match the existing 2:1 rule: 20 personal / 45 stored, request 30 -> restore 22, leave 42 personal / 1 stored; the following 10 MP spell leaves 32 and still must apply its morale effect. Original assertions were retained.

**Scene lifecycle:** the greybox fixture enters the real scene tree instead of invoking _Ready outside it. The companion-target fixture uses local Position before AddChild. These fixes remove the remaining 16 out-of-tree transform diagnostics in the PR #10 baseline. The final logs contain zero such diagnostics; they were not suppressed.

## Executed verification

For code `40daf7560a53b66d8210989f695bfb3e3a8ec692`:

- **Build Smoke Check #535**, run **`34517742952`**: success.
- **Godot 4.7 Mono CI #527**, run **`34517743009`**: success, including restore, language-contract checks, compilation, 34 launcher regression tests, engine resolution/import and isolated smoke.
- CI evaluates both `OpenMakaiRanchGame.csproj` and `OpenMakaiRanch.csproj` as LangVersion 12.0. A deliberate preview override is rejected by the build guard. The primary game is compiled; this is not a claim that the separate importer builds.
- **1,547 SMOKE OK, zero failed assertions, one terminal SMOKE PASS**. Includes **39 new deterministic playability assertions** and **18 new frame-walkthrough assertions**, plus existing community/save/input/combat coverage.
- Import log: zero engine errors. Smoke log: zero out-of-tree Transform3D errors; five expected malformed/unsupported-save rejection diagnostics remain. Do not suppress those negative-fixture errors.

Evidence artifact **`10168420601`**, `godot-4.7-verification`, SHA-256 **`29d9079234d73ec4c1007a1f783e9b0231d6541f34be1e3b41f06f1b95043402`**. Smoke log `.artifacts/godot/smoke-ozygihzg/console.log`; engine log beside it. Import evidence `.artifacts/godot/import-rwxu8pqw/`.

### What the walkthrough establishes

From a fresh playable-world entry: wake-up dialogue -> real W movement over physics frames -> real F bedroom-door interaction -> ranch welcome -> real F pasture assignment -> a separate worker's dairy assignment via Schedule -> evening -> tutorial encounter with the unmodified starting party -> conversation -> prepared bath -> one day settlement -> Day 2 report -> Escape/pause -> production-backed courier delivery -> nested Back then world -> root save/load with stock, gold, story and paid receipt intact. No injected money, stock or combat stats fund this path. Ordinary movement does not spend daily stamina.

Positions near doors/stations/the intruder are deliberately staged, and buttons are activated by their live signals. This is **not** proof of obstacle-aware navigation, clicking visible rendered targets, the entire main-menu/character-creation input route, physical gamepad behavior, OS cursor behavior under a real display, or visual quality. Mouse tests establish ownership transitions, not hardware capture in the headless display backend.

### Failed attempts retained as evidence, not hidden

Run `34516503157` found long/int notification-constant mismatches in the new code/tests; explicit comparisons/casts fixed them without changing C# 12. Run `34516975557` then passed all 39 control checks and the first day, but failed Escape/board access in the frame fixture. The fixture captured Bootstrap before its deferred MainMenu route and left the actual MainMenu processing alongside the test world. Capturing and isolating the current scene after that route fixed the harness; the real Escape/Back assertions remain and pass in #527. Do not claim that this last harness correction was a newly discovered production Escape bug.

A temporary branch-only workflow applied hash-checked edits to large existing sources, using explicit staged paths and non-force pushes. It was removed in `15dddd818c400b325dcd86880383a32d1231e9ec`; it is not part of the final PR tree.

## Next work

First run a rendered first-day/skip playthrough from the real main menu in an isolated profile, with mouse/keyboard and a physical controller. Check focus/cursor restoration on pause and Alt-Tab, controller deadzones, one-press recenter, narrow-window text/buttons, travel and the day report. Inspect actual paths around obstacles rather than teleporting as the regression fixture does. Forward+ low/high quality and camera feel still need representative hardware measurements.

Then develop one bounded optional exploration/companion encounter or a visible ranch improvement using the existing commands and save authority. Do not turn the optional courier board into compulsory chores or tax walking with stamina. Rewards and long-term progression are not balance-certified. Preserve existing content/eligibility gates and the read-only original directory; this continuation did not add adult-specific content or approve character designs.

## Commands and safety

```bash
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python -m unittest discover -s Tools/Godot -p "test_*.py"
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/launch.py --mode runtime --isolated
```

Smoke writes/deletes disposable slot 99: always use the isolated launcher, never the raw smoke flag against personal saves. Preserve GameRoot as the only simulation/economy/calendar authority. Older KANBAN/WORK_LOG counts describe historical checkpoints; use the evidence above for this continuation's verification status.
