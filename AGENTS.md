# OpenMakaiRanch

Godot 4.6 .NET (C#) game — a SFW remake of eraMakaiRanch.

## Commands
- **Build**: `dotnet build` from `OpenMakaiRanchGame/`
- **Run**: Launch `Godot_v4.6.3-stable_mono_win64.exe` with project
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

## Character Creation Labels

Invisible grid labels (e.g. "Hair Color:") in `CharacterCreationScene.tscn`:

- `ConfigureReadableLabel()` sets `AutowrapMode.WordSmart` + `TextOverrunBehavior.TrimEllipsis`
- On labels inside `GridContainer`, autowrap + trim can clip text to zero height during container sizing
- **Fix**: Don't call `ConfigureReadableLabel` on grid labels. Use only `AddThemeColorOverride("font_color", ...)` + `VerticalAlignment.Center`.
- Scene labels must have `custom_minimum_size` (140px+) and explicit `text` values in TSCN.

## Available Agents
- `@godot-dev` — Godot Godot 4.x C# expert
- `@unity-dev` — Unity C# expert
- `@minecraft-dev` — Minecraft Fabric/Forge modding
- `@python-game-dev` — Python game dev (Pygame/Arcade)
- `@csharp-game-dev` — C# patterns across engines
- `@java-game-dev` — Java game dev (LWJGL/LibGDX)

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
