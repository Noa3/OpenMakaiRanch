using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.Ui;

public partial class UiShellController
{
    private const string ControlsExtensionName = "ControlsAndInteractionExtension";
    private string _bindingCaptureAction = string.Empty;
    private string _bindingCaptureDevice = string.Empty;
    private Button? _bindingCaptureButton;
    private Label? _controlsStatus;
    private ulong _captureArmedAt;

    private void EnsureInputOptionsExtension()
    {
        if (!IsVisibleInTree() || _currentScreen != "options" || !GodotObject.IsInstanceValid(_content))
        {
            if (!string.IsNullOrEmpty(_bindingCaptureAction)) CancelBindingCapture("Binding cancelled.");
            return;
        }

        if (_content.GetNodeOrNull<Control>(ControlsExtensionName) is not null) return;

        InputBindingService.EnsureApplied();
        var card = new PanelContainer { Name = ControlsExtensionName, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        card.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(.055f, .075f, .11f, .96f),
            BorderColor = new Color(.24f, .43f, .52f, .75f),
            BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = 9, CornerRadiusTopRight = 9, CornerRadiusBottomLeft = 9, CornerRadiusBottomRight = 9,
            ContentMarginLeft = 14, ContentMarginTop = 12, ContentMarginRight = 14, ContentMarginBottom = 12
        });
        _content.AddChild(card);

        var body = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        body.AddThemeConstantOverride("separation", 8);
        card.AddChild(body);

        var title = new Label { Text = "Controls & Interaction" };
        title.AddThemeFontSizeOverride("font_size", 20);
        body.AddChild(title);
        body.AddChild(new Label
        {
            Text = "Keyboard and controller bindings apply to every save. UI Accept/Back navigation remains available as a safety fallback.",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });

        _controlsStatus = new Label { Text = InputBindingService.ConnectedControllerSummary(), AutowrapMode = TextServer.AutowrapMode.WordSmart };
        body.AddChild(_controlsStatus);

        var accessibility = new HBoxContainer();
        accessibility.AddThemeConstantOverride("separation", 8);
        body.AddChild(accessibility);

        var highlight = new Button
        {
            Text = HighlightToggleText(),
            TooltipText = "Highlight the selected world interaction target while it is in range.",
            CustomMinimumSize = new Vector2(230, 38)
        };
        highlight.Pressed += () =>
        {
            InputBindingService.SetInteractionHighlightEnabled(!InputBindingService.InteractionHighlightEnabled);
            highlight.Text = HighlightToggleText();
            _game.Feedback.PlayConfirm();
        };
        accessibility.AddChild(highlight);

        var resetAll = new Button { Text = "Reset Controls to Defaults", CustomMinimumSize = new Vector2(220, 38) };
        resetAll.Pressed += () =>
        {
            InputBindingService.ResetAllBindings();
            RebuildInputOptionsExtension("Controls restored to defaults.");
            _game.Feedback.PlayConfirm();
        };
        accessibility.AddChild(resetAll);

        var header = new HBoxContainer();
        body.AddChild(header);
        header.AddChild(ColumnLabel("Action", 190));
        header.AddChild(ColumnLabel("Keyboard", 180));
        header.AddChild(ColumnLabel("Controller", 190));
        header.AddChild(ColumnLabel(string.Empty, 72));

        foreach (var descriptor in InputBindingService.ConfigurableActions) body.AddChild(BuildBindingRow(descriptor));
    }

    private Control BuildBindingRow(InputBindingService.ActionDescriptor descriptor)
    {
        var row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        row.AddThemeConstantOverride("separation", 8);
        row.AddChild(ColumnLabel(descriptor.DisplayName, 190));

        var keyboard = new Button { Text = InputBindingService.GetKeyboardLabel(descriptor.Action), CustomMinimumSize = new Vector2(180, 36) };
        keyboard.Pressed += () => BeginBindingCapture(descriptor.Action, "keyboard", keyboard);
        row.AddChild(keyboard);

        var controller = new Button { Text = InputBindingService.GetGamepadLabel(descriptor.Action), CustomMinimumSize = new Vector2(190, 36) };
        controller.Pressed += () => BeginBindingCapture(descriptor.Action, "gamepad", controller);
        row.AddChild(controller);

        var reset = new Button { Text = "Reset", CustomMinimumSize = new Vector2(72, 36) };
        reset.Pressed += () =>
        {
            InputBindingService.ResetAction(descriptor.Action);
            keyboard.Text = InputBindingService.GetKeyboardLabel(descriptor.Action);
            controller.Text = InputBindingService.GetGamepadLabel(descriptor.Action);
            if (_controlsStatus is not null) _controlsStatus.Text = $"Reset {descriptor.DisplayName}.";
        };
        row.AddChild(reset);
        return row;
    }

    private static Label ColumnLabel(string text, float width) => new()
    {
        Text = text,
        CustomMinimumSize = new Vector2(width, 30),
        VerticalAlignment = VerticalAlignment.Center
    };

    private void BeginBindingCapture(string action, string device, Button button)
    {
        if (!string.IsNullOrEmpty(_bindingCaptureAction)) CancelBindingCapture("Previous binding cancelled.");
        _bindingCaptureAction = action;
        _bindingCaptureDevice = device;
        _bindingCaptureButton = button;
        _captureArmedAt = Time.GetTicksMsec() + 180;
        button.Text = device == "keyboard" ? "Press a key…" : "Press button / move stick…";
        if (_controlsStatus is not null)
            _controlsStatus.Text = device == "keyboard" ? "Waiting for keyboard input. Esc cancels." : "Waiting for controller input. B / Circle cancels.";
    }

    public override void _Input(InputEvent @event)
    {
        if (string.IsNullOrEmpty(_bindingCaptureAction) || Time.GetTicksMsec() < _captureArmedAt) return;

        if (_bindingCaptureDevice == "keyboard" && @event is InputEventKey key && key.Pressed && !key.Echo)
        {
            var code = key.Keycode != Key.None ? key.Keycode : key.PhysicalKeycode;
            if (code == Key.Escape) CancelBindingCapture("Binding cancelled.");
            else if (code != Key.None)
            {
                InputBindingService.SetKeyboardBinding(_bindingCaptureAction, code);
                CompleteBindingCapture("Keyboard binding updated.");
            }
            GetViewport().SetInputAsHandled();
            return;
        }

        if (_bindingCaptureDevice != "gamepad") return;
        if (@event is InputEventJoypadButton button && button.Pressed)
        {
            if ((int)button.ButtonIndex == 1) CancelBindingCapture("Binding cancelled.");
            else
            {
                InputBindingService.SetGamepadButtonBinding(_bindingCaptureAction, (int)button.ButtonIndex);
                CompleteBindingCapture("Controller binding updated.");
            }
            GetViewport().SetInputAsHandled();
        }
        else if (@event is InputEventJoypadMotion motion && Mathf.Abs(motion.AxisValue) >= .72f)
        {
            InputBindingService.SetGamepadAxisBinding(_bindingCaptureAction, (int)motion.Axis, motion.AxisValue);
            CompleteBindingCapture("Controller axis binding updated.");
            GetViewport().SetInputAsHandled();
        }
    }

    private void CompleteBindingCapture(string message)
    {
        if (_bindingCaptureButton is not null && GodotObject.IsInstanceValid(_bindingCaptureButton))
            _bindingCaptureButton.Text = _bindingCaptureDevice == "keyboard"
                ? InputBindingService.GetKeyboardLabel(_bindingCaptureAction)
                : InputBindingService.GetGamepadLabel(_bindingCaptureAction);
        ClearCapture();
        if (_controlsStatus is not null) _controlsStatus.Text = $"{message}  •  {InputBindingService.ConnectedControllerSummary()}";
    }

    private void CancelBindingCapture(string message)
    {
        if (_bindingCaptureButton is not null && GodotObject.IsInstanceValid(_bindingCaptureButton))
            _bindingCaptureButton.Text = _bindingCaptureDevice == "keyboard"
                ? InputBindingService.GetKeyboardLabel(_bindingCaptureAction)
                : InputBindingService.GetGamepadLabel(_bindingCaptureAction);
        ClearCapture();
        if (_controlsStatus is not null) _controlsStatus.Text = message;
    }

    private void ClearCapture()
    {
        _bindingCaptureAction = string.Empty;
        _bindingCaptureDevice = string.Empty;
        _bindingCaptureButton = null;
    }

    private void RebuildInputOptionsExtension(string message)
    {
        if (_content.GetNodeOrNull<Control>(ControlsExtensionName) is { } existing) existing.QueueFree();
        _controlsStatus = null;
        CallDeferred(nameof(EnsureInputOptionsExtension));
        SetStatus(message, false);
    }

    private static string HighlightToggleText() => $"Interaction Highlight: {(InputBindingService.InteractionHighlightEnabled ? "On" : "Off")}";

    private void EnsureControllerFocus()
    {
        if (!IsVisibleInTree() || GetViewport().GuiGetFocusOwner() is not null) return;
        if (_navButtons.TryGetValue(_currentScreen, out var current) && GodotObject.IsInstanceValid(current) && current.Visible && !current.Disabled)
            current.GrabFocus();
    }
}
