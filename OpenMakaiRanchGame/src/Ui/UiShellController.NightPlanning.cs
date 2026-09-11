using Godot;
using OpenMakaiRanch.Core.Models;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Ui;

public partial class UiShellController
{
    private bool IsLiveDayControl(Button control, ulong revision, ulong generation, int day, DayPhase phase) =>
        IsInsideTree() && IsVisibleInTree() && !_fullScreenMode && _viewRevision == revision
        && GodotObject.IsInstanceValid(control) && !control.IsQueuedForDeletion()
        && control.IsVisibleInTree() && _content.IsAncestorOf(control)
        && _game.StateGeneration == generation && _game.State.Calendar.Day == day
        && _game.State.Calendar.Phase == phase;

    private void AddNightPlanningCard()
    {
        var banner = CardContainer();
        banner.Name = "NightPlanningCard";
        _content.AddChild(banner);
        var inner = CardContent();
        inner.Name = "NightPlanningContent";
        banner.AddChild(inner);
        inner.AddChild(AddStyledLine(T("screen.night.title", "Night Phase — Choose Tonight's Work"), true));
        var selected = _game.State.Calendar.NightAction;
        if (_game.HasNightPlan)
            inner.AddChild(MutedLabel($"{T("screen.night.selected", "Selected")}: {NightActionLabel(selected)}"));
        inner.AddChild(MutedLabel("You can change this choice until End Day. Choosing an option does not advance time or spend stamina. A prepared bath's next-morning bonus is kept."));
        var generation = _game.StateGeneration;
        var day = _game.State.Calendar.Day;
        var revision = _viewRevision;
        foreach (var (action, label, detail) in new[]
        {
            ("rest", T("screen.night.rest", "Rest (restore energy)"), "Before work: fatigue -20, energy +15 and morale +4 per resident, within existing limits."),
            ("train", T("screen.night.train", "Train (growth practice)"), "Working residents receive one extra growth pass before ordinary daily growth. Existing fatigue and talent modifiers apply. Bond +1, morale +2."),
            ("admin", T("screen.night.admin", "Admin (reduce workload)"), "Reduce ranch workload by 10, down to zero. This choice does not directly discount upkeep or award gold.")
        })
        {
            var button = PrimaryButton(label, detail);
            button.Name = $"NightChoice_{action}";
            button.Disabled = selected == action;
            button.Pressed += () =>
            {
                if (!IsLiveDayControl(button, revision, generation, day, DayPhase.Night)) return;
                if (_game.TrySelectNightAction(action, generation, day)) _game.Feedback.PlayConfirm();
            };
            inner.AddChild(button);
            inner.AddChild(MutedLabel(detail));
        }
    }

    private void AdvanceFromManagement(ulong generation, int day, DayPhase phase)
    {
        if (!IsVisibleInTree() || _fullScreenMode || _game.CombatWorldTimeLocked
            || generation != _game.StateGeneration || day != _game.State.Calendar.Day
            || phase != _game.State.Calendar.Phase) return;
        if (phase == DayPhase.Night && !_game.HasNightPlan)
        {
            ShowScreen("ranch");
            SetStatus("Choose tonight's work before ending the day.");
            return;
        }
        if (!_game.TryAdvanceTime(generation, day, phase)) return;
        _game.Feedback.PlayConfirm();
        if (_game.StateGeneration == generation && _game.State.Calendar.Day != day)
            ShowScreen("report");
    }

    private Button DayAdvanceButton()
    {
        var generation = _game.StateGeneration;
        var day = _game.State.Calendar.Day;
        var phase = _game.State.Calendar.Phase;
        var revision = _viewRevision;
        var label = phase == DayPhase.Night
            ? _game.HasNightPlan ? T("screen.ranch.end_day", "End Day") : "Plan Night"
            : T("screen.ranch.advance_phase", "Advance Phase");
        var button = PrimaryButton(label, "Advance one phase. At Night choose Rest, Training or Admin before settling the day.");
        button.Name = "OverviewAdvanceTime";
        button.Pressed += () =>
        {
            if (IsLiveDayControl(button, revision, generation, day, phase))
                AdvanceFromManagement(generation, day, phase);
        };
        return button;
    }
}
