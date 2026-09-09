using System;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Asset-free touch overlay for the 3D world. It feeds the same player/camera/interaction boundaries
/// used by desktop input; it never executes gameplay rewards or owns simulation state.
/// </summary>
public partial class MobileWorldControls : Control
{
    public event Action? InteractPressed;
    public event Action? ManagementPressed;
    public event Action? CycleWorkerPressed;

    private ThirdPersonPlayerController? _player;
    private WorldCameraRig? _camera;
    private Button? _interactButton;
    private Button? _sprintButton;
    private Button? _managementButton;
    private Button? _cycleButton;

    private int _moveFinger = -1;
    private int _lookFinger = -1;
    private Vector2 _moveOrigin;
    private Vector2 _moveThumb;
    private float _controlScale = 1f;
    private bool _externallyBlocked;
    private bool _showCycle;
    private ScreenLayoutMetrics _layout;

    public bool ControlsVisible => Visible;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        MouseFilter = MouseFilterEnum.Pass;
        BuildButtons();

        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged += ApplySettings;
        }

        ApplySettings();
    }

    public override void _ExitTree()
    {
        ResetTouches();
        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged -= ApplySettings;
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            LayoutButtons();
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (!Visible)
        {
            return;
        }

        var radius = 72f * _controlScale;
        var baseCenter = _moveFinger >= 0
            ? _moveOrigin
            : new Vector2(
                _layout.SafeLeft + 104f * _controlScale,
                Size.Y - _layout.SafeBottom - 112f * _controlScale);
        var thumb = _moveFinger >= 0 ? _moveThumb : baseCenter;

        DrawCircle(baseCenter, radius, new Color(0.05f, 0.08f, 0.12f, 0.42f));
        DrawArc(baseCenter, radius, 0f, Mathf.Tau, 48, new Color(0.8f, 0.9f, 1f, 0.36f), 3f);
        DrawCircle(thumb, 30f * _controlScale, new Color(0.65f, 0.82f, 1f, 0.48f));
    }

    public void Bind(ThirdPersonPlayerController? player, WorldCameraRig? camera, bool showCycle)
    {
        if (_player is not null && !ReferenceEquals(_player, player))
        {
            _player.MobileMovementInput = Vector2.Zero;
            _player.MobileSprintHeld = false;
        }

        _player = player;
        _camera = camera;
        _showCycle = showCycle;
        ApplySettings();
    }

    public void SetBlocked(bool blocked)
    {
        if (_externallyBlocked == blocked)
        {
            return;
        }

        _externallyBlocked = blocked;
        ApplyVisibility();
        if (blocked)
        {
            ResetTouches();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!Visible || _player is null || !_player.InputGate.WorldInputEnabled)
        {
            return;
        }

        if (@event is InputEventScreenTouch touch)
        {
            if (IsOverActionButton(touch.Position))
            {
                return;
            }

            if (touch.Pressed)
            {
                if (_moveFinger < 0 && IsMovementRegion(touch.Position))
                {
                    _moveFinger = touch.Index;
                    _moveOrigin = touch.Position;
                    _moveThumb = touch.Position;
                    UpdateMove(touch.Position);
                    GetViewport().SetInputAsHandled();
                }
                else if (_lookFinger < 0)
                {
                    _lookFinger = touch.Index;
                    GetViewport().SetInputAsHandled();
                }
            }
            else
            {
                if (touch.Index == _moveFinger)
                {
                    _moveFinger = -1;
                    _player.MobileMovementInput = Vector2.Zero;
                    QueueRedraw();
                }
                if (touch.Index == _lookFinger)
                {
                    _lookFinger = -1;
                }
            }

            return;
        }

        if (@event is InputEventScreenDrag drag)
        {
            if (drag.Index == _moveFinger)
            {
                UpdateMove(drag.Position);
                GetViewport().SetInputAsHandled();
            }
            else if (drag.Index == _lookFinger)
            {
                _camera?.ApplyLookDelta(drag.Relative);
                GetViewport().SetInputAsHandled();
            }
        }
    }

    private void ApplySettings()
    {
        if (GameRoot.Instance is not { } game)
        {
            Visible = false;
            return;
        }

        _controlScale = Mathf.Clamp(game.State.Settings.TouchControlScale, 0.75f, 1.50f);
        ApplyVisibility();
        LayoutButtons();
        QueueRedraw();
    }

    private void ApplyVisibility()
    {
        var game = GameRoot.Instance;
        var shouldShow = game is not null && game.RuntimeSettings.ShouldShowTouchControls;
        Visible = shouldShow && !_externallyBlocked;

        if (_cycleButton is not null)
        {
            _cycleButton.Visible = Visible && _showCycle;
        }

        if (!Visible)
        {
            ResetTouches();
        }
    }

    private void BuildButtons()
    {
        _interactButton = MakeButton("InteractButton", "Interact");
        _sprintButton = MakeButton("SprintButton", "Sprint");
        _managementButton = MakeButton("ManagementButton", "Menu");
        _cycleButton = MakeButton("CycleWorkerButton", "Worker");

        _interactButton.Pressed += () => InteractPressed?.Invoke();
        _managementButton.Pressed += () => ManagementPressed?.Invoke();
        _cycleButton.Pressed += () => CycleWorkerPressed?.Invoke();
        _sprintButton.ButtonDown += () =>
        {
            if (_player is not null) _player.MobileSprintHeld = true;
        };
        _sprintButton.ButtonUp += () =>
        {
            if (_player is not null) _player.MobileSprintHeld = false;
        };

        LayoutButtons();
    }

    private Button MakeButton(string name, string text)
    {
        var button = new Button
        {
            Name = name,
            Text = text,
            FocusMode = FocusModeEnum.None,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };
        button.AddThemeFontSizeOverride("font_size", 16);
        AddChild(button);
        return button;
    }

    private void LayoutButtons()
    {
        if (_interactButton is null || _sprintButton is null || _managementButton is null || _cycleButton is null)
        {
            return;
        }

        _layout = ScreenLayout.Calculate(GetViewport());
        var buttonSize = 94f * _controlScale;
        var gap = 16f * _controlScale;
        var edge = 24f * _controlScale;
        var right = edge + _layout.SafeRight;
        var bottom = edge + _layout.SafeBottom;
        var top = edge + _layout.SafeTop;

        PlaceBottomRight(_sprintButton, right, bottom, buttonSize);
        PlaceBottomRight(_interactButton, right, bottom + buttonSize + gap, buttonSize);
        PlaceBottomRight(_cycleButton, right + buttonSize + gap, bottom, buttonSize);

        _managementButton.SetAnchorsPreset(LayoutPreset.TopRight);
        _managementButton.OffsetLeft = -(buttonSize + right);
        _managementButton.OffsetTop = top;
        _managementButton.OffsetRight = -right;
        _managementButton.OffsetBottom = top + buttonSize * 0.72f;

        _cycleButton.Visible = Visible && _showCycle;
    }

    private static void PlaceBottomRight(Control control, float right, float bottom, float size)
    {
        control.SetAnchorsPreset(LayoutPreset.BottomRight);
        control.OffsetLeft = -(right + size);
        control.OffsetTop = -(bottom + size);
        control.OffsetRight = -right;
        control.OffsetBottom = -bottom;
    }

    private bool IsMovementRegion(Vector2 position)
    {
        return position.X <= Size.X * 0.48f && position.Y >= Size.Y * 0.25f;
    }

    private bool IsOverActionButton(Vector2 position)
    {
        foreach (var button in new[] { _interactButton, _sprintButton, _managementButton, _cycleButton })
        {
            if (button is not null && button.Visible && button.GetGlobalRect().HasPoint(position))
            {
                return true;
            }
        }
        return false;
    }

    private void UpdateMove(Vector2 position)
    {
        if (_player is null)
        {
            return;
        }

        var radius = 72f * _controlScale;
        var delta = position - _moveOrigin;
        if (delta.Length() > radius)
        {
            delta = delta.Normalized() * radius;
        }

        _moveThumb = _moveOrigin + delta;
        var normalized = radius <= 0f ? Vector2.Zero : delta / radius;
        _player.MobileMovementInput = new Vector2(normalized.X, -normalized.Y);
        QueueRedraw();
    }

    private void ResetTouches()
    {
        _moveFinger = -1;
        _lookFinger = -1;
        if (_player is not null)
        {
            _player.MobileMovementInput = Vector2.Zero;
            _player.MobileSprintHeld = false;
        }
        QueueRedraw();
    }
}
