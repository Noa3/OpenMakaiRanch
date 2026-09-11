# ASTRA Handoff

Checkpoint **2026-09-11**, verified code head **`4ffd97c84929ed307fa623dde709760dcec28762`**: **1,889 passing smoke assertions, 526/526 rendered checks, 55 Python tests and 64 PNGs**. All three CI workflows succeeded. **OPTIMIZATION_VALIDATION.md** records exact-head evidence, source/merge receipts, inspected images, archive hashes, the prior failed timeout and fixture limits. Documentation-only commits may follow. Preserve historical resident/design/localization validation documents as evidence for their own revisions.

## Branch and contracts

Continue **`feature/world-stations-and-interiors-20260911`**, **PR #15**, repository Noa3/OpenMakaiRanch. Main was checked at merged PR #13 `3209f4c`; PR #14's old test-only baseline is included/repaired and should not be merged separately. Read live Git before editing; preserve concurrent changes, no force push or automatic merge.

Keep **C#12, SDK10.0.401, net8.0, Godot4.7.2 Mono and schema16**. Fresh games are expected; current-version saves must work and personal files must not be deleted. Original reference content remains read-only. GameRoot/services own simulation, economy and time. No adult-specific asset, source relabeling or production identity approval was added.

User wants efficient low-day play respected, not blocked with arbitrary date gates, hidden penalties or new costs. Read **OPTIMIZATION_AND_PACING.md**. More authored decisions/content and optional challenges are proposed; they are not implemented just because a progress calculator exists. User also requires translation-aware UI and later ten-target localization excluding Russian. Use LocaleCatalog, full keyed templates, canonical IDs and adaptable layouts. Current en/de/ja availability is not full translation.

## Latest completed work

**Work-station planning:** Overview, Team and Equipment stay scoped to the physical workplace. The guided introduction retains direct Build/Assign. Canonical offers show one-time price, wallet, local level-scaled upkeep and whole-ranch upkeep after research. Zero-level ghost upkeep is repaired. Actual purchases compare the displayed quote; assignments compare the previous job. World context/proximity plus root session/day/phase/pause/combat/settlement/busy guards reject stale, remote and reentrant commands. Production is paid only during normal settlement. Read STATION_PLANNING.md.

**Transparent output:** Team now shows each resident's current-condition gross units and gold, with a breakdown tooltip. Preview and committed work share the original formula, order and rounding. Effective CombatSkill applies to Adventure, effective RanchSkill to other productive jobs as before; this is not a hidden role-skill rebalance. Fatigue>=70 can cause automatic Rest before night recovery and production, and the UI warns about it. Later care, night plans, upkeep and events make this different from a final daily balance.

**Inventory conservation:** failed equipment replacements no longer return/duplicate the still-equipped old item. Stock/return capacity are validated first; real owned swaps still exchange exactly one item. Bonus inspection of missing equipment maps is read-only. Other inventory paths/extreme arithmetic remain separate audit work.

**Completion and pacing:** the existing WinConditionService exposes read-only catalog-based counts, excluding unknown/duplicate substitutes. No new date gate or thresholds. Synthetic service tests meet goals on day2, record the next morning and continue three more days without reset, repeat event or automatic NG+. The separate seven-day seeded economy fixture pays construction/meals and stays solvent, but normal days lose8G after meals; it is not an optimal or indefinitely profitable strategy and not a whole-game playthrough.

**Important exclusion:** a blocked central station-file write meant the proposed new ambitions page and revised victory routing were NOT integrated. Central WorldStationPanel.cs remains unchanged from baseline. No alternate path installed the blocked change. The existing house/report route may overwrite victory presentation; return from a locked screen still needs location-preserving review. Service-level continuation tests do not certify the existing physical Continue Ranching control.

**Translation:** 31 station keys followed the resident slice, then four forecast keys bring matching English/German UI catalogs to **344**. Kitchen project wording no longer promises a production multiplier absent from current rules. Japanese/new-language fallback and older untranslated story/result messages remain. No new locales were enabled.

## Earlier work retained

Resident Overview/Care/Practice/Company/Work pages, separate gift list and addressed-NPC waiting remain. Free chat is read-only; encouragement/meals/ordinary gifts/recovery/mentoring reuse existing services. Practical ranch/craft/combat/magic lessons have the original two-session ranch budget and one focus per resident/day through the new route. Invalid/capped training is rejected before effects. World/root command guards and bounded per-resident last-day receipts survive save/load and normal day rollover. RESIDENT_INTERACTIONS.md and its validation record exact limits; raw legacy APIs are not universally protected by the new interface.

Places has four optional starting projects, not a finished campaign. Existing voluntary shared nights, rest/bath separation and bounded saved receipts remain; no pregnancy/family state was added and production identity gates remain. NSFW_CONTENT_HANDOFF.md is a neutral authoring/location handoff, not explicit scene scripts.

Eight metre-scale building plots protect roofs, doors and the gate; visual grades0-3 keep shell/collision size stable. Imported/fallback tree bounds respect reservations. These are not all-route/free-placement or annex certifications. Held-input preservation, physical doorway movement, starting-fund tutorial construction, local planning and opening/bedroom-return repairs remain tested.

The title owns a separate bounded diorama viewport/camera. Actual native language-picker input preserves window/settings/state, and language-only persistence avoids reapplying graphics/audio/input. Prior combat, day ledger/growth/planning/reentrancy, purchase and save journeys remain connected. Read prior validation files without recycling their counts as new evidence.

## Current verification

Head4ffd97c: Build #660 **34649261398**, Godot #652 **34649261392**, UI #90 **34649261422**: all success. Tested merge **`7436f2222b1d8142b4b59399fe398062bbc8384c`** is recorded in the source receipt. **1,889 SMOKE OK / zero FAIL / one PASS**, **526/526 UI checks / zero UI runtime errors /64 captures**, **55 Python tests**. The full local Python retry took22.048 seconds; no local Godot/.NET run is claimed. Current source bytes and downloaded artifact hashes were checked; German forecast/equipment/purchase images were inspected.

The prior station UI run at0356785 really failed at its240-second process timeout, not an unknown result or a pass. The same existing workflow now allows a bounded360-second rendered process, still15minutes overall. It retains all assertions/error rejection and restores the intended test viewport after LoadSlot. No new workflow or write-capable helper was added this turn.

Five invalid-save smoke errors are intentional. Import retains EditorSettings and software rendering its VSync diagnostics; existing build warnings remain. Do not call this warning-free or universal lifecycle certification. UI proximity/resources/strong stats and early completion are synthetic. The economic seven-day fixture uses real starting resources/transactions but skips the world/tutorial and does not optimize the full action space.

## Next priorities

Review reliable completion-screen presentation and same-ranch continuation; compare casual, efficient and specialist multi-day policies before changing rates. Design useful marginal benefits for equipment levels rather than escalating upkeep for checklist-only progression. Add a concrete restoration chapter/resident payoff and optional mastery experiences, not forced waiting. Continue stepwise localized creation, less dense Team/help text, dedicated service/results, resident recovery/equipment/stories and NPC/camera routes. Organic town/river, final assets/building stages, all scripts/plurals/RTL, actual exports/devices, Forward+ performance and original-engine parity remain open.

## Safe commands

```bash
python Tools/Godot/validate_locales.py
python -m unittest discover -s Tools/Godot -p "test_*.py"
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/ui_acceptance.py --rendered --timeout 360
python Tools/Godot/launch.py --mode runtime --isolated
```

Only isolated test profiles: disposable slots99/3 must never target personal saves. Archives are evidence/review subsets, not standalone game exports. Live Git and exact-head receipts supersede older numerical KANBAN/WORK_LOG summaries.
