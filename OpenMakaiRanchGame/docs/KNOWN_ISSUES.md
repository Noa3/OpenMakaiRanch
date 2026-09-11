# Known Issues

Updated **2026-09-11**, PR #12. Verified code-inclusive head **`897871eadbdf02c19d905c6e0b33e5edf335e649`**. A passing suite is not certification of a complete remake. Exact runs, artifact hashes and commands are in ASTRA_HANDOFF; rendered baselines and scope are in UI_LAYOUT_ACCEPTANCE.

## Current verified status

Godot CI **#563 / `34546801277`**, Build **#571 / `34546801316`**, and Rendered UI **#17 / `34546801281`** succeeded. **1,691 smoke assertions**, **207 rendered UI assertions**, **48 Python launcher/evidence tests**, **31 viewport PNGs**. Both evidence archives were downloaded, checked and inspected. The rendered suite uses native engine input routing and actual screenshots, not physical input hardware or Forward+ performance measurements.

Smoke has zero out-of-tree transform errors and five intentional malformed/unsupported-save rejection diagnostics. They test rejection before mutation, not personal-save corruption. The UI-only scenario has no deliberate bad saves and rejects logged ERROR/SCRIPT ERROR even when a signal callback exception does not propagate into the test task.

### TOOLS-003 — editor shutdown diagnostic remains

Import exits successfully but still logs `EditorSettings not instantiated yet when getting setting "export/android/shutdown_adb_on_exit"`. Its root cause remains unisolated. The software-rendered UI run also retains an unsupported-VSync driver warning. Neither was hidden and no unverified engine upgrade was mixed into gameplay changes. Existing restore-source warnings are not evidence of a failed compilation.

## Open acceptance

### UI-PLAY-001 — bounded rendered coverage, not complete device/action acceptance

Main Menu -> character creation, name input, Tab focus, selected creation/control layouts, compact route navigation, synthetic joypad Accept/Back/Start, binding cancellation, scaling buttons and Ranch/Town Help are covered. Tested management windows: 1280x720, 960x540, 640x480, 480x800 and 1920x720. Runtime scale stress fixtures at 0.8/1.5 are distinct from actual button checks constrained by the existing 0.85–1.35 authority. The 4K/ultrawide canvas checks are math contracts, not corresponding physical-display certification.

Every picker/action, complete rendered first-day/skip/combat/courier/leisure/save journey, physical keyboard/controller/touch behavior, Alt-Tab, deadzones, camera feel and representative hardware remain incompletely covered. Interface Day-2 state is synthetic; world proximity is staged. Pixel thresholds and one renderer do not certify touch ergonomics or accessibility compliance. The resource-backed first-day/leisure smoke walkthrough remains separate.

### WORLD-NAV-001 — authored navigation and presentation

Bench geometry lacks authored collision, seated pose and sitting animation. Bounded nodes and UI presentation do not prove reachable world routes. Final obstacle-aware navmeshes/collision, character animation/morph assets, weather readability, final art and Forward+ low/high acceptance remain open. No character design/identity approval or external asset admission was added.

### GAMEPLAY-BALANCE-001 — bounds are not long-term balance proof

Community Board entries still share one daily delivery. Optional corner restoration costs 40 G / 3 supplies once; recovery gives at most 10 stamina daily without upkeep/time advance, independent of the next-day bath bonus. These initial bounds do not certify all upgrade/win paths or stamina balance. Day-1 upkeep consumes starting supplies; the tested route uses real Office Work output for Day-3 restoration. Shared Quiet Rest retains cost/phase/eligibility rules; synthetic positive tests do not approve shipped designs or guarantee availability for each character.

## Fixed and covered in the rendered continuation

**UI-LIFECYCLE-001:** a disposed MainMenu remained subscribed to GameRoot.StateChanged; ordinary name input exposed the exception. Scene exit now detaches it.

**UI-SCALE-001 / UI-HEADER-001:** the old virtual 1920-wide canvas shrank small-window controls. Adaptive canvas dimensions, wrapping header rows, responsive creation grids/minimums and natural wrapped-label heights retain usable space. Options buttons no longer multiply RootPanel.Scale in addition to central viewport scaling. Short-height headers reserve content space so Scale Down remains reachable. All twelve increase/decrease actions now have explicit pre-click visibility assertions.

**UI-WORLD-LAYOUT-001:** Ranch/Town summary/actions, worker/alerts, interaction affordance and help are bounded at tested sizes. Detailed warnings/tutorial instructions remain in scrollable Help; compacting does not mark tutorials completed. Duplicate legacy prompts are hidden after the host has bound its areas.

**INPUT-CAPTURE-002:** either Escape or controller Back cancels either capture device, including the arming delay. Retired/reset cards cannot re-arm; capture text stays inside stable button widths. This is not an exhaustive audit of every historical settings callback.

Fixture errors are recorded separately: no double EnsureControlVisible after FollowFocus; unbuilt Dairy has a negative control and built Pasture a positive hit-target check; named captures assert physical dimensions. No facility or assertion was removed to obtain a pass.

## Prior repairs retained

**UI-VIEW-001 / HUD-OWNERSHIP-001 / COMBAT-UI-001 / INPUT-CAPTURE-001 / TEST-CLOCK-001:** pending value-only scroll/focus restoration, active HUD ownership, hidden-command rejection, canonical HP, explicit Night choice, retained tactical session/results clock release, stale captures and corrected ordinary-Day-2 clock fixture remain covered. Full first-day/skip tests are not replaced by the synthetic fixture.

**LEISURE-001 / WORLD-POINTS-001 / SAVE-REJECT-001 / INPUT-BACK-001 / GAMEPLAY-BOARD-001 / TEST-STALE-001:** real-resource transactions, bounded saved receipts, stale/reentrant/hidden-command rejection, capped recovery and separate bath bonus, nested Back, save rejection before mutation and 2:1 stored-mana supply remain intact. No character approval or legacy-support expansion was used to satisfy tests.

**WORLD-ERROR-001 / INPUT-ANALOG-001 / CAMERA-OWNERSHIP-001 / STORY-INPUT-001:** scene-tree readiness, active-viewport/analog controls, focus resets, first-person capture, recenter and overlapping story/transition locks remain covered.

## Earlier audit leads — not certified here

Importer recovery, complete runtime JSON/reference validation, night-growth multiplicity, exhaustive settlement idempotency/report-ledger accounting and original-engine parity still require dedicated reproduction/verification. Keep development MCP endpoints local pending separate authentication/size/export hardening. Historical source snapshots are not current implementation or design approval. Preserve schema-16 personal saves and the read-only original source; old local-engine paths/counts must not override current code/evidence.
