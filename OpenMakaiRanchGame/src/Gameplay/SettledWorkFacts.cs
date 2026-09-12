using System.Collections.Generic;

namespace OpenMakaiRanch.Gameplay;

/// <summary>Facts captured by the real settlement before events/growth. Not another output forecast.</summary>
public sealed record SettledWorkFacts(int Day, int Residents, int Workers, int RestingResidents,
    int AutomaticallyRested, LunchPlan Lunch, bool TeamWell, long WorkGold, int OperatingExpenses, long PortableMealReplacementCost,
    IReadOnlyDictionary<string, long> Produced)
{
    public long OperatingNet => WorkGold - OperatingExpenses - PortableMealReplacementCost;
    public long Units(string resourceId) => Produced.TryGetValue(resourceId, out var amount) ? amount : 0;
}
