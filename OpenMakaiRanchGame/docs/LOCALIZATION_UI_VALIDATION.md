# Localized world UI and playability validation

Checkpoint **2026-09-11**; PR #15, branch `feature/world-stations-and-interiors-20260911`. Verified branch head **`fc6994a2c36fd0345319499ee8dbb6510aa77967`**. Documentation-only updates may follow. Existing C# 12, net8.0, SDK 10.0.401, Godot 4.7.2 Mono, schema 16 and original read-only data remain unchanged.

## Executed result

| Workflow | Run | Result |
| --- | --- | --- |
| Build Smoke Check #639 | `34609768470` | success |
| Godot 4.7 Mono CI #631 | `34609768598` | success |
| Rendered UI acceptance #69 | `34609768503` | success |

These are PR-triggered checks of synthetic merge **`193a690066db97c240c789613f0650da32c87557`**, corresponding to branch head `fc6994a` against main `3209f4c`. The reviewed-source receipt contains that exact merge SHA. No claim that unrelated later code was checked.

- **55 Python tests pass**, including seven new catalog tests. The same 55 were run locally on the downloaded exact review source. `validate_locales.py` reports **200 valid matching English/German UI keys** and checks export filters.
- **1,740 SMOKE OK / zero SMOKE FAIL / one SMOKE PASS**. Smoke console: `.artifacts/godot/smoke-58poyaj0/console.log`.
- **408/408 rendered checks pass**, **53 PNG captures**, one UI ACCEPTANCE PASS and zero UI-runtime ERROR/SCRIPT ERROR. Results: `godot/ui-k1x0nr88/results.json`; runtime console in the same directory.
- Compilation succeeds, but the logs retain developer-local NuGet source warnings and the existing nullable warning at `RanchLeisureFrameTests.cs:215`. Do not describe this checkpoint as zero warnings.

## Receipts and inspection

The actual downloaded ZIP bytes match these independently calculated SHA-256 values:

| Artifact | ID | SHA-256 |
| --- | --- | --- |
| Godot import/smoke verification | `10267223952` | `6e4c20d90f1d1a01871f3eff76596faca94eb824a1fac4e4f278c7fc48178f2f` |
| Rendered UI/source/screens | `10267906900` | `2ba80ea03d70f8de338027362ce4f76006bf906c003c97eed3e5b6b7761f035c` |

Opened and visually inspected final `title-german-640x480.png`, `places-german-480x800.png`, `dairy-german-640x480.png` and `walk-in-dairy-960x540.png`. The German images actually display German text, not just German filenames. The station footer remains visible while longer text and worker choices scroll. The doorway image proves only the selected prototype view and also shows remaining large 3D-label/HUD clutter; final world presentation is not complete.

Smoke includes exactly five deliberate invalid-save ERROR diagnostics; no GCHandle/native fatal remains in the inspected final smoke console. Import exits successfully but still logs the known EditorSettings shutdown diagnostic (`import-hqhxxdvs` for smoke, `import-43h8g2l6` for UI). Software rendering retains its VSync warning. No new log filtering was used to manufacture a pass. The review archive is whitelisted source/data/catalogs, not a full standalone project or exported game. Evidence artifacts expire and can be regenerated with the isolated launchers.

## Gameplay defects repaired

**Held input:** live prompt calls used to reapply the complete InputMap on every binding read, erasing held movement events. The loaded guard now makes ordinary reads inert; explicit rebinding/reload remains available. The rendered fixture retains the exact held W event and pressed state across ten prompt reads, then uses real physics frames to walk through the barn doorway. No walking stamina charge or scene teleport substitutes for movement.

**First-day barn construction:** the tutorial previously requested staffing without explaining that Dairy Barn must first be built. Text now explains Build and its displayed price, then a different resident so Pasture stays staffed. The frame walkthrough performs the real Build command using starting funds, checks the actual facility price and proceeds through assignment without a fixture unlock or early work payout.

**Physical supply planning:** the quiet corner/community board may route its visible planning action to the read-only Places guide even though global management shortcuts are disabled. It marks the Office destination; it does not remotely assign work, teleport or grant supplies. Existing hidden/reentrant/stale-origin guards remain.

**Language side effects:** the existing SetLocale path reapplied graphics through broad settings synchronization, potentially changing window resolution/mode. Language now uses its own apply/persist path. The actual title picker check preserves physical window size/mode; state snapshots retain session/generation/day/gold. Options-to-world additionally retains area, horizontal position and the selected destination ID. Open station refreshes retain context/focus and do not change inventory, stockpile, assignments or stamina.

## Translation and title scope

LocaleCatalog remains the shared system. It merges the new `locale/ui/en.json` and `de.json` slice with existing catalogs. English fallback, native chooser names and supported regional normalization retain a usable interface. Formatting uses the display locale without changing global CultureInfo.CurrentCulture; save parsing, numeric simulation and identifiers are not localized.

LocaleTemplate now validates argument sets as well as syntax before formatting. A dropped price/name, extra index, malformed brace or alignment larger than the bound falls back to English. Reordered arguments remain supported. Static validation covers duplicate/missing/blank/null/non-text entries, file size, key parity, placeholder syntax/set parity, literal world-key references and raw JSON export inclusion in all five presets.

Physical station, resident/house view, destination guide and warning strings use complete keyed templates and canonical IDs. The new title diorama owns its own bounded viewport/world/camera, never loading or advancing the live session. Reduced Motion stops its camera; German main actions/picker fit 640x480 and 480x800. The old full character creator has not been converted into a stepwise wizard. Japanese new-key fallback is not Japanese translation completion; many story/service/command-result messages are still English. RTL, script/font coverage and grammatical plural rules need separate work.

## What the rendered input test actually does

ChooseLanguage sends balanced mouse press/release events and navigation keys through **Input.ParseInputEvent**, with the owning window ID and buffered-event flushes. It opens the actual OptionButton popup, uses Down until the real focused index matches the requested locale, and activates Enter. Assertions require both catalog and stored locale to change. It never emits ItemSelected or directly calls SetLocale for these two selection checks. Separate open-panel refresh and cleanup intentionally call SetLocale and are not described as picker interaction.

This matters because earlier attempts were wrong: viewport-only mouse injection omitted global held-button state used by popup opening-release handling; emitting WindowInput bypassed the native window handler; Home was not a popup navigation command. Those fixtures left the title in English or retired the options picker on its opening click. Their red runs are not evidence of a translated UI. The final code uses engine dispatch and stops immediately on an unopen/retired picker, rather than continuing to capture mislabeled screens.

At `f26b605`, 385 checks included four failures (English selection/title and retired picker). UI artifact `10267805958` is retained as failed evidence. At `2180989`, compilation separately caught an unavailable Input.ReleasePressedEvents test API; it was removed, leaving explicit paired releases. No runtime check was disabled to obtain the final pass.

## Retained world and opening coverage

The same final run includes the non-erasing prologue Continue, playable bedroom Wake up, utility Options return to the same bedroom/position/stage, earlier night/report, tactical battle and shop/save journeys. The station continuation checks camera-relative cardinal directions, bounded warning markers, ID-only tracking, human-scale door clearance, collision-derived navmesh, unblocked doorway/blocked adjacent wall rays, actual held-key traversal and roof cutaway. Actual station Build and Assign clicks spend the canonical price/change the shared schedule once, resolve the dairy staffing warning and reject a retired or remote action.

The UI fixture deliberately uses synthetic Day-2 state/strong stats before the night/combat/station sequence. Proximity is staged for translated station inspection separately from the physically walked doorway. These are not an uninterrupted organically earned full playthrough, physical-device/desktop automation, combat balancing, all-building navigation, accessibility certification or Forward+ hardware performance tests. The separate frame walkthrough retains resource-backed first-day/leisure behavior. Packed-scene retention and finalization changes are smoke teardown only, not ordinary gameplay memory management.

## Reproduction

```bash
python Tools/Godot/validate_locales.py
python -m unittest discover -s Tools/Godot -p "test_*.py"
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/ui_acceptance.py --rendered
python Tools/Godot/launch.py --mode runtime --isolated
```

Use isolated profiles: automated fixtures write/delete disposable slots 99 and 3. Never pass raw test flags against personal saves. Remaining work and priorities are in ASTRA_HANDOFF.md and KNOWN_ISSUES.md; translation authoring rules are in TRANSLATION_GUIDE.md.
