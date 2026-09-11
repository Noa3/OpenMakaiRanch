# Presence and Makai prototype: validation status

Branch: `feature/makai-presence-and-worldstyle-20260911`. Fixed main base: `1e7f90936849443f1605e573a37d3e43e0676245`.

## Status before first CI run

The existing lookdev Python staging/evidence suite was executed locally: **15 tests passed**. `git diff --check` passed. No local Godot or .NET runtime was available, so C# compilation and visual validation are **not yet claimed** here. The first attempt at the broader Python suite timed out; it is not counted as a pass.

Use the associated PR's exact-commit check results and subsequent validation receipt for executed C# and renderer results. The existing `Anime lookdev rendered acceptance` workflow is reused; no new build pipeline or third-party dependency is introduced.

## Scope

The existing 16 material captures are retained. Five required captures are added: `presence-neutral`, `presence-blink`, `presence-warm`, `makai-off`, `makai-night`. Contract checks cover deterministic preferences/motion, availability-gated advice, explicit invitation state, stale actor/session/revision rejection, pause/visibility, bounded timing, rig ownership/restore, reimport/external writer revocation, and phenomenon budgets/shelter/reduced motion. Actual rendered pixel changes are required for blink/expression and veil toggles.

The rendered host is autoload-free and uses disposable profiles, not production save slots. Both real Forward+ and Compatibility backends must be inspected; dummy/headless rendering is not visual evidence. CI uses software drivers: results cannot establish player-hardware FPS or finished art quality. Existing full-game NuGet warnings may remain; diagnostics are not suppressed to obtain a pass.

## Boundaries

No original-game source, save schema, simulation/economy, production navigation, NPC schedules or player controls change. The demonstration body and face remain schematic. Microgestures use original calibration targets; no final rigged character, voice/phoneme alignment, hand gesture library or actual autonomous ranch execution is delivered. The phenomenon is an optional lab node, not a global sky/weather replacement. See `CHARACTER_PRESENCE_AND_AUTONOMY.md` for the logic-agent handoff and `MAKAI_WORLD_STYLE_AND_LORE.md` for the primary-source research boundaries.
