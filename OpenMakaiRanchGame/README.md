# Open Makai Ranch Game

This is the Godot .NET remake scaffold for the local eraMakaiRanch project.

Project target: recreate the original systems in a maintainable Godot .NET architecture under `OpenMakaiRanchGame`, starting with a strong playable systems core and iterating toward deeper parity.

The current milestone is a SFW systems-first rebuild: full gameplay pillars are represented by typed services and screens, while the playable slice focuses on roster, schedules, ranch production, daily settlement, simple shop/adventure hooks, and typed save/load.

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
- Public mainline content is kept content-safe. Adult-only original systems are not recreated explicitly in this repository; they are replaced with SFW bond, mentorship, ranch work, and adventure hooks.
- If mature customizations are ever developed, keep them as private extensions and ensure they follow local law and platform policy.
- `net10.0` is used because this machine currently has only .NET SDK 10 and the .NET 10 reference pack installed. Retarget to `net8.0` after installing the .NET 8 SDK if strict Godot LTS targeting is needed.

## Binary Storage Note

Large editor binaries are tracked with Git LFS to avoid hitting normal GitHub file-size limits.

After cloning, run:

```powershell
git lfs install
git lfs pull
```
