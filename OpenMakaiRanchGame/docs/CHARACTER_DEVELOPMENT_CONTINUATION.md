# Character development continuation: commands and player feedback

2026-09-12. Branch `feature/gameplay-progression-20260912`, draft PR #19.
Code checkpoint: `1dcd5a71a2ab17feb20f1b18b03f1d840952e58c`, based on
`2763afba23a02268b188fe1fe7aaddd540a6a0bb`. Verification is recorded below.

## The gameplay problem

The previous character-development slice recorded resident practice and normal root settlement,
but the older public `GameRoot.TrainCharacter` still called the low-level training simulation
directly. It omitted player stamina, the resident daily receipt and development observation.
It also lacked the resident command's context/phase and reentrancy locks. A parallel entry
could therefore produce different results from the visible resident interaction.

Players also received only capacity-crossing results, with no explicit forecast of the next
stage. Existing resident dialogue did not acknowledge recorded development.

## One root practice boundary

`TrainCharacter(id, focus)` is now a synchronous compatibility forwarder to
`TryTrainCharacter(id, focus, generation, day, phase)`. The latter maps exactly `ranch`,
`craft`, `combat` or `magic` to the corresponding ResidentAction and delegates to
`TryResidentAction`. Unknown focus values are rejected without mutation.

Both entries now share the same introduction/daytime checks, current target lookup, player
stamina (20), resident energy (10), existing fatigue/morale eligibility, one focused lesson
per resident per day and two across the ranch. They share the development observer and its
single notification. Pause, combat, settlement and synchronous observer reentry cannot open
another practice path. Failed commands neither spend resources nor create development records.

The stamped form rejects an old session/day/phase. The legacy bool form uses the current
context at invocation; queued UI work should use the stamped form, not treat the synchronous
compatibility wrapper as a captured-context command. No source-compatible low-level method
was removed. Direct `TrainingService.Train` simulation calls and arbitrary field edits remain
outside this root observer. Mentorship, adult action handlers and other services are unchanged.

## Progression a player can understand

At an ordinary resident, open **Learn and practice**. Available combat/magic buttons already
have tooltips; those now explain current stage, skill, next target and the available capacity
increase. A completed lesson repeats the updated forecast in its existing result message.
There is no new screen, image, overlay or change to world/model presentation.

For example, a first-observed combat skill of 3 forecasts the next stage at 5. One real lesson
raises it to 4 and leaves one point to go; the next valid day can reach 5 and add 5 maximum HP,
without healing current HP. The pre-existing three-stage/+15 limit has not been increased.

Forecasts use the same stored high-water marks as actual awards. Losing a skill does not
reset earned progress or create a second reward for regaining old points. A skill-limit state
explicitly explains when current lessons cannot reach the next capacity stage (for example,
starting at combat 9 with a lesson cap of 10). Completed capacity tracks do not claim skills
are also complete. Near integer capacity limits the forecast reports only the amount that
can actually fit. Magic forecasts respect the existing two-point lesson increments, including
parity at the maximum integer value and the existing zero-to-three normalization.

The snapshot is a long-term forecast, NOT permission to act now. Daily affordability and
eligibility still come from the existing resident offer. All forecast reads are free of writes.
No XP multiplier, new money source, daily task, cooldown or day-advancing operation was added.

## Residents acknowledge real changes

The existing free **Ask how things are** action can reflect the most recent meaningful recorded
ranch, craft, combat or magic skill change that still matches the resident's current value.
It has different brief reactions for the four skills and acknowledges a recorded loss rather
than describing it as growth. Imported high skills without history receive no invented story.
An unmatched obsolete journal value does not supply a misleading reflection after a direct edit.

Immediate fatigue/low energy and low morale retain priority over the reflection. Conversation
remains free at zero player stamina and never increments bond, XP, daily receipts or history.
The reflection is derived from existing recorded facts, not an additional stateful conversation
system. It may be repeated while relevant. These are contextual lines, not individual character
arcs, branching memories or a full personality simulation.

Nineteen `development.*` English/German keys are formatted when read. The pre-existing 18
`character.*` keys and 344-key UI catalog remain separate and unchanged in scope.

## Storage and integration

No new flags or saved fields were added. Existing baseline, high-water values and 12-entry
journal remain authoritative; schema 16 is unchanged. Capacity tuning is now named once in
CharacterDevelopmentService and reused by the observer and forecast. Current-save load uses
the same effective roster definition and does not re-award any development stage.

New public read: `GameRoot.GetCharacterDevelopmentTrack(id, focus)`.
New stamped command: `GameRoot.TryTrainCharacter(id, focus, generation, day, phase)`.
Existing consumers of CharacterDevelopmentSnapshot, body identity, millimetre height and 0..1
morph channels remain compatible. No model deformation or anatomical/body-profile edits were
introduced by this continuation. Original ERA files and parallel visual PR #18 remain untouched.

## Verification

Verified code head: **`1dcd5a71a2ab17feb20f1b18b03f1d840952e58c`**. The downloaded
rendered review artifact records tested PR merge **`3c5834c596a14a37c1edae869bff482dc99d1006`**.
GitHub compare confirms this merge has the same tree as the code head.
The merge receipt is not the branch head. Subsequent documentation-only commits do not change
these code results. The initial implementation's 2,047-check evidence remains historical.

| Workflow | Run | Result |
| --- | --- | --- |
| Build Smoke Check #676 | `34661101769` | success |
| Godot 4.7 Mono CI #666 | `34661101932` | success |
| Rendered UI acceptance #105 | `34661101831` | success |

The downloaded smoke console contains **2,091 SMOKE OK, zero SMOKE FAIL, one SMOKE PASS**.
**44** assertions cover this continuation; the prior 47 character-development assertions remain.
The rendered results contain **526/526 passing checks**, overall success, and 64 PNG captures.
The existing UI scenarios have no ERROR or SCRIPT ERROR. They are regressions of the existing
presentation, not a new model-morph test or certification that every new tooltip was captured.

The full-checkout CI Python run passes **78 tests**, including the two original-source
Git-blob receipts. The local review-source run separately discovers 78: **76 pass and two
are explicitly skipped** because its archive excludes the original ERA source. The existing
344-key UI locale validator also passes; the 19 new English/German gameplay keys have separate
literal/placeholder tests. No local C# compiler or Godot runtime execution is claimed.

The new runtime checks exercise forecasts versus actual capacity awards, lost-skill catch-up,
imported skill limits, integer capacity saturation and magic lesson parity. They also exercise
real resident offer/result/chat feedback, fatigue and morale priority, recorded losses,
stale-history rejection, German formatting and read-only repeated conversations.

Root tests execute the legacy and stamped practice entries with the existing resident command,
checking identical costs, daily receipts, save/load, next-day reset and real next-day capacity
growth. Stale session/day/phase, invalid focus, introduction, night, pause, combat and insufficient
stamina reject without mutation. Synchronous notifications cannot reenter practice or advance
time. The two ranch-wide lessons remain shared even when mixing entry points.

These are isolated fixtures with staged prerequisites, not an organically earned campaign.
They refuse an occupied disposable slot 99 and use the isolated launchers. Build warnings for
inherited nullable/obsolete calls and NuGet sources remain. The smoke output retains five
intentional invalid-save ERROR diagnostics; the known import EditorSettings shutdown diagnostic
and software VSync warning also remain. No workflow, test or error filter was weakened.

### Evidence receipts

Smoke artifact `10286674399` from run `34661101932` was downloaded as
`character-continuation-smoke-1dcd5a7.zip` (78,071 bytes), SHA-256:
`b52efe2be4579aadc8521bb162619d54c220d4fb6d2704cd3528f8af5c678004`.
Rendered/review artifact `10286994155` from run `34661101831` was downloaded as
`character-continuation-ui-1dcd5a7.zip` (5,857,577 bytes), SHA-256:
`433de1a4da4d12e6cf8451c3c793afcd4144751ae82807d921f323d1102c9183`.
These are hashes of the actual downloaded ZIP bytes, not service-side upload digest claims.
The final packaging check corrected earlier transcribed archive digests, UI archive size and
merge receipt; the retained console/result counts are unchanged.
The evidence package contains selected logs, receipts, result files, these documents and a
manifest. It is not an exported playable build or a full repository checkout.

```bash
python -m unittest discover -s Tools/Godot -p "test_*.py"
python Tools/Godot/validate_locales.py
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/ui_acceptance.py --rendered --timeout 360
```

Use the isolated launchers; never execute raw test flags against personal save profiles.

## Limits and next work

This makes the existing developmental loop more consistent and visible; it does not add new
body/race/trait transformation recipes, equipment-fit effects, a complete resistance/action
matrix, original level conversion or actual model morphs. The previous ward snapshot remains
read-only and is not converted into a permission system. No intimate/coercive implementation
was added or changed. Full resident stories and long-term balance remain separate work.

Before claiming universal development tracking, identify real command owners for the remaining
stat-changing systems and integrate at their successful boundaries, not through a second XP
pass or passive per-frame polling. Future visual work should consume snapshots rather than
mutating gameplay during drawing. Keep the earlier evidence attributed to its tested commits.
