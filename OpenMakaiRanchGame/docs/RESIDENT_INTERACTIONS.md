# Non-explicit resident interactions

2026-09-11. Continuation on `feature/world-stations-and-interiors-20260911` / PR #15.
This document describes the new scope. Exact executed CI receipts belong in the validation
checkpoint after the branch has run; writing tests is not evidence that they pass.

## Player flow

Approach a resident and interact. The overview shows resources, a free conversation and four
sections: Care, Practice, Companionship and Work. Gifts are a separate choice under Care.
Only the current resident is addressed. Back to the conversation and Back to the world are
separate controls. The NPC pauses while addressed and resumes its existing target afterward;
this does not pay work or change a job. The owner's `anon` record is not another interaction
partner; it directs to the ranch house instead of granting self-care relationship rewards.

Conversation is read-only and free even at zero player stamina. It reports a situational line
about tiredness, mood, rest or the assigned job. Encouragement is the distinct progression
action using Visit.CareTalk. A meal, gift, recovery break and mentoring reuse their existing
shared effects. Only eight explicitly listed ordinary gifts are offered, not every Keepsake
(including quest/adoption items). An unavailable action has a visible reason and costs nothing.

Ranch/craft/combat basics/magic practice reuse TrainingService. Each costs 20 player stamina
(the shared Mentorship budget), 10 resident energy and the existing talent-adjusted fatigue.
At most one practice per resident and two across the ranch per day. A tired, unwell or very
low-morale resident cannot take a lesson; Night remains for its existing night plan. Skill
caps, invalid focus and integer overflow reject before any costs. Mentoring is separate and
available once per resident/day for the existing 20 stamina. Care actions each have one
per-resident daily receipt. Talking/inspection/cancelling menus never consumes those receipts.

The raw TrainingService bug that charged Energy/Fatigue/Morale before its invalid-focus switch
has been removed. Existing zero-MagicPower testing now explicitly uses a living (50 HP) fixture;
its original assertions remain. Visit meal/rest recovery respects definition/override energy
limits and never lowers an already boosted value. This does not audit every older consumable.

## Authority, saving and translation

GameRoot validates captured generation, day, phase, combat, pause and settlement. A command stays
busy through notifications so a synchronous observer cannot start a second resident action.
World dispatch additionally requires the matching open resident context and current proximity.
Old/hidden/remote UI actions do not charge. Inspect does not normalize or mutate gameplay.
Existing raw simulation/legacy APIs remain; the new daily limits apply to this world dispatcher,
not arbitrary mods directly calling raw Visit/Bond/Training services.

FlagService per-character integer slots 1_230_300–1_230_305 store the last action day, not an
unbounded history. Save schema stays 16. The existing next-day cycle resets the shared training
count/stamina; earlier receipt days naturally unlock next-day actions. Ordinary daily production
and rest remain in DailySettlementService. No new clock, economy or adult training dispatcher.

62 new matching English/German keys cover reasons, controls, ordinary gift names and results.
New displays use whole sentences and stable action IDs, never parse translated text to detect
success. Full results appear in scrollable content; page/target survive locale refresh. Missing
Japanese/new-language entries retain English fallback. This is not complete-game localization.

## Test scope / remaining work

New numeric fixtures cover free chat at zero stamina, read-only previews, rejected/duplicate
commands, exact care/item costs, special-gift exclusion, training caps/overflow/wellbeing,
shared daily slots and saved receipt/new-day behavior. The new rendered journey uses actual
resident-page/care/practice/gift/Sleep buttons, language changes at small window sizes, NPC wait,
hidden/remote rejection, root reentrancy and same/next-day save/load. Staged proximity, inherited
resources and selected skill/tiredness values are explicit fixtures, not an organic playthrough.

All existing tests remain wired. Not every resident story, mission, path, physical input device,
seven-day balance or final animation is certified by these new tests. Character-specific arcs,
full ordinary result localization in older services, equipment presentation, safe recovery of
collapsed characters through the rest of the game and full first-week balance remain open.
`NSFW_CONTENT_HANDOFF.md` lists omitted content without implementing explicit scenes.
