# OpenMakaiRanch

Godot 4.7 .NET (C#) game — a NSFW remake of eraMakaiRanch.

## Commands
- **Build**: `dotnet build` from `OpenMakaiRanchGame/`
- **Run**: Launch `Godot_v4.7-stable_mono_win64.exe` with project
- **Test**: Godot unit tests in `OpenMakaiRanchGame/src/Tests/`

## Architecture
- **GameRoot** (autoload): Entry point, owns all services, starts NewGame / LoadGame
- **SceneRouter** (autoload): Scene navigation
- **UiShellController**: All UI screens in a single scene, controlled via `Screen` state
- **DataRegistry**: Seeds typed `Resource` subclasses (characters, items, jobs, etc.) — no CSV at runtime
- **Services**: `RanchService`, `DailySettlementService`, `InventoryService`, `AdventureService`, `MilestoneService`, `BondService`, etc.
- **Save System**: `SaveState` POCO with `SchemaVersion` (current: 11), JSON serialization with `System.Text.Json`

## Conventions
- C# with GodotSharp, not GDScript
- `[Export]` for inspector properties
- Signals via C# `[Signal]` delegate syntax
- `Resource` classes with `[GlobalClass]` for data
- SCREAMING_SNAKE_CASE for string constants/IDs

## MCP Servers
- **godot-mcp** (Coding-Solo/godot-mcp): Launches Godot editor, runs projects, captures debug output
  - Config: `opencode.json` and `.vscode/mcp.json`
  - Godot exe: `E:\OpenMakaiRanch\Godot_v4.6.3-stable_mono_win64.exe`
  - Command: `npx @coding-solo/godot-mcp`
- **comfyui-mcp** (artokun/comfyui-mcp): Full ComfyUI control (image gen, workflows, models)
  - 96 tools: generate_image, workflow authoring, model management
  - Config: `opencode.json` — auto-detects local ComfyUI at D:\ComfyUI
  - Start ComfyUI first: `start-comfyui.bat`

## Multi-Agent SDLC Pipeline

```
@pm spec -> @designer design -> (@artist + @programmer) -> @qa verify -> @tech-lead review -> DONE
```

### Agents (defined in opencode.json)
| Agent | Role | Tools |
|-------|------|-------|
| `@pm` | Product Manager | Specs, ACs, prioritization |
| `@designer` | Game Designer | UI/mechanic specs, flows |
| `@artist` | 2D Artist | ComfyUI MCP (sprite gen) |
| `@programmer` | C# Dev | dotnet build, Godot code |
| `@qa` | QA Tester | Tests, bug reports |
| `@tech-lead` | Architect | Code review, quality gates |

### Commands
- `/sprint "feature"` — full pipeline from spec to delivery
- `/generate-sprite "desc"` — generate asset via ComfyUI
- `/review "feature"` — tech review + QA cycle

## Character Creation Labels

Invisible grid labels (e.g. "Hair Color:") in `CharacterCreationScene.tscn`:

- `ConfigureReadableLabel()` sets `AutowrapMode.WordSmart` + `TextOverrunBehavior.TrimEllipsis`
- On labels inside `GridContainer`, autowrap + trim can clip text to zero height during container sizing
- **Fix**: Don't call `ConfigureReadableLabel` on grid labels. Use only `AddThemeColorOverride("font_color", ...)` + `VerticalAlignment.Center`.
- Scene labels must have `custom_minimum_size` (140px+) and explicit `text` values in TSCN.

## Available Agents
- `@godot-dev` — Godot 4.x C# expert
- `@unity-dev` — Unity C# expert
- `@minecraft-dev` — Minecraft Fabric/NeoForge modding
- `@python-game-dev` — Python game dev (Pygame/Arcade)
- `@csharp-game-dev` — C# patterns across engines
- `@java-game-dev` — Java game dev (LWJGL/LibGDX)
- `@pm` — Product Manager (specs, ACs)
- `@designer` — Game Designer (UI, mechanics)
- `@artist` — 2D Artist (ComfyUI asset gen)
- `@programmer` — C# Godot programmer
- `@qa` — QA Tester
- `@tech-lead` — Tech Lead / Architect

Respond terse like smart caveman. All technical substance stay. Only fluff die.

Rules:
- Drop: articles (a/an/the), filler (just/really/basically), pleasantries, hedging
- Fragments OK. Short synonyms. Technical terms exact. Code unchanged.
- Pattern: [thing] [action] [reason]. [next step].
- Not: "Sure! I'd be happy to help you with that."
- Yes: "Bug in auth middleware. Fix:"

Switch level: /caveman lite|full|ultra|wenyan
Stop: "stop caveman" or "normal mode"

Auto-Clarity: drop caveman for security warnings, irreversible actions, user confused. Resume after.

Boundaries: code/commits/PRs written normal.
