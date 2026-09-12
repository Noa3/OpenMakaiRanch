# W01 / C01 visual-target preparation — 2026-09-12

## Boundary

Work branch: `astar`, explicitly assigned by Noa3, in `E:/OpenMakaiRanch`. HEAD at preparation: `e9aac5c01e6244c91cf30d2ca86fba15e73c9708`. Other worktrees were left alone. No merge, reset, commit, push, production-world replacement, economy change or original-game edit was performed.

`origin/feature/visual-target-atlas-20260912` points to `cc4fbc8ac202f6c5220cc6c101e6014bfcbfccec` and is already an ancestor of HEAD. The graphics base `71fb3a170b6f4baaf1cc8a1a45cdbd1dceab935f` is also an ancestor. `References/VisualTargets` initially matched the reference branch. Its green ranch / mana-lantern graphics were not imported twice.

This is **target preparation**, not a completed playable vertical slice. No approved W01 or C01 exists. No Dream Loop implementation round has started. The Pro workflow is selected for independent visual criticism after a target is accepted; it is not mixed with the Plus workflow. Human review precedes a binding construction target. Existing SVGs and historical baseline receipts remain distinct from generated proposals.

## Actual source captures

Added isolated dev harness:

- `src/Dev/VisualTargetCapture.cs`
- `scenes/dev/VisualTargetCapture.tscn`
- `Tools/VisualTargets/capture.py` (repository root)
- `Tools/VisualTargets/test_capture.py` (repository root)
- `Tools/VisualTargets/test_capture_provenance.py` (repository root)

Capture command, from repository root after `dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj`:

```bash
python Tools/VisualTargets/capture.py
```

The wrapper discovers a stable Godot 4.7 .NET executable, checks runtime port 9501, redirects the child profile, verifies the actual engine `user://` location, fingerprints scoped tracked/untracked sources, performs a Debug build, and writes its isolation marker before starting the capture scene. The C# scene requires debug mode, explicit capture argument, a real renderer and the isolated profile. It starts a disposable new game, bypasses presentation onboarding only, disables autosave, fixes weather to Clear, hides HUD/labels, and reads the viewport after `FramePostDraw`. Production scenes and service implementations are unchanged.

W01 source run: `.dream-loop/capture-0ghaskkk/`. Actual context: Godot `4.7.2.stable.mono.official.ed1daf0bf`, Forward+, NVIDIA GeForce RTX 5090, 1600×900, quality preset **Medium**, render scale 1.0, English, Morning/Clear. Perspective camera P01: position `(38,23,42)`, FOV 55°, looking toward `(0,0,-1)`. Fresh facility map: `kitchen: 1`, `pasture: 1`; the house is already available. Five other facility plots remain unbuilt. No invented day gates or prices.

This input includes an uncommitted dev harness on top of HEAD; its commit field is not evidence of a clean committed build. Full camera/plot context, original PNG, engine log and isolation receipt are archived. It is a reference input, not a performance benchmark.

C01 source run: `.artifacts/anime-lookdev/forward_plus-fd78a6cbb80840b6aa27e17c84ff5e8a/evidence/`. `neutral-high.png` supplies only cropped pose/color initialization. `presence-neutral.png` is also archived. Neither the primitive study model nor a production-roster identity is a character-design lock. REF_A–REF_D are independent, fully clothed adult design proposals. Production identity/adult-content gates are untouched.

## Real local generation

User authorized local ComfyUI use and model changes/downloads. No paid generation API, external reference-image upload, subscription, asset purchase or image-to-3D job was used. Model-weight downloads used public Civitai / Hugging Face sources; generation and image uploads ran only against loopback ComfyUI.

Model files were admitted after upstream SHA-256 verification, using `safetensors`; no new Custom Node package was installed. Exact revisions, hashes, license metadata and byte counts are archived. Models installed under `D:/ComfyUI/ComfyUI/models/`:

- `raMix_v20.safetensors`, RA-Mix v2.0, Illustrious, Civitai model `2658582`, version `3266045`.
- `qwen_image_edit_2511_fp8mixed.safetensors`.
- `qwen_2.5_vl_7b_fp8_scaled.safetensors`.
- `qwen_image_vae.safetensors`.
- `Qwen-Image-Edit-2511-Lightning-4steps-V1.0-bf16.safetensors`.

Native Qwen API graphs were derived from the installed official `image_qwen_image_edit_2511.json` template. Deliberate differences are recorded: explicit sampler mode instead of UI switches; exact 1536×864 scaling instead of variable preset resizing; official repack without optional third-party latent override. Full actual prompts, workflows, source hashes, prompt IDs, histories and output hashes are preserved. These are actual outputs, not fabricated render evidence.

### Bounded preparation rounds

1. AlbedoBase XL: W01 at two denoise strengths; four C01 figures, then a labeled composite. W01 loses most plot geometry; C01 has inconsistent realism and hidden hands. Superseded.
2. RA-Mix v2: four C01 figures with better clothing/forms, then composite. Qwen Lightning: W01 edited from live source plus labeled geometry guide. Better materials; persistent framing/plot/state drift.
3. Qwen full 40-step W01 edit from original source with a stricter geometry brief; Qwen Lightning edit of the Ra-Mix C01 lineup. The world still invents built structures; C01 still has age/hair/style gaps. No automatic target lock follows from a successful image job.

Archive: `References/VisualTargets/reviews/20260912-W01-C01/`. `evidence-index.json` enumerates **13 raw generated outputs**, separately from composites, source PNGs and diagnostic labels. Original attempts remain available; no historical capture was renamed to a generated target.

## Verified checks and remaining failures

| Check | Actual result |
|---|---|
| `dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj` | Exit 0, 0 errors; final actual recompile reports existing CS8602 in `src/Tests/RanchLeisureFrameTests.cs:218`; subsequent incremental build reports 0 warnings |
| VisualTargets Python tests after review fixes | 44 run, 43 passed, 1 skipped |
| Same suite with `python -O` after review fixes | 44 run, 43 passed, 1 skipped |
| Godot Python tooling tests | 80 run, 79 passed, 1 skipped |
| Isolated W01 capture | `VISUAL CAPTURE PASS`; real 1600×900 PNG |
| Forward+ anime lookdev | Exit 0; `ANIME LOOKDEV PASS`; 194 checks and 21 rendered PNGs; results JSON `passed: true` |
| Isolated full smoke | Exit 0; 2,091 `SMOKE OK`, zero `SMOKE FAIL`, one `SMOKE PASS` |
| Rendered UI acceptance | **Not green: 508/530 passed, 22 failed** |
| `git diff --check` | Exit 0 |

The real Windows symlink integration test is skipped because the process lacks symlink privilege. A separate mocked detected-symlink rejection test runs. The capture-wrapper tests mock Godot execution; they complement, not replace, the real captured run. The Godot-tooling suite also reports one skip. Smoke intentionally logs invalid-save rejection errors in its disposable slot 99; these are not silent unexpected crashes.

UI failure evidence is archived verbatim in `verification/ui-acceptance-results.json`: Help/input release, town-travel/return and nearby station/panel checks at 480×800 and 960×540. This run occurred before the new capture harness was used and is not a green UI acceptance claim. No speculative changes were made to parallel UI/gameplay code.

## Review and next gate

Independent capture-harness review reported two P2 findings: HEAD alone did not identify dirty source bytes, and the wrapper did not build the current assembly. Both were reproduced through failing wrapper regression checks and fixed in `Tools/VisualTargets/capture.py`: a mandatory successful Debug build, SHA-256 manifest of scoped tracked/untracked source inputs, recorded DLL hash, and before/after source/DLL equality checks. Secret-like files and linked/external inputs in the source scope are refused rather than read. This is a scoped fingerprint, not a hermetic build attestation covering the OS or all external package caches.

Fresh post-fix capture `.dream-loop/capture-rj2q6s1h/` passed with a real HUD-free 1600×900 PNG, 1,398 fingerprinted source files and assembly SHA-256 `f419ccac038fe4352d962c6d4e9a7e91444458468e93bfb7b0a97047984e00eb`. Source fingerprint: `680f72f7dd423ef8d2df498712eacd5cfb119f5229ad3b02a06981f10bec59a0`. The live capture/build and all impacted Python tests were re-run. Evidence is under `verification-after-review/`; original generation inputs were not overwritten or retrospectively relabeled.

Independent visual candidate criticism timed out after 600 seconds without a verdict. No independent visual score is claimed; the actual candidate inspection is explicitly a parent self-review. No human approval is recorded. Registered candidates are `targets/W01-fa65ba410e7b.png` and `targets/C01-770989db797f.png` under the reference pack; detailed findings and raw alternatives are in `References/VisualTargets/reviews/20260912-W01-C01/README.md`. Registry status: 2 candidates, 0 approved, 47 awaiting generation. The more attractive four-step W01 is proposed for style discussion only; the full-step variant did not preserve inactive facilities.

An earlier independent maintainer review also identified time-dependent GPU particles surviving `ProcessMode.Disabled`. The fixture now explicitly stops and hides its GPU emitters after freezing world processing and verifies their exclusion after `FramePostDraw`. `quality.gpu_particles` and the exact excluded node paths are serialized. Production atmosphere code is unchanged. Final real run `.dream-loop/capture-kaf4r_wn/` passed with six GPU emitters excluded, source fingerprint `0f0c1a373c8f67b8e317c1fb491c85764ef63c0df66ed91812fb3358885736d1` and DLL hash `5229c06af849a85380889a66d0f0d2028f1b6ed5fde9afb105dc9de55baf227c`. Final logs and PNG are under `verification-final/`. Shader time, anti-aliasing history and real-frame pacing are not claimed to be pixel-exact deterministic. The raw game source itself renders roofed study shells on inactive station plots; these visual previews must not be mistaken for facility ownership. Whole-image edits repeatedly promote them into finished buildings. Next correction should isolate the inactive plots and lock spatial anchors rather than keep sampling unrelated full-ranch variants.

Before construction: settle W01 style **and** correct the spatial/state contract; select/refine C01 identities with consistent shared soft-anime rendering, visible adult age cues and complete multi-view anatomy. W02/W03, building sheets and interiors follow the accepted source, not independent unrelated generation. The first implementation scope remains one connected ranch area, one approved clothed character, one enterable building/interior and one existing-service-backed interaction. Preserve canonical eight plots and all gameplay authority. FPS and moving-camera/interaction tests remain unmeasured for that future slice.
