using System;
using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// Camera-relative player movement. This node changes presentation transforms only;
/// rewards, stamina and the day clock remain owned by GameRoot and its services.
/// </summary>
public partial class ThirdPersonPlayerController : CharacterBody3D
{
    [Export] public float MaxWalkSpeed { get; set; } = 5f;
    [Export] public float SprintMultiplier { get; set; } = 1.65f;
    [Export] public float Acceleration { get; set; } = 40f;
    [Export] public float Gravity { get; set; } = 20f;
    [Export] public float TurnSpeed { get; set; } = 10f;
    [Export] public float HeadHeight { get; set; } = 1.6f;

    private WorldInputGate _inputGate = new();
    private bool _applicationFocused = true;

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

    /// <summary>Normalized input from the existing mobile overlay.</summary>
    public Vector2 MobileMovementInput { get; set; } = Vector2.Zero;
    public bool MobileSprintHeld { get; set; }

    [Export] public Node3D? CameraTarget { get; set; }
    public Vector3 LastComputedVelocity { get; private set; } = Vector3.Zero;

    internal Vector3 CameraBasisForward { get; set; } = Vector3.Back;
    internal Vector3 CameraBasisRight { get; set; } = Vector3.Right;

    public override void _EnterTree()
    {
        _inputGate.InputStateDidChange += OnInputGateChanged;
    }

    public override void _Ready()
    {
        EnsureCameraTarget();
    }

    public override void _ExitTree()
    {
        _inputGate.InputStateDidChange -= OnInputGateChanged;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut)
        {
            _applicationFocused = false;
            InputGate.SetWindowFocused(false);
            StopHorizontalMovement();
        }
        else if (what == NotificationApplicationFocusIn)
        {
            _applicationFocused = true;
            InputGate.SetWindowFocused(true);
        }
        else if (what is NotificationPaused or NotificationDisabled)
        {
            StopHorizontalMovement();
        }
    }

    private void OnInputGateChanged()
    {
        if (!InputGate.WorldInputEnabled)
            StopHorizontalMovement();
    }

    private void StopHorizontalMovement()
    {
        // No residual sliding or held touch sprint after a menu/focus/area transition.
        // Preserve vertical velocity so an ordinary management overlay does not suspend gravity.
        Velocity = new Vector3(0f, Velocity.Y, 0f);
        LastComputedVelocity = Velocity;
        MobileMovementInput = Vector2.Zero;
        MobileSprintHeld = false;
    }

    public Node3D EnsureCameraTarget()
    {
        if (CameraTarget is not null && GodotObject.IsInstanceValid(CameraTarget))
        {
            CameraTarget.Position = new Vector3(0f, HeadHeight, 0f);
            return CameraTarget;
        }

        var target = new Node3D { Name = "CameraTarget", Position = new Vector3(0f, HeadHeight, 0f) };
        AddChild(target);
        CameraTarget = target;
        return target;
    }

    public void SetFirstPersonVisualHidden(bool hidden)
    {
        var visual = GetNodeOrNull<Node3D>("Visual");
        if (visual is not null)
            visual.Visible = !hidden;
    }

    internal void RefreshCameraBasis()
    {
        if (!IsInsideTree()) return;

        // The viewport is the camera authority. Recursive scene searches can pick a character
        // preview's SubViewport camera, or retain a previous area's now-inactive camera.
        var camera = GetViewport().GetCamera3D();
        if (camera is null || !GodotObject.IsInstanceValid(camera) || !camera.IsInsideTree()) return;
        var basis = camera.GlobalTransform.Basis;
        CameraBasisForward = -basis.Z.Normalized();
        CameraBasisRight = basis.X.Normalized();
    }

    public Vector2 ReadMovementInput()
    {
        if (!InputGate.WorldInputEnabled || !_applicationFocused)
            return Vector2.Zero;

        // Godot applies the configured circular deadzone. Our world convention uses +Y forward.
        var mapped = Input.GetVector("move_left", "move_right", "move_backward", "move_forward");
        var touch = MobileMovementInput.IsFinite() ? MobileMovementInput : Vector2.Zero;
        return (mapped + touch).LimitLength(1f);
    }

    public float MoveSpeedFor(bool sprinting)
    {
        return Mathf.Max(0f, MaxWalkSpeed) * (sprinting ? Mathf.Max(1f, SprintMultiplier) : 1f);
    }

    internal Vector3 ComputeDesiredVelocity(Vector2 input, bool sprinting) =>
        WorldMovementMath.ComputeTargetVelocity(CameraBasisForward, CameraBasisRight, input, MoveSpeedFor(sprinting));

    public override void _PhysicsProcess(double delta)
    {
        if (delta <= 0 || !double.IsFinite(delta)) return;
        var dt = (float)delta;
        RefreshCameraBasis();

        var input = ReadMovementInput();
        var direction = WorldMovementMath.ComputeMovementDirection(CameraBasisForward, CameraBasisRight, input);
        var sprinting = InputGate.WorldInputEnabled && _applicationFocused
            && (Input.IsActionPressed("move_sprint") || MobileSprintHeld);
        var moveSpeed = MoveSpeedFor(sprinting);
        var targetVelocity = ComputeDesiredVelocity(input, sprinting);

        if (!InputGate.WorldInputEnabled || !_applicationFocused)
            StopHorizontalMovement();

        if (direction.LengthSquared() > 0.0001f)
        {
            var targetYaw = Mathf.Atan2(-direction.X, -direction.Z);
            Rotation = new Vector3(Rotation.X,
                Mathf.LerpAngle(Rotation.Y, targetYaw, Mathf.Clamp(TurnSpeed * dt, 0f, 1f)), Rotation.Z);
        }

        if (!IsOnFloor())
            Velocity = WorldMovementMath.ApplyGravity(Velocity, Gravity, dt);
        else
            Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);

        var target = new Vector3(targetVelocity.X, Velocity.Y, targetVelocity.Z);
        Velocity = WorldMovementMath.BlendVelocity(Velocity, target, Acceleration, dt);
        Velocity = WorldMovementMath.ClampSpeed(Velocity, moveSpeed);
        LastComputedVelocity = Velocity;
        MoveAndSlide();
    }
}
