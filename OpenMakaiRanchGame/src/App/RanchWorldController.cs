using Godot;
using OpenMakaiRanch.Ui;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.App;

/// <summary>
/// Composes the 3D world areas (ranch + town, …) and the existing 2D management UI into one boot
/// world, all on the shared <see cref="GameRoot"/> (autoload — a single simulation).
///
/// Design (no second economy / clock / job path):
///   - Each authored area (<c>scenes/dev/RanchGreybox.tscn</c>, <c>scenes/Town.tscn</c>, …) is a
///     self-contained <see cref="RanchGreyboxController"/> root: its own player, camera, stations,
///     day-phase lighting, roster stand-ins, and travel gates — all derived from the shared state.
///   - The management UI is the *existing* <c>scenes/Game.tscn</c> (the tested 2D game: economy,
///     clock, jobs, save/load) — not a duplicate. It opens as a full-viewport overlay.
///   - <b>Travel is presentation</b>: <see cref="TravelTo"/> switches which area is active
///     (visible + processed) and repositions that area's player to its entry point. It spends no
///     gold, advances no clock, and grants nothing — so it lives here, not in the GameRoot command
///     boundary. This is the "natural traversal / gate" the world-design goal calls for.
///   - World input ownership stays in the active area controller's <see cref="WorldInputGate"/>.
///     This composition drives it through the tested <c>EnterManagementUi</c> /
///     <c>LeaveManagementUi</c>, so opening management suspends world input and closing resumes it
///     safely — one gate per area, never left dangling.
/// </summary>
public partial class RanchWorldController : Node3D, ITravelHandler
{
    [Export] public NodePath ManagementOverlayPath { get; set; } = "ManagementCanvas/Game";

    /// <summary>The area the player must be in at boot (default: the ranch).</summary>
    [Export] public string InitialAreaId { get; set; } = "ranch";

    public enum Mode { World, Management }

    public Mode CurrentMode { get; private set; } = Mode.World;
    public bool ManagementOpen => CurrentMode == Mode.Management;

    /// <summary>The active area id (e.g. "ranch", "town") — <see cref="ITravelHandler.ActiveAreaId"/>.</summary>
    public string? ActiveAreaId => _activeArea?.AreaId;

    bool ITravelHandler.TravelTo(string areaId) => TravelTo(areaId, 0);

    /// <summary>The primary (ranch) input gate — exposed for tests/UI (stable across areas).</summary>
    public WorldInputGate? InputGate => _primaryArea?.InputGate;

    private readonly System.Collections.Generic.List<RanchGreyboxController> _areas = new();
    private RanchGreyboxController? _primaryArea;
    private RanchGreyboxController? _activeArea;
    private CanvasItem? _overlay;
    private UiShellController? _uiShell;
    private bool _bootInManagement;

    public override void _EnterTree()
    {
        // Parent _EnterTree runs before children, so the pending screen is still set here.
        _bootInManagement = GameRoot.PendingInitialScreen is not null;
    }

    public override void _Ready()
    {
        CollectAreas();

        _overlay = GetNodeOrNull<CanvasItem>(ManagementOverlayPath);
        _uiShell = GetNodeOrNull<UiShellController>(ManagementOverlayPath + "/UiShell");

        foreach (var area in _areas)
        {
            if (area is null) continue;
            area.ManagementUiRequested += HandleManagementUiRequested;
        }

        var initial = FindArea(ResolveInitialAreaId()) ?? _primaryArea;
        SetActiveArea(initial, reposition: true);

        if (_overlay is not null)
            _overlay.Visible = _bootInManagement;

        if (_bootInManagement)
        {
            _activeArea?.EnterManagementUi();
            CurrentMode = Mode.Management;
        }
        else
        {
            CurrentMode = Mode.World;
        }
    }

    private void CollectAreas()
    {
        _areas.Clear();
        foreach (var child in GetChildren())
        {
            if (child is RanchGreyboxController area)
                _areas.Add(area);
        }

        _primaryArea = null;
        foreach (var area in _areas)
        {
            if (string.Equals(area.AreaId, "ranch", System.StringComparison.OrdinalIgnoreCase))
            {
                _primaryArea = area;
                break;
            }
        }

        _primaryArea ??= _areas.Count > 0 ? _areas[0] : null;
        if (_primaryArea is null)
            GD.PushError("RanchWorldController: no 3D world area found (expected RanchGreyboxController children).");
    }

    // ── ITravelHandler ──────────────────────────────────────────────────

    /// <summary>
    /// Travel the player to another authored area. Pure presentation: swaps visibility/process,
    /// repositions the destination's player to its entry point, and re-derives that area's
    /// lighting/roster from the shared state. No gold/clock/job change.
    /// </summary>
    public bool TravelTo(string areaId, ulong _expectedGeneration = 0)
    {
        if (string.IsNullOrEmpty(areaId))
            return false;

        var area = FindArea(areaId);
        if (area is null)
        {
            GD.PushWarning($"RanchWorldController: travel to unknown area '{areaId}' ignored");
            return false;
        }

        if (ReferenceEquals(area, _activeArea))
            return false;

        SetActiveArea(area, reposition: true);

        // Canonical persisted location. The dev merge previously referenced Player.CurrentArea,
        // while the current production world/save path uses SaveState.WorldAreaId.
        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
            game.State.WorldAreaId = area.AreaId;

        return true;
    }

    private RanchGreyboxController? FindArea(string areaId)
    {
        foreach (var area in _areas)
        {
            if (area is not null && string.Equals(area.AreaId, areaId, System.StringComparison.OrdinalIgnoreCase))
                return area;
        }
        return null;
    }

    /// <summary>
    /// Resolve the initial area id from the canonical saved world location, then fall back to the
    /// authored <see cref="InitialAreaId"/>. This avoids maintaining two competing area fields.
    /// </summary>
    private string ResolveInitialAreaId()
    {
        var saved = GameRoot.Instance?.State?.WorldAreaId;
        if (!string.IsNullOrWhiteSpace(saved) && FindArea(saved) is not null)
            return saved;
        return InitialAreaId;
    }

    private void SetActiveArea(RanchGreyboxController? area, bool reposition)
    {
        if (area is null)
            return;

        foreach (var candidate in _areas)
        {
            if (candidate is null)
                continue;
            var active = ReferenceEquals(candidate, area);
            candidate.Visible = active;
            candidate.ProcessMode = active ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
        }

        _activeArea = area;
        area.RefreshFromGame();

        if (reposition && area.Player is not null)
            area.Player.GlobalPosition = area.EntryPosition;
    }

    // ── Management UI ───────────────────────────────────────────────────

    public bool EnterManagement()
    {
        if (CurrentMode == Mode.Management)
            return true;
        if (_activeArea is null)
            return false;

        _activeArea.EnterManagementUi();
        if (_overlay is not null)
            _overlay.Visible = true;
        _uiShell?.ShowScreen("ranch");
        CurrentMode = Mode.Management;
        return true;
    }

    public bool ReturnToWorld()
    {
        if (CurrentMode == Mode.World)
            return true;

        _activeArea?.LeaveManagementUi();
        if (_overlay is not null)
            _overlay.Visible = false;
        CurrentMode = Mode.World;
        return true;
    }

    public bool ToggleManagement()
    {
        return CurrentMode == Mode.World ? EnterManagement() : ReturnToWorld();
    }

    private void HandleManagementUiRequested()
    {
        if (CurrentMode == Mode.Management)
            return;
        if (_overlay is not null)
            _overlay.Visible = true;
        _uiShell?.ShowScreen("ranch");
        CurrentMode = Mode.Management;
    }

    public override void _ExitTree()
    {
        foreach (var area in _areas)
        {
            if (area is not null)
                area.ManagementUiRequested -= HandleManagementUiRequested;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            _ = ReturnToWorld();
            GetViewport().SetInputAsHandled();
        }
    }
}