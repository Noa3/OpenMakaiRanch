# Visual atlas delivery checks

Checkpoint: 2026-09-12. Branch `feature/visual-target-atlas-20260912`, based on graphics commit `71fb3a170b6f4baaf1cc8a1a45cdbd1dceab935f`.

## Executed locally

- **28 Python unit tests passed** with `python -m unittest discover -s Tools/VisualTargets -p 'test_*.py' -v`.
- `atlas.py validate` passed. Registry status: **49 awaiting generation, zero candidates, zero approved targets**.
- **27 SVG files** parsed as XML: 25 full-size menu layout drafts, one overview atlas and one plot diagram. Regenerating them with `draw_drafts.py --write` produced identical file hashes.
- Seven historical baseline PNGs and their two original evidence archives matched their SHA-256 receipts.
- Full-size workplace/options drafts and the 25-layout overview were opened for visual review. These are schematic UI layouts, not final art or runtime usability evidence.

The registry checks route coverage, file integrity, approval metadata, safe paths and comparison-context equality. It cannot prove that a claimed reviewer actually approved an image, that a PNG is aesthetically suitable, or that a supplied context describes a real game capture. Human/art review and actual runtime evidence remain necessary.

## Versioned versus downloadable content

Git contains the brief registry, style lock, research, Astra instructions, tools, eight key full-size menu SVGs, the 25-family overview, the plot diagram and historical baseline receipts. The full 25 SVGs can be regenerated from the same source. The downloadable reference bundle additionally contains all 25 full-size drafts, derived previews, expanded prompts and the seven historical PNGs. The PNGs have not been uploaded to Git through a binary-file action; use the exact-artifact importer or copy the bundle into the repository root. Reference files remain outside `OpenMakaiRanchGame/` apart from its documentation pointer.

## Not executed or delivered

No image-generation provider was connected or called, no credits were spent, and no final-world/character target images were generated. No Dream Loop judge or completed target-matching loop was run. No new Godot runtime, C# build, gameplay smoke, menu interaction, animation or performance test is claimed for this reference-only change. Historical images retain their old source/CI commit identifiers; they are not new screenshots.

The menu route map describes 35 existing legacy routes plus explicitly proposed presentation entry points. It is not a claim that the 25 redesigned menus are implemented. Architectural dimensions are source constraints; the plot diagram is unrotated for labeling and is not a construction/collision map. Stage briefs are illustrative facility states, not new day gates, reward schedules or economy rules.

Next blocking dependency: a real image-generation tool or user-supplied target images, followed by review of the first world and character targets. An approved target, working scene and measured performance are three separate gates.
