using System;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Gameplay;

public sealed class EconomyService
{
    private readonly SaveState _state;

    public EconomyService(SaveState state)
    {
        _state = state;
    }

    public int Gold => _state.Economy.Gold;

    public bool Spend(int amount)
    {
        if (amount < 0 || _state.Economy.Gold < amount)
        {
            return false;
        }

        _state.Economy.Gold -= amount;
        return true;
    }

    public void AddGold(int amount)
    {
        _state.Economy.Gold += Math.Max(0, amount);
    }

    public void ApplySettlement(int income, int expenses)
    {
        _state.Economy.LastIncome = income;
        _state.Economy.LastExpenses = expenses;
        _state.Economy.Gold += income - expenses;
    }
}
