# Known Issues

Updated **2026-09-10**, PR #12, code **`44138f3c9da33dc8a234fb9a13d4701ce7d42b68`**. Separate executed evidence from untested acceptance and historical audit leads. See ASTRA_HANDOFF for the current branch and exact test sequence.

## Current verified status

**Godot 4.7 Mono CI #536**, run `34524505949`, and **Build Smoke Check #544**, run `34524505908`, succeeded. The C# 12 checks, preview-override denial, primary-game compilation, launcher regressions and engine import completed. Full isolated smoke: **1,629 OK / zero failed assertions / SMOKE PASS**, including all 82 new leisure checks.

Artifact `10171044862`, SHA-256 `ec72c327d4459dd6628cf10bfcdf47527a476802c6697a4db20918bf4ffd101c`. Smoke `.artifacts/godot/smoke-59zt22d_/console.log` has **zero out-of-tree transform errors** and five intentional malformed/unsupported-save rejection diagnostics. Those negative-fixture errors were not suppressed.

### TOOLS-003 — editor shutdown diagnostic still present

Import `.artifacts/godot/import-13q8rkmg/console.log` finishes asset import and editor-layout loading, then logs `EditorSettings not instantiated yet when getting setting "export/android/shutdown_adb_on_exit"` from `_EDITOR_GET`. The importer exits successfully and the subsequent runtime smoke passes. Record this as an observed engine/editor-lifecycle diagnostic, not a clean import log or an established gameplay failure. Root cause was not isolated by this slice; do not hide it or mix an unverified engine upgrade into the gameplay PR.

## Open acceptance and risks

### UI-PLAY-001 — rendered and physical-device acceptance remains open

The frame suite now reaches Day 3 through the first-day story, production-backed delivery, the corner's supply-planning route, an explicit Night choice and ordinary settlement. It verifies spatial interaction events, construction, recovery, planning/Back and save/load. It stages positions and invokes live button signals; it does not validate visible hit targets, all traversable routes, main-menu/character-creation input, physical hardware or user enjoyment.

Run a rendered isolated playthrough with mouse/keyboard and a physical controller. Check pause/Alt-Tab/travel cursor ownership, deadzones, camera collision/recenter feel, narrow-window focus/scrolling/text, and return routes from management. Touch assistance reuses the existing interaction route but is not physically device-tested here. Representative hardware performance remains unmeasured.

### WORLD-NAV-001 — authored navigation and bench presentation are incomplete

The new quiet corner intentionally uses a greybox bench, with no authored collision, seated pose or sitting animation. Node-visibility and bounded-child-count checks prove state synchronization, not final art or accessible navigation. Existing simple navigation regions and stand-ins still need final obstacle-aware authoring, collision, character animation/morph work, weather readability and low/high Forward+ acceptance. No shipped character identity/design approval or external asset admission was added.

### GAMEPLAY-BALANCE-001 — bounded rewards are not long-term balance certification

The ranch now has a physical Community Board entry in addition to Pause access; both share the existing one-delivery-per-day rule. The optional corner costs 40 G / 3 supplies once and restores at most 10 stamina daily without upkeep or time advancement. Those numbers remain initial bounds. They do not prove all upgrade/win paths or long-term stamina decisions are balanced.

Starting supplies are consumed by existing facility maintenance. Do not claim restoration is automatically affordable on Day 2. The verified route plans Office Work on Day 2 and restores from Day 3 output; neither production nor maintenance was altered. The panel explains shortages and offers planning. Shared Quiet Rest retains existing cost/phase/eligibility rules; positive numeric verification uses a separate synthetic fixture, not guaranteed access for every starting resident.

## Implemented and verified in PR #12

**LEISURE-001:** permanent restoration uses live gold/supplies and two bounded FlagService receipts. Insufficient resources, stale day/phase/generation, wrong area and combat reject before mutation. Reentrant construction/recovery, repeated button signals and saved receipt replay are denied. Full stamina preserves the daily break, partial recovery uses it, existing bath bonuses remain intact, and skipped days add no backlog.

**WORLD-POINTS-001:** the physical board and corner use the existing nearest-target/dispatcher/guard route. Personal points do not require a worker or emit job-completion events; ordinary workstations retain that behavior. The corner closes directly to the world, planning hands control to Schedule, and the board keeps its existing nested Back route. Session replacement closes stale UI and updates the existing presentation without node growth.

The initial new frame fixture failed because it assumed untouched starting supplies; its extension then omitted the intentional Night choice. Both are documented test-harness assumptions, not claims of production defects. The final walkthrough obtains supplies and selects the night workload through ordinary UI. No assertions or valid gameplay rules were removed to make it pass.

## Prior repairs retained and covered

**TEST-STALE-001:** isolated synthetic positive production plus Unknown-denial fixtures retain eligibility gating. Mana supply remains 2 stored MP per personal MP, and the spell-effect assertion remains. No production character was approved to satisfy a test.

**WORLD-ERROR-001:** real scene-tree readiness and pre-tree local positioning repaired prior invalid transform accesses. Current smoke still has zero out-of-tree diagnostics, without suppressed engine logging.

**INPUT-ANALOG-001 / CAMERA-OWNERSHIP-001 / STORY-INPUT-001:** analog magnitude, active-viewport movement, input/focus resets, first-person capture ownership, one-press recenter and overlapping story/transition locks from PR #11 remain covered. The earlier frame fixture's Bootstrap/MainMenu issue is historical, not an uncorrected production Escape defect.

**SAVE-REJECT-001 / INPUT-BACK-001 / GAMEPLAY-BOARD-001:** future/null-invalid saves reject before mutation; parent and pause share event-owned Back; real stock, the gold ledger, bounded saved courier receipts and replay protection remain intact. Schema stays 16; no legacy-support expansion.

## Earlier audit leads — not certified by this slice

Importer reports previously cited an absent Core project/missing Main; do not regenerate JSON from guessed conversions. ContentValidator was reported to check limited metadata instead of complete runtime reference integrity. Prior source review raised night-growth multiplicity, settlement idempotency and report-ledger concerns; reproduce current behavior before claiming a repair. The successful two-settlement walkthrough is not exhaustive proof against every reentrancy or accounting issue.

Raw development MCP endpoints have separate authentication/size/export-hardening leads; keep them local. Old source-audit snapshots are not current code verification or identity/design approval. Original-game parity still lacks a certified original-engine differential suite.

Historical counts, schema 14, machine-local engine paths and old pending-PR notes do not describe this branch. Preserve the original read-only source and current-version personal saves under D-011.
