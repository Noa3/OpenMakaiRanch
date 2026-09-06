using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// Pure camera-math helpers for the collision-aware third-person follow rig.
/// Node-free and deterministic so the follow/zoom/clamp behavior can be verified
/// headlessly. The <c>WorldCameraRig</c> node supplies yaw/pitch/zoom and the
/// raycast hit distance; these methods turn that into a concrete camera transform.
/// </summary>
public static class WorldCameraMath
{
    public const float MinDistance = 1.5f;
    public const float MaxDistance = 18f;
    public const float MinPitchDegrees = -70f;
    public const float MaxPitchDegrees = 85f;

    /// <summary>
    /// Compute the desired camera position from a third-person orbit.
    /// </summary>
    /// <param name="target">The point the camera orbits (the player's head).</param>
    /// <param name="yaw">Horizontal orbit angle in radians.</param>
    /// <param name="pitch">Vertical orbit angle in radians (positive = above).</param>
    /// <param name="distance">Orbital radius (zoom).</param>
    public static Vector3 ComputeCameraPosition(Vector3 target, float yaw, float pitch, float distance)
    {
        var clampedDistance = Mathf.Clamp(distance, MinDistance, MaxDistance);
        var cosPitch = Mathf.Cos(pitch);

        // Offset in the orbit frame: x = right, y = up, z = back.
        var offset = new Vector3(
            Mathf.Cos(yaw) * cosPitch,
            Mathf.Sin(pitch),
            Mathf.Sin(yaw) * cosPitch) * clampedDistance;

        return target + offset;
    }

    /// <summary>
    /// Compute the direction the camera looks at (normalized, from camera toward target).
    /// </summary>
    public static Vector3 ComputeLookAt(Vector3 cameraPosition, Vector3 target)
    {
        var direction = (target - cameraPosition);
        return direction.Length() < 0.0001f ? Vector3.Back : direction.Normalized();
    }

    /// <summary>
    /// Clamp the camera position so it does not penetrate geometry.
    /// </summary>
    /// <param name="target">The orbit point (player head).</param>
    /// <param name="desiredPosition">The unclamped camera position from <see cref="ComputeCameraPosition"/>.</param>
    /// <param name="raycastHitDistance">Distance from the target to the nearest obstacle along the camera ray (Infinity if no hit).</param>
    /// <returns>
    /// A camera position that sits at most <c>clearance</c> in front of the obstacle,
    /// never closer than <see cref="MinDistance"/> from the target.
    /// </returns>
    public static Vector3 ClampToGeometry(Vector3 target, Vector3 desiredPosition, float raycastHitDistance, float clearance = 0.2f)
    {
        var rayOrigin = target;
        var rayDirection = (desiredPosition - target);
        var rayLength = rayDirection.Length();
        if (rayLength < 0.0001f)
        {
            return desiredPosition;
        }

        rayDirection = rayDirection.Normalized();

        var available = rayLength;
        if (float.IsFinite(raycastHitDistance) && raycastHitDistance > 0f)
        {
            available = Mathf.Min(available, Mathf.Max(MinDistance, raycastHitDistance - clearance));
        }

        return target + rayDirection * available;
    }

    /// <summary>
    /// Apply a zoom delta to a distance, keeping it within the supported range.
    /// </summary>
    public static float ApplyZoom(float currentDistance, float delta)
    {
        return Mathf.Clamp(currentDistance - delta, MinDistance, MaxDistance);
    }

    /// <summary>
    /// Clamp an orbit pitch to the supported range so the camera never under-runs the floor.
    /// </summary>
    public static float ClampPitch(float pitchRadians)
    {
        return Mathf.Clamp(pitchRadians, Mathf.DegToRad(MinPitchDegrees), Mathf.DegToRad(MaxPitchDegrees));
    }
}
