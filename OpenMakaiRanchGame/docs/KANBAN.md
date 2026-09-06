# Kanban

Updated 2026-09-05. Status is evidence-based; DONE applies only to the named scope. Long-term 3D remake remains incomplete. See `3D_REMAKE_PLAN.md` for execution details and `KNOWN_ISSUES.md` for defect evidence.

## Done — baseline scope

- **AUDIT-001** Repository/docs/code inventory and composition map. Evidence: CURRENT_PROJECT_STATE and audit/simulation-map.
- **AUDIT-002** Main build and disposable-profile smoke; malformed scene headers repaired, navigation assertions corrected. Smoke: 868 assertions, PASS. Existing compile warning recorded separately.
- **TOOLS-001** Shared stable-4.7 .NET launcher; real editor/runtime MCP adapter, screenshot verification and safety regressions. Preserve loopback/development-only limits.
- **DATA-001** All 11 original Chara CSVs neutrally audited, mapping uncertainty and age risks documented. 136 field citations and 12 input hashes rechecked. This is not a full dialogue/image identity audit or runtime adult guard.
- **TOOLS-004** Character audit evidence verifier hardened: explicit checks survive Python optimization, validate citation identity/paths/lines, and distinguish original-source verification from stale code snapshots. Original source evidence rechecked; full snapshot remains stale after SAVE-001/CORE-002. This does not complete DATA-002. See WORK_LOG for fresh verification.
- **DATA-002** Fail-closed adult eligibility gates implemented in code (not assets, not numeric relabeling). `AdultEligibilityGate` enforces Unknown/Minor/Ambiguous denial at definition import, generation, mature-action dispatch, player mutation, player picker, and save-load. 12 dedicated gate smoke assertions added. Full isolated smoke **998 assertions PASS**. See WORK_LOG for scope and evidence.
- **CORE-001** Current state, additive 3D migration plan and resumable repository documents established.
- **SAVE-001** Flag roundtrips and explicit-null migration repaired. 81 root regression assertions; full isolated smoke 949 PASS. Invalid-load fixtures preserve live references and original file contents. No schema bump.
- **WORLD-001 (greybox slice)** Opt-in ranch greybox against the ART-001 floor plan. `src/World/` holds the deterministic, Node-free math + guard layer (`WorldMovementMath`, `WorldCameraMath`, `WorldInputGate`, `WorldInteractionGuard`) and the node layer (`ThirdPersonPlayerController`, `WorldCameraRig`, `WorldStation`, `RanchGreyboxController`, `WorldInputBootstrap`). The `IWorldCommandDispatcher` boundary routes every station interaction to `GameRoot` (no second reward calculator). `scenes/dev/RanchGreybox.tscn` is opt-in (not the boot scene) with ground/walls/interior/station/player/camera/prompt/management button. 40+ dedicated world smoke assertions cover diagonal-vs-straight speed, camera clamps, input ownership, double-activation, dispatcher dispatch, and the scene node contract. Full isolated smoke **1038 assertions PASS** on Godot 4.7.2 mono.
- **WORLD-002 (shared-simulation proof)** A `WorldStation` bound to the production `GameRootCommandDispatcher` (live `GameRoot.Instance`) mutates the **same** `ScheduleService` state the management UI reads, through the real `TryAssignJob` boundary. Exactly one settlement reward authority (the world path does not pay work, advance time, or create a second economy). Stale `StateGeneration` commands are rejected. 6 dedicated smoke assertions. Full isolated smoke **1044 assertions PASS** on Godot 4.7.2 mono.
- **CHAR-001 (gate-safe stand-in avatars)** `src/Character/` — `CharacterVisualProfile` (presentation-only Resource, stable `DefinitionId`, fail-closed `AdultEligibility` + `Provenance`, `IsDebugStandIn` flag, **no gameplay numbers**), `CharacterAvatarFactory` (pure Node-free, `CreateProfile` + fail-closed `CanUseRealAvatar` gate: Unknown/Minor/Ambiguous deny, only ConfirmedAdult permits — but CHAR-001 ships stand-ins only, the gate does not unlock real models yet), `CharacterAvatar3D` (honest debug stand-in: capsule body + sphere head, `Rebuild()` idempotent, `MaterialOverride` tint from profile). 19 dedicated smoke assertions (gate, profile binding, presentation/simulation separation, node geometry, rebuild idempotency). Full isolated smoke **1063 assertions PASS** on Godot 4.7.2 mono.
- **EVENT-001 (dialogue staging + safe panel coordinator)** `DialogueStaging` (pure presentation, state-free — no bond/morale/stockpile/completed fields, structurally cannot duplicate event state; deterministic framing) + `WorldPanelCoordinator` (open a management panel from the world, suspend world input, close and resume world input safely — gate never left UI-owned; single-panel rule + unknown-panel rejection). 26 dedicated smoke assertions. The command boundary (Mentorship + BondEvent, duplicate protection, exact rewards) was already proven by WORLD-002 + GameCommandTests. Full isolated smoke **1088 assertions PASS** on Godot 4.7.2 mono.
- **SAVE-002 (full 3D-world save/load round-trip)** `TestSaveLoadRoundTrip`: job assignment + mentorship (social action) + a full day transition through the 3D world path (station → production dispatcher → GameRoot) persist to the disposable smoke slot (99), survive a fresh start, and load back intact (world assignment, bond, morale, day counter, morning phase — no data loss). A stale (pre-load) `StateGeneration` is rejected via the world path; the disposable slot is cleaned up. 17 dedicated smoke assertions. Fresh starts expected (D-011); no old-save migration. Full isolated smoke **1105 assertions PASS** on Godot 4.7.2 mono.
- **DATA-003 (abgesagt)** No original-CSV import: the game is 100% self-contained — runtime reads only `res://data/*.json` + seed fallback; every CSV hit in `src/` is a comment, not a read. The curated `data/characters.json` (10 chars) + seed is the sole source. `Tools/EraDataImporter` stays `.csproj`-only.

## Next — priority order

Engine: use **Godot 4.7.2 mono** from `E:\GodotEditor\Godot_v4.7.2-stable_mono_win64.exe`. `launch.py` auto-discovers it (rejects the 198 KB `*_console.exe` stub by size, prefers the highest 4.7.x). No `GODOT_BIN` needed.

1. **WORLD-003** First complete playable day: management UI, inventory, NPC schedules, work/social/event, lighting and save/load — the greybox becomes the boot world.
2. **CHAR-002 / ART-002 (gate-blocked)** One real character (candidate: Noir) — master source/context review, non-explicit identity references, .blend/GLB + material/rig/export validation, shared Godot toon prototype. Gate-safe: only `ConfirmedAdult` after **independent design review**; no age renumbering. Blocked: `data/characters.json` has no `AdultEligibility` field, no ConfirmedAdult character, no design review yet. Needs CHAR-001 (done) + ComfyUI asset generation + design clearance.

## Ready after prerequisites

- **CHAR-002** One master character source/context review and non-explicit identity references. Candidate only: Noir; no age/design clearance yet. Needs CHAR-001 (done) + independent design review (ConfirmedAdult) + ComfyUI asset generation.
- **ART-002** Master character .blend, GLB, material/rig/export validation; shared Godot toon prototype. Needs selected references; no mass conversion.
- **MORPH-001** Gameplay-to-visual curves, matching body/clothing shapes, extreme-combination lab. No mesh internals in saves.
- **ANIM-001** Shared rig and idle/walk/run/work/talk animation validation at gameplay camera.
- **AI-001** Derive logical NPC locations from existing assignments/phases; nearby navigation, distant logical simulation, bounded stuck recovery. No second work economy.
- **WORLD-003** First complete playable day with management UI, inventory, NPC schedules, work/social/event, lighting and save/load.

## Backlog / isolated follow-ups

- **CORE-003** Night-training growth multiplication: exact reproduction and original-rule comparison.
- **CORE-004** Settlement idempotency, effective-job semantics and report ledger tests.
- **TOOLS-002** Raw MCP request limits/authentication/development-only export policy; do not equate adapter policy with endpoint security.
- **TOOLS-003** Isolate engine editor-shutdown warning; no engine upgrade mixed into game work.
- **UI-001** Full management visual/action playthrough and narrow-viewport coverage.
- **BUG-001 (fixed)** CS8604 in UiShellController.Screens.cs:3598 (nullable Path.Combine path1) resolved with `?? Directory.GetCurrentDirectory()` fallback; clean `--no-incremental` build now 0 warnings / 0 errors.
- **DATA-004** Replace placeholder ContentValidator with actual bounded JSON/reference validation.
- **PARITY-001** Original formula/fixture comparison matrix; no blanket parity claims.
- **PERF-001** Measure representative 3D CPU/GPU frame time, draw calls, triangles, animation/navigation and memory. Then choose LOD budgets.

## Deferred

Weather, mass character conversion, expensive secondary motion, portrait rendering, broad UI redesign and explicit presentation are not blockers for the first non-explicit 3D loop. Engine maintenance upgrades require a separate verified baseline checkpoint and task. No 4.8 development upgrade.
