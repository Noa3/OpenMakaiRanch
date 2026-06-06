# Contributing to OpenMakaiRanch

Thank you for contributing.

## Development Focus

- Keep changes scoped and testable.
- Preserve typed C# architecture in `OpenMakaiRanchGame`.
- Prefer small PRs with clear intent.

## Setup

1. Install .NET SDK 10 (or matching project target).
2. Build project:

```powershell
dotnet build OpenMakaiRanchGame/OpenMakaiRanchGame.csproj
```

3. Run headless smoke launch:

```powershell
.\Godot_v4.6.3-stable_mono_win64_console.exe --headless --path OpenMakaiRanchGame --quit-after 5
```

## Branch and PR Rules

- Create a feature branch from `main`.
- Use descriptive commit messages.
- Link an issue if applicable.
- Include test or verification notes in the PR.

## Code Style

- Use clear names and small methods.
- Avoid hidden global side effects.
- Add concise comments only where logic is non-obvious.

## Content Policy

This is a public repository. Contributions must remain content-safe.

- Do not submit explicit adult material.
- Do not submit illegal or exploitative content.
- Keep mature or experimental private modifications out of this public mainline.

## Pull Request Checklist

- [ ] Builds locally
- [ ] No unrelated file changes
- [ ] Updated docs if behavior changed
- [ ] Added or updated tests where practical
