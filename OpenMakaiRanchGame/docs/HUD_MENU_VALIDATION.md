# HUD and menu integration validation

Continuation: **2026-09-11**, PR #12, `playability/ranch-quiet-corner-20260910`.
C# 12 remains enforced. No SDK, engine, target-framework or save-schema change.

## Executed result

Verified head **`9d8eb0967012b9c9c4d7d22fe16e96f1b379327c`**, **Godot 4.7 Mono CI #545**, run **`34538829758`**: **success**. Includes C#-12 checks/preview denial, primary-game build, all 34 launcher regressions, verified Godot 4.7.2 Mono import and isolated full smoke.

**1,691 passing assertions / zero failures / one terminal SMOKE PASS**. All **52 HUD/menu integration checks** and **nine additional view-state checks** pass, together with the existing suites and the additional ordinary-clock fixture guard. Documentation-only updates may follow; the PR records subsequent exact-head status.

Artifact **`10176503313`**, SHA-256 **`f17eeae0fa39320f61a97b0763a02203a6b4b1e0ea234d6c440f2e44fc390ef2`**. Smoke `.artifacts/godot/smoke-ohfy9cr3/console.log`; import `.artifacts/godot/import-tcn460wg/console.log`.

Smoke has zero out-of-tree transform errors and five expected malformed/unsupported-save diagnostics. Import succeeds with the separately tracked EditorSettings shutdown diagnostic. Neither diagnostic category was suppressed.

## Starting evidence and repairs

Code `a45a0bc8d4331ac122c1873947dd8a7b4067088b` compiled, but Godot CI #540 (run `34536254574`) failed: **1,678 OK / three failed assertions**. The failures were the two ordinary world settlement assertions and same-screen options scroll preservation. The earlier 1,629-check leisure result did not certify the later HUD changes.

`UiShellController.ViewState.cs` now composes the options input extension before restoring the view and waits for the next SceneTree.ProcessFrame instead of only CallDeferred. Nested layout changes can clamp a rebuilt scrollbar before final sizing. A pending snapshot preserves scroll/logical focus across multiple notifications during that interval. Restoration applies focus first, then the user's scroll; FollowFocus remains available afterward. Revision, screen, generation, visibility and node-validity checks reject retired callbacks. Snapshots hold values rather than discarded controls; callbacks unsubscribe on their first call.

The ordinary clock fixture explicitly establishes a completed-story Day-2 session before Morning -> Night -> planning -> settlement -> report. Previously it activated the mandatory fresh tutorial, then attempted ordinary settlement through that tutorial's input lock. All original assertions and runtime guards remain. Separate full first-day and skip-to-night walkthroughs still pass. This synthetic clock fixture is not described as earned progression or a new production skip fix.

## Related changes verified on this branch

The previous continuation supplied active Ranch/Town HUD ownership, canonical PlayerState HP, combat entry/exit clock ownership, hidden/inactive world-action rejection, help/Back coordination, Plan Night and retired binding-capture cancellation. These are now verified in the same complete suite as leisure, save/load and first-day behavior.

| Scope | Executed coverage | Boundary |
| --- | --- | --- |
| HUD / world | Active area, management visibility, hidden time/travel/management callbacks, help/Back and pause | Not a rendered HUD overlap/clipping review |
| Ordinary menus | Fifteen routes contain live content; browsing preserves gold, stamina and day; research gating remains | Not every command on every route |
| Player status | Immediate canonical HP with normal and empty rosters | Not every stat/bar value |
| Options | Exact scroll across repeated notifications, replacement-control focus, focus-follow, route/hide cancellation, reopening, retired binding capture | Not every settings callback or physical device |
| Clock | Explicit Night choice, ordinary settlement/report and separate complete first-day/skip paths | Not exhaustive economic settlement/idempotency proof |
| Combat | Rejected entry, actual Fight/Tactical Battle buttons, one stamina charge, retained unresolved session, disabled time controls, results exit and restored clock | Not all missions or difficulty balance |

`HudViewStateFrameTests` adds nine checks: exact scroll under four same-frame notifications, logical focus on the replacement binding control, preserved scroll after focus restoration, focus-follow retention, route-scroll cancellation, route-focus cancellation, hide/world ownership, hidden focus rejection and clean reopening.

## Remaining acceptance

Use the real Main Menu and character creation in an isolated rendered session. Play first day and shortcut, travel, open/close menus/help, finish tactical combat, plan Night, inspect the report, use board/corner/planning and save/load. Inspect narrow-window text, scrolling, visible hit targets, mouse/controller/touch focus and Alt-Tab. No headless signal invocation can certify those presentation results.

Gameplay/economy changes still go through GameRoot and existing services. UI tests use explicitly synthetic Day-2 state; the separate production walkthrough obtains actual resources. World interaction proximity is staged and buttons use live signals. Authored navigation, representative-hardware Forward+ performance, final art and long-term balance remain open.
