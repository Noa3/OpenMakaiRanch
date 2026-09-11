# Presence and Makai prototype: validation receipt

Checkpoint: **2026-09-12**. Branch `feature/makai-presence-and-worldstyle-20260911`, **PR #18**. Design and integration: [CHARACTER_PRESENCE_AND_AUTONOMY.md](CHARACTER_PRESENCE_AND_AUTONOMY.md). Research and world direction: [MAKAI_WORLD_STYLE_AND_LORE.md](MAKAI_WORLD_STYLE_AND_LORE.md).

## Exact tested source and ownership

- Fixed main base: `1e7f90936849443f1605e573a37d3e43e0676245`.
- Verified implementation head: **`e527a3a273a9116358e745320fc33dfae34a52a2`**.
- Actual GitHub Actions PR merge checkout: **`424799da99ab1a14c61da2ba49d61714d3fdfede`**.
- This receipt is a subsequent **documentation-only** commit; it does not change the tested implementation. Later exact-head reruns are visible on the PR.
- No main/concurrent branch update, automatic merge, original-source edit, engine/language upgrade, new workflow or package dependency was performed.

C# compilation and actual Godot rendering ran in GitHub Actions. No local Godot/.NET execution is claimed. The local workspace is a whitelisted review snapshot, not a full clone/export. The downloaded review-source ZIP and all 15 corresponding changed files were compared with the local implementation before this receipt: no byte differences.

## Executed checks

| Check | Result |
| --- | --- |
| Full game C# build in the lookdev workflow | Successful, **zero errors**, seven existing warnings described below. |
| Isolated lookdev host build | Successful, **zero warnings / zero errors**. |
| Forward+ rendered acceptance | **194 passed / zero failed / 21 PNGs**. |
| Compatibility rendered acceptance | **194 passed / zero failed / 21 PNGs**. |
| Local lookdev staging/evidence Python suite | **15 passed**. The same 15 tests pass in CI. |
| Local whitespace check | `git diff --check` passed. |
| Downloaded evidence verification | Both renderer ZIP SHA-256 values and every evidence manifest entry verified. |

The 194 checks retain the previous 121 material checks and add **73 presence/phenomenon checks**. Each renderer runs the same 21 views: the original 16 material captures plus `presence-neutral`, `presence-blink`, `presence-warm`, `makai-off`, and `makai-night`. There are **42 PNGs in total**, not 42 different character assets or scenes.

All four existing workflows completed successfully on the verified implementation head:

| Workflow | Run |
| --- | --- |
| Anime lookdev rendered acceptance | [34654493713](https://github.com/Noa3/OpenMakaiRanch/actions/runs/34654493713) |
| Godot 4.7 Mono CI | [34654493850](https://github.com/Noa3/OpenMakaiRanch/actions/runs/34654493850) |
| Build Smoke Check | [34654493699](https://github.com/Noa3/OpenMakaiRanch/actions/runs/34654493699) |
| Rendered UI acceptance | [34654493795](https://github.com/Noa3/OpenMakaiRanch/actions/runs/34654493795) |

The general workflow conclusions were checked; this receipt does not invent new gameplay/UI assertion counts from them. Attempts to run the broader local Python discovery suite timed out, including while an existing character-audit test was active; those attempts are **not counted as passes**. No unrelated logic/test file was changed to conceal that limitation.

## Reproduced defects and visual review

The first implementation run exposed a strict endpoint failure in personality interpolation. Returning the validated input directly at weight zero/one fixes it without weakening the assertion.

Actual screenshot inspection then exposed a disappearing open mouth that the broad image-difference test had missed. The original jaw target dragged the top of the graphic mouth through its lower edge, reversing triangle winding. The corrected target expands the opening. New checks verify target winding and a rendered mouth/cheek contrast probe.

A subsequent visual pass found that full blink collapsed the eyelid strip into degenerate geometry. Closed lids now retain finite crease width, and the flattened eye patch stays behind that crease. A targeted rendered crease probe protects the fix. Both backends pass these probes; the changes are not based solely on successful compilation or unrelated whole-image differences.

Forward+ expression/blink/Makai and Compatibility expression/blink/Makai samples were opened during review. The specimen remains visibly schematic: simplified face and hair, graphic mouth, capsule body and no production hands or eye sockets. The two renderers are not perceptually identical. Passing static probes does not certify animation naturalness, temporal stability, every expression mixture or finished artist/AAA quality.

## What is covered

Tests exercise neutral/custom/blended preferences, finite validation, exact endpoints, deterministic per-actor sampling, actual blink intervals, acknowledgement response, bounded motion, pause/hidden-state freezing, explicit session reset and stale actor/session/revision rejection.

Advice tests cover closed-by-default opportunities, unavailable-facility reasons, urgent needs/weather, preserved duties, explicit accepted companionship, ordinary free-time preference differences, visible-phenomenon requirements and bounded retention. They execute **no production NPC command** and grant no resources.

Rig tests cover explicit named shape slots, competing leases, instance-local weights, baseline restore, external pivot/morph writers and mesh-reference replacement. They do not certify arbitrary in-place edits to imported resource contents or every AnimationTree/SkeletonModifier integration. Production channels require authored ownership and blending.

Phenomenon checks cover bounded instancing, quality tiers, reduced motion, pause, supplied time-of-day, shelter, disable and actual rendered off/on differences while retaining the existing environment/camera objects. The effect remains an optional lab node, not a global sky/weather replacement.

## Artifact receipts

| Artifact | ID | SHA-256 of downloaded ZIP |
| --- | --- | --- |
| Forward+ evidence | `10284934007` | `67df6f0498a20d9c5723633377cbf1d27903a390beda6e0585db75931058c9d5` |
| Compatibility evidence | `10285198027` | `a9f28cdc98c59816028ced68a15b449097fa9820355594b7bc3241e548567709` |
| Whitelisted review source | `10285237801` | `4726f4faffd886f43550c66fc26e2c1ada968a00bcf8140be199af2d3fdfca2d` |

Both results files identify the tested merge checkout above, Godot **4.7.2 stable Mono**, and **llvmpipe (LLVM 15.0.7, 256 bits)**. Vulkan Forward+ and OpenGL Compatibility rendered real frames, not a dummy headless renderer. This software device is **not a target-player GPU benchmark**. Rendered artifacts expire after fourteen days and review source after seven; regenerate expired evidence rather than attaching old images to new code.

## Diagnostics retained

Neither rendered runtime log contains ERROR, SCRIPT ERROR or SHADER ERROR. Compatibility retains the software driver's unsupported-VSync warning. Forward+ runtime has no warning in this run. The isolated host imports/builds without errors.

The full game build reports **six NU1801 warnings** about its existing Windows-local NuGet source missing on Linux, plus **CS8602 in `src/Tests/RanchLeisureFrameTests.cs(218,16)`**. These existing files/configuration were not modified by this branch. CI action wrappers also emit Node deprecation warnings. The overall pipeline is not described as globally warning-free.

## Reproduction and remaining scope

```bash
python -m unittest discover -s Tools/Godot -p 'test_anime_lookdev.py' -v
python Tools/Godot/anime_lookdev.py --renderer forward_plus
python Tools/Godot/anime_lookdev.py --renderer gl_compatibility
python Tools/Godot/anime_lookdev.py --renderer forward_plus --interactive --timeout 3600
```

Set `GODOT_BIN` or use `--godot` for the installed Godot Mono binary. Linux requires a display or `xvfb-run -a`. The host is autoload-free and uses disposable profile directories, not production save slots.

No final production facial rig, independent eye-gaze solver, phoneme alignment, authored hand-gesture library, autonomous ranch movement or creator UI was delivered. The original source remains read-only. Save schema, economy, clock, needs, original traits, NPC navigation, existing player input and the other agents' ownership are unchanged. The next integration should feed canonical snapshots into this read-only interface and connect an approved authored character through the existing command/animation owners. Keep PR #18 unmerged until that visual/integration review is accepted.
