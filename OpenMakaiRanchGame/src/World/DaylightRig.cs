using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.World;

/// <summary>
/// Applies a <see cref="DaylightState"/> to the greybox's sun and world environment. Pure
/// presentation: it only writes light/environment properties from a resolved state. It owns no clock
/// and reads the phase from the shared <see cref="GameRoot"/> (or an explicit phase for tests).
/// </summary>
public partial class DaylightRig : Node3D
{
    private DirectionalLight3D? _sun;
    private WorldEnvironment? _worldEnvironment;
    private bool _wired;

    public DaylightState LastApplied { get; private set; }

    /// <summary>Bind the scene's sun + environment (the scene/controller injects these).</summary>
    public void Bind(DirectionalLight3D? sun, WorldEnvironment? worldEnvironment)
    {
        _sun = sun;
        _worldEnvironment = worldEnvironment;
        _wired = sun is not null || worldEnvironment is not null;
    }

    public bool Wired => _wired;

    /// <summary>
    /// Resolve the daylight for a phase and apply it to the bound sun/environment. Returns the applied
    /// state so callers can assert on it. Safe with no bindings (returns the state, writes nothing).
    /// </summary>
    public DaylightState Apply(DayPhase phase)
    {
        var state = DaylightMath.For(phase);

        if (_sun is not null)
        {
            _sun.LightEnergy = state.SunEnergy;
            _sun.LightColor = state.SunColor;
            // Deterministic orientation: pitch down by elevation, swing by azimuth. Night (negative
            // elevation) aims the sun away from the scene; energy is 0 anyway.
            _sun.RotationDegrees = new Vector3(-state.SunElevationDegrees, -state.SunAzimuthDegrees, 0f);
        }

        if (_worldEnvironment is not null && _worldEnvironment.Environment is not null)
        {
            var environment = _worldEnvironment.Environment;
            environment.AmbientLightColor = state.AmbientColor;
            environment.AmbientLightEnergy = state.AmbientEnergy;
            environment.TonemapExposure = state.TonemapExposure;
        }

        LastApplied = state;
        return state;
    }

    /// <summary>Apply daylight from the shared simulation's current phase (single source of truth).</summary>
    public DaylightState ApplyFrom(GameRoot? game)
    {
        var phase = game?.State.Calendar.Phase ?? DayPhase.Morning;
        return Apply(phase);
    }
}
