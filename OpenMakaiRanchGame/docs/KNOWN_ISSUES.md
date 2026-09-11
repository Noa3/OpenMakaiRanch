# Known Issues

Updated **2026-09-11**, PR #15, branch `feature/world-stations-and-interiors-20260911`. Current verified code head **`4ffd97c84929ed307fa623dde709760dcec28762`**: **1,889 smoke assertions,526/526 rendered checks,55 Python tests,64 PNGs**, all three CI workflows successful. OPTIMIZATION_VALIDATION.md records exact runs/source/artifacts and limitations. Earlier resident/design/localization/day validation documents retain their own historical scopes. Main remains unchanged; PR #14's old baseline is included here, not a separate merge target.

## Priority gaps

### ENDING-001 — physical completion presentation and continuation

Service-level early completion and continued days are tested, but the complete victory interface is not certified. HouseSleep/ordinary report routing can replace a newly reached victory presentation. Return from a locked screen also needs a location-preserving review. The existing Continue Ranching button must be tested through actual world/input transitions; its mere presence is not a verified endgame. Proposed new ambitions/ending UI was excluded after a blocked central-file write, and no alternate path installed that change. Central WorldStationPanel.cs remains at its baseline blob. New Game+ remains a separate user decision; no automatic reset was added.

### DESIGN-001 — longevity without punishing optimization

Four starting projects are not a complete campaign. Authored restoration payoff, resident arcs, specializations and optional mastery challenges remain absent. OPTIMIZATION_AND_PACING.md recommends preserving fast success and adding meaningful choices/content, not hidden penalties, minimum-day gates or exponential cost padding. No new caps, production reductions or prices were introduced in this continuation.

The seven-day seeded dairy/office economy fixture stays solvent but loses8G on ordinary days after paid meal boxes; larger receipts keep this short run afloat. It is not an optimal policy, proof of indefinite profit or a campaign-duration bound. Compare multiple casual/efficient/specialist policies with full resources and actions before retuning. Current higher facility levels do not multiply ordinary job output; expensive checklist-only progression needs better benefits, not larger upkeep as a substitute for content.

### STATIONS-001 — useful forecasts, not guaranteed final daily profit

Overview/Team/Equipment, price/level-scaled upkeep, zero-level ghost-bill repair and guarded local transactions now have a completed rendered journey through the next day and save/load. Team current-condition gross output shares the production calculator and preserves rounding/rates. Night plans, later care, consumption, events and bills remain outside that displayed estimate. Fatigue>=70 auto-rest happens before night recovery/work and is warned about. Final net forecasting, clear equipment marginal benefits and dedicated store/research/result presentation remain open. Forecast help still occupies substantial space in a small viewport; tooltip contents do not certify all pointer/device behavior.

### INVENTORY-001 — targeted duplication repair only

Unavailable equipment replacement previously returned the old equipped item before validating new stock, enabling repeated duplication. The swap now checks stock and return capacity first; tests cover twenty denied repeats, a real owned swap and a full return stack. Bonus inspection of a null map is inert. Other Equip/Unequip/GetEquippedItem behaviors, all consumables, negative/extreme counts and upstream overflow are not comprehensively audited. Do not confuse invalid item duplication with legitimate efficient play.

### TOOLS-003 — diagnostics and rendered timeout

Build success retains known developer-local NuGet and nullable warnings. Import still logs the successful-exit EditorSettings shutdown diagnostic; software rendering warns about VSync. Five smoke errors deliberately reject invalid saves; the final UI runtime has zero ERROR/SCRIPT ERROR. Test-only scene retention/teardown is not ordinary-game memory management or a universal native lifecycle fix.

The preceding station UI run0356785 failed after240.795 seconds, exit-9, without final results.json. It is not a pass. Existing ui-acceptance.yml now permits360 seconds for the growing software-rendered process, still15minutes overall, retaining assertions and runtime-error rejection. The fixture restores the small viewport after loading profile settings. No new pipeline/workflow or write-capable patch helper was created this turn; the higher test budget is not a frame-rate performance claim.

### I18N-001 — partial translation

Matching English/German UI keys total344 across selected title/world/resident/project/station/forecast views. Japanese and other missing keys still use fallback; story, creation, old services and result messages remain incomplete. Saved report text stays in its original language. Plurals, RTL, scripts/fonts, audio/subtitles and actual exported-build locale acceptance remain open. Current en/de/ja selection is not full localization. Preserve player names and internal IDs. TRANSLATION_GUIDE.md and LOCALIZATION_ROADMAP.md describe the later ten-target plan excluding Russian, not a universal audience census.

### UI-PLAY-001 — incomplete presentation and content

The character creator is not yet the requested stepwise editor. The title has an isolated procedural diorama and localized actions but remains placeholder art. Large world labels, warning/help density and HUD/tutorial overlap require polish. Existing service renderers are context-scoped, not all deleted. Optional shared-night planning can repeat status text. Actual physical keyboard/controller/touch/Alt-Tab/deadzones and complete accessibility/camera feel are not certified.

Resident care/practice/gift/work pages and real next-day receipts are exercised. Character-specific dialogue, final talking/training animations, complete preferences/equipment/recovery paths and an earned whole-game week remain open. Ordinary actions are separate from adult/romantic eligibility; existing voluntary shared-night gates and original role/source names remain. No pregnancy/family implementation or production identity approval was added; NSFW_CONTENT_HANDOFF.md and AUTHOR_CONTENT_HANDOFF.md remain neutral technical/content handoffs.

### WORLD-NAV-001 — bounded buildings and selected traversal

Metre-scale shells have real openings, wall/furniture collision, cutaways, shelter and collision-derived navigation. Eight plot envelopes protect doors/roofs/gate; static tree meshes and placeholder crowns respect them. Visual grades0-3 preserve the shell through higher numeric upgrades. Selected actual held-key doorway walking, rays and local actions pass. This does not certify annexes/upper floors/free placement, every character size, furniture/other props, shader/skeleton expansion, NPC avoidance/recovery, all camera angles or complete routes. The leisure bench still lacks final seating animation/collision.

The title water strip is not the requested organic playable town river. Final authored buildings/nature/characters/morphs, weather readability, LOD/memory and Forward+ hardware tests remain open.

### SETTLEMENT-001 — guarded commands, not universal rollback

Root captures session/day/phase, rejects stale/replayed/reentrant ordinary commands and prevents completion observers from recursively settling tomorrow. Daily ledgers reconcile actual wallet changes. Local station quotes and resident receipts are bounded. Raw EndDay/legacy Visit/Bond/Training APIs do not universally inherit world proximity or receipt guards. Exception rollback, multithreaded transactions and exhaustive stock/upkeep/automation/event arithmetic remain unaudited. Preview extraction preserves prior work arithmetic, not proof of every extreme input.

## Continuity

Keep previous opening text/bedroom return, held-input repair, starting-fund tutorial construction, local station/resident guards, native locale choice, same/next-day saving, separate bath/rest bonuses, shared-evening receipts, combat costs/results and optional project navigation. Do not replace their fixtures with fabricated grants or treat synthetic UI setup as organic balance evidence. Original reference data remains read-only and schema16 personal saves must be preserved.

Importer provenance, complete runtime data/reference validation, original-engine differential parity and local development MCP request/auth/export hardening need separate work. Live Git and exact-head validation override stale KANBAN/WORK_LOG counts.
