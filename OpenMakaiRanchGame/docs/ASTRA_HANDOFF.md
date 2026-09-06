# ASTRA Handoff

Checkpoint: 2026-09-05. Resume from repository evidence, not prior chat. Canonical docs directory is `OpenMakaiRanchGame/docs/`. Long-term 3D remake is **not complete**.

## Current Objective

Scope correction (D-011): no public release exists; fresh games are expected. Do not implement further old-save migrations or backwards compatibility. Keep current-version save/load correct and prioritize gameplay. Earlier compatibility commitments in this handoff and other documents are superseded. Existing migration code/tests remain untouched, not a new development goal.

Preserve the verified existing-game baseline, then make persistence and shared mutation/rebinding reliable before adding the third-person world. Initial 3D slice is non-explicit. No production art has started.

## Current Task ID

Baseline AUDIT-001/AUDIT-002/TOOLS-001/DATA-001/CORE-001, SAVE-001 and CORE-002 foundation completed within their documented scope. User authorized one budget-bounded further step; that step is complete. Stop at this verified checkpoint. Full importer, content safety, parity and 3D acceptance remain open. Next visible milestone: WORLD-001 after a minimal floor plan; consult Kanban for the other unresolved priorities.

## Current Git Branch

`feat/godot-universal-mcp-plugin` at repository root `E:/OpenMakaiRanch`.

## Current Commit

`5aba2bc3edbfae2f2a218d1e9e8d2b2493b0f214` — `feat: Schedule polish + current job highlight + nav hidden screens fix`.

Baseline changes are a local patch; no commit/push performed. Inspect staged/unstaged diff before continuing. Do not reset existing dirty `.worktrees/*`, delete `OpenMakaiRanch.zip`, or remove `material_maker_1_7_windows/`.

## Completed Recently

- TOOLS-004: support-agent slice limited to the read-only character audit verifier, its Python regressions and evidence documentation. Removed optimization-disabled assertions and the documented execution of report-embedded Python. Original 11 CSVs / 136 listed citations still match; full snapshot is correctly stale for SaveMigrator.cs, GameRoot.cs and UiShellController.Screens.cs. No game code, assets, source CSVs or report hashes changed. DATA-002 remains open; this is not runtime enforcement. See WORK_LOG for exact checks.

- CORE-002: GameRoot.StateGeneration and TryAssignJob/TryConductMentorship/TryCompleteBondEvent; live UI call sites now use the root boundary with captured generation. GameCommandTests.cs adds 35 assertions. Full smoke 984 PASS, analyzer build succeeds with existing CS8604. Evidence `.artifacts/core-002-verification.log`, `.artifacts/godot/smoke-d920v4z3/`. No new migration, art or 3D code.

- Recalculated project/original inventory and traced startup, data, services, settlement, saves and UI.
- Restored six malformed scene-node headers; exact sidebar/compact smoke assertions now match intentional UI.
- Unified Godot launchers with stable-4.7 .NET discovery and safe disposable smoke profile.
- Fixed addon descriptor, regenerated four duplicate import UIDs with Godot, and verified actual editor/runtime MCP SDK calls/screenshots.
- Fixed two tooling-review findings: cross-project runtime reads and false success opening nonexistent scene. Added failing regressions first; all six bridge tests now pass.
- Audited every original Chara CSV neutrally and checked source/hash/line evidence. No adult/visual clearance granted.
- Created current state, 3D migration plan, issue/decision/kanban/work-log/handoff documents, parity seed and draft art bible/manifest.
- SAVE-001: fixed root flag storage synchronization and explicit-null migration; added SaveRegressionTests.cs. Full isolated smoke now 949 assertions including 81 root-save assertions, PASS. Analyzer build succeeds with existing CS8604. Evidence: `.artifacts/save-001-verification.log`, `.artifacts/godot/smoke-ak2j8ydg/`. Original failure runs are recorded in WORK_LOG.

## Current Working Systems

GameRoot composition, JSON-first typed registry, existing management scene, menu/creation, service APIs and covered save/day/combat/social smoke behavior. Schema is **14**. JSON total **378**, source Chara definitions **11**. See CURRENT_PROJECT_STATE for exact counts and scope; no claim of complete original parity.

Local stable executable: `Godot_v4.7-stable_mono_win64_console.exe`, `4.7.stable.mono.official.5b4e0cb0f`. Project SDK `Godot.NET.Sdk/4.7.0`, target net8.0; selected CLI SDK 10.0.300. No engine upgrade.

## Current Broken Systems

- Standalone EraDataImporter csproj cannot build: missing Core project and Main.
- ContentValidator is a placeholder, not a full JSON validator.
- SAVE-001 flag/null-section gaps are fixed in tested root paths. Arbitrary malformed-save validation is not exhaustive.
- Shared job/social root notifications and stale-generation rejection now exist. Actual world navigation/conversation/reservation cancellation does not; integrate when those objects are implemented. Raw service calls outside these commands are not universally wrapped.
- Source/generator age/design eligibility gaps; adult presentation blocked. Audit is not runtime enforcement.
- Existing CS8604 at UiShellController.Screens.cs:3594 and editor shutdown warning remain.
- No 3D controller, navigation, avatar, morph/animation labs, production shader or playable 3D day.

## Files Changed

Root: `.gitignore`, `AGENTS.md`, `README.md`, `OpenMakaiRanchGame-Editor.bat`, `start-godot-mcp.bat`, new portable `build-and-verify.bat`.

Game: `scenes/Game.tscn`, `src/Tests/SmokeTestRunner.cs`, addon `plugin.cfg`, `editor_bridge.gd`, `runtime_bridge.gd`; four regenerated `assets/portraits/{anon,ayaka,en,yukina}.png.import` UID lines. Portrait pixels and original source unchanged.

New tooling under root `Tools/Godot/`: launch.py, check_user_data.gd, test_launch.py, bridge.mjs, bridge_server.mjs, verify_mcp.mjs, test_bridge.mjs, verify_character_audit.py, README.md.

New docs: CURRENT_PROJECT_STATE, 3D_REMAKE_PLAN, ASTRA_HANDOFF, KANBAN, DECISIONS, KNOWN_ISSUES, WORK_LOG, ADULT_CHARACTER_VALIDATION; audit/simulation-map.md and character-metadata.json; parity/ORIGINAL_FEATURE_MATRIX.md; art/VISUAL_BIBLE.md and ASSET_MANIFEST.md.

Use Git for the exact final file list; this is a scoped summary, not permission to stage unrelated changes.

## Art Assets In Progress

None. Visual bible is Draft 0. Manifest entries are planned, not completed assets. Noir is only a possible non-explicit source-audit candidate; apparent age 26 is not confirmed adult identity or visual approval.

## Generated References

None. Existing-game evidence PNG: `.artifacts/mcp/runtime_screenshot.png` (Main Menu). Earlier native captures documented in WORK_LOG are verification, not art references or golden targets.

## Blender Files In Progress

None created or modified. Existing unrelated unsaved Blender session was not touched.

## Tests Performed

- Actual Windows build-and-verify.bat: exit 0; one existing CS8604, zero errors; 9 Python tests; USER_DATA_ISOLATION_PASS; **868 assertions, SMOKE PASS**.
- Node bridge tests: **6 passed**; tests first failed on missing scene/project rejection before fixes.
- Character evidence verifier: **11 sources, 136 field citations, 12 code/data hashes**.
- Real MCP handshake/list/status/open/tree/run/runtime/screenshot/performance/stop. Missing scene rejected; wrong-project requests rejected on both 9500/9501.
- Live editor tree: **109 nodes**, repaired Rooms/Bond/Pets nodes confirmed. Latest running-editor log had no ERROR/WARNING lines.
- Separate importer build failed with MSB9008/CS5001; recorded, not hidden.

Evidence: `.artifacts/baseline-build-verify.log`, `.artifacts/godot/smoke-6_3vpn91/`, `.artifacts/godot/editor-_06z1hha/`, `.artifacts/mcp/`, `.artifacts/baseline-tooling-review.md`. Review document describes findings before their subsequent fixes; WORK_LOG records resolution.

## Visual Checks Performed

Main Menu screenshot inspected after final bridge changes: title/buttons/language controls readable. Character Creation observed with visible labels; small viewport required scrolling. A later Start Game action was not verified; no complete management-day playthrough, movement/camera feel, 3D or hero visual review was completed.

## Important Decisions

Existing simulation remains authority. Four command-driven time phases remain. First work station assigns work; only day settlement pays. Stable IDs bind visuals; load/new game cancels transient tasks and rebinds current services. No mesh/node internals in saves. No numeric age relabeling or implicit visual approval. No mass art production before one validated pipeline. See DECISIONS and 3D_REMAKE_PLAN.

## Blockers

No tool-access blocker for continued core work. Persistence/notification and content-safety findings block claiming a release-ready integrated 3D/adult pipeline. Importer is broken. Art identity/style remains unselected. Raw MCP endpoint is development-only and not authenticated; do not port-forward or treat it as release hardened.

## Next Exact Action

Audit follow-up: use `python Tools/Godot/verify_character_audit.py --sources-only` only for original-source evidence. The default full check intentionally fails on stale code hashes; do not rebaseline them without reviewing the changed boundaries. The verifier does not check uncited narrative conclusions or implement DATA-002. TOOLS-004 is isolated from the world milestone below.

When work resumes, do not repeat SAVE-001/CORE-002. Inspect SceneRouter and project input/scene setup for an opt-in non-explicit ranch greybox. Agree on a minimal layout from existing facilities; preserve management UI. Future interactions call the three root commands on the main thread with the generation captured when interaction starts; StateChanged observers re-resolve current state/services. Implement actual movement/camera tests and state-replacement task cancellation with the world, not another abstract framework.

## Next 5 Tasks

1. ART-001 / layout subset — minimal source-grounded floor plan for the first non-explicit ranch section.
2. WORLD-001 — opt-in greybox and movement/camera, using the completed CORE-002 boundary.
3. DATA-002 — fail-closed identity/design eligibility requirements before any adult-specific presentation.
4. DATA-003 — recover/build bounded traceable importer without overwriting runtime JSON.
5. ART-001 / visual subset — concept candidates and selected direction before detailed modeling.

## Useful Commands

From repository root:

```bash
git status --short
git diff --stat
git diff --cached --stat
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python -m unittest discover -s Tools/Godot -p 'test_*.py' -v
node --test Tools/Godot/test_bridge.mjs
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/verify_character_audit.py
python Tools/Godot/launch.py --mode editor --isolated
node Tools/Godot/verify_mcp.mjs editor_get_status
OMR_MCP_ALLOW_CONTROL=1 node Tools/Godot/verify_mcp.mjs editor_run_project
node Tools/Godot/verify_mcp.mjs runtime_screenshot
OMR_MCP_ALLOW_CONTROL=1 node Tools/Godot/verify_mcp.mjs editor_stop_project
```

Bash environment prefix shown; Windows cmd uses `set OMR_MCP_ALLOW_CONTROL=1`. Root batch launcher uses Python. SDK adapter dependency comes from mcp-server/node_modules; run `npm --prefix mcp-server ci` only if absent.

## How To Resume

Read this file, KANBAN, DECISIONS and KNOWN_ISSUES; then current Git state, build and relevant code. Do not redo the complete audit or start by generating art. Finish one verified small slice, record exact tests and update these files.

At checkpoint, the editor launched by this task remains open on Game.tscn (port 9500); its runtime was stopped and port 9501 verified closed. Process state can change: re-check before launching. Smoke can run with that editor open if no runtime uses 9501; headless import needs editor port free. Never kill unrelated processes. Never run raw smoke against personal saves.
