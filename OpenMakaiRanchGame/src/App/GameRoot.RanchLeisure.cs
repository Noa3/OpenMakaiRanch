using System;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.App;

public partial class GameRoot
{
    private RanchLeisureService RanchLeisure => new(State, Flags, Economy, PlayerStamina);

    public RanchCornerStatus GetRanchCornerStatus() => RanchLeisure.GetStatus();
    public bool IsRanchCornerRestored => RanchLeisure.IsRestored;

    public bool TryRestoreRanchCorner(int expectedDay, DayPhase expectedPhase,
        ulong expectedGeneration, out string message)
    {
        if (!ValidateRanchCornerCommand(expectedDay, expectedPhase, expectedGeneration, out message)
            || !RanchLeisure.TryRestore(expectedDay, out message)) return false;
        StateChanged?.Invoke();
        return true;
    }

    public bool TryRestAtRanchCorner(int expectedDay, DayPhase expectedPhase,
        ulong expectedGeneration, out string message)
    {
        if (!ValidateRanchCornerCommand(expectedDay, expectedPhase, expectedGeneration, out message)
            || !RanchLeisure.TryRest(expectedDay, out message)) return false;
        StateChanged?.Invoke();
        return true;
    }

    public bool CanShareRanchCorner(out string reason)
    {
        reason = RanchLeisure.AccessReason;
        if (reason.Length > 0) return false;
        if (!IsRanchCornerRestored)
        {
            reason = "Restore this corner before sharing a quiet moment here.";
            return false;
        }
        if (CombatWorldTimeLocked || ActiveCombatSession is not null)
        {
            reason = "Finish the encounter before spending time together.";
            return false;
        }
        if (!Dating.HasActivePartner)
        {
            reason = "Invite an available resident through their existing companionship screen first. Solo recovery does not require a companion.";
            return false;
        }
        if (State.Dating.ActiveApproach != DateInviteApproach.Respectful)
        {
            reason = "This quiet moment is voluntary. End the pressured outing and respect the resident's choice.";
            return false;
        }
        return Dating.CanPerformActivity(DateActivityKind.QuietRest, out reason);
    }

    public bool TryShareRanchCorner(string expectedPartnerId, int expectedDay, DayPhase expectedPhase,
        ulong expectedGeneration, out string message)
    {
        if (!ValidateRanchCornerCommand(expectedDay, expectedPhase, expectedGeneration, out message)) return false;
        if (string.IsNullOrWhiteSpace(expectedPartnerId)
            || !string.Equals(expectedPartnerId, Dating.ActivePartnerId, StringComparison.Ordinal))
        {
            message = "Your companion changed. Review the quiet moment again.";
            return false;
        }
        if (!CanShareRanchCorner(out message)) return false;
        // Use the existing activity cost, phase receipt, relationship/mental effects and eligibility.
        // This action does not also award the solo player's daily recovery.
        var result = Dating.PerformActivity(DateActivityKind.QuietRest);
        message = result.Message;
        if (!result.Success) return false;
        StateChanged?.Invoke();
        return true;
    }

    private bool ValidateRanchCornerCommand(int expectedDay, DayPhase expectedPhase,
        ulong expectedGeneration, out string message)
    {
        if (expectedGeneration != StateGeneration || expectedDay != State.Calendar.Day
            || expectedPhase != State.Calendar.Phase || !Enum.IsDefined(expectedPhase))
        {
            message = "The session or time changed. Reopen the quiet corner before acting.";
            return false;
        }
        if (CombatWorldTimeLocked || ActiveCombatSession is not null)
        {
            message = "Finish the encounter before using the quiet corner.";
            return false;
        }
        message = RanchLeisure.AccessReason;
        return message.Length == 0;
    }
}
