# Implementation Plan: Game.tscn Scene Rework

## Phase 1: Scene And Shell Cleanup
1. Remove duplicate direct `TopBar` children from `Game.tscn`; keep the two-row top bar used by `UiShellController` paths.
2. Ensure root, margin, sidebar, compact navigation, and content nodes have stable size flags/minimums for responsive layouts.
3. Update smoke tests to assert the actual navigation path and duplicate-free top bar.

## Phase 2: Responsive Layout Improvements
1. Add reusable UI helpers for readable action rows, building/status cards, and disabled requirement text.
2. Replace fragile `HBoxContainer` action rows in high-traffic screens with wrapping `HFlowContainer` rows where button text can grow.
3. Tune compact top-bar/chip sizing and button minimums so mobile-like widths do not overlap.

## Phase 3: Playable Hub And Locked Destinations
1. Improve Ranch Overview as the main play hub: status summary, facility/building status, next actions, stockpile/progress/report.
2. Keep all major screens reachable from navigation unless intentionally contextual; disable gated destinations with tooltip/status requirement text.
3. In Town Hub, show buildings/services visibly but disable entry when unpurchased/unavailable, with requirement labels.
4. Treat level 0 facilities as unpurchased; level >= 1 as open.

## Phase 4: Verification
1. Run `dotnet build OpenMakaiRanchGame\OpenMakaiRanchGame.csproj`.
2. Run Godot headless smoke test: `.\Godot_v4.6.3-stable_mono_win64_console.exe --headless --path OpenMakaiRanchGame --quit-after 5 -- --run-smoke-tests` from repo root, using the actual executable name present.
3. Fix touched-path failures for up to 5 QA cycles.
4. Run read-only review/validation passes for functional completeness, quality, and security-sensitive regressions.

## Guardrails
- Keep changes scoped to `Game.tscn`, `UiShellController` partials, smoke tests, and directly needed helpers.
- Preserve the existing C# single-scene UI architecture.
- Do not add runtime CSV parsing.
- Do not create or restore explicit adult content.
- Do not commit or push.