# NSFW Scope — Work Plan & Censorship Boundary

Updated 2026-09-07. This document separates what this agent **can** do (code, gates, non-explicit tooling) from what is **blocked** by content policy (explicit visual generation, explicit dialogue, age/design clearance that requires independent human review).

## Hard boundary (censorship)

The following are **blocked** and **not attempted**:

1. **Explicit visual generation** — no sexual imagery, explicit character art, or explicit body/clothing rendering via ComfyUI or any other pipeline. The ComfyUI MCP is available but will **not** be used to generate adult content.
2. **Explicit dialogue / mature scene text** — no writing of sexual scenes, explicit dialogue lines, or mature narrative content.
3. **Age / design clearance** — no agent can self-clear a character as `ConfirmedAdult`. The `AdultEligibilityGate` is fail-closed; only an **independent human design review** can set `AdultEligibility.ConfirmedAdult`. Numeric age, race, name, or supernatural chronology do not establish adult status.
4. **Minor / minor-coded / ambiguous characters** — Slay (apparent 13), Maria (apparent 15), Ayaka (JK marker) are permanently blocked from adult presentation. No redesign, no renumbering, no "fix" by metadata change.

## What IS allowed (code / safety / non-explicit)

1. **Fail-closed gate enforcement** — wiring `AdultEligibilityGate` into every adult-relevant dispatch path (mature action, portrait render, save/load, generation). This is safety engineering, not content generation.
2. **Negative tests** — asserting that minors, ambiguous, and Unknown characters are **denied** at every boundary. Proving the gate works.
3. **Non-explicit character identity** — neutral metadata (hair color, eye color, height, race, occupation) is fine. Explicit body description is not.
4. **Blender / ComfyUI for non-explicit work** — greybox meshes, environment models, toon-shader prototypes, UI assets. Adult-specific visuals remain blocked.
5. **Architecture / data model** — `CharacterVisualProfile`, `AdultEligibility` enum, `Provenance` fields, `IsDebugStandIn` flag. All gate-safe.
6. **Milk / bond / training mechanics** — the *gameplay* mechanics (resource production, relationship state, skill growth) are non-explicit simulation. The *explicit presentation* of those mechanics is blocked.

## Current state (evidence-based)

### Gate infrastructure (DONE — DATA-002)

- `AdultEligibilityGate` (`src/Gameplay/AdultEligibilityGate.cs`, 150 lines): `IsEligibleForAdult`, `CanPerformAdultAction`, `CanRenderAdultPortrait`, `ValidateAndSetEligibility`, `GetDenialReason`. Fail-closed: `Unknown`, `Minor`, `Ambiguous` → deny.
- 12 dedicated gate smoke assertions (SmokeTestRunner, ~line 2350): minor (13), minor (15), ambiguous (JK), unknown (21), ConfirmedAdult (grants), denial reasons.
- `DataRegistry.ValidateAndSetEligibility` at import (line 1432).
- `SaveMigrator.ValidateAndSetEligibility` at load (line 167).
- `MatureServices.PerformAction` gate (line 389).
- `CharacterAvatarFactory.CanUseRealAvatar` gate (CHAR-001): only `ConfirmedAdult` permits real avatar; ships stand-ins only.

### Blocked: no ConfirmedAdult character

`data/characters.json` — 10 definitions, **0** have `AdultEligibility` field (0 `ConfirmedAdult` hits). All characters are `Unknown` by default → gate denies all adult presentation. This is correct fail-closed behavior. **CHAR-002 is blocked** until an independent human design review clears one character (candidate: Noir, apparent 26, no contextual markers) AND the reviewer sets `ConfirmedAdult` in the data.

### Unblocked safety gaps (NOT blocked by censorship — fixable now)

| Gap | Location | Risk |
|-----|----------|------|
| `PortraitRenderer.BuildLayeredPortrait` (line 51) only **comments** the gate; does not call `CanRenderAdultPortrait` | `src/Gameplay/PortraitRenderer.cs:51` | Adult portrait render path is unenforced |
| `PortraitRenderer.BuildFallbackPortrait` (line 115) — no gate at all | `src/Gameplay/PortraitRenderer.cs:115` | Same |
| `CharacterGenerationPools.GenerateApparentAge` (lines 379-435) — weights 5/8/15 for ages 12/14/16 = **31.11% of rolls produce minor apparent age**; no gate in generation path | `src/Gameplay/CharacterGenerationPools.cs:420-435` | Generated characters can enter runtime state with minor apparent age |
| `SaveStateFactory` (lines 126-234) — generation path stores `ApparentAge` without eligibility validation | `src/Gameplay/SaveStateFactory.cs:289` | Same |
| `MatureServices.ProduceMilk` / `SetMilkQuality` (lines 512, 592) — no gate check before milk production on a character | `src/Gameplay/MatureServices.cs:512,592` | Milk action dispatches regardless of eligibility |
| `MatureServices.BondScenePlaceholder` (line 10) — returns placeholder text with no gate | `src/Gameplay/MatureServices.cs:10` | Bond scene entry unenforced |
| `CharacterState.ApparentAge` default 18, `PlayerState.ApparentAge` default 20 (`SaveModels.cs:138,242`) — defaults are not confirmed ages | `src/Core/Models/SaveModels.cs:138,242` | Legacy/missing metadata must not acquire approval through defaults |

### Blocked work (censorship)

| Work | Why blocked |
|------|-------------|
| Explicit character art (any character) | Explicit visual generation |
| Explicit dialogue / mature scenes | Explicit text generation |
| Setting `ConfirmedAdult` on any character | Requires independent human design review |
| Redesigning Slay / Maria / Ayaka for adult clearance | Minor / minor-coded / ambiguous — permanently blocked |
| Explicit body/clothing rendering in Blender | Adult-specific visual |
| Writing mature narrative content | Explicit text |

## Action items (ordered, unblocked first)

### 1. Enforce gate in PortraitRenderer (safety fix, no content)

- `BuildLayeredPortrait`: call `AdultEligibilityGate.CanRenderAdultPortrait(character, definition)` at the top; if denied, fall back to a non-explicit placeholder (grey capsule / stand-in), never to explicit layered art.
- `BuildFallbackPortrait`: same gate; if denied, return a neutral stand-in.
- Add smoke assertions: minor character → portrait render returns stand-in, not layered art.

### 2. Enforce gate in generation path (safety fix, no content)

- `CharacterGenerationPools.GenerateApparentAge`: after rolling, call `AdultEligibilityGate.ValidateAndSetEligibility` so the result carries the correct `AdultEligibility` state.
- `SaveStateFactory`: ensure the generation path runs the gate before storing.
- The 31% minor-apparent-age roll rate is **not a bug to fix** (the original game had it); the gate must **deny** those characters from adult presentation, not remove them from the pool.
- Add smoke assertions: generated character with apparent age 13 → `AdultEligibility.Minor`, `IsEligibleForAdult` → false.

### 3. Enforce gate in Milk / Bond dispatch (safety fix, no content)

- `MatureServices.ProduceMilk`: gate check at the top; deny for non-eligible characters.
- `MatureServices.SetMilkQuality`: same.
- `MatureServices.BondScenePlaceholder`: gate check; return neutral placeholder for non-eligible.
- Add smoke assertions.

### 4. Blender: non-explicit environment / greybox models (allowed)

- Blender via Steam: `D:\SteamLibrary\steamapps\common\Blender\blender.exe`
- Use for: environment greybox refinement, station models, terrain, toon-shader prototypes. **Not** for adult character models.
- ComfyUI: `D:\ComfyUI` — use for UI assets, icons, non-explict environment textures. **Not** for character art.

### 5. CHAR-002 (BLOCKED — requires human)

- One character (candidate: Noir, apparent 26, Human, black mage, silver hair, red eyes — no contextual markers in source) needs:
  1. Independent human design review (non-explicit context check).
  2. Reviewer sets `AdultEligibility: "ConfirmedAdult"` in `data/characters.json` for Noir.
  3. Then: ComfyUI can generate a **non-explicit** character portrait (upper-body, neutral pose, no explicit content) for the game UI.
  4. Blender: non-explicit 3D model (toon-shaded, neutral pose) for the world view.
- **Until step 2 is done by a human, CHAR-002 remains blocked.**

### 6. ART-002 / MORPH-001 / ANIM-001 (blocked until CHAR-002)

- Master .blend, GLB, material/rig/export validation, shared Godot toon prototype — all require a cleared character first.
- MORPH-001 (gameplay-to-visual curves, body/clothing shapes) — requires cleared character.
- ANIM-001 (rig + idle/walk/run/work/talk animation) — requires cleared character.

## Verification

Each safety fix (items 1-3) is verified by:
- Build: `dotnet build` — 0 errors, 0 warnings.
- Smoke: `python Tools/Godot/launch.py --mode smoke` — new negative assertions pass, existing 1231 assertions still pass.
- Gate assertions prove: minor → deny, ambiguous → deny, unknown → deny, ConfirmedAdult → grant.
