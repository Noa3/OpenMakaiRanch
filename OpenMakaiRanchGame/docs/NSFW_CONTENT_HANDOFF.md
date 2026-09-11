# Resident interaction content handoff

2026-09-11. Scope: missing content and integration boundaries, not explicit scene scripts.
Original files, source IDs and role names have not been renamed or mass-deleted. See also
AUTHOR_CONTENT_HANDOFF.md for the separate shared-evening and family-planning boundaries.

## Implemented non-explicit resident route

`src/Gameplay/ResidentInteractionService.cs` defines ordinary conversation, encouragement,
a meal, an ordinary gift, a recovery break, mentoring and four practical lessons (ranch,
craft, combat basics, magic). It delegates effects to existing services. Conversation is
free/read-only. Progression actions spend the shared daily stamina and have fixed per-resident
receipts. Practical lessons additionally use the existing two-session ranch-wide limit.

`src/App/GameRoot.ResidentInteractions.cs` validates session/day/phase, combat, pause and
settlement and holds a reentrancy guard through state notifications. The world partial of the
same name rechecks physical proximity and that this resident's panel is still open.
`src/Ui/WorldStationPanel.Residents.cs` offers small contextual sections, not the old global
management hub. It does not discover or execute arbitrary actions from an adult-action catalog.
Ordinary care and practical skills do not require romantic/adult content approval.

## Not supplied / remains an authoring gap

| Area | Location to inspect | Open work |
| --- | --- | --- |
| Legacy adult training | `src/Gameplay/MatureServices.cs`, `src/App/GameRoot.cs` (`PerformTraining`), `data/training_actions.json` | Not connected to this new resident panel, expanded or renamed. The new practical lessons are not a replacement implementation of the adult catalog. Any separate content revision needs its own deliberate review and tests; do not auto-import every ID as a button. |
| Older visit/training screens | `src/Ui/UiShellController.Screens.cs` (`RenderTraining`, visit/mental screens) | Some internal service renderers still contain old presentation. The new resident route does not open them. Their complete redesign and any explicit content are outside this slice. |
| Character-specific conversations | `src/Ui/WorldStationPanel.Residents.cs`, `locale/ui/*.json`, `data/characters.json` | The new conversations reflect tiredness, morale and today's job. Distinct personalities, story arcs, voice, animation and authored dialogue remain to be written. No fabricated character identity approvals are recorded. |
| Romantic content | `src/Gameplay/DatingService.cs`, `SharedEveningService.cs`, `WorldStationPanel.Projects.cs` | Existing voluntary companionship/shared-night boundaries remain. Explicit prose, images, animation, sound or coercive sexual scenes were not supplied. Unknown/minor/ambiguous eligibility is not upgraded automatically. |
| Family progression | `src/Gameplay/LifecycleServices.cs`, `src/Core/Models/SaveModels.cs` | Not newly implemented here. Sharing a quiet night does not imply a pregnancy trigger. Household/family timing and care responsibilities require a separate opt-in design. Minors must stay outside romantic/adult targets. |
| Final presentation | `src/Character/`, `src/World/RosterRig.cs`, world/resident UI | Conversation currently pauses one stand-in avatar. Final talking/work/practice gestures, portraits and character art are still missing; no explicit assets were created. |

Do not treat a neutral fade, a translated label or an adult-looking numeric age as a content
review. Keep current save files, canonical settlement and source reference data intact. Runtime
utility/graphics code and the existing private test fixtures are not approval of production designs.
