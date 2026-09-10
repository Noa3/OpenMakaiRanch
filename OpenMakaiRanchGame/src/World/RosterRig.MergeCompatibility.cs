using System.Collections.Generic;
using OpenMakaiRanch.Character;

namespace OpenMakaiRanch.World;

/// <summary>
/// Compatibility surface recovered after the dev/world merge. The current RosterRig keeps the
/// newer companion-following and thought-bubble implementation; these accessors restore the
/// read-only API still used by capture/debug tooling without reintroducing a second roster path.
/// </summary>
public partial class RosterRig
{
    public IEnumerable<string> AvatarIds => _avatars.Keys;

    public CharacterAvatar3D? GetAvatar(string characterId)
    {
        return TryGetAvatar(characterId, out var avatar) ? avatar : null;
    }
}