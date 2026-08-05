# OpenMakaiRanch — Projekt-Board (Kanban)

## TODO (Backlog)

- [ ] #1 DataRegistry mit allen 10 Charakteren aus CSV erweitern
- [ ] #2 Training Action Catalog aus Train.csv importieren (170+ Actions)
- [ ] #3 Mental State / Fall State UI implementieren
- [ ] #4 Milk Economy Service implementieren
- [ ] #5 .gitignore für GodotSharp und Build-Artifacts erweitern
- [ ] #6 Portrait Layer Assets erweitern (kagura, ayaka, en, yukina)
- [ ] #7 Job/Skill-System erweitern (Work-Skills 1-10)
- [ ] #8 Item-System erweitern (500+ Items)
- [ ] #9 Mission-System erweitern (6+ Missions, Tiers)
- [ ] #10 Bond Event Chains pro Charakter

## IN PROGRESS

- [ ] #11 Kanban-Board initialisieren + .gitignore fixen + commit/push

## DONE

- [x] Godot Universal MCP Plugin integrieren
- [x] ResearchTreeService implementieren
- [x] MagicService implementieren
- [x] SaveModels: WithdrawalRecord, ResearchSkillDefinition
- [x] Plugin README umfassend aktualisiert
- [x] Setup-Skript erstellt

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
| P0   | Kritisch — Core Systems |
| P1   | Wichtig — Content Expansion |
| P2   | Nice to have — Modernization |
