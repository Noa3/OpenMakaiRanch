# Known Issues

Updated **2026-09-11** for **PR #15**, `feature/world-stations-and-interiors-20260911`. Verified localization/playability checkpoint **`fc6994a2c36fd0345319499ee8dbb6510aa77967`**: **1,740 smoke assertions,408/408 rendered checks,55 Python tests,53 PNGs**, all CI workflows successful. LOCALIZATION_UI_VALIDATION.md records exact evidence.

**Later code is distinct:** concurrent `6324763653433c22d1736ffe034c65523ce71c10` adds projects/shared evenings/building plots and more locale entries. It was preserved, not overwritten with the older tested tree. Its initial CI required action; the fc699 counts do not certify those later additions. Consult live runs for their status. PR #13 is merged; PR #14's old test-only baseline is included and repaired by this branch.

## Open issues

### TOOLS-003 — warnings and lifecycle

Builds succeed but retain warnings for the unavailable developer-local NuGet source and nullable dereference at RanchLeisureFrameTests.cs:215. Import exits successfully with the known EditorSettings shutdown diagnostic; software-rendered UI warns about VSync. The final fc699 smoke has exactly five intentional invalid-save diagnostics, zero failed assertions and no GCHandle/native fatal; its UI runtime has zero ERROR/SCRIPT ERROR. Smoke-only PackedScene retention and staged teardown are not normal-game GC or proof of every engine shutdown path. Keep diagnostics distinct and do not hide them to manufacture a pass.

### I18N-001 — partial translation

The initial 200-key English/German slice covers title, physical stations, Places, destinations/directions and ranch warnings. Existing Japanese entries remain; new missing entries fall back to English. Concurrent design work extends these catalogs, but does not complete story, character creation, retained service menus or command-result/status messages. Grammatical plurals, RTL, additional scripts/font coverage, subtitles/voice and language-specific physical-device checks remain open.

Actual title/Options language choices, unchanged window settings and stable open-station context/focus/destination IDs pass at fc699. All five export filters include raw data/catalog JSON, but an actual exported-build localization playthrough remains required. Use TRANSLATION_GUIDE.md; never translate persistent IDs or player-entered names.

### UI-PLAY-001 — creator and presentation

The character editor is not yet the requested stepwise creator. Main menu now has a private procedural ranch diorama, localized caption and revised action styles, but placeholder art remains and the panel covers much of a small viewport. Do not call it final AAA presentation.

Physical station/resident panels and read-only Places replace the global player-facing hub. Some services still use context-restricted retained renderers. Large 3D labels, warning details and HUD/tutorial overlap can clutter a doorway view. These are visible polish defects despite passing selected geometry/hit-target tests.

An uninterrupted organically earned first-day/skip/world/courier/leisure/save journey, every service action and mission outcome, physical keyboard/controller/touch/Alt-Tab/deadzones and camera feel are not complete. Later UI fixtures use synthetic Day-2 resources/strong stats; staged proximity is not traversal evidence.

### WORLD-NAV-001 — walk-in prototypes

Metre-scale building shells now have real doorway openings, wall/furniture collision, visual roof/wall cutaways, shelter and a collision-derived navigation bake. Selected rays and actual held-key player walking pass without travel or stamina tax. All doorways, larger character variants, furniture clearances, camera angles, NPC avoidance/stuck recovery and full world routes remain unaudited. The leisure bench lacks its final collision and seated animation.

The title's water strip is not a playable organic town river/bridges/landmarks. Final authored buildings/nature/character/morph assets, weather readability, LOD/memory budgets and representative-hardware Forward+ low/high performance remain open. Concurrent plot-layout changes need their own current-head acceptance. No external asset or character eligibility/design approval was added.

### GAMEPLAY-BALANCE-001 — selected rules, not all progression

The first-day barn now explains and exercises real starting-fund construction before staffing. Held input survives prompt reads. Physical supply planning marks Places without granting resources. Existing one-pass night training, growth markers, editable plans, separate bath bonus and once-daily community delivery remain. Quiet-corner restoration remains 40 G/3 supplies and up to10 daily stamina recovery, with no time advance/upkeep. Full upgrade/win paths and long-term economic/stamina balance are unverified. Additional projects/shared-evening code after fc699 is not covered by the earlier checkpoint merely because it uses the same branch.

### SETTLEMENT-BOUNDARY-001 — unchanged limits

Captured time commands require a Night plan and reject stale/replayed/reentrant transitions. Completion observers cannot settle tomorrow or attach old reports to a replaced session. Raw EndDay retains explicit simulation-call compatibility, not universal idempotency. Exception rollback, multithreaded transactions and exhaustive upstream overflow/accounting remain unaudited. Daily ledger/display repairs do not create a second economy or certify every producer's extreme arithmetic.

## Repairs and audit leads to preserve

LOCALIZATION_UI_VALIDATION.md records the real blank-prologue/bedroom-return, held-input/construction, physical planning, local Build/Assign/Back, stale/remote denial, directional warnings, selected doorway/cutaway and locale/focus repairs. It also distinguishes failed synthetic popup input attempts from actual engine-dispatched selection; earlier screenshots with German filenames but English text are not translation acceptance.

PR #13 card/focus/night-growth/ledger/planning, adventure and actual Save/Load receipts remain in CORE_PLAYABILITY_VALIDATION.md and DAY_LOOP_VALIDATION.md. Prior responsive scaling, active HUD ownership, camera/input-capture, pre-mutation save rejection, voluntary activity guards, receipts and stored-mana rules remain. None of the original-source or eligibility gates were relaxed.

Importer recovery/provenance, complete runtime JSON/reference validation, original-engine differential parity and development MCP authentication/request-size/export hardening still need dedicated work. Preserve current-schema personal files and read-only original data. ASTRA_HANDOFF and live Git supersede stale numerical KANBAN/WORK_LOG snapshots.
