using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

/// <summary>
/// Handles equipment slot mapping, bonus computation, clothing style resolution,
/// and item-use effects (potions, drugs, transformations).
/// </summary>
public sealed class ClothingService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;

    public ClothingService(SaveState state, DataRegistry data)
    {
        _state = state;
        _data = data;
    }

    // Canonical map in SaveState: slot-key -> itemId
    private static string SlotToDictionaryKey(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Weapon => "weapon",
        EquipmentSlot.Armor => "armor",
        EquipmentSlot.Accessory => "accessory",
        EquipmentSlot.Head => "head",
        EquipmentSlot.Feet => "feet",
        EquipmentSlot.UnderwearTop => "underwear_top",
        EquipmentSlot.UnderwearBottom => "underwear_bottom",
        EquipmentSlot.Necklace => "necklace",
        EquipmentSlot.Coat => "coat",
        EquipmentSlot.Ears => "ears",
        EquipmentSlot.Arms => "arms",
        EquipmentSlot.Legs => "legs",
        _ => slot.ToString().ToLowerInvariant()
    };

    private static bool TryParseSlotKey(string value, out EquipmentSlot slot)
    {
        if (Enum.TryParse<EquipmentSlot>(value, true, out slot))
        {
            return true;
        }

        var normalized = value.Trim().ToLowerInvariant();
        slot = normalized switch
        {
            "weapon" => EquipmentSlot.Weapon,
            "armor" => EquipmentSlot.Armor,
            "accessory" => EquipmentSlot.Accessory,
            "head" => EquipmentSlot.Head,
            "feet" => EquipmentSlot.Feet,
            "underwear_top" or "underweartop" => EquipmentSlot.UnderwearTop,
            "underwear_bottom" or "underwearbottom" => EquipmentSlot.UnderwearBottom,
            "necklace" or "neck" => EquipmentSlot.Necklace,
            "coat" or "jacket" => EquipmentSlot.Coat,
            "ears" or "eyes" => EquipmentSlot.Ears,
            "arms" => EquipmentSlot.Arms,
            "legs" => EquipmentSlot.Legs,
            _ => default
        };

        return normalized is "weapon" or "armor" or "accessory" or "head" or "feet"
            or "underwear_top" or "underweartop" or "underwear_bottom" or "underwearbottom"
            or "necklace" or "neck" or "coat" or "jacket" or "ears" or "eyes" or "arms" or "legs";
    }

    /// <summary>
    /// Equip an item to the appropriate slot for a character.
    /// Returns (success, error) tuple.
    /// </summary>
    public (bool Success, string Error) EquipItem(CharacterState character, string itemId)
    {
        NormalizeAndRebuildEquipment(character);

        if (!_data.Items.TryGetValue(itemId, out var item))
        {
            return (false, $"Unknown item: {itemId}");
        }

        if (item.Category != ItemCategory.Equipment)
        {
            return (false, $"Item {itemId} is not equipment");
        }

        if (!_state.Inventory.Items.TryGetValue(itemId, out var quantity) || quantity <= 0)
        {
            return (false, $"Item {itemId} is not in inventory");
        }

        var slot = item.Slot;
        var slotKey = SlotToDictionaryKey(slot);

        if (character.EquippedItems.TryGetValue(slotKey, out var currentItemId))
        {
            if (currentItemId == itemId)
            {
                return (true, string.Empty);
            }

            _state.Inventory.Items[currentItemId] = _state.Inventory.Items.GetValueOrDefault(currentItemId, 0) + 1;
        }

        if (quantity <= 1)
        {
            _state.Inventory.Items.Remove(itemId);
        }
        else
        {
            _state.Inventory.Items[itemId] = quantity - 1;
        }

        character.EquippedItems[slotKey] = itemId;
        NormalizeAndRebuildEquipment(character);

        return (true, string.Empty);
    }

    /// <summary>
    /// Unequip an item from a specific slot.
    /// </summary>
    public (bool Success, string Error) UnequipItem(CharacterState character, EquipmentSlot slot)
    {
        NormalizeAndRebuildEquipment(character);

        var slotKey = SlotToDictionaryKey(slot);
        if (!character.EquippedItems.TryGetValue(slotKey, out var currentItemId))
        {
            return (false, $"Nothing equipped in slot {slot}");
        }

        character.EquippedItems.Remove(slotKey);

        // Return to inventory
        _state.Inventory.Items[currentItemId] = _state.Inventory.Items.GetValueOrDefault(currentItemId, 0) + 1;

        NormalizeAndRebuildEquipment(character);

        return (true, string.Empty);
    }

    public string GetEquippedItemId(CharacterState character, EquipmentSlot slot)
    {
        NormalizeAndRebuildEquipment(character);
        return GetCurrentEquippedItemId(character, slot);
    }

    public void SyncCharacterEquipment(CharacterState character)
    {
        NormalizeAndRebuildEquipment(character);
    }

    /// <summary>
    /// Apply item bonuses to a character. Positive multiplier adds, negative removes.
    /// </summary>
    private void ApplyItemBonuses(CharacterState character, ItemDefinition item, int multiplier)
    {
        // Apply bonuses to EquipmentState
        character.Equipment.TotalBonusRanchSkill += item.BonusRanchSkill * multiplier;
        character.Equipment.TotalBonusCraftSkill += item.BonusCraftSkill * multiplier;
        character.Equipment.TotalBonusCombatSkill += item.BonusCombatSkill * multiplier;
        character.Equipment.TotalBonusMaxHp += item.BonusMaxHp * multiplier;
        character.Equipment.TotalBonusMaxEnergy += item.BonusMaxEnergy * multiplier;
        character.Equipment.TotalBonusMorale += item.BonusMorale * multiplier;
    }

    /// <summary>
    /// Get the item ID currently equipped in a slot.
    /// </summary>
    private string GetCurrentEquippedItemId(CharacterState character, EquipmentSlot slot)
    {
        var slotKey = SlotToDictionaryKey(slot);
        return character.EquippedItems.TryGetValue(slotKey, out var itemId) ? itemId : string.Empty;
    }

    /// <summary>
    /// Update the character's active clothing style based on equipped items.
    /// Uses the most prominent (highest count) clothing style.
    /// </summary>
    private void UpdateClothingStyle(CharacterState character)
    {
        var styleCounts = new Dictionary<ClothingStyle, int>();

        foreach (var kvp in character.EquippedItems)
        {
            if (!_data.Items.TryGetValue(kvp.Value, out var item))
                continue;

            var style = item.ClothingStyleValue;
            if (style == ClothingStyle.Default)
                continue;

            if (!styleCounts.ContainsKey(style))
                styleCounts[style] = 0;

            styleCounts[style]++;
        }

        // Find most prominent style
        var maxCount = 0;
        var dominantStyle = ClothingStyle.Default;

        foreach (var kvp in styleCounts)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                dominantStyle = kvp.Key;
            }
        }

        character.Equipment.ActiveClothingStyle = dominantStyle;
    }

    private void NormalizeAndRebuildEquipment(CharacterState character)
    {
        character.EquippedItems ??= new Dictionary<string, string>();

        var normalized = new Dictionary<string, string>();
        foreach (var kvp in character.EquippedItems)
        {
            // canonical/legacy slot->item
            if (_data.Items.ContainsKey(kvp.Value) && TryParseSlotKey(kvp.Key, out var slotFromKey))
            {
                normalized[SlotToDictionaryKey(slotFromKey)] = kvp.Value;
                continue;
            }

            // older clothing prototype map item->slot
            if (_data.Items.ContainsKey(kvp.Key) && TryParseSlotKey(kvp.Value, out var slotFromValue))
            {
                normalized[SlotToDictionaryKey(slotFromValue)] = kvp.Key;
            }
        }

        character.EquippedItems = normalized;

        character.Equipment.Clear();
        foreach (var kvp in character.EquippedItems)
        {
            if (!_data.Items.TryGetValue(kvp.Value, out var item) || !TryParseSlotKey(kvp.Key, out var slot))
                continue;

            SetEquipmentStateSlot(character.Equipment, slot, kvp.Value);
            ApplyItemBonuses(character, item, 1);
        }

        UpdateClothingStyle(character);
    }

    private static void SetEquipmentStateSlot(EquipmentState equipment, EquipmentSlot slot, string itemId)
    {
        switch (slot)
        {
            case EquipmentSlot.Weapon:
                equipment.ClothesId = itemId;
                break;
            case EquipmentSlot.Armor:
                equipment.ArmorId = itemId;
                break;
            case EquipmentSlot.Accessory:
                equipment.AccessoryId = itemId;
                break;
            case EquipmentSlot.Head:
                equipment.HeadId = itemId;
                break;
            case EquipmentSlot.Feet:
            case EquipmentSlot.Legs:
                equipment.LegsId = itemId;
                break;
            case EquipmentSlot.UnderwearTop:
                equipment.UnderwearTopId = itemId;
                break;
            case EquipmentSlot.UnderwearBottom:
                equipment.UnderwearBottomId = itemId;
                break;
            case EquipmentSlot.Necklace:
                equipment.NecklaceId = itemId;
                break;
            case EquipmentSlot.Coat:
                equipment.CoatId = itemId;
                break;
            case EquipmentSlot.Ears:
                equipment.EyesId = itemId;
                break;
            case EquipmentSlot.Arms:
                equipment.ArmsId = itemId;
                break;
        }
    }

    /// <summary>
    /// Use an item on a character. Handles potions, drugs, consumables, and special effects.
    /// Returns (success, error, newEffectDescription).
    /// </summary>
    public (bool Success, string Error, string EffectDescription) UseItemOnCharacter(string itemId, CharacterState character)
    {
        if (!_data.Items.TryGetValue(itemId, out var item))
        {
            return (false, $"Unknown item: {itemId}", string.Empty);
        }

        if (item.Category == ItemCategory.Equipment)
        {
            var (success, error) = EquipItem(character, itemId);
            return (success, error, success ? $"Equipped {item.DisplayName}" : string.Empty);
        }

        if (item.Category != ItemCategory.Consumable)
        {
            return (false, $"Item {itemId} is not a consumable", string.Empty);
        }

        // Consume the item
        _state.Inventory.Items[itemId] = _state.Inventory.Items.GetValueOrDefault(itemId, 0) - 1;
        if (_state.Inventory.Items[itemId] <= 0)
        {
            _state.Inventory.Items.Remove(itemId);
        }

        var effectDesc = string.Empty;

        // Apply effect based on type
        switch (item.EffectType)
        {
            case ItemEffectType.EnergyRestore:
                character.Energy = Math.Clamp(character.Energy + item.EffectValue, 0, character.MaxEnergyOverride ?? 150);
                effectDesc = $"Restored {item.EffectValue} Energy.";
                break;

            case ItemEffectType.FatigueReduce:
                character.Fatigue = Math.Clamp(character.Fatigue - item.EffectValue, 0, 100);
                effectDesc = $"Reduced Fatigue by {item.EffectValue}.";
                break;

            case ItemEffectType.MoraleBoost:
                character.Morale = Math.Clamp(character.Morale + item.EffectValue, 0, 100);
                effectDesc = $"Boosted Morale by {item.EffectValue}.";
                break;

            case ItemEffectType.HpRestore:
                character.Hp = Math.Clamp(character.Hp + item.EffectValue, 0, character.MaxHpOverride ?? 150);
                effectDesc = $"Restored {item.EffectValue} HP.";
                break;

            case ItemEffectType.HairColorChange:
                character.HairColor = "Random";
                effectDesc = "Hair color changed permanently.";
                break;

            case ItemEffectType.MilkCapacityIncrease:
                character.Milk.Capacity += item.EffectValue;
                effectDesc = $"Milk capacity increased by {item.EffectValue}.";
                break;

            case ItemEffectType.MilkQualityIncrease:
                character.Milk.Quality = Math.Clamp(character.Milk.Quality + item.EffectValue, 0, 100);
                effectDesc = $"Milk quality increased by {item.EffectValue}.";
                break;

            case ItemEffectType.BreastSizeIncrease:
                character.BustSize += item.EffectValue;
                effectDesc = $"Breast size increased by {item.EffectValue}.";
                break;

            case ItemEffectType.SensitivityIncrease:
                character.Mature.PleasureV += item.EffectValue;
                character.Mature.PleasureA += item.EffectValue;
                effectDesc = $"Sensitivity increased by {item.EffectValue}.";
                break;

            case ItemEffectType.MilkConstitution:
                character.Milk.HasMilkConstitution = true;
                effectDesc = "Body constitution transformed to produce milk.";
                break;

            case ItemEffectType.MagicMilkConstitution:
                character.Milk.HasMagicMilkConstitution = true;
                effectDesc = "Body constitution transformed to produce magical milk.";
                break;

            case ItemEffectType.ConcentrationThicken:
                character.Milk.Concentration = "thick";
                effectDesc = "Milk concentration thickened.";
                break;

            case ItemEffectType.Transformation:
                character.Mature.Marks.Add("transformed");
                effectDesc = "Body underwent a transformation.";
                break;

            case ItemEffectType.TalentGrant:
                if (!character.Talents.Contains(item.EffectTarget))
                {
                    character.Talents.Add(item.EffectTarget);
                    effectDesc = $"Talent granted: {item.EffectTarget}.";
                }
                else
                {
                    effectDesc = "Talent already present.";
                }
                break;

            case ItemEffectType.FacilityUnlock:
                if (!_state.Ranch.Facilities.ContainsKey(item.EffectTarget))
                {
                    _state.Ranch.Facilities[item.EffectTarget] = 1;
                    effectDesc = $"Facility unlocked: {item.EffectTarget}.";
                }
                else
                {
                    effectDesc = "Facility already exists.";
                }
                break;

            case ItemEffectType.PetAdopt:
                if (!_state.Pets.AdoptedPetIds.Contains(item.EffectTarget))
                {
                    _state.Pets.AdoptedPetIds.Add(item.EffectTarget);
                    effectDesc = $"Pet adopted: {item.EffectTarget}.";
                }
                else
                {
                    effectDesc = "Pet already adopted.";
                }
                break;

            default:
                effectDesc = "Item used.";
                break;
        }

        return (true, string.Empty, effectDesc);
    }

    /// <summary>
    /// Get all equipment bonuses for a character.
    /// </summary>
    public (int RanchSkill, int CraftSkill, int CombatSkill, int MaxHp, int MaxEnergy, int Morale) GetTotalBonuses(CharacterState character)
    {
        NormalizeAndRebuildEquipment(character);

        var ranchSkill = 0;
        var craftSkill = 0;
        var combatSkill = 0;
        var maxHp = 0;
        var maxEnergy = 0;
        var morale = 0;

        foreach (var kvp in character.EquippedItems)
        {
            if (!_data.Items.TryGetValue(kvp.Value, out var item))
                continue;

            ranchSkill += item.BonusRanchSkill;
            craftSkill += item.BonusCraftSkill;
            combatSkill += item.BonusCombatSkill;
            maxHp += item.BonusMaxHp;
            maxEnergy += item.BonusMaxEnergy;
            morale += item.BonusMorale;
        }

        return (ranchSkill, craftSkill, combatSkill, maxHp, maxEnergy, morale);
    }
}
