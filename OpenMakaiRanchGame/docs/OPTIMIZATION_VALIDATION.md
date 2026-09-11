# Work forecasts, station planning and fair optimization: validation

Checkpoint **2026-09-11**, PR #15, branch `feature/world-stations-and-interiors-20260911`. Verified code head **`4ffd97c84929ed307fa623dde709760dcec28762`**. PR checks tested merge **`7436f2222b1d8142b4b59399fe398062bbc8384c`** against main `3209f4c`. Subsequent documentation-only commits do not imply another runtime change. OPTIMIZATION_AND_PACING.md separates the implemented slice from proposed campaign and mastery content.

## Implemented scope

The station Team view exposes per-resident gross work units and gross gold at current condition. Preview and actual ApplyJobOutput use the same calculator, preserving the previous ordering and integer rounding for skill, equipment, talents, fatigue and specialist research. An optional breakdown tooltip contains those factors. Looking at a missing equipment map no longer allocates it. This is not a whole-day net-profit forecast: meals, automatic rest, the night plan, later care, upkeep and events are separate. The existing fatigue-at-least-70 pre-production auto-rest rule is stated explicitly.

The EquipmentService swap now validates the replacement stock and return capacity before returning the old item. Previously a failed request for an unavailable replacement could repeatedly add the still-equipped old item to inventory. Genuine owned swaps retain their behavior. Other inventory/service arithmetic is not exhaustively audited.

WinConditionService exposes a read-only progress snapshot and checks real catalog discoveries, skills and facilities rather than accepting duplicate/unknown IDs as substitutes. Legitimate completion thresholds remain unchanged. There is no new minimum day, daily limit, price increase, production reduction or automatic New Game+.

The preceding station changes are retained: Overview/Team/Equipment separation, canonical level-scaled upkeep, no zero-level ghost bill, displayed price and local/whole-ranch running costs, stale-quote and prior-job checks, local proximity/context and reentrant command guards. The upgrade result returns to a visible team-planning next step, not an immediate repeated purchase prompt. No early job payment is added.

## Exact-head executed results

| Workflow | Run | Result |
| --- | --- | --- |
| Build Smoke Check #660 | `34649261398` | success |
| Godot 4.7 Mono CI #652 | `34649261392` | success |
| Rendered UI acceptance #90 | `34649261422` | success |

- **1,889 SMOKE OK / zero SMOKE FAIL / one SMOKE PASS**. There are **55 optimization assertions**, extending the previous station count of 1,834 (resident baseline 1,811 plus 23 station assertions).
- **526/526 rendered checks**, **64 PNG captures**, one UI ACCEPTANCE PASS and **zero UI runtime ERROR/SCRIPT ERROR**. The resident checkpoint had 470 checks/59 captures; the completed station continuation and forecast checks now add 56 checks/five captures in total.
- **55 Python tests pass** in CI and the local full retry (22.048 seconds, `minmax-current-python-tests.log`). Static locale validation reports **344 matching English/German keys**. Four forecast templates extend the preceding 340-key station slice; the kitchen project wording was corrected without adding another key.
- C#12, SDK10.0.401, net8.0, Godot4.7.2 Mono and schema16 remain unchanged. No local Godot/compiler run is claimed. Successful compilation is not a zero-warning certification.

## What was actually exercised

Numeric cases compare seven job categories against literal hand-calculated pre-refactor outcomes, including fatigue boundaries and exact preview/commit agreement. More effective skill improves output and the same conditions produce the same return early or late in the calendar. Collapsed/rest/null-equipment previews remain read-only. Twenty unavailable replacement attempts leave the complete state unchanged; a real owned swap consumes and returns exactly one item, and a full return stack is rejected before consumption.

A synthetic completion fixture supplies the existing goals on day two, records completion through ordinary root settlement on the next morning, then advances three additional days through ordinary time commands. It preserves the recorded completion day, session and non-NG+ state and does not repeat the completion event. Current-schema JSON keeps the continued day and completion record together. This is not an earned speedrun or physical victory-button test.

The rendered kitchen journey uses actual tab/review/cancel/confirm/assign/rest/reassign controls, canonical charges and upkeep, remote/closed/stale denial, current-schema save/load, actual world phase buttons, house Rest/Sleep and a reconciled next-morning report followed by another load. The German forecast label is compared to the shared calculator and its breakdown text is checked; hover behavior is not a separate physical-device acceptance. Earlier resident, intro, language, station, project, night, combat and save journeys remain connected. Proximity and inherited resources/strong stats are explicit fixtures, not a naturally earned whole-game run.

## Seven-day economic baseline: useful but not sufficient

This separate numeric baseline uses the seeded data registry and ordinary SaveStateFactory resources with seed41. It pays 250G for the Dairy, selects a suitable dairy worker and assigns Office Work to the other resident, buys missing meal boxes through ShopService, chooses Rest and invokes seven normal settlements. No money or skill boost is injected, but physical navigation, the introduction, optional daytime actions and optimal-strategy search are outside its scope.

| Completed day | Settlement income | Settlement expenses | Wallet after settlement |
| --- | ---: | ---: | ---: |
| 1 | 193 | 72 | 371 |
| 2 | 124 | 72 | 363 |
| 3 | 124 | 72 | 355 |
| 4 | 199 | 72 | 422 |
| 5 | 124 | 72 | 414 |
| 6 | 124 | 72 | 406 |
| 7 | 124 | 72 | 398 |

Day2-7 meal purchases cost 60G before each settlement. Therefore the ordinary 124-72 settlement margin is **52G before purchases, or -8G after those meals**. The larger day1/day4 receipts keep this short run solvent. Do not call this policy indefinitely profitable or use it to claim a minimum campaign duration. It remains incomplete at day8 and does not prove an expert cannot finish other goals sooner. Compare multiple policies and resource uses before selecting final rates.

## Source and evidence receipts

Downloaded ZIP bytes independently match:

| Evidence | Artifact ID | SHA-256 |
| --- | --- | --- |
| Godot import/smoke | `10283302311` | `aa6c6e8d23e85ba576e94808233e9c53c6f808e3d4760a88a2882fb27f788385` |
| Rendered UI/review source/screens | `10283282659` | `03fc44777a5a8a1ebba73fb7cf2a6618a32d90e3de137067bd72ec2f0e58bb92` |

Smoke console: `.artifacts/godot/smoke-x1d3_mxq/console.log`. UI results/console: `godot/ui-7pq0e83b/`; UI import: `import-5cylrjyz`. `ui-source/commit.txt` records the tested merge above. All eleven changed runtime/test/catalog files in the downloaded source match the intended local bytes. The central WorldStationPanel.cs still has its baseline blob `4d41174c4b91b861fa5725ff6ec0ac33b5dc9124`; none of the excluded completion/ambition UI partials are present.

Opened and visually inspected `team-forecast-german-640x480.png`, `kitchen-equipment-german-640x480.png` and `kitchen-purchased-german-640x480.png`. The German preview, purchase costs, result and Back control are visible. The Team page still contains dense introductory text and needs scrolling to see all workers/actions at 640x480. This remains a polish issue, not a claim of final presentation.

Five smoke ERROR diagnostics are deliberate invalid-save rejections. Import exits successfully with the known EditorSettings shutdown diagnostic; software rendering retains its VSync warning. No new runtime-error filtering was added. Evidence archives expire and their review-source subset is not an exported game.

## Previous timeout and excluded changes

The preceding rendered run at `0356785` (run34624539764) failed by exceeding the 240-second process limit: 240.795 seconds, exit -9, no completed results.json. It reached late house/day checks; incomplete checks were never counted as success. The existing software-rendered process limit is now 360 seconds while the overall job remains bounded at 15 minutes. Assertions and runtime-error rejection are retained. The station fixture also explicitly restores its intended 640x480 viewport after profile settings are loaded. No new workflow or write-capable helper was introduced.

A write of the central station file was blocked. The proposed new long-term-goals UI and revised completion routing were excluded from the published commit rather than installed through another path. Existing service-level continuation tests do not certify the physical completion interface: the house/report route can still overwrite the victory presentation and locked-screen return needs a separate location-preserving review. The existing Continue Ranching control is not a newly implemented feature in this checkpoint.

New authored chapters, optional mastery challenges, personal-best leaderboards, character arcs, free placement, wider building tiers and a playable organic river remain absent. See KNOWN_ISSUES.md. Better forecasts and conservation fixes are useful groundwork, not additional campaign content or a finished balance pass.

## Safe reproduction

Use `python Tools/Godot/validate_locales.py`, the existing Python unittest suite, `dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj`, and the isolated smoke/UI/runtime launchers. Automated acceptance uses disposable slots99/3; never pass raw test flags against personal saves. Main and original reference content were not changed. No force push or automatic merge was used.
