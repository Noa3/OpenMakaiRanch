# Known Issues

Updated **2026-09-11**, **PR #13**, verified code head **`2a958b1c4425e31acd48cf5f604cdda378fc4846`**. PR #12 is merged, not the current work branch. ASTRA_HANDOFF records resume commands; CORE_PLAYABILITY_VALIDATION records the latest mission/management scope; DAY_LOOP_VALIDATION retains the prior daily-gameplay baseline and repairs. Passing bounded tests do not certify a complete remake.

## Current verified status and diagnostics

Godot **#591 / `34587496873`**, Build **#599 / `34587496874`**, and Rendered UI **#34 / `34587496917`** succeeded. **1,731 smoke assertions**, **316 rendered assertions**, **48 Python tests**, **43 viewport PNGs**. The build reports zero warnings/errors. Downloaded archives were hash-checked; guild, combat, store, schedule and save/load screenshots were opened and inspected. New mission/management coverage follows the retained 240 UI checks; the previous day-contract and nightly-plan guards remain.

Smoke retains five intentional invalid-save rejection diagnostics. The UI-only scenario submits no deliberately bad saves and rejects logged ERROR/SCRIPT ERROR even if Godot swallows a callback exception; the inspected final rendered console has none.

### TOOLS-003 — editor and native shutdown distinctions

Import exits successfully but still logs `EditorSettings not instantiated yet when getting setting "export/android/shutdown_adb_on_exit"`. Its root cause remains unisolated. Software-rendered UI retains an unsupported-VSync driver warning. Neither was suppressed or used to justify an unverified engine upgrade.

The f420bee UI baseline separately passed its 207 assertions but failed at native C# bridge shutdown. The opt-in scenario now retires its temporary world and permits finalization before quitting; the fatal did not recur in subsequent inspected runs. Forced collection is confined to acceptance teardown. This is not proof that ordinary editor/game shutdown can never fail.

## Open acceptance

### UI-PLAY-001 — selected rendered actions, not all menus or physical devices

Coverage includes Main Menu entry, name input/Tab, selected creation/control layouts, compact routes, standardized synthetic joypad Accept/Back/Start, binding cancellation, scale buttons, Ranch/Town Help, night-choice/bath/report/save, Road Patrol tactical commands/results/return, purchase, job assignment and authored Save/Load buttons. Prior management sizes remain 1280x720, 960x540, 640x480, 480x800 and 1920x720. New guild geometry uses 640x480/960x540; combat/store/schedule/milestone/save captures use 640x480. Runtime scale stress 0.8/1.5 is distinct from ordinary 0.85–1.35 button limits; 4K/ultrawide math is not physical-display certification.

Not every picker, action or stat display is audited. The connected night/mission/store/save scenario inherits synthetic Day-2 resources and strong stats; it is not an organically earned new-game or difficulty playthrough. An uninterrupted rendered first-day/skip/world/courier/leisure journey, all mission outcomes, Research action coverage and physical keyboard/controller/touch/Alt-Tab, deadzones and camera feel remain incomplete. Original resource-backed smoke walkthroughs remain separate. Staged proximity and pixel thresholds are not world-navigation, accessibility or touch-ergonomics certification.

### WORLD-NAV-001 — authored world navigation and presentation

The bench lacks authored collision, seated pose and sitting animation. Bounded nodes and UI hit targets do not prove reachable world routes. Final obstacle-aware navmeshes/collision, character animation/morph assets, weather readability, final art and Forward+ low/high performance on representative hardware remain open. No character identity/design approval or external asset admission was added.

### GAMEPLAY-BALANCE-001 — corrected rules are not long-term balance certification

Night training no longer scales its extra growth passes with roster size; rest-job, fatigue and talent rules remain. This fixes a reproduced defect, not complete progression balance or original-engine parity. Rest/Training/Admin effects are explained and revisable before settlement; the bath's next-day stamina bonus remains independent. Guild readiness guidance reflects existing stamina rules, not a new balancing pass.

The shared community board still allows one delivery daily. Optional quiet-corner restoration remains 40 G / 3 supplies once, with up to 10 daily stamina recovery and no upkeep/time advance. Day-1 upkeep consumes starting supplies; the tested construction path uses actual Office Work output. Shared Quiet Rest retains activity/phase/eligibility gates; synthetic positive tests do not approve shipped characters. All upgrade/win paths and long-term stamina/economic tuning remain unverified.

### SETTLEMENT-BOUNDARY-001 — explicit scope of the guard and ledger

Player time commands validate session/day/phase and reject unplanned or replayed Night actions. Synchronous completion observers cannot recursively settle tomorrow or advance its Morning; observer session replacement is guarded. Raw EndDay retains explicit simulation-call compatibility and is not generally idempotent for arbitrary sequential callers. Exception rollback, multithreaded transactions and exhaustive upstream integer overflow/accounting remain unaudited.

Daily report and overview include actual work/upkeep, shipments, events and milestone credit. Int display fields saturate rather than overflow, with exact long balance text where necessary. This does not certify every producer's own extreme-input arithmetic or impose a new economic authority.

## Repaired and tested in PR #13

**CORE-CARD-001:** sequential Adventure/Combat/Shop/Schedule/Research/Milestone panel children receive a vertical layout owner. Original child identities/order/signals and idempotence are tested. Selected rendered guild/combat/store/schedule/milestone layouts have non-overlapping rectangles. Intentional overlays elsewhere are not automatically rewritten.

**CORE-FOCUS-001:** a reproduced results Back command no longer remains focused outside the viewport when battle commands disappear. Different fallback commands are revealed after layout; exact logical matches still preserve manual scroll. Burst, route, hidden-view and clipped-replacement regressions remain tested. The large-viewport fixture was corrected to prove initial clipping, not to accept an invisible fallback.

**CORE-JOURNEY-001:** real tactical commands/results/return, one-time entry stamina, time-lock release, no repeated result rewards, battle-wear saving, exact purchase accounting, deferred job payment and actual Slot 3 Save/Load state restoration pass. These certify the selected fixture path, not all combat outcomes or organic progression. Acceptance writes only disposable isolated slots 99 and 3 and refuses an occupied Slot 3.

**NIGHT-GROWTH-001:** one extra growth pass, independent of resident count; daily growth marker cleared once before both possible passes. Tests include 1/4/8 residents, rest-job exclusion and an early-pass level-up.

**REPORT-GOLD-001:** actual event/milestone credits, including cap/affordability cases, enter final report and overview totals. Seeded positive/negative events and wallet limits are covered without changed reward rates.

**NIGHT-COMMAND-001:** editable current plans, no immediate repeated effects, stale/duplicate/combat rejection, preserved selected-plan bathing, captured time commands and reentrant completion/session replacement are covered. The old save fixture explicitly selects Rest; none of its original assertions were removed.

**NIGHT-UI-001:** bounded status-header text leaves content reachable and preserves the full message in its tooltip. Night choices and recovery use vertical card content. Actual choice revisions, bath and End Day clicks pass. The intermediate night-card mistake was caught while developing the feature, not misreported as an old completed feature.

## Prior repairs retained

**UI-LIFECYCLE-001 / UI-SCALE-001 / UI-HEADER-001 / UI-WORLD-LAYOUT-001 / INPUT-CAPTURE-002:** main-menu event detachment, readable adaptive canvas and header/creation reflow, single scale authority, compact world/help layouts and safe cross-device capture cancellation remain. Earlier doubled-scroll and locked-Dairy fixture corrections are documented separately in UI_LAYOUT_ACCEPTANCE.

**UI-VIEW-001 / HUD-OWNERSHIP-001 / COMBAT-UI-001 / INPUT-CAPTURE-001 / TEST-CLOCK-001:** value-only deferred scroll/focus, active HUD ownership, hidden command rejection, canonical player HP, tactical retention/results clock release, stale capture and explicit ordinary-Day-2 fixture remain covered. Full first-day/skip walkthroughs are not replaced by synthetic UI setup.

**LEISURE-001 / WORLD-POINTS-001 / SAVE-REJECT-001 / INPUT-BACK-001 / GAMEPLAY-BOARD-001 / TEST-STALE-001:** bounded resource-backed transactions and saved receipts, stale/reentrant/hidden command rejection, capped recovery, separate bath bonus, nested Back, pre-mutation save rejection and the 2:1 stored-mana rule remain intact. No character approval or legacy expansion was used to pass tests.

**WORLD-ERROR-001 / INPUT-ANALOG-001 / CAMERA-OWNERSHIP-001 / STORY-INPUT-001:** scene-tree readiness, active-camera analog control, focus reset, first-person capture, recenter and overlapping story/transition locks remain covered.

## Remaining earlier audit leads

Importer recovery, full runtime JSON/reference validation, broader settlement exception/idempotency/overflow cases and original-engine differential parity still need dedicated reproduction. Reproduced night-growth multiplicity, missing event accounting, sequential-card overlap and hidden results focus now have executed repairs; do not list those particular defects as wholly uninvestigated. Keep development MCP endpoints local pending separate authentication/size/export hardening. Historical snapshots are not current implementation or character-design approval. Preserve schema-16 personal saves and read-only original data; do not reuse stale paths/counts as verification.
