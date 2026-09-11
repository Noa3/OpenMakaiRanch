using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Data;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Gameplay;

// Explicit stored values: never reorder or reuse a retired field/cause ID.
public enum DevelopmentField { Ranch = 0, Craft = 1, Combat = 2, Magic = 3, MaxHp = 4, MaxEnergy = 5,
    MaxSpirit = 6, MaxMana = 7, HeightMillimetres = 8, Conditioning = 9, Attunement = 10 }
public enum DevelopmentCause { Practice = 1, Workday = 2 }
public sealed record DevelopmentValue(DevelopmentField Field, int Current, int? Baseline);
public sealed record DevelopmentChange(int Day, DevelopmentCause Cause, DevelopmentField Field, int Before, int After);
public sealed record DevelopmentMorph(double Conditioning, double Attunement, int HeightMillimetres, string BodyTypeId);
public sealed record CharacterDevelopmentSnapshot(string CharacterId, int Revision, int? FirstObservedDay,
    DevelopmentMorph Morph, IReadOnlyList<DevelopmentValue> Values, IReadOnlyList<DevelopmentChange> RecentChanges,
    CharacterProtectionSnapshot Protection);

/// <summary>
/// Observe existing, successful non-explicit growth; never run a second XP/work/clock system.
/// Persistent fixed-size flags hold a first-observed baseline, high-water marks and a short journal.
/// </summary>
public sealed class CharacterDevelopmentService(SaveState state, DataRegistry data, FlagService flags)
{
    public const int FlagStart = 1_231_000;
    public const int JournalCapacity = 12;
    private const int BaselineDay = FlagStart, Revision = FlagStart + 1;
    private const int ConditioningPoints = FlagStart + 2, AttunementPoints = FlagStart + 3;
    private const int JournalNext = FlagStart + 4, JournalCount = FlagStart + 5;
    private const int BaselineStart = FlagStart + 20, HighWaterStart = FlagStart + 40, JournalStart = FlagStart + 100;
    public const int FlagEnd = JournalStart + JournalCapacity * 5 - 1;

    internal sealed class Observation(CharacterState character, SaveState owner, FlagService storage,
        int day, int revision, int[] before, bool eligible, DevelopmentCause cause)
    {
        internal CharacterState Character { get; } = character;
        internal string CharacterId { get; } = character.Id;
        internal SaveState Owner { get; } = owner;
        internal FlagService Storage { get; } = storage;
        internal int Day { get; } = day;
        internal int Revision { get; } = revision;
        internal int[] Before { get; } = before;
        internal bool Eligible { get; } = eligible;
        internal DevelopmentCause Cause { get; } = cause;
        internal bool Consumed { get; set; }
    }

    public CharacterDevelopmentSnapshot? Inspect(string? id)
    {
        var c = Find(id);
        if (c is null) return null;
        var current = Values(c);
        var baselineDay = Get(c, BaselineDay);
        var values = Enumerable.Range(0, current.Length).Select(i => new DevelopmentValue((DevelopmentField)i,
            current[i], baselineDay > 0 ? Get(c, BaselineStart + i) : null)).ToArray();
        var changes = new List<DevelopmentChange>();
        var count = Math.Clamp(Get(c, JournalCount), 0, JournalCapacity);
        var next = Math.Clamp(Get(c, JournalNext), 0, JournalCapacity - 1);
        for (var i = 0; i < count; i++)
        {
            var slot = JournalStart + ((next - count + i + JournalCapacity) % JournalCapacity) * 5;
            var cause = (DevelopmentCause)Get(c, slot + 1);
            var field = (DevelopmentField)Get(c, slot + 2);
            if (Get(c, slot) > 0 && Enum.IsDefined(cause) && Enum.IsDefined(field))
                changes.Add(new(Get(c, slot), cause, field, Get(c, slot + 3), Get(c, slot + 4)));
        }
        return new(c.Id, Get(c, Revision), baselineDay > 0 ? baselineDay : null,
            new(current[9] / 6.0, current[10] / 12.0, c.Height, c.BodyTypeOverride),
            Array.AsReadOnly(values), changes.AsReadOnly(), CharacterProtectionService.Inspect(c));
    }

    internal Observation? Capture(string id, DevelopmentCause cause)
    {
        var c = Find(id);
        if (c is null || c.Id == "anon" || state.Calendar.Day < 1 || !Enum.IsDefined(cause)) return null;
        return new(c, state, flags, state.Calendar.Day, Get(c, Revision), Values(c),
            !CharacterProtectionService.NeedsRecovery(c), cause);
    }

    internal IReadOnlyList<Observation> CaptureWorkday() => state.Roster.Characters
        .Select(c => Capture(c.Id, DevelopmentCause.Workday)).OfType<Observation>().ToArray();

    internal void CompleteWorkday(IReadOnlyList<Observation> observations, DailyReport report)
    {
        foreach (var observation in observations)
        {
            if (observation.Day != report.Day) continue;
            var changes = Complete(observation);
            foreach (var change in changes)
                report.Lines.Add(Describe(observation.Character.DisplayNameOverride.Length > 0
                    ? observation.Character.DisplayNameOverride : observation.Character.Id, change));
        }
    }

    internal IReadOnlyList<DevelopmentChange> Complete(Observation? observation)
    {
        if (observation is null || observation.Consumed || !ReferenceEquals(observation.Owner, state)
            || !ReferenceEquals(observation.Storage, flags)) return Array.Empty<DevelopmentChange>();
        observation.Consumed = true;
        var c = observation.Character;
        var expectedDay = (long)observation.Day + (observation.Cause == DevelopmentCause.Workday ? 1 : 0);
        if (c.Id != observation.CharacterId || !ReferenceEquals(Find(c.Id), c) || expectedDay != state.Calendar.Day
            || Get(c, Revision) != observation.Revision || observation.Revision == int.MaxValue)
            return Array.Empty<DevelopmentChange>();
        var before = observation.Before;
        var after = Values(c);
        // Successful no-growth days may establish the first observed baseline, never retroactive gains.
        if (Get(c, BaselineDay) <= 0)
        {
            Set(c, BaselineDay, observation.Day);
            for (var i = 0; i < before.Length; i++) Set(c, BaselineStart + i, before[i]);
            for (var i = 0; i < 4; i++) Set(c, HighWaterStart + i, before[i]);
        }
        var combatGain = PositiveNewHigh(c, 2, before[2], after[2]);
        var magicGain = PositiveNewHigh(c, 3, before[3], after[3]);
        // Only these existing gameplay boundaries can award these small, bounded remake benefits.
        // Exhaustion, source-ward depletion and mental state changes are never an alternate route.
        if (observation.Eligible && !CharacterProtectionService.NeedsRecovery(c))
        {
            var conditioning = (int)Math.Min(6, (long)before[9] + combatGain);
            var attunement = (int)Math.Min(12, (long)before[10] + magicGain);
            var hpGain = (conditioning / 2 - before[9] / 2) * 5;
            var manaGain = (attunement / 4 - before[10] / 4) * 5;
            if (hpGain > 0 && after[4] > 0)
                c.MaxHpOverride = (int)Math.Min(int.MaxValue, (long)after[4] + hpGain);
            if (manaGain > 0 && c.MaxMana >= 0)
                c.MaxMana = (int)Math.Min(int.MaxValue, (long)c.MaxMana + manaGain);
            // Capacity growth does not refill current HP, EP, SP, MP or daily stamina.
            Set(c, ConditioningPoints, conditioning); Set(c, AttunementPoints, attunement);
        }
        for (var i = 0; i < 4; i++) Set(c, HighWaterStart + i, Math.Max(Get(c, HighWaterStart + i), after[i]));
        after = Values(c);
        var result = new List<DevelopmentChange>();
        for (var i = 0; i < before.Length; i++)
        {
            if (before[i] == after[i]) continue;
            var change = new DevelopmentChange(observation.Day, observation.Cause, (DevelopmentField)i, before[i], after[i]);
            Append(c, change); result.Add(change);
        }
        Set(c, Revision, observation.Revision + 1);
        return result.AsReadOnly();
    }

    public static string Describe(string displayName, DevelopmentChange change) =>
        T("character.development.change", "{0}: {1} {2} → {3} ({4}, day {5}).", displayName,
            FieldName(change.Field), change.Before, change.After,
            change.Cause == DevelopmentCause.Practice
                ? T("character.development.practice", "practice") : T("character.development.workday", "workday"), change.Day);

    private static string FieldName(DevelopmentField field) => field switch
    {
        DevelopmentField.Ranch => T("character.development.ranch", "ranch skill"),
        DevelopmentField.Craft => T("character.development.craft", "craft skill"),
        DevelopmentField.Combat => T("character.development.combat", "combat skill"),
        DevelopmentField.Magic => T("character.development.magic", "magical aptitude"),
        DevelopmentField.MaxHp => T("character.development.hp", "health capacity"),
        DevelopmentField.MaxEnergy => T("character.development.energy", "energy capacity"),
        DevelopmentField.MaxSpirit => T("character.development.spirit", "spirit capacity"),
        DevelopmentField.MaxMana => T("character.development.mana", "mana capacity"),
        DevelopmentField.HeightMillimetres => T("character.development.height", "height (mm)"),
        DevelopmentField.Conditioning => T("character.development.conditioning", "conditioning"),
        _ => T("character.development.attunement", "attunement")
    };

    private CharacterState? Find(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        var matches = state.Roster.Characters.Where(c => c.Id == id).Take(2).ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }
    private int[] Values(CharacterState c)
    {
        data.Characters.TryGetValue(c.DefinitionId ?? c.Id, out var definition);
        return new[] { c.RanchSkill, c.CraftSkill, c.CombatSkill, c.MagicPower,
            c.MaxHpOverride ?? definition?.MaxHp ?? 100, c.MaxEnergyOverride ?? definition?.MaxEnergy ?? 150,
            c.MaxSpirit, c.MaxMana, c.Height,
            Math.Clamp(Get(c, ConditioningPoints), 0, 6), Math.Clamp(Get(c, AttunementPoints), 0, 12) };
    }
    private long PositiveNewHigh(CharacterState c, int skill, int before, int after) =>
        before < 0 || after < 0 ? 0 : Math.Max(0L, (long)after - Math.Max(before, Get(c, HighWaterStart + skill)));
    private int Get(CharacterState c, int flag) => flags.GetCharIntFlag(c.Id, flag);
    private void Set(CharacterState c, int flag, int value) => flags.SetCharIntFlag(c.Id, flag, value);
    private void Append(CharacterState c, DevelopmentChange change)
    {
        var next = Math.Clamp(Get(c, JournalNext), 0, JournalCapacity - 1);
        var slot = JournalStart + next * 5;
        Set(c, slot, change.Day); Set(c, slot + 1, (int)change.Cause); Set(c, slot + 2, (int)change.Field);
        Set(c, slot + 3, change.Before); Set(c, slot + 4, change.After);
        Set(c, JournalNext, (next + 1) % JournalCapacity);
        Set(c, JournalCount, Math.Min(JournalCapacity, Math.Clamp(Get(c, JournalCount), 0, JournalCapacity) + 1));
    }
}
