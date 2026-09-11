# Rendered UI and small-window acceptance

Continuation 2026-09-11, PR #12, branch `playability/ranch-quiet-corner-20260910`. The new compact-world changes are under verification. Do not treat the older 1,691-check smoke checkpoint as rendered acceptance of subsequent work.

## Reproduced defects

The initial real MainMenu -> New Game -> character-creation typing route (`dca7da1`, rendered run `34542832663`, artifact `10177933223`) exposed a disposed MainMenu callback retained by GameRoot.StateChanged. `a834944` detaches it on scene exit. The subsequent rendered run `34543227958`, artifact `10178079986`, passed that route without the exception.

That second run exposed nine physical-size failures for Return to World, time advance and the interaction binding at 960x540, 640x480 and 480x800. The former 1920-wide virtual UI shrank controls below the physical-size threshold even though their logical rectangles stayed inside the viewport. Main-menu and character-creation minimum widths, header wrapping and wrapped-label heights also required adjustment once the canvas respected small-window dimensions.

The adaptive canvas preserves actual pixels below the desktop baseline, proportional scaling above it, expanded ultrawide space and the separate UI-scale preference. Binding captions now clip within stable button widths, preventing the capture instruction from unexpectedly reflowing its row. Escape and controller Back cancel either capture device even during the arming delay; retired/reset cards cannot start a capture.

## Fixture corrections, not production fixes

The initial 1280-labelled startup image actually used the saved default window size after scene startup. The current suite explicitly resizes and asserts the physical window size before named captures.

At `aab2cf1`, 86 of 91 rendered checks passed; five narrow-options checks failed. Focus geometry from `cb49980` showed that the fixture called both GrabFocus (which invokes FollowFocus) and EnsureControlVisible in the same frame. The latter saw stale child coordinates and overscrolled the already-scrolled view. The explicit second scroll has been removed; the same exact visibility/click/cancellation assertions remain and now exercise ordinary FollowFocus. The unnecessary root-window NotifyMouseEntered call was also removed rather than ignoring its warnings.

## Compact world coverage

The readable canvas exposed previously undersized world hints overlapping at small window sizes. Shared Ranch/Town layout now separates summary/action rows, worker and warning panels, and compresses optional tutorial detail without changing its completion state. Full tutorial/resource/warning detail remains in the bounded, scrollable Help panel. The interaction affordance wraps its existing action button and suppresses the legacy duplicate prompt after the host has actually bound its areas.

The added rendered checks stage proximity to an existing station, inspect real button bounds, open Help through viewport mouse events, verify full warning text, focus-scroll to Close, return input to the world, and repeat in town. Staging is not proof of traversable routes. No rewards or gameplay commands are implemented in these presentation helpers.

## Running the suite

Use `python Tools/Godot/ui_acceptance.py --rendered` after normal build/import. Windows/Linux disposable profile directories are created under `.artifacts/godot/ui-*`. A separate engine preflight must verify the user-data path before the scene performs any actions. Unsupported profile platforms fail closed. The opt-in scene rejects normal startup, missing debug flags and unverified paths.

`results.json`, runtime logs and viewport PNGs are the evidence. The runner requires an actual renderer in rendered mode, a nonempty all-passing assertion list, exactly one terminal success marker and the stated PNG files. A logged ERROR or SCRIPT ERROR fails this UI-only scenario even when Godot swallows a C# exception inside a button signal. The ordinary smoke suite's intentional invalid-save diagnostics are separate.

The dedicated read-only GitHub workflow uses hash-pinned Godot 4.7.2 Mono, Mesa OpenGL Compatibility rendering and Xvfb. It stores the exact tested source/commit with the evidence for 14 days. No personal saves are uploaded. Fourteen Python regressions cover profile isolation and evidence rejection; they join the existing 34 launcher checks.

## Limits

Synthetic mouse/keyboard/standardized joypad events are routed through the engine, not emitted button signals, but are still not physical device testing. The ordinary Day-2 menu fixture is explicitly synthetic; it does not replace the resource-backed first-day/leisure smoke walkthrough. Tested pixel thresholds do not certify ergonomic touch targets. Compatibility screenshots are not Forward+ visual/performance acceptance, full controller hardware coverage, authored navigation, all menu actions, final assets or long-term balance.

C# 12, SDK 10.0.401, net8.0, Godot 4.7.2 Mono, schema 16, existing simulation authorities, original read-only source and eligibility rules are unchanged. See ASTRA_HANDOFF and the live PR for the exact final executed checkpoint.
