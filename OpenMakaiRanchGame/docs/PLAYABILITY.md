# Playability: first day, controls and production with a purpose

Updated **2026-09-10** against code `40daf7560a53b66d8210989f695bfb3e3a8ec692`. C# 12 is the source-language contract. This is a bounded playable slice, not a claim that the whole remake is finished or fun-tested.

## First-day loop and repeat-player shortcut

The guided playable-world route connects waking up, leaving the bedroom, assigning Pasture Work, planning another worker's Dairy assignment, the evening tutorial encounter/conversation, the optional bath and the normal Day 2 report. The existing starting party can complete the tested tutorial encounter without injected combat stats. Actual first-day pasture production can supply the Day 2 market basket without injected stock or money.

**Skip First Day -> Night** reaches the existing Night routine without settling the day, granting mission rewards or spending daily stamina. The player still chooses the night action. A prepared evening/night bath supplies the existing extra stamina on the next day; it does not refill the current day's stamina. Ordinary movement and exploration remain free of this daily stamina cost. This continuation verifies and hardens these existing rules rather than creating another clock, recovery economy or reward path.

Finishing an area fade does not release input while a story dialogue is still visible. Closing a dialogue likewise preserves a pending fade or management overlay's ownership. Both the normal first day and the repeat-player night shortcut are covered by the current suite.

## Movement and camera

Movement preserves partial controller-stick and touch input, uses configured circular deadzones and keeps keyboard diagonals bounded. Camera-relative movement uses the current main viewport camera, not a preview SubViewport or a cached camera from a previous area. Opening management, pausing, disabling an area or losing focus clears residual horizontal motion and held touch sprint. No gameplay stamina is deducted by the movement controller.

First-person mouse ownership belongs only to the active visible camera. Pause, management, focus loss and area changes release it; a valid active update reacquires it. Resume checks the real mouse mode rather than trusting an old captured flag. An inactive camera cannot steal or release another area's cursor ownership.

Analog camera look has a dedicated turn rate and retains stick magnitude. Normal mouse-up looks up; Invert Y reverses it. One recenter press finishes behind the player's facing direction instead of moving only for the initial frame. Manual look cancels recenter; Reduced Motion recenters immediately. Recenter preserves the chosen zoom. Detached/freed camera targets are ignored until a live target exists.

These are implemented behavior contracts. Headless tests do not establish physical gamepad feel, OS cursor capture or final camera collision/readability under rendered play.

## Optional courier board

Open **Pause -> Community Board** in the playable world. Day 1 previews the orders and explains how production works; deliveries open on Day 2. Choose a market basket, prepared meals, or workshop supplies. The board displays the actual stockpile, required quantity, full payment, remaining stock after delivery, and production hints. **Plan ranch work** opens the existing `schedule` screen.

Orders use the current in-game day, not wall-clock time or a new random roll. Reopening the board does not reroll prices. A courier collects the goods, so this small reward does not demand repeated trips across town.

| Order | Existing resource | Quantity | Reward | Production source |
| --- | --- | --- | --- | --- |
| Market basket | `farm_goods` | 3-5 | 30-50 G | Pasture Work |
| Town crew lunch | `meals` | 2-3 | 35-45 G | Kitchen Chores / Cooking |
| Workshop delivery | `supplies` | 2-4 | 40-60 G | Workshop Crafting / Office Work |

At most **one delivery across the whole board per in-game day**. The 60 G maximum is a starting balance limit, not a playtested economic optimum. Ignoring orders does not damage relationships, create debt, reset a streak, or accumulate a backlog. There is no streak system, accepted-order deadline, mandatory visit, or new stamina cost. Other gameplay remains available after delivery.

## Shared-state contract

`CommunityRequestService` reads existing ranch stock and pays through the existing `EconomyService`. It does not create goods, assign workers, advance the calendar, award production twice, or modify the previous settlement's income field. UI writes go through `GameRoot.TryDeliverCommunityRequest` with the displayed day and state generation. Stale callbacks, changed days, active encounters, shortages and wallet overflow reject before consuming goods. The receipt is complete before the single `StateChanged` notification.

Four global integer flag IDs are reserved for this feature: `1230000` last paid day, `1230001` saturating lifetime delivery count, `1230002` last order kind, and `1230003` last payment. They use the live `FlagService`, synchronized at the existing root save boundary. Do not write these only into `State.Flags`: root saving would overwrite unsynchronized copies. No new schema or growing per-day history is introduced.

Loading a save made after a delivery retains payment, consumed stock and receipt together. Loading a save made before delivery also restores its old gold and stock; it does not create additional accumulated gold. Rebuilding services after NewGame/LoadSlot cannot retain an old state-bound courier service. Save schema remains 16.

## Pause/Back ownership

The board belongs to the pause menu. Back closes the board first, then the pause menu on the next press. Keyboard/controller focus cycles through enabled actions, and the order list follows focus when scrolling. Opening Schedule releases pause and delegates to the existing world/management coordinator.

Pause/Back are handled by input events, not re-polled in world `_Process` after a UI event has consumed the press. Both parent and pause controller use `PauseMenuController.GoBack()`. Do not fix double processing with arbitrary cooldowns, forced input releases, or another UI/calendar authority. The latest frame walkthrough verifies actual Escape events opening pause, closing the nested board and returning to the world without same-frame reopening.

## Executed verification

Code **`40daf7560a53b66d8210989f695bfb3e3a8ec692`**, **Godot 4.7 Mono CI #527** / run `34517743009`, and **Build Smoke Check #535** / run `34517742952`: **success**. The effective C# 12 checks, primary-game compilation, 34 launcher regressions, engine import and isolated full smoke succeeded: **1,547 passing checks / zero failed assertions / SMOKE PASS**. All 39 new deterministic playability checks and 18 new frame-walkthrough checks passed. The three old fixture/expectation failures were corrected without changing runtime gates or the existing 2:1 stored-mana rule.

`PlayabilityFrameTests` follows the full first-day playable-world route to Day 2 production, delivery and root save/load. It injects real Godot keyboard events over process/physics frames; positions near interactables are staged and button presses use their live signals. It deliberately isolates the routed main menu before constructing the test world. This is not a rendered main-menu/character-creation traversal, navigation test, physical-controller test or enjoyment evaluation.

Import produced no engine errors. The full smoke log contains zero out-of-tree Transform3D errors and five intentional invalid-save rejection diagnostics. No engine errors or failing assertions were silenced. Exact evidence and intermediate failed attempts are documented in `ASTRA_HANDOFF.md`.

Artifact `10168420601`, SHA-256 `29d9079234d73ec4c1007a1f783e9b0231d6541f34be1e3b41f06f1b95043402`, smoke log `.artifacts/godot/smoke-ozygihzg/console.log`.

## Manual acceptance still required

Start through the real main menu in an isolated rendered session. Play the normal first day and separately the skip-to-night route. Walk the routes without staged teleports. Inspect the Day 2 report, deliver one order, save/load and return to exploration. Skipping later orders must not create a penalty or backlog. Check that bath recovery remains an optional benefit rather than required maintenance.

Repeat camera/menu navigation with mouse/keyboard and a physical controller. Check small-window scrolling and text, focus visibility, Back -> pause -> world, Plan ranch work -> Schedule -> world, first-person pause/Alt-Tab/travel, deadzones, analog movement, recenter and Invert Y. Review weather and low/high Forward+ visuals on representative hardware. Headless checks cannot certify these presentation results.

## Next gameplay priorities

Use the verified loop as the base for one optional world encounter, companion reaction or visible ranch improvement. Reuse existing exploration, relationship and upgrade commands rather than adding another disconnected menu or reward system. Do not turn the courier board into the main progression gate, an endless checklist, or a reason to tax walking with stamina.

Observe a rendered playthrough before raising rewards or adding more systems. Record confusing transitions, unnecessary travel, repetitive interactions and unclear outcomes. Prefer fixing demonstrated friction over increasing feature count.
