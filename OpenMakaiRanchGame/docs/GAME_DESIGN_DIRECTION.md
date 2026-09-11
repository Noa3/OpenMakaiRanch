# Player motivation and ranch design

Working design, 2026-09-11. This is a remake-facing proposal, not a transcription of the original's story or a claim of complete original-rule parity. Preserve existing original systems and IDs; build a usable non-explicit presentation on top.

## Diagnosis

The project exposes many rules but has not consistently answered: what do I care about, what can I accomplish today, where do I go, and what visibly changes afterwards? Warning panels identify maintenance problems; they cannot carry the whole motivation loop. More warnings, currencies, or required chores are not the solution. Relationship numbers without choices and memories are likewise not a full relationship story.

## Player promise

Build a ranch worth coming home to: a working household, a personal place, and voluntary relationships with distinct residents. The suggested initial conflict is a disorganised ranch that needs a viable routine, not a new hard bankruptcy countdown. This framing needs the author's story approval before replacing original narrative text.

Daily loop: notice one opportunity -> choose a destination -> make a meaningful local decision -> retain time for exploration or company -> choose an evening -> see tomorrow's consequence. Walking, checking the journal and choosing an objective remain free. Keep routine recovery accessible without requiring romance, and do not make a person's affection a mandatory production multiplier.

## Implemented in this continuation

`RanchProjectService` reads current progress without modifying it. `WorldStationPanel.Projects` exposes four optional projects from Places: restoring the quiet corner, upgrading the kitchen to level two, completing three existing deliveries, and a first successful expedition. Each explains its purpose and one next destination. Existing facilities, receipts and milestones supply progress; there are no duplicate payments, quest timers, per-frame completion awards, or new victory requirements.

The supply-starved quiet-corner step directs the player to Office Work instead of merely repeating an unaffordable cost. Work still pays only through the existing daily settlement. The kitchen starts at level one in a fresh game, so the project targets level two rather than awarding a meaningless instant build achievement.

These are player-selected guidance projects, NOT a new full story campaign. Completed deliveries/restoration remain accomplished after spending their rewards; facility progress reflects current owned level. A mission's actual existing `first_patrol` milestone is used rather than treating merely opening the guild as a victory.

## Proposed next content, not implemented

1. A small opening-to-Day-7 chapter: restore one personal area, support a town request and learn one resident's aspiration. Use several valid solutions, no forced romance or exact daily schedule.
2. Resident arcs with personality, invitations, refusals without punishment, remembered preferences and follow-up dialogue. A night together should acknowledge an already positive relationship, not bypass earning one.
3. Distinct medium-term choices: comfortable household, local supplier, exploration-ready ranch. They can converge later; do not lock the player into a single career by an early uninformed choice.
4. A community celebration as a visible capstone, subject to original-story review. A small scene with residents and changed surroundings is more legible than another currency total. Existing win conditions remain unchanged until this is actually authored and tested.

## Acceptance and balancing questions

Can a new player name one achievable next step without opening a spreadsheet-like hub? Can someone who ignores romance still recover, build and finish the original main progression? Can someone who chooses romance afford an outing without losing the entire day? Does a completed project alter the world or access, not merely remove a warning? How long does reaching the next two original facility levels take with an unboosted save?

Test an earned first-week run, low-funds recovery, different roster sizes, bad weather and repeated load/resume. Track bottlenecks and time spent commuting, not only assertion counts. No claims of AAA-quality assets, long-term balance or complete branching narrative follow from this code slice.
