# Known Issues

Updated **2026-09-11**, **PR #13**, verified code-inclusive head **`6af8466dc3bd4338339a6c9b79ea4dd344e6b9db`**. PR #12 is merged, not the current work branch. ASTRA_HANDOFF records exact evidence and resume commands; DAY_LOOP_VALIDATION records the latest daily-gameplay scope and failed baselines. Passing bounded tests do not certify a complete remake.

## Current verified status and diagnostics

Godot **#581 / `34558277149`**, Build **#589 / `34558277154`**, and Rendered UI **#29 / `34558277148`** succeeded. **1,728 smoke assertions**, **240 rendered assertions**, **48 Python tests**, **34 viewport PNGs**. Downloaded archives were hash-checked; night-planning and report screenshots were opened and inspected. New night/report coverage follows the retained 207 UI checks; 36 day-contract assertions and an explicit-night save-fixture guard join the earlier smoke tests.

Smoke retains five intentional invalid-save rejection diagnostics and has zero out-of-tree transform errors. The UI-only scenario submits no deliberately bad saves and rejects logged ERROR/SCRIPT ERROR even if Godot swallows a callback exception.

### TOOLS-003 — editor and native shutdown distinctions

Import exits successfully but still logs `EditorSettings not instantiated yet when getting setting "export/android/shutdown_adb_on_exit"`. Its root cause remains unisolated. Software-rendered UI retains an unsupported-VSync driver warning. Neither was suppressed or used to justify an unverified engine upgrade.

The f420bee UI baseline separately passed its 207 assertions but failed at native C# bridge shutdown. The opt-in scenario now retires its temporary world and permits finalization before quitting; the fatal did not recur in subsequent inspected runs. Forced collection is confined to acceptance teardown. This is not proof that ordinary editor/game shutdown can never fail.

## Open acceptance

### UI-PLAY-001 — selected rendered actions, not all menus or physical devices

Coverage includes Main Menu entry, name input/Tab, selected creation/control layouts, compact routes, standardized synthetic joypad Accept/Back/Start, binding cancellation, scale buttons, Ranch/Town Help, and the new night-choice/bath/report/save flow. Prior management sizes remain 1280x720, 960x540, 640x480, 480x800 and 1920x720. Night/report screenshots use 640x480/960x540. Runtime scale stress 0.8/1.5 is distinct from ordinary 0.85–1.35 button limits; 4K/ultrawide math is not physical-display certification.

Not every picker, action or stat display is audited. Full rendered first-day/skip/combat/courier/leisure/save journeys and physical keyboard/controller/touch/Alt-Tab, deadzones and camera feel remain incompletely covered. Day-2 UI setup is synthetic and world proximity is staged; original resource-backed smoke walkthroughs remain separate. Pixel thresholds are not accessibility or touch-ergonomics certification.

### WORLD-NAV-001 — authored world navigation and presentation

The bench lacks authored collision, seated pose and sitting animation. Bounded nodes and UI hit targets do not prove reachable world routes. Final obstacle-aware navmeshes/collision, character animation/morph assets, weather readability, final art and Forward+ low/high performance on representative hardware remain open. No character identity/design approval or external asset admission was added.

### GAMEPLAY-BALANCE-001 — corrected rules are not long-term balance certification

Night training no longer scales its extra growth passes with roster size; rest-job, fatigue and talent rules remain. This fixes a reproduced defect, not complete progression balance or original-engine parity. Rest/Training/Admin effects are now explained and revisable before settlement; the bath's next-day stamina bonus remains independent.

The shared community board still allows one delivery daily. Optional quiet-corner restoration remains 40 G / 3 supplies once, with up to 10 daily stamina recovery and no upkeep/time advance. Day-1 upkeep consumes starting supplies; the tested construction path uses actual Office Work output. Shared Quiet Rest retains activity/phase/eligibility gates; synthetic positive tests do not approve shipped characters. All upgrade/win paths and long-term stamina/economic tuning remain unverified.

### SETTLEMENT-BOUNDARY-001 — explicit scope of the new guard and ledger

Player time commands validate session/day/phase and reject unplanned or replayed Night actions. Synchronous completion observers cannot recursively settle tomorrow or advance its Morning; observer session replacement is guarded. Raw EndDay retains explicit simulation-call compatibility and is not generally idempotent for arbitrary sequential callers. Exception rollback, multithreaded transactions and exhaustive upstream integer overflow/accounting remain unaudited.

Daily report and overview now include actual work/upkeep, shipments, events and milestone credit. Int display fields saturate rather than overflow, with exact long balance text where necessary. This does not certify every producer's own extreme-input arithmetic or impose a new economic authority.

## Repaired and tested in PR #13

**NIGHT-GROWTH-001:** one extra growth pass, independent of resident count; daily growth marker cleared once before both possible passes. Tests include 1/4/8 residents, rest-job exclusion and an early-pass level-up.

**REPORT-GOLD-001:** actual event/milestone credits, including cap/affordability cases, enter final report and overview totals. Seeded positive/negative events and wallet limits are covered without changed reward rates.

**NIGHT-COMMAND-001:** editable current plans, no immediate repeated effects, stale/duplicate/combat rejection, preserved selected-plan bathing, captured time commands and reentrant completion/session replacement are covered. The old save fixture now explicitly selects Rest; none of its original assertions were removed.

**NIGHT-UI-001:** bounded status-header text leaves content reachable and preserves the full message in its tooltip. Night choices and the existing recovery card use vertical card content, not overlapping panel children. Actual choice revisions, bath and End Day clicks pass; non-overlap and physical target checks remain. The intermediate night-card mistake was caught while developing this feature, not misreported as an old completed feature.

## Prior repairs retained

**UI-LIFECYCLE-001 / UI-SCALE-001 / UI-HEADER-001 / UI-WORLD-LAYOUT-001 / INPUT-CAPTURE-002:** main-menu event detachment, readable adaptive canvas and header/creation reflow, single scale authority, compact world/help layouts and safe cross-device capture cancellation remain. Earlier doubled-scroll and locked-Dairy fixture corrections are documented separately in UI_LAYOUT_ACCEPTANCE.

**UI-VIEW-001 / HUD-OWNERSHIP-001 / COMBAT-UI-001 / INPUT-CAPTURE-001 / TEST-CLOCK-001:** value-only deferred scroll/focus, active HUD ownership, hidden command rejection, canonical player HP, tactical retention/results clock release, stale capture and explicit ordinary-Day-2 fixture remain covered. Full first-day/skip walkthroughs are not replaced by synthetic UI setup.

**LEISURE-001 / WORLD-POINTS-001 / SAVE-REJECT-001 / INPUT-BACK-001 / GAMEPLAY-BOARD-001 / TEST-STALE-001:** bounded resource-backed transactions and saved receipts, stale/reentrant/hidden command rejection, capped recovery, separate bath bonus, nested Back, pre-mutation save rejection and the 2:1 stored-mana rule remain intact. No character approval or legacy expansion was used to pass tests.

**WORLD-ERROR-001 / INPUT-ANALOG-001 / CAMERA-OWNERSHIP-001 / STORY-INPUT-001:** scene-tree readiness, active-camera analog control, focus reset, first-person capture, recenter and overlapping story/transition locks remain covered.

## Remaining earlier audit leads

Importer recovery, full runtime JSON/reference validation, broader settlement exception/idempotency/overflow cases and original-engine differential parity still need dedicated reproduction. The formerly suspected night-growth multiplicity and missing event accounting now have executed repairs, so do not list those specific defects as wholly uninvestigated. Keep development MCP endpoints local pending separate authentication/size/export hardening. Historical snapshots are not current implementation or character-design approval. Preserve schema-16 personal saves and read-only original data; do not reuse stale paths/counts as verification.
