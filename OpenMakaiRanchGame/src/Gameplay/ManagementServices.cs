using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

/// <summary>
/// Runtime gameplay services focused on inventory, progression, combat, and town systems.
/// Split from GameServices.cs to keep each module easier to navigate.
/// </summary>
public sealed class InventoryService
{
    private readonly SaveState _state;

    public InventoryService(SaveState state)
    {
        _state = state;
    }

    public IReadOnlyDictionary<string, int> Items => _state.Inventory.Items;

    public void AddItem(string itemId, int quantity)
    {
        if (quantity <= 0)
        {
            return;
        }

        _state.Inventory.Items.TryGetValue(itemId, out var currentQuantity);
        _state.Inventory.Items[itemId] = currentQuantity + quantity;
    }

    public bool TryConsume(string itemId, int quantity)
    {
        if (quantity <= 0 || !_state.Inventory.Items.TryGetValue(itemId, out var currentQuantity) || currentQuantity < quantity)
        {
            return false;
        }

        var remaining = currentQuantity - quantity;
        if (remaining == 0)
        {
            _state.Inventory.Items.Remove(itemId);
        }
        else
        {
            _state.Inventory.Items[itemId] = remaining;
        }

        return true;
    }

    public bool UseItemOnCharacter(string itemId, CharacterState character)
    {
        if (!TryConsume(itemId, 1))
        {
            return false;
        }

        var herbalism = _state.Research.UnlockedSkillIds.Contains("herbalism");

        switch (itemId)
        {
            case "meal_box":
                character.Fatigue = Math.Clamp(character.Fatigue - 18, 0, 100);
                character.Morale = Math.Clamp(character.Morale + 4, 0, 100);
                return true;
            case "energy_drink":
                character.Energy = Math.Clamp(character.Energy + 20, 0, character.MaxEnergyOverride ?? 150);
                character.Fatigue = Math.Clamp(character.Fatigue - 8, 0, 100);
                return true;
            case "herb_tea":
                character.Morale = Math.Clamp(character.Morale + (herbalism ? 18 : 12), 0, 100);
                character.Fatigue = Math.Clamp(character.Fatigue - (herbalism ? 8 : 5), 0, 100);
                return true;
            case "first_aid":
                character.Fatigue = Math.Clamp(character.Fatigue - 25, 0, 100);
                character.Hp = Math.Clamp(character.Hp + 15, 0, character.MaxHpOverride ?? 150);
                return true;
            case "lotion":
                character.Morale = Math.Clamp(character.Morale + 6, 0, 100);
                character.Fatigue = Math.Clamp(character.Fatigue - 4, 0, 100);
                return true;
            case "lube":
                character.Fatigue = Math.Clamp(character.Fatigue - 5, 0, 100);
                return true;
            case "hair_dye":
                character.Morale = Math.Clamp(character.Morale + 10, 0, 100);
                return true;
            case "collar_tag":
                character.Bond = Math.Clamp(character.Bond + 2, 0, 100);
                return true;
            case "guts_carrot":
                character.Energy = Math.Clamp(character.Energy + 25, 0, character.MaxEnergyOverride ?? 150);
                character.Fatigue = Math.Clamp(character.Fatigue + 3, 0, 100);
                return true;
            case "milk_tea":
                character.Morale = Math.Clamp(character.Morale + 8, 0, 100);
                return true;
            case "protein_bar":
                character.Fatigue = Math.Clamp(character.Fatigue - 12, 0, 100);
                character.Energy = Math.Clamp(character.Energy + 10, 0, character.MaxEnergyOverride ?? 150);
                return true;
            case "bandage":
                character.Hp = Math.Clamp(character.Hp + (herbalism ? 15 : 10), 0, character.MaxHpOverride ?? 150);
                character.Fatigue = Math.Clamp(character.Fatigue - (herbalism ? 8 : 5), 0, 100);
                return true;
            case "tonic":
                character.Hp = Math.Clamp(character.Hp + (herbalism ? 30 : 20), 0, character.MaxHpOverride ?? 150);
                character.Energy = Math.Clamp(character.Energy + (herbalism ? 22 : 15), 0, character.MaxEnergyOverride ?? 150);
                return true;
            default:
                return false;
        }
    }
}

public sealed class ShopService
{
    private readonly DataRegistry _data;
    private readonly EconomyService _economy;
    private readonly InventoryService _inventory;

    public ShopService(DataRegistry data, EconomyService economy, InventoryService inventory)
    {
        _data = data;
        _economy = economy;
        _inventory = inventory;
    }

    public bool Buy(string itemId, int quantity)
    {
        if (!_data.Items.TryGetValue(itemId, out var item) || quantity <= 0 || item.Price <= 0)
        {
            return false;
        }

        var cost = item.Price * quantity;
        if (!_economy.Spend(cost))
        {
            return false;
        }

        _inventory.AddItem(itemId, quantity);
        return true;
    }

    public bool Sell(string itemId, int quantity)
    {
        if (!_data.Items.TryGetValue(itemId, out var item) || quantity <= 0 || !_inventory.TryConsume(itemId, quantity))
        {
            return false;
        }

        _economy.AddGold(Math.Max(1, item.Price / 2) * quantity);
        return true;
    }
}

public sealed class AdventureService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private readonly EconomyService _economy;
    private readonly InventoryService _inventory;
    private readonly MilestoneService _milestones;
    private readonly SaveStateFactory _factory;

    public AdventureService(SaveState state, DataRegistry data, EconomyService economy, InventoryService inventory, MilestoneService milestones, Random? random = null)
    {
        _state = state;
        _data = data;
        _economy = economy;
        _inventory = inventory;
        _milestones = milestones;
        _factory = new SaveStateFactory(data, random);
    }

    public CombatReport ResolveMission(string missionId, IReadOnlyList<string> partyIds)
    {
        return ResolveMission(missionId, partyIds, false);
    }

    public CombatReport ResolveMission(string missionId, IReadOnlyList<string> partyIds, bool attemptCapture)
    {
        if (!_data.Missions.TryGetValue(missionId, out var mission))
        {
            return new CombatReport { MissionId = missionId, Outcome = MissionOutcome.Failure, Summary = "Mission data was missing." };
        }

        var party = _state.Roster.Characters.Where(character => partyIds.Contains(character.Id)).ToList();
        var turnLog = new List<string>();
        var partyPower = party.Sum(character => character.CombatSkill + character.Morale / 20 - character.Fatigue / 25);
        var tacticalSupport = party.Sum(character => character.CraftSkill / 2 + character.RanchSkill / 3);
        var openingRoll = DeterministicRoll(_state.Calendar.Day, mission.Id, 1);
        turnLog.Add($"Turn 1: Party engages with opening initiative {openingRoll}.");
        var pressureRoll = DeterministicRoll(_state.Calendar.Day, mission.Id, 2);
        turnLog.Add($"Turn 2: Enemy pressure rises by {pressureRoll + mission.Difficulty / 2}.");
        var closingRoll = DeterministicRoll(_state.Calendar.Day, mission.Id, 3);
        turnLog.Add($"Turn 3: Finisher sequence lands with force {closingRoll}.");

        var trainingBonus = _state.Research.UnlockedSkillIds.Contains("adventure_training") ? 6 : 0;
        var score = partyPower + tacticalSupport + openingRoll + closingRoll - pressureRoll + trainingBonus;
        var outcome = score >= mission.Difficulty + 6 ? MissionOutcome.Success : score >= mission.Difficulty ? MissionOutcome.PartialSuccess : MissionOutcome.Failure;
        var rewardGold = outcome == MissionOutcome.Success ? mission.RewardGold : outcome == MissionOutcome.PartialSuccess ? mission.RewardGold / 2 : 0;

        foreach (var character in party)
        {
            var medicineBonus = _state.Research.UnlockedSkillIds.Contains("field_medicine") ? 4 : 0;
            character.Fatigue = Math.Clamp(character.Fatigue + Math.Max(2, (outcome == MissionOutcome.Failure ? 18 : 10) - medicineBonus), 0, 100);
            character.Morale = Math.Clamp(character.Morale + (outcome == MissionOutcome.Failure ? -5 : 4), 0, 100);
        }

        if (rewardGold > 0)
        {
            _economy.AddGold(rewardGold);
        }

        if (outcome == MissionOutcome.Success && !string.IsNullOrWhiteSpace(mission.RewardItemId))
        {
            _inventory.AddItem(mission.RewardItemId, 1);
        }

        var captureSucceeded = false;
        var capturedCharacterId = string.Empty;
        if (attemptCapture)
        {
            var captureControl = party.Sum(character => character.CraftSkill + character.CombatSkill + character.Morale / 15);
            var captureRoll = DeterministicRoll(_state.Calendar.Day, mission.Id, 4);
            var captureScore = captureControl + captureRoll;
            var captureThreshold = mission.Difficulty * 3 + 18;
            captureSucceeded = outcome != MissionOutcome.Failure && captureScore >= captureThreshold;
            turnLog.Add($"Capture: control {captureScore} vs threshold {captureThreshold} ({(captureSucceeded ? "SUCCESS" : "FAILED")}).");

            if (captureSucceeded)
            {
                var recruit = _factory.CreateGeneratedRecruit(_state, new[] { _state.Recruitment.CurrentOffer?.Id ?? string.Empty });
                recruit.TraitOverride = string.IsNullOrWhiteSpace(recruit.TraitOverride) ? "Captured" : $"Captured {recruit.TraitOverride}";
                _state.Roster.Characters.Add(recruit);
                _state.Schedule.AssignedJobs[recruit.Id] = "rest";
                capturedCharacterId = recruit.Id;
            }
        }

        var report = new CombatReport
        {
            MissionId = mission.Id,
            Outcome = outcome,
            RewardGold = rewardGold,
            RewardItemId = outcome == MissionOutcome.Success ? mission.RewardItemId : string.Empty,
            CaptureAttempted = attemptCapture,
            CaptureSucceeded = captureSucceeded,
            CapturedCharacterId = capturedCharacterId,
            TurnLog = turnLog,
            Summary = $"{mission.DisplayName}: {outcome} with score {score} against difficulty {mission.Difficulty}."
        };

        if (attemptCapture)
        {
            var captureSummary = captureSucceeded
                ? $"Capture succeeded. New recruit {capturedCharacterId} joined the ranch."
                : "Capture attempt failed.";
            report.Summary += $" {captureSummary}";
            _state.Adventure.LastCaptureSummary = captureSummary;
        }

        _state.Adventure.LastMissionId = mission.Id;
        _state.Adventure.LastOutcome = outcome;
        _state.Adventure.LastSummary = report.Summary;
        var missionMilestone = _milestones.MarkMissionCompleted(mission.Id);
        if (!string.IsNullOrWhiteSpace(missionMilestone))
        {
            report.Summary += $" {missionMilestone}";
            _state.Adventure.LastSummary = report.Summary;
        }
        return report;
    }

    private static int DeterministicRoll(int day, string missionId, int phase)
    {
        var seed = day * (31 + phase * 7) + missionId.Sum(character => character) + phase * 11;
        return ((seed % 10) + 10) % 10 + 1;
    }
}

public sealed class MilestoneService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private readonly EconomyService _economy;

    public MilestoneService(SaveState state, DataRegistry data, EconomyService economy)
    {
        _state = state;
        _data = data;
        _economy = economy;
    }

    public IReadOnlyList<string> Completed => _state.Milestones.CompletedIds;

    public void CheckAfterSettlement(DailyReport report)
    {
        foreach (var milestone in _data.Milestones.Values)
        {
            if (_state.Milestones.CompletedIds.Contains(milestone.Id))
            {
                continue;
            }

            var completed = milestone.TriggerKind switch
            {
                MilestoneTriggerKind.DayReached => _state.Calendar.Day + 1 >= milestone.TriggerAmount,
                MilestoneTriggerKind.GoldReached => _economy.Gold >= milestone.TriggerAmount,
                MilestoneTriggerKind.BondReached => _state.Roster.Characters.Any(character => character.Bond >= milestone.TriggerAmount),
                MilestoneTriggerKind.ResearchUnlocked => (milestone.TriggerId == "any" && _state.Research.UnlockedSkillIds.Count > 0) || (milestone.TriggerId != "any" && _state.Research.UnlockedSkillIds.Contains(milestone.TriggerId)),
                MilestoneTriggerKind.CharacterCount => _state.Roster.Characters.Count >= milestone.TriggerAmount,
                MilestoneTriggerKind.FacilityMaster => _state.Ranch.Facilities.Count > 0 && _state.Ranch.Facilities.All(f => f.Value >= 5),
                MilestoneTriggerKind.PetCount => _state.Pets.AdoptedPetIds.Count >= milestone.TriggerAmount,
                MilestoneTriggerKind.EquipmentCount => _state.Inventory.Items.Count(kv => _data.Items.TryGetValue(kv.Key, out var item) && item.Category == ItemCategory.Equipment) >= milestone.TriggerAmount,
                _ => false
            };
            if (!completed)
            {
                continue;
            }

            _state.Milestones.CompletedIds.Add(milestone.Id);
            _economy.AddGold(milestone.RewardGold);
            _state.Economy.LastIncome += milestone.RewardGold;
            report.Income += milestone.RewardGold;
            report.NetGold += milestone.RewardGold;
            report.Lines.Add($"Milestone unlocked: {milestone.DisplayName} (+{milestone.RewardGold} gold)." );
        }
    }

    public string MarkMissionCompleted(string missionId)
    {
        var unlocked = new List<string>();
        foreach (var milestone in _data.Milestones.Values.Where(value =>
            value.TriggerKind == MilestoneTriggerKind.MissionCompleted &&
            (value.TriggerId == "any" || value.TriggerId == missionId)))
        {
            if (_state.Milestones.CompletedIds.Contains(milestone.Id))
            {
                continue;
            }

            _state.Milestones.CompletedIds.Add(milestone.Id);
            _economy.AddGold(milestone.RewardGold);
            _state.Economy.LastIncome += milestone.RewardGold;
            unlocked.Add($"Milestone unlocked: {milestone.DisplayName} (+{milestone.RewardGold} gold).");
        }

        return string.Join(" ", unlocked);
    }

    public void CheckBondMilestones()
    {
        foreach (var milestone in _data.Milestones.Values.Where(value => value.TriggerKind == MilestoneTriggerKind.BondReached))
        {
            if (_state.Milestones.CompletedIds.Contains(milestone.Id) || !_state.Roster.Characters.Any(character => character.Bond >= milestone.TriggerAmount))
            {
                continue;
            }

            _state.Milestones.CompletedIds.Add(milestone.Id);
            _economy.AddGold(milestone.RewardGold);
        }
    }

    public void CheckResearchMilestones(string skillId)
    {
        foreach (var milestone in _data.Milestones.Values.Where(value => value.TriggerKind == MilestoneTriggerKind.ResearchUnlocked))
        {
            var matches = milestone.TriggerId == "any" || milestone.TriggerId == skillId;
            if (!matches || _state.Milestones.CompletedIds.Contains(milestone.Id))
            {
                continue;
            }

            _state.Milestones.CompletedIds.Add(milestone.Id);
            _economy.AddGold(milestone.RewardGold);
        }
    }
}

public sealed class BondService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private readonly MilestoneService _milestones;

    public BondService(SaveState state, DataRegistry data, MilestoneService milestones)
    {
        _state = state;
        _data = data;
        _milestones = milestones;
    }

    public IReadOnlyList<BondEventDefinition> AvailableEvents(string characterId)
    {
        var character = _state.Roster.Characters.FirstOrDefault(value => value.Id == characterId);
        if (character is null)
        {
            return Array.Empty<BondEventDefinition>();
        }

        return _data.BondEvents.Values
            .Where(value => value.CharacterId == characterId && value.RequiredBond <= character.Bond && !_state.Bond.CompletedEventIds.Contains(value.Id))
            .OrderBy(value => value.RequiredBond)
            .ToList();
    }

    public void ConductMentorship(string characterId)
    {
        var character = _state.Roster.Characters.FirstOrDefault(value => value.Id == characterId);
        if (character is null)
        {
            return;
        }

        var bondGain = 5;
        if (_state.Research.UnlockedSkillIds.Contains("hospitality"))
        {
            bondGain += 3;
        }

        character.Bond = Math.Clamp(character.Bond + bondGain, 0, 100);
        character.Morale = Math.Clamp(character.Morale + 4, 0, 100);
        character.Fatigue = Math.Clamp(character.Fatigue + 4, 0, 100);
        _milestones.CheckBondMilestones();
    }

    public bool CompleteEvent(string eventId)
    {
        if (!_data.BondEvents.TryGetValue(eventId, out var bondEvent) || _state.Bond.CompletedEventIds.Contains(eventId))
        {
            return false;
        }

        var character = _state.Roster.Characters.FirstOrDefault(value => value.Id == bondEvent.CharacterId);
        if (character is null || character.Bond < bondEvent.RequiredBond)
        {
            return false;
        }

        character.Bond = Math.Clamp(character.Bond + bondEvent.BondReward, 0, 100);
        character.Morale = Math.Clamp(character.Morale + bondEvent.MoraleReward, 0, 100);
        if (!string.IsNullOrWhiteSpace(bondEvent.StockpileRewardId) && bondEvent.StockpileRewardAmount > 0)
        {
            _state.Ranch.Stockpile.TryGetValue(bondEvent.StockpileRewardId, out var currentAmount);
            _state.Ranch.Stockpile[bondEvent.StockpileRewardId] = currentAmount + bondEvent.StockpileRewardAmount;
        }

        _state.Bond.CompletedEventIds.Add(eventId);
        _milestones.CheckBondMilestones();
        return true;
    }
}

public sealed class TownService
{
    public IReadOnlyList<string> Actions { get; } = new[]
    {
        "General store",
        "Facility planning",
        "Research office",
        "Adventure guild",
        "Town hall records"
    };
}

public interface IMatureContentHooks
{
    string BondScenePlaceholder(string characterId);
}

public sealed class NullMatureContentHooks : IMatureContentHooks
{
    public string BondScenePlaceholder(string characterId)
    {
        // TODO(mature-content): Replace with pluggable external module hooks if policy/build rules allow.
        // This repository intentionally remains SFW and should not contain explicit content.
        return $"Mature scene hook for {characterId} is unavailable in this build.";
    }
}

public sealed class RecruitmentService
{
    public const int DefaultRecruitCost = 160;
    public const int RerollOfferCost = 30;

    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private readonly EconomyService _economy;
    private readonly SaveStateFactory _factory;

    public RecruitmentService(SaveState state, DataRegistry data, EconomyService economy, Random? random = null)
    {
        _state = state;
        _data = data;
        _economy = economy;
        _factory = new SaveStateFactory(data, random);
    }

    public CharacterState? CurrentOffer => _state.Recruitment.CurrentOffer;

    public CharacterState EnsureOffer()
    {
        if (_state.Recruitment.CurrentOffer is null)
        {
            _state.Recruitment.CurrentOffer = CreateOfferWithReservation();
        }

        return _state.Recruitment.CurrentOffer;
    }

    public bool RerollOffer(int cost = RerollOfferCost)
    {
        if (cost < 0 || !CanGenerateOffer() || _state.Recruitment.CurrentOffer is null || !_economy.Spend(cost))
        {
            return false;
        }

        _state.Recruitment.CurrentOffer = CreateOfferWithReservation();
        return true;
    }

    public bool HireOffer(int cost = DefaultRecruitCost)
    {
        if (cost < 0 || !CanGenerateOffer() || _state.Recruitment.CurrentOffer is null)
        {
            return false;
        }

        var recruit = _state.Recruitment.CurrentOffer;
        if (!_economy.Spend(cost))
        {
            return false;
        }

        _state.Roster.Characters.Add(recruit);
        _state.Schedule.AssignedJobs[recruit.Id] = "rest";
        if (_state.Adventure.SelectedPartyIds.Count < Math.Min(3, _state.Roster.Characters.Count)
            && !_state.Adventure.SelectedPartyIds.Contains(recruit.Id))
        {
            _state.Adventure.SelectedPartyIds.Add(recruit.Id);
        }

        _state.Recruitment.CurrentOffer = CreateOfferWithReservation();

        return true;
    }

    public bool RecruitRandom(int cost = DefaultRecruitCost)
    {
        return HireOffer(cost);
    }

    private bool CanGenerateOffer()
    {
        return _data.CharacterList().Count > 0;
    }

    private CharacterState CreateOfferWithReservation()
    {
        var reservedIds = _state.Recruitment.CurrentOffer is { Id: var id } && !string.IsNullOrWhiteSpace(id)
            ? new[] { id }
            : Array.Empty<string>();
        return _factory.CreateGeneratedRecruit(_state, reservedIds);
    }
}

public sealed class ResearchService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private readonly MilestoneService _milestones;

    public ResearchService(SaveState state, DataRegistry data, MilestoneService milestones)
    {
        _state = state;
        _data = data;
        _milestones = milestones;
    }

    public bool Unlock(string skillId)
    {
        if (!_data.Skills.TryGetValue(skillId, out var skill) || _state.Research.UnlockedSkillIds.Contains(skillId))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(skill.CostResourceId))
        {
            _state.Ranch.Stockpile.TryGetValue(skill.CostResourceId, out var available);
            if (available < skill.CostAmount)
            {
                return false;
            }

            _state.Ranch.Stockpile[skill.CostResourceId] = available - skill.CostAmount;
        }

        _state.Research.UnlockedSkillIds.Add(skillId);
        _milestones.CheckResearchMilestones(skillId);
        return true;
    }
}

public sealed class TrainingService
{
    private readonly SaveState _state;

    public TrainingService(SaveState state)
    {
        _state = state;
    }

    public bool Train(string characterId, string focus)
    {
        var character = _state.Roster.Characters.FirstOrDefault(value => value.Id == characterId);
        if (character is null || character.Energy < 10)
        {
            return false;
        }

        character.Energy = Math.Max(0, character.Energy - 10);
        character.Fatigue = Math.Clamp(character.Fatigue + 12, 0, 100);
        character.Morale = Math.Clamp(character.Morale + 1, 0, 100);

        switch (focus)
        {
            case "ranch":
                character.RanchSkill = Math.Clamp(character.RanchSkill + 1, 1, 10);
                break;
            case "craft":
                character.CraftSkill = Math.Clamp(character.CraftSkill + 1, 1, 10);
                break;
            case "combat":
                character.CombatSkill = Math.Clamp(character.CombatSkill + 1, 1, 10);
                break;
            default:
                return false;
        }

        return true;
    }
}

public sealed class PetService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private readonly EconomyService _economy;

    public PetService(SaveState state, DataRegistry data, EconomyService economy)
    {
        _state = state;
        _data = data;
        _economy = economy;
    }

    public bool Adopt(string petId)
    {
        if (!_data.Pets.TryGetValue(petId, out var pet) || _state.Pets.AdoptedPetIds.Contains(petId))
        {
            return false;
        }

        if (!_economy.Spend(pet.CareCost * 4))
        {
            return false;
        }

        _state.Pets.AdoptedPetIds.Add(petId);
        _state.Pets.Entries[petId] = new PetEntryState();
        return true;
    }

    public string Feed(string petId)
    {
        if (!_state.Pets.Entries.TryGetValue(petId, out var entry))
            return "Not adopted";

        if (!_economy.Spend(10))
            return "Not enough gold";

        entry.Hunger = Math.Clamp(entry.Hunger + 20, 0, 100);
        entry.Mood = Math.Clamp(entry.Mood + 5, 0, 100);
        entry.Bond = Math.Clamp(entry.Bond + 2, 0, 100);
        entry.TimesFed++;
        return $"Fed successfully. Hunger +20, Mood +5, Bond +2";
    }

    public string Play(string petId)
    {
        if (!_state.Pets.Entries.TryGetValue(petId, out var entry))
            return "Not adopted";

        if (!_economy.Spend(5))
            return "Not enough gold";

        entry.Mood = Math.Clamp(entry.Mood + 15, 0, 100);
        entry.Bond = Math.Clamp(entry.Bond + 3, 0, 100);
        entry.Hunger = Math.Clamp(entry.Hunger - 5, 0, 100);
        entry.TimesPlayed++;
        return $"Played successfully. Mood +15, Bond +3, Hunger -5";
    }

    public string Train(string petId)
    {
        if (!_state.Pets.Entries.TryGetValue(petId, out var entry))
            return "Not adopted";

        if (!_economy.Spend(15))
            return "Not enough gold";

        entry.Training = Math.Clamp(entry.Training + 10, 0, 100);
        entry.Bond = Math.Clamp(entry.Bond + 1, 0, 100);
        entry.Hunger = Math.Clamp(entry.Hunger - 10, 0, 100);
        entry.Mood = Math.Clamp(entry.Mood - 5, 0, 100);
        entry.TimesTrained++;
        return $"Trained successfully. Training +10, Bond +1, Hunger -10, Mood -5";
    }

    public string Status(string petId)
    {
        if (!_state.Pets.Entries.TryGetValue(petId, out var entry))
            return "Not adopted";

        var moodDesc = entry.Mood switch { >= 80 => "joyful", >= 50 => "content", >= 25 => "restless", _ => "unhappy" };
        var hungerDesc = entry.Hunger switch { >= 80 => "sated", >= 50 => "peckish", >= 25 => "hungry", _ => "starving" };
        var bondDesc = entry.Bond switch { >= 80 => "devoted", >= 50 => "friendly", >= 25 => "wary", _ => "distrustful" };
        return $"{moodDesc}, {hungerDesc}, {bondDesc} (Bond {entry.Bond}, Training {entry.Training})";
    }
}
