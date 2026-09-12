# ASTRA Handoff

## Current direction and working snapshot — 2026-09-12

The user asked to persist the agreed living-world direction and correct older planning. This is a **documentation-only continuation**, based on the uploaded `astar` head **`1a65868b73e1af48277d8ac9ac338e3edcc5ce34`**. No game code, asset, runtime data, scene, workflow or personal save is changed by this planning update. It is not a new build, traversal or visual acceptance result.

Read in order:

1. [LIVING_WORLD_PLAN.md](LIVING_WORLD_PLAN.md): current requirements, proposed stages, architecture, unresolved decisions and evidence rules.
2. [KANBAN.md](KANBAN.md): sole status board, stable LW-01..LW-20 tasks, dependencies, completion evidence and retained non-world work.
3. [DECISIONS.md](DECISIONS.md): D-013..D-017 supersede conflicting old layout/population priorities without discarding the existing simulation or C# 12.
4. [KNOWN_ISSUES.md](KNOWN_ISSUES.md), [habitation facts](art/RANCH_HABITATION_FACTS.md), [asset recovery](art/RANCH_ASSET_RECOVERY.md) and [coastal plan](execplan/COASTAL_REGION.md): concrete upload blockers and prior evidence.

The earlier feature-branch handoff is preserved verbatim in [the historical snapshot](archive/20260912-before-living-world/ASTRA_HANDOFF.md). Its instruction to continue PR #15 is historical for that work, not a reason to ignore the user's newer local `astar` upload. Verify live Git and current user assignment before any write; preserve concurrent/dirty work, no force push or automatic merge. Historical local 'no commit/push' notes described those sessions, not a grant or denial for future sessions; follow the current explicit authorization and stage only its scope.

## What is now the durable target

- A larger coherent ranch/valley/coastal town region with plausible water, terrain, routes, growth reserves and permanent green/shore space. Important daily paths stay compact; an open-world experience does not require one monolithic scene or a new streaming engine.
- Functions in usable buildings/rooms/objects/contacts, not one menu-station facade per service. Kitchen and ranch office belong in the main house. Own houses and important public premises are enterable; ordinary private homes need not all be accessible.
- Consistent metre-scale figures, fences, furniture and architecture, actual sit/lie use where supported, and explicit ranch housing/upgrade policy. Add rooms/wings rather than inflating the entire house.
- A town that eventually suggests roughly 300 inhabitants. Important NPC continuity is required; 300 persistent citizens or full agents are expressly **not** required. Background activity derives from place, time, weather and stage.
- Visible civic growth as the ranch supports the town: a modest civic/supply house, market and later quarters. Actual tax/mana pool, official/landlord role, thresholds and accounting remain to be audited/decided. No invented double payments, punitive costs or minimum-day gates.
- Local Astra primarily models, textures, furnishes, animates neutral uses, integrates and visually checks real assets. General economy/trait/dialogue/system rewrites are not the art task.
- Remove genuinely unused/replaced scenes only after dependency, dynamic-path, UID, tool/test/export and replacement checks; keep sources and historical evidence.

## First actions

Establish the actual uploaded runtime baseline (LW-01). In parallel plan final geography/growth envelopes and measure scale (LW-02/LW-04), and audit civic resource/source contracts (LW-09). Integrate the recovered house through a function-to-room mapping (LW-03/LW-05), rather than making another show prototype. Decide the household/capacity issue explicitly (LW-06). Resolve the rejected stream/path crossing and walk it with a companion (LW-08).

The first complete delivery is **main house -> useful ranch rooms -> connection -> civic/supply house -> modest market courtyard**, with **one** visible before/construction/after civic improvement and real use. Local art can prepare variants while economic decisions remain open, but a test trigger is not ordinary-play financial integration. Complete that slice before mass-producing later quarters.

## Upload facts and blockers

The new 20 x 16 m house and 12 x 12 m wing are still a furnished authoring prototype, not the ordinary ranch. `organic_layout.json` retains a 7.2 x 6.4 m house footprint plus separate kitchen/office plots. Replan complete footprints and approaches; do not squeeze the asset into the old plot. Fifteen furniture GLBs and sources exist, but the recorded furniture collider count is zero and articulated furniture use is not wired. One neutral stand-in was measured at its 1.62 m target; that is not an all-body clearance test.

The habitation audit found uncapped ranch recruitment, no actual bed/room/seat allocation and a player/roster-`anon` identity overlap. Seven sample beds are not a capacity policy. The town atmosphere requirement must not accidentally create 300 ranch residents or a new household simulation for generic passersby.

The coastal candidate remains rejected: reported path ground deviation 0.1394483787870735 exceeds 0.12, with a stream/valley-lane crossing unresolved. Its files and opt-in controller are not a walked or production-accepted region. Keep the threshold and rejection evidence. The prior document's geometry contracts remain available through the coastal plan.

## Verification provenance — do not reuse old counts

The prior handoff reports **1,889 smoke assertions, 526/526 rendered checks, 55 Python tests and 64 PNGs** for **`4ffd97c84929ed307fa623dde709760dcec28762`**, with exact workflows and receipts in OPTIMIZATION_VALIDATION.md. That is an earlier code checkpoint, not an executed test of the later asset-upload head `1a65868`. This documentation update ran no Godot/.NET/gameplay/asset acceptance tests and does not change historical result status.

Asset recovery separately records a local Forward+ prototype capture, 15-model kit verification and VisualTargets tests, with their narrower limitations. The rejected coastal geometry test and previously recorded UI failures are not erased by those results. Establish and report an actual combined baseline before saying the upload is green.

## Contracts and unrelated work retained

Keep **C# 12, pinned SDK 10.0.401, net8.0, Godot 4.7.2 Mono and current schema 16**. Fresh games are expected; current-version save/load and personal-file protection remain mandatory. Any necessary new persistent housing/development fields need a small reviewed addition in the existing architecture, not another save system or a broad migration project. Original source is read-only; existing identity/design/eligibility gates remain unchanged.

GameRoot/services retain economic, calendar and progression authority. Preserve the verified scope of previous day ledger/reentrancy repairs, independent bath/rest bonuses, efficient play, local quote/receipt guards, resident interactions, merchant stock and equipment conservation. Existing source/code audit constraints and localization through LocaleCatalog/keyed templates remain. No new language is enabled here.

Keep ENDING-001 (physical victory/Continue Ranching and same-location return), broader settlement/inventory/data audits, useful equipment progression, creation/localization/accessibility, original-engine parity and tooling diagnostics in the retained backlog. This world plan does not claim them solved or authorize bypassing a previously blocked write by another path. The complete prior issue details are preserved in the historical snapshot.

## Safe execution and evidence

```bash
python Tools/Godot/validate_locales.py
python -m unittest discover -s Tools/Godot -p "test_*.py"
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python Tools/Godot/launch.py --mode smoke
python Tools/Godot/ui_acceptance.py --rendered --timeout 360
python Tools/Godot/launch.py --mode runtime --isolated
```

These are existing documented commands to run when executing the tasks, **not commands claimed executed for this documentation commit**. Discover configured local executables; do not assume another machine's path. Disposable test slots99/3 must never target personal saves. Log exact commit/source, real walking versus staged positions, renderer/hardware, limits and evidence paths. Update KANBAN status, the handoff next action and current issues together. Successful automated tests do not confer human artistic approval.
