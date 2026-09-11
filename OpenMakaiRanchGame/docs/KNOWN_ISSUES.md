# Known Issues

Updated **2026-09-11**, PR #15, branch `feature/world-stations-and-interiors-20260911`. Verified code head **`4d3c9893a4621bd09f1e358e178f0f671ba18537`**: **1,811 smoke assertions, 470/470 rendered checks, 55 Python tests, 59 PNGs**, all three CI workflows successful. RESIDENT_INTERACTIONS_VALIDATION.md records current evidence and first-run fixture failures; earlier design/localization receipts remain in their own documents. PR #13 is merged; PR #14's test-only baseline is included and repaired here.

## Open issues

### RESIDENT-001 — bounded ordinary interactions, not the entire original catalog

Free contextual conversation, encouragement, meals, eight allowed ordinary gifts, recovery, mentoring and four practical lessons are implemented through resident-focused pages and existing services. The addressed NPC waits; close releases it. Hidden/remote/stale/reentrant commands, exact costs, daily receipts and next-day save/load are tested. Invalid/capped/overflowing training focuses cannot charge first, and recovery no longer lowers above-capacity energy boosts.

This does not complete individual personality arcs, practice/talking animations, equipment presentation, all gift preferences or every incapacitated-character recovery path. Unwell residents decline this action route; broader hospital/rest recovery and long-term balance need separate acceptance. The practical lesson path is not the adult-action catalog. Raw Visit/Bond/Training APIs used by internal legacy callers/mods do not all inherit the world's receipt/proximity/reentrancy boundary. See RESIDENT_INTERACTIONS.md and the neutral NSFW_CONTENT_HANDOFF.md; no blanket renaming, character approval or new explicit content was supplied.

### DESIGN-001 — motivating campaign and relationship content

Four optional projects are guidance, not a complete campaign. An earned first-week balancing run, resident aspirations, distinct dialogue and visible chapter payoff remain open. Main win conditions are unchanged. Existing shared nights use reviewed adult definitions/state and voluntary romantic eligibility; stock content may stay unavailable pending individual review. AUTHOR_CONTENT_HANDOFF.md covers missing presentation and a separate unimplemented family-planning system. No pregnancy trigger, explicit prose/animation/audio or coercive sexual route was added.

### TOOLS-003 — warnings and lifecycle

Build success does not remove known developer-local NuGet source warnings or the nullable warning in RanchLeisureFrameTests. Import exits successfully but logs the known EditorSettings shutdown diagnostic; software-rendered UI retains VSync warnings. Current smoke has five intentional invalid-save errors, zero failed assertions and one PASS. Rendered runtime has zero ERROR/SCRIPT ERROR. Test-only PackedScene retention/staged teardown is not normal-game GC or proof of all engine shutdown paths. No new log filtering was added to manufacture a pass.

### I18N-001 — partial translation

The English/German world UI slice now contains **309 matching keys**: the previous 247 plus 62 resident control/reason/gift/outcome entries. Full sentences, stable IDs and checked placeholders remain. German resident pages pass selected 640x480/480x800 layout checks; language refresh preserves page/target and discards stale-language feedback. Existing Japanese entries remain, with English fallback for missing new keys. Character creation, authored stories, retained services and many old result messages remain untranslated. Saved report strings keep their original language.

Plurals, RTL, additional scripts/fonts, subtitles/voice and language-specific physical-device tests remain open. Actual title/Options pickers and language-only settings preserve window mode, state and existing contexts. Five export filters include JSON, but an actual export localization playthrough remains required. TRANSLATION_GUIDE.md forbids translating persistent IDs/player names. LOCALIZATION_ROADMAP.md plans ten localized editions excluding Russian using a Steam-client-language proxy, not an all-gamers census; future locales require real text and region/script-aware normalization.

### UI-PLAY-001 — creator and remaining presentation density

The stepwise creator is not yet implemented. The main menu has its own procedural diorama and localized actions but still uses placeholder art and covers much of a small viewport. Physical stations/residents and read-only Places replace the player-facing global hub; some service renderers remain context-restricted instead of deleted.

Resident results now scroll into view and repeated identical daily-limit explanations are consolidated, with per-button tooltips retained. Nevertheless, instructional text occupies much of the 640x480 practice view and choices still require scrolling. This is usable selected geometry, not final interaction ergonomics. Large 3D labels, HUD/tutorial overlap and duplicated shared-evening planning status remain separate visible polish issues.

An uninterrupted earned first-day/skip/world/courier/leisure/resident/save route, every service action/mission outcome, physical keyboard/controller/touch/Alt-Tab/deadzones and camera feel remain incomplete. Later fixtures stage Day-2 resources/stats/proximity; shared-night fixtures use separate temporary adult identities. They are not natural progression or shipped-character approval.

### WORLD-NAV-001 — building stages and all-route acceptance

Walk-in shells have doorways, wall/furniture collision, roof/wall cutaways, shelter and collision-derived navigation. Selected rays and actual held-key doorway movement pass without teleport or walking stamina tax. Eight plots reserve rotated roof envelopes, entrances and the town gate; static imported/fallback tree bounds respect them. Stable visual grades through extreme numerical levels are tested.

Free placement, annexes/upper floors, every doorway/character size, other props, animated shader/skeleton bounds, camera angles, NPC avoidance/stuck recovery and full-route connectivity remain unaudited. Conversation waiting is not a general NPC navigation repair. The leisure bench still lacks final collision/seated animation. Additional facility types need authored plots; arbitrary shell scaling is not a valid expansion.

The title water strip is not a playable organic town river/bridge/landmark system. Final buildings/nature/character/morph assets, weather readability, LOD/memory budgets and representative-hardware Forward+ low/high performance remain open. No new external asset or production character design approval was added.

### GAMEPLAY-BALANCE-001 — selected rules rather than full progression

First-day construction uses starting funds; held input survives hints and supply planning only marks Places. One-pass night training, daily growth markers, editable plans, independent bath bonus and once-daily delivery remain. Quiet-corner restoration stays 40 G/3 supplies/up to 10 daily stamina without time advance/upkeep. Projects add no new rewards or timers. Shared nights record one modest +1 Bond/+2 Morale acknowledgement without duplicate rest/bath/energy payment.

Resident care and gifts now have per-resident daily limits in the new world route. Practice allows one lesson per resident and two globally, charging existing stamina plus resident energy/fatigue; mentoring is separate. This is not a final seven-day balance, romance or main-victory certificate. Facility numerical levels remain independent of visual grade 3 saturation.

### SETTLEMENT-BOUNDARY-001 — limited transaction guarantees

Captured time commands reject missing plans, stale/replayed/reentrant advances and cross-session reports. Shared nights attach to normal successful settlement; resident actions keep a busy guard through notifications. Raw EndDay is not universally idempotent, and raw legacy services are not all transactional. Exception rollback, multi-threading and exhaustive upstream overflow/accounting remain unaudited. Facility upgrade and practical lesson arithmetic have specific pre-payment guards; upkeep/automation/other producers still need review.

## Evidence and audit leads to preserve

RESIDENT_INTERACTIONS_VALIDATION.md records the current 33 numeric/38 rendered additions, outcome visibility and the five initial fixture failures. Existing assertions were retained when their living-trainee/missing-meal preconditions were fixed. RANCH_DESIGN_VALIDATION.md retains projects/plots/shared-night receipts; LOCALIZATION_UI_VALIDATION.md retains real native picker, intro, held-input, Build/Assign and doorway evidence. Earlier German-named screenshots still showing English are not localization acceptance.

PR #13 card/focus/night-growth/ledger/planning, adventure and actual Save/Load evidence remain in CORE_PLAYABILITY_VALIDATION.md and DAY_LOOP_VALIDATION.md. Responsive scaling, HUD ownership, input capture, pre-mutation save rejection, voluntary gates and stored-mana rules remain. Original-source and eligibility protections were not relaxed.

Importer recovery/provenance, complete runtime JSON/reference validation, original-engine differential parity and development MCP authentication/request-size/export hardening remain separate work. Preserve schema-16 personal files and read-only original data. Live Git/ASTRA_HANDOFF supersede historical KANBAN/WORK_LOG counts.
