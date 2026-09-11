# Work-station planning, upkeep and output forecasts

Checkpoint **2026-09-11**, PR #15 on `feature/world-stations-and-interiors-20260911`. The complete station sequence and new forecast checks pass on code head **`4ffd97c84929ed307fa623dde709760dcec28762`**. Exact receipts and limits are in OPTIMIZATION_VALIDATION.md. The previous0356785 UI run exceeded its process timeout and is not counted as a successful acceptance.

## Player flow

Ordinary workstations use Overview, Team and Equipment (where a facility exists). Overview describes staffing, level, actual base upkeep, stock and local issues. Team offers local assignment/rest controls with current job/energy/fatigue. Equipment shows the next level, one-time price, wallet, local upkeep before research and whole-ranch facility upkeep after the existing logistics discount. Pet care and the unstaffed-dairy penalty are separate. Back without buying is inert.

Price and running cost precede the confirmation; longer explanations follow it. A successful upgrade returns to Overview with its charged amount and a visible Team-planning next step, not another immediately active purchase. First-day guidance keeps direct Build then Assign without adding a tab tutorial. Workshop/pharmacy services require the built workplace. Office services remain local. Other house/resident/companion routes were not expanded by the forecast work.

Stable action IDs and complete English/German templates separate display language from commands. Language refresh retains page/context and clears previous-language transient results. Scrollable outcomes and nested focus restoration remain. The station slice added31 keys to the prior309; four forecast keys now bring the matching UI slice to344. This is not complete-game translation.

## One upkeep authority

RanchService.Facilities.cs owns read-only FacilityUpgradeOffer values and actual upkeep. Reviewing a quote never mutates facilities, reserves money, produces stock or advances time. Prices retain BuildCost + current level *75. Explicit zero/negative levels do not accrue positive upkeep. Positive levels multiply local upkeep; logistics removes one quarter rounded from the whole positive ranch total. Long intermediates and bounded display/payment avoid negative wrapping, including added pet/missing-dairy costs at settlement. This is a targeted repair, not a complete rollback or arithmetic audit.

FacilityDefinition.OutputBonus is not currently a general level multiplier on ordinary job production. The interface and kitchen project wording do not promise one. Existing automation/project dependencies remain. Higher-tier benefits need deliberate design and measured returns, not a second payout implemented in the UI.

## Current-condition forecasts

Team shows gross production units and gold for each resident using RanchService.PreviewJobOutput. ApplyJobOutput calls the same calculator just before committing work. The original effective-skill, equipment, research, talent, fatigue and integer-rounding order is preserved; Adventure uses effective CombatSkill and other productive roles effective RanchSkill as before. A breakdown tooltip exposes the factors without paying output or repairing a missing equipment map.

This is not final daily net profit. The night plan, subsequent care, resource consumption and events can alter the eventual result; bills are separate. At fatigue>=70 the existing resource-consumption pass assigns Rest before the night recovery/work pass. Team warns about that scheduling consequence rather than presenting the current-condition estimate as a guaranteed payment.

## Command boundaries

World commands require matching visible station context and current proximity. The resolved station supplies its actual job/facility IDs. Root validation also checks captured generation/day/phase, pause/combat/settlement and mutually exclusive resident/station commands. A purchase recomputes and compares price, level and upkeep before payment. Changed research, used quotes and stale assignments fail without charge.

Team changes compare the old job; Rest only releases a worker still assigned there. Unbuilt work is rejected in code. Jobs pay at normal settlement, not assignment. Synchronous observers cannot start another station/resident command or ordinary time advance while the action is busy. Raw internal services remain outside world proximity and are not universally transactional/idempotent.

## Executed acceptance

Numeric cases cover ghost upkeep, level-scaled bills/logistics rounding, inert quotes, unknown/unaffordable/extreme upgrades, bounded settlement bills, stale/replayed quotes/assignments, unbuilt work and observer reentrancy. Forecast fixtures additionally compare seven categories to pre-refactor golden values and check read-only state/preview-to-commit agreement.

The final rendered sequence clicks Overview/Team/Equipment, German inspection/cancellation, one canonical upgrade, assign/rest/reassign, save/load, real world phase controls and house Rest/Sleep, then checks the actual bill/reconciled report and saves/loads the next morning. It includes closed/remote/stale denial, no early production, visible price/result/next-step and a German canonical forecast. German station layouts include640x480 and480x800; the forecast capture is640x480. The imported Team screenshot remains text-heavy and scrolling is required for all workers/actions. Tooltip contents are tested; all physical pointer behavior is not.

These are synthetic integration fixtures with inherited funds/skills and staged proximity. They do not prove an earned optimal first week, hardware performance or universal route accessibility. The preceding240-second timeout was investigated; the same existing workflow now has a360-second process budget while retaining its15-minute overall bound and all checks. The fixture restores its intended viewport after profile settings load. Current schema16, C#12, SDK10.0.401, net8.0 and Godot4.7.2 are unchanged.

Remaining: final whole-day net forecasting, worthwhile tier benefits, dedicated store/research/result surfaces, less dense instructions, stepwise creation, campaign content, full translation, world/NPC/camera routes and actual export/device/Forward+ acceptance. Use isolated launchers; slots99/3 are never personal profiles.
