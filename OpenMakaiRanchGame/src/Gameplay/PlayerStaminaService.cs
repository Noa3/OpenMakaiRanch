using System;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Gameplay;

public enum PlayerActivityKind
{
    Mentorship,
    BondEvent,
    PetFeed,
    PetPlay,
    PetTraining,
    VisitCare,
    VisitFeed,
    VisitGift,
    Adventure
}

/// <summary>
/// Daily player agency budget for the real-time remake.
///
/// This deliberately does not model locomotion stamina. Walking, sprinting, exploration, dialogue,
/// shopping and menus stay free. Stamina is spent only by repeatable actions that create persistent
/// progression or large rewards, replacing the original game's one-meaningful-action-per-time-slot
/// constraint without turning the 3D world into an exhaustion simulator.
/// </summary>
public sealed class PlayerStaminaService
{
    public const int DefaultMaxStamina = 100;
    public const int HotBathNextDayBonus = 25;
    public const int MaxRestedBonus = 50;

    private readonly SaveState _state;

    public PlayerStaminaService(SaveState state)
    {
        _state = state;
        Normalize();
    }

    public int Current => _state.Player.Stamina;
    public int Max => _state.Player.MaxStamina;
    public int DailyBonus => _state.Player.DailyStaminaBonus;
    public int CurrentCapacity => _state.Player.MaxStamina + _state.Player.DailyStaminaBonus;
    public bool HasRecoveredFromBathToday => _state.Player.BathedToday;

    public int Cost(PlayerActivityKind kind) => kind switch
    {
        PlayerActivityKind.Mentorship => 20,
        PlayerActivityKind.BondEvent => 15,
        PlayerActivityKind.PetFeed => 5,
        PlayerActivityKind.PetPlay => 10,
        PlayerActivityKind.PetTraining => 15,
        PlayerActivityKind.VisitCare => 10,
        PlayerActivityKind.VisitFeed => 8,
        PlayerActivityKind.VisitGift => 5,
        PlayerActivityKind.Adventure => 30,
        _ => 0
    };

    public bool CanSpend(PlayerActivityKind kind) => CanSpend(Cost(kind));

    public bool CanSpend(int amount)
    {
        Normalize();
        return amount >= 0 && _state.Player.Stamina >= amount;
    }

    public bool Spend(PlayerActivityKind kind) => Spend(Cost(kind));

    public bool Spend(int amount)
    {
        if (amount <= 0)
            return true;

        if (!CanSpend(amount))
            return false;

        _state.Player.Stamina -= amount;
        return true;
    }

    public int Restore(int amount)
    {
        Normalize();
        if (amount <= 0)
            return 0;

        var before = _state.Player.Stamina;
        _state.Player.Stamina = Math.Min(CurrentCapacity, before + amount);
        return _state.Player.Stamina - before;
    }

    public PlayerRecoveryResult TryBathRecovery(bool cleanBathAvailable, DayPhase phase)
    {
        Normalize();
        if (phase is not (DayPhase.Evening or DayPhase.Night))
            return new PlayerRecoveryResult(false, false, 0, "Bathing is available for recovery planning in the Evening or Night.");

        if (_state.Player.BathedToday)
            return new PlayerRecoveryResult(false, false, 0, "You already used today's bath/shower routine.");

        _state.Player.BathedToday = true;
        if (!cleanBathAvailable)
        {
            return new PlayerRecoveryResult(
                true,
                false,
                0,
                "You take a quick shower. It handles hygiene, but a prepared hot bath is required for tomorrow's Well Rested stamina bonus.");
        }

        _state.Player.NextDayStaminaBonus = Math.Max(
            _state.Player.NextDayStaminaBonus,
            HotBathNextDayBonus);

        return new PlayerRecoveryResult(
            true,
            true,
            HotBathNextDayBonus,
            $"The prepared hot bath leaves you Well Rested: tomorrow starts with +{HotBathNextDayBonus} stamina.");
    }

    public void ResetForNewDay()
    {
        Normalize();
        _state.Player.DailyStaminaBonus = Math.Clamp(_state.Player.NextDayStaminaBonus, 0, MaxRestedBonus);
        _state.Player.NextDayStaminaBonus = 0;
        _state.Player.Stamina = _state.Player.MaxStamina + _state.Player.DailyStaminaBonus;
        _state.Player.BathedToday = false;
    }

    private void Normalize()
    {
        if (_state.Player.MaxStamina <= 0)
            _state.Player.MaxStamina = DefaultMaxStamina;

        _state.Player.DailyStaminaBonus = Math.Clamp(_state.Player.DailyStaminaBonus, 0, MaxRestedBonus);
        _state.Player.NextDayStaminaBonus = Math.Clamp(_state.Player.NextDayStaminaBonus, 0, MaxRestedBonus);
        _state.Player.Stamina = Math.Clamp(_state.Player.Stamina, 0, CurrentCapacity);
    }
}

public readonly record struct PlayerRecoveryResult(
    bool Used,
    bool UsedCleanBath,
    int ScheduledNextDayBonus,
    string Message);
