using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;

namespace OpenMakaiRanch.App;

/// <summary>Persistent device-wide controls. Gameplay saves never own input preferences.</summary>
public static class InputBindingService
{
    public const string PreferencesPath = "user://controls.json";

    public sealed record ActionDescriptor(string Action, string DisplayName, Key PrimaryKey, Key? AlternateKey = null,
        int GamepadButton = -1, int GamepadAxis = -1, float GamepadAxisValue = 0f);

    public sealed class BindingPreference
    {
        public bool KeyboardCustomized { get; set; }
        public long KeyboardKeycode { get; set; }
        public bool GamepadCustomized { get; set; }
        public string GamepadKind { get; set; } = string.Empty;
        public int GamepadButton { get; set; } = -1;
        public int GamepadAxis { get; set; } = -1;
        public float GamepadAxisValue { get; set; }
    }

    public sealed class Preferences
    {
        public bool InteractionHighlightEnabled { get; set; } = true;
        public Dictionary<string, BindingPreference> Actions { get; set; } = new(StringComparer.Ordinal);
    }

    public static readonly IReadOnlyList<ActionDescriptor> ConfigurableActions = new[]
    {
        new ActionDescriptor("move_forward", "Move Forward", Key.W, Key.Up, GamepadAxis: 1, GamepadAxisValue: -1),
        new ActionDescriptor("move_backward", "Move Backward", Key.S, Key.Down, GamepadAxis: 1, GamepadAxisValue: 1),
        new ActionDescriptor("move_left", "Move Left", Key.A, Key.Left, GamepadAxis: 0, GamepadAxisValue: -1),
        new ActionDescriptor("move_right", "Move Right", Key.D, Key.Right, GamepadAxis: 0, GamepadAxisValue: 1),
        new ActionDescriptor("move_sprint", "Sprint", Key.Shift, GamepadButton: 7),
        new ActionDescriptor("interact", "Interact / Use", Key.F, Key.Space, GamepadButton: 0),
        new ActionDescriptor("camera_look_up", "Camera Up", Key.None, GamepadAxis: 3, GamepadAxisValue: -1),
        new ActionDescriptor("camera_look_down", "Camera Down", Key.None, GamepadAxis: 3, GamepadAxisValue: 1),
        new ActionDescriptor("camera_look_left", "Camera Left", Key.None, GamepadAxis: 2, GamepadAxisValue: -1),
        new ActionDescriptor("camera_look_right", "Camera Right", Key.None, GamepadAxis: 2, GamepadAxisValue: 1),
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
    private static Preferences _preferences = new();
    private static bool _loaded;

    public static bool InteractionHighlightEnabled { get { EnsureApplied(); return _preferences.InteractionHighlightEnabled; } }

    public static void EnsureApplied(bool forceReload = false)
    {
        if (!_loaded || forceReload) { _preferences = Load(); _loaded = true; }
        foreach (var descriptor in ConfigurableActions) ApplyAction(descriptor);
        EnsureUiControllerNavigation();
    }

    public static void SetInteractionHighlightEnabled(bool enabled)
    {
        EnsureApplied();
        _preferences.InteractionHighlightEnabled = enabled;
        Save();
    }

    public static void SetKeyboardBinding(string action, Key key)
    {
        EnsureApplied();
        var descriptor = Find(action);
        if (descriptor is null || key == Key.None) return;
        var pref = PreferenceFor(action);
        pref.KeyboardCustomized = true;
        pref.KeyboardKeycode = (long)key;
        ApplyAction(descriptor);
        Save();
    }

    public static void SetGamepadButtonBinding(string action, int button)
    {
        EnsureApplied();
        var descriptor = Find(action);
        if (descriptor is null || button < 0) return;
        var pref = PreferenceFor(action);
        pref.GamepadCustomized = true;
        pref.GamepadKind = "button";
        pref.GamepadButton = button;
        pref.GamepadAxis = -1;
        ApplyAction(descriptor);
        Save();
    }

    public static void SetGamepadAxisBinding(string action, int axis, float value)
    {
        EnsureApplied();
        var descriptor = Find(action);
        if (descriptor is null || axis < 0 || Mathf.Abs(value) < .5f) return;
        var pref = PreferenceFor(action);
        pref.GamepadCustomized = true;
        pref.GamepadKind = "axis";
        pref.GamepadButton = -1;
        pref.GamepadAxis = axis;
        pref.GamepadAxisValue = value < 0 ? -1 : 1;
        ApplyAction(descriptor);
        Save();
    }

    public static void ResetAction(string action)
    {
        EnsureApplied();
        var descriptor = Find(action);
        if (descriptor is null) return;
        _preferences.Actions.Remove(action);
        ApplyAction(descriptor);
        Save();
    }

    public static void ResetAllBindings()
    {
        EnsureApplied();
        var highlight = _preferences.InteractionHighlightEnabled;
        _preferences = new Preferences { InteractionHighlightEnabled = highlight };
        foreach (var descriptor in ConfigurableActions) ApplyAction(descriptor);
        EnsureUiControllerNavigation();
        Save();
    }

    public static string GetKeyboardLabel(string action)
    {
        EnsureApplied();
        var descriptor = Find(action);
        if (descriptor is null) return "—";
        if (_preferences.Actions.TryGetValue(action, out var pref) && pref.KeyboardCustomized)
            return KeyLabel((Key)pref.KeyboardKeycode);
        if (descriptor.PrimaryKey == Key.None) return "—";
        return descriptor.AlternateKey is { } alt && alt != Key.None
            ? $"{KeyLabel(descriptor.PrimaryKey)} / {KeyLabel(alt)}"
            : KeyLabel(descriptor.PrimaryKey);
    }

    public static string GetGamepadLabel(string action)
    {
        EnsureApplied();
        var descriptor = Find(action);
        if (descriptor is null) return "—";
        if (_preferences.Actions.TryGetValue(action, out var pref) && pref.GamepadCustomized)
            return pref.GamepadKind == "axis" ? AxisLabel(pref.GamepadAxis, pref.GamepadAxisValue) : ButtonLabel(pref.GamepadButton);
        if (descriptor.GamepadAxis >= 0) return AxisLabel(descriptor.GamepadAxis, descriptor.GamepadAxisValue);
        return descriptor.GamepadButton >= 0 ? ButtonLabel(descriptor.GamepadButton) : "—";
    }

    public static string GetCombinedLabel(string action)
    {
        var keyboard = GetKeyboardLabel(action);
        var pad = GetGamepadLabel(action);
        if (keyboard == "—") return pad;
        if (pad == "—") return keyboard;
        return $"{keyboard} / {pad}";
    }

    public static string ConnectedControllerSummary()
    {
        var ids = Input.GetConnectedJoypads();
        if (ids.Count == 0) return "No controller detected";
        return string.Join(", ", ids.Select(id => string.IsNullOrWhiteSpace(Input.GetJoyName(id)) ? $"Controller {id + 1}" : Input.GetJoyName(id)));
    }

    private static void ApplyAction(ActionDescriptor descriptor)
    {
        EnsureAction(descriptor.Action);
        InputMap.ActionSetDeadzone(descriptor.Action, .22f);
        var has = _preferences.Actions.TryGetValue(descriptor.Action, out var pref);
        ReplaceKeyboard(descriptor.Action, has && pref!.KeyboardCustomized
            ? new[] { (Key)pref.KeyboardKeycode }
            : DefaultKeys(descriptor));
        if (has && pref!.GamepadCustomized)
        {
            if (pref.GamepadKind == "axis") ReplaceGamepad(descriptor.Action, -1, pref.GamepadAxis, pref.GamepadAxisValue);
            else ReplaceGamepad(descriptor.Action, pref.GamepadButton, -1, 0);
        }
        else ReplaceGamepad(descriptor.Action, descriptor.GamepadButton, descriptor.GamepadAxis, descriptor.GamepadAxisValue);
    }

    private static IReadOnlyList<Key> DefaultKeys(ActionDescriptor descriptor)
    {
        var result = new List<Key>(2);
        if (descriptor.PrimaryKey != Key.None) result.Add(descriptor.PrimaryKey);
        if (descriptor.AlternateKey is { } alt && alt != Key.None) result.Add(alt);
        return result;
    }

    private static void ReplaceKeyboard(string action, IReadOnlyList<Key> keys)
    {
        foreach (var ev in InputMap.ActionGetEvents(action).Where(ev => ev is InputEventKey).ToList()) InputMap.ActionEraseEvent(action, ev);
        foreach (var key in keys.Distinct()) InputMap.ActionAddEvent(action, new InputEventKey { Keycode = key });
    }

    private static void ReplaceGamepad(string action, int button, int axis, float axisValue)
    {
        foreach (var ev in InputMap.ActionGetEvents(action).Where(ev => ev is InputEventJoypadButton or InputEventJoypadMotion).ToList()) InputMap.ActionEraseEvent(action, ev);
        if (button >= 0) InputMap.ActionAddEvent(action, new InputEventJoypadButton { ButtonIndex = (JoyButton)button });
        else if (axis >= 0) InputMap.ActionAddEvent(action, new InputEventJoypadMotion { Axis = (JoyAxis)axis, AxisValue = axisValue < 0 ? -1 : 1 });
    }

    private static void EnsureUiControllerNavigation()
    {
        EnsureAction("ui_accept"); EnsureAction("ui_cancel"); EnsureAction("ui_up"); EnsureAction("ui_down"); EnsureAction("ui_left"); EnsureAction("ui_right");
        AddJoyButtonIfAbsent("ui_accept", 0); AddJoyButtonIfAbsent("ui_cancel", 1);
        AddJoyButtonIfAbsent("ui_up", 11); AddJoyButtonIfAbsent("ui_down", 12); AddJoyButtonIfAbsent("ui_left", 13); AddJoyButtonIfAbsent("ui_right", 14);
        AddJoyAxisIfAbsent("ui_up", 1, -1); AddJoyAxisIfAbsent("ui_down", 1, 1); AddJoyAxisIfAbsent("ui_left", 0, -1); AddJoyAxisIfAbsent("ui_right", 0, 1);
    }

    private static void EnsureAction(string action) { if (!InputMap.HasAction(action)) InputMap.AddAction(action, .22f); }
    private static void AddJoyButtonIfAbsent(string action, int button)
    {
        if (!InputMap.ActionGetEvents(action).Any(ev => ev is InputEventJoypadButton joy && (int)joy.ButtonIndex == button))
            InputMap.ActionAddEvent(action, new InputEventJoypadButton { ButtonIndex = (JoyButton)button });
    }
    private static void AddJoyAxisIfAbsent(string action, int axis, float value)
    {
        if (!InputMap.ActionGetEvents(action).Any(ev => ev is InputEventJoypadMotion joy && (int)joy.Axis == axis && Math.Sign(joy.AxisValue) == Math.Sign(value)))
            InputMap.ActionAddEvent(action, new InputEventJoypadMotion { Axis = (JoyAxis)axis, AxisValue = value < 0 ? -1 : 1 });
    }

    private static ActionDescriptor? Find(string action) => ConfigurableActions.FirstOrDefault(x => x.Action == action);
    private static BindingPreference PreferenceFor(string action)
    {
        if (!_preferences.Actions.TryGetValue(action, out var pref)) _preferences.Actions[action] = pref = new BindingPreference();
        return pref;
    }

    private static Preferences Load()
    {
        var path = ProjectSettings.GlobalizePath(PreferencesPath);
        if (!File.Exists(path)) return new Preferences();
        try
        {
            var result = JsonSerializer.Deserialize<Preferences>(File.ReadAllText(path), JsonOptions) ?? new Preferences();
            result.Actions ??= new Dictionary<string, BindingPreference>(StringComparer.Ordinal);
            return result;
        }
        catch (Exception ex) { GD.PushWarning($"Could not load controls: {ex.Message}"); return new Preferences(); }
    }

    private static void Save()
    {
        var path = ProjectSettings.GlobalizePath(PreferencesPath);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ProjectSettings.GlobalizePath("user://"));
            File.WriteAllText(path, JsonSerializer.Serialize(_preferences, JsonOptions));
        }
        catch (Exception ex) { GD.PushError($"Could not save controls: {ex.Message}"); }
    }

    private static string KeyLabel(Key key) => key switch
    {
        Key.None => "—", Key.Space => "Space", Key.Shift => "Shift", Key.Tab => "Tab", Key.Escape => "Esc",
        Key.Up => "↑", Key.Down => "↓", Key.Left => "←", Key.Right => "→", _ => key.ToString()
    };
    private static string ButtonLabel(int button) => button switch
    {
        0 => "A / Cross", 1 => "B / Circle", 2 => "X / Square", 3 => "Y / Triangle", 4 => "View / Back",
        6 => "Menu / Start", 7 => "L3", 8 => "R3", 9 => "LB / L1", 10 => "RB / R1",
        11 => "D-Pad ↑", 12 => "D-Pad ↓", 13 => "D-Pad ←", 14 => "D-Pad →", _ => button >= 0 ? $"Pad Button {button}" : "—"
    };
    private static string AxisLabel(int axis, float direction) => axis switch
    {
        0 => direction < 0 ? "Left Stick ←" : "Left Stick →", 1 => direction < 0 ? "Left Stick ↑" : "Left Stick ↓",
        2 => direction < 0 ? "Right Stick ←" : "Right Stick →", 3 => direction < 0 ? "Right Stick ↑" : "Right Stick ↓",
        4 => "LT / L2", 5 => "RT / R2", _ => axis >= 0 ? $"Pad Axis {axis} {(direction < 0 ? "-" : "+")}" : "—"
    };
}
