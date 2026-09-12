# Okachi Town — inhabitable, developing coastal settlement

Updated 2026-09-12. This document refines the accepted direction; it does not claim that the town-growth implementation exists. Read [LIVING_WORLD_PLAN.md](../LIVING_WORLD_PLAN.md), [the task board](../KANBAN.md) and [current handoff](../ASTRA_HANDOFF.md). The previous greybox diagram and routing descriptions remain in Git at `1a65868b73e1af48277d8ac9ac338e3edcc5ce34` as historical implementation context.

## Purpose and architecture

The user wants a believable place with useful buildings/rooms, merchants and a breathing background population. The town can visibly develop as the ranch supports it, ultimately suggesting roughly 300 inhabitants. These people do not all need individual existence, saved records or rendering. Important contacts retain continuity; background life is bounded presentation.

Town services still use GameRoot and existing shop/research/adventure/roster/bond/milestone/economy authorities. A spatial ID is not a mandate for a separate facade or a whole-screen universal menu. Shared premises are allowed. Do not add another treasury, time source or hidden reward simulation.

## Travel and regional fit

Preserve current south-gate labels, convenient free existing travel, saved area and return ownership unless a separately approved gameplay change says otherwise. Free portal travel is not evidence of walking along the new valley route.

[COASTAL_REGION.md](../execplan/COASTAL_REGION.md) describes candidate common coordinates, terrain/water and seams. The rejected candidate remains opt-in. A larger final town may require revised bounds; current 180 x 130 m regional bounds and tiny service footprints are blockout inputs, not an approved 300-inhabitant layout. Maintain shared orientation/height logic and realistic dry passages when revising them.

Plan a compact market/service core, quieter housing courts, appropriate work/supply access and a modest waterfront. Preserve green space, shore access and expansion reserves across all stages. Houses may align with roads and courts; arbitrary rotation is not 'organic' design.

## Service-to-place mapping to implement

These are proposed spatial homes for existing functions, not a guarantee that every new renderer, room or trade option already exists. Audit current routing/unlocks before replacement.

| Function / retained concept | Intended physical place |
| --- | --- |
| General Store / existing shop authority | Sales floor, stockroom, merchant contact, potentially dwelling above/behind. |
| Adventure Guild | Public room/counter and relevant briefing/service context. |
| Research Office | Appropriate work/study rooms; preserve the existing workshop prerequisite where still applicable. Not the same thing as the ranch office. |
| Tavern | Gastraum/common room, kitchen, operator and relevant social/recruitment uses. |
| Bathhouse | Real public bathing/support spaces with existing nonduplicated actions and gates. |
| Town Hall / civic contact | Initially modest administrative/supply house; account desk, planning context and adjacent supply yard. Exact official/landlord identity depends on lore audit. |
| Planning / construction | Readable civic contact/board and actual before/construction/after premises, not an empty new progression menu. |

Entering a building does not automatically open an overlay. Interact with a relevant NPC or object; show the contextual actions and preserve convenient information elsewhere. Own and important public buildings are enterable. Private homes can remain closed unless a use/story calls for entry; avoid identical bait doors and empty buildings as advertised rewards.

## Proposed growth structure

1. Modest supply settlement: core functions in small/shared premises, simple market, dwellings and landing.
2. Market settlement: covered stalls, improved supply yard, housing court and local access improvements.
3. Growing coastal settlement: work yards, expanded public/tavern spaces, homes and shore route.
4. Small coastal town: multiple coherent neighborhoods, developed civic premises and an appropriately modest waterfront.

These stages are provisional visual production targets. They are not original-game canon, fixed populations or prices. Retain all already available necessities in earlier smaller premises; do not remove existing services to fabricate progression. Reserve permanent landscape/breathing room as well as future buildable plots.

LW-09 audits original/current taxes, gold, mana/energy, points and facility progression. Only after the actual pool, obligation, ledger, trigger and useful outcome are decided should LW-12 enable a payment/development binding. Avoid double deductions, repeated credit from reloads, free 'donations' of sold goods, new compulsory trips and penalty/cost escalation. Efficient contributions can accelerate growth; no arbitrary minimum-day gate or loss of finished buildings after missing optional support.

Show a meaningful before/work/after change: material delivery, bounded workers/construction, then a usable public place. Town labor participates; the player does not manage every nail or citizen. Keep essential routes usable and perform geometry/collision/nav changes only at a safe transition. Retain stable merchant inventory/story/development state through scene changes and saves.

## Ambient life

Use a bounded pool appropriate to zone, time, weather and development. Daytime shopping/work/rest, evening social places/window lights, quieter nights and rain shelter should agree with actual visible premises and audio. Generic passersby need no complete household simulation. Do not populate the ranch roster with the implied town population.

Keep important contacts and active conversations stable. Avoid visible popping, synchronized loops, obstructed doors and overlapping occupied seats. Reduce distant background work rather than making 300 agents a prerequisite. The user does not manage town beds or meals. New character designs, identities and economic automation are not side effects of ambience.

## Local asset and acceptance scope

Local Astra models the civic/supply house, useful market courtyard, a restrained dwelling/workshop family and reusable construction-stage parts. Preserve editable sources and source/license records. Reuse the existing art direction and recovered resources. Final engine pictures, not just Blender renders, judge fit. General service/finance architecture remains targeted integration work.

First finish one connected civic/market segment and one useful visible upgrade, then expand quarters. Actually walk ranch -> town -> relevant interior -> shore -> return with collision/camera/companion checks. Verify service entry, unlock reasons, return ownership, saved area, merchant state and load/replay-safe development. Check stages at day/evening/night/rain, real Forward+ low/high and relevant localization. Teleports, a calculated nav path or an older suite count are not town acceptance.

Implementation status belongs to LW-02/LW-08..LW-20 on the board. Old rules allowing civic premises to remain only facades, assuming collision-free buildings or requiring unchanged service transforms are not binding for the new world. Stable gameplay IDs, commands and verified existing behavior remain binding.
