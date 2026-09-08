using System;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Character;

namespace OpenMakaiRanch.World;

/// <summary>
/// Scene controller for the opt-in <c>scenes/dev/RanchGreybox.tscn</c>. It wires the shared
/// input gate, the command dispatcher, and the interact prompt, and handles the single
/// "interact" action: find the nearest in-range station and dispatch through the guard.
///
/// This node owns no simulation. It only routes input to the controller / camera / station
/// and surfaces a prompt. All game effects flow through <see cref="GameRoot"/>.
/// </summary>
public partial class RanchGreyboxController : Node3D
{
    [Export] public float InteractionRange { get; set; } = 3.0f;

    // ── Travel (WORLD-TOWN-003) ─────────────────────────────────────────
    /// <summary>Stable area id this root represents (e.g. "ranch", "town").</summary>
    [Export] public string AreaId { get; set; } = "ranch";

    /// <summary>Where the player stands when arriving in this area.</summary>
    [Export] public Vector3 EntryPosition { get; set; } = new Vector3(0f, 1.6f, 13f);

    /// <summary>Camera yaw (degrees) when arriving — 180 frames the area centre from the entry.</summary>
    [Export] public float EntryYawDegrees { get; set; } = 180f;

    /// <summary>Camera pitch (degrees) when arriving.</summary>
    [Export] public float EntryPitchDegrees { get; set; } = 14f;

    /// <summary>Camera distance from the player when arriving.</summary>
    [Export] public float EntryDistance { get; set; } = 8f;

    /// <summary>Camera position matching the entry player position (derived from the rig math).</summary>
    public Vector3 EntryCameraPosition
    {
        get
        {
            return WorldCameraMath.ComputeCameraPosition(EntryPosition, Mathf.DegToRad(EntryYawDegrees), Mathf.DegToRad(EntryPitchDegrees), EntryDistance);
        }
    }

    /// <summary>Re-derive this area's daylight + roster placement from the shared <see cref="GameRoot"/> (called when the area becomes active).</summary>
    public void RefreshFromGame() => WireLiveWorld();

    public WorldInputGate InputGate { get; private set; } = new();

    private ThirdPersonPlayerController? _player;
    private Label? _prompt;
    private Button? _openManagementButton;
    private bool _wired;
    private readonly System.Collections.Generic.List<WorldTravelGate> _travelGates = new();
    private readonly System.Collections.Generic.List<WorldStation> _stations = new();

    public ThirdPersonPlayerController? Player => _player;
    /// <summary>The first service station in this area (or null). Prefer <see cref="Stations"/> for multi-station areas.</summary>
    public WorldStation? Station => _stations.Count > 0 ? _stations[0] : null;
    /// <summary>All service stations in this area.</summary>
    public System.Collections.Generic.IReadOnlyList<WorldStation> Stations => _stations;
    public bool Wired => _wired;

    /// <summary>The travel gates in this area (world smart objects for area-to-area traversal).</summary>
    public System.Collections.Generic.IReadOnlyList<WorldTravelGate> TravelGates => _travelGates;

    /// <summary>
    /// The presentation handler that performs area travel. Bound to the nearest
    /// <see cref="ITravelHandler"/> ancestor (the composition) when not set explicitly.
    /// </summary>
    public ITravelHandler? TravelHandler { get; set; }

    /// <summary>Raised when the player presses the in-world "Open Management UI" button. The
    /// composition (RanchWorldController) listens and shows the management overlay; the gate is
    /// flipped here so the world input suspends regardless of the host.</summary>
    public event Action? ManagementUiRequested;

    /// <summary>Applies the shared phase to the scene's sun + environment (WORLD-003 lighting).</summary>
    public DaylightRig? Daylight { get; private set; }

    /// <summary>Places CHAR-001 stand-ins for the live roster (WORLD-003 / AI-001 placement).</summary>
    public RosterRig? Roster { get; private set; }

    public override void _Ready()
    {
        _player = GetNodeOrNull<ThirdPersonPlayerController>("Player");
        CollectStations();

        // Shared input gate: the player reads it, the controller drives it.
        if (_player is not null)
        {
            _player.InputGate = InputGate;
        }

        // The camera rig must be suspended too when the management UI owns input.
        if (GetNodeOrNull<WorldCameraRig>("CameraRig") is { } cameraRig)
        {
            cameraRig.InputGate = InputGate;
        }

        // Production dispatcher binding every station to GameRoot.
        foreach (var st in _stations)
        {
            if (st is not null && st.Dispatcher is null)
            {
                st.Dispatcher = new GameRootCommandDispatcher();
            }
        }

        // Travel gates: collect every WorldTravelGate in this area and bind the shared handler.
        CollectTravelGates();

        // Prompt (optional; the scene may author it).
        var promptLayer = GetNodeOrNull<CanvasLayer>("PromptLayer");
        if (promptLayer is not null)
        {
            _prompt = promptLayer.GetNodeOrNull<Label>("Prompt");
        }

        // WORLD-003: the scene is a live view of the shared simulation, not a static greybox.
        // Lighting derives from the current DayPhase; roster placement derives from assignments.
        WireLiveWorld();

        _wired = true;
    }

    /// <summary>
    /// Collect every <see cref="WorldStation"/> in this area (recursively) and bind the shared
    /// dispatcher. Unifies single-station (ranch) and multi-station (town) areas.
    /// </summary>
    private void CollectStations()
    {
        _stations.Clear();
        foreach (var child in GetChildren())
        {
            if (child is WorldStation st)
            {
                _stations.Add(st);
            }
        }
    }

    /// <summary>
    /// Collect every <see cref="WorldTravelGate"/> in this area and bind the shared travel handler
    /// (the nearest <see cref="ITravelHandler"/> ancestor — the composition). A gate that is not
    /// inside the tree at _Ready (authored later) is picked up on the next call.
    /// </summary>
    private void CollectTravelGates()
    {
        _travelGates.Clear();
        foreach (var child in GetChildren())
        {
            if (child is WorldTravelGate gate)
            {
                if (gate.Handler is null)
                {
                    gate.Handler = ResolveTravelHandler();
                }
                _travelGates.Add(gate);
            }
        }
    }

    /// <summary>Find the nearest <see cref="ITravelHandler"/> ancestor (the composition root).</summary>
    private ITravelHandler? ResolveTravelHandler()
    {
        var node = GetParent();
        while (node is not null)
        {
            if (node is ITravelHandler handler)
            {
                return handler;
            }
            node = node.GetParent();
        }
        return null;
    }

    /// <summary>
    /// Bind the day-phase lighting + roster placement to the shared <see cref="GameRoot"/> and
    /// apply them for the current state. Safe when a rig or the GameRoot is missing (headless,
    /// pre-boot) — the scene then simply stays in its authored state.
    /// </summary>
    private void WireLiveWorld()
    {
        var game = GameRoot.Instance;
        if (game is null || !GodotObject.IsInstanceValid(game))
        {
            return;
        }

        // React to every shared-simulation change (phase advance, settlement, assignment,
        // mentorship, load/new game) so the scene is a live view across the whole day —
        // no manual refresh needed from any caller (WORLD-003d).
        game.StateChanged += OnSharedStateChange;

        var dayRig = GetNodeOrNull<DaylightRig>("DaylightRig");
        if (dayRig is not null)
        {
            dayRig.Bind(GetNodeOrNull<DirectionalLight3D>("Sun"), GetNodeOrNull<WorldEnvironment>("WorldEnvironment"));
            dayRig.ApplyFrom(game);
            Daylight = dayRig;
        }

        var rosterRig = GetNodeOrNull<RosterRig>("RosterRig");
        if (rosterRig is not null)
        {
            rosterRig.Refresh(game);
            Roster = rosterRig;
        }
    }

    private void OnSharedStateChange()
    {
        if (!GodotObject.IsInstanceValid(this))
        {
            // The node was freed without going through _ExitTree (e.g. a headless test drives
            // _Ready manually, then Free()). Detach so we don't linger on the shared event.
            if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
            {
                game.StateChanged -= OnSharedStateChange;
            }
            return;
        }
        RefreshLiveWorld();
    }

    public override void _ExitTree()
    {
        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged -= OnSharedStateChange;
        }
    }

    /// <summary>
    /// Re-derive the live world from the shared simulation — call when the phase or assignments
    /// change (e.g. after End Day, after a world interaction, after loading).
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
    }

    /// <summary>
    /// Open the management UI: the world loses input ownership. Called by the scene's
    /// "Open Management UI" button (TSCN connection) and re-broadcast via
    /// <see cref="ManagementUiRequested"/> so the composition can show the overlay.
    /// </summary>
    public void EnterManagementUi()
    {
        InputGate.SetUiOwnsInput(true);
    }

    /// <summary>
    /// Leave the management UI: the world regains input ownership.
    /// </summary>
    public void LeaveManagementUi()
    {
        InputGate.SetUiOwnsInput(false);
    }

    /// <summary>
    /// The in-world "Open Management UI" button handler (authored TSCN connection): flip the
    /// shared gate and notify the composition so it can reveal the management overlay.
    /// </summary>
    public void RequestManagementUi()
    {
        EnterManagementUi();
        ManagementUiRequested?.Invoke();
    }

    public override void _Input(InputEvent @event)
    {
        if (!InputGate.WorldInputEnabled)
        {
            return;
        }

        if (@event is InputEventAction action && action.Pressed && action.Action == "interact")
        {
            HandleInteract();
        }
    }

    public void HandleInteract()
    {
        if (_player is null)
        {
            return;
        }

        var generation = ResolveGeneration();
        var (bestDistance, station, avatar, travelGate, label) = FindNearestInteractable();

        if (bestDistance > InteractionRange)
        {
            return; // no target in range
        }

        bool ok;
        if (station is not null)
        {
            ok = station.Activate(new WorldInteractionContext(station.TargetId, generation));
        }
        else if (travelGate is not null)
        {
            ok = travelGate.Activate(new WorldInteractionContext(travelGate.TargetId, generation));
            // A successful travel re-parents/represents the player; clear the cooldown so the
            // next area's prompt shows immediately (the gate itself leaves the tree on swap).
        }
        else
        {
            ok = avatar!.Activate(new WorldInteractionContext(avatar.CharacterId!, generation));
        }

        if (_prompt is not null)
        {
            _prompt.Text = ok ? $"{label}: done" : $"{label}: unavailable";
            _promptCooldown = 1.5f; // hold the result message briefly (UI-002)
        }
    }

    /// <summary>
    /// Find the nearest in-range interactable: a service <see cref="WorldStation"/>, an NPC
    /// <see cref="CharacterAvatar3D"/>, or a <see cref="WorldTravelGate"/>. Returns
    /// (distance, station, avatar, travelGate, label). distance &gt; InteractionRange when none in
    /// range. At most one of station/avatar/travelGate is non-null.
    /// </summary>
    private (float distance, WorldStation? station, CharacterAvatar3D? avatar, WorldTravelGate? travelGate, string label) FindNearestInteractable()
    {
        if (_player is null)
        {
            return (float.MaxValue, null, null, null, string.Empty);
        }

        var bestDistance = float.MaxValue;
        WorldStation? bestStation = null;
        CharacterAvatar3D? bestAvatar = null;
        WorldTravelGate? bestGate = null;
        string label = string.Empty;

        void Consider(float distance, bool available, WorldStation? st, CharacterAvatar3D? av, WorldTravelGate? gate, string lab)
        {
            if (!available || distance > InteractionRange || distance >= bestDistance)
            {
                return;
            }

            bestDistance = distance;
            bestStation = st;
            bestAvatar = av;
            bestGate = gate;
            label = lab;
        }

        // Service stations (one or many).
        foreach (var st in _stations)
        {
            if (st is null || !st.IsAvailable) continue;
            Consider(_player.GlobalPosition.DistanceTo(st.GlobalPosition), true, st, null, null, st.Label);
        }

        // Travel gates (area-to-area traversal).
        foreach (var gate in _travelGates)
        {
            if (!gate.IsAvailable) continue;
            Consider(_player.GlobalPosition.DistanceTo(gate.GlobalPosition), true, null, null, gate, gate.Label);
        }

        // Nearest NPC avatar
        if (Roster is not null)
        {
            var game = GameRoot.Instance;
            if (game is not null && GodotObject.IsInstanceValid(game))
            {
                foreach (var id in Roster.AvatarIds)
                {
                    var avatar = Roster.GetAvatar(id);
                    if (avatar is null || !avatar.IsInteractable) continue;
                    if (avatar.Dispatcher is null)
                    {
                        avatar.Dispatcher = new GameRootCommandDispatcher();
                    }
                    var ad = _player.GlobalPosition.DistanceTo(avatar.GlobalPosition);
                    Consider(ad, true, null, avatar, null, avatar.DisplayName);
                }
            }
        }

        return (bestDistance, bestStation, bestAvatar, bestGate, label);
    }

    // ── UI-002: in-world interaction prompt ──────────────────────────────

    private float _promptCooldown;

    public override void _Process(double delta)
    {
        if (_prompt is null || _player is null) return;

        // After a successful/failed interaction, hold the "done"/"unavailable"
        // message briefly, then return to the proximity prompt.
        if (_promptCooldown > 0f)
        {
            _promptCooldown -= (float)delta;
            return;
        }

        var (distance, station, avatar, travelGate, label) = FindNearestInteractable();
        _prompt.Text = distance <= InteractionRange
            ? $"Press F — {label}"
            : string.Empty;
    }

    private ulong ResolveGeneration()
    {
        return GameRoot.Instance?.StateGeneration ?? 0UL;
    }
}
