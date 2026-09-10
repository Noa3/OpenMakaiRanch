using System;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.Ui;

/// <summary>
/// Adds account-wide keyboard/controller remapping and interaction accessibility to the existing
/// Options screen without duplicating its graphics/audio/camera settings.
/// </summary>
public partial class UiShellController
{
    private const string ControlsExtensionName = "ControlsAndInteractionExtension";
    private string _bindingCaptureAction = string.Empty;
    private string _bindingCaptureDevice = string.Empty;
    private Button? _bindingCaptureButton;
    private Label? _controlsLiveStatus;
    private ulong _captureArmedAt;

    private void EnsureInputOptionsExtension()
    {
        if (!IsVisibleInTree() || _currentScreen != "options" || !GodotObject.IsInstanceValid(_content))
        {
            if (!string.IsNullOrEmpty(_bindingCaptureAction))
            {
                CancelBindingCapture("Binding cancelled.");
            }
            return;
        }

        if (_content.GetNodeOrNull<Control>(ControlsExtensionName) is not null)
        {
            return;
        }

        InputBindingService.EnsureApplied();

        var card = new PanelContainer
        {
            Name = ControlsExtensionName,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        card.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.055f, 0.075f, 0.11f, 0.96f),
            BorderColor = new Color(0.24f, 0.43f, 0.52f, 0.75f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 9,
            CornerRadiusTopRight = 9,
            CornerRadiusBottomLeft = 9,
            CornerRadiusBottomRight = 9,
            ContentMarginLeft = 14,
            ContentMarginTop = 12,
            ContentMarginRight = 14,
            ContentMarginBottom = 12
        });
        _content.AddChild(card);

        var body = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        body.AddThemeConstantOverride("separation", 8);
        card.AddChild(body);

        var title = new Label { Text = "Controls & Interaction" };
        title.AddThemeFontSizeOverride("font_size", 20);
        body.AddChild(title);

        var help = new Label
        {
            Text = "Keyboard and controller bindings are stored for this device and apply to every save. UI navigation keeps standard Accept/Back controls as a safety fallback.",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        help.Modulate = new Color(0.82f, 0.86f, 0.91f);
        body.AddChild(help);

        _controlsLiveStatus = new Label
        {
            Text = InputBindingService.ConnectedControllerSummary(),
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _controlsLiveStatus.Modulate = new Color(0.64f, 0.82f, 0.82f);
        body.AddChild(_controlsLiveStatus);

        var accessibilityRow = new HFlowContainer();
        accessibilityRow.AddThemeConstantOverride("separation", 8);
        body.AddChild(accessibilityRow);

        var highlight = new Button
        {
            Name = "InteractionHighlightToggle",
            Text = HighlightToggleText(),
            TooltipText = "Highlight only the currently selected interaction target while it is in range. Disable this for a cleaner world view.",
            CustomMinimumSize = new Vector2(230, 38)
        };
        highlight.Pressed += () =>
        {
            InputBindingService.SetInteractionHighlightEnabled(!InputBindingService.InteractionHighlightEnabled);
            highlight.Text = HighlightToggleText();
            _game.Feedback.PlayConfirm();
        };
        accessibilityRow.AddChild(highlight);

        var resetAll = new Button
        {
            Text = "Reset Controls to Defaults",
            TooltipText = "Restore the default keyboard and standardized controller layout.",
            CustomMinimumSize = new Vector2(220, 38)
        };
        resetAll.Pressed += () =>
        {
            InputBindingService.ResetAllBindings();
            RebuildInputOptionsExtension("Controls restored to defaults.");
            _game.Feedback.PlayConfirm();
        };
        accessibilityRow.AddChild(resetAll);

        var header = new HFlowContainer();
        header.AddThemeConstantOverride("separation", 8);
        body.AddChild(header);
        header.AddChild(ColumnLabel("Action", 190));
        header.AddChild(ColumnLabel("Keyboard", 180));
        header.AddChild(ColumnLabel("Controller", 190));
        header.AddChild(ColumnLabel(string.Empty, 72));

        foreach (var descriptor in InputBindingService.ConfigurableActions)
        {
            body.AddChild(BuildBindingRow(descriptor));
        }
    }

    private Control BuildBindingRow(InputBindingService.ActionDescriptor descriptor)
    {
        var row = new HFlowContainer
        {
            Name = $"Binding_{descriptor.Action}",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        row.AddThemeConstantOverride("separation", 8);

        var actionLabel = new Label
        {
            Text = descriptor.DisplayName,
            CustomMinimumSize = new Vector2(190, 36),
            VerticalAlignment = VerticalAlignment.Center
        };
        row.AddChild(actionLabel);

        var keyboard = new Button
        {
            Name = $"Keyboard_{descriptor.Action}",
            Text = InputBindingService.GetKeyboardLabel(descriptor.Action),
            TooltipText = $"Rebind keyboard input for {descriptor.DisplayName}.",
            CustomMinimumSize = new Vector2(180, 36)
        };
        keyboard.Pressed += () => BeginBindingCapture(descriptor.Action, "keyboard", keyboard);
        row.AddChild(keyboard);

        var controller = new Button
        {
            Name = $"Controller_{descriptor.Action}",
            Text = InputBindingService.GetGamepadLabel(descriptor.Action),
            TooltipText = $"Rebind controller button or stick direction for {descriptor.DisplayName}.",
            CustomMinimumSize = new Vector2(190, 36)
        };
        controller.Pressed += () => BeginBindingCapture(descriptor.Action, "gamepad", controller);
        row.AddChild(controller);

        var reset = new Button
        {
            Text = "Reset",
            TooltipText = $"Restore default bindings for {descriptor.DisplayName}.",
            CustomMinimumSize = new Vector2(72, 36)
        };
        reset.Pressed += () =>
        {
            InputBindingService.ResetAction(descriptor.Action);
            keyboard.Text = InputBindingService.GetKeyboardLabel(descriptor.Action);
            controller.Text = InputBindingService.GetGamepadLabel(descriptor.Action);
            if (GodotObject.IsInstanceValid(_controlsLiveStatus))
            {
                _controlsLiveStatus.Text = $"Reset {descriptor.DisplayName}.";
            }
            _game.Feedback.PlayConfirm();
        };
        row.AddChild(reset);

        return row;
    }

    private static Label ColumnLabel(string text, float width)
    {
        var label = new Label
        {
            Text = text,
            CustomMinimumSize = new Vector2(width, 28),
            VerticalAlignment = VerticalAlignment.Center
        };
        label.Modulate = new Color(0.66f, 0.75f, 0.84f);
        return label;
    }

    private void BeginBindingCapture(string action, string device, Button button)
    {
        if (!IsVisibleInTree() || _currentScreen != "options" || !IsLiveBindingControl(button)) return;
        if (!string.IsNullOrEmpty(_bindingCaptureAction))
        {
            CancelBindingCapture("Previous binding cancelled.");
        }

        _bindingCaptureAction = action;
        _bindingCaptureDevice = device;
        _bindingCaptureButton = button;
        _captureArmedAt = Time.GetTicksMsec() + 180;
        button.Text = device == "keyboard" ? "Press a key…" : "Press button / move stick…";
        if (GodotObject.IsInstanceValid(_controlsLiveStatus))
        {
            _controlsLiveStatus.Text = device == "keyboard"
                ? "Waiting for keyboard input. Esc cancels."
                : "Waiting for controller input. B / Circle cancels; sticks need a deliberate full direction.";
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!IsVisibleInTree() || _currentScreen != "options"
            || !IsLiveBindingControl(_bindingCaptureButton))
        {
            CancelBindingCapture("Binding cancelled.");
            return;
        }
        if (string.IsNullOrEmpty(_bindingCaptureAction))
        {
            return;
        }

        if (TryCancelBindingInput(@event)) return;
        if (Time.GetTicksMsec() < _captureArmedAt)
        {
            if (@event.IsPressed()) GetViewport().SetInputAsHandled();
            return;
        }

        if (_bindingCaptureDevice == "keyboard" && @event is InputEventKey key && key.Pressed && !key.Echo)
        {
            var keycode = key.Keycode != Key.None ? key.Keycode : key.PhysicalKeycode;
            if (keycode == Key.Escape)
            {
                CancelBindingCapture("Binding cancelled.");
            }
            else if (keycode != Key.None)
            {
                InputBindingService.SetKeyboardBinding(_bindingCaptureAction, keycode);
                CompleteBindingCapture("Keyboard binding updated.");
            }
            GetViewport().SetInputAsHandled();
            return;
        }

        if (_bindingCaptureDevice != "gamepad")
        {
            return;
        }

        if (@event is InputEventJoypadButton joyButton && joyButton.Pressed)
        {
            if ((int)joyButton.ButtonIndex == 1)
            {
                CancelBindingCapture("Binding cancelled.");
            }
            else
            {
                InputBindingService.SetGamepadButtonBinding(_bindingCaptureAction, (int)joyButton.ButtonIndex);
                CompleteBindingCapture("Controller binding updated.");
            }
            GetViewport().SetInputAsHandled();
            return;
        }

        if (@event is InputEventJoypadMotion joyMotion && Mathf.Abs(joyMotion.AxisValue) >= 0.72f)
        {
            InputBindingService.SetGamepadAxisBinding(
                _bindingCaptureAction,
                (int)joyMotion.Axis,
                joyMotion.AxisValue);
            CompleteBindingCapture("Controller axis binding updated.");
            GetViewport().SetInputAsHandled();
        }
    }

    private void CompleteBindingCapture(string message)
    {
        if (_bindingCaptureButton is not null && GodotObject.IsInstanceValid(_bindingCaptureButton))
        {
            _bindingCaptureButton.Text = _bindingCaptureDevice == "keyboard"
                ? InputBindingService.GetKeyboardLabel(_bindingCaptureAction)
                : InputBindingService.GetGamepadLabel(_bindingCaptureAction);
        }

        _bindingCaptureAction = string.Empty;
        _bindingCaptureDevice = string.Empty;
        _bindingCaptureButton = null;
        if (GodotObject.IsInstanceValid(_controlsLiveStatus))
        {
            _controlsLiveStatus.Text = $"{message}  •  {InputBindingService.ConnectedControllerSummary()}";
        }
        _game.Feedback.PlayConfirm();
    }

    private void CancelBindingCapture(string message)
    {
        if (_bindingCaptureButton is not null && GodotObject.IsInstanceValid(_bindingCaptureButton))
        {
            _bindingCaptureButton.Text = _bindingCaptureDevice == "keyboard"
                ? InputBindingService.GetKeyboardLabel(_bindingCaptureAction)
                : InputBindingService.GetGamepadLabel(_bindingCaptureAction);
        }

        _bindingCaptureAction = string.Empty;
        _bindingCaptureDevice = string.Empty;
        _bindingCaptureButton = null;
        if (GodotObject.IsInstanceValid(_controlsLiveStatus))
        {
            _controlsLiveStatus.Text = message;
        }
    }

    private void RebuildInputOptionsExtension(string message)
    {
        CancelBindingCapture("Binding cancelled.");
        if (_content.GetNodeOrNull<Control>(ControlsExtensionName) is { } existing)
        {
            _content.RemoveChild(existing);
            existing.QueueFree();
        }

        _controlsLiveStatus = null;
        CallDeferred(nameof(EnsureInputOptionsExtension));
        SetStatus(message, false);
    }

    private static string HighlightToggleText() =>
        $"Interaction Highlight: {(InputBindingService.InteractionHighlightEnabled ? "On" : "Off")}";

    private void EnsureControllerFocus()
    {
        if (!IsVisibleInTree() || !CanProcess()) return;
        var owner = GetViewport().GuiGetFocusOwner();
        if (owner is not null && owner.IsVisibleInTree() && IsAncestorOf(owner)
            && owner is not BaseButton { Disabled: true }) return;
        FirstMenuFocus()?.GrabFocus();
    }
}
