# WORLD DESIGN (vertical slice)

> Design + area inventory for the 3D world. Goal (criterion 2/3): the world is
> cohesive, readable, explorable, and preserves the original's important areas —
> not a single flat greybox room. Non-adult scope only; adult content stays
> gate-blocked.

## 1. Original area baseline (read-only audit)

Source: `eraMakaiRanch-game-eng-translation/` (ERB folder structure). The original
is a **ranch-management game with two spatial contexts + an adventure loop**:

| Original area | Original role | Remake data trace | Remake 3D status |
|---|---|---|---|
| **Ranch (base)** | Player's home; 9 upgradable facilities + training + bath | `data/facilities.json` (9) | **PARTIAL** — flat 40×30 greybox with barn + well + trees + fences + pasture; no interior, no per-facility zones, no exploration |
| **Town (街にお出かけ)** | Shops: real-estate (不動産), demon co-op (魔界農協), demon-army lab (魔王軍研究所), general shop | management UI (`Town`/`Shop` screens) | **MISSING** from the 3D world (UI-only) |
| **Adventure (combat)** | Missions/patrols, capture recruits | `data/missions.json`, `enemies.json` | **MISSING** from the 3D world (UI-only) |
| **Training / Bath (ranch interior)** | Character care/training + bath | `data/jobs.json`, UI | **MISSING** from the 3D world (UI-only) |

**Key finding:** the remake's 3D world currently models only the **ranch**, as a
single flat greybox. The original's **Town** and **Adventure** are real areas that
exist only as management-UI screens. The world-design subgoal's "preserve important
original areas" + "more cohesive than a single test room" + "exploration with
rewards" all point at the same gap: **a walkable world with more than one area.**

## 2. Remake area inventory (current state)

| Area | Present | Quality | Notes |
|---|---|---|---|
| Ranch (greybox) | ✅ | NEEDS REDESIGN | single flat room; no landmarks, no interior, no paths to other areas; barn/well/trees/fences/pasture exist as props |
| Town | ❌ | MISSING | original location; UI-only |
| Adventure | ❌ | MISSING | original loop; UI-only |
| Training/Bath | ❌ | MISSING | original loop; UI-only |

## 3. Target world structure (guided freedom)

Keep the ranch as the **hub**. Add the original's areas as **reachable zones** so the
world is cohesive and explorable without breaking the existing management flow:

```
                 ┌──────────────┐
   player spawn  │   RANCH (hub)│  9 facilities as zones, landmark barn, well,
   ┌────────────>│  (base)      │  pasture, training/bath interior
   │             └──────┬───────┘
   │                    │  main path (stones)
   │             ┌──────▼───────┐
   │             │    TOWN      │  plaza + 3 shop POIs (real-estate, co-op, lab)
   │             │ (街にお出かけ)│  + a general shop counter (interactable)
   │             └──────┬───────┘
   │                    │  gate / path
   │             ┌──────▼───────┐
   │             │ ADVENTURE    │  patrol gate → mission dispatch (UI handoff)
   │             │ (combat)     │
   │             └──────────────┘
   └─────────────────────────────────────────────
   management UI (existing, unchanged) = same simulation, reachable anywhere
```

**Design rules (from the subgoal):**
- **Preserve** the original areas (ranch, town, adventure) at minimum — none removed.
- **Hub** = the ranch (return destination, facilities, rest).
- **Guided freedom** — the player can explore town/adventure voluntarily, but the main
  progression (facilities, bonds, missions) is still reachable from the hub and UI.
- **Landmarks** per area (barn, town plaza, patrol gate).
- **Exploration rewards** — each area has at least one interactable + one visual/lore
  touch; no empty corridors.
- **Collision readability** — walkable path, clear bounds, interactables look interactable.
- **Consistent anime art direction** — soft natural shading (SOFT-ANIME-001), no hard
  corners; PBR where present, honest stand-ins where not (CHAR-001 rule).
- **No second simulation** — town shop / adventure dispatch route through the **existing**
  `GameRootCommandDispatcher` → `GameRoot` boundary (WORLD-002 pattern), never a new economy/clock.

## 4. World progression overview

| Stage | Player can |
|---|---|
| Start | spawn at ranch hub; move (keyboard+gamepad), look (mouse+stick), interact with barn/well/station |
| Ranch | use 9 facility zones (management UI or in-world interactables); train/bath |
| Town (new) | walk to town plaza; shop POIs; general shop counter (existing shop simulation) |
| Adventure (new) | walk to patrol gate; dispatch a mission (existing adventure simulation) |
| Anywhere | open management UI = same simulation (existing, unchanged) |

## 5. Connection / hub plan

- **Ranch → Town:** a stone path leads out of the pasture boundary to a town gate; the
  town plaza is a distinct space (landmark + 3 shop POIs + shop counter).
- **Ranch → Adventure:** a patrol gate at the world edge; interacting opens the existing
  adventure dispatch (UI handoff, no new combat sim).
- **Hub return:** town/adventure have a clear return path to the ranch (the hub).

## 6. Asset / state ledger (per area)

| Area | Landmark | Interactables | Status |
|---|---|---|---|
| Ranch | barn, well | milk station, facility zones | props present; zones/interior NOT built |
| Town | plaza + 3 shop POIs | shop counter | **NOT built** (top implementation card) |
| Adventure | patrol gate | mission dispatch | **NOT built** |

## 7. Kanban (world-design epic)

| Card | Title | Priority | Status |
|---|---|---|---|
| WORLD-AUDIT-001 | World/area inventory + design plan (this doc) | P1 | DONE |
| WORLD-TOWN-001 | Town area: plaza + 3 shop POIs + shop counter (walkable, interactable, existing shop sim) | P1 | READY |
| WORLD-ADVENTURE-001 | Adventure area: patrol gate + mission dispatch (existing adventure sim) | P1 | READY |
| WORLD-RANCH-001 | Ranch zones: per-facility placement + interior (training/bath) + landmarks | P2 | READY |
| WORLD-PATH-001 | Walkable paths + navigation (ranch→town→adventure) + collision readability | P1 | READY |
| WORLD-EXPL-001 | Exploration rewards (one per area: lore/interactable) | P2 | BACKLOG |
| WORLD-VIS-001 | World visual cohesion (lighting/props/palette per area, soft-anime) | P2 | BACKLOG |
