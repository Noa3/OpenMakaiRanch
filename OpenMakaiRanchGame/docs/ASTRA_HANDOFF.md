# ASTRA Handoff

Checkpoint **2026-09-11**, verified head **`9d8eb0967012b9c9c4d7d22fe16e96f1b379327c`**. Full isolated Godot CI is green: **1,691 passing checks, zero failed assertions**. This is gameplay/UI integration evidence, not a finished-game or rendered-playtest certification. Canonical docs remain here.

## Branch and requirements

- Repository `Noa3/OpenMakaiRanch`; branch `playability/ranch-quiet-corner-20260910`; PR **#12** against `main`.
- Base `6f4512c7ef50754c7f88cc490564c2ee608534e5` is merged PR #11. Do not assume live PR/branch status from an old chat. Read Git before editing; never force-reset concurrent work or automatically merge.
- **C# 12 only**: effective `LangVersion=12.0`, enforced by Directory.Build.props/targets and CI including a negative preview override. SDK **10.0.401**, primary target **net8.0** and verified Godot **4.7.2 Mono** remain unchanged.
- Save schema stays **16**. D-011 targets fresh games while preserving current-version personal saves; legacy migration expansion is not a development goal. Keep original `eraMakaiRanch-game-eng-translation/` read-only.
- GameRoot and existing services remain the single simulation/calendar/economy authority. No runtime eligibility approvals or adult-specific assets were added.
- Documentation-only commits may follow the verified checkpoint. See the live PR for subsequent exact-head CI status.

## Gameplay slice retained

The physical Community Board opens the existing courier orders, stock and one-delivery-per-day receipt; Pause access remains. Opening it does not deliver or pay automatically.

The optional permanent quiet corner costs **40 G / 3 supplies**, has no upkeep, and switches the existing greybox presentation without growing nodes on refresh/load. Solo recovery restores up to **10 stamina once per in-game day**, capped at daily capacity. Full stamina retains the break; a partial refill consumes it. No phase advance, walking tax, backlog or replacement of the prepared bath's next-morning bonus. Shared Quiet Rest delegates to the existing voluntary activity cost/phase/eligibility rules and does not additionally grant solo recovery.

Personal points need no selected worker and change no job assignments. Root commands retain generation/day/phase, area and combat checks. The panel/dispatcher also check proximity and the captured partner. Live FlagService IDs **1230100** (restored) and **1230101** (last recovery day) are bounded and synchronized at root save. Receipts are written before notifications to prevent repeated/reentrant payment.

The progression walkthrough obtains real Day-3 supplies through Office Work, an explicit Night choice and ordinary settlement before restoring the bench. Starting supplies do not automatically survive Day-1 maintenance. No injected money/stock/day/stamina funds this progression. The bench still has no authored collision, seated pose or sitting animation.

## HUD/menu integration now verified together

The prior continuation added explicit Ranch/Town HUD visibility, PlayerState-based HP (including an empty roster), help/Back coordination, rejection of hidden/inactive world commands, Plan Night, stale binding-capture cancellation, and combat routing that preserves an unresolved encounter and releases the clock after results.

This continuation closes the remaining options layout race. `UiShellController.ViewState` composes the input extension before waiting for the next ProcessFrame, retains pending scroll/logical-focus snapshots across repeated state notifications, restores focus before scroll, and keeps ordinary FollowFocus enabled. Revision/screen/generation/visibility/instance guards reject retired callbacks. Snapshots hold values, not old Control nodes; each frame callback is one-shot.

The ordinary clock fixture is now explicitly a completed-story Day-2 fixture after its separate fresh-story assertions. Previously it activated the mandatory tutorial and then attempted ordinary settlement through that tutorial's input lock. The new guard assertion verifies the intended fixture ownership. Existing runtime guards, night-choice requirements and all old assertions remain. Full first-day and skip-to-night walkthroughs remain separate and pass; do not misreport the fixture correction as a newly fixed production skip bug.

## Executed verification

**Godot 4.7 Mono CI #545**, run **`34538829758`**, head **`9d8eb0967012b9c9c4d7d22fe16e96f1b379327c`**: **success**. It executed C#-12 checks/preview denial, primary-game compilation, launcher regressions, verified engine import and the isolated full suite.

- **1,691 SMOKE OK / zero failed assertions / one terminal SMOKE PASS**.
- Includes all **52 HUD/menu integration checks**, **9 new view-state frame checks**, and one additional ordinary-clock fixture guard. The prior leisure, first-day, input, combat and save checks remain.
- **34 launcher regressions passed**. Compilation is under C# 12, not latest/preview.
- Smoke has **zero out-of-tree transform errors** and five expected malformed/unsupported-save rejection diagnostics.
- Import exits successfully but still logs the separate EditorSettings shutdown message for `export/android/shutdown_adb_on_exit`. Do not describe its log as error-free or suppress the diagnostic.

Artifact **`10176503313`**, `godot-4.7-verification`, SHA-256 **`f17eeae0fa39320f61a97b0763a02203a6b4b1e0ea234d6c440f2e44fc390ef2`**. Smoke `.artifacts/godot/smoke-ohfy9cr3/console.log`; import `.artifacts/godot/import-tcn460wg/console.log`.

Starting head `a45a0bc8d4331ac122c1873947dd8a7b4067088b` had **1,678 OK and three failures** in CI #540 / run `34536254574`: two ordinary settlement assertions and options scroll preservation. That is the actual pre-fix baseline, not the earlier 1,629-check leisure-only result. All three now pass without dropping assertions or relaxing gameplay rules.

A one-use branch-scoped workflow applied SHA-checked edits to three large existing files, used explicit staged paths and a non-force push, and removed itself in commit `b2d5f01`. It is absent from the final source tree. The earlier leisure compile/fixture iterations remain in Git history and the previous checkpoint documentation.

## Next work and limits

See `HUD_MENU_VALIDATION.md` for exact UI coverage. Opening fifteen ordinary routes is not proof that every action on every menu has been exercised. Interface tests use explicitly synthetic Day-2 state; do not conflate them with the separate production-backed walkthrough. Buttons are activated by live signals and positions near world interactions are staged.

Next perform rendered isolated acceptance from the real Main Menu and character creation: ordinary first day and shortcut, world travel, menu routing, help/Back, tactical results, night/report, physical board, supply planning, construction, recovery and save/load. Check narrow-window hit targets/text and keyboard/gamepad/touch focus, not only headless node state. Author and test walkable collision/navmeshes before calling routes accessible; benchmark low/high Forward+ on representative hardware. Long-term balance, final character assets and bench presentation remain open. Prefer resolving demonstrated friction to adding more mandatory daily systems.

## Commands and safety

```bash
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python -m unittest discover -s Tools/Godot -p "test_*.py"
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/launch.py --mode runtime --isolated
```

Smoke overwrites/deletes disposable slot 99. Always use the isolated launcher, never a raw smoke flag against personal saves. Older KANBAN/WORK_LOG/PLAYABILITY checkpoints describe historical scopes and counts; use this handoff and current PR evidence for the latest verification.
