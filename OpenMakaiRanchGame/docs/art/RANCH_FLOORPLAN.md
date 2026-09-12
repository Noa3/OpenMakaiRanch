# Ranch floor plan — inhabitable center and expandable housing

Updated 2026-09-12. **Current design direction, not a completed production floor plan.** Follow [LIVING_WORLD_PLAN.md](../LIVING_WORLD_PLAN.md), D-013/D-014/D-015 and LW-02..LW-07 in [KANBAN.md](../KANBAN.md). The original Draft 1 remains in Git at `1a65868b73e1af48277d8ac9ac338e3edcc5ce34`.

## Superseded assumptions

The first-greybox central hub/production ring, one separate facility plot per function, mandatory universal 4 m paths/6 m setbacks and deferred-interiors scope are not the final housing contract. Old seed/UI capacity and price tables are historical data, not proof of the normal JSON catalog, resident capacity or present upgrade rules. Re-read [RANCH_HABITATION_FACTS.md](RANCH_HABITATION_FACTS.md) before relying on them.

## Current physical program

| Space | Arrangement and use |
| --- | --- |
| Entrance / ranch office | Work planning and existing administrative functions near the front door, without crossing private bedrooms. |
| Shared kitchen / pantry / dining | Kitchen functions inside the main house; courtyard access and sensible supply/storage route. |
| Common room / veranda | Comfortable shared use with existing interactions; nearby outdoor rest corner rather than disconnected seating. |
| Player bedroom / bath | Quiet private area, short night/bath route and consistent introduction/return location. |
| Resident bedrooms / wing | Suitable stable sleeping places and optional shared rooms; space for real extensions, not scaled-up beds/doors. |
| Barn / workshop / distinct work areas | Separate structures only when size/use/animal or industrial function warrants it; coherent yards and access. |

The ordinary day remains convenient. No daily re-confirmation of unchanged jobs at multiple furnishings. Guidance outside targets the house entrance before the interior destination. A useful overview can remain accessible without every object opening all management screens.

## Existing asset versus required site

[RANCH_ASSET_RECOVERY.md](RANCH_ASSET_RECOVERY.md) records a 20 x 16 m main house, 12 x 12 m wing, source door/ceiling dimensions and a furnished authoring scene. The current JSON plot is 7.2 x 6.4 m and retains separate kitchen/office lots. Replan the entire footprint, roof, neighbor clearance and approach; do not distort the asset into the old reserved rectangle.

The source dimensions are measurements of that candidate, not final approved floor-space requirements. Check against actual imported supported characters and camera. Keep inside/outside volumes consistent and all relevant gameplay IDs/unlocks intact. Reuse existing WalkInBuilding/interaction architecture where appropriate rather than introducing a second authority.

## Habitation and furniture gates

The recorded seven prototype beds are examples only. The source audit found uncapped recruitment, missing allocations and a player/roster-`anon` identity overlap. LW-06 must resolve that contract before advertising total capacity. Do not add an arbitrary recruitment cap, rent or invisible housing because the asset has finite bedrooms.

Model appropriate bed/chair/bench/table/counter sizes with collision, approach, pose and safe exit spaces. Assign real ranch beds by stable identity; reserve shared seats transiently. Handle multiple users, cancellation, scene replacement, load and expansion. Use genuine neutral sit/lie poses/transitions on supported rigs; current socket/support points and whole-body rotation do not prove a working articulated animation.

Show added capacity/equipment as new rooms, wings or nearby houses. Retain orientation landmarks, main door and player essentials across stages where possible. A locked kitchen/wing can show incomplete equipment, but cannot silently unlock production. Existing numerical levels are not automatically authored room-capacity stages.

## Acceptance record required

Start with the actual normal-game site, not only `RanchHomePrototype.tscn`. Walk courtyard -> entrance -> kitchen -> office -> bedrooms/bath -> courtyard, repeat relevant routes with a companion and check furniture use with representative sizes. Test tutorial/skip, evening bath, chosen night action, upgrades, job continuity and current save/load. Record source/export/scene paths, exact revision, before/after cameras, capacities and unresolved rig/body cases.

Use LW-02..LW-07/LW-20 for status; no completed checkboxes are asserted here. Keep ordinary town ambience separate: the city impression does not require ranch-style bed assignment for every implied townsperson.
