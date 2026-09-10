# Playability: first days, a useful ranch and optional recovery

Updated **2026-09-10** for PR #12, code `44138f3c9da33dc8a234fb9a13d4701ce7d42b68`. **C# 12** remains mandatory. This is a bounded playable slice, not a finished-game or enjoyment certification.

## First-day loop and repeat-player shortcut

The existing guided world route connects waking up, leaving the bedroom, assigning Pasture Work, planning another worker's Dairy assignment, the evening tutorial encounter/conversation, the optional bath and the Day 2 report. The starting party completes the tested tutorial encounter without injected combat stats. Actual first-day production funds the tested Day 2 market basket.

**Skip First Day -> Night** reaches the existing Night routine without settling the day, granting mission rewards or spending daily stamina. The player still chooses the night action. A prepared evening/night bath gives the existing extra stamina on the next day, not an immediate refill. Walking and exploration remain free of the daily stamina cost.

Finishing an area fade retains an open story dialogue's input lock. Closing a dialogue retains a pending fade or management overlay's ownership. Those PR #11 controls and the complete original first-day regression are retained.

## Optional quiet corner

After finishing Day 1, approach the marked **Quiet corner** on the ranch and use the ordinary interaction action (default **F**). The same nearest-target selection and touch-assistance route used elsewhere owns this interaction; there is no second input poller. It does not require a roster worker.

Restoration is a **one-time 40 G + 3 supplies** purchase. The weathered-board stand-in becomes a bench and remains restored after saving/loading. No new upkeep, obligation, streak or repeated construction cost is added. The panel displays available gold/supplies and explains a shortage before any payment is taken.

Starting supplies can already have been consumed by existing facility maintenance. **Plan ranch work / supplies** opens Schedule; assign Office Work or Workshop Crafting, choose the normal night action and finish the day. The verified route produces supplies during Day 2 and restores the corner on Day 3. This is not a promise that the initial Day 2 stock covers construction.

**Take a break** restores up to **10 stamina once per day**, capped at today's existing capacity. A full bar leaves the break available; a partial refill uses it. Reopening, changing phases or loading a completed break cannot award a second refill. Skipped days do not accumulate unused breaks. This action advances no time and neither replaces nor changes the prepared bath's next-day bonus.

**Share a quiet moment** is separate from solo recovery. With an existing eligible, willing active companion, it invokes DatingService's Quiet Rest activity, including its current **8 stamina cost**, one-activity-per-phase limit and relationship effects. It grants no additional solo refill. Missing/ineligible partners and pressured outings remain unavailable. No character approval or new dating rule is supplied by this entry point.

The corner panel is pause-owned, scrollable and explains disabled actions. Back closes it directly to the world. Planning releases pause and hands control to the existing management screen. A new/loaded session closes stale UI before old callbacks can act.

The bench is deliberately a **greybox stand-in**, built once and switched by the live restoration flag. It has no authored collision, seated pose or seating animation. Visibility/lifetime tests do not certify final artwork or walkable access around obstacles.

## Optional courier board

Open **Pause -> Community Board**, or approach the ranch's marked **Community Board** after the first day and interact. Both entrances open the same existing board and share stock, offers and receipts. Opening it does not automatically deliver or pay. The original pause entry still previews orders on Day 1; delivery opens on Day 2.

The board displays actual stock, required quantity, full payment, remaining stock and production hints. **Plan ranch work** opens Schedule. A courier collects goods; there is no required repeat commute through town. Offers depend on the current in-game day, not wall-clock time, and reopening does not reroll prices.

| Order | Existing resource | Quantity | Reward | Production source |
| --- | --- | --- | --- | --- |
| Market basket | `farm_goods` | 3-5 | 30-50 G | Pasture Work |
| Town crew lunch | `meals` | 2-3 | 35-45 G | Kitchen Chores / Cooking |
| Workshop delivery | `supplies` | 2-4 | 40-60 G | Workshop Crafting / Office Work |

At most **one delivery across both entrances per in-game day**. Ignoring orders causes no relationship penalty, debt, lost streak or backlog. No accepted-order deadline or new stamina charge is added. These rewards and the new corner costs/recovery are preliminary balance bounds, not a validated long-term economy.

## Shared state and persistence

CommunityRequestService reads existing ranch stock and pays through EconomyService. It never assigns workers, creates production, advances the calendar or changes the previous report's income field. Root commands validate generation/day, combat state, shortages and wallet bounds. The receipt is complete before the single StateChanged notification.

Courier integer flag IDs remain **1230000** last paid day, **1230001** saturating lifetime delivery count, **1230002** last order kind and **1230003** last payment. The corner adds only **1230100** permanent restoration (bool) and **1230101** last recovery day (int). Use live FlagService, synchronized by the existing root save boundary; writing only State.Flags would be overwritten on save. No schema change or growing per-day history is introduced. Schema remains 16.

Corner commands additionally validate captured phase and area; the spatial dispatcher/panel validate proximity, and a shared moment verifies the captured active partner. Construction and recovery finish their receipts before notification to reject reentrant calls. The service is derived from the current root state rather than cached across NewGame/LoadSlot.

Loading a completed action retains its payment, stock/stamina changes and receipt together. Loading an earlier save also restores earlier resources; it does not accumulate extra rewards across timelines. Personal inspection points leave worker assignments unchanged and do not fire onboarding work-completion events.

## Movement, camera and Back

Retained PR #11 controls preserve analog/touch strength and circular deadzones, bound keyboard diagonals and resolve movement from the main viewport's active camera. UI, pause, disabling and focus loss clear horizontal drift and held touch sprint. First-person capture belongs only to the active visible camera; resume checks the actual mouse mode. Inactive cameras cannot steal another area's cursor ownership.

Normal mouse-up looks up; Invert Y reverses it. One recenter press completes behind the player's facing direction, preserving zoom. Manual look cancels it and Reduced Motion recenters immediately. Detached/freed targets are ignored until a live target exists.

The courier board retains **Back -> pause -> world**, including when opened spatially. The quiet corner returns directly to the world. Pause/Back remain event-owned, never separately re-polled after the UI has consumed the press. Do not add arbitrary input cooldowns or another calendar/UI authority to fix double routing.

## Executed verification

Code `44138f3c9da33dc8a234fb9a13d4701ce7d42b68`: **Godot CI #536 / run 34524505949** and **Build Smoke Check #544 / run 34524505908 succeeded**. C# 12 contract and preview-denial checks, compilation, launcher regressions, Godot 4.7.2 Mono import and isolated smoke passed. **1,629 checks passed, zero assertions failed**: all 1,547 prior checks plus **47 deterministic and 35 frame-driven leisure checks**.

The extended path uses real first-day production and delivery, discovers the subsequent supply shortage, assigns Office Work through Schedule, uses the visible Night choice and End Day, and funds restoration from the resulting Day 3 stock/gold. Existing mentorship spends stamina before the break. Tests also cover shared-activity contracts, repeat/reentrant/hidden callbacks, stale sessions, no-worker access and current-version save/load. Positive companion numeric tests use an isolated synthetic fixture, not a newly approved shipped character.

Artifact `10171044862`, SHA-256 `ec72c327d4459dd6628cf10bfcdf47527a476802c6697a4db20918bf4ffd101c`; smoke `.artifacts/godot/smoke-59zt22d_/console.log`. Smoke has zero out-of-tree transform errors and five expected rejected-save diagnostics. Import exits successfully with a separate EditorSettings/Android-shutdown diagnostic, recorded in KNOWN_ISSUES. No failure or engine diagnostic was suppressed.

## Manual acceptance and next work

Run the real main-menu/character-creation route in an isolated rendered session. Walk to both points without staged teleports; check labels and reachable approaches, supply planning, night choice, reports, restoration, recovery, Back, travel and save/load. Inspect narrow-window scrolling/focus and physical controller/touch/mouse behavior. The automation stages proximity and activates live button signals; it is not proof of visible hit targets, obstacle-aware navigation, actual OS cursor behavior or seating animation.

Review low/high Forward+ visuals, weather, camera feel and performance on representative hardware before expanding asset density. Refine demonstrated friction and this small improvement's presentation before adding more chores or reward menus. Optional authored companion encounters and long-term balance remain separate work.
