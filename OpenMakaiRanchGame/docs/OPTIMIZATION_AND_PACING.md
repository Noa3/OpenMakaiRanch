# Optimization without artificial delays

Design direction and implementation scope, 2026-09-11. Continue PR #15 on `feature/world-stations-and-interiors-20260911`. The user wants efficient players to accomplish more in fewer in-game days without exhausting the interesting game immediately. This is not permission to punish efficiency, silently raise costs or require a minimum completion day.

## Design contract

A skilled player finishing earlier is a legitimate success. A finite campaign cannot guarantee the same duration for a new player and an expert while also letting expertise save time. Extend the interesting decisions and authored content, not the time spent waiting. Separate in-game days, active real playtime, meaningful choices and completion coverage when evaluating pace.

The normal campaign should have a clear, attainable endpoint; optional ambitions should make continuing attractive without invalidating that endpoint. Completion, optional mastery and exhaustive collection are different goals. A fast first completion must not retroactively gain extra requirements, erase residents or automatically trigger New Game+. The existing Continue Ranching choice should remain primary and preserve the current ranch. New Game+ stays an explicit separate decision.

No new daily cap, date lock, price increase, hidden production penalty, forced romance, dynamic difficulty tax or loss of rewards is introduced by this continuation. Existing energy, training, delivery and discovery rules still exist; this is not a claim that every older pacing constraint has been removed or balanced.

## Implemented in this bounded continuation

### Station planning that exposes the real rules

The Team view shows each resident's current-condition gross output and gross gold. An optional tooltip breaks out base production, effective skill, planning research, talent/fatigue factors and specialist research. The existing settlement calls the same calculator immediately before applying work; there is no second forecasting formula that drifts away from production.

The calculation preserves prior ordering and integer rounding. In the current rules Adventure jobs use effective CombatSkill, while other productive jobs use effective RanchSkill. Do not silently replace these with CraftSkill or change rates during a presentation refactor. Whether workshop and other roles should eventually use different skills is a separate design decision with migration and balance tests.

This is not a final whole-day profit promise. Resource consumption, automatic rest, night planning, subsequent care, facility upkeep, pet care and events can change the day. In particular the existing fatigue >=70 automatic rest check happens before the night-rest recovery and work pass. The Team view explicitly warns about that condition. Buying equipment levels does not itself multiply ordinary work output; the kitchen project wording now says so instead of promising a nonexistent return.

The existing completion service now exposes a read-only progress snapshot and counts actual catalog mission/research/facility IDs. Duplicate or unknown IDs do not stand in for legitimate goals. The original criteria remain: catalog discoveries and research, facilities at level five, and the existing ordinary bond threshold. No minimum day is added. The four beginner projects remain beginner guidance, not the complete campaign.

### Fix exploits, not successful planning

An unavailable equipment replacement used to return the old equipped item to inventory before discovering the replacement was missing. Repeating that failed action could duplicate the old item. The swap now verifies the requested item and return capacity before modifying either side. A genuine owned swap still consumes one replacement and returns one old item. Equipment bonus inspection no longer allocates a missing equipment map, so a read-only forecast cannot change serialized state.

This is a conservation bug fix, not a balance nerf or proof that all inventory arithmetic is audited. Legitimate skills, equipment, research and recovery combinations retain their previous production returns.

### Tests rather than a claimed optimal strategy

The numeric suite compares seven job categories against hand-calculated pre-refactor outcomes, covers fatigue thresholds, verifies preview/commit agreement and read-only inspection, and rejects repeated unavailable equipment swaps. A seven-day economic subsystem fixture starts from the ordinary factory resources, pays for Dairy construction and needed meal boxes through existing services, then settles actual daily work and bills. It chooses a good dairy worker and office work for the rest, but does not search for a global optimum. It skips physical navigation and introduction presentation; it is not an earned first-week full-game or speedrun test.

A separate synthetic completion fixture supplies the existing goals on day two, verifies that the ordinary root settlement accepts them and records the next morning, and then advances three more ordinary days without changing the stored completion day or repeating the completion event. It checks existing save fields, not the victory screen's physical Continue button. No reward or eligibility grant is added to production content.

Four matching English/German forecast templates extend the new UI catalog from 340 to 344 keys. The current title language list is not extended and missing language coverage still uses fallback.

## Explicitly not integrated in this checkpoint

The proposed new long-term-goals page and revised completion-screen routing are not included. A connector write of the central station file was blocked, so that file and dependent navigation changes were left unchanged; no alternative write path was used to install them. The ordinary house/report route can still overwrite the victory presentation, and the old flow-to-ranch transition needs a separate review to preserve the current location. The existing Continue Ranching button already exists; the task is to make its complete world transition reliable, not to claim that a new button alone provides an endgame.

New authored chapters, resident arcs, mastery challenges, annexes and additional story endings are also absent. A more accurate forecast and checklist do not create more content by themselves.

## Next content and balance work — proposals, not shipped requirements

Use three compatible layers:

1. **First-run purpose:** a concrete restoration chapter with resident reactions and a visible payoff. Teach a few decisions at a time, but allow experienced players to complete prerequisites early and skip explanations.
2. **Specialization and optional mastery:** let early economic success fund different experiences, not just larger totals. Candidates include a high-quality kitchen route, efficient small-crew operation, exploration-oriented equipment plans and a comfortable ranch with ample leisure. Completion should recognize what was accomplished, not merely how many nights were skipped.
3. **Personal bests and optional new runs:** record the relevant completion day without changing existing goals after success. More detailed records could later include money spent, optional projects completed or resident wellbeing. Labels must distinguish current-run records from global or competitive leaderboards; no leaderboard is implemented here.

Design a few handcrafted optional challenges with transparent conditions and distinct rewards, for example an expedition with a limited loadout or simultaneous supplies/meal production without overworking the team. These are voluntary alternatives, not new mandatory daily chores. Provide narrative/cosmetic recognition or a new play option instead of compounding permanent output boosts that immediately trivialize every remaining goal.

Reinvestment should buy a new choice. An expensive level that only raises upkeep and fills a checklist is weak longevity. Before adding wider tiers, design the specific benefit and its tradeoff, verify the maximum reserved building footprint and entrances, then price it from measured returns. Avoid exponential costs whose only effect is making the same optimal action take longer.

Do not make affection a compulsory production multiplier or a requirement to see the ordinary completion. Useful resident stories and voluntary shared activities should be rewarding in their own right, including for a player who does not optimize them.

## Evidence-driven balance plan

Compare at least a casual policy, an informed low-day policy and a specialist policy using the same starting resources and recorded seed. Log purchases, assignments, energy spending, recovery, exploration and every settlement. Measure days to the first useful upgrade, sustainable income after full bills, routes to existing completion, and how often one action is strictly superior across all resources. A solvent seven-day dairy/office baseline is a starting check, not enough to select final prices or assert that min-max players need a given number of days.

Where an option dominates, first fix wrong arithmetic or misleading information. Then consider visible opportunity costs or additional viable alternatives. Do not add an invisible penalty that detects the player succeeding. Optional discoveries and character stories should not disappear solely because the player reaches an economic goal early.

## Test infrastructure boundary

The preceding rendered station run at `0356785` was killed after its 240-second process limit (240.795 seconds, exit -9), near the final house/day checks, without a completed results.json. This is a failed run, not a pass. The enlarged existing software-rendered suite now has a bounded 360-second process limit; the workflow's overall 15-minute timeout, assertions and runtime-error rejection remain. The station fixture restores its intended small viewport after profile settings are loaded. No new workflow or write-capable patch helper is introduced, and the longer test budget is not a hardware-performance claim.

Run only isolated launchers. Existing schema16, C#12, SDK10.0.401, net8.0 and Godot4.7.2 Mono remain unchanged. Exact-head verification results must be recorded separately after the complete suite finishes.
