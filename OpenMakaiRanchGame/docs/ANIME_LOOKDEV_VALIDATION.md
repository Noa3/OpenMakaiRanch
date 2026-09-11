# Anime lookdev validation — material-quality follow-up

Checkpoint: **2026-09-11**. Research/design: [ANIME_REFERENCE_QUALITY.md](ANIME_REFERENCE_QUALITY.md). Parent architecture: [ANIME_VISUAL_PIPELINE.md](ANIME_VISUAL_PIPELINE.md).

## Branch and exact tested source

- Follow-up branch: `feature/anime-material-quality-20260911`, **PR #17**.
- Parent branch: `feature/anime-lookdev-pipeline-20260911`, **PR #16**, fixed base `7aa7f0dbe197aa8df77f20b2b3464490d0ec41f8`.
- Verified implementation/documentation head: **`79deb15b7e64427baab690b2e64cc165c1b26617`**.
- GitHub Actions tested its synthetic PR merge checkout **`b68f7bb8ec1c47f9d39f12cdc822cc992279b058`**, merging that head into the fixed parent. This is not a merge into main.
- Completed workflow: [Anime lookdev rendered acceptance, run 34642914479](https://github.com/Noa3/OpenMakaiRanch/actions/runs/34642914479).
- This receipt is a subsequent documentation-only commit. It does not change the tested code; consult PR checks for later exact-head reruns.

The review-source archive from this run was downloaded, hash-checked, and compared byte-for-byte with the edited local source: no differences in corresponding files. The local source is a whitelisted review snapshot, not a full game export. Actual C# compilation and Godot rendering were performed by GitHub Actions, not by an unavailable local Godot installation.

## Executed results

| Check | Result | Scope |
| --- | --- | --- |
| Full game C# build in CI | Passed, zero errors | Existing Godot/.NET/C# version contracts retained; not an end-to-end gameplay run. |
| Autoload-free lookdev host build | Passed, zero warnings and zero errors | Actual compiled rendering source and existing quality policy. |
| Forward+ rendered acceptance | **121 checks passed, zero failed; 16 PNGs** | Vulkan software driver, real frame rendering, not dummy/headless rendering. |
| Compatibility rendered acceptance | **121 checks passed, zero failed; 16 PNGs** | OpenGL software driver, real frame rendering. |
| CI Python lookdev staging/evidence tests | **15 passed** in each renderer job | Whitelist, isolation, renderer selection, stale/invalid evidence rejection. |
| Local Python tests across Tools/Godot | **63 passed** | Includes the existing launcher/UI evidence tests; this does not execute the full game. |
| Local whitespace check | Passed | `git diff --check`. |
| Downloaded artifact/file hashes | Passed | Both renderer ZIP digests and every entry in their evidence manifests. |

Godot: **4.7.2 stable Mono**. Source language: **C# 12**. Main target: **net8.0**, SDK **10.0.401**. Renderer adapter in both results files: **llvmpipe (LLVM 15.0.7, 256 bits)**. The software renderer is suitable for bounded correctness checks, not a minimum-PC/maximum-PC FPS estimate.

The earlier parent Forward+ evidence had 78 checks and 11 captures. This pass extends the sequence by **43 checks and five captures per renderer**; it does not count all parent shader/quality features as new work.

## What is actually exercised

The test sequence builds original face, hair, eye, facial-line and mouth meshes and checks finite vertices, normalized normals, UVs, tangent components/handedness, valid indices and bounded geometry. Factory checks cover independent material instances, retained texture references, selective-tint inputs, finite/bounded UV transforms and non-mutation of caller profiles.

The generated-player adapter tests cover disabled preview, recognized opaque parts only, unchanged accessories/nested models/transparent parts, idempotence, retained source tints and UV transforms. A textured eye must keep its authored image and must not receive the spherical procedural iris.

Rendered checks use projected iris and sclera pixel neighborhoods: amber-to-blue recoloring must visibly affect the iris while preserving the eye whites. An intentionally unmasked control must affect the whites, proving that the mask makes a visible difference. Changing eye UV scale/offset must change the rendered artwork. These complement, rather than replace, shader compilation and resource-property tests.

All captures check the actual renderer, expected temporal-AA policy and non-empty tonal variation. Tests retain the parent binding ownership/restore safeguards, fixed exposure, isolated World3D, native-size comparison, bounded sample count and portrait-only DOF policy. Portrait mode must hide the large swatches; leaving it must restore them.

Required captures per renderer:

```text
neutral-high              neutral-matte
neutral-low               neutral-medium              neutral-ultra
daylight-high             interior-high               sunset-high
night-high                portrait-high               portrait-rotated-high
portrait-blue-iris-high    portrait-unmasked-high      portrait-uv-shift-high
portrait-profile-high     portrait-night-high
```

There are **32 PNGs in total**, not 32 different scenes or 32 production character tests. The same 16 scenarios run on two backends.

## Artifact receipts

| Artifact | ID | SHA-256 of downloaded ZIP |
| --- | --- | --- |
| Forward+ evidence | `10280803804` | `b61d599bea92b7fa609c45b3e293444a8c1dda13c74af05ccfa3efcb269a7589` |
| Compatibility evidence | `10279874897` | `a12c6cea9114dddba48e18653ad73a74e9525df4315e3743c42b583fdbffc9a9` |
| Whitelisted review source | `10280244313` | `a1f5d85efc36b12d57fce5edbaedcab2dc5d7a2cb696d602ccdf41e45ca0a83d` |

Renderer archives contain PNGs, `results.json`, `source.json`, complete import/build/runtime logs, the ownership marker and a SHA-256 manifest. Source evidence expires after seven days and rendered evidence after fourteen days; regenerate expired artifacts rather than presenting an old screenshot as evidence for new code. No artist reference images, production asset packs or personal save profiles are in these evidence packages.

## Visual inspection and honest limits

Downloaded Forward+ neutral and night portraits and Compatibility neutral, full framing and near-profile captures were opened and visually inspected. They show a more useful shaped material specimen than the parent's sphere-head mannequin, separate eye art, selective palette behavior and a material hierarchy. The face remains schematic, hair locks remain simplified and helmet-like, and the capsule body is explicitly a placeholder. The new specimen is **not** a finished character at BlobCG/Kyoko3D reference quality.

The forward and compatibility images are not visually identical: lighting, shadow softness, tone and supported effects differ. The tests certify their defined material behavior, not perceptual parity across renderers. Fine shadow patterns at the hairline and highlight behavior require further moving-camera/light review on representative hardware. Static TAA warmup and tonal-variation checks do not certify temporal stability, natural animation, all color combinations or production image quality.

No final face rig, blink/speech animation, hair/clothing secondary motion, corrective deformation, production character LODs, approved character identity, new ranch environment, or global post-processing replacement was delivered in this pass. The generated-player preview remains session opt-in; imported residents keep authored materials until explicit surface review. No gameplay balance, calendar, inventory, save-schema or content-eligibility authority changed.

Next production gate: one original/licensed and properly rigged hero character, with authored face/eyelid topology and hair clumps, is reviewed under the five study lights and then in a real ranch exterior/interior. Shader or mannequin acceptance does not replace that gate.

## Diagnostics retained, not concealed

The **full game** CI build reports six `NU1801` warnings from an existing Windows-local Godot NuGet source (`E:/GodotEditor/GodotSharp/Tools/nupkgs`) that is absent on Linux, with zero errors. The separate lookdev-host build reports zero warnings/errors. The package-source configuration was not changed as part of this graphics branch.

Neither rendered runtime log contains `ERROR`, `SCRIPT ERROR` or `SHADER ERROR`. Compatibility retains the driver's unsupported-VSync warning; Forward+ runtime has no warning in this run. The CI action wrappers also emit Node deprecation warnings. No claim of globally warning-free CI is made. Intermediate cancelled runs were superseded by new commits, not counted as passing evidence.

## Reproduction and handoff

```bash
# Run from the repository root with the existing installed Godot Mono binary available.
python -m unittest discover -s Tools/Godot -p 'test_anime_lookdev.py' -v
python Tools/Godot/anime_lookdev.py --renderer forward_plus
python Tools/Godot/anime_lookdev.py --renderer gl_compatibility
python Tools/Godot/anime_lookdev.py --renderer forward_plus --interactive --timeout 3600
```

Use `--godot /actual/path/to/godot-mono` or `GODOT_BIN` when the engine is not on PATH. Linux rendering needs a display or `xvfb-run -a`; the launcher refuses dummy/headless rendered acceptance. It copies only rendering source into a disposable autoload-free host and never accesses a player's normal save slots.

Keep PR #17 stacked until #16 is reviewed/merged, then retarget and rerun against main. Main and the concurrent world/stations/interiors branch were not updated by this work. No PR was automatically merged. The draft is retained for visual review, not because a known material test remains failing.
