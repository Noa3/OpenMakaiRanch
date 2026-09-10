using System;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Presentation-only orbit/first-person camera. Only the active viewport camera can own the
/// mouse; UI, focus, pause and area changes release that ownership synchronously.
/// </summary>
public partial class WorldCameraRig : Node3D
{
    [Export] public float LookSensitivity { get; set; } = 0.0025f;
    [Export] public float StickLookSpeedDegrees { get; set; } = 120f;
    [Export] public float ZoomSensitivity { get; set; } = 1.5f;
    [Export] public float RecenterSpeed { get; set; } = 6f;
    [Export] public bool RequireRightMouseButtonForLook { get; set; } = true;
    [Export] public float CameraNearClip { get; set; } = 0.03f;
    [Export] public float CollisionClearance { get; set; } = 0.28f;
    [Export] public Node3D? Target { get; set; }
    [Export] public float Yaw { get; set; } = Mathf.DegToRad(-90f);
    [Export] public float Pitch { get; set; } = Mathf.DegToRad(30f);
    [Export] public float Distance { get; set; } = 7f;

    private WorldInputGate _inputGate = new();
    private Camera3D? _camera;
    private Vector3 _desiredPosition;
    private float _userSensitivity = 1f;
    private bool _invertY;
    private bool _reducedMotion;
    private bool _firstPerson;
    private bool _applicationFocused = true;
    private bool _recentering;
    private float _recenterYaw;
    private float _recenterPitch;

    // Transient presentation ownership, not saved state. A hidden/retiring rig must not release
    // a mouse already claimed by the next area's camera during the same scene transition.
    private static WorldCameraRig? _mouseCaptureOwner;

    public WorldInputGate InputGate
    {
        get => _inputGate;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (ReferenceEquals(_inputGate, value)) return;
            _inputGate.InputStateDidChange -= OnInputGateChanged;
            _inputGate = value;
            if (IsInsideTree())
                _inputGate.InputStateDidChange += OnInputGateChanged;
            OnInputGateChanged();
        }
    }

    public Camera3D? Camera => _camera;
    public bool IsFirstPerson => _firstPerson;
    public Vector3 DesiredPosition => _desiredPosition;
    internal bool IsRecentering => _recentering;
    internal bool OwnsMouseCapture => ReferenceEquals(_mouseCaptureOwner, this);

    private bool CanControlCamera => IsInsideTree() && IsVisibleInTree() && CanProcess()
        && _applicationFocused && InputGate.WorldInputEnabled
        && _camera is not null && GodotObject.IsInstanceValid(_camera) && _camera.IsInsideTree()
        && _camera.Current && Target is not null && GodotObject.IsInstanceValid(Target) && Target.IsInsideTree();

    public override void _EnterTree()
    {
        _inputGate.InputStateDidChange += OnInputGateChanged;
        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
            game.StateChanged += ApplyUserSettings;
    }

    public override void _Ready()
    {
        _camera = GetNodeOrNull<Camera3D>("Camera");
        if (_camera is null)
        {
            _camera = new Camera3D { Name = "Camera", Current = true };
            AddChild(_camera);
        }
        ApplyUserSettings();
        _camera.Near = Mathf.Clamp(CameraNearClip, 0.01f, 0.20f);
    }

    public override void _ExitTree()
    {
        _inputGate.InputStateDidChange -= OnInputGateChanged;
        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
            game.StateChanged -= ApplyUserSettings;
        _recentering = false;
        ReleaseMouseCapture();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut)
        {
            _applicationFocused = false;
            InputGate.SetWindowFocused(false);
            _recentering = false;
            ReleaseMouseCapture();
        }
        else if (what == NotificationApplicationFocusIn)
        {
            _applicationFocused = true;
            InputGate.SetWindowFocused(true);
        }
        else if (what is NotificationPaused or NotificationDisabled)
        {
            _recentering = false;
            ReleaseMouseCapture();
        }
        else if (what == NotificationVisibilityChanged && IsInsideTree() && !IsVisibleInTree())
        {
            _recentering = false;
            ReleaseMouseCapture();
        }
        // Reacquire only on the next active update, after any new UI owner has been installed.
    }

    private void OnInputGateChanged()
    {
        if (!InputGate.WorldInputEnabled)
        {
            _recentering = false;
            ReleaseMouseCapture();
        }
    }

    public void ApplyUserSettings()
    {
        if (GameRoot.Instance is not { } game || !GodotObject.IsInstanceValid(game)) return;
        _userSensitivity = Mathf.Clamp(game.State.Settings.CameraSensitivity, 0.35f, 2.50f);
        _invertY = game.State.Settings.InvertCameraY;
        _reducedMotion = game.State.Settings.ReducedMotion;
        if (_camera is not null && GodotObject.IsInstanceValid(_camera))
        {
            _camera.Fov = Mathf.Clamp(game.State.Settings.CameraFov, 55f, 95f);
            _camera.Near = Mathf.Clamp(CameraNearClip, 0.01f, 0.20f);
        }
    }

    public void SetOrbit(float yaw, float pitch, float distance)
    {
        _recentering = false;
        Yaw = yaw;
        Pitch = WorldCameraMath.ClampPitch(pitch);
        Distance = WorldCameraMath.ApplyZoom(distance, 0f);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!CanControlCamera) return;
        if (@event is InputEventMouseMotion motion)
        {
            if (_firstPerson || !RequireRightMouseButtonForLook || Input.IsMouseButtonPressed(MouseButton.Right))
                ApplyLookDelta(motion.Relative);
            return;
        }

        if (@event is InputEventMouseButton { Pressed: true } button)
        {
            if (button.ButtonIndex == MouseButton.WheelUp)
                Distance = WorldCameraMath.ApplyZoom(Distance, ZoomSensitivity);
            else if (button.ButtonIndex == MouseButton.WheelDown)
                Distance = WorldCameraMath.ApplyZoom(Distance, -ZoomSensitivity);
        }
    }

    public void ApplyLookDelta(Vector2 relative)
    {
        if (!InputGate.WorldInputEnabled || !_applicationFocused || !relative.IsFinite()) return;
        if (relative.LengthSquared() > 0f) _recentering = false;
        var sensitivity = LookSensitivity * _userSensitivity;
        Yaw -= relative.X * sensitivity;
        // Positive orbit pitch looks DOWN. Mouse-up must therefore reduce pitch by default.
        Pitch = WorldCameraMath.ClampPitch(Pitch + relative.Y * sensitivity * (_invertY ? -1f : 1f));
    }

    internal void ApplyStickLook(Vector2 input, float delta)
    {
        if (!InputGate.WorldInputEnabled || !_applicationFocused || !input.IsFinite()
            || !float.IsFinite(delta) || delta <= 0f || input.LengthSquared() <= 0.000001f) return;
        _recentering = false;
        input = input.LimitLength(1f);
        var rate = Mathf.DegToRad(Mathf.Max(0f, StickLookSpeedDegrees)) * _userSensitivity * delta;
        Yaw -= input.X * rate;
        Pitch = WorldCameraMath.ClampPitch(Pitch + input.Y * rate * (_invertY ? -1f : 1f));
    }

    public void RequestRecenter()
    {
        if (!CanControlCamera) return;
        _recenterYaw = Mathf.DegToRad(-90f);
        if (Target?.GetParent() is Node3D actor && actor.IsInsideTree())
        {
            var behind = actor.GlobalTransform.Basis.Z;
            if (new Vector2(behind.X, behind.Z).LengthSquared() > 0.0001f)
                _recenterYaw = Mathf.Atan2(behind.Z, behind.X);
        }
        _recenterPitch = _firstPerson ? 0f : Mathf.DegToRad(30f);
        _recentering = true;
        if (_reducedMotion) CompleteRecenter();
    }

    private void AdvanceRecenter(float delta)
    {
        if (!_recentering) return;
        if (_reducedMotion)
        {
            CompleteRecenter();
            return;
        }
        var t = 1f - Mathf.Exp(-Mathf.Max(1f, RecenterSpeed) * delta);
        Yaw = Mathf.LerpAngle(Yaw, _recenterYaw, t);
        Pitch = Mathf.Lerp(Pitch, _recenterPitch, t);
        if (Mathf.Abs(Mathf.Wrap(Yaw - _recenterYaw, -Mathf.Pi, Mathf.Pi)) < 0.005f
            && Mathf.Abs(Pitch - _recenterPitch) < 0.005f)
            CompleteRecenter();
    }

    private void CompleteRecenter()
    {
        Yaw = _recenterYaw;
        Pitch = _recenterPitch;
        _recentering = false;
    }

    public override void _Process(double delta)
    {
        if (!double.IsFinite(delta) || delta < 0) return;
        if (CanControlCamera)
        {
            if (Input.IsActionJustPressed("camera_first_person")) SetFirstPerson(!_firstPerson);
            if (Input.IsActionJustPressed("camera_recenter")) RequestRecenter();
            ApplyStickLook(Input.GetVector("camera_look_left", "camera_look_right", "camera_look_up", "camera_look_down"), (float)delta);
            AdvanceRecenter((float)delta);
            if (Input.IsActionJustPressed("camera_zoom_in"))
                Distance = WorldCameraMath.ApplyZoom(Distance, ZoomSensitivity);
            if (Input.IsActionJustPressed("camera_zoom_out"))
                Distance = WorldCameraMath.ApplyZoom(Distance, -ZoomSensitivity);
        }
        else
        {
            _recentering = false;
        }
        RefreshMouseCapture();
        UpdateCameraTransform();
    }

    public void SetFirstPerson(bool enabled)
    {
        _firstPerson = enabled;
        _recentering = false;
        if (Target is not null && GodotObject.IsInstanceValid(Target)
            && Target.GetParent() is ThirdPersonPlayerController player)
            player.SetFirstPersonVisualHidden(enabled);
        RefreshMouseCapture();
    }

    internal void RefreshMouseCapture()
    {
        if (!_firstPerson || !CanControlCamera)
        {
            ReleaseMouseCapture();
            return;
        }
        _mouseCaptureOwner = this;
        // A pause menu can change the real mouse mode independently. Never trust a stale latch.
        if (Input.MouseMode != Input.MouseModeEnum.Captured)
            Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private void ReleaseMouseCapture()
    {
        if (!OwnsMouseCapture) return;
        _mouseCaptureOwner = null;
        if (Input.MouseMode == Input.MouseModeEnum.Captured)
            Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void UpdateCameraTransform()
    {
        if (_camera is null || Target is null
            || !GodotObject.IsInstanceValid(_camera) || !GodotObject.IsInstanceValid(Target)) return;

        // A detached target has no world-space position. Do not mix its local transform with a
        // live camera's parent space; wait for real tree entry instead of producing a false frame.
        if (!IsInsideTree() || !_camera.IsInsideTree() || !Target.IsInsideTree()) return;
        var targetPos = Target.GlobalPosition;
        if (_firstPerson)
        {
            var viewDirection = WorldCameraMath.ComputeViewDirection(Yaw, Pitch);
            var eye = targetPos + viewDirection * 0.04f;
            _desiredPosition = eye;
            _camera.GlobalTransform = new Transform3D(Basis.LookingAt(viewDirection, Vector3.Up), eye);
            return;
        }

        _desiredPosition = WorldCameraMath.ComputeCameraPosition(targetPos, Yaw, Pitch, Distance);
        var hitDistance = float.PositiveInfinity;
        var spaceState = GetWorld3D()?.DirectSpaceState;
        if (spaceState is not null && targetPos.DistanceTo(_desiredPosition) > 0.0001f)
        {
            var query = PhysicsRayQueryParameters3D.Create(targetPos, _desiredPosition);
            if (Target.GetParent() is CollisionObject3D owner && owner.IsInsideTree())
                query.Exclude = new Godot.Collections.Array<Rid> { owner.GetRid() };
            var hit = spaceState.IntersectRay(query);
            if (hit.ContainsKey("position"))
                hitDistance = targetPos.DistanceTo(hit["position"].As<Vector3>());
        }

        var clamped = WorldCameraMath.ClampToGeometry(targetPos, _desiredPosition, hitDistance,
            Mathf.Clamp(CollisionClearance, 0.08f, 0.60f));
        var direction = WorldCameraMath.ComputeLookAt(clamped, targetPos);
        _camera.GlobalTransform = new Transform3D(Basis.LookingAt(direction, Vector3.Up), clamped);
    }

}
