using Godot;

namespace OpenMakaiRanch.Core.Resources;

public enum JobCategory
{
    Rest,
    RanchWork,
    Chore,
    Mentorship,
    Adventure
}

public enum ItemCategory
{
    Consumable,
    Material,
    Tool,
    Keepsake
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
    ResearchUnlocked
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

[GlobalClass]
public partial class ItemDefinition : Resource
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public ItemCategory Category { get; set; }
    public int Price { get; set; }
    public string Description { get; set; } = string.Empty;
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