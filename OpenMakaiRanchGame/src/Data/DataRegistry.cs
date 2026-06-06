using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Resources;

namespace OpenMakaiRanch.Data;

public sealed class DataRegistry
{
    public Dictionary<string, CharacterDefinition> Characters { get; } = new();
    public Dictionary<string, JobDefinition> Jobs { get; } = new();
    public Dictionary<string, ItemDefinition> Items { get; } = new();
    public Dictionary<string, FacilityDefinition> Facilities { get; } = new();
    public Dictionary<string, MissionDefinition> Missions { get; } = new();
    public Dictionary<string, MilestoneDefinition> Milestones { get; } = new();
    public Dictionary<string, SkillDefinition> Skills { get; } = new();
    public Dictionary<string, PetDefinition> Pets { get; } = new();
    public Dictionary<string, BondEventDefinition> BondEvents { get; } = new();

    public static DataRegistry CreateSeeded()
    {
        var registry = new DataRegistry();
        registry.SeedCharacters();
        registry.SeedJobs();
        registry.SeedItems();
        registry.SeedFacilities();
        registry.SeedMissions();
        registry.SeedMilestones();
        registry.SeedSkills();
        registry.SeedPets();
        registry.SeedBondEvents();
        return registry;
    }

    public CharacterDefinition Character(string id) => Characters[id];
    public JobDefinition Job(string id) => Jobs[id];
    public ItemDefinition Item(string id) => Items[id];
    public MissionDefinition Mission(string id) => Missions[id];

    public IReadOnlyList<CharacterDefinition> CharacterList() => Characters.Values.ToList();
    public IReadOnlyList<JobDefinition> AssignableJobs() => Jobs.Values.Where(job => job.Assignable).ToList();
    public IReadOnlyList<ItemDefinition> ShopItems() => Items.Values.Where(item => item.Price > 0).ToList();

    private void SeedCharacters()
    {
        Add(new CharacterDefinition { Id = "rancher", DisplayName = "Rancher", PortraitPath = "res://assets/portraits/sampleprt.png", BodyImagePath = "res://assets/portraits/sampleprt.png", BodyType = "Balanced", MaxHp = 120, MaxEnergy = 90, RanchSkill = 4, CraftSkill = 4, CombatSkill = 3, Trait = "Steady" });
        Add(new CharacterDefinition { Id = "slay", DisplayName = "Slay", PortraitPath = "res://assets/portraits/slay.png", BodyImagePath = "res://assets/portraits/slay.png", BodyType = "Athletic", MaxHp = 130, MaxEnergy = 80, RanchSkill = 5, CraftSkill = 2, CombatSkill = 5, Trait = "Bold" });
        Add(new CharacterDefinition { Id = "maria", DisplayName = "Maria", PortraitPath = "res://assets/portraits/maria.png", BodyImagePath = "res://assets/portraits/maria.png", BodyType = "Refined", MaxHp = 95, MaxEnergy = 110, RanchSkill = 4, CraftSkill = 6, CombatSkill = 2, Trait = "Careful" });
        Add(new CharacterDefinition { Id = "kagura", DisplayName = "Kagura", PortraitPath = "res://assets/portraits/kagura.png", BodyImagePath = "res://assets/portraits/kagura.png", BodyType = "Athletic", MaxHp = 105, MaxEnergy = 95, RanchSkill = 3, CraftSkill = 5, CombatSkill = 6, Trait = "Focused" });
        Add(new CharacterDefinition { Id = "sharon", DisplayName = "Sharon", PortraitPath = "res://assets/portraits/sharon.png", BodyImagePath = "res://assets/portraits/sharon.png", BodyType = "Sturdy", MaxHp = 100, MaxEnergy = 100, RanchSkill = 6, CraftSkill = 3, CombatSkill = 3, Trait = "Warm" });
        Add(new CharacterDefinition { Id = "noir", DisplayName = "Noir", PortraitPath = "res://assets/portraits/noir.png", BodyImagePath = "res://assets/portraits/noir.png", BodyType = "Lean", MaxHp = 90, MaxEnergy = 120, RanchSkill = 2, CraftSkill = 6, CombatSkill = 5, Trait = "Quiet" });
    }

    private void SeedJobs()
    {
        Add(new JobDefinition { Id = "rest", DisplayName = "Rest", Category = JobCategory.Rest, FatigueDelta = -24, MoraleDelta = 5, Assignable = true });
        Add(new JobDefinition { Id = "pasture", DisplayName = "Pasture Work", Category = JobCategory.RanchWork, ResourceId = "farm_goods", ResourceAmount = 5, GoldIncome = 35, FatigueDelta = 12, MoraleDelta = 1 });
        Add(new JobDefinition { Id = "kitchen", DisplayName = "Kitchen Chores", Category = JobCategory.Chore, ResourceId = "meals", ResourceAmount = 3, GoldIncome = 20, FatigueDelta = 8, MoraleDelta = 2 });
        Add(new JobDefinition { Id = "workshop", DisplayName = "Workshop Crafting", Category = JobCategory.Chore, ResourceId = "supplies", ResourceAmount = 2, GoldIncome = 25, FatigueDelta = 10, MoraleDelta = 0 });
        Add(new JobDefinition { Id = "mentorship", DisplayName = "Mentorship", Category = JobCategory.Mentorship, ResourceId = "trust", ResourceAmount = 1, GoldIncome = 10, FatigueDelta = 4, MoraleDelta = 8, BondDelta = 6 });
        Add(new JobDefinition { Id = "patrol", DisplayName = "Adventure Patrol", Category = JobCategory.Adventure, ResourceId = "intel", ResourceAmount = 1, GoldIncome = 45, FatigueDelta = 16, MoraleDelta = 3 });
    }

    private void SeedItems()
    {
        Add(new ItemDefinition { Id = "meal_box", DisplayName = "Meal Box", Category = ItemCategory.Consumable, Price = 30, Description = "A packed meal that helps with recovery." });
        Add(new ItemDefinition { Id = "tool_kit", DisplayName = "Tool Kit", Category = ItemCategory.Tool, Price = 90, Description = "Basic ranch repair tools." });
        Add(new ItemDefinition { Id = "feed_bundle", DisplayName = "Feed Bundle", Category = ItemCategory.Material, Price = 25, Description = "Supplies for ranch facilities." });
        Add(new ItemDefinition { Id = "keepsake", DisplayName = "Keepsake Charm", Category = ItemCategory.Keepsake, Price = 120, Description = "A morale-boosting gift." });
    }

    private void SeedFacilities()
    {
        Add(new FacilityDefinition { Id = "pasture", DisplayName = "Pasture", BuildCost = 180, UpkeepGold = 20, OutputResourceId = "farm_goods", OutputBonus = 3 });
        Add(new FacilityDefinition { Id = "kitchen", DisplayName = "Kitchen", BuildCost = 140, UpkeepGold = 12, OutputResourceId = "meals", OutputBonus = 1 });
        Add(new FacilityDefinition { Id = "workshop", DisplayName = "Workshop", BuildCost = 170, UpkeepGold = 16, OutputResourceId = "supplies", OutputBonus = 1 });
        Add(new FacilityDefinition { Id = "guest_room", DisplayName = "Guest Rooms", BuildCost = 120, UpkeepGold = 8, OutputResourceId = "comfort", OutputBonus = 1 });
    }

    private void SeedMissions()
    {
        Add(new MissionDefinition { Id = "road_patrol", DisplayName = "Road Patrol", Tier = MissionTier.Local, Difficulty = 10, RewardGold = 80, RewardItemId = "feed_bundle" });
        Add(new MissionDefinition { Id = "forest_survey", DisplayName = "Forest Survey", Tier = MissionTier.Regional, Difficulty = 16, RewardGold = 130, RewardItemId = "tool_kit" });
    }

    private void SeedMilestones()
    {
        Add(new MilestoneDefinition { Id = "first_day", DisplayName = "First Settlement", TriggerKind = MilestoneTriggerKind.DayReached, TriggerAmount = 2, RewardGold = 50 });
        Add(new MilestoneDefinition { Id = "steady_ranch", DisplayName = "Steady Ranch", TriggerKind = MilestoneTriggerKind.GoldReached, TriggerAmount = 750, RewardGold = 100 });
        Add(new MilestoneDefinition { Id = "first_patrol", DisplayName = "First Patrol", TriggerKind = MilestoneTriggerKind.MissionCompleted, TriggerId = "any", RewardGold = 75 });
        Add(new MilestoneDefinition { Id = "first_trust", DisplayName = "First Trust", TriggerKind = MilestoneTriggerKind.BondReached, TriggerAmount = 20, RewardGold = 60 });
        Add(new MilestoneDefinition { Id = "first_research", DisplayName = "First Research", TriggerKind = MilestoneTriggerKind.ResearchUnlocked, TriggerId = "any", RewardGold = 70 });
    }

    private void SeedSkills()
    {
        Add(new SkillDefinition { Id = "ranch_planning", DisplayName = "Ranch Planning", Description = "Improves job output and schedule efficiency.", CostResourceId = "supplies", CostAmount = 3 });
        Add(new SkillDefinition { Id = "field_medicine", DisplayName = "Field Medicine", Description = "Reduces adventure fatigue risk.", CostResourceId = "meals", CostAmount = 4 });
    }

    private void SeedPets()
    {
        Add(new PetDefinition { Id = "stable_cat", DisplayName = "Stable Cat", CareCost = 15 });
        Add(new PetDefinition { Id = "yard_hound", DisplayName = "Yard Hound", CareCost = 20 });
    }

    private void SeedBondEvents()
    {
        Add(new BondEventDefinition { Id = "slay_morning_rounds", CharacterId = "slay", Title = "Morning Rounds", Description = "Walk the boundary fences together and turn routine patrol into a calm leadership lesson.", RequiredBond = 0, BondReward = 7, MoraleReward = 3, StockpileRewardId = "intel", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "maria_recipe_notes", CharacterId = "maria", Title = "Recipe Notes", Description = "Review meal planning and preserve a few practical kitchen tricks for the whole ranch.", RequiredBond = 0, BondReward = 6, MoraleReward = 5, StockpileRewardId = "meals", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "kagura_focus_drill", CharacterId = "kagura", Title = "Focus Drill", Description = "Practice measured breathing and field awareness before the evening chores begin.", RequiredBond = 8, BondReward = 8, MoraleReward = 2, StockpileRewardId = "supplies", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "sharon_guest_care", CharacterId = "sharon", Title = "Guest Care", Description = "Prepare the guest rooms and discuss what makes the ranch feel safe and welcoming.", RequiredBond = 6, BondReward = 7, MoraleReward = 4, StockpileRewardId = "comfort", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "noir_quiet_inventory", CharacterId = "noir", Title = "Quiet Inventory", Description = "Sort supplies in companionable silence and notice what the ranch is short on.", RequiredBond = 6, BondReward = 7, MoraleReward = 3, StockpileRewardId = "supplies", StockpileRewardAmount = 1 });
    }

    private void Add(CharacterDefinition definition) => Characters[definition.Id] = definition;
    private void Add(JobDefinition definition) => Jobs[definition.Id] = definition;
    private void Add(ItemDefinition definition) => Items[definition.Id] = definition;
    private void Add(FacilityDefinition definition) => Facilities[definition.Id] = definition;
    private void Add(MissionDefinition definition) => Missions[definition.Id] = definition;
    private void Add(MilestoneDefinition definition) => Milestones[definition.Id] = definition;
    private void Add(SkillDefinition definition) => Skills[definition.Id] = definition;
    private void Add(PetDefinition definition) => Pets[definition.Id] = definition;
    private void Add(BondEventDefinition definition) => BondEvents[definition.Id] = definition;
}