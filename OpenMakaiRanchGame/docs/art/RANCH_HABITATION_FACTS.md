# Ranch habitation — implemented facts

Audit: 2026-09-12, branch `astar`, live working tree (including concurrent organic/coastal presentation edits). Source audit only; no gameplay edits, save inspection, build or rendered acceptance. Paths below are relative to `OpenMakaiRanchGame/`. Read root `AGENTS.md` and current handoff/decisions/issues first; older floor-plan counts are not runtime authority.

**Subsequent implementation note:** this audit predates the scale repair and authored asset prototypes. The scale-trap calculations below describe the old implementation, not current measured height. `CharacterAvatar3D` now calibrates the active mannequin's bodily bounds/foot origin, and `RosterRig` supplies persisted instance height directly rather than relying on `DefinitionFor`. `DefinitionFor` itself still omits the extended metadata; this was not a general appearance/identity repair. The later Forward+ fixture measured the existing neutral stand-in at 1.62 m against a 1.62 m target. See `RANCH_ASSET_RECOVERY.md` for exact capture evidence and the separate furnished prototype. Resident caps, bed allocation and articulated furniture-use APIs were **not** added by those changes.

## Resident counts and actual limits

| Population / quantity | Implemented fact | Source |
|---|---|---|
| Loaded character catalog | **10 rows / 10 unique IDs**: `anon`, `ayaka`, `en`, `kagura`, `maria`, `noir`, `rancher`, `sharon`, `slay`, `yukina` | `data/characters.json`; `src/Data/DataRegistry.cs:29–117` |
| Fallback character catalog | **10 unique definitions**, same IDs; not an additional population | `DataRegistry.cs:121–133` |
| Fresh `CreateNewGame()` roster | **2 entries**, `anon` and `rancher`; both assigned `rest` | `src/Gameplay/SaveStateFactory.cs:31–74` |
| Player | Separate `SaveState.Player` object, default name Anon, height 1900 mm, apparent age 20. Do **not** call this three distinct people: roster `anon` and the separate player representation overlap in identity; no household deduplication contract exists | `SaveStateFactory.cs:80–102`; `src/Core/Models/SaveModels.cs` |
| Optional generated starting recruits | `RerollGeneratedRecruits` removes previously tagged starting recruits and adds **2 generated entries**, preserving hires/captures. With the base roster and no other additions this yields **4 roster entries**; not the unconditional fresh-game count | `SaveStateFactory.cs:129–156,194–202`; `src/App/GameRoot.cs:258`; `src/Ui/UiShellController.Screens.cs:41` |
| Pending hire offer | **1**, saved separately; not yet resident | `SaveStateFactory.cs:74`; `src/Gameplay/ManagementServices.cs:551–615`; `SaveModels.cs` recruitment state |
| Permanent roster growth | **No implemented gameplay resident upper bound.** Hire checks catalog availability, offer and payment, then appends. Offers repeatedly reuse catalog archetypes with new instance IDs; catalog exhaustion does not stop recruitment | `ManagementServices.cs:574–615`; `SaveStateFactory.cs:160–239` |
| Additional acquisition | Both adventure completion and combat capture append generated recruits to the same roster, without bed/room checks | `ManagementServices.cs:278–281`; `src/Gameplay/CombatServices.cs:421–424` |
| Active companion | **At most 1 active outing partner**, selected from existing roster; not an extra resident. Partner history is separate from active partner | `src/Gameplay/DatingService.cs:30–40,97–115`; `SaveModels.cs:521–532` |
| Visitors | No separate persisted visitor/guest occupancy collection or visitor-to-resident admission service found. `VisitService` is care for roster characters, not visitor spawning. Intro `ChildhoodFriend` is an authored presentation node, not evidence of an extra permanent roster member | `ManagementServices.cs:795–924`; `scenes/dev/RanchHouseIntro.tscn:143–152` |
| Physical bed/room/seat allocations | **0 implemented allocation systems found**: no resident bed ID, room ownership map, seat reservation or occupancy enforcement | `SaveModels.cs:336–421`; recruitment paths; UI room assignment below |

Counts were parsed/deduplicated with Python from actual JSON/C# source. No personal save was read; current residents in any user's played save are unknown. Finite memory/identifier space is not a designed resident cap. A finite set of modeled bedrooms cannot currently be advertised as housing every possible permanent recruit.

`RosterRig.Refresh` iterates the roster, keys avatars by instance ID, and filters only for companion-only mode; it does not establish a residence limit (`src/World/RosterRig.cs:201–250`). Auto-party fill toward three in recruitment is not a household capacity. Never infer bedroom count from camera-visible avatars.

## Capacity and unlock table: UI is not simulation

**Normal JSON path loads 9 facility IDs, all with omitted `Capacity`, hence default 0.** IDs: `bathhouse`, `dairy_barn`, `guest_room`, `kitchen`, `pasture`, `pharmacy_lab`, `storage`, `well`, `workshop`. `DataRegistry.CreateSeeded` returns after successful JSON loading; fallback facilities are **not merged** into this catalog. Fallback seed contains **43 unique facility IDs** (`src/Data/DataRegistry.cs:29–51,714–760`; `src/Core/Resources/GameDefinitions.cs` FacilityDefinition).

| Legacy living-building ID | UI capacity | UI starts open? | Normal JSON definition / capacity | Fallback seed capacity |
|---|---:|---|---|---:|
| `office` | 1 | Yes | Absent | 1 |
| `private_room` | 1 | Yes | Absent | 1 |
| `barn` | 3 | Yes | Absent | 3 |
| `guest_room` | 2 | Only when facility level > 0 | Present / 0 | 2 |
| `dormitory` | 4 | Yes | Absent | 4 |
| **UI sum** | **11** | **9 initially open** | **Not a resident limit** | **11 nominal** |

Source: `src/Ui/UiShellController.Screens.cs:67–107,4091–4178`; `data/facilities.json`; `DataRegistry.cs:716–720`.

- Ranch overview assigns at most one displayed character per building **by roster list index**, not saved residence. Its other facility-space display uses a hard-coded living-building occupant count (`UiShellController.Screens.cs:244–247`).
- “Room Assignment” displays those constants, but its Assign button opens **job scheduling**, not bedroom assignment (`:4162–4178`). Capacities do not multiply with facility level.
- `RanchState` stores facility levels and stockpile, not rooms/beds (`SaveModels.cs:336–347`). Starting saved levels are only **pasture 1 / kitchen 1** (`SaveStateFactory.cs:67–68`). UI's always-open buildings are not proof of matching saved facility levels.
- `guest_room` becomes UI-open after its first paid facility upgrade. Upgrade service has no prerequisite-wing/research/room unlock graph: it checks known definition, valid numeric level/cost and affordability, then increments level. Existing arithmetic is base build cost + 75 per current level; representability guards are not authored upgrade stages (`src/Gameplay/RanchService.Facilities.cs:14–44`).
- Seed-only `office_extension` (capacity 2), `system_kitchen` (4), `slave_dormitory` (10) are **absent from normal JSON**. They are generic fallback facility records, not implemented main-house wings or guaranteed resident slots (`DataRegistry.cs:734–736`). Preserve IDs; do not reinterpret them into new mechanics.
- Kitchen level 2 completes the existing optional kitchen project; that project does not unlock bedrooms (`src/Gameplay/RanchProjectService.cs:20–38`).

## Existing building/prop geometry

`data/world/organic_layout.json` has **8 unique ranch plots**; current main house footprint **7.2 × 6.4 m**. Kitchen and office each have separate **6.2 × 5.6 m** plots. Other IDs are dairy_barn, pasture, workshop, pharmacy_lab, pet_care. `RanchBuildingPlots.All` now loads this JSON via `OrganicWorldLayout`, rather than old fixed positions (`src/World/RanchBuildingPlots.cs:39`; `src/World/OrganicWorldLayout.cs:24–57`).

Current shells remain generated `WalkInBuilding` nodes, not a Blender-authored habitable main house. `SetFacilityLevel` maps to **visual grades 0–3**, adds a wall shelf at grade 2 and wall panel at grade 3, and keeps shell/floor area fixed at higher simulation levels. **No annexes, upper floors or stage-specific bedroom unlocks** (`src/World/WalkInBuilding.cs:29–40`; `RanchBuildingPlots.cs:41`).

Existing defaults: door **1.8 m wide / 2.55 m high**, wall **3.2 m**. Ranch-house branch creates **one bed prop** sized **1.4 × 0.6 × 2.15 m**, pillow and bath proxy, plus generic work/storage props. This is not resident allocation. Kitchen counter is **0.9 × 0.96 × 1.9 m** and currently belongs to the separate kitchen branch (`WalkInBuilding.cs:10–13,102–120`). Neither office nor kitchen is currently a room within the house.

## Bodies, rigs and furniture-use APIs

| Component | Actual implemented support |
|---|---|
| `CharacterAvatarFactory` | Always builds debug profiles; active resource `res://scenes/dev/GenericCharacterPlaceholder.tscn`. `CreateProfile`, `BuildAvatar`, `CanUseRealAvatar` exist. Real-avatar gate alone loads no new asset (`src/Character/CharacterAvatarFactory.cs:24–76`). |
| Active NPC mannequin | **7 rigid MeshInstance3D nodes**, **0 Skeleton3D**, **0 AnimationPlayer**, **0 clips**. Separate whole arms/legs, no elbow/knee articulation or skinning (`scenes/dev/GenericCharacterPlaceholder.tscn`). |
| `CharacterAvatar3D` | Public `Rebuild()` and `PlayLocomotion(float horizontalSpeed, bool sprinting)`. Animation cache searches idle/walk/run/jog and plays through `AnimationPlayer.Play(..., customBlend: 0.15)`, but current mannequin contains none, so locomotion call has no clip to play. No Sit/Lie/pose/IK/furniture-use API (`src/Character/CharacterAvatar3D.cs:48–124,161–200`). |
| `PlayerAvatar3D` | Procedural capsule/head/hair/eyes; optional horns and glasses, coarse long-hair mass. `RefreshFrom(PlayerState)`, `VisualScale`, `AnimateMovement`; movement is whole-root vertical bob, not skeletal walking. No skeleton, clips, elbow/knee rig or sit API (`src/Character/PlayerAvatar3D.cs:18–24,48–169`). |
| Existing lying presentation | Intro `WakeVisual` uses PlayerAvatar3D rotated `(0,90,90)`; controller hides it and reveals/enables player when awake. **Static whole-avatar rotation**, not lying animation or general reusable bed system (`scenes/dev/RanchHouseIntro.tscn:138–141`; `src/World/IntroHouseController.cs:54–81`). |

**Scale trap:** Factory parses definition height in centimetres, clamps 1.45–2.25 m, with short/tall/default fallbacks 1.55/1.82/1.70 m. External model scales uniformly by `clamp(Profile.Height / 1.8, 0.78, 1.25)`. However authored mannequin head top is **2.01 m**, debug marker top **2.20 m**, not 1.8 m. At default profile height 1.70, actual head top is about **1.898 m**, marker top **2.078 m**. These are geometry calculations, not measured rendered clearance. Capsule fallback is fixed-size, not height-scaled (`CharacterAvatar3D.cs:119–155`; mannequin scene).

**Metadata trap:** Production roster path calls `RosterService.DefinitionFor`, which copies identity/display/basic stats but **omits Height, skin/hair, age/eligibility and other extended metadata**. Thus current roster factory normally receives blank height (1.70 m fallback), not generated resident height (`src/Gameplay/RosterService.cs:23–46`; `RosterRig.cs:242`). Generated height data spans **1300–2100 mm** in six ranges, but that is not an implemented corresponding body range (`src/Gameplay/CharacterGenerationPools.cs`, `HeightRanges` and `GenerateHeight`). Player visuals separately clamp saved height to **1.45–2.25 m**, scale base geometry by height/1.80 and `max(0.25, VisualScale)`; horns/hair extend beyond nominal height. Body-type labels do not provide skeletal morphology. NPC equipment/talents do not attach 3D accessories in the active mannequin path.

### Neutral verification subject and concrete blockers

Use the **existing default Anon player stand-in**, unchanged (1900 mm, apparent age 20, ordinary covered placeholder, existing horns), for neutral standing/door/bed-scale checks. This reuses existing identity; it is **not** a new hero design or adult-specific asset approval. Catalog JSON has **zero explicit ConfirmedAdult entries** and no approved production character rig; numeric source ages must not be substituted for design clearance. Noir is only a historically proposed art candidate, not an available approved rig.

Neutral whole-body lying can reuse the existing intro presentation approach. Anatomically credible sitting/lying with bent knees/hips **cannot be claimed on an existing skeletal rig: none is wired in**. Moving rigid meshes is possible in a new presentation implementation, but is not an existing supported pose API. No sitting/lying clearance was rendered or tested in this audit.

Before promising complete habitation, remaining facts to resolve are: finite authored bedroom capacity versus uncapped roster; player/roster-`anon` household identity overlap; actual room/bed/seat ownership/use (currently absent); genuine articulated neutral pose assets; avatar nominal/actual scale mismatch; and kitchen/office relocation plus wing geometry (not existing unlocks). Do not silently solve these by introducing a resident cap, new prices, economy or save schema. Existing docs `art/RANCH_FLOORPLAN.md` and `BUILDING_PLOTS.md` contain historical/draft scope, not guarantees that these gaps are implemented.
