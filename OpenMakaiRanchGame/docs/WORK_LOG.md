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


## 2026-09-09 — WORLD-003c prep: world function + HUD pass

Branch: `feat/world-hud-function-pass`.

Scope is deliberately non-explicit and leaves all adult-content data/services untouched.

Implemented:
- Added `WorldHudController`: shared-simulation HUD for day/season/phase/weather, gold/spirit/mana, roster size, selected worker/current job, nearest station prompt and transient feedback.
- Expanded the opt-in RanchGreybox from one job marker to six ordinary spatial job stations: Office, Kitchen, Workshop, Pasture, Pharmacy Lab and Dairy Barn. All dispatch through the existing `GameRootCommandDispatcher`; no second economy/reward path was added.
- Fixed the actual controller interaction context: the old scene path passed the station target id as `CharacterId`; the world controller now maintains a selected roster worker and passes that real runtime id to `TryAssignJob`. Tab cycles the worker.
- Fixed camera composition: `WorldCameraRig.Target` is now explicitly bound to the player's stable head target. Added hold-RMB mouse orbit and mouse-wheel zoom, gated by the same `WorldInputGate` used by movement/UI ownership.
- Replaced the controller's `InputEventAction`-only interaction handling with `Input.IsActionJustPressed`, so ordinary mapped F input reaches the spatial interaction path.
- Added smoke assertions for authored HUD nodes, multi-station discovery/dispatcher binding, camera target wiring, selected-worker binding and a controller-level station interaction that must mutate the selected roster worker's shared Schedule state.

Validation status:
- Repository diff reviewed through GitHub.
- Local build/runtime cannot be executed from the ChatGPT runtime used for this patch (no local .NET/Godot toolchain available there).
- Pull-request CI is the verification path; do not record a new smoke-pass count until CI/local Godot 4.7 verification actually succeeds.

Still intentionally deferred:
- Boot-world composition with the existing full management shell.
- NavigationAgent3D travel/reservations.
- Final ranch art/assets and concept selection.
- Character model production.

### WORLD-003c composition extension
- Added `scenes/WorldGame.tscn`: permanent 3D RanchGreybox + the existing `Game.tscn` management shell on a CanvasLayer. Both use the same GameRoot.
- Added `WorldGameController`: M/HUD-button toggles ordinary management; character creation, prologue, victory/title are mandatory UI flows and cannot be hidden. Exiting the new-game prologue to `ranch` automatically returns to the 3D world.
- MainMenu now routes New Game / Continue / New Game+ to `WorldGame.tscn` instead of replacing the world with `Game.tscn`.
- Added a generic `UiShellController.ScreenChanged` event plus read-only current-screen/full-screen state; no screen content was changed.
- The world now guarantees a runtime `Environment` resource before applying shared DayPhase lighting, so ambient/tonemap state is not silently discarded.
- Added WORLD-003c smoke coverage for composed scene nodes, overlay visibility, world-input suspension/restoration, mandatory character-creation/prologue lock and automatic return to the ranch.
- Added a nested-camera lookup fix in `ThirdPersonPlayerController`; the authored camera is under `CameraRig/Camera`, not a direct player sibling.

External visual-direction research (reference only, no copied assets): GodotCon anime/stylized 3D workflows emphasize deliberate toon shading, outlines, edited normals and modular Blender→Godot asset authoring. The environment should use readable paths/landmarks and authored lighting rather than attempting photorealism.


## 2026-09-09 — WORLD-003c/003d continuation: mixed creation + playable world flow

Branch: `feat/world-hud-function-pass`, PR #2. No CI/build-pipeline files were added or modified in this continuation.

Implemented:
- Kept Bootstrap/MainMenu fully 2D. New Game still starts from `MainMenu.tscn`; gameplay routing remains `WorldGame.tscn` only after the menu action.
- `CharacterCreationScreen.tscn` is now a mixed UI: existing editable 2D settings live beside a `SubViewport` 3D preview. Added `CharacterCreationPreviewController` and a reusable neutral `PlayerAvatar3D`.
- The 3D player stand-in reflects ordinary player presentation data (height, skin/hair/eye colors, horns, glasses) and deliberately excludes adult-specific body presentation. The same stand-in is used in the ranch so creation and gameplay have one presentation source.
- Character creation no longer rebuilds the full screen on every `GameRoot.StateChanged`, preventing LineEdit focus loss and picker resets while typing/editing. The 3D preview listens to shared state independently and its camera reframes for player height.
- Added world sprint (`Shift`), smooth facing toward camera-relative travel, and visible player reuse in the `CharacterBody3D`. The old box mesh remains as a hidden debug geometry/collision reference only.
- Added explicit management exit paths: `Return to World` button plus `Esc` for ordinary management; mandatory character-creation/prologue/victory/title flows remain locked visible.
- Fixed Continue/New Game+ save selection. MainMenu now considers autosave slot 0 and manual slots 1-3 and selects the most recently saved usable/victory slot instead of ignoring slots 2/3.
- Ranch presentation now subscribes to shared `GameRoot.StateChanged` while in-tree, so load, assignments, facility upgrades and time advancement refresh daylight/roster/HUD immediately.
- Spatial job stations now respect the same facility progression used by management. Pasture/Kitchen are available in a new game; unbuilt Workshop/Pharmacy Lab/Dairy Barn expose a lock reason and cannot bypass management construction.
- Added a world `Advance Phase`/`Plan Night`/`End Day` HUD action. Normal phases advance through `GameRoot.AdvanceTime`; Night without a plan opens the existing management choice; completed settlement opens the existing Daily Report. No second clock, settlement, reward or report system was introduced.
- Night-plan controls in the management shell are now shown only during the actual Night phase and never on mandatory full-screen creation/prologue screens.

Regression coverage added (not executed in this ChatGPT environment):
- mixed character-creation scene contract, live SubViewport binding and generated neutral player geometry;
- shared player visual in the ranch plus sprint > walk;
- explicit Return-to-World and world phase-control nodes;
- station progression locks and immediate unlock after the shared RanchService builds Workshop;
- complete world clock path Morning -> Afternoon -> Evening -> Night -> management night plan -> settlement -> next Morning -> existing Daily Report;
- existing controller-to-shared-Schedule assignment path remains covered.

Static verification performed through repository inspection:
- all 7 `.tscn` files were re-read at branch HEAD;
- every Script/PackedScene ext_resource exists;
- every authored non-root node parent path resolves;
- static scene audit result: 0 missing resource references and 0 invalid parent paths.

Validation still required before merge:
- run the existing `build-and-verify.bat` / Godot 4.7.x Mono smoke locally;
- manually verify gameplay feel, viewport sizing/focus, camera collision, input, Return-to-World, save/load and the full day flow;
- no new assertion count or runtime PASS is claimed by this continuation.


## 2026-09-09 — onboarding, world readability and resident interaction continuation

Branch: `feat/world-hud-function-pass`, PR #2. No GitHub workflow/build-pipeline files were changed.

Implemented:
- Added persistent tutorial preferences to `SettingsState` / `settings.json`: tutorial hints can be disabled, completed basic steps are remembered across save slots, and progress can be reset.
- Added `WorldTutorialController` plus authored HUD nodes: five contextual first-run steps (movement, camera, worker assignment, management, day progression), per-step skip, full tutorial skip, and an always-available F1 help panel.
- F1 Help takes world input ownership while open and restores it on close; it does not pause or duplicate simulation state. Help contains controls, the basic ranch loop, tutorial toggle and restart.
- Added Settings-menu controls for tutorial hints and restarting onboarding.
- Added contextual world prompts/tooltips: nearby stations show the selected worker in the action text, locked facilities expose the reason, and the idle prompt advertises F1 Help.
- Added a persistent `Next Step` guidance panel. It derives suggestions from the shared phase/schedule (resting workers, Night planning, End Day readiness) but never blocks the player's choice.
- Added a collision-free `RanchPresentationBuilder` placeholder pass: readable paths, entry arch, custom ranch-name sign, central well/notice landmark, stylized boundary vegetation and facility building proxies. Facility colors/labels follow the existing RanchService built/unbuilt state.
- Added `docs/assets/CC0_ASSET_CANDIDATES.md` as the external-asset provenance gate. Current evaluated source families are Quaternius Farm Buildings, Quaternius Ultimate Stylized Nature, Kenney Nature Kit and Poly Haven; no external binary was silently vendored.
- Roster stand-ins now expose readable nameplates and can be found spatially. Pressing F near a closer resident requests the existing Character Detail screen through `WorldGameController`; no second bond/dialogue/reward path exists.
- Main-menu and character-preview controls received explanatory tooltips.

Regression coverage added (runtime execution still pending locally):
- tutorial settings defaults and clone isolation;
- authored TutorialOverlay/F1 Help/Presentation nodes;
- help input ownership and restoration;
- stylized placeholder generation and facility-state refresh;
- nearby resident -> existing character detail -> Return to World;
- existing world/day/station/save flows remain covered.

Verification in the connected repository environment:
- static audit re-read all 7 `.tscn` files;
- 0 missing Script/PackedScene external resources;
- 0 invalid authored parent paths;
- compare against `main` shows no `.github/workflows/*` changes.

Still required before merge:
- run the existing local Godot 4.7.x Mono / `build-and-verify.bat` validation;
- visually inspect tutorial/help layout at several resolutions, placeholder landmark framing, NPC nameplates, contextual prompts and resident interaction;
- after that validation, admit exact CC0 asset packages individually with recorded package/hash/source/license rather than copying untracked downloads.


## 2026-09-09 — TOWN-001: playable Okachi Town + travel/navigation slice

Branch: `feat/world-hud-function-pass`, PR #2. No GitHub workflow/build-pipeline files changed.

Implemented after the previously recommended world-readability/onboarding work:
- Added `SimpleNavigationRegionBuilder` and real `NavigationAgent3D` children to roster stand-ins. Pathfollowing now updates from `_PhysicsProcess()` using `TargetPosition` / `GetNextPathPosition()`; the current open ranch uses a simple rectangular nav region and keeps straight-line fallback behavior if no path is available.
- Added `WorldTravelPortal`, Ranch south/town gate and persistent world-area composition.
- Added `TownGreybox.tscn` as a second playable 3D area inside the same `WorldGame` / `GameRoot`.
- `WorldGameController` now switches active area (ranch/town) by visibility, ProcessMode, camera and input ownership rather than creating a second game session.
- Travel currently costs no extra gold/time because no existing shared rule defines such a cost.
- Added additive `SaveState.WorldAreaId` plus validated `GameRoot.SetWorldArea`; old/unknown values normalize to `ranch`. Saving in Town and loading restores Town; closing a load/management overlay immediately applies the loaded area.
- Added 3D Town services that route only to existing UI/service authorities: General Store -> shop, Adventure Guild -> adventure, Research Office -> research (existing Workshop prerequisite preserved), Tavern -> roster, Bathhouse -> bond, Town Hall -> milestones, Construction & Planning -> town/facility planning.
- Added physical Town south gate and explicit Return-to-Ranch button. Existing 2D Town Hub `Return to Ranch` now requests physical travel when hosted by `WorldGame`, with legacy UI fallback otherwise.
- Added `TownHudController`, contextual service/tooltips, state-aware suggested errands, first-visit tutorial and independent F1 Town Help.
- Added `TownPresentationBuilder`: collision-free roads, plaza/fountain, service-building proxies, doors/signs, town gate, lamps and vegetation. Locked services are visually greyed and labelled.
- Ranch tutorial now includes a dedicated Okachi Town travel step.
- Added `docs/art/OKACHI_TOWN_WORLD.md` with layout, service mapping, travel rules, navigation plan, acceptance criteria and follow-up slices.
- Extended CC0 candidate provenance with Kenney Fantasy Town Kit and Quaternius Medieval Village Pack. Exact binary archives remain unvendored pending local admission/hash/source records.

Regression coverage added (runtime still pending locally):
- Ranch NavigationRegion3D and NavigationAgent3D roster followers;
- Ranch -> Town area persistence and active camera/input ownership;
- Town authored services and DaylightRig;
- Research Office Workshop lock;
- General Store spatial interaction -> existing shop UI -> return to Town;
- Town F1 Help input ownership;
- physical Town south gate -> Ranch;
- Town Hub UI Return to Ranch -> physical travel;
- save/load preserves `WorldAreaId`.

Static verification at branch HEAD:
- 8 `.tscn` scenes re-read;
- 0 missing Script/PackedScene ext_resources;
- 0 invalid authored parent paths;
- compare against `main`: 0 `.github/workflows/*` changes.

Still required before merge:
- run existing local Godot 4.7.x Mono / `build-and-verify.bat`;
- manually verify NavigationAgent path behavior after NavigationServer sync, camera ownership during travel, Town service prompts, F1 layouts, save/load area restoration and UI-return travel;
- when real CC0 building/fence collision is admitted, replace the simple navigation region with an editor-baked navmesh and add bounded stuck recovery.
