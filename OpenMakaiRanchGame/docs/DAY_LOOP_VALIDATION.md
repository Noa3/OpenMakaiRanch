# Daily gameplay loop continuation

2026-09-11, **PR #13**, branch `fix/daily-gameplay-loop-20260911`. PR #12 was merged externally during this work at baseline `f420bee96e9b99a38ea1260705dbf1e4fe7030d6`; this continuation follows without rewriting history. C# 12, pinned engine/framework and save schema 16 remain unchanged. Current card-layout source `42b33d6e92fbcaab7a92bd96529d5daae2011091` requires exact-head verification before review readiness.

## Executed failure baseline

Head `f420bee`, Godot CI #565 / `34555543717`: compilation and 48 Python tests passed; smoke **1,701 OK / nine failed assertions**. Downloaded artifact `10182455368`, verified SHA-256 **`a222f1580d6eb0c7d123d2982e1e7942e20563da6024fe39b23e22fb2bd7a65b`**. This hash corrects an earlier documentation transcription; it agrees with the live artifact metadata and downloaded bytes.

The nine failures reproduce training multiplied by roster size (4 and 8 workers), a lost same-day growth mark, event money omitted from report net, phantom capped event credit, unplanned Night advancement, Morning night selection, recursive settlement and reentrant next-Morning advancement. All original assertions remain.

The same baseline's rendered #19 / `34555543731` completed all 207 UI assertions but failed at native C# bridge shutdown. Artifact `10182459376`, SHA-256 `31bdff8ae1e96cd565cf97f3bfa4e60e8baac4094c20fefcb00fae6e6d844154`. Remaining script bindings/unsafe references were not filtered or reclassified as a pass.

## Executed post-fix stages

At `8c6189e`, Godot CI #571 / `34556984511` passed all **36 new day-contract checks**, but total smoke was 1,724 OK / three failures. An old save-round-trip fixture advanced Night without choosing work. It now explicitly selects Rest and asserts acceptance before the fourth phase advance; its original day/phase/save assertions remain. Runtime guards were not weakened.

That smoke artifact `10182960033` has SHA-256 `e18efcefab2322fc3d07bbaed170fd1984388515a3933084df57eeddfcc54032`. The later **Godot CI #576 / `34557814289` at `3e4f001` completed successfully**, including the full smoke, explicit night-plan save fixture and all earlier tests. Final counts still require the final-head artifact inspection below.

Rendered #23 / `34556984450` retained the original 207 passing UI checks but failed 17/231 while exercising the new Night path. A long status message made the header's zero-minimum-width column wrap vertically until it consumed the scroll viewport. Header-specific width/line bounds now preserve usable content space; the complete status remains in a tooltip and body wrapping is unchanged. Tests explicitly check both facts.

Rendered #26 / `34557814188` at `3e4f001` passed those header assertions and completed real settlement/report/save-load, but still failed nine of 238 UI checks. Inspection showed the newly composed night buttons sharing one rectangle: CardContainer is a PanelContainer, not a vertical list. The current correction uses the existing CardContent VBox pattern and adds a non-overlapping-target assertion. The existing recovery card had the same direct-panel-child problem and now also uses vertical content. The rendered bath step now clicks its actual viewport target instead of calling the root method directly. No failed assertions were removed.

Rendered #23 artifact `10182960381`, SHA-256 `9eabc195a387d437ba3ccd54dbe6c2d4ed3bd60021fb518640ccb3b75fe3e5f1`, evidence `godot/ui-65hbx7_2/`. Rendered #26 artifact `10183258800`, SHA-256 `7469f117a4bca4719e9ef06995a2e0bd4f5ea4c394c72a7d0f8c6625946708c5`, evidence `godot/ui-20lod5bv/`. Failing screenshots and geometry were inspected. Both runs had no runtime ERROR/SCRIPT ERROR or previous shutdown fatal; their UI failures still make them failed runs.

## Implemented gameplay

Night training adds one extra ranch-wide growth pass independent of resident count. Ordinary growth, fatigue/talent modifiers and rest-job exclusion remain. HasGrownToday resets once before settlement, preserving growth from either pass.

DailyGoldLedger observes already-applied work/upkeep, shipments, events and milestones; it pays nothing or changes reward rates. Report and overview totals reconcile to the wallet. Saturated event/milestone messages use actual credit. Existing int fields stay bounded; a balance line retains exact long totals at display limits. No second financial/save authority is introduced.

Normal AdvanceTime rejects unplanned Night. Captured session/day/phase commands reject stale/replayed inputs. Explicit simulation EndDay compatibility remains, while its synchronous completion guard prevents observers settling tomorrow or advancing the new Morning. Observer session replacement cannot attach/autosave the old report into a new session. General exception rollback and concurrent-thread transaction guarantees are not claimed.

Rest, Training and Admin remain editable until End Day, with effects explained in the planning card. Overview presents that card first and shares the guarded time path with the top bar. Retired controls reject old revision/session/day/phase calls. Selection itself gives no recovery/growth, workload reduction or stamina charge. A prepared bath retains its next-day stamina bonus without replacing chosen Training/Admin; without a plan it still defaults to Rest.

## Coverage and limits

Day-command tests cover current/stale/duplicate choices, single notifications, combat locks, replay, selected-plan bathing, observer session replacement, growth reset/exclusion, bounded ledger and capped milestones. Existing full first-day/leisure tests remain resource-backed.

The rendered extension exercises actual choice revisions, Plan Night, bath, End Day/report, wallet reconciliation and current-schema save/load at 640x480/960x540. Its Day-2 setup is explicitly synthetic; settlement uses normal work/services. Queued-signal rejection is labeled separately from viewport clicks.

Test-only teardown retires the temporary world and finalizes C# resources while the engine is alive. Forced collection is not normal gameplay or proof that all engine-lifecycle problems are fixed. Errors remain fail-closed. No original-source edits, character approvals, adult-specific assets, reward-rate changes or legacy expansion. Slot 99 remains disposable under isolated launchers. Physical devices, full rendered first-day journeys, authored collision/navigation, final assets, Forward+ hardware performance, parity and long-term balance remain open. Record final exact-head results before marking PR #13 ready.
