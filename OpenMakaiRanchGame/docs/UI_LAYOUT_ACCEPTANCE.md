# Rendered UI and small-window acceptance

Verified **2026-09-11**, PR #12, code-inclusive head **`897871eadbdf02c19d905c6e0b33e5edf335e649`**: **207 passed assertions, zero failures, 31 viewport PNGs**. Rendered UI **#17 / `34546801281`** succeeded alongside Godot **#563 / `34546801277`** and Build **#571 / `34546801316`**. The full smoke suite independently passed 1,691 assertions. ASTRA_HANDOFF records the companion smoke artifact and the live PR records later documentation-inclusive heads.

## Evidence

Rendered artifact **`10179361232`**, SHA-256 **`671af1cfafa35895ef8fcf634170af9de5db59924706fcea7c57c43ebae2f92a`**. Downloaded archive/hash, results and screenshots independently inspected. UI logs/results/screens: `godot/ui-ly84_15e/` inside the artifact. Its exact checked-out commit/source archive is included separately. There is one terminal UI ACCEPTANCE PASS, no runtime ERROR/SCRIPT ERROR, and the retained unsupported-VSync software-driver warning. This is not a warning-free renderer claim.

The last intermediate compact-world checkpoint `9f2a768` passed 166 assertions/29 PNGs in #11 / `34545898746`, artifact `10179027332`, SHA-256 `54b63c897f9311254959edf672fca7e4d0a89714733229740ea0a2a1b93785fd`. The subsequent scale-control checks are included in the final 207, not inferred from that earlier pass.

## Reproduced production defects and repairs

**Main-menu lifecycle.** Initial real MainMenu -> New Game -> character-creation typing (`dca7da1`, run `34542832663`, artifact `10177933223`) exposed an ObjectDisposedException because the removed MainMenu still observed GameRoot.StateChanged. `a834944` detaches it on scene exit. Subsequent rendered runs complete that route without the exception.

**Readable canvas and reflow.** The next baseline (`34543227958`, artifact `10178079986`) had nine physical-size failures: Return to World, phase advance and the interaction binding at 960x540, 640x480 and 480x800. The former 1920-wide virtual UI shrank controls even though logical bounds remained inside the viewport. The adaptive canvas preserves actual pixels below the authored desktop baseline, proportional scaling above it, expanded ultrawide space and the independent user scale. Main-menu/creation widths, creation grids, flow-based header rows and wrapped-label natural heights adapt to the available space.

**Capture controls.** Escape and controller Back cancel either capture device even during the arming delay. Reset/retired cards cannot re-arm. Binding captions clip inside stable button widths instead of reflowing their row when capture instructions appear.

**Compact world.** The readable canvas exposed undersized Ranch/Town hints overlapping in small windows. Summary/actions, worker and warning panels are separated. Optional tutorial detail is compressed without changing completion state; full tutorial/resource/alert detail remains in bounded scrollable Help. The existing interaction action wraps, and duplicate legacy prompts are hidden after the host actually binds its areas. No rewards or progression logic was moved into presentation helpers.

**Scale buttons and short-height headers.** Source review found Options Scale Up/Down calling central SetUiScale and also multiplying RootPanel.Scale by the pre-clamp request. The additional transform/notification/rerender was removed; the existing 0.85–1.35 authority remains. The first actual-button follow-up (`c4ee73a`, #14 / `34546331041`) passed 189/195 checks but failed six Scale Down clicks: the header consumed nearly all available logical height at high scale. Dense layout now also considers short height and suppresses the decorative title in compact mode. All six failed commands remain and twelve additional checks require fully visible buttons before input. Final screenshots show both scale controls reachable at the upper limit.

## Fixture corrections, not production fixes

The earliest startup screenshot name assumed 1280x720, but startup had applied a saved default size. Named captures now explicitly resize and assert physical dimensions.

At `aab2cf1`, 86/91 rendered checks passed. Focus geometry from `cb49980` showed the fixture calling GrabFocus (which invokes FollowFocus) and then EnsureControlVisible in the same frame. The latter saw stale coordinates and overscrolled the view. The redundant second scroll was removed; the original exact visibility/click/cancellation assertions now pass with ordinary FollowFocus. The redundant root-window NotifyMouseEntered call was removed rather than ignoring its warnings.

The first world fixture mistakenly selected the unbuilt Dairy Barn as its positive Interact-button case. The missing button was correct gating. The final test explicitly checks that lock/explanation, then uses the already-built Pasture for the unchanged positive hit-target assertion. No facility unlock or gameplay-rule relaxation was introduced.

## Executed scope

Main-menu focus and actual New Game hit test; name typing/Tab; creation form reflow and focus-reachable Name/Start controls; options at 1280x720, 960x540, 640x480, 480x800 and 1920x720; logical/physical bounds; binding cancellation; compact navigation through standardized joypad events; pause/Back; direct runtime scale stress at 0.8/1.5; staged station presentation; Ranch/Town Help opening, full alert detail, focus-scrolled Close and returned input; twelve real Scale Up/Down clicks through both ordinary 0.85–1.35 limits, single scaling, matching captions, visible targets and return to world. The broader runtime stress values are not the ordinary control range. 4K/ultrawide pure canvas checks are not hardware/display certification.

The scenario starts through the real Main Menu, then explicitly constructs a completed-story Day-2 interface fixture. It verifies no gold/stamina/day mutation during layout inspection. This is not organically earned progression, a walkable-route test or a full rendered first-day journey. The separate resource-backed first-day/leisure smoke walkthrough remains intact.

## Running and evidence rejection

After normal build/import, run `python Tools/Godot/ui_acceptance.py --rendered`. The Windows/Linux launcher creates a disposable profile under `.artifacts/godot/ui-*`; a separate engine preflight must verify the user-data directory before UI actions start. Unsupported platforms fail closed. The opt-in debug scene requires its flags and verified paths.

The runner requires a real renderer when requested, a nonempty all-passing assertion list, one exact terminal success marker and the stated PNG files. A logged ERROR or SCRIPT ERROR fails this UI-only scenario even if Godot swallowed a signal callback exception. Full-smoke invalid-save fixtures are separate. Fourteen Python regressions cover isolation and evidence rejection, joining 34 earlier tests: 48 passed, including the local rerun.

The read-only workflow uses SHA-pinned Godot 4.7.2 Mono, Mesa OpenGL Compatibility and Xvfb. It uploads whitelisted logs/results/screenshots/exact source, not personal saves, with 14-day retention. Temporary branch-scoped source patch helpers removed themselves after hash-checked non-force commits. No automatic merge was used.

## Limits

Synthetic mouse/keyboard/joypad events pass through the engine rather than emitted button signals, but are not physical-controller/touch/Alt-Tab acceptance. Not every creation picker or menu action is exercised. Pixel thresholds do not certify touch ergonomics or accessibility compliance. Compatibility images are not Forward+ visual/performance acceptance. Authored navigation/collision, complete rendered gameplay journeys, final art/animation, weather readability, representative hardware and long-term balance remain open.

C# 12, SDK 10.0.401, net8.0, Godot 4.7.2 Mono, schema 16, shared simulation authorities, original read-only source and eligibility rules are unchanged.
