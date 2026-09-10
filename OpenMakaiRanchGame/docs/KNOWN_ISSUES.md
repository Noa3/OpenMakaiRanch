# Known Issues

Updated **2026-09-11**, PR #12, verified head **`9d8eb0967012b9c9c4d7d22fe16e96f1b379327c`**. Distinguish reproduced defects, fixed contracts and untested acceptance. The remake is not certified complete merely because the current suite passes.

## Current verified status

**Godot 4.7 Mono CI #545**, run **`34538829758`**, succeeded: effective C# 12 and negative preview-override check, primary-game compilation, 34 launcher regressions, engine import and isolated smoke. Full result: **1,691 OK / zero failed assertions / SMOKE PASS**. All 52 HUD/menu checks and nine additional layout/view-state checks pass alongside existing first-day, leisure, combat, input and save tests.

Artifact **`10176503313`**, SHA-256 **`f17eeae0fa39320f61a97b0763a02203a6b4b1e0ea234d6c440f2e44fc390ef2`**. Smoke `.artifacts/godot/smoke-ohfy9cr3/console.log` has zero out-of-tree transform errors and five intentional malformed/unsupported-save rejection diagnostics. Do not hide the negative-fixture messages or describe them as newly observed personal-save corruption.

### TOOLS-003 — editor shutdown diagnostic remains

Import `.artifacts/godot/import-tcn460wg/console.log` finishes successfully but logs `EditorSettings not instantiated yet when getting setting "export/android/shutdown_adb_on_exit"`. This separate editor-lifecycle warning/error remains observed, not isolated as a gameplay failure. No engine upgrade or diagnostic suppression was mixed into these fixes.

## Open acceptance

### UI-PLAY-001 — rendered and physical-device coverage

The suite verifies fifteen ordinary screen destinations have content, HUD ownership, help/Back, hidden callbacks, empty-roster HP, options layout, bindings, night selection and tactical entry/results. It does **not** exercise every feature/action/reward on every menu. Synthetic Day-2 UI fixtures are separate from the resource-backed full first-day/leisure walkthrough.

Run the actual Main Menu -> character creation -> world in an isolated rendered session, including ordinary first day and skip, pause/Alt-Tab/travel, tactical results, night/report, board/corner/planning and save/load. Check visible click targets, small-window text/clipping/scrolling and real keyboard/gamepad/touch behavior. Headless input events and button signals are not physical-device or visual acceptance. Representative hardware performance remains unmeasured.

### WORLD-NAV-001 — authored navigation and presentation

The corner bench remains greybox geometry with no authored collision, seated pose or sitting animation. Bounded node count and visibility prove synchronization, not visual quality or walkability. Interaction tests stage player proximity. Final obstacle-aware navmeshes, collision, character animation/morph assets, weather readability and Forward+ low/high acceptance remain open. No character design/identity approval or external asset admission was added.

### GAMEPLAY-BALANCE-001 — bounded rewards are not long-term balance proof

The physical and Pause community boards share one delivery per day. Corner restoration costs 40 G / 3 supplies once; recovery gives at most 10 stamina daily without upkeep or time advancement. These are initial bounds, not a certification of all upgrades/win paths or long-term stamina decisions.

Day-1 facility maintenance consumes starting supplies. The tested route obtains Office Work output for Day-3 restoration. It does not inject stock, money, time or stamina to fund the transaction. Shared Quiet Rest retains existing cost/phase/eligibility requirements; positive synthetic numeric coverage does not approve or guarantee access for every shipped character.

## Fixed and covered HUD/menu integration

**UI-VIEW-001:** same-screen scroll restoration previously ran amid dynamic container layout, and options appended controls in a later process step. The view now composes those controls before a ProcessFrame-boundary restore. Pending value snapshots retain exact scroll/logical focus across burst notifications; rerouted/hidden/replaced views reject stale callbacks. Nine frame tests cover preservation and cancellation. Normal FollowFocus remains enabled.

**HUD-OWNERSHIP-001:** explicit active-area CanvasLayer visibility; hidden/inactive world time, management and travel callbacks reject without mutation. HP comes from PlayerState rather than indexing a resident. Help closes before Back opens another layer; management retains input ownership when help closes.

**COMBAT-UI-001:** rejected combat navigation does not lock time; a live tactical session is retained across state notifications and cannot be abandoned by switching ordinary screens. Entry charges stamina once; results exit clears the combat session/clock and restores world time. Night planning remains explicit; the management button reads Plan Night until a choice exists.

**INPUT-CAPTURE-001:** retiring/hiding an options screen cancels its binding capture, and an old binding button cannot re-arm capture after routing away. This is not an exhaustive audit of every historical settings callback.

**TEST-CLOCK-001:** the old composition test activated mandatory Day 1 then tried ordinary settlement while tutorial input ownership was active. It now explicitly establishes a completed-story Day-2 fixture for the clock subsection and asserts that ownership. Original fresh-story assertions, settlement assertions, runtime guards, full first-day and skip-to-night tests remain. This is a test-fixture correction, not a newly fixed production skip bug.

Pre-fix code `a45a0bc` / CI #540 (`34536254574`) had 1,678 OK and three failures. All three pass in #545 with one additional fixture guard and nine new layout checks. No assertions or gameplay requirements were removed to obtain the result.

## Prior repairs retained

**LEISURE-001 / WORLD-POINTS-001:** live-resource restoration and bounded saved receipts; shortage/stale/reentrant/hidden-command rejection; capped recovery; independent bath bonus; personal points without workers; existing Schedule/Back handoff; session replacement and root save/load remain covered.

**WORLD-ERROR-001 / INPUT-ANALOG-001 / CAMERA-OWNERSHIP-001 / STORY-INPUT-001:** real scene-tree readiness, active-viewport/analog controls, focus resets, first-person capture, one-press recenter and overlapping transition/story input locks remain covered. Earlier Bootstrap/MainMenu fixture interference is historical.

**SAVE-REJECT-001 / INPUT-BACK-001 / GAMEPLAY-BOARD-001 / TEST-STALE-001:** invalid saves reject before mutation, nested Back stays event-owned, courier transactions persist together, production fixtures retain gating, and stored mana remains 2:1. No production character was approved to satisfy tests. Schema remains 16; no legacy expansion.

## Earlier audit leads — not certified here

Importer recovery, complete runtime JSON/reference validation, night-growth multiplicity, exhaustive settlement idempotency/report ledger accounting, and original-engine parity still require dedicated reproduction/verification. Successful bounded walkthroughs are not exhaustive proof. Keep raw development MCP endpoints local pending separate authentication/size/export hardening. Source-audit snapshots are not current implementation or character-design approval.

Preserve current-version personal files and the original read-only source. Old schema-14 counts, local engine paths and pending-PR notes must not override current code and executed evidence.
