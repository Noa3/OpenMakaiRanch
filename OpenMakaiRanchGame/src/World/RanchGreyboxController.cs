using System;
using System.Collections.Generic;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Scene controller for the opt-in <c>scenes/dev/RanchGreybox.tscn</c>. It wires the shared
/// input gate, command dispatchers, camera target, roster/daylight presentation and the world HUD.
///
/// This node owns no simulation. Spatial interactions are translated into commands against the
/// existing <see cref="GameRoot"/>. The selected roster character is presentation state only and
/// determines which worker receives an assignment when the player uses a job station.
/// </summary>
public partial class RanchGreyboxController : Node3D
{
    [Export] public float InteractionRange { get; set; } = 2.5f;

    public WorldInputGate InputGate { get; private set; } = new();

    private readonly List<WorldStation> _stations = new();
    private ThirdPersonPlayerController? _player;
    private WorldCameraRig? _cameraRig;
    private WorldHudController? _hud;
    private Label? _legacyPrompt;
    private WorldStation? _nearbyStation;
    private float _nearbyDistance = float.PositiveInfinity;
    private int _selectedCharacterIndex;
    private bool _wired;

    public ThirdPersonPlayerController? Player => _player;
    public WorldCameraRig? CameraRig => _cameraRig;
    public WorldStation? Station => _stations.Count > 0 ? _stations[0] : null;
    public IReadOnlyList<WorldStation> Stations => _stations;
    public int StationCount => _stations.Count;
    public WorldStation? NearbyStation => _nearbyStation;
    public WorldHudController? Hud => _hud;
    public bool Wired => _wired;
    public string SelectedCharacterId => ResolveSelectedCharacterId();

    /// <summary>
    /// Presentation notification for onboarding/feedback only. Gameplay state has already been
    /// mutated through GameRoot before this event fires.
    /// </summary>
    public event Action<string, string>? StationInteractionSucceeded;

    /// <summary>Applies the shared phase to the scene's sun + environment.</summary>
    public DaylightRig? Daylight { get; private set; }

    /// <summary>Places stand-ins for the live roster from shared assignments.</summary>
    public RosterRig? Roster { get; private set; }

    /// <summary>Collision-free stylized placeholder/readability layer.</summary>
    public RanchPresentationBuilder? Presentation { get; private set; }

    public override void _Ready()
    {
        _player = GetNodeOrNull<ThirdPersonPlayerController>("Player");
        _cameraRig = GetNodeOrNull<WorldCameraRig>("CameraRig");
        _hud = GetNodeOrNull<WorldHudController>("WorldHud");

        _stations.Clear();
        CollectStations(this);

        // Shared input ownership and explicit camera target. This closes a real composition gap:
        // the camera rig previously existed as a sibling but was never bound to the player's head.
        if (_player is not null)
        {
            _player.InputGate = InputGate;
            var target = _player.EnsureCameraTarget();
            if (_cameraRig is not null)
            {
                _cameraRig.InputGate = InputGate;
                _cameraRig.Target = target;
            }
        }

        // Every authored smart object dispatches through the same GameRoot boundary.
        foreach (var station in _stations)
        {
            if (station.Dispatcher is null)
            {
                station.Dispatcher = new GameRootCommandDispatcher();
            }

            station.AvailabilityResolver = () => ResolveStationAvailability(station);
        }

        // Legacy prompt is retained for backwards-compatible scene/tests but hidden by the authored
        // scene once WorldHud is present.
        var promptLayer = GetNodeOrNull<CanvasLayer>("PromptLayer");
        if (promptLayer is not null)
        {
            _legacyPrompt = promptLayer.GetNodeOrNull<Label>("Prompt");
        }

        WireLiveWorld();
        EnsureSelectedCharacter();
        UpdateNearbyStation();
        RefreshHud();

        if (IsInsideTree() && GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged += OnSharedStateChanged;
        }

        _wired = _player is not null && _stations.Count > 0;
    }

    public override void _ExitTree()
    {
        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged -= OnSharedStateChanged;
        }
    }

    public override void _Process(double delta)
    {
        if (InputGate.WorldInputEnabled)
        {
            if (Input.IsActionJustPressed("cycle_character"))
            {
                CycleSelectedCharacter();
            }

            // Using Input.IsActionJustPressed handles real InputMap keyboard/gamepad actions. The
            // previous InputEventAction-only path did not reliably receive ordinary F-key input.
            if (Input.IsActionJustPressed("interact"))
            {
                TryInteractWithNearestStation();
            }
        }

        UpdateNearbyStation();
        RefreshHud();
    }

    /// <summary>
    /// Bind day-phase lighting + roster placement to the shared <see cref="GameRoot"/> and apply the
    /// current state. Safe when GameRoot is not active (headless/pre-boot).
    /// </summary>
    private void WireLiveWorld()
    {
        var game = GameRoot.Instance;
        if (game is null || !GodotObject.IsInstanceValid(game))
        {
            return;
        }

        var dayRig = GetNodeOrNull<DaylightRig>("DaylightRig");
        if (dayRig is not null)
        {
            var worldEnvironment = GetNodeOrNull<WorldEnvironment>("WorldEnvironment");
            if (worldEnvironment is not null && worldEnvironment.Environment is null)
            {
                // The greybox authors the WorldEnvironment node, while the controller guarantees
                // a runtime Environment resource so DaylightRig can apply ambient + tonemap values.
                worldEnvironment.Environment = new Godot.Environment();
            }

            dayRig.Bind(GetNodeOrNull<DirectionalLight3D>("Sun"), worldEnvironment);
            dayRig.ApplyFrom(game);
            Daylight = dayRig;
        }

        var rosterRig = GetNodeOrNull<RosterRig>("RosterRig");
        if (rosterRig is not null)
        {
            rosterRig.Refresh(game);
            Roster = rosterRig;
        }

        var presentation = GetNodeOrNull<RanchPresentationBuilder>("Presentation");
        if (presentation is not null)
        {
            presentation.Refresh(game);
            Presentation = presentation;
        }
    }

    private void OnSharedStateChanged()
    {
        // StateChanged is raised by assignments, time advancement, load/new game and management
        // actions. Keeping this presentation subscribed means the hidden 3D world is already current
        // when the overlay closes.
        RefreshLiveWorld();
    }

    /// <summary>
    /// Re-derive the live world from the shared simulation after phase/assignment/load changes.
    /// </summary>
    public void RefreshLiveWorld()
    {
        var game = GameRoot.Instance;
        if (game is null || !GodotObject.IsInstanceValid(game))
        {
            return;
        }

        Daylight?.ApplyFrom(game);
        Roster?.Refresh(game);
        Presentation?.Refresh(game);
        EnsureSelectedCharacter();
        UpdateNearbyStation();
        RefreshHud();
    }

    /// <summary>Cycle the worker affected by spatial job stations.</summary>
    public void CycleSelectedCharacter(int direction = 1)
    {
        var game = GameRoot.Instance;
        if (game is null || !GodotObject.IsInstanceValid(game) || game.Roster.Characters.Count == 0)
        {
            _selectedCharacterIndex = 0;
            RefreshHud();
            return;
        }

        var count = game.Roster.Characters.Count;
        var step = direction >= 0 ? 1 : -1;
        _selectedCharacterIndex = (_selectedCharacterIndex + step + count) % count;
        RefreshHud();
    }

    /// <summary>
    /// Activate the closest station when it is in range. The selected roster id is passed as the
    /// command context; the station id itself is never substituted for a character id.
    /// </summary>
    public bool TryInteractWithNearestStation()
    {
        UpdateNearbyStation();

        if (_player is null || _nearbyStation is null)
        {
            SetFeedback("No ranch station nearby.");
            return false;
        }

        if (_nearbyDistance > InteractionRange)
        {
            SetFeedback($"Move closer to {_nearbyStation.Label}.");
            return false;
        }

        if (!_nearbyStation.IsAvailable)
        {
            SetFeedback($"{_nearbyStation.Label}: {_nearbyStation.UnavailableReason}");
            return false;
        }

        var characterId = ResolveSelectedCharacterId();
        if (string.IsNullOrWhiteSpace(characterId))
        {
            SetFeedback("No roster worker is available.");
            return false;
        }

        var context = new WorldInteractionContext(characterId, ResolveGeneration());
        var ok = _nearbyStation.Activate(context);

        if (ok)
        {
            var worker = ResolveSelectedCharacterName();
            SetFeedback($"{_nearbyStation.Label}: {worker} updated.");
            StationInteractionSucceeded?.Invoke(characterId, _nearbyStation.CommandTargetId);
            RefreshLiveWorld();
        }
        else
        {
            var reason = string.IsNullOrWhiteSpace(_nearbyStation.UnavailableReason)
                ? "interaction rejected by the shared simulation"
                : _nearbyStation.UnavailableReason;
            SetFeedback($"{_nearbyStation.Label}: {reason}");
        }

        return ok;
    }

    /// <summary>
    /// Open the management UI: the world loses input ownership. Kept as the boundary used by the
    /// upcoming boot-world composition; this scene does not invent a second management shell.
    /// </summary>
    public void EnterManagementUi()
    {
        InputGate.SetUiOwnsInput(true);
        _hud?.SetStatus("Management UI owns input.");
    }

    /// <summary>Leave the management UI and deliberately return input to the world.</summary>
    public void LeaveManagementUi()
    {
        InputGate.SetUiOwnsInput(false);
        _hud?.SetStatus("World controls restored.");
    }

    private void EnsureSelectedCharacter()
    {
        var game = GameRoot.Instance;
        if (game is null || !GodotObject.IsInstanceValid(game) || game.Roster.Characters.Count == 0)
        {
            _selectedCharacterIndex = 0;
            return;
        }

        _selectedCharacterIndex = Math.Clamp(_selectedCharacterIndex, 0, game.Roster.Characters.Count - 1);
    }

    private string ResolveSelectedCharacterId()
    {
        var game = GameRoot.Instance;
        if (game is null || !GodotObject.IsInstanceValid(game) || game.Roster.Characters.Count == 0)
        {
            return string.Empty;
        }

        EnsureSelectedCharacter();
        return game.Roster.Characters[_selectedCharacterIndex].Id;
    }

    private string ResolveSelectedCharacterName()
    {
        var game = GameRoot.Instance;
        var id = ResolveSelectedCharacterId();
        if (game is null || string.IsNullOrWhiteSpace(id))
        {
            return "worker";
        }

        var character = game.Roster.Find(id);
        return character is null ? id : game.Roster.DefinitionFor(character).DisplayName;
    }

    private void UpdateNearbyStation()
    {
        _nearbyStation = null;
        _nearbyDistance = float.PositiveInfinity;

        if (_player is null)
        {
            return;
        }

        foreach (var station in _stations)
        {
            if (station is null || !GodotObject.IsInstanceValid(station))
            {
                continue;
            }

            var distance = _player.GlobalPosition.DistanceTo(station.GlobalPosition);
            if (distance < _nearbyDistance)
            {
                _nearbyDistance = distance;
                _nearbyStation = station;
            }
        }

        UpdateLegacyPrompt();
    }

    private void RefreshHud()
    {
        var game = GameRoot.Instance;
        if (_hud is null)
        {
            return;
        }

        _hud.RefreshSimulation(game);
        _hud.SetSelectedCharacter(game, ResolveSelectedCharacterId());
        _hud.SetInteractionTarget(_nearbyStation, _nearbyDistance, InteractionRange);
    }

    private void SetFeedback(string message)
    {
        _hud?.SetStatus(message);
        if (_legacyPrompt is not null)
        {
            _legacyPrompt.Text = message;
        }
    }

    private void UpdateLegacyPrompt()
    {
        if (_legacyPrompt is null)
        {
            return;
        }

        if (_nearbyStation is null)
        {
            _legacyPrompt.Text = "Explore the ranch.";
            return;
        }

        _legacyPrompt.Text = _nearbyDistance <= InteractionRange
            ? $"F: {_nearbyStation.Label}"
            : $"{_nearbyStation.Label} {_nearbyDistance:0.0}m";
    }

    private void CollectStations(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            if (child is WorldStation station)
            {
                _stations.Add(station);
            }
            CollectStations(child);
        }
    }

    private static (bool Available, string Reason) ResolveStationAvailability(WorldStation station)
    {
        if (string.IsNullOrWhiteSpace(station.RequiredFacilityId))
        {
            return (true, string.Empty);
        }

        var game = GameRoot.Instance;
        if (game is null || !GodotObject.IsInstanceValid(game))
        {
            return (false, "ranch state is unavailable");
        }

        var built = game.Ranch.Facilities.TryGetValue(station.RequiredFacilityId, out var level) && level > 0;
        if (built)
        {
            return (true, string.Empty);
        }

        var displayName = game.Data.Facilities.TryGetValue(station.RequiredFacilityId, out var definition)
            ? definition.DisplayName
            : station.Label;
        return (false, $"{displayName} is not built yet");
    }

    private ulong ResolveGeneration()
    {
        return GameRoot.Instance?.StateGeneration ?? 0UL;
    }
}
