# OpenMakaiRanch — Vollständiger Implementierungsplan

Vergleicht IDEA.md, REMAKE_TODO.md, gap-analysis.md, AUDIT.md mit dem aktuellen Code und listet alles auf — umgesetzt, teilweise, fehlt.

---

## 0. STATUS-ZUSAMMENFASSUNG

| Kategorie | Status |
|-----------|--------|
| **Build** | ✅ dotnet build, CI, Smoke Tests |
| **Speicher-Schema** | v14 (SaveState, 16 State-Subklassen) |
| **Datenquelle** | JSON in `data/` + Seed-Fallback (DataRegistry) |
| **UI** | UiShellController mit 14 Screens, scene-first UI |
| **Charaktere** | 8 aus DataRegistry (seeded) + generierte Rekruten |
| **Jobs** | 11 Jobs (seeded) |
| **Items** | 60+ Items (seeded) |
| **Facilities** | 11 Facilities (seeded) |
| **Missions** | 12 Missions (seeded) |
| **Enemies** | 14 Enemies (seeded) |
| **Milestones** | 18 Milestones (seeded) |
| **Skills** | 12 Skills (seeded) |
| **Pets** | 5 Pets (seeded) |
| **Bond Events** | 30+ Bond Events (seeded) |
| **Talents** | 40+ Talents (seeded) |
| **NSFW** | MatureContentHooks + TrainingActionCatalog (170+ Actions) — implementiert als Core-Gameplay |
| **Portraits** | Layered Portrait System (226+ Layer-Frames) |

---

## 1. IMPLEMENTIERTE SYSTEME (vollständig)

### 1.1 Core-Architektur

| System | Datei | Status |
|--------|-------|--------|
| GameRoot (Autoload, Service-Komposition) | `src/App/GameRoot.cs` (570 Zeilen) | ✅ Vollständig |
| SceneRouter | `src/App/SceneRouter.cs` | ✅ Vollständig |
| DataRegistry (JSON + Seed) | `src/Data/DataRegistry.cs` (449 Zeilen) | ✅ Vollständig |
| SaveState (Schema v14) | `src/Core/Models/SaveModels.cs` (482 Zeilen) | ✅ Vollständig |
| SaveService (JSON Save/Load) | `src/Gameplay/SaveService.cs` | ✅ Vollständig |
| SaveMigrator (Schema-Migration) | `src/Gameplay/SaveMigrator.cs` | ✅ Vollständig |
| SaveStateFactory (NewGame/Recruit) | `src/Gameplay/SaveStateFactory.cs` | ✅ Vollständig |
| CharacterGenerationPools | `src/Gameplay/CharacterGenerationPools.cs` | ✅ Vollständig |
| PortraitLayerCatalog | `src/Gameplay/PortraitLayerCatalog.cs` | ✅ Vollständig |
| FeedbackService (Audio/Haptic) | `src/App/FeedbackService.cs` | ✅ Vollständig |
| SettingsStorage | `src/App/SettingsStorage.cs` | ✅ Vollständig |
| DailyReport | `src/Core/Models/SaveModels.cs` | ✅ Vollständig |
| CombatReport | `src/Core/Models/SaveModels.cs` | ✅ Vollständig |

### 1.2 Gameplay-Services

| System | Datei | Zeilen | Status |
|--------|-------|--------|--------|
| RanchService (Jobs, Facilities, Stockpile) | `src/Gameplay/RanchService.cs` | 199 | ✅ Vollständig |
| RosterService (Charaktere, Definition) | `src/Gameplay/RosterService.cs` | 50 | ✅ Vollständig |
| ScheduleService (Job-Zuweisung) | `src/Gameplay/ScheduleService.cs` | 35 | ✅ Vollständig |
| EconomyService (Gold) | `src/Gameplay/EconomyService.cs` | 39 | ✅ Vollständig |
| DayCycleService (Tag/Phase/Wetter) | `src/Gameplay/DayCycleService.cs` | 50 | ✅ Vollständig |
| DailySettlementService (End-of-Day) | `src/Gameplay/DailySettlementService.cs` | 114 | ✅ Vollständig |
| InventoryService (Items, UseItem) | `src/Gameplay/ManagementServices.cs` (1-130) | 130 | ✅ Vollständig |
| ShopService (Buy/Sell) | `src/Gameplay/ManagementServices.cs` (132-172) | 40 | ✅ Vollständig |
| AdventureService (Missions, Capture) | `src/Gameplay/ManagementServices.cs` (174-298) | 124 | ✅ Vollständig |
| MilestoneService (Achievements) | `src/Gameplay/ManagementServices.cs` (300-399) | 100 | ✅ Vollständig |
| BondService (Events, Mentorship) | `src/Gameplay/ManagementServices.cs` (401-473) | 73 | ✅ Vollständig |
| TownService (Town-Aktionen) | `src/Gameplay/ManagementServices.cs` (475-485) | 11 | ⚠️ Minimal — nur Actions-Liste |
| EquipmentService (Equip/Unequip) | `src/Gameplay/EquipmentService.cs` | 140 | ✅ Vollständig |
| TalentService (Talents, Bonuses) | `src/Gameplay/TalentService.cs` | 96 | ✅ Vollständig |
| CombatService (Round-based) | `src/Gameplay/CombatServices.cs` (454 Zeilen) | 454 | ✅ Vollständig |
| DailyEventService (Tägliche Events) | `src/Gameplay/LifecycleServices.cs` (DailyEventService) | 287 | ✅ Vollständig |

### 1.3 Mature Content Systems

| System | Datei | Status |
|--------|-------|--------|
| MatureContentHooks + NullMatureContentHooks | `src/Gameplay/ManagementServices.cs` (487-500) | ✅ Interface + Null-Impl |
| SensationType Enum | `src/Gameplay/MatureServices.cs` (16-20) | ✅ Vollständig |
| TrainingActionDefinition | `src/Gameplay/MatureServices.cs` (22-38) | ✅ Vollständig |
| TrainingActionCatalog (170+ Actions) | `src/Gameplay/MatureServices.cs` (40-569) | ✅ Vollständig |
| TrainingCategory Enum | `src/Core/Models/SaveModels.cs` (56-72) | ✅ Vollständig |
| MatureState (SaveState) | `src/Core/Models/SaveModels.cs` | ✅ Vollständig |

### 1.4 UI

| System | Datei | Status |
|--------|-------|--------|
| UiShellController (Main) | `src/Ui/UiShellController.cs` | ✅ Vollständig |
| UiShellController.Screens | `src/Ui/UiShellController.Screens.cs` | ✅ Vollständig |
| UiShellController.Styling | `src/Ui/UiShellController.Styling.cs` | ✅ Vollständig |
| UiThemePalette | `src/Ui/UiThemePalette.cs` | ✅ Vollständig |
| MainMenu.tscn | `scenes/MainMenu.tscn` | ✅ Scene-authored |
| Game.tscn (Shell) | `scenes/Game.tscn` | ✅ Scene-authored |
| Bootstrap.tscn | `scenes/Bootstrap.tscn` | ✅ Vollständig |
| CharacterCreationScreen.tscn | `scenes/CharacterCreationScreen.tscn` | ✅ Vollständig |
| 14 Navigation Screens | UiShellController | ✅ Implementiert |

### 1.5 Data (JSON)

| Datei | Einträge | Status |
|-------|----------|--------|
| `data/characters.json` | 8 Characters | ✅ Vollständig |
| `data/jobs.json` | 11 Jobs | ✅ Vollständig |
| `data/items.json` | 60+ Items | ✅ Vollständig |
| `data/facilities.json` | 11 Facilities | ✅ Vollständig |
| `data/missions.json` | 12 Missions | ✅ Vollständig |
| `data/enemies.json` | 14 Enemies | ✅ Vollständig |
| `data/milestones.json` | 18 Milestones | ✅ Vollständig |
| `data/skills.json` | 12 Skills | ✅ Vollständig |
| `data/pets.json` | 5 Pets | ✅ Vollständig |
| `data/bond_events.json` | 30+ Bond Events | ✅ Vollständig |
| `data/talents.json` | 40+ Talents | ✅ Vollständig |

---

## 2. TEILWEISE IMPLEMENTIERTE SYSTEME

### 2.1 Milk Economy

| System | Status | Details |
|--------|--------|---------|
| MilkState (SaveState) | ✅ Definiert | Capacity, Production, BaseOutput, Quality, CurrentAmount |
| MilkEconomyService | ✅ Implementiert | `ProduceMilk()`, `ShipMilk()` — in DailySettlementService integriert |
| Milk UI Screen | ❌ Fehlend | Kein UI-Screen für Produktion, Qualität, Preise, Versand |
| Milk Processing (Cheese/Butter) | ❌ Fehlend | Kein Rezept-System |
| Milk Quality Tiers | ⚠️ Teilweise | Quality-Felder in Models, aber keine UI/Verkauf |
| Milk Equipment (Milking Machines) | ❌ Fehlend | Kein Equipment-Upgrade für Milch |

### 2.2 Mental State / Fall States

| System | Status | Details |
|--------|--------|---------|
| FallState Enum | ✅ Definiert | Normal/Love/Devotion/Collapse/MilkCow/Slave |
| MentalStateService | ❌ Fehlend | Keine separate Service-Datei |
| Mental State UI | ❌ Fehlend | Keine Anzeige von Resistance, Dignity, Aversion, Corruption |
| Fall State Progression | ⚠️ Teilweise | RanchService prüft Collapse für Job-Ausgabe |
| Emotional Portrait Variants | ❌ Fehlend | Keine Portrait-Varianten pro emotionaler Zustand |
| Palam.csv Mapping | ❌ Fehlend | Original-Emotion-Parameter nicht gemappt |

### 2.3 Addiction System

| System | Status | Details |
|--------|--------|---------|
| AddictionState (SaveState) | ✅ Definiert | Felder in MatureState |
| AddictionService | ❌ Fehlend | Keine Service-Datei |
| Addiction UI | ❌ Fehlend | Keine Anzeige von Addiction-Typen, Withdrawal |
| Abl.csv Mapping | ❌ Fehlend | Original-Addiction-Parameter nicht gemappt |
| Withdrawal Effects | ❌ Fehlend | Keine gameplay-Effekte |

### 2.4 Clothing / Equipment

| System | Status | Details |
|--------|--------|---------|
| EquipmentService (5 Slots) | ✅ Implementiert | weapon/armor/accessory/head/feet |
| EquipmentState (SaveState) | ✅ Definiert | EquippedItems Dictionary |
| 8-Slot Equipment | ⚠️ Teilweise | Models existieren, nur 5 Slots implementiert |
| 100+ Clothing Items | ❌ Fehlend | Items.json hat nur Seed-Daten, kein vollständiges Outfit-System |
| Outfit Layering | ❌ Fehlend | Kein Outfit-Management |
| Lewd Exposure Settings | ❌ Fehlend | Keine Exposure-Modifier |
| Clothing Damage/Dirt | ❌ Fehlend | |
| Clothing UI Screen | ❌ Fehlend | |

### 2.5 Training System

| System | Status | Details |
|--------|--------|---------|
| TrainingActionCatalog | ✅ 170+ Actions | Kategorien: Hand, Mouth, VInsertion, AInsertion, PenisAction, Tool, Pain, Tentacle, Massage, Item, BodyMod, ForbiddenMagic, Interrogation, Service |
| TrainingService | ❌ Fehlend | Keine Service-Datei |
| Training UI Screen | ❌ Fehlend | Keine interaktive Training-Auswahl |
| Training Intensity | ❌ Fehlend | Light/Medium/Heavy nicht implementiert |
| Training Feedback (Visual) | ❌ Fehlend | |
| Training Bond Events | ❌ Fehlend | |
| EnhancedTrainingService | ⚠️ Referenziert | In GameRoot.cs Zeile 44, aber Implementierung unklar |

### 2.6 Research / Skill Tree

| System | Status | Details |
|--------|--------|---------|
| ResearchService | ❌ Fehlend | Keine Service-Datei |
| ResearchState (SaveState) | ✅ Definiert | UnlockedSkillIds Dictionary |
| 12 Research Skills | ✅ Geseedet | dairy_science, culinary_arts, herbalism, hospitality, craftsmanship, logistics, ranch_automation, adventure_training, field_medicine, plus weitere |
| Research UI Screen | ❌ Fehlend | |
| Skill Dependencies | ❌ Fehlend | Kein Abhängigkeitssystem |
| Research Costs | ❌ Fehlend | Keine Ressourcen-Kosten |
| Research Cooldowns | ❌ Fehlend | |

### 2.7 Pet System

| System | Status | Details |
|--------|--------|---------|
| PetService | ✅ Implementiert | Adopt, Feed, Play, Train, Status |
| PetState (SaveState) | ✅ Definiert | AdoptedPetIds, Entries Dictionary |
| 5 Pets | ✅ Geseedet | Stable Cat, Yard Hound, Fallen Angel Horse, Orthrus, Demon Hamster |
| Pet Mounting | ❌ Fehlend | |
| Pet Jobs | ❌ Fehlend | Guarding, Herding, Scouting etc. |
| Pet Traits | ⚠️ Teilweise | Hunger/Mood/Training/Bond existieren |
| Pet Events | ❌ Fehlend | |
| Pet Visuals | ❌ Fehlend | |

### 2.8 Character System

| System | Status | Details |
|--------|--------|---------|
| RosterService | ✅ Implementiert | Characters, DefinitionFor, Find |
| 8 Characters | ✅ Geseedet | Slay, Kagura, Maria, Sharon, Noir, Ayaka, En, Yukina |
| Character Creation | ✅ Vollständig | CharacterCreationScreen.tscn mit voller Customization |
| Generated Recruits | ✅ Implementiert | CharacterGenerationPools + SaveStateFactory |
| Name Pools | ✅ Implementiert | Japanese/English/French/German/Italian/Russian/Fantasy |
| Race Pools | ✅ Implementiert | 24 Races mit Gewichtung |
| Body Types | ✅ Implementiert | 4 Body Types mit Gewichtung |
| Talent Assignment | ✅ Implementiert | Talents werden generiert |
| Character Memory/Flags | ❌ Fehlend | First arrival, injury events etc. |
| Character Lifecycle | ❌ Fehlend | Retirement, leave, promotion |
| Character Renaming | ❌ Fehlend | |
| Portrait Customization | ❌ Fehlend | |
| Cosmetic Item Effects | ❌ Fehlend | |
| Archetype Tags | ❌ Fehlend | Ranch/Craft/Combat/Scholar specialist etc. |
| Backstory Snippets | ❌ Fehlend | |
| Rarity Levels | ❌ Fehlend | |
| Starting Bond Variance | ⚠️ Teilweise | |
| Contract Cost Variance | ❌ Fehlend | |

### 2.9 Ranch / Schedule

| System | Status | Details |
|--------|--------|---------|
| RanchService | ✅ Implementiert | Job-Ausgabe, Facility-Upgrades, Automation |
| 11 Facilities | ✅ Geseedet | Pasture, Barn, Kitchen, Workshop, Well, Storage, Dairy Barn, Pharmacy Lab, Bathhouse, Guest Rooms, plus |
| Facility Upgrades | ✅ Implementiert | Cost = BuildCost + currentLevel * 75 |
| Facility Upkeep | ✅ Implementiert | Mit Logistics-Skill Rabatt |
| Job Assignment | ✅ Einfach | Ein Job pro Character, kein Zeit-Block-System |
| Multi-Phase Schedule | ❌ Fehlend | Kein Morning/Afternoon/Evening/Night Assignments |
| Schedule Templates | ❌ Fehlend | Balanced week, production rush etc. |
| Job Requirements | ❌ Fehlend | Facility level, stat/skill requirements |
| Job Outcome Variance | ❌ Fehlend | Great success, mishap, injury etc. |
| Job Synergies | ❌ Fehlend | Pair bonuses, mentorship |
| Ranch Map Screen | ❌ Fehlend | |
| Facility Placement/Upgrade Map | ❌ Fehlend | |
| Upkeep Pressure | ⚠️ Teilweise | PetCareCost + FacilityUpkeep, aber keine Food/Medicine consumption |
| Economy Simulation Tests | ❌ Fehlend | |

### 2.10 Combat / Adventure

| System | Status | Details |
|--------|--------|---------|
| CombatService | ✅ Round-based (454 Zeilen) | Turn-log, party/enemy, deterministic rolls |
| AdventureService | ✅ Mission Resolve | Capture mechanics, deterministic scoring |
| 12 Missions | ✅ Geseedet | Local/Regional/Dangerous tiers |
| 14 Enemies | ✅ Geseedet | |
| Combat UI Screen | ⚠️ Teilweise | CombatReport existiert, Screen unklar |
| Party Size Limit | ❌ Fehlend | Kein expliziter Cap |
| Combat Roles | ❌ Fehlend | Vanguard, Striker, Defender etc. |
| Tactical Presets | ❌ Fehlend | |
| Enemy Preview | ❌ Fehlend | |
| Map/Dungeon Progression | ❌ Fehlend | |
| Mission Types | ⚠️ Teilweise | MissionDefinition hat Difficulty, RewardGold, RewardItemId |
| Risk/Reward Rules | ⚠️ Teilweise | Injury/Fatigue/Morale existieren |
| Adventure UI Polish | ❌ Fehlend | Mission cards, danger rating etc. |
| Item Consumables in Combat | ❌ Fehlend | |
| Boss Missions | ❌ Fehlend | |
| Multi-Stage Expeditions | ❌ Fehlend | |
| Equipment Durability | ❌ Fehlend | |

### 2.11 Town / Shop

| System | Status | Details |
|--------|--------|---------|
| ShopService | ✅ Buy/Sell | |
| TownService | ⚠️ Minimal | Nur Actions-Liste ("General store", "Facility planning" etc.) |
| Shop Stock Refresh | ❌ Fehlend | |
| Limited Stock | ❌ Fehlend | |
| Price Fluctuation | ❌ Fehlend | |
| Bulk Purchase/Sell | ❌ Fehlend | |
| Item Use Depth | ⚠️ Teilweise | UseItemOnCharacter mit 14 Items |
| Town Reputation | ❌ Fehlend | |
| Contract Board | ❌ Fehlend | |
| Town Event Calendar | ❌ Fehlend | |
| Town NPCs | ❌ Fehlend | |
| Town Locations | ❌ Fehlend | Market, guild hall, clinic etc. |

### 2.12 Milestones / Achievements

| System | Status | Details |
|--------|--------|---------|
| MilestoneService | ✅ Implementiert | CheckAfterSettlement, MarkMissionCompleted |
| 18 Milestones | ✅ Geseedet | DayReached, GoldReached, BondReached, ResearchUnlocked, CharacterCount, FacilityMaster, PetCount, EquipmentCount, MissionCompleted |
| Milestone UI Screen | ✅ Implementiert | |
| Milestone Chains | ❌ Fehlend | |
| Visible Goal Screen | ❌ Fehlend | Short/mid/long-term goals |
| Endings | ❌ Fehlend | |
| Post-Ending Continuation | ❌ Fehlend | |
| NG+ | ⚠️ Teilweise | NgPlusActive Flag existiert, aber keine Implementierung |
| Ranch Prestige Ranks | ❌ Fehlend | |
| Regional Expansion | ❌ Fehlend | Outpost ranches |
| Legendary Contracts | ❌ Fehlend | |
| Collection Books | ❌ Fehlend | |
| Mastery/Specialization | ❌ Fehlend | |

### 2.13 Bond Events / Relationships

| System | Status | Details |
|--------|--------|---------|
| BondService | ✅ Implementiert | AvailableEvents, ConductMentorship, CompleteEvent |
| 30+ Bond Events | ✅ Geseedet | Across 8 characters |
| Bond Event Screen | ✅ Implementiert | Narrative display mit Portrait + styled box |
| Narrative Text Display | ⚠️ Teilweise | Event-Text existiert, aber keine Dialog-Linien pro Character |
| Choice-Based Branching | ❌ Fehlend | |
| Story Progression Tracking | ⚠️ Teilweise | CompletedEventIds verhindert Replay |
| Dialogue Presentation | ❌ Fehlend | Speaker portrait, nameplate, dialogue box |
| Character-Specific Goals | ❌ Fehlend | |
| Relationship Web | ❌ Fehlend | Character-to-Character bonds |
| Text Localization | ❌ Fehlend | EN/JP support |
| Max Bond Cap | ⚠️ Teilweise | Bond auf 100 geklamped, kein Post-Max-Content |
| Bond Scene Integration | ⚠️ Teilweise | MatureContentHooks existiert, aber BondScenePlaceholder nur als Placeholder |

### 2.14 Magic System

| System | Status | Details |
|--------|--------|---------|
| MagicService | ❌ Fehlend | |
| General Magic (Teleportation, Hypnosis) | ❌ Fehlend | |
| Tentacle Magic | ❌ Fehlend | |
| Body Modification Magic | ❌ Fehlend | |
| Magic Marks/Runes | ❌ Fehlend | |
| Forbidden Magic | ❌ Fehlend | Brainwashing, Time Compression |
| Magic UI Screen | ❌ Fehlend | |

### 2.15 Pregnancy System

| System | Status | Details |
|--------|--------|---------|
| Pregnancy State | ❌ Fehlend | |
| Pregnancy Mechanics | ❌ Fehlend | |
| Childbirth | ❌ Fehlend | |
| Inheritance | ❌ Fehlend | |

### 2.16 Cooking / Crafting

| System | Status | Details |
|--------|--------|---------|
| Cooking System | ❌ Fehlend | |
| Crafting/Recipe System | ❌ Fehlend | |
| Workshop Upgrades (Recipes) | ❌ Fehlend | |
| Gardening/Farming | ❌ Fehlend | |

### 2.17 Breeding / Procreation

| System | Status | Details |
|--------|--------|---------|
| Breeding System | ❌ Fehlend | |
| Offspring System | ❌ Fehlend | |

### 2.18 Town NPCs / Events

| System | Status | Details |
|--------|--------|---------|
| Named Town NPCs | ❌ Fehlend | |
| Special Town Events | ❌ Fehlend | |
| Town Event Calendar | ❌ Fehlend | |

---

## 3. FEHLENDE UI SCREENS

| Screen | Status |
|--------|--------|
| Ranch Map Screen | ❌ Fehlend |
| Facility Detail Screen | ❌ Fehlend |
| Character Detail Screen | ⚠️ Teilweise (Roster Screen) |
| Character Event Screen | ⚠️ Teilweise (Bond Screen) |
| Schedule Planner (Time Blocks) | ❌ Fehlend |
| Contract Board Screen | ❌ Fehlend |
| Research Tree Screen | ❌ Fehlend |
| Crafting/Processing Screen | ❌ Fehlend |
| Storage/Inventory Screen | ⚠️ Teilweise (Shop Screen) |
| Equipment Screen mit Vergleich | ❌ Fehlend |
| Mission Party Setup Screen | ⚠️ Teilweise |
| Combat Result Replay Screen | ⚠️ Teilweise (Combat Report) |
| Pet Detail Screen | ⚠️ Teilweise (Pet Screen) |
| Collection Book Screen | ❌ Fehlend |
| Ending/Epilogue Screen | ❌ Fehlend |
| NG+ Setup Screen | ❌ Fehlend |
| Milk Economy Screen | ❌ Fehlend |
| Mental State Screen | ❌ Fehlend |
| Training Room Screen | ❌ Fehlend |
| Magic Lab Screen | ❌ Fehlend |
| Clothing/Equipment Management | ❌ Fehlend |

---

## 4. FEHLENDE SPIELSYSTEME (aus Original)

### P0 — Core-Systeme die fehlen

| System | Original | Godot | Gap |
|--------|----------|-------|-----|
| Training Actions | 170+ aus Train.csv | Catalog existiert, aber kein Gameplay | TrainingService + UI fehlt |
| Mental State Engine | Palam.csv + base.csv | FallState Enum + Models | MentalStateService + UI fehlt |
| Milk Economy Full | Abl.csv + Mark.csv | MilkEconomyService (basic) | UI, Processing, Pricing fehlt |
| Addiction System | Abl.csv + Mark.csv | AddictionState Model | AddictionService + UI fehlt |
| Full Equipment (8 Slots) | Equip.csv | 5 Slots implementiert | 3 Slots + UI fehlt |
| Full Clothing (100+ Items) | Item.csv | Seed-Daten | Outfit-System + UI fehlt |
| Magic System | Item.csv 600-900 | Nicht existiert | MagicService + UI fehlt |
| Research Tree | Skills mit Dependencies | 12 Skills ohne Dependencies | ResearchService + Tree UI fehlt |
| Full Schedule (Multi-Phase) | Time.csv | Ein Job pro Tag | Time-Block-System fehlt |
| Ranch Map | — | Nicht existiert | Ranch Map Screen fehlt |

### P1 — Charakter- und Inhaltserweiterung

| System | Gap |
|--------|-----|
| 10 originale Charaktere aus CSV | Nur 8 + Generated |
| Full CSV Stats (HP/SP/EP etc.) | Nur Ranch/Craft/Combat Skills |
| Talente (3-5 pro Char) | Talents existieren, aber nur als Seed |
| Body Type Mapping | 4 Types, aber keine CSV-Mapping |
| Character Lifecycle | Retirement, Leave, Promotion |
| Character Memory/Flags | First arrival, injury events etc. |
| Archetype Tags | Ranch/Craft/Combat/Scholar specialist |
| Generated Recruit Diversity | Name/Body/Job/Talent Pools existieren |

### P2 — Systemerweiterung

| System | Gap |
|--------|-----|
| Town Expansion | Nur 5 Actions, keine Locations |
| Town Reputation | |
| Contract Board | |
| Crafting System | |
| Cooking System | |
| Gardening/Farming | |
| Exploration/Adventure Zones | |
| Character Questlines | |
| Relationship Fall States | Nur Enum, keine Progression |
| Pregnancy System | |
| Breeding/Procreation | |
| Boss Missions | |
| Multi-Stage Expeditions | |
| Ranch Prestige Ranks | |
| Regional Expansion (Outposts) | |
| Legendary Contracts | |
| Collection Books | |
| Mastery/Specialization Trees | |

### P3 — Modernisierung

| System | Gap |
|--------|-----|
| Ranch Map mit Buildings | |
| Character Cards mit Readability | |
| Dashboard UX | |
| Recommendation System | |
| Tutorialization | |
| Accessibility | |
| Controller Support | |
| Audio Polish | |
| Animation Polish | |
| Modern Save Features | |
| Settings Depth | |
| Mod Support | |
| Endgame Progression | |
| NG+ Vollständig | |
| Challenge Modes | |

---

## 5. ARCHITEKTUR-GAPS

| Gap | Detail |
|-----|--------|
| GameServices.cs existiert nicht | Wurde in ManagementServices.cs + separate Services aufgeteilt |
| TownService nur Stubs | Actions-Liste, keine Logik |
| MentalStateService fehlt | Models existieren, kein Service |
| MilkEconomyService fehlt | In DailySettlement inline implementiert (MilkEconomyService) |
| AddictionService fehlt | Models existieren, kein Service |
| ResearchService fehlt | ResearchState existiert, kein Service |
| TrainingService fehlt | Catalog existiert, kein Service |
| MagicService fehlt | Nicht existiert |
| EnhancedTrainingService | In GameRoot referenziert, aber Implementierung unklar |
| DiscoveryService | In GameRoot referenziert (Zeile 48) |
| MercenaryService | In GameRoot referenziert (Zeile 49) |
| WinConditionService | In GameRoot referenziert (Zeile 50) |
| DailyReport-Integration | DailyEventService + CharacterGrowthService + ResourceConsumptionService in DailySettlement inline |
| ContentVersioning | Separate von SaveSchema — nicht implementiert |
| SaveID-Stabilität | Nicht dokumentiert ob stable ASCII IDs verwendet werden |

---

## 6. ASSET-GAPS

| Kategorie | Status | Gap |
|-----------|--------|-----|
| Layered Portrait Parts | 226+ Layer-Frames | Missing: kagura.png, ayaka/en/yukina portraits |
| Character Portraits | Layered system | 6 main cast portraits |
| Full-Body Character Art | ❌ | |
| Chibi/Map Sprites | ❌ | |
| Pet Portraits | ❌ | |
| Enemy Portraits | ❌ | |
| Boss Creature Art | ❌ | |
| Ranch Overview Map | ❌ | |
| Facility Building Icons | ❌ | |
| Town Location Backgrounds | ❌ | |
| Adventure Region Backgrounds | ❌ | |
| Seasonal Ranch Variants | ❌ | |
| UI Panels/Decorative Frames | ❌ | |
| Button States | ❌ | |
| Resource Icons | ❌ | |
| Stat Icons | ❌ | |
| Facility Icons | ❌ | |
| Job Category Icons | ❌ | |
| Mission Type Icons | ❌ | |
| Status Effect Icons | ❌ | |
| Equipment Slot Icons | ❌ | |
| Item Icons | ❌ | |
| Equipment Icons | ❌ | |
| Tool Props | ❌ | |
| Product Icons | ❌ | |
| Contract Reward Icons | ❌ | |
| Rare Material Icons | ❌ | |
| Pet Sprites | ❌ | |
| Animation/VFX | ❌ | |
| Weather Overlays | ❌ | |
| Music/Ambient Loops | ❌ | |
| UI SFX | ❌ | |
| App Icon | ❌ | |
| Logo/Title | ❌ | |

---

## 7. TEXT/NARRATIVE-GAPS

| Kategorie | Gap |
|-----------|-----|
| Main Premise Rewrite | ❌ |
| Intro Sequence | ❌ |
| First-Week Tutorial Dialogue | ❌ |
| Character Bios (all main cast) | ❌ |
| Generated Recruit Backstories | ❌ |
| Town Location Descriptions | ❌ |
| Mission Descriptions | ❌ |
| Contract Descriptions | ❌ |
| Facility Flavor Text | ❌ |
| Item Descriptions | ❌ |
| Research Descriptions | ❌ |
| Pet Descriptions | ❌ |
| Bond Event Texts | ❌ |
| Seasonal Event Scripts | ❌ |
| Ending Epilogues | ❌ |
| NG+ Intro Variants | ❌ |
| Public/Private Content Boundary Notes | ❌ |

---

## 8. IMPLEMENTIERUNGSREIHENFOLGE (Empfohlen)

### Phase 0 — Stabilisieren (erledigt)
- [x] Build/CI/Smoke Tests
- [x] Content Validation Tooling
- [x] Export Presets (Windows/Linux/macOS/Android/Web)
- [x] Authoring Guide

### Phase 1 — Core-Systeme vervollständigen
1. MentalStateService + UI (Fall States, Resistance, Dignity, Aversion, Corruption)
2. MilkEconomyService (vollständig) + Milk Economy Screen
3. AddictionService + UI (Addiction Types, Withdrawal)
4. TrainingService + Training Room Screen (170+ Actions)
5. ResearchService + Research Tree Screen (Skill Dependencies)
6. EquipmentService (8 Slots) + Clothing UI Screen
7. TownService (vollständig) + Town Locations + Events
8. ScheduleService (Multi-Phase) + Schedule Planner Screen
9. Ranch Map Screen
10. Character Detail Screen
11. Facility Detail Screen
12. Equipment Comparison Screen
13. Pet Detail Screen
14. Collection Book Screen
15. Ending/Epilogue Screen
16. NG+ Setup Screen

### Phase 2 — Systemerweiterung
17. Character Lifecycle (Retirement, Leave, Promotion)
18. Character Memory/Flags System
19. Archetype Tags für Generated Recruits
20. Town Reputation System
21. Contract Board
22. Crafting/Recipe System
23. Cooking System
24. Gardening/Farming
25. Exploration/Adventure Zones
26. Character Questlines
27. Relationship Web (Character-to-Character)
28. Magic System
29. Pregnancy System
30. Breeding/Procreation
31. Boss Missions
32. Multi-Stage Expeditions
33. Equipment Durability
34. Combat Roles (Vanguard, Striker, Defender etc.)
35. Tactical Presets
36. Mission Types (Scout, Gather, Escort, Hunt, Rescue, Defense)
37. Risk/Reward Rules
38. Enemy Variety (Bosses, Seasonal Threats)

### Phase 3 — Endgame & Modernisierung
39. Ranch Prestige Ranks
40. Regional Expansion (Outposts)
41. Legendary Contracts
42. Collection Books
43. Mastery/Specialization Trees
44. NG+ Vollständig
45. Challenge Modes
46. Dashboard UX
47. Recommendation System
48. Tutorialization
49. Accessibility
50. Controller Support
51. Audio/Animation Polish
52. Modern Save Features
53. Settings Depth
54. Mod Support
55. Platform Exports (Steam/itch)

### Phase 4 — Content Polish
56. Character Assets (Portraits, Full-Body, Chibi)
57. Environment Assets (Ranch, Town, Adventure)
58. UI Assets (Icons, Panels, Buttons)
59. Item/Equipment Icons
60. Pet Assets
61. Animation/VFX
62. Weather Overlays
63. Music/Ambient Loops
64. UI SFX
65. App Icon + Logo
66. Narrative Content (Premise, Tutorial, Bios, Events, Endings)

---

## 9. OPEN DESIGN DECISIONS

| Entscheidung | Optionen | Status |
|-------------|----------|--------|
| Public Game Direction | Cozy-Management vs. Dark-Fantasy vs. Character RPG | ❌ Offen |
| Combat Presentation | Automated Reports vs. Tactical Choices vs. Turn-Based View | ❌ Offen |
| Ranch Map | Decorative vs. Clickable vs. Placeable/Buildable | ❌ Offen |
| Generated Recruits | Endless vs. Capped vs. Reputation-Tied | ❌ Offen |
| Time Pressure | Relaxed vs. Strict | ❌ Offen |
| Debt as Campaign Driver | Ja vs. Nein | ❌ Offen |
| Endings | Exclusive Routes vs. Collectible Epilogues | ❌ Offen |
| NG+ | Carry Characters vs. Legacy Bonuses Only | ❌ Offen |
| Private Extension | Separate Save Block vs. Shared Save Fields | ❌ Offen |

---

## 10. OPEN QUESTIONS (aus IDEA.md)

| ID | Frage | Status |
|----|-------|--------|
| R1 | Wie funktioniert Job-Arbeit bei Fatigue/low Morale? | ⚠️ Fatigue existiert, Morale-Block unklar |
| R2 | Maximale Anzahl pro Facility-Typ? | ❌ Unklar |
| R3 | Stockpile-Limits? | ❌ Kein Cap sichtbar |
| R4 | Fatigue=100 — noch arbeitsfähig? | ⚠️ Reduziert Output, kein "incapacitated" |
| R5 | Pasture: passiv oder job-abhängig? | ❌ Unklar |
| R6 | Pet-Feeding/Training Economy? | ⚠️ CareCost existiert |
| C1 | Trigger für neuen Generated Recruit? | ⚠️ CurrentOffer existiert |
| C2 | Maximale Roster-Größe? | ❌ Kein Hard Cap |
| C3 | Characters können Ranch verlassen? | ❌ Kein Departure-Mechanic |
| C4 | Captured Recruit Flow? | ✅ Implementiert |
| C5 | Race/Body/Appearance Distribution? | ✅ CharacterGenerationPools |
| C6 | Talent Stacking/Conflicts? | ❌ Kein Conflict Resolution |
| A1 | Deterministic oder Random Combat? | ✅ Deterministic (seed-basiert) |
| A2 | Party-Size-Limit? | ❌ Kein Cap |
| A3 | Captured = Mercenary oder Recruit? | ✅ Nur Recruit |
| A4 | Capture-Difficulty Scaling? | ✅ Implementiert |
| A5 | Item Consumables in Combat? | ❌ Nicht implementiert |
| E1 | Gold Income/Expense Balance? | ❌ Keine Analyse |
| E2 | Milk Production? | ✅ MilkEconomyService |
| E3 | Research Cost/Cooldown? | ❌ Keine Costs/Cooldowns |
| E4 | Win/Lose Conditions? | ⚠️ VictoryDay existiert |
| E5 | NG+ Implementation? | ⚠️ NgPlusActive Flag |
| B1 | Bond Events Replay? | ❌ One-time (CompletedEventIds) |
| B2 | Post-Max Bond Content? | ❌ Kein Post-Max |
| B3 | Bond Events unlock neue Jobs/Abilities? | ❌ Nur Bond/Morale/Stockpile |
| B4 | Mentorship vs Bond Events? | ✅ Separate (BondService) |
| T1 | Save Schema Migration History? | ❌ Keine dokumentiert |
| T2 | Missing Portrait Assets? | ⚠️ ValidateVisualPools() |
| T3 | DataRegistry JSON production-ready? | ✅ TryLoadDatabase + Seed-Fallback |
| T4 | Testing Situation? | ✅ Smoke Tests (30+ tests) |
| T5 | Mature Content Integration? | ✅ Core gameplay, kein Toggle |
| T6 | Asset Pipeline für Portraits? | ✅ PortraitLayerCatalog |
| S1 | MVP Feature Set v1.0? | ❌ Offen |
| S2 | Out-of-Scope Features? | ❌ Offen |
| S3 | Seasons/Weather Gameplay? | ⚠️ Wetter existiert, kein Gameplay |
| S4 | Day/Night Cycle Effekte? | ⚠️ Phase enum, keine Effekte |
| S5 | Target Platform/Release? | ❌ Offen |

---

## 11. COMPLETENESS METRICS

| Bereich | Original (eraMakaiRanch) | Godot Remake | Coverage |
|---------|--------------------------|-------------|----------|
| Characters | 10+ CSV-Charaktere | 8 + Generated | 80% (Daten) |
| Jobs | 6+ mit Skill-Levels | 11 Jobs | 100% (Anzahl), 40% (Tiefe) |
| Items | 500+ Item.csv | 60+ Items | 12% |
| Equipment | 50+ Outfit-Typen | 10 Equipment | 20% |
| Training | 170+ Train.csv | 170+ Catalog | 100% (Catalog), 0% (Gameplay) |
| Facilities | 10+ | 11 | 100% (Anzahl) |
| Missions | 15+ | 12 | 80% |
| Enemies | 20+ | 14 | 70% |
| Bond Events | 50+ | 30+ | 60% |
| Milestones | 20+ | 18 | 90% |
| Skills | 12+ | 12 | 100% |
| Pets | 5 | 5 | 100% |
| Talents | 250+ Talent.csv | 40+ | 16% |
| Mental System | Palam.csv + base.csv | FallState Enum | 30% |
| Milk Economy | Abl.csv + Mark.csv | MilkEconomyService | 40% |
| Addiction | Abl.csv + Mark.csv | AddictionState Model | 10% |
| Magic | Item.csv 600-900 | Nicht implementiert | 0% |
| Clothing | 100+ Equip.csv | Seed-Daten | 5% |
| Schedule | Time.csv Multi-Phase | Ein Job/Tag | 20% |
| Ranch Map | — | Nicht implementiert | 0% |
| Town | Multiple Locations | 5 Actions | 10% |
| Combat | Party + Turn-based | Round-based | 60% |
| Pregnancy | Cflag.csv | Nicht implementiert | 0% |
| Breeding | — | Nicht implementiert | 0% |
| Cooking | — | Nicht implementiert | 0% |
| Crafting | — | Nicht implementiert | 0% |
| NG+ | NgPlusActive | Flag nur | 5% |
| Endings | Multiple | Nicht implementiert | 0% |
| UI Screens | 14 | 14 | 100% (Anzahl), 40% (Inhalt) |
| Save Schema | — | v14 | 100% |
| Layered Portraits | portrait.csv | 226+ Layer-Frames | 100% (System), 70% (Assets) |

### Gesamt-Coverage: ~45%

---

## 12. IMPLEMENTIERUNGS-PRIO MATRIX

| Feature | P-Level | Aufwand | Impact |
|---------|---------|---------|--------|
| MentalStateService | P0 | Mittel | Hoch |
| Milk Economy Screen | P0 | Mittel | Hoch |
| AddictionService | P0 | Mittel | Hoch |
| TrainingService + UI | P0 | Hoch | Hoch |
| ResearchService + Tree | P0 | Mittel | Hoch |
| 8-Slot Equipment | P1 | Klein | Mittel |
| Clothing UI | P1 | Mittel | Hoch |
| TownService (vollständig) | P1 | Mittel | Hoch |
| Multi-Phase Schedule | P1 | Mittel | Hoch |
| Ranch Map Screen | P1 | Mittel | Hoch |
| Character Detail Screen | P1 | Klein | Mittel |
| Facility Detail Screen | P1 | Klein | Mittel |
| Equipment Comparison | P1 | Klein | Mittel |
| Pet Detail Screen | P1 | Klein | Mittel |
| Collection Book Screen | P1 | Mittel | Mittel |
| Ending Screen | P1 | Mittel | Hoch |
| NG+ Vollständig | P1 | Mittel | Hoch |
| Character Lifecycle | P2 | Hoch | Mittel |
| Town Reputation | P2 | Mittel | Hoch |
| Contract Board | P2 | Mittel | Hoch |
| Crafting System | P2 | Hoch | Mittel |
| Cooking System | P2 | Hoch | Mittel |
| Gardening/Farming | P2 | Hoch | Mittel |
| Exploration Zones | P2 | Hoch | Hoch |
| Character Questlines | P2 | Hoch | Hoch |
| Relationship Web | P2 | Mittel | Hoch |
| Magic System | P2 | Sehr Hoch | Hoch |
| Pregnancy System | P2 | Sehr Hoch | Hoch |
| Boss Missions | P2 | Hoch | Hoch |
| Multi-Stage Expeditions | P2 | Hoch | Hoch |
| Combat Roles | P2 | Mittel | Mittel |
| Tactical Presets | P2 | Mittel | Mittel |
| Ranch Prestige Ranks | P2 | Mittel | Hoch |
| Regional Expansion | P2 | Sehr Hoch | Hoch |
| Legendary Contracts | P2 | Hoch | Hoch |
| Mastery/Specialization | P2 | Hoch | Hoch |
| Dashboard UX | P2 | Mittel | Hoch |
| Tutorialization | P2 | Mittel | Hoch |
| Accessibility | P2 | Mittel | Hoch |
| Controller Support | P2 | Mittel | Mittel |
| Audio/Animation Polish | P2 | Hoch | Hoch |
| Mod Support | P3 | Sehr Hoch | Mittel |
| Platform Exports | P3 | Mittel | Hoch |
| Character Assets | P3 | Sehr Hoch | Hoch |
| Environment Assets | P3 | Sehr Hoch | Hoch |
| UI Assets | P3 | Hoch | Hoch |
| Item/Equipment Icons | P3 | Hoch | Hoch |
| Pet Assets | P3 | Hoch | Mittel |
| Animation/VFX | P3 | Hoch | Mittel |
| Music/Ambient | P3 | Sehr Hoch | Hoch |
| Narrative Content | P3 | Sehr Hoch | Hoch |
