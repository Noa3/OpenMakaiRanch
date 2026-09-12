using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.App;

public partial class GameRoot
{
    // Fresh session-bound values; StateChanged invalidates presentation caches after a real action.
    public CharacterDevelopmentSnapshot? GetCharacterDevelopment(string characterId) =>
        new CharacterDevelopmentService(State, Data, Flags).Inspect(characterId);

    public CharacterProtectionSnapshot? GetCharacterProtection(string characterId) =>
        GetCharacterDevelopment(characterId)?.Protection;

    public DevelopmentTrack? GetCharacterDevelopmentTrack(string? characterId, string? focus) =>
        new CharacterDevelopmentService(State, Data, Flags).InspectPracticeTrack(characterId, focus);

    /// <summary>All root practice entry points share costs, receipts, development and command locks.</summary>
    public ResidentActionResult TryTrainCharacter(string? characterId, string? focus,
        ulong generation, int day, DayPhase phase)
    {
        var action = ResidentInteractionService.PracticeAction(focus);
        return action.HasValue ? TryResidentAction(characterId, action.Value, generation, day, phase)
            : new(false, false, T("world.resident.reason.unknown", "This interaction is not available."));
    }
}
