using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

public sealed class CombatService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private readonly EquipmentService _equipment;
    private readonly TalentService _talents;
    private readonly MagicService _magic;

    public const int SkillSpCost = 10;
    public const int PlayerMagicHealCost = 10;

    public CombatService(SaveState state, DataRegistry data, EquipmentService equipment, TalentService talents, MagicService? magic = null)
    {
        _state = state;
        _data = data;
        _equipment = equipment;
        _talents = talents;
        _magic = magic ?? new MagicService(state, data);
    }

    public CombatReport ResolveMissionRounds(string missionId, bool autoResolve)
    {
        if (!_data.Missions.TryGetValue(missionId, out var mission))
            return FailReport(missionId, "Mission not found.");

        var report = new CombatReport { MissionId = missionId, IsRoundBased = true };

        var enemies = PickEnemies(mission);
        if (enemies.Count == 0)
            return FailReport(missionId, "No enemies for this mission.");

        var partyChars = _state.Roster.Characters
            .Where(c => _state.Adventure.SelectedPartyIds.Contains(c.Id) || _state.Adventure.SelectedPartyIds.Count == 0)
            .ToList();
        if (partyChars.Count == 0)
            return FailReport(missionId, "No party members available.");

        var party = InitParty(partyChars);
        var enemySide = InitEnemies(enemies);

        report.PartyState = party.Select(CombatantSnapshot).ToList();
        report.EnemyState = enemySide.Select(CombatantSnapshot).ToList();

        int maxRounds = 10;
        int roundNum = 0;
        bool partyWon = false;
        bool partyWiped = false;

        while (roundNum < maxRounds)
        {
            roundNum++;
            var activeParty = party.Where(c => c.Hp > 0).ToList();
            var activeEnemies = enemySide.Where(e => e.Hp > 0).ToList();
            if (activeParty.Count == 0) { partyWiped = true; break; }
            if (activeEnemies.Count == 0) { partyWon = true; break; }

            var allCombatants = new List<BattleCombatant>();
            allCombatants.AddRange(activeParty);
            allCombatants.AddRange(activeEnemies);

            var round = new BattleRound { RoundNumber = roundNum };

            foreach (var actor in allCombatants.OrderByDescending(c => c.Speed + _state.Calendar.Day % 3))
            {
                if (actor.Hp <= 0 || activeEnemies.All(e => e.Hp <= 0) || activeParty.All(p => p.Hp <= 0))
                    break;

                var action = autoResolve
                    ? AutoChooseAction(actor, activeParty, activeEnemies)
                    : ManualChooseAction(actor, activeParty, activeEnemies);

                if (action.ActionType == "Defend")
                {
                    round.Actions.Add(new BattleAction
                    {
                        ActorName = action.ActorName, ActionType = "Defend",
                        Description = $"{action.ActorName} braces for impact. Defense doubled this round."
                    });
                    actor.Defending = true;
                    continue;
                }

                var target = action.TargetEnemy
                    ? activeEnemies.FirstOrDefault(e => e.Id == action.TargetId) ?? activeEnemies.FirstOrDefault()
                    : activeParty.FirstOrDefault(p => p.Id == action.TargetId) ?? activeParty.FirstOrDefault();
                if (target == null || target.Hp <= 0)
                    continue;

                if (action.ActionType == "Skill")
                {
                    if (actor.Sp < SkillSpCost)
                        continue;

                    actor.Sp -= SkillSpCost;
                    var heal = Math.Min(15 + Math.Max(0, actor.MagicPower), target.MaxHp - target.Hp);
                    target.Hp += heal;
                    round.Actions.Add(new BattleAction
                    {
                        ActorName = action.ActorName, ActionType = "Skill", TargetName = target.DisplayName,
                        Healing = heal, ResourceCost = SkillSpCost, ResourceName = "SP",
                        Description = $"{action.ActorName} uses a combat skill and restores {heal} HP to {target.DisplayName} (-{SkillSpCost} SP)."
                    });
                    continue;
                }

                if (action.ActionType == "Magic")
                {
                    if (!actor.UsesPlayerMana || !_magic.SpendPlayerMana(PlayerMagicHealCost))
                        continue;

                    report.PlayerManaSpent += PlayerMagicHealCost;
                    var heal = Math.Min(24 + Math.Max(0, actor.MagicPower * 2), target.MaxHp - target.Hp);
                    target.Hp += heal;
                    round.Actions.Add(new BattleAction
                    {
                        ActorName = action.ActorName, ActionType = "Magic", TargetName = target.DisplayName,
                        Healing = heal, ResourceCost = PlayerMagicHealCost, ResourceName = "MP",
                        Description = $"{action.ActorName} channels mana and restores {heal} HP to {target.DisplayName} (-{PlayerMagicHealCost} MP)."
                    });
                    continue;
                }

                var rawDmg = Math.Max(1, actor.Attack + _state.Calendar.Day % 5 - target.Defense / 2);
                if (target.Defending) rawDmg = Math.Max(1, rawDmg / 2);
                target.Hp = Math.Max(0, target.Hp - rawDmg);
                bool killed = target.Hp <= 0;

                round.Actions.Add(new BattleAction
                {
                    ActorName = action.ActorName, ActionType = action.ActionType,
                    TargetName = target.DisplayName, Damage = rawDmg,
                    Description = $"{action.ActorName} attacks {target.DisplayName} for {rawDmg} damage!",
                    KilledTarget = killed
                });

                target.Defending = false;
            }

            report.Rounds.Add(round);
            report.TurnLog.Add($"Round {roundNum}: {round.Actions.Count} actions resolved.");
        }

        int totalPartyHp = party.Sum(p => p.MaxHp);
        int remainingPartyHp = party.Where(p => p.Hp > 0).Sum(p => p.Hp);
        float survivalRatio = totalPartyHp > 0 ? (float)remainingPartyHp / totalPartyHp : 0;

        if (partyWiped)
        {
            report.Outcome = MissionOutcome.Failure;
            report.Summary = $"Defeated after {roundNum} rounds. The party was overwhelmed.";
            ApplyFatigueAndMorale(partyChars, false);
        }
        else if (partyWon)
        {
            int diff = mission.Difficulty;
            if (survivalRatio >= 0.7f)
            {
                report.Outcome = MissionOutcome.Success;
                report.RewardGold = mission.RewardGold;
                report.RewardItemId = mission.RewardItemId;
                report.Summary = $"Victory in {roundNum} rounds! Party at {(int)(survivalRatio * 100)}% strength. Full rewards earned.";
            }
            else if (survivalRatio >= 0.3f)
            {
                report.Outcome = MissionOutcome.PartialSuccess;
                report.RewardGold = mission.RewardGold / 2;
                report.Summary = $"Hard-won victory in {roundNum} rounds. Party at {(int)(survivalRatio * 100)}% strength. Partial rewards.";
            }
            else
            {
                report.Outcome = MissionOutcome.PartialSuccess;
                report.RewardGold = mission.RewardGold / 3;
                report.Summary = $"Barely survived {roundNum} rounds. Party at {(int)(survivalRatio * 100)}% strength. Minimal rewards.";
            }
            ApplyFatigueAndMorale(partyChars, true);
        }
        else
        {
            report.Outcome = MissionOutcome.PartialSuccess;
            report.RewardGold = mission.RewardGold / 2;
            report.Summary = $"Stalemate after {maxRounds} rounds. Party at {(int)(survivalRatio * 100)}% strength.";
            ApplyFatigueAndMorale(partyChars, true);
        }

        ApplyPersistentCombatWear(partyChars, party, report);

        if (report.Outcome != MissionOutcome.Failure && report.RewardGold > 0)
        {
            _state.Economy.Gold += report.RewardGold;
            _state.Economy.LastIncome += report.RewardGold;
            if (!string.IsNullOrEmpty(report.RewardItemId))
                _state.Inventory.Items[report.RewardItemId] = _state.Inventory.Items.GetValueOrDefault(report.RewardItemId) + 1;
        }

        report.PartyState = party.Select(CombatantSnapshot).ToList();

        _state.Adventure.LastMissionId = missionId;
        _state.Adventure.LastOutcome = report.Outcome;
        _state.Adventure.LastSummary = report.Summary;

        return report;
    }

    public CombatReport AttemptCapture(string missionId)
    {
        // Original-game rule: capture requires a hired mercenary and a mana shackle.
        if (_state.Adventure.ActiveMercenaryHpBonus <= 0)
        {
            return new CombatReport
            {
                MissionId = missionId,
                Outcome = MissionOutcome.None,
                CaptureAttempted = true,
                CaptureSucceeded = false,
                Summary = "Capture blocked: a hired mercenary is required for capture. Hire one from the Adventure Guild."
            };
        }

        if (!_state.Inventory.Items.TryGetValue("mana_shackle", out var shackles) || shackles <= 0)
        {
            return new CombatReport
            {
                MissionId = missionId,
                Outcome = MissionOutcome.None,
                CaptureAttempted = true,
                CaptureSucceeded = false,
                Summary = "Capture blocked: no mana shackles available. Buy them at the shop or earn from missions."
            };
        }

        var report = ResolveMissionRounds(missionId, true);
        if (report.Outcome == MissionOutcome.Failure)
        {
            report.CaptureAttempted = true;
            report.CaptureSucceeded = false;
            report.Summary += " Capture impossible — mission failed.";
            return report;
        }

        report.CaptureAttempted = true;

        var partyChars = _state.Roster.Characters
            .Where(c => _state.Adventure.SelectedPartyIds.Contains(c.Id) || _state.Adventure.SelectedPartyIds.Count == 0)
            .ToList();
        var control = partyChars.Sum(c => c.CombatSkill * 3 + c.CraftSkill + c.Morale / 10);
        var roll = (Math.Abs(_state.Calendar.Day * 17 + missionId.Sum(ch => ch) + 4) % 20) + 1;
        var score = control + roll;

        var mission = _data.Missions[missionId];
        var enemies = PickEnemies(mission);
        var threshold = enemies.Sum(e => e.CaptureDifficulty) + 15;

        if (score >= threshold)
        {
            report.CaptureSucceeded = true;
            var factory = new SaveStateFactory(_data);
            var recruit = factory.CreateGeneratedRecruitWithPreferences(_state, _state.Adventure.CapturePrefs);
            recruit.TraitOverride = $"Captured{(string.IsNullOrEmpty(recruit.TraitOverride) ? "" : ", " + recruit.TraitOverride)}";
            recruit.IsCaptured = true;
            _state.Roster.Characters.Add(recruit);
            _state.Schedule.AssignedJobs[recruit.Id] = "rest";
            report.CapturedCharacterId = recruit.Id;
            report.Summary += $" Capture successful! {recruit.DisplayNameOverride} recruited.";
            _state.Adventure.LastCaptureSummary = $"Captured {recruit.DisplayNameOverride}";

            // The mana shackle and hired mercenary are spent on the capture (original-game rule).
            if (_state.Inventory.Items.TryGetValue("mana_shackle", out var shackleCount) && shackleCount > 0)
                _state.Inventory.Items["mana_shackle"] = shackleCount - 1;
            _state.Adventure.ActiveMercenaryHpBonus = 0;
        }
        else
        {
            report.CaptureSucceeded = false;
            report.Summary += " Capture attempt failed. Target resisted (roll " + roll + " vs threshold " + threshold + ").";
            _state.Adventure.LastCaptureSummary = "Capture failed.";
        }

        return report;
    }

    public List<EnemyDefinition> PickEnemies(MissionDefinition mission)
    {
        if (string.IsNullOrEmpty(mission.EnemyGroupId))
            return new List<EnemyDefinition>();
        var pool = _data.Enemies.Values.Where(e => e.GroupId == mission.EnemyGroupId).ToList();
        if (pool.Count == 0) return new List<EnemyDefinition>();
        int idx = Math.Abs(_state.Calendar.Day * 7 + mission.Id.Sum(c => c)) % pool.Count;
        return new List<EnemyDefinition> { pool[idx] };
    }

    private List<BattleCombatant> InitParty(List<CharacterState> chars)
    {
        int trainingBonus = _state.Research.UnlockedSkillIds.Contains("adventure_training") ? 4 : 0;
        int tacticalBonus = _state.Research.UnlockedSkillIds.Contains("tactical_training") ? 3 : 0;
        return chars.Select((c, index) => new BattleCombatant
        {
            Id = c.Id, DisplayName = string.IsNullOrWhiteSpace(c.DisplayNameOverride) ? c.Id : c.DisplayNameOverride,
            Hp = Math.Max(50, c.Hp / 20), MaxHp = Math.Max(50, ((c.MaxHpOverride ?? 100) + _equipment.BonusMaxHp(c.Id) + _talents.BonusMaxHp(c.Id)) / 20),
            Sp = Math.Max(20, c.Energy / 10), MaxSp = Math.Max(20, ((c.MaxEnergyOverride ?? 100) + _equipment.BonusMaxEnergy(c.Id) + _talents.BonusMaxEnergy(c.Id)) / 10),
            Attack = (c.CombatSkill + _equipment.BonusCombatSkill(c.Id) + _talents.BonusCombatSkill(c.Id)) * 3 + c.Morale / 20 + trainingBonus + tacticalBonus,
            Defense = (c.CombatSkill + _equipment.BonusCombatSkill(c.Id) + _talents.BonusCombatSkill(c.Id)) * 2 + (c.CraftSkill + _equipment.BonusCraftSkill(c.Id) + _talents.BonusCraftSkill(c.Id)) / 3 + trainingBonus / 2 + tacticalBonus / 2,
            Speed = 5 + (c.CombatSkill + _equipment.BonusCombatSkill(c.Id) + _talents.BonusCombatSkill(c.Id)) / 2 - c.Fatigue / 25 + trainingBonus / 2 + tacticalBonus / 3,
            MagicPower = Math.Max(0, c.MagicPower),
            UsesPlayerMana = c.Provenance == CharacterProvenance.OriginalPlayer
                || string.Equals(c.Id, "anon", StringComparison.OrdinalIgnoreCase),
            IsEnemy = false
        }).ToList();
    }

    private List<BattleCombatant> InitEnemies(List<EnemyDefinition> defs)
    {
        int dayScale = 1 + _state.Calendar.Day / 10;
        return defs.Select(e => new BattleCombatant
        {
            Id = e.Id, DisplayName = e.DisplayName,
            Hp = e.BaseHp * dayScale, MaxHp = e.BaseHp * dayScale,
            Sp = e.BaseSp, MaxSp = e.BaseSp,
            Attack = e.Attack + dayScale - 1,
            Defense = e.Defense + dayScale / 2,
            Speed = e.Speed,
            MagicPower = 0,
            UsesPlayerMana = false,
            IsEnemy = true
        }).ToList();
    }

    private BattleActionRecord AutoChooseAction(BattleCombatant actor, List<BattleCombatant> party, List<BattleCombatant> enemies)
    {
        bool isEnemy = actor.IsEnemy;
        var allies = isEnemy ? enemies : party;
        var foes = isEnemy ? party : enemies;
        var healthyFoes = foes.Where(f => f.Hp > 0).ToList();
        if (healthyFoes.Count == 0) return new BattleActionRecord { ActorName = actor.DisplayName, ActionType = "Wait" };

        int hpPct = actor.Hp * 100 / Math.Max(1, actor.MaxHp);
        var hurtAlly = allies
            .Where(a => a.Hp > 0 && a.Hp * 100 / Math.Max(1, a.MaxHp) < 70)
            .OrderBy(a => (float)a.Hp / Math.Max(1, a.MaxHp))
            .FirstOrDefault();

        if (!isEnemy && actor.UsesPlayerMana && hurtAlly is not null && _magic.CanSpendPlayerMana(PlayerMagicHealCost))
            return new BattleActionRecord { ActorName = actor.DisplayName, ActionType = "Magic", TargetEnemy = false, TargetId = hurtAlly.Id };

        if (hurtAlly is not null && actor.Sp >= SkillSpCost)
            return new BattleActionRecord { ActorName = actor.DisplayName, ActionType = "Skill", TargetEnemy = isEnemy, TargetId = hurtAlly.Id };

        if (hpPct < 20)
            return new BattleActionRecord { ActorName = actor.DisplayName, ActionType = "Defend" };

        var target = healthyFoes.OrderBy(f => f.Hp).First();
        return new BattleActionRecord
        {
            ActorName = actor.DisplayName, ActionType = "Attack",
            TargetEnemy = !isEnemy, TargetId = target.Id
        };
    }

    private BattleActionRecord ManualChooseAction(BattleCombatant actor, List<BattleCombatant> party, List<BattleCombatant> enemies)
    {
        // There is not yet an interactive turn-command UI. Keep this deterministic fallback for
        // scripted/tutorial callers, but do not present it to players as a manual battle mode.
        return AutoChooseAction(actor, party, enemies);
    }

    private static CombatReport FailReport(string missionId, string reason)
    {
        return new CombatReport
        {
            MissionId = missionId, Outcome = MissionOutcome.Failure,
            Summary = reason, IsRoundBased = true,
            TurnLog = new List<string> { reason }
        };
    }

    private static void ApplyFatigueAndMorale(List<CharacterState> chars, bool victory)
    {
        foreach (var c in chars)
        {
            c.Fatigue = Math.Clamp(c.Fatigue + (victory ? 8 : 15), 0, 100);
            c.Morale = Math.Clamp(c.Morale + (victory ? 3 : -5), 0, 100);
        }
    }

    private void ApplyPersistentCombatWear(List<CharacterState> characters, List<BattleCombatant> combatants, CombatReport report)
    {
        foreach (var character in characters)
        {
            var battle = combatants.FirstOrDefault(value => value.Id == character.Id);
            if (battle is null)
                continue;

            var maxPersistentHp = character.MaxHpOverride
                ?? (_data.Characters.TryGetValue(character.DefinitionId, out var definition) ? definition.MaxHp : Math.Max(character.Hp, 1));
            maxPersistentHp = Math.Max(1, maxPersistentHp);

            if (battle.Hp <= 0)
            {
                character.Hp = 0;
                report.TurnLog.Add($"{battle.DisplayName} was defeated and is left at 0 persistent HP.");
                continue;
            }

            var ratio = (float)battle.Hp / Math.Max(1, battle.MaxHp);
            var wearPercent = ratio switch
            {
                < 0.20f => 80,
                < 0.50f => 50,
                < 0.80f => 30,
                < 1.00f => 15,
                _ => 0
            };

            if (wearPercent <= 0)
                continue;

            var hpCost = Math.Max(1, maxPersistentHp * wearPercent / 100);
            character.Hp = Math.Max(0, character.Hp - hpCost);
            report.TurnLog.Add($"{battle.DisplayName} carries battle wear into the day: -{hpCost} HP ({wearPercent}% tier).");
        }
    }

    private CombatantSnapshot CombatantSnapshot(BattleCombatant b) => new()
    {
        Id = b.Id, DisplayName = b.DisplayName,
        CurrentHp = b.Hp, MaxHp = b.MaxHp,
        CurrentSp = b.Sp, MaxSp = b.MaxSp,
        CurrentMana = b.UsesPlayerMana ? _magic.CurrentMana : 0,
        MaxMana = b.UsesPlayerMana ? _magic.MaxMana : 0,
        IsAlive = b.Hp > 0, IsEnemy = b.IsEnemy,
        Attack = b.Attack, Defense = b.Defense, Speed = b.Speed
    };
}

internal sealed class BattleCombatant
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Sp { get; set; }
    public int MaxSp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
    public int MagicPower { get; set; }
    public bool UsesPlayerMana { get; set; }
    public bool IsEnemy { get; set; }
    public bool Defending { get; set; }
}

internal sealed class BattleActionRecord
{
    public string ActorName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public bool TargetEnemy { get; set; }
    public string TargetId { get; set; } = string.Empty;
}

public sealed class DiscoveryService
{
    private readonly SaveState _state;
    private readonly DataRegistry _data;

    public DiscoveryService(SaveState state, DataRegistry data)
    {
        _state = state;
        _data = data;
    }

    public int DiscoveredCount => _state.Adventure.DiscoveredMissionIds.Count;
    public bool AllDiscovered => _state.Adventure.DiscoveredMissionIds.Count >= _data.Missions.Count;

    public List<MissionDefinition> AvailableMissions()
    {
        return _data.Missions.Values
            .Where(m => _state.Adventure.DiscoveredMissionIds.Contains(m.Id))
            .OrderBy(m => m.Tier)
            .ThenBy(m => m.Difficulty)
            .ToList();
    }

    public void DiscoverNext()
    {
        var undiscovered = _data.Missions.Values
            .Where(m => !_state.Adventure.DiscoveredMissionIds.Contains(m.Id))
            .OrderBy(m => m.Difficulty)
            .ToList();

        if (undiscovered.Count == 0) return;

        int idx;
        if (_state.Calendar.Day <= 2)
            idx = 0;
        else
            idx = Math.Abs(_state.Calendar.Day * 3) % undiscovered.Count;

        _state.Adventure.DiscoveredMissionIds.Add(undiscovered[idx].Id);
    }

    public bool IsDiscovered(string missionId) =>
        _state.Adventure.DiscoveredMissionIds.Contains(missionId);
}

public sealed class MercenaryService
{
    private readonly SaveState _state;
    private readonly EconomyService _economy;

    public MercenaryService(SaveState state, EconomyService economy)
    {
        _state = state;
        _economy = economy;
    }

    public List<MercenaryOffer> AvailableMercenaries()
    {
        return _state.Adventure.AvailableMercenaries;
    }

    public void RefreshMercenaries()
    {
        _state.Adventure.AvailableMercenaries.Clear();
        int count = 2 + _state.Calendar.Day / 5;
        var names = new[] { "Ronin", "Huntress", "Knight", "Archer", "Mage", "Berserker" };
        for (int i = 0; i < Math.Min(count, 6); i++)
        {
            int skill = Math.Clamp(2 + (_state.Calendar.Day / 5) + (i % 3), 1, 10);
            int cost = 30 + skill * 8;
            _state.Adventure.AvailableMercenaries.Add(new MercenaryOffer
            {
                Id = $"merc_{i}_{_state.Calendar.Day}",
                DisplayName = names[i % names.Length],
                CombatSkill = skill,
                HpBonus = skill * 8,
                Cost = cost
            });
        }
    }

    public bool Hire(string mercId, out MercenaryOffer? hired)
    {
        hired = null;
        var merc = _state.Adventure.AvailableMercenaries.FirstOrDefault(m => m.Id == mercId);
        if (merc == null) return false;
        if (!_economy.Spend(merc.Cost)) return false;
        _state.Adventure.AvailableMercenaries.Remove(merc);
        hired = merc;
        return true;
    }
}
