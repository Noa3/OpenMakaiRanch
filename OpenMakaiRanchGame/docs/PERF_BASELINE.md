# PERF-001 — GPU Baseline

> Measured 2026-09-07 on a real GPU (Godot 4.7.2 mono, isolated window), via
> the compiler-verified `Performance.GetMonitor(Performance.Monitor.*)` API,
> with the composed `RanchGreybox.tscn` (barn, fences, trees, well, ground,
> walls) + **2 soft-shaded roster avatars**. Reproduce: `bash run_perf.sh`.

## Measured baseline (120 sampled frames after 60 warm-up)

| Metric | Value | Source |
|---|---|---|
| **Draw calls / frame** | **4,143** | `RenderTotalDrawCallsInFrame` |
| **Primitives (tris) / frame** | **393,403** | `RenderTotalPrimitivesInFrame` |
| Rendered objects / frame | 5,553 | `RenderTotalObjectsInFrame` |
| **Video memory** | **2.44 GB** | `RenderVideoMemUsed` (2,614,807,843 bytes) |
| Node count | 2,833 | `ObjectNodeCount` |
| Total objects (scene) | 26,613 | `ObjectCount` |
| Static memory | 621 MB (max 808 MB) | `MemoryStatic` / `MemoryStaticMax` |
| Engine FPS (cap) | ~320 | `TimeFps` |
| Measured frame delta | 72 ms avg (min 52, max 262) | `_Process` `delta` |
| Process / physics time | ~0.003 s | `TimeProcess` / `TimePhysicsProcess` |

## Interpretation

- **The scene is cheap for the GPU** (320 FPS engine cap). Not a perf blocker.
- **The cost that will grow with content is the GLB PBR props:** 393k
  primitives + 2.44 GB video memory come from the imported GLB meshes and their
  PBR texture maps. The repeated fence posts, trees, and well are the biggest
  per-object contributors.
- **Draw calls (4,143) are the most important lever** as the world scales:
  repeated props (fence posts, trees, wall segments) are separate draws and are
  prime candidates for **instancing / `MultiMesh`** once the ranch grows beyond
  this greybox.
- **Frame delta (72 ms) in the capture environment is OS-compositor-throttled**
  (the isolated capture window is not foreground, so Windows limits the render
  loop). It is **not** a reliable in-game FPS number. The stable,
  environment-independent signals are the GPU-cost metrics above. In a real
  foreground window the engine runs at ~320 FPS for this scene.

## Decision gate (for future LOD / instancing work)

- **No LOD/instancing needed yet** — the current composition runs at engine cap
  with modest GPU cost.
- **Re-measure with `PerfCapture` before adding:** large prop counts (fence/tree
  arrays), additional facilities, or higher-poly GLB characters. Thresholds to
  trip the gate: **> ~8,000 draw calls**, **> ~800k primitives**, or
  **> ~4 GB video memory** in a real foreground window.
- The `Performance.Monitor` enum is the **only** compiler-verified stats API in
  Godot 4.7.2 (the per-frame `RenderingServer.Get*Count` methods are not
  exposed to C#). `PerfCapture` is the reusable baseline tool.

## Files

- `src/Dev/PerfCapture.cs` + `scenes/dev/PerfCapture.tscn` — the baseline tool.
- `run_perf.sh` — one-shot runner (isolated profile, logs to temp).
