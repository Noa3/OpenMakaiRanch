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

## Next — priority order

Engine: use **Godot 4.7.2 mono** from `E:\GodotEditor\Godot_v4.7.2-stable_mono_win64.exe` (full, no-console build; the 198 KB `*_console.exe` is a broken stub that crashes Mono with "Assemblies not found"). The repo-root 4.7.0 binaries are not valid for Mono headless runs. Pass the working exe via `GODOT_BIN`.
1. **DATA-003** Recover importer from history or implement a bounded read-only importer with fixtures and provenance, writing only to a test output directory first. Current importer project has no source files (csproj only); do not overwrite existing JSON.
2. **CHAR-001** Avatar/visual-profile stable-ID adapter and useful CharacterLab on the WORLD-001 greybox (reuses the third-person controller + camera). Missing assets use explicit debug stand-ins.
3. **WORLD-002** Bind the greybox station to a real `GameRoot.TryAssignJob` target so a world interaction dispatches an existing job assignment; exactly one settlement reward authority.
4. **EVENT-001** One social interaction/event with camera/dialogue presentation and existing service completion rules.
5. **WORLD-003** First complete playable day: management UI, inventory, NPC schedules, work/social/event, lighting and save/load — the greybox becomes the boot world.

## Ready after prerequisites

- **WORLD-001** Opt-in ranch greybox, third-person movement/camera/collision/input ownership. Needs CORE-002 and floor plan. Acceptance: keyboard/gamepad, walls/camera, focus/UI transitions, actual screenshot/playtest.
- **CHAR-001** Avatar/visual-profile stable-ID adapter and useful CharacterLab. Needs rebinding tests. Missing assets use explicit debug stand-ins.
- **CHAR-002** One master character source/context review and non-explicit identity references. Candidate only: Noir; no age/design clearance yet.
- **ART-002** Master character .blend, GLB, material/rig/export validation; shared Godot toon prototype. Needs selected references; no mass conversion.
- **MORPH-001** Gameplay-to-visual curves, matching body/clothing shapes, extreme-combination lab. No mesh internals in saves.
- **ANIM-001** Shared rig and idle/walk/run/work/talk animation validation at gameplay camera.
- **AI-001** Derive logical NPC locations from existing assignments/phases; nearby navigation, distant logical simulation, bounded stuck recovery. No second work economy.
- **WORLD-002** One reservable station; physical interaction dispatches existing job assignment. Exactly one settlement reward authority.
- **EVENT-001** One social interaction/event with camera/dialogue presentation and existing service completion rules.
- **SAVE-002** Current-version 3D interaction/day-transition save-load tests. No old-save migration or backwards-compatibility requirement; fresh starts expected (D-011).
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
