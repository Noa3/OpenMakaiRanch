# Ranch Floor Plan — Draft 1

Status: **DRAFT / FOR_WORLD-001**. Source-grounded on `DataRegistry` facilities (11) and living buildings (5). Not an approved visual direction; see `VISUAL_BIBLE.md`. Serves as the layout contract for the `RanchGreybox.tscn` scene in WORLD-001.

## Source data (from `DataRegistry.cs`, not invented)

| Facility ID      | DisplayName    | BuildCost | Upkeep | Output           | Bonus | Capacity |
|------------------|----------------|-----------|--------|------------------|-------|----------|
| office           | Office         | 0         | 0      | —                | —     | 1        |
| private_room     | Private Room   | 0         | 0      | —                | —     | 1        |
| barn             | Barn           | 0         | 0      | —                | —     | 3        |
| guest_room       | Guest Rooms    | 120       | 8      | comfort          | 1     | 2        |
| dormitory        | Dormitory      | 0         | 0      | —                | —     | 4        |
| pasture          | Pasture        | 180       | 20     | farm_goods       | 3     | —        |
| kitchen          | Kitchen        | 140       | 12     | meals            | 1     | —        |
| workshop         | Workshop       | 170       | 16     | supplies         | 1     | —        |
| well             | Well           | 160       | 10     | farm_goods       | 2     | —        |
| storage          | Storage Shed   | 130       | 6      | supplies         | 1     | —        |
| dairy_barn       | Dairy Barn     | 250       | 25     | farm_goods       | 5     | —        |
| pharmacy_lab     | Pharmacy Lab   | 300       | 20     | supplies         | 3     | —        |

Living buildings (5): office, private_room, barn, guest_room, dormitory.
Production facilities (7): pasture, kitchen, workshop, well, storage, dairy_barn, pharmacy_lab.

## Layout principles

1. **One entry point** (south gate) — camera never penetrates geometry behind the player spawn.
2. **Central hub** — office + private_room at center; all routes pass within 30 m of it.
3. **Production ring** — 7 production facilities arranged in a loose arc around the hub, each with an approach pad (interaction range for smart objects, WORLD-002).
4. **Dormitory cluster** — guest_room + dormitory + barn on the east wing, away from the pasture noise.
5. **Pasture** — largest open area, north edge; fenced perimeter, one gate.
6. **Well** — on the route between hub and pasture (natural waypoint, not a dead end).
7. **Camera clearance** — every route ≥ 4 m wide; no building closer than 6 m to any route centerline; one elevated spot (office roof or storage shed roof) for an overhead debug view.
8. **Event space** — one open clearing (12 m × 12 m) between the hub and the well for EVENT-001 staging (dialogue + bond event).
9. **Work stations** — each production facility has exactly one interaction point (the smart object in WORLD-002). No station is a dead end; all reachable from the hub in ≤ 2 turns.

## Top-down sketch (north up)

```
        PASTURE (fenced, gate south)
        |
   [well]
        |
[workshop]  HUB  [dairy_barn]
   |        (office +      |
[storage]  private_room)  [pharmacy_lab]
   |         |
[kitchen]  EVENT SPACE (12x12 clearing)
   |
[dormitory cluster: guest_room + dormitory + barn]
   |
        SOUTH GATE (entry, camera behind)
```

## Camera and movement constraints (for WORLD-001)

- Spawn: south gate, facing north (toward hub).
- Camera: third-person follow, collision-aware (wall penetration must not occur).
- Input: keyboard WASD/arrows + mouse look; gamepad left stick + right stick.
- While management UI is open, world input is released and mouse capture is released; closing UI restores input deliberately.
- Diagonal movement must not exceed axis movement speed (input normalization).
- Walls: collision on all building perimeters and pasture fence.

## Smart-object targets (WORLD-002, one per facility)

Each production facility exposes one `WorldInteractable` with:
- stable target ID = facility ID
- label = DisplayName
- approach point = pad center
- action = dispatch through `GameRoot.TryAssignJob` command boundary (no second reward path)
- availability reason = built (FacilityLevel > 0) or locked

## Acceptance criteria (ART-001)

- [x] Top-down plan with entrances, routes, work stations, interior, camera clearance and event space.
- [x] All 11 source facilities represented.
- [x] No invented facilities.
- [ ] User selects visual direction (daytime/evening/interior concept candidates).
- [ ] Greybox built against this plan (WORLD-001).

## Deferred

- Interior layouts (one interior entrance per building, not full interior for the first greybox).
- Lighting plan (derives from `CalendarState.Phase`, EVENT-001).
- Character placement per facility (AI-001, derived from `ScheduleService` assignments).
