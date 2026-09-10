# Known Issues

Updated **2026-09-10** against code `b7cf272d9b1af4a18da0753c992f1eafdf8591bc`. Distinguish reproduced current findings, older audit leads and untested acceptance. See `ASTRA_HANDOFF.md` for exact evidence and the next action.

## Current verified status

Godot 4.7 Mono CI #515, run `34509942143`: restore/build/launcher regressions/import succeeded; isolated full smoke produced **1486 OK / 3 FAIL**. All 50 community and 8 pause-input checks passed. The full suite remains red and PR #8 remains unmerged.

### TEST-STALE-001 — three existing fixture/expectation mismatches

In `src/Tests/SmokeTestRunner.cs`:

- `constitution enables milk production` uses the starting rancher without approved eligibility. The existing production gate denies correctly. Use separate synthetic positive and unreviewed-denial fixtures; do not approve production characters or weaken the gate to satisfy this assertion.
- The supply-device assertion expects a 1:1 transfer. Current MagicService charges 2 stored MP per personal MP: 20 personal / 45 stored, requested 30 -> restore 22, end at 42 personal / 1 stored.
- The following 10 MP spell assertion expects 40 personal MP; the current result is 32. Preserve the gameplay-effect assertion when correcting the resource expectation.

These assertions were not removed or masked. New isolated mana contract coverage passes but does not make the old full suite green.

### WORLD-ERROR-001 — out-of-tree transform access

The latest smoke log contains 20 `Condition "!is_inside_tree()"` Transform3D errors. The same count was present in the integration baseline before this slice. Root cause and player-facing impact were not established here; locate the actual accesses before patching. Expected malformed-save rejection diagnostics are separate and should not be suppressed with these engine errors.

### UI-PLAY-001 — rendered playability acceptance outstanding

The new board is compiled/imported and its pause-owned UI, stock/payment logic, real production path and save round-trip are tested. Physical keyboard/controller behavior, rendered Ranch/Town routing, small-window scrolling/text clipping, focus visibility and the complete first-day experience still need manual review. Headless assertions do not certify enjoyment or graphical quality. The reward ceiling is preliminary, not a validated economy balance.

The courier board currently lives in Pause -> Community Board and delegates planning to Schedule. It does not add a physical town noticeboard or new exploration encounters. Optional world encounters, companion-specific reactions and visible upgrade rewards are future gameplay slices, not completed features.

## Repaired in this continuation

- **SAVE-REJECT-001:** future schema was stamped to the current version, allowing an unsupported load to replace the live session. It is now rejected before mutation. Null roster entries are also rejected before migration. Existing root state/service/notification/source-preservation checks pass. Schema remains 16; this is not a legacy-save-support expansion.
- **INPUT-BACK-001:** parent pause routing bypassed the nested board and separately polled Back during world updates. Parent and pause menu now share GoBack, both input actions are event-owned, and the no-polling guard plus explicit handler tests pass after a failing negative-control run. Physical frame timing still needs an actual input playtest.
- **GAMEPLAY-BOARD-001:** optional deliveries use real stock and the existing gold ledger. One per day, fixed-size saved receipts, stale-command/overflow validation, no repeat payouts after a saved delivery and no penalties for skipped days are covered by tests. This does not establish broad progression balance.

## Earlier audit leads — not revalidated in this continuation

These remain investigation leads, not fresh reproductions or permission to perform unrelated cleanup:

- Importer build reports previously cited an absent Core project and missing Main. Current JSON must not be overwritten from guessed source conversions.
- ContentValidator was previously reported to check limited .tres metadata rather than full runtime JSON/reference integrity.
- Earlier source review raised night-growth multiplicity and non-idempotent settlement/auto-rest/report-accounting concerns. Trace current code and reproduce before claiming or fixing a specific bug.
- Raw development MCP endpoints previously lacked authentication/size/export hardening. Keep them local; bridge tooling is not release-certified.
- Prior source-audit code hashes can become stale when save/root/UI code changes. Source-only checks are not full-code or identity/design approval.

## Scope that remains unverified or incomplete

Original-game parity has no certified original-engine differential suite. Final character art, animation/morph pipeline, authored navigation/obstacle behavior, camera feel, weather readability and representative hardware performance remain separate acceptance tasks. Existing eligibility gates are present; an older claim that all gates were absent is historical, not the current state. This continuation changes neither eligibility approvals nor adult-specific content.

Historical baseline successes and fixes remain in Git/WORK_LOG. Do not apply old 949/1358 assertion counts, schema 14 or September 5 local paths to the current branch without checking repository evidence.
