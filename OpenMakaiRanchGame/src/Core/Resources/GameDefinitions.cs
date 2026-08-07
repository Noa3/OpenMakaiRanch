using Godot;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Core.Resources;

public enum JobCategory
{
    Rest,
    RanchWork,
    Chore,
    Mentorship,
    Adventure,
    Dairy,
    Office,
    Cleaning,
    Cooking,
    Pharmacy,
    CustomerService
}

public enum ItemCategory
{
    Consumable,
    Material,
    Tool,
    Keepsake,
    Equipment
}

public enum MissionTier
{
    Local,
    Regional,
    Dangerous
}

public enum MilestoneTriggerKind
{
    DayReached,
    GoldReached,
    MissionCompleted,
    BondReached,
    ResearchUnlocked,
    CharacterCount,
    FacilityMaster,
    PetCount,
    EquipmentCount
}

[GlobalClass]
public partial class CharacterDefinition : Resource
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PortraitPath { get; set; } = string.Empty;
    public string BodyImagePath { get; set; } = string.Empty;
    public string BodyType { get; set; } = string.Empty;
    public int MaxHp { get; set; }
    public int MaxEnergy { get; set; }
    public int RanchSkill { get; set; }
    public int CraftSkill { get; set; }
    public int CombatSkill { get; set; }
    public string Trait { get; set; } = string.Empty;
    // Extended fields from original CSV
    public string Race { get; set; } = string.Empty;
    public string Personality { get; set; } = string.Empty;
    public string JobClass { get; set; } = string.Empty;
    public string Height { get; set; } = string.Empty;
    public string SkinColor { get; set; } = string.Empty;
    public string HairColor { get; set; } = string.Empty;
    public string HairStyle { get; set; } = string.Empty;
    public string EyeColor { get; set; } = string.Empty;
    public string EyeFeature { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int MagicPower { get; set; }
    public List<string> Talents { get; set; } = new();
    public List<string> StartingItems { get; set; } = new();
}

[GlobalClass]
public partial class JobDefinition : Resource
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public JobCategory Category { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public int ResourceAmount { get; set; }
    public int GoldIncome { get; set; }
    public int FatigueDelta { get; set; }
    public int MoraleDelta { get; set; }
    public int BondDelta { get; set; }
    public bool Assignable { get; set; } = true;
}

public enum EquipmentSlot
{
    Weapon,
    Armor,
    Accessory,
    Head,
    Feet,
    UnderwearTop,
    UnderwearBottom,
    Necklace,
    Coat,
    Ears,
    Arms,
    Legs
}

public enum ClothingStyle
{
    Default,
    Workwear,
    Maid,
    Bunny,
    Nurse,
    School,
    Exorcist,
    Slave,
    CowGirl,
    Swimsuit,
    Lingerie,
    Formal,
    Casual,
    Tactical
}

public enum ItemEffectType
{
    None,
    EnergyRestore,
    FatigueReduce,
    MoraleBoost,
    HpRestore,
    SkillBoost,
    BondBoost,
    HairColorChange,
    MilkCapacityIncrease,
    MilkQualityIncrease,
    BreastSizeIncrease,
    SensitivityIncrease,
    MilkConstitution,
    MagicMilkConstitution,
    ConcentrationThicken,
    Transformation,
    TalentGrant,
    FacilityUnlock,
    PetAdopt
}

public enum SpellType
{
    Drain,
    Empower,
    Transform,
    Summon,
    Enchant,
    Curse,
    Bless,
    Teleport
}

[GlobalClass]
public partial class SpellDefinition : Resource
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SpellType Type { get; set; }
    public int ManaCost { get; set; }
    public int SpiritEnergyCost { get; set; }
    public int CooldownDays { get; set; }
    public int RequiredMagicPower { get; set; }
    public string EffectTarget { get; set; } = string.Empty;
    public int EffectValue { get; set; }
    public string EffectDescription { get; set; } = string.Empty;
    public bool RequiresTarget { get; set; }
}

[GlobalClass]
public partial class ItemDefinition : Resource
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public ItemCategory Category { get; set; }
    public int Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public EquipmentSlot Slot { get; set; }
    public int BonusRanchSkill { get; set; }
    public int BonusCraftSkill { get; set; }
    public int BonusCombatSkill { get; set; }
    public int BonusMaxHp { get; set; }
    public int BonusMaxEnergy { get; set; }
    public int BonusMorale { get; set; }
    public ClothingStyle ClothingStyleValue { get; set; } = ClothingStyle.Default;
    public ItemEffectType EffectType { get; set; } = ItemEffectType.None;
    public string EffectTarget { get; set; } = string.Empty;
    public int EffectValue { get; set; }
    public string EffectDescription { get; set; } = string.Empty;
}

[GlobalClass]
public partial class TalentDefinition : Resource
{
    [Export] public string Id { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "";
    [Export] public string Description { get; set; } = "";
    [Export] public int BonusRanchSkill { get; set; }
    [Export] public int BonusCraftSkill { get; set; }
    [Export] public int BonusCombatSkill { get; set; }
    [Export] public int BonusMaxHp { get; set; }
    [Export] public int BonusMaxEnergy { get; set; }
    [Export] public float GrowthMultiplier { get; set; } = 1f;
    [Export] public float JobOutputMultiplier { get; set; } = 1f;
    [Export] public float TrainingEfficiency { get; set; } = 1f;
    [Export] public int MoraleCapBonus { get; set; }
    [Export] public int FatigueResistance { get; set; }
}

[GlobalClass]
public partial class FacilityDefinition : Resource
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int BuildCost { get; set; }
    public int UpkeepGold { get; set; }
    public string OutputResourceId { get; set; } = string.Empty;
    public int OutputBonus { get; set; }
    public int Capacity { get; set; }
}

[GlobalClass]
public partial class MissionDefinition : Resource
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public MissionTier Tier { get; set; }
    public int Difficulty { get; set; }
    public int RewardGold { get; set; }
    public string RewardItemId { get; set; } = string.Empty;
    public string EnemyGroupId { get; set; } = string.Empty;
}

[GlobalClass]
public partial class EnemyDefinition : Resource
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public MissionTier Tier { get; set; }
    public int BaseHp { get; set; } = 50;
    public int BaseSp { get; set; } = 20;
    public int Attack { get; set; } = 8;
    public int Defense { get; set; } = 4;
    public int Speed { get; set; } = 5;
    public int RewardGold { get; set; }
    public string RewardItemId { get; set; } = string.Empty;
    public int CaptureDifficulty { get; set; } = 30;
}

[GlobalClass]
public partial class MilestoneDefinition : Resource
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public MilestoneTriggerKind TriggerKind { get; set; }
    public string TriggerId { get; set; } = string.Empty;
    public int TriggerAmount { get; set; }
    public int RewardGold { get; set; }
}

[GlobalClass]
public partial class SkillDefinition : Resource
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CostResourceId { get; set; } = string.Empty;
    public int CostAmount { get; set; }
}

[GlobalClass]
public partial class PetDefinition : Resource
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int CareCost { get; set; }
    public bool IsMountable { get; set; }
}

public enum BondEventTrigger
{
    BondReached,
    DayReached,
    AfterJob,
    AfterMission,
    AfterTraining,
    Random
}

[GlobalClass]
public partial class TrainingActionDefinition : Resource
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public TrainingCategory Category { get; set; }
    public int ActionId { get; set; }
    public int FatigueDelta { get; set; }
    public int MoraleDelta { get; set; }
    public List<string> XpTypes { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}

[GlobalClass]
public partial class BondEventDefinition : Resource
{
    public string Id { get; set; } = string.Empty;
    public string CharacterId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RequiredBond { get; set; }
    public int BondReward { get; set; }
    public int MoraleReward { get; set; }
    public string StockpileRewardId { get; set; } = string.Empty;
    public int StockpileRewardAmount { get; set; }
}