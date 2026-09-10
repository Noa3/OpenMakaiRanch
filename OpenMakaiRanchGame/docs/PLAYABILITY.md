# Playability: production with a purpose

Updated: 2026-09-10. This is a bounded playable slice, not a claim that the whole remake is finished or fun-tested.

## Implemented: optional courier board

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

Four global integer flag IDs are reserved for this feature: `1230000` last paid day, `1230001` saturating lifetime delivery count, `1230002` last order kind, and `1230003` last payment. They use the live `FlagService`, which is synchronized at the existing root save boundary. Do not write these only into `State.Flags`: root saving would overwrite unsynchronized copies. No new schema or per-day growing history is introduced.

Loading a save made after a delivery retains the payment, consumed stock and receipt together. Loading a save made before delivery also restores its old gold and stock; it does not create additional accumulated gold. Day changes use the existing calendar. Rebuilding services after NewGame/LoadSlot cannot retain an old state-bound courier service.

## Input ownership

The board belongs to the pause menu. Back closes the board first, then the pause menu on the next press. Keyboard/controller focus cycles through enabled actions, and the order list follows focus when scrolling. Opening the schedule releases pause and delegates to the existing world/management coordinator.

Pause/Back are handled by input events, not re-polled by the world's `_Process` after a UI event has consumed the press. Both the pause controller and the parent world handler use `PauseMenuController.GoBack()`. Do not fix double processing with arbitrary cooldowns, forced input releases, or a second calendar/UI authority.

## Executed verification

Code commit `b7cf272d9b1af4a18da0753c992f1eafdf8591bc`, GitHub Actions **Godot 4.7 Mono CI #515**, run `34509942143`: restore, compile, launcher tests and Godot 4.7.2 Mono import succeeded. The isolated full smoke suite produced **1486 passing assertions and 3 failing assertions**. All **50 community assertions and 8 pause-input assertions passed**. The full suite is not green; see `ASTRA_HANDOFF.md` and `KNOWN_ISSUES.md` for the three pre-existing fixture/expectation mismatches.

The community tests exercise real pasture production through day settlement, bounded delivery/payment, invalid/stale commands, duplicate and reentrant delivery rejection, flag serialization, root SaveSlot/LoadSlot, next-day availability, pause-owned UI and schedule routing. The pause suite combines an explicit no-polling source guard with actual handler calls for standard Back and the remappable pause action; it is not a physical-controller or frame-timing playtest.

Evidence artifact `10165424061`, SHA-256 `82f1cf4bb465c63f2115d8d52b7b69f80cb3b8ede4bd2cc0e346a14a185f83be`, smoke log `.artifacts/godot/smoke-xo9wgypj/console.log`.

## Manual acceptance still required

On a fresh game, schedule ordinary pasture work on Day 1 and finish the day through the normal interface. On Day 2, open the board, compare the market offer against the daily report, deliver it, verify the exact stock/gold changes, and save/load. Other orders must remain unavailable until another in-game day. Skip several days and confirm there is no penalty or accumulated obligation.

Repeat menu navigation using keyboard and a physical controller, including both the remapped pause shortcut and standard Back. Check small-window scrolling, focus visibility, text clipping, Back -> pause -> world, and Plan ranch work -> schedule -> world. Review in the actual rendered Ranch/Town composition, not only a synthetic UI fixture. Headless tests do not establish graphical quality or enjoyment.

## Next gameplay priorities, not implemented by this slice

First correct the three documented stale smoke assertions without changing valid production rules or weakening eligibility checks. Then validate the first ordinary day as a coherent loop: choose an intention, do something useful in the world, see a result, and understand the next opportunity.

The next content work should connect a small number of optional world encounters to existing exploration, companionship and visible ranch upgrades. Avoid making the courier board the main progression gate, adding an endless checklist, or rewarding repeated menu clicks. Walking/exploration must not become an energy tax; evening recovery should remain a useful choice rather than compulsory maintenance.

Observe a real playthrough before raising rewards or adding more systems. Record confusing transitions, unnecessary travel, repetitive interactions and unclear outcomes. Prefer fixing one of those friction points over adding another menu.
