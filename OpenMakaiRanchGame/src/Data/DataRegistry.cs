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
    public Dictionary<string, EnemyDefinition> Enemies { get; } = new();

    public static DataRegistry CreateSeeded()
    {
        var registry = new DataRegistry();
        registry.SeedCharacters();
        registry.SeedJobs();
        registry.SeedItems();
        registry.SeedFacilities();
        registry.SeedMissions();
        registry.SeedEnemies();
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
        Add(new CharacterDefinition { Id = "rancher", DisplayName = "Rancher", PortraitPath = "res://assets/portraits/sampleprt.png", BodyImagePath = "res://assets/portraits/sampleprt.png", BodyType = "Balanced", MaxHp = 200, MaxEnergy = 200, RanchSkill = 7, CraftSkill = 7, CombatSkill = 5, Trait = "Steady", Race = "Makai-jin", Personality = "Quiet", JobClass = "Rancher", Height = "Tall", SkinColor = "Standard", HairColor = "Black", HairStyle = "Short", EyeColor = "Red", Level = 10, MagicPower = 100, Talents = new List<string> { "horns", "male", "owner", "makai_race" }, StartingItems = new List<string> { "work_wear" } });
        Add(new CharacterDefinition { Id = "slay", DisplayName = "Slay", PortraitPath = "res://assets/portraits/slay.png", BodyImagePath = "res://assets/portraits/slay.png", BodyType = "Athletic", MaxHp = 100, MaxEnergy = 200, RanchSkill = 8, CraftSkill = 2, CombatSkill = 5, Trait = "Bold", Race = "Human", Personality = "Quiet", JobClass = "Foundling", Height = "Short (144cm)", SkinColor = "Standard", HairColor = "Blonde", HairStyle = "Short", EyeColor = "Blue", Description = "A girl who stumbled into the Makai realm by accident. Grateful for being taken in.", Level = 25, Talents = new List<string> { "mouth_paradise", "devoted", "docile", "fast_learner", "pharmacy_knowledge", "hospitality_clumsy" }, StartingItems = new List<string> { "work_wear" } });
        Add(new CharacterDefinition { Id = "kagura", DisplayName = "Kagura", PortraitPath = "res://assets/portraits/kagura.png", BodyImagePath = "res://assets/portraits/kagura.png", BodyType = "Athletic", MaxHp = 150, MaxEnergy = 200, RanchSkill = 3, CraftSkill = 5, CombatSkill = 6, Trait = "Focused", Race = "Human", Personality = "Gentle", JobClass = "Exorcist Miko", Height = "Short (151cm)", SkinColor = "Standard", HairColor = "Black", HairStyle = "Semi-long", EyeColor = "Black", Description = "A captive exorcist miko protected by a virginity barrier. Clear black hair and a calm demeanor.", Level = 25, MagicPower = 3, Talents = new List<string> { "virginity_barrier", "chastity", "devoted", "faith", "obedient", "self_control", "shy", "pure" }, StartingItems = new List<string> { "combat_miko_robe" } });
        Add(new CharacterDefinition { Id = "maria", DisplayName = "Maria", PortraitPath = "res://assets/portraits/maria.png", BodyImagePath = "res://assets/portraits/maria.png", BodyType = "Refined", MaxHp = 150, MaxEnergy = 200, RanchSkill = 4, CraftSkill = 6, CombatSkill = 2, Trait = "Careful", Race = "Human", Personality = "Earnest", JobClass = "Battle Sister", Height = "Standard (158cm)", SkinColor = "Standard", HairColor = "Blonde", HairStyle = "Ponytail", EyeColor = "Blue", Description = "A captive battle sister protected by a virginity barrier. Earnest and principled.", Level = 25, MagicPower = 3, Talents = new List<string> { "virginity_barrier", "chastity", "devoted", "justice", "faith", "baby_face", "rebellious", "conservative" }, StartingItems = new List<string> { "combat_sister_robe" } });
        Add(new CharacterDefinition { Id = "sharon", DisplayName = "Sharon", PortraitPath = "res://assets/portraits/sharon.png", BodyImagePath = "res://assets/portraits/sharon.png", BodyType = "Sturdy", MaxHp = 120, MaxEnergy = 150, RanchSkill = 6, CraftSkill = 3, CombatSkill = 3, Trait = "Warm", Race = "Human", Personality = "Timid", JobClass = "White Mage", Height = "Short (149cm)", SkinColor = "Pale", HairColor = "Pink", HairStyle = "Long", EyeColor = "Sky Blue", Description = "A white mage separated from her companions while searching for Makai Crystals. Timid and gentle.", Level = 31, MagicPower = 5, Talents = new List<string> { "extreme_milk_pressure", "pharmacy_knowledge", "cleaning_clumsy", "honest_to_pleasure", "cowardly", "shy", "breast_abuse_hatred" }, StartingItems = new List<string> { "robe", "hairband" } });
        Add(new CharacterDefinition { Id = "noir", DisplayName = "Noir", PortraitPath = "res://assets/portraits/noir.png", BodyImagePath = "res://assets/portraits/noir.png", BodyType = "Lean", MaxHp = 180, MaxEnergy = 220, RanchSkill = 2, CraftSkill = 6, CombatSkill = 5, Trait = "Quiet", Race = "Human", Personality = "Confident", JobClass = "Black Mage", Height = "Standard (162cm)", SkinColor = "Standard", HairColor = "Silver", HairStyle = "Very Long", EyeColor = "Red", Description = "A black mage separated from her companions. Confident and glamorous, but easily charmed.", Level = 43, MagicPower = 5, Talents = new List<string> { "extreme_milk_pressure", "pharmacy_knowledge", "cleaning_clumsy", "easily_charmed", "shameless", "moody", "breast_proud" }, StartingItems = new List<string> { "robe", "ribbon" } });
        Add(new CharacterDefinition { Id = "ayaka", DisplayName = "Ayaka", PortraitPath = "res://assets/portraits/ayaka.png", BodyImagePath = "res://assets/portraits/ayaka.png", BodyType = "Refined", MaxHp = 210, MaxEnergy = 200, RanchSkill = 3, CraftSkill = 7, CombatSkill = 2, Trait = "Graceful", Race = "Human", Personality = "Confident", JobClass = "Exorcist", Height = "Short (155cm)", SkinColor = "Pale", HairColor = "Red", HairStyle = "Semi-long", EyeColor = "Blue", Description = "A Nordic-quarter exorcist. Tsundere with strong pride and a sense of justice.", Level = 46, MagicPower = 1, Talents = new List<string> { "proud", "steadfast", "doesnt_cross_line", "tsundere", "rebellious", "justice", "chastity", "maiden_heart", "denies_pleasure", "shy", "fast_learner", "jk" }, StartingItems = new List<string> { "blazer_uniform" } });
        Add(new CharacterDefinition { Id = "en", DisplayName = "En", PortraitPath = "res://assets/portraits/en.png", BodyImagePath = "res://assets/portraits/en.png", BodyType = "Sturdy", MaxHp = 240, MaxEnergy = 200, RanchSkill = 6, CraftSkill = 4, CombatSkill = 4, Trait = "Nurturing", Race = "Dhampir", Personality = "Gentle", JobClass = "Exorcist", Height = "Standard (165cm)", SkinColor = "Standard", HairColor = "Chestnut", HairStyle = "Semi-long", EyeColor = "Brown", Description = "A half-vampire exorcist. Graceful and dignified with a nurturing nature.", Level = 47, MagicPower = 5, Talents = new List<string> { "self_control", "indifferent", "doesnt_cross_line", "conservative", "devoted", "maternal_instinct", "chastity", "rebellious", "dignity", "proud", "shy", "instigator" }, StartingItems = new List<string> { "cloth_clothes" } });
        Add(new CharacterDefinition { Id = "yukina", DisplayName = "Yukina", PortraitPath = "res://assets/portraits/yukina.png", BodyImagePath = "res://assets/portraits/yukina.png", BodyType = "Athletic", MaxHp = 220, MaxEnergy = 200, RanchSkill = 4, CraftSkill = 3, CombatSkill = 7, Trait = "Determined", Race = "Werewolf", Personality = "Airhead", JobClass = "Exorcist", Height = "Standard (158cm)", SkinColor = "Pale", HairColor = "Silver", HairStyle = "Ponytail", EyeColor = "Red", Description = "A werewolf exorcist. Airheaded and cheerful with animal ears and a wagging tail.", Level = 50, MagicPower = 3, Talents = new List<string> { "obedient", "cowardly", "doesnt_cross_line", "docile", "shy", "klutz", "chastity", "animal_ears", "denies_pleasure", "rebellious", "fast_learner", "weak_to_pain" }, StartingItems = new List<string> { "cloth_clothes" } });
        Add(new CharacterDefinition { Id = "anon", DisplayName = "Anon", PortraitPath = "res://assets/portraits/anon.png", BodyImagePath = "res://assets/portraits/anon.png", BodyType = "Balanced", MaxHp = 200, MaxEnergy = 200, RanchSkill = 10, CraftSkill = 10, CombatSkill = 5, Trait = "Curious", Race = "Makai-jin", Personality = "Whimsical", JobClass = "Ranch Owner", Height = "Tall (190cm)", SkinColor = "Standard", HairColor = "Black", HairStyle = "Short", EyeColor = "Red", Description = "The ranch owner. A Makai-jin with a playful curiosity and a practical streak.", Level = 50, MagicPower = 5, Talents = new List<string> { "horns", "male", "owner", "virgin", "a_virgin", "m_virgin", "makai_race" }, StartingItems = new List<string> { "work_wear" } });
    }

    private void SeedJobs()
    {
        Add(new JobDefinition { Id = "rest", DisplayName = "Rest", Category = JobCategory.Rest, FatigueDelta = -24, MoraleDelta = 5, Assignable = true });
        Add(new JobDefinition { Id = "pasture", DisplayName = "Pasture Work", Category = JobCategory.RanchWork, ResourceId = "farm_goods", ResourceAmount = 5, GoldIncome = 35, FatigueDelta = 12, MoraleDelta = 1, Assignable = true });
        Add(new JobDefinition { Id = "kitchen", DisplayName = "Kitchen Chores", Category = JobCategory.Chore, ResourceId = "meals", ResourceAmount = 3, GoldIncome = 20, FatigueDelta = 8, MoraleDelta = 2, Assignable = true });
        Add(new JobDefinition { Id = "workshop", DisplayName = "Workshop Crafting", Category = JobCategory.Chore, ResourceId = "supplies", ResourceAmount = 2, GoldIncome = 25, FatigueDelta = 10, MoraleDelta = 0, Assignable = true });
        Add(new JobDefinition { Id = "mentorship", DisplayName = "Mentorship", Category = JobCategory.Mentorship, ResourceId = "trust", ResourceAmount = 1, GoldIncome = 10, FatigueDelta = 4, MoraleDelta = 8, BondDelta = 6, Assignable = true });
        Add(new JobDefinition { Id = "patrol", DisplayName = "Adventure Patrol", Category = JobCategory.Adventure, ResourceId = "intel", ResourceAmount = 1, GoldIncome = 45, FatigueDelta = 16, MoraleDelta = 3, Assignable = true });
        // Original work skills
        Add(new JobDefinition { Id = "dairy", DisplayName = "Dairy Work", Category = JobCategory.Dairy, ResourceId = "farm_goods", ResourceAmount = 6, GoldIncome = 40, FatigueDelta = 14, MoraleDelta = 0, Assignable = true });
        Add(new JobDefinition { Id = "office", DisplayName = "Office Work", Category = JobCategory.Office, ResourceId = "supplies", ResourceAmount = 4, GoldIncome = 30, FatigueDelta = 6, MoraleDelta = 2, Assignable = true });
        Add(new JobDefinition { Id = "cleaning", DisplayName = "Cleaning", Category = JobCategory.Cleaning, ResourceId = "comfort", ResourceAmount = 3, GoldIncome = 15, FatigueDelta = 10, MoraleDelta = 1, Assignable = true });
        Add(new JobDefinition { Id = "cooking", DisplayName = "Cooking", Category = JobCategory.Cooking, ResourceId = "meals", ResourceAmount = 5, GoldIncome = 25, FatigueDelta = 10, MoraleDelta = 3, Assignable = true });
        Add(new JobDefinition { Id = "pharmacy", DisplayName = "Pharmacy", Category = JobCategory.Pharmacy, ResourceId = "supplies", ResourceAmount = 3, GoldIncome = 35, FatigueDelta = 8, MoraleDelta = 1, Assignable = true });
        Add(new JobDefinition { Id = "customer_service", DisplayName = "Customer Service", Category = JobCategory.CustomerService, ResourceId = "comfort", ResourceAmount = 2, GoldIncome = 40, FatigueDelta = 12, MoraleDelta = 2, Assignable = true });
    }

    private void SeedItems()
    {
        // SFW consumables (original game nos 200-400)
        Add(new ItemDefinition { Id = "energy_drink", DisplayName = "Energy Drink", Category = ItemCategory.Consumable, Price = 40, Description = "Restores energy and sharpens focus. (Orig: 300g nutrient drink)" });
        Add(new ItemDefinition { Id = "herb_tea", DisplayName = "Herbal Tea", Category = ItemCategory.Consumable, Price = 35, Description = "Soothing Makai herb tea that lifts spirits. (Orig: 500g herb tea)" });
        Add(new ItemDefinition { Id = "first_aid", DisplayName = "First Aid Kit", Category = ItemCategory.Consumable, Price = 60, Description = "Bandages and salves for treating injuries." });
        Add(new ItemDefinition { Id = "meal_box", DisplayName = "Meal Box", Category = ItemCategory.Consumable, Price = 30, Description = "A packed meal that helps with recovery." });
        Add(new ItemDefinition { Id = "pet_jerky", DisplayName = "Mystery Jerky", Category = ItemCategory.Consumable, Price = 20, Description = "Dried meat strips. Ranch pets love these. (Orig: 500g mystery jerky)" });
        Add(new ItemDefinition { Id = "pet_seeds", DisplayName = "Sunflower Seeds", Category = ItemCategory.Consumable, Price = 15, Description = "Roasted hell sunflower seeds. A favorite pet treat. (Orig: 500g sunflower seeds)" });
        // New consumables from Item.csv
        Add(new ItemDefinition { Id = "lotion", DisplayName = "Lotion", Category = ItemCategory.Consumable, Price = 25, Description = "Smoothing lotion for massage and recovery. (Orig: 200g lotion)" });
        Add(new ItemDefinition { Id = "lube", DisplayName = "Lubricant", Category = ItemCategory.Consumable, Price = 30, Description = "General-purpose lubricant for equipment maintenance. (Orig: 300g condom)" });
        Add(new ItemDefinition { Id = "hair_dye", DisplayName = "Hair Color Treatment", Category = ItemCategory.Consumable, Price = 100, Description = "Changes hair color permanently. (Orig: 10000g hair color)" });
        Add(new ItemDefinition { Id = "collar_tag", DisplayName = "Livestock Management Tag", Category = ItemCategory.Consumable, Price = 50, Description = "For identifying and cataloging ranch residents. (Orig: 500g management tag)" });
        Add(new ItemDefinition { Id = "guts_carrot", DisplayName = "Guts Carrot", Category = ItemCategory.Consumable, Price = 25, Description = "A crunchy Makai carrot that boosts stamina. (Orig: 500g guts carrot)" });
        Add(new ItemDefinition { Id = "milk_tea", DisplayName = "Milk Tea Mix", Category = ItemCategory.Consumable, Price = 20, Description = "A sweet milk tea powder. A comforting treat." });
        Add(new ItemDefinition { Id = "protein_bar", DisplayName = "Protein Bar", Category = ItemCategory.Consumable, Price = 35, Description = "Nutritious compressed energy bar for active ranch hands." });
        Add(new ItemDefinition { Id = "bandage", DisplayName = "Bandage Pack", Category = ItemCategory.Consumable, Price = 25, Description = "Clean bandages for minor injuries and fatigue recovery." });
        Add(new ItemDefinition { Id = "tonic", DisplayName = "General Tonic", Category = ItemCategory.Consumable, Price = 45, Description = "A general health tonic that promotes recovery." });

        // Materials
        Add(new ItemDefinition { Id = "fabric_patch", DisplayName = "Fabric Patch Kit", Category = ItemCategory.Material, Price = 50, Description = "Repairs torn clothing and gear around the ranch. (Orig: 1000g fabric patch)" });
        Add(new ItemDefinition { Id = "premium_feed", DisplayName = "Premium Feed", Category = ItemCategory.Material, Price = 60, Description = "Nutritious feed blend that boosts ranch output." });
        Add(new ItemDefinition { Id = "feed_bundle", DisplayName = "Feed Bundle", Category = ItemCategory.Material, Price = 25, Description = "Supplies for ranch facilities and livestock." });
        Add(new ItemDefinition { Id = "herb_pack", DisplayName = "Dried Herb Pack", Category = ItemCategory.Material, Price = 40, Description = "A bundle of dried medicinal herbs for remedies." });
        Add(new ItemDefinition { Id = "leather_scrap", DisplayName = "Leather Scraps", Category = ItemCategory.Material, Price = 35, Description = "Leftover leather suitable for small repairs and crafts." });
        Add(new ItemDefinition { Id = "magic_crystal", DisplayName = "Magic Crystal Shard", Category = ItemCategory.Material, Price = 80, Description = "A faintly glowing crystal that stores ambient energy." });

        // Tools
        Add(new ItemDefinition { Id = "tool_kit", DisplayName = "Tool Kit", Category = ItemCategory.Tool, Price = 90, Description = "Basic ranch repair and maintenance tools." });
        Add(new ItemDefinition { Id = "sewing_kit", DisplayName = "Sewing Kit", Category = ItemCategory.Tool, Price = 70, Description = "For mending and light fabric crafting." });
        Add(new ItemDefinition { Id = "milking_kit", DisplayName = "Milking Kit", Category = ItemCategory.Tool, Price = 100, Description = "Essential equipment for dairy collection." });
        Add(new ItemDefinition { Id = "styling_kit", DisplayName = "Styling Kit", Category = ItemCategory.Tool, Price = 90, Description = "Grooming and styling tools for ranch hands." });
        Add(new ItemDefinition { Id = "camping_gear", DisplayName = "Camping Gear", Category = ItemCategory.Tool, Price = 150, Description = "Sturdy gear that reduces fatigue on long missions." });
        Add(new ItemDefinition { Id = "pet_frisbee", DisplayName = "Sturdy Frisbee", Category = ItemCategory.Tool, Price = 30, Description = "A durable flying disc. Pets love to chase it." });
        Add(new ItemDefinition { Id = "work_wear", DisplayName = "Ranch Work Wear", Category = ItemCategory.Tool, Price = 120, Description = "Sturdy clothing built for ranch labor." });
        Add(new ItemDefinition { Id = "travel_gear", DisplayName = "Traveler's Gear", Category = ItemCategory.Tool, Price = 130, Description = "Well-worn road gear for adventuring." });
        Add(new ItemDefinition { Id = "restraint_rope", DisplayName = "Restraint Rope", Category = ItemCategory.Tool, Price = 50, Description = "Durable rope useful around the ranch." });
        Add(new ItemDefinition { Id = "milk_storage", DisplayName = "Milk Storage Tank", Category = ItemCategory.Tool, Price = 200, Description = "Increases milk storage capacity for daily collection." });

        // Keepsakes (gifts)
        Add(new ItemDefinition { Id = "gift_ribbon", DisplayName = "Colorful Ribbon", Category = ItemCategory.Keepsake, Price = 50, Description = "A bright hair ribbon that makes a thoughtful gift." });
        Add(new ItemDefinition { Id = "gift_band", DisplayName = "Flower Hairband", Category = ItemCategory.Keepsake, Price = 45, Description = "A delicate hairband with pressed Makai flowers." });
        Add(new ItemDefinition { Id = "gift_hat", DisplayName = "Straw Hat", Category = ItemCategory.Keepsake, Price = 50, Description = "A sun-shading straw hat. A practical gift." });
        Add(new ItemDefinition { Id = "gift_charm", DisplayName = "Charm Bracelet", Category = ItemCategory.Keepsake, Price = 80, Description = "A woven bracelet with tiny bells. Brings good luck." });
        Add(new ItemDefinition { Id = "keepsake", DisplayName = "Keepsake Charm", Category = ItemCategory.Keepsake, Price = 120, Description = "A finely crafted morale-boosting charm." });
        Add(new ItemDefinition { Id = "gift_flowers", DisplayName = "Makai Bouquet", Category = ItemCategory.Keepsake, Price = 60, Description = "A carefully arranged bouquet of luminescent Makai blooms." });
        Add(new ItemDefinition { Id = "gift_scarf", DisplayName = "Wool Scarf", Category = ItemCategory.Keepsake, Price = 70, Description = "A warm, hand-knitted scarf in earthy tones." });
        Add(new ItemDefinition { Id = "gift_journal", DisplayName = "Leather Journal", Category = ItemCategory.Keepsake, Price = 55, Description = "A blank journal with quality paper, perfect for notes or sketches." });

        // Equipment
        Add(new ItemDefinition { Id = "iron_boots", DisplayName = "Iron Boots", Category = ItemCategory.Equipment, Price = 100, Description = "Reinforced boots for rough terrain.", Slot = EquipmentSlot.Feet, BonusMaxHp = 10 });
        Add(new ItemDefinition { Id = "leather_armor", DisplayName = "Leather Armor", Category = ItemCategory.Equipment, Price = 150, Description = "Tough rawhide armor for adventuring.", Slot = EquipmentSlot.Armor, BonusMaxHp = 20, BonusCombatSkill = 1 });
        Add(new ItemDefinition { Id = "ranch_hat", DisplayName = "Ranch Hat", Category = ItemCategory.Equipment, Price = 60, Description = "A wide-brimmed hat for sun protection.", Slot = EquipmentSlot.Head, BonusRanchSkill = 1 });
        Add(new ItemDefinition { Id = "lucky_amulet", DisplayName = "Lucky Amulet", Category = ItemCategory.Equipment, Price = 120, Description = "Boosts all abilities slightly.", Slot = EquipmentSlot.Accessory, BonusRanchSkill = 1, BonusCraftSkill = 1, BonusCombatSkill = 1 });
        Add(new ItemDefinition { Id = "bronze_sword", DisplayName = "Bronze Sword", Category = ItemCategory.Equipment, Price = 130, Description = "A reliable blade for patrol duty.", Slot = EquipmentSlot.Weapon, BonusCombatSkill = 2 });
        Add(new ItemDefinition { Id = "tool_belt", DisplayName = "Tool Belt", Category = ItemCategory.Equipment, Price = 80, Description = "A well-organized belt for ranch work.", Slot = EquipmentSlot.Accessory, BonusRanchSkill = 2 });
        Add(new ItemDefinition { Id = "craft_apron", DisplayName = "Craft Apron", Category = ItemCategory.Equipment, Price = 70, Description = "A sturdy apron with many pockets.", Slot = EquipmentSlot.Armor, BonusCraftSkill = 2 });
        Add(new ItemDefinition { Id = "sturdy_boots", DisplayName = "Sturdy Boots", Category = ItemCategory.Equipment, Price = 80, Description = "Comfortable work boots for long days.", Slot = EquipmentSlot.Feet, BonusMaxEnergy = 15 });
        Add(new ItemDefinition { Id = "woven_bandana", DisplayName = "Woven Bandana", Category = ItemCategory.Equipment, Price = 40, Description = "Breathable headwear for hot days.", Slot = EquipmentSlot.Head, BonusMaxEnergy = 10 });
        Add(new ItemDefinition { Id = "combat_miko_robe", DisplayName = "Combat Miko Robe", Category = ItemCategory.Equipment, Price = 200, Description = "A battle-ready shrine maiden outfit.", Slot = EquipmentSlot.Armor, BonusMaxHp = 15, BonusCombatSkill = 2 });
        Add(new ItemDefinition { Id = "combat_sister_robe", DisplayName = "Combat Sister Habit", Category = ItemCategory.Equipment, Price = 200, Description = "A battle-ready sister's habit.", Slot = EquipmentSlot.Armor, BonusMaxHp = 15, BonusCombatSkill = 1, BonusCraftSkill = 1 });
        Add(new ItemDefinition { Id = "robe", DisplayName = "Mage's Robe", Category = ItemCategory.Equipment, Price = 100, Description = "A simple but practical mage robe.", Slot = EquipmentSlot.Armor, BonusMaxEnergy = 20 });
        Add(new ItemDefinition { Id = "blazer_uniform", DisplayName = "Blazer Uniform", Category = ItemCategory.Equipment, Price = 150, Description = "A crisp school-style blazer uniform.", Slot = EquipmentSlot.Armor, BonusCraftSkill = 1 });
        Add(new ItemDefinition { Id = "cloth_clothes", DisplayName = "Cloth Clothes", Category = ItemCategory.Equipment, Price = 50, Description = "Simple everyday clothing.", Slot = EquipmentSlot.Armor });
        Add(new ItemDefinition { Id = "hairband", DisplayName = "Hairband", Category = ItemCategory.Equipment, Price = 20, Description = "A simple hairband accessory.", Slot = EquipmentSlot.Head, BonusMaxEnergy = 5 });
        Add(new ItemDefinition { Id = "ribbon", DisplayName = "Ribbon", Category = ItemCategory.Equipment, Price = 25, Description = "A decorative ribbon for the hair.", Slot = EquipmentSlot.Head, BonusMorale = 2 });
        Add(new ItemDefinition { Id = "magic_ring", DisplayName = "Magic Ring", Category = ItemCategory.Equipment, Price = 250, Description = "A ring that channels magical energy.", Slot = EquipmentSlot.Accessory, BonusMaxEnergy = 30 });
    }

    private void SeedFacilities()
    {
        Add(new FacilityDefinition { Id = "pasture", DisplayName = "Pasture", BuildCost = 180, UpkeepGold = 20, OutputResourceId = "farm_goods", OutputBonus = 3 });
        Add(new FacilityDefinition { Id = "kitchen", DisplayName = "Kitchen", BuildCost = 140, UpkeepGold = 12, OutputResourceId = "meals", OutputBonus = 1 });
        Add(new FacilityDefinition { Id = "workshop", DisplayName = "Workshop", BuildCost = 170, UpkeepGold = 16, OutputResourceId = "supplies", OutputBonus = 1 });
        Add(new FacilityDefinition { Id = "guest_room", DisplayName = "Guest Rooms", BuildCost = 120, UpkeepGold = 8, OutputResourceId = "comfort", OutputBonus = 1 });
        Add(new FacilityDefinition { Id = "well", DisplayName = "Well", BuildCost = 160, UpkeepGold = 10, OutputResourceId = "farm_goods", OutputBonus = 2 });
        Add(new FacilityDefinition { Id = "storage", DisplayName = "Storage Shed", BuildCost = 130, UpkeepGold = 6, OutputResourceId = "supplies", OutputBonus = 1 });
        // New facilities
        Add(new FacilityDefinition { Id = "dairy_barn", DisplayName = "Dairy Barn", BuildCost = 250, UpkeepGold = 25, OutputResourceId = "farm_goods", OutputBonus = 5 });
        Add(new FacilityDefinition { Id = "pharmacy_lab", DisplayName = "Pharmacy Lab", BuildCost = 300, UpkeepGold = 20, OutputResourceId = "supplies", OutputBonus = 3 });
        Add(new FacilityDefinition { Id = "bathhouse", DisplayName = "Bathhouse", BuildCost = 200, UpkeepGold = 15, OutputResourceId = "comfort", OutputBonus = 2 });
    }

    private void SeedMissions()
    {
        Add(new MissionDefinition { Id = "road_patrol", DisplayName = "Road Patrol", Tier = MissionTier.Local, Difficulty = 10, RewardGold = 80, RewardItemId = "feed_bundle", EnemyGroupId = "group_wild_local" });
        Add(new MissionDefinition { Id = "field_clear", DisplayName = "Field Clear", Tier = MissionTier.Local, Difficulty = 8, RewardGold = 60, RewardItemId = "feed_bundle", EnemyGroupId = "group_wild_local" });
        Add(new MissionDefinition { Id = "beast_hunt", DisplayName = "Beast Hunt", Tier = MissionTier.Local, Difficulty = 12, RewardGold = 90, RewardItemId = "pet_jerky", EnemyGroupId = "group_beast_local" });
        Add(new MissionDefinition { Id = "forest_survey", DisplayName = "Forest Survey", Tier = MissionTier.Regional, Difficulty = 16, RewardGold = 130, RewardItemId = "tool_kit", EnemyGroupId = "group_forest_regional" });
        Add(new MissionDefinition { Id = "trade_escort", DisplayName = "Trade Escort", Tier = MissionTier.Regional, Difficulty = 18, RewardGold = 150, RewardItemId = "camping_gear", EnemyGroupId = "group_bandit_regional" });
        Add(new MissionDefinition { Id = "ruin_delve", DisplayName = "Ruin Delve", Tier = MissionTier.Regional, Difficulty = 22, RewardGold = 200, RewardItemId = "travel_gear", EnemyGroupId = "group_ruin_regional" });
        Add(new MissionDefinition { Id = "dragon_outcrop", DisplayName = "Dragon Outcrop", Tier = MissionTier.Dangerous, Difficulty = 30, RewardGold = 350, RewardItemId = "keepsake", EnemyGroupId = "group_dragon_dangerous" });
        // New missions
        Add(new MissionDefinition { Id = "bandit_supply", DisplayName = "Bandit Supply Raid", Tier = MissionTier.Local, Difficulty = 14, RewardGold = 100, RewardItemId = "feed_bundle", EnemyGroupId = "group_bandit_regional" });
        Add(new MissionDefinition { Id = "mountain_pass", DisplayName = "Mountain Pass Survey", Tier = MissionTier.Regional, Difficulty = 20, RewardGold = 170, RewardItemId = "herb_pack", EnemyGroupId = "group_forest_regional" });
        Add(new MissionDefinition { Id = "moonlight_grove", DisplayName = "Moonlight Grove", Tier = MissionTier.Regional, Difficulty = 24, RewardGold = 220, RewardItemId = "magic_crystal", EnemyGroupId = "group_ruin_regional" });
        Add(new MissionDefinition { Id = "abyssal_cavern", DisplayName = "Abyssal Cavern", Tier = MissionTier.Dangerous, Difficulty = 35, RewardGold = 400, RewardItemId = "magic_ring", EnemyGroupId = "group_dragon_dangerous" });
        Add(new MissionDefinition { Id = "demon_tower", DisplayName = "Demon Tower Approach", Tier = MissionTier.Dangerous, Difficulty = 40, RewardGold = 500, RewardItemId = "lucky_amulet", EnemyGroupId = "group_dragon_dangerous" });
    }

    private void SeedEnemies()
    {
        Add(new EnemyDefinition { Id = "wild_slime", DisplayName = "Makai Slime", GroupId = "group_wild_local", Tier = MissionTier.Local, BaseHp = 30, BaseSp = 10, Attack = 6, Defense = 3, Speed = 3, RewardGold = 15, CaptureDifficulty = 20 });
        Add(new EnemyDefinition { Id = "wild_goblin", DisplayName = "Goblin Scout", GroupId = "group_wild_local", Tier = MissionTier.Local, BaseHp = 40, BaseSp = 15, Attack = 8, Defense = 4, Speed = 5, RewardGold = 20, CaptureDifficulty = 25 });
        Add(new EnemyDefinition { Id = "beast_wolf", DisplayName = "Fang Wolf", GroupId = "group_beast_local", Tier = MissionTier.Local, BaseHp = 50, BaseSp = 15, Attack = 10, Defense = 5, Speed = 6, RewardGold = 25, CaptureDifficulty = 28 });
        Add(new EnemyDefinition { Id = "forest_treant", DisplayName = "Corrupted Treant", GroupId = "group_forest_regional", Tier = MissionTier.Regional, BaseHp = 70, BaseSp = 25, Attack = 12, Defense = 8, Speed = 3, RewardGold = 35, CaptureDifficulty = 35 });
        Add(new EnemyDefinition { Id = "forest_sprite", DisplayName = "Wisp Sprite", GroupId = "group_forest_regional", Tier = MissionTier.Regional, BaseHp = 45, BaseSp = 40, Attack = 14, Defense = 3, Speed = 8, RewardGold = 30, CaptureDifficulty = 32 });
        Add(new EnemyDefinition { Id = "bandit_raider", DisplayName = "Bandit Raider", GroupId = "group_bandit_regional", Tier = MissionTier.Regional, BaseHp = 60, BaseSp = 20, Attack = 13, Defense = 6, Speed = 6, RewardGold = 35, CaptureDifficulty = 38 });
        Add(new EnemyDefinition { Id = "bandit_mage", DisplayName = "Rogue Mage", GroupId = "group_bandit_regional", Tier = MissionTier.Regional, BaseHp = 50, BaseSp = 50, Attack = 16, Defense = 4, Speed = 7, RewardGold = 40, CaptureDifficulty = 40 });
        Add(new EnemyDefinition { Id = "ruin_golem", DisplayName = "Ancient Golem", GroupId = "group_ruin_regional", Tier = MissionTier.Regional, BaseHp = 90, BaseSp = 10, Attack = 15, Defense = 12, Speed = 2, RewardGold = 45, CaptureDifficulty = 42 });
        Add(new EnemyDefinition { Id = "ruin_specter", DisplayName = "Wailing Specter", GroupId = "group_ruin_regional", Tier = MissionTier.Regional, BaseHp = 55, BaseSp = 60, Attack = 18, Defense = 5, Speed = 9, RewardGold = 40, CaptureDifficulty = 38 });
        Add(new EnemyDefinition { Id = "dragon_whelp", DisplayName = "Flame Whelp", GroupId = "group_dragon_dangerous", Tier = MissionTier.Dangerous, BaseHp = 120, BaseSp = 50, Attack = 22, Defense = 10, Speed = 7, RewardGold = 70, CaptureDifficulty = 55 });
        Add(new EnemyDefinition { Id = "dragon_matron", DisplayName = "Elder Drake", GroupId = "group_dragon_dangerous", Tier = MissionTier.Dangerous, BaseHp = 160, BaseSp = 80, Attack = 28, Defense = 14, Speed = 6, RewardGold = 90, CaptureDifficulty = 65 });
        // New enemies
        Add(new EnemyDefinition { Id = "bandit_leader", DisplayName = "Bandit Leader", GroupId = "group_bandit_regional", Tier = MissionTier.Regional, BaseHp = 80, BaseSp = 30, Attack = 17, Defense = 8, Speed = 7, RewardGold = 50, CaptureDifficulty = 45 });
        Add(new EnemyDefinition { Id = "shadow_wraith", DisplayName = "Shadow Wraith", GroupId = "group_ruin_regional", Tier = MissionTier.Regional, BaseHp = 60, BaseSp = 70, Attack = 20, Defense = 4, Speed = 10, RewardGold = 45, CaptureDifficulty = 44 });
        Add(new EnemyDefinition { Id = "demon_knight", DisplayName = "Demon Knight", GroupId = "group_dragon_dangerous", Tier = MissionTier.Dangerous, BaseHp = 200, BaseSp = 60, Attack = 30, Defense = 16, Speed = 8, RewardGold = 100, CaptureDifficulty = 70 });
    }

    private void SeedMilestones()
    {
        Add(new MilestoneDefinition { Id = "first_day", DisplayName = "First Settlement", TriggerKind = MilestoneTriggerKind.DayReached, TriggerAmount = 2, RewardGold = 50 });
        Add(new MilestoneDefinition { Id = "ranch_community", DisplayName = "Ranch Community", TriggerKind = MilestoneTriggerKind.DayReached, TriggerAmount = 5, RewardGold = 75 });
        Add(new MilestoneDefinition { Id = "seasoned_rancher", DisplayName = "Seasoned Rancher", TriggerKind = MilestoneTriggerKind.DayReached, TriggerAmount = 10, RewardGold = 100 });
        Add(new MilestoneDefinition { Id = "veteran_rancher", DisplayName = "Veteran Rancher", TriggerKind = MilestoneTriggerKind.DayReached, TriggerAmount = 20, RewardGold = 200 });
        Add(new MilestoneDefinition { Id = "ranch_foundation", DisplayName = "Ranch Foundation", TriggerKind = MilestoneTriggerKind.GoldReached, TriggerAmount = 500, RewardGold = 80 });
        Add(new MilestoneDefinition { Id = "steady_ranch", DisplayName = "Steady Ranch", TriggerKind = MilestoneTriggerKind.GoldReached, TriggerAmount = 750, RewardGold = 100 });
        Add(new MilestoneDefinition { Id = "modest_fortune", DisplayName = "Modest Fortune", TriggerKind = MilestoneTriggerKind.GoldReached, TriggerAmount = 2000, RewardGold = 150 });
        Add(new MilestoneDefinition { Id = "ranch_empire", DisplayName = "Ranch Empire", TriggerKind = MilestoneTriggerKind.GoldReached, TriggerAmount = 5000, RewardGold = 300 });
        Add(new MilestoneDefinition { Id = "first_patrol", DisplayName = "First Patrol", TriggerKind = MilestoneTriggerKind.MissionCompleted, TriggerId = "any", RewardGold = 75 });
        Add(new MilestoneDefinition { Id = "mission_veteran", DisplayName = "Mission Veteran", TriggerKind = MilestoneTriggerKind.MissionCompleted, TriggerId = "any", RewardGold = 200 });
        Add(new MilestoneDefinition { Id = "first_trust", DisplayName = "First Trust", TriggerKind = MilestoneTriggerKind.BondReached, TriggerAmount = 20, RewardGold = 60 });
        Add(new MilestoneDefinition { Id = "deep_bonds", DisplayName = "Deep Bonds", TriggerKind = MilestoneTriggerKind.BondReached, TriggerAmount = 40, RewardGold = 100 });
        Add(new MilestoneDefinition { Id = "first_research", DisplayName = "First Research", TriggerKind = MilestoneTriggerKind.ResearchUnlocked, TriggerId = "any", RewardGold = 70 });
        Add(new MilestoneDefinition { Id = "research_master", DisplayName = "Research Master", TriggerKind = MilestoneTriggerKind.ResearchUnlocked, TriggerId = "any", RewardGold = 200 });
        Add(new MilestoneDefinition { Id = "all_hired", DisplayName = "Full Roster", TriggerKind = MilestoneTriggerKind.CharacterCount, TriggerAmount = 11, RewardGold = 250 });
        Add(new MilestoneDefinition { Id = "facility_master", DisplayName = "Facility Master", TriggerKind = MilestoneTriggerKind.FacilityMaster, RewardGold = 150 });
        Add(new MilestoneDefinition { Id = "pet_lover", DisplayName = "Pet Lover", TriggerKind = MilestoneTriggerKind.PetCount, TriggerAmount = 3, RewardGold = 80 });
        Add(new MilestoneDefinition { Id = "equipment_collector", DisplayName = "Well Equipped", TriggerKind = MilestoneTriggerKind.EquipmentCount, TriggerAmount = 5, RewardGold = 180 });
    }

    private void SeedSkills()
    {
        Add(new SkillDefinition { Id = "ranch_planning", DisplayName = "Ranch Planning", Description = "Improves job output and schedule efficiency.", CostResourceId = "supplies", CostAmount = 3 });
        Add(new SkillDefinition { Id = "field_medicine", DisplayName = "Field Medicine", Description = "Reduces adventure fatigue risk.", CostResourceId = "meals", CostAmount = 4 });
        Add(new SkillDefinition { Id = "ranch_automation", DisplayName = "Automated Feeding", Description = "Facilities produce bonus resources each day.", CostResourceId = "supplies", CostAmount = 5 });
        Add(new SkillDefinition { Id = "adventure_training", DisplayName = "Adventure Training", Description = "Improves party combat performance on missions.", CostResourceId = "meals", CostAmount = 3 });
        // New skills from original
        Add(new SkillDefinition { Id = "dairy_science", DisplayName = "Dairy Science", Description = "Increases dairy output and milk quality.", CostResourceId = "supplies", CostAmount = 4 });
        Add(new SkillDefinition { Id = "culinary_arts", DisplayName = "Culinary Arts", Description = "Improves meal quality and cooking output.", CostResourceId = "meals", CostAmount = 4 });
        Add(new SkillDefinition { Id = "herbalism", DisplayName = "Herbalism", Description = "Improves pharmacy and potion crafting.", CostResourceId = "supplies", CostAmount = 3 });
        Add(new SkillDefinition { Id = "hospitality", DisplayName = "Hospitality", Description = "Improves guest comfort and bond gains.", CostResourceId = "comfort", CostAmount = 3 });
        Add(new SkillDefinition { Id = "craftsmanship", DisplayName = "Craftsmanship", Description = "Improves workshop output and item quality.", CostResourceId = "supplies", CostAmount = 4 });
        Add(new SkillDefinition { Id = "logistics", DisplayName = "Logistics", Description = "Reduces facility upkeep costs.", CostResourceId = "supplies", CostAmount = 5 });
        Add(new SkillDefinition { Id = "tactical_training", DisplayName = "Tactical Training", Description = "All party members gain bonus combat skill in missions.", CostResourceId = "meals", CostAmount = 5 });
    }

    private void SeedPets()
    {
        Add(new PetDefinition { Id = "stable_cat", DisplayName = "Stable Cat", CareCost = 15 });
        Add(new PetDefinition { Id = "yard_hound", DisplayName = "Yard Hound", CareCost = 20 });
        // Original large pets
        Add(new PetDefinition { Id = "fallen_pegasus", DisplayName = "Fallen Angel Horse", CareCost = 60 });
        Add(new PetDefinition { Id = "orthrus", DisplayName = "Orthrus", CareCost = 50 });
        Add(new PetDefinition { Id = "demon_hamster", DisplayName = "Demon Hamster", CareCost = 30 });
    }

    private void SeedBondEvents()
    {
        Add(new BondEventDefinition { Id = "slay_morning_rounds", CharacterId = "slay", Title = "Morning Rounds", Description = "Walk the boundary fences together and turn routine patrol into a calm leadership lesson.", RequiredBond = 0, BondReward = 7, MoraleReward = 3, StockpileRewardId = "intel", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "slay_field_exercise", CharacterId = "slay", Title = "Field Exercise", Description = "Run a fast obstacle course through the pastures. Slay's competitive streak makes it lively.", RequiredBond = 12, BondReward = 8, MoraleReward = 4, StockpileRewardId = "farm_goods", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "slay_night_tea", CharacterId = "slay", Title = "Night Tea", Description = "Share a quiet cup of tea after a long day. Slay opens up about her past.", RequiredBond = 20, BondReward = 9, MoraleReward = 5, StockpileRewardId = "comfort", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "kagura_focus_drill", CharacterId = "kagura", Title = "Focus Drill", Description = "Practice measured breathing and field awareness before the evening chores begin.", RequiredBond = 8, BondReward = 8, MoraleReward = 2, StockpileRewardId = "supplies", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "kagura_technique_share", CharacterId = "kagura", Title = "Technique Share", Description = "Kagura demonstrates a precise combat maneuver.", RequiredBond = 16, BondReward = 9, MoraleReward = 3, StockpileRewardId = "intel", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "kagura_prayer", CharacterId = "kagura", Title = "Evening Prayer", Description = "Kagura performs a quiet ritual and invites you to join. The flames dance in the twilight.", RequiredBond = 24, BondReward = 10, MoraleReward = 5, StockpileRewardId = "comfort", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "maria_recipe_notes", CharacterId = "maria", Title = "Recipe Notes", Description = "Review meal planning and preserve a few practical kitchen tricks for the whole ranch.", RequiredBond = 0, BondReward = 6, MoraleReward = 5, StockpileRewardId = "meals", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "maria_preservation", CharacterId = "maria", Title = "Preservation Trial", Description = "Test new pickling and drying methods for the ranch's surplus ingredients.", RequiredBond = 12, BondReward = 7, MoraleReward = 3, StockpileRewardId = "meals", StockpileRewardAmount = 2 });
        Add(new BondEventDefinition { Id = "maria_faith_discussion", CharacterId = "maria", Title = "Faith and Duty", Description = "Maria speaks about her faith and how it guides her sense of duty. A rare vulnerable moment.", RequiredBond = 22, BondReward = 9, MoraleReward = 6, StockpileRewardId = "comfort", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "sharon_guest_care", CharacterId = "sharon", Title = "Guest Care", Description = "Prepare the guest rooms and discuss what makes the ranch feel safe and welcoming.", RequiredBond = 6, BondReward = 7, MoraleReward = 4, StockpileRewardId = "comfort", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "sharon_evening_story", CharacterId = "sharon", Title = "Evening Story", Description = "Share a quiet cup of tea by the fire while Sharon tells a folk tale from her homeland.", RequiredBond = 14, BondReward = 8, MoraleReward = 5, StockpileRewardId = "comfort", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "sharon_herb_garden", CharacterId = "sharon", Title = "Herb Garden", Description = "Sharon tends to the medicinal herbs she's been cultivating in a quiet corner of the ranch.", RequiredBond = 20, BondReward = 9, MoraleReward = 4, StockpileRewardId = "supplies", StockpileRewardAmount = 2 });
        Add(new BondEventDefinition { Id = "noir_quiet_inventory", CharacterId = "noir", Title = "Quiet Inventory", Description = "Sort supplies in companionable silence and notice what the ranch is short on.", RequiredBond = 6, BondReward = 7, MoraleReward = 3, StockpileRewardId = "supplies", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "noir_night_watch", CharacterId = "noir", Title = "Night Watch", Description = "Keep watch together under the Makai stars. Noir speaks more freely in the dark.", RequiredBond = 14, BondReward = 8, MoraleReward = 4, StockpileRewardId = "intel", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "noir_magic_discussion", CharacterId = "noir", Title = "Magic Discussion", Description = "Noir enthusiastically explains advanced magical theory. She's surprisingly patient when teaching.", RequiredBond = 22, BondReward = 9, MoraleReward = 5, StockpileRewardId = "supplies", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "ayaka_tea_reading", CharacterId = "ayaka", Title = "Tea and Reading", Description = "Share a quiet afternoon with tea and a well-worn book, discussing the ranch's history.", RequiredBond = 0, BondReward = 7, MoraleReward = 5, StockpileRewardId = "meals", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "ayaka_music_evening", CharacterId = "ayaka", Title = "Music Evening", Description = "Ayaka plays a haunting melody on a borrowed instrument.", RequiredBond = 10, BondReward = 8, MoraleReward = 6, StockpileRewardId = "comfort", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "ayaka_research", CharacterId = "ayaka", Title = "Research Collaboration", Description = "Ayaka's exorcist knowledge proves useful for understanding old ranch records.", RequiredBond = 18, BondReward = 9, MoraleReward = 4, StockpileRewardId = "supplies", StockpileRewardAmount = 2 });
        Add(new BondEventDefinition { Id = "en_garden_tending", CharacterId = "en", Title = "Garden Tending", Description = "Work the herb garden together, sharing stories of plants that thrive under gentle care.", RequiredBond = 0, BondReward = 6, MoraleReward = 4, StockpileRewardId = "farm_goods", StockpileRewardAmount = 2 });
        Add(new BondEventDefinition { Id = "en_harvest_prep", CharacterId = "en", Title = "Harvest Prep", Description = "Plan the upcoming harvest rotation. En's steady optimism makes even heavy work feel light.", RequiredBond = 10, BondReward = 7, MoraleReward = 4, StockpileRewardId = "farm_goods", StockpileRewardAmount = 2 });
        Add(new BondEventDefinition { Id = "en_cooking_lesson", CharacterId = "en", Title = "Cooking Lesson", Description = "En teaches a traditional dish from her homeland. The kitchen fills with warmth and laughter.", RequiredBond = 18, BondReward = 8, MoraleReward = 6, StockpileRewardId = "meals", StockpileRewardAmount = 2 });
        Add(new BondEventDefinition { Id = "yukina_sparring", CharacterId = "yukina", Title = "Morning Sparring", Description = "A brisk training bout that sharpens instincts and builds mutual respect through combat.", RequiredBond = 4, BondReward = 8, MoraleReward = 2, StockpileRewardId = "supplies", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "yukina_boundary_patrol", CharacterId = "yukina", Title = "Boundary Patrol", Description = "Walk the ranch perimeter with Yukina, who points out every weak spot in the fence line.", RequiredBond = 12, BondReward = 9, MoraleReward = 3, StockpileRewardId = "intel", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "yukina_hunt_training", CharacterId = "yukina", Title = "Hunt Training", Description = "Yukina's instincts come alive in the field. Watch and learn from a natural predator's grace.", RequiredBond = 20, BondReward = 10, MoraleReward = 4, StockpileRewardId = "farm_goods", StockpileRewardAmount = 2 });
        Add(new BondEventDefinition { Id = "anon_explore", CharacterId = "anon", Title = "Explore the Grounds", Description = "Wander the ranch's outer edges together, discovering hidden nooks and sharing curiosity.", RequiredBond = 0, BondReward = 6, MoraleReward = 5, StockpileRewardId = "intel", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "anon_tool_tinkering", CharacterId = "anon", Title = "Tool Tinkering", Description = "Anon has a half-built gadget and needs a second pair of hands. The result might even be useful.", RequiredBond = 8, BondReward = 7, MoraleReward = 4, StockpileRewardId = "supplies", StockpileRewardAmount = 1 });
        Add(new BondEventDefinition { Id = "anon_stargazing", CharacterId = "anon", Title = "Stargazing", Description = "Lie on the ranch roof and name constellations. Anon's playful commentary makes the night memorable.", RequiredBond = 16, BondReward = 8, MoraleReward = 6, StockpileRewardId = "comfort", StockpileRewardAmount = 1 });
    }

    private void Add(CharacterDefinition definition) => Characters[definition.Id] = definition;
    private void Add(JobDefinition definition) => Jobs[definition.Id] = definition;
    private void Add(ItemDefinition definition) => Items[definition.Id] = definition;
    private void Add(FacilityDefinition definition) => Facilities[definition.Id] = definition;
    private void Add(MissionDefinition definition) => Missions[definition.Id] = definition;
    private void Add(MilestoneDefinition definition) => Milestones[definition.Id] = definition;
    private void Add(SkillDefinition definition) => Skills[definition.Id] = definition;
    private void Add(PetDefinition definition) => Pets[definition.Id] = definition;
    private void Add(EnemyDefinition definition) => Enemies[definition.Id] = definition;
    private void Add(BondEventDefinition definition) => BondEvents[definition.Id] = definition;
}
