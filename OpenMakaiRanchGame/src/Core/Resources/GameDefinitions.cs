using Godot;

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
    Feet
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