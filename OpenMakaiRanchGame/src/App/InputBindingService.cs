using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;

namespace OpenMakaiRanch.App;

/// <summary>
/// Account-wide input preferences for the real-time remake. Bindings are stored separately from
/// gameplay saves so one control scheme applies to every slot and NG+ run.
/// </summary>
public static class InputBindingService
{
    public const string PreferencesPath = "user://controls.json";

    public sealed record ActionDescriptor(
        string Action,
        string DisplayName,
        Key PrimaryKey,
        Key? AlternateKey = null,
        int GamepadButton = -1,
        int GamepadAxis = -1,
        float GamepadAxisValue = 0f);

    public sealed class ActionBindingPreference
    {
        public bool KeyboardCustomized { get; set; }
        public long KeyboardKeycode { get; set; }
        public bool GamepadCustomized { get; set; }
        public string GamepadKind { get; set; } = string.Empty;
        public int GamepadButton { get; set; } = -1;
        public int GamepadAxis { get; set; } = -1;
        public float GamepadAxisValue { get; set; }
    }

    public sealed class ControlPreferences
    {
        public bool InteractionHighlightEnabled { get; set; } = true;
        public Dictionary<string, ActionBindingPreference> Actions { get; set; } = new(StringComparer.Ordinal);
    }

    public static readonly IReadOnlyList<ActionDescriptor> ConfigurableActions = new[]
    {
        new ActionDescriptor("move_forward", "Move Forward", Key.W, Key.Up, GamepadAxis: 1, GamepadAxisValue: -1f),
        new ActionDescriptor("move_backward", "Move Backward", Key.S, Key.Down, GamepadAxis: 1, GamepadAxisValue: 1f),
        new ActionDescriptor("move_left", "Move Left", Key.A, Key.Left, GamepadAxis: 0, GamepadAxisValue: -1f),
        new ActionDescriptor("move_right", "Move Right", Key.D, Key.Right, GamepadAxis: 0, GamepadAxisValue: 1f),
        new ActionDescriptor("move_sprint", "Sprint", Key.Shift, GamepadButton: 7),
        new ActionDescriptor("interact", "Interact / Use", Key.F, Key.Space, GamepadButton: 0),
        new ActionDescriptor("camera_look_up", "Camera Up", Key.None, GamepadAxis: 3, GamepadAxisValue: -1f),
        new ActionDescriptor("camera_look_down", "Camera Down", Key.None, GamepadAxis: 3, GamepadAxisValue: 1f),
        new ActionDescriptor("camera_look_left", "Camera Left", Key.None, GamepadAxis: 2, GamepadAxisValue: -1f),
        new ActionDescriptor("camera_look_right", "Camera Right", Key.None, GamepadAxis: 2, GamepadAxisValue: 1f),
        new ActionDescriptor("camera_zoom_in", "Camera Zoom In", Key.None, GamepadButton: 9),
        new ActionDescriptor("camera_zoom_out", "Camera Zoom Out", Key.None, GamepadButton: 10),
        new ActionDescriptor("camera_recenter", "Recenter Camera", Key.R, GamepadButton: 8),
        new ActionDescriptor("camera_first_person", "Toggle First Person", Key.V, GamepadButton: 3),
        new ActionDescriptor("cycle_character", "Cycle Worker", Key.Tab, GamepadButton: 2),
        new ActionDescriptor("toggle_management", "Management", Key.M, GamepadButton: 4),
        new ActionDescriptor("open_help", "Help", Key.F1, GamepadButton: 11),
        new ActionDescriptor("pause_menu", "Pause Menu", Key.Escape, GamepadButton: 6),
    };

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static ControlPreferences _preferences = new();
    private static bool _loaded;

    public static bool InteractionHighlightEnabled
    {
        get
        {
            EnsureApplied();
            return _preferences.InteractionHighlightEnabled;
        }
    }

    public static void EnsureApplied(bool forceReload = false)
    {
        if (!_loaded || forceReload)
        {
            _preferences = Load();
            _loaded = true;
        }

        foreach (var descriptor in ConfigurableActions)
        {
            ApplyAction(descriptor);
        }

        EnsureUiControllerNavigation();
    }

    public static bool IsConfigurable(string action) =>
        ConfigurableActions.Any(value => string.Equals(value.Action, action, StringComparison.Ordinal));

    public static void SetInteractionHighlightEnabled(bool enabled)
    {
        EnsureApplied();
        if (_preferences.InteractionHighlightEnabled == enabled) return;
        _preferences.InteractionHighlightEnabled = enabled;
        Save();
    }

    public static void SetKeyboardBinding(string action, Key key)
    {
        EnsureApplied();
        if (key == Key.None || Find(action) is not { } descriptor) return;
        var preference = PreferenceFor(action);
        preference.KeyboardCustomized = true;
        preference.KeyboardKeycode = (long)key;
        ApplyAction(descriptor);
        Save();
    }

    public static void SetGamepadButtonBinding(string action, int button)
    {
        EnsureApplied();
        if (button < 0 || Find(action) is not { } descriptor) return;
        var preference = PreferenceFor(action);
        preference.GamepadCustomized = true;
        preference.GamepadKind = "button";
        preference.GamepadButton = button;
        preference.GamepadAxis = -1;
        preference.GamepadAxisValue = 0f;
        ApplyAction(descriptor);
        Save();
    }

    public static void SetGamepadAxisBinding(string action, int axis, float axisValue)
    {
        EnsureApplied();
        if (axis < 0 || Mathf.Abs(axisValue) < 0.5f || Find(action) is not { } descriptor) return;
        var preference = PreferenceFor(action);
        preference.GamepadCustomized = true;
        preference.GamepadKind = "axis";
        preference.GamepadButton = -1;
        preference.GamepadAxis = axis;
        preference.GamepadAxisValue = axisValue < 0f ? -1f : 1f;
        ApplyAction(descriptor);
        Save();
    }

    public static void ResetAction(string action)
    {
        EnsureApplied();
        if (Find(action) is not { } descriptor) return;
        _preferences.Actions.Remove(action);
        ApplyAction(descriptor);
        Save();
    }

    public static void ResetAllBindings()
    {
        EnsureApplied();
        var highlight = _preferences.InteractionHighlightEnabled;
        _preferences = new ControlPreferences { InteractionHighlightEnabled = highlight };
        foreach (var descriptor in ConfigurableActions) ApplyAction(descriptor);
        EnsureUiControllerNavigation();
        Save();
    }

    public static string GetKeyboardLabel(string action)
    {
        EnsureApplied();
        var descriptor = Find(action);
        if (descriptor is null) return "—";

        if (_preferences.Actions.TryGetValue(action, out var preference) && preference.KeyboardCustomized)
            return KeyLabel((Key)preference.KeyboardKeycode);

        if (descriptor.PrimaryKey == Key.None) return "—";
        var primary = KeyLabel(descriptor.PrimaryKey);
        return descriptor.AlternateKey is { } alternate && alternate != Key.None
            ? $"{primary} / {KeyLabel(alternate)}"
            : primary;
    }

    public static string GetGamepadLabel(string action)
    {
        EnsureApplied();
        var descriptor = Find(action);
        if (descriptor is null) return "—";

        if (_preferences.Actions.TryGetValue(action, out var preference) && preference.GamepadCustomized)
        {
            return preference.GamepadKind == "axis"
                ? AxisLabel(preference.GamepadAxis, preference.GamepadAxisValue)
                : ButtonLabel(preference.GamepadButton);
        }

        if (descriptor.GamepadAxis >= 0) return AxisLabel(descriptor.GamepadAxis, descriptor.GamepadAxisValue);
        return descriptor.GamepadButton >= 0 ? ButtonLabel(descriptor.GamepadButton) : "—";
    }

    public static string GetCombinedLabel(string action)
    {
        var keyboard = GetKeyboardLabel(action);
        var gamepad = GetGamepadLabel(action);
        if (keyboard == "—") return gamepad;
        if (gamepad == "—") return keyboard;
        return $"{keyboard} / {gamepad}";
    }

    public static string ConnectedControllerSummary()
    {
        var joypads = Input.GetConnectedJoypads();
        if (joypads.Count == 0) return "No controller detected";
        var names = new List<string>();
        foreach (var id in joypads)
        {
            var name = Input.GetJoyName(id);
            names.Add(string.IsNullOrWhiteSpace(name) ? $"Controller {id + 1}" : name);
        }
        return string.Join(", ", names);
    }

    private static void ApplyAction(ActionDescriptor descriptor)
    {
        EnsureAction(descriptor.Action);
        InputMap.ActionSetDeadzone(descriptor.Action, 0.22f);

        var hasPreference = _preferences.Actions.TryGetValue(descriptor.Action, out var preference);
        ReplaceKeyboardEvents(
            descriptor.Action,
            hasPreference && preference!.KeyboardCustomized
                ? new[] { (Key)preference.KeyboardKeycode }
                : DefaultKeys(descriptor));

        if (hasPreference && preference!.GamepadCustomized)
        {
            if (preference.GamepadKind == "axis" && preference.GamepadAxis >= 0)
                ReplaceGamepadEvents(descriptor.Action, -1, preference.GamepadAxis, preference.GamepadAxisValue);
            else
                ReplaceGamepadEvents(descriptor.Action, preference.GamepadButton, -1, 0f);
        }
        else
        {
            ReplaceGamepadEvents(descriptor.Action, descriptor.GamepadButton, descriptor.GamepadAxis, descriptor.GamepadAxisValue);
        }
    }

    private static IReadOnlyList<Key> DefaultKeys(ActionDescriptor descriptor)
    {
        var values = new List<Key>(2);
        if (descriptor.PrimaryKey != Key.None) values.Add(descriptor.PrimaryKey);
        if (descriptor.AlternateKey is { } alternate && alternate != Key.None) values.Add(alternate);
        return values;
    }

    private static void ReplaceKeyboardEvents(string action, IReadOnlyList<Key> keys)
    {
        var remove = InputMap.ActionGetEvents(action).Where(value => value is InputEventKey).ToList();
        foreach (var value in remove) InputMap.ActionEraseEvent(action, value);
        foreach (var key in keys.Distinct()) InputMap.ActionAddEvent(action, new InputEventKey { Keycode = key });
    }

    private static void ReplaceGamepadEvents(string action, int button, int axis, float axisValue)
    {
        var remove = InputMap.ActionGetEvents(action)
            .Where(value => value is InputEventJoypadButton or InputEventJoypadMotion).ToList();
        foreach (var value in remove) InputMap.ActionEraseEvent(action, value);

        if (button >= 0)
        {
            InputMap.ActionAddEvent(action, new InputEventJoypadButton { ButtonIndex = (JoyButton)button });
        }
        else if (axis >= 0)
        {
            InputMap.ActionAddEvent(action, new InputEventJoypadMotion
            {
                Axis = (JoyAxis)axis,
                AxisValue = axisValue < 0f ? -1f : 1f
            });
        }
    }

    private static void EnsureUiControllerNavigation()
    {
        EnsureAction("ui_accept");
        EnsureAction("ui_cancel");
        EnsureAction("ui_up");
        EnsureAction("ui_down");
        EnsureAction("ui_left");
        EnsureAction("ui_right");
        AddJoyButtonIfAbsent("ui_accept", 0);
        AddJoyButtonIfAbsent("ui_cancel", 1);
        AddJoyButtonIfAbsent("ui_up", 11);
        AddJoyButtonIfAbsent("ui_down", 12);
        AddJoyButtonIfAbsent("ui_left", 13);
        AddJoyButtonIfAbsent("ui_right", 14);
        AddJoyAxisIfAbsent("ui_up", 1, -1f);
        AddJoyAxisIfAbsent("ui_down", 1, 1f);
        AddJoyAxisIfAbsent("ui_left", 0, -1f);
        AddJoyAxisIfAbsent("ui_right", 0, 1f);
    }

    private static void EnsureAction(string action)
    {
        if (!InputMap.HasAction(action)) InputMap.AddAction(action, 0.22f);
    }

    private static void AddJoyButtonIfAbsent(string action, int button)
    {
        if (InputMap.ActionGetEvents(action).Any(value =>
                value is InputEventJoypadButton joy && (int)joy.ButtonIndex == button)) return;
        InputMap.ActionAddEvent(action, new InputEventJoypadButton { ButtonIndex = (JoyButton)button });
    }

    private static void AddJoyAxisIfAbsent(string action, int axis, float value)
    {
        if (InputMap.ActionGetEvents(action).Any(input =>
                input is InputEventJoypadMotion joy
                && (int)joy.Axis == axis
                && Math.Sign(joy.AxisValue) == Math.Sign(value))) return;
        InputMap.ActionAddEvent(action, new InputEventJoypadMotion
        {
            Axis = (JoyAxis)axis,
            AxisValue = value < 0f ? -1f : 1f
        });
    }

    private static ActionDescriptor? Find(string action) =>
        ConfigurableActions.FirstOrDefault(value => string.Equals(value.Action, action, StringComparison.Ordinal));

    private static ActionBindingPreference PreferenceFor(string action)
    {
        if (!_preferences.Actions.TryGetValue(action, out var preference))
        {
            preference = new ActionBindingPreference();
            _preferences.Actions[action] = preference;
        }
        return preference;
    }

    private static ControlPreferences Load()
    {
        var absolute = ProjectSettings.GlobalizePath(PreferencesPath);
        if (!File.Exists(absolute)) return new ControlPreferences();
        try
        {
            var preferences = JsonSerializer.Deserialize<ControlPreferences>(File.ReadAllText(absolute), JsonOptions)
                ?? new ControlPreferences();
            preferences.Actions ??= new Dictionary<string, ActionBindingPreference>(StringComparer.Ordinal);
            return preferences;
        }
        catch (Exception exception)
        {
            GD.PushWarning($"Could not load controls: {exception.Message}");
            return new ControlPreferences();
        }
    }

    private static void Save()
    {
        var absolute = ProjectSettings.GlobalizePath(PreferencesPath);
        var temporary = absolute + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(absolute) ?? ProjectSettings.GlobalizePath("user://"));
            File.WriteAllText(temporary, JsonSerializer.Serialize(_preferences, JsonOptions));
            File.Move(temporary, absolute, true);
        }
        catch (Exception exception)
        {
            GD.PushError($"Could not save controls: {exception.Message}");
            try { if (File.Exists(temporary)) File.Delete(temporary); } catch { }
        }
    }

    private static string KeyLabel(Key key) => key switch
    {
        Key.None => "—", Key.Space => "Space", Key.Shift => "Shift", Key.Tab => "Tab",
        Key.Escape => "Esc", Key.Up => "↑", Key.Down => "↓", Key.Left => "←", Key.Right => "→",
        _ => key.ToString()
    };

    private static string ButtonLabel(int button) => button switch
    {
        0 => "A / Cross", 1 => "B / Circle", 2 => "X / Square", 3 => "Y / Triangle",
        4 => "View / Back", 6 => "Menu / Start", 7 => "L3", 8 => "R3", 9 => "LB / L1",
        10 => "RB / R1", 11 => "D-Pad ↑", 12 => "D-Pad ↓", 13 => "D-Pad ←", 14 => "D-Pad →",
        _ => button >= 0 ? $"Pad Button {button}" : "—"
    };

    private static string AxisLabel(int axis, float direction) => axis switch
    {
        0 => direction < 0f ? "Left Stick ←" : "Left Stick →",
        1 => direction < 0f ? "Left Stick ↑" : "Left Stick ↓",
        2 => direction < 0f ? "Right Stick ←" : "Right Stick →",
        3 => direction < 0f ? "Right Stick ↑" : "Right Stick ↓",
        4 => "LT / L2", 5 => "RT / R2",
        _ => axis >= 0 ? $"Pad Axis {axis} {(direction < 0f ? "-" : "+")}" : "—"
    };
}
