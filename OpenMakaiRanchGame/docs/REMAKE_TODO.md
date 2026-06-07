# OpenMakaiRanch Remake TODO

This file is the long-form implementation backlog for turning the current Godot remake into a complete modern game. It has four jobs:

1. Track what is still missing from the original eraMakaiRanch project.
2. Separate public-safe remake work from private mature-extension work.
3. Record future content ideas that move the project beyond being an era-style menu game.
4. Give artists clear asset briefs for the images, UI, animation, and environment work the game will need.

## Scope Baseline

- Remake target: `eraMakaiRanch-game-eng-translation` to Godot .NET in `OpenMakaiRanchGame`.
- Runtime data strategy: do not parse era CSV/ERB files at runtime. Convert or author Godot-friendly resources instead.
- Platform target: desktop first, then mobile and web.
- Public repository stays content-safe.
- Sensitive or adult-only original systems, if any, belong in a private extension module outside public mainline.
- Public mainline should still support deep ranch, management, character, combat, relationship, and progression systems without needing private content.

## Current Godot Build Snapshot

Implemented today:

- Godot .NET scene flow: `Bootstrap`, `MainMenu`, and `Game`.
- Scene-authored main menu and gameplay shell navigation.
- Typed save model and save/load migration.
- Core service layer for calendar, economy, ranch production, schedules, roster, recruitment, inventory, shop, facilities, research, milestones, bond events, pets, adventure missions, combat reports, equipment bonuses, basic dairy production, basic training, and mental-state models.
- Layered portrait support for race, body, face, hair, and clothing layers.
- Smoke test runner covering core economy, settlement, recruitment, save/load, missions, capture flow, portraits, and scene loading.

Current authored content breadth in `DataRegistry`:

- 10 character definitions.
- 12 job definitions.
- 56 item definitions.
- 12 mission definitions.
- 9 facility definitions.
- 18 milestone definitions.
- 11 research skill definitions.
- 27 bond event definitions.
- 5 pet definitions.

Main gap summary:

- The current build is a playable systems prototype, not a content-complete remake.
- It has the outline of many original systems, but not their full data volume, branching, event depth, UI detail, tuning, or long-run progression.
- The biggest missing pieces are content pipeline, full data conversion, original-system parity decisions, modern presentation, long-term progression, and asset production.

## Priority Legend

- P0: Required before the game should be stable for heavy content work.
- P1: Required for a satisfying public demo.
- P2: Required for a complete public game loop.
- P3: Expansion and modernization work.
- P4: Optional long-term polish, live-ops, or modding work.

## P0 - Immediate Technical Blockers

- [ ] Decide long-term .NET target.
  - Current project uses `net10.0` because the local machine has .NET 10 installed.
  - For broad Godot contributor compatibility, evaluate retargeting to `net8.0` after installing .NET 8 SDK.
- [ ] Add CI build workflow.
  - Build `OpenMakaiRanchGame/OpenMakaiRanchGame.csproj`.
  - Run Godot headless smoke tests.
  - Fail CI on compile errors, missing scene nodes, missing resources, or smoke failures.
- [ ] Add export presets for Windows, Linux, macOS, Android, and Web.
- [ ] Add a content validation command.
  - Validate all IDs are unique.
  - Validate all references point to existing definitions.
  - Validate all image paths exist.
  - Validate all save migrations initialize newly added fields.
- [ ] Add an editor/content authoring guide.
  - Explain how to add characters, items, missions, jobs, facilities, research nodes, pets, bond events, and portraits.
  - Explain naming conventions and asset sizes.
- [ ] Split oversized code files safely.
  - `GameServices.cs` should be split only after tests prove no logic was dropped.
  - Suggested split: `PortraitLayerCatalog.cs`, `SaveStateFactory.cs`, `SaveService.cs`, `SaveMigrator.cs`, `RosterService.cs`, `ScheduleService.cs`, `RanchService.cs`, `EconomyService.cs`, `DayCycleService.cs`, `DailySettlementService.cs`.
  - Add one commit per safe split or one commit after full compile/smoke verification.
- [ ] Add regression tests for the service split.
  - Compile check catches missing types.
  - Smoke check catches missing save factory logic and migration initialization.
  - Add explicit tests for new game defaults, recruited characters, schedule assignment, and save migration.

## P0 - Content Architecture Needed Before Mass Conversion

- [ ] Replace hardcoded `DataRegistry` seed data with authored resources.
  - `CharacterDatabase.tres` or equivalent.
  - `JobDatabase.tres`.
  - `ItemDatabase.tres`.
  - `MissionDatabase.tres`.
  - `FacilityDatabase.tres`.
  - `MilestoneDatabase.tres`.
  - `ResearchDatabase.tres`.
  - `PetDatabase.tres`.
  - `BondEventDatabase.tres`.
- [ ] Create a one-time offline converter for legacy source data.
  - Convert legacy CSV to intermediate JSON.
  - Normalize names and IDs.
  - Detect unsupported original fields.
  - Generate a conversion report rather than silently dropping fields.
  - Import the cleaned result into Godot resources.
- [ ] Create a missing-field report for every era CSV.
  - `Abl.csv`: abilities and skill-like stats.
  - `base.csv`: base character stats.
  - `Talent.csv`: traits/talents.
  - `Train.csv`: training/action definitions.
  - `Item.csv`: items and usable resources.
  - `Equip.csv`: equipment.
  - `Cflag.csv`, `Tflag.csv`, `Flag.csv`: persistent flags.
  - `Cstr.csv`, `Str.csv`, `Savestr.csv`: text/string fields.
  - `Palam.csv`, `Juel.csv`, `Mark.csv`, `Stain.csv`, `Source.csv`, `ex.csv`, `exp.csv`, `Nowex.csv`: state/status systems that need modern equivalents or deliberate removal.
  - `Money.csv`, `Day.csv`, `Time.csv`: economy and time state.
  - `GameBase.csv`, `Global.csv`, `Globals.csv`, `VariableSize.csv`: global metadata and limits.
  - Character CSV files: player and character definitions.
- [ ] Decide field mapping policy.
  - Keep as direct gameplay stat.
  - Merge into a modern stat.
  - Convert into tag/trait.
  - Move into private extension.
  - Drop because it does not fit the public remake.
- [ ] Add stable save IDs.
  - Never base save IDs on localized display names.
  - Use lowercase ASCII IDs.
  - Keep legacy aliases for migration.
- [ ] Add content versioning separate from save schema versioning.

## P1 - Missing Era Game Parity: Characters And Creation

- [ ] Convert all original character CSV definitions into Godot character resources.
  - Preserve public-safe identity, role, job, race, broad personality, stat identity, and starting equipment.
  - Mark content requiring private handling as private-extension-only.
- [ ] Expand player creation.
  - Name, ranch name, race/species, pronoun or dialogue voice, body silhouette category, hair, eyes, skin tone, horns/accessories, starting job/background, difficulty/debt mode, starting facility package, and starting companion package.
- [ ] Add character archetype tags.
  - Ranch specialist, craft specialist, combat specialist, social specialist, scholar/research specialist, healer/support specialist, merchant/logistics specialist, rare visitor/unique recruit.
- [ ] Make generated recruits more diverse.
  - Better weighted name generation.
  - Race-specific visual pools.
  - Job-specific talents.
  - Trait combinations with incompatibility rules.
  - Rarity levels.
  - Starting loyalty/bond variance.
  - Starting contract cost variance.
  - Backstory snippets.
- [ ] Add character memory/event flags.
  - First arrival, first assigned job, first successful mission, injury/recovery events, facility preference discovery, rival/friend relationship state, personal quest progress.
- [ ] Add character lifecycle systems.
  - Training history, skill growth preferences, fatigue burnout risk, recovery/rest needs, optional retirement/leave events, promotion or specialization class.

## P1 - Missing Era Game Parity: Ranch And Schedule

- [ ] Expand schedule beyond one daily job assignment.
  - Morning, afternoon, evening assignments.
  - Rest blocks.
  - Training blocks.
  - Social blocks.
  - Town errands.
  - Mission prep.
  - Facility maintenance.
- [ ] Add schedule templates.
  - Balanced week, production rush, recovery week, mission week, research sprint, festival prep.
- [ ] Add job requirements.
  - Required facility level, required item/tool, required stat/skill, required research unlock, required character trait or role.
- [ ] Add job outcome variance.
  - Great success, normal success, partial success, mishap, injury/fatigue event, bonus discovery.
- [ ] Add job synergies.
  - Pair compatible characters for output bonus.
  - Mentor and trainee assignment.
  - Facility manager assignment.
  - Pet support assignment.
  - Equipment/tool support.
- [ ] Expand ranch resources.
  - Crops, feed, herbs, ore/materials, fabric/leather/wood, medicine, preserved food, luxury goods, reputation, town favor.
- [ ] Expand facilities.
  - Pasture, barn, kitchen, clinic, workshop, training yard, library/research room, guest house, market stall, storage warehouse, greenhouse, monster stable, watchtower, bathhouse/spa as a public-safe recovery facility.
- [ ] Add facility placement or upgrade map.
  - Even if it remains menu-driven, give each facility a visible node on the ranch map.
  - Let upgrades visibly change the facility art.
  - Add tooltips for output, upkeep, and assigned staff.
- [ ] Add upkeep pressure.
  - Food consumption, medicine/supplies consumption, facility maintenance, wages/contract fees, debt repayment, seasonal taxes or guild fees.
- [ ] Add long-run economy balance tests.
  - 30-day simulation, 100-day simulation, no-action fail state, optimal production runaway check, soft-lock check when gold reaches zero.

## P1 - Missing Era Game Parity: Town, Shop, And External Systems

- [ ] Expand town from simple buttons into locations.
  - Market, guild hall, clinic, library, shrine/temple, blacksmith/workshop, inn/tavern, harbor/caravan stop, town hall, notice board.
- [ ] Add town reputation.
  - Reputation unlocks contracts, discounts, recruits, rare items, and festivals.
  - Reputation can be local per district.
- [ ] Add contract board.
  - Delivery contracts, production contracts, defense contracts, research contracts, character-specific requests, time-limited orders.
- [ ] Expand shop systems.
  - Stock refresh by day/week/season, limited stock items, rare vendor visits, bulk purchase/sell, price fluctuation, sell order fulfillment, crafting material purchase, equipment preview before buy.
- [ ] Add item use depth.
  - Character targeting UI, batch use, item cooldowns where needed, item tags, recipe ingredient usage, gift preferences.
- [ ] Add town event calendar.
  - Weekly market, harvest fair, combat tournament, research symposium, pet show, weather emergency, visiting merchant.

## P1 - Missing Era Game Parity: Training, Mental State, And Sensitive Systems

Public mainline should keep this track non-explicit, system-driven, and safe. Anything that requires adult-only presentation should be moved to a private extension.

- [ ] Replace the current small training focus system with authored training/action definitions.
  - Public-safe categories: ranch work, craft practice, combat drills, etiquette, study, therapy/recovery, teamwork, pet handling, exploration prep.
  - Each action needs cost, time, risk, reward, requirements, and text outcome.
- [ ] Build a training action database.
  - ID, display name, category, requirements, resource cost, fatigue change, morale change, bond change, skill XP effects, mental-state effects, unlock conditions, optional private-extension hook ID.
- [ ] Add mental-state clarity.
  - Show player-readable descriptions for each state.
  - Make effects visible in tooltips.
  - Add recovery actions.
  - Add safeguards against irreversible accidental outcomes.
  - Add difficulty settings for how harsh stress/fatigue systems are.
- [ ] Add consent/configuration gates for sensitive modules.
  - Public build: safe defaults only.
  - Private extension: separate build profile, separate content folder, no public assets.
  - Feature flags must fail closed.
- [ ] Add history/log review.
  - Show recent training/actions.
  - Explain why stats changed.
  - Include filters by character, system, day, and category.
- [ ] Add support roles.
  - Counselor/therapist style recovery role, trainer role, medic role, team leader role, facility manager role.
- [ ] Add tests around sensitive system boundaries.
  - Public build has no private content dependency.
  - Private hooks can be absent without crashing.
  - Save files do not leak private-only text into public logs.

## P1 - Missing Era Game Parity: Dairy And Production Economy

Keep public implementation framed as ranch production and fantasy-resource management.

- [ ] Expand the dairy economy from auto-ship into a real production chain.
  - Collection, storage, processing, quality grading, spoilage, packaging, contracts, specialty products.
- [ ] Add production quality.
  - Poor, standard, fine, premium, legendary.
  - Quality affected by facility level, character state, equipment, research, and schedule.
- [ ] Add storage capacity and logistics.
  - Tank capacity, warehouse capacity, cold storage upgrade, shipment timing, rush shipment fees.
- [ ] Add recipes and processing.
  - Milk tea, cheese, butter, yogurt, medicine ingredient, festival goods.
- [ ] Add market demand.
  - Daily base price, contract price, festival price spikes, regional demand, reputation effect.
- [ ] Add UI for production chain.
  - Inventory by quality, storage gauges, expected daily output, shipment forecast, contract delivery progress.

## P1 - Missing Era Game Parity: Combat And Adventure

- [ ] Expand combat from report-only into readable encounters.
  - Party setup screen, enemy preview, turn log with icons, status effects, skill choices or tactics presets, replayable summary.
- [ ] Add character combat roles.
  - Vanguard, striker, defender, support, scout, healer, controller.
- [ ] Add tactical presets.
  - Balanced, defensive, aggressive, capture/control, resource-conserving, protect weak allies.
- [ ] Add enemy variety.
  - Small pests, wild beasts, bandits/raiders, rival ranch teams, magical anomalies, dungeon bosses, seasonal threats.
- [ ] Add map/dungeon progression.
  - Local fields, forest, ruins, caverns, mountain pass, haunted pasture, demon gate frontier, endgame tower/abyss equivalent.
- [ ] Add mission types.
  - Scout, gather, escort, hunt, rescue, defense, boss challenge, capture/control objective in a public-safe framing, contract delivery.
- [ ] Add risk/reward rules.
  - Injury chance, equipment durability loss, fatigue cost, rare material reward, reputation reward, character bonding reward, unlock new areas.
- [ ] Add adventure UI polish.
  - Mission cards with art, danger rating, party readiness meter, reward preview, recommended roles, recent success/failure history.

## P1 - Missing Era Game Parity: Relationships, Dialogue, And Events

- [ ] Replace simple bond events with a full event graph.
  - Intro events, bond rank events, job-related events, facility events, mission events, recovery events, rival/friend events between characters, seasonal events, personal quest events.
- [ ] Add dialogue presentation.
  - Speaker portrait, nameplate, dialogue box, choice buttons, auto/skip/log buttons, event replay gallery.
- [ ] Add choices with consequences.
  - Bond change, morale change, trait discovery, unlock job/facility/research, branch personal quests, different ending flags.
- [ ] Add character-specific goals.
  - Personal milestone chain, favorite gifts/jobs, disliked jobs/events, unique facility bonus, unique combat tactic.
- [ ] Add relationship web.
  - Character-to-character bonds, team synergy, rivalry and mentorship, group events.
- [ ] Add text localization workflow.
  - Text database keyed by stable IDs, EN/JP support as first targets, translation status tracking, missing translation fallback report.

## P1 - Missing Era Game Parity: Pets

- [ ] Expand pet system.
  - Adoption requirements, feeding, training, mood, loyalty, ranch task support, mission support, pet events, pet traits.
- [ ] Add pet visuals.
  - Portrait/icon per pet, small idle animation, mood expression variants.
- [ ] Add pet jobs.
  - Guarding, herding, scouting, comfort/recovery, fetching supplies, festival performance.

## P1 - Missing Era Game Parity: Milestones, Achievements, And Endings

- [ ] Convert original milestone concepts into modern milestone chains.
  - Ranch wealth, facility levels, character count, bond progress, mission clears, research unlocks, production quality, pet mastery, collection progress.
- [ ] Add visible goal screen.
  - Short-term tasks, mid-term chapter goals, long-term achievements, endgame goals, optional challenge goals.
- [ ] Add endings.
  - Economic success ending, beloved community ranch ending, adventurer frontier ending, research institute ending, monster sanctuary ending, character-specific epilogues, true ending requiring multiple pillars.
- [ ] Add post-ending continuation.
  - Continue ranch after credits, New Game Plus unlocks, prestige upgrades, legacy bonuses.

## P2 - Make It A Modern Game, Not Just An Era Remake

The original era style is mostly text/menu simulation. The remake should keep the strategic depth but present it as a modern management RPG.

- [ ] Add a ranch map screen.
  - Facilities as visible buildings, characters assigned to buildings, resource bubbles, upgrade indicators, weather/time-of-day layer, click/tap facility inspection.
- [ ] Add character cards with strong readability.
  - Portrait, role badges, HP/energy/fatigue/morale/bond bars, current assignment, warnings, next suggested action.
- [ ] Add dashboard UX.
  - Today's plan, resource forecast, problems needing attention, upcoming events, end-day preview, profit/loss forecast.
- [ ] Add recommendation system.
  - Suggest rest for exhausted characters, facility upgrades when affordable, contracts based on stockpile, party lineup for missions, transparent explanations.
- [ ] Add tutorialization.
  - First-day tutorial, context hints, optional tutorial tasks, glossary, restartable help panels.
- [ ] Add accessibility.
  - UI scale, colorblind-safe palettes, reduced motion, high contrast, remappable controls, touch-friendly layout, screen-reader-friendly text where Godot support allows.
- [ ] Add controller support.
  - Focus order, button prompts, gamepad shortcuts, modal navigation.
- [ ] Add audio polish.
  - Ranch ambience, town ambience, button feedback, day transition jingle, combat stingers, event music, seasonal themes.
- [ ] Add animation polish.
  - Card transitions, number tweening, resource gain popups, character portrait reactions, facility upgrade effect, mission result reveal.
- [ ] Add modern save features.
  - Multiple slots, autosave, save preview with day/gold/party/thumbnail, export/import save, cloud-save support later.
- [ ] Add settings depth.
  - Language, audio levels, UI scale, text speed, combat animation speed, content mode, difficulty, auto-advance options.
- [ ] Add mod/content pack support later.
  - Data pack loading from a safe folder, content validation, unsafe script prevention, new characters/items/missions/events/portraits.

## P2 - Endgame Progression Ideas

Endgame should let the player keep progressing after the ranch is profitable. Good endgame should create new goals, not only bigger numbers.

### Ranch Prestige

- [ ] Add ranch rank tiers.
  - Local Ranch, Regional Supplier, Guild Partner, Royal Contractor, Makai Frontier Institution, Legendary Ranch.
- [ ] Each rank unlocks new facilities, larger staff cap, rare recruits, harder contracts, new map regions, and cosmetic ranch upgrades.
- [ ] Add prestige objectives.
  - Maintain profit for 30 days, reach high reputation in all districts, complete rare product chain, clear a boss mission, finish multiple character questlines.

### Regional Expansion

- [ ] Add outpost ranches.
  - Forest outpost, mountain outpost, ruins research camp, frontier watch ranch.
- [ ] Outposts have limited slots and unique resources.
- [ ] Assign managers and small teams to outposts.
- [ ] Add logistics routes between outposts and main ranch.
- [ ] Add route risk, guards, and convoy upgrades.

### Legendary Contracts

- [ ] Add contract chains with multiple deliveries and story steps.
  - Royal banquet supply chain, guild expedition support, town restoration project, monster sanctuary founding, research institute grant.
- [ ] Contracts should require production, combat, research, relationships, pets, and facilities.
- [ ] Add contract grades.
  - Bronze, silver, gold, platinum.
  - Higher grade means stricter time and quality requirements.

### Boss Missions And Dungeons

- [ ] Add multi-stage expedition zones.
  - Explore nodes, rest camps, resource finds, mini-bosses, final boss.
- [ ] Add party fatigue across expedition stages.
- [ ] Add rare materials used for top-tier facilities and equipment.
- [ ] Add unique character event unlocks after bosses.

### Mastery And Specialization

- [ ] Add character mastery ranks.
  - Novice, Skilled, Expert, Master, Legend.
- [ ] Add specialization trees.
  - Ranch master, artisan, combat leader, researcher, caretaker, merchant.
- [ ] Add mastery perks.
  - Unique job bonuses, unique combat tactics, unique event options, facility management bonuses.
- [ ] Add skill cap unlocks through quests rather than pure grinding.

### Collection And Completion

- [ ] Add collection books.
  - Characters met, pets adopted, items discovered, recipes crafted, missions cleared, facilities built, events seen, endings unlocked.
- [ ] Add gallery with public-safe art and event replays.
- [ ] Add rare variants.
  - Rare recruit traits, rare pet colors, rare product quality, rare dungeon materials.
- [ ] Add completion rewards.
  - Cosmetic ranch skins, new title screen background, New Game Plus starts, special challenge rules.

### New Game Plus

- [ ] Add NG+ after any major ending.
- [ ] Let player carry selected things forward.
  - Ranch legacy level, cosmetic unlocks, one companion bond memory, one facility blueprint, collection book progress.
- [ ] Add NG+ modifiers.
  - Faster economy, harder contracts, rare recruit rate up, new boss chain, alternate opening event.

### Seasonal And Challenge Modes

- [ ] Add challenge scenarios.
  - Debt race: pay off huge debt in 100 days.
  - Frontier survival: harsh resource scarcity.
  - No-shop run: rely on production and contracts.
  - Pacifist ranch: no combat missions.
  - Combat guild ranch: mission-heavy economy.
  - Tiny ranch: limited facility slots.
- [ ] Add optional leaderboards later for challenge scores if platform-safe.

## P2 - Balance And Simulation Improvements

- [ ] Add economy simulation tests.
  - Average gold/day for early, mid, late game.
  - Resource bottleneck detection.
  - Overpowered job detection.
  - Facility payback time.
  - Contract profit per effort.
- [ ] Add tuning tables.
  - Global difficulty multipliers, economy curve, fatigue curve, morale curve, mission difficulty curve, research cost curve.
- [ ] Add analytics for local test builds.
  - Days played, most used jobs, most ignored jobs, most profitable resources, fail states, quit points.
- [ ] Add debug tools.
  - Add gold/resource, jump day, unlock all facilities, unlock all research, force event, generate recruit, validate current save.

## P2 - UI Screens Still Needed

- [ ] Ranch map screen.
- [ ] Facility detail screen.
- [ ] Character detail screen.
- [ ] Character event screen.
- [ ] Schedule planner screen with time blocks.
- [ ] Contract board screen.
- [ ] Research tree screen.
- [ ] Crafting/processing screen.
- [ ] Storage/inventory screen with filters.
- [ ] Equipment screen with comparison.
- [ ] Mission party setup screen.
- [ ] Combat result replay screen.
- [ ] Pet detail screen.
- [ ] Collection book screen.
- [ ] Ending/epilogue screen.
- [ ] New Game Plus setup screen.

## P2 - Scene-Editable UI Work

- [ ] Continue moving stable UI into scene nodes.
  - Main menu: done.
  - Game shell navigation: done.
  - Compact navigation: done.
  - Title screen content panel: still dynamic.
  - Quick-flow tutorial card: still dynamic.
  - End-day report card: still dynamic.
  - Settings rows: still dynamic.
- [ ] Add reusable scene components.
  - Character card scene, resource row scene, facility card scene, mission card scene, contract card scene, pet card scene, stat bar scene, notification toast scene, dialogue box scene.
- [ ] Add C# binding helpers for reusable components.
  - `CharacterCardView`, `FacilityCardView`, `MissionCardView`, `ResourceRowView`, `DialogueView`.
- [ ] Keep dynamic data lists as instances of scene-authored reusable components.
  - This makes layout editable while still allowing data-driven gameplay.

## P3 - Art Direction Goals

Target presentation: cozy dark-fantasy ranch management RPG with clear UI, readable characters, and a slightly strange Makai frontier mood. Avoid making it look like a plain spreadsheet or an era terminal UI.

Visual pillars:

- Warm ranch life against a supernatural frontier.
- Practical workspaces with magical details.
- Character-first UI.
- Strong silhouettes for races/jobs.
- Clear resource and facility icons.
- Seasonal atmosphere.
- Public-safe default art.

Avoid:

- Generic mobile idle-game gloss.
- Purely beige/brown farm palette.
- Overly dark unreadable fantasy UI.
- Tiny unrecognizable icons.
- Text-only presentation for important events.
- UI cards nested inside too many other cards.

## Artist Brief: General Delivery Rules

Tell the artist:

- Deliver layered source files plus exported PNGs.
- Keep important characters and objects readable at small UI sizes.
- Use transparent backgrounds for character sprites, portraits, icons, and props.
- Use consistent lighting direction for character assets.
- Keep public-build assets safe for general storefront presentation.
- If private-extension art is ever commissioned, keep it in a separate private delivery package with clear labels and no public-repo dependencies.
- Provide filenames in lowercase ASCII with underscores.
- Include a simple contact sheet for each batch.
- Include color palette notes for each major area.

Preferred source files:

- PSD, Krita, or layered PNG where possible.
- Exported PNG for runtime.
- Optional SVG only for simple UI icons.

## Artist Brief: Character Assets

Needed character asset sets:

- [ ] Main cast portrait set.
  - Size: 512x512 or larger source.
  - Runtime exports: 256x256 and 128x128 PNG.
  - Transparent background.
  - Expressions: neutral, happy, worried, tired, angry, surprised.
  - Notes: keep faces readable in dialogue and cards.
- [ ] Full-body character art.
  - Size: 1200-1800 px tall source.
  - Runtime export: 700-900 px tall PNG.
  - Transparent background.
  - Pose: relaxed standing, game-ready silhouette.
  - Variants: normal outfit, work outfit, combat/adventure outfit where relevant.
- [ ] Small chibi/map sprites.
  - Size: 64x64 or 96x96 runtime.
  - Directions: front, back, side if movement map is planned.
  - Idle frame plus simple walk frames if possible.
- [ ] Layered portrait parts for generated characters.
  - Match current layered portrait logic or define a new standard before mass production.
  - Current small-layer style uses assets such as race/body/hair/clothing layers with transparent PNGs.
  - Ask artist for consistent alignment guides and anchor points.
  - Needed layers: body bases by body type, race/species traits, faces, eyes, mouths, hair fronts and backs, horns/ears/tails/accessories, clothing tops/outfits, hats/headwear, held tools if used in cards.
- [ ] Character-specific event CGs, public-safe.
  - Examples: arriving at ranch, cooking together, training yard practice, festival scene, mission camp, pet rescue.
  - Size: 1920x1080 source.
  - Runtime export: 1280x720 PNG/WebP.
  - Include empty-space composition for dialogue text overlay.

Artist prompt examples:

- "Create a public-safe dark-fantasy ranch character portrait for a practical ranch worker, readable at 128x128, warm lantern lighting, transparent background, six expressions."
- "Create a full-body standing illustration of a Makai frontier guild scout, non-explicit outfit, strong silhouette, transparent background, suitable for a management RPG character card."
- "Create layered modular hair assets for a 2D portrait generator, front and back layers separated, consistent anchor point, transparent PNG."

## Artist Brief: Ranch And Environment Assets

Needed environment sets:

- [ ] Ranch overview map.
  - Size: 1920x1080 source.
  - Isometric or 3/4 top-down style.
  - Buildings should be separate layers if possible.
  - Include empty zones for future facilities.
- [ ] Facility building icons/art.
  - Pasture, barn, kitchen, workshop, clinic, research room, training yard, warehouse, market stall, greenhouse, pet area, guest house.
  - Each facility needs level 1, level 2, level 3 visual upgrade if possible.
  - Runtime export: 256x256 or 512x512 PNG.
- [ ] Town location backgrounds.
  - Market, guild hall, clinic, library, shrine, blacksmith, inn, town hall, caravan stop.
  - Size: 1920x1080 source.
  - Runtime export: 1280x720.
  - Composition should leave safe space for UI overlays.
- [ ] Adventure region backgrounds.
  - Fields, forest, ruins, cave, mountain, frontier gate, night road, boss arena.
  - Include day/night or weather variants if budget allows.
- [ ] Seasonal ranch variants.
  - Spring, summer, autumn, winter.
  - Optional weather overlays: rain, fog, snow, magical storm.

Artist prompt examples:

- "Create a cozy dark-fantasy ranch overview map with modular building layers, readable facility silhouettes, warm windows, supernatural frontier beyond the fences, public-safe management game style."
- "Create a 1280x720 town market background for a fantasy ranch management RPG, clear vendor stalls, warm lighting, space for UI on the lower third."

## Artist Brief: UI Assets

Needed UI art:

- [ ] App icon.
  - 1024x1024 source.
  - Must read at 64x64.
  - Motifs: ranch gate, horned moon, milk bottle/resource crate, fantasy pasture.
- [ ] Logo/title treatment.
  - Horizontal and stacked versions.
  - Transparent PNG.
  - Light and dark variants.
- [ ] Resource icons.
  - Gold, farm goods, meals, supplies, herbs, ore/material, reputation, research, contract token, rare material.
- [ ] Stat icons.
  - HP, energy, fatigue, morale, bond, ranch skill, craft skill, combat skill, research skill.
- [ ] Facility icons.
- [ ] Job category icons.
- [ ] Mission type icons.
- [ ] Status effect icons.
- [ ] Equipment slot icons.
  - Weapon, armor, accessory, head, feet, tool.
- [ ] UI panels and decorative frames.
  - Subtle, not too ornate.
  - Should work with scalable Godot UI.
  - Prefer 9-slice friendly borders.
- [ ] Button states.
  - Normal, hover, pressed, disabled, selected.
  - Or provide style guide for drawing them in code.

Icon requirements:

- Source size: 512x512.
- Runtime exports: 128x128, 64x64, 32x32.
- Transparent PNG.
- Thick readable silhouette.
- Do not rely only on color to communicate meaning.

## Artist Brief: Items, Equipment, And Props

- [ ] Item icons for all shop/inventory items.
- [ ] Equipment icons for all equipment.
- [ ] Tool props.
  - Milking kit, tool belt, cooking tools, first aid kit, research notes, camping gear.
- [ ] Product icons.
  - Milk bottle, cheese wheel, butter block, yogurt jar, meal box, herb tea, premium goods crate.
- [ ] Contract reward icons.
- [ ] Rare material icons.

## Artist Brief: Pets And Creatures

- [ ] Pet portraits.
  - 256x256 runtime.
  - Expressions: neutral, happy, hungry, tired.
- [ ] Pet small sprites.
  - 64x64 or 96x96.
  - Idle and simple movement frames.
- [ ] Creature/enemy portraits.
  - 512x512 source.
  - Runtime 256x256.
  - Public-safe fantasy creatures only.
- [ ] Boss creature art.
  - 1024x1024 or larger source.
  - Strong silhouette for mission preview.

## Artist Brief: Animation And VFX

- [ ] UI feedback animations.
  - Button sparkle/confirm, error shake, resource gain flyout, level-up burst.
- [ ] Ranch ambient animations.
  - Smoke from chimney, wind on grass, lantern flicker, small pet idle, facility upgrade glow.
- [ ] Combat/action VFX.
  - Slash, shield, heal, buff, debuff, capture/control ring as public-safe tactical effect.
- [ ] Weather overlays.
  - Rain, snow, fog, magical particles.

## Writer/Narrative Content Needed

- [ ] Main premise rewrite for modern game framing.
- [ ] Intro sequence.
- [ ] First-week tutorial dialogue.
- [ ] Character bios for all main cast.
- [ ] Generated recruit backstory snippets.
- [ ] Town location descriptions.
- [ ] Mission descriptions.
- [ ] Contract descriptions.
- [ ] Facility flavor text.
- [ ] Item descriptions.
- [ ] Research descriptions.
- [ ] Pet descriptions.
- [ ] Bond events.
- [ ] Seasonal event scripts.
- [ ] Ending epilogues.
- [ ] New Game Plus intro variants.
- [ ] Public/private content boundary notes for every imported legacy event.

## Audio Needed

- [ ] Music tracks.
  - Main menu, ranch day, ranch night, town, shop, training/work, mission prep, combat, boss, event emotional, festival, ending.
- [ ] Ambient loops.
  - Ranch morning, rain, night insects, market crowd, workshop, library, forest, cave.
- [ ] UI SFX.
  - Navigate, confirm, cancel, error, open panel, close panel, buy/sell, reward, level up, day transition.

## Platform Readiness TODO

- [ ] Desktop.
  - Export preset, window/fullscreen settings, save path validation, installer or zip packaging, icon and metadata.
- [ ] Android.
  - Touch UI audit, safe area handling, back button handling, suspend/resume autosave, asset memory budget.
- [ ] Web.
  - Web export build, save persistence test, asset loading budget, font fallback test, browser compatibility matrix.
- [ ] Steam/itch style desktop distribution later.
  - Store capsule art, screenshots, trailer, public-safe content rating notes.

## QA Checklist Needed For Every Milestone

- [ ] Build passes.
- [ ] Smoke tests pass.
- [ ] New game works.
- [ ] Continue game works.
- [ ] Save/load roundtrip works.
- [ ] Day settlement works.
- [ ] No missing scene nodes.
- [ ] No missing referenced art/audio.
- [ ] No hard crash when optional private hooks are absent.
- [ ] UI is usable at 1280x720.
- [ ] UI is usable at 1920x1080.
- [ ] UI is usable on narrow/mobile aspect ratio.
- [ ] Keyboard/mouse works.
- [ ] Touch interaction works where targeted.
- [ ] Controller focus works where targeted.

## Suggested Implementation Order

1. Restore and protect compile/smoke CI.
2. Add content validation tooling.
3. Convert hardcoded `DataRegistry` to resource-backed databases.
4. Add reusable scene components for cards/lists.
5. Expand ranch schedule and facility systems.
6. Expand item/equipment/crafting/production.
7. Expand town contracts and reputation.
8. Expand character events and dialogue UI.
9. Expand missions/combat roles/tactics.
10. Add ranch map and modern dashboard.
11. Add endgame ranch ranks, legendary contracts, and outposts.
12. Add NG+ and endings.
13. Add platform exports and release pipeline.

## Open Design Decisions

- [ ] Should the public game lean more cozy-management, dark-fantasy strategy, or character RPG?
- [ ] Should combat be automated reports, tactical choices, or a full turn-based battle view?
- [ ] Should the ranch map be decorative, clickable, or fully placeable/buildable?
- [ ] Should generated recruits be endless, capped, or tied to world reputation?
- [ ] Should time pressure be relaxed or strict?
- [ ] Should debt be a main campaign driver?
- [ ] Should endings be exclusive routes or collectible epilogues?
- [ ] Should New Game Plus carry characters forward or only legacy bonuses?
- [ ] Should private extension systems share save fields with public core, or use a separate private save extension block?

## Done Definition For A Complete Public 1.0

- [ ] Public build contains no private-only assets, scripts, or text.
- [ ] All core systems have authored data, not only seed examples.
- [ ] Ranch management has meaningful choices for at least 100 in-game days.
- [ ] At least 3 major progression routes are viable:
  - production/economy
  - adventure/combat
  - relationship/community/research
- [ ] At least 1 true ending and several alternate endings are implemented.
- [ ] Endgame continuation exists after credits.
- [ ] Save migration has been tested across at least one schema upgrade.
- [ ] Desktop export is tested.
- [ ] Storefront-safe screenshots and capsule art exist.
- [ ] Build, smoke, content validation, and export checks pass in CI.
