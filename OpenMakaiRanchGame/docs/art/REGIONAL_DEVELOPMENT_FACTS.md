# Regional development: source facts and integration boundary

Audit baseline: branch `astar`, HEAD `1a65868`; initial working tree clean. Original and remake source inspected read-only. This is a handoff, not an implemented upgrade or a runtime test report. Paths below are repository-relative; `G` means `OpenMakaiRanchGame`, `O` means `eraMakaiRanch-game-eng-translation`. Line citations refer to this baseline.

## Decision for the playable slice

**No existing remake municipal-tax or public-building upgrade transaction was found.** Do not portray a supply yard or market as already financed through taxes. There IS a durable, earned event: a successful paid community delivery, exposed by `GameRoot.CompletedCommunityDeliveries > 0`. It proves trade occurred, not that the player donated goods, financed construction, earned municipal authority, or unlocked a public tier. Using it for a cosmetic recognition milestone is a new presentation decision, not original-game parity.

Recommended honest first slice: keep current services usable, author base/construction/complete asset variants in an explicitly isolated **test-mode development preview**. First successful delivery may demonstrate the transition without charging anything again; until public-development design is approved, do not enable that transition as a financed production mechanic. A permanent construction interval has no verified existing authority; do not fabricate one using elapsed days or a second delivery threshold.

## Original: actual tax → town linkage

- `O/CSV/Flag.csv:189–206`: allocation weights 700–703 = military/science/municipal/dairy; accumulated budgets 710–713; levels 720–723. Municipal budget is **712**, Okachi Town level **722**. These are distinct from ranch funds and contribution points.
- `O/ERB/◯精算/●精算_COMMON.ERB:43–75`: income minus expenditure → `SETTLEMENT_BUDGET_COST` → `SETTLEMENT_TAX` → `MAKAI_LVUP` → remaining owner funds. Tax is not a second later debit of already-paid owner money.
- `O/ERB/◯精算/●精算_経費計上.ERB:9–32`: protected worker-based funds first, then recorded expenses and loans. Thus “any positive gross income pays tax” is not the actual condition.
- `O/ERB/◯精算/●精算_納税.ERB:3–35`: returns when residual `MONEY_TOTAL <= 0`; sets the tax milestone bit even with tax exemption; exempt residual returns to funds. Otherwise computes `MONEY_ADD = MONEY_TOTAL / 100`, multiplies by `PROFIT_MONEY`, returns that amount to owner funds, then allocates residual using `MONEY_TOTAL / TOTAL_RATIO * weight` (integer order matters). `O/ERB/●ERH/広域定数.ERH:104` defines `PROFIT_MONEY = 10`. These are source facts, NOT proposed remake rates.
- `O/ERB/○街にお出かけ/魔界農協_納税割合変更.ERB:3–51,61–85`: `AGRI_COOP_TAX_RATIO_SELECT` edits normalized departmental weights (0–1000), not a freely chosen new overall tax percentage. Initial four weights are 10 each (`O/ERB/●スタートアップ設定/スタート直前_初期設定反映.ERB:32–36`).
- `O/ERB/◯精算/魔界レベルアップ.ERB:3–40`: `MAKAI_LVUP` consumes accumulated budget when sufficient, increments its level and revisits for further levels. `MAKAI_LVUP_NEED_MONEY_CALC(ARG)` returns 10000 for level 0 and 100000 for level 1; subsequent formula is `ARG * MAKAI_LVUP_MONEY * (ARG / MAKAI_LVUP_PROGRESSIVE + 1)`.
- `O/ERB/マイルストーン/マイルストーン_920～.ERB:2–114`: Okachi level 1 unlocks loans/adult shop; 2 clothing categories; 3 furniture shop; 4 more clothing; 5 special scissors; 6 and 8 further clothing assortments. **No first public supply-yard/market tier is specified here.** Do not substitute that asset for an original tier reward.
- `O/ERB/マイルストーン/マイルストーン_830～.ERB:2–17`: tax milestone story unlocks tax allocation at the agricultural cooperative. `O/ERB/○街にお出かけ/魔界農協.ERB:30–63` exposes allocation, early repayment and donation. Separate voluntary `AGRI_COOP_DONATION` explicitly debits ranch money and credits a selected budget, then calls `MAKAI_LVUP` (`魔界農協_寄付.ERB:46–56`). This is NOT permission to donate goods already sold through courier orders.

## Remake: authority and resource meanings

- `G/src/Gameplay/DailySettlementService.cs:45–165` calls existing work output, upkeep, `Economy.ApplySettlement`, shipping, events and milestone checks; no municipal allocation/level-up call exists in this inspected settlement. `EconomyService.ApplySettlement(int income, int expenses)` records income/expenses and applies net owner gold (`G/src/Gameplay/EconomyService.cs:183–191`). Gameplay-source searches for tax/municipal and original budget/level IDs found no corresponding live town development implementation. Generic `FlagStorage` capacity is not such an implementation.
- `G/src/Gameplay/EconomyService.cs:32–116`: `Gold`, `ExpenseAccount`, `LoanBalance`, `StoredSpirit` → `Economy.SpiritEnergy`, `StoredMana` → `Economy.ManaReservoir`, `ContributionPoints` are separate ledger fields/APIs. `Spend(int)` and `AddGold(int)` operate owner gold only. No municipal treasury API is established.
- `O/CSV/Money.csv:4–13` independently labels funds, recorded expenses, loan, stored spirit, stored mana and contribution points. Contribution points are not generic civic reputation: original settlement credits them for a particular milk-shipping revenue channel (`O/ERB/◯精算/●精算_COMMON.ERB:77–81`); original character creation spends them (`O/ERB/○キャラメイク/手動キャラメイク/手動キャラメイク_目次.ERB:5–18,169`). Remake searches found contribution ledger accessors/normalization, not a public-project earn/spend linkage.
- Personal MP is `State.Player.Mana`; stored mana is not personal MP or spirit. `MagicService.CurrentMana`, `StoredMana`, `SpiritEnergy`, `HasManaSupplyDevice` make the separation explicit (`G/src/Gameplay/MagicService.cs:24–45`). `RechargePlayerManaFromStorage(int)` requires the device and uses the existing two-stored-to-one-personal conversion (`:127–142`); no public-building finance meaning follows.
- Player stamina is `State.Player.Stamina`, a daily meaningful-action budget; walking, sprinting, dialogue, shopping and menus remain free (`G/src/Gameplay/PlayerStaminaService.cs:19–44`). Do not relabel it as civic labor, mana or tax capacity.
- Private ranch equipment already has a real paid upgrade: `Ranch.InspectFacilityUpgrade(string?)` produces `FacilityUpgradeOffer`; `GameRoot.TryUpgradeFacility(FacilityUpgradeOffer? quote, ulong generation, int day, DayPhase phase)` validates current quote/context, then delegates to `Ranch.UpgradeFacility` once and publishes state change (`G/src/App/GameRoot.StationPlanning.cs:15–36`). Cost/level/upkeep belong to existing facility definitions (`G/src/Gameplay/RanchService.Facilities.cs:14–65`). This is not a public-building API and has no construction-duration state. Reusing it for municipal scenery would create unintended owner charges and upkeep.

## Proven earned event and exact delivery API

`G/src/App/GameRoot.CommunityRequests.cs:10–35`:

```csharp
IReadOnlyList<CommunityRequestOffer> GetCommunityRequests();
int CompletedCommunityDeliveries { get; }
bool TryDeliverCommunityRequest(string? requestId, int expectedDay,
    ulong expectedGeneration, out string message);
```

- Offers: `market_basket` → `farm_goods`; `kitchen_delivery` → `meals`; `workshop_delivery` → `supplies` (`G/src/Gameplay/CommunityRequestService.cs:42–63`). Existing stock counts. Production hints identify existing ranch work; assignment itself does not generate inventory.
- Root rejects stale generation and combat. Service rejects stale day, pre-opening day, unknown ID, insufficient stock, already-delivered day and full gold balance before mutations (`:66–120`). Existing board opens Day 2, one delivery per day, maximum advertised daily reward 60 G (`:20–25`). Preserve these existing rules; do not add an arbitrary development day gate.
- Successful transaction consumes the offered quantity from **Ranch.Stockpile**, pays owner **Gold**, and records four flags (`:90–100`): `LastDeliveryDayFlag = 1_230_000`, `CompletedDeliveriesFlag = 1_230_001`, `LastDeliveryKindFlag = 1_230_002`, `LastDeliveryRewardFlag = 1_230_003`. They are receipts, not construction funding.
- `CompletedCommunityDeliveries > 0` is a durable any-order predicate. Last-kind is only the latest kind, not a permanent “ever delivered market basket” history. Do not claim a market-specific lifetime achievement from it.
- Regression source covers real ranch production → delivery, duplicate/reentrant rejection and root save/load (`G/src/Tests/CommunityRequestRegressionTests.cs:43–100,132–196`). **Tests were read, not executed during this audit.**

## Existing physical services and entry boundaries

Scene authority: `G/scenes/dev/TownGreybox.tscn:77–152`.

| ServiceId | ScreenId | Existing requirement/use |
|---|---|---|
| `general_store` | `shop` | Existing shop; no RequiredFacilityId |
| `adventure_guild` | `adventure` | Existing adventure interface |
| `research_office` | `research` | `RequiredFacilityId = "workshop"` |
| `tavern` | `roster` | Roster interface, not proof of a mayor/recruitment building |
| `bathhouse` | `bond` | Existing bond interface |
| `town_hall` | `milestones` | Read-only milestone listing, NOT tax administration |
| `planning_board` | `town` | **Stale route:** `town` is absent from `IsKnownService`; do not promise working planning here |

- Physical town path: `TownWorldController.TryInteract()` checks active tree/visibility/process/pause/input, recalculates nearest target, requires range (default 2.5), and checks facility availability before `ServiceScreenRequested(ScreenId)` (`G/src/World/TownWorldController.cs:14,130–170,278–300`). Host `OnTownServiceRequested` routes to `OpenDedicatedService` (`G/src/World/WorldGameController.cs:393–402`). `OpenDedicatedService(string)` validates known screen and sets scoped service context (`G/src/World/WorldGameController.Stations.cs:109–118`); guided-opening policy still applies (`WorldGameController.OpeningPolicy.cs:17–38`).
- `G/src/Ui/UiShellController.ServiceContext.cs:23–55`: dedicated shop may enter inventory; roster may enter character detail/ability/room assignment; Back closes local UI instead of opening a remote hub. Moving/retrofitting assets must preserve IDs, requirements, local targets and input ownership. The root methods themselves do NOT establish proximity.
- Generation-aware transaction entry points already exist: `GameRoot.TryBuyItem(string? itemId, int quantity, ulong expectedGeneration)`, `TryRecruit(ulong expectedGeneration)`, `TryRunMission(string? missionId, ulong expectedGeneration)` (`G/src/App/GameRoot.WorldCommands.cs:13–43`), and `TryCompleteBondEvent(string? eventId, ulong expectedGeneration)` (`GameRoot.cs:423`). Retained shop UI still calls `game.Shop.Buy(item.Id, 1)` (`UiShellController.Screens.cs:983`); do not claim all legacy callbacks have modern world guards.
- Research's active UI calls `game.Research.Unlock(skill.Id)` (`UiShellController.Screens.cs:1729–1745`), the root-built `ResearchService` (`GameRoot.cs:1386`; implementation `Gameplay/ManagementServices.cs:619–647`), using configured stockpile cost. Do not substitute the separately named `ResearchTreeService` gold/cooldown path.
- Existing courier physical point is ranch `RanchLeisureController.BoardId = "POINT_COMMUNITY_BOARD"` (`G/src/World/RanchLeisureController.cs:15`). Its `Dispatch(WorldCommand, WorldInteractionContext)` validates availability, generation, nearby point, area and UI/transition state before `PauseMenu.OpenCommunityBoardFromWorld("ranch")` (`:65–98`). Public `OpenCommunityBoardFromWorld(string areaId)` exists (`PauseMenuController.RanchLeisure.cs:13–19`), but is not itself a town proximity guard. A town courier counter needs explicit equivalent guarded routing, not an invented `ScreenId = "community"`.

## Ownership/story: what can be represented

`O/ERB/資料室/魔界の住民.ERB:108–116` identifies the deceased grandfather ウェーバー as former ranch owner and the player as inheritor. `:119–124` identifies フドウ as land/building manager and intermediary at Okachi real estate, not owner of the entire valley or mayor. `:127–135` identifies ケンチ as an eastern construction-branch head. These are original roles, not authorization for new character designs or automatic assignment to remake C01 characters. Use neutral administration/supply signage; no invented landlord, municipal ruler, rent or player public-office title. Existing town hall UI only proves milestone access (`G/src/Ui/UiShellController.Screens.cs:1717–1726`).

## Proposed isolated presentation contract (not implemented)

1. **Base:** habitable ranch and traversable road remain usable; modest supply/market shell exposes existing shop and milestone services with their real labels. No new recurring costs or work payout.
2. **Construction preview:** explicitly marked test-only asset selection, no escrow, timers, requisition or save mutation. Do not suggest paid goods remain on site as donated building materials. For an event demo, construction may be a transient visual transition AFTER one successful root delivery; failed/repeated commands must not trigger it.
3. **Complete preview:** derive delivery recognition from current `game.CompletedCommunityDeliveries > 0`, if the preview deliberately demonstrates an earned trigger. This is not proof of public finance. The alternative manual base/construction/complete selector must remain an isolated art-test override and never alter persistent progression or service unlocks.
4. Subscribe/rederive on `GameRoot.StateChanged`; resolve current root services after NewGame/LoadSlot. Never retain an old `CommunityRequestService`, increment receipt flags for scenery, replay deliveries, or charge for a visual refresh. Existing services stay available regardless of cosmetic variant unless an independently verified existing unlock says otherwise.
5. Persistence authority: `SaveState.CurrentSchemaVersion = 16`, `SaveState.Flags` (`G/src/Core/Models/SaveModels.cs:97–134`). `FlagService` owns live dictionaries, with `SyncToStorage`/`SyncFromStorage` (`G/src/Gameplay/FlagService.cs:45–90`). `GameRoot.SaveSlot(int)` syncs flags before saving (`GameRoot.cs:381–386`); `LoadSlot(int)` replaces state/rebuilds services (`:516–536`); service rebuild imports flags and increments `StateGeneration` (`:1399–1402`). Read the root delivery property, not a potentially unsynchronized direct `State.Flags` snapshot. No new schema or durable mesh-stage field is needed for the proposed derived preview.
6. Integration acceptance, still to run: base/preview/complete walkability and entry collision; research workshop lock; all relevant nearby/paused/hidden/stale-generation guards; real production → one paid delivery → one visual transition; failure/reentry no transition/payment; SaveSlot/LoadSlot preserves count and derives complete without another debit; NewGame resets derived appearance; construction override does not leak into ordinary save/load. Use disposable launcher profiles; never raw smoke against personal slot 99.

**Blockers:** public funding/authorization and persistent construction lifecycle are undecided; tax parity is absent from inspected remake authority; `planning_board → town` is a stale dedicated-service route; town-local courier interaction needs explicit integration. No code, original files, JSON, scenes, engine settings or character designs were changed by this audit.
