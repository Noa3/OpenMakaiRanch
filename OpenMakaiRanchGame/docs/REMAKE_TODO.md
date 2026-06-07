# OpenMakaiRanch Remake TODO

## Scope Baseline (Confirmed)

- Remake target: `eraMakaiRanch-game-eng-translation` -> Godot .NET in `OpenMakaiRanchGame`.
- Data strategy: no runtime CSV parser. Data should be authored and stored in Godot-optimized formats.
- Platform target: desktop, mobile, and web.
- Public repository stays content-safe.
- Mature-content support, if any, must be a private extension module outside public mainline.

## Current State Snapshot

Already present:

- Godot .NET project scaffold and scene flow (`Bootstrap`, `MainMenu`, `Game`).
- Typed save model and save/load service.
- Core service layer for ranch, economy, schedules, adventure, milestones, bond, research, pets.
- Scene-authored shell UI and dynamic gameplay panels.
- Smoke test runner for key loops.

Still missing for full remake goals:

- Deep feature parity with original systems and data breadth.
- Cross-platform readiness and per-platform export pipeline.
- Content authoring pipeline suitable for large-scale game data.
- Robust balancing, progression pacing, and long-run economy stability.
- Private extension architecture hardening for mature-content hooks.

## Priority Backlog

## P0 - Foundation Completion (Must Do Next)

- Lock target framework strategy for contributors (`net8.0` vs `net10.0`) and align CI/runtime.
- Add `export_presets.cfg` and baseline export profiles for Windows/Linux/macOS/Android/Web.
- Implement input abstraction layer (mouse/keyboard + touch + controller).
- Add responsive UI pass for mobile and small aspect ratios (safe areas, scalable fonts, touch hit targets).
- Add platform-safe save storage abstraction (desktop files, web IndexedDB/local storage behavior, mobile sandbox paths).
- Expand smoke tests to cover: new game flow, save/load across scenes, mission loops, and recruitment edge cases.

## P1 - Godot-Optimized Data Pipeline (No CSV Runtime Parser)

- Replace hardcoded seed-only approach with Resource-based game database assets:
  - `CharacterDatabase.tres`
  - `JobDatabase.tres`
  - `ItemDatabase.tres`
  - `MissionDatabase.tres`
  - `MilestoneDatabase.tres`
- Add editor tooling to validate IDs, references, and duplicate keys before runtime.
- Add lightweight schema versioning/migration for content assets.
- Optional one-time migration tool (offline only) to convert legacy source data into Godot resources.
  - Not used at runtime.
  - Safe to keep under `Tools/`.

## P1 - Gameplay Depth and Parity Expansion

- Expand mission catalog, encounter variety, and reward tables.
- Add facility specialization tree and meaningful tradeoffs.
- Add richer schedule outcomes and event-driven daily reports.
- Add persistent world progression (town state, contracts, unlock chains).
- Add companion relationship progression content (safe public track).
- Improve combat readability and deterministic replay/debug hooks.

## P2 - Multi-Platform Production Readiness

- Desktop:
  - Packaging scripts for win/linux/mac.
  - Proper icon/splash and signed builds strategy.
- Mobile:
  - Touch-first shortcuts for core actions.
  - Performance profile for low-memory devices.
  - Suspend/resume handling and background-safe autosave behavior.
- Web:
  - Web export constraints audit (threading, file access, memory budget).
  - Save synchronization strategy and migration path.
  - Asset size budget and lazy-load strategy.

## P2 - QA/CI/CD

- Add GitHub Actions matrix for build + smoke tests.
- Add export validation job (headless where possible).
- Add content lint job for Resource data integrity.
- Add release checklist automation and changelog generation.

## P3 - Private Mature-Content Extension Track (Non-Public Mainline)

Public core responsibilities:

- Keep only neutral extension interfaces and feature flags in public repo.
- Keep default implementation as no-op placeholders.

Private extension responsibilities:

- Implement separate plugin/package that binds to mature-content hooks.
- Keep all private assets/content/scripts outside public repository.
- Use explicit build profiles:
  - `PublicCore` (default)
  - `PrivateMatureExtension` (private only)
- Add compatibility contract tests so core updates do not break the private extension API.

Suggested technical TODO for this track:

- Define stable extension contract around `IMatureContentHooks`.
- Add extension discovery/loading path with strict null-safe fallbacks.
- Add telemetry/logging guardrails to avoid leaking private extension state into public logs.
- Add legal/compliance checklist per target platform before private distribution.

## Done Definition (Milestone Gate)

A milestone is complete only when:

- Desktop/mobile/web builds are exportable from the same content source.
- Save compatibility is preserved across at least one schema upgrade.
- Smoke + regression tests pass in CI.
- Public build has no private-extension dependency.
- Private extension (if enabled in a separate private repo) passes compatibility tests.

## Open Questions For You

Please answer these to lock the next implementation wave:

1. Framework target for contributors: do you want to switch to `net8.0` now, or keep `net10.0` temporarily?
2. Platform order: which should be first-class first (desktop, web, or mobile)?
3. Data authoring preference: pure `.tres/.res` assets, or JSON-in-Godot with import to Resources?
4. Legacy conversion: should we build an offline conversion tool now, or hand-author initial Godot databases first?
5. Mature extension architecture: do you want one private plugin repo, or multiple modules (events, scenes, assets separated)?
6. Release model: single unified build cadence, or public core and private extension on separate release cycles?
7. Minimum supported mobile/web specs: do you have target devices/browsers now?
8. Language targets: keep EN first, or multi-language from start (JP/EN/ZH)?
