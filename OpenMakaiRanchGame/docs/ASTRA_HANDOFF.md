# ASTRA Handoff

Checkpoint **2026-09-11**, verified code-inclusive head **`6af8466dc3bd4338339a6c9b79ea4dd344e6b9db`**. **1,728 smoke assertions, 240 rendered UI assertions and 48 Python launcher/evidence tests pass.** Rendered evidence contains 34 viewport PNGs. This is bounded gameplay/interface evidence, not certification that the entire remake or every device is finished. Documentation-only commits may follow; consult the live PR for subsequent exact-head results.

## Current branch and requirements

- Repository `Noa3/OpenMakaiRanch`; working branch **`fix/daily-gameplay-loop-20260911`**, **PR #13** against main. PR #12 was merged externally during this continuation at **`f420bee96e9b99a38ea1260705dbf1e4fe7030d6`**. That merge included the new failing baseline tests; PR #13 repairs them. Do not continue on the old PR #12 branch from stale chat notes.
- Read live Git before editing. Preserve concurrent changes; no force resets or automatic merge. The new branch followed main without divergence.
- **C# 12 only**, effective LangVersion 12.0, enforced by Directory.Build.props/targets and CI including deliberate preview denial. SDK **10.0.401**, primary target **net8.0**, Godot **4.7.2 Mono**. Linux rendered CI additionally installs the .NET 8 runtime; source/framework/engine versions were not upgraded here.
- Save schema **16**. D-011 targets fresh games while preserving current-version personal saves; no legacy-migration expansion. Original `eraMakaiRanch-game-eng-translation/` stays read-only.
- GameRoot and existing services remain the sole simulation/calendar/economy authority. No character identity/eligibility approvals or adult-specific assets were added.

## Latest gameplay continuation

**Night training:** one extra ranch-wide growth pass, not one pass per resident. Ordinary growth, fatigue/talent modifiers and rest-job exclusions remain. Reset HasGrownToday once at settlement start so a level-up in night training is not erased by ordinary growth.

**Complete daily accounting:** DailyGoldLedger observes payments already made by work/upkeep, shipments, events and milestones. It pays nothing and changes no reward rates. Report income/expenses/net and the overview's LastIncome/LastExpenses reconcile to the wallet. Capped event/milestone entries report actual credit rather than nominal awards. Existing int display fields remain bounded; an additional balance line records exact long totals at display limits. Exhaustive overflow of every upstream producer is not certified.

**Guarded decisions and completion:** ordinary AdvanceTime requires a valid Night plan. TryAdvanceTime and TrySelectNightAction validate captured session/day/phase and reject stale or repeated commands. The EndDay completion guard remains active through notifications/autosave, preventing synchronous observers from settling tomorrow or advancing its Morning. Observer NewGame/Load cannot attach/publish/save the old report into the replacement session. Raw EndDay retains explicit simulation-call compatibility; this is not universal idempotency for arbitrary sequential simulation calls, multithreaded transactions or exception rollback.

**Playable night planning:** Rest, Training and Admin stay available for revision until End Day. Their current effects are explained; choosing them does not immediately apply recovery, growth, workload reduction or stamina cost. Overview shows Plan Night until a choice exists and puts the planning card first. Retired view/session/day controls reject old callbacks. Bathing preserves an already selected Training/Admin plan and its separate next-day stamina bonus; without a plan a night bath still selects Rest.

**Night UI layout:** the header's status column has bounded width/lines with complete tooltip text, rather than growing vertically until content disappears. New nightly choices and the existing recovery card use the normal CardContent VBox inside their PanelContainer, so text/buttons no longer share an overlapping rectangle. Both planning and bath are exercised with viewport clicks, not merely emitted signals.

## Executed verification

All three workflows succeeded on **`6af8466dc3bd4338339a6c9b79ea4dd344e6b9db`**:

| Workflow | Run | Result |
| --- | --- | --- |
| Godot 4.7 Mono CI #581 | `34558277149` | success |
| Build Smoke Check #589 | `34558277154` | success |
| Rendered UI acceptance #29 | `34558277148` | success |

C# 12/preview denial, compilation, 48 launcher tests, verified engine import and isolated smoke pass. The downloaded smoke log contains **1,728 OK / zero failed assertions / one SMOKE PASS**, including all **36 day-contract checks** and the additional explicit-night save-fixture guard. Existing first-day, skip, leisure, combat, input, save and prior HUD/view-state checks remain.

The rendered JSON contains **240 passing checks / zero failures / one UI ACCEPTANCE PASS / 34 PNGs**. The previous 207 UI assertions remain, followed by 33 night/report checks: actual revisions among all three choices, separate hit targets, full status context, stale-control denial, actual prepared-bath click, End Day/report, matching wallet totals, next-day stamina bonus and current-schema save/load. Day-2 interface setup remains explicitly synthetic; no claim that this is an organically earned full playthrough. The prior resource-backed first-day/leisure walkthroughs remain separate.

Both downloaded archives were SHA-256 checked; the night-planning and daily-report screenshots were opened and inspected:

| Evidence | Artifact ID | SHA-256 |
| --- | --- | --- |
| Godot import/smoke | `10183422808` | `0b31f56d257b358472728cf2b8ab8c0f735a2831ecbd2167f18db817159a3bd9` |
| Rendered UI/source/screens | `10183428448` | `0efd594f124c9ef89b5fec5086d7b024e8d60008ae41b6011e84cc898b39223a` |

Smoke `.artifacts/godot/smoke-xltt6pti/console.log`: five expected invalid-save diagnostics, no out-of-tree transform errors. Import `.artifacts/godot/import-gg4bwbuj/console.log`: successful exit with the known EditorSettings shutdown diagnostic. UI evidence `godot/ui-tpwvo56m/`: no runtime ERROR/SCRIPT ERROR or prior native shutdown fatal; the unsupported-VSync driver warning remains. Logs were not filtered to manufacture success. Rendered artifacts expire after 14 days; regenerate missing evidence.

## Baselines and remaining limits

DAY_LOOP_VALIDATION.md records the nine pre-fix day-contract failures at f420bee, the corrected old save fixture, the long-header regression and the newly composed overlapping-night-card defect found during rendered testing. Original assertions and gameplay guards remain. The separate native shutdown failure was addressed by retiring/finalizing the temporary test world while the engine is alive; forced collection is test teardown only, not normal gameplay or proof that every engine lifecycle issue is fixed.

Prior gameplay remains: shared physical/Pause courier board and daily receipt; optional 40 G / 3 supplies quiet corner; up to 10 daily stamina recovery without walking tax/upkeep; independent prepared-bath bonus; voluntary shared activity guards; bounded saved receipts and actual resource-backed construction. Bench collision/sitting animation remain unfinished. Prior responsive menu, camera, help, input ownership and scale repairs are retained; UI_LAYOUT_ACCEPTANCE/HUD_MENU_VALIDATION record those historical scopes.

Next prioritize a complete rendered first-day/skip/combat/courier/leisure/save journey and physical mouse/controller/touch/Alt-Tab acceptance, followed by authored collision and obstacle-aware navigation. Selected menu actions and staged proximity are not proof of every action or traversable route. Final assets, weather readability, representative-hardware Forward+ low/high performance, long-term balance and original-engine differential parity remain open. See KNOWN_ISSUES for untouched importer/content-validation/MCP audit leads.

## Commands and safety

```bash
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python -m unittest discover -s Tools/Godot -p "test_*.py"
python Tools/Godot/launch.py --mode import
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/ui_acceptance.py --rendered
python Tools/Godot/launch.py --mode runtime --isolated
```

Smoke and the opt-in UI scenario write/delete disposable slot 99: use the isolated launchers, never raw test flags on personal saves. UI CI is read-only and uploads whitelisted evidence/source, not personal profiles. Temporary branch-scoped source patch helpers used exact blob guards, explicit staged paths and non-force pushes, removed themselves, and are absent from the final diff. Older KANBAN/WORK_LOG snapshots and PR #12 pending notes are historical, not current verification.
