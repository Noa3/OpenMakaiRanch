# Gap Analysis: OpenMakaiRanch vs. Original eraMakaiRanch

## Implemented Features
- **Bond Events** — 27 events across all 9 characters with interactive dialog buttons
- **Training System** — Ranch/Craft/Combat training per character (10 energy cost)
- **Milk Economy** — Daily production and auto-shipment with revenue tracking
- **Combat/Adventure** — Mission system with round-based combat, party selection, enemy variety
- **Equipment** — 10 equipment items, 5 slots (weapon/armor/accessory/head/feet), stat bonuses
- **Facilities** — 6 upgradable facilities with stockpile costs and upkeep
- **Research Skills** — 7 skills with gameplay effects (dairy_science, culinary_arts, etc.)
- **Character Stats** — Ranch/Craft/Combat skills, HP, Energy, Fatigue, Morale, Bond
- **Items** — 22 consumables with effects, 10 equipment items, shop buy/sell
- **Pets** — 3 adoptable pets with daily benefits
- **Save/Load** — Schema migration, 3 save slots
- **Victory/NG+** — Win condition tracking, victory screen, New Game+ with gold bonus
- **Talents** — 45 talent definitions with gameplay effects (stat bonuses, growth multipliers, job output modifiers)
- **Responsive Layout** — Auto-switch between sidebar and compact navigation at 900px viewport
- **Item Usage on Roster** — Use consumable items directly from character cards

## Missing / Incomplete Features

### 1. Interactive Bond Event Narrative Screen
Bond events show dialog buttons but lack:
- Full narrative text display (dialog lines with character voice)
- Choice-based branching (multiple response options)
- Story progression tracking per character

### 2. Enhanced Training Interactions
- No visual feedback for training results
- No choice of training intensity (light/medium/heavy)
- No training-related bond events

### 3. Character Customization
- No way to rename characters
- No portrait customization
- No cosmetic item effects

### 4. Crafting / Item Creation
- No recipe system for crafting items from materials
- No workshop upgrades that unlock new recipes

### 5. Breeding / Procreation
- No child/offspring system (present in original)
- No inheritance mechanics

### 6. Complete Content Parity
Original had ~50+ items (we have 32)
Original had ~15 missions (we have 8)
Original had ~50 bond events (we have 27)
Original had more facility types

### 7. UI Polish
- No character sorting/filtering
- No detailed tooltips with stat previews
- No animation/transition effects
- No music or sound effects (placeholder)

### 8. Town NPCs
- No named NPC interactions beyond shop
- No special town events

## Priority Matrix

| Priority | Feature | Effort | Impact |
|----------|---------|--------|--------|
| P0 | Bond event narrative screen | Medium | High |
| P1 | Character renaming | Low | Medium |
| P1 | Training feedback | Low | Medium |
| P2 | More content parity | High | High |
| P2 | Sound/music Placeholder -> Real | Medium | High |
| P3 | Crafting system | High | Medium |
| P3 | Breeding system | Very High | High |
| P4 | Town NPCs | Medium | Low |
