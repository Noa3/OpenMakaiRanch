# Gameplay-only progression continuation

Branch: `feature/gameplay-progression-20260912`, based on merged main
`1e7f90936849443f1605e573a37d3e43e0676245`. The merged anime-material work is inherited,
not edited. No scenes, shaders, models, world layout, UI renderer or graphics settings
are changed. C# 12, net8.0, pinned Godot/SDK, original source and save schema 16 remain.

## Changes players receive through existing actions

### Kitchen production is usable food

Workday lunch uses one stored `meals` unit per scheduled non-resting resident before
using portable `meal_box` inventory. Only existing quantities are consumed. A shortfall
retains the old morale consequence; resting retains the existing zero-lunch-cost rule.
Today's production occurs later in the ordinary settlement and supplies the next day,
not the already-consumed lunch. No conversion button, free item or second daily timer.
Portable meals remain available for personal care, outings and gifts.

This intentionally changes which source feeds lunch. The former code completely ignored
ranch-produced meals for this purpose. `InspectNextLunch()` exposes the exact source split
without mutating state. Ordinary energy recovery preserves already boosted above-cap values.
Manual care/meal actions still use their own existing costs, inventory and daily receipts.

### Productive equipment tiers

For the explicit existing dairy/pasture/kitchen/cooking/workshop/pharmacy/service/cleaning
job-to-building mappings, each tier beyond level 1 (up to useful level 5) adds the
facility catalog's `OutputBonus` units of its matching resource per working resident.
The extra stock is applied after the existing formula and scaled down by existing fatigue.
Level 0/1 and unmapped jobs retain their old forecast and wages. No automatic worker,
unstaffed output, extra gold, daily cap or price increase is introduced. Work and forecast
share the calculator. `InspectWorkBenefit(job)` and `JobOutputPreview.FacilityBonus` expose
it to the presentation branch. Above level 5 has no further new production benefit;
numerical upgrades remain as before. This is initial transparent tuning, not an optimal ROI claim.

Building footprint/collision/stages are untouched. Not every facility has a staffed output
role; storage/well are not arbitrarily attached to another job's bonus. A different future
benefit for these support facilities needs a separate design/test pass.

### Optional run-local recognition

`GetRanchAchievements()` returns five stable IDs with translated titles/requirements and
first achieved day. Four operating challenges recognize self-catering, balanced production,
a small crew or a paid day off. A restoration chapter acknowledges all four existing starting
projects. These are independent of main completion and may finish before or after it.

The normal root settlement records actual work and food facts before random events/growth.
Profit challenges compare work income to operating expenses AND replacement value of used
portable meals, not lucky event money or milestone rewards. This is an operating test, not a
whole-day cash-flow statement (equipment purchases, gifts etc. still have real separate costs).
There is no mandatory attempt, failure penalty, streak reset, minimum date or new win condition.
Success appears once in the existing report as a zero-currency event and remains a durable
run-local record. No permanent stat multipliers or automatic New Game+ are added.

Flags `1_230_500` through `1_230_505` hold a last-observed day and five first-success days.
Queries do not allocate history. Current-schema save/load retains them; explicit New Game+
starts fresh optional records under existing new-run behavior. This is not a global leaderboard.

## Presentation integration (no UI files changed)

- `GameRoot.InspectNextLunch()` returns `LunchPlan`: workers, pantry meals, portable boxes,
  and missing servings based on the *current* assignments/stocks.
- `RanchService.InspectWorkBenefit(JobDefinition)` returns the local tier contribution.
- `JobOutputPreview.Amount` already includes the contribution; `Gold` stays ordinary pay.
  Existing station forecasts automatically show the new total. Future breakdown layout can
  show `FacilityBonus`; do not add it to `Amount` again.
- `GameRoot.GetRanchAchievements()` returns fresh read-only values, not retained UI controls.
  IDs and `CompletedDay` are stable; title/requirements can refresh with language.
- `DailySettlementService.LastWorkFacts` describes one actual settlement and is transient,
  not a serialized second economy. Reports use the existing event list.
- Text entries use existing `locale/en.json` and `locale/de.json` loading/export paths.
  Existing `locale/ui` catalogs, layouts and scene files remain unchanged.

## Verification scope

Added smoke checks cover source conservation/shortfalls, boosted energy, next-day food,
base/tier forecast agreement, unrelated/collapsed jobs, stock capacity, read-only records,
once-only receipts, root save/load/continued play and explicit new-run reset.

Three fixed 14-day economic policies use the same ordinary starting resources and seed:
portable-food dairy/office, adaptive pantry/kitchen, and the same adaptive policy with a
paid kitchen upgrade. They pay construction/groceries and reconcile the wallet with every
actual settlement including events. These are reproducible subsystem comparisons, not
physical traversal, an optimal solver, an organically completed campaign or final balancing.
Their full lines begin `BALANCE`; record actual results after CI finishes, not predictions.

Validation status when first committed: two new Python catalog checks pass locally;
engine/build/smoke/UI outcomes are pending remote execution. Existing tests remain registered.
