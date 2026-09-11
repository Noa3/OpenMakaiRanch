# Core Playability Validation

Checkpoint: **2026-09-11**. Verified code head: **`2a958b1c4425e31acd48cf5f604cdda378fc4846`**. Branch: `fix/daily-gameplay-loop-20260911`, PR #13. Documentation-only commits may follow this exact tested code revision. This extends the night/report checkpoint in DAY_LOOP_VALIDATION.md; it does not replace its evidence or certify the whole remake.

## Player-facing repairs

Legacy Adventure, Combat, Shop, Schedule, Research and Milestones renderers appended sequential controls directly to PanelContainer. Those controls were assigned overlapping rectangles. UiShellController.CardLayout now gives these explicitly scoped cards one vertical layout owner during composition. Existing nodes, order, button subscriptions, logical focus keys and correctly authored single-content cards are preserved. Recursive composition is bottom-up and idempotent. Other screens and intentional visual overlays are not globally rewritten.

The new rendered mission journey exposed a separate defect at `6ae54bf`: Auto Finish disappeared at battle completion, the replacement Back button received focus, but restoring an unrelated old log position left it outside the viewport. View restoration now reveals a newly selected fallback command after layout. An exact logical-key match still preserves the user's manual scroll, including deliberate scrolling away from that same control. Existing session/revision/route/visibility guards remain.

The guild has compact readiness guidance using the canonical stamina service and party selection: current/capacity stamina, ordinary mission entry cost, automatic-all or explicitly selected party, and free preparation. Detailed help distinguishes daily stamina from combat HP/SP/MP, the owner's tactical participation, automatic resolution and returning from results. This adds no resource authority or new combat balance formula.

## Executed rendered journeys

The existing scenario retains its initial 240 assertions, including night-plan revisions, prepared bathing, real End Day, wallet/report reconciliation and save/load. The continuation adds **76 checks and nine screenshots**:

1. Guild layout at 640x480 and 960x540; compact readiness card; one layout owner per card; non-overlapping vertical content and no horizontal overflow.
2. Actual viewport clicks on Road Patrol Fight, Tactical Battle, Defend, Attack, Auto Finish, results Back and Return to World. The command checks identify the player's actions, not an unrelated companion action.
3. Preparing a mission is free; entry charges stamina once; visible and shared time controls are blocked during battle; turns and result redraws do not charge again, repeat rewards or advance the day. The newly focused results Back is physically visible before the test refocuses it. Returning releases the encounter/time lock and restores world input.
4. Current-schema root save/load retains mission identity, wallet, daily stamina and roster HP/energy without resurrecting a phantom encounter.
5. Actual Meal Box purchase charges its listed price once and adds one item. An actual worker-job button changes the canonical schedule without paying production early. Milestone cards are inspected without changing time or stamina.
6. Actual Slot 3 Save, a second real unsaved purchase, then actual Slot 3 Load restore wallet, inventory, job assignment, day and stamina through the session replacement boundary.

This is an explicitly synthetic Day-2 interface fixture continued through settlement into Day 3. These new journeys do not inject additional money, inventory or combat boosts, but inherit the scenario's existing synthetic resources and strong character stats. They are **not an organically earned new-game playthrough, a combat-difficulty test or long-term economic balancing evidence**. Existing resource-backed first-day/leisure smoke walkthroughs remain separate.

## Exact-head CI results

All three existing workflows completed successfully on `2a958b1c4425e31acd48cf5f604cdda378fc4846`:

| Workflow | Run | Result |
| --- | --- | --- |
| Godot 4.7 Mono CI #591 | `34587496873` | success |
| Build Smoke Check #599 | `34587496874` | success |
| Rendered UI acceptance #34 | `34587496917` | success |

The Godot Mono job records **48 Python tests passing**, **build success with zero warnings and zero errors**, and **1,731 SMOKE OK / zero SMOKE FAIL / one SMOKE PASS**. The rendered result records **316/316 checks passing, 43 PNG captures and zero runtime ERROR/SCRIPT ERROR**. C# 12, SDK 10.0.401, net8.0, Godot 4.7.2 Mono, save schema 16, original read-only reference data and existing CI workflows remain unchanged.

Three additional headless view-state checks cover manual scroll preservation, a genuinely clipped replacement fixture and revealing its fallback. An intermediate assertion wrongly assumed Keyboard_interact was clipped on the large headless viewport. Diagnostic geometry proved it was already visible at scroll zero. The final fixture selects the last keyboard binding and explicitly proves clipping before requiring automatic scrolling; it does not weaken the visibility assertion. The final Keyboard_pause_menu fallback scrolls to 676 and fits fully inside the content viewport.

## Evidence receipts and inspection

Downloaded ZIP bytes were independently SHA-256 checked:

| Evidence | Artifact ID | SHA-256 |
| --- | --- | --- |
| Godot import/smoke | `10194262176` | `576c5c344957b472ed671b0a52318edd84395a17360a34bc1687e83a9a2788fd` |
| Rendered UI/source/screens | `10194281155` | `7a36cebde5371c8e952c7f862a04d9aa4fcc685889e80f0585e938a11ff199e8` |

Final smoke console: `.artifacts/godot/smoke-3_kdmwm8/console.log`. Final rendered evidence: `godot/ui-pqpuw9zk/`, including results.json and console.log. Guild, player-turn, results, store, schedule and save/load PNGs were opened and visually inspected. In particular, the results screenshot shows the full Back hit target and the guild screenshot confirms that the compact guide no longer occupies the whole small viewport. The included source archive is a whitelisted review subset, **not an exported game or complete standalone project**. Download expiring artifacts or rerun the scenario when evidence is unavailable.

Five invalid-save rejection ERROR diagnostics are intentional smoke fixtures. Engine import still exits successfully with the known EditorSettings shutdown diagnostic; the software-rendered runner retains its unsupported-VSync warning. No new log filtering hides these diagnostics. The clean rendered scenario does not submit invalid saves and rejects runtime errors. This does not certify all ordinary engine shutdown paths.

## Scope and safe reproduction

Changed runtime files: `src/Ui/UiShellController.CardLayout.cs`, `src/Ui/UiShellController.ViewState.cs`. Changed/added tests: `src/Tests/UiLayoutAcceptance.Adventure.cs`, `UiLayoutAcceptance.Management.cs`, `UiLayoutAcceptance.Night.cs`, `HudViewStateFrameTests.cs`.

```bash
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python -m unittest discover -s Tools/Godot -p "test_*.py"
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/ui_acceptance.py --rendered
python Tools/Godot/launch.py --mode runtime --isolated
```

Use the isolated launchers. The opt-in UI suite writes/deletes disposable slots **99 and 3**; it first validates profile isolation and refuses an occupied Slot 3. Never run raw acceptance flags against personal saves. No production character state is meant to be reset by these fixtures.

Remaining gaps include an uninterrupted organically earned first-day/skip/world/combat/courier/leisure/save route, all dungeon/mission outcomes, full Research action coverage, authored collision/navmeshes, physical input-device acceptance, final visual assets, representative-hardware Forward+ performance, long-term stamina/economic balance and original-engine differential parity. See KNOWN_ISSUES.md; passing this bounded checkpoint is not completion of those tasks.
