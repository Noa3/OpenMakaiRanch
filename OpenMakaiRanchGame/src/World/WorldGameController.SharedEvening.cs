using OpenMakaiRanch.App;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.World;

public partial class WorldGameController
{
    public void RevealSharedEvening(ulong generation, int settledDay)
    {
        var game = GameRoot.Instance;
        if (game.StateGeneration != generation || !game.HadSharedEvening(settledDay)
            || (long)game.State.Calendar.Day != (long)settledDay + 1 || _transition is null) return;
        _transition.Reveal(T("evening.reveal.title", "A quiet night together"),
            T("evening.reveal.body", "♥  You shared a peaceful night. Morning arrives."),
            game.State.Settings.ReducedMotion ? 0 : 1.2, game.State.Settings.ReducedMotion ? 0.05 : 0.8);
        RefreshWorldInputOwnership(); RefreshHudOwnership();
    }
}
