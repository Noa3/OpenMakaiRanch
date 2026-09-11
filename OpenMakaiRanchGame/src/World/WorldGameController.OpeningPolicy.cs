using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>Presentation access during the guided opening. No calendar or economic mutations.</summary>
public partial class WorldGameController
{
    public bool IsGuidedOpening => GameRoot.Instance is { } game && !game.State.NgPlusActive
        && game.State.Calendar.Day == 1 && !game.State.Story.FirstDayCompleted;

    public bool OpeningUtilityOnly => IsGuidedOpening && (_activeAreaId == "intro"
        || GameRoot.Instance.State.Story.FirstDayStage < FirstDayFlowController.StageManagementDairy
        || _firstDayFlow?.BlocksWorldInput == true);

    private static bool IsUtilityScreen(string screenId) => screenId is "options" or "settings" or "saveload";

    public bool CanOpenManagementScreen(string screenId)
    {
        if (_shell is null || _flowLocksUi || GetTree().Paused || _transition?.IsTransitioning == true) return false;
        if (!IsGuidedOpening || IsUtilityScreen(screenId)) return true;
        return screenId == "schedule"
            && GameRoot.Instance.State.Story.FirstDayStage == FirstDayFlowController.StageManagementDairy
            && _firstDayFlow?.BlocksWorldInput != true;
    }

    // Enforce the same restriction for authored navigation buttons and retired UI callbacks.
    // A same-screen refresh is presentation only and remains available to StateChanged.
    public bool CanRouteManagementScreen(string screenId)
    {
        if (_shell is null || _flowLocksUi || !IsGuidedOpening || screenId == _shell.CurrentScreen) return true;
        if (IsUtilityScreen(screenId)) return true;
        return screenId == "schedule"
            && GameRoot.Instance.State.Story.FirstDayStage == FirstDayFlowController.StageManagementDairy
            && _firstDayFlow?.BlocksWorldInput != true;
    }

    private static string ResumeAreaFor(GameRoot game)
    {
        if (!game.State.NgPlusActive && game.State.Calendar.Day == 1 && !game.State.Story.FirstDayCompleted)
            return game.State.Story.FirstDayStage <= FirstDayFlowController.StageLeaveBedroom ? "intro" : "ranch";
        return game.State.WorldAreaId is "ranch" or "town" ? game.State.WorldAreaId : "ranch";
    }
}
