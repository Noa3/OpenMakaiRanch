using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// Pure movement-math helpers for the third-person player controller.
/// Kept static and Node-free so it can be verified headlessly and unit-tested
/// without a live scene. The <c>ThirdPersonPlayerController</c> node delegates
/// its per-frame direction/velocity math to these methods.
/// </summary>
public static class WorldMovementMath
{
    /// <summary>
    /// Compute the camera-relative movement direction on the world XZ plane.
    /// </summary>
    /// <param name="cameraForward">The camera's world forward vector (may have a Y component).</param>
    /// <param name="cameraRight">The camera's world right vector (may have a Y component).</param>
    /// <param name="input">The raw input vector; X = right positive, Y = forward positive.</param>
    /// <returns>A unit vector on the XZ plane, or <see cref="Vector3.Zero"/> if the input magnitude is below the dead zone.</returns>
    public static Vector3 ComputeMovementDirection(Vector3 cameraForward, Vector3 cameraRight, Vector2 input)
    {
        if (input is (0, 0) || input.Length() < 0.001f)
        {
            return Vector3.Zero;
        }

        // Project onto the XZ plane; the camera's pitch must not leak into the movement direction.
        var flatForward = new Vector3(cameraForward.X, 0f, cameraForward.Z).Normalized();
        var flatRight = new Vector3(cameraRight.X, 0f, cameraRight.Z).Normalized();

        var direction = (flatForward * input.Y) + (flatRight * input.X);
        if (direction.Length() < 0.001f)
        {
            return Vector3.Zero;
        }

        return direction.Normalized();
    }

    /// <summary>
    /// Blend the current velocity toward the target using exponential smoothing, so acceleration is bounded
    /// and the controller does not snap to full speed in one frame.
    /// </summary>
    public static Vector3 BlendVelocity(Vector3 current, Vector3 target, float accel, float delta)
    {
        // Exponential approach: the closer we are to the target, the less we change.
        // t in [0,1] is the fraction of the gap closed this frame.
        var t = 1f - Mathf.Exp(-accel * delta);
        return current.Lerp(target, t);
    }

    /// <summary>
    /// Apply gravity to the vertical velocity component when the body is not on the floor.
    /// </summary>
    public static Vector3 ApplyGravity(Vector3 velocity, float gravity, float delta)
    {
        return new Vector3(velocity.X, velocity.Y - gravity * delta, velocity.Z);
    }

    /// <summary>
    /// Clamp the speed so diagonal movement is never faster than straight-line movement.
    /// This is the core acceptance check from the 3D_REMAKE_PLAN ("diagonal movement must not be faster").
    /// </summary>
    public static Vector3 ClampSpeed(Vector3 velocity, float maxSpeed)
    {
        var horizontal = new Vector3(velocity.X, 0f, velocity.Z);
        if (horizontal.Length() > maxSpeed)
        {
            horizontal = horizontal.Normalized() * maxSpeed;
        }

        return new Vector3(horizontal.X, velocity.Y, horizontal.Z);
    }
}
