# Daily gameplay loop continuation

2026-09-11, **PR #13**, branch `fix/daily-gameplay-loop-20260911`. PR #12 was merged externally during this continuation at baseline `f420bee96e9b99a38ea1260705dbf1e4fe7030d6`; the current changes follow it without rewriting history. C# 12, the engine/framework pins and save schema 16 remain unchanged. Current post-header source `116157566fb209e6d8dfaea6c6c4aed386fdc41e` requires exact-head CI before review readiness.

## Executed pre-fix baseline

Head `f420bee96e9b99a38ea1260705dbf1e4fe7030d6`, Godot CI #565 / `34555543717`: compilation and all 48 Python launcher/evidence checks passed. The smoke produced **1,701 OK and nine failed assertions**. Artifact `10182455368`, SHA-256 `d2b872a441390f6d6e779a03d7f1d9d7a763f61c58d34bdfc7fe08f74602b`, was downloaded and inspected.

The failures reproduce roster-multiplied training (4 and 8 workers), loss of a same-day growth mark, missing event money in report net, phantom capped event credit, Night advancement without a plan, selecting a night action during Morning, recursive settlement, and reentrant advancement of the following Morning. Original gameplay assertions and the one-worker training fixture were retained.

The rendered baseline #19 / `34555543731` completed all 207 UI assertions but **failed at C# bridge shutdown** with remaining script bindings/unsafe references. Artifact `10182459376`, SHA-256 `31bdff8ae1e96cd565cf97f3bfa4e60e8baac4094c20fefcb00fae6e6d844154`, records that distinct failure. It was not filtered or reclassified as a pass.

## First post-fix result and follow-up

Head `8c6189e5591314b10d2b0d9175054b0c4153c209`, Godot CI #571 / `34556984511`: **all 36 new day-contract checks pass**, but the total is **1,724 OK / three failures**. Those three belong to an old save-round-trip fixture that advanced out of Night without selecting an action. It now explicitly selects Rest before its fourth phase advance and asserts the choice was accepted. All three original day/phase/save assertions remain; the runtime guard was not weakened.

Post-fix smoke artifact `10182960033`, SHA-256 `e18efcefab2322fc3d07bbaed170fd1984388515a3933084df57eeddfcc54032`, log `.artifacts/godot/smoke-7bje854j/console.log`.

Rendered UI #23 / `34556984450` retained the 207 earlier passing UI assertions but failed 17 of 231 total checks while attempting the new night flow. A long Plan Night status message exposed a real zero-minimum-width header bug: the status column wrapped vertically until the header consumed the scroll viewport. Actual night buttons became unreachable and subsequent plan/report assertions failed. Header-specific minimum width and line limits now preserve content space while the complete status remains in its tooltip. Two new assertions require usable content height and preservation of the full status text. Body text keeps its natural wrapping.

Rendered artifact `10182960381`, SHA-256 `9eabc195a387d437ba3ccd54dbe6c2d4ed3bd60021fb518640ccb3b75fe3e5f1`, evidence `godot/ui-65hbx7_2/`. The failing night screenshot and geometry were inspected. This post-fix UI run had zero runtime ERROR/SCRIPT ERROR lines and no prior shutdown fatal, but its failed UI assertions still make the run a failure.

## Implemented gameplay changes

Training contributes one extra ranch-wide growth pass independent of resident count. Ordinary daily growth, fatigue/talent modifiers and rest-job exclusion remain. HasGrownToday resets once before settlement, preserving a level-up in either pass.

DailyGoldLedger observes already-applied work/upkeep, shipments, events and milestone payments; it pays nothing and changes no reward rates. Report income/expenses/net and overview totals reconcile to the actual wallet. Saturated event/milestone messages use actual credit. Existing int report fields stay bounded; a balance line retains exact long totals if display limits are reached. This is not a new financial/save authority.

Normal AdvanceTime rejects unplanned Night. Captured session/day/phase commands reject stale/replayed inputs. EndDay retains explicit simulation-call compatibility while the synchronous completion guard prevents an observer settling tomorrow or advancing the new Morning. Session replacement during a callback cannot attach or autosave the old report into a new session. No general rollback or concurrent-thread transaction guarantee is implied.

Night planning explains Rest, Training and Admin and permits revising the choice until End Day. Ranch Overview presents the planning card before other cards; overview/top-bar time controls share one guard. Queued/retired controls reject old view/session/day/phase calls. Selecting a plan itself grants no recovery/growth and spends no stamina. Prepared bathing retains the next-day bonus without replacing a chosen Training/Admin plan; an unplanned bath still defaults to Rest.

## Test scope and remaining limits

DayCommandRegressionTests covers valid/stale/duplicate choices, one notification per change, combat locks, phase/session replay, selected-plan bathing, observer session replacement, growth reset/exclusion, bounded ledger and capped milestone messages. Original failed assertions remain.

The opt-in rendered scenario adds actual nightly choice revisions, Plan Night, End Day/report, wallet reconciliation and current-schema save/load at 640x480/960x540. It follows the unchanged layout/no-mutation checks. Day-2 state is explicitly synthetic; settlement uses existing work/services. The original first-day/leisure walkthroughs remain the resource-backed tests. Queued-signal rejection checks are labeled separately from viewport clicks.

Acceptance-only teardown retires the temporary world and permits C# finalization while the engine is alive. Forced collection is not part of ordinary gameplay or proof that every engine-lifecycle problem is fixed. Native errors and failed assertions remain fail-closed.

No original-source edits, character approvals, adult-specific assets, reward-rate changes or legacy expansion. Test slot 99 is disposable and used only through isolated launchers. Physical controller/touch/Alt-Tab, full rendered first-day journeys, authored collision/navigation, final assets, Forward+ hardware performance, original-engine parity and long-term balance remain open. Record final executed results here, in ASTRA_HANDOFF and in PR #13 before review readiness.
