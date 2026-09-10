# ASTRA Handoff

Checkpoint **2026-09-10**, verified code **`44138f3c9da33dc8a234fb9a13d4701ce7d42b68`**. This is a tested optional gameplay slice, not a release, rendered-playtest or complete-parity certification. Canonical documents remain in `OpenMakaiRanchGame/docs/`.

## Current branch and contract

- Repository `Noa3/OpenMakaiRanch`; branch `playability/ranch-quiet-corner-20260910`; **PR #12** against `main`.
- Base `6f4512c7ef50754c7f88cc490564c2ee608534e5`, the now-merged PR #11. Its movement/camera/input fixes and 1,547-check baseline are retained.
- **C# 12 only**, effective `LangVersion=12.0`, enforced by root Directory.Build.props/targets and CI. Do not switch to latest/preview to fix errors. D-012 remains in force.
- Existing SDK **10.0.401**, primary target **net8.0**, Godot package configuration and verified engine **4.7.2 Mono** are unchanged. Save schema stays **16**.
- D-011: fresh games are the development target. Preserve current-version saves; do not add legacy compatibility work or delete personal data implicitly.
- Documentation-only commits may follow the verified code. Read the live branch before writing; no force reset, automatic merge or assumption that an old chat describes current main.

## Implemented in this continuation

**Optional quiet corner:** a permanent ranch improvement available after the completed first day. Restore it for **40 G and 3 supplies**. Its greybox presentation switches from weathered boards to a bench when the existing live flags record completion; repeated refresh/load does not create more nodes. There is no upkeep, new facility ledger or recurring construction cost.

**Bounded solo recovery:** up to **10 stamina once per in-game day**, capped at the existing daily capacity including the existing rested bonus. Full stamina does not consume the break; a partial refill does. No phase advance, backlog, streak or penalty for skipping it. Walking remains free. The prepared evening bath's next-day bonus is neither granted nor consumed by the corner.

**Shared quiet moment:** a separate entry to the existing voluntary `DateActivityKind.QuietRest`. It delegates cost, phase-use receipt and relationship effects to DatingService and retains existing eligibility checks. It does not also pay the solo recovery. Positive numeric coverage uses an isolated synthetic fixture; no shipped character identity/design or eligibility has been approved by this work.

**Spatial courier board:** the ranch noticeboard opens the existing Community Board through the same nearest-target/InputMap/touch-assistance path as other world interactions. Pause-menu access remains. Merely opening it never delivers goods or awards gold; both entries share the existing one-delivery-per-day receipt.

**Personal points and UI ownership:** personal inspection points no longer require a roster worker or fire job-assignment tutorial events. Existing work stations still do. The corner has one pause owner, scrollable cost/reason labels, disabled unavailable actions and a planning route to the existing Schedule. Back closes the corner directly to the world; the board preserves its existing nested Back -> pause -> world behavior. Full session changes close stale corner UI and release pause.

The new commands validate state generation, day, phase, area and combat state before mutation. The spatial dispatcher and panel also validate proximity; shared moments validate the captured partner. Construction/recovery receipts are complete before StateChanged, preventing reentrant payouts. Two bounded live FlagService IDs are reserved: **1230100** permanent restoration (bool), **1230101** last recovery day (int). Root saving synchronizes them through the existing boundary.

## Executed verification

Code **`44138f3c9da33dc8a234fb9a13d4701ce7d42b68`**:

- **Godot 4.7 Mono CI #536**, run **`34524505949`**: success.
- **Build Smoke Check #544**, run **`34524505908`**: success.
- Effective C# 12 and deliberate preview-override rejection, primary-game compilation, launcher regressions, verified engine import and isolated smoke: passed.
- **1,629 SMOKE OK, zero failed assertions, one terminal SMOKE PASS**. This retains all 1,547 baseline checks and adds **47 deterministic leisure checks plus 35 frame-driven leisure checks**.
- Smoke contains **zero out-of-tree transform errors**, plus the five expected malformed/unsupported-save rejection diagnostics. Import exits successfully but logs a separate `EditorSettings not instantiated yet` message for `export/android/shutdown_adb_on_exit` after loading the editor layout. Do not describe that import as error-free or suppress this diagnostic; see KNOWN_ISSUES.

Artifact **`10171044862`**, `godot-4.7-verification`, SHA-256 **`ec72c327d4459dd6628cf10bfcdf47527a476802c6697a4db20918bf4ffd101c`**. Smoke `.artifacts/godot/smoke-59zt22d_/console.log`; import `.artifacts/godot/import-13q8rkmg/console.log`.

### Actual progression exercised

The existing first-day walkthrough still reaches the Day 2 report, production-backed delivery and save/load. The extension then discovers the real supply shortage, opens Schedule from the corner, assigns Office Work through its live button, advances ordinary phases, selects the explicit nightly Rest choice, and ends Day 2 through the normal management button. Day 3 supplies and gold fund restoration. Existing mentorship spends stamina before the live recovery action; tests cover repeat/hidden callbacks, Back, planning, no-worker access and save/load of the resulting bench, stock, gold, stamina and receipt.

No stock, gold, day or stamina is injected to fund that progression. A separate temporary empty-roster edge case restores every resident before saving. Positions near interactables are staged and buttons are activated by their real signals; this is not a rendered navigation or physical-device playtest.

### Failed attempts and limits

Initial CI `34522469432` exposed two test compile mistakes: a helper shadowed Godot's Key enum, and the synthetic fixture lacked the existing Resources namespace. Both were corrected without changing the C# version.

Run `34523042369` failed seven frame assertions because the fixture assumed the starting three supplies survived Day 1 facility maintenance. Run `34523895212` then failed nine assertions because the extended fixture omitted the existing explicit Night choice. The final test follows real supply production and the visible Night/End Day controls. Costs, maintenance, production and night rules were not weakened, and assertions were not skipped to get green CI.

A temporary branch-only hash-checked workflow wired large existing sources using explicit paths and non-force commits, then was deleted. It is absent from the final diff. No original source, engine config, runtime approval data or external art package was changed.

## Next work

First perform rendered acceptance from the actual main menu in an isolated profile: first day and shortcut, walk to the new points without staged teleports, planning/production/night/report, restoration, pause/focus/travel, and save/load. Inspect narrow-window focus/scrolling and controller navigation. The bench remains a greybox with **no authored collision, seated pose or seating animation**; those presentation tasks must not be marked done by the node-visibility tests.

Then improve demonstrated friction and the presentation of this small slice before adding more disconnected menus or daily requirements. Optional companion encounters and authored obstacle-aware navigation remain future work. Long-term prices, recovery balance, low/high Forward+ quality and representative-hardware performance remain unvalidated. Older KANBAN/WORK_LOG entries describe historical scopes; this handoff and PLAYABILITY describe PR #12.

## Commands

```bash
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python -m unittest discover -s Tools/Godot -p "test_*.py"
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/launch.py --mode runtime --isolated
```

Smoke writes/deletes disposable slot 99. Always use the isolated launcher, never a raw smoke flag against personal saves. Keep the original `eraMakaiRanch-game-eng-translation/` read-only and GameRoot as the single simulation, calendar and economy authority.
