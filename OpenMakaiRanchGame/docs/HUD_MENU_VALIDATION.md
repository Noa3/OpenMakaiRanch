# HUD and menu integration validation

Updated 2026-09-11, PR #12, `playability/ranch-quiet-corner-20260910`. This page records the retained headless integration contracts. **Current rendered scope, screenshots and exact checkpoint evidence are in `UI_LAYOUT_ACCEPTANCE.md` and `ASTRA_HANDOFF.md`.** C# 12, SDK, engine, target framework and save schema are unchanged.

## Historical integration checkpoint, retained by the current suite

Head `9d8eb0967012b9c9c4d7d22fe16e96f1b379327c`, Godot 4.7 Mono CI #545 / run `34538829758`, passed **1,691 assertions**, including **52 HUD/menu integration checks**, **nine view-state checks** and the ordinary-clock fixture guard. It also ran the then-current 34 launcher regressions. Artifact `10176503313`, SHA-256 `f17eeae0fa39320f61a97b0763a02203a6b4b1e0ea234d6c440f2e44fc390ef2`, records that historical proof, not automatic certification of later changes. Current launcher coverage has 48 tests.

The smoke log had zero out-of-tree transform errors and five deliberately malformed/unsupported-save diagnostics. Import succeeded with the separately tracked EditorSettings shutdown diagnostic. Neither category was suppressed.

## Retained repairs and regression coverage

The pre-fix `a45a0bc` / CI #540 (`34536254574`) compiled but had 1,678 passing assertions and three failures: ordinary settlement twice and same-screen options scroll preservation. The three original assertions remain.

`UiShellController.ViewState` composes the input extension before a ProcessFrame-boundary restore, preserves pending scroll/logical focus across notification bursts, restores focus before scroll and retains FollowFocus. Revision, screen, generation, visibility and instance guards reject retired callbacks. Snapshots hold values, not old controls; frame callbacks are one-shot.

The ordinary clock subsection explicitly establishes a completed-story Day-2 fixture. It previously activated the fresh mandatory tutorial and then attempted ordinary settlement through that tutorial's input lock. Runtime guards, explicit night choice, original fresh-story assertions and separate full-first-day/skip walkthroughs remain. This correction is not a production skip fix or organically earned progression.

| Scope | Retained integration coverage | Boundary |
| --- | --- | --- |
| HUD/world | Active area, management visibility, hidden time/travel/management callbacks, help/Back, pause | The original headless checks alone do not prove visible layout |
| Menus | Fifteen routes contain live content; browsing preserves gold, stamina and day; research gating | Not every command on every route |
| Player status | Immediate canonical HP with normal and empty rosters | Not every stat/bar value |
| Options | Exact burst-refresh scroll, replacement-control focus, focus-follow, route/hide cancellation, reopening, retired capture | Not every settings callback or physical device |
| Clock | Explicit Night choice, ordinary settlement/report and separate first-day/skip paths | Not exhaustive settlement/idempotency accounting |
| Combat | Rejected entry, Fight/Tactical buttons, one stamina charge, retained unresolved session, results exit/clock release | Not all missions or difficulty balance |

The nine view-state checks cover exact scroll across four same-frame notifications, replacement-control logical focus, preserved scroll after focusing, focus-follow retention, route-scroll cancellation, route-focus cancellation, hide/world ownership, hidden focus rejection and clean reopening.

## Rendered continuation and latest reproduced issue

The separate opt-in acceptance suite starts through the actual Main Menu and character creation, then declares a synthetic Day-2 fixture for menu testing. It routes mouse, key and standardized joypad events through the viewport instead of emitting button signals. At `9f2a768`, **166 checks and 29 viewport PNGs** passed. That includes small-window layouts, typing/focus, binding cancellation, compact navigation, Ranch/Town help, full warning detail and existing facility gating. See the rendered evidence page for run/artifact IDs and the distinction between production repairs and fixture corrections.

The actual Scale Up/Down follow-up removed a redundant RootPanel scale alongside the central viewport scale. Its first rendered run (`c4ee73a`, #14 / `34546331041`) passed 189 of 195 checks but exposed six failed Scale Down clicks: at high scale the header consumed nearly all remaining logical height. Root panel bounds alone had not detected the unusable content area. The next correction suppresses the decorative title in compact mode and treats short logical height as a dense-layout condition. All six failing commands remain, with twelve added checks requiring each scale button to be fully visible before clicking. Do not label that follow-up successful before its exact-head evidence is recorded.

## Remaining acceptance

The rendered continuation narrows the earlier visual/input gap; it does not certify every screen/action, physical controller/touch behavior, Alt-Tab on target hardware or a complete rendered first-day/combat/courier/leisure/save journey. The resource-backed first-day/leisure smoke walkthrough remains separate from synthetic interface fixtures and staged world proximity.

Original gameplay, economy, costs and progression stay with GameRoot and existing services. Authored obstacle-aware navigation/collision, representative Forward+ low/high performance, final art and long-term balance remain open. Prefer demonstrated playability fixes over additional mandatory daily systems.
