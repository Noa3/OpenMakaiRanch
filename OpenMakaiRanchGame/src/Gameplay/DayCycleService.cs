using System;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Gameplay;

public sealed class DayCycleService
{
    private readonly SaveState _state;

    public DayCycleService(SaveState state)
    {
        _state = state;
    }

    public void AdvanceToNextDay()
    {
        _state.Calendar.Day += 1;
        _state.Calendar.Phase = DayPhase.Morning;

        // Original parity: yesterday's forecast becomes today's weather after the date increments,
        // then a new forecast is generated from the new season/day.
        _state.Calendar.CurrentWeather = _state.Calendar.TomorrowWeather;
        _state.Calendar.TomorrowWeather = OriginalCalendarRules.RollTomorrow(_state.Calendar);
        _state.Calendar.TrainedToday = 0;
        MagicService.RecoverPlayerManaForRest(_state);
    }

    public bool AdvancePhase()
    {
        if (_state.Calendar.Phase == DayPhase.Night)
        {
            return false;
        }

        _state.Calendar.Phase = _state.Calendar.Phase switch
        {
            DayPhase.Morning => DayPhase.Afternoon,
            DayPhase.Afternoon => DayPhase.Evening,
            DayPhase.Evening => DayPhase.Night,
            _ => DayPhase.Night
        };
        return true;
    }
}
