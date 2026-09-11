# Resident interaction acceptance

Work branch: `feature/world-stations-and-interiors-20260911`, PR #15. The resident source continuation was integrated at `b27078d2ad62cee63535a645a7d2716ab960900d`. Its exact-preimage patch helpers removed themselves; no main change, force push or engine/framework/schema upgrade was used.

## Scope under test

- Ordinary free conversation, separate encouragement, meals, an explicit ordinary-gift list, recovery, practical advice and ranch/craft/combat/magic practice use the existing simulation services.
- Resident UI separates conversation, care, practice, company and work. It must preserve visible Back, stable target/context, declared stamina costs and readable translated outcomes.
- A resident must wait while their interaction is open and resume afterward. Closed/remote/stale/reentrant commands must not consume items, stamina or daily receipts.
- Practical training must reject invalid or capped focuses before changing energy, morale, fatigue or the global practice counter. Meals/recovery must respect actual energy capacity without reducing pre-existing boosts.
- A rendered synthetic-fixture journey clicks the actual conversation/practice/care/gift controls, switches the open panel to German, then saves/loads, sleeps through the actual house action and checks the next day. Proximity and starting stats are staged; item prerequisites are bought through canonical shop transactions, not claimed as physical shopping clicks.
- Temporary isolated slots 99 and 3 are test fixtures, never personal profiles. The resident scenario refuses occupied slot 99. Existing day, combat, station, language-picker and shared-evening scenarios remain.

## Verification status

The local review subset passes `validate_locales.py` with **309 matching English/German UI keys** and the **seven catalog unit tests**. `git diff --check` passes. A local attempt at the entire launcher suite timed out; no full-suite local pass is claimed. No local Godot or .NET compiler was available. CI import/build/smoke/rendered checks are pending and must be recorded from completed exact-head runs before claiming runtime acceptance.

## Limits

This is a non-explicit interaction slice, not all original commands or a complete balancing playthrough. The old explicit catalog and internal legacy screens remain untouched; NSFW_CONTENT_HANDOFF.md lists the omissions and integration locations without explicit scenes or instructions. Raw internal legacy service calls are not universally guarded by the new world command receipts. The existing two-practice-session daily budget is retained. Character-specific arcs, full animations, all gifts/preferences, family planning, all NPC routes and every language remain open.
