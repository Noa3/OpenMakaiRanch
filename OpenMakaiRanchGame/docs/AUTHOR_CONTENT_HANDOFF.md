# Non-explicit relationship slice and author content handoff

2026-09-11. Keep existing source/content IDs intact. This file describes missing content and technical boundaries, not explicit scene scripts or instructions to bypass eligibility checks.

## Shipped code in this continuation

`src/Gameplay/SharedEveningService.cs` supplies a reversible overnight invitation at Night after the introduction, while at the ranch and with Rest selected. Both participants require reviewed adult state AND definition, a voluntary active invitation, an existing romantic relationship, and a companion who is well enough to agree. Pressured/forced approaches do not qualify. A refusal/cancellation has no relationship penalty.

`src/App/GameRoot.SharedEvening.cs` validates session/day and combat/settlement boundaries. `GameRoot.DayLoop.cs` captures an eligible plan before settlement and completes it only after the ordinary settlement succeeds. Rest and the separate bath bonus are not run twice. The small acknowledgement is +1 Bond/+2 Morale once for the completed night. No money, free resources, conception chance or additional energy is added.

`src/Ui/WorldStationPanel.Projects.cs` provides voluntary invitations/outings at a nearby resident and the overnight option at the house. `src/World/WorldGameController.SharedEvening.cs` reuses the normal blackout/reveal layer with hearts and a short non-explicit morning message. Reduced Motion is respected. The summary is companionship only, not a hidden pregnancy trigger.

Durable receipt flags: per-resident 1_230_200 planned day and 1_230_201 last completed day; global 1_230_202 last completed shared night. Current save schema remains 16. The existing FlagService owns serialization. Stored daily report lines retain their saved language; the live interface uses text keys. Full retroactive report localization is still open.

## Missing or deliberately not supplied

| Area | Relevant location | Remaining work |
| --- | --- | --- |
| Character-specific romance | `data/characters.json`, `src/Gameplay/DatingService.cs`, resident UI partial | Author distinct non-explicit dialogue and review adult identity/design provenance individually. No existing character has been globally relabeled or automatically approved here. Synthetic adult test fixtures are not shipped character approvals. |
| Narrative presentation | `WorldGameController.SharedEvening.cs`, normal dialogue/transition layer | Author appropriate music, portraits, facial expressions and morning follow-ups. The current fade and neutral text are a prototype, not a finished romantic scene. Explicit prose, imagery, animation and audio are not supplied. |
| Family planning | `src/Core/Models/SaveModels.cs`, `src/Gameplay/LifecycleServices.cs`, a future dedicated household service | Not implemented. A separate, explicit, optional family-planning choice is needed, distinct from sharing a bed. Decide narrative timing, care responsibilities, cancellation boundaries and save fixtures before adding a family-state machine. Do not represent a decorative heart effect as proof that a pregnancy system exists. |
| Family safety boundaries | future household roster and existing `AdultEligibilityGate` | Family members who are minors must remain outside adult/romantic action targets and presentations. Do not inherit adult eligibility from parents or turn child characters into adult content through a numeric age edit. |
| Legacy content and terminology | `MatureServices.cs`, `training_actions.json`, original reference directory | Existing content is not mass-renamed, expanded or deleted by this slice. Any separate author revisions need source mapping, review and the same canonical simulation boundaries. The original reference directory remains read-only. |

## Rendering and gameplay are separate

Skipping the fade, disabling effects, changing language, reopening a report or using Reduced Motion must not replay settlement or alter pregnancy/family state. In this slice there is no pregnancy/family mutation at all. A future family system needs explicit authoring and independent tests; it should never be inferred from relationship score alone.

The current per-character approval requirement can leave the overnight action unavailable in stock content. That is an honest content-review gap, not permission to disable the guard or mark every definition as adult.
