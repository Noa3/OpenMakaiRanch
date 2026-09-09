# World / Calendar / Forward+ checkpoint — 2026-09-09

Branch: `feat/world-hud-function-pass`

This checkpoint records the current implementation state. It is **not** a runtime-verification certificate; a real Godot 4.7 Mono editor/build/play pass is still required on the user's machine.

## First playable day

- Guided Day 1 remains resumable through `StoryProgressState.FirstDayStage`.
- A persistent `Skip First Day -> Night` control is available to experienced players before the night routine.
- Skip confirmation states that tutorial story/ranch-tour/intruder content is bypassed without tutorial rewards.
- The skip advances the shared `CalendarState.Phase` only to `Night`; it does not settle the day, increment the date, resolve work, or award combat resources.
- The player still chooses the ordinary night action and completes the normal `DailySettlementService` path.
- New Game+ bypasses the guided first-day flow.

## Original calendar evidence now implemented

The remake calendar rules are traced from the original sources named in `OriginalCalendarRules.cs`:

- `CSV/Day.csv`
- `CSV/Time.csv`
- `ERB/表示関数/日付表示.ERB`
- `ERB/内部計算・ステータス増減/天気変更.ERB`

Current shared calendar contract:

- 7 weekdays.
- 28 days per season.
- Spring, Summer, Autumn, Winter.
- 112 days per year.
- Day 1 = Spring 1 / Year 1 / Monday.
- Current weather + tomorrow forecast.
- Forecast rolls into current weather once per day.
- Season-dependent original weather tables include drizzle, heavy/torrential rain, strong wind, snow, heavy snow and blizzard.
- Season rollover is reported through the ordinary daily report.

The modern 3D world derives presentation from this calendar; it does not own a second clock.

## Renderer / quality policy

`project.godot` currently selects:

- Desktop: `forward_plus`.
- Mobile: `mobile`.
- Web: `gl_compatibility` (architectural target only; Godot 4 C# web export remains a separate product constraint).

Quality scaling currently combines render scale, shadow enablement, world-detail density, particle density and environment feature gates.

Forward+ advanced atmosphere may use:

- SSAO.
- SSIL on higher presets.
- situation-aware SSR (especially wet/snow/night scenes).
- volumetric fog for appropriate weather/night situations.
- glow, tonemapping and color adjustment.
- ordinary depth/height fog remains the cheaper fallback.

Low quality disables expensive optional atmosphere while retaining the shared gameplay weather and basic lighting. Mobile/Compatibility never assume Forward+-only effects.

## Living world presentation

`WorldAtmosphereController` follows the active player with bounded emitters rather than simulating precipitation over the whole map.

- Rain and snow scale by weather severity and quality.
- Spring: light blossom/pollen drift.
- Summer: sparse seed/dust motes.
- Autumn: falling/blowing leaves.
- Winter: subtle dry-weather frost motes; snow weather uses the precipitation emitter instead.
- Calm Spring/Summer evenings can show emissive night motes.
- Ground color/roughness reacts to season, wet weather and snow.

`WorldBoundaryBuilder` provides a permanent collision contract around authored world areas. Visual dressing (trees, rocks/fence clusters) scales with quality and changes color/shape by season. Travel gates remain the intentional exits; the player cannot simply walk off the authored ground.

## Camera

`WorldCameraRig` supports:

- collision ray clamp with clearance so the camera is pulled in front of walls instead of clipping through them;
- mouse/controller orbit;
- wheel/action zoom;
- explicit first-person toggle;
- very small near plane for close inspection;
- user FOV/sensitivity/invert-Y settings.

First person only hides the local player representation needed to avoid self-intersection. Other character/prop meshes are not intentionally culled when inspecting them closely.

## Turn-based combat world-time contract

`GameRoot.CombatWorldTimeLocked` prevents `AdvanceTime()` while a combat session is active. The ordinary combat UI now has a screen-exit guard in `WorldGameController.CombatLock.cs`: entering combat locks phase/day/weather progression, and leaving the combat screen ends the session and releases the lock. The first-day intruder encounter owns and closes its in-world combat session explicitly.

## Day 2+ planning feedback

`WorldAlertEvaluator` now also derives calendar/forecast planning notices:

- upcoming season change;
- last days of a season;
- blizzard forecast;
- severe rain/storm forecast;
- heavy snow/rain/strong-wind forecast;
- ordinary winter snow forecast.

These alerts are presentation-only and do not change job output or weather formulas.

## Remaining validation / next work

1. Run the actual Godot 4.7 Mono project and compile all new C# bindings.
2. Visually inspect Forward+ High/Ultra against Low/Medium on representative hardware.
3. Art-direct particle sizes/density, especially rain streaks and Autumn leaf motion.
4. Inspect close-camera behavior against final character meshes/hair/clothing once those assets replace placeholders.
5. Continue turning Personal Time into a source-traced modern presentation of original free-time actions without inventing duplicate reward paths.
6. Continue Day-2+ world activities, NPC schedules, authored interiors, town/ranch environmental detail and manual combat presentation.
7. Do not add GitHub Actions or a build pipeline for this project unless the user later explicitly requests one.
