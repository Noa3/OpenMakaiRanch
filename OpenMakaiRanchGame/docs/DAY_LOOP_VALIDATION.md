# Daily gameplay loop continuation

2026-09-11, PR #12, `playability/ranch-quiet-corner-20260910`. C# 12 remains enforced; engine, framework and schema 16 are unchanged. Post-fix source `3191c3aaf2ebe1b067742721c29f8cb99d7944be` requires its own executed CI before this continuation is review-ready.

## Executed pre-fix baseline

Head `f420bee96e9b99a38ea1260705dbf1e4fe7030d6`, Godot CI #565 / run `34555543717`: compilation and all 48 Python launcher/evidence checks passed. The full smoke then produced **1,701 OK and nine failing assertions**. Existing assertions were retained. Artifact `10182455368`, SHA-256 `d2b872a441390f6d6e779a03d7f1d9d7a763f61c58d34bdfc7fe08f74602b`, was downloaded and inspected.

The failures reproduce roster-multiplied training (4 and 8 workers), loss of a same-day growth mark, missing event money in reported net, phantom positive event credit at the wallet cap, Night advancement without a plan, selecting a night action during Morning, recursive day settlement, and reentrant advancement of the following Morning. The normal one-worker training fixture and the original gameplay checks remain passing.

The separate rendered baseline #19 / `34555543731` completed all 207 UI assertions but **failed at shutdown**, with the native C# bridge reporting remaining script bindings and unsafe references. Artifact `10182459376`, SHA-256 `31bdff8ae1e96cd565cf97f3bfa4e60e8baac4094c20fefcb00fae6e6d844154`, records that distinct failure. It is not a gameplay assertion failure and was not filtered or reclassified as success.

## Implemented corrections under verification

Training now adds one extra ranch-wide growth pass, not one for each resident. Ordinary daily growth, fatigue/talent modifiers and rest-job exclusions remain unchanged. HasGrownToday is reset once before settlement, preserving a level-up in either pass.

DailyGoldLedger observes already-applied work/upkeep, shipments, events and milestone payments. It pays nothing and changes no reward rates. Report income/expenses/net and the overview's last income/expenses reconcile to the actual wallet. Event and milestone messages use actual credited gold at saturation. Existing int report fields remain bounded; an additional balance line retains exact long totals when display limits are reached. This is not a new financial/save authority.

Normal AdvanceTime rejects an unplanned Night. Captured session/day/phase commands reject stale/replayed inputs. EndDay retains explicit simulation-call compatibility while its synchronous completion guard prevents an observer settling tomorrow or advancing the new Morning. Session replacement during a callback cannot publish/save the old report into the new session. No general exception-rollback or concurrent-thread transaction guarantee is implied.

Night planning exposes Rest, Training and Admin with effect explanations and permits changing the choice until End Day. Ranch Overview places it before the other cards, and both overview/top-bar time controls route through the same guard. Queued/retired controls reject old view/session/day/phase calls. Selection itself grants no growth/recovery and spends no stamina. A prepared bath keeps its independent next-day stamina bonus without replacing an already selected Training/Admin plan; an unplanned bath still defaults to Rest.

## Additional coverage

The added DayCommandRegressionTests exercise current/stale/duplicate choices, notifications, combat locks, phase/session replay, selected-plan bath behavior, observer session replacement, growth resets/exclusions, bounded ledger and capped milestone messages. The original nine failing assertions are retained.

The opt-in rendered suite adds actual nightly choice revisions, Plan Night, one End Day/report, exact wallet totals and current-schema save/load at 640x480/960x540. These follow the unchanged layout/no-gameplay-mutation assertions. The Day-2 fixture is explicitly synthetic; the settlement itself uses normal work output and services. Existing first-day/leisure smoke walkthroughs remain the organically resource-backed tests. Queued-control signal tests are labeled separately from real viewport mouse events.

The opt-in acceptance scenario now finishes before teardown retires its temporary world and allows C# finalization while the engine is still alive. Forced collection is restricted to test teardown, not runtime gameplay. This is under verification against the observed shutdown failure and is not a claim that all engine lifecycle issues are solved.

## Limits

No new character approval, adult-specific asset, reward formula, original-source edit or legacy-save expansion. Current personal saves remain protected by isolated test launchers; test slot 99 is disposable. Physical controller/touch testing, full rendered first-day journeys, authored navigation/collision, final assets, Forward+ hardware performance, original-engine parity and long-term balance remain open. Final exact-head results belong in this page, ASTRA_HANDOFF and the PR before marking ready.
