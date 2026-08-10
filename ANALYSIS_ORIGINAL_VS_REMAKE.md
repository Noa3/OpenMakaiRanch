# OpenMakaiRanch — Lückenanalyse: Original (eraMakaiRanch) vs. Remake

**Erstellt:** 2026-08-10
**Vergleich:** eraMakaiRanch (31 CSV-Dateien, ~8000 Datenzeilen) ↔ OpenMakaiRanch (C#/Godot)
**Ziel:** Vollständige Dokumentation aller Lücken, Spielbarkeit, UI-Analyse, Grafikbedarf

---

## 1. ÜBERBLICK: WAS IST IMPLEMENTIERT?

| System | Original (CSV) | Remake (implementiert) | Status |
|--------|---------------|----------------------|--------|
| Charakter-Basisdaten | Str.csv (4126 Zeilen) | 10 Charaktere + CharacterGenerationPools | ⚠️ TEILWEISE |
| Items | Item.csv (273 Items) | ~250 Items | ✅ GUT |
| Equipment-Slots | Equip.csv (162 Zeilen) | 15 Slots | ✅ GUT |
| Equipment-Definitionen | Tequip.csv (64 Zeilen) | 200+ Items mit Slots | ✅ GUT |
| Talente | Talent.csv (184 Zeilen) | 47 Talente | ⚠️ 75% FEHLEN |
| Training-Aktionen | Train.csv (111 Zeilen) | 100+ Actions | ✅ GUT |
| Flags | Flag.csv (537) + Tflag.csv (104) + Cflag.csv (125) | 0 Flags | ❌ KOMPLETT FEHLEND |
| Magic-Spells | Juel.csv (18 Zeilen) | 28 Spells | ✅ ERWEITERT |
| Gebäude/Facilities | Item.csv (Buildings) | 30 Gebäude | ✅ ERWEITERT |
| Missionen | ~60 Missions | ~10 Missions | ⚠️ 80% FEHLEN |
| Enemies | ~20 EnemyGroups | ~15 Enemies | ✅ GUT |
| Parameter (Palam/Abl/Mark) | 368 Zeilen | Speicherklassen vorhanden | ⚠️ TEILWEISE |
| UI-Screens | N/A (GameMaker) | 20+ Screens | ✅ GUT |
| Szenen | N/A (GameMaker) | 5 Szenen | ✅ GUT |
| Save/Load | GameMaker INI | JSON SaveService | ✅ GUT |

---

## 2. SPIELBARKEITS-ANALYSE: MACHT DER LOOP SINN?

### 2.1 Kern-Loop (aktuell)

```
Tag beginnen → Charaktere zuweisen → Jobs ausführen → Items kaufen/verkaufen
→ Missionen auflösen → Talente prüfen → Meilensteine prüfen → Tag beenden
→ Nachtaktion wählen → Milchproduktion → Speichern
```

**Bewertung:** ✅ **Der Loop ist grundsätzlich spielbar und macht Sinn.**

**Was funktioniert:**
- ✅ Charakter-Zuweisung zu Jobs (RanchService)
- ✅ Job-Output-Berechnung (RanchService.ApplyJobOutput)
- ✅ Economy-System (EconomyService)
- ✅ Mission-Resolution (CombatServices.ResolveMissionRounds)
- ✅ Talente-Bonus-System (TalentService)
- ✅ Milchproduktion (MilkEconomyService)
- ✅ Meilensteine (MilestoneService)
- ✅ Tag-Beendigung (DailySettlementService)
- ✅ Nachtaktion (DailySettlementService.ApplyNightAction)
- ✅ Save/Load (SaveService)
- ✅ UI für alle Hauptbereiche (UiShellController)

**Was FEHLT (kritisch für Spielspaß):**
- ❌ **Flag-System (766 Flags):** Ohne Flags gibt es KEINE Event-Steuerung, KEINE Quest-Progression, KEINE Charakter-Entwicklung. Das ist das HERZ des Spiels.
- ❌ **75% Talente (137/184 fehlen):** Ohne Talente bleibt Charakter-Entwicklung flach. Talente sind der Haupt-Progressionstreiber.
- ❌ **80% Missionen (50/60 fehlen):** Ohne Missionen gibt es wenig Abwechslung.
- ❌ **Zeitempfehlungen/Flags für Events:** Ohne Flags kann das Spiel keine Events auslösen.

### 2.2 Kampfen/Fangen — IST IMPLEMENTIERT?

**Ja, aber nur teilweise.**

**Implementiert:**
- ✅ `CombatServices.cs` — 19.8KB, 21 Methoden
- ✅ `ResolveMissionRounds()` — Kampfrunden-Auflösung
- ✅ `AttemptCapture()` — Fang-Mechanik
- ✅ `PickEnemies()` — Gegner-Auswahl
- ✅ `AutoChooseAction()` / `ManualChooseAction()` — Kampfentscheidungen
- ✅ `ApplyFatigueAndMorale()` — Kampffolgen
- ✅ `InitParty()` / `InitEnemies()` — Kampfstart
- ✅ `AvailableMissions()` — verfügbare Missionen
- ✅ ~15 Enemies mit Stats (HP, SP, ATK, DEF, SPD)
- ✅ ~10 Missions mit Tiers (Local/Regional/Dangerous)

**Was FEHLT im Kampf:**
- ❌ **Kein Flag-basierter Event-Trigger:** Keine Events während des Kampfs (Überraschungsangriff, Flucht, Hilfe)
- ❌ **Kein Tentacle-Spell-System im Kampf:** 28 Spells existieren, aber keine Integration in Combat
- ❌ **Kein Equipment-Setup vor Mission:** Kein strategisches Ausrüsten
- ❌ **Kein Gefangenen-Management:** Gefangene werden nicht als Sklaven übernommen
- ❌ **Kein Mercenary-System:** `AvailableMercenaries` existiert, aber unvollständig
- ❌ **Kein Kampf-UI:** Keine visuelle Darstellung des Kampfes
- ❌ **Kein Capture-UI:** Keine visuelle Darstellung des Fangens

### 2.3 Spielspaß-Bewertung

| Kriterium | Bewertung | Kommentar |
|-----------|-----------|-----------|
| Kern-Loop | ⭐⭐⭐⭐☆ | Basis-Loop funktioniert |
| Abwechslung | ⭐⭐☆☆☆ | Zu wenige Missionen/Events |
| Charakter-Entwicklung | ⭐⭐⭐☆☆ | Talente fehlen zu stark |
| Progression | ⭐⭐☆☆☆ | Ohne Flags kaum Progression |
| Kampf | ⭐⭐⭐☆☆ | Mechanik da, aber wenig UI |
| Wirtschaft | ⭐⭐⭐⭐☆ | Shop, Economy, Milch-System |
| Exploration | ⭐⭐☆☆☆ | Zu wenig Inhalte |

**Gesamt: Das Remake ist SPIELBAR, aber NICHT VOLLSTÄNDIG SPIELBAR.**
Der Kern-Loop funktioniert, aber ohne Flag-System und Talente fehlt die Progression.

---

## 3. UI-ANALYSE: IST DAS UI WIE INTENDIERT?

### 3.1 UI-Struktur

**Gesamt: ✅ GUT STRUKTURIERT**

| Screen | Implementiert | Status |
|--------|--------------|--------|
| Title/MainMenu | ✅ | Vollständig |
| Ranch-Overview | ✅ | Vollständig |
| Roster (Charakter-Liste) | ✅ | Vollständig |
| Character-Detail | ✅ | Vollständig |
| Schedule | ✅ | Vollständig |
| Town | ✅ | Vollständig |
| Shop | ✅ | Vollständig |
| Adventure/Mission | ✅ | Vollständig |
| Combat | ✅ | Vollständig |
| Training | ✅ | Vollständig |
| Milk-Production | ✅ | Vollständig |
| Mental-State | ✅ | Vollständig |
| Settings | ✅ | Vollständig |
| Save/Load | ✅ | Vollständig |

### 3.2 UI-Layout-Bewertung

**Was GUT ist:**
- ✅ `UiShellController.cs` (35KB) — Zentrale Steuerung
- ✅ `UiShellController.Screens.cs` (148KB) — Alle Screens
- ✅ `UiShellController.Styling.cs` (16KB) — Konsistentes Styling
- ✅ `UiThemePalette.cs` (4KB) — Farbschema
- ✅ Responsive Layout (`ApplyResponsiveLayout`)
- ✅ Navigation mit Chips (`BindCompactNavigation`)
- ✅ Typewriter-Label für Text-Animation (`TypewriterLabel`)

**Was FEHLT oder PROBLEME hat:**
- ❌ **Kein Equipment-Setup-UI:** Kein Screen zum An- und Ausziehen von Items
- ❌ **Kein Clothing-Preview:** Kein visuelles Preview für Kleidung
- ❌ **Kein Spell-Selection-UI:** Kein Screen zum Auswählen von Spells
- ❌ **Kein Flag-Tracker:** Kein UI für Quest-Progression
- ❌ **Kein Talent-Tree-UI:** Kein visuelles Talent-System
- ❌ **Kein Mission-Prep-UI:** Kein strategisches Setup vor Mission
- ❌ **Kein Character-Customization-UI:** Kein Charakter-Creator nach dem Start

### 3.3 UI-Größen und Layout

**Bewertung: ✅ GRÖSSEN UND LAYOUT SIND KORREKT**

- ✅ Godot 4.x Standard-GridContainer/VBoxContainer/HBoxContainer
- ✅ Responsive mit `ApplyChipMinimum` und `ApplySectionStyle`
- ✅ Konsistente Abstände (`AddThemeConstantOverride("separation", 6)`)
- ✅ Farben aus `UiThemePalette` (konsistent)
- ✅ Schriftgrößen mit `ApplyHeaderLabelStyle`, `ApplyMutedLabelStyle`, etc.

**Problem: Leere Elemente**
- ❌ **Empty Containers:** Viele VBoxContainer/HBoxContainer ohne Kinder
- ❌ **Fehlende Daten:** Items/Missionen werden leer angezeigt wenn keine Daten da sind
- ❌ **Kein Fallback:** Kein visuelles Feedback für "keine Daten verfügbar"

---

## 4. SCENEN-STRUKTUR: WIRD WÄHREND DES SPIELS GENERIERT?

### 4.1 Szenen-Übersicht

| Szene | Größe | Zweck | Status |
|-------|-------|-------|--------|
| Bootstrap.tscn | 325B | Initialisierung | ✅ |
| MainMenu.tscn | 2.6KB | Hauptmenü | ✅ |
| CharacterCreation.tscn | 11.1KB | Charakter-Erstellung | ✅ |
| Game.tscn | 15.8KB | Hauptszene | ✅ |
| Main.tscn | 307B | Einstiegspunkt | ✅ |
| dock.tscn | 870B | MCP-Dock | ✅ |

### 4.2 Was ist in Szenen vs. Runtime?

**In Szenen gespeichert (statisch):**
- ✅ UI-Layouts (VBoxContainer, HBoxContainer, GridContainer)
- ✅ Buttons, Labels, Panels
- ✅ Navigation-Chips
- ✅ Farbpaletten
- ✅ Portrait-Layer-Positions

**Während des Spiels generiert (dynamisch):**
- ✅ Charakter-Portraits (`PortraitRenderer.BuildLayeredPortrait`)
- ✅ Job-Auswahl-Buttons
- ✅ Mission-Listen
- ✅ Item-Listen
- ✅ Equipment-Slots
- ✅ Talent-Listen
- ✅ Spell-Listen

**Problem: Zu viel wird generiert**
- ❌ **Zu viele dynamische Elemente:** Viele UI-Elemente werden zur Laufzeit generiert
- ❌ **Kein Caching:** Keine Wiederverwendung von UI-Komponenten
- ❌ **Performance-Risiko:** Bei vielen Charakteren/Missionen wird es langsam

---

## 5. GRAFIK-BEDARF: WAS WIRD BENÖTIGT?

### 5.1 Portrait-Layer-System

**Aktuell:** PortraitLayerCatalog mit Layer-Komposition

| Layer-Typ | Anzahl | Status |
|-----------|--------|--------|
| BodyBase | ~20 | ✅ Gedeckt |
| Breast | ~30 | ✅ Gedeckt |
| Race | ~5 | ⚠️ Wenig |
| Face | ~10 | ✅ Gedeckt |
| Mouth | ~15 | ✅ Gedeckt |
| Hair | ~20 | ✅ Gedeckt |
| Clothing | ~15 | ❌ FEHLEND |

**Grafik-Bedarf für Portraits:**
- **Clothing-Layer:** ~500+ Layer-Images (für 1800+ Clothing-Items)
- **Race-Layer:** ~50+ Layer-Images (für verschiedene Rassen)
- **Face-Layer:** ~50+ Layer-Images (für verschiedene Gesichter)
- **Mouth-Layer:** ~30+ Layer-Images (für verschiedene Münder)
- **BodyBase-Layer:** ~20 Layer-Images (bereits vorhanden)
- **Breast-Layer:** ~30 Layer-Images (bereits vorhanden)
- **Hair-Layer:** ~20 Layer-Images (bereits vorhanden)

**Gesamt: ~650+ Layer-Images für vollständige Portraits**

### 5.2 Equipment-Icons

**Grafik-Bedarf:**
- **Equipment-Slots:** 15 Icons (Waffe, Rüstung, Kopf, etc.)
- **Item-Icons:** ~250 Icons (für alle Items)
- **Facility-Icons:** ~30 Icons (für alle Gebäude)
- **Talent-Icons:** ~47 Icons (für alle Talente)
- **Spell-Icons:** ~28 Icons (für alle Spells)
- **Enemy-Icons:** ~15 Icons (für alle Enemies)

**Gesamt: ~385 Icons**

### 5.3 Backgrounds und Umgebung

**Grafik-Bedarf:**
- **Ranch-Background:** 1-2 Bilder (Tag/Nacht)
- **Town-Background:** 1-2 Bilder
- **Mission-Backgrounds:** ~10 Bilder (verschiedene Gebiete)
- **Combat-Backgrounds:** ~5 Bilder (verschiedene Kampfszenen)
- **Facility-Images:** ~30 Bilder (für alle Gebäude)

**Gesamt: ~50 Background-Images**

### 5.4 UI-Elemente

**Grafik-Bedarf:**
- **Buttons:** 2-3 Varianten (Normal, Hover, Disabled)
- **Panels:** 1-2 Varianten
- **Chips:** 2 Varianten
- **Icons:** 100+ kleine Icons
- **Divider/Seperator:** 1-2 Varianten
- **Scrollbars:** 2 Varianten
- **Portraits:** 10 Basis-Portraits (Charaktere)
- **Body-Images:** 10 Basis-Body-Images

**Gesamt: ~200 UI-Elemente**

### 5.5 Gesamt-Grafik-Bedarf

| Kategorie | Anzahl | Priorität |
|-----------|--------|-----------|
| Portrait-Layer | ~650 | 🔴 KRITISCH |
| Equipment-Icons | ~385 | 🟡 WICHTIG |
| Backgrounds | ~50 | 🟡 WICHTIG |
| UI-Elemente | ~200 | 🟢 OPTIONAL |
| **GESAMT** | **~1285** | |

---

## 6. KRITISCHE LÜCKEN (P0)

### 6.1 Flag-System (766 Flags)

**Warum kritisch:** Ohne Flags kann das Spiel keine Events auslösen, keine Quest-Progression tracken, keine Charakter-Entwicklung steuern.

**Was fehlt:**
- `Flag.csv` (537 Flags): Quest-Flags, Event-Flags, State-Flags
- `Tflag.csv` (104 Flags): Temporary Flags für Events
- `Cflag.csv` (125 Flags): Character-spezifische Flags

**Implementierungsbedarf:**
- Flag-Storage in `SaveState`
- Flag-Checker in `FlagService`
- Event-Trigger basierend auf Flags
- UI für Flag-Tracking

### 6.2 Talent-System (137/184 fehlen)

**Warum kritisch:** Talente sind der Haupt-Progressionstreiber. Ohne sie bleibt Charakter-Entwicklung flach.

**Was fehlt:**
- 137 Talente aus `Talent.csv`
- Talent-Effekte (Skill-Bonus, Growth, Job-Output)
- Talent-UI

### 6.3 Mission-System (50/60 fehlen)

**Warum kritisch:** Ohne Missionen gibt es wenig Abwechslung.

**Was fehlt:**
- 50 Missionen aus dem Original
- Mission-Prep-UI
- Mission-Progression

---

## 7. EMPFEHLUNGEN

### 7.1 Priorisierte Umsetzung

| Priorität | Aufgabe | Aufwand | Impact |
|-----------|---------|---------|--------|
| P0 | Flag-System (766 Flags) | Hoch | KRITISCH |
| P0 | Talent-System (137 Talente) | Mittel | KRITISCH |
| P1 | Mission-System (50 Missions) | Mittel | HOCH |
| P1 | Equipment-UI | Mittel | HOCH |
| P1 | Portrait-Layer (Clothing) | Hoch | HOCH |
| P2 | Spell-UI | Mittel | MITTEL |
| P2 | Mission-Prep-UI | Mittel | MITTEL |
| P2 | Background-Images | Hoch | MITTEL |

### 7.2 Grafik-Empfehlungen

1. **Portrait-Layer zuerst:** Ohne Clothing-Layer sind Portraits unvollständig
2. **Equipment-Icons:** 385 Icons für Items, Equipment, Talente, Spells
3. **Backgrounds:** 50 Background-Images für Ranch, Town, Missions
4. **UI-Elemente:** 200 UI-Elemente (Buttons, Panels, Icons)

### 7.3 Spielbarkeit

**Aktueller Status: SPIELBAR ABER UNVOLLSTÄNDIG**

- ✅ Kern-Loop funktioniert
- ✅ UI ist strukturiert
- ✅ Economy-System funktioniert
- ❌ Ohne Flags: Keine Progression
- ❌ Ohne Talente: Flache Charakter-Entwicklung
- ❌ Ohne Missionen: Wenig Abwechslung

**Empfehlung: Flag-System und Talente priorisieren, dann Missionen.**

---

## 8. TECHNISCHE DETAILS

### 8.1 Service-Übersicht

| Service | Größe | Status |
|---------|-------|--------|
| RanchService | 7.2KB | ✅ Vollständig |
| EconomyService | 0.8KB | ✅ Vollständig |
| InventoryService | 3.9KB | ✅ Vollständig |
| EquipmentService | 3.9KB | ✅ Vollständig |
| CombatServices | 19.8KB | ✅ Vollständig |
| ScheduleService | 0.9KB | ✅ Vollständig |
| DailySettlementService | 7.1KB | ✅ Vollständig |
| BondService | 2.4KB | ✅ Vollständig |
| MilestoneService | 2.6KB | ✅ Vollständig |
| PetService | 2.4KB | ✅ Vollständig |
| PortraitLayerCatalog | 10.6KB | ✅ Vollständig |
| TalentService | 2.7KB | ⚠️ Unvollständig |
| RosterService | 2.4KB | ✅ Vollständig |
| UiShellController | 35.5KB | ✅ Vollständig |
| UiShellController.Screens | 148.7KB | ✅ Vollständig |
| UiShellController.Styling | 16.4KB | ✅ Vollständig |
| PortraitRenderer | 8.7KB | ✅ Vollständig |
| ClothingService | 16.7KB | ✅ Vollständig |
| MagicService | 2.6KB | ⚠️ Unvollständig |
| SaveService | 10.7KB | ✅ Vollständig |
| SaveMigrator | 6.9KB | ✅ Vollständig |
| DayCycleService | 1.2KB | ✅ Vollständig |
| ResearchService | 2.6KB | ✅ Vollständig |
| CharacterGenerationPools | 21.4KB | ⚠️ Unvollständig |
| SaveStateFactory | 17.3KB | ✅ Vollständig |
| MainMenuController | 6.4KB | ✅ Vollständig |
| SceneRouter | 1.3KB | ✅ Vollständig |
| FeedbackService | 3.2KB | ✅ Vollständig |
| LocaleCatalog | 2.7KB | ✅ Vollständig |
| SettingsStorage | 2.4KB | ✅ Vollständig |

**Gesamt: ~32 Services, ~300KB Code**

### 8.2 DataRegistry-Übersicht

| Registry | Anzahl | Status |
|----------|--------|--------|
| Characters | 10 | ⚠️ Unvollständig |
| Jobs | 18 | ✅ Vollständig |
| Items | ~250 | ✅ Vollständig |
| Facilities | 30 | ✅ Vollständig |
| Missions | ~10 | ⚠️ Unvollständig |
| Enemies | ~15 | ✅ Vollständig |
| Milestones | 10 | ✅ Vollständig |
| Skills | 12 | ✅ Vollständig |
| Talents | 47 | ⚠️ Unvollständig |
| Spells | 28 | ✅ Vollständig |
| TrainingActions | 100+ | ✅ Vollständig |

**Gesamt: ~520 Einträge in DataRegistry**

### 8.3 SaveState-Struktur

```
SaveState
├── Calendar (Tag, Monat, Jahr, Zeit)
├── Economy (Gold, Ressourcen, Mana, Spirit)
├── Characters (10+ Charaktere)
│   ├── State (Hp, Energy, Fatigue, Morale, etc.)
│   ├── Equipment (15 Slots)
│   ├── Talents (List<string>)
│   ├── Mature (Pleasure, Lubrication, etc.)
│   └── Milk (Capacity, Quality, etc.)
├── Ranch (Facilities, Jobs)
├── Inventory (Items, Gold)
├── Missions (Active, Completed)
├── Schedule (Tagesablauf)
├── Flags (FEHLEND)
├── Research (Unlocked Skills)
├── Pet (Adopted Pets)
└── Settings (Audio, Haptics, Theme)
```

---

## 9. FAZIT

### 9.1 Was GUT ist

- ✅ **Kern-Loop funktioniert:** Tag-Beendigung, Nachtaktion, Milchproduktion
- ✅ **UI ist strukturiert:** 20+ Screens, konsistentes Styling
- ✅ **Economy-System funktioniert:** Shop, Economy, Milch-Preise
- ✅ **Kampf-Mechanik ist vorhanden:** CombatServices mit Round-Resolution
- ✅ **Service-Architektur ist sauber:** 32 Services, 300KB Code
- ✅ **Save/Load funktioniert:** JSON-basiert, Migration vorhanden
- ✅ **Portrait-System ist gut:** Layered Rendering mit Caching

### 9.2 Was FEHLT (kritisch)

- ❌ **Flag-System (766 Flags):** Ohne Flags keine Events, keine Quests, keine Progression
- ❌ **Talente (137/184):** Ohne Talente flache Charakter-Entwicklung
- ❌ **Missionen (50/60 fehlen):** Ohne Missionen wenig Abwechslung
- ❌ **Equipment-UI:** Kein Screen zum An- und Ausziehen
- ❌ **Clothing-Layer für Portraits:** Portraits unvollständig
- ❌ **Spell-UI:** Kein Screen zum Auswählen von Spells
- ❌ **Mission-Prep-UI:** Kein strategisches Setup vor Mission

### 9.3 Was OPTIONAL ist

- 🟡 **Background-Images:** 50 Bilder für Ranch, Town, Missions
- 🟡 **Equipment-Icons:** 385 Icons für Items, Equipment, Talente
- 🟡 **Portrait-Layer:** 650 Layer-Images für vollständige Portraits
- 🟢 **UI-Elemente:** 200 UI-Elemente (Buttons, Panels, Icons)

### 9.4 Spielbarkeit

| Kriterium | Bewertung | Kommentar |
|-----------|-----------|-----------|
| Kern-Loop | ⭐⭐⭐⭐☆ | Basis-Loop funktioniert |
| Abwechslung | ⭐⭐☆☆☆ | Zu wenige Missionen/Events |
| Charakter-Entwicklung | ⭐⭐⭐☆☆ | Talente fehlen zu stark |
| Progression | ⭐⭐☆☆☆ | Ohne Flags kaum Progression |
| Kampf | ⭐⭐⭐☆☆ | Mechanik da, aber wenig UI |
| Wirtschaft | ⭐⭐⭐⭐☆ | Shop, Economy, Milch-System |
| Exploration | ⭐⭐☆☆☆ | Zu wenig Inhalte |
| **GESAMT** | **⭐⭐⭐☆☆** | **SPIELBAR ABER UNVOLLSTÄNDIG** |

---

## 10. NÄCHSTE SCHRITTE

### 10.1 Priorisierte Aufgaben

1. **Flag-System implementieren** (766 Flags) — KRITISCH
2. **Talente erweitern** (137 Talente) — KRITISCH
3. **Missionen erweitern** (50 Missions) — HOCH
4. **Equipment-UI** — HOCH
5. **Clothing-Layer für Portraits** — HOCH
6. **Spell-UI** — MITTEL
7. **Mission-Prep-UI** — MITTEL
8. **Background-Images** — MITTEL
9. **Equipment-Icons** — MITTEL
10. **Portrait-Layer** — MITTEL

### 10.2 Grafik-Bedarf zusammengefasst

| Kategorie | Anzahl | Priorität |
|-----------|--------|-----------|
| Portrait-Layer | ~650 | 🔴 KRITISCH |
| Equipment-Icons | ~385 | 🟡 WICHTIG |
| Backgrounds | ~50 | 🟡 WICHTIG |
| UI-Elemente | ~200 | 🟢 OPTIONAL |
| **GESAMT** | **~1285** | |

---

## 11. ANHANG: CSV-ÜBERSICHT ORIGINAL

### 11.1 Haupt-CSVs

| CSV | Zeilen | Datenzeilen | Beschreibung |
|-----|--------|-------------|--------------|
| Str.csv | 4825 | 4126 | Charakterdaten (Namen, Körper, Aussehen) |
| Item.csv | 499 | 273 | Items (Potions, Equipment, Buildings) |
| Equip.csv | 303 | 162 | Equipment-Slots |
| Tequip.csv | 94 | 64 | Equipment-Typen |
| Talent.csv | 260 | 184 | Talente |
| Train.csv | 179 | 111 | Training-Aktionen |
| Flag.csv | 787 | 537 | Quest/Event-Flags |
| Tflag.csv | 190 | 104 | Temporary Flags |
| Cflag.csv | 199 | 125 | Character-Flags |
| base.csv | 71 | 36 | Basis-Parameter (HP, SP, EP, MP) |
| Abl.csv | 127 | 76 | Sensitivitäts-Parameter |
| Palam.csv | 46 | 33 | Pleasure-Parameter |
| Mark.csv | 124 | 81 | Marks (Körpermerkmale) |
| exp.csv | 365 | 153 | Experience-Records |
| Juel.csv | 29 | 18 | Magic-Spells |
| Nowex.csv | 16 | 10 | Orgasm-Records |
| ex.csv | 24 | 15 | Extra-Records |
| GameBase.csv | 8 | 7 | Spiel-Basisdaten |
| Global.csv | 36 | 19 | Globale Variablen |
| Globals.csv | 35 | 9 | Globale Konstanten |
| Savestr.csv | 50 | 27 | Save-String-Konstanten |
| Money.csv | 38 | 25 | Geld-Variablen |
| Time.csv | 9 | 3 | Zeit-Variablen |
| Day.csv | 12 | 5 | Tages-Variablen |
| VariableSize.csv | 132 | 101 | Dynamische Größen |
| Stain.csv | 24 | 13 | Stain-Parameter |
| _replace.csv | 3 | 2 | Replace-Mapping |

**GESAMT: ~31 CSVs, ~8000+ Datenzeilen**

### 11.2 Kategorien

| Kategorie | CSVs | Datenzeilen |
|-----------|------|-------------|
| Charakter | Str.csv, Cstr.csv, base.csv | 4289 |
| Items | Item.csv | 273 |
| Equipment | Equip.csv, Tequip.csv | 226 |
| Talente | Talent.csv | 184 |
| Training | Train.csv | 111 |
| Flags | Flag.csv, Tflag.csv, Cflag.csv | 766 |
| Parameter | Abl.csv, Palam.csv, Mark.csv, exp.csv, Nowex.csv, ex.csv | 368 |
| Magic | Juel.csv | 18 |
| System | GameBase.csv, Global.csv, Globals.csv, Savestr.csv, Money.csv, Time.csv, Day.csv, VariableSize.csv, Stain.csv | 209 |
