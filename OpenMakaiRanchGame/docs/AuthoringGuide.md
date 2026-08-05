# OpenMakaiRanch Content Authoring Guide

This document provides a quick reference for contributors who want to add or modify game content such as characters, items, missions, jobs, facilities, research nodes, pets, and bond events.

## 1. Resource Types

| Resource | File Extension | Typical Location |
|----------|----------------|------------------|
| Character Database | `.tres` | `resources/CharacterDatabase.tres` |
| Job Database | `.tres` | `resources/JobDatabase.tres` |
| Item Database | `.tres` | `resources/ItemDatabase.tres` |
| Mission Database | `.tres` | `resources/MissionDatabase.tres` |
| Facility Database | `.tres` | `resources/FacilityDatabase.tres` |
| Milestone Database | `.tres` | `resources/MilestoneDatabase.tres` |
| Research Database | `.tres` | `resources/ResearchDatabase.tres` |
| Pet Database | `.tres` | `resources/PetDatabase.tres` |
| Bond Event Database | `.tres` | `resources/BondEventDatabase.tres` |

All databases are Godot `Resource` files that can be edited directly in the Godot editor. Open the file, add a new entry, and ensure the `id` field is unique and uses lower‑case ASCII characters.

## 2. Naming Conventions

* **IDs** – lower‑case, snake_case, e.g., `character_john_doe`.
* **Display Names** – human‑readable, capitalised, e.g., `John Doe`.
* **Asset Files** – match the ID where possible, e.g., `portrait_john_doe.png`.

## 3. Adding a New Character

1. Open `resources/CharacterDatabase.tres` in Godot.
2. Click **Add Resource → Character** (or duplicate an existing entry).
3. Fill required fields: `id`, `display_name`, `race`, `base_stats`, `starting_job`.
4. Save the file.
5. Add portrait images under `assets/portraits/` using the naming convention.
6. Run the **ContentValidator** tool (`dotnet run --project Tools/ContentValidator.csproj`) to ensure the ID is unique.

## 4. Adding a New Item

Follow the same steps as characters but edit `ItemDatabase.tres`. Define `category`, `price`, `effects` and any required `icon` asset.

## 5. Updating References

When you add a new ID, other resources may need to reference it (e.g., a job may list allowed character IDs). Use the **Find in Files** search in VS Code to locate existing references and update them accordingly.

## 6. Validation

The CI pipeline runs `dotnet run --project Tools/ContentValidator.cs` which checks for duplicate IDs and missing image files. Ensure the build passes locally before pushing.

## 7. Private‑Extension Content

If the content is adult‑only or otherwise private, place the resource in the `private/` folder and add a feature flag in `project.godot` to exclude it from public builds.

---

For more detailed guidelines, see the full design document in `docs/DesignSpecification.md` (to be created).