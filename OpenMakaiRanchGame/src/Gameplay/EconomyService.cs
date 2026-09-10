using System;
using System.Collections.Generic;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Gameplay;

/// <summary>
/// Canonical ranch-owner resource ledger. Mirrors the durable resource groups from the original
/// Money.csv while preserving the remake's existing Gold API for callers.
/// </summary>
public sealed class EconomyService
{
    public const string TrainingAp = "training";
    public const string MagicAp = "magic";
    public const string TentacleAp = "tentacle";
    public const string CombatAp = "combat";
    public const string ForbiddenAp = "forbidden";

    private static readonly string[] KnownApPools =
    {
        TrainingAp, MagicAp, TentacleAp, CombatAp, ForbiddenAp
    };

    private readonly SaveState _state;

    public EconomyService(SaveState state)
    {
        _state = state;
        NormalizeCollections();
    }

    public int Gold => _state.Economy.Gold;
    public int ExpenseAccount => _state.Economy.ExpenseAccount;
    public int LoanBalance => _state.Economy.LoanBalance;
    public int StoredSpirit => _state.Economy.SpiritEnergy;
    public int StoredMana => _state.Economy.ManaReservoir;
    public int ContributionPoints => _state.Economy.ContributionPoints;

    public bool Spend(int amount)
    {
        if (amount < 0 || _state.Economy.Gold < amount)
            return false;

        _state.Economy.Gold -= amount;
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;
        _state.Economy.Gold = SaturatingAdd(_state.Economy.Gold, amount);
    }

    public bool TrySpendExpenseAccount(int amount) =>
        TrySpend(ref _state.Economy.ExpenseAccount, amount);

    public void AddExpenseAccount(int amount) =>
        _state.Economy.ExpenseAccount = SaturatingAdd(_state.Economy.ExpenseAccount, amount);

    public void AddLoan(int amount) =>
        _state.Economy.LoanBalance = SaturatingAdd(_state.Economy.LoanBalance, amount);

    public bool RepayLoan(int amount)
    {
        if (amount <= 0 || _state.Economy.LoanBalance <= 0 || _state.Economy.Gold <= 0)
            return false;

        var payment = Math.Min(amount, Math.Min(_state.Economy.LoanBalance, _state.Economy.Gold));
        _state.Economy.Gold -= payment;
        _state.Economy.LoanBalance -= payment;
        return payment > 0;
    }

    public bool TrySpendStoredSpirit(int amount) =>
        TrySpend(ref _state.Economy.SpiritEnergy, amount);

    public void AddStoredSpirit(int amount) =>
        _state.Economy.SpiritEnergy = SaturatingAdd(_state.Economy.SpiritEnergy, amount);

    public bool TrySpendStoredMana(int amount) =>
        TrySpend(ref _state.Economy.ManaReservoir, amount);

    public void AddStoredMana(int amount, int capacity = int.MaxValue)
    {
        if (amount <= 0 || capacity <= 0)
            return;
        var target = (long)_state.Economy.ManaReservoir + amount;
        _state.Economy.ManaReservoir = (int)Math.Min(Math.Min(target, int.MaxValue), capacity);
    }

    public bool TrySpendContributionPoints(int amount) =>
        TrySpend(ref _state.Economy.ContributionPoints, amount);

    public void AddContributionPoints(int amount) =>
        _state.Economy.ContributionPoints = SaturatingAdd(_state.Economy.ContributionPoints, amount);

    public int GetActionPoints(string pool) =>
        GetNonNegative(_state.Economy.ActionPoints, NormalizePool(pool));

    public int GetActionPointProgress(string pool) =>
        GetNonNegative(_state.Economy.ActionPointProgress, NormalizePool(pool));

    public long GetLifetimeActionPoints(string pool)
    {
        var key = NormalizePool(pool);
        return _state.Economy.LifetimeActionPoints.TryGetValue(key, out var value)
            ? Math.Max(0L, value)
            : 0L;
    }

    public bool TrySpendActionPoints(string pool, int amount)
    {
        if (amount < 0)
            return false;

        var key = NormalizePool(pool);
        var current = GetActionPoints(key);
        if (current < amount)
            return false;

        _state.Economy.ActionPoints[key] = current - amount;
        return true;
    }

    public int AddActionPoints(string pool, int amount)
    {
        if (amount <= 0)
            return 0;

        var key = NormalizePool(pool);
        var before = GetActionPoints(key);
        var after = SaturatingAdd(before, amount);
        _state.Economy.ActionPoints[key] = after;

        var gained = after - before;
        if (gained > 0)
        {
            var lifetime = GetLifetimeActionPoints(key);
            _state.Economy.LifetimeActionPoints[key] = lifetime > long.MaxValue - gained
                ? long.MaxValue
                : lifetime + gained;
        }
        return gained;
    }

    /// <summary>
    /// Adds progress toward an AP pool without assuming an unverified conversion threshold.
    /// A later source-traced progression service can consume this accumulated value.
    /// </summary>
    public int AddActionPointProgress(string pool, int amount)
    {
        if (amount <= 0)
            return 0;

        var key = NormalizePool(pool);
        var before = GetActionPointProgress(key);
        var after = SaturatingAdd(before, amount);
        _state.Economy.ActionPointProgress[key] = after;
        return after - before;
    }

    public void ApplySettlement(int income, int expenses)
    {
        income = Math.Max(0, income);
        expenses = Math.Max(0, expenses);
        _state.Economy.LastIncome = income;
        _state.Economy.LastExpenses = expenses;

        var net = (long)_state.Economy.Gold + income - expenses;
        _state.Economy.Gold = (int)Math.Clamp(net, int.MinValue, int.MaxValue);
    }

    public IReadOnlyList<string> ActionPointPools => KnownApPools;

    private void NormalizeCollections()
    {
        _state.Economy.ActionPoints ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        _state.Economy.ActionPointProgress ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        _state.Economy.LifetimeActionPoints ??= new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

        foreach (var pool in KnownApPools)
        {
            if (!_state.Economy.ActionPoints.ContainsKey(pool))
                _state.Economy.ActionPoints[pool] = 0;
            if (!_state.Economy.ActionPointProgress.ContainsKey(pool))
                _state.Economy.ActionPointProgress[pool] = 0;
            if (!_state.Economy.LifetimeActionPoints.ContainsKey(pool))
                _state.Economy.LifetimeActionPoints[pool] = 0;
        }
    }

    private static string NormalizePool(string pool)
    {
        if (string.IsNullOrWhiteSpace(pool))
            throw new ArgumentException("Action-point pool cannot be empty.", nameof(pool));
        return pool.Trim().ToLowerInvariant();
    }

    private static int GetNonNegative(Dictionary<string, int> values, string key) =>
        values.TryGetValue(key, out var value) ? Math.Max(0, value) : 0;

    private static bool TrySpend(ref int balance, int amount)
    {
        if (amount < 0 || balance < amount)
            return false;
        balance -= amount;
        return true;
    }

    private static int SaturatingAdd(int value, int amount)
    {
        if (amount <= 0)
            return value;
        var result = (long)value + amount;
        return result > int.MaxValue ? int.MaxValue : (int)result;
    }
}