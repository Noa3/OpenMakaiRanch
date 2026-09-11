# Ranch projects, shared evenings and building plots: validation

Checkpoint **2026-09-11**, PR #15, branch `feature/world-stations-and-interiors-20260911`. Verified code head **`67a052806e9241f68110e13494e944c90bac6203`**, PR merge tested **`441f0730c4b507d592fad3aa16353d1bce3bca4c`** against main `3209f4c`. Documentation-only updates may follow. This extends, rather than replaces, LOCALIZATION_UI_VALIDATION.md.

## What was implemented

**Optional projects:** `RanchProjectService` derives four projects from existing facilities, restoration/delivery receipts and the first-patrol milestone. Places opens a project list with purpose, progress and one physical next destination. Reading/tracking does not assign jobs, teleport, create quest payments or change the clock. Missing quiet-corner supplies point to Office Work; the kitchen project targets level two because a fresh game already owns level one. Existing completion/reward authorities remain unchanged. This is guidance, not a finished chapter campaign or new victory condition.

**Non-explicit shared nights:** at the ranch house, an eligible adult pair with a voluntary active outing and romantic relationship can plan or cancel a quiet night after choosing Rest. Both source definitions and state require existing review; a companion who is exhausted, collapsed or unwell cannot qualify. Pressure/force is rejected. Planning/cancelling has no reward or penalty. GameRoot captures the plan before ordinary settlement and records it once after successful settlement. +1 Bond/+2 Morale acknowledges the night without repeating Rest, the bath bonus, gold or stamina recovery. Existing flags save the pending plan and receipt. The normal transition layer shows a black screen, heart and non-explicit morning message; Reduced Motion is respected. No pregnancy/conception/family mutation was added. Stock character eligibility was not relaxed; synthetic approved test identities are not production approvals.

**Reserved space:** eight metre-scale authored plots include conservative rotated roof envelopes, clear entrances, world bounds and the town-gate approach. The presentation builder uses these instead of independently expanding nearby station landmarks. Facility levels retain their original numerical progression, with narrow overflow rejection before payment. Visual grades 0-3 add interior equipment while preserving shell size, door and collision count; higher numerical levels retain grade 3. Trees are checked against the same reservations using imported mesh bounds or conservative placeholder crowns before being added. This does not constitute free-placement, full-route or all-prop validation.

**Translation and author handoff:** 47 matching English/German keys extend the previously verified 200-key slice to **247**. Literal project/evening keys join the static validator. Selectable locales remain en/de/ja; missing Japanese new keys use English fallback. LOCALIZATION_ROADMAP.md plans ten localized editions excluding Russian using the displayed July 2026 Valve survey as a proxy, not a census of all gamers. AUTHOR_CONTENT_HANDOFF.md identifies missing dialogue/art/review and separate family planning without supplying explicit scenes or changing original content.

## Exact-head executed result

| Workflow | Run | Result |
| --- | --- | --- |
| Build Smoke Check #644 | `34611892143` | success |
| Godot 4.7 Mono CI #636 | `34611892078` | success |
| Rendered UI acceptance #74 | `34611892077` | success |

- **1,778 SMOKE OK / zero SMOKE FAIL / one SMOKE PASS**, including **38 new ranch-design assertions**. The existing 1,740 remain.
- **432/432 rendered checks**, **56 PNGs**, one UI ACCEPTANCE PASS and **zero UI runtime ERROR/SCRIPT ERROR**. This adds 24 checks/three captures to the previous 408/53 checkpoint.
- **55 Python tests pass**. The same local suite was executed and passed in 20.256 seconds. The locale validator reports 247 valid English/German keys; no engine was claimed to run locally.
- C#12, SDK10.0.401, net8.0, Godot4.7.2 Mono and schema16 remain unchanged. Build success is not a zero-warning claim; pre-existing NuGet/nullable warnings remain tracked separately.

## Actual journeys and fixture limits

The rendered project journey clicks Places -> Projects -> one next step and compares the complete serialized game state and generation. It proves read-only navigation and small-viewport geometry, not organic acquisition of every project reward.

The shared-evening journey uses separate test definitions in the isolated profile, actual house invitation/cancellation/reinvitation buttons, pending-plan save/load, the actual Sleep button, an input-locked reveal, duplicate-redraw rejection, return to world control and completed-receipt save/load. Production definitions are never approved for the fixture. Temporary state identities/settings are restored and disposable slot99 is removed. The explicit Reduced Motion preference must be persisted through the normal setter: LoadSlot replaces settings from the profile. The final fixture proves this rather than disabling the transition assertion or bypassing the real load. Proximity and strong relationship values are synthetic; this is not an earned romance playthrough.

Numeric fixtures cover eight non-overlapping plots, deliberately overlapping/out-of-bounds/invalid authoring, tree mesh local-transform bounds, stable grades through level30/int.MaxValue and downgrade, unrepresentable upgrade costs, read-only project evaluation, voluntary/eligibility/wellbeing denial, cancellation without punishment and idempotent saved night receipts. Existing first-day, language picker, station Build/Assign, held-key doorway walking, night/report, tactical combat and store/save journeys remain.

## Evidence receipts

Downloaded ZIP bytes were independently SHA-256 checked:

| Evidence | Artifact ID | SHA-256 |
| --- | --- | --- |
| Godot import/smoke | `10268154529` | `cc8cdca3579996821b2c9db25688eee1a3e009212e6c82ae14c84af6b13f8dfa` |
| Rendered UI/review source/screens | `10268299585` | `56e5783ba79d2825a670875621b5ec51eb0618f6cffa317c85be5c1d44f51750` |

Smoke: `.artifacts/godot/smoke-i4waur_i/console.log`; import `import-mwk6y4r4`. UI: `godot/ui-pt33744n/results.json` and console; import `import-wv4bmxen`. Review-source commit.txt is the exact merge SHA above. Project list, planned evening, black-screen heart/morning reveal and walk-in Dairy PNGs were opened and inspected. The reveal is visibly non-explicit. The images also show remaining duplicated planning status, large world labels and HUD/tutorial clutter; passing checks do not make these final presentation.

Smoke has exactly five intentional invalid-save rejection diagnostics. Import exits successfully with the known EditorSettings shutdown diagnostic; software rendering retains its VSync warning. No new filtering hid runtime failures. Artifacts are expiring evidence and whitelisted review inputs, not a complete game/export or personal profile.

## Remaining work

An earned first-week balancing run, character-specific arcs and visible chapter payoff remain open. The project journal is not a complete campaign. Family planning is deliberately separate and absent. Stock eligibility review may leave shared nights unavailable; do not bypass it to make a demonstration possible. Final building stages, annexes/upper floors, all entrances/NPC routes/camera angles, other props and dynamic shader/skeleton bounds are not certified by static plot/tree checks. Stepwise creation, broader translation, export/device testing, organic town/river, final assets and Forward+ hardware performance remain open.

Concurrent native language-popup repairs and their documentation were preserved after non-fast-forward protection rejected an outdated tree. Temporary exact-preimage patch helpers deleted themselves. Main and the older PR #14 branch were not changed.
