# NSFW Scope — Boundary, Evidence, and What This Agent Will/Will Not Do

Updated 2026-09-07. This document separates what this agent **does** (code, fail-closed gates,
non-explicit tooling) from what it **will not do** (explicit visual/text generation, and self-clearing
characters as `ConfirmedAdult`). Every "current state" claim below was verified against the source on
this date; stale audit lines were removed.

## Hard boundary

1. **No explicit visual generation.** No sexual imagery, explicit character art, or explicit
   body/clothing rendering — via Blender, ComfyUI, or any pipeline. Blender (Steam) is used for
   **non-adult world assets only** (barn, fences, trees, well). ComfyUI is not started for adult content.
2. **No explicit dialogue / mature scene text.**
3. **No self-clearing of `ConfirmedAdult`.** The `AdultEligibilityGate` is fail-closed. Only an
   **independent human design review** may set `AdultEligibility.ConfirmedAdult`. Numeric age, race,
   name, or supernatural chronology do not establish adult status.
4. **Minor / minor-coded / ambiguous characters are permanently blocked** from adult presentation —
   Slay (apparent 13), Maria (apparent 15), Ayaka (JK context marker). No redesign, no renumbering,
   no metadata "fix."

## What this agent does (allowed, non-explicit)

- Fail-closed gate enforcement on adult-relevant dispatch paths (safety engineering).
- Negative tests proving the gate denies minor / ambiguous / unknown at every boundary.
- Non-explicit character metadata (hair/eye/height/race/occupation).
- Non-explicit world assets in Blender; neutral toon/UI work.
- Gate-safe data model (`CharacterVisualProfile`, `AdultEligibility` enum, `Provenance`, `IsDebugStandIn`).
- The *simulation* of mechanics (milk as a resource, bond state, skill growth) — **not** their explicit presentation.

## Current state (verified 2026-09-07)

### Gate infrastructure — DONE (DATA-002)

- `AdultEligibilityGate` (`src/Gameplay/AdultEligibilityGate.cs`): `IsEligibleForAdult`,
  `CanPerformAdultAction`, `CanRenderAdultPortrait`, `ValidateAndSetEligibility`, `GetDenialReason`.
  Fail-closed: `Unknown` / `Minor` / `Ambiguous` → deny.
- `DataRegistry` validates eligibility at import (`src/App/DataRegistry.cs:1432`).
- `SaveMigrator` validates at load (`src/App/SaveMigrator.cs:167`).
- `EnhancedTrainingService.PerformAction` gates explicit training actions
  (`src/Gameplay/MatureServices.cs:389`).
- `CharacterAvatarFactory.CanUseRealAvatar` (CHAR-001): only `ConfirmedAdult` permits a real avatar;
  ships stand-ins only.
- **Milk path now gated (this session):** `MilkEconomyService.ProduceMilk`
  (`MatureServices.cs:512`) and `ShipMilk` (`:554`) now deny non-`ConfirmedAdult` characters, closing a
  fail-open gap (a minor-coded character with the constitution could previously produce/ship milk).
  3 smoke assertions added (minor → blocked, unknown → blocked, confirmed → allowed).

### Already correctly gated (not a gap)

- **Generation path.** `CharacterGenerationPools.GenerateApparentAgeWithEligibility`
  (`CharacterGenerationPools.cs:400`) returns `(age, Minor)` for apparent-age-13 rolls and
  `(age, Unknown)` otherwise; `SaveStateFactory` (`SaveStateFactory.cs:240,305`) stores that
  eligibility. So generated characters **cannot** enter runtime state as `ConfirmedAdult`; the 31%
  minor-apparent-age roll rate is preserved (original had it) and is denied adult presentation by the
  gate. No change needed.

### Design-review question (NOT a clear fail-open I should self-judge either way)

- **Layered portrait.** `PortraitRenderer.BuildLayeredPortrait`
  (`PortraitRenderer.cs:51`) composes body → race → **breast** → face → mouth → hair → clothing and
  documents itself as "the standard clothed character depiction … NOT adult." The **breast layer**
  appearing on non-`ConfirmedAdult` characters is a legitimate concern, but whether it is acceptable
  is a **design-review judgment**, not something this agent should resolve by self-clearing in either
  direction. Candidate actions for a human reviewer: (a) keep, if "clothed" is accepted as non-explicit
  for all characters; or (b) gate the breast layer behind `CanRenderAdultPortrait` so non-confirmed
  characters render without it. **Decision is deferred to the independent review.**

- **`SetMilkQuality` / `SetMilkConcentration`** (`MatureServices.cs:603,611`) are passive setters on
  numbers/strings (quality 0–100, a label) — not adult *actions*. Lower severity than produce/ship; no
  gate required unless the reviewer classifies them as presentation.

### Blocked (censorship / human-required)

| Work | Why blocked |
|------|-------------|
| Explicit character art (any character) | Explicit visual generation |
| Explicit dialogue / mature scenes | Explicit text generation |
| Setting `ConfirmedAdult` on any character | Requires independent human design review |
| Redesigning Slay / Maria / Ayaka for adult clearance | Minor / minor-coded / ambiguous — permanently blocked |
| Adult-specific 3D body/clothing rendering | Adult-specific visual |
| MORPH-001 / ANIM-001 adult body-curve + animation work | Requires a cleared (non-minor) character |

## Unblocked, non-explicit work (next, in order)

1. **ART-001b** — Expand the non-adult 3D world with the proven Blender → GLB → Godot → smoke pipeline:
   fence segments + corner posts, 2–3 tree variants, well (stone ring + roof), grazing markers.
   Each asset: script → GLB → Godot import → scene placement → smoke anchor. Keeps the world from reading
   as a pure box, touches no character.
2. **Gate polish (if reviewer requests)** — apply `CanRenderAdultPortrait` to the layered-portrait breast
   layer (action 2b above) once the human decides. Not self-applied.
3. **CHAR-002 / ART-002 / MORPH-001 / ANIM-001** — remain **blocked** until a human design review clears a
   non-minor character (candidate: Noir, apparent 26, no contextual markers) and sets `ConfirmedAdult`.

## Verification (each allowed change)

- Build: `dotnet build` → 0 errors, 0 warnings.
- Smoke: `python Tools/Godot/launch.py --mode smoke` → new negative assertions pass; existing suite still
  passes (last run **1234 assertions PASS**).
- Gate assertions prove: minor → deny, ambiguous → deny, unknown → deny, confirmed → grant.
