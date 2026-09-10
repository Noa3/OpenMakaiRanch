using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

/// <summary>
/// Original-style personal MP versus ranch-stored mana.
/// Personal MP (BASE:0:魔力) pays for magic. Stored mana (MONEY:貯蔵魔力) is a separate reserve.
/// Rest recovery can store one fifth of sufficiently large overflow when a reservoir exists, while
/// transferring stored mana back into the player requires the original Magic Supply Device.
/// </summary>
public sealed class MagicService
{
    public const int HomeStorageCapacity = 10_000;
    public const int BusinessStorageCapacity = 50_000;
    public const int LargeStorageCapacity = 1_000_000;
    public const int ModifiedStorageCapacity = 10_000_000;
    public const int MinimumOverflowStorageGain = 1_000;

    private readonly SaveState _state;
    private readonly DataRegistry _data;

    public MagicService(SaveState state, DataRegistry data)
    {
        _state = state;
        _data = data;
        Normalize();
    }

    public int CurrentMana => _state.Player.Mana;
    public int MaxMana => _state.Player.MaxMana;
    public int StoredMana => _state.Economy.ManaReservoir;
    public int StorageCapacity => StorageCapacityFor(_state);
    public int SpiritEnergy => _state.Economy.SpiritEnergy;
    public bool HasManaSupplyDevice => HasItem(_state, "magic_supply_device");

    public bool CanSpendPlayerMana(int amount) => amount >= 0 && CurrentMana >= amount;

    public bool SpendPlayerMana(int amount)
    {
        if (amount <= 0 || !CanSpendPlayerMana(amount))
            return false;

        _state.Player.Mana -= amount;
        return true;
    }

    public bool CanCast(string spellId, int manaCost) =>
        !string.IsNullOrWhiteSpace(spellId) && manaCost >= 0 && CanSpendPlayerMana(manaCost);

    public bool CastSpell(string spellId, int manaCost, string casterId)
    {
        if (!CanCast(spellId, manaCost) || !SpendPlayerMana(manaCost))
            return false;

        ApplySpellEffect(spellId, manaCost);
        return true;
    }

    /// <summary>
    /// Original Magic Supply Device behavior: stored mana can refill current personal MP but never
    /// increases Max MP. Without the device this transfer is unavailable.
    /// </summary>
    public int RechargePlayerManaFromStorage(int requestedAmount = int.MaxValue)
    {
        Normalize();
        if (!HasManaSupplyDevice || requestedAmount <= 0 || MaxMana <= 0
            || CurrentMana >= MaxMana || StoredMana <= 0)
        {
            return 0;
        }

        var transferred = Math.Min(requestedAmount, Math.Min(MaxMana - CurrentMana, StoredMana));
        _state.Economy.ManaReservoir -= transferred;
        _state.Player.Mana += transferred;
        return transferred;
    }

    public int IncreasePlayerManaCapacity(int amount, bool fillNewCapacity = false)
    {
        if (amount <= 0)
            return 0;

        var before = MaxMana;
        _state.Player.MaxMana = before > int.MaxValue - amount ? int.MaxValue : before + amount;
        var gained = _state.Player.MaxMana - before;
        if (fillNewCapacity)
            _state.Player.Mana = Math.Min(_state.Player.MaxMana, _state.Player.Mana + gained);
        return gained;
    }

    public int RegeneratePlayerMana(int amount)
    {
        Normalize();
        if (amount <= 0 || MaxMana <= 0)
            return 0;

        var before = CurrentMana;
        _state.Player.Mana = Math.Min(MaxMana, CurrentMana + amount);
        return _state.Player.Mana - before;
    }

    /// <summary>
    /// Rest/day-rollover recovery from the original MP_HEAL_RATIO path. The player receives the
    /// configured percentage of Max MP. If that heal overflows Max MP, one fifth of the overflow is
    /// stored when it yields at least 1,000 MP and an actual reservoir is available.
    /// </summary>
    public static (int recovered, int stored) RecoverPlayerManaForRest(SaveState state)
    {
        var player = state.Player;
        player.MaxMana = Math.Max(0, player.MaxMana);
        player.Mana = Math.Clamp(player.Mana, 0, player.MaxMana);
        player.ManaRecoveryPercent = Math.Clamp(player.ManaRecoveryPercent, 0, 100);
        state.Economy.ManaReservoir = Math.Max(0, state.Economy.ManaReservoir);

        if (player.MaxMana <= 0 || player.ManaRecoveryPercent <= 0)
            return (0, 0);

        var heal = Math.Max(1, player.MaxMana * player.ManaRecoveryPercent / 100);
        var before = player.Mana;
        player.Mana = Math.Min(player.MaxMana, player.Mana + heal);
        var recovered = player.Mana - before;

        var overflow = Math.Max(0, before + heal - player.Mana);
        var storageGain = overflow / 5;
        var capacity = StorageCapacityFor(state);
        if (storageGain < MinimumOverflowStorageGain || capacity <= 0 || state.Economy.ManaReservoir >= capacity)
            return (recovered, 0);

        var beforeStored = state.Economy.ManaReservoir;
        state.Economy.ManaReservoir = Math.Min(capacity, beforeStored + storageGain);
        return (recovered, state.Economy.ManaReservoir - beforeStored);
    }

    public static int StorageCapacityFor(SaveState state)
    {
        if (HasItem(state, "magic_storage_mod") || HasFacility(state, "magic_storage_3"))
            return ModifiedStorageCapacity;
        if (HasItem(state, "magic_storage_huge"))
            return LargeStorageCapacity;
        if (HasItem(state, "magic_storage_large") || HasFacility(state, "magic_storage_2"))
            return BusinessStorageCapacity;
        if (HasItem(state, "magic_storage_small") || HasFacility(state, "magic_storage"))
            return HomeStorageCapacity;
        return 0;
    }

    public List<ItemDefinition> AvailableMagicItems() => _data.Items.Values
        .Where(i => i.Category == ItemCategory.Consumable)
        .OrderBy(i => i.Price)
        .ToList();

    public List<SkillDefinition> MagicSkills() => _data.Skills.Values
        .Where(s => s.Id.Contains("arcane", StringComparison.OrdinalIgnoreCase)
            || s.Id.Contains("magic", StringComparison.OrdinalIgnoreCase))
        .ToList();

    private void ApplySpellEffect(string spellId, int manaCost)
    {
        switch (spellId)
        {
            case "mana_regen":
                RegeneratePlayerMana(manaCost * 2);
                break;
            case "morale_boost":
                foreach (var character in _state.Roster.Characters)
                    character.Morale = Math.Clamp(character.Morale + Math.Max(2, manaCost / 2), 0, 100);
                break;
            case "fatigue_reduce":
                foreach (var character in _state.Roster.Characters)
                    character.Fatigue = Math.Clamp(character.Fatigue - Math.Max(2, manaCost / 2), 0, 100);
                break;
        }
    }

    private void Normalize()
    {
        _state.Player.MaxMana = Math.Max(0, _state.Player.MaxMana);
        _state.Player.Mana = Math.Clamp(_state.Player.Mana, 0, _state.Player.MaxMana);
        _state.Player.ManaRecoveryPercent = Math.Clamp(_state.Player.ManaRecoveryPercent, 0, 100);
        _state.Economy.ManaReservoir = Math.Max(0, _state.Economy.ManaReservoir);
        var capacity = StorageCapacity;
        if (capacity > 0)
            _state.Economy.ManaReservoir = Math.Min(capacity, _state.Economy.ManaReservoir);
    }

    private static bool HasItem(SaveState state, string itemId) =>
        state.Inventory.Items.TryGetValue(itemId, out var count) && count > 0;

    private static bool HasFacility(SaveState state, string facilityId) =>
        state.Ranch.Facilities.TryGetValue(facilityId, out var level) && level > 0;
}
