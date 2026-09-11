# ASTRA Handoff

Checkpoint **2026-09-11**, verified code head **`2a958b1c4425e31acd48cf5f604cdda378fc4846`**. **1,731 smoke assertions, 316 rendered UI assertions and 48 Python launcher/evidence tests pass.** Rendered evidence contains 43 viewport PNGs. The build reports zero warnings and zero errors. This is bounded gameplay/interface evidence, not certification that the entire remake or every device is finished. Documentation-only commits may follow; consult the live PR for subsequent exact-head results.

## Current branch and requirements

- Repository `Noa3/OpenMakaiRanch`; working branch **`fix/daily-gameplay-loop-20260911`**, **PR #13** against main. PR #12 was merged externally at **`f420bee96e9b99a38ea1260705dbf1e4fe7030d6`**. That merge included failing baseline tests repaired by PR #13. Do not continue on the old PR #12 branch from stale chat notes.
- Read live Git before editing. Preserve concurrent changes; no force resets or automatic merge.
- **C# 12 only**, effective LangVersion 12.0, enforced by Directory.Build.props/targets and CI including deliberate preview denial. SDK **10.0.401**, primary target **net8.0**, Godot **4.7.2 Mono**. Linux rendered CI also installs the .NET 8 runtime; source/framework/engine versions were not upgraded.
- Save schema **16**. D-011 targets fresh games while preserving current-version personal saves; no legacy-migration expansion. Original `eraMakaiRanch-game-eng-translation/` stays read-only.
- GameRoot and existing services remain the sole simulation/calendar/economy authority. No character identity/eligibility approvals or adult-specific assets were added.

## Latest core playability continuation

**Usable sequential cards:** Adventure, Combat, Shop, Schedule, Research and Milestones no longer place multiple sequential controls in the same PanelContainer rectangle. A scoped composition pass supplies a VBox while preserving original nodes, subscriptions, order and logical focus. Already authored single-content cards and other intentional overlays remain untouched; composition is idempotent.

**Visible replacement commands:** a real rendered battle exposed an offscreen focused Back after Auto Finish disappeared. Replacement focus now scrolls into view after layout. Exact-key restoration still preserves deliberate manual scroll; stale session/revision/route/hidden-view protections remain.

**Compact mission guidance:** the guild displays canonical daily stamina/capacity, ordinary entry cost, automatic-all versus selected party and free preparation. Detailed tooltip help distinguishes daily stamina from combat resources and explains tactical/automatic resolution and returning from results. No duplicate stamina rules or combat tuning were added.

**Actual connected menu journeys:** Road Patrol preparation, Tactical Battle, Defend, Attack, Auto Finish, Back and Return to World are exercised with viewport clicks. Checks cover one-time entry cost, time lock, player-authored actions, no duplicate rewards, restored world input and battle-wear persistence. Store purchase, worker assignment, milestone layout and actual Slot 3 Save/Load are exercised afterward. An unsaved second purchase proves that Load genuinely restores wallet/inventory/job, rather than merely returning success on unchanged state.

See **CORE_PLAYABILITY_VALIDATION.md** for exact artifacts, reproduced failures, the corrected headless fixture, files changed and limits. The new journeys inherit synthetic Day-2 resources/strong stats; they are not combat balancing or an organically earned start-to-finish playthrough.

## Prior day-loop repairs retained

**Night training:** one extra ranch-wide growth pass, not one per resident. Ordinary growth, fatigue/talent modifiers and rest-job exclusions remain. Reset HasGrownToday once at settlement start so a night-training level-up is not erased by ordinary growth.

**Complete daily accounting:** DailyGoldLedger observes payments already made by work/upkeep, shipments, events and milestones. It pays nothing and changes no reward rates. Report income/expenses/net and LastIncome/LastExpenses reconcile to the wallet. Capped event/milestone entries report actual credit; bounded int fields retain an exact long balance line at display limits. Exhaustive overflow of every upstream producer is not certified.

**Guarded decisions/completion:** ordinary AdvanceTime requires a valid Night plan. Captured session/day/phase guards reject stale or repeated commands. The EndDay completion guard remains active through notifications/autosave, preventing synchronous observers from settling tomorrow or advancing its Morning. Observer NewGame/Load cannot publish/save the old report into a replacement session. Raw EndDay retains simulation-call compatibility, not universal idempotency for arbitrary sequential calls, multithreaded transactions or exception rollback.

**Playable night planning:** Rest, Training and Admin remain revisable until End Day; selecting them does not immediately apply recovery, growth, workload reduction or stamina cost. Overview shows Plan Night until a choice exists and puts planning first. Old controls reject stale callbacks. A bath preserves an already selected Training/Admin plan and the separate next-day stamina bonus; without a plan a night bath still selects Rest.

**Night layout:** bounded header text keeps content reachable with full tooltip context. Night choices and recovery use vertical card content. Actual revisions, bath, End Day/report and current-schema saving remain tested. DAY_LOOP_VALIDATION.md retains prior scope and baseline receipts, including the nine reproduced day-contract failures and intermediate layout/shutdown defects.

## Executed verification

All three workflows succeeded on **`2a958b1c4425e31acd48cf5f604cdda378fc4846`**:

| Workflow | Run | Result |
| --- | --- | --- |
| Godot 4.7 Mono CI #591 | `34587496873` | success |
| Build Smoke Check #599 | `34587496874` | success |
| Rendered UI acceptance #34 | `34587496917` | success |

C# 12/preview denial, compilation, 48 Python tests, verified engine import and isolated smoke pass. Downloaded logs contain **1,731 OK / zero failed smoke assertions / one SMOKE PASS**. The rendered JSON has **316 passing checks / zero failures / 43 PNGs**. This adds three headless assertions and 76 rendered assertions/nine PNGs to the prior 1,728/240/34 checkpoint; previous assertions remain.

Both final downloaded archives were SHA-256 checked; selected guild/combat/store/schedule/save screenshots were opened and inspected:

| Evidence | Artifact ID | SHA-256 |
| --- | --- | --- |
| Godot import/smoke | `10194262176` | `576c5c344957b472ed671b0a52318edd84395a17360a34bc1687e83a9a2788fd` |
| Rendered UI/source/screens | `10194281155` | `7a36cebde5371c8e952c7f862a04d9aa4fcc685889e80f0585e938a11ff199e8` |

Smoke retains five expected invalid-save diagnostics; import exits successfully with the known EditorSettings shutdown diagnostic. Rendered evidence has zero runtime ERROR/SCRIPT ERROR; the unsupported-VSync driver warning remains. Logs were not filtered to manufacture success. The earlier native C# shutdown failure was addressed in test-only teardown; this is not proof that all engine lifecycle failures are fixed. Rendered artifacts expire; regenerate missing evidence. Included whitelisted review source is not a full standalone game/export.

## Remaining priorities and limits

Prior gameplay remains: shared physical/Pause courier board and daily receipt; optional 40 G / 3 supplies quiet corner; up to 10 daily stamina recovery without walking tax/upkeep; independent prepared-bath bonus; voluntary shared activity guards; bounded saved receipts and resource-backed construction. Bench collision/sitting animation remain unfinished. Previous responsive menu, camera, help, input ownership and scale repairs are retained.

Next prioritize an uninterrupted organically earned first-day/skip/world/combat/courier/leisure/save journey, plus physical mouse/controller/touch/Alt-Tab acceptance and authored collision/obstacle-aware navigation. The selected rendered mission and management paths are now covered, but not every action, dungeon, outcome or traversable route. Research receives the layout repair without a complete research-action journey. Final assets, weather readability, representative-hardware Forward+ performance, long-term balance and original-engine differential parity remain open. KNOWN_ISSUES retains untouched importer/content-validation/MCP audit leads.

## Commands and safety

```bash
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python -m unittest discover -s Tools/Godot -p "test_*.py"
python Tools/Godot/launch.py --mode import
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/ui_acceptance.py --rendered
python Tools/Godot/launch.py --mode runtime --isolated
```

Use isolated launchers, never raw test flags on personal saves. Smoke/UI fixtures use disposable slot 99; the extended UI journey also writes/deletes slot 3 to exercise authored Save/Load buttons, validates profile isolation first and refuses an occupied slot. UI CI uploads whitelisted evidence/source, not personal profiles. Older branch-scoped patch helpers removed themselves and are absent from the final diff. KANBAN/WORK_LOG snapshots and PR #12 pending notes are historical, not current verification.
