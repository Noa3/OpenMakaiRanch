# Work-station planning and facility upkeep

Continuation 2026-09-11, PR #15 on `feature/world-stations-and-interiors-20260911`. Implementation entered at `0438e449a9515d8a0aa853f57581e57dd662e09d`. Engine acceptance is pending at this document's initial commit; do not reuse the previous resident checkpoint as a pass for these changes.

## Player flow

Ordinary workstations use three local views: Overview, Team, and Equipment (where a facility exists). Overview describes assigned staff, equipment level, actual base upkeep, relevant stock, base job output and local problems. Team contains assignment/rest controls and current job/energy/fatigue. Equipment shows the displayed next level, one-time price, wallet, local upkeep before research and the whole-ranch facility bill after the existing logistics discount. Pet care and the unstaffed-dairy penalty are explicitly separate. Back without buying is inert.

First-day guidance is intentionally different: direct Build followed by Assign remains visible in the existing tutorial, without requiring a new tab lesson. Workshop/pharmacy service entry requires a built workplace. Office services remain local to the office rather than reopening a global management hub. House, resident, companion, guide and existing adult-content routes are not expanded in this change.

All actions and outcomes use stable IDs and complete English/German keyed templates. The station keeps its selected page during language refresh; previous-language transient results clear. Results are displayed within scrollable content, not only a clipped footer. Nested tab focus participates in the existing deferred view restoration. The new slice adds 31 matching UI keys (309 to 340); this does not complete the entire game's translation.

## Single source of truth

`RanchService.Facilities.cs` owns read-only `FacilityUpgradeOffer` values and the real facility upkeep calculation. The offer is not a pending transaction or saved gameplay object. Opening/reviewing it does not temporarily mutate facilities, reserve gold, pay output or advance the clock. Upgrade prices retain the existing BuildCost + current level * 75 rule; gameplay rates are not retuned here.

An explicit zero/negative facility level no longer accrues positive upkeep. Positive levels multiply the facility's upkeep. Logistics deducts a quarter, rounded once from the whole positive ranch bill. Long intermediate arithmetic and bounded display/payment values prevent wrapping negative. Daily settlement also adds pet care and the missing-dairy penalty without wrapping a capped bill into a free/negative payment. This is a targeted upkeep repair, not a complete audit of every producer or transactional rollback.

The current job-production code does not use FacilityDefinition.OutputBonus as a general level multiplier. The UI therefore does not promise one. Existing project and automation dependencies remain. Defining worthwhile marginal benefits for every higher tier needs a separate balance/design pass instead of silently inventing duplicate job rewards.

## Command boundaries

World station commands require the matching visible station context and current proximity. The actual resolved station supplies its stable job and facility IDs. The root additionally validates captured session generation, day/phase, pause, combat, settlement and mutually exclusive resident/station commands. A purchase recomputes and compares its price/level/upkeep quote immediately before payment. Changed research, used quotes and stale work assignments fail without charge.

Team changes compare the captured previous job. Rest can only release the worker still assigned to that station. Assigning an unbuilt facility is rejected, not merely a disabled UI control. The existing schedule remains authoritative; jobs pay only at the ordinary settlement. Synchronous state observers cannot run a second station/resident command or an ordinary time advance before the current command finishes.

Raw internal simulation methods remain available and are not universally protected by world proximity. The new guard is not a rollback engine, multithreaded transaction framework or a promise that arbitrary direct EndDay/service calls are idempotent.

## Planned acceptance and constraints

Numeric tests reproduce zero-level ghost upkeep, level-scaled bills, whole-total logistics rounding, read-only quotes, unaffordable/unknown/extreme upgrades, saturated settlement costs, stale/replayed quotes and assignments, unbuilt work and synchronous observer reentrancy. Existing assertions remain connected.

The extended rendered journey is designed to click kitchen Overview/Team/Equipment, inspect/cancel a translated price, buy one level, assign/rest/reassign a worker, save/load, then advance through actual time and house Rest/Sleep controls into one next-morning report and save/load again. Separate assertions cover remote and closed commands, no early production, displayed costs and unchanged resources during inspection. German landscape/portrait capture sizes are 640x480 and 480x800.

This continues a synthetic integration fixture with inherited funds/resources and staged proximity, not an organically earned first week or physical-device validation. Slots 99/3 belong only to isolated acceptance profiles; never run raw test flags on personal saves. The locale validator and 55 Python tests ran locally; no local Godot or .NET execution is claimed. Engine results and visual inspection must be recorded separately after CI finishes.

Still open: dedicated store/research/result presentation, exact per-resident output forecasts, higher-tier balance, the stepwise creator, all world/NPC/camera routes, campaign payoff, final assets, exports/device/Forward+ tests and full translation. Existing schema16, C#12, SDK10.0.401, net8.0 and pinned Godot4.7.2 remain unchanged.
