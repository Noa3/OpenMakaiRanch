# Autopilot Specification: SFW Godot .NET Remake

Source of truth: `.omg/specs/deep-interview-sfw-godot-remake.md`.

## Goal
Rebuild `OpenMakaiRanchGame` as a Godot .NET game based on the local eraMakaiRanch project, using a SFW systems-first interpretation. The first execution must protect a full-game scaffold and implement a playable vertical slice.

Also provide repository-level GitHub starter documentation that states the remake target clearly and documents public content boundaries.

## Decisions
- Archive the current broken `OpenMakaiRanchGame` before replacing it.
- Do not push to GitHub yet.
- Use local Godot 4.6.3 Mono tooling.
- Use the local `Godot.NET.Sdk.4.6.3.nupkg` package source.
- Target the locally installed SDK path first. The machine currently has .NET SDK 10.0.300 and only the .NET 10 ref pack; `net8.0` requires installing the .NET 8 SDK later.
- Use typed C# models, services, and Godot Resource definition classes.
- Avoid runtime copied CSV files. Use seeded Godot-native resource objects in this pass, with a future manifest/editor conversion path.
- Keep adult-only original systems out of the remake. Represent them with SFW bond, mentorship, ranch work, and adventure hooks.
- Allow only policy-safe mention of mature extension points as private, out-of-mainline customization.

## First Milestone Scope
Tier 1 must complete:
- Clean Godot project shell and buildable C# assembly.
- `GameRoot` and `SceneRouter` autoloads.
- Typed data definitions and seeded catalog manifest/fallback.
- Typed `SaveState` and JSON save/load.
- Full UI navigation shell for all major systems.
- Ranch day loop: schedule assignment, end-day settlement, resource/gold/fatigue/morale changes, daily report.
- Skeleton services/screens for town, shop, adventure/combat, bond, pets, milestones, research, inventory, settings.

Tier 2 best effort:
- Shop transactions.
- Simple adventure/combat mission resolution with rewards and fatigue risk.
- Live milestone checks.

## Acceptance Criteria
- Current project state is archived locally before replacement.
- `OpenMakaiRanchGame` has a clean Godot project config, C# project, ignores, scenes, and scripts.
- The app can start, create a new game, navigate screens, assign schedules, end a day, view a report, and save/load typed state.
- All major original gameplay pillars have named services and UI entry points.
- Incomplete full-game systems have explicit method hooks and concise TODO comments.
- Build and Godot headless smoke checks are attempted and results recorded.