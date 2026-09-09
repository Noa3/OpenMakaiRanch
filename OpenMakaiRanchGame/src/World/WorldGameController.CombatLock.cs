using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Keeps the turn-based combat time lock scoped to the combat surface. Adventure/pre-battle starts
/// the lock through GameRoot.StartNewCombat(); leaving the combat screen releases it. The first-day
/// intruder flow resolves in-world and explicitly ends its own session, so this adapter does not
/// interfere with that story path.
/// </summary>
public partial class WorldGameController
{
    public override void _EnterTree()
    {
        CallDeferred(nameof(BindCombatScreenLockGuard));
    }

    private void BindCombatScreenLockGuard()
    {
        if (_shell is null || !GodotObject.IsInstanceValid(_shell))
        {
            return;
        }

        _shell.ScreenChanged += OnCombatScreenChangedForTimeLock;
    }

    private void OnCombatScreenChangedForTimeLock(string screenId)
    {
        var game = GameRoot.Instance;
        if (game is null || !GodotObject.IsInstanceValid(game) || !game.CombatWorldTimeLocked)
        {
            return;
        }

        // While the actual combat UI is active, phase/day/weather settlement is intentionally frozen.
        // Once the player leaves that UI (Adventure, Ranch, Town, etc.), the session is over and the
        // world clock is permitted to advance again.
        if (!string.Equals(screenId, "combat", System.StringComparison.OrdinalIgnoreCase))
        {
            game.EndCombatSession();
        }
    }
}
