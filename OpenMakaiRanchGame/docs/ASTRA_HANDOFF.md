# ASTRA Handoff

Checkpoint **2026-09-11**, verified code head **`67a052806e9241f68110e13494e944c90bac6203`**: **1,778 passing smoke assertions, 432/432 rendered UI checks, 55 Python tests and 56 PNGs**. All three CI workflows succeeded. **RANCH_DESIGN_VALIDATION.md** records the current exact runs, source receipt, inspected screenshots and independently checked archive hashes. Documentation-only commits may follow. LOCALIZATION_UI_VALIDATION.md retains the preceding fc6994a checkpoint rather than being rewritten as evidence of later work.

## Branch and requirements

- Continue on **`feature/world-stations-and-interiors-20260911`**, **PR #15**, repository `Noa3/OpenMakaiRanch`. Main was last checked at merged PR #13 `3209f4c`. PR #14's test-only intro baseline is included and repaired here; do not merge it separately.
- Read live Git before editing. Preserve concurrent changes, no force push or automatic merge. Native language-popup fixes, their docs and the shared-evening profile-setting fixture correction were preserved. Temporary exact-preimage patch helpers remove themselves, not normal runtime/workflows.
- **C#12**, SDK **10.0.401**, target **net8.0**, **Godot4.7.2 Mono**, save schema **16**. Separate contracts; no version upgrade. Fresh games are expected (D-011), current saves must work, personal files must not be deleted. Original reference content stays read-only.
- GameRoot/services remain the sole simulation/economy authority. No explicit assets, source identity relabeling or production character eligibility approval was added.
- User requires translation-aware UI and a later ten-target plan excluding Russian. Read TRANSLATION_GUIDE.md and LOCALIZATION_ROADMAP.md. Current en/de/ja availability is not complete translation; future Chinese scripts/locale variants need explicit normalization support.

## Current design implementation

**Player goals:** Places opens four optional projects: restore the quiet corner, kitchen level2, three community deliveries and first successful expedition. Purpose/progress/one next destination derive from existing state and durable receipts. No new reward authority, timers, remote assignment, teleport or victory condition. Missing repair supplies direct to Office Work. GAME_DESIGN_DIRECTION.md distinguishes these implemented steps from a proposed first-week chapter, resident aspirations and celebration.

**Shared evenings:** voluntary adult companionship at the ranch house after introduction, at Night with Rest selected, an existing romantic bond and a well companion. State and source definition eligibility both remain required; no pressured/forced route. Invitations are free/cancellable. Successful ordinary settlement records one bounded receipt and +1 Bond/+2 Morale, not another bath/rest/energy payment. The existing transition shows a black screen, heart and non-explicit morning message with Reduced Motion support. Pending/completed plans survive save/load. No pregnancy/family outcome exists; AUTHOR_CONTENT_HANDOFF.md identifies separate authoring and safety requirements. Production characters remain subject to individual review; test identities are synthetic.

**Building space:** eight fixed metre-scale plots protect roof envelopes, entrances and the town-gate approach. Higher numerical facility levels keep bounded interior visual grades0-3, not unlimited shell growth. Upgrade cost/level overflow is rejected before payment. Imported and placeholder tree crowns are checked before placement against the same reservations. This is not free placement, annexes/upper floors, all-prop or all-route certification. BUILDING_PLOTS.md describes future expansion requirements.

**Translation:** 47 matching project/evening entries extend the English/German slice to247 keys and join literal-source validation. Full sentences, checked placeholders and display-only culture remain. No seven empty English catalogs were labeled as new translations. Japanese fallback and old English-only story/result messages still need translation work.

## Earlier implementation retained

Held movement: InputBindingService no longer rebuilds InputMap during ordinary HUD reads. Tests retain held W across hints and physically walk through a barn doorway. The first-day Dairy step explains actual Build cost before staffing and preserving Pasture staffing; frame tests use starting funds, not fixture unlocks. Corner/board supply planning routes to read-only Places without granting resources or reopening the old global hub.

The title owns a separate procedural diorama SubViewport/world/camera with capped resolution and Reduced Motion. It remains placeholder art, not a playable town river or final AAA visuals. Title and Options language pickers use actual engine-dispatched mouse/key input, window IDs and balanced releases. Language changes persist without reapplying window/graphics/audio/control settings; context, logical focus, tracked IDs and resources survive. Bounded template validation rejects missing/extra/invalid arguments and huge alignment. All five export filters retain raw data/catalog JSON.

The opening patch is integrated: non-erasing text completion, same-bedroom utility return, guarded first-day callbacks/navigation, dialogue layout and forward-facing outdoor movement. Physical station/resident panels and Places replace the player-facing global hub; some service renderers remain context-restricted rather than deleted.

Walk-in shells have metre-scale door clearance, segmented wall/furniture collision, shelter, visual roof/wall cutaways and collision-derived navigation. Selected rays, actual doorway walking, Build/Assign, resolved warnings and remote/stale denial pass. Large labels and HUD/tutorial overlap remain visible. PR #13 night growth/ledger/planning/reentrancy, adventure costs/results and save/load remain in DAY_LOOP_VALIDATION.md and CORE_PLAYABILITY_VALIDATION.md.

## Current verification and limits

Verified head67a0528: Build #644 `34611892143`, Godot #636 `34611892078`, UI #74 `34611892077`: all success. PR merge **`441f0730c4b507d592fad3aa16353d1bce3bca4c`** appears in the exact review-source receipt. **1,778 SMOKE OK / zero FAIL**, **432 passing rendered checks / zero UI runtime errors /56 captures**, **55 Python tests**. Thirty-eight new numeric assertions and24 rendered checks/three captures extend the fc699 baseline. RANCH_DESIGN_VALIDATION.md holds the artifact hashes and scopes.

Smoke retains five intentional invalid-save diagnostics; import logs the known EditorSettings shutdown diagnostic and software rendering its VSync warning. Build success is not warning-free certification: known developer-local NuGet source and nullable warnings remain. Test-only scene retention/teardown is not normal-game GC or a universal lifecycle fix. Shared-night test definitions, Day-2 resources and strong stats are explicitly synthetic, not organic balance/romance or production approval. The final night fixture uses SetReducedMotion to preserve profile settings across a real load and restores the preference afterward.

Next prioritize an earned first-week playthrough and content payoff, then the localized stepwise creator with retained preview/Back/Next/Start, further service/result-message localization, world label/HUD density and real NPC/camera traversal. Organic town/river, final authored stages/assets, full languages/RTL/scripts/plurals, exported builds, physical input devices, Forward+ hardware performance, long-term balance and original-engine parity remain open. No family-state machine has been added.

## Safe commands

```bash
python Tools/Godot/validate_locales.py
python -m unittest discover -s Tools/Godot -p "test_*.py"
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/ui_acceptance.py --rendered
python Tools/Godot/launch.py --mode runtime --isolated
```

Use isolated launchers only: acceptance writes/deletes disposable slots99 and3, with occupied-slot checks in the new scenario. Artifacts contain review inputs/logs/screens, not a standalone exported game. Numerical KANBAN/WORK_LOG snapshots are historical; live Git and exact-head receipts take precedence.
