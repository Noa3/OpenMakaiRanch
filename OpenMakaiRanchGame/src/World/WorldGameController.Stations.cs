using System;
using System.Linq;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Ui;

namespace OpenMakaiRanch.World;

/// <summary>The player-facing world entry points. The old shell remains a service renderer, not a hub.</summary>
public partial class WorldGameController
{
    private WorldStationPanel? _stationPanel;
    private WorldNavigationController? _navigationGuide;
    public WorldStationPanel? StationPanel => _stationPanel;
    public WorldNavigationController? NavigationGuide => _navigationGuide;
    public bool IsStationPanelOpen => _stationPanel?.Visible == true;
    public ThirdPersonPlayerController? ActivePlayer => _activeAreaId == "town" ? _town?.Player
        : _activeAreaId == "intro" ? _introHouse?.Player : _ranch?.Player;

    private void BindStationPresentation()
    {
        if (_ranch is null) return;
        var layer = new CanvasLayer { Name = "StationInterfaceLayer", Layer = 8 };
        AddChild(layer);
        _stationPanel = new WorldStationPanel { Name = "StationPanel" };
        layer.AddChild(_stationPanel);
        _stationPanel.Bind(this);
        _stationPanel.Closed += OnStationPanelClosed;
        _ranch.StationPanelRequested += OnStationPanelRequested;
        _navigationGuide = new WorldNavigationController { Name = "WorldNavigation", World = this };
        AddChild(_navigationGuide);
        if (_managementButton is not null)
        {
            _managementButton.Text = "Places [M]";
            _managementButton.TooltipText = "Find a place in the world. Actions are performed at its station.";
        }
    }

    private void UnbindStationPresentation()
    {
        if (GodotObject.IsInstanceValid(_ranch)) _ranch!.StationPanelRequested -= OnStationPanelRequested;
        if (GodotObject.IsInstanceValid(_stationPanel)) _stationPanel!.Closed -= OnStationPanelClosed;
    }

    private void OnStationPanelRequested(WorldStation station) => OpenStation(station);

    public bool OpenStation(WorldStation station)
    {
        if (_stationPanel is null || !WorldActionsAvailable || !CanUseStationHere(station, opening: true)) return false;
        _stationPanel.OpenStation(station.TargetId);
        ActiveEnterManagement();
        RefreshHudOwnership();
        return true;
    }

    public bool CanUseStationHere(WorldStation? station, bool opening = false)
    {
        if (station is null || !GodotObject.IsInstanceValid(station) || !station.IsInsideTree()
            || _activeAreaId != "ranch" || _ranch?.Player is not { } player || GetTree().Paused
            || _flowLocksUi || _transition?.IsTransitioning == true || GameRoot.Instance.CombatWorldTimeLocked
            || player.GlobalPosition.DistanceTo(station.GlobalPosition) > _ranch.InteractionRange + 0.05f) return false;
        if (IsGuidedOpening)
        {
            var stage = GameRoot.Instance.State.Story.FirstDayStage;
            return _firstDayFlow?.BlocksWorldInput != true && ((stage == FirstDayFlowController.StagePastureAssignment && station.CommandTargetId == "pasture")
                || (stage == FirstDayFlowController.StageManagementDairy && station.CommandTargetId == "dairy"));
        }
        return !opening || !IsManagementVisible;
    }

    public WorldStation? ResolveStation(string id) => _ranch?.Stations.FirstOrDefault(station => station.TargetId == id);

    public Node3D? ResolveDestination(WorldDestination destination, out bool viaGate)
    {
        viaGate = destination.AreaId != _activeAreaId;
        if (viaGate) return _activeAreaId == "ranch" ? _ranch?.TravelPortal
            : _activeAreaId == "town" ? _town?.ReturnPortal : null;
        if (destination.AreaId == "ranch") return ResolveStation(destination.TargetId);
        return _town?.Services.FirstOrDefault(service => service.ServiceId == destination.TargetId);
    }

    public Node3D? ResolveResidentNode(string id)
    {
        var roster = _activeAreaId == "town" ? _town?.Companion : _ranch?.Roster;
        return roster is not null && roster.TryGetAvatar(id, out var avatar) ? avatar : null;
    }

    public bool CanVisitResidentHere(string id) => !IsGuidedOpening && !_flowLocksUi && !GetTree().Paused
        && _transition?.IsTransitioning != true && !GameRoot.Instance.CombatWorldTimeLocked
        && GameRoot.Instance.Roster.Find(id) is not null && ActivePlayer is { } player
        && ResolveResidentNode(id) is { } resident && resident.IsInsideTree()
        && player.GlobalPosition.DistanceTo(resident.GlobalPosition) <= 2.6f;

    public void OpenWorldGuide()
    {
        if (_stationPanel is null || !WorldActionsAvailable || IsGuidedOpening) return;
        _stationPanel.OpenGuide();
        ActiveEnterManagement();
        RefreshHudOwnership();
    }

    private void OnStationPanelClosed()
    {
        ActiveLeaveManagement();
        RefreshWorldInputOwnership();
        RefreshHudOwnership();
    }

    // Only this entry is used by physical services and Pause. Internal shell regression fixtures
    // may still open legacy screens directly, but no player-facing global navigation leads there.
    public bool OpenDedicatedService(string screen)
    {
        if (_shell is null) return false;
        if (IsStationPanelOpen) _stationPanel!.Close();
        _shell.SetServiceContext(screen);
        if (OpenManagementScreen(screen)) return true;
        _shell.ClearServiceContext();
        return false;
    }
}
