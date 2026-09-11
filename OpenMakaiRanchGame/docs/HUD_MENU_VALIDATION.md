# HUD and menu integration validation

Updated **2026-09-11**, PR #12, `playability/ranch-quiet-corner-20260910`. This page records retained integration contracts. Current exact rendered scope/screenshots are in **UI_LAYOUT_ACCEPTANCE.md**; the resumable checkpoint is in **ASTRA_HANDOFF.md**. C# 12, SDK, engine, target framework and save schema remain unchanged.

## Current verification

Code-inclusive head **`897871eadbdf02c19d905c6e0b33e5edf335e649`** passed Godot CI **#563 / `34546801277`**, Build **#571 / `34546801316`**, and Rendered UI **#17 / `34546801281`**. Results: **1,691 smoke assertions**, including **52 HUD/menu and nine view-state checks**; **207 separate rendered UI assertions** and **31 PNGs**; **48 launcher/evidence Python tests**. Exact artifact IDs/hashes and log diagnostics are in ASTRA_HANDOFF. Do not combine these different kinds of checks into a finished-game claim.

## Historical baseline and retained repairs

Pre-fix `a45a0bc` / CI #540 (`34536254574`) compiled but had 1,678 passing assertions and three failures: ordinary settlement twice and same-screen options scroll preservation. The three original assertions remain. Historical `9d8eb09` / #545 passed 1,691, including the then-current 34 launcher tests; its artifact `10176503313` is historical proof, not automatic certification of later changes.

`UiShellController.ViewState` composes the input extension before a ProcessFrame-boundary restore, preserves pending scroll/logical focus across notification bursts, restores focus before scroll and retains FollowFocus. Revision, screen, generation, visibility and instance guards reject retired callbacks. Snapshots retain values, not old controls; frame callbacks are one-shot.

The ordinary clock subsection explicitly establishes a completed-story Day-2 fixture. It previously activated the mandatory tutorial and then attempted ordinary settlement through that tutorial's input lock. Runtime guards, explicit Night choice, original fresh-story assertions and separate full-first-day/skip walkthroughs remain. This is a fixture correction, not a new production skip fix or organically earned progression.

## Coverage contracts

| Scope | Retained integration coverage | Boundary |
| --- | --- | --- |
| HUD/world | Active area, management visibility, hidden time/travel/management callbacks, help/Back, pause | Headless checks alone do not prove visible layout; see the separate rendered suite |
| Menus | Fifteen routes contain live content; browsing preserves gold, stamina and day; research gating | Not every command on every route |
| Player status | Immediate canonical HP with normal and empty rosters | Not every stat/bar value |
| Options | Exact burst-refresh scroll, replacement-control focus, focus-follow, route/hide cancellation, reopening, retired capture | Not every settings callback or physical device |
| Clock | Explicit Night choice, ordinary settlement/report and separate first-day/skip paths | Not exhaustive settlement/idempotency accounting |
| Combat | Rejected entry, Fight/Tactical buttons, one stamina charge, retained unresolved session, results exit/clock release | Not all missions or difficulty balance |

The nine view-state checks cover exact scroll across four same-frame notifications, replacement-control logical focus, preserved scroll after focusing, focus-follow retention, route-scroll cancellation, route-focus cancellation, hide/world ownership, hidden focus rejection and clean reopening.

## Rendered continuation

The separate opt-in suite starts through Main Menu and character creation, then declares synthetic Day-2 state. Mouse, key and standardized joypad events route through the viewport instead of emitted button signals. It exposed the disposed MainMenu subscription, undersized small-window UI and compact-world overlaps. It also retains explicit negative gating for an unbuilt Dairy Barn and positive hit-target coverage at the already-built Pasture.

The actual Scale Up/Down follow-up removed redundant panel scaling alongside central viewport scaling. At `c4ee73a`, #14 / `34546331041`, 189/195 checks passed; six Scale Down commands failed because the high-scale header consumed the content area. The header now considers short height and omits the decorative title in compact mode. All failed commands pass in the final suite, with twelve additional checks verifying each scale button is visible before input. Both original scale limits, captions, bounds and return to world are tested. See UI_LAYOUT_ACCEPTANCE for detailed failed baselines and fixture corrections.

## Remaining acceptance

Rendered coverage narrows the visual/input gap without certifying all pickers/actions, physical controller/touch behavior, Alt-Tab or a complete rendered first-day/combat/courier/leisure/save journey. The resource-backed first-day/leisure smoke walkthrough is distinct from synthetic interface fixtures and staged proximity.

Gameplay, economy, costs and progression remain with GameRoot and existing services. Authored obstacle-aware navigation/collision, representative Forward+ low/high performance, final art and long-term balance remain open. Prefer demonstrated playability fixes over additional mandatory daily systems.
