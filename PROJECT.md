# OpenMakaiRanch — Projekt-Board (Kanban)

## P0 — Kritisch (Core Systems)

### ✅ DONE
- [x] DataRegistry mit 10 Charakteren aus CSV
- [x] Training Action Catalog (100+ actions, 10 Kategorien)
- [x] TrainingActionDefinition Resource
- [x] MentalStateService (Fall States)
- [x] MilkEconomyService
- [x] EnhancedTrainingService
- [x] Mental State / Fall State UI
- [x] ResearchTreeService + MagicService
- [x] SaveModels (WithdrawalRecord, ResearchSkillDefinition, EquipmentState)
- [x] GameRoot (NewGame, NG+, Save/Load, DayCycle)
- [x] RanchService (Facilities, Job Output, Automation)
- [x] EconomyService
- [x] InventoryService
- [x] EquipmentService
- [x] CombatServices
- [x] ScheduleService
- [x] DailySettlementService
- [x] BondService
- [x] MilestoneService
- [x] PetService
- [x] PortraitLayerCatalog
- [x] TalentService
- [x] RosterService
- [x] UiShellController (Title, Ranch, Roster, Town, Shop, Adventure, Combat, Schedule, Save/Load, Settings, Training, Milk, Mental, Character Detail)
- [x] Build clean (0 errors, 0 warnings)

---

## P1 — Wichtig (Content aus Original)

### ✅ DONE
- [x] **Item-System erweitern** — 200+ Items (Clothing, Potions, Restraints, Buildings, Tools, Magic items)
- [x] **Building-System erweitern** — 30 Gebäude (Bäder,魔力貯蔵器, 霊力抽出装置, NSFW-Räume)
- [x] **Equipment-System** — 15 Slots (Clothes, Underwear, Armor, Head, Arms, Legs, Necklace, Coat, Accessory, etc.)
- [x] **ClothingService** — Equipment-Apply-Logic, Bonus-Computation, Item-Use-Handler für Potions
- [x] **Talent-System** — 47 Talente (処女, 母乳体質, 魔界種族, etc.)
- [x] **SpellDefinition + 28 MagicSpells** — エナジードレイン, 洗脳, 淫紋付与, 時間圧縮, etc.

### 🔄 IN PROGRESS
- [ ] **Clothing-System (1800+ Einträge)**
  - Status: 0 Clothing-Definitionen
  - Fehlt: 衣服 (1000+), 下着 (1100+), アクセサリー (1200+), 外装 (1300+), 特殊衣装 (1400+), 拡張 (1500+), 触手服 (1800+)
  - Priorität: Hoch — Basis für Visual-System und Equipment-UI

- [ ] **Talent-System erweitern (259 Einträge)**
  - Status: 47 Talente
  - Fehlt: 212 Talente aus Talent.csv (身体特徴, 体質, 経験, 属性)

### ⏳ TODO
- [ ] **Flag-System (786 Flags + 198 Cflags)**
  - Status: Keine Flag-Tracking
  - Fehlt: Quest-Flags, Charakter-Flags, Event-Flags

- [ ] **Mission-System erweitern (50+ Missions)**
  - Status: ~10 Missions
  - Fehlt: 40+ Missions aus dem Original

- [ ] **Clothing-UI**
  - [x] Equipment-Slot-UI für Character-Detail (inkl. Equip/Unequip für bestehende Slots + ClothingStyle-Anzeige)
  - Clothing-Preview im Shop
  - Outfit-Management

---

## P2 — Nice to have

### ⏳ TODO
- [ ] **Portrait Layer Assets erweitern** (kagura, ayaka, en, yukina)
- [ ] **Localization** (de, en, ja)
- [ ] **NG+ System** (carry over gold, research, facilities)
- [ ] **Save/Load Schema Migration** (v11 → v14)
- [ ] **Day/Night Cycle + Weather Effects**
- [ ] **Recruitment System** (random encounters, negotiations)
- [ ] **Training Action UI** (action selection, preview)
- [ ] **Bond Event Chains** (pro Charakter)
- [ ] **Ranch Facility Building** (Baths, Milk Tank, Training Room)
- [ ] **Combat System** (Mission-Rounds, Capture)

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
| P0   | Kritisch — Core Systems (✅ DONE) |
| P1   | Wichtig — Content Expansion (🔄 IN PROGRESS) |
| P2   | Nice to have — Modernization |

## Status

- **P0 Core Systems**: ✅ DONE
- **P1 Content**: 🔄 IN PROGRESS (Clothing-System, Talente, Flag-System)
- **P2 Polish**: ⏳ TODO

## Vergleich: Original vs Remake

| Kategorie | Original (CSV) | Remake (implementiert) | Status |
|-----------|---------------|----------------------|--------|
| Charaktere | 10+ (Str.csv) | 10 (DataRegistry) | ✅ |
| Items | 273 (Item.csv) | ~250 | ✅ |
| Equipment | 162 (Equip.csv) | 15 Slots | ✅ |
| Clothing | 1800+ (Str.csv) | 0 | ❌ |
| Training | 178 (Train.csv) | 100+ Actions | ✅ |
| Buildings | 20+ (Item.csv) | 30 | ✅ |
| Jutsu | 28 (Juel.csv) | 28 Spells | ✅ |
| Talents | 259 (Talent.csv) | 47 | ⚠️ |
| Flags | 786+198 | 0 | ❌ |
| UI Screens | N/A | 20+ | ✅ |
| Services | N/A | 15+ | ✅ |

## Nächste Schritte

1. Clothing-System (1800+ Einträge)
2. Talent-System erweitern (212 weitere)
3. Flag-System (786+198 Einträge)
4. Mission-System erweitern (40+ Missions)
5. Clothing-UI (Equipment-Slots, Shop-Preview)
