# OpenMakaiRanch

OpenMakaiRanch is a remake effort for eraMakaiRanch using Godot .NET.

## Project Target

The main target is to rebuild the gameplay framework from `eraMakaiRanch-game-eng-translation` inside `OpenMakaiRanchGame` with:

- A modern, maintainable Godot .NET codebase
- Typed C# domain models and services
- A playable vertical slice first, then iterative feature parity
- Safe public repository standards for code and content

## Content Scope And Boundaries

This repository's public core stays content-safe.

- The default implementation is systems-first and safe-for-work.
- Any mature customization must stay out of this public mainline.
- If private extensions are created, they should follow local law, platform policy, and project contributor rules.

## Repository Layout

- `OpenMakaiRanchGame/` - Godot .NET game project (main remake target)
- `eraMakaiRanch-game-eng-translation/` - Legacy reference material
- `GodotSharp/` - Local Godot .NET SDK package source
- `mcp-server/` - Supporting MCP server project

## Local Build (Windows)

From the repository root:

```powershell
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
.\Godot_v4.6.3-stable_mono_win64_console.exe --headless --path OpenMakaiRanchGame --build-solutions --quit
.\Godot_v4.6.3-stable_mono_win64_console.exe --headless --path OpenMakaiRanchGame --quit-after 5
```

## Roadmap (Starter)

- [x] Godot .NET scaffold and typed service architecture
- [x] Baseline playable loop shell and save/load foundation
- [ ] Expand system depth toward feature parity
- [ ] Add automated test and CI coverage
- [ ] Prepare first public milestone release

## Contributing

Please read `CONTRIBUTING.md` before opening pull requests.

## Security

Please report security issues using `SECURITY.md`.
