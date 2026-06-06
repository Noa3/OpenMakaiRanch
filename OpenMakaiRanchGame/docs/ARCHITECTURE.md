# OpenMakaiRanchGame Architecture

## Scene Flow

The startup flow is now split into focused scenes:

1. `scenes/Bootstrap.tscn`
- Script: `src/App/BootstrapController.cs`
- Responsibility: project startup handoff and scene routing bootstrap.

2. `scenes/MainMenu.tscn`
- Script: `src/App/MainMenuController.cs`
- Responsibility: create a new game, continue slot 1, or quit.
- UI authoring: buttons, labels, and layout are scene nodes (GameObjects) edited directly in `MainMenu.tscn`; controller handles bindings only.

3. `scenes/Game.tscn`
- Script: `src/App/GameSceneController.cs`
- Hosts `UiShellController` and enters gameplay screens.
- UI authoring: top bar, status labels, day/phase/gold chips, end-day button, navigation menu, scroll container, and content root are scene nodes under `UiShell`.

## Core Autoloads

- `GameRoot` (`src/App/GameRoot.cs`): game state and service composition.
- `SceneRouter` (`src/App/SceneRouter.cs`): screen-level routing for `UiShellController`.

## Gameplay Service Layout

Gameplay services are now grouped across files:

- `src/Gameplay/GameServices.cs`
- Focuses on save/state factory, migration, roster/schedule/ranch/economy, and day settlement.

- `src/Gameplay/ManagementServices.cs`
- Focuses on inventory/shop, combat/adventure, milestones, bond/recruitment, research, and pets.

## UI Layout

`UiShellController` is split into partial classes:

- `src/Ui/UiShellController.cs`
- Scene-node binding, screen lifecycle, dynamic gameplay content rendering, and user action orchestration.

- `src/Ui/UiShellController.Styling.cs`
- Shared UI styling, portrait/layer composition, and reusable helper controls.

## Scene-First UI Convention

- Prefer scene-authored node trees for stable menus and shell layout.
- Keep scripts focused on behavior and event binding (`Pressed`, state refresh, scene changes).
- Data-driven gameplay lists can still render dynamically inside the scene-owned content container.
- Smoke tests load `MainMenu.tscn` and `Game.tscn` to verify the authored UI nodes still exist after scene edits.
- Current state:
	- Main menu is scene-authored (`scenes/MainMenu.tscn`).
	- Gameplay shell layout and navigation are scene-authored (`scenes/Game.tscn`).
	- Individual gameplay screen contents remain dynamic because they depend on roster, inventory, missions, and save state.

## Notes

- Existing behavior is preserved while reducing file size and coupling.
- `Main.tscn` remains in the repository for compatibility during transition.
