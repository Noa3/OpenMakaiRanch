using Godot;

namespace OpenMakaiRanch.App;

/// <summary>
/// Minimal bootstrap scene controller. It validates the critical playable scene graph once,
/// then routes into the main menu after autoload initialization has completed.
/// </summary>
public partial class BootstrapController : Control
{
	public override void _Ready()
	{
		// Defer scene swap so autoloads and this root are fully initialized first.
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
