using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// Collision-aware third-person camera follow rig. Owns yaw/pitch/zoom and keeps the
/// camera out of geometry by clamping along its ray to the nearest obstacle
/// (via <see cref="WorldCameraMath.ClampToGeometry"/> and a raycast).
///
/// Per the 3D_REMAKE_PLAN: "camera must not penetrate geometry." The follow target is the
/// player's head node. Mouse look, wheel zoom, and recenter are implemented here.
/// </summary>
public partial class WorldCameraRig : Node3D
{
    [Export] public float LookSensitivity { get; set; } = 0.0025f;
    [Export] public float ZoomSensitivity { get; set; } = 1.5f;
    [Export] public float RecenterSpeed { get; set; } = 6f;

    /// <summary>The node the camera orbits around (the player's head).</summary>
    [Export] public Node3D? Target { get; set; }

    [Export] public float Yaw { get; set; } = Mathf.DegToRad(-90f);
    [Export] public float Pitch { get; set; } = Mathf.DegToRad(30f);
    [Export] public float Distance { get; set; } = 7f;

    private Camera3D? _camera;
    private Vector3 _desiredPosition;

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
    }

    /// <summary>
    /// Drive the rig from synthetic yaw/pitch/distance — used by headless verification.
    /// </summary>
    public void SetOrbit(float yaw, float pitch, float distance)
    {
        Yaw = yaw;
        Pitch = WorldCameraMath.ClampPitch(pitch);
        Distance = WorldCameraMath.ApplyZoom(distance, 0f);
    }

    public override void _Process(double delta)
    {
        var dt = (float)delta;

        if (Input.IsActionJustPressed("camera_recenter"))
        {
            var t = Mathf.Clamp(RecenterSpeed * dt, 0f, 1f);
            Yaw = Mathf.LerpAngle(Yaw, Mathf.DegToRad(-90f), t);
            Pitch = Mathf.Lerp(Pitch, Mathf.DegToRad(30f), t);
        }

        // Mouse look (only when the world owns input — the input gate is checked by the scene
        // via WorldInputGate; here we only react to the mapped actions).
        if (Input.IsActionJustPressed("camera_look_up"))
        {
            Pitch = WorldCameraMath.ClampPitch(Pitch + LookSensitivity);
        }
        if (Input.IsActionJustPressed("camera_look_down"))
        {
            Pitch = WorldCameraMath.ClampPitch(Pitch - LookSensitivity);
        }
        if (Input.IsActionJustPressed("camera_look_left"))
        {
            Yaw += LookSensitivity;
        }
        if (Input.IsActionJustPressed("camera_look_right"))
        {
            Yaw -= LookSensitivity;
        }

        if (Input.IsActionJustPressed("camera_zoom_in"))
        {
            Distance = WorldCameraMath.ApplyZoom(Distance, ZoomSensitivity);
        }
        if (Input.IsActionJustPressed("camera_zoom_out"))
        {
            Distance = WorldCameraMath.ApplyZoom(Distance, -ZoomSensitivity);
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

        // Collision-aware clamp: raycast from target toward the desired camera position.
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
                var hit = spaceState.IntersectRay(query);
                if (hit.ContainsKey("position"))
                {
                    hitDistance = from.DistanceTo(hit["position"].As<Vector3>());
                }
            }
        }

        var clamped = WorldCameraMath.ClampToGeometry(targetPos, _desiredPosition, hitDistance);
        // Basis.LookingAt(direction, up): the camera looks along `direction` (its -Z axis).
        // So the direction is from the camera position toward the target.
        var direction = (targetPos - clamped).Normalized();
        var basis = Basis.LookingAt(direction, Vector3.Up);
        _camera.GlobalTransform = new Transform3D(basis, clamped);
    }
}
