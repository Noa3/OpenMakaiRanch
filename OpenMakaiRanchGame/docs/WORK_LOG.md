# Work Log

## WORLD-001 greybox slice + engine path fix (2026-09-06)

### Engine path root cause
The repo-root 4.7.0 binaries were broken for Mono: `Godot_v4.7-stable_mono_win64_console.exe` is a **198 KB stub** (a real build is ~179 MB) that crashes Mono with "Assemblies not found" (signal 11) the moment it inits. `launch.py` prefers the `*_console` name (first in `NAMES`) and only validates via `--version`, so it silently selected the broken stub. The working engine is **Godot 4.7.2 mono** at `E:\GodotEditor\Godot_v4.7.2-stable_mono_win64.exe` (full build + bundled GodotSharp API). Pass it via `GODOT_BIN` or `GODOT_HOME`. This unblocks all headless smoke/import runs.

### WORLD-001 greybox slice (playable, opt-in)
Built against the ART-001 floor plan. Two layers, both in `src/World/`:

- **Deterministic, Node-free** (unit-testable headlessly): `WorldMovementMath` (camera-relative XZ movement, dead zone, bounded accel, gravity, diagonal speed clamp), `WorldCameraMath` (orbit position, zoom clamp, pitch clamp, geometry clamp so the camera never penetrates walls), `WorldInputGate` (world vs UI ownership), `WorldInteractionGuard` (double-activation + despawn guard).
- **Node layer**: `ThirdPersonPlayerController` (CharacterBody3D, camera-relative, gravity, collision), `WorldCameraRig` (yaw/pitch/zoom, collision-aware follow, recenter), `WorldStation` (smart object: stable `TargetId`, `Label`, `CommandKind`/`CommandTargetId`, one reserved slot), `RanchGreyboxController` (wiring, interact prompt, management-UI enter/leave), `WorldInputBootstrap` (idempotent InputMap: WASD/arrows, F/Space interact, R recenter).
- **Command boundary**: `IWorldCommandDispatcher` + `GameRootCommandDispatcher` route every station interaction to `GameRoot.TryAssignJob/TryConductMentorship/TryCompleteBondEvent` with the `StateGeneration` guard. The station never computes income/exp/bond — the plan's "no second reward calculator" rule holds.
- **Scene**: `scenes/dev/RanchGreybox.tscn` — opt-in (NOT the boot scene), ground/walls/interior/station/player/camera/prompt/management button.

### Verification
- `dotnet build OpenMakaiRanch.sln`: **0 warnings, 0 errors**.
- `GODOT_BIN=E:/GodotEditor/Godot_v4.7.2-stable_mono_win64.exe python Tools/Godot/launch.py --mode smoke`: **SMOKE PASS, 1038 assertions OK, 0 FAIL** (was 998 before the world slice — +40 world assertions: diagonal speed, camera clamps, input ownership, double-activation, dispatcher dispatch, scene node contract).
- Committed on `main`, pushed to `origin/main`.

## Quality audit + BUG-001 fix + ART-001 floor plan (2026-09-06)

Continued after DATA-002 gate was verified. Three scoped slices:

### DATA-002 quality audit (gate completeness)
Traced every `ConfirmedAdult` writer in production code. Only test fixtures set it; the recruit pool (`GenerateApparentAgeWithEligibility`) returns `Minor`/`Unknown` only. No production bypass exists. Found one real inconsistency: `SaveStateFactory.CreateNewGame` created the starting roster `CharacterState` without `ApparentAge`/`AdultEligibility`/`Provenance`/`AgeContextNote`, while the recruit path (`:299-301`) preserved them. anon/rancher ended `Provenance=Unknown` despite the definition carrying `OriginalPlayer`/`RemakeFallback`. Fixed: the starting roster now copies all four from the validated definition, so the audit trail is identical across both creation paths. Gate still fail-closed (anon/rancher are `Unknown`, never auto-approved).

### BUG-001 (CS8604) fixed
`UiShellController.Screens.cs:3598` — `Path.GetDirectoryName(Assembly.Location)` returns `string?` fed into `Path.Combine`. Incremental build masked it; a `--no-incremental` build surfaced it. Fixed with `?? Directory.GetCurrentDirectory()` fallback. Clean `--no-incremental` build now **0 warnings / 0 errors**.

### ART-001 floor plan drafted
Wrote `docs/art/RANCH_FLOORPLAN.md` — source-grounded on the 11 `FacilityDefinition` records (no invented facilities): entry, central hub, production ring, dormitory cluster, pasture, well waypoint, camera clearance, event space, and one smart-object target per production facility. This is the layout contract for WORLD-001's `RanchGreybox.tscn`. Remaining for ART-001: user selects daytime/evening/interior visual direction (deferred to user per plan).

### Verification
- `dotnet build OpenMakaiRanch.sln --no-incremental`: **0 warnings, 0 errors**.
- `python Tools/Godot/launch.py --mode smoke`: **SMOKE PASS, 998 assertions OK, 0 FAIL**.
- Staged, not committed.

## DATA-002 — fail-closed adult eligibility gates (code, not assets)

Scope: runtime enforcement of the audit policy in `ADULT_CHARACTER_VALIDATION.md`. No age relabeling, no asset clearance, no new visual content. Non-adult gameplay untouched.

Implemented:
- `AdultEligibility` enum (Unknown, ConfirmedAdult, Minor, Ambiguous) on `CharacterDefinition` and `CharacterState`; `AgeContextNote` carries the review context.
- `AdultEligibilityGate` (new, 150 lines): single classification source. Minor apparent age → `Minor`; school/jk/baby-face/child context markers → `Ambiguous`; everything else → `Unknown` (no implicit approval). All query methods fail closed.
- `DataRegistry.Add(CharacterDefinition)` runs `ValidateAndSetEligibility` at import/seed; seeded Slay=13 and Maria=15 classify `Minor`, Ayaka's JK marker classifies `Ambiguous`.
- `CharacterGenerationPools.GenerateApparentAgeWithEligibility` tags generated ages; `PlayerApparentAges` exposes only 18+ to the player avatar picker (audit line 95: "Players are not exempt").
- `MatureServices.PerformAction` rejects non-`ConfirmedAdult` characters before any mature action dispatch, with a denial reason in the report.
- `GameRoot.ModifyPlayer` clamps below-18 player ages to 18 and logs the denial; `LoadSlot` downgrades any corrupt/legacy `ConfirmedAdult` record with apparent age below 18 to `Minor` (audit line 96: defaults must not grant approval).
- `SaveMigrator` backfills missing eligibility fields as `Unknown` on load (never `ConfirmedAdult`).
- `PortraitRenderer` no longer gates the standard clothed roster portrait (that broke character UI); adult/sexual presentation gating remains at the action and asset layer.

Verification:
- `dotnet build OpenMakaiRanch.sln`: 0 errors, 0 warnings.
- `python Tools/Godot/launch.py --mode smoke`: **SMOKE PASS, 998 assertions OK, 0 FAIL**, including 12 new gate assertions (minor denied at definition + state level, Ambiguous denied, Unknown denied, ConfirmedAdult granted) and the previously failing training/consent/tool/two-per-day assertions now exercising their real mechanics.
- Test fixtures grant `ConfirmedAdult` directly as a review-approval record; no test sets `ApparentAge = 20` to bypass the gate (audit line 107 forbids number-based fixes).

Remaining (explicit, not claimed done):
- Independent non-explicit design/context review (audit item 6) has not been performed; no character is `ConfirmedAdult` in shipped data.
- `verify_character_audit.py` full snapshot is stale after the SaveMigrator/GameRoot/Screens edits; source-only evidence still passes. Re-audit changed boundaries before refreshing hashes.
- Staged, not committed.

## TOOLS-004 — character audit evidence support slice

Kept the original reference, audit JSON, gameplay, assets and pre-existing staged changes intact. Scope is evidence-tool correctness, not adult content generation, age/design clearance, runtime enforcement or a new source-dialogue audit. DATA-002 remains open.

Reproduced a false pass: the old assert-based verifier returned `AUDIT_EVIDENCE_PASS` with exit 0 for a changed code fixture under `python -O`. Replaced assertions with explicit validation errors. Added source ID/file correspondence, positive 1-based line validation, schema/count checks, literal repository-relative path checks, duplicate input-path rejection, bounded reads and actionable CLI errors. Added portable `--root`/`--report` and an explicit `--sources-only` diagnostic with a different pass marker. Removed the documentation command that executed Python embedded in the JSON report; the reviewed verifier never executes it.

Test-first failure rounds exposed optimization bypass, zero-as-last-line aliasing, mismatched citation files/IDs, empty metadata, duplicate manifest paths, path traversal, unsupported schema and silently overwritten duplicate JSON fields. CLI regressions failed before the new argument/error handling. Final checks: **33 Python tooling tests passed**, including **24 audit tests**; the audit suite also passed under optimized Python. **6 bridge tests passed**. Analyzer rebuild succeeded with **0 errors and the pre-existing CS8604 at UiShellController.Screens.cs:3598**. Evidence: `.artifacts/tools-004-verification.log`.

Independent read-only review found two further gaps: a valid ID citation could be mislabeled as a display-name citation, and integer-length/nesting decoder errors escaped the CLI error format. Reproduced both with failing regressions, then bound schema-1 neutral metadata names to their source keys and translated decoder ValueError/RecursionError into AuditError. The final counts above include these regressions; all 136 real-source field citations still pass the stricter mapping.

Fresh isolated Godot smoke passed **984 assertions**, with `USER_DATA_ISOLATION_PASS`. Evidence: `.artifacts/godot/smoke-q8o2_8ox/`; rejection diagnostics from the deliberately invalid save fixtures are expected. This is headless regression coverage, not a visual/gameplay-feel review. No personal saves used. A separate MCP SDK status probe completed tool discovery but editor access failed with `ECONNREFUSED 127.0.0.1:9500`; no live editor verification is claimed for this support slice. The earlier handoff's open-editor state is historical, not current.

Fresh source-only check passes **11 source CSVs / 136 listed field citations**. Default full check correctly exits 1: the snapshot predates edits to `SaveMigrator.cs`, `GameRoot.cs` and `UiShellController.Screens.cs`. Hashes were not refreshed. Re-audit those changed boundaries before revising the snapshot; source-only success must not be described as full audit success. The verifier checks declared evidence, not uncited conclusions or inferred approval. No commit/push.

## CORE-002 — one budget-bounded follow-up

Added three GameRoot presentation commands for job assignment, mentorship and bond-event completion. Existing UI uses them with required captured StateGeneration. Successful commands emit one StateChanged; invalid/no-op job/stale/duplicate-event commands do not. Generation changes on service rebuild; delayed UI actions after NewGame/Load are rejected. Underlying formulas and settlement payout remain unchanged. No save-schema/migration work.

RED: smoke-ekf2dhby showed missing job/mentorship observer notifications; smoke-lgf9wg56 showed missing event notification and stale UI mutations after NewGame/Load. GREEN: smoke-d920v4z3, 984 assertions including 35 command assertions; analyzer build succeeded with existing CS8604 at UiShellController.Screens.cs:3598. Evidence: `.artifacts/core-002-verification.log`. Includes real Godot button signals, two observers, NG+ and failed-load generation checks. Headless functional verification, not a graphical playthrough. Focused local review only; no new reviewer agent was launched to conserve allowance.

World navigation/conversation/reservation objects do not exist yet; cancellation for them remains future integration. Raw services remain available internally and do not universally notify. Next visible milestone is WORLD-001 after a minimal floor plan; keep other priorities in Kanban explicit.

## SAVE-001 checkpoint — budget-limited continuation

User requested conserving remaining GPT weekly allowance. Finished the already-started save repair; CORE-002 not started.

GameRoot.SaveSlot now calls Flags.SyncToStorage before serialization. SaveMigrator normalizes roster before schema-13 conversion, plus Reports/Flags and nested flag maps. SaveRegressionTests exercises actual root save/load, six flag stores, schema-13/current null sections, preserved valid neighbors, and rejected-load state/file preservation. Schema remains 14.

RED: smoke-96zwu074 lost all six flag stores; smoke-uodj6sgm rejected null legacy roster and threw on null flags. GREEN: smoke-ak2j8ydg, 949 assertions including 81 root-save assertions; USER_DATA_ISOLATION_PASS; analyzer build exit 0 with existing CS8604 only. Full log: `.artifacts/save-001-verification.log`. Invalid-input tests intentionally emit rejection diagnostics. Personal saves untouched. No commit/push.

## 2026-09-05 — baseline / tooling / evidence checkpoint

Scope: preserve the existing remake and original reference; establish a reproducible baseline before 3D art. Branch `feat/godot-universal-mcp-plugin`, initial HEAD `5aba2bc3edbfae2f2a218d1e9e8d2b2493b0f214`. No commit/push performed. Unrelated dirty worktrees, local archive and Material Maker directory preserved.

### Investigation and fixes

Read repository rules, README/AUDIT and architecture/plan/remake/gap documents; traced startup, service composition/rebinding, scheduling/settlement, saves, JSON loading, UI and addon protocols. Recounted 378 JSON entries, 41 src C# files, 5 scenes, zero runtime GLBs; original inventory 881 ERB, 43 CSV, 498 PNG.

Initial smoke exposed scene/test drift. Six Game.tscn node headers started with `|`, hiding Rooms/Bond/Pets nodes. Removed prefixes, corrected expected sidebar names, documented the intentional compact subset, added explicit section checks. Existing gameplay services/formulas/data were not rewritten.

Added shared Python launcher with version verification/discovery, port collision checks, exit/timeout logging and Windows disposable-profile smoke preflight. Updated root batch entry points, README and AGENTS. The actual local executable is stable 4.7 Mono; no upgrade. Portable scripts are no longer ignored.

Fixed plugin.cfg section `[configuration]` to `[plugin]`, verified against Godot 4.7 source and live editor startup. Replaced orphan stdio-server launch with client-owned project adapter using existing MCP SDK dependency. Four duplicated portrait .import UIDs were backed up under `.artifacts/portrait-import-backup/`; Godot regenerated unique UIDs. No images were changed.

Independent tooling review identified missing runtime project binding and nonexistent-scene false success. Added failing Node regressions (missing rejection/exception), then request/response project identity checks in both addon endpoints and adapter, missing-file validation and active-scene readback. Six Node tests now pass. Raw endpoint authentication/export hardening remains open.

### Executed checks

- `dotnet build ... --configuration Debug --no-restore -p:RunAnalyzers=true`: succeeded with 0 warnings/errors on the incremental run.
- Actual Windows `build-and-verify.bat` via cmd: exit 0. Restore/recompile surfaced **one existing CS8604** at `src/Ui/UiShellController.Screens.cs:3594` (Path.Combine path1), zero errors. Do not call the whole project warning-free.
- Same batch ran **9 Python tests**, then engine-resolved storage preflight **USER_DATA_ISOLATION_PASS**, then **868 SMOKE OK assertions / SMOKE PASS**. Log `.artifacts/baseline-build-verify.log`; engine/test profile `.artifacts/godot/smoke-6_3vpn91/`.
- `node --test Tools/Godot/test_bridge.mjs`: **6 passed**, including timeout/framing/identity/missing scene.
- Real MCP SDK handshake/list/calls: editor status, open Game.tscn with active-scene readback, full editor tree, run main scene, runtime status, tree, screenshot, performance, stop. Saved exact responses under `.artifacts/mcp/`.
- Live wrong-project checks rejected responses from editor 9500 and runtime 9501. Missing-scene MCP request rejected with `Scene does not exist`. No unrelated project was controlled.
- `python Tools/Godot/verify_character_audit.py`: **11 sources, 136 field citations, 12 code/data hashes** verified independently without executing the report's embedded extractor. No adult/visual clearance.
- Separate EraDataImporter project build: **failed**, MSB9008 missing Core project and CS5001 missing Main. This is an explicit baseline blocker, not silently counted as a passing game build.

### Visual checks

Captured and inspected actual Main Menu via runtime MCP PNG. Controls readable; no obvious clipping. Clicked New Game and observed Character Creation; labels visible, smaller viewport scrolls. A complete management/day playthrough was **not** achieved; a later Start Game click did not establish a verified transition. Native window resizing/input coordinates limited that attempt. Do not claim gameplay-feel validation from these screenshots.

Editor Game.tscn tree contains the repaired Rooms/Bond/Pets nodes. Headless smoke also renders management screens, but screenshots of those screens were not individually reviewed. No 3D scene/model/material, movement, animation, morph or lighting-target comparison exists yet.

### Documentation and remaining work

Created CURRENT_PROJECT_STATE, 3D_REMAKE_PLAN, KANBAN, DECISIONS, KNOWN_ISSUES, this WORK_LOG and ASTRA_HANDOFF. Added neutral character audit/evidence, parity matrix seed and draft art bible/manifest. Drafts contain no fake approvals or generated assets.

Next engineering task: SAVE-001, focused root flag roundtrip and null migration regression tests. CORE-002 follows for mutation notifications/rebinding before world implementation. Visual direction and source character review remain prerequisites for detailed art. See handoff for exact resume commands.
