# Living coastal world — ranch, inhabitable places and town growth

Updated 2026-09-12 from the user's world-planning discussion and a documentation/source review of `astar` at `1a65868b73e1af48277d8ac9ac338e3edcc5ce34`.

**Status: accepted direction with open implementation and design decisions.** This document does not approve final art, invent finance rules or certify the uploaded assets as playable. [KANBAN.md](KANBAN.md) is the single implementation-status board; task IDs `LW-01` to `LW-20` below refer there. [DECISIONS.md](DECISIONS.md) records the durable decisions D-013 to D-017. [ASTRA_HANDOFF.md](ASTRA_HANDOFF.md) identifies the next work and the evidence boundary.

## 1. What the user has asked for

| Requirement | Direction to preserve | Not implied |
| --- | --- | --- |
| Places, not menu stations | Functions belong in plausible buildings, rooms, furnishings and people. Own housing and important public buildings are enterable and useful. Kitchen and ranch office belong in the main house. | One exterior building for every service ID; an automatic universal menu on entering every room; every private town house open from the start. |
| A larger coherent region | Ranch, dry valley routes, town and an accessible coast/sea form a believable region. Terrain, drainage, roads and buildings explain one another. Leave expansion space and permanent breathing room. | A huge empty map; arbitrary curves; scaled-up doors/furniture; a new seamless-streaming engine before a useful walking route works. |
| Habitation and upgrades | Real ranch residents need suitable beds, seats and usable rooms. Extensions add space and equipment at consistent scale. | Seven example beds proving unlimited capacity; new recruitment caps or upkeep invented to fit an asset; merely rotating a standing mannequin as a completed lying animation. |
| A living town | The eventual settlement should suggest roughly 300 inhabitants, with merchants and everyday life. The user explicitly says they need not all be rendered or individually exist. | 300 permanent NPC records, 300 full AI agents, 300 individual houses, or player micromanagement of town bedrooms. |
| Visible response to the ranch | The town can start around a modest civic/supply house and develop as the ranch helps it. Contributions, possible taxes and mana/energy support should be understandable and visibly useful. | Already approved new tax rates, resource conversions, a landlord role, an implemented town treasury or mandatory waiting days. |
| Local asset production | Local Astra should model, texture, furnish, animate neutral usage where supported, integrate and inspect the work in Godot. | Another code-only pass, endless concept generation, an engine upgrade or many disconnected show scenes. |
| Project order | Truly unused/replaced scenes may be removed after reference and replacement checks. | Deleting by filename, removing failed tests, deleting editable sources/evidence, or discarding another agent's work. |

The architectural layouts, four growth stages and first-project examples below are **implementation proposals**, not original-game canon or fixed balance. Do not silently promote provisional dimensions, household examples or numeric thresholds from earlier chat into requirements. The latest explicit population clarification supersedes earlier suggestions for 300 persistent citizens.

## 2. Reviewed starting point: assets are not the same as integration

The following is supported by the uploaded documents/data at the source commit, not a new Godot run:

| Source | What it establishes | Open consequence |
| --- | --- | --- |
| [Ranch asset recovery](art/RANCH_ASSET_RECOVERY.md) | Editable main house 20 x 16 m, 12 x 12 m wing, 15 furniture/fence GLBs and a furnished authoring scene. A recorded Forward+ capture inspected the prototype. | The prototype is not referenced by the production ranch. Furniture collision count was zero in that capture; usable sit/lie transitions and live housing remain open. |
| [Habitation audit](art/RANCH_HABITATION_FACTS.md) | No designed ranch-resident upper bound or actual bed/room/seat allocation system was found. Player versus roster `anon` overlaps in identity; normal JSON and seed-only capacities differ. A later neutral stand-in height correction is documented. | Re-audit current code before integration. Do not infer housing capacity from the catalog, UI constants, camera-visible actors or seven example beds. Preserve character identity and existing gameplay until the housing policy is decided. |
| `data/world/organic_layout.json` | Main-house plot remains 7.2 x 6.4 m; kitchen and office remain separate plots. Regional ranch/town bounds and coastline data are proposals; the service core is still compact. | A 20 x 16 m house plus wing cannot simply replace the old shell in its old reserved footprint. Update the plot/room/approach contract without multiplying game services. |
| [Coastal execution plan](execplan/COASTAL_REGION.md) | Ten regional GLBs exist, but the candidate is explicitly rejected: full-width ground deviation 0.1394483787870735 exceeds 0.12; stream and valley lane cross without a resolved dry passage. | Reroute or author a deliberate crossing, re-export and re-test. No threshold weakening, route teleports or production activation merely because export files exist. |
| `src/World/CoastalRegionWorldHost.cs` and `CoastalRegionController.cs` | Coastal hosting is opt-in; the ordinary host's subtype cast does not activate it. The controller contains seam handoff logic. | Not proof of walked seam continuity, companion arrival, production save/load or generalized streaming. |
| Prior handoff and known issues | They report 1,889 smoke assertions and 526 rendered checks for `4ffd97c`, an earlier code checkpoint. | Those numbers must not certify the later `1a65868` local asset upload. Establish a new exact-head baseline in LW-01. |

Claims in the discussion about original tax distribution and town-facility leveling are **not independently verified in this documentation pass**. LW-09 must cite the actual original paths/formulas and current remake callers before wiring contributions. A similarly named resource or facility is not sufficient evidence.

## 3. Place hierarchy and daily use

Use the hierarchy **region -> building -> room -> furnishing/contact -> action**. Keep gameplay identifiers separate from spatial identifiers: multiple existing service actions may share one building, and one room may have several meaningful interaction points. Preserve localization keys, save references, tutorial targets and unlock reasons during relocation.

### Ranch center

The preferred main-house arrangement is an office near the entrance, shared kitchen/pantry/dining facing the work yard, a modest common room, and private bedroom/bath away from through traffic. Keep important everyday rooms on a convenient level initially. Workshops, barns and genuinely different industrial/animal functions can remain separate buildings. Resident wings or additional homes surround the same working center rather than moving daily functions farther apart with every upgrade.

Show meaningful actions on a desk, stove, bed, bath or relevant NPC. Do not open a screen just because a player crosses a doorway. Plans persist until changed; a routine job must not require daily confirmation at several pieces of furniture. A compact journal can provide information and convenient overview without becoming a second command authority or another mandatory device. Outside guidance should lead to the entrance, then the room; do not permit through-wall interaction or place every indoor marker above the roof.

### Town

Group uses into real places: store with stockroom and possibly a dwelling; tavern with kitchen and common room; craft premises with delivery access; public bathing spaces; civic office with a supply yard. Public contacts and existing services remain available in coherent early premises even when the later town is not built. Research office and ranch office are not automatically the same service.

Private residences may stay closed where there is no purpose for entry. Their facades, doors, gardens, upper floors and suggested occupancy must still make sense. Avoid a street of identical teaser doors or a separate service pavilion for every menu.

## 4. Landscape, growth reserves and scale

LW-02 plans the final regional structure before detailing another fixed ground layout. Preserve the documented common coordinate/height convention unless a measured change is justified. A candidate structure is wooded high ground, ranch terrace and meadows, a descending valley lane, town on dry terraces, and a bay with an accessible shore and modest pier. Roads follow terrain and use deliberate bridges. Maintain complete path widths and door/approach envelopes, not only centerline clearance.

The latest coastal numerical bounds are blockout inputs, not approved final settlement size. Expand usable land if the coherent plan needs it; never enlarge bodies, doors or furniture to make a town seem bigger. Streets, buildings and courtyards may align or curve for terrain, parcel and use reasons. Being non-grid is not an acceptance test in itself.

Reserve future housing/civic/workshop plots early. Give unused future plots plausible temporary uses such as gardens, orchard, modest storage or pasture. Separately designate permanent green space, views, shore access and paths that will **not** be filled by the final stage. Keep frequent errands compact and preserve convenient existing travel. Measure actual walk times rather than compensating for poor layout by changing player speed, day length or stamina charges.

LW-04 establishes a common metre-scale asset reference and checks imported world bounds, model/export/parent scaling, bodily heights, furniture, doors, fences, colliders, camera and agent clearance. Measure body height separately from hair/horns/accessories and test representative supported body sizes. Do not equalize character heights or apply destructive rig transforms to fit undersized furniture. Interior volumes must fit the exterior. An upgrade adds a room/wing/equipment; it does not inflate the complete house.

## 5. Ranch habitation versus town atmosphere

Permanent ranch residents require a deliberate housing policy, suitable sleeping places and stable identity-aware assignments. The current uncapped recruitment and player/roster identity overlap must be resolved explicitly in LW-06; do not invent a resident cap, duplicate bed for one identity or new rent to hide an architectural shortage. Finite example bedrooms are not a completed unlimited-capacity solution.

Beds, chairs and benches need measured approach, pose, clearance and exit points. Separate permanent bed assignment from transient seat reservation. Test simultaneous users, cancellation, actor departure, new/load session and physical upgrades. Release or rebuild transient reservations correctly; persistent assignments cannot depend only on live scene nodes. Keep bedroom allocation and household growth within the existing save architecture, with a reviewed minimal extension where actually required.

Use neutral sit/lie assets for suitable existing rigs and validate physical contact. The current recorded stand-ins have no wired articulated furniture-use animation. Modeling furniture and placing socket nodes does not complete those motions. Do not claim compatibility with every character from one neutral pose or alter identity/design clearance. If an appropriate rig is absent, preserve the finished furnishing and document the concrete animation dependency rather than fabricating a demonstration.

Town background population is different. Roughly 300 inhabitants is a final impression/architectural target, not mandatory persistent actors. Important merchants, story contacts and companions retain continuity; generic passersby can be pooled and omitted while unseen. Buildings and ambient cues imply the rest. These background residents do not automatically join the ranch roster or need player-assigned beds.

## 6. Civic center, contributions and visible growth

### Civic identity and resource decisions remain open

Start from a modest civic/supply house with a named role supported by the story. A mayor, local administrator and landlord are not interchangeable; do not invent rent on property the player owns. A desk handles accounts; planning has a readable context; a supply yard or appropriate existing mana facility explains deliveries.

Before implementation, LW-09 records the actual source and remake contracts for:

- existing mandatory tax/upkeep obligations versus voluntary additional contributions;
- gold, personal mana, stored mana, Spirit energy and contribution points;
- current recipients, deduction timing, ledgers, thresholds and saves;
- any source facility-level distribution and what is or is not ported;
- relationship to already paid community deliveries and ordinary goods sales.

Do not collapse these resources into a fictional interchangeable energy pool. Do not spend personal daily stamina as a tax. Do not repurpose existing points solely because their name sounds suitable. Proposed gold/mana support is the user's direction; specific pool, rates, conversion, debt and project thresholds require verified rules/design decisions.

### Accounting and player comfort

Use the existing root/services for actual payments and one consistent development record. A paid tax reused for municipal development is not a second deduction. Selling goods is not donating those same goods again. A rewarded delivery may be recognized where deliberately designed, but cannot be consumed, paid or counted twice through another entry point. Preserve save/load, capped arithmetic, stale-view rejection and reentrant/repeated-event protection.

Keep recurring accounting automatic where it already exists. A first introduction and occasional project choice are enough; no daily mandatory trip to a desk. New automatic deductions require an explicit understandable opt-in and bounds. Do not add arbitrary tax rates, penalties, rent, escalating compulsory maintenance or extra waiting days as part of an art pass.

Support efficient play: use actual contributions and suitable milestones, not elapsed days alone. Construction may use a natural safe transition, but must not require extra sleeps just to meet a hidden minimum date. Completed public improvements persist; missing a voluntary contribution does not demolish homes or revoke established services. The town contributes its own labor and activity, so the ranch is a partner rather than manager of every civic chore.

### Proposed visual stages, not fixed balance

| Stage proposal | Visible pattern | Functional direction |
| --- | --- | --- |
| Supply settlement | Modest civic house, basic dwellings, simple stalls and landing | Existing core contacts and necessities in small/shared premises. |
| Market settlement | Covered stalls, improved supply yard, a housing courtyard and better local paths | Useful existing services, comfort and more social activity. |
| Growing coastal settlement | Craft yards, expanded tavern/public spaces, shore route and further homes | Additional authored contacts/uses where genuinely implemented. |
| Small coastal town | Several coherent quarters, developed civic house and appropriately sized waterfront | Impression of about 300 inhabitants, new possibilities and enduring visible payoff. |

Do not remove an already accessible service simply to make the first stage look poor. New buildings need not all give income multipliers. Better access, seating, shelter, contacts, stories and comfort are valid rewards when real and useful. New merchant inventories, bonuses and economic unlocks remain separate deliberate work, not an unbounded prosperity feedback loop.

For each first project define before/construction/after geometry, actual trigger, visible local work, safe activation point, resulting use, preserved entrances, contribution feedback and exact replay/save tests. A permanently unfinished scaffold or an empty 'coming later' menu is not completion. Do not pop walls into occupied spaces. Collision, navigation, furniture and access must change coherently; do not leave old invisible colliders under a new facade.

## 7. Breathing town and environmental presentation

Derive life from existing area, time, weather and development states. During the day show suitable working/shopping/resting groups; in the evening shift activity toward common places and window lighting; at night reduce it; rain moves suitable activity under shelter. Avoid synchronized starts, repetitive circles and crowds without destinations. Background ambience is not a new production or reward simulation.

Ambient pooling must not visibly spawn/despawn people in conversation or reset important shop/story state. Keep entrances free and reserve seats briefly when used. Use existing appropriate characters/neutral motion, not a new mass character-design task. Density follows measured budgets; an illustrative visible-NPC count is not an untested hardware guarantee.

Materials, lighting, animation and sound must remain coherent with the existing illustrated anime direction. Full tutorial/interaction information stays accessible without filling the world with floating labels. Reuse day/weather/audio systems; no rain through roofs, doubled ambient loops or another world clock. Check actual audio mix and existing volume controls. Distinguish measured/integrated audio from an outstanding listening review.

Test real Forward+ output and the existing low/high settings. Compatibility screenshots are separate evidence. Measure a repeatable route under fixed conditions, without concurrent self-started rendering load. Keep nearby detail and distant simplification appropriate, but do not build a general streaming framework without demonstrated need.

## 8. Production sequence and ownership

Implementation status lives only in [KANBAN.md](KANBAN.md). These phases express dependencies, not completed tasks:

1. **Establish the uploaded baseline and master spatial plan** — LW-01 to LW-04. Audit the source snapshot, test current code, check scale, map functions into rooms and reserve complete growth footprints.
2. **Make the existing assets usable** — LW-05 to LW-08. Integrate main house, resolve housing policy, implement collision/furnishing use and repair the coastal route. Readable blockouts precede detailed dressing; walking tests precede claiming connectivity.
3. **Prove one civic development** — LW-09 to LW-13. Decide source-grounded finance/role contracts, author civic house/market plus construction/final assets, connect an actual approved trigger and demonstrate a new useful place.
4. **Expand atmosphere and later quarters** — LW-14 to LW-16 and LW-19. Small ambient population, coordinated audio/weather, modular neighborhood kit and keyed text, without a 300-agent prerequisite.
5. **Accept, measure and clean incrementally** — LW-17, LW-18 and LW-20 are gates throughout, not a final excuse to postpone every check.

**Local Astra / art-world work:** use Blender, Material Maker, Krita and the local Godot renderer for actual assets, room arrangement, meaningful variant geometry, materials, neutral usage animation where supported, audio and measured world review. Existing sources/exports are preferred over another prototype family. Modeling is explicitly desired; a code-only generator or screenshots of an unintegrated show scene are not the intended main deliverable.

**Targeted gameplay/integration work:** map stable service IDs to rooms; resolve housing identity/capacity; bind a reviewed contribution/development contract; preserve root commands/saves/localization and integrate rendering lifecycles. Keep the interface small and documented. Do not refactor the economy, mental/trait systems, dialogue architecture or global character rules just to complete an asset task.

If a financial or housing rule is unresolved, local modeling can continue on a clearly identified visual prototype and test interface, but it must not be described as ordinary-game integration. Do not invent deductions or admit all recruits to seven beds to obtain a green demo.

### First complete delivery

Target **main house -> useful ranch rooms -> real connection -> civic/supply house -> modest market courtyard**, plus **one** visible civic project through before/construction/after and an actual use after completion. The full growth layout is planned, but later quarters can remain lower-detail. If LW-09 is not resolved, deliver source assets and the precise integration decision rather than pretending the financial loop ships.

## 9. Scene cleanup and durable sources

Unused/replaced scenes may be removed only with evidence. Check boot scenes, autoloads, inherited/instanced scenes, resources/UIDs, code including dynamic paths, data, editor tools, exports and tests. Names such as `dev`, `test`, `old` and `Greybox` are not evidence of non-use. Old kitchen/office exterior entries can retire only after verified replacement routes, tutorial/upgrade behavior and command equivalence.

Keep editable .blend/material/texture/audio sources and appropriate runtime exports. Track foreign assets with actual license, origin and attribution. No unapproved paid generation or cloud uploads. Backups/duplicate generated textures can be cleanup candidates, not automatic deletions. Preserve personal saves, test evidence and another worker's dirty files; use scoped reversible changes, never broad clean/reset commands.

Record each scene removal with old path, references checked, replacement or reason, tested revision and test result. Uncertain dynamic use remains an open candidate. Keep useful reusable modules and verification scenes: a clean project is understandable, not merely small.

## 10. Acceptance, evidence and maintenance

A task moves to DONE only when its own stated scope is met. Distinguish **planned**, **modeled**, **integrated**, **automatically tested**, **visually inspected**, **audio-reviewed**, and **human design approved**. An asset receipt or high suite count is not proof of habitation, physical walking, original-game parity or final art approval.

For each completed task, update its KANBAN evidence cell and handoff with:

- task/requirement ID and exact code/asset commit (dirty local work must be identified as such);
- edited scene/source/export paths and source/export provenance;
- actual commands, results, image/audio/trajectory paths and relevant limits;
- dependencies, open blockers and the next small action;
- whether setup used synthetic residents/resources/time and what was truly played.

Inspect player and companion walking through doors, slopes, bridges and area seams without teleporting past failures. Compare before/after images from the same cameras. Verify required services remain available, world input returns correctly, house states and payment/development records survive load, and repeated view/loading events do not double credit. Test several supported body sizes and simultaneous furniture users. Preserve the original tutorial/skip, free exploration, bath/next-morning bonus, existing assignments, combat and receipts.

Compare casual and efficient progression before adding new benefits/costs. Check building variants at day/evening/night/rain, low/high quality, actual Forward+, and real playback where possible. Physical devices, complete campaign balance and untested renderer/hardware remain explicitly open.

### Deliberately unresolved decisions

| Decision | Required next evidence | Blocks |
| --- | --- | --- |
| Civic official versus landlord; ownership story | Current lore and dialogue/source references | LW-09's final role/wording, not neutral building shell modeling. |
| Which mana/energy resource, obligations, voluntary contributions and distribution | Original formula/path plus current service and ledger audit; no unsupported conversion | LW-12 financial activation and ordinary-play LW-13. |
| Exact project thresholds, completed use and any new benefit | Existing progression mapping and policy comparison | Final project rules, not before/after asset variants. |
| Unbounded ranch recruitment, household identity and finite bedrooms | Rechecked roster/player identity and explicit capacity/expansion policy | Production habitation claims in LW-06/LW-07. |
| Final town footprint, districts and construction slots | Measured scale, traversable terrain and growth-envelope review | Final landscape export/placement, not current asset recovery. |

These are tracked work, not a request to stop the whole local session for unspecified approval. Work on independent assets inside the accepted direction while keeping blocked integration honest.
