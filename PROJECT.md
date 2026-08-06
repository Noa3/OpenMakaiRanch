# OpenMakaiRanch — Projekt-Board (Kanban)

## TODO (Backlog)

### P0 — Core Systems (Kritisch)
- [ ] #1 MilkEconomyService: ProduceMilk, ShipMilk, Quality-System, Concentration-System
- [ ] #2 MentalStateService: Fall-State-Logik (Collapse, MilkCow, Slave, Devotion, Love)
- [ ] #3 EnhancedTrainingService: Action-Effects, Sensation-Tracking
- [ ] #4 TrainingActionCatalog: 100+ Actions aus Train.csv importieren
- [ ] #5 MatureServices: TrainingAction-Effects, BodyMod-System
- [ ] #6 SaveState Schema v14 Migration
- [ ] #7 TrainingActionDefinition Resource-Klasse erstellen
- [ ] #8 DataRegistry: TrainingActions-Seeding implementieren
- [ ] #9 SaveModels: WithdrawalRecord, ResearchSkillDefinition, EquipmentState
- [ ] #10 GameRoot: NewGame, NewGamePlus, Save/Load, DayCycle, WinCondition

### P1 — Content Expansion (Wichtig)
- [ ] #11 Item-System: 497 Items aus Item.csv importieren
- [ ] #12 Facility-System: Alle Gebäude aus Original (Barn, Office, GuestRoom, Dormitory, TrainingRoom, Bath, MilkTank, Shop)
- [ ] #13 Equipment-System: 301 Einträge aus Equip.csv
- [ ] #14 Mission-System: 6+ Mission-Tiers erweitern
- [ ] #15 BondEventChains: Pro Charakter Event-Ketten implementieren
- [ ] #16 RecruitmentSystem: Random Encounters, Negotiations
- [ ] #17 PetSystem: Fütterung, Training, Adoption
- [ ] #18 MagicService: Spells, Tentacles, BodyMod
- [ ] #19 PortraitLayerCatalog: Layer-basierte Portrait-Generierung

### P2 — UI/UX (Nice to have)
- [ ] #20 Training Action UI: Action-Auswahl mit Preview
- [ ] #21 Ranch Facility Building UI: Build/Upgrade-Flows
- [ ] #22 Combat System UI: Mission-Rounds, Capture-Mechaniken
- [ ] #23 NG+ System UI: Carry-over Auswahl
- [ ] #24 Save/Load Schema Migration UI
- [ ] #25 Localization: de, en, ja
- [ ] #26 Achievement/Milestone UI
- [ ] #27 Pet System UI
- [ ] #28 Recruitment System UI

### P3 — Assets & Polish
- [ ] #29 Portrait Layer Assets: kagura, ayaka, en, yukina
- [ ] #30 Ranch Facility Sprites
- [ ] #31 Combat Visual Effects
- [ ] #32 Training Room Visuals

## IN PROGRESS

### P0 Core
- [x] #1 MilkEconomyService: ProduceMilk, ShipMilk, Quality-System, Concentration-System ✅
- [x] #2 MentalStateService: Fall-State-Logik (Collapse, MilkCow, Slave, Devotion, Love) ✅
- [x] #3 EnhancedTrainingService: Action-Effects, Sensation-Tracking ✅
- [x] #4 TrainingActionCatalog: 100+ Actions aus Train.csv importiert ✅
- [x] #5 MatureServices: TrainingAction-Effects, BodyMod-System ✅
- [x] #6 SaveState Schema v14 Migration ✅
- [x] #7 TrainingActionDefinition Resource ✅
- [x] #8 DataRegistry: TrainingActions-Seeding ✅
- [x] #9 SaveModels: WithdrawalRecord, ResearchSkillDefinition, EquipmentState ✅
- [x] #10 GameRoot: NewGame, NewGamePlus, Save/Load, DayCycle, WinCondition ✅

### P1 Content
- [ ] #11 Item-System: 497 Items aus Item.csv importieren
- [ ] #12 Facility-System: Alle Gebäude aus Original
- [ ] #13 Equipment-System: 301 Einträge aus Equip.csv
- [ ] #14 Mission-System: 6+ Mission-Tiers erweitern
- [ ] #15 BondEventChains: Pro Charakter Event-Ketten
- [ ] #16 RecruitmentSystem
- [ ] #17 PetSystem
- [ ] #18 MagicService: Spells, Tentacles, BodyMod
- [ ] #19 PortraitLayerCatalog

### P2 UI/UX
- [ ] #20 Training Action UI
- [ ] #21 Ranch Facility Building UI
- [ ] #22 Combat System UI
- [ ] #23 NG+ System UI
- [ ] #24 Save/Load Schema Migration UI
- [ ] #25 Localization: de, en, ja
- [ ] #26 Achievement/Milestone UI
- [ ] #27 Pet System UI
- [ ] #28 Recruitment System UI

### P3 Assets
- [ ] #29 Portrait Layer Assets
- [ ] #30 Ranch Facility Sprites
- [ ] #31 Combat Visual Effects
- [ ] #32 Training Room Visuals

---

## Gap-Analysis: eraMakaiRanch vs. OpenMakaiRanch

### Gebäude (Facilities)
| Original | Status | Bemerkung |
|----------|--------|-----------|
| Office | ✅ | Implementiert |
| Private Room | ✅ | Implementiert |
| Barn | ✅ | Implementiert |
| Guest Room | ✅ | Implementiert |
| Dormitory | ✅ | Implementiert |
| Training Room | ❌ | Fehlend |
| Bath House | ❌ | Fehlend |
| Milk Tank | ❌ | Fehlend |
| General Shop | ❌ | Fehlend |
| Adventure Guild | ✅ | Implementiert |

### Charaktere
| Original | Status | Bemerkung |
|----------|--------|-----------|
| kagura | ✅ | Implementiert |
| ayaka | ✅ | Implementiert |
| en | ✅ | Implementiert |
| yukina | ✅ | Implementiert |
| rize | ✅ | Implementiert |
| shizuku | ✅ | Implementiert |
| mizuki | ✅ | Implementiert |
| akari | ✅ | Implementiert |
| rei | ✅ | Implementiert |
| hina | ✅ | Implementiert |

### Items (497 Einträge in Item.csv)
| Kategorie | Original | Remake | Status |
|-----------|----------|--------|--------|
| Consumables | ~100+ | 0 | ❌ |
| Materials | ~80+ | 0 | ❌ |
| Tools | ~50+ | 0 | ❌ |
| Equipment | ~301 Einträge | 5 Slots | ❌ |
| Special Items | ~30+ | 0 | ❌ |
| Facility Supplies | ~10+ | 0 | ❌ |

### Equipment (301 Einträge in Equip.csv)
| Slot | Status | Bemerkung |
|------|--------|-----------|
| Weapon | ❌ | 0 Items |
| Armor | ❌ | 0 Items |
| Accessory | ❌ | 0 Items |
| Head | ❌ | 0 Items |
| Feet | ❌ | 0 Items |
| Underwear Top | ❌ | Nicht implementiert |
| Underwear Bottom | ❌ | Nicht implementiert |
| Upper Body | ❌ | Nicht implementiert |
| Eyes | ❌ | Nicht implementiert |
| Neck | ❌ | Nicht implementiert |
| Training Equipment | ❌ | Nicht implementiert |

### Missionen
| Tier | Status | Bemerkung |
|------|--------|-----------|
| Local | ❌ | 0 Missions |
| Regional | ❌ | 0 Missions |
| Dangerous | ❌ | 0 Missions |

### Train.csv (178 Zeilen)
- Training Actions: ✅ 100 Actions importiert (10 Kategorien)

### Palam.csv (Pleasure Parameters)
- 45 pleasure parameters: ✅ MentalStateService implementiert

### Source.csv (199 training parameters)
- Training parameter mapping: ✅ in MatureServices

### Str.csv (4824 lines — name database)
- Character names: ❌ Nur 10 hardcoded characters
- Height, BodyType, SkinColor, HairColor, EyeColor, etc.: ❌

### Talent.csv (259 lines)
- Talents: ❌ 0 Talents im Remake

### Flag/Cflag/Tflag (Spielzustands-Flags)
- Game state flags: ❌ Nicht implementiert
- Character flags: ❌ Nicht implementiert
- Training flags: ❌ Nicht implementiert

---

## Architektur-Ziel

```
GameRoot (autoload)
├── DataRegistry (static seeded data — all systems)
├── SaveState (runtime state — all systems)
├── Service Layer
│   ├── RosterService
│   ├── ScheduleService
│   ├── RanchService / EconomyService
│   ├── InventoryService / ShopService
│   ├── AdventureService / BondService
│   ├── MilestoneService / ResearchService
│   ├── PetService / TrainingService
│   ├── MatureService (training actions, sensations)
│   ├── MentalStateService (fall states, corruption)
│   ├── MilkEconomyService (production, pricing)
│   ├── AddictionService (addiction tracking, withdrawal)
│   ├── ClothingService (equipment/outfit slots)
│   └── MagicService (spells, tentacles, body mod)
├── UiShellController
│   ├── Core Screens
│   └── Mature Screens
└── No toggle — all content is core
```

## Priorität

| Prio | Bedeutung |
|------|-----------|
| P0   | Kritisch — Core Systems (DONE) |
| P1   | Wichtig — Content Expansion |
| P2   | Nice to have — Modernization |
| P3   | Assets & Polish |

## Status

- **P0 Core Systems**: ✅ DONE (Training, MentalState, MilkEconomy, Save/Load, DayCycle, NG+, UI-Screens)
- **P1 Content**: 🔴 IN PROGRESS (Items, Facilities, Equipment, Missions, BondEvents, Recruitment, Pets, Magic)
- **P2 UI/UX**: ⏳ TODO (Training UI, Facility UI, Combat UI, Localization)
- **P3 Assets**: ⏳ TODO (Portraits, Sprites, VFX)
