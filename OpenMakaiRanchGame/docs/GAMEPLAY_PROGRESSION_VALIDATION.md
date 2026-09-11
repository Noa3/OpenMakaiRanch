# Gameplay progression: executed validation

Checkpoint **2026-09-12**. **PR #19**, branch `feature/gameplay-progression-20260912`.
Base main: `1e7f90936849443f1605e573a37d3e43e0676245`.
Verified code head: **`90fdf47c30f671c869da1c102091a613d4a8f59f`**.
Tested PR merge/source receipt: **`1c264ed46b7a359cb9f39045e01e5797de5550da`**.
Documentation-only commits may follow. This is not the parallel presentation PR #18.

## Executed checks

| Workflow | Run | Result |
| --- | --- | --- |
| Build Smoke Check #666 | `34654862280` | success |
| Godot 4.7 Mono CI #658 | `34654862369` | success |
| Rendered UI acceptance #95 | `34654862273` | success |

- **2,000 SMOKE OK, zero SMOKE FAIL, one SMOKE PASS.** This includes 111 new gameplay-progression assertions on top of the existing 1,889. No existing tests were removed or weakened.
- **526/526 rendered checks and 64 PNG captures**, one UI ACCEPTANCE PASS, zero UI runtime ERROR/SCRIPT ERROR. These are retained regression scenarios, not newly implemented graphics or a separate rendered challenge menu.
- **69 Python tests pass** in the complete repository (19.841 seconds in the inspected Godot job). The local downloaded review subset separately passed 57 in 22.606 seconds; its missing 12 anime-tool tests are not described as locally executed.
- Build succeeds with **zero errors**, but **19 warnings** in the inspected Godot compile step: unavailable developer-local NuGet source, inherited deprecated drawing calls and a nullable test warning. Import retains the known EditorSettings shutdown diagnostic; software rendering retains its VSync warning. Smoke contains exactly five intentional invalid-save ERROR diagnostics. No filtering change hid errors.
- C# 12, net8.0, SDK 10.0.401, Godot 4.7.2 Mono and schema 16 remain unchanged. No local Godot or .NET execution is claimed.

## Functional evidence

Food fixtures prove read-only source allocation, exact pantry/portable consumption, shortages,
invalid negative stock rejection without normalization, high stock safety and preservation of
an already boosted energy value. Two consecutive regular settlements prove that real cooking
output feeds the next shift rather than retroactively supplying the current one.

Eight existing job/building mappings are checked at levels 0/1, 2, 5 and int.MaxValue. Level
one output/pay stays unchanged; the local stock bonus agrees between preview and execution,
is affected by fatigue, does not apply to unrelated office work, and does not make a collapsed
worker produce. A full stockpile saturates rather than wrapping negative.

Achievement fixtures check stable read-only values, early completion without a minimum day,
no gold/items or stat multiplier, once-only reporting, preserved first-success day, old boundary
rejection, post-victory completion, current-schema flag serialization and wellbeing/food/cost
requirements. A root-level scenario performs ordinary EndDay, SaveSlot/LoadSlot, another day
and explicit New Game+. Only the explicit new run resets run-local recognition. This root
scenario stages day, assignments and pantry stock; it is not an organically earned challenge run.

## Fourteen-day economic policy comparison

All three scenarios start with SaveStateFactory and Random(41), 500 gold and the ordinary two
residents/items. Each pays 250 gold to build the Dairy Barn. The same resident is selected for
dairy by the current output preview. The second works in the office, or switches to the kitchen
when stored meals are below twice the crew size. The investment variant additionally pays
215 gold for Kitchen level two. Missing lunch boxes are bought through the ordinary shop when
affordable. There are no granted skill/cash bonuses during these runs.

Each iteration sets the next Night/Rest boundary and calls the normal settlement; no walking,
combat, resident care, story dialogue or elapsed real-time day traversal is being simulated.
The same seeded day-event rules and actual milestone payouts remain active. Starting with the
same conditions does not imply identical later milestones: the strategies reach thresholds at
different times. All 42 day records reconcile gold and keep stock nonnegative.

| Fixed policy | Gold after day 14 | Portable-food purchases | Construction/upgrades | Ranch meals consumed |
| --- | ---: | ---: | ---: | ---: |
| Dairy + office; portable lunch | 457 | 780 | 250 | 0 |
| Dairy + adaptive kitchen/office | 1,352 | 0 | 250 | 26 |
| Adaptive policy + immediate kitchen upgrade | 982 | 0 | 465 | 26 |

The starting two meal boxes cover the first lunch. All three inspected runs have zero missing
servings. Final gold obeys `initial - construction - groceries + sum(actual report net)`.
The operating-only portable-food strategy still loses gold on ordinary non-reward days;
self-supply removes those repeat purchases. The immediate kitchen upgrade produces more food
but costs money and increases existing upkeep, so it is NOT the best cash strategy for this
small crew over this horizon. This is a measured tradeoff, not proof that every upgrade is
balanced, that these policies are optimal, or that early upgrades always pay back later.

The test output contains all daily rows as `BALANCE` and summaries as `BALANCE TOTAL`.
Random events/milestones affect final wealth, so the table is not a pure marginal-output study.
Optional profitable-shift achievements use separately captured work facts BEFORE those payments,
and charge consumed portable meals at replacement price; lucky income cannot qualify them.

## Evidence receipts

The downloaded ZIP bytes were independently SHA-256 checked:

| Evidence | Artifact ID | SHA-256 |
| --- | --- | --- |
| Godot import/smoke | `10284909168` | `3792d96dc3296327d41d27face66294503fba9bd96535eb3b805eb18e8f5ee30` |
| Rendered regression/review source | `10284919413` | `228791dcf1c58c236f88ab02ed22de0caea31c63370726f32b2653dba0ab8a25` |

Smoke console: `.artifacts/godot/smoke-etzrz5lx/console.log`; import `import-dof3wrfq`.
UI results/console: `godot/ui-6fszsc79/`; import `godot/import-r0xxbdmx/`.
`ui-source/commit.txt` contains the exact tested merge above. The reviewed gameplay test and
tier-calculator source bytes match the staged local source. Artifacts contain expiring evidence
and a whitelisted source subset, not an exported game or a complete standalone checkout.
No screenshot is presented as a new visual design in this gameplay-only change.

## Boundaries and next work

No changes to scenes, shaders, models, world geometry, UI renderers/layouts, graphic settings,
existing locale/ui catalogs, CI workflows or other branches. Existing completion criteria,
role/source IDs, prices, working clocks and personal save files remain. No automatic merge,
forced push, write-enabled helper workflow or implicit New Game+ was used.

Current integrations are accessible through the existing day report, work actions and forecast,
plus read-only APIs documented in GAMEPLAY_PROGRESSION.md. A separate overview UI, repaired
Continue Ranching navigation, resident-specific stories and a full first-week chapter remain
unimplemented here. No asset reward, new mission chain or new difficulty mode was added.

The present operating criteria are initial tuning. Full NPC/world traversal, higher roster sizes,
all wellbeing/recovery paths, support-building benefits, return on investment over longer runs,
food preference controls, extreme arithmetic in older producers, exported saves/builds and
physical-device/Forward+ performance require separate work. Static tests are not full-game
or main-campaign completion certification.

Reproduce only through isolated launchers; tests write/delete disposable slots 99 and 3 and
check occupancy where appropriate. Never run raw test flags on personal profiles:

```bash
python -m unittest discover -s Tools/Godot -p "test_*.py"
python Tools/Godot/validate_locales.py
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/ui_acceptance.py --rendered
```
