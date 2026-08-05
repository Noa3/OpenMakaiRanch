# OpenMakaiRanch — Product Vision & Strategy

> Purpose: What this game is, where it stands, and what matters next.

---

## 1. What Is This?

A **systems-first**, **NSFW-integrated** Godot 4.7 .NET remake of **eraMakaiRanch** — a Japanese ranch-sim / life-sim.

Adult content is **core gameplay**, not an addon:
- Training (170+ actions across Hand, Mouth, V/A Insertion, Tools, Pain, Tentacle, Massage, Item, BodyMod, ForbiddenMagic)
- Mental state / fall state engine (Resistance → Dignity → Aversion → Corruption → Collapse)
- Breast milk economy (production, quality tiers, pricing, processing)
- Addiction system (multiple types, withdrawal effects)
- Layered portrait rendering (226+ frames: skin, body, breasts, race, hair, cloth)
- Bond events (30+ scripted scenes across 8 characters)

Not abstracted. Not placeholdered. No toggle system.

---

## 2. Core Pillars

| # | Pillar | Status |
|---|--------|--------|
| 1 | **Ranch Management** | Jobs, facilities, stockpile, economy — functional, needs depth |
| 2 | **Character Roster** | 8 seeded + generated recruits, full creation screen — needs lifecycle |
| 3 | **Adventure / Combat** | Round-based deterministic combat with capture — needs roles, tactics, zones |
| 4 | **Economy** | Gold, stockpile, shop buy/sell — needs depth (milk, crafting, contracts) |
| 5 | **Progression** | Talents (40+), skills (12), milestones (18) — needs trees, dependencies |
| 6 | **Bond Events** | 30+ events, narrative display — needs branching, dialogue, relationship web |
| 7 | **Pet System** | 5 pets with hunger/mood/training — needs jobs, events, visuals |
| 8 | **Mature Content** | Training catalog, milk economy, addiction, mental state — partially wired |
| 9 | **Save System** | Schema v14, JSON, 3 slots — functional |
| 10 | **UI Shell** | 14 screens, scene-first — functional but many screens incomplete |

---

## 3. Current State

### What Works
- **Build**: `dotnet build` → 0 errors (net10.0)
- **CI**: Build + smoke tests (30+ tests)
- **New Game**: Full character creation → ranch loop
- **Day Cycle**: Morning → Afternoon → Evening → Night → settlement
- **Daily Settlement**: Job output, facility upkeep, pet care, milk auto-ship, random events, milestone checks
- **Combat**: Round-based deterministic resolution, turn logs, capture
- **Shop**: Buy/sell items
- **Equipment**: 5-slot equip/unequip with stat bonuses
- **Talents**: 40+ talents with stat bonuses, fatigue resistance, job output multipliers
- **Bond Service**: Event availability, mentorship, completion tracking
- **Milestones**: 18 milestones with multiple trigger types
- **Data**: JSON in `data/` with seed fallback (DataRegistry)
- **Portraits**: Layered system with 226+ frames
- **Character Generation**: Weighted pools (24 races, name pools, body types, talents)
- **Save/Load**: Schema v14, migration pipeline

### Architecture
```
GameRoot (autoload)
├── DataRegistry (JSON + seed, 11 dictionaries)
├── SaveState (Schema v14, 16 sub-states)
├── Services (18+ gameplay services)
├── UiShellController (14 screens)
└── SceneRouter
```

### Data Breadth (seeded)
- 8 Characters, 11 Jobs, 60+ Items, 11 Facilities, 12 Missions
- 14 Enemies, 18 Milestones, 12 Skills, 5 Pets, 30+ Bond Events, 40+ Talents

---

## 4. Where We Stand

**Gesamt-Coverage des Originals: ~45%**

| Bereich | Coverage | Gap |
|---------|----------|-----|
| Core-Systeme (Ranch, Schedule, Combat, Save) | 80% | Multi-Phase-Schedule, Ranch Map, Town |
| Mature Content (Catalog, Models, Basic Hooks) | 40% | Services, UI, Integration |
| Items/Clothing (Seed-Daten existieren) | 12% | Outfit-System, 8-Slot Equipment, UI |
| Talents (40+ geseedet) | 16% | 250+ aus Talent.csv, Conflict Resolution |
| UI Screens (14 existieren) | 40% | Viele Screens unvollständig/inhaltlos |
| Assets (Layered Portrait System) | 70% | Missing portraits, keine Environment/UI Icons |
| Narrative/Text | 5% | Kein Premise, Tutorial, Bios, Events, Endings |
| Endgame (NG+, Endings, Prestige) | 0% | |
| Modern Features (Dashboard, Tutorial, Accessibility) | 0% | |

---

## 5. Critical Gaps (Müssen geschlossen werden)

### P0 — Ohne diese gibt es kein spielbares Spiel

| Gap | Warum kritisch |
|-----|----------------|
| **Mental State Engine** | Fall States (Normal→Love→Devotion→Collapse→MilkCow) sind Core-Gameplay, aber nur ein Enum |
| **Milk Economy UI** | Milch ist Core-Wirtschaftssäule, aber kein Verkauf/Verarbeitung/Preise |
| **Addiction System** | Core-NSFW-Mechanic, aber nur SaveState-Model ohne Service |
| **Training Gameplay** | 170+ Actions im Catalog, aber kein TrainingService oder UI |
| **Research Tree** | 12 Skills ohne Dependencies, kein ResearchService |
| **8-Slot Equipment + Clothing UI** | Nur 5 Slots, kein Outfit-Management |
| **Ranch Map** | Kein visuelles Ranch-Überblick — Spieler sieht nur Listen |
| **Town Vollständig** | Nur 5 Text-Actions, keine Locations/Events/Reputation |

### P1 — Ohne diese ist das Spiel nicht befriedigend

| Gap | Warum wichtig |
|-----|---------------|
| **Multi-Phase Schedule** | Ein Job pro Tag ist zu flach |
| **Character Detail Screen** | Spieler kann keine Charaktere inspizieren |
| **Facility Detail Screen** | Keine Facility-Inspektion |
| **Equipment Comparison** | Kein Gear-Management |
| **Ending + NG+** | Kein Ziel, kein Replay |
| **Character Lifecycle** | Keine Retirement/Leave/Promotion |

### P2 — Ohne diese bleibt das Spiel flach

| Gap | Warum wichtig |
|-----|---------------|
| **Crafting/Cooking** | Keine Produktionstiefe |
| **Town Reputation + Contracts** | Keine externe Wirtschaft |
| **Exploration Zones** | Abenteuer nur als Mission-Buttons |
| **Relationship Web** | Bond nur Character→Player, keine Character→Character |
| **Magic System** | 300+ Magic-Einträge aus Item.csv, komplett leer |
| **Pregnancy/Breeding** | Original-Mechanic nicht umgesetzt |
| **Boss Missions** | Keine Endgame-Herausforderung |

---

## 6. Open Design Decisions (Brauchen eine Antwort)

| Entscheidung | Optionen | Empfehlung |
|-------------|----------|------------|
| **Public Game Direction** | Cozy-Management vs. Dark-Fantasy vs. Character RPG | **Dark-Fantasy Ranch-Management** — der Originalton |
| **Combat Presentation** | Reports vs. Tactical Choices vs. Turn-Based View | **Tactical Choices** — Balance zwischen Strategie und Lesbarkeit |
| **Ranch Map** | Decorative vs. Clickable vs. Placeable | **Clickable** — Inspektion ohne Komplexität |
| **Generated Recruits** | Endless vs. Capped vs. Reputation-Tied | **Reputation-Tied** — pacing + Strategie |
| **Time Pressure** | Relaxed vs. Strict | **Moderate** — Debt als sanfter Druck |
| **Endings** | Exclusive Routes vs. Collectible Epilogues | **Collectible Epilogues** — passt zum Ranch-Sim |
| **NG+** | Carry Characters vs. Legacy Bonuses | **Legacy Bonuses** — einfacher, weniger Save-Bloat |
| **Private Extension** | Separate Save vs. Shared Fields | **Shared Save Fields** — weniger Migration |

---

## 7. Next Steps (Nächste 10 Tasks)

1. **MentalStateService + UI** — Fall State Engine, Resistance/Dignity/Aversion/Corruption
2. **Milk Economy Screen** — Produktion, Qualität, Preise, Versand
3. **AddictionService** — Tracking, Withdrawal, UI
4. **TrainingService + Training Room** — 170+ Actions als Gameplay
5. **ResearchService + Tree** — Skill Dependencies, Costs
6. **8-Slot Equipment + Clothing UI** — Outfit-Management
7. **TownService (vollständig)** — Locations, Reputation, Events
8. **Multi-Phase Schedule** — Morning/Afternoon/Evening/Night
9. **Ranch Map Screen** — Clickable Facility-Überblick
10. **Character Detail Screen** — Inspektion, Stats, Equipment

Siehe `docs/plan.md` für den vollständigen technischen Abgleich (66 Tasks, 12 Phasen, Prio-Matrix).

---

## 8. Original Reference

| Quelle | Pfad | Inhalt |
|--------|------|--------|
| eraMakaiRanch CSVs | `eraMakaiRanch-game-eng-translation/` | 40+ CSVs: base, Abl, Palam, Train, Item, Talent, Equip, Str, Cstr, Cflag, Mark, portrait.csv (8185 Zeilen) |
| Architecture | `docs/ARCHITECTURE.md` | Scene-Flow, Core Autoloads, Gameplay Layout, UI Convention |
| TODO/Backlog | `docs/REMAKE_TODO.md` | P0-P4 Backlog, Artist Briefs, QA Checklist, Implementation Order |
| Gap Analysis | `docs/gap-analysis.md` | Implemented vs. Missing, Priority Matrix |
| Full Audit | `AUDIT.md` | Original Systems, CSV Mapping, Character Portraits, NSFW Details |
| This File | `IDEA.md` | Product Vision, Strategy, Decisions, Next Steps |

---

*Let updated: 2026-08-05*
*Previous: 2026-07-26*
