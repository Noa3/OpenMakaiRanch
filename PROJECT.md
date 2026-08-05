# OpenMakaiRanch — Projekt-Board (Kanban)

## TODO (Backlog)

- [ ] #6 Portrait Layer Assets erweitern (kagura, ayaka, en, yukina)
- [ ] #7 Job/Skill-System erweitern (Work-Skills 1-10)
- [ ] #8 Item-System erweitern (500+ Items)
- [ ] #9 Mission-System erweitern (6+ Missions, Tiers)
- [ ] #10 Bond Event Chains pro Charakter
- [ ] #11 UI-Screens für Mental State / Fall State
- [ ] #12 Ranch Facility Building (Baths, Milk Tank, Training Room)
- [ ] #13 Day/Night Cycle + Weather Effects
- [ ] #14 Combat System (Mission-Rounds, Capture)
- [ ] #15 NG+ System (carry over gold, research, facilities)
- [ ] #16 Save/Load Schema Migration (v11 → v14)
- [ ] #17 Localization (de, en, ja)
- [ ] #18 Achievement/Milestone System
- [ ] #19 Pet System (feeding, training, adoption)
- [ ] #20 Recruitment System (random encounters, negotiations)

## IN PROGRESS

- [ ] #21 Training Action UI (action selection, preview)

## DONE

- [x] Kanban-Board initialisiert
- [x] .gitignore für GodotSharp und Build-Artifacts erweitert
- [x] DataRegistry mit allen 10 Charakteren aus CSV erweitert
- [x] Training Action Catalog aus Train.csv importiert (100+ actions, 10 Kategorien)
- [x] TrainingActionDefinition Resource-Klasse erstellt
- [x] MentalStateService mit Fall-State-Logik implementiert
- [x] MilkEconomyService implementiert (ProduceMilk, ShipMilk, Quality, Concentration)
- [x] EnhancedTrainingService mit Action-Effects
- [x] ResearchTreeService + MagicService implementiert
- [x] SaveModels: WithdrawalRecord, ResearchSkillDefinition, EquipmentState
- [x] GameRoot: NewGame, NewGamePlus, Save/Load, DayCycle
- [x] Godot Universal MCP Plugin integrieren
- [x] Plugin README + Setup-Skript erstellt

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

## Status

- **P0 Core Systems**: ✅ DONE (DataRegistry, Training, MentalState, MilkEconomy, Save/Load, DayCycle, NG+)
- **P1 Content**: 🔄 IN PROGRESS (UI-Screens, Facility Building, Combat)
- **P2 Polish**: ⏳ TODO (Localization, Achievements, Pet System)
