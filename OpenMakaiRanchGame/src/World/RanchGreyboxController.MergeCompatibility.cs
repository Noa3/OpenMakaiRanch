using System;
using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// Restores the area/composition contract that was dropped by the manual dev merge while keeping
/// the newer RanchGreyboxController interaction HUD, companion and presentation implementation.
/// </summary>
public partial class RanchGreyboxController
{
    /// <summary>Stable authored area id used by the legacy multi-area composition.</summary>
    [Export] public string AreaId { get; set; } = "ranch";

    /// <summary>Player spawn point when entering this authored area.</summary>
    [Export] public Vector3 EntryPosition { get; set; } = new(0f, 1.6f, 13f);

    [Export] public float EntryYawDegrees { get; set; } = 180f;
    [Export] public float EntryPitchDegrees { get; set; } = 14f;
    [Export] public float EntryDistance { get; set; } = 8f;

    public Vector3 EntryCameraPosition => WorldCameraMath.ComputeCameraPosition(
        EntryPosition,
        Mathf.DegToRad(EntryYawDegrees),
        Mathf.DegToRad(EntryPitchDegrees),
        EntryDistance);

    /// <summary>
    /// Compatibility notification for RanchWorldController. The production WorldGame path may
    /// continue to use its newer management routing; this event has no simulation side effects.
    /// </summary>
    public event Action? ManagementUiRequested;

    public void RefreshFromGame() => RefreshLiveWorld();

    public void RequestManagementUi()
    {
        EnterManagementUi();
        ManagementUiRequested?.Invoke();
    }
}