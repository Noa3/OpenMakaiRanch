using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.App;

public partial class GameRoot
{
    // Fresh session-bound values; StateChanged invalidates presentation caches after a real action.
    public CharacterDevelopmentSnapshot? GetCharacterDevelopment(string characterId) =>
        new CharacterDevelopmentService(State, Data, Flags).Inspect(characterId);

    public CharacterProtectionSnapshot? GetCharacterProtection(string characterId) =>
        GetCharacterDevelopment(characterId)?.Protection;
}
