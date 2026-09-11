using System;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Gameplay;

/// <summary>Records payments already applied by the existing services. Never pays or invents rewards.</summary>
internal sealed class DailyGoldLedger(int openingBalance)
{
    private long _income;
    private long _expenses;

    public void RecordWorkAndUpkeep(int income, int expenses, int balanceAfter)
    {
        _income = Math.Max(0, income);
        _expenses = Math.Max(0, expenses);
        var nominal = _income - _expenses;
        var actual = (long)balanceAfter - openingBalance;
        // The existing wallet clamps at its integer limits. Report only credited/paid gold.
        _income -= Math.Max(0, nominal - actual);
        _expenses -= Math.Max(0, actual - nominal);
    }

    public int RecordChange(int before, int after)
    {
        var change = (long)after - before;
        if (change >= 0) _income += change;
        else _expenses -= change;
        return Bounded(change);
    }

    public void Complete(DailyReport report, EconomyState economy)
    {
        var delta = (long)economy.Gold - openingBalance;
        report.Income = Bounded(_income);
        report.Expenses = Bounded(_expenses);
        report.NetGold = Bounded(delta);
        economy.LastIncome = report.Income;
        economy.LastExpenses = report.Expenses;
        report.Lines.Add($"Gold balance: {openingBalance} -> {economy.Gold} ({delta:+0;-0;0} gold). Income includes shipments, events and milestones.");
        if (_income > int.MaxValue || _expenses > int.MaxValue || delta > int.MaxValue || delta < int.MinValue)
            report.Lines.Add($"Display limit reached: exact receipts {_income}, payments {_expenses}, net {delta} gold.");
    }

    private static int Bounded(long value) => (int)Math.Clamp(value, int.MinValue, int.MaxValue);
}
