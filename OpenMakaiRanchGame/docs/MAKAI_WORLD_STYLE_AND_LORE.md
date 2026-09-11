# Makai world direction: a green, welcoming ranch

**Art-direction revision, 2026-09-12: the user's natural starter-area direction supersedes the previous demon-frontier proposal.** Continue on `feature/makai-presence-and-worldstyle-20260911` / PR #18; other agents retain gameplay and navigation ownership. Reference character materials remain described in [ANIME_REFERENCE_QUALITY.md](ANIME_REFERENCE_QUALITY.md).

## Approved direction, not an optional alternative

The ranch is in an ordinary-looking, beautiful green area: **grass-covered ground, familiar trees, a naturally flowing river, a predominantly blue daytime sky, a sun and ordinary weather**. It should feel like an attractive starter area where the player wants to spend time. Makai's premise does not require every object to look demonic.

**Do not build the rejected proposal into this area:** volcanic/black-basalt dominance, alien tree/root shapes, floating land, reversed waterfalls, supernatural marsh reflections or exaggerated demonic roofs. These ideas are not automatically approved for later regions either. Do not preserve them as default implementation tasks elsewhere in the roadmap.

Fantasy should be quiet, understandable and selective: a normal-looking moon with an additional **fictional Earth-like companion world** at night, modest mana phenomena, and lamps powered visually by softly luminous stones. The companion world is an adjustable visual choice, not a statement that the real Earth is physically orbiting Makai. No new astronomy, lunar calendar, gravity, tides, consumable fuel or mana economy is implied.

## Visual rules

- **Landscape:** fresh but varied greens, ordinary broadleaf trees, open grass, readable paths, plausible riverbanks and gentle terrain. Do not make every surface noisy, glossy, luminous or fantastical. Preserve existing accessible routes while authored terrain is developed.
- **Water:** downstream motion, calm pools and natural river bends. The starter river must eventually have coherent banks, bridges/crossings, collision/navigation and a deliberate playable boundary. A decorative water strip is not a complete playable river.
- **Sky:** blue by day, familiar clouds and sunshine, softer warm horizons at dusk, dark blue at night. Keep both celestial bodies behind clouds and scene geometry; no planetary disc pasted over a roof or thunderstorm. A small companion world should invite noticing, not dominate the entire sky.
- **Architecture:** approachable rural buildings with natural wood/plaster/stone and restrained proportions. No mandatory gothic/demonic silhouette. Existing buildings are not replaced in this pass.
- **Mana lighting:** an opaque, pale stone inside a recognizable lantern housing, warm local illumination at night, subdued daytime appearance. No forced purple cast on skin, no flashing, no continuous pulsing, no requirement for bloom to understand that the stone emits light.
- **Characters:** face and hand readability remain important, but achieve this through composition, lighting and sensible detail density, not by making the whole landscape empty. Existing microgesture/personality work remains applicable.

“AAA” remains the intended production ambition, not a quality certification for procedural placeholders. Final foliage, terrain, architecture, water, facial rigs, motion and target-hardware performance need authored assets and separate acceptance.

## Implemented follow-up

`PastoralSky` supplies an original Godot sky material with blue-sky gradients, an ordinary sun disc, clouds, a moon and an optional fictional blue/green companion world. Celestial bodies appear in the evening/night rather than changing the familiar daytime view. Cloud cover is applied after them, so overcast hides them. Terrain/roofs occlude the sky normally. Body positions and cloud shapes are currently static; this is not an orbital or animated-weather simulation. The sky deliberately omits TIME/POSITION so idle frames do not invalidate its radiance cubemap. Native background evaluation retains celestial edge detail while the incremental reflection map stays bounded at 128x128. Small celestial discs are omitted from that reflection map to avoid unstable bright specks; literal reflected moon/planet discs are not delivered.

`ManaStoneLantern3D` supplies an original small lamp with a metal frame, ordinary support and luminous stone. Its light comes on from caller-supplied evening/night state. Low retains the visible emissive stone without a point light; Medium omits lamp shadow passes; High/Ultra may use shadows when the existing settings permit them. The point-light radius is 4.5 world units with camera-distance fading. Exterior local lights are suppressed while the viewer is sheltered; the mesh still obeys ordinary visibility/occlusion. It has no inventory, charging or fuel behavior.

**Actual ranch integration:** `WorldGame.tscn` mounts `RanchSkyAndLanterns` under the existing RanchWorld, adding the sky to that ranch camera and exactly two decorative gate-side lanterns. It does not replace the ranch scene, terrain, trees, water, buildings, colliders, navigation or world boundary. The gate/path is not moved. Lantern supports remain decorative geometry without new collision.

The adapter takes a camera-local duplicate of the existing environment after the authoritative DaylightRig handlers finish. Exposure, fog and lighting settings come from that source; the global WorldEnvironment and the town/intro cameras remain untouched. Existing authored sky/camera overrides are not silently replaced. External camera takeover makes the layer yield; disabling restores its original null camera override only while it still owns it. Canonical calendar/weather/settings remain the source of truth. There is no second day/season clock and no world-layout change on another agent's branch.

**Separate style study:** `AnimeLookDevPastoral.tscn` shows an original green meadow, ordinary placeholder trees, low hills and a normal-direction river surface, plus the same sky and lantern components. It is an isolated viewing scene, not replacement production terrain. Riverbank geometry, depth, water collision, authored vegetation and naturalistic water shading are not certified by this specimen. Existing character/material/presence acceptance remains a separate study and is not weakened to accommodate the revised palette.

The accepted mana-veil prototype and existing ranch night-mote/weather effects are retained; this pass does not automatically blanket the ranch in an aurora. Otherworldly spectacles should remain optional accents to the natural area.

## Run and handoff

```bash
# Existing isolated launcher; neither command reads normal personal save slots.
python Tools/Godot/anime_lookdev.py --pastoral --renderer forward_plus --interactive --timeout 3600
python Tools/Godot/anime_lookdev.py --pastoral --renderer gl_compatibility
```

Use the installed Godot Mono binary via GODOT_BIN or --godot, and a display/xvfb-run on Linux. The normal WorldGame on this branch includes the camera/lantern integration. In the scene inspector, `RanchWorld/PastoralSkyAndLamps` has `Enabled` and `ShowCompanionWorld` controls; no new user settings/save schema is introduced. See [PASTORAL_RANCH_VALIDATION.md](PASTORAL_RANCH_VALIDATION.md) for executed results and remaining limitations.

Next: one coherent authored green ranch area and organic river/crossing, reviewed with a high-quality character. Preserve the other agents' doors, stations and navigation; terrain cannot simply be painted across them. Improve face/gaze/gesture assets alongside the environment rather than generating a larger collection of placeholders.

## Original-game evidence retained

The revised art direction is the user's interpretation, not invented original canon. The source findings below were read from the bundled translated snapshot at `1e7f90936849443f1605e573a37d3e43e0676245`, under `eraMakaiRanch-game-eng-translation/`.

| Source | Supported finding | Boundary |
| --- | --- | --- |
| `CSV/GameBase.csv` (`796758501feb311a46567b9df1ec16f56af47d0e`) | polt; metadata years 2021–2024; stored version 1041; unfinished development. | Not proof of the first publication or latest Japanese release. |
| `ERB/●スタートアップ設定/◯プロローグ.ERB` (`3e74815a15588f58802d38670ccd7c9b3623e332`) | Makai and surface humans are in conflict; many demons do not want to fight; the protagonist operates a countryside ranch rather than joining the demon lord's army. | Does not prescribe lava, alien vegetation, supernatural architecture or a real-world theology. |
| Same prologue | Research, paperwork, regular collection and a Makai agricultural cooperative establish civilian life. | Not a complete geography or visual canon. |
| Same prologue, `SIF FLAG:ジョーク表示` | Okachi / Makkai Plains naming is conditional on joke display. | Not automatically a mandatory serious place name. |
| `ERB/表示関数/日付表示.ERB` (`cd5725ef38d40bd7cccfed16305186e7d005608e`) | Seven-day weeks, 28-day seasons, four seasons and 112-day years. | Reuse the existing canonical calendar. |
| `ERB/●スタートアップ設定/ゲーム内readme.ERB` (`6310c517371daaf9cca400a9cf556b6af53119ce`) | An eramaker/Emuera variant and a Makaian ranch operator. | Does not establish a specific franchise's canon. |
| Selected dialogue headers | Author/contributor names and CC BY-NC notices. | Upstream text/assets need their own provenance review; this pass copies none into new runtime content. |

The earlier community Era Wiki page distinguished this setting from Touhou's Makai, but its historical version/date are not current upstream authority. The original developer thread and GitGud upstream were not directly verifiable in that research pass. First-publication history and current Japanese upstream state remain unresolved. This follow-up does not claim a new historical investigation.

Historical references: https://wiki.eragames.rip/index.php/EraMakaiRanch ; https://www.kyodemo.net/sdemo/r/s_otaku_16783/1683559340/ ; https://jbbs.shitaraba.net/bbs/read.cgi/otaku/16783/1683559340/ . Pinned bundled source: https://github.com/Noa3/OpenMakaiRanch/tree/1e7f90936849443f1605e573a37d3e43e0676245/eraMakaiRanch-game-eng-translation .

## Technical references checked for this revision

Godot 4.7 sky shader and radiance invalidation: https://docs.godotengine.org/en/4.7/tutorials/shaders/shader_reference/sky_shader.html

Sky update/radiance modes: https://docs.godotengine.org/en/4.7/classes/class_sky.html

Environment/camera presentation: https://docs.godotengine.org/en/4.7/classes/class_environment.html

Bounded point lights: https://docs.godotengine.org/en/4.7/classes/class_omnilight3d.html and https://docs.godotengine.org/en/4.7/classes/class_light3d.html
