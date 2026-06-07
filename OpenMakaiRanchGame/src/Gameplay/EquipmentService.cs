using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

public sealed class EquipmentService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private static readonly string[] SlotKeys = { "weapon", "armor", "accessory", "head", "feet" };

    public EquipmentService(SaveState state, DataRegistry data)
    {
        _state = state;
        _data = data;
    }

    public bool Equip(string characterId, string itemId)
    {
        var character = _state.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null) return false;

        if (!_data.Items.TryGetValue(itemId, out var item))
            return false;
        if (item.Category != ItemCategory.Equipment)
            return false;

        var slot = SlotForCategory(item.Slot);
        if (slot is null) return false;

        character.EquippedItems ??= new Dictionary<string, string>();

        // Unequip current item back to inventory
        if (character.EquippedItems.TryGetValue(slot, out var currentItemId))
        {
            _state.Inventory.Items[currentItemId] = _state.Inventory.Items.GetValueOrDefault(currentItemId) + 1;
        }

        // Remove from inventory
        if (!_state.Inventory.Items.TryGetValue(itemId, out var count) || count <= 0)
            return false;
        if (count <= 1)
            _state.Inventory.Items.Remove(itemId);
        else
            _state.Inventory.Items[itemId] = count - 1;

        character.EquippedItems[slot] = itemId;
        return true;
    }

    public bool Unequip(string characterId, string slot)
    {
        var character = _state.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null) return false;
        character.EquippedItems ??= new Dictionary<string, string>();

        if (!character.EquippedItems.TryGetValue(slot, out var itemId))
            return false;

        _state.Inventory.Items[itemId] = _state.Inventory.Items.GetValueOrDefault(itemId) + 1;
        character.EquippedItems.Remove(slot);
        return true;
    }

    public ItemDefinition? GetEquippedItem(string characterId, string slot)
    {
        var character = _state.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null) return null;
        character.EquippedItems ??= new Dictionary<string, string>();

        if (!character.EquippedItems.TryGetValue(slot, out var itemId))
            return null;

        _data.Items.TryGetValue(itemId, out var item);
        return item;
    }

    public int BonusRanchSkill(string characterId)
    {
        return SumBonuses(characterId, i => i.BonusRanchSkill);
    }

    public int BonusCraftSkill(string characterId)
    {
        return SumBonuses(characterId, i => i.BonusCraftSkill);
    }

    public int BonusCombatSkill(string characterId)
    {
        return SumBonuses(characterId, i => i.BonusCombatSkill);
    }

    public int BonusMaxHp(string characterId)
    {
        return SumBonuses(characterId, i => i.BonusMaxHp);
    }

    public int BonusMaxEnergy(string characterId)
    {
        return SumBonuses(characterId, i => i.BonusMaxEnergy);
    }

    private int SumBonuses(string characterId, System.Func<ItemDefinition, int> selector)
    {
        var character = _state.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null) return 0;
        character.EquippedItems ??= new Dictionary<string, string>();

        int total = 0;
        foreach (var kvp in character.EquippedItems)
        {
            if (_data.Items.TryGetValue(kvp.Value, out var item))
                total += selector(item);
        }
        return total;
    }

    private static string? SlotForCategory(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Weapon => "weapon",
        EquipmentSlot.Armor => "armor",
        EquipmentSlot.Accessory => "accessory",
        EquipmentSlot.Head => "head",
        EquipmentSlot.Feet => "feet",
        _ => null
    };

    public static string SlotDisplayName(string slot) => slot switch
    {
        "weapon" => "Weapon",
        "armor" => "Armor",
        "accessory" => "Accessory",
        "head" => "Head",
        "feet" => "Feet",
        _ => slot
    };
}
