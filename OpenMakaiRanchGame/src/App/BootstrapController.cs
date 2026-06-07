using Godot;

namespace OpenMakaiRanch.App;

/// <summary>
/// Minimal bootstrap scene controller. It exists to keep startup routing explicit and easy to evolve.
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
		var error = GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
		if (error != Error.Ok)
		{
			GD.PushError($"Bootstrap failed to open MainMenu scene: {error}");
		}
	}
}
