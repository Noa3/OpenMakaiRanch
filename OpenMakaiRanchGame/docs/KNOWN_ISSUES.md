# Known Issues

Updated **2026-09-11**, **PR #15**, branch `feature/world-stations-and-interiors-20260911`. Verified code checkpoint **`67a052806e9241f68110e13494e944c90bac6203`**: **1,778 smoke assertions,432/432 rendered checks,55 Python tests,56 PNGs**, all three CI workflows successful. RANCH_DESIGN_VALIDATION.md records the current exact evidence; LOCALIZATION_UI_VALIDATION.md preserves the preceding fc6994a scope. The projects/shared-evening/plot continuation now has its own executed checks, not only the older localization pass. PR #13 is merged; PR #14's test-only baseline is included and repaired here.

## Open issues

### DESIGN-001 — motivating campaign and relationship content

Four optional guidance projects are implemented, not a complete campaign. An earned first-week balancing run, resident aspirations, distinct non-explicit dialogue and visible chapter payoff remain open. Existing win conditions are unchanged. Shared nights are optional and use reviewed adult definitions/state plus voluntary romantic eligibility; stock content may remain unavailable pending individual review. Do not bypass that gap with global approval. AUTHOR_CONTENT_HANDOFF.md documents missing art/story and a separate, unimplemented family-planning system. No pregnancy event, explicit prose, animation or audio was supplied.

### TOOLS-003 — warnings and lifecycle

Builds succeed but retain known warnings for the unavailable developer-local NuGet source and nullable dereference at RanchLeisureFrameTests.cs:215. Import exits successfully with the known EditorSettings shutdown diagnostic; software-rendered UI warns about VSync. The final smoke has five intentional invalid-save diagnostics, zero failed assertions and no observed native fatal; its UI runtime has zero ERROR/SCRIPT ERROR. Smoke-only PackedScene retention/staged teardown are not normal-game GC or proof of every engine shutdown path. Keep diagnostics distinct and do not hide them to manufacture a pass.

### I18N-001 — partial translation

The initial200-key English/German slice plus47 project/evening keys covers selected title/world/station/guide/warning/project surfaces. Existing Japanese entries remain; missing new entries fall back to English. Story, character creation, retained service menus and command-result/status messages are not fully localized. Saved report lines retain their original language. Grammatical plurals, RTL, additional scripts/font coverage, subtitles/voice and language-specific device checks remain open.

Actual title/Options choices, unchanged window settings and stable open-station context/focus/destination IDs remain tested. Five export filters include raw JSON, but an actual exported-build localization playthrough remains required. Use TRANSLATION_GUIDE.md; never translate persistent IDs/player-entered names. LOCALIZATION_ROADMAP.md plans ten localized editions excluding Russian using the retrieved July2026 Valve survey as a Steam-client proxy, not universal audience measurement. New locales need real reviewed text and script/region-preserving normalization before enabling them.

### UI-PLAY-001 — creator and presentation

The character editor is not yet the requested stepwise creator. Main menu has a private procedural ranch diorama, localized caption and revised actions, but placeholder art and small-viewport panel coverage remain. Do not call this final AAA presentation.

Physical station/resident panels and read-only Places replace the global player-facing hub; some services retain context-restricted renderers. Large3D labels, warning details and HUD/tutorial overlap clutter the inspected doorway view. The planned-evening status can repeat the same text already shown by the panel. These remain polish issues despite selected geometry/hit-target tests.

An uninterrupted organically earned first-day/skip/world/courier/leisure/save route, every service action/mission outcome, physical keyboard/controller/touch/Alt-Tab/deadzones and camera feel are incomplete. Later UI fixtures use synthetic Day-2 resources/strong stats. Shared-night tests use separate synthetic identities and staged proximity, not naturally earned relationships or shipped-design approval.

### WORLD-NAV-001 — building stages and walk-in prototypes

Metre-scale shells have real doorways, wall/furniture collision, roof/wall cutaways, shelter and collision-derived navigation. Selected rays and held-key player walking pass without teleport or walking stamina tax. Eight authored plot envelopes, doors, gate approach and stable grades through extreme numerical levels are checked; tree meshes/fallback crowns now reject reserved space before placement. These are static bounds and selected traversal tests, not complete dynamic collision/navigation certification.

Annexes/upper floors/free placement, every doorway, larger character variants, furniture/other props, animated shader/skeleton bounds, camera angles, NPC avoidance/stuck recovery and full routes remain unaudited. The leisure bench lacks final collision/seated animation. Additional facility types require their own authored plots; arbitrary mesh scaling is not a valid expansion.

The title's water strip is not a playable organic town river/bridges/landmarks. Final buildings/nature/character/morph assets, weather readability, LOD/memory budgets and representative-hardware Forward+ low/high performance remain open. No external asset or character design approval was added.

### GAMEPLAY-BALANCE-001 — selected rules, not all progression

First-day barn construction uses actual starting funds before staffing. Held input survives prompt reads. Supply planning marks Places without granting resources. Existing one-pass night training, growth markers, editable plans, separate bath bonus and once-daily delivery remain. Quiet-corner restoration stays40G/3supplies/up to10 daily stamina without time advance/upkeep.

Projects add no new currency/reward loop or deadlines. Shared nights acknowledge a successful settlement once with+1Bond/+2Morale, not a duplicate rest/bath/energy payout; planning/cancellation is free and saved. This is not a long-term balance certificate for romance, upgrades or main victory. Higher numerical building levels do not imply larger visual shells; visual grade3 currently saturates.

### SETTLEMENT-BOUNDARY-001 — bounded guards, not universal transactions

Captured time commands require a Night plan and reject stale/replayed/reentrant transitions. Completion observers cannot settle tomorrow or attach old reports to a replaced session. Shared-night snapshot/receipt logic is attached to the ordinary successful settlement, not a second day loop. Raw EndDay retains explicit simulation-call compatibility, not universal idempotency. Exception rollback, multithreaded transactions and exhaustive upstream overflow/accounting remain unaudited. Facility upgrade arithmetic now rejects unrepresentable cost/level changes before payment; upkeep/automation/extreme production still need a separate audit.

## Repairs and audit leads to preserve

RANCH_DESIGN_VALIDATION.md documents read-only projects, reserved plots/tree bounds, optional invitation/cancellation/settlement, receipt saving and the profile-settings test correction. LOCALIZATION_UI_VALIDATION.md retains actual engine-dispatched language selection, blank-prologue/bedroom-return, held-input/construction, local Build/Assign/Back, stale/remote denial, directional warnings and selected doorway/cutaway evidence. Earlier popup attempts with English screenshots under German filenames are not translation acceptance.

PR #13 card/focus/night-growth/ledger/planning, adventure and Save/Load receipts remain in CORE_PLAYABILITY_VALIDATION.md and DAY_LOOP_VALIDATION.md. Responsive scaling, HUD ownership, camera/input-capture, pre-mutation save rejection, voluntary activity guards, receipts and stored-mana rules remain. Original-source and eligibility gates were not relaxed.

Importer recovery/provenance, complete runtime JSON/reference validation, original-engine differential parity and development MCP authentication/request-size/export hardening need dedicated work. Preserve schema16 personal files and read-only original data. ASTRA_HANDOFF/live Git supersede stale numerical KANBAN/WORK_LOG snapshots.
