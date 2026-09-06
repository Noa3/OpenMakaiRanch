using System;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Production dispatcher binding a world command to the <see cref="GameRoot"/> command boundary.
/// This is the single place where world interaction touches the simulation. It owns the
/// StateGeneration guard by passing <see cref="WorldInteractionContext.ExpectedGeneration"/>
/// straight into the GameRoot commands, which re-check it themselves (defense in depth).
///
/// No reward, economy, or bond math happens here — the GameRoot commands are the only place
/// those numbers move. This type is deliberately tiny so it can be swapped for a stub in tests.
/// </summary>
public sealed class GameRootCommandDispatcher : IWorldCommandDispatcher
{
    private readonly Func<GameRoot> _resolve;

    public GameRootCommandDispatcher(Func<GameRoot>? resolve = null)
    {
        _resolve = resolve ?? (() => GameRoot.Instance);
    }

    public bool Dispatch(WorldCommand command, WorldInteractionContext context)
    {
        GameRoot game;
        try
        {
            game = _resolve();
        }
        catch (Exception)
        {
            return false;
        }

        if (game is null)
        {
            return false;
        }

        var characterId = command.TargetId ?? context.CharacterId;
        switch (command.Kind)
        {
            case WorldCommandKind.AssignJob:
                return game.TryAssignJob(context.CharacterId, characterId, context.ExpectedGeneration);
            case WorldCommandKind.Mentorship:
                return game.TryConductMentorship(characterId, context.ExpectedGeneration);
            case WorldCommandKind.BondEvent:
                return game.TryCompleteBondEvent(characterId, context.ExpectedGeneration);
            default:
                return false;
        }
    }
}
