# Valley bridge: verified offline geometry slice

Parent follow-up: the receipt below describes the earlier worker snapshot. After resolving the house/gate/stream-bank conflict, all eleven modules were re-exported against layout SHA-256 `3cb6fa1be0408f148666281f54938e98b8cbe1a11600981b83a25567f3c494bf`. Nineteen WorldDesign tests and the full isolated smoke pass. Fourteen new engine checks verify deck queries, provenance rejection and actual unit-scale bridge import with seven collision shapes. Current logs/receipt/layout are in `parent-resync/`; see `../../execplan/GROWING_REGION.md`. Physical player/companion crossing and baked-navigation acceptance remain open.

## Explicit plan implemented

Keep the original stream, sea datum, area origins/yaws, walkable polygons, scene seam, coast and meadow bridge. Between existing regional controls `[61,4.4,165]` and `[70,4,185]`, bend dry approaches to one perpendicular, level 12 m bridge centered at X65.15140845070422/Z174.22535211267606.

- Regional deck endpoints: `[68.53517832764942,4.22,169.27054622143487]` and `[61.76763857375903,4.22,179.18015800391726]`.
- Deck width 3.8 m; rail inner clearance 3.4 m for the unchanged 3.2 m lane. Deck thickness 0.3 m, guards 1.3 m high; four bank supports. Deck collision is separate from the unfilled carved stream.
- New module: `res://assets/3d/coastal_region/ranch_valley_bridge.glb`, ranch-local XYZ; instantiate under the existing ranch root without additional offsets. Seven mesh nodes: `valley_footbridge_deck-col`, two `valley_rail_*-col`, four `valley_support_*-col`.
- `region.connection.bridge` contains geometry and segment metadata. Both stream-bank collider openings follow its oriented footprint plus 0.35 m margin. The path overlay skips only the explicit bridge segment; approaches remain on actual triangulated ground.
- Heightfield module manifest now includes this module, retaining all ten original modules. No C# or scene files changed.

## Preservation

Before replacing generated candidates, archived 28 files (including blend, GLBs, receipts, heightfields and layout) in `.dream-loop/coastal-pre-bridge-20260912-134035.zip`; every archived member verified against its SHA-256 manifest. Archive SHA-256: `274baec50b34e68c8f3b9e89d2ce4341db90e8fde9e19e9271c6dd0cc415e39e`.

Read-back comparison confirmed every pre-existing region section except `connection` unchanged, including polygons and seam. Original candidates remain in the archive.

## Executed evidence

- Headless Blender 5.2.1 LTS export exited 0; eleven `EXPORT_OK` modules and `COASTAL_BLOCKOUT_OK 11` in `assets/3d/coastal_region/blender_generation.log`.
- Reopened `assets/3d/coastal_region/coastal_region.blend` in a separate headless process: `BLEND_REOPEN_OK 7 42` for the bridge collection (seven meshes, 42 source quad faces).
- New GLB: 9,172 bytes, seven meshes, 84 triangles. SHA-256 `e55cd08dc0311e154a6b2b7223d18ec7664ffe7e19bdec8cc5478a82945723d8`.
- `python -m unittest discover -s Tools/WorldDesign -v`: **19/19 pass**, including **9/9 coastal geometry tests**. Real GLB hashes/counts, full-width path/terrain contact, exported bridge coverage/guards/gaps, stream bed, beach, plot envelopes and shared seam checked.
- Five lateral lanes sampled at intervals <=0.25 m: 3,830 samples; maximum designed-surface height error **0.04456993657628683 m** against unchanged **0.12 m** limit; maximum longitudinal grade **0.17457263338922854** against unchanged **0.25** limit. Bridge deck sampled separately from carved ground. Pre-export bank rejection retains the original 1 m bank buffer, applied to actual full-width sample positions instead of an isotropic centerline radius.
- Layout, heightfield and generation receipt source SHA-256: `1d1a76fd1a30b6f6f2f6f4e7794ba97377c7a4920f78597324295e3d4cc280be`.
- Shared regional player-center seam remains `[70,4.8,185]`.

## Remaining integration limits

Offline geometry is green, not runtime acceptance. Parent must import the new module through Godot, verify imported deck/guard collision, ensure navigation/height queries choose the separate deck above the underlying stream, and perform real player/companion approach–crossing–seam traversal. No runtime tests, rendered art approval, save/load or production activation claimed. Later parent ranch/town layout edits change the whole-source hash and require a coordinated re-export before provenance tests can pass again.
