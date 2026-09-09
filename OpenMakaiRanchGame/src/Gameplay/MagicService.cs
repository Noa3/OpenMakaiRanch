using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

/// <summary>
/// Personal player MP and ranch-stored mana are deliberately separate. This mirrors the original
/// BASE:0:魔力 / MAXBASE:0:魔力 versus stored-mana split: spells spend personal MP, while ranch
/// storage can refill it after capacity has been unlocked.
/// </summary>
public sealed class MagicService
{
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
    public int SpiritEnergy => _state.Economy.SpiritEnergy;

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

    public int RechargePlayerManaFromStorage(int requestedAmount = int.MaxValue)
    {
        Normalize();
        if (requestedAmount <= 0 || MaxMana <= 0 || CurrentMana >= MaxMana || StoredMana <= 0)
            return 0;

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
    }
}
