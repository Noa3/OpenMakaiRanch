# OpenMakaiRanch - Full Project Audit & Development Roadmap

## 1. PROJECT OVERVIEW

### Godot Project (Current): `OpenMakaiRanchGame/`
- Godot 4.6 + C# (.NET 10)
- NSFW systems-first remake of eraMakaiRanch
- Service-oriented architecture
- 14 navigation screens, dynamic content UI
- 6 main characters + generated recruits
- Smoke test suite

### Original Game: `eraMakaiRanch-game-eng-translation/`
- Era/HSP engine
- CSV-driven data (40+ CSV files)
- Full NSFW adult game
- 10+ characters with deep stat systems
- Complex training/mental/economic simulation

---

## 2. EXISTING GODOT IMPLEMENTATION (What Works)

### Architecture
- GameRoot autoload (singleton) orchestrates all services
- SceneRouter for screen routing
- UiShellController with 14 screens
- DataRegistry for all game data
- SaveState/SaveService/SaveMigrator for persistence
- Smoke tests run automatically with `--run-smoke-tests`

### Working Systems
| System | Files | State |
|--------|-------|-------|
| Character roster | SaveStateFactory, RosterService | 6 characters + generated recruits |
| Daily schedule | ScheduleService | 5 assignable jobs |
| Ranch management | RanchService | 4 facilities, stockpile |
| Economy | EconomyService | Gold tracking, income/expenses |
| Shop/Inventory | ShopService, InventoryService | 4 shop items, inventory |
| Combat/Adventure | AdventureService | 2 missions, party system, capture |
| Bond/Mentorship | BondService | 5 bond events, mentorship action |
| Milestones | MilestoneService | 5 milestones with triggers |
| Research | ResearchService | 2 skills (ranch_planning, field_medicine) |
| Pets | PetService | 2 pets (stable_cat, yard_hound) |
| Recruitment | RecruitmentService | Generated recruits, reroll/hire |
| Training | TrainingService | Skill training (ranch/craft/combat) |
| Save/Load | SaveService | JSON save, migration v1-v7 |
| Settings | SettingsStorage | Theme, UI scale, locale, content mode |
| Theme | UiThemePalette, UiThemeCatalog | Multiple color themes |
| Feedback | FeedbackService | Audio tones, haptic vibration |
| Portrait layers | PortraitLayerCatalog | Body, race, hair, cloth, face, bg layers |
| Smoke tests | SmokeTestRunner | 30+ tests for all systems |

### UI Screens
- title (main menu)
- ranch (overview)
- roster (characters + training)
- schedule (job assignment)
- town (facilities + recruitment)
- shop (buy/sell items)
- adventure (party + missions)
- combat (mission results)
- milestones
- research
- bond (mentorship + events)
- pets
- saveload
- settings (audio, haptics, theme, scale, locale, content mode)

### Content Mode Architecture
- `ContentMode` enum: `Sfw` / `MatureSkeleton`
- `IMatureContentHooks` interface with `NullMatureContentHooks`
- Settings toggle in UI
- Currently only placeholder hooks exist

---

## 3. ORIGINAL GAME SYSTEMS (Content to Port)

### CSV Data Structure

```
CSV/                  # Core game data
├── base.csv          # Base parameters (HP, SP, MP, EP, combat, H-params, mental)
├── Abl.csv           # Abilities (sensitivity, work skills, H-skills, addiction)
├── Palam.csv         # Pleasure/emotion parameters
├── Source.csv        # Source (gain) parameters
├── exp.csv           # Experience tracking
├── ex.csv            # Extended experience
├── Nowex.csv         # Current experience
├── Cstr.csv          # Character strings (name, race, job, personality, body, etc.)
├── Cflag.csv         # Character flags (height, appearance, status, pregnancy, etc.)
├── Tflag.csv         # Temporary flags
├── Flag.csv          # Game flags
├── Global.csv        # Global flags
├── Globals.csv       # Global variables
├── GameBase.csv      # Game base settings
├── Money.csv         # Pricing data
├── Time.csv          # Time system
├── Day.csv           # Day/calendar
├── Equip.csv         # Equipment slots
├── Tequip.csv        # Temporary equipment
├── Train.csv         # 170+ training actions
├── Item.csv          # 500+ items
├── Talent.csv        # 250+ talents/traits
├── Mark.csv          # Marks, brands, corruption
├── Str.csv           # String resources (4800+ entries)
├── Savestr.csv       # Save strings
├── VariableSize.csv  # Variable size definitions
├── Stall.csv         # ??? (check)
├── Tcvar.csv         # Temporary CVAR
├── _replace.csv      # Data replacement
├── 000~/
│   ├── Chara0.csv    # Player character (Anon)
│   ├── Chara1.csv    # Slay
│   ├── Chara2.csv    # Default-Chan
│   ├── Chara3.csv    # Kagura
│   ├── Chara4.csv    # Maria
│   ├── Chara5.csv    # Sharon
│   └── Chara6.csv    # Noir
├── 100~/
│   ├── Chara100.csv  # Ayaka (original)
│   ├── Chara101.csv  # En (original)
│   └── Chara102.csv  # Yukina (original)
└── 200~/Chara200.csv # Sample character

resources/
├── Resources.csv      # Resource definitions
├── portrait.csv       # Portrait sprite sheets (8185 lines!)
└── オリジナル/       # Original character artwork
```

### Characters (From CSV)

| # | Name | Type | HP | SP | Traits |
|---|------|------|----|----|--------|
| 0 | Anon | Player | 2000 | 2000 | Demon race, male, owner |
| 1 | Slay/Surei | Default | 1000 | 2000 | Ranch 8, office 2, cleaning 3, quiet, gratitude |
| 2 | Default-Chan | Template | 2000 | 2000 | Generic template character |
| 3 | Kagura | Miko | 1500 | 2000 | Cleaning 5, cooking 5, serving 5, virginity barrier, gentle |
| 4 | Maria | Battle Sister | 1500 | 2000 | Cleaning 5, cooking 5, serving 5, virginity barrier, serious |
| 5 | Sharon | White Mage | 1200 | 1500 | Pharmacy 7, maternal 3, timid, fair skin |
| 6 | Noir | Black Mage | 1800 | 2200 | Pharmacy 7, office 2, confident, glamorous |
| 100 | Ayaka | Exorcist | 2100 | 2000 | Office 2, cleaning 2, cooking 2, tsundere, JK |
| 101 | En | Half-Vampire Exorcist | 2400 | 2000 | Office 4, cleaning 4, cooking 4, graceful, dignified |
| 102 | Yukina | Werewolf Exorcist | 2200 | 2000 | Office 1, cleaning 1, cooking 1, airhead, animal ears |

### Core Game Systems (In Original but Missing from Godot)

**A. Training System** (`Train.csv`)
- 170+ training actions in categories:
  - Hand actions (breast massage, nipple pinch, clit play, fingering, etc.)
  - Mouth actions (breast sucking, kissing, cunnilingus, etc.)
  - V insertion (various positions)
  - A insertion (various positions)
  - Other penis actions (paizuri, fellatio, handjob, etc.)
  - Tool actions (milking machines, vibrators, etc.)
  - Pain actions (spanking)
  - Tentacle actions (30+ tentacle types)
  - Massage actions (breast growth, milk massage)
  - Item actions
  - Body modification (magic marks, pleasure-pain conversion, penis change, time compression)
  - Interrogation/brainwashing
  - Magic training

**B. Mental/Emotional System** (`base.csv`, `Palam.csv`, `Cflag.csv`)
- Mental parameters (0-10000 = 100%):
  - Resistance (base defense)
  - Dignity (shame multiplier)
  - Aversion (likability defense)
  - Reason (milk cow defense)
  - Mental strength (20% = yield, 0% = collapse)
- Emotional parameters:
  - Milk cow
  - Lust
  - Obedience
  - Submission
  - Pain, Fear, Disgust, Antipathy, Despair
- Affection parameters:
  - Favorability (50% = fall in love, 100% = devotion, 200% = slavery)
  - Milk cow (100% = milk cow)
- Fall states: Normal → Love → Devotion → Collapse → Milk Cow → Slave

**C. Breast Milk System** (`Abl.csv`, `Mark.csv`, `base.csv`)
- Milk capacity, production, base output
- Milk concentration levels
- Milk price bonuses
- Equipment: milking machines, breast pumps
- Products: milk sales, milk cooking, magic milk
- Related talents: innate milk constitution, magic milk constitution

**D. Addiction System** (`Abl.csv`, `Mark.csv`)
- Vaginal ejaculation addiction
- Anal ejaculation addiction
- Breast ejaculation addiction
- Semen drinking addiction
- Semen addiction
- Gangbang addiction
- Masochism, sadism, lesbian tendencies
- Milking addiction
- Tentacle addiction

**E. Equipment/Clothing** (`Equip.csv`, `Item.csv`)
- Equipment slots: clothes, underwear top, underwear bottom, armor, eyes, head, arms, legs, neck, jacket
- 500+ items including 50+ outfit types
- Outfit sets (work clothes, miko, sister, maid, bunny, gothloli, etc.)
- Lingerie (bra types, panty types, stockings)
- Accessories (glasses, ribbons, chokers, tiaras)
- Armor/combat gear
- "Lewd" outfits (with exposure settings)

**F. Items** (`Item.csv`)
- Consumables: energy drinks, herbal tea, aphrodisiacs, potions
- Equipment: milking machines, training equipment, furniture
- Magic items: teleportation, hypnosis, energy drain
- Tentacle items: modification, symbiotic suits
- Clothing: 100+ items organized in categories
- Body modification: breast enhancement, milk enhancement
- Special: slave management tags, collars

**G. Talents** (`Talent.csv`)
- 250+ talents organized in categories:
  - Sexual experience (virgin, barrier, etc.)
  - Breast size (flat → magic)
  - Player traits (male, owner)
  - Innate sexual talents
  - Body features (racial traits, ears, horns, tail)
  - Positive/negative body traits
  - Positive/negative skills (teaching, pharmacy, cleaning, etc.)
  - Positive/negative personality (proud, brave, rebellious, modest, etc.)
  - Independent personality (dignified, devoted, tsundere, sadistic, etc.)
  - Preferences (animal, gender, activity, taste)
  - Phobias/traumas
  - Special traits (JS/JC/JK, idol, protagonist privilege)

**H. Combat/Adventure** (`base.csv`, partial)
- Party system (up to 3 members)
- Turn-based combat
- Skills: large attack, recovery
- Combat ability rating
- Capture mechanics
- Mission rewards

**I. Schedule/Jobs** (`Cstr.csv`, `Time.csv`)
- Time system: morning/day/evening/night
- Schedule planning per time slot
- Job types: dairy, office, cleaning, cooking, pharmacy, customer service
- Auto-schedule system
- Work skill levels (1-10ish)

**J. Magic/Research** (`Item.csv` sections 600-900)
- General magic: teleportation, hypnosis, energy drain
- Tentacle magic: 20+ tentacle types
- Body modification magic: breast transformation, sensitivity
- Magic marks/runes: orgasm marks, pleasure conversion
- Forbidden magic: brainwashing, time compression, dimensional magic
- Training SP abilities: restraint, stamina, cleaning ignore

**K. Pet System** (`Cstr.csv`)
- Large pets: fallen angel horse, orthrus, demon hamster
- Taming/familiarity (0-100%)
- Pet care costs
- Pet interactions

**L. Milestones** (`Str.csv` section 7800)
- Action-based milestones
- Military level unlocks
- Science/technology level unlocks
- Town level unlocks
- Dairy industry level unlocks

**M. Character-Specific Content**
- Dialogue system per character
- Voice/line system with personality-specific scripts
- Speech patterns per fall state
- Event scripts per character
- Portrait variations per emotional state

---

## 4. GAP ANALYSIS: What's Missing & Priority

### P0 - Core SFW Expansion (Already Started)
Status: Partially done, needs expansion

| Feature | Current | Needed | Priority |
|---------|---------|--------|----------|
| Characters | 6 main + gen | 10 named characters from CSV | P0 |
| Jobs/Schedule | 5 jobs, simple | Full work skill system with levels | P0 |
| Facilities | 4 basic | More with deeper upgrade trees | P0 |
| Items | 4 shop | 20+ SFW consumables, tools, materials | P0 |
| Missions | 2 missions | 6+ missions, tiers, rewards | P0 |
| Bond events | 5 events | Per-character event chains, story arcs | P0 |
| Research | 2 skills | Skill tree with dependencies | P0 |
| Milestones | 5 milestones | 20+ achievements, unlocks | P0 |
| Pets | 2 pets | 3-6 pets with interactions | P0 |
| Training | 3 focuses | Expanded with facility-based training | P0 |

### P1 - Character & Visual Expansion

| Feature | Current | Needed |
|---------|---------|--------|
| Character data | Names + 3 skills | Full CSV data: talents, personality, race, body, flavor text |
| Portrait layers | Fixed layers per char | Per-character portrait variations, emotion states |
| Character traits | 1 trait per char | 3-5 talents per character from CSV |
| Body types | 4 types | Per-character body from CSV (height, bust, skin, etc.) |
| Generated recruits | Random names | Name pool from original CSV (Str.csv sections 10000+) |

### P2 - Expanded Systems (SFW)

| System | Description |
|--------|-------------|
| Town expansion | More town services, events, NPCs |
| Facility chain upgrades | Facility trees with branching upgrades |
| Cooking system | Meal preparation from stockpile, stat bonuses |
| Gardening/farming | Plant crops, harvest ingredients |
| Exploration | Adventure zones with random events |
| Character quests | Per-character storylines with progression |
| Crafting | Item creation from resources |
| Reputation system | Town/faction standing, discounts, unlocks |

### P3 - NSFW Content Module (Toggleable)

| System | Description | Original Reference |
|--------|-------------|-------------------|
| Training screen | Interactive training actions | Train.csv (170 actions) |
| Mental state engine | Emotions, fall states, corruption | Palam.csv, base.csv |
| Breast milk economy | Production, pricing, sales | Abl.csv, Mark.csv, base.csv |
| Addiction system | Drug/pleasure addiction mechanics | Abl.csv |
| Clothing/equipment | Layered outfits, lewd variants | Equip.csv, Item.csv |
| Relationship system | Love/Devotion/Collapse/Milk Cow | Cflag.csv fall states |
| Magic system | Spells, tentacles, body mod | Item.csv (600-900) |
| Character dialogue | Per-character lines per state | Cstr.csv speech patterns |
| Portrait emotions | Emotional state portrait variants | portrait.csv sprites |
| Bond scenes | Character-specific NSFW scenes | Event system |
| Pregnancy system | Pregnancy, childbirth | Cflag.csv pregnancy flags |

---

## 5. ARCHITECTURE RECOMMENDATIONS

### Current Architecture
```
GameRoot (autoload)
├── DataRegistry (static seeded data)
├── SaveState (runtime state)
├── Service Layer (rosters, economy, combat, etc.)
└── UiShellController (14 screens)
    └── SceneRouter
```

### Recommended Architecture
```
GameRoot (autoload)
├── DataRegistry (static seeded data)
│   ├── SfwContentModule (always loaded)
│   └── MatureContentModule (loaded on toggle)
├── SaveState (runtime state)
│   ├── SfwState (existing)
│   └── MatureState (NSFW toggleable data)
├── Service Layer
│   ├── Core Services (existing)
│   │   ├── RosterService
│   │   ├── ScheduleService
│   │   ├── RanchService / EconomyService
│   │   ├── InventoryService / ShopService
│   │   ├── AdventureService / BondService
│   │   ├── MilestoneService
│   │   ├── ResearchService
│   │   ├── PetService
│   │   └── TrainingService
│   └── Mature Services (toggleable module)
│       ├── TrainingService (expanded with NSFW actions)
│       ├── MentalStateService
│       ├── MilkEconomyService
│       ├── AddictionService
│       ├── ClothingService
│       ├── MagicService
│       └── RelationshipService
├── UiShellController
│   ├── Core Screens (existing 14)
│   └── Mature Screens (conditionally shown)
│       ├── Training Room
│       ├── Milk Processing
│       ├── Magic Lab
│       ├── Relationship Status
│       └── Equipment
└── ContentMode Filter
    ├── SFW: Core screens only
    └── Mature: Core + Mature screens
```

### Data Flow for Content Mode
```
Settings.ContentMode → GameRoot → Service Factory → Screen Visibility
1. SFW mode: All existing services, NullMatureContentHooks
2. Mature mode: Core services + MatureModule services, MatureContentHooksImpl
```

---

## 6. IMPLEMENTATION PHASES

### Phase 0: Data Expansion (Current)
- Update DataRegistry with all 10 characters from CSV
- Add more jobs, items, facilities, missions
- Expand bond events per character
- Add more skills, pets, milestones
- Add character talents/traits system

### Phase 1: Visual & Content
- Layered portrait system with emotion states
- Name pool for generated recruits
- Cooking/farming system
- Expanded town interactions
- Character questlines (SFW)

### Phase 2: System Expansion
- Facility upgrade trees
- Adventure zone exploration
- Crafting system
- Reputation/faction system
- More combat depth

### Phase 3: NSFW Module (Toggleable)
- MatureContentModule with all NSFW systems
- Training system with 170+ actions
- Mental state / corruption engine
- Breast milk economy
- Clothing/equipment system
- Relationship fall states
- Character NSFW content (scenes, dialogue)
- Age verification gate

---

## 7. ORIGINAL CHARACTER PORTRAIT MAPPING

| Godot ID | CSV # | Name | Portrait Asset | Body Type |
|----------|-------|------|----------------|-----------|
| rancher | 0 | Anon | sampleprt.png | Balanced |
| slay | 1 | Slay | slay.png | Athletic |
| default | 2 | Default-Chan | sampleprt.png | Balanced |
| kagura | 3 | Kagura | kagura.png (missing) | Athletic |
| maria | 4 | Maria | maria.png | Refined |
| sharon | 5 | Sharon | sharon.png | Sturdy |
| noir | 6 | Noir | noir.png | Lean |
| ayaka | 100 | Ayaka | (missing) | Slender |
| en | 101 | En | (missing) | Glamorous |
| yukina | 102 | Yukina | (missing) | Slender |

Missing portrait assets: kagura.png, ayaka/en/yukina portraits

---

## 8. NSFW ORIGINAL SYSTEMS - DETAILED

### Mental State System
5 tiers of "fall" tracked per character:
1. **Normal** (好感度/affection 0-49%): Resist training, maintain dignity
2. **Love/Fall** (50-99%): Accept training, begin enjoying it
3. **Devotion** (100-199%): Actively seek approval, loyalty
4. **Collapse** (精神0%): Mental strength = 0%, broken state
5. **Milk Cow** (乳牛100%): Complete submission, identified as livestock

These would be reimagined as SFW equivalents in vanilla mode:
- Normal → Trust → Close → Family → Partner

### Training Actions (170+) - Categories
| Category | Count | SFW Equivalent |
|----------|-------|----------------|
| Hand | 10 | Massage, grooming, comfort |
| Mouth | 10 | Feeding, encouragement |
| V Insertion | 8 | (NSFW only) |
| A Insertion | 8 | (NSFW only) |
| Penis Actions | 8 | (NSFW only) |
| Tools | 5 | Massage tools, training aids |
| Pain | 1 | (NSFW only) |
| Tentacle | 20+ | (NSFW only - fantasy horror) |
| Massage | 3 | Massage therapy (SFW) |
| Items | 20+ | (mostly NSFW) |
| Body Mod | 30+ | (mostly NSFW) |
| Forbidden Magic | 30+ | (mostly NSFW) |

SFW Training could include: meditation, exercise, study, skill practice, bonding activities

### Breast Milk Economy (NSFW Core)
- Base production per character
- Milk quality tiers
- Equipment upgrades (milking machines)
- Product lines (raw milk, magic milk, milk cooking)
- Pricing influenced by character traits (JS/JK tags give price bonus)
- Daily milk quotas and shipping
- Breastfeeding mechanics

### Addiction System
Tracked per character (0-100%):
- Vaginal/anal/breast ejaculation addiction
- Semen drinking/swallowing addiction
- Gangbang addiction
- Masochism/sadism
- Milking/tentacle addiction
- Effects: withdrawal symptoms, stat changes, behavior changes

### Equipment & Clothing
- 8 equipment slots
- 100+ clothing items
- Outfit layering
- "Lewd" modifiers (exposure settings)
- Clothing damage/dirt system
- Outfit presets per schedule

---

## 9. IMMEDIATE NEXT STEPS (Priority Order)

1. **Update DataRegistry.cs** - Add all 10 original characters with their CSV data (talents, stats, body, personality)
2. **Add missing portraits** - kagura.png, ayaka/en/yukina portraits, plus NSFW variants
3. **Expand job/training system** - Map original work skills to SFW jobs
4. **Expand item system** - Add 20+ SFW items from original item list
5. **Expand mission system** - Add 4+ missions from original content
6. **Expand bond events** - Per-character event chains based on personality
7. **Expand research tree** - Add skills from original skill list
8. **Add NSFW content module** - Start with MentalStateService and TrainingService
9. **Full training UI** - Training screen with categorized actions
10. **Mental state UI** - Character status with mental/emotional display
