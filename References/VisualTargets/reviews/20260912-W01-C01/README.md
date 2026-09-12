# W01 / C01 candidate review package

**Two registered candidates; zero approvals. Both need correction before construction.**

Prepared locally on the user-assigned `astar` branch. Read the canonical technical handoff in [`OpenMakaiRanchGame/docs/VISUAL_TARGET_PREPARATION_20260912.md`](../../../../OpenMakaiRanchGame/docs/VISUAL_TARGET_PREPARATION_20260912.md).

## Proposed images for human direction review

| Shot | Registered PNG | Actual generation | Decision status |
|---|---|---|---|
| W01 | [`W01-fa65ba410e7b.png`](../../targets/W01-fa65ba410e7b.png) | Qwen-Image-Edit-2511 fp8mixed + Lightning 4-step, from fresh Godot source and geometry guide | Style proposal only; not spatially valid construction target |
| C01 | [`C01-770989db797f.png`](../../targets/C01-770989db797f.png) | Qwen-Image-Edit-2511 fp8mixed + Lightning 4-step edit of four local RA-Mix v2.0 figures | Original clothed adult reference proposal; not production identities |

Each registered PNG is 1536×864. Their generation inputs and settings differ from a live engine render: do not attach a fictional engine camera/FPS record to the generated pixels. `manifest.target.prompt_sha256` hashes the registry brief; full actual model conditioning is stored in each archived workflow and prompt/receipt.

## Self-review, explicitly not independent approval

A fresh independent visual reviewer was requested but timed out after 600 seconds without returning a verdict. No independent visual score, pass or approval is claimed. The following findings are the parent agent's direct inspection of the actual PNGs and fresh source images. No further broad generation rounds were launched after the repeated spatial failures.

### W01 — choose the four-step version as a style conversation starter

Strengths: normal green landscape, blue sky, readable wood/plaster/terracotta/stone material families, natural-looking external stream, useful sunlight/shadow direction, grass and foliage detail. These form a coherent possible visual direction.

Blocking gaps:

- Camera is closer/lower than source P01; the property no longer has the original wide framing.
- Building/ruin count, centers, proportions and entrances do not match the eight canonical plots. The center contains additional roofed structures. Visible ruins do not reliably map to the five unavailable stations.
- The fresh `kitchen:1`, `pasture:1` plus house state is not faithfully represented. Do not infer facility ownership from the generated roofs.
- Boundary trees became oversized/repositioned; small primitive round tree markers remain in the foreground.
- Front gate/approach and interior/exterior door correspondence are unverified. No navmesh, collision or playable layout is supplied by the image.
- Foliage, trim and roof forms still lean toward stylized toy-like repetition rather than final authored 3D detail.

The 40-step variant is preserved in `attempts/round-3-W01-qwen-full/candidate.png`. It was not selected: although some building materials are useful, it promotes almost all visible plots to completed buildings, enlarges/rearranges structures and loses the proposed inactive-state distinction. More diffusion steps did not solve the state/spatial failure.

**Recommended decision:** accept/reject the material, color and landscape *direction only*. Correct geometry through localized edits with locked camera/plot anchors before approving W01 as a build target. Never silently change canonical footprints to match this picture.

### C01 — Qwen refinement of RA-Mix lineup

Left to right are proposed REF_A, REF_B, REF_C and REF_D. Names are column designations, not existing roster identities. All are fully clothed; the frame includes shoes and hands. Clothing layers, workwear differentiation and subdued material palette are useful.

Remaining gaps:

- REF_A's tied ash hair is not clearly readable; the visible silhouette resembles short/loose hair. Require tied-hair front/side/back confirmation.
- REF_B's workwear/copper braid is the strongest current role read. Fingers still need closer anatomical review and multi-view consistency.
- REF_C's facial treatment is more semi-realistic than the intended shared soft-anime style. Align eye/face/hair construction, not just color grading.
- REF_D does not read distinctly as the requested older caretaker; hair is too long and age cues too weak. Silver color alone is not age validation.
- Hands are visible but no neutral-pose topology, rig, weights, deformation, turntable or animation test exists. One generated view is not a production character sheet.

**Recommended decision:** confirm clothing/rendering direction and choose a first reference to refine. REF_B has the fewest obvious brief deviations; this is not automatic identity approval. No existing minor/minor-coded roster character is used as an adult reference.

## Evidence map

- `evidence-index.json`: hashed list of source captures, model receipts, API graphs/history, raw variants and verification files. Thirteen raw generated images; composites and source PNGs are counted separately.
- `sources/W01/`: original fresh-game image used for generation, P01/fixture JSON, engine/isolation logs, projected geometry guide. Labels are an input guide, not generated target art.
- `sources/C01/`: actual lookdev pose/material-study input and lookdev results.
- `attempts/round-1-*`: superseded AlbedoBase XL attempts.
- `attempts/round-2-C01-ra-mix/`: four individual RA-Mix outputs plus labeled composite and source initialization.
- `attempts/round-2-W01-qwen/`: selected W01 raw output, actual API graph, prompts, job history and receipt.
- `attempts/round-3-*`: full-step W01 comparison and selected C01 edited output.
- `provenance/`: exact downloaded model versions/hashes/license metadata, official-template notes and generation-script snapshots. These snapshots retain their original `.dream-loop` execution paths; they are records, not an automatically runnable installation from this archive directory.
- `verification/`: earlier baseline checks, smoke and failing rendered UI acceptance.
- `verification-after-review/`: capture provenance fixes and regression checks.
- `verification-final/`: final real build/capture with GPU particle exclusion, current tests. Neither later capture replaces the original image-generation input.

Model weights are installed locally, not committed here. No external image uploads, paid jobs, new Custom Nodes, commits, pushes or production-world edits were performed.

## Human gate

Do not invoke `atlas.py approve` from this document alone. A real user decision and corrected matching target are required. No `.dream-loop/target.png` is locked while W01 remains spatially invalid. The small playable ranch slice begins only after that gate; current state is candidate preparation, not a completed game upgrade.
