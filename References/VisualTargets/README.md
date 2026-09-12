# OpenMakaiRanch — visual target atlas

## Status, not a promise of finished screenshots

**49 image briefs, 25 UI layout families, 35 existing legacy routes covered. Zero generated or approved target screenshots.** Image generation is unavailable in the authoring session; no paid provider was called. The SVGs are editable layout studies and the included historical captures are baselines, neither is AI-generated final-game artwork. The generator/provider must be connected before target generation. Do not ask a build agent to match the primitive study models as final character/landscape quality.

Own branch: `feature/visual-target-atlas-20260912`, from graphics branch `feature/makai-presence-and-worldstyle-20260911` at `71fb3a170b6f4baaf1cc8a1a45cdbd1dceab935f`. No other agent's logic/world branches, runtime source, existing CI, engine, save schema or source game are changed. This folder sits **outside `OpenMakaiRanchGame/`**, as requested. Canonical implementation docs still live in the game's docs folder; this is the art-reference pack, not a second gameplay roadmap.

## Start here

Open `ui/UI_ATLAS.svg` for all 25 proposed menu families. Eight full-size key layouts are versioned separately: U01, U02, U03, U04, U05, U16, U17, U20. `plans/ranch-layout.svg` is a continuity diagram using the eight source plot centers and sizes. It deliberately labels its unrotated diagram simplification; use source yaw/envelopes for any construction work. All 25 full-size SVGs can be regenerated locally, and are included in the downloadable bundle.

Read `style-lock.json`, `manifest.json`, `UI_ROUTE_MAP.md`, `RESEARCH.md` and **`ASTRA_PROMPT.md`**. Brief groups: W01–W03 progression; W04–W08 world/weather; B01–B04 building sheets; I01–I05 interiors; C01–C03 character/rig/performance; A01–A04 interactions; U01–U25 UI.

The same ranch should remain recognizable across progress images. W01/W02/W03 are proposed visual fixtures, not Day 1/30/90 gates, verified construction sequences or new prices. Eight existing plot footprints, doors and gate clearance remain authoritative. Recurring characters and buildings need approved multi-view sheets before reuse. REF_A–REF_D are proposed original adult reference designs, not production identity approvals.

## Tools from the repository root

```bash
python -m unittest discover -s Tools/VisualTargets -p 'test_*.py' -v
python Tools/VisualTargets/atlas.py validate
python Tools/VisualTargets/atlas.py status
python Tools/VisualTargets/atlas.py prompt W01
python Tools/VisualTargets/draw_drafts.py --write
```

A future image provider creates a real PNG from the exported prompt and attached references. Import that actual file as **candidate**, not approved art:

```bash
python Tools/VisualTargets/atlas.py import-target W01 /actual/output.png --provider ACTUAL_PROVIDER --model ACTUAL_MODEL --provenance "Actual generation receipt and input-image hashes"
# Only after the user's recorded decision, not autonomous self-approval:
python Tools/VisualTargets/atlas.py approve W01 --reviewer Noa3 --decision-reference "Actual approval message or review ID"
```

The structural PNG checks verify dimensions and chunk CRCs, not image semantics. Open the image and review it; a valid file is not evidence of adequate artwork. The registry refuses changing approved images, stale hashes and path traversal. To revise a locked target, make a reviewed new version rather than silently replacing it.

## Historical baselines

`baselines/receipts.json` pins seven existing game/lab captures by archive and file hashes. The download bundle contains those PNGs. They are **historical**, from implementation `5e7d446`, CI merge checkout `413d7b5`; no fresh game run is claimed. Bare Git checkout can import the same captures from the exact downloaded evidence ZIPs:

```bash
python Tools/VisualTargets/atlas.py import-baselines /actual/pastoral-world-ui-evidence.zip
python Tools/VisualTargets/atlas.py import-baselines /actual/pastoral-forward-evidence.zip
```

Artifacts: lookdev run `34658962938` / artifact `10287195826`; UI run `34658962948` / artifact `10287015864`. Artifact expiry means a new source capture is needed, not permission to attach unrelated imagery. The tool matches archive hashes, not its filename, and never follows archive paths outside the pack.

## Completion boundary

This pass builds the brief/registry/layout/review tooling. It does **not** deliver generated world/character art, a working in-game menu redesign, animated character performance, authored buildings or AAA-quality playable terrain. The reference review must precede 3D construction. Existing screenshots are evidence of their old code only. New runtime changes require separate builds, screenshots, route checks and representative hardware profiling.
