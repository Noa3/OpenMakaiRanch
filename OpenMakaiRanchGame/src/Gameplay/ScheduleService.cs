using System.Collections.Generic;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

public sealed class ScheduleService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;

    public ScheduleService(SaveState state, DataRegistry data)
    {
        _state = state;
        _data = data;
    }

    public IReadOnlyList<JobDefinition> AssignableJobs => _data.AssignableJobs();

    public string GetAssignment(string characterId)
    {
        return _state.Schedule.AssignedJobs.TryGetValue(characterId, out var jobId) ? jobId : "rest";
    }

    public void AssignJob(string characterId, string jobId)
    {
        if (!_data.Jobs.ContainsKey(jobId))
        {
            return;
        }

        _state.Schedule.AssignedJobs[characterId] = jobId;
    }
}
