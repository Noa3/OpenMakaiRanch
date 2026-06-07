# Implementation Plan: SFW Godot .NET Rebuild

## Phase 1: Archive And Replace Shell
1. Create a local archive of the current `OpenMakaiRanchGame` folder under `.archive/`.
2. Remove the broken prototype folder after the archive exists.
3. Recreate `OpenMakaiRanchGame` with clean Godot project files.
4. Add `.gitignore` rules for `.godot/`, `.mono/`, `bin/`, `obj/`, `.vs/`, `*.user`, logs, and local archives.

## Phase 2: Buildable Godot .NET Foundation
1. Add `NuGet.config` pointing at `../GodotSharp/Tools/nupkgs`.
2. Create `OpenMakaiRanchGame.csproj` with `Godot.NET.Sdk/4.6.3` and local `net10.0` target. Retarget to `net8.0` later if .NET 8 SDK is installed.
3. Create clean `project.godot` with one `[application]`, one `[dotnet]`, and one `[autoload]` section.
4. Add `GameRoot` and `SceneRouter` autoload scripts.

## Phase 3: Domain Architecture
1. Add resource definition classes for characters, jobs, items, facilities, missions, milestones, skills, and pets.
2. Add typed save models: calendar, economy, roster, schedule, inventory, ranch, adventure, milestones, research, pets.
3. Add `DataRegistry` with an explicit future `DataManifest` hook plus a safe seeded fallback factory for this pass.
4. Add services for day cycle, roster, schedule, ranch, economy, daily settlement, save/load, inventory, shop, adventure/combat, town, bond, pets, milestones, and research.

## Phase 4: UI Shell And Screens
1. Create a Control-based `Main.tscn` and `UiShellController` entirely in C# to avoid fragile hand-authored complex scene trees.
2. Add navigation buttons for Ranch, Roster, Schedule, Town, Shop, Adventure, Combat, Milestones, Research, Bond, Pets, Save/Load, and Settings.
3. Render dynamic screen contents in C# using Godot controls.
4. Wire New Game bootstrap in `GameRoot` and route to the UI shell.

## Phase 5: Playable Vertical Slice
Tier 1:
1. Assign schedules for at least two characters.
2. End day and produce a daily report.
3. Apply production, gold, fatigue, morale, and bond changes.
4. Save/load typed state.

Tier 2 best effort:
1. Shop buy/sell transactions.
2. Simple adventure/combat mission resolution.
3. Basic milestone checks.

## Phase 6: Verification
Run, in order:
1. `dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj`
2. `./Godot_v4.6.3-stable_mono_win64_console.exe --headless --path OpenMakaiRanchGame --build-solutions --quit`
3. `./Godot_v4.6.3-stable_mono_win64_console.exe --headless --path OpenMakaiRanchGame --quit-after 3`
4. `git status --short`

## Phase 7: GitHub Starter Docs
1. Add root `README.md` with explicit remake target and scope boundaries.
2. Add `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SECURITY.md`, and `LICENSE`.
3. Add issue templates and pull request template under `.github/`.
4. Add a short upload checklist for first push.

## Phase 8: LFS And Launcher
1. Enable Git LFS for large local editor binaries (`git lfs install`).
2. Track `Godot_v*_mono_win64*.exe` in `.gitattributes`.
3. Add a root launcher script that opens `OpenMakaiRanchGame` in Godot.
4. Document preferred binary strategy (LFS + package-manager fallback).

## Phase 9: Backlog Definition
1. Write an explicit missing-work backlog document for the remake.
2. Include no-runtime-CSV-parser constraint and Godot-optimized data pipeline tasks.
3. Include cross-platform workstreams for desktop/mobile/web.
4. Include private mature-extension track tasks as non-public modular integration work.
5. Capture open product questions required to prioritize next implementation cycle.

## Phase 10: Portrait And Mobile UI Follow-Up
1. Read the current Godot portrait composition code and compare it against the legacy `resources/portrait.csv` sprite region format.
2. Replace hardcoded whole-PNG portrait paths with typed frame metadata for body, race, hair, face, mouth, and clothing layers.
3. Render layered portraits with cropped atlas regions and legacy offsets.
4. Rename non-English files under `OpenMakaiRanchGame/assets/portrait_layers` to English names and update their `.import` metadata.
5. Remove unreferenced exact duplicate portrait-layer PNG/import pairs and record the canonical retained files.
6. Improve menu layout behavior for compact/mobile-like viewports by avoiding visual root scaling, wrapping/trimming labels, and adding a horizontal compact navigation strip.
7. Run build and headless smoke verification, then fix blockers in the touched path.

## Guardrails
- Do not recreate explicit adult content.
- Do not copy the old CSV folder into the Godot runtime project.
- Do not push to GitHub until the user approves.
- Do not touch sibling projects unless required for build verification.
- Prefer working Tier 1 systems over deep incomplete Tier 2 work.