# Daily gameplay loop continuation

2026-09-11, PR #12, branch `playability/ranch-quiet-corner-20260910`. C# 12 remains enforced. This continuation targets actual daily progression and decisions, not another menu-only redesign.

## Reproduced source paths; baseline execution pending

- `DailySettlementService.ApplyNightAction` invokes ranch-wide growth once for every resident. Training should contribute one extra growth pass, independent of roster size; ordinary daily growth still follows. Existing XP/fatigue/talent formulas are retained, not presented as newly verified original-game parity.
- `CharacterGrowthService` clears HasGrownToday on each pass, losing an earlier same-day level-up marker.
- Random-event gold is applied to the wallet but omitted from the final report/overview totals. At the wallet cap the event can report gold not actually credited.
- The ordinary root AdvanceTime command accepts Night without a choice; the overview button can bypass the world/top-bar planning check.
- A DaySettled observer can synchronously call EndDay again and settle the following day. The bounded regression deliberately attempts this once, never recursively without a limit.

## Intended scope

Add numeric regression fixtures first, retain all existing smoke and rendered assertions, then repair the shared service/root boundaries. Let players revise Rest/Training/Admin until settlement, with exact effect explanations and stale-view/day/session rejection. Preserve the independent next-morning bath bonus, free exploration, economic reward formulas, save schema 16 and original read-only source. No character approvals or adult-specific content changes.

Pure fixtures and direct seeded dates are test arrangements, not organically earned progression. The established first-day and leisure walkthroughs remain the resource-backed checks. No complete original-engine parity, physical-device playtest or transaction rollback guarantee is implied.
