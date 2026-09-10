using System.Collections.Generic;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.App;

public partial class GameRoot
{
    // Resolve the current state and services at the command boundary. Do not keep a service bound
    // to an old save when NewGame/LoadSlot rebuilds the root's services.
    public IReadOnlyList<CommunityRequestOffer> GetCommunityRequests() =>
        new CommunityRequestService(State, Flags, Economy).GetOffers();

    public int CompletedCommunityDeliveries =>
        new CommunityRequestService(State, Flags, Economy).CompletedDeliveries;

    public bool TryDeliverCommunityRequest(string? requestId, int expectedDay,
        ulong expectedGeneration, out string message)
    {
        if (expectedGeneration != StateGeneration)
        {
            message = "The game has changed. Reopen the board before delivering.";
            return false;
        }
        if (CombatWorldTimeLocked || ActiveCombatSession is not null)
        {
            message = "Finish the encounter before arranging a delivery.";
            return false;
        }

        var requests = new CommunityRequestService(State, Flags, Economy);
        if (!requests.TryDeliver(requestId, expectedDay, out message))
            return false;

        StateChanged?.Invoke();
        return true;
    }
}
