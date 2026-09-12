# Add the first shared-simulation 3D ranch slice

This is a living execution plan. Keep Progress, Surprises & Discoveries, Decision Log and Outcomes & Retrospective current. Canonical plan location follows the requested existing project docs directory; do not create a competing root-level plan.

## Purpose / Big Picture


A player should enter the ranch, move with a third-person camera, find scheduled characters, use one work station and one social action, open the existing management UI, advance the day, save and load. Both world interactions and UI must operate on the same simulation. The first target is a coherent, non-explicit playable section, not a replacement of existing services or mass production of character meshes.

The long-term art target is modern stylized anime 3D. A functioning greybox proves layout and controls first. A greybox is simple editable geometry used to test scale, collision and travel before detailed art.

## Progress


- [x] Baseline repository, docs, C# composition, data and save schema inspected.
- [x] Main game builds; isolated smoke passes after scene navigation repair.
- [x] Stable 4.7 .NET launcher and actual editor/runtime MCP path verified.
- [x] All 11 source Chara CSVs audited for neutral metadata; adult-validation gaps documented, no visual approvals.
- [x] Current-project evidence and migration boundaries documented.
- [x] SAVE-001 completed: root flag persistence and null-section regression fixes; full smoke 949 PASS. Per user clarification, no further legacy-save migration or backwards-compatibility work is required.
- [x] CORE-002 foundation: root job/mentorship/event commands and live UI rebinding with required StateGeneration guards; 35 new assertions, smoke 984 PASS. World-specific navigation/reservation cancellation remains part of WORLD-001.
- [ ] Source-grounded floor plan, draft visual bible, non-explicit concept candidates and selected visual targets.
- [ ] Third-person ranch greybox and automated movement/camera/scene checks.
- [ ] Reusable avatar binding, visual profiles and master-character test labs.
- [ ] Spatial schedule presentation, smart-object reservations and shared-service interactions.
- [ ] End-to-end saved/loaded playable day, visual and performance acceptance.

## Surprises & Discoveries


Current schema is 14; older docs said 11. Runtime JSON contains 378 definitions. Existing ScheduleService offers only AssignableJobs, GetAssignment and AssignJob. It has no spatial routine API or GetEffectiveJobId. EndDay calculates an entire day's output synchronously. Triggering ApplyJobOutput on arrival or animation would duplicate rewards.

GameRoot replaces State and rebuilds most services on NewGame/LoadSlot. Those services capture the old state in readonly fields. Direct UI Schedule/Bond calls sometimes refresh only the shell. A world subscribing only to StateChanged would miss some mutations and could retain stale services after load.

Six malformed `|[node` declarations hid Rooms/Bond/Pets navigation. Repair is covered by exact sidebar/compact node assertions. The apparent importer project is incomplete: missing Core project reference and no entry point. Do not discard current JSON on the assumption it can be regenerated.

Original source includes minor apparent-age values; the generated-age pool also includes minors. No character has passed an identity/design review. Numeric relabeling is not clearance. Non-explicit world work can proceed without adult-specific presentation.

## Decision Log


Decision: retain GameRoot, all simulation services, JSON definitions, save serialization and existing UI. Rationale: this is an additive presentation migration, not a new game. Author: Astra, 2026-09-05.

Decision: first work station changes an assignment through the shared command boundary, then presents work; only day settlement pays rewards. Rationale: preserves demonstrated current behavior and avoids inventing partial-work/capacity mechanics. Physical travel, blocked paths and reservation failure initially affect presentation only. Author: Astra, 2026-09-05.

Decision: keep four existing phases and command-driven day advancement. Rationale: a realtime sky clock or fifth phase would change simulation and save semantics without original-game evidence. Author: Astra, 2026-09-05.

Decision: resolve transient positions from stable actor/location IDs on load. Add persisted world state only when behavior requires it. Pre-release fresh starts are expected; schema changes do not require old-save migration or backwards compatibility. Keep current-version save/load correct. User clarification supersedes the previous dedicated-migration requirement; see D-011.

Decision: stop before art identity lock until source/context and non-explicit visual direction are reviewed. Do not generate adult-specific imagery, remodel minor-coded source designs for sexual use, or certify unknown ages from defaults. Author: Astra, 2026-09-05.

## Outcomes & Retrospective


Baseline tooling and evidence exist. No 3D feature is yet implemented. Smoke PASS proves the covered current contracts, not complete original parity, all save cases or character safety. The next useful change is persistence/notification hardening, not detailed character art. Update this section at every verified milestone.

## Context and Orientation


Repository root is `E:/OpenMakaiRanch` on the audited machine. Game files are under `OpenMakaiRanchGame/`. Run commands from repository root unless stated otherwise. Python tooling discovers the local stable 4.7 .NET executable; .NET game target remains net8.0. Original reference `eraMakaiRanch-game-eng-translation/` is read-only.

`src/App/GameRoot.cs` owns current State and services. `src/Gameplay/SaveService.cs` serializes schema-14 POCOs from `src/Core/Models/SaveModels.cs`; SaveMigrator normalizes older state. `src/Data/DataRegistry.cs` loads typed resources from `data/*.json` with fallback seeds. `src/Gameplay/ScheduleService.cs` stores job assignments. RanchService calculates output but DailySettlementService owns its day-level application. Economy/Inventory/Shop and Bond services already implement state mutation.

`scenes/Game.tscn` is a full-screen Control UI; `src/Ui/UiShellController.*` renders screens. Its opaque background and mouse input cannot simply remain on top of a 3D viewport. SceneRouter currently routes shell screen IDs. Keep the menu, character-creation and Continue flow operational while adding an opt-in world view.

The Universal MCP addon lives in `addons/godot_universal_mcp/`. The repository adapter under `Tools/Godot/` speaks its actual protocol; installed Coding-Solo tools are separate. Read tools verify project identity; mutations require opt-in. Runtime logs are files, not the addon's placeholder log methods.

## Plan of Work


### SAVE-001: preserve state before adding world state

Completed historical task, not remaining work. Legacy fixture coverage below documents what was tested; it is not a continuing compatibility commitment. User explicitly excludes further pre-release old-save support. Current-version flag persistence remains required.


In root SaveSlot and FlagService, first reproduce a flag set through the service disappearing after root SaveSlot/LoadSlot. Use an isolated profile and a disposable slot. Repair the storage synchronization at the actual save boundary. Add explicit-null JSON fixtures for schema 13 Roster/Characters and current Reports/Flags; normalize before migration accesses them or reject invalid data without replacing the current root. Do not silently coerce future schema into current. Verify load failure leaves the live game unchanged. Keep original saves untouched and do not bump schema for a repair of existing fields.

### CORE-002: one command boundary and safe rebinding

Foundation completed. GameRoot exposes TryAssignJob(characterId, jobId, expectedGeneration), TryConductMentorship(characterId, expectedGeneration), and TryCompleteBondEvent(eventId, expectedGeneration). All run on the Godot main thread, resolve current services, reject stale generation/invalid targets and notify once on success. UI captures StateGeneration during rendering. BuildServices increments the generation before observers run. Tests emit real button signals before/after state replacement and verify two observers, duplicate-event rejection and no assignment payout. No universal event bus or actual world-task cancellation was introduced.


Add narrowly scoped GameRoot wrappers for job assignment and mentorship used by both UI and world, preserving existing service formulas and validation. Each successful mutation emits one shared notification; failed commands must not fake state changes. Keep raw service methods for internal composition but migrate the corresponding UI call sites. Avoid a generic event bus or a second state container.

World adapters retain stable character IDs and resolve current State/services at execution time. Give a world controller a state-generation value incremented when root State identity changes; cancel pending navigation, conversations and reservations before rebinding. Test NewGame and LoadSlot through the real GameRoot with a live test adapter, not a manually copied substitute.

### ART-001 / ART-002: establish references and floor plan


Use `docs/art/VISUAL_BIBLE.md` as a draft, not an approved style. Derive ranch facilities and character roles from audited definitions and original source. Draw a top-down plan with entrances, routes, work station, interior, camera clearance and event space. Produce daytime/evening/interior concept candidates; keep rejected concepts out of the active reference set without deleting source material. Significant visual identity changes require user selection. Keep prompts, model/tool provenance and references.

For a hero candidate, complete a neutral source audit beyond JSON. Noir is only a candidate because the source has apparent age 26; this is not confirmed adulthood or visual clearance. Review rights/context and actual non-explicit references before selecting. Preserve exact measurements versus artistic approximations separately. Do not start ten character models.

### WORLD-001: independent, playable greybox


Add an opt-in development scene `OpenMakaiRanchGame/scenes/dev/RanchGreybox.tscn`. Author editable nodes, simple ground/collisions, one interior entrance and one station. Add C# scripts under a new `src/World/` directory only after checking class-name collisions. A proposed ThirdPersonPlayerController uses CharacterBody3D, camera-relative input and bounded acceleration/gravity. A proposed camera controller owns a collision-aware follow rig, mouse/gamepad look, zoom and recenter. Use Godot's documented 4.7 API; do not assume property names from an older engine.

Add InputMap actions in the project and verify keyboard plus controller mappings. Movement must stop while management UI owns input, release capture on focus loss and restore it deliberately. Walking into walls must collide, camera must not penetrate geometry, and diagonal movement must not be faster. Start with no jump/stairs complexity unless the floor plan requires it. Compare actual travel time to management shortcuts before adding detail.

Create a WorldInteractable contract describing stable target ID, label, availability/reason and an action dispatched through the command boundary. Interaction range is spatial presentation, not a second reward calculator. Reject missing/despawned targets and double activation while a command is running.

### CHAR-001 / CHAR-002: avatar adapter and one master asset


Create proposed CharacterAvatar3D plus visual-profile resources mapping DefinitionId/runtime ID to assets. Missing art gets an honest debug stand-in, not a random hero model. Separate mesh, skeleton, clothing, expressions and morph presentation from gameplay state. Use data/visual_asset_manifest.json or typed Resources for paths; do not scatter hardcoded GLB paths through C#.

Create CharacterLab, MorphLab and AnimationLab only as their features become usable. The first lab must spawn one candidate, rotate/zoom, change lighting and show identity/state binding. Later add animation, outfit and morph controls with numeric values. Use .blend authoring sources under root art/blender and runtime GLB under game assets/3d. Validate scale, bones, animation names, material count and actual blend-shape import. Body/clothing morph mapping is presentation-only; save stable appearance parameters, never mesh internals. Extreme morph combinations and animations must be rendered and checked, not only loaded headlessly.

Follow identity-master and sequential-view workflow. Produce front, profile, back and gameplay-camera renders at each major pass. Toon shading belongs in Godot; choose outlines only after face/hair/distance tests. Do not mark a rough mesh GAMEPLAY_APPROVED.

### AI-001 / WORLD-002: schedules and one smart object


A smart object is a world prop offering an approach point, facing/animation anchor and reservable interaction slot. Build one station first. Reservations are transient and released on success, interruption, scene exit, load/new game and error. Test two actors competing for one slot without overlap or leaked reservation.

Map existing phase and assigned job to logical locations through a small data-driven registry. Nearby NPCs navigate via NavigationRegion3D/NavigationAgent3D; distant NPCs retain logical location only. Navigation failure gets bounded retry and safe fallback. Unloaded or blocked actors must not lose or duplicate settlement income. Introduce effective-job resolution only after auditing existing forced-rest/collapse rules across consumption, output and growth. Do not silently add capacity or phase restrictions to core mechanics.

### EVENT-001 / SAVE-002: complete the first day


Expose existing mentorship and one bond event through 3D interaction, preserving duplicate completion protection and exact existing rewards. Dialogue staging may control camera/look-at/animation but must not duplicate event state. Open existing inventory and management panels from the world; closing them resumes world input safely. Advance phases through GameRoot, derive lighting from the same Calendar, settle once, show report, save, load, and rebind NPCs.

Test the complete path with existing UI and world commands producing equivalent state. Include save/load after assignment, social action and day transition. Select one measured hardware/rendering profile for frame-time, draw-call, triangle, memory and navigation measurements. The initial 60 FPS goal is unverified until measured in the 3D slice.

## Concrete Steps


From repository root, inspect Git and run:

    git status --short
    dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
    python -m unittest discover -s Tools/Godot -p "test_*.py" -v
    node --test Tools/Godot/test_bridge.mjs
    python Tools/Godot/launch.py --mode smoke
    python Tools/Godot/verify_character_audit.py

The smoke command must print USER_DATA_ISOLATION_PASS before tests and end with SMOKE PASS. Never substitute a raw Godot smoke invocation against personal storage. Close the owned runtime before smoke; do not kill another project's processes.

For graphical verification:

    python Tools/Godot/launch.py --mode editor --isolated
    node Tools/Godot/verify_mcp.mjs editor_get_status
    OMR_MCP_ALLOW_CONTROL=1 node Tools/Godot/verify_mcp.mjs editor_run_project
    node Tools/Godot/verify_mcp.mjs runtime_get_status
    node Tools/Godot/verify_mcp.mjs runtime_screenshot

The environment prefix shown is bash syntax. Windows cmd uses `set OMR_MCP_ALLOW_CONTROL=1` before the Node command. Read returned project/currentScene, open the saved PNG and inspect it. When a new feature lands, add its exact launch/navigation and validation steps here; do not claim planned scene paths already exist.

## Validation and Acceptance


Baseline acceptance is build plus safe smoke plus real editor/runtime readback. New world acceptance additionally requires visible motion, collision-aware camera, an NPC reaching the expected station, one assignment/social action changing the shared state, management shortcuts remaining usable, and save/load reconstructing the world without stale references. Two views must not produce two rewards. A failed load must not corrupt the current session.

Art acceptance requires selected references, editable Blender source, verified GLB skeleton/material/morph import, actual gameplay-camera screenshots and animation/clothing checks. Adult-specific presentation has a separate fail-closed identity/design gate; this plan does not grant content approval. Original parity requires cited source formulas and matched fixtures, not a screenshot or a test name containing parity.

## Idempotence and Recovery


Keep changes small and review Git before each milestone. Preserve unrelated dirty worktrees, archives and original source. Stage only scoped verified files; make a coherent commit when explicitly authorized, before risky architecture work. Do not reset the repository to obtain a clean tree. Keep the management UI available until the corresponding world behavior is verified. Disable the opt-in world view to recover from a presentation regression without changing saves.

Profiles and evidence are disposable under .artifacts but are not automatically deleted. Retain evidence for handoff. Do not serialize temporary Godot node paths into save data. For any later schema change, add old/current/future fixtures and a copy-preserving rollback plan before writing real saves.

## Artifacts and Notes


`CURRENT_PROJECT_STATE.md` records inventory and execution. `audit/simulation-map.md` contains precise code anchors; `ADULT_CHARACTER_VALIDATION.md` and `audit/character-metadata.json` preserve source citations/hashes. `KANBAN.md` tracks status; `KNOWN_ISSUES.md` distinguishes defects from unverified risks; `ASTRA_HANDOFF.md` gives the exact next action. Visual assets are tracked separately in `art/ASSET_MANIFEST.md`.

## Interfaces and Dependencies


Use existing Godot .NET, C# services and System.Text.Json. No new game framework, physics engine or event-bus package is needed. Proposed world/character classes in this plan are new work, not existing APIs. Their behavior is constrained by stable ID binding, shared command notifications, cancellation on rebind and no independent settlement. Verify engine API signatures against local 4.7 assemblies/docs before implementation.

Revision 2026-09-05: created from executable-code audit and real baseline checks. The plan adds persistence/notification gates ahead of spatial integration because the inspected services otherwise permit lost flags, stale state and duplicate work rewards.
