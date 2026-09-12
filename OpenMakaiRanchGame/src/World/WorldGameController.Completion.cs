using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>Presentation-only completion routing. It never settles a day, pays or resets a save.</summary>
public partial class WorldGameController
{
    private ulong _completionGeneration;
    private bool _completionPending;
    private bool CompletionOwnsPresentation => _completionPending && _completionGeneration == GameRoot.Instance.StateGeneration
        && GameRoot.Instance.State.VictoryDay.HasValue && _shell?.CurrentScreen == "victory";

    public void PresentGameCompletion()
    {
        var game = GameRoot.Instance;
        if (_shell is null || !game.State.VictoryDay.HasValue) return;
        _completionPending = true;
        _completionGeneration = game.StateGeneration;
        if (IsStationPanelOpen) _stationPanel!.Close();
        _shell.ClearServiceContext();
        _shell.SetServiceContext("victory");
        _shell.ShowScreen("victory");
    }

    public bool LeaveCompletion(ulong generation, int recordedDay, bool showReport)
    {
        var game = GameRoot.Instance;
        if (_shell?.CurrentScreen != "victory" || generation != game.StateGeneration
            || game.State.VictoryDay != recordedDay || GetTree().Paused || _transition?.IsTransitioning == true) return false;
        _completionPending = false;
        // A non-mandatory screen releases the full-screen flow lock without taking the new-game
        // 'ranch' route, which would activate the ranch even when completion happened in town.
        _shell.ClearServiceContext();
        _shell.SetServiceContext("report");
        _shell.ShowScreen("report");
        if (_shell.CurrentScreen != "report") return false;
        if (showReport) ApplyManagementVisibility(true);
        else CloseManagement();
        return true;
    }
}
