# Okachi Town — 3D World Flow

Status: implemented greybox / validation pending.

## Goal

Okachi Town is a second playable 3D location inside the same `WorldGame` and `GameRoot`.
The town does not own duplicate shop, research, adventure, roster, bond, milestone, facility or
economy logic. Spatial buildings route into the existing management screens/services.

## Travel rules

Current rule:
- Ranch south gate -> Okachi Town;
- Town south gate / Return to Ranch -> Ranch;
- no added gold cost;
- no added time cost;
- current area is persisted as `SaveState.WorldAreaId`;
- old/unknown save area safely falls back to `ranch`.

A travel cost should only be added later if the original/remake design explicitly adopts one, and
then it must be implemented once in shared simulation rather than in portal scripts.

## Town services

| 3D location | Existing screen | Existing authority |
|---|---|---|
| General Store | `shop` | ShopService / Economy / Inventory |
| Adventure Guild | `adventure` | Adventure/Discovery/Mercenary/combat systems |
| Research Office | `research` | ResearchService; requires ranch Workshop, matching existing Town Hub |
| Tavern | `roster` | existing roster/recruitment presentation |
| Bathhouse | `bond` | existing bond systems |
| Town Hall | `milestones` | MilestoneService / progress |
| Construction & Planning | `town` | existing Town Hub + Facility Planning |

The existing 2D Town Hub remains useful as detailed management. The 3D city is a spatial navigation
layer, not a replacement for dense management UI.

## Player flow

1. Player walks from the ranch to the south/town gate.
2. Context prompt shows `F — Travel to Okachi Town`.
3. `WorldGameController` switches the active area without replacing `GameRoot`.
4. Town player/camera/input become active; ranch rendering/process is disabled.
5. First visit shows a short contextual tutorial; F1 always opens town help.
6. Player walks to a service building.
7. `F` routes the authored service ID to its existing UI screen.
8. Management UI owns input while the service screen is open.
9. Return-to-World closes the overlay and restores Town input.
10. South gate or Return-to-Ranch travels physically back to the ranch.
11. Save/load retains the current area.

## World layout

Current greybox:

```text
                       NORTH

          Bathhouse             Town Hall
               \                 /
                \   central     /
 Research Office -- fountain -- Tavern
                /      |         \
               /       |          \
      General Store  Planning   Adventure Guild
                       |
                    main road
                       |
                  SOUTH GATE
                 back to ranch
```

The layout deliberately prioritizes:
- central landmark visibility;
- short walking distances;
- distinct left/right service silhouettes;
- no maze navigation for frequently used management functions;
- room for later ambient citizens/props without narrowing player paths.

## Navigation

Ranch roster NPCs now carry `NavigationAgent3D` and update path-following in
`_PhysicsProcess()`, matching Godot's intended agent workflow.

Both Ranch and Town own a simple open `NavigationRegion3D` today. Current procedural buildings are
collision-free, so this region is sufficient for the greybox. When final building/fence collision is
authored:
1. replace the simple region with an editor-baked NavigationMesh;
2. keep `RosterRig` / NavigationAgent3D unchanged;
3. shrink walkable polygons for actual agent radius;
4. add bounded stuck recovery rather than teleporting immediately;
5. keep distant NPC work logical/simulation-driven.

## Town onboarding

Town HUD provides:
- location/day/phase;
- gold/mana;
- contextual service prompt;
- service descriptions as tooltips;
- next-action guidance;
- Return to Ranch;
- first-visit card;
- F1 Help.

Ranch tutorial also contains a dedicated physical-town-travel step.

## Current placeholder art

`TownPresentationBuilder` creates collision-free:
- main road and cross road;
- central plaza/fountain;
- service-building proxies;
- roofs/doors/signs;
- town gate;
- lamps;
- trees.

These are deliberately replaceable and do not contain gameplay IDs.

## CC0 replacement candidates

Preferred candidates currently recorded in `docs/assets/CC0_ASSET_CANDIDATES.md`:
- Kenney Fantasy Town Kit — CC0;
- Quaternius Medieval Village Pack — CC0;
- Quaternius Ultimate Stylized Nature — CC0;
- Kenney Nature Kit — CC0.

Exact archives/files still need local admission with source/license/package/hash records before
binary assets are committed.

## Recommended next town slices

### TOWN-002 — environment production pass
- admit exact CC0 town/nature packages;
- replace one service building at a time;
- preserve service transforms/IDs;
- author collision independently from decorative meshes;
- bake navigation after collision is stable;
- validate all service prompts from gameplay camera.

### TOWN-003 — ambient life
- add a small pool of non-essential ambient adult town residents;
- deterministic daytime waypoint schedules;
- NavigationAgent3D walking;
- no hidden economy/rewards;
- avoid crowding service entrances;
- reduce/disable ambient agents when distant.

### TOWN-004 — service staging
- entering a service may optionally use a short doorway/camera staging transition;
- no forced long loading animation;
- UI still remains the actual functional service;
- restore exact town position/camera when closing.

### TOWN-005 — interiors, only where worthwhile
Do not make an explorable interior for every management screen by default.

Good candidates for later interiors:
- General Store;
- Tavern;
- Adventure Guild.

Research, Milestones and Facility Planning can remain fast UI overlays unless an interior adds
meaningful gameplay.

## Acceptance criteria

- New Game still begins with 2D Main Menu and mixed character creation.
- Ranch -> Town -> Ranch works without changing day/gold.
- Town area survives save/load.
- General Store spatial entry opens the existing shop.
- Research Office is locked until Workshop is built.
- Town service close restores Town movement.
- M in Town opens the Town Hub.
- F1 Help works independently in Ranch and Town.
- no duplicated Shop/Economy/Research/Adventure/Bond reward authority.
- inactive world area has no active player processing/camera.
- final asset admission retains provable license/source records.
