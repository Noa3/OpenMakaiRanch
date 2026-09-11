# Makai: inhabited otherworld, not a generic lava level

Checkpoint: 2026-09-12. Branch: `feature/makai-presence-and-worldstyle-20260911`, from fixed main `1e7f90936849443f1605e573a37d3e43e0676245`. The earlier anime pipeline/material branches are already included in that main snapshot. This document extends [ANIME_REFERENCE_QUALITY.md](ANIME_REFERENCE_QUALITY.md); it does not replace world simulation or the other agent's stations/interiors work.

## Source findings and confidence

The English text bundled in this repository is a translated snapshot, not a guarantee of the Japanese author's latest wording or release. Source paths below are under `eraMakaiRanch-game-eng-translation/`, read at the fixed main commit above.

| Source | Directly supported | Limits |
| --- | --- | --- |
| `CSV/GameBase.csv`, blob `796758501feb311a46567b9df1ec16f56af47d0e` | Names **polt**, development years **2021–2024**, stored version **1041**, and unfinished development. | This is bundled metadata, not an independently established first publication date or latest upstream version. |
| `ERB/●スタートアップ設定/◯プロローグ.ERB`, blob `3e74815a15588f58802d38670ccd7c9b3623e332` | Makai and surface humans are in conflict. Many demons do not wish to fight. The protagonist avoids joining the demon lord's army and already operates a small countryside ranch. | Do not replace the civilian premise with a mandatory battlefield or assume a particular real-world theology. |
| Same prologue | A visiting researcher, paperwork, a regular collection service and a Makai agricultural cooperative establish institutions and civilian routines. | These support a lived-in setting, not a complete geography or visual canon. The original also has coercive adult premises; this neutral visual work neither reproduces them nor certifies character/content eligibility. |
| Same prologue, `SIF FLAG:ジョーク表示` | The line naming **Okachi / Makkai Plains** is conditional on joke display. | Do not silently make this a binding serious location name. |
| `ERB/表示関数/日付表示.ERB`, blob `cd5725ef38d40bd7cccfed16305186e7d005608e` | Seven-day weeks, 28-day seasons, four seasons and a 112-day year. `DATE_CALC` computes the displayed date. | Reuse the remake's canonical calendar. No extra celestial clock that advances gameplay independently. |
| `ERB/●スタートアップ設定/ゲーム内readme.ERB`, blob `6310c517371daaf9cca400a9cf556b6af53119ce` | Identifies an eramaker/Emuera variant and the player as a Makaian ranch operator. | The generic mention of fan creations does not establish a connection to a specific franchise. |
| Headers of `ERB/○口上/○専用口上ベース.ERB` and several generic dialogue files | Identify authors/contributors and explicitly carry **CC BY-NC** notices. | The remake repository's license is not a blanket permission for every bundled upstream text or asset. Review provenance and reuse permission separately; this pass copies no original dialogue into runtime content. |

The community Era Wiki describes a farm in hell and expressly distinguishes this setting from Touhou's Makai [1]. It is **secondary** evidence: its displayed version 1.011 and September 2024 edit date are stale relative to the bundled metadata. The linked GitGud upstream and original developer thread were not readable during this pass. A mirrored developer-thread introduction was found [2], but it is not treated as authenticated current author guidance. The exact first release history and current Japanese upstream state remain unresolved. The strongest design evidence here is the bundled prologue/calendar/metadata, not a search-result summary.

## Interpretation adopted for look development

**A beautiful, inhabited demon frontier: familiar everyday life under unfamiliar natural laws.** This is our proposed interpretation, not a claim that the original specifies auroras, floating islands or these biome names.

The ranch is an understandable, welcoming place inside a much stranger world. Buildings, trade, work, rest, shared meals and sheltered interiors make everyday life legible. Distant skies and ecosystems supply the extraordinary element. The conflict can appear through distant landmarks, repairs, notices and character histories without making every outdoor surface hostile.

Use the existing soft anime character direction. Character faces, hands and silhouettes carry fine expressive detail; the environment uses larger, quieter color/value masses behind them. Finish one actual ranch exterior/interior/conversation slice before multiplying biomes or placeholder residents. “AAA” is an aspiration evaluated through authored assets, animation, composition and measured performance, not a checkbox or a promise attached to this prototype.

## Art direction

**Architecture.** Warm timber, dark volcanic masonry, patinated metal fittings and pale plaster or mineral surfaces. Believable doors, stairs, ceilings, furniture and sightlines relative to resident height. Slightly unusual arches and roof silhouettes distinguish Makai without obstructing movement or interactions. Preserve the world agent's collision/door/interior ownership.

**Landscape.** Lush rather than permanently burnt. Broad leaf clusters and readable grass silhouettes, muted local rock/soil color, occasional emissive veins or luminous vegetation. More detail at touchable places and landmarks; less high-frequency texture noise behind dialogue faces. Avoid one enormous glowing material on every surface.

**Light and palette.** Restrained cool exterior fill, warmer inhabited windows, locally controlled accents. Keep the sun/day/night/weather grammar understandable even with a supernatural sky. Do not replace all ordinary light with purple rim light. Keep non-emissive skin readable at night without rendering it self-luminous. Gameplay remains sharp; cinematic blur is not a substitute for geometry or texture quality.

**Phenomenon proposals.** One dominant impossible rule per region, with visual limits and ordinary spaces between events:

| Working concept | Visual idea | Gameplay/readability rule | This branch |
| --- | --- | --- | --- |
| Mana veil | Slow teal/violet ribbons high above the horizon with sparse motes. | Evening/night only; no screen flashes, no extra weather rolls, no compulsory light on faces. | Optional lab prototype implemented. |
| Reversed falls | Water rises through a small rift before returning to a basin. | Stable collision and clear water boundaries; not random gravity on the player. | Design only. |
| Glassroot groves | Translucent roots and a restrained seasonal bloom under solid canopies. | Ground routes remain opaque and visible; color does not replace icons. | Design only. |
| Drifting basalt crowns | Distant floating stone formations moving extremely slowly. | Sky landmarks, not unbounded explorable terrain or surprise falling hazards. | Design only. |
| Mirror marsh | A reflective wetland showing a different celestial pattern. | Do not depend on expensive screen-space reflection for essential clues. | Design only. |

Do not activate every phenomenon simultaneously. Characters may notice a **visible, accessible** phenomenon when free, but the atmosphere effect must not grant rewards, override jobs or decide who goes on a date.

## Implemented prototype and its boundaries

`MakaiPhenomenon3D` is an explicit optional scene node. It draws an original low-poly ribbon plus a bounded instanced mote set. The caller supplies enabled state, phase, quality, shelter and reduced motion. Its visual time is caller-advanced and pauseable; the shader deliberately does not use uncontrolled `TIME`.

Mote budgets: **Low/unknown: 0, Medium: 24, High: 64, Ultra: 96**. Low and reduced motion retain a static veil; reduced motion disables motes. Morning/afternoon, shelter and disabled state suppress the effect. There are no new lights, global sky/environment replacements, collisions, weather seeds, game-time changes or rewards. In the lab, it is behind the specimen, not a filter painted over the face.

The prototype is **not integrated into `WorldAtmosphereController` or a production ranch scene**. Those systems already own weather/season effects and other agents are editing the world. Integration should mount one approved node at a curated location, feed existing canonical state, hide it indoors/offscreen and keep the current atmosphere controller as sole owner. One global instancing batch is not a license to scatter unlimited emitters across the map.

## Production acceptance gates

1. Compare an original/licensed rigged hero character in the actual exterior and interior, with post-processing disabled first. Review silhouette, materials, face readability and interaction visibility.
2. Review front/three-quarter/profile, several skin/hair palettes, rain/snow/day/night, and a moving camera. Static lab success does not establish temporal stability or art quality.
3. Author environmental landmarks and props with provenance, texture density, LODs, collision, navigation, interior occlusion and lighting transitions; no arbitrary imported asset collection.
4. Profile a specified minimum/target PC at a stated resolution and resident count. Record CPU/GPU frame time, VRAM, draw/shadow passes and animation cost before setting final budgets. Software-rendered CI is not an FPS benchmark.
5. Test reduced motion, low graphics, UI readability and indoor suppression. No flashing spectacles or foreground particles that obscure gestures and input prompts.

## Sources

[1] Era Wiki, community overview: https://wiki.eragames.rip/index.php/EraMakaiRanch (historical secondary page; not latest-version authority).

[2] Mirror of developer thread with polt introduction: https://www.kyodemo.net/sdemo/r/s_otaku_16783/1683559340/ . Original linked thread: https://jbbs.shitaraba.net/bbs/read.cgi/otaku/16783/1683559340/ . Original and GitGud upstream could not be verified directly in this pass.

Primary bundled source browser, pinned to the inspected commit: https://github.com/Noa3/OpenMakaiRanch/tree/1e7f90936849443f1605e573a37d3e43e0676245/eraMakaiRanch-game-eng-translation . Exact paths and blob IDs are in the source table.

Godot MultiMesh documentation: https://docs.godotengine.org/en/stable/classes/class_multimesh.html . This is an implementation reference, not proof of either reference artist's renderer.
