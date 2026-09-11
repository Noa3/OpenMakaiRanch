# ASTRA Handoff

Checkpoint **2026-09-11**, verified code head **`4d3c9893a4621bd09f1e358e178f0f671ba18537`**: **1,811 passing smoke assertions, 470/470 rendered UI checks, 55 Python tests and 59 PNGs**. All three CI workflows succeeded. **RESIDENT_INTERACTIONS_VALIDATION.md** records exact runs, merge/source receipt, inspected screenshots, archive hashes and failed fixture baselines. Documentation-only commits may follow. RANCH_DESIGN_VALIDATION.md and LOCALIZATION_UI_VALIDATION.md retain their preceding scopes instead of being rewritten as evidence for later code.

## Branch and requirements

- Continue on **`feature/world-stations-and-interiors-20260911`**, **PR #15**, repository `Noa3/OpenMakaiRanch`. Main remains at merged PR #13 `3209f4c`. PR #14's test-only intro baseline is included and repaired here; do not merge it separately.
- Read live Git before editing. Preserve concurrent changes; no force push or automatic merge. Temporary exact-preimage patch helpers remove themselves, never become ordinary runtime/workflows.
- **C# 12**, SDK **10.0.401**, target **net8.0**, **Godot 4.7.2 Mono**, save schema **16**. Separate unchanged contracts. Fresh games are expected (D-011), current saves must work, personal files must not be deleted. Original reference content remains read-only.
- GameRoot and existing services remain the sole simulation/economy authority. No explicit assets, source identity/role relabeling or production character eligibility approvals were added.
- User requires translation-aware UI and a later ten-target plan excluding Russian. Use LocaleCatalog, full keyed sentences, stable IDs, adaptable layouts and checked placeholders. Read TRANSLATION_GUIDE.md/LOCALIZATION_ROADMAP.md. Current en/de/ja availability is not complete-game translation.

## Latest resident continuation

**Contextual pages:** approach a resident for free conversation, then Care, Practice, Company or Work; gifts have a separate list. The old global/adult-training screen is not opened by this route. The owner record redirects to the ranch house rather than yielding self-care relationship rewards. The addressed NPC pauses and resumes its existing target after closing; work and output are untouched.

**Ordinary actions:** conversation is free/read-only; encouragement, a meal, an allowed ordinary gift, recovery and practical advice reuse Visit/Bond effects. Fixed per-resident last-day flags prevent repeats without growing histories. Ordinary friendship/care/practice is separate from adult/romantic eligibility. Existing voluntary companionship and shared-night gates remain.

**Practical training:** ranch/craft/combat/magic use TrainingService, one focus per resident/day and the existing two-session ranch budget. Current cost is 20 player stamina plus 10 resident energy and talent-adjusted fatigue. Mentoring is separate. Invalid/capped/overflowing focuses are rejected before effects; trainees must be living, recovered enough and not in the Night phase through the new resident entry point. The old invalid-focus-before-cost bug is repaired. Meal/recovery respects definition/override capacity without reducing existing boosts.

**Command ownership:** root checks generation/day/phase, combat, pause and settlement, remaining busy through StateChanged. World checks matching visible resident context and proximity. Old/hidden/remote/reentrant commands cannot spend items/stamina/receipts. Receipt saving and normal next-day reset use existing services; no second clock or daily production payment. Raw legacy service callers remain outside the new world's full transaction guards.

**Translation/presentation:** 62 matching English/German keys extend the world slice to **309**. Outcomes use full keyed templates, not parsing legacy English return strings. Long results stay in scrollable content; a completed action scrolls its outcome into view. Repeated identical limit messages appear once, with per-button reasons preserved in tooltips. Page and target survive language changes; old-language feedback is cleared. German layouts pass 640x480/480x800, but instructional density remains high at the smaller size.

Read RESIDENT_INTERACTIONS.md for contracts and NSFW_CONTENT_HANDOFF.md for omitted content/technical locations. The latter is a neutral task handoff, not explicit scene scripts. All original role names and source IDs were preserved. No family-state implementation was added.

## Prior design/world work retained

Places offers four optional state-derived projects: restore the quiet corner, kitchen level two, three community deliveries and first expedition. Progress/tracking adds no reward authority, timer, remote assignment or victory condition. Missing supplies direct to Office Work. A first-week chapter, resident aspirations and visible celebration remain design work.

Shared nights remain optional, voluntary and restricted by reviewed adult state/definition, a romantic bond, companion wellbeing, Night and Rest. Invitations are free/cancellable. Ordinary settlement records one receipt and +1 Bond/+2 Morale without duplicating rest/bath/stamina/gold. Existing transition shows a black screen, heart and non-explicit morning text, respecting Reduced Motion. Pending/completed receipts survive save/load. Production characters are not approved by synthetic test identities; no pregnancy trigger exists.

Eight fixed metre-scale plots protect roofs, entrances and the town gate; interior visual grades 0-3 do not enlarge the shell. Upgrade arithmetic rejects unrepresentable transactions before payment. Imported/fallback tree geometry respects reservations. This is not free placement, all-prop/all-route certification or finished annexes/upper floors.

InputBindingService keeps ordinary HUD reads from rebuilding InputMap; real held-key doorway traversal remains tested. The first-day Dairy sequence explains and uses actual starting-fund construction before staffing. Physical supply planning marks Places without granting supplies. Opening text completion, same-bedroom utility return, story callback guards and forward-facing movement remain.

The private title diorama uses its own bounded SubViewport/world/camera and Reduced Motion. Native language-picker tests preserve window settings and live state; language-only persistence does not reapply graphics/audio/input. Five export presets include raw JSON. Walk-in shells have doorways, wall/furniture collision, shelter, cutaways and collision-derived navigation; selected Build/Assign/ray/traversal/remote-denial checks remain. Large world labels and HUD/tutorial clutter are unfinished.

PR #13 night-growth/ledger/planning/reentrancy, adventure costs/results and actual store/save paths remain documented in DAY_LOOP_VALIDATION.md and CORE_PLAYABILITY_VALIDATION.md.

## Current verification and limits

Head `4d3c989`: Build #651 **34619090555**, Godot #643 **34619090565**, UI #81 **34619090773**, all success. PR merge **`046d96f608f8595ea0aaca852f2e6c1768df7c29`** appears in the review-source receipt. There are **1,811 SMOKE OK / zero FAIL / one PASS**, **470 passing rendered checks / zero UI runtime errors / 59 PNGs**, and **55 Python tests**. This adds 33 numeric checks, 38 rendered checks and three captures over the design checkpoint. Actual archive hashes/paths are in RESIDENT_INTERACTIONS_VALIDATION.md.

The first resident smoke run failed five assertions because an old budget fixture retained previous battle injuries and a new missing-item fixture retained starting meals. Preconditions were made explicit; assertions and runtime protections were not removed. A successful local full Python retry took 36.940 seconds; the earlier timed-out attempt is not a pass. No local engine/compiler run is claimed.

The resident UI journey stages proximity/resources/tiredness/skill, buys meal/journal prerequisites through canonical shop transactions, clicks actual resident actions, exercises language/reentrancy/hidden/remote guards, saves/loads, sleeps through the house and verifies the next morning's budget and availability. This is not an organically earned week or physical-device test. Prior shared-evening fixtures use separate temporary adult identities and correctly persisted/restored profile settings, not production approval.

Smoke retains five intentional invalid-save diagnostics. Import retains the known EditorSettings shutdown diagnostic; software-rendered UI retains VSync warnings. Build success is not a zero-warning or universal lifecycle claim. Test-only scene retention/teardown is not ordinary gameplay GC.

Next prioritize an earned first-week balancing/content pass and remaining ordinary resident recovery/equipment/story paths, then stepwise translated creation, remaining service/result localization, world label/HUD density and complete NPC/camera traversal. Organic town/river, final animations/building stages/assets, all languages/scripts/plurals/RTL, exported builds, physical inputs, Forward+ hardware performance and original-engine parity remain open.

## Safe commands

```bash
python Tools/Godot/validate_locales.py
python -m unittest discover -s Tools/Godot -p "test_*.py"
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/ui_acceptance.py --rendered
python Tools/Godot/launch.py --mode runtime --isolated
```

Use isolated launchers only: acceptance writes/deletes slots 99 and 3 and the new resident scenario refuses an occupied slot 99. Artifacts contain review inputs/evidence, not a standalone game export. Live Git/exact-head receipts supersede old KANBAN/WORK_LOG counts.
