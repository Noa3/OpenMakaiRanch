# Simulation architecture evidence map: additive 3D migration

## Scope and confidence

Read-only inspection of the current project/source under `E:/OpenMakaiRanch/OpenMakaiRanchGame`; this report is the only requested project write. No build, tests, game/editor launch, save access, Git mutation, or archive/worktree changes were performed. Evidence paths below are relative to `OpenMakaiRanchGame/`, except the explicitly named root `AGENTS.md`. Line numbers refer to the inspected working files, not a commit-pinned snapshot.

**Implemented** means visible executable source, not successful execution. **Test present** means an assertion was inspected, not that it passes. No runtime or original-game parity is certified by this audit. Root `AGENTS.md:14-16` describes seeded data and schema 11; actual source uses JSON-first loading and schema 14 (`src/Data/DataRegistry.cs:29-85`; `src/Core/Models/SaveModels.cs:75-99`). Existing planning claims are not used as implementation evidence.

**Overall:** an additive 3D presentation can reuse the existing stateful services. A physical-work simulation that changes rewards is not an existing adapter and cannot safely be bolted onto animation callbacks. The present authority is a synchronous, whole-day settlement over mutable shared state.

## 1. Exact startup and scene ownership

1. `project.godot:13-22` selects `res://scenes/Bootstrap.tscn`, then declares autoloads `GameRoot`, `SceneRouter`, and the MCP runtime bridge. Bootstrap is a `Control` with `BootstrapController` (`scenes/Bootstrap.tscn:3-12`).
2. `GameRoot._Ready` sets the singleton, creates/adds feedback, loads the registry, creates a fresh in-memory save, loads local settings, synchronizes feedback, and builds services (`src/App/GameRoot.cs:64-74`). There is no automatic Continue/load here.
3. Export and smoke-test switches are checked **after** ordinary state/service initialization. Export exits directly; smoke tests are deferred and exit using their result (`src/App/GameRoot.cs:75-98`). Test mode is not an isolated simulation boot path.
4. Bootstrap defers `ChangeSceneToFile("res://scenes/MainMenu.tscn")` (`src/App/BootstrapController.cs:10-22`). Menu Continue tries slot 1, then slot 0; New Game calls `NewGame`, sets `PendingInitialScreen = "character_creation"`, and switches to `Game.tscn` (`src/App/MainMenuController.cs:195-218,231-237`). New Game+ specifically loads slot 1 before carryover (`:220-228`).
5. `Game.tscn` is a `Control` host plus a `Control` UI shell, initial screen `ranch`; its full-screen Canvas is opaque (`scenes/Game.tscn:3-33`). `GameSceneController` only checks that the UI shell exists (`src/App/GameSceneController.cs:9-20`).
6. UI shell holds the persistent `GameRoot`, subscribes to `StateChanged`/`GameComplete`, registers with the router, consumes the pending screen, and unsubscribes/unregisters on exit (`src/Ui/UiShellController.cs:164-215`). The router switches **shell screen IDs**, not world scenes; absent shell means warning/no-op (`src/App/SceneRouter.cs:33-55`).

**3D seam:** introduce a world view alongside the shell while retaining autoload authority and existing menu/Continue flow. Opaque UI canvas/layout needs an explicit overlay or viewport policy. A world scene must not create a second `GameRoot` or advance days independently.

## 2. Composition and rebinding

`GameRoot.BuildServices` is the composition root (`src/App/GameRoot.cs:565-598`):

| Construction order / dependency | Evidence |
|---|---|
| Roster, Schedule, Equipment, Clothing, Talents receive current `State` and `Data` | `:567-571` |
| Ranch receives current state/data plus Equipment and Talents | `:572` |
| Economy and Inventory receive state; Milestones receives state/data/Economy; Shop receives data/Economy/Inventory | `:573-576` |
| Adventure receives state/data/Economy/Inventory/Milestones plus RNG seeded from day, roster count and gold | `:577-578` |
| Bond receives state/data/Milestones; Recruitment receives state/data/Economy and immediately ensures an offer | `:579-581` |
| Research, Pets, Training, MentalState, EnhancedTraining, Visit, MilkEconomy, Addiction, Combat, Discovery, Mercenary, WinCondition are recreated | `:582-593` |
| Flags is recreated and copied from save storage; transient combat phase/round reset | `:594-598` |

`Data`, `Save`, settings storage, feedback, Town and the content hook are not recreated by `BuildServices` (`src/App/GameRoot.cs:24-57,565-598`). DayCycle and DailySettlement are not long-lived root properties: actions construct them against the current state (`:384-418`). Settlement also constructs its own event/growth/resource/production helpers (`src/Gameplay/DailySettlementService.cs:24-38`).

### State replacement paths

- **NewGame:** clone current settings → fresh factory state → restore settings → clear transient reports → feedback sync → `BuildServices` → initialize character magic → `StateChanged` (`src/App/GameRoot.cs:100-110`).
- **LoadSlot:** deserialize/migrate first; failure returns without replacing root state. Success replaces state → overrides saved settings with local settings file → clears transient reports → feedback sync → `BuildServices` → character initialization → `StateChanged` (`:290-306`). Historical `State.Reports` remains loaded even though `LastDailyReport` is cleared.
- **NewGame+:** creates fresh state, transfers selected progression, then performs the same rebuild/reset/notification. Player and carried pet entries are assigned by reference rather than cloned (`:113-194`, especially `:174-187`).
- Recruitment rebinding is not pure: `EnsureOffer` generates an offer when absent (`src/Gameplay/ManagementServices.cs:541-558`). Initial factory generation defaults to `Random.Shared` unless injected (`src/Gameplay/SaveStateFactory.cs:24-28`).

**Adapter rule:** keep stable IDs and resolve current services/state from root after replacement. Captured service or `CharacterState` references will continue mutating old state: Schedule and Ranch store readonly state references (`src/Gameplay/ScheduleService.cs:10-17`; `src/Gameplay/RanchService.cs:12-23`). There is no dedicated state-generation/rebound event, only general `StateChanged`.

**Notification gap:** direct Schedule/Bond calls do not emit root events. UI invokes them through `ExecuteUiAction(..., false)` (`src/Ui/UiShellController.Screens.cs:872,1569,1619`), whose fallback refreshes only the current shell (`src/Ui/UiShellController.cs:648-680,683-719`). A 3D subscriber listening only to root events can therefore miss UI-originated assignments/social changes. A shared command/notification boundary is needed before two active views coexist.

## 3. Schedule and day settlement authority

### There is no `GetEffectiveJobId` in this checkout

`ScheduleService` contains only constructor, `AssignableJobs`, `GetAssignment`, and `AssignJob` (`src/Gameplay/ScheduleService.cs:8-35`). Full-source searches for `GetEffectiveJobId`, then broader `GetEffective`/physical-adapter terms, did not identify an implementation. Do not design against a presumed existing method.

- `GetAssignment` falls back to `rest` only for missing dictionary keys (`:21-24`).
- `AssignJob` validates job existence, but not roster membership, assignability, phase, facility capacity, actor reachability, or work completion (`:26-34`).
- `ScheduleState` is just character-ID → job-ID (`src/Core/Models/SaveModels.cs:246-249`), not a phase schedule or task queue.
- Actual effective work is distributed: consumption force-writes `rest` for fatigue >=70; settlement resolves unknown jobs to the rest definition; Ranch blocks collapsed workers; growth independently reads raw assignments (`src/Gameplay/LifecycleServices.cs:186-236,118-139`; `src/Gameplay/DailySettlementService.cs:49-60`; `src/Gameplay/RanchService.cs:90-102`).

### Clock and exact settlement order

`AdvanceTime` advances Morning → Afternoon → Evening → Night, emitting `StateChanged`; trying to advance from Night calls `EndDay` (`src/App/GameRoot.cs:384-395`; `src/Gameplay/DayCycleService.cs:35-49`). This is not elapsed-time simulation.

`SettleDay` executes synchronously in this order (`src/Gameplay/DailySettlementService.cs:41-122`):

1. Record outgoing day. Consume meals for raw non-rest assignments; consume facility supplies; permanently overwrite exhausted workers' assignments to `rest` (`src/Gameplay/LifecycleServices.cs:186-236`).
2. Apply selected night action, then clear it (`src/Gameplay/DailySettlementService.cs:47,129-170`). Missing/unrecognized action falls through to rest.
3. For every roster entry, resolve assignment/job, apply Ranch output, then independently apply fatigue/morale/bond deltas (`:49-61`). A blocked output does **not** suppress those deltas.
4. Compute facility and pet upkeep; penalize missing raw `dairy` assignment; apply income/expenses and initialize report totals (`:63-82`). The dairy-worker check does not check successful output or collapse.
5. Apply ranch automation; run production/shipping and add that revenue to report net (`:84-97`).
6. Generate daily events; apply growth; check milestones (`:99-101`).
7. Increment calendar, set Morning, roll weather, reset daily training count (`:102`; `src/Gameplay/DayCycleService.cs:15-32`).
8. Discover a mission on eligible **new** days; clear mercenary availability and active bonus (`src/Gameplay/DailySettlementService.cs:104-120`).
9. Root replaces any report for the outgoing day, appends report, emits `DaySettled`, then `StateChanged`, then checks victory/sets `VictoryDay` on the new day/emits `GameComplete` (`src/App/GameRoot.cs:404-418`).

**Important behavioral hazards:**

- `EndDay` and `SettleDay` have no phase prerequisite, transaction boundary, duplicate-command token, or rollback. Repeated calls settle successive days; report deduplication is not an execution guard (`src/App/GameRoot.cs:404-418`; `src/Gameplay/DailySettlementService.cs:41-122`).
- Night training calls whole-roster `ApplyGrowth` *inside* the roster loop, and ordinary growth runs again later. Growth itself loops over every character (`src/Gameplay/DailySettlementService.cs:134-143,100`; `src/Gameplay/LifecycleServices.cs:118-139`). This is a source-visible multiplicative-growth risk, not a verified intended parity rule.
- Auto-rest occurs before night recovery, overwrites player intent, and meal consumption occurs before that reassignment (`src/Gameplay/LifecycleServices.cs:186-236`). Simply reproducing `GetAssignment` visually will disagree with later effective work.
- Reports are not an authoritative gold ledger: event gold changes happen after `NetGold` is initialized and are stored separately in events (`src/Gameplay/DailySettlementService.cs:78-101`; `src/Gameplay/LifecycleServices.cs:43-55,68-83`).
- Day events seed RNG from day/roster count; weather uses `Random.Shared`; a seeded factory is optional (`src/Gameplay/LifecycleServices.cs:24-29`; `src/Gameplay/DayCycleService.cs:23-32`; `src/Gameplay/SaveStateFactory.cs:24-28`). Full replay determinism is not implemented.

## 4. Physical-work adapter constraints

**Existing computation:** `RanchService.ApplyJobOutput` immediately changes stockpile and returns gold to its caller; it is not a pure preview and does not itself credit the returned job gold (`src/Gameplay/RanchService.cs:90-197`). It handles resource-less work/rest, collapse, fatigue multipliers, equipment/talent skill bonuses and research effects. Settlement separately credits gold and applies worker deltas (`src/Gameplay/DailySettlementService.cs:49-81`).

**No spatial contract:** job definitions contain IDs/category/resource/reward/stat deltas/assignability, not a workstation or duration (`src/Core/Resources/GameDefinitions.cs:82-94`). Ranch state stores facility levels, stockpile and scalar maintenance values, not building instances/transforms (`src/Core/Models/SaveModels.cs:180-187`). Character state has IDs, stats and appearance metadata, not actor position/path/task progress (`:194-244`). Searches across `src/` found no `Node3D`, `CharacterBody3D`, `NavigationAgent3D`, or physical-work adapter implementation.

`FacilityDefinition` exposes capacity/output fields (`src/Core/Resources/GameDefinitions.cs:223-232`), but Ranch upgrade/output/upkeep code does not use capacity as a work gate (`src/Gameplay/RanchService.cs:28-70,90-197`). A visual workstation-capacity rule would add a new gameplay restriction, not preserve a demonstrated existing one.

**Safe additive baseline (recommendation, not implemented):**

- Keep settlement the sole reward authority. Render assignments/phase/report outcomes with actors; do not call `ApplyJobOutput` on animation loops, arrival, or interaction completion.
- Map persistent character/facility IDs to transient scene nodes. Rebuild mappings after state replacement; cancel in-flight callbacks before rebinding.
- Choose explicit semantics before physical travel affects output: intent vs effective job, reservation/capacity, failed paths, interruptions, unloaded actors, partial work, and day-boundary completion.
- If physical work becomes authoritative, introduce a bounded work-result/commit protocol consumed once by settlement, rather than adding a second reward path. Centralize effective-job resolution across consumption, output, growth, maintenance checks and visual tasks.
- Keep existing menu/schedule controls usable as a regression/reference view. No persisted world schema is necessary for a purely derived presentation; authoritative placements/progress require a deliberate versioned schema.

## 5. Social events

Bond is a stateful service, not an actor interaction system. Available events match **runtime character ID** exactly, require sufficient bond, exclude completed event IDs, and sort by threshold (`src/Gameplay/ManagementServices.cs:443-455`). It does not use `DefinitionId` to inherit a template's events. Generated actors therefore do not automatically acquire template-authored events.

Mentorship immediately modifies bond/morale/fatigue and checks milestones; it has no phase/time-cost, proximity, cooldown, or per-day guard (`:457-475`). Event completion validates definition, duplicate completion, character existence and threshold, applies rewards, records the event ID, then checks milestones (`:477-500`). This gives duplicate-reward protection for completed bond events, unlike repeatable mentorship. Save stores completed IDs only (`src/Core/Models/SaveModels.cs:314-317`), not an in-progress conversation.

Daily events are separate settlement-time random table outcomes that directly modify economy/inventory/roster and append report entries (`src/Gameplay/LifecycleServices.cs:22-104`). They have no spatial trigger, actor identity, choice state, or persistent event-instance ID (`src/Core/Models/SaveModels.cs:360-368`). A 3D dialogue/event presenter should invoke existing completion once and display results, not assume the current services manage staging or scene duration.

## 6. Persistence and migration

- Current schema is **14**, with calendar/economy/ranch/roster/schedule/inventory/adventure/progression/social/settings/player/report/flag state (`src/Core/Models/SaveModels.cs:75-99`). Calendar persists phase/weather/night action/training count; season is derived and ignored by JSON (`:159-168`).
- `SaveService` uses indented System.Text.Json, string enums, a legacy equipment converter, depth 32, and a 4 MiB size ceiling. It writes `SavedAt`, serializes, writes a `.tmp`, then overwrites the destination via `File.Move` (`src/Gameplay/SaveService.cs:14-50`). Directory creation and serialization are before the write try/catch; a failed write may still change the in-memory timestamp. No backup or concurrent-save arbitration is visible here.
- Slots map directly to `user://saves/slot{slot}.json`; Delete removes that exact path (`:141-152`). No test root or slot range is injected. Metadata parsing is separate from full deserialization/migration (`:56-92`).
- Load enforces size, deserializes, migrates, and rejects schema not equal to current; exceptions become null load (`:95-138`). Root settings are subsequently replaced from `user://settings.json`, so raw serializer roundtrip and application LoadSlot intentionally differ (`src/App/GameRoot.cs:298-305`; `src/App/SettingsStorage.cs:16-32`).
- Migration increments versions 1→13 largely without per-version transformations; 13→14 converts portrait-related indices, then normalizes many nullable structures (`src/Gameplay/SaveMigrator.cs:10-148`). **Ordering defect:** version-13 conversion accesses `state.Roster.Characters` before either is repaired (`:75-88,104`). An older save with explicitly null roster/list can therefore fail instead of normalizing.
- Normalization does not cover `Reports` or `Flags` in the inspected migrator (`src/Gameplay/SaveMigrator.cs:85-165`). Explicit null Flags may deserialize successfully then fail root service composition; null Reports may later fail EndDay (`src/App/GameRoot.cs:409-410,594-595`). Missing properties and explicit null are different cases.
- **Flag persistence gap:** Flags copies storage into private dictionaries (`src/Gameplay/FlagService.cs:47-67`) and offers `SyncToStorage` (`:70-90`), but source search finds no caller. Root SaveSlot directly serializes state without synchronizing (`src/App/GameRoot.cs:283-287`). Changes through FlagService can be lost on save/rebind. Do not use it as authoritative persistent world-interaction state until the write path is repaired and tested.
- Reports are appended without retention cap (`src/App/GameRoot.cs:404-418`). Added world history must consider the existing save-size ceiling.

## 7. Tests present versus verified/parity

**No tests were executed.** The smoke suite is a custom Godot-in-process runner, selected by `--run-smoke-tests`, with a single outer exception catch and terminal PASS/FAIL line (`src/Tests/SmokeTestRunner.cs:20-68`). A test name containing “parity” is not evidence of comparison against the original engine.

| Area | Inspected assertions | What remains unproven |
|---|---|---|
| Day settlement | Day increment, nonzero goods, changed gold, report lines, milestone (`src/Tests/SmokeTestRunner.cs:80-100`) | Exact settlement ledger/order, duplicate commands, nightly growth multiplicity, physical work |
| Scheduling | Default rest, known assignment, rejected unknown job (`:633-660`) | Effective-job resolver, invalid character IDs, facilities, phase/physical eligibility |
| Social/output | Starting event completion/persisted ID/bond reward; research raises output (`:528-567`) | Duplicate/threshold rejection suite, generated-ID story mapping, spatial dialogue lifecycle |
| Service wiring | Manually constructs services and settles several days (`:663-691`) | Actual `GameRoot.BuildServices` rebinding after NewGame/Load; stale adapter references |
| Save | Real slot 99 roundtrip for selected settings/phase/gold/generated metadata/offer (`:191-214`); constructed v0/v10 migrations (`:874-888`) | Isolated filesystem, old JSON fixtures across all versions, null roster/reports/flags, future-schema handling, interrupted writes |
| New Game+ | Test manually copies a subset into a fresh factory state (`:791-826`) | It does not call `GameRoot.StartNewGamePlus`; production carryover/rebinding is not directly covered |
| Report history | Test manually repeats root-style append logic (`:694-721`) | Actual root events/history ordering, save-history roundtrip and size growth |
| Fatigue/collapse | Direct Ranch output reduction and collapse rejection (`:974-1035`) | Actual forced-rest settlement sequence; rest test computes a delta rather than settling it |
| UI | Instantiates Game scene and calls screen render methods (`:306-363`) | Interaction actions, visual correctness, 3D input/layout, state synchronization across two views |
| Named parity checks | Selected prerequisite and daily-cap/night-rest assertions (`:1043-1163`) | No original-engine fixture replay/differential comparison is performed there |

**Test safety blocker:** SaveRoundTrip overwrites slot 99 and then deletes it without preserving any previous file; deletion is not in a finally (`src/Tests/SmokeTestRunner.cs:202-214`). Combined with hardwired `user://` and normal startup, running the suite against a user's profile can destroy an existing slot 99. UI walk additionally attaches scenes to the live root and reads application state (`:308-345`). Before execution, use a separately verified disposable user-data location or add injected test storage and restoration. Do not infer that a high slot number is safe.

## 8. Prioritized blockers and acceptance gates

1. **P0 — Single simulation authority / missing physical-work contract.** No `GetEffectiveJobId`; split assignment rules and immediate mutable output invite double rewards. Gate: one effective-work decision and exactly-once day/work commit; or explicitly presentation-only 3D.
2. **P0 — State replacement and notifications.** Rebuilt services retain different save instances; direct UI service calls refresh only shell. Gate: rebinding generation/cancellation policy and shared mutation notifications, tested through actual root NewGame and LoadSlot with a live world adapter.
3. **P0 — Safe test execution.** Slot 99 overwrite/delete under ordinary user storage. Gate: isolated test paths verified before any smoke run; preserve existing user data.
4. **P1 — Persistence robustness.** FlagService sync omission, migration null-ordering, unnormalized Reports/Flags, no spatial schema. Gate: real historical/null/future-schema fixtures and root-load tests; version any authoritative world additions.
5. **P1 — Day semantics and replay.** Night growth multiplication, permanent auto-rest, unrestricted EndDay, partial report ledger and mixed RNG. Gate: specify intended behavior and assert exact outcomes before letting travel/animation affect it.
6. **P1 — Spatial/social modeling.** Facility levels are not instances/reservations; social actions lack world conditions and generated-ID mapping. Gate: explicit actor/workstation/event mapping with missing-asset/path fallbacks that preserve existing economy.
7. **P2 — Presentation integration.** Screen router is shell-only and canvas opaque. Gate: world/overlay input and visibility contract while retaining current 2D management paths.

These gates are recommendations inferred from inspected source, not changes made or acceptance tests already passed. No original-game completeness percentage or parity certification is justified by this inspection.
