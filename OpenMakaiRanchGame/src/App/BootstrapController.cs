using Godot;

namespace OpenMakaiRanch.App;

/// <summary>
/// Minimal bootstrap scene controller. It validates the critical playable scene graph once,
/// applies account-wide input preferences, then routes into the main menu.
/// </summary>
public partial class BootstrapController : Control
{
	public override void _Ready()
	{
		InputBindingService.EnsureApplied();
		CallDeferred(nameof(RouteToMainMenu));
	}

	private void RouteToMainMenu()
	{
		var missingScenes = GameRouteCatalog.MissingCriticalScenes();
		if (missingScenes.Count > 0)
		{
			foreach (var path in missingScenes)
			{
				GD.PushError($"Bootstrap route validation failed: required scene '{path}' does not exist.");
			}
			return;
		}

		var error = GetTree().ChangeSceneToFile(GameRouteCatalog.MainMenu);
		if (error != Error.Ok)
		{
			GD.PushError($"Bootstrap failed to open MainMenu scene: {error}");
		}
	}
}
