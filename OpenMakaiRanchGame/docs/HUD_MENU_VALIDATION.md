# HUD and menu integration validation

Continuation: 2026-09-11, PR #12, `playability/ranch-quiet-corner-20260910`.
C# 12 remains enforced. No SDK, engine, target-framework or save-schema change.

## Starting evidence

Code `a45a0bc8d4331ac122c1873947dd8a7b4067088b` compiled, but Godot CI #540
(run `34536254574`) failed: **1,678 SMOKE OK and three failed assertions**.
The failures were the two ordinary world settlement assertions and same-screen options scroll
preservation. The earlier 1,629-check leisure result is not evidence that these later changes pass.

## Repairs in this continuation

`UiShellController.ViewState.cs` now composes the options input extension before restoring the
view and waits for the next SceneTree.ProcessFrame rather than only CallDeferred. Nested layout
changes can clamp the scrollbar while rebuilt content has not acquired its final size. A pending
snapshot preserves scroll and logical focus across several state notifications in that interval.
Restoration applies focus first, then the user's scroll position; normal FollowFocus remains
available for subsequent keyboard/controller navigation. Retired callbacks are rejected by view
revision, current screen, state generation, visibility and node validity. Snapshots retain only
values, not references to discarded controls; each frame callback unsubscribes on its first call.

The ordinary clock fixture is now explicitly a completed-story Day-2 presentation fixture before
it exercises Morning -> Night -> planning -> settlement -> report. Previously it first activated
the mandatory fresh tutorial and then tried to run an ordinary day through the tutorial's input
ownership. No runtime guard, night-choice requirement or assertion is weakened. The original
fresh-story assertions remain, alongside the separately existing full first-day and skip-to-night
walkthroughs. The synthetic clock fixture is not described as organically earned progression.

## Related fixes already present on this branch

The prior continuation supplied explicit Ranch/Town HUD ownership, canonical PlayerState HP,
combat entry/exit clock ownership, hidden world-action rejection, help/Back coordination,
Plan Night, and cancellation of retired binding capture. They need to pass the same complete
suite as the leisure, save/load and first-day tests, not merely compile in isolation.

## Regression coverage

`HudMenuFrameTests` covers live HUD switching and hidden callbacks; help, pause and management
handoff; ordinary screen content; research access gating; empty-roster PlayerState HP; options
focus, scroll and binding capture; mandatory night selection; and tactical combat entry, one-time
stamina charging, unresolved-session retention, results exit and restored world-clock control.

`HudViewStateFrameTests` adds exact scroll preservation under four same-frame notifications,
logical focus restoration onto a replacement control, focus-follow retention, cancellation when
routing away or hiding the menu, and reopening without a retired options snapshot.

## Verification status

The fixes and additional regressions are committed. Full post-fix CI results must be read from
the exact head and recorded in PR #12 / ASTRA_HANDOFF before this work is called verified.
Do not treat this paragraph or the presence of a test method as a passing test result.

## Limits and remaining acceptance

All gameplay/economy changes still go through the existing services and GameRoot. The UI fixtures
use synthetic Day-2 state and input events; the separate production walkthrough follows actual
first-day production. Do not conflate the two. Button signal invocation is not a rendered click
or physical-device test. Rendered mouse/gamepad/touch acceptance, narrow-window hit targets,
text clipping, accessible navigation, and Forward+ performance remain open. Loading unsupported
or malformed saves intentionally emits five rejection diagnostics; do not suppress them. The
separate engine/editor shutdown diagnostic remains tracked as TOOLS-003.
