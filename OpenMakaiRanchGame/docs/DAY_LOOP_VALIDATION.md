# Daily gameplay loop validation

Verified **2026-09-11**, **PR #13**, `fix/daily-gameplay-loop-20260911`, code-inclusive head **`6af8466dc3bd4338339a6c9b79ea4dd344e6b9db`**. The three workflows pass: **Godot #581 / `34558277149`**, **Build #589 / `34558277154`**, **Rendered UI #29 / `34558277148`**. Exact source, limits and resume instructions are in ASTRA_HANDOFF. C# 12, SDK/framework/engine pins and schema 16 are unchanged.

## Final executed results

**1,728 smoke assertions / zero failures**, including 36 day-contract checks and one added explicit-night save-fixture guard. **240 rendered UI assertions / zero failures / 34 PNGs**, retaining the earlier 207 plus 33 night/report checks. **48 Python launcher/evidence tests**. These categories are not a single coverage percentage or finished-game claim.

Downloaded and SHA-256-checked artifacts:

| Evidence | Artifact | SHA-256 |
| --- | --- | --- |
| Godot import/smoke | `10183422808` | `0b31f56d257b358472728cf2b8ab8c0f735a2831ecbd2167f18db817159a3bd9` |
| Rendered UI/source/screens | `10183428448` | `0efd594f124c9ef89b5fec5086d7b024e8d60008ae41b6011e84cc898b39223a` |

Smoke `.artifacts/godot/smoke-xltt6pti/console.log` contains one SMOKE PASS and five expected invalid-save diagnostics, without out-of-tree transform errors. Successful import `.artifacts/godot/import-gg4bwbuj/console.log` retains the EditorSettings shutdown diagnostic. Rendered `godot/ui-tpwvo56m/results.json` and console contain one UI ACCEPTANCE PASS, no runtime ERROR/SCRIPT ERROR or prior native shutdown fatal; unsupported VSync remains a driver warning. Night-planning and report screenshots were inspected at 640x480/960x540. Errors were not filtered or assertions removed.

## Gameplay corrections

**One bonus growth pass.** Night training formerly called ranch-wide growth inside the resident loop. Four/eight residents gained multiple extra passes rather than one. Training now contributes one extra pass independent of roster size, followed by ordinary growth. Existing fatigue/talent modifiers and rest-job exclusions are retained. HasGrownToday resets once at settlement start so growth from either pass remains visible.

**Complete daily totals.** DailyGoldLedger observes work/upkeep, shipping, events and milestone amounts after their existing services apply them. It creates no money or reward rates. Report income, expenses and net match the wallet and overview. Event/milestone entries at the wallet cap report only credited gold. Existing int fields remain bounded, with exact long totals in the balance line at display limits. Exhaustive upstream integer-overflow and rollback guarantees are not claimed.

**Explicit current commands.** Ordinary AdvanceTime rejects an unplanned Night. Captured session/day/phase commands reject stale/replayed inputs; repeated identical selections do not notify or apply effects. A synchronous completion observer cannot call EndDay to settle tomorrow or advance its Morning. If an observer starts/loads another session, the old report is not published/autosaved into it. Explicit raw EndDay simulation-call compatibility remains; this is not general idempotency for arbitrary sequential simulation calls or multithreaded transaction support.

**Editable plans and independent bathing.** Rest, Training and Admin stay available until End Day, with existing effects explained. Planning itself grants no recovery/XP/workload reduction and spends no stamina. Overview places the card first and shows Plan Night until a valid choice exists; top-bar and overview use the guarded path. Old view/session/day controls are rejected. Bathing retains a chosen Training/Admin plan plus the independent next-morning stamina bonus; an unplanned bath still defaults to Rest.

**Usable cards and feedback.** Header text has bounded width/lines and full tooltip content instead of consuming the scroll viewport. Night and recovery cards contain the existing vertical CardContent container, keeping their labels and button hit targets separate. The rendered bath step is an actual viewport click, not just a root-service call.

## Baseline evidence and intermediate failures

PR #12 was merged externally during this work at **`f420bee96e9b99a38ea1260705dbf1e4fe7030d6`**, including the new failing regression baseline. PR #13 follows it without rewriting history.

At f420bee, Godot #565 / `34555543717` compiled and passed 48 Python checks but smoke had **1,701 OK / nine failed assertions**. Artifact `10182455368`, verified SHA-256 **`a222f1580d6eb0c7d123d2982e1e7942e20563da6024fe39b23e22fb2bd7a65b`**. An earlier documentation hash transcription was corrected against metadata and downloaded bytes. The nine checks reproduce multiplied training at two roster sizes, lost growth marker, missing event net, phantom capped credit, unplanned Night advancement, Morning selection, recursive settlement and reentrant new-Morning advancement. All nine now pass.

The same baseline's Rendered #19 / `34555543731` completed 207 assertions but failed at native C# bridge shutdown. Artifact `10182459376`, SHA-256 `31bdff8ae1e96cd565cf97f3bfa4e60e8baac4094c20fefcb00fae6e6d844154`. Test-only teardown now retires/finalizes the temporary world while the engine remains alive. The fatal did not recur in subsequent inspected runs. Collection is not added to ordinary gameplay and does not certify all engine-lifecycle cases.

At `8c6189e`, Godot #571 / `34556984511` passed all 36 new contracts but smoke had **1,724 OK / three old save-fixture failures**. That fixture advanced Night without selecting work. It now explicitly selects Rest and asserts acceptance before its fourth phase advance. Original day/phase/save assertions remain; runtime guards were not weakened. Artifact `10182960033`, SHA-256 `e18efcefab2322fc3d07bbaed170fd1984388515a3933084df57eeddfcc54032`.

Rendered #23 / `34556984450` retained the old 207 passes but failed 17/231 because a long planning status collapsed the content viewport. The header repair added explicit usable-height/full-message assertions. Artifact `10182960381`, SHA-256 `9eabc195a387d437ba3ccd54dbe6c2d4ed3bd60021fb518640ccb3b75fe3e5f1`.

At `3e4f001`, Godot #576 / `34557814289` passed the full smoke. Rendered #26 / `34557814188` still failed nine of 238 checks: the newly composed night controls were direct PanelContainer children sharing one rectangle. Screenshot/geometry inspection exposed this, and the existing recovery card had the same composition pattern. Both now use vertical card content. The original clicks remain; a non-overlapping-target assertion and actual bath click strengthened coverage. Artifact `10183258800`, SHA-256 `7469f117a4bca4719e9ef06995a2e0bd4f5ea4c394c72a7d0f8c6625946708c5`.

## Scope limits

The new rendered sequence revises all three choices, attempts a stale queued callback, clicks the prepared bath and End Day, checks the next-morning report/wallet/bonus and saves/loads through the real root boundary. Its initial Day-2 state is explicitly synthetic; actual work/settlement uses existing services. Queued-signal rejection is distinct from viewport mouse input. Existing full first-day/leisure resource-backed walkthroughs remain separate.

No original-source edits, character approvals, adult-specific assets, reward-rate changes or legacy expansion. Disposable slot 99 is restricted to isolated test profiles. Physical controller/touch/Alt-Tab, complete rendered first-day/combat journeys, authored navigation/collision, final assets, Forward+ hardware performance, original-engine parity and long-term balance remain open. Exact-head tests must be rerun after future code changes; prior green counts are not transferable proof.
