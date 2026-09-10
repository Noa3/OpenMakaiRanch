using System;
using System.Collections.Generic;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Playable 3D Okachi Town shell. It handles movement, spatial service discovery and travel requests
/// only. Every actual town action still executes in the existing shared UI/services.
/// </summary>
public partial class TownWorldController : Node3D
{
    [Export] public float InteractionRange { get; set; } = 2.5f;

    public WorldInputGate InputGate { get; } = new();

    private readonly List<TownServicePoint> _services = new();
    private ThirdPersonPlayerController? _player;
    private WorldCameraRig? _cameraRig;
    private TownHudController? _hud;
    private WorldTravelPortal? _returnPortal;
    private TownServicePoint? _nearbyService;
    private float _nearbyServiceDistance = float.PositiveInfinity;
    private string _nearbyCompanionId = string.Empty;
    private float _nearbyCompanionDistance = float.PositiveInfinity;
    private float _returnPortalDistance = float.PositiveInfinity;
    private Button? _returnRanchButton;

    public DaylightRig? Daylight { get; private set; }
    public TownPresentationBuilder? Presentation { get; private set; }
    public RosterRig? Companion { get; private set; }

    public ThirdPersonPlayerController? Player => _player;
    public WorldCameraRig? CameraRig => _cameraRig;
    public TownHudController? Hud => _hud;
    public IReadOnlyList<TownServicePoint> Services => _services;
    public WorldTravelPortal? ReturnPortal => _returnPortal;

    public event Action<string>? ServiceScreenRequested;
    public event Action<string>? CharacterInteractionRequested;
    public event Action<string>? TravelRequested;

    public override void _Ready()
    {
        _player = GetNodeOrNull<ThirdPersonPlayerController>("Player");
        _cameraRig = GetNodeOrNull<WorldCameraRig>("CameraRig");
        _hud = GetNodeOrNull<TownHudController>("TownHud");
        _returnPortal = GetNodeOrNull<WorldTravelPortal>("TravelToRanch");
        _returnRanchButton = GetNodeOrNull<Button>("TownHud/ReturnRanchButton");

        _services.Clear();
        CollectServices(this);

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

        foreach (var service in _services)
        {
            service.AvailabilityResolver = () => ResolveAvailability(service.RequiredFacilityId, service.Label);
        }

        if (_returnRanchButton is not null)
        {
            _returnRanchButton.Pressed += RequestReturnToRanch;
        }

        WireDaylight();
        Presentation = GetNodeOrNull<TownPresentationBuilder>("Presentation");
        Companion = GetNodeOrNull<RosterRig>("CompanionRig");
        Companion?.BindFollowTarget(_player);

        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged += OnSharedStateChanged;
        }

        Refresh();
    }

    public override void _ExitTree()
    {
        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged -= OnSharedStateChanged;
        }

        if (_returnRanchButton is not null && GodotObject.IsInstanceValid(_returnRanchButton))
        {
            _returnRanchButton.Pressed -= RequestReturnToRanch;
        }
    }

    public override void _Process(double delta)
    {
        if (InputGate.WorldInputEnabled && Input.IsActionJustPressed("interact"))
        {
            TryInteract();
        }

        UpdateNearbyTargets();
        RefreshHud();
    }

    public void EnterManagementUi() => InputGate.SetUiOwnsInput(true);
    public void LeaveManagementUi() => InputGate.SetUiOwnsInput(false);

    public void Refresh()
    {
        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            Daylight?.ApplyFrom(game);
            Presentation?.Refresh(game);
            Companion?.BindFollowTarget(_player);
            Companion?.Refresh(game);
        }

        UpdateNearbyTargets();
        RefreshHud();
    }

    public bool TryInteract()
    {
        UpdateNearbyTargets();

        var portalInRange = _returnPortal is not null && _returnPortalDistance <= InteractionRange;
        var serviceInRange = _nearbyService is not null && _nearbyServiceDistance <= InteractionRange;
        var companionInRange = !string.IsNullOrWhiteSpace(_nearbyCompanionId)
            && _nearbyCompanionDistance <= InteractionRange;

        if (companionInRange
            && (!portalInRange || _nearbyCompanionDistance < _returnPortalDistance)
            && (!serviceInRange || _nearbyCompanionDistance < _nearbyServiceDistance))
        {
            CharacterInteractionRequested?.Invoke(_nearbyCompanionId);
            return true;
        }

        if (portalInRange
            && (!companionInRange || _returnPortalDistance <= _nearbyCompanionDistance)
            && (!serviceInRange || _returnPortalDistance <= _nearbyServiceDistance))
        {
            TravelRequested?.Invoke(_returnPortal!.DestinationId);
            return true;
        }

        if (!serviceInRange)
        {
            _hud?.SetStatus("Move closer to a town building or the ranch gate.");
            return false;
        }

        if (!_nearbyService!.IsAvailable)
        {
            _hud?.SetStatus($"{_nearbyService.Label}: {_nearbyService.UnavailableReason}");
            return false;
        }

        ServiceScreenRequested?.Invoke(_nearbyService.ScreenId);
        return true;
    }

    private void RequestReturnToRanch()
    {
        TravelRequested?.Invoke("ranch");
    }

    private void WireDaylight()
    {
        var game = GameRoot.Instance;
        if (game is null || !GodotObject.IsInstanceValid(game))
        {
            return;
        }

        var rig = GetNodeOrNull<DaylightRig>("DaylightRig");
        if (rig is null)
        {
            return;
        }

        var worldEnvironment = GetNodeOrNull<WorldEnvironment>("WorldEnvironment");
        if (worldEnvironment is not null && worldEnvironment.Environment is null)
        {
            worldEnvironment.Environment = new Godot.Environment();
        }

        rig.Bind(GetNodeOrNull<DirectionalLight3D>("Sun"), worldEnvironment);
        rig.ApplyFrom(game);
        Daylight = rig;
    }

    private void UpdateNearbyTargets()
    {
        _nearbyService = null;
        _nearbyServiceDistance = float.PositiveInfinity;
        _nearbyCompanionId = string.Empty;
        _nearbyCompanionDistance = float.PositiveInfinity;
        _returnPortalDistance = float.PositiveInfinity;

        if (_player is null)
        {
            return;
        }

        foreach (var service in _services)
        {
            var distance = _player.GlobalPosition.DistanceTo(service.GlobalPosition);
            if (distance < _nearbyServiceDistance)
            {
                _nearbyService = service;
                _nearbyServiceDistance = distance;
            }
        }

        if (Companion is not null
            && Companion.TryFindNearest(_player.GlobalPosition, InteractionRange * 1.5f,
                out var companionId, out _, out var companionDistance))
        {
            _nearbyCompanionId = companionId;
            _nearbyCompanionDistance = companionDistance;
        }

        if (_returnPortal is not null)
        {
            _returnPortalDistance = _player.GlobalPosition.DistanceTo(_returnPortal.GlobalPosition);
        }
    }

    private void RefreshHud()
    {
        if (_hud is null)
        {
            return;
        }

        _hud.Refresh(GameRoot.Instance);

        var companionIsClosest = !string.IsNullOrWhiteSpace(_nearbyCompanionId)
            && (_nearbyService is null || _nearbyCompanionDistance < _nearbyServiceDistance)
            && (_returnPortal is null || _nearbyCompanionDistance < _returnPortalDistance);

        var portalIsClosest = !companionIsClosest
            && _returnPortal is not null
            && (_nearbyService is null || _returnPortalDistance <= _nearbyServiceDistance);

        if (companionIsClosest)
        {
            var character = GameRoot.Instance?.Roster.Find(_nearbyCompanionId);
            var name = character is null
                ? _nearbyCompanionId
                : GameRoot.Instance!.Roster.DefinitionFor(character).DisplayName;
            _hud.SetCompanionPrompt(name, _nearbyCompanionDistance, InteractionRange);
        }
        else if (portalIsClosest)
        {
            _hud.SetTravelPrompt(_returnPortal, _returnPortalDistance, InteractionRange);
        }
        else
        {
            _hud.SetServicePrompt(_nearbyService, _nearbyServiceDistance, InteractionRange);
        }
    }

    private static (bool Available, string Reason) ResolveAvailability(string requiredFacilityId, string label)
    {
        if (string.IsNullOrWhiteSpace(requiredFacilityId))
        {
            return (true, string.Empty);
        }

        var game = GameRoot.Instance;
        if (game is null || !GodotObject.IsInstanceValid(game))
        {
            return (false, "Ranch state unavailable.");
        }

        var built = game.Ranch.Facilities.TryGetValue(requiredFacilityId, out var level) && level > 0;
        if (built)
        {
            return (true, string.Empty);
        }

        var facilityName = game.Data.Facilities.TryGetValue(requiredFacilityId, out var facility)
            ? facility.DisplayName
            : requiredFacilityId;
        return (false, $"{facilityName} must be built before {label} is available.");
    }

    private void OnSharedStateChanged() => Refresh();

    private void CollectServices(Node node)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is TownServicePoint service)
            {
                _services.Add(service);
            }
            CollectServices(child);
        }
    }
}
