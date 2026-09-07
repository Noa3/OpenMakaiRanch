using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// Idempotently registers the world InputMap actions (keyboard + gamepad buttons) on scene ready.
/// Analog stick input (left stick = movement, right stick = camera look) is read directly
/// via <see cref="Input.GetJoyAxis"/> in the consumers, which is the canonical Godot pattern
/// for directional stick input. This keeps the mapping in one place and lets the smoke test
/// verify the exact action set. The 3D_REMAKE_PLAN requires "keyboard plus controller mappings".
/// </summary>
public partial class WorldInputBootstrap : Node
{
    public static readonly (string action, int keycode)[] KeyboardMappings =
    {
        ("move_forward",  (int)Key.Up),
        ("move_forward",  (int)Key.W),
        ("move_backward", (int)Key.Down),
        ("move_backward", (int)Key.S),
        ("move_left",     (int)Key.Left),
        ("move_left",     (int)Key.A),
        ("move_right",    (int)Key.Right),
        ("move_right",    (int)Key.D),
        ("interact",      (int)Key.F),
        ("interact",      (int)Key.Space),
        ("camera_recenter", (int)Key.R),
    };

    /// <summary>
    /// Discrete gamepad button mappings (InputMap events).
    /// A/Start = interact, B = recenter, L1 = zoom in, R1 = zoom out.
    /// </summary>
    public static readonly (string action, JoyButton button)[] JoypadButtonMappings =
    {
        ("interact",        JoyButton.A),
        ("interact",        JoyButton.Start),
        ("camera_recenter", JoyButton.B),
        ("camera_zoom_in",  JoyButton.LeftShoulder),
        ("camera_zoom_out", JoyButton.RightShoulder),
    };

    public override void _Ready()
    {
        EnsureAction("move_forward");
        EnsureAction("move_backward");
        EnsureAction("move_left");
        EnsureAction("move_right");
        EnsureAction("interact");
        EnsureAction("camera_zoom_in");
        EnsureAction("camera_zoom_out");
        EnsureAction("camera_recenter");

        foreach (var (action, keycode) in KeyboardMappings)
        {
            AddKeyIfAbsent(action, keycode);
        }

        foreach (var (action, button) in JoypadButtonMappings)
        {
            AddJoypadButtonIfAbsent(action, button);
        }
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

        InputMap.ActionAddEvent(action, new InputEventKey { Keycode = (Key)keycode, Pressed = true });
    }

    private static void AddJoypadButtonIfAbsent(string action, JoyButton button)
    {
        var existing = InputMap.ActionGetEvents(action);
        foreach (var ev in existing)
        {
            if (ev is InputEventJoypadButton b && b.ButtonIndex == button)
            {
                return;
            }
        }

        InputMap.ActionAddEvent(action, new InputEventJoypadButton { ButtonIndex = button, Pressed = true });
    }
}
