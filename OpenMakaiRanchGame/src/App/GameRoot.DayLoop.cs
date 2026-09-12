using System;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.App;

public partial class GameRoot
{
    private bool _settlingDay;
    private DailyReport? _settlingReport;

    public bool HasNightPlan => State.Calendar.NightAction is "rest" or "train" or "admin";

    /// <summary>One command against the state/day/phase the player actually saw.</summary>
    public bool TryAdvanceTime(ulong expectedGeneration, int expectedDay, DayPhase expectedPhase)
    {
        if (expectedGeneration != StateGeneration || expectedDay != State.Calendar.Day
            || expectedPhase != State.Calendar.Phase) return false;
        return AdvanceTime();
    }

    public bool TrySelectNightAction(string action, ulong expectedGeneration, int expectedDay)
    {
        if (_settlingDay || _stationCommandBusy || _residentCommandBusy || _combatWorldTimeLocked || expectedGeneration != StateGeneration
            || expectedDay != State.Calendar.Day || State.Calendar.Phase != DayPhase.Night
            || action is not ("rest" or "train" or "admin") || State.Calendar.NightAction == action) return false;
        State.Calendar.NightAction = action;
        StateChanged?.Invoke();
        return true;
    }

    private DailyReport SettleAndPublishDay()
    {
        // Preserve explicit EndDay simulation callers; ordinary player entry uses AdvanceTime.
        // Synchronous observers cannot settle tomorrow while this completion is being published.
        if (_settlingDay)
            return _settlingReport ?? throw new InvalidOperationException("Settlement has not produced a report yet.");
        _settlingDay = true;
        try
        {
            var state = State;
            var generation = StateGeneration;
            var evening = SharedEvening;
            var sharedNight = evening.CaptureForSettlement();
            var development = new CharacterDevelopmentService(state, Data, Flags);
            var developmentBefore = development.CaptureWorkday();
            var settlement = new DailySettlementService(state, Data, Schedule, Ranch, Economy,
                new DayCycleService(state), Milestones, Inventory, Talents);
            var report = settlement.SettleDay();
            evening.CompleteAfterSettlement(sharedNight, report);
            development.CompleteWorkday(developmentBefore, report);
            if (settlement.LastWorkFacts is { } facts)
                new RanchProgressionService(state, Flags).ObserveSettlement(facts, report);
            _settlingReport = report;
            LastDailyReport = report;
            state.Reports.RemoveAll(entry => entry.Day == report.Day);
            state.Reports.Add(report);
            var completed = WinCondition.IsGameComplete() && !state.VictoryDay.HasValue;
            if (completed) state.VictoryDay = state.Calendar.Day;

            DaySettled?.Invoke(report);
            // A callback may intentionally start/load another session. Do not publish or save
            // the replacement as though it were the state just settled.
            if (StateGeneration != generation || !ReferenceEquals(State, state)) return report;
            StateChanged?.Invoke();
            if (StateGeneration != generation || !ReferenceEquals(State, state)) return report;
            TryAutosave("day settled");
            if (completed && StateGeneration == generation && ReferenceEquals(State, state))
                GameComplete?.Invoke();
            return report;
        }
        finally
        {
            _settlingReport = null;
            _settlingDay = false;
        }
    }
}
