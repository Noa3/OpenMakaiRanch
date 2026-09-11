# Pastoral ranch revision: rendered validation receipt

Checkpoint: **2026-09-12**. Own branch `feature/makai-presence-and-worldstyle-20260911`, **PR #18**. The user-approved natural starter-area direction supersedes the earlier volcanic/alien proposal in [MAKAI_WORLD_STYLE_AND_LORE.md](MAKAI_WORLD_STYLE_AND_LORE.md).

## Exact code and scope

- Prior branch head: `152df1af02f11928298e3646068348faf387ec98`.
- Implementation head: **`5e7d44655a38261607a9724a2840cffe826192fd`**.
- Actual GitHub Actions synthetic merge checkout: **`413d7b51a53e014d6204b432f6794aa5287059a4`** against unchanged main `1e7f90936849443f1605e573a37d3e43e0676245`.
- This receipt is a subsequent documentation-only commit; it does not change the tested implementation. Consult the PR for later exact-head reruns.

No other branch or main was updated, no PR merged, and no engine/framework/language/save-schema version changed. The original game folder remains read-only. Existing character/presence work is retained, not counted as new implementation here.

There are two distinct deliverables: **the real WorldGame ranch camera gets the new sky and two gate-side mana-stone lamps**; a **separate autoload-free pastoral viewing specimen** demonstrates those components against placeholder grass, trees, hills and a normal-flow river strip. The specimen does not replace the ranch terrain, buildings, colliders, vegetation assets or navigation. There is no new production river or water simulation in this pass.

## Executed results

| Check | Result |
| --- | --- |
| Full game C# build | Successful, zero errors; seven retained warnings below. |
| Isolated study-host builds | Zero warnings and zero errors. |
| New pastoral study, Forward+ | **42 passed, zero failed, seven PNGs**. |
| New pastoral study, Compatibility | **42 passed, zero failed, seven PNGs**. |
| Retained character/presence study, each renderer | **194 passed, zero failed, 21 PNGs**. |
| Connected rendered UI acceptance | **530 passed, zero failed, 64 PNGs**; includes four new ranch-integration assertions. |

All four existing workflows completed successfully for the implementation head:

| Workflow | Run |
| --- | --- |
| Anime lookdev rendered acceptance | [34658962938](https://github.com/Noa3/OpenMakaiRanch/actions/runs/34658962938) |
| Godot 4.7 Mono CI | [34658962942](https://github.com/Noa3/OpenMakaiRanch/actions/runs/34658962942) |
| Build Smoke Check | [34658962983](https://github.com/Noa3/OpenMakaiRanch/actions/runs/34658962983) |
| Rendered UI acceptance | [34658962948](https://github.com/Noa3/OpenMakaiRanch/actions/runs/34658962948) |

The downloaded UI results and runtime logs explicitly contain all four new passing ranch integration checks. General CI/smoke conclusions were checked; no new smoke assertion count is inferred from those conclusions.

The original 194 character/material/presence checks and 21 captures run separately from the new 42 pastoral checks and seven captures on each renderer. Do not describe the combined totals as that many different finished assets or gameplay scenes. The four extra connected-world assertions test mounting, camera-local ownership, disabling and re-enabling through the actual WorldGame scene.

Local checks: **17 lookdev Python tests passed**, covering the original guards plus pastoral staging/capture separation; `git diff --check` passed. No local Godot or .NET runtime was available. Actual C# builds, shader compilation and real frame rendering ran in GitHub Actions, not a dummy headless renderer. The reviewed source archive was SHA-256 checked and all **349 whitelisted files** compared byte-for-byte with the local review snapshot, with no mismatches before this receipt-only edit. The snapshot is not a complete standalone export.

## Captures and what they exercise

- `pastoral-day`: blue sky, ordinary clouds, green landscape, no visible companion world and no active lamp point light.
- `pastoral-night` / `pastoral-moon-only`: actual changed pixels at the companion-world position, while the ordinary moon stays present.
- `pastoral-overcast`: cloud compositing masks the companion; no planet pasted over cloud cover.
- `pastoral-low`: the sky and emissive stone remain while point-light detail is removed.
- `pastoral-lantern-on` / `pastoral-lantern-off`: changed emitted-stone/local-light response, not a bloom-only effect.

Other checks cover sky-lease conflicts, explicit authored-sky replacement and restoration, external takeover, finite uniforms and no repeated dirtying on unchanged context. Lamp checks cover a 4.5-unit range, distance fading, Medium shadow suppression, shelter and opacity. Water sampling checks cover pause, reduced motion, invalid delta and bounded catch-up; they do not certify water physics.

## Downloaded evidence

| Artifact | ID | Downloaded ZIP SHA-256 |
| --- | --- | --- |
| Forward+ original and pastoral studies | `10287195826` | `d99a54cf49eead66736911144ec3b66688c093b44917e6f141b0da053795edf4` |
| Compatibility original and pastoral studies | `10286636268` | `616fd00c407a77ed41bef91697f5dead3ecc5e9533f2c04481489c1f08fe0288` |
| Whitelisted study review source | `10286845454` | `de571dcea4b2f932614a8bd69d1c6d4948e6a779c426cd283a7ecc55c2746f54` |
| Connected UI evidence | `10287015864` | `97cc2955d46ee54b20c7adb710c230da15236e2e46ddf98dee80870328591c08` |

Both renderer archives contain their original and pastoral studies in separate subdirectories, each with results/source identifiers, logs, captures and a SHA-256 manifest. All entries in the four renderer-study manifests were checked against the downloaded bytes; the separate UI archive digest was also checked. Software adapter: **llvmpipe (LLVM 15.0.7, 256 bits)**; Godot **4.7.2 stable Mono**. Rendered artifacts expire after fourteen days, source evidence after seven. Regenerate expired evidence for new code; do not reuse an old PNG as proof of a new commit.

## Visual review and limits

Downloaded day, night and lantern views were opened in **both** renderers. They show an ordinary green meadow, a blue daytime sky, two distinct night bodies and a readable luminous-stone lantern. No floating terrain or reversed river is introduced. The Forward+ result is softer and less saturated than Compatibility; those backend differences are not called perceptual parity. The connected 960x540 ranch UI capture was also opened: the existing playable layout is retained, while that high-angle shot is not a dedicated sky/lantern beauty view. The dedicated celestial and lamp pixel checks are from the isolated study, not an uninterrupted gameplay cinematics test.

The study intentionally uses geometric trees, flat ground, a simple bank/water strip and low hills. It demonstrates sky composition and lamp readability, **not finished AAA landscape art**. Cloud shapes/body placement are static; no orbital simulation, moon phases, moving cloud/weather field or literal reflected celestial discs were added. The stylized companion planet is fictional. Its visibility can be disabled without removing the familiar moon.

The actual ranch retains its established landscape and regular weather/season effects. A camera-local copy follows the authoritative environment; town/intro cameras and the global environment are not overwritten. External camera overrides cause this layer to yield. The connected tests do not certify every load, weather transition, cinematic or possible competing camera writer. The copied source environment is refreshed on state notifications; this is not a target-hardware performance profile.

The stone lamps are decorative opaque geometry without new colliders or fuel/charging behavior. No avatar, face rig, autonomous NPC execution, persistent user setting or economy change is delivered here. Representative GPU/CPU frame-time measurement, authored foliage/terrain/water, detailed animation and a complete organically played ranch route remain separate production tasks.

## Diagnostics

The complete Forward+ job log reports **six NU1801 warnings** for the existing Windows-local Godot NuGet source absent on Linux and **one CS8602 warning in `RanchLeisureFrameTests.cs(218,16)`**. Neither source of those warnings was changed. All isolated host builds have zero warnings/errors; both study runtime logs are free of ERROR/SCRIPT ERROR/SHADER ERROR. Compatibility retains its software driver's VSync warning.

The connected UI runtime has no ERROR/SCRIPT ERROR. It retains VSync and navigation voxel-precision warnings; its successful editor import retains the known EditorSettings shutdown diagnostic. CI action wrappers also emit Node deprecation warnings. No diagnostics were suppressed for these changes, and the complete game pipeline is not described as globally warning-free.

## Reproduction

```bash
python -m unittest discover -s Tools/Godot -p 'test_*lookdev.py' -v
python Tools/Godot/anime_lookdev.py --pastoral --renderer forward_plus
python Tools/Godot/anime_lookdev.py --pastoral --renderer gl_compatibility
python Tools/Godot/anime_lookdev.py --pastoral --renderer forward_plus --interactive --timeout 3600
```

Supply the existing Godot Mono binary via `GODOT_BIN` or `--godot`. Linux needs a display or `xvfb-run -a`. The launcher creates disposable projects/profiles and never reads ordinary personal save slots. In the normal WorldGame scene, `RanchWorld/PastoralSkyAndLamps` exposes `Enabled` and `ShowCompanionWorld`; these do not add a save setting. Keep the PR in draft for artistic/integration review; no automatic merge over the other agents' work.
