# Kanban

Updated 2026-09-12. This is the single task-status board for the accepted living-world direction in [LIVING_WORLD_PLAN.md](LIVING_WORLD_PLAN.md). Reviewed upload: `astar` / `1a65868b73e1af48277d8ac9ac338e3edcc5ce34`. This update changes documentation only and marks no new gameplay task DONE.

## Status and evidence rules

- **OPEN:** specified, not accepted as implemented.
- **PARTIAL:** named artifacts or a bounded implementation exist; acceptance below is still required.
- **BLOCKED:** stated dependency or failed verification prevents the named completion; independent asset work may continue.
- **DONE:** exact tested scope, commit and evidence recorded. A generated file, proposed rule or historical suite count is insufficient.

Owners below are work roles, not claims that an agent is currently running. Dependencies use stable IDs. Update status/evidence here rather than keeping conflicting checkbox lists in every design document. [ASTRA_HANDOFF.md](ASTRA_HANDOFF.md) selects the next action; [KNOWN_ISSUES.md](KNOWN_ISSUES.md) records defects and limitations. Historical tasks and their full evidence remain in [the prior board](archive/20260912-before-living-world/KANBAN.md).

## Living world — implementation queue

| ID | Priority / owner | Status and dependency | Completion scope | Current evidence / next action |
| --- | --- | --- | --- | --- |
| LW-01 | P0 / integration + local QA | OPEN | Establish build/import/smoke/rendered baseline of the actual uploaded branch; separate old failures from new ones. | Prior `4ffd97c` counts do not certify `1a65868`. Record exact new code/asset revision and actual results. |
| LW-02 | P0 / local world design | OPEN; start alongside LW-01 | Joint terrain/town-growth plan with plausible drainage, coherent routes, expansion reserves and permanent breathing room. | Regional JSON exists, not an accepted 300-inhabitant settlement plan. Measure scale/walk times; agree complete footprints before fixed exports. |
| LW-03 | P0 / integration + local world | OPEN; LW-02 | Map stable services into buildings/rooms/objects. Kitchen and ranch office move inside the main house without duplicated commands or lost unlocks. | Current JSON requires separate plots. Produce a service-to-place map and replace constraints deliberately, not by dropping service IDs. |
| LW-04 | P0 / local art + QA | PARTIAL; LW-01 | Metre-scale figures, house, fences and furniture match visible/collision/door/camera dimensions across supported samples. | One neutral 1.62 m capture and scale-yard assets exist. Test more bodies and import/parent scales; no universal fit claim. |
| LW-05 | P1 / local art + integration | PARTIAL; LW-02, LW-03, LW-04 | Authored main house is used by ordinary play with kitchen/office/private rooms, real approaches and preserved tutorial/night functions. | 20 x 16 m house and 12 x 12 m wing are prototypes; 7.2 x 6.4 m production plot is not compatible. Expand/replan, do not rescale the house into it. |
| LW-06 | P1 / gameplay design + integration | BLOCKED: explicit household/capacity decision | Resolve player/roster identity, uncapped recruitment versus finite beds, stable room assignment and meaningful expansion. | Habitation audit found no allocation system. Seven sample beds are not capacity. No automatic new recruitment cap, rent or upkeep. |
| LW-07 | P1 / local furniture/animation + integration | PARTIAL; LW-04, LW-05; allocation depends on LW-06 | Collision, reservations, approach/exit and neutral sit/lie use for supported figures; no occupied overlap or stale reservation. | 15 furniture GLBs; recorded furniture collider count zero; no wired articulated furniture-use rig. State rig/variant blockers rather than rotating a standing proxy. |
| LW-08 | P1 / local terrain + integration QA | BLOCKED: rejected coastal candidate; LW-02 | Dry full-width ranch-town-shore route, correct heights/collisions and coincident seams walked by player and companion. | Recorded 0.1394483787870735 deviation exceeds 0.12; stream cuts valley lane. Resolve crossing, re-export and walk it. No acceptance-threshold reduction. |
| LW-09 | P1 / source/gameplay design | OPEN; may run alongside art | Source/remake audit of civic identity, taxes, mana/energy/points and facility growth; specify payment versus voluntary support. | Claims from discussion are unverified here. Record original paths/formulas plus current callers, pool, timing, ledger and unresolved choices. |
| LW-10 | P1 / local architecture + design | OPEN; LW-02; economic binding LW-09 | Before/construction/after civic and town-growth layout; persistent anchors, safe expansion slots and real proposed uses. | Four visual stages are proposals, not source facts or approved prices. Reserve permanent green/shore areas. |
| LW-11 | P1 / local art | OPEN; LW-02, LW-04, LW-10 | Enterable civic/supply house, useful small market courtyard and editable material/building modules. | Main local modeling target after house integration; no universal menu pavilion or isolated beauty scene as sole delivery. |
| LW-12 | P1 / targeted integration | BLOCKED: LW-09; stage contract LW-10 | One authoritative, bounded contribution/development binding with no double charge/credit; safe save/load and replay handling. | No new town economy asserted implemented. Reuse existing services; unresolved new financial rules stay disabled/proposed. |
| LW-13 | P1 / joint delivery + QA | OPEN; LW-05, LW-08, LW-11, LW-12 | One useful civic expansion played through before/work/after from the normal world and preserved on load. | First complete slice: main house -> connection -> civic house -> market. A test-only trigger is not ordinary-play completion. |
| LW-14 | P2 / local ambience + small integration | OPEN; LW-10, LW-11 | Believable time/weather/stage-sensitive street life with stable important NPCs and bounded background population. | About 300 inhabitants is an impression, not 300 records/agents. Prevent visible pop/despawn, door blockage and merchant-state reset. |
| LW-15 | P2 / local art/audio | OPEN; LW-05, LW-11 | Cohesive materials, light, weather shelter, usage motion and actual ambient mix in existing systems. | Recorded home prototype is dim/coarse. Compare same cameras; separate listening review from file/level checks. |
| LW-16 | P2 / local modular architecture | OPEN; LW-02, LW-10; expand after LW-13 | Additional housing/work/shore quarter kit with consistent scale and access, not 300 separate detailed houses. | Complete one useful quarter before mass placement. No guaranteed benefits from empty buildings. |
| LW-17 | P1 gate / local performance QA | OPEN; each integrated slice | Repeatable actual Forward+ low/high frame/memory/navigation measurements and targeted optimization. | Compatibility evidence and low preset on a strong GPU are not low-end hardware certification. No generic streaming rewrite without a measured need. |
| LW-18 | P2 gate / integration + local cleanup | OPEN; verified replacements | Remove proven unused/replaced scenes after code/data/UID/tool/test/export reference checks; preserve sources/evidence. | File names are not non-use proof. Record removal path, replacement, evidence and revision. No broad clean/reset. |
| LW-19 | P1 gate / integration + localized UI | OPEN; new interactions as added | New civic/room/growth text uses existing keys/templates; localized layouts retain readable use and navigation. | Existing localization is partial. Preserve canonical IDs and do not reintroduce hard-coded strings as a new standard. |
| LW-20 | P1 gate / joint QA | OPEN; each slice, final LW-13 | Actual player/companion journey, furniture occupancy, upgrades, contribution conservation and save/load across stages; casual/efficient policies. | Keep original tutorial/skip/bath/free-exploration tests. Distinguish synthetic setup, walking, visual/audio review and untested hardware. |

## First work package

Start LW-01/LW-02/LW-04 and the independent source audit LW-09. Then integrate the recovered house through LW-03/LW-05; solve LW-06 explicitly instead of pretending prototype beds complete habitation. LW-08's rejected route must pass before the connected slice is claimed. Art for LW-10/LW-11 can proceed without unapproved payments. Finish one LW-13 project before expanding all neighborhoods. Local Astra's main output is usable assets and their rendered integration, not additional generic infrastructure.

## Retained non-world work and previous task IDs

These are not silently completed, deleted or made part of the art assignment:

| Existing ID / area | Current continuation boundary |
| --- | --- |
| ENDING-001 | Test real completion presentation and same-ranch Continue Ranching; service-level early completion does not certify the interface. |
| DESIGN-001 / STATIONS-001 | Meaningful equipment/upgrade benefits and policy comparisons without penalizing efficient play; gross output is not final daily net. |
| INVENTORY-001 / SETTLEMENT-001 | Targeted known repairs exist; other extreme inputs, raw legacy APIs, exception rollback and complete conservation remain separate audits. |
| CORE-003 | Night-training multiplication was repaired in the later daily-loop work, not still a fresh defect by default. Reproduce before changing it again; historical board retained. |
| CORE-004 | Command reentrancy/ledger repairs do not prove universal settlement rollback or every event/upkeep path. |
| I18N-001 / UI-001 | Existing partial translation, stepwise creation, service/results density, full actions and physical-device acceptance remain open. |
| TOOLS-002 / TOOLS-003 | Development MCP limits/auth/export policy and documented engine/driver diagnostics remain independent; no engine upgrade in the world slice. |
| DATA-004 / PARITY-001 | Runtime data/reference integrity and original-engine differential parity need evidence; importer regeneration remains guarded. |
| CHAR-002 / ART-002 / MORPH-001 / ANIM-001 | Existing identity/design/rig gates and exact asset evidence still apply. Ambient population is not permission for unreviewed character designs or mass conversion. |
| WORLD-003c/003d, TOWN-001, WORLD-004 and earlier DONE entries | Historical implemented baseline scopes stay recorded in the archive; they are not duplicate new TODOs or current all-route proof. |
| AI-001 / TOWN-002 / PERF-001 | Their world-production/navigation/performance continuation maps to LW-08/LW-11/LW-14/LW-16/LW-17; do not maintain a contradictory parallel checklist. |

## Updating this board

For a change, retain the ID and replace its status/evidence with the exact delivered scope, tested commit, command/result and relevant image/audio/trajectory paths. Split a task if only a bounded subcase is accepted. Update the handoff with the next action and the issues document with remaining failures. Do not mark all stages DONE because the plan itself has been committed. Historical evidence remains linked, never recycled as a test of a new upload.
