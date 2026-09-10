using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Collision-aware third-person camera follow rig. Owns yaw/pitch/zoom and keeps the camera out
/// of geometry by clamping along its ray to the nearest obstacle.
///
/// The rig is presentation-only. Input ownership is shared with the player through
/// <see cref="WorldInputGate"/> so management overlays cannot accidentally keep rotating the world.
/// </summary>
public partial class WorldCameraRig : Node3D
{
    [Export] public float LookSensitivity { get; set; } = 0.0025f;
    [Export] public float ZoomSensitivity { get; set; } = 1.5f;
    [Export] public float RecenterSpeed { get; set; } = 6f;
    [Export] public bool RequireRightMouseButtonForLook { get; set; } = true;
    [Export] public float CameraNearClip { get; set; } = 0.03f;
    [Export] public float CollisionClearance { get; set; } = 0.28f;

    /// <summary>The node the camera orbits around (normally the player's head target).</summary>
    [Export] public Node3D? Target { get; set; }

    [Export] public float Yaw { get; set; } = Mathf.DegToRad(-90f);
    [Export] public float Pitch { get; set; } = Mathf.DegToRad(30f);
    [Export] public float Distance { get; set; } = 7f;

    public WorldInputGate InputGate { get; set; } = new();

    private Camera3D? _camera;
    private Vector3 _desiredPosition;
    private float _userSensitivity = 1f;
    private bool _invertY;
    private bool _firstPerson;
    private bool _mouseCapturedForFirstPerson;

    public Camera3D? Camera => _camera;
    public bool IsFirstPerson => _firstPerson;
    public Vector3 DesiredPosition => _desiredPosition;

    public override void _Ready()
    {
        if (_camera is null)
        {
            _camera = GetNodeOrNull<Camera3D>("Camera");
        }

        if (_camera is null)
        {
            _camera = new Camera3D { Name = "Camera", Current = true };
            AddChild(_camera);
        }

        _camera.Near = Mathf.Clamp(CameraNearClip, 0.01f, 0.20f);

        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged += ApplyUserSettings;
        }
        ApplyUserSettings();
    }

    public override void _ExitTree()
    {
        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged -= ApplyUserSettings;
        }
    }

    public void ApplyUserSettings()
    {
        if (GameRoot.Instance is not { } game)
        {
            return;
        }

        _userSensitivity = Mathf.Clamp(game.State.Settings.CameraSensitivity, 0.35f, 2.50f);
        _invertY = game.State.Settings.InvertCameraY;
        if (_camera is not null)
        {
            _camera.Fov = Mathf.Clamp(game.State.Settings.CameraFov, 55f, 95f);
            _camera.Near = Mathf.Clamp(CameraNearClip, 0.01f, 0.20f);
        }
    }

    /// <summary>Drive the orbit directly; used by deterministic verification and dev tools.</summary>
    public void SetOrbit(float yaw, float pitch, float distance)
    {
        Yaw = yaw;
        Pitch = WorldCameraMath.ClampPitch(pitch);
        Distance = WorldCameraMath.ApplyZoom(distance, 0f);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!InputGate.WorldInputEnabled)
        {
            return;
        }

        if (@event is InputEventMouseMotion motion)
        {
            var canLook = _firstPerson || !RequireRightMouseButtonForLook || Input.IsMouseButtonPressed(MouseButton.Right);
            if (canLook)
            {
                ApplyLookDelta(motion.Relative);
            }
            return;
        }

        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            if (mouseButton.ButtonIndex == MouseButton.WheelUp)
            {
                Distance = WorldCameraMath.ApplyZoom(Distance, ZoomSensitivity);
            }
            else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                Distance = WorldCameraMath.ApplyZoom(Distance, -ZoomSensitivity);
            }
        }
    }

    public void ApplyLookDelta(Vector2 relative)
    {
        if (!InputGate.WorldInputEnabled)
        {
            return;
        }

        var sensitivity = LookSensitivity * _userSensitivity;
        Yaw -= relative.X * sensitivity;
        var y = relative.Y * sensitivity * (_invertY ? -1f : 1f);
        Pitch = WorldCameraMath.ClampPitch(Pitch - y);
    }

    public override void _Process(double delta)
    {
        var dt = (float)delta;

        UpdateFirstPersonMouseCapture();

        if (InputGate.WorldInputEnabled)
        {
            if (Input.IsActionJustPressed("camera_first_person"))
            {
                SetFirstPerson(!_firstPerson);
            }

            if (Input.IsActionJustPressed("camera_recenter"))
            {
                var t = Mathf.Clamp(RecenterSpeed * dt, 0f, 1f);
                Yaw = Mathf.LerpAngle(Yaw, Mathf.DegToRad(-90f), t);
                Pitch = Mathf.Lerp(Pitch, Mathf.DegToRad(30f), t);
            }

            if (Input.IsActionPressed("camera_look_up"))
            {
                Pitch = WorldCameraMath.ClampPitch(Pitch + LookSensitivity * _userSensitivity * 60f * dt * (_invertY ? -1f : 1f));
            }
            if (Input.IsActionPressed("camera_look_down"))
            {
                Pitch = WorldCameraMath.ClampPitch(Pitch - LookSensitivity * _userSensitivity * 60f * dt * (_invertY ? -1f : 1f));
            }
            if (Input.IsActionPressed("camera_look_left"))
            {
                Yaw += LookSensitivity * _userSensitivity * 60f * dt;
            }
            if (Input.IsActionPressed("camera_look_right"))
            {
                Yaw -= LookSensitivity * _userSensitivity * 60f * dt;
            }

            if (Input.IsActionJustPressed("camera_zoom_in"))
            {
                Distance = WorldCameraMath.ApplyZoom(Distance, ZoomSensitivity);
            }
            if (Input.IsActionJustPressed("camera_zoom_out"))
            {
                Distance = WorldCameraMath.ApplyZoom(Distance, -ZoomSensitivity);
            }
        }

        UpdateCameraTransform();
    }

    public void SetFirstPerson(bool enabled)
    {
        if (_firstPerson == enabled)
        {
            return;
        }

        _firstPerson = enabled;
        if (Target?.GetParent() is ThirdPersonPlayerController player)
        {
            player.SetFirstPersonVisualHidden(enabled);
        }

        if (!enabled && _mouseCapturedForFirstPerson)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            _mouseCapturedForFirstPerson = false;
        }
    }

    private void UpdateFirstPersonMouseCapture()
    {
        if (!_firstPerson)
        {
            return;
        }

        if (InputGate.WorldInputEnabled)
        {
            if (!_mouseCapturedForFirstPerson)
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
                _mouseCapturedForFirstPerson = true;
            }
        }
        else if (_mouseCapturedForFirstPerson)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            _mouseCapturedForFirstPerson = false;
        }
    }

    private void UpdateCameraTransform()
    {
        if (_camera is null || Target is null
            || !GodotObject.IsInstanceValid(_camera) || !GodotObject.IsInstanceValid(Target))
        {
            return;
        }

        // Global transforms are only valid after both nodes enter the SceneTree. Tests and scene
        // composition can call _Process manually before that happens; falling back to local
        // coordinates keeps the math deterministic without asking Godot for an invalid transform.
        var treeReady = IsInsideTree() && _camera.IsInsideTree() && Target.IsInsideTree();
        var targetPos = treeReady ? Target.GlobalPosition : Target.Position;

        if (_firstPerson)
        {
            var viewDirection = WorldCameraMath.ComputeViewDirection(Yaw, Pitch);
            var basis = Basis.LookingAt(viewDirection, Vector3.Up);
            var eye = targetPos + viewDirection * 0.04f;
            _desiredPosition = eye;
            SetCameraTransform(new Transform3D(basis, eye), treeReady);
            return;
        }

        _desiredPosition = WorldCameraMath.ComputeCameraPosition(targetPos, Yaw, Pitch, Distance);

        var hitDistance = float.PositiveInfinity;
        var spaceState = treeReady ? GetWorld3D()?.DirectSpaceState : null;
        if (spaceState is not null)
        {
            var from = targetPos;
            var to = _desiredPosition;
            var rayDir = to - from;
            var length = rayDir.Length();
            if (length > 0.0001f)
            {
                var query = PhysicsRayQueryParameters3D.Create(from, to);
                if (Target.GetParent() is CollisionObject3D owner && owner.IsInsideTree())
                {
                    query.Exclude = new Godot.Collections.Array<Rid> { owner.GetRid() };
                }

                var hit = spaceState.IntersectRay(query);
                if (hit.ContainsKey("position"))
                {
                    hitDistance = from.DistanceTo(hit["position"].As<Vector3>());
                }
            }
        }

        var clamped = WorldCameraMath.ClampToGeometry(
            targetPos,
            _desiredPosition,
            hitDistance,
            Mathf.Clamp(CollisionClearance, 0.08f, 0.60f));
        var direction = (targetPos - clamped).Normalized();
        var cameraBasis = Basis.LookingAt(direction, Vector3.Up);
        SetCameraTransform(new Transform3D(cameraBasis, clamped), treeReady);
    }

    private void SetCameraTransform(Transform3D transform, bool global)
    {
        if (_camera is null)
            return;

        if (global)
            _camera.GlobalTransform = transform;
        else
            _camera.Transform = transform;
    }
}
