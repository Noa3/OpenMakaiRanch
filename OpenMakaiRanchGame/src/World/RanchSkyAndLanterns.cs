using System;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Visuals;

namespace OpenMakaiRanch.World;

/// <summary>
/// Additive ranch-camera presentation. Copies the canonical environment AFTER its owners refresh;
/// never changes the global WorldEnvironment or another location's camera. No simulation writes.
/// </summary>
public partial class RanchSkyAndLanterns : Node3D
{
    [Export] public bool Enabled { get; set; } = true;
    [Export] public bool ShowCompanionWorld { get; set; } = true;
    private PastoralSky? _sky;
    private WorldEnvironment? _source;
    private readonly WorldEnvironment _holder = new(); // Not in the tree: never registers a global environment.
    private Camera3D? _camera;
    private Godot.Environment? _owned;
    private DirectionalLight3D? _sun;
    private WorldAtmosphereController? _atmosphere;
    private GameRoot? _game;
    private readonly ManaStoneLantern3D[] _lamps = new ManaStoneLantern3D[2];
    private bool _queued, _yielded;
    private (DayPhase Phase, Weather Weather, string Quality, bool Shadows, bool Sheltered, bool Enabled, bool Planet)? _last;
    public bool SkyActive => _sky?.Bound == true && _camera?.Environment == _owned;
    public int LampCount { get; private set; }

    public override void _Ready() => Callable.From(Initialize).CallDeferred();
    private void Initialize()
    {
        if (!IsInsideTree()) return;
        var root = GetParent();
        _source = root.GetNodeOrNull<WorldEnvironment>("WorldEnvironment");
        _camera = root.GetNodeOrNull<Camera3D>("CameraRig/Camera");
        _sun = root.GetNodeOrNull<DirectionalLight3D>("Sun");
        _atmosphere = root.GetNodeOrNull<WorldAtmosphereController>("Atmosphere");
        _sky = new PastoralSky();
        for (var i = 0; i < _lamps.Length; i++) {
            _lamps[i] = new ManaStoneLantern3D { Name = "GateManaLantern" + i, Position = new Vector3(i == 0 ? -4f : 4f, 0f, 17.5f) };
            AddChild(_lamps[i]); LampCount++;
        }
        _game = GameRoot.Instance;
        if (_game is not null && GodotObject.IsInstanceValid(_game)) _game.StateChanged += QueueRefresh;
        Refresh(true);
    }
    private void QueueRefresh()
    {
        if (_queued || !IsInsideTree()) return;
        _queued = true;
        // Canonical DaylightRig handlers finish first, regardless of signal subscription order.
        Callable.From(() => { _queued = false; if (IsInsideTree()) Refresh(true); }).CallDeferred();
    }
    public override void _Process(double delta) => Refresh(false);
    private void Refresh(bool sourceChanged)
    {
        if (_sky is null || _game is null || !GodotObject.IsInstanceValid(_game)) return;
        var cal = _game.State.Calendar;
        var sheltered = _atmosphere?.IsPlayerSheltered ?? false;
        var current = (Phase: cal.Phase, Weather: cal.CurrentWeather, Quality: _game.State.Settings.GraphicsQuality,
            Shadows: _game.RuntimeSettings.EffectiveShadowsEnabled, Sheltered: sheltered, Enabled, Planet: ShowCompanionWorld);
        if (!sourceChanged && _last == current) return;
        var night = cal.Phase switch { DayPhase.Night => 1f, DayPhase.Evening => .48f, _ => 0f };
        if (!Enabled) ReleaseCamera();
        else if (_camera is not null && GodotObject.IsInstanceValid(_camera) && _source?.Environment is { } source)
        {
            if (_last is { Enabled: false }) _yielded = false;
            if (_owned is not null && _camera.Environment != _owned) { _yielded = true; ReleaseCamera(); }
            if (!_yielded && (_camera.Environment is null || _camera.Environment == _owned))
            {
                ReleaseCamera();
                // A phase/settings change makes a fresh camera-local snapshot, not a second lighting authority.
                _holder.Environment = (Godot.Environment)source.Duplicate();
                if (_sky.TryBind(_holder)) {
                    var clouds = cal.CurrentWeather switch { Weather.Clear or Weather.StrongWind => .24f, Weather.Cloudy => .8f, _ => 1f };
                    _sky.Configure(night, clouds, _sun?.GlobalBasis.Z ?? new Vector3(.2f,.7f,-.6f), ShowCompanionWorld);
                    _owned = _holder.Environment; _camera.Environment = _owned;
                }
            }
            else _yielded = true; // Never overwrite an authored/externally assigned camera environment.
        }
        foreach (var lamp in _lamps) { lamp.Visible = Enabled; lamp.Configure(night,current.Quality,current.Shadows,sheltered,Enabled); }
        _last = current;
    }
    private void ReleaseCamera()
    {
        if (_camera is not null && GodotObject.IsInstanceValid(_camera) && _owned is not null && _camera.Environment == _owned)
            _camera.Environment = null;
        _sky?.Dispose(); _owned = null;
    }
    public override void _ExitTree()
    {
        if (_game is not null && GodotObject.IsInstanceValid(_game)) _game.StateChanged -= QueueRefresh;
        ReleaseCamera(); _holder.Free();
    }
}
