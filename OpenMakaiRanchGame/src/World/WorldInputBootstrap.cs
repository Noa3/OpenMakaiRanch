using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// Idempotently registers the world InputMap actions (keyboard + gamepad) on scene ready.
/// Keeping the mapping in one place avoids scattering AddAction calls and lets the
/// smoke test verify the exact action set. The 3D_REMAKE_PLAN requires "keyboard plus
/// controller mappings" for movement and camera.
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
        EnsureAction("move_forward");
        EnsureAction("move_backward");
        EnsureAction("move_left");
        EnsureAction("move_right");
        EnsureAction("interact");
        EnsureAction("camera_look_up");
        EnsureAction("camera_look_down");
        EnsureAction("camera_look_left");
        EnsureAction("camera_look_right");
        EnsureAction("camera_zoom_in");
        EnsureAction("camera_zoom_out");
        EnsureAction("camera_recenter");

        foreach (var (action, _, keycode) in KeyboardMappings)
        {
            AddKeyIfAbsent(action, keycode);
        }

        // Interact on F / Space.
        AddKeyIfAbsent("interact", (int)Key.F);
        AddKeyIfAbsent("interact", (int)Key.Space);
        // Recenter on R.
        AddKeyIfAbsent("camera_recenter", (int)Key.R);
    }

    private static void EnsureAction(string action)
    {
        if (!InputMap.HasAction(action))
        {
            InputMap.AddAction(action);
        }
    }

    private static void AddKeyIfAbsent(string action, int keycode)
    {
        var existing = InputMap.ActionGetEvents(action);
        foreach (var ev in existing)
        {
            if (ev is InputEventKey k && k.Keycode == (Key)keycode)
            {
                return;
            }
        }

        var keyEvent = new InputEventKey
        {
            Keycode = (Key)keycode,
            Pressed = true
        };
        InputMap.ActionAddEvent(action, keyEvent);
    }
}
