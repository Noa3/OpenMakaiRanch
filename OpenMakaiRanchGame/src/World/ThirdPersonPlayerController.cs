using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// Third-person player controller for the ranch greybox. Uses a <see cref="CharacterBody3D"/>,
/// camera-relative movement, and bounded acceleration/gravity via <see cref="WorldMovementMath"/>.
///
/// It is deliberately presentation-only: it moves the body and reports nothing to the
/// simulation. All game effects flow through the <see cref="OpenMakaiRanch.App.GameRoot"/>
/// command boundary, never through this node.
/// </summary>
public partial class ThirdPersonPlayerController : CharacterBody3D
{
    [Export] public float MaxWalkSpeed { get; set; } = 5f;
    [Export] public float Acceleration { get; set; } = 40f;
    [Export] public float Gravity { get; set; } = 20f;
    [Export] public float HeadHeight { get; set; } = 1.6f;

    public WorldInputGate InputGate { get; set; } = new();

    /// <summary>The node the camera should orbit around (the player's head).</summary>
    [Export] public Node3D? CameraTarget { get; set; }

    /// <summary>
    /// Called once per frame by <see cref="_PhysicsProcess"/>. Exposed for automated
    /// headless verification: tests can drive the controller with synthetic camera vectors
    /// without a live mouse/keyboard.
    /// </summary>
    public Vector3 LastComputedVelocity { get; private set; } = Vector3.Zero;

    internal Vector3 CameraBasisForward { get; set; } = Vector3.Back;
    internal Vector3 CameraBasisRight { get; set; } = Vector3.Right;

    private Camera3D? _camera;

    public override void _Ready()
    {
        if (CameraTarget is null)
        {
            var target = new Node3D { Name = "CameraTarget" };
            target.Position = new Vector3(0f, HeadHeight, 0f);
            AddChild(target);
            CameraTarget = target;
        }
    }

    private void ResolveCamera()
    {
        if (_camera is null || !_camera.IsInsideTree())
        {
            _camera = GetCamera();
        }

        if (_camera is not null && _camera.IsInsideTree())
        {
            // ProjectRayNormal from the visible rect center returns the camera's true
            // forward direction in world space (Godot cameras look along -Z).
            var viewport = _camera.GetViewport();
            var visible = viewport?.GetVisibleRect();
            if (visible.HasValue && visible.Value.Size.X > 0f && visible.Value.Size.Y > 0f)
            {
                var center = visible.Value.Size * 0.5f;
                var forward = _camera.ProjectRayNormal(center);
                var up = _camera.GlobalTransform.Basis.Y;
                CameraBasisForward = forward.Normalized();
                CameraBasisRight = (CameraBasisForward.Cross(up)).Normalized();
            }
        }
    }

    /// <summary>
    /// Compute the input vector for this frame from the InputMap. Returns (0,0) when the
    /// input gate is closed (UI owns input or the window lost focus).
    /// </summary>
    public Vector2 ReadMovementInput()
    {
        if (!InputGate.WorldInputEnabled)
        {
            return Vector2.Zero;
        }

        var forward = (Input.IsActionPressed("move_forward") ? 1f : 0f) - (Input.IsActionPressed("move_backward") ? 1f : 0f);
        var strafe = (Input.IsActionPressed("move_right") ? 1f : 0f) - (Input.IsActionPressed("move_left") ? 1f : 0f);
        var input = new Vector2(strafe, forward);
        if (input.Length() > 1f)
        {
            input = input.Normalized();
        }

        return input;
    }

    public override void _PhysicsProcess(double delta)
    {
        var dt = (float)delta;
        ResolveCamera();

        var input = ReadMovementInput();
        var direction = WorldMovementMath.ComputeMovementDirection(CameraBasisForward, CameraBasisRight, input);
        var targetVelocity = direction * MaxWalkSpeed;

        // Gravity (bounded) when not on the floor.
        if (!IsOnFloor())
        {
            Velocity = WorldMovementMath.ApplyGravity(Velocity, Gravity, dt);
        }
        else
        {
            Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);
        }

        // Bounded horizontal acceleration toward the target.
        var target = new Vector3(targetVelocity.X, Velocity.Y, targetVelocity.Z);
        Velocity = WorldMovementMath.BlendVelocity(Velocity, target, Acceleration, dt);
        Velocity = WorldMovementMath.ClampSpeed(Velocity, MaxWalkSpeed);
        LastComputedVelocity = Velocity;

        MoveAndSlide();
    }

    private Camera3D? GetCamera()
    {
        // Find the world camera rig's Camera3D by walking up from this body's parent chain
        // and into its children. The scene authors the camera as a sibling under the same root.
        var root = GetParent();
        while (root is not null)
        {
            foreach (var child in root.GetChildren())
            {
                if (child is Camera3D cam)
                {
                    return cam;
                }
            }

            root = root.GetParent();
        }

        return GetNodeOrNull<Camera3D>("Camera");
    }
}
