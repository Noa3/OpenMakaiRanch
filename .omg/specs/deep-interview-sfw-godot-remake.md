# Deep Interview Spec: SFW Godot Remake

## Goal
Rebuild the local eraMakaiRanch project as a Godot .NET game in `OpenMakaiRanchGame`, prioritizing a full-game scaffold that names and wires every major gameplay pillar, then deepening the playable systems step by step.

The remake should stay close to the original systems at the structural level while targeting a SFW presentation. Original adult-only interaction content must not be recreated explicitly. Where direct porting is inappropriate or too large for the current pass, create clear service methods, data models, and TODO skeletons showing what is needed.

## User Decisions
- Workflow: interview first, then continue Autopilot.
- Cleanup: archive the current state before replacing the broken Godot prototype.
- GitHub: do not push yet. Keep GitHub upload/checkpoint for a later approved milestone.
- Content boundary: SFW systems-first remake.
- First-run ambition: scaffold the full game and implement as much as possible in this run.
- Fallback priority: protect the full system scaffold if scope pressure appears.
- Data strategy: Godot-native resources and typed C# Resource/model architecture. No runtime dependency on copied CSV files.
- Assets: user confirmed local assets may be used, including all local asset folders. Distribution risk should still be documented where relevant.

## Core Systems To Represent
- Game/application flow: title/new game/load game/main play loop.
- Time cycle: day number, phase progression, end-of-day settlement.
- Character roster: identities, stats, skills, traits, fatigue, morale, bond, schedules.
- Schedule system: SFW jobs, rest, chores, mentorship, ranch work, adventure assignment.
- Ranch production: facilities, work output, upkeep, harvest/resource generation.
- Economy: gold, income, expenses, loans/taxes hooks, daily report.
- Inventory/items/equipment: catalog, quantities, purchase/sell hooks, equipment slots.
- Town hub: shop, hardware/facility, research, guild/adventure, town hall hooks.
- Bond and mentorship: SFW replacement for stripped interaction systems.
- Adventure/combat: party setup, mission resolution, turn/battle service skeleton, rewards/injuries.
- Pets: purchase/care hooks.
- Milestones/unlocks: progress tracking and rewards.
- Skill/research tree: ability unlock hooks and resource costs.
- Save/load: typed save state, no freeform dictionary gameplay state.
- UI: navigable Godot Control shell for all major screens, with a playable ranch/status/day loop.

## First Verified Milestone
The first milestone should prove the architecture and provide a playable vertical slice:

1. Clean Godot .NET project configuration.
2. Full service/model scaffold for all core systems above.
3. Godot-native data/resource layer with seeded character, item, job, facility, milestone, and mission data.
4. UI navigation shell exposing ranch, roster/status, schedule, town/shop, adventure/combat, milestones, and settings/save-load areas.
5. Working ranch day loop: assign schedules, advance day, produce resources/income, apply fatigue/morale changes, show a daily report.
6. Working simple adventure/combat resolution that produces rewards and risk outcomes.
7. Working save/load for typed game state.
8. Build/run verification with the local Godot .NET executable where available.

## Non-Goals For This Pass
- Do not recreate explicit adult content.
- Do not keep copied CSV files inside the Godot project as runtime data.
- Do not claim complete feature parity with the original until implemented and verified.
- Do not push to GitHub unless the user approves a later checkpoint.
- Do not spend this pass on broad platform exports or CI before the local game runs.

## Acceptance Criteria
- `OpenMakaiRanchGame` opens as a coherent Godot .NET project.
- The project has clean ignores for generated Godot/.NET artifacts.
- All major gameplay pillars have named C# services/models and UI entry points.
- The playable loop can start a game, view roster/status, assign work, end a day, view settlement results, run a simple adventure, and save/load.
- Systems that are too large for the current pass contain explicit method hooks and concise TODO comments explaining intended behavior.
- Verification evidence is collected before completion.

## Interview Transcript Summary
- User requested cleanup, local state preservation, and a Godot .NET recreation of `eraMakaiRanch-game-eng-translation` in `OpenMakaiRanchGame`.
- User chose interview-first, archive-before-replace, no GitHub push yet, and SFW systems-first content.
- User wants the result to be as close as possible to the original, with skeleton methods/comments where full porting is not possible.
- User accepted SFW replacement systems: bond/mentorship, ranch specialization, and adventure/guild gameplay as needed.
- User chose Godot-native resources as the data strategy.
- User confirmed local assets may be used.
- User wants all major systems if possible, with full scaffold as the protected fallback priority.

Ambiguity score after interview: 18%.