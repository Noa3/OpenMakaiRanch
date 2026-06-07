# Open Makai Ranch Game

This is the Godot .NET remake scaffold for the local eraMakaiRanch project.

Project target: recreate the original systems in a maintainable Godot .NET architecture under `OpenMakaiRanchGame`, starting with a strong playable systems core and iterating toward deeper parity.

The current milestone is a NSFW systems-first rebuild: full gameplay pillars are represented by typed services and screens, including mature content systems (mental state, training, milk economy, addiction, clothing) alongside roster, schedules, ranch production, daily settlement, shop/adventure hooks, and typed save/load. Characters are randomly generated from the full data pools defined in the original eraMakaiRanch CSV files.

## Local Build

The repository includes a local Godot .NET SDK package under `../GodotSharp/Tools/nupkgs` and the project is configured to use it through `NuGet.config`.

```powershell
dotnet build OpenMakaiRanchGame.csproj
..\Godot_v4.6.3-stable_mono_win64_console.exe --headless --path . --build-solutions --quit
..\Godot_v4.6.3-stable_mono_win64_console.exe --headless --path . --quit-after 3
```

## Open Editor Quickly

From repository root:

```powershell
.\OpenMakaiRanchGame-Editor.bat
```

This launcher opens the project with the local Godot editor binary if present, or falls back to `godot` on PATH.

## Scope Notes

- Runtime gameplay does not depend on copied CSV files.
- Adult-only original systems are recreated from the eraMakaiRanch CSV data: training (170+ actions), mental state/corruption, breast milk economy, addiction, clothing/equipment, and relationship fall states. Content can be toggled via the ContentMode setting.
- Characters are randomly generated from the full data pools defined in the original CSV files (names, talents, traits, body types, races, jobs, personalities).
- `net10.0` is used because this machine currently has only .NET SDK 10 and the .NET 10 reference pack installed. Retarget to `net8.0` after installing the .NET 8 SDK if strict Godot LTS targeting is needed.

## Binary Storage Note

Large editor binaries are tracked with Git LFS to avoid hitting normal GitHub file-size limits.

After cloning, run:

```powershell
git lfs install
git lfs pull
```

## Roadmap

- Architecture reference: `docs/ARCHITECTURE.md`
- Missing-work backlog: `docs/REMAKE_TODO.md`
