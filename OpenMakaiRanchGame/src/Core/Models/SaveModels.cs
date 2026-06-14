using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenMakaiRanch.Core.Models;

public enum DayPhase
{
    Morning,
    Afternoon,
    Evening,
    Night
}

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}

public enum Weather
{
    Clear,
    Cloudy,
    Rain,
    Storm
}

public enum MissionOutcome
{
    None,
    Failure,
    PartialSuccess,
    Success
}

public enum CombatPhase
{
    PreBattle,
    BattleResults,
    PostBattle
}

public enum FallState
{
    Normal,
    Love,
    Devotion,
    Collapse,
    MilkCow,
    Slave
}

public enum TrainingCategory
{
    Hand,
    Mouth,
    VInsertion,
    AInsertion,
    PenisAction,
    Tool,
    Pain,
    Tentacle,
    Massage,
    Item,
    BodyMod,
    ForbiddenMagic,
    Interrogation,
    Service
}

public sealed class SaveState
{
    public const int CurrentSchemaVersion = 14;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public DateTime? SavedAt { get; set; }
    public CalendarState Calendar { get; set; } = new();
    public EconomyState Economy { get; set; } = new();
    public RanchState Ranch { get; set; } = new();
    public RosterState Roster { get; set; } = new();
    public ScheduleState Schedule { get; set; } = new();
    public InventoryState Inventory { get; set; } = new();
    public AdventureState Adventure { get; set; } = new();
    public MilestoneState Milestones { get; set; } = new();
    public ResearchState Research { get; set; } = new();
    public PetState Pets { get; set; } = new();
    public BondState Bond { get; set; } = new();
    public RecruitmentState Recruitment { get; set; } = new();
    public SettingsState Settings { get; set; } = new();
    public MatureState Mature { get; set; } = new();
    public PlayerState Player { get; set; } = new();
    public bool NgPlusActive { get; set; }
    public int? VictoryDay { get; set; }
}

public sealed class PlayerState
{
    public string Name { get; set; } = "Anon";
    public string Race { get; set; } = "Demonfolk";
    public string RanchName { get; set; } = "Okachi Ranch";
    public string Gender { get; set; } = "Male";
    public int Height { get; set; } = 1900;
    public int ApparentAge { get; set; } = 20;
    public string BodyShape { get; set; } = "Standard";
    public string SkinColor { get; set; } = "Standard";
    public string HairColor { get; set; } = "Black";
    public string HairStyle { get; set; } = "Short";
    public string HairFeature { get; set; } = "None";
    public string EyeColor { get; set; } = "Red";
    public string EyeShape { get; set; } = "Standard";
    public string Job { get; set; } = "Dairy Farmer";
    public string Personality { get; set; } = "Quiet";
    public bool HasHorns { get; set; } = true;
    public bool HasGlasses { get; set; } = false;
    public string BustSize { get; set; } = "Flat";
    public string BreastFirmness { get; set; } = "Firm";
    public string FirstPersonPronoun { get; set; } = "I";
    public string StartingPetId { get; set; } = "stable_cat";
    public string StartingMountId { get; set; } = "none";
    public string TailType { get; set; } = "None";
    public string BodyFur { get; set; } = "None";
}

public sealed class CalendarState
{
    public int Day { get; set; } = 1;
    public DayPhase Phase { get; set; } = DayPhase.Morning;
    public Weather CurrentWeather { get; set; } = Weather.Clear;

    [JsonIgnore]
    public Season Season => (Season)(((Day - 1) / 30) % 4);
}

public sealed class EconomyState
{
    public int Gold { get; set; } = 500;
    public int LastIncome { get; set; }
    public int LastExpenses { get; set; }
    public int SpiritEnergy { get; set; }
    public int ManaReservoir { get; set; }
}

public sealed class RanchState
{
    public Dictionary<string, int> Stockpile { get; set; } = new();
    public Dictionary<string, int> Facilities { get; set; } = new();
    public int CattleHealth { get; set; } = 80;
    public int Workload { get; set; }
    public bool BathtubClean { get; set; } = true;
}

public sealed class RosterState
{
    public List<CharacterState> Characters { get; set; } = new();
}

public sealed class CharacterState
{
    public string Id { get; set; } = string.Empty;
    public string DefinitionId { get; set; } = string.Empty;
    public bool IsGenerated { get; set; }
    public bool IsStartingRecruit { get; set; }
    public string DisplayNameOverride { get; set; } = string.Empty;
    public string PortraitPathOverride { get; set; } = string.Empty;
    public string BodyImagePathOverride { get; set; } = string.Empty;
    public string BodyTypeOverride { get; set; } = string.Empty;
    public int BodyLayerIndex { get; set; }
    public int SkinColorIndex { get; set; }
    public int BreastSizeIndex { get; set; }
    public int FaceLayerIndex { get; set; }
    public int HairLayerIndex { get; set; }
    public int RaceLayerIndex { get; set; }
    public int ClothLayerIndex { get; set; }
    public string TraitOverride { get; set; } = string.Empty;
    public int? MaxHpOverride { get; set; }
    public int? MaxEnergyOverride { get; set; }
    public int Hp { get; set; }
    public int Energy { get; set; }
    public int Fatigue { get; set; }
    public int Morale { get; set; } = 50;
    public int Bond { get; set; }
    public int RanchSkill { get; set; }
    public int CraftSkill { get; set; }
    public int CombatSkill { get; set; }
    public int MagicPower { get; set; }
    public Dictionary<string, int> SkillXp { get; set; } = new();
    public bool HasGrownToday { get; set; }
    public Dictionary<string, string> EquippedItems { get; set; } = new();

    // NSFW fields
    public MentalState Mature { get; set; } = new();
    public MilkState Milk { get; set; } = new();
    public AddictionState Addictions { get; set; } = new();
    public EquipmentState Equipment { get; set; } = new();
    public List<string> Talents { get; set; } = new();
    public string Race { get; set; } = string.Empty;
    public string Personality { get; set; } = string.Empty;
    public string JobClass { get; set; } = string.Empty;
    public string HairColor { get; set; } = string.Empty;
    public string HairStyle { get; set; } = string.Empty;
    public string EyeColor { get; set; } = string.Empty;
    public string SkinColor { get; set; } = string.Empty;
    public int Height { get; set; } = 1600;
    public int ApparentAge { get; set; } = 18;
    public int BustSize { get; set; } = 3;
}

public sealed class ScheduleState
{
    public Dictionary<string, string> AssignedJobs { get; set; } = new();
}

public sealed class InventoryState
{
    public Dictionary<string, int> Items { get; set; } = new();
}

public sealed class AdventureState
{
    public string LastMissionId { get; set; } = string.Empty;
    public MissionOutcome LastOutcome { get; set; } = MissionOutcome.None;
    public string LastSummary { get; set; } = "No adventure has been attempted yet.";
    public string LastCaptureSummary { get; set; } = string.Empty;
    public List<string> SelectedPartyIds { get; set; } = new();
    public List<string> DiscoveredMissionIds { get; set; } = new();
    public List<MercenaryOffer> AvailableMercenaries { get; set; } = new();
    public int ActiveMercenaryHpBonus { get; set; }
}

public sealed class MercenaryOffer
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int CombatSkill { get; set; }
    public int HpBonus { get; set; }
    public int Cost { get; set; }
}

public sealed class MilestoneState
{
    public List<string> CompletedIds { get; set; } = new();
}

public sealed class ResearchState
{
    public List<string> UnlockedSkillIds { get; set; } = new();
}

public sealed class PetState
{
    public List<string> AdoptedPetIds { get; set; } = new();
    public Dictionary<string, PetEntryState> Entries { get; set; } = new();
}

public sealed class PetEntryState
{
    public int Hunger { get; set; } = 80;
    public int Mood { get; set; } = 60;
    public int Training { get; set; }
    public int Bond { get; set; } = 20;
    public int TimesFed { get; set; }
    public int TimesPlayed { get; set; }
    public int TimesTrained { get; set; }
}

public sealed class BondState
{
    public List<string> CompletedEventIds { get; set; } = new();
}

public sealed class RecruitmentState
{
    public CharacterState? CurrentOffer { get; set; }
}

public sealed class SettingsState
{
    public bool AudioEnabled { get; set; } = true;
    public bool HapticsEnabled { get; set; } = true;
    public string ThemeId { get; set; } = "midnight";
    public float UiScale { get; set; } = 1.0f;
    public string Locale { get; set; } = "en";
    public bool ReducedMotion { get; set; }

    public SettingsState Clone()
    {
        return new SettingsState
        {
            AudioEnabled = AudioEnabled,
            HapticsEnabled = HapticsEnabled,
            ThemeId = ThemeId,
            UiScale = UiScale,
            Locale = Locale,
            ReducedMotion = ReducedMotion
        };
    }
}

public sealed class DailyReport
{
    public int Day { get; set; }
    public int Income { get; set; }
    public int Expenses { get; set; }
    public int NetGold { get; set; }
    public int MilkRevenue { get; set; }
    public int SkillGains { get; set; }
    public List<string> Lines { get; set; } = new();
    public List<DailyEvent> Events { get; set; } = new();
    public List<CharacterGrowthEntry> CharacterGrowth { get; set; } = new();
}

public sealed class DailyEvent
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPositive { get; set; }
    public int GoldDelta { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public int ItemAmount { get; set; }
}

public sealed class CharacterGrowthEntry
{
    public string CharacterId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string SkillGained { get; set; } = string.Empty;
    public int Amount { get; set; }
}

public sealed class CombatReport
{
    public string MissionId { get; set; } = string.Empty;
    public MissionOutcome Outcome { get; set; }
    public int RewardGold { get; set; }
    public string RewardItemId { get; set; } = string.Empty;
    public bool CaptureAttempted { get; set; }
    public bool CaptureSucceeded { get; set; }
    public string CapturedCharacterId { get; set; } = string.Empty;
    public List<string> TurnLog { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public bool IsRoundBased { get; set; }
    public List<BattleRound> Rounds { get; set; } = new();
    public List<CombatantSnapshot> PartyState { get; set; } = new();
    public List<CombatantSnapshot> EnemyState { get; set; } = new();
}

public sealed class BattleRound
{
    public int RoundNumber { get; set; }
    public List<BattleAction> Actions { get; set; } = new();
}

public sealed class BattleAction
{
    public string ActorName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string TargetName { get; set; } = string.Empty;
    public int Damage { get; set; }
    public int Healing { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool KilledTarget { get; set; }
}

public sealed class CombatantSnapshot
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int CurrentSp { get; set; }
    public int MaxSp { get; set; }
    public bool IsAlive { get; set; }
    public bool IsEnemy { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
}

// === NSFW State Models ===

public sealed class MatureState
{
    public List<TrainingRecord> TrainingHistory { get; set; } = new();
    public int TotalMilkProduced { get; set; }
    public int TotalMilkRevenue { get; set; }
    public int TotalTrainingSessions { get; set; }
}

public sealed class MentalState
{
    // Mental parameters (0-10000 = 100%)
    public int Resistance { get; set; } = 10000;
    public int Dignity { get; set; } = 10000;
    public int Aversion { get; set; } = 10000;
    public int Reason { get; set; } = 10000;
    public int MentalStrength { get; set; } = 10000;
    public int Pain { get; set; }
    public int Fear { get; set; }
    public int Disgust { get; set; }
    public int Antipathy { get; set; }
    public int Despair { get; set; }

    // Affection (0-20000 = 200%)
    public int Favorability { get; set; }
    public int Obedience { get; set; }
    public int Lust { get; set; }
    public int Submission { get; set; }
    public int MilkCow { get; set; }

    // Derived state
    public FallState FallState { get; set; } = FallState.Normal;
    public bool IsCollapsed { get; set; }
    public bool IsBrainwashed { get; set; }
    public int PleasureA { get; set; }
    public int PleasureB { get; set; }
    public int PleasureC { get; set; }
    public int PleasureV { get; set; }
    public int PleasureN { get; set; }
    public int LubricationV { get; set; }
    public int LubricationA { get; set; }
}

public sealed class MilkState
{
    public int Capacity { get; set; } = 100;
    public int Production { get; set; } = 50;
    public int BaseOutput { get; set; } = 10;
    public int CurrentAmount { get; set; }
    public int Quality { get; set; } = 100;
    public int TotalProduced { get; set; }
    public int TotalShipped { get; set; }
    public int TotalRevenue { get; set; }
    public bool HasMilkConstitution { get; set; }
    public bool HasMagicMilkConstitution { get; set; }
    public string Concentration { get; set; } = "standard";
    public int EquippedMilkerId { get; set; }
}

public sealed class AddictionState
{
    public int VaginalEjaculation { get; set; }
    public int AnalEjaculation { get; set; }
    public int BreastEjaculation { get; set; }
    public int SemenDrinking { get; set; }
    public int SemenAddiction { get; set; }
    public int Gangbang { get; set; }
    public int Masochism { get; set; }
    public int Sadism { get; set; }
    public int Lesbian { get; set; }
    public int Milking { get; set; }
    public int Tentacle { get; set; }
    public int ServiceSpirit { get; set; }
}

public sealed class EquipmentState
{
    public int ClothesId { get; set; }
    public int UnderwearTopId { get; set; }
    public int UnderwearBottomId { get; set; }
    public int ArmorId { get; set; }
    public int EyesId { get; set; }
    public int HeadId { get; set; }
    public int ArmsId { get; set; }
    public int LegsId { get; set; }
    public int NeckId { get; set; }
    public int JacketId { get; set; }
    public int CollarId { get; set; }
}

public sealed class TrainingRecord
{
    public string ActionId { get; set; } = string.Empty;
    public string CharacterId { get; set; } = string.Empty;
    public int Day { get; set; }
    public int Phase { get; set; }
    public int PleasureGained { get; set; }
    public int PainGained { get; set; }
    public int MentalEffect { get; set; }
    public string Summary { get; set; } = string.Empty;
}