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

    public Camera3D? Camera => _camera;
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
            var canLook = !RequireRightMouseButtonForLook || Input.IsMouseButtonPressed(MouseButton.Right);
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

        if (InputGate.WorldInputEnabled)
        {
            if (Input.IsActionJustPressed("camera_recenter"))
            {
                var t = Mathf.Clamp(RecenterSpeed * dt, 0f, 1f);
                Yaw = Mathf.LerpAngle(Yaw, Mathf.DegToRad(-90f), t);
                Pitch = Mathf.Lerp(Pitch, Mathf.DegToRad(30f), t);
            }

            // These mapped actions remain useful for accessibility/controller bindings. Mouse look
            // is handled in _UnhandledInput so ordinary desktop play no longer depends on synthetic
            // InputEventAction events.
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

    private void UpdateCameraTransform()
    {
        if (_camera is null || Target is null)
        {
            return;
        }

        var targetPos = Target.GlobalPosition;
        _desiredPosition = WorldCameraMath.ComputeCameraPosition(targetPos, Yaw, Pitch, Distance);

        var hitDistance = float.PositiveInfinity;
        var spaceState = GetWorld3D()?.DirectSpaceState;
        if (spaceState is not null)
        {
            var from = targetPos;
            var to = _desiredPosition;
            var rayDir = to - from;
            var length = rayDir.Length();
            if (length > 0.0001f)
            {
                var query = PhysicsRayQueryParameters3D.Create(from, to);
                // Ignore the target player body when the camera target is parented to it.
                if (Target.GetParent() is CollisionObject3D owner)
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

        var clamped = WorldCameraMath.ClampToGeometry(targetPos, _desiredPosition, hitDistance);
        var direction = (targetPos - clamped).Normalized();
        var basis = Basis.LookingAt(direction, Vector3.Up);
        _camera.GlobalTransform = new Transform3D(basis, clamped);
    }
}
