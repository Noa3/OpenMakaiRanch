# Market kit — parent import and three-state assembly

## Status

Five real GLBs and their unchanged editable Blender source were independently verified and imported. Three authored Godot market variants now share one court and the same imported assets. **They are not yet a placed town market, a live construction state machine, or an earned/persisted project.** No financial or save code changed.

## Delivered scenes

- `scenes/world/OkachiMarketCourt.tscn`: shared 14×12 m reference paving and ground collision. Its permanent side path is X[-6,-4], Z[-6,6], local metres; connect both ends when placing the court. Paving is simple authoring material, not final detailed landscape surfacing.
- `OkachiMarketBase.tscn`: two open counters, X±2, Z0.
- `OkachiMarketWork.tscn`: same counters moved to Z4.6, standing frame without its 17 roof visuals, scaffolding, stocked timber and perimeter barriers. Imported solid frame remains physically present. TSCN editable-child overrides hide roof-only meshes; no duplicate derived GLB was needed. Counters remain physically present outside the work zone, but no trade/service binding is claimed.
- `OkachiMarketFinished.tscn`: completed canopy over counters at their original positions; work barriers/scaffold/stock are absent, not merely hidden.

All three variants use separate authored scene instances for review. Switching these screenshots is **not** evidence of safe live stage replacement or persistence.

## Verified execution

`verify_saved_asset.py` reopens the exact saved `.blend`, checks generator/source/export SHA-256, independently decodes GLB POSITION/index buffers, verifies identity mesh transforms, compares evaluated source bounds, counts collision meshes and confirms no linked libraries/images. It never executes the generator or saves over the source. Result: `MARKET_PARENT_SOURCE_AND_EXPORT_PASS 5`; five exports total 1512 triangles. See `parent-verification.json` and `.log`.

Run engine review from repository root:

```
OMR_MARKET_ASSET_REVIEW=1 python Tools/VisualTargets/capture.py
```

Actual successful run: `.dream-loop/capture-xmzp8x50`, preserved in `godot-review/` with source snapshot, assembly hash, profile checks, logs, measured JSON and six images. Godot 4.7.2 Mono, Vulkan Forward+, RTX 5090, 1600×900, clear-morning Medium fixture, Dummy audio. No FPS or preset/weather/audio acceptance was measured.

| Variant | Asset instances | Asset collision shapes | Visible roof meshes |
|---|---:|---:|---:|
| Base | 2 | 28 | 0 |
| Work | 15 | 172 | 0 |
| Finished | 3 | 65 | 17 |

These collision counts exclude the shared court's one ground shape. Each asset instance loaded its expected collisions at unit world scale and remained within the reserved 14×12 m court.

**Real engine physics:** stationary `IntersectShape` box queries found the 2 m wide × 2.8 m high × 11.5 m long bypass clear in all states. A separate 2×2.8×4 m central query was clear; during work that interior is deliberately inaccessible from the front behind a barrier. A front ray hit that barrier only in Work. Floor is below the probe. These test empty collision volumes and correct temporary obstruction, not continuous player/companion motion, navmesh reachability, floor continuity across region seams or body animation.

Six actual screenshots use matching camera positions/targets/FOV across states: overview and eye height. Existing `noir` mannequin is a standing scale reference, not a persistent trader or production character rig. The overview shows a distinct open/work/roofed progression and free side strip. Work eye view intentionally faces the closed site; its relocated scale reference is partly at the left image edge.

Build passed with existing CS8602 warning. Capture Python suite: 50 tests, 49 passed and one skipped, normal and `python -O`; added guards reject a blocked bypass or premature roof. Synthetic wrapper fixtures are separate from real engine evidence. No fresh full gameplay smoke was run for these opt-in review additions. Existing navigation voxel-rounding warnings remain.

## Integration and art limits

Town placement, existing service routing, real movement/use, weather shelter behavior, construction animation/audio, named traders, progression, safe occupied-state transition and save/load are still separate tasks. No days, charges, contributions, stock or roster entries are fabricated by these scenes. Parent delegated the civic/Base-market town integration as `deleg_cf879fcf`; its result is not yet verified.

Materials are flat, deliberately low-detail; scaffold joint intersection patches remain. Court surfacing and furniture/stock dressing are sparse. The roof has no collision and does not by itself confer rain shelter in gameplay. The scaffold is a static prop, not climbing functionality. Do not describe the first inhabited-region slice as completed on the basis of these images.

Original worker evidence, initial corrective-generation archive and editable assets remain unchanged. No Git publication or user-save changes.
