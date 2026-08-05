# Autopilot Specification: Game.tscn Scene Rework

Source of truth: `IDEA.md` plus the 2026-06-14 Game.tscn follow-up interview.

## Goal
Rework `OpenMakaiRanchGame/scenes/Game.tscn` and the attached Godot C# UI shell so the main game scene feels playable, readable, and coherent on desktop, web, and mobile-sized viewports.

This pass is not a complete eraMakaiRanch clone. It should tighten the existing single-scene UI shell, expose existing gameplay screens reliably, add sensible ranch/map/menu affordances where they fit the current architecture, and fix obvious code/test blockers found during verification.

## User Decisions
- Prioritize safe wins across shell polish, 2D ranch/menu affordances, and era-style density.
- Success means readable labels, buttons sized for text, enough spacing, desktop/mobile/web usability, all existing screens reachable, coherent playable day loop, and modern era-style readability where useful.
- Do not let players enter locked or unpurchased buildings.
- Locked/unpurchased buildings should remain visible but disabled with requirement text.
- Execute directly with OMG Autopilot after interview; ambiguity score: 18%.

## Acceptance Criteria
- `Game.tscn` has no vestigial duplicate top-bar nodes that waste layout space or confuse tests.
- Top bar, navigation, content cards, and action rows avoid obvious text clipping/overlap at desktop and compact/mobile-like widths.
- Side navigation and compact navigation expose all intended major screens, while unavailable/locked destinations are disabled with requirement text instead of silently entered.
- Ranch overview provides a clearer playable hub, including facility status and visible next actions that connect the day loop, town/shop/research/adventure, roster, schedule, and save/load.
- Town/building actions are visible but disabled when requirements are not met; locked state communicates the requirement.
- Smoke tests verify current scene paths, expected navigation buttons, compact navigation, and absence of duplicate top-bar nodes.
- `dotnet build OpenMakaiRanchGame\OpenMakaiRanchGame.csproj` succeeds.
- Godot headless smoke test is attempted and any blocker is reported or fixed.

## Non-Goals
- Do not import runtime CSV parsing into the Godot project.
- Do not replace the existing C# UI shell with a large new scene architecture unless required by verification.
- Do not push or commit changes.