# ASTRA Handoff

Checkpoint **2026-09-11**. Translation/playability head **`fc6994a2c36fd0345319499ee8dbb6510aa77967`** is verified: **1,740 passing smoke assertions, 408/408 rendered UI checks, 55 Python tests and 53 PNGs**. All three CI workflows succeeded. LOCALIZATION_UI_VALIDATION.md records exact runs, source receipt, inspected screenshots and independently checked archive hashes.

**Concurrent continuation:** while recording this checkpoint, `6324763653433c22d1736ffe034c65523ce71c10` landed additional ranch projects, shared evenings, reserved building plots and more translation keys. Those changes were preserved. The documentation update follows that commit without resetting the branch. The 200-key / 408-check receipts below describe the verified fc699 slice, not an assertion that those additional gameplay changes were tested in the same run. Consult live CI for their later status; their initial workflows required action rather than executing. Documentation-only updates after 6324763 do not remove its code.

## Branch and requirements

- Continue on **`feature/world-stations-and-interiors-20260911`**, **PR #15**, repository `Noa3/OpenMakaiRanch`. Main was last checked at merged PR #13 `3209f4c`. PR #14's test-only intro baseline is included and repaired here; do not merge it separately.
- Read live Git before editing. Preserve concurrent changes, no force push or automatic merge. One-use exact-source patch helpers remove themselves; do not retain them as normal workflows.
- **C#12**, SDK **10.0.401**, target **net8.0**, **Godot4.7.2 Mono**, save schema **16**. These are separate contracts and were not upgraded. Fresh games are expected (D-011), current saves work, personal files must not be deleted. Original reference content stays read-only.
- GameRoot/services remain the sole simulation/economy authority. No adult-specific assets or character eligibility approvals were added.
- User requires translation-aware UI. Read TRANSLATION_GUIDE.md: use existing LocaleCatalog, complete keyed sentences, stable command IDs, adaptable layouts, English fallback and safe language-only setting changes. A selectable language is not a claim of complete translation.

## Verified implementation at fc6994a

**Held movement:** InputBindingService stops rebuilding the InputMap during ordinary HUD reads. Tests retain the exact held key event and pressed state across repeated hints, then physically walk through the barn doorway.

**Guided construction:** the first-day Dairy step now explains Build and its displayed price before assignment, and a different resident to preserve Pasture staffing. Frame tests build using actual starting funds and canonical cost, without a fixture unlock or early production payment.

**Physical supply planning:** visible corner/community-board planning can reach the read-only Places guide while global management shortcuts remain restricted. Marking the Office does not assign work, grant resources or teleport. Existing hidden/stale/reentrant guards remain.

**Translation:** 200 matching English/German entries in the initial slice cover the title, physical station panels, destinations, direction hints and ranch warnings. Existing Japanese text remains, with English fallback for missing new keys. Locale normalization/native names and display-only formatting do not change global culture, save parsing or command IDs. Bounded template checks reject missing/extra/invalid slots and huge alignment; valid argument reordering works. Validator tests and all five export filters cover raw JSON inclusion.

**Safe switching/layout:** SetLocale persists only language rather than reapplying window mode, resolution, audio or controls. Real title and Options pickers are selected with Input.ParseInputEvent, window IDs and balanced mouse/key events, not direct ItemSelected. Separate refresh tests intentionally call SetLocale. German title/Places panels fit 640x480 and 480x800; Dairy fits 640x480. Open station context and stable focus, destination IDs, session/day/resources remain intact.

**Main menu:** a small procedural ranch diorama owns a separate SubViewport/world/camera and capped rendering resolution. Reduced Motion stops camera drift. Title/actions have clearer styling and wrap translated labels. This remains placeholder art, not final AAA presentation or the playable organic town/river.

## Earlier work retained and now exercised

The formerly downloadable opening patch is integrated: non-erasing typewriter completion, same-bedroom utility return, guarded first-day callbacks/navigation, dialogue layout and forward-facing outdoor movement. The player-facing global hub is replaced by physical station/resident entry points and Places; service renderers are context-restricted, not entirely deleted.

Walk-in shells have metre-scale door clearance, segmented wall/furniture collision, shelter and visual roof/wall cutaways; ranch navigation is baked from collision. Selected rays, actual doorway walking, Build/Assign, resolved warnings and remote/stale denial pass. This is not proof of every route, NPC recovery or camera angle. Large world labels and overlapping HUD/tutorial elements still need polish.

PR #13 night growth/ledger/planning/reentrancy, adventure costs/results and save/load remain; historical receipts are in DAY_LOOP_VALIDATION.md and CORE_PLAYABILITY_VALIDATION.md.

## Verification and remaining limits

fc699 workflows: Build #639 `34609768470`, Godot #631 `34609768598`, UI #69 `34609768503`: all success. PR CI checked merge `193a690066db97c240c789613f0650da32c87557`, recorded in the source archive. Smoke: 1,740 OK, zero FAIL, five intentional invalid-save errors and no GCHandle/native fatal. UI:408 OK, zero runtime errors,53 PNGs. German title/Places/Dairy and walked doorway images were opened and inspected.

Build succeeds but is not warning-free: CI lacks the developer-local NuGet source and RanchLeisureFrameTests retains CS8602. Import still logs the known EditorSettings shutdown diagnostic; software rendering warns about VSync. Smoke-only scene retention/teardown is not normal-game GC or a universal lifecycle fix. The later UI sequence uses synthetic Day-2 resources/strong stats; only its named paths are established.

Next finish the localized stepwise creator with retained preview and stable Back/Next/Start. Extend physical service/result-message translation, improve world label/HUD density and actual NPC/camera traversal, then organic town/river landmarks. Full language coverage, RTL/scripts/plurals, actual exported builds, physical input devices, Forward+ hardware performance, long-term balance and original-engine parity remain open. Review the concurrent design work's GAME_DESIGN_DIRECTION.md, LOCALIZATION_ROADMAP.md and BUILDING_PLOTS.md without equating them to the earlier verified slice.

## Safe commands

```bash
python Tools/Godot/validate_locales.py
python -m unittest discover -s Tools/Godot -p "test_*.py"
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/ui_acceptance.py --rendered
python Tools/Godot/launch.py --mode runtime --isolated
```

Use isolated launchers only: acceptance writes/deletes disposable slots 99 and 3. Artifacts contain review inputs/logs/screens, not a standalone exported game. Numerical KANBAN/WORK_LOG snapshots are historical; live Git and exact-head receipts take precedence.
