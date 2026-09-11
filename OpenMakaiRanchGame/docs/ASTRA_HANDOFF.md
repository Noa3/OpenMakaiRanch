# ASTRA Handoff

Checkpoint **2026-09-11**, verified code-inclusive head **`897871eadbdf02c19d905c6e0b33e5edf335e649`**. **1,691 smoke assertions, 207 rendered UI assertions and 48 launcher/evidence tests pass.** The rendered run produced 31 viewport PNGs. This is bounded gameplay/interface evidence, not a finished-game or physical-device certification. Documentation-only commits may follow; the live PR records subsequent exact-head results.

## Branch and requirements

- Repository `Noa3/OpenMakaiRanch`; branch `playability/ranch-quiet-corner-20260910`; PR **#12** against `main`. Base `6f4512c7ef50754c7f88cc490564c2ee608534e5` is merged PR #11. Read live Git before editing; never force-reset concurrent work or automatically merge.
- **C# 12 only**, effective `LangVersion=12.0`, enforced by Directory.Build.props/targets and CI including preview-override rejection. SDK **10.0.401**, primary target **net8.0**, engine **Godot 4.7.2 Mono**. The rendered Linux job installs the .NET 8 runtime alongside the pinned SDK; this is not a project framework/engine upgrade.
- Save schema **16**. D-011 targets fresh games while preserving current-version personal saves; expanding legacy migration is not a development goal. Keep `eraMakaiRanch-game-eng-translation/` read-only.
- GameRoot and existing services remain the sole calendar/economy/progression authority. No character eligibility approvals, identity changes or adult-specific assets were added.

## Gameplay slice retained

The physical Community Board and Pause entry share courier stock, rewards and one-delivery-per-day receipts. Opening the board does not deliver or pay. The permanent quiet corner costs **40 G / 3 supplies** once, has no upkeep, and updates its existing greybox presentation without accumulating nodes. Solo recovery restores up to **10 stamina once per day**, capped at current daily capacity. Full stamina preserves the break; partial recovery uses it. No walking tax, phase advance, backlog or replacement of the prepared bath's next-morning bonus. Shared Quiet Rest delegates to the existing voluntary activity rules and does not also grant solo recovery.

Personal points need no selected worker or new job assignment. Commands retain generation/day/phase, area, combat, proximity and captured-partner checks. FlagService receipts **1230100** (restored) and **1230101** (last recovery day) are bounded, saved together and written before notifications. The existing resource-backed walkthrough obtains real Day-3 Office Work supplies after Day-1 upkeep and an explicit Night choice; it does not inject transaction funding. The bench still lacks authored collision, seated pose and sitting animation.

## Latest interface work

The previous HUD/menu integration remains: active-area CanvasLayer ownership, canonical player HP including empty rosters, help/Back coordination, rejection of hidden world callbacks, Plan Night, safe binding capture, retained unresolved tactical combat and results/clock release. Pending scroll/focus snapshots remain value-only, revision/generation/visibility guarded and one-shot.

The rendered continuation repairs a disposed MainMenu event subscription on scene exit; uses readable small-window canvas dimensions while preserving HiDPI/ultrawide behavior and independent user scale; wraps management header rows; adapts main-menu/creation minimum widths and creation grids; preserves wrapped-label height; and bounds binding-button captions. Escape and controller Back cancel either capture device even during arming. Removed/reset binding cards cannot re-arm.

Compact Ranch/Town HUDs separate summary/actions, workers and warnings. Optional detail is compacted, not marked completed. Full resource, tutorial and alert detail remains in bounded scrollable Help. The interaction strip reflows and hides duplicate legacy prompts after area binding. Existing work/facility gates are unchanged.

Options Scale Up/Down now call the single central SetUiScale authority, without an additional panel transform or duplicate notification. Short logical window heights use the denser header and hide the decorative title in compact mode, retaining space to reach Scale Down. Twelve actual scale-button clicks cover both existing 0.85–1.35 limits, caption values, single scaling, pre-click visibility, panel bounds and return to the world.

## Executed verification

All three workflows passed on **`897871eadbdf02c19d905c6e0b33e5edf335e649`**:

- **Godot CI #563 / `34546801277`**: C# 12 and preview denial, compilation, launcher tests, verified import and isolated full smoke. **1,691 OK, zero failed assertions, one SMOKE PASS**. Retains the 52 HUD/menu and nine view-state checks alongside first-day, leisure, combat, save and input coverage.
- **Build #571 / `34546801316`**: success.
- **Rendered UI #17 / `34546801281`**: **207 OK, zero failed assertions, one UI ACCEPTANCE PASS, 31 PNGs**, using Mesa/Xvfb OpenGL Compatibility. Actual viewport mouse/key/standardized joypad events; not physical controllers. Windows/Linux profile isolation and evidence rejection have **48 Python tests** (34 prior plus 14 new); the local rerun also passed all 48.

Both artifacts were independently downloaded and their SHA-256 checked:

| Evidence | Artifact ID | SHA-256 |
| --- | --- | --- |
| Godot import/smoke | `10179353049` | `932387a80b5844ec3e29600cc71034e85932dcd79da7b8b1a920067bcacc060a` |
| Rendered UI/source/screens | `10179361232` | `671af1cfafa35895ef8fcf634170af9de5db59924706fcea7c57c43ebae2f92a` |

Smoke `.artifacts/godot/smoke-b9q6dgw7/console.log`: zero out-of-tree transform errors and five intentional invalid-save diagnostics. Import `.artifacts/godot/import-ke9xntab/console.log`: successful exit, but the known EditorSettings shutdown diagnostic remains. UI evidence `godot/ui-ly84_15e/` inside its artifact: no runtime ERROR/SCRIPT ERROR, retained unsupported-VSync driver warning. Do not describe all logs as warning-free. Rendered artifacts expire after 14 days; regenerate rather than invent missing evidence.

## Fixture boundaries and next work

`UI_LAYOUT_ACCEPTANCE.md` records the failed baselines and fixes. A doubled focus-scroll request and the mistaken expectation of an active unbuilt Dairy Barn button were fixture errors. The former extra request was removed; the latter now has an explicit negative control followed by the built Pasture. The high-scale unreachable Scale Down button was a real layout defect. Original assertions/gates were retained and coverage strengthened.

The rendered suite proves MainMenu entry, name typing/focus, selected creation layouts, menus, scale controls, help and staged world interaction presentation. Its ordinary Day-2 state is explicitly synthetic, not earned progression. It does not exercise every character picker, every menu action or the entire rendered first-day/combat/leisure/save journey. The separate resource-backed smoke walkthrough remains.

Next prioritize an isolated full rendered playthrough and physical mouse/controller/touch/Alt-Tab acceptance, then authored collision and obstacle-aware navigation. The current greybox models, route traversal, final art/animation, weather readability, representative-hardware Forward+ low/high performance and long-term balance remain open. Prefer fixing demonstrated friction over adding mandatory daily systems. See KNOWN_ISSUES for earlier unreproduced audit leads.

## Commands and safety

```bash
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python -m unittest discover -s Tools/Godot -p "test_*.py"
python Tools/Godot/launch.py --mode import
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/ui_acceptance.py --rendered
python Tools/Godot/launch.py --mode runtime --isolated
```

Smoke overwrites/deletes disposable slot 99: use the isolated launcher, never the raw smoke flag on personal saves. UI acceptance requires its own verified disposable profile and debug-only dev scene. Its CI is read-only and uploads only whitelisted evidence/source, not saves. Temporary exact-hash, branch-scoped patch helpers removed themselves after non-force commits; no automatic merge was used. Older KANBAN/WORK_LOG/PLAYABILITY snapshots are historical, not current verification.
