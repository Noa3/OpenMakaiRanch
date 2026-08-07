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

    // Slot mapping from EquipmentSlot enum to EquipmentState fields
    private static string SlotToStateKey(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Armor => "ArmorId",
        EquipmentSlot.Head => "HeadId",
        EquipmentSlot.Weapon => "ClothesId",
        EquipmentSlot.Accessory => "AccessoryId",
        EquipmentSlot.Feet => "LegsId",
        EquipmentSlot.UnderwearTop => "UnderwearTopId",
        EquipmentSlot.UnderwearBottom => "UnderwearBottomId",
        EquipmentSlot.Necklace => "NecklaceId",
        EquipmentSlot.Coat => "CoatId",
        EquipmentSlot.Ears => "EyesId",
        EquipmentSlot.Arms => "ArmsId",
        EquipmentSlot.Legs => "LegsId",
        _ => string.Empty
    };

    /// <summary>
    /// Equip an item to the appropriate slot for a character.
    /// Returns (success, error) tuple.
    /// </summary>
    public (bool Success, string Error) EquipItem(CharacterState character, string itemId)
    {
        if (!_data.Items.TryGetValue(itemId, out var item))
        {
            return (false, $"Unknown item: {itemId}");
        }

        if (item.Category != ItemCategory.Equipment)
        {
            return (false, $"Item {itemId} is not equipment");
        }

        var slot = item.Slot;
        var slotKey = SlotToStateKey(slot);

        if (string.IsNullOrEmpty(slotKey))
        {
            return (false, $"No state field for slot {slot}");
        }

        // Unequip current item in this slot (return to inventory)
        var currentItemId = GetCurrentEquippedItemId(character, slot);
        if (!string.IsNullOrEmpty(currentItemId))
        {
            character.EquippedItems.Remove(currentItemId);
            _state.Inventory.Items[currentItemId] = _state.Inventory.Items.GetValueOrDefault(currentItemId, 0) + 1;
        }

        // Equip new item
        character.EquippedItems[itemId] = slot.ToString();
        _state.Inventory.Items[itemId] = _state.Inventory.Items.GetValueOrDefault(itemId, 0) - 1;
        if (_state.Inventory.Items[itemId] <= 0)
        {
            _state.Inventory.Items.Remove(itemId);
        }

        // Apply bonuses
        ApplyItemBonuses(character, item, 1);

        // Recompute clothing style
        UpdateClothingStyle(character);

        return (true, string.Empty);
    }

    /// <summary>
    /// Unequip an item from a specific slot.
    /// </summary>
    public (bool Success, string Error) UnequipItem(CharacterState character, EquipmentSlot slot)
    {
        var currentItemId = GetCurrentEquippedItemId(character, slot);
        if (string.IsNullOrEmpty(currentItemId))
        {
            return (false, $"Nothing equipped in slot {slot}");
        }

        if (!_data.Items.TryGetValue(currentItemId, out var item))
        {
            return (false, $"Unknown item {currentItemId}");
        }

        // Remove bonuses
        ApplyItemBonuses(character, item, -1);

        // Remove from equipped items
        character.EquippedItems.Remove(currentItemId);

        // Return to inventory
        _state.Inventory.Items[currentItemId] = _state.Inventory.Items.GetValueOrDefault(currentItemId, 0) + 1;

        // Recompute clothing style
        UpdateClothingStyle(character);

        return (true, string.Empty);
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
        foreach (var kvp in character.EquippedItems)
        {
            if (kvp.Value == slot.ToString())
            {
                return kvp.Key;
            }
        }
        return string.Empty;
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
            if (!_data.Items.TryGetValue(kvp.Key, out var item))
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
        var ranchSkill = 0;
        var craftSkill = 0;
        var combatSkill = 0;
        var maxHp = 0;
        var maxEnergy = 0;
        var morale = 0;

        foreach (var kvp in character.EquippedItems)
        {
            if (!_data.Items.TryGetValue(kvp.Key, out var item))
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
