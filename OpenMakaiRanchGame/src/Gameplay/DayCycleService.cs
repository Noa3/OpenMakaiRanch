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
        _state.Calendar.CurrentWeather = RandomWeather();
    }

    private static Weather RandomWeather()
    {
        var roll = Random.Shared.NextDouble();
        return roll switch
        {
            < 0.40 => Weather.Clear,
            < 0.70 => Weather.Cloudy,
            < 0.90 => Weather.Rain,
            _ => Weather.Storm
        };
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
