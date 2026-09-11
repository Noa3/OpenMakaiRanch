# Non-explicit resident interactions

Checkpoint 2026-09-11 on `feature/world-stations-and-interiors-20260911` / PR #15. Code head `4d3c9893a4621bd09f1e358e178f0f671ba18537` passes the complete existing CI suite plus the new resident checks. Exact counts, merge receipt, hashes and fixture limitations are in RESIDENT_INTERACTIONS_VALIDATION.md; this document describes the runtime contract, not blanket completion of every original interaction.

## Player flow

Approach a resident and interact. The overview shows resources, a free conversation and four sections: Care, Practice, Company and Work. Gifts are a separate choice under Care. Only the current resident is addressed. Back to the conversation and Back to the world are separate controls. The NPC pauses while addressed and resumes its existing target afterward; this does not pay work or change a job. The owner's `anon` record is not another interaction partner; it directs to the ranch house instead of granting self-care relationship rewards.

Conversation is read-only and free even at zero player stamina. It reports a situational line about tiredness, mood, rest or the assigned job. Encouragement is the separate progression action using Visit.CareTalk. Meals, gifts, recovery and mentoring delegate to existing services. Only eight explicitly listed ordinary gifts are offered, not every Keepsake-category item; quest/adoption items are excluded. Unavailable actions show reasons and cost nothing.

| Action | Current player stamina | Additional requirement/budget |
| --- | --- | --- |
| Conversation/inspection | 0 | No daily receipt or farmable reward |
| Encouragement | 10 | Once per resident/day |
| Meal | 8 | One meal box, once per resident/day |
| Ordinary gift | 5 | One selected allowed item, one gift per resident/day |
| Recovery break | 10 | Recovery needed, once per resident/day |
| Practical advice/mentoring | 20 | Daytime/evening, once per resident/day |
| Ranch/craft/combat/magic practice | 20 | 10 resident energy, fatigue; one focus per resident/day, two across ranch/day |

Prices come from the existing PlayerStaminaService. UI displays those canonical prices; the table is documentation, not a second cost authority. Lessons require living non-collapsed trainees, at least 10 energy, less than 80 fatigue and at least 25 morale through this new route. Night is reserved for the existing night plan. Looking around, walking and menu navigation consume no action receipt. Work output and daily recovery remain in normal settlement.

TrainingService now validates unknown/capped/overflowing focuses before spending energy or modifying fatigue/morale/the global count. Lesson skills use existing gains and talent efficiency. Meal/rest energy recovery respects definition/override limits and never lowers an already boosted value. This does not audit every older consumable or producer.

## Authority, saving and translation

GameRoot validates captured generation/day/phase, combat, pause and settlement. A resident command remains busy through state notifications so synchronous observers cannot start a second action. World dispatch adds the matching open resident context and current proximity. Old/hidden/remote callbacks do not charge. Inspect never normalizes/mutates gameplay or allocates histories. Raw simulation/legacy APIs remain; new world receipts/proximity guards are not universal restrictions on arbitrary direct Visit/Bond/Training callers.

FlagService per-character integer slots 1_230_300–1_230_305 store the last successful activity day, not unbounded logs. Save schema stays 16. Existing daily settlement resets the shared practice counter and player budget; an earlier receipt date naturally unlocks next-day actions. Current save/load keeps used receipts used within the same day and permits the next day's actions without another reset. There is no new clock, economy or adult-training dispatcher.

62 matching English/German keys cover complete reasons, controls, gift names and outcomes. Success is structured, not inferred by parsing translated text. Long results appear in scrollable content; successful persistent actions reveal their outcome. Identical limit explanations are consolidated while every affected button keeps a tooltip. Page and target survive language refresh; stale-language feedback clears. Missing Japanese/new-language entries retain English fallback. This is not complete-game localization.

## Test scope and remaining work

33 new numeric assertions cover read-only free chat/previews, invalid/duplicate commands, exact care/item costs, special-gift exclusions, training caps/overflow/wellbeing, daily slots and saved receipt/new-day behavior. The rendered journey clicks actual resident pages and care/practice/gift/Sleep buttons, checks NPC waiting, German small-window layouts, hidden/remote/reentrant denial and same/next-day save/load. Staged proximity, inherited resources and skill/tiredness values are explicit fixtures, not an organic playthrough. Shop prerequisites use real canonical transactions, not claimed physical shop clicks.

The first smoke run exposed missing fixture prerequisites, documented rather than hidden: the old budget check inherited battle injuries and the new absent-meal check still owned start meals. Those setups now declare the intended conditions; assertions and runtime guards remain. All earlier suites stay wired.

Not every resident arc, route, physical input device, seven-day balance or final animation is certified. Character-specific stories, equipment presentation, all older service outcomes, broader collapsed-character recovery and complete first-week balance remain open. NSFW_CONTENT_HANDOFF.md lists omitted catalogs and technical locations without explicit scene scripts or changes to original role names.
