using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Applies shared account-wide keyboard/controller bindings when a playable world is ready.
/// KeyboardMappings remains public for smoke-test/backwards compatibility.
/// </summary>
public partial class WorldInputBootstrap : Node
{
    public static readonly (string action, string key, int keycode)[] KeyboardMappings =
    {
        ("move_forward", "Up", (int)Key.Up),
        ("move_forward", "W", (int)Key.W),
        ("move_backward", "Down", (int)Key.Down),
        ("move_backward", "S", (int)Key.S),
        ("move_left", "Left", (int)Key.Left),
        ("move_left", "A", (int)Key.A),
        ("move_right", "Right", (int)Key.Right),
        ("move_right", "D", (int)Key.D),
    };

    public override void _Ready()
    {
        InputBindingService.EnsureApplied();
    }
}
