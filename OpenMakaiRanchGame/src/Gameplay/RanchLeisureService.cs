using System;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Gameplay;

public sealed record RanchCornerStatus(
    int Day, bool Restored, int Supplies, int Gold, int RecoveryAmount,
    bool CanRestore, string RestoreReason, bool CanRest, string RestReason);

/// <summary>
/// One optional, permanent ranch improvement. Uses existing gold, supplies, stamina and durable
/// flags; no new clock, production, upkeep or per-day history. Inspecting the corner is read-only.
/// </summary>
public sealed class RanchLeisureService
{
    public const int RestoreGoldCost = 40;
    public const int RestoreSupplyCost = 3;
    public const int DailyRecovery = 10;
    public const int RestoredFlag = 1_230_100;
    public const int LastRestDayFlag = 1_230_101;

    private readonly SaveState _state;
    private readonly FlagService _flags;
    private readonly EconomyService _economy;
    private readonly PlayerStaminaService _stamina;

    public RanchLeisureService(SaveState state, FlagService flags,
        EconomyService economy, PlayerStaminaService stamina)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _flags = flags ?? throw new ArgumentNullException(nameof(flags));
        _economy = economy ?? throw new ArgumentNullException(nameof(economy));
        _stamina = stamina ?? throw new ArgumentNullException(nameof(stamina));
    }

    public bool IsRestored => _flags.GetGlobalFlag(RestoredFlag);

    public string AccessReason => _state.Calendar.Day < 2 || !_state.Story.FirstDayCompleted
        ? "Available after the first day. This corner is optional; finish or skip the introduction first."
        : !string.Equals(_state.WorldAreaId, "ranch", StringComparison.OrdinalIgnoreCase)
            ? "Return to the ranch to use the quiet corner."
            : string.Empty;

    public RanchCornerStatus GetStatus()
    {
        var supplies = _state.Ranch.Stockpile.TryGetValue("supplies", out var value) ? Math.Max(0, value) : 0;
        var restored = IsRestored;
        var restoreReason = AccessReason;
        if (restoreReason.Length == 0)
        {
            if (restored) restoreReason = "The quiet corner is already restored. No maintenance or upkeep is required.";
            else if (supplies < RestoreSupplyCost)
                restoreReason = $"Need {RestoreSupplyCost - supplies} more supplies. Schedule Workshop Crafting or Office Work, then finish the day.";
            else if (_economy.Gold < RestoreGoldCost)
                restoreReason = $"Need {RestoreGoldCost} G. No supplies will be taken until the full cost is available.";
        }

        var restReason = AccessReason;
        var capacity = (long)_state.Player.MaxStamina + _state.Player.DailyStaminaBonus;
        var validStamina = _state.Player.MaxStamina > 0 && capacity <= int.MaxValue
            && _state.Player.DailyStaminaBonus is >= 0 and <= PlayerStaminaService.MaxRestedBonus
            && _state.Player.Stamina >= 0 && _state.Player.Stamina <= capacity;
        var recovery = validStamina ? (int)Math.Min(DailyRecovery, capacity - _state.Player.Stamina) : 0;
        if (restReason.Length == 0)
        {
            if (!restored) restReason = "Restore the quiet corner before taking a break here.";
            else if (!validStamina) restReason = "Player stamina is invalid. No recovery has been applied.";
            else if (_flags.GetGlobalIntFlag(LastRestDayFlag) >= _state.Calendar.Day)
                restReason = "You already took today's restorative break. You can still explore or spend time with a companion.";
            else if (recovery == 0) restReason = "Your stamina is already full. Today's break remains available for later.";
        }

        return new RanchCornerStatus(_state.Calendar.Day, restored, supplies, _economy.Gold, recovery,
            restoreReason.Length == 0, restoreReason, restReason.Length == 0, restReason);
    }

    public bool TryRestore(int expectedDay, out string message)
    {
        if (!CheckDay(expectedDay, out message)) return false;
        var status = GetStatus();
        if (!status.CanRestore)
        {
            message = status.RestoreReason;
            return false;
        }
        // All checks happen before mutation. EconomyService remains the gold authority.
        if (!_economy.Spend(RestoreGoldCost))
        {
            message = "The available gold changed. Review the cost before restoring the corner.";
            return false;
        }
        _state.Ranch.Stockpile["supplies"] = status.Supplies - RestoreSupplyCost;
        _flags.SetGlobalFlag(RestoredFlag, true);
        message = $"Quiet corner restored: -{RestoreGoldCost} G, -{RestoreSupplyCost} supplies. The bench is permanent, with no upkeep.";
        return true;
    }

    public bool TryRest(int expectedDay, out string message)
    {
        if (!CheckDay(expectedDay, out message)) return false;
        var status = GetStatus();
        if (!status.CanRest)
        {
            message = status.RestReason;
            return false;
        }
        // Commit the receipt before GameRoot publishes any notifications. Repeated callbacks,
        // reopening the panel and saved-session replay cannot grant a second daily recovery.
        _flags.SetGlobalIntFlag(LastRestDayFlag, expectedDay);
        var restored = _stamina.Restore(status.RecoveryAmount);
        message = $"A quiet break restores {restored} stamina. Today's recovery is used; no time was advanced. A prepared evening bath still grants its separate next-day bonus.";
        return true;
    }

    private bool CheckDay(int expectedDay, out string message)
    {
        message = expectedDay == _state.Calendar.Day && expectedDay >= 2
            ? string.Empty : "The day changed or the introduction is unfinished. Review the quiet corner again.";
        return message.Length == 0;
    }
}
