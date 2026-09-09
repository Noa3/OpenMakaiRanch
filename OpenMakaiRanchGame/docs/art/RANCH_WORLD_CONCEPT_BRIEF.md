# Ranch World Concept Brief — Modern Anime 3D

Status: **DRAFT / SAFE ENVIRONMENT ART DIRECTION**  
Scope: environment/world only. No character or adult-content art is defined here.

This brief turns the existing source-grounded `RANCH_FLOORPLAN.md` into a visual target that can be used by Hermes, Astra, Blender, ComfyUI/image-reference workflows, or a human environment artist.

## Core visual goal

The ranch should read immediately as a small but active Makai countryside settlement rather than an empty realistic farm.

Target qualities:

- modern stylized anime 3D;
- clear third-person navigation;
- strong landmark silhouettes;
- warm, inviting ranch materials;
- restrained supernatural/Makai accents rather than horror decoration everywhere;
- vegetation grouped into readable masses instead of dense visual noise;
- buildings visibly communicate their gameplay purpose;
- day/evening/night remain readable;
- deliberately stylized lighting and materials rather than photorealistic PBR.

Commercial games may be studied only for composition/readability principles. Do not copy their assets, buildings, characters, textures, or exact designs.

## Reference principles

Recent stylized farming/life-sim references consistently show useful principles for this project:

- paths are visually obvious from gameplay camera height;
- major buildings use different roof/silhouette shapes, not just different signs;
- fences, crops, trees and props frame routes without blocking them;
- saturated vegetation is balanced by warmer neutral buildings and paths;
- interactive locations have strong foreground landmarks;
- the world uses broad color masses before small detail.

Godot-specific stylized/anime production should evaluate toon/cel shading, edited normals, outlines, vertex/material masks and controlled Environment lighting in-engine.

Useful technique references:

- GodotCon 2025: Preparing Anime-Style 3D Characters for Godot Using Blender
- GodotCon 2025: Making Stylized 3D Games In Godot
- Godot 4 Environment and Post-Processing documentation

These are technique references, not art assets.

## World identity

Working visual description:

> A compact demon-world ranch built from timber, warm plaster and dark volcanic stone, surrounded by vivid grass and cultivated pasture. Most structures look practical and lived-in. Small Makai details — unusual roof ornaments, softly glowing crystals/lanterns, subtly unnatural plant colors and a distant magical skyline — establish the setting without reducing readability.

The ranch should feel maintained but imperfect:

- repaired fence sections;
- stacked feed sacks;
- carts and crates;
- wood piles;
- tool racks;
- barrels;
- laundry/cloth where appropriate;
- workshop scraps;
- signposts;
- water troughs;
- worn path edges.

Avoid random clutter. Props should tell the player what happens in each zone.

## Top-down spatial composition

Keep the current source-grounded functional structure:

```text
                    NORTH

              ┌─────────────────┐
              │     PASTURE     │
              │ fenced + gate   │
              └────────┬────────┘
                       │
                     WELL
                       │
     WORKSHOP ─── CENTRAL HUB ─── DAIRY BARN
        │          office +           │
      STORAGE     private room    PHARMACY LAB
        │              │
      KITCHEN     EVENT CLEARING
        │              │
        └──── DORMITORY CLUSTER ────┘
                       │
                  SOUTH GATE
                    SPAWN
```

The first playable implementation does not need every building finished, but paths and empty construction pads should reserve their final locations.

## Navigation hierarchy

Three route levels:

### Primary route

South Gate → Central Hub → Well → Pasture.

Use the broadest path and strongest visual guidance.

Suggested usable width: 4–5 m.

### Production loop

Hub → Workshop → Storage → Kitchen → south/east return.

Hub → Dairy Barn → Pharmacy Lab → event-space return.

Suggested usable width: 3.5–4.5 m.

### Local paths

Short building approaches, garden paths and staff-only-looking decorative branches.

Suggested usable width: 2–3 m, but never make required player/NPC movement unnecessarily narrow.

## Central hub

This should become the strongest ranch landmark.

Composition:

- office building facing south/south-east;
- porch or small covered entrance;
- notice board / work schedule board;
- central tree, lantern pole, well-designed sign or equivalent focal prop;
- open circular/rounded plaza;
- route sightlines toward pasture, dairy barn, workshop and south gate.

Gameplay purposes:

- orientation;
- management access;
- meeting NPCs;
- event gathering;
- natural camera establishing shot.

Do not place dense props in the center of the traversal lane.

## South gate / first impression

Player spawn should immediately communicate where to go.

From the initial third-person camera the player should see:

1. a clear path;
2. the central hub landmark;
3. at least two side-area silhouettes;
4. enough sky/terrain depth to avoid feeling like an enclosed test arena.

Use fences and vegetation to frame the view rather than invisible walls.

## Pasture

Largest open area.

Visual language:

- broad green field;
- strong timber fence silhouette;
- one obvious south-facing gate;
- trough/water/feed landmarks;
- sparse trees at perimeter;
- subtle terrain height change if navigation remains reliable.

Keep central pasture space open enough for NPC/animal simulation later.

## Dairy barn

Should look more substantial than generic storage.

Visual identifiers:

- wider doors;
- covered work side;
- feed/storage props;
- darker timber structural frame;
- roof vent/cupola or another original landmark shape.

The interactable pad should be visible but eventually represented by an actual work object rather than a glowing cube.

## Workshop

Visual identifiers:

- open/half-open work area;
- chimney/vent;
- stacked timber/metal;
- workbench silhouette;
- warmer practical task lighting at evening.

## Kitchen

Connect visually to the living cluster.

Visual identifiers:

- chimney;
- herb/vegetable planters;
- delivery/service entrance;
- warm window light at evening/night.

## Pharmacy lab

More precise and slightly magical, without becoming a science-fiction building.

Visual identifiers:

- cleaner plaster/stone surfaces;
- glass/plant silhouettes;
- restrained colored lantern/crystal accent;
- small herb garden or drying rack.

## Storage

Simple low silhouette and strong prop grouping.

Use it as a transition landmark rather than a hero building.

## Well

Important waypoint.

It should be visible from hub and pasture route.

Potential visual treatment:

- dark stone circular base;
- timber canopy;
- water bucket;
- subtle Makai rune/metal accent;
- nearby small vegetation patch.

Do not make it so large that it blocks the main route.

## Event clearing

Keep approximately 12 m × 12 m.

Requirements:

- minimal permanent clutter;
- multiple camera directions;
- enough room for several NPC stand-ins/characters;
- nearby landmark for orientation;
- lighting can be overridden for story events later.

## Living cluster

Guest room, dormitory and barn/living support should form a coherent secondary courtyard.

This area should feel quieter and more domestic than the production ring.

Possible props:

- benches;
- clothesline;
- small garden;
- lanterns;
- stacked firewood;
- personal storage;
- sheltered seating.

Do not invent gameplay functions that are not supported by the simulation.

## Modular environment kit

Create reusable Blender/Godot modules before unique hero buildings.

### Structure

- 2 m / 4 m wall sections;
- plaster wall;
- timber frame wall;
- stone base;
- interior wall;
- wood floor;
- simple ceiling;
- roof straight;
- roof corner;
- roof ridge;
- awning;
- porch;
- stairs 1 m / 2 m;
- doorway;
- window variants.

### Ranch boundaries

- straight fence;
- fence corner;
- fence gate;
- short stone retaining edge;
- hedge/vegetation boundary.

### Paths

- main dirt path;
- compact dirt/stone blend;
- plaza material;
- stepping-stone/local path;
- worn grass edge decals/masks.

### Props

- crate;
- barrel;
- feed sack;
- tool rack;
- bench;
- lantern;
- sign;
- cart;
- trough;
- wood pile;
- bucket;
- shelves;
- workbench.

### Vegetation

- 3–5 tree silhouettes;
- 3 shrub groups;
- grass clumps;
- flowers;
- herb/crop groups.

Prefer a small coherent kit with variation over dozens of unrelated generated assets.

## Provisional palette

These are starting points for concept iteration, not immutable canon:

| Use | Hex | Intent |
|---|---|---|
| grass mid | `#6FAE61` | vivid but not neon |
| grass shadow | `#3F7452` | cooler depth |
| main dirt path | `#C69A68` | warm navigation guide |
| plaster | `#E8D8B5` | warm neutral building mass |
| timber | `#6B432F` | structural contrast |
| dark stone | `#3C424B` | Makai grounding |
| roof warm | `#A95645` | primary ranch roof family |
| roof cool accent | `#466B78` | selective secondary buildings |
| magical accent | `#79C7C5` | restrained supernatural highlight |
| evening lamp | `#F5C06A` | warm interaction/light landmark |

Do not assign a different saturated color to every building.

## Lighting targets

### Morning

- cool-to-neutral ambient;
- warm low-angle directional light;
- long but soft readable shadows;
- slight morning haze only if it does not flatten silhouettes.

### Afternoon

- highest clarity;
- neutral/warm sun;
- strongest color readability;
- benchmark for material authoring.

### Evening

- warm orange/gold directional light;
- cooler ambient/shadow family;
- windows/lanterns begin to carry navigation cues.

### Night

- no direct daytime sun;
- cool ambient;
- warm local lamps at entrances/routes;
- silhouettes and interaction targets must remain readable.

All lighting derives from the existing `CalendarState.Phase` through `DaylightRig`. Do not create a second time clock.

## World HUD relationship

The world HUD should remain visually lighter than the management shell.

Recommended hierarchy:

- top-left: day / season / phase / weather;
- top-right: economy + roster at a glance;
- small worker context panel;
- center-bottom contextual interaction prompt;
- short transient result/error text;
- Management button/key hint.

Avoid permanently covering large portions of the 3D world.

## Concept-art/reference shots to generate later

Generate these as separate images to reduce visual drift.

### CONCEPT-WORLD-01 — South gate daytime

Camera:
- third-person gameplay height;
- 35–50 mm equivalent feel;
- looking north from just inside south gate.

Must show:
- path to hub;
- hub landmark;
- side-route hints;
- stylized ranch palette;
- clear scale.

Prompt intent:

> Modern stylized 3D anime ranch environment, demon-world countryside but welcoming, third-person game camera from the south entrance, strong central timber-and-warm-plaster ranch office landmark, vivid grouped grass and shrubs, broad warm dirt path, readable production buildings in the distance, dark volcanic stone accents, subtle turquoise magical lantern details, clean cel-shaded material language, broad simple shapes, no characters, no text, production concept art for a playable game environment, practical navigation and believable building scale.

### CONCEPT-WORLD-02 — Central hub evening

Must show:
- office/hub plaza;
- routes to workshop/dairy/pasture;
- warm local light;
- cool shadows;
- enough empty movement space.

### CONCEPT-WORLD-03 — Pasture + dairy landmark

Must show:
- pasture gate;
- well route;
- dairy barn silhouette;
- fence readability;
- world scale.

### CONCEPT-WORLD-04 — Modular kit sheet

Separate clean objects on neutral background:

- wall;
- roof;
- fence;
- gate;
- sign;
- lantern;
- crate;
- barrel;
- bench;
- trough;
- tree/shrub families.

Use as modeling reference, not as a texture atlas.

## Reference-image rules

For generated concepts:

1. select one approved world identity/master image;
2. subsequent shots should explicitly preserve architecture, palette, material language and scale;
3. change only camera/time/location when possible;
4. reject inconsistent roof/material redesigns;
5. do not model directly from a single perspective image without a top-down/floorplan check;
6. concept art may never override navigation requirements.

## Implementation order

1. Validate current WORLD-003c scene locally.
2. Replace greybox floor with readable grass/path/plaza materials.
3. Add route meshes matching the floor plan.
4. Add construction-pad/placeholder masses for all source facilities.
5. Build the modular fence/path/structure kit.
6. Finish central hub exterior first.
7. Finish pasture + well + dairy route.
8. Finish workshop/storage/kitchen route.
9. Finish pharmacy + living cluster.
10. Add grouped vegetation and practical props.
11. Tune Day/Afternoon/Evening/Night.
12. Capture real Godot screenshots and compare with approved concepts.
13. Only then increase detail density.

## Acceptance criteria for the first art pass

- player can identify the hub from spawn without HUD arrows;
- every required route is visually distinct and at least 4 m clear where specified;
- six currently implemented world stations have recognizable environment landmarks;
- remaining source facilities have reserved placement;
- no critical camera collision traps;
- day and evening both remain readable;
- the world looks coherent from gameplay camera, not only from a beauty-shot angle;
- no copied commercial assets or untracked external assets;
- all imported external assets have explicit compatible license/provenance.
