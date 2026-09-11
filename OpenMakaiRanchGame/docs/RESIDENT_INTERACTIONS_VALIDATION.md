# Resident interaction acceptance

Checkpoint **2026-09-11**, PR #15, branch `feature/world-stations-and-interiors-20260911`. Verified code head **`4d3c9893a4621bd09f1e358e178f0f671ba18537`**. PR-triggered CI tested merge **`046d96f608f8595ea0aaca852f2e6c1768df7c29`** against main `3209f4c`; that merge appears in the downloaded reviewed-source receipt. Documentation-only commits may follow. Earlier design/localization receipts remain in RANCH_DESIGN_VALIDATION.md and LOCALIZATION_UI_VALIDATION.md.

## Implemented scope

Ordinary free conversation, encouragement, meals, eight explicitly allowed ordinary gifts, recovery, mentoring and ranch/craft/combat/magic practice now use resident-focused world pages. The initial view does not expose every command at once. Companionship retains the existing voluntary eligibility path; work shows the current assignment, permits an explicit day off and otherwise directs the player to a physical workplace. The owner record does not grant self-directed relationship rewards.

ResidentInteractionService validates conditions and records a bounded last-day receipt per activity group while delegating effects to existing Visit/Bond/Training services. GameRoot checks session/day/phase/combat/pause/settlement and keeps its busy guard through synchronous notifications. World dispatch additionally requires the matching visible resident view and current proximity. An addressed NPC waits until the conversation closes, without changing work or paying production.

Practical lessons use one shared focus receipt per resident/day and the existing ranch-wide two-session budget. A lesson costs the existing Mentorship stamina price (currently 20), 10 resident energy and talent-adjusted fatigue. Mentoring is separate. Unknown/capped/overflowing focuses and unavailable trainees are rejected before training costs. Meal/recovery energy respects the actual definition/override capacity and cannot lower a pre-existing above-capacity boost. Ordinary care/practice does not require romantic or adult content approval; it is not the legacy adult-training catalog.

62 matching English/German keys extend the new UI slice from 247 to **309**. Complete keyed outcomes do not infer success from English strings. Long results remain inside the scrollable conversation. Completed actions reveal their outcome; duplicate daily-limit explanations are shown once, while each affected button retains its own tooltip. Language refresh preserves page/target and clears old-language feedback. New Japanese keys remain English fallback, not complete Japanese translation.

## Exact-head results

| Workflow | Run | Result |
| --- | --- | --- |
| Build Smoke Check #651 | `34619090555` | success |
| Godot 4.7 Mono CI #643 | `34619090565` | success |
| Rendered UI acceptance #81 | `34619090773` | success |

- **1,811 SMOKE OK / zero failed assertions / one SMOKE PASS**, including **33 new resident assertions**. Previous 1,778 checks remain.
- **470/470 rendered checks**, **59 PNG captures**, one UI ACCEPTANCE PASS and **zero UI runtime ERROR/SCRIPT ERROR**. This extends the previous 432/56 checkpoint by 38 checks and three captures.
- **55 Python tests pass**. A complete local retry against the reviewed-source subset finished in **36.940 seconds**; the initial time-limited attempt did not finish and is not counted as a pass. The seven catalog tests also passed separately. `validate_locales.py` reports 309 valid matching English/German keys; `git diff --check` passes.
- Compilation and engine tests ran in CI, not the local review container. Existing C# 12, SDK 10.0.401, net8.0, Godot 4.7.2 Mono and save schema 16 remain unchanged. Success is not a zero-warning claim.

## Rendered journey and what it proves

The added journey opens a nearby resident, checks waiting over 40 frames, clicks free conversation and compares complete state JSON, opens Practice and clicks Craft. It checks skill/energy/stamina/one shared daily slot, unchanged work and disabled second-focus training. Full outcome visibility and a single shared limit explanation are asserted separately. German practice pages are inspected at 640x480 and 480x800, preserving the same resident and assignment.

An actual encouragement click triggers a test observer attempting a different reentrant action; it is denied without a second stamina payment. Meal and journal prerequisites are bought through canonical root shop transactions using inherited fixture funds, not asserted to be physical store clicks. Actual meal/gift buttons consume exactly their chosen items. Retired hidden callbacks and remote actions cannot reopen the view or spend resources; closing releases NPC and player control.

Real current-schema SaveSlot/LoadSlot retain daily receipts and reject the previous generation's command. Ordinary phase advancement reaches Night, then the actual house Rest/Sleep buttons produce one report and a new morning with recovered player budget, reset global practice slots and the next lesson available. A second real save/load retains that next-morning availability. This is a connected resident-to-day-loop acceptance path, not a wholly organic new-game/week playthrough.

The fixture explicitly stages proximity, selected skill/tiredness/health, daytime and player stamina, and inherits the earlier synthetic day/resources. It does not approve any production character or inject a romance eligibility override. The resident scenario refuses an occupied isolated slot 99 and removes only its own save. Prior first-day, station/held-key doorway, native language-picker, night/report, adventure, store/save, project and shared-evening journeys remain.

## First run and fixture corrections

At `d59f06f`, Build #648 and rendered UI #78 passed; Godot #640 produced **1,806 OK and five failed assertions**. Three were in the older training-budget sequence, whose trainees inherited injuries/collapse or capped skills from an earlier adventure. The corrected fixture explicitly establishes living recovered trainees, uncapped focus skills and an unused budget before testing that budget. The original assertions remain, including the third-session denial and night-rest check.

The other two failures came from the new missing-meal fixture: new-game starting meals were still present, so its supposedly rejected action succeeded and consumed the receipt needed by the next check. It now explicitly removes only that isolated fixture's starting meal stock before checking absence. No runtime condition or assertion was weakened. The initial zero-MagicPower fixture likewise declares a living trainee instead of relying on default zero HP.

First smoke artifact `10270817826` SHA-256 `f6cbf853c201f4d8e42f97cb64cc0239b5346431d62deaffc46aaaf00a83834f` retains the failures. First rendered artifact `10271389502` SHA-256 `447d4b9c55de5121f0b4ad7dd92bab0237a579292cf5164199cfdf06c2ff807f` passed 467 checks but visually showed repetitive limit text and outcomes above the scroll position. The final UI improvement adds three explicit checks rather than treating that earlier presentation as final.

## Final evidence and visual inspection

Actual downloaded ZIP bytes were independently SHA-256 checked:

| Evidence | Artifact ID | SHA-256 |
| --- | --- | --- |
| Godot import/smoke | `10271921447` | `982cf0c2161773222b25ec8fa881a208e99a465cf537f3c3d351fa214b8f7005` |
| Rendered UI/review source/screens | `10271816766` | `77b70b3654b8c04e91551899a0c2c270edf7c187ffff5d7705303f1878261207` |

Smoke: `.artifacts/godot/smoke-404a3i_u/console.log`; import `import-2fh7ip44`. UI: `godot/ui-s_zzfzzx/results.json` and console; import `import-nc03zc5i`. Final `resident-practice-german-640x480.png`, `resident-practice-german-480x800.png` and `resident-gifts-480x800.png` were opened and inspected. They show genuine German wrapping, reachable fixed Back and visible gift outcome. At 640x480, explanatory content still occupies much of the view and actions require scrolling; this is a remaining density issue, not final visual quality.

Exactly five invalid-save ERROR diagnostics are intentional smoke fixtures. Engine import still exits successfully with the known EditorSettings shutdown diagnostic. The rendered runtime contains zero errors; software-rendering VSync and known build warnings remain distinct. No new error filtering or normal-game forced collection was added. Expiring artifacts contain review inputs/evidence, not a standalone exported game or personal profile.

## Remaining limits and reproduction

Not every original interaction, resident arc, physical input device, route, collapsed-character recovery path or seven-day balancing outcome is certified. Raw internal Visit/Bond/Training callers are not universally constrained by the new world-action receipts. Exception rollback and exhaustive extreme arithmetic remain separate work. Training/care animation, equipment presentation, distinct authored dialogue and full translation remain open. NSFW_CONTENT_HANDOFF.md neutrally lists excluded catalogs and integration locations without explicit scenes or instructions; existing source IDs/roles and eligibility gates are unchanged.

```bash
python Tools/Godot/validate_locales.py
python -m unittest discover -s Tools/Godot -p "test_*.py"
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/ui_acceptance.py --rendered
python Tools/Godot/launch.py --mode runtime --isolated
```

Use isolated launchers only: the combined suite writes/deletes disposable slots 99 and 3. Temporary branch-scoped exact-preimage helpers removed themselves and are absent from the final diff. Main and the older PR #14 branch remain untouched.
