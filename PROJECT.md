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

### 🔄 IN PROGRESS
- [ ] **Item-System erweitern (273 Items aus Item.csv)**
  -现状: ~50 Items im Remake
  - Fehlt: 50+ Clothing-Sets, 20+ Equipment-Slots, 30+ Potions, 15+ Training Tools, 20+ Magic items, 10+ Buildings
  - Item.csv Kategorien: 魔力貯蔵器, 霊力抽出装置, 調教設備, 衣服セット, 防具, 薬品, 触手変化, 淫紋, 転移門, etc.

### ⏳ TODO
- [ ] **Building-System erweitern**
  -现状: 5 Gebäude (office, private_room, barn, guest_room, dormitory)
  - Fehlt aus Original: 家族風呂, 大浴場, 天然温泉, 事務所増築, システムキッチン, 奴隷寮, 魔改造工房, 触手部屋, 授乳ルーム, 拘束室, 実験室, etc.
  - Teuer: 魔力貯蔵器 (5 Stufen), 霊力抽出装置, 家庭用/業務用/大容量/魔改造

- [ ] **Equipment-System (162 Einträge)**
  -现状: 5 Slots (weapon, armor, accessory, head, feet)
  - Fehlt: 衣服/下着/鎧/眼/頭/腕/脚/首/上着 (9+ Slots), 露出-Werte (横乳, ズリ穴, 谷間, 乳暖簾, etc.), 調教用装備 (Ａ/Ｂ/Ｃ/Ｖ/Ｎ)

- [ ] **Clothing-System (1800+ Einträge)**
  -现状: Keine Clothing-Definitionen
  - Fehlt: 衣服 (1000+), 下着 (1100+), アクセサリー (1200+), 外装 (1300+), 特殊衣装 (1400+), 拡張 (1500+), 触手服 (1800+)

- [ ] **Potion/Drug-System (350+ Einträge)**
  -现状: Keine Potions
  - Fehlt: 母乳体質化薬, 魔力母乳体質化薬, 膨乳薬, 濃厚化薬, 媚薬, 精力剤, 母乳分泌促進薬, etc.

- [ ] **Jutsu/Techniken (28 Einträge)**
  -现状: MagicService existiert
  - Fehlt: エナジードレイン, 霊力注入, 洗脳, 体内凌辱, 時間圧縮, 淫紋付与, etc.

- [ ] **Talent-System (259 Einträge)**
  -现状: ~5 Talente
  - Fehlt: 250+ Talente aus Talent.csv

- [ ] **Flag-System (786 Flags + 198 Cflags)**
  -现状: Keine Flag-Tracking
  - Fehlt: Quest-Flags, Charakter-Flags, Event-Flags

- [ ] **Mission-System erweitern (6+ Missions, Tiers)**
  -现状: ~10 Missions
  - Fehlt: 50+ Missions aus dem Original

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
- **P1 Content**: 🔄 IN PROGRESS (Item-System, Buildings, Equipment)
- **P2 Polish**: ⏳ TODO

## Vergleich: Original vs Remake

| Kategorie | Original (CSV) | Remake (implementiert) | Status |
|-----------|---------------|----------------------|--------|
| Charaktere | 10+ (Str.csv) | 10 (DataRegistry) | ✅ |
| Items | 273 (Item.csv) | ~50 | ⚠️ |
| Equipment | 162 (Equip.csv) | 5 Slots | ⚠️ |
| Clothing | 1800+ (Str.csv) | 0 | ❌ |
| Training | 178 (Train.csv) | 100+ Actions | ✅ |
| Buildings | 20+ (Item.csv) | 5 | ⚠️ |
| Jutsu | 28 (Juel.csv) | Teilweise | ⚠️ |
| Talents | 259 (Talent.csv) | ~5 | ⚠️ |
| Flags | 786+198 | 0 | ❌ |
| UI Screens | N/A | 20+ | ✅ |
| Services | N/A | 15+ | ✅ |

## Nächste Schritte

1. Item-System erweitern (273 Items aus Item.csv)
2. Building-System erweitern (20+ Gebäude)
3. Equipment-System (9+ Slots)
4. Clothing-System (1800+ Einträge)
5. Potion/Drug-System (350+ Einträge)
6. Jutsu/Techniken (28 Einträge)
7. Talent-System (259 Einträge)
8. Flag-System (786+198 Einträge)
