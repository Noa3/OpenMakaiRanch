# Rendered UI and small-window acceptance

Continuation 2026-09-11, PR #12, branch `playability/ranch-quiet-corner-20260910`. The compact-world and menu checkpoint `9f2a7686f28b1046fa037bcf91b262f580d11ce8` passed **166 rendered checks with 29 viewport PNGs**. The subsequent scale-button correction is under verification; the older 1,691-check smoke checkpoint alone does not certify later work.

## Reproduced defects

The initial real MainMenu -> New Game -> character-creation typing route (`dca7da1`, rendered run `34542832663`, artifact `10177933223`) exposed a disposed MainMenu callback retained by GameRoot.StateChanged. `a834944` detaches it on scene exit. The subsequent rendered run `34543227958`, artifact `10178079986`, passed that route without the exception.

That second run exposed nine physical-size failures for Return to World, time advance and the interaction binding at 960x540, 640x480 and 480x800. The former 1920-wide virtual UI shrank controls below the physical-size threshold even though their logical rectangles stayed inside the viewport. Main-menu and character-creation minimum widths, header wrapping and wrapped-label heights also required adjustment once the canvas respected small-window dimensions.

The adaptive canvas preserves actual pixels below the desktop baseline, proportional scaling above it, expanded ultrawide space and the separate UI-scale preference. Binding captions clip within stable button widths, preventing the capture instruction from unexpectedly reflowing its row. Escape and controller Back cancel either capture device even during the arming delay; retired/reset cards cannot start a capture.

A further source review found that Options Scale Up/Down called the central SetUiScale but also multiplied RootPanel.Scale by their pre-clamp request. This applied scaling twice and could disagree with the canonical 0.85–1.35 range. The follow-up routes those buttons only through SetUiScale, removing the redundant transform, notification and rerender. New engine-routed click checks exercise repeated increase/decrease, both existing limits, displayed percentages, viewport bounds and return to the world. The broader 0.8/1.5 direct-runtime fixtures remain explicitly stress tests, not a claim that the ordinary controls allow that range.

## Fixture corrections, not production fixes

The initial 1280-labelled startup image actually used the saved default window size after scene startup. The current suite explicitly resizes and asserts the physical window size before named captures.

At `aab2cf1`, 86 of 91 rendered checks passed; five narrow-options checks failed. Focus geometry from `cb49980` showed that the fixture called both GrabFocus (which invokes FollowFocus) and EnsureControlVisible in the same frame. The latter saw stale child coordinates and overscrolled the already-scrolled view. The explicit second scroll was removed; the same exact visibility/click/cancellation assertions now pass using ordinary FollowFocus. The unnecessary root-window NotifyMouseEntered call was removed rather than ignoring its warnings.

The initial world fixture selected the authored primary Station, which is the unbuilt Dairy Barn. Its missing Interact button was correct production gating, not a clipping defect. The final fixture explicitly checks the locked dairy explanation/button denial and then uses the already-built Pasture for the unchanged positive hit-target assertion. No facility was unlocked or gameplay rule weakened to satisfy the test.

## Compact world coverage

The readable canvas exposed previously undersized world hints overlapping at small window sizes. Shared Ranch/Town layout separates summary/action rows, worker and warning panels, and compresses optional tutorial detail without changing completion state. Full tutorial/resource/warning detail remains in the bounded, scrollable Help panel. The interaction affordance wraps its existing action button and suppresses the legacy duplicate prompt after the host actually binds its areas.

Rendered checks stage proximity to existing stations, inspect real button bounds, open Help through viewport mouse events, verify full warning text, focus-scroll to Close, return input to the world, and repeat in town. Staging is not proof of traversable routes. Presentation helpers implement no rewards or gameplay commands.

## Executed compact-world checkpoint

Head **`9f2a7686f28b1046fa037bcf91b262f580d11ce8`**, rendered run **`34545898746`** / #11: **166 passed, zero failed assertions, 29 PNG captures**, one terminal UI ACCEPTANCE PASS. Artifact **`10179027332`**, SHA-256 **`54b63c897f9311254959edf672fca7e4d0a89714733229740ea0a2a1b93785fd`**, independently downloaded and inspected. Logs/results are under `godot/ui-auk23stm/` inside that artifact. The subsequent scale-button slice requires its own exact-head evidence.

## Running the suite

Use `python Tools/Godot/ui_acceptance.py --rendered` after normal build/import. Windows/Linux disposable profiles are created under `.artifacts/godot/ui-*`. An engine preflight must verify the user-data path before the scene performs any actions. Unsupported profile platforms fail closed. The opt-in scene rejects normal startup, missing debug flags and unverified paths.

The runner requires an actual renderer in rendered mode, a nonempty all-passing assertion list, exactly one terminal success marker and the stated PNG files. A logged ERROR or SCRIPT ERROR fails this UI-only scenario even when Godot swallows a C# exception inside a button signal. The ordinary smoke suite's intentional invalid-save diagnostics are separate. Mesa reports unsupported VSync changes; this renderer warning is retained, not presented as a gameplay exception.

The dedicated read-only workflow uses hash-pinned Godot 4.7.2 Mono, Mesa OpenGL Compatibility and Xvfb. It stores exact tested source/commit, logs, results and screenshots for 14 days. No personal saves are uploaded. Fourteen Python regressions cover profile isolation and evidence rejection; together with the existing 34 launcher tests, **48 passed locally**.

## Limits

Synthetic mouse/keyboard/standardized joypad events route through the engine rather than emitted button signals, but are not physical-device tests. The Day-2 menu fixture is synthetic and does not replace the resource-backed first-day/leisure smoke walkthrough. Tested pixel thresholds do not certify ergonomic touch targets. Compatibility screenshots are not Forward+ visual/performance acceptance, full controller hardware coverage, authored navigation, all menu actions, final assets or long-term balance.

C# 12, SDK 10.0.401, net8.0, Godot 4.7.2 Mono, schema 16, existing simulation authorities, original read-only source and eligibility rules are unchanged. See ASTRA_HANDOFF and the live PR for the final executed checkpoint.
