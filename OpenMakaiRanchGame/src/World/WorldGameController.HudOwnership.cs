using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>One presentation owner for area HUDs and world-facing actions. No simulation state.</summary>
public partial class WorldGameController
{
    private bool _hudOwnershipBound;
    private WorldTutorialController? _ranchHelp;
    private TownTutorialController? _townHelp;

    public bool WorldHelpVisible => _ranchHelp?.HelpVisible == true || _townHelp?.HelpVisible == true;
    private bool OrdinaryInterfaceAvailable => IsInsideTree() && !GetTree().Paused && !IsManagementVisible
        && !_flowLocksUi && _pauseMenu?.IsOpen != true && _transition?.IsTransitioning != true
        && _firstDayFlow?.BlocksWorldInput != true && GameRoot.Instance?.CombatWorldTimeLocked != true;

    public bool CanOpenWorldHelp => OrdinaryInterfaceAvailable && _activeAreaId is "ranch" or "town";

    public bool WorldActionsAvailable => CanOpenWorldHelp && !WorldHelpVisible
        && (_activeAreaId == "town" ? _town?.InputGate.WorldInputEnabled : _ranch?.InputGate.WorldInputEnabled) == true;

    // Story code also advances the shared clock in the intro. Player-facing HUD commands apply
    // their stricter active-area/tutorial gate before calling this common, UI-locked boundary.
    private bool WorldClockCommandAvailable => OrdinaryInterfaceAvailable && !WorldHelpVisible
        && (_activeAreaId == "intro" ? _introHouse?.InputGate.WorldInputEnabled
            : _activeAreaId == "town" ? _town?.InputGate.WorldInputEnabled : _ranch?.InputGate.WorldInputEnabled) == true;

    public void RefreshWorldInputOwnership() => SetTransitionInputLock(_transition?.IsTransitioning == true);

    private void BindHudOwnership()
    {
        if (_hudOwnershipBound) return;
        _hudOwnershipBound = true;
        _ranchHelp = _ranch?.GetNodeOrNull<WorldTutorialController>("WorldHud/TutorialOverlay");
        _townHelp = _town?.GetNodeOrNull<TownTutorialController>("TownHud/TutorialOverlay");
        if (_ranch is not null) _ranch.InputGate.InputStateDidChange += RefreshHudOwnership;
        if (_town is not null) _town.InputGate.InputStateDidChange += RefreshHudOwnership;
        if (_pauseMenu is not null) _pauseMenu.VisibilityChanged += RefreshHudOwnership;
        GameRoot.Instance.StateChanged += RefreshHudOwnership;
    }

    private void UnbindHudOwnership()
    {
        if (!_hudOwnershipBound) return;
        _hudOwnershipBound = false;
        if (_ranch is not null) _ranch.InputGate.InputStateDidChange -= RefreshHudOwnership;
        if (_town is not null) _town.InputGate.InputStateDidChange -= RefreshHudOwnership;
        if (GodotObject.IsInstanceValid(_pauseMenu)) _pauseMenu!.VisibilityChanged -= RefreshHudOwnership;
        if (GodotObject.IsInstanceValid(GameRoot.Instance)) GameRoot.Instance.StateChanged -= RefreshHudOwnership;
    }

    private void RefreshHudOwnership()
    {
        // Node3D.Visible does not control descendant CanvasLayers. Explicitly switch both HUDs.
        var showHud = !IsManagementVisible && !_flowLocksUi;
        if (_ranch?.Hud is { } ranchHud) ranchHud.Visible = showHud && _activeAreaId == "ranch";
        if (_town?.Hud is { } townHud) townHud.Visible = showHud && _activeAreaId == "town";
        if (_managementButton is not null)
            _managementButton.Disabled = !CanOpenWorldHelp || _activeAreaId != "ranch"
                || _firstDayFlow?.BlocksManagement == true;
        if (_advanceTimeButton is not null)
            _advanceTimeButton.Disabled = !WorldActionsAvailable || _activeAreaId != "ranch"
                || _firstDayFlow?.IsActive == true;
        if (_town?.GetNodeOrNull<Button>("TownHud/ReturnRanchButton") is { } travel)
            travel.Disabled = !WorldActionsAvailable || _activeAreaId != "town";
        if (_returnToWorldButton is not null)
        {
            _returnToWorldButton.Disabled = _flowLocksUi || GameRoot.Instance?.ActiveCombatSession is { IsFinished: false };
            _returnToWorldButton.TooltipText = _returnToWorldButton.Disabled
                ? "Complete the current flow before returning to the world." : "Close management and resume exploration.";
        }
    }

    private void OnHudAdvanceTime()
    {
        if (_activeAreaId == "ranch" && _ranch?.Hud?.Visible == true
            && WorldActionsAvailable && _firstDayFlow?.IsActive != true) AdvanceWorldTime();
    }

    private void OnHudManagement()
    {
        if (_activeAreaId == "ranch" && _ranch?.Hud?.Visible == true && CanOpenWorldHelp) ToggleManagement();
    }

    private void CloseWorldHelp()
    {
        if (_ranchHelp?.HelpVisible == true) _ranchHelp.CloseHelp();
        if (_townHelp?.HelpVisible == true) _townHelp.CloseHelp();
    }
}
