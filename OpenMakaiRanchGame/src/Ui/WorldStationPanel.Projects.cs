using System.Linq;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.World;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Ui;

public partial class WorldStationPanel
{
    private void RenderProjects()
    {
        _title.Text = T("project.journal.title", "Your ranch projects");
        _content.AddChild(Text(T("project.journal.help", "Make this ranch a place worth coming home to. Choose your own pace: these projects have no deadlines and do not require romance.")));
        foreach (var step in RanchProjectService.Evaluate(_game.State, _game.Flags))
        {
            _content.AddChild(Text(T("project.journal.progress", "{0} — {1}/{2}", step.Title, step.Progress, step.Target)));
            _content.AddChild(Text(step.Detail));
            if (step.Complete) _content.AddChild(Text(T("project.journal.done", "Completed")));
            else Action("Project_" + step.Id, T("project.journal.track", "Show the way: {0}", step.Action), () =>
            {
                _world.NavigationGuide?.Track(new WorldDestination(step.AreaId, step.StationId, step.Action));
                Close(); return "";
            });
        }
        Action("ProjectPlaces", T("project.journal.places", "Show all places"), () => { OpenGuide(); return ""; });
    }

    private void RenderSharedEvening()
    {
        var status = _game.GetSharedEveningStatus();
        _content.AddChild(Text(T("evening.title", "Optional: a quiet night together")));
        if (status.Planned)
        {
            _content.AddChild(Text(T("evening.planned", "A quiet night together is planned. Nothing happens until you sleep; you can still cancel.")));
            Action("CancelSharedEvening", T("evening.cancel", "Rest separately tonight"), () =>
                _game.TryCancelSharedEvening(_generation, _day) ? T("evening.cancelled", "You will rest separately. No relationship penalty.") : T("evening.reason.changed", "The situation changed. Review tonight's plan again."));
        }
        else if (status.CanPlan)
            Action("PlanSharedEvening", T("evening.plan", "Invite your companion to stay tonight"), () =>
            { _game.TryPlanSharedEvening(_generation, _day, out var message); return message; });
        if (status.Reason.Length > 0) _content.AddChild(Text(status.Reason));
        _content.AddChild(Text(T("evening.no_family", "This is companionship only. It does not start a pregnancy or family event.")));
    }

    private void RenderVoluntaryCompanionship(string id)
    {
        var character = _game.Roster.Find(id);
        if (character is null || !_game.Dating.IsEligiblePartner(character)) return;
        var active = _game.Dating.ActivePartnerId == id;
        Action("ResidentCompanion", active ? T("evening.outing.end", "End this outing") : T("evening.outing.invite", "Invite to spend time together"),
            () => active ? _game.EndDate().Message : _game.StartDate(id, DateInviteApproach.Respectful).Message,
            !active && _game.Dating.HasActivePartner);
        if (!active || _game.State.Dating.ActiveApproach != DateInviteApproach.Respectful) return;
        foreach (var activity in new[] { DateActivityKind.RanchWalk, DateActivityKind.SharedMeal, DateActivityKind.TownOuting })
        {
            var allowed = _game.Dating.CanPerformActivity(activity, out var reason);
            var label = activity switch
            {
                DateActivityKind.RanchWalk => T("evening.activity.walk", "Take a ranch walk"),
                DateActivityKind.SharedMeal => T("evening.activity.meal", "Share a meal"),
                _ => T("evening.activity.town", "Spend time in town")
            };
            Action("CompanionActivity_" + activity, T("evening.activity.cost", "{0} — {1} stamina", label, _game.Dating.ActivityCost(activity)),
                () => _game.PerformDateActivity(activity).Message, !allowed);
        }
    }
}
