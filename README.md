# OpenMakaiRanch

OpenMakaiRanch is a remake effort for eraMakaiRanch using Godot .NET.

## Project Target

The main target is to rebuild the gameplay framework from `eraMakaiRanch-game-eng-translation` inside `OpenMakaiRanchGame` with:

- A modern, maintainable Godot .NET codebase
- Typed C# domain models and services
- A playable vertical slice first, then iterative feature parity
- Safe public repository standards for code and content

## Content Scope

The existing simulation includes mature-related state/services, alongside ranch management, relationships and adventures. Full original-game parity is **not verified**. The 3D migration is additive and begins with non-explicit gameplay. Adult-specific presentation requires identity and visual-age validation; see `OpenMakaiRanchGame/docs/ADULT_CHARACTER_VALIDATION.md`.

Current evidence and continuity: [project state](OpenMakaiRanchGame/docs/CURRENT_PROJECT_STATE.md), [3D migration plan](OpenMakaiRanchGame/docs/3D_REMAKE_PLAN.md), [handoff](OpenMakaiRanchGame/docs/ASTRA_HANDOFF.md).

## Repository Layout

- `OpenMakaiRanchGame/` - Godot .NET game project (main remake target)
- `eraMakaiRanch-game-eng-translation/` - Legacy reference material
- `GodotSharp/` - Local Godot .NET SDK package source
- `mcp-server/` - Supporting MCP server project

## Local Build (Windows)

From the repository root:

```powershell
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
python Tools/Godot/launch.py --mode check
python Tools/Godot/launch.py --mode smoke
```

## Open In Godot (Shortcut)

Use the launcher script from repo root:

```powershell
.\OpenMakaiRanchGame-Editor.bat
```

The script opens the editor for `OpenMakaiRanchGame`, verifies stable Godot 4.7 .NET, and prints the selected executable. Discovery supports explicit `GODOT_BIN`/`GODOT_PATH`, `GODOT_HOME`, repository-relative installs, bounded installed locations and PATH. See [tooling](Tools/Godot/README.md) for isolated tests and verified Universal MCP access. No engine upgrade is performed.

## Large Editor Binary Strategy

Better way than committing large binaries directly to Git history:

- Use Git LFS for local pinned editor binaries.
- Prefer installing Godot through a package manager for most contributors (for example, `winget install GodotEngine.GodotMono`).
- Keep a launcher script in the repo so both approaches work consistently.

Initialize LFS after cloning:

```powershell
git lfs install
git lfs pull
```

This repository tracks `Godot_v*_mono_win64*.exe` through Git LFS.

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
