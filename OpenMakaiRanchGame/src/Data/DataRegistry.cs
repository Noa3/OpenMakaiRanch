using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Tools;

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
    public Dictionary<string, TalentDefinition> Talents { get; } = new();
    public Dictionary<string, BondEventDefinition> BondEvents { get; } = new();
    public Dictionary<string, EnemyDefinition> Enemies { get; } = new();
    public Dictionary<string, TrainingActionDefinition> TrainingActions { get; } = new();

	public static DataRegistry CreateSeeded()
	{
		var registry = new DataRegistry();
		if (registry.TryLoadDatabase())
		{
			return registry;
		}

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
		registry.SeedTalents();
		registry.SeedTrainingActions();
		return registry;
	}

	private bool TryLoadDatabase()
	{
		var dataDir = ProjectSettings.GlobalizePath("res://data");
		if (!Directory.Exists(dataDir))
			return false;

		var opts = new JsonSerializerOptions
		{
			Converters = { new ResourceJsonConverter() }
		};

		try
		{
			var loaded = 0;
			loaded += TryLoadJson<CharacterDefinition>(dataDir, "characters.json", opts, c => Add(c));
			loaded += TryLoadJson<JobDefinition>(dataDir, "jobs.json", opts, c => Add(c));
			loaded += TryLoadJson<ItemDefinition>(dataDir, "items.json", opts, c => Add(c));
			loaded += TryLoadJson<FacilityDefinition>(dataDir, "facilities.json", opts, c => Add(c));
			loaded += TryLoadJson<MissionDefinition>(dataDir, "missions.json", opts, c => Add(c));
			loaded += TryLoadJson<EnemyDefinition>(dataDir, "enemies.json", opts, c => Add(c));
			loaded += TryLoadJson<MilestoneDefinition>(dataDir, "milestones.json", opts, c => Add(c));
			loaded += TryLoadJson<SkillDefinition>(dataDir, "skills.json", opts, c => Add(c));
			loaded += TryLoadJson<PetDefinition>(dataDir, "pets.json", opts, c => Add(c));
			loaded += TryLoadJson<BondEventDefinition>(dataDir, "bond_events.json", opts, c => Add(c));
			loaded += TryLoadJson<TalentDefinition>(dataDir, "talents.json", opts, c => Add(c));
		loaded += TryLoadJson<TrainingActionDefinition>(dataDir, "training_actions.json", opts, c => Add(c));

			if (Characters.Count > 0)
			{
				GD.Print($"DataRegistry loaded {loaded} entries from res://data/ JSON files ({Characters.Count} characters, {Jobs.Count} jobs, {Items.Count} items)");
				return true;
			}

			return false;
		}
		catch (System.Exception exception)
		{
			GD.PushWarning($"DataRegistry could not load from res://data/: {exception.Message}. Falling back to seed methods.");
			return false;
		}
	}

	private static int TryLoadJson<T>(string dataDir, string fileName, JsonSerializerOptions opts, System.Action<T> add) where T : Resource
	{
		var path = Path.Combine(dataDir, fileName);
		if (!File.Exists(path))
			return 0;

		var json = File.ReadAllText(path);
		var items = JsonSerializer.Deserialize<List<T>>(json, opts);
		if (items is null)
			return 0;

		foreach (var item in items)
			add(item);
		return items.Count;
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
		Add(new JobDefinition { Id = "dairy", DisplayName = "Dairy Work", Category = JobCategory.Dairy, ResourceId = "farm_goods", ResourceAmount = 6, GoldIncome = 40, FatigueDelta = 14, MoraleDelta = 0, Assignable = true });
		Add(new JobDefinition { Id = "office", DisplayName = "Office Work", Category = JobCategory.Office, ResourceId = "supplies", ResourceAmount = 4, GoldIncome = 30, FatigueDelta = 6, MoraleDelta = 2, Assignable = true });
		Add(new JobDefinition { Id = "cleaning", DisplayName = "Cleaning", Category = JobCategory.Cleaning, ResourceId = "comfort", ResourceAmount = 3, GoldIncome = 15, FatigueDelta = 10, MoraleDelta = 1, Assignable = true });
		Add(new JobDefinition { Id = "cooking", DisplayName = "Cooking", Category = JobCategory.Cooking, ResourceId = "meals", ResourceAmount = 5, GoldIncome = 25, FatigueDelta = 10, MoraleDelta = 3, Assignable = true });
		Add(new JobDefinition { Id = "pharmacy", DisplayName = "Pharmacy", Category = JobCategory.Pharmacy, ResourceId = "supplies", ResourceAmount = 3, GoldIncome = 35, FatigueDelta = 8, MoraleDelta = 1, Assignable = true });
		Add(new JobDefinition { Id = "customer_service", DisplayName = "Customer Service", Category = JobCategory.CustomerService, ResourceId = "comfort", ResourceAmount = 2, GoldIncome = 40, FatigueDelta = 12, MoraleDelta = 2, Assignable = true });
	}

	private void SeedItems()
	{
		Add(new ItemDefinition { Id = "energy_drink", DisplayName = "Energy Drink", Category = ItemCategory.Consumable, Price = 40, Description = "Restores energy and sharpens focus. (Orig: 300g nutrient drink)" });
		Add(new ItemDefinition { Id = "herb_tea", DisplayName = "Herbal Tea", Category = ItemCategory.Consumable, Price = 35, Description = "Soothing Makai herb tea that lifts spirits. (Orig: 500g herb tea)" });
		Add(new ItemDefinition { Id = "first_aid", DisplayName = "First Aid Kit", Category = ItemCategory.Consumable, Price = 60, Description = "Bandages and salves for treating injuries." });
		Add(new ItemDefinition { Id = "meal_box", DisplayName = "Meal Box", Category = ItemCategory.Consumable, Price = 30, Description = "A packed meal that helps with recovery." });
		Add(new ItemDefinition { Id = "pet_jerky", DisplayName = "Mystery Jerky", Category = ItemCategory.Consumable, Price = 20, Description = "Dried meat strips. Ranch pets love these. (Orig: 500g mystery jerky)" });
		Add(new ItemDefinition { Id = "pet_seeds", DisplayName = "Sunflower Seeds", Category = ItemCategory.Consumable, Price = 15, Description = "Roasted hell sunflower seeds. A favorite pet treat. (Orig: 500g sunflower seeds)" });
		Add(new ItemDefinition { Id = "lotion", DisplayName = "Lotion", Category = ItemCategory.Consumable, Price = 25, Description = "Smoothing lotion for massage and recovery. (Orig: 200g lotion)" });
		Add(new ItemDefinition { Id = "lube", DisplayName = "Lubricant", Category = ItemCategory.Consumable, Price = 30, Description = "General-purpose lubricant for equipment maintenance. (Orig: 300g condom)" });
		Add(new ItemDefinition { Id = "hair_dye", DisplayName = "Hair Color Treatment", Category = ItemCategory.Consumable, Price = 100, Description = "Changes hair color permanently. (Orig: 10000g hair color)" });
		Add(new ItemDefinition { Id = "collar_tag", DisplayName = "Livestock Management Tag", Category = ItemCategory.Consumable, Price = 50, Description = "For identifying and cataloging ranch residents. (Orig: 500g management tag)" });
		Add(new ItemDefinition { Id = "guts_carrot", DisplayName = "Guts Carrot", Category = ItemCategory.Consumable, Price = 25, Description = "A crunchy Makai carrot that boosts stamina. (Orig: 500g guts carrot)" });
		Add(new ItemDefinition { Id = "milk_tea", DisplayName = "Milk Tea Mix", Category = ItemCategory.Consumable, Price = 20, Description = "A sweet milk tea powder. A comforting treat." });
		Add(new ItemDefinition { Id = "protein_bar", DisplayName = "Protein Bar", Category = ItemCategory.Consumable, Price = 35, Description = "Nutritious compressed energy bar for active ranch hands." });
		Add(new ItemDefinition { Id = "bandage", DisplayName = "Bandage Pack", Category = ItemCategory.Consumable, Price = 25, Description = "Clean bandages for minor injuries and fatigue recovery." });
		Add(new ItemDefinition { Id = "tonic", DisplayName = "General Tonic", Category = ItemCategory.Consumable, Price = 45, Description = "A general health tonic that promotes recovery." });
		Add(new ItemDefinition { Id = "fabric_patch", DisplayName = "Fabric Patch Kit", Category = ItemCategory.Material, Price = 50, Description = "Repairs torn clothing and gear around the ranch. (Orig: 1000g fabric patch)" });
		Add(new ItemDefinition { Id = "premium_feed", DisplayName = "Premium Feed", Category = ItemCategory.Material, Price = 60, Description = "Nutritious feed blend that boosts ranch output." });
		Add(new ItemDefinition { Id = "feed_bundle", DisplayName = "Feed Bundle", Category = ItemCategory.Material, Price = 25, Description = "Supplies for ranch facilities and livestock." });
		Add(new ItemDefinition { Id = "herb_pack", DisplayName = "Dried Herb Pack", Category = ItemCategory.Material, Price = 40, Description = "A bundle of dried medicinal herbs for remedies." });
		Add(new ItemDefinition { Id = "leather_scrap", DisplayName = "Leather Scraps", Category = ItemCategory.Material, Price = 35, Description = "Leftover leather suitable for small repairs and crafts." });
		Add(new ItemDefinition { Id = "magic_crystal", DisplayName = "Magic Crystal Shard", Category = ItemCategory.Material, Price = 80, Description = "A faintly glowing crystal that stores ambient energy." });
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
		Add(new ItemDefinition { Id = "gift_ribbon", DisplayName = "Colorful Ribbon", Category = ItemCategory.Keepsake, Price = 50, Description = "A bright hair ribbon that makes a thoughtful gift." });
		Add(new ItemDefinition { Id = "gift_band", DisplayName = "Flower Hairband", Category = ItemCategory.Keepsake, Price = 45, Description = "A delicate hairband with pressed Makai flowers." });
		Add(new ItemDefinition { Id = "gift_hat", DisplayName = "Straw Hat", Category = ItemCategory.Keepsake, Price = 50, Description = "A sun-shading straw hat. A practical gift." });
		Add(new ItemDefinition { Id = "gift_charm", DisplayName = "Charm Bracelet", Category = ItemCategory.Keepsake, Price = 80, Description = "A woven bracelet with tiny bells. Brings good luck." });
		Add(new ItemDefinition { Id = "keepsake", DisplayName = "Keepsake Charm", Category = ItemCategory.Keepsake, Price = 120, Description = "A finely crafted morale-boosting charm." });
		Add(new ItemDefinition { Id = "gift_flowers", DisplayName = "Makai Bouquet", Category = ItemCategory.Keepsake, Price = 60, Description = "A carefully arranged bouquet of luminescent Makai blooms." });
		Add(new ItemDefinition { Id = "gift_scarf", DisplayName = "Wool Scarf", Category = ItemCategory.Keepsake, Price = 70, Description = "A warm, hand-knitted scarf in earthy tones." });
		Add(new ItemDefinition { Id = "gift_journal", DisplayName = "Leather Journal", Category = ItemCategory.Keepsake, Price = 55, Description = "A blank journal with quality paper, perfect for notes or sketches." });
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
		Add(new ItemDefinition { Id = "mana_root", DisplayName = "Mana Root", Category = ItemCategory.Consumable, Price = 45, Description = "A glowing root that restores 25 Energy and reduces fatigue by 10." });
		Add(new ItemDefinition { Id = "calming_incense", DisplayName = "Calming Incense", Category = ItemCategory.Consumable, Price = 50, Description = "Aromatic incense that reduces fatigue by 15 and boosts morale by 5." });
		Add(new ItemDefinition { Id = "spirit_water", DisplayName = "Spirit Water", Category = ItemCategory.Consumable, Price = 50, Description = "Purified spring water that restores 30 Energy." });
		Add(new ItemDefinition { Id = "fortitude_ring", DisplayName = "Fortitude Ring", Category = ItemCategory.Equipment, Price = 180, Description = "A ring that hardens the wearer\u0027s resolve against fatigue.", Slot = EquipmentSlot.Accessory, BonusMaxHp = 15, BonusMorale = 3 });
		Add(new ItemDefinition { Id = "leather_gloves", DisplayName = "Leather Gloves", Category = ItemCategory.Equipment, Price = 60, Description = "Sturdy gloves for handling rough materials.", Slot = EquipmentSlot.Accessory, BonusCraftSkill = 1 });
		Add(new ItemDefinition { Id = "reinforced_vest", DisplayName = "Reinforced Vest", Category = ItemCategory.Equipment, Price = 180, Description = "A padded vest offering extra protection.", Slot = EquipmentSlot.Armor, BonusCombatSkill = 2, BonusMaxHp = 25 });
		// === Clothing: Basic (1100-1199) ===
		Add(new ItemDefinition { Id = "work_clothes", DisplayName = "Work Clothes", Category = ItemCategory.Equipment, Price = 50, Description = "Simple everyday clothing for ranch work.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Workwear });
		Add(new ItemDefinition { Id = "cloth_outfit", DisplayName = "Cloth Outfit", Category = ItemCategory.Equipment, Price = 60, Description = "A basic cloth outfit for daily wear.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Casual });
		Add(new ItemDefinition { Id = "overalls", DisplayName = "Overalls", Category = ItemCategory.Equipment, Price = 70, Description = "Durable overalls with many pockets. Great for ranch work.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Workwear, BonusRanchSkill = 1 });
		Add(new ItemDefinition { Id = "white_coat", DisplayName = "White Coat", Category = ItemCategory.Equipment, Price = 80, Description = "A clean white coat. Looks professional.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Tactical });
		Add(new ItemDefinition { Id = "kitchen_apron", DisplayName = "Kitchen Apron", Category = ItemCategory.Equipment, Price = 40, Description = "A sturdy apron for cooking and cleaning.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Workwear, BonusCraftSkill = 1 });
		Add(new ItemDefinition { Id = "dress", DisplayName = "Dress", Category = ItemCategory.Equipment, Price = 90, Description = "A simple but elegant dress for special occasions.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Formal, BonusMorale = 2 });
		Add(new ItemDefinition { Id = "negligee", DisplayName = "Negligee", Category = ItemCategory.Equipment, Price = 70, Description = "A delicate nightgown made of fine fabric.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Lingerie });

		// === Clothing: Underwear (1150-1199) ===
		Add(new ItemDefinition { Id = "bra", DisplayName = "Bra", Category = ItemCategory.Equipment, Price = 30, Description = "A standard bra for everyday support.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "bandage_bra", DisplayName = "Bandage Bra", Category = ItemCategory.Equipment, Price = 20, Description = "Simple bandage wraps for binding.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "panties", DisplayName = "Panties", Category = ItemCategory.Equipment, Price = 25, Description = "Standard panties.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "drawers", DisplayName = "Drawers", Category = ItemCategory.Equipment, Price = 25, Description = "Loose-fitting drawers for comfort.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "trunks", DisplayName = "Trunks", Category = ItemCategory.Equipment, Price = 25, Description = "Athletic-style trunks.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Default });

		// === Clothing: Special Underwear (1160-1199) ===
		Add(new ItemDefinition { Id = "front_bra", DisplayName = "Front-Hook Bra", Category = ItemCategory.Equipment, Price = 35, Description = "A bra with a front clasp for easy access.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "nursing_bra", DisplayName = "Nursing Slit Bra", Category = ItemCategory.Equipment, Price = 40, Description = "A bra with nursing slits for easy access.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "corset", DisplayName = "Corset", Category = ItemCategory.Equipment, Price = 50, Description = "A tight corset that shapes the torso.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "string_bra", DisplayName = "String Bra", Category = ItemCategory.Equipment, Price = 30, Description = "A minimal string bra for maximum exposure.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "string_panties", DisplayName = "String Panties", Category = ItemCategory.Equipment, Price = 30, Description = "Minimal string panties.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "bikini", DisplayName = "Bikini", Category = ItemCategory.Equipment, Price = 45, Description = "A two-piece swimsuit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Swimsuit });

		// === Clothing: Head/Accessory (1200-1299) ===
		Add(new ItemDefinition { Id = "glasses", DisplayName = "Glasses", Category = ItemCategory.Equipment, Price = 40, Description = "Corrective lenses for clear vision.", Slot = EquipmentSlot.Ears, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "sunglasses", DisplayName = "Sunglasses", Category = ItemCategory.Equipment, Price = 50, Description = "Dark lenses for sun protection.", Slot = EquipmentSlot.Ears, ClothingStyleValue = ClothingStyle.Default, BonusMorale = 1 });
		Add(new ItemDefinition { Id = "headband", DisplayName = "Headband", Category = ItemCategory.Equipment, Price = 15, Description = "A simple headband to keep hair in place.", Slot = EquipmentSlot.Head, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "choke", DisplayName = "Choker", Category = ItemCategory.Equipment, Price = 35, Description = "A tight neck accessory.", Slot = EquipmentSlot.Necklace, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "eyepatch", DisplayName = "Eyepatch", Category = ItemCategory.Equipment, Price = 30, Description = "A decorative eyepatch.", Slot = EquipmentSlot.Ears, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "gloves", DisplayName = "Gloves", Category = ItemCategory.Equipment, Price = 35, Description = "Protective gloves for hand coverage.", Slot = EquipmentSlot.Arms, ClothingStyleValue = ClothingStyle.Default, BonusCraftSkill = 1 });
		Add(new ItemDefinition { Id = "socks", DisplayName = "Socks", Category = ItemCategory.Equipment, Price = 20, Description = "Standard socks for comfort.", Slot = EquipmentSlot.Legs, ClothingStyleValue = ClothingStyle.Default });

		// === Clothing: Extended Accessories (1210-1299) ===
		Add(new ItemDefinition { Id = "cloak", DisplayName = "Cloak", Category = ItemCategory.Equipment, Price = 80, Description = "A flowing cloak for dramatic effect.", Slot = EquipmentSlot.Coat, ClothingStyleValue = ClothingStyle.Formal, BonusMaxHp = 5 });
		Add(new ItemDefinition { Id = "cape", DisplayName = "Cape", Category = ItemCategory.Equipment, Price = 70, Description = "A short cape that adds flair.", Slot = EquipmentSlot.Coat, ClothingStyleValue = ClothingStyle.Formal });
		Add(new ItemDefinition { Id = "sister_veil", DisplayName = "Sister Veil", Category = ItemCategory.Equipment, Price = 45, Description = "A traditional veil worn by sisters.", Slot = EquipmentSlot.Head, ClothingStyleValue = ClothingStyle.Exorcist });
		Add(new ItemDefinition { Id = "tiara", DisplayName = "Tiara", Category = ItemCategory.Equipment, Price = 60, Description = "A small jeweled crown.", Slot = EquipmentSlot.Head, ClothingStyleValue = ClothingStyle.Formal, BonusMorale = 3 });
		Add(new ItemDefinition { Id = "maid_brim", DisplayName = "Maid Brim", Category = ItemCategory.Equipment, Price = 30, Description = "A decorative maid headpiece.", Slot = EquipmentSlot.Head, ClothingStyleValue = ClothingStyle.Maid });
		Add(new ItemDefinition { Id = "high_socks", DisplayName = "High Socks", Category = ItemCategory.Equipment, Price = 25, Description = "Knee-high socks.", Slot = EquipmentSlot.Legs, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "thigh_highs", DisplayName = "Thigh-High Socks", Category = ItemCategory.Equipment, Price = 30, Description = "Socks that reach the thigh.", Slot = EquipmentSlot.Legs, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "over_thigh", DisplayName = "Over-Thigh Socks", Category = ItemCategory.Equipment, Price = 35, Description = "Socks that reach above the thigh.", Slot = EquipmentSlot.Legs, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "stockings", DisplayName = "Fishnet Stockings", Category = ItemCategory.Equipment, Price = 40, Description = "Fishnet-patterned stockings.", Slot = EquipmentSlot.Legs, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "long_gloves", DisplayName = "Long Gloves", Category = ItemCategory.Equipment, Price = 45, Description = "Elongated gloves that reach the elbow.", Slot = EquipmentSlot.Arms, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "arm_warmers", DisplayName = "Arm Warmers", Category = ItemCategory.Equipment, Price = 30, Description = "Sleeveless arm coverings.", Slot = EquipmentSlot.Arms, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "leg_warmers", DisplayName = "Leg Warmers", Category = ItemCategory.Equipment, Price = 25, Description = "Soft leg coverings for warmth.", Slot = EquipmentSlot.Legs, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "wing_accessory", DisplayName = "Wing Accessory", Category = ItemCategory.Equipment, Price = 55, Description = "Decorative wings worn on the back.", Slot = EquipmentSlot.Coat, ClothingStyleValue = ClothingStyle.Default });

		// === Clothing: Maid Series (1300-1399) ===
		Add(new ItemDefinition { Id = "maid_set", DisplayName = "Maid Set", Category = ItemCategory.Equipment, Price = 120, Description = "A complete maid outfit with apron and headpiece.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Maid });
		Add(new ItemDefinition { Id = "nursing_maid", DisplayName = "Nursing Maid Outfit", Category = ItemCategory.Equipment, Price = 130, Description = "A maid outfit with nursing access points.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Maid });
		Add(new ItemDefinition { Id = "bunny_set", DisplayName = "Bunny Suit", Category = ItemCategory.Equipment, Price = 100, Description = "A playful bunny-themed outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Bunny });
		Add(new ItemDefinition { Id = "reverse_bunny", DisplayName = "Reverse Bunny Suit", Category = ItemCategory.Equipment, Price = 110, Description = "A variant with back exposure.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Bunny });
		Add(new ItemDefinition { Id = "cow_girl_set", DisplayName = "Cowgirl Outfit", Category = ItemCategory.Equipment, Price = 110, Description = "A cowgirl-themed outfit with cowbell.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "micro_cowgirl", DisplayName = "Micro Cowgirl Set", Category = ItemCategory.Equipment, Price = 120, Description = "A minimal cowgirl outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "hole_cowgirl", DisplayName = "Holey Cowgirl Set", Category = ItemCategory.Equipment, Price = 125, Description = "A cowgirl outfit with strategic openings.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });

		// === Clothing: Exotic (1400-1499) ===
		Add(new ItemDefinition { Id = "slave_rags", DisplayName = "Slave Rags", Category = ItemCategory.Equipment, Price = 20, Description = "Tattered clothing for enslaved workers.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Slave });
		Add(new ItemDefinition { Id = "dancer_outfit", DisplayName = "Dancer's Outfit", Category = ItemCategory.Equipment, Price = 90, Description = "A revealing outfit for performance.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Casual });
		Add(new ItemDefinition { Id = "miko_robe_ex", DisplayName = "Exorcist Miko Robe", Category = ItemCategory.Equipment, Price = 150, Description = "A traditional shrine maiden outfit for battle.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Exorcist, BonusMaxEnergy = 15 });
		Add(new ItemDefinition { Id = "sister_robe_ex", DisplayName = "Sister Robe", Category = ItemCategory.Equipment, Price = 140, Description = "A sister's habit adapted for combat.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Exorcist, BonusMorale = 2 });
		Add(new ItemDefinition { Id = "maid_ex", DisplayName = "Maid Robe", Category = ItemCategory.Equipment, Price = 130, Description = "A formal maid robe for elegant service.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Maid });
		Add(new ItemDefinition { Id = "china_dress", DisplayName = "Qipao", Category = ItemCategory.Equipment, Price = 140, Description = "A form-fitting Chinese-style dress.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Formal });
		Add(new ItemDefinition { Id = "evening_dress", DisplayName = "Evening Dress", Category = ItemCategory.Equipment, Price = 160, Description = "A formal evening gown.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Formal, BonusMorale = 5 });

		// === Clothing: Travel/Combat (1450-1499) ===
		Add(new ItemDefinition { Id = "traveler_clothes", DisplayName = "Traveler's Clothes", Category = ItemCategory.Equipment, Price = 80, Description = "Durable clothing designed for long journeys.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Casual });
		Add(new ItemDefinition { Id = "mage_robe", DisplayName = "Mage's Robe", Category = ItemCategory.Equipment, Price = 120, Description = "A robe imbued with magical properties.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Exorcist, BonusMaxEnergy = 25 });
		Add(new ItemDefinition { Id = "noble_clothes", DisplayName = "Noble's Attire", Category = ItemCategory.Equipment, Price = 150, Description = "Fine clothing fit for nobility.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Formal, BonusMorale = 3 });
		Add(new ItemDefinition { Id = "exorcist_robe", DisplayName = "Exorcist Robe", Category = ItemCategory.Equipment, Price = 160, Description = "A robe blessed for exorcism work.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Exorcist, BonusCombatSkill = 2 });
		Add(new ItemDefinition { Id = "nun_robe", DisplayName = "Nun's Robe", Category = ItemCategory.Equipment, Price = 140, Description = "A simple nun's habit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Exorcist });
		Add(new ItemDefinition { Id = "monk_robe", DisplayName = "Monk's Robe", Category = ItemCategory.Equipment, Price = 130, Description = "A meditation robe for spiritual practice.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Exorcist });
		Add(new ItemDefinition { Id = "ninja_outfit", DisplayName = "Ninja Outfit", Category = ItemCategory.Equipment, Price = 170, Description = "A stealth-focused outfit for covert operations.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Tactical, BonusCombatSkill = 2 });
		Add(new ItemDefinition { Id = "combat_miko", DisplayName = "Combat Miko Outfit", Category = ItemCategory.Equipment, Price = 200, Description = "A battle-ready shrine maiden outfit with reinforced fabric.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Exorcist, BonusMaxHp = 15, BonusCombatSkill = 2 });
		Add(new ItemDefinition { Id = "combat_sister", DisplayName = "Combat Sister Habit", Category = ItemCategory.Equipment, Price = 200, Description = "A reinforced habit for combat.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Exorcist, BonusMaxHp = 15, BonusCombatSkill = 1, BonusCraftSkill = 1 });
		Add(new ItemDefinition { Id = "combat_maid", DisplayName = "Combat Maid Outfit", Category = ItemCategory.Equipment, Price = 180, Description = "A maid outfit reinforced for combat.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Maid, BonusCraftSkill = 2 });
		Add(new ItemDefinition { Id = "combat_china", DisplayName = "Combat Qipao", Category = ItemCategory.Equipment, Price = 190, Description = "A qipao modified for combat mobility.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Exorcist, BonusCombatSkill = 1 });
		Add(new ItemDefinition { Id = "combat_dress", DisplayName = "Combat Dress", Category = ItemCategory.Equipment, Price = 210, Description = "A formal dress reinforced with armor plating.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Formal, BonusMaxHp = 20, BonusMorale = 3 });
		Add(new ItemDefinition { Id = "heavenly_robe", DisplayName = "Heavenly Robe", Category = ItemCategory.Equipment, Price = 500, Description = "A legendary robe of celestial origin. Grants immense magical power.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Exorcist, BonusMaxEnergy = 50, BonusCombatSkill = 5, BonusMorale = 10 });

		// === Clothing: School/Swimsuit (1500-1599) ===
		Add(new ItemDefinition { Id = "gym_bloomers", DisplayName = "Gym Bloomers", Category = ItemCategory.Equipment, Price = 40, Description = "Traditional school gym shorts.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.School });
		Add(new ItemDefinition { Id = "shirt_pants", DisplayName = "Shirt & Jeans", Category = ItemCategory.Equipment, Price = 60, Description = "A casual shirt and jeans combo.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Casual });
		Add(new ItemDefinition { Id = "blouse_skirt", DisplayName = "Blouse & Skirt", Category = ItemCategory.Equipment, Price = 65, Description = "A classic school uniform.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.School });
		Add(new ItemDefinition { Id = "cooking_frock", DisplayName = "Cooking Frock", Category = ItemCategory.Equipment, Price = 55, Description = "A traditional Japanese cooking outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Workwear });
		Add(new ItemDefinition { Id = "prisoner_uniform", DisplayName = "Prisoner Uniform", Category = ItemCategory.Equipment, Price = 30, Description = "A striped prisoner's uniform.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Slave });
		Add(new ItemDefinition { Id = "gothic_lolita", DisplayName = "Gothic Lolita Outfit", Category = ItemCategory.Equipment, Price = 130, Description = "An elaborate gothic-style dress.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Formal, BonusMorale = 4 });
		Add(new ItemDefinition { Id = "diandl", DisplayName = "D'n'd Outfit", Category = ItemCategory.Equipment, Price = 110, Description = "A fantasy adventurer's outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Tactical });
		Add(new ItemDefinition { Id = "nurse_outfit", DisplayName = "Nurse Outfit", Category = ItemCategory.Equipment, Price = 80, Description = "A classic nurse uniform.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Nurse });
		Add(new ItemDefinition { Id = "bunny_suit", DisplayName = "Bunny Suit", Category = ItemCategory.Equipment, Price = 100, Description = "A sleek bunny costume.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Bunny });
		Add(new ItemDefinition { Id = "sailor_outfit", DisplayName = "Sailor Outfit", Category = ItemCategory.Equipment, Price = 55, Description = "A nautical-style school uniform.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.School });
		Add(new ItemDefinition { Id = "swimsuit_school", DisplayName = "School Swimsuit", Category = ItemCategory.Equipment, Price = 45, Description = "A classic school swimsuit (sukumizu).", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Swimsuit });
		Add(new ItemDefinition { Id = "leotard", DisplayName = "Leotard", Category = ItemCategory.Equipment, Price = 50, Description = "A form-fitting leotard for dance.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Swimsuit });
		Add(new ItemDefinition { Id = "slingshot_suit", DisplayName = "Slingshot Swimsuit", Category = ItemCategory.Equipment, Price = 55, Description = "A minimal slingshot-style swimsuit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Swimsuit });
		Add(new ItemDefinition { Id = "camisole", DisplayName = "Camisole", Category = ItemCategory.Equipment, Price = 35, Description = "A light camisole for summer.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "cat_ears", DisplayName = "Cat Ears", Category = ItemCategory.Equipment, Price = 30, Description = "Cute cat-ear headband.", Slot = EquipmentSlot.Ears, ClothingStyleValue = ClothingStyle.Bunny, BonusMorale = 2 });
		Add(new ItemDefinition { Id = "dog_ears", DisplayName = "Dog Ears", Category = ItemCategory.Equipment, Price = 30, Description = "Playful dog-ear headband.", Slot = EquipmentSlot.Ears, ClothingStyleValue = ClothingStyle.Bunny, BonusMorale = 2 });
		Add(new ItemDefinition { Id = "tail_accessory", DisplayName = "Tail Accessory", Category = ItemCategory.Equipment, Price = 25, Description = "A decorative tail attachment.", Slot = EquipmentSlot.Coat, ClothingStyleValue = ClothingStyle.Bunny, BonusMorale = 1 });
		Add(new ItemDefinition { Id = "horn_accessory", DisplayName = "Horn Accessory", Category = ItemCategory.Equipment, Price = 35, Description = "Decorative horns to wear.", Slot = EquipmentSlot.Head, ClothingStyleValue = ClothingStyle.Default });

		// === Clothing: Lingerie (1600-1699) ===
		Add(new ItemDefinition { Id = "cat_bra", DisplayName = "Cat-Ear Bra", Category = ItemCategory.Equipment, Price = 40, Description = "A bra with cat-ear details.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "sports_bra", DisplayName = "Sports Bra", Category = ItemCategory.Equipment, Price = 45, Description = "A supportive bra for physical activity.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "striped_panties", DisplayName = "Striped Panties", Category = ItemCategory.Equipment, Price = 30, Description = "Striped-pattern panties.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "t_back", DisplayName = "T-Back Panties", Category = ItemCategory.Equipment, Price = 35, Description = "A T-back style panty.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "c_string", DisplayName = "C-String Panties", Category = ItemCategory.Equipment, Price = 40, Description = "A minimal C-string design.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "camisole_lingerie", DisplayName = "Camisole Lingerie", Category = ItemCategory.Equipment, Price = 50, Description = "A delicate camisole.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "slingshot_lingerie", DisplayName = "Slingshot Lingerie", Category = ItemCategory.Equipment, Price = 55, Description = "A daring slingshot-style lingerie.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "swimsuit_school", DisplayName = "School Swimsuit", Category = ItemCategory.Equipment, Price = 45, Description = "A classic school swimsuit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Swimsuit });
		Add(new ItemDefinition { Id = "bunny_ears", DisplayName = "Bunny Ears", Category = ItemCategory.Equipment, Price = 30, Description = "Bunny-ear headband.", Slot = EquipmentSlot.Ears, ClothingStyleValue = ClothingStyle.Bunny, BonusMorale = 2 });
		Add(new ItemDefinition { Id = "bunny_tail", DisplayName = "Bunny Tail", Category = ItemCategory.Equipment, Price = 25, Description = "A fluffy bunny tail.", Slot = EquipmentSlot.Coat, ClothingStyleValue = ClothingStyle.Bunny, BonusMorale = 1 });
		Add(new ItemDefinition { Id = "tentacle_clothes", DisplayName = "Tentacle Outfit", Category = ItemCategory.Equipment, Price = 300, Description = "A living outfit woven from tentacles. Highly adaptive.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Default, BonusMaxHp = 10, BonusMaxEnergy = 10 });

		// === Potions/Drugs (200-399) ===
		Add(new ItemDefinition { Id = "milk_boost_potion", DisplayName = "Milk Secretion Potion", Category = ItemCategory.Consumable, Price = 150, Description = "Increases milk production capacity. (Orig: 母乳分泌促進薬)", EffectType = ItemEffectType.MilkCapacityIncrease, EffectValue = 20 });
		Add(new ItemDefinition { Id = "magic_milk_potion", DisplayName = "Magic Milk Potion", Category = ItemCategory.Consumable, Price = 300, Description = "Transforms milk into magical milk. (Orig: 魔力母乳分泌促進薬)", EffectType = ItemEffectType.MagicMilkConstitution, EffectValue = 1 });
		Add(new ItemDefinition { Id = "hair_color_potion", DisplayName = "Hair Color Potion", Category = ItemCategory.Consumable, Price = 10000, Description = "Permanently changes hair color. (Orig: 10000g hair color)", EffectType = ItemEffectType.HairColorChange });
		Add(new ItemDefinition { Id = "contact_lens", DisplayName = "Colored Contact Lenses", Category = ItemCategory.Consumable, Price = 500, Description = "Changes eye color temporarily. (Orig: カラーコンタクト)" });
		Add(new ItemDefinition { Id = "aphrodisiac", DisplayName = "Aphrodisiac", Category = ItemCategory.Consumable, Price = 200, Description = "Increases sensitivity and arousal. (Orig: 媚薬)", EffectType = ItemEffectType.SensitivityIncrease, EffectValue = 10 });
		Add(new ItemDefinition { Id = "energy_tonic", DisplayName = "Energy Tonic", Category = ItemCategory.Consumable, Price = 100, Description = "A powerful energy supplement. (Orig: 精力剤)", EffectType = ItemEffectType.EnergyRestore, EffectValue = 30 });
		Add(new ItemDefinition { Id = "milk_body_potion", DisplayName = "Milk Body Potion", Category = ItemCategory.Consumable, Price = 500, Description = "Transforms body constitution to produce milk. (Orig: 母乳体質化薬)", EffectType = ItemEffectType.MilkConstitution, EffectValue = 1 });
		Add(new ItemDefinition { Id = "magic_milk_body", DisplayName = "Magic Milk Body Potion", Category = ItemCategory.Consumable, Price = 800, Description = "Transforms body for magical milk production. (Orig: 魔力母乳体質化薬)", EffectType = ItemEffectType.MagicMilkConstitution, EffectValue = 1 });
		Add(new ItemDefinition { Id = "breast_growth_potion", DisplayName = "Breast Growth Potion", Category = ItemCategory.Consumable, Price = 600, Description = "Increases breast size. (Orig: 膨乳薬)", EffectType = ItemEffectType.BreastSizeIncrease, EffectValue = 1 });
		Add(new ItemDefinition { Id = "milk_thicken_potion", DisplayName = "Milk Thicken Potion", Category = ItemCategory.Consumable, Price = 400, Description = "Thickens milk concentration. (Orig: 母乳濃厚化薬)", EffectType = ItemEffectType.ConcentrationThicken, EffectValue = 1 });
		Add(new ItemDefinition { Id = "penetration_aphrodisiac", DisplayName = "Penetration Aphrodisiac", Category = ItemCategory.Consumable, Price = 350, Description = "A potent aphrodisiac with deeper effects. (Orig: 浸透媚薬)", EffectType = ItemEffectType.SensitivityIncrease, EffectValue = 15 });
		Add(new ItemDefinition { Id = "super_breast_potion", DisplayName = "Super Breast Potion", Category = ItemCategory.Consumable, Price = 900, Description = "Dramatically increases breast size. (Orig: 超乳薬)", EffectType = ItemEffectType.BreastSizeIncrease, EffectValue = 2 });
		Add(new ItemDefinition { Id = "sensitivity_potion", DisplayName = "Sensitivity Potion", Category = ItemCategory.Consumable, Price = 450, Description = "Sharpens all sensory receptors. (Orig: 受容器鋭敏化薬)", EffectType = ItemEffectType.SensitivityIncrease, EffectValue = 12 });
		Add(new ItemDefinition { Id = "multi_organ_potion", DisplayName = "Multi-Organ Activation Potion", Category = ItemCategory.Consumable, Price = 700, Description = "Activates multiple organs simultaneously. (Orig: 多臓器活性薬)", EffectType = ItemEffectType.Transformation, EffectValue = 1 });

		// === Restraint/Training Tools (500-699) ===
		Add(new ItemDefinition { Id = "vibrator", DisplayName = "Vibrator", Category = ItemCategory.Tool, Price = 150, Description = "A vibrating device for pleasure. (Orig: バイブ)" });
		Add(new ItemDefinition { Id = "anal_vibrator", DisplayName = "Anal Vibrator", Category = ItemCategory.Tool, Price = 160, Description = "A vibrator designed for anal use. (Orig: アナルバイブ)" });
		Add(new ItemDefinition { Id = "nipple_rotor", DisplayName = "Nipple Rotor", Category = ItemCategory.Tool, Price = 130, Description = "A rotor device for nipple stimulation. (Orig: 乳首ローター)" });
		Add(new ItemDefinition { Id = "clit_rotor", DisplayName = "Clit Rotor", Category = ItemCategory.Tool, Price = 130, Description = "A rotor for clitoral stimulation. (Orig: クリローター)" });
		Add(new ItemDefinition { Id = "nipple_suction", DisplayName = "Nipple Suction Device", Category = ItemCategory.Tool, Price = 140, Description = "A device that applies suction to nipples. (Orig: 乳首吸引器)" });
		Add(new ItemDefinition { Id = "clit_suction", DisplayName = "Clit Suction Device", Category = ItemCategory.Tool, Price = 140, Description = "A device that applies suction to the clitoris. (Orig: クリ吸引器)" });
		Add(new ItemDefinition { Id = "blindfold", DisplayName = "Blindfold", Category = ItemCategory.Tool, Price = 60, Description = "A soft blindfold for sensory deprivation. (Orig: アイマスク)" });
		Add(new ItemDefinition { Id = "mouth_gag", DisplayName = "Mouth Gag", Category = ItemCategory.Tool, Price = 70, Description = "A gag to silence the mouth. (Orig: 口枷)" });
		Add(new ItemDefinition { Id = "ball_gag", DisplayName = "Ball Gag", Category = ItemCategory.Tool, Price = 80, Description = "A ball-type gag. (Orig: ボールギャグ)" });
		Add(new ItemDefinition { Id = "forced_mouth", DisplayName = "Forced Mouth Opener", Category = ItemCategory.Tool, Price = 90, Description = "A device that forces the mouth open. (Orig: 強制口開け)" });
		Add(new ItemDefinition { Id = "rough_rope", DisplayName = "Rough SM Rope", Category = ItemCategory.Tool, Price = 100, Description = "Thick rope for restraint. (Orig: ＳＭ用荒縄)" });
		Add(new ItemDefinition { Id = "nipple_tags", DisplayName = "Nipple Tags", Category = ItemCategory.Tool, Price = 50, Description = "Tags attached to nipples. (Orig: 乳首札)" });
		Add(new ItemDefinition { Id = "nipple_lock", DisplayName = "Nipple Lock", Category = ItemCategory.Tool, Price = 65, Description = "A device that locks nipples in place. (Orig: 乳首固定具)" });
		Add(new ItemDefinition { Id = "hand_cuffs", DisplayName = "Hand Cuffs", Category = ItemCategory.Tool, Price = 80, Description = "Metal cuffs for restraining hands. (Orig: 手枷)" });
		Add(new ItemDefinition { Id = "suspension_chain", DisplayName = "Suspension Chain", Category = ItemCategory.Tool, Price = 120, Description = "A chain for suspension. (Orig: 吊るし鎖)" });
		Add(new ItemDefinition { Id = "milking_stand", DisplayName = "Training Milking Stand", Category = ItemCategory.Tool, Price = 250, Description = "A stand designed for milking training. (Orig: 調教用搾乳台)" });
		Add(new ItemDefinition { Id = "cross_restraint", DisplayName = "Cross Restraint Table", Category = ItemCategory.Tool, Price = 300, Description = "A cross-shaped restraint table. (Orig: 十字架拘束台)" });
		Add(new ItemDefinition { Id = "x_restraint", DisplayName = "X-Restraint Table", Category = ItemCategory.Tool, Price = 300, Description = "An X-shaped restraint table. (Orig: Ｘ字拘束台)" });
		Add(new ItemDefinition { Id = "restraint_bed", DisplayName = "Restraint Bed", Category = ItemCategory.Tool, Price = 350, Description = "A padded bed with restraints. (Orig: 拘束ベッド)" });
		Add(new ItemDefinition { Id = "suspension_harness", DisplayName = "Suspension Harness", Category = ItemCategory.Tool, Price = 180, Description = "A harness for suspension play. (Orig: 吊るしハーネス)" });
		Add(new ItemDefinition { Id = "wall_milk_restraint", DisplayName = "Wall Milk Restraint", Category = ItemCategory.Tool, Price = 280, Description = "A wall-mounted milk restraint device. (Orig: 壁乳拘束台)" });
		Add(new ItemDefinition { Id = "pommel_horse", DisplayName = "Pommel Horse", Category = ItemCategory.Tool, Price = 200, Description = "A triangular pommel device. (Orig: 三角木馬)" });
		Add(new ItemDefinition { Id = "magic_scissors", DisplayName = "Magical Shears", Category = ItemCategory.Tool, Price = 150, Description = "Shears enchanted for cutting magic barriers. (Orig: マジカル裁ちバサミ)" });

		// === Magic Items (600-699) ===
		Add(new ItemDefinition { Id = "teleport", DisplayName = "Teleport", Category = ItemCategory.Tool, Price = 500, Description = "A one-way teleportation gate. (Orig: 個人用転移門)" });
		Add(new ItemDefinition { Id = "teleport_scroll", DisplayName = "Teleport Scroll", Category = ItemCategory.Tool, Price = 200, Description = "A scroll that enables teleportation. (Orig: テレポート)" });
		Add(new ItemDefinition { Id = "energy_drain_device", DisplayName = "Energy Drain Device", Category = ItemCategory.Tool, Price = 250, Description = "Extracts energy from targets. (Orig: 搾乳エナジードレイン)" });
		Add(new ItemDefinition { Id = "milk_drain_device", DisplayName = "Milk Drain Device", Category = ItemCategory.Tool, Price = 280, Description = "Extracts concentrated milk. (Orig: 濃厚ミルクドレイン)" });
		Add(new ItemDefinition { Id = "magic_injection", DisplayName = "Magic Injection", Category = ItemCategory.Tool, Price = 300, Description = "Injects magical energy. (Orig: 魔力注入)" });
		Add(new ItemDefinition { Id = "hypnosis_device", DisplayName = "Hypnosis Device", Category = ItemCategory.Tool, Price = 350, Description = "A device for simple hypnosis. (Orig: 簡易催眠)" });
		Add(new ItemDefinition { Id = "tentacle_transform", DisplayName = "Tentacle Transformation", Category = ItemCategory.Material, Price = 1000, Description = "Transforms the body to produce tentacles. (Orig: 触手変化)" });
		Add(new ItemDefinition { Id = "brush_tentacle", DisplayName = "Brush Tentacle", Category = ItemCategory.Material, Price = 200, Description = "A soft brush-type tentacle attachment. (Orig: ブラシ触手)" });
		Add(new ItemDefinition { Id = "penis_tentacle", DisplayName = "Penis Tentacle", Category = ItemCategory.Material, Price = 250, Description = "A tentacle resembling a penis. (Orig: ペニス触手)" });
		Add(new ItemDefinition { Id = "suction_tentacle", DisplayName = "Suction Tentacle", Category = ItemCategory.Material, Price = 220, Description = "A tentacle with suction cups. (Orig: 吸引触手)" });
		Add(new ItemDefinition { Id = "massage_tentacle", DisplayName = "Massage Tentacle", Category = ItemCategory.Material, Price = 200, Description = "A tentacle designed for massage. (Orig: 揉み触手)" });
		Add(new ItemDefinition { Id = "split_tentacle", DisplayName = "Split Tentacle", Category = ItemCategory.Material, Price = 230, Description = "A tentacle that splits at the tip. (Orig: 先割れ触手)" });
		Add(new ItemDefinition { Id = "transparent_tentacle", DisplayName = "Transparent Tentacle", Category = ItemCategory.Material, Price = 240, Description = "A nearly invisible tentacle. (Orig: 半透明触手)" });
		Add(new ItemDefinition { Id = "mouth_tentacle", DisplayName = "Mouth Tentacle", Category = ItemCategory.Material, Price = 250, Description = "A tentacle with a mouth at the tip. (Orig: 口型触手)" });
		Add(new ItemDefinition { Id = "injection_tentacle", DisplayName = "Injection Tentacle", Category = ItemCategory.Material, Price = 260, Description = "A tentacle that injects substances. (Orig: 注入触手)" });
		Add(new ItemDefinition { Id = "thin_tentacle", DisplayName = "Thin Tentacle", Category = ItemCategory.Material, Price = 210, Description = "A very thin, flexible tentacle. (Orig: 極細触手)" });
		Add(new ItemDefinition { Id = "poison_venom", DisplayName = "Aphrodisiac Venom", Category = ItemCategory.Material, Price = 300, Description = "Venom with aphrodisiac properties. (Orig: 媚毒生成)" });
		Add(new ItemDefinition { Id = "milk_body_extract", DisplayName = "Milk Body Extract", Category = ItemCategory.Material, Price = 400, Description = "An extract that induces milk body constitution. (Orig: 母乳体質化エキス)" });
		Add(new ItemDefinition { Id = "breast_reform_extract", DisplayName = "Breast Reform Extract", Category = ItemCategory.Material, Price = 500, Description = "An extract that reforms breast tissue. (Orig: 膨乳改造エキス)" });
		Add(new ItemDefinition { Id = "sensitive_mucus", DisplayName = "Sensitivity Mucus", Category = ItemCategory.Material, Price = 350, Description = "A mucus that increases sensitivity. (Orig: 感度上昇粘液)" });
		Add(new ItemDefinition { Id = "tentacle_ejaculation", DisplayName = "Tentacle Ejaculation", Category = ItemCategory.Material, Price = 300, Description = "Tentacle-based fertilization fluid. (Orig: 触手射精)" });
		Add(new ItemDefinition { Id = "tentacle_fertilization", DisplayName = "Tentacle Fertilization", Category = ItemCategory.Material, Price = 500, Description = "Tentacle-based conception fluid. (Orig: 触手受胎)" });
		Add(new ItemDefinition { Id = "tentacle_equipment", DisplayName = "Tentacle Equipment Kit", Category = ItemCategory.Material, Price = 400, Description = "Materials for crafting tentacle equipment. (Orig: 触手装備作成)" });
		Add(new ItemDefinition { Id = "secretion_booster", DisplayName = "Secretion Booster", Category = ItemCategory.Material, Price = 350, Description = "Boosts secretion volume. (Orig: 触手分泌液増量)" });
		Add(new ItemDefinition { Id = "淫_mark", DisplayName = "Mark of Lust", Category = ItemCategory.Material, Price = 600, Description = "A magical mark that enhances pleasure sensitivity. (Orig: 淫紋付与)" });
		Add(new ItemDefinition { Id = "orgasm_healing_mark", DisplayName = "Orgasm Healing Mark", Category = ItemCategory.Material, Price = 500, Description = "A mark that heals at orgasm. (Orig: 絶頂体力回復淫紋)" });
		Add(new ItemDefinition { Id = "orgasm_magic_mark", DisplayName = "Orgasm Magic Mark", Category = ItemCategory.Material, Price = 500, Description = "A mark that restores magic at orgasm. (Orig: 絶頂魔力回復淫紋)" });
		Add(new ItemDefinition { Id = "pain_pleasure_convert", DisplayName = "Pain-Pleasure Converter", Category = ItemCategory.Material, Price = 700, Description = "Converts pain into pleasure. (Orig: 苦痛快楽変換)" });
		Add(new ItemDefinition { Id = "penis_transform", DisplayName = "Penis Transformation", Category = ItemCategory.Material, Price = 800, Description = "Transforms the body to produce a penis. (Orig: ペニス変化)" });
		Add(new ItemDefinition { Id = "time_compress", DisplayName = "Time Compression", Category = ItemCategory.Material, Price = 1000, Description = "Compresses time for accelerated processes. (Orig: 時間圧縮)" });
		Add(new ItemDefinition { Id = "brainwash", DisplayName = "Brainwashing", Category = ItemCategory.Material, Price = 2000, Description = "A powerful mental alteration technique. (Orig: 洗脳)" });
		Add(new ItemDefinition { Id = "体内凌辱", DisplayName = "Internal Humiliation", Category = ItemCategory.Material, Price = 900, Description = "Internal stimulation technique. (Orig: 体内凌辱)" });
		Add(new ItemDefinition { Id = "volume_increase", DisplayName = "Body Volume Increase", Category = ItemCategory.Material, Price = 600, Description = "Increases overall body capacity. (Orig: 体内容量増加)" });
		Add(new ItemDefinition { Id = "permanent_time_compress", DisplayName = "Permanent Time Compression", Category = ItemCategory.Material, Price = 1500, Description = "Permanently compresses time. (Orig: 時間圧縮永続化)" });

		// === Farming/Breeding Items (800-899) ===
		Add(new ItemDefinition { Id = "fertility_boost", DisplayName = "Fertility Boost", Category = ItemCategory.Material, Price = 400, Description = "Boosts fertility and breeding capacity. (Orig: 豊穣)" });
		Add(new ItemDefinition { Id = "rich_milk_massage", DisplayName = "Rich Milk Massage", Category = ItemCategory.Material, Price = 300, Description = "A massage technique that boosts milk production. (Orig: 濃厚母乳マッサージ)" });
		Add(new ItemDefinition { Id = "milk_tank_massage", DisplayName = "Milk Tank Massage", Category = ItemCategory.Material, Price = 350, Description = "A deep massage for maximum milk output. (Orig: ミルクタンクマッサージ)" });
		Add(new ItemDefinition { Id = "endurance_potion", DisplayName = "Endurance Potion", Category = ItemCategory.Consumable, Price = 250, Description = "Increases physical endurance. (Orig: 絶倫)" });
		Add(new ItemDefinition { Id = "hermaphrodite_potion", DisplayName = "Hermaphrodite Potion", Category = ItemCategory.Consumable, Price = 1500, Description = "Transforms the body to produce both sexes. (Orig: ふたなりちんぽ)" });
		Add(new ItemDefinition { Id = "inter_species_potion", DisplayName = "Inter-Species Breeding Potion", Category = ItemCategory.Consumable, Price = 2000, Description = "Enables breeding across species. (Orig: 異種族孕ませ)" });

		// === Milking Devices (900-999) ===
		Add(new ItemDefinition { Id = "livestock_milker", DisplayName = "Livestock Milking Machine", Category = ItemCategory.Tool, Price = 200, Description = "An automated milking device for livestock. (Orig: 家畜用搾乳器)" });
		Add(new ItemDefinition { Id = "magic_milker", DisplayName = "Magic Milking Device", Category = ItemCategory.Tool, Price = 300, Description = "A magical device that enhances milking. (Orig: 魔動快楽搾乳器)" });
		Add(new ItemDefinition { Id = "tentacle_milker", DisplayName = "Tentacle Milking Device", Category = ItemCategory.Tool, Price = 350, Description = "A device using tentacles for milking. (Orig: 触手快楽搾乳器)" });

		// === Pet Adoption Tickets (100-102) ===
		Add(new ItemDefinition { Id = "pegasus_ticket", DisplayName = "Fallen Pegasus Ticket", Category = ItemCategory.Keepsake, Price = 500, Description = "A ticket to adopt a fallen pegasus. (Orig: 堕天馬)", EffectType = ItemEffectType.PetAdopt, EffectTarget = "fallen_pegasus" });
		Add(new ItemDefinition { Id = "orthrus_ticket", DisplayName = "Orthrus Ticket", Category = ItemCategory.Keepsake, Price = 400, Description = "A ticket to adopt Orthrus. (Orig: オルトロス)", EffectType = ItemEffectType.PetAdopt, EffectTarget = "orthrus" });
		Add(new ItemDefinition { Id = "demon_hamster_ticket", DisplayName = "Demon Hamster Ticket", Category = ItemCategory.Keepsake, Price = 200, Description = "A ticket to adopt a demon hamster. (Orig: 魔界ハムスター)", EffectType = ItemEffectType.PetAdopt, EffectTarget = "demon_hamster" });

		// === New Buildings (120-129) ===
		Add(new ItemDefinition { Id = "family_bath", DisplayName = "Family Bath", Category = ItemCategory.Tool, Price = 800, Description = "A private bath for the whole ranch. (Orig: 家族風呂)" });
		Add(new ItemDefinition { Id = "public_bath", DisplayName = "Public Bathhouse", Category = ItemCategory.Tool, Price = 1200, Description = "A large bathhouse for ranch guests. (Orig: 大浴場)" });
		Add(new ItemDefinition { Id = "hot_spring", DisplayName = "Natural Hot Spring", Category = ItemCategory.Tool, Price = 2000, Description = "A natural hot spring on ranch grounds. (Orig: 天然温泉)" });
		Add(new ItemDefinition { Id = "office_expansion", DisplayName = "Office Expansion", Category = ItemCategory.Tool, Price = 600, Description = "Expands the office space. (Orig: 事務所増築)" });
		Add(new ItemDefinition { Id = "slave_dorm", DisplayName = "Slave Dormitory", Category = ItemCategory.Tool, Price = 700, Description = "A dormitory for enslaved workers. (Orig: 奴隷寮)" });
		Add(new ItemDefinition { Id = "slave_dorm_expansion", DisplayName = "Slave Dormitory Expansion", Category = ItemCategory.Tool, Price = 500, Description = "Expands the slave dormitory. (Orig: 奴隷寮増築)" });
		Add(new ItemDefinition { Id = "kitchen_system", DisplayName = "System Kitchen", Category = ItemCategory.Tool, Price = 1000, Description = "A fully equipped modern kitchen. (Orig: システムキッチン)" });
		Add(new ItemDefinition { Id = "pet_kennel", DisplayName = "Pet Kennel", Category = ItemCategory.Tool, Price = 400, Description = "A kennel for ranch pets. (Orig: ペット小屋)" });
		Add(new ItemDefinition { Id = "cow_barn", DisplayName = "Cow Barn", Category = ItemCategory.Tool, Price = 600, Description = "A barn for dairy cows. (Orig: 牛舎)" });

		// === Special Items ===
		Add(new ItemDefinition { Id = "necronomicon", DisplayName = "Necronomicon", Category = ItemCategory.Keepsake, Price = 5000, Description = "A forbidden tome of dark knowledge. (Orig: ネクロノミコン)" });
		Add(new ItemDefinition { Id = "tentacle_encyclopedia", DisplayName = "Tentacle Ecology Encyclopedia", Category = ItemCategory.Keepsake, Price = 3000, Description = "A comprehensive guide to tentacle biology. (Orig: 触手生物生態図鑑)" });
		Add(new ItemDefinition { Id = "succubus_novel", DisplayName = "Succubus Pamphlet", Category = ItemCategory.Keepsake, Price = 2000, Description = "A pamphlet distributed by succubi. (Orig: サキュバス頒布の薄い本)" });
		Add(new ItemDefinition { Id = "dragon_egg", DisplayName = "Dragon Egg", Category = ItemCategory.Keepsake, Price = 10000, Description = "A mysterious egg that may hatch something. (Orig: 龍のタマ)" });
		Add(new ItemDefinition { Id = "alchemy_table", DisplayName = "Advanced Alchemy Table", Category = ItemCategory.Tool, Price = 1500, Description = "A high-grade alchemy workstation. (Orig: 高度薬学台)" });
		Add(new ItemDefinition { Id = "magic_storage_small", DisplayName = "Personal Magic Storage", Category = ItemCategory.Tool, Price = 800, Description = "A small device for storing magic power. (Orig: 家庭用魔力貯蔵器)" });
		Add(new ItemDefinition { Id = "magic_storage_large", DisplayName = "Large Magic Storage", Category = ItemCategory.Tool, Price = 1500, Description = "A large-scale magic storage device. (Orig: 業務用魔力貯蔵器)" });
		Add(new ItemDefinition { Id = "magic_storage_huge", DisplayName = "Huge Magic Storage", Category = ItemCategory.Tool, Price = 3000, Description = "A massive magic storage facility. (Orig: 大容量魔力貯蔵器)" });
		Add(new ItemDefinition { Id = "magic_storage_mod", DisplayName = "Modified Magic Storage", Category = ItemCategory.Tool, Price = 5000, Description = "A heavily modified magic storage device. (Orig: 魔改造魔力貯蔵器)" });
		Add(new ItemDefinition { Id = "magic_supply_device", DisplayName = "Magic Supply Device", Category = ItemCategory.Tool, Price = 2000, Description = "A device that supplies magic power. (Orig: 魔力補給装置)" });
		Add(new ItemDefinition { Id = "magic_cuffs", DisplayName = "Magic Cuffs", Category = ItemCategory.Tool, Price = 600, Description = "Cuffs that suppress magical abilities. (Orig: 魔力枷)" });
		Add(new ItemDefinition { Id = "spirit_extractor", DisplayName = "Spirit Extraction Device", Category = ItemCategory.Tool, Price = 400, Description = "Extracts spiritual energy. (Orig: 霊力抽出装置)" });
		Add(new ItemDefinition { Id = "small_spirit_extractor", DisplayName = "Small Spirit Extractor", Category = ItemCategory.Tool, Price = 250, Description = "A compact spirit extraction device. (Orig: 小型霊力抽出装置)" });
		Add(new ItemDefinition { Id = "energy_drain", DisplayName = "Energy Drain", Category = ItemCategory.Material, Price = 150, Description = "Extracts energy from targets. (Orig: エナジードレイン効率)" });
		Add(new ItemDefinition { Id = "magic_absorb", DisplayName = "Magic Absorption", Category = ItemCategory.Material, Price = 200, Description = "Absorbs magic from the environment. (Orig: 魔力吸収効率)" });
		Add(new ItemDefinition { Id = "cow_bed_mat", DisplayName = "Cow Bed Mat", Category = ItemCategory.Material, Price = 50, Description = "A comfortable mat for livestock. (Orig: 牛床マット)" });
		Add(new ItemDefinition { Id = "water_filter", DisplayName = "Water Filter", Category = ItemCategory.Material, Price = 40, Description = "A filter for clean water. (Orig: 給水器フィルター)" });
		Add(new ItemDefinition { Id = "maid_collar", DisplayName = "Maid Collar", Category = ItemCategory.Equipment, Price = 70, Description = "A decorative maid collar.", Slot = EquipmentSlot.Necklace, ClothingStyleValue = ClothingStyle.Maid });
		Add(new ItemDefinition { Id = "cow_bell", DisplayName = "Cowbell", Category = ItemCategory.Equipment, Price = 30, Description = "A bell worn around the neck.", Slot = EquipmentSlot.Necklace, ClothingStyleValue = ClothingStyle.CowGirl, BonusMorale = 1 });
		Add(new ItemDefinition { Id = "cow_girl_headband", DisplayName = "Cowgirl Headband", Category = ItemCategory.Equipment, Price = 35, Description = "A headband with cow ears.", Slot = EquipmentSlot.Head, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "cow_bikini", DisplayName = "Cow Bikini", Category = ItemCategory.Equipment, Price = 80, Description = "A spotted bikini.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "cow_micro_bikini", DisplayName = "Cow Micro Bikini", Category = ItemCategory.Equipment, Price = 90, Description = "A minimal spotted bikini.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "cow_holey_bikini", DisplayName = "Cow Holey Bikini", Category = ItemCategory.Equipment, Price = 95, Description = "A spotted bikini with holes.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "cow_socks", DisplayName = "Cow Socks", Category = ItemCategory.Equipment, Price = 40, Description = "Spotted socks.", Slot = EquipmentSlot.Legs, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "cow_gloves", DisplayName = "Cow Gloves", Category = ItemCategory.Equipment, Price = 45, Description = "Spotted gloves.", Slot = EquipmentSlot.Arms, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "cow_sling", DisplayName = "Cow Slingshot", Category = ItemCategory.Equipment, Price = 60, Description = "A slingshot bikini.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "nipple_expose_bra", DisplayName = "Nipple-Expose Bra", Category = ItemCategory.Equipment, Price = 50, Description = "A bra with nipple openings.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "nippleless", DisplayName = "Nippleless", Category = ItemCategory.Equipment, Price = 40, Description = "A bra without nipple coverage.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "eyepatch_bra", DisplayName = "Eyepatch Bra", Category = ItemCategory.Equipment, Price = 55, Description = "A bra with an eyepatch design.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "exposed_panties", DisplayName = "Exposed Panties", Category = ItemCategory.Equipment, Price = 45, Description = "Panties with strategic openings.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "front_patch", DisplayName = "Front Patch", Category = ItemCategory.Equipment, Price = 30, Description = "A patch for the front.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "crotch_tag", DisplayName = "Crotch Tag", Category = ItemCategory.Equipment, Price = 35, Description = "A tag attached to the crotch.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "micro_bikini", DisplayName = "Micro Bikini", Category = ItemCategory.Equipment, Price = 60, Description = "A minimal bikini.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Swimsuit });
		Add(new ItemDefinition { Id = "garter_belt", DisplayName = "Garter Belt", Category = ItemCategory.Equipment, Price = 50, Description = "A garter belt for stockings.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "string", DisplayName = "String", Category = ItemCategory.Equipment, Price = 20, Description = "A simple string.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "leather_armor_exp", DisplayName = "Leather Armor", Category = ItemCategory.Equipment, Price = 200, Description = "Tough leather armor.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Tactical, BonusMaxHp = 25, BonusCombatSkill = 2 });
		Add(new ItemDefinition { Id = "light_armor", DisplayName = "Light Armor", Category = ItemCategory.Equipment, Price = 300, Description = "Lightweight but protective armor.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Tactical, BonusMaxHp = 30, BonusMaxEnergy = 10 });
		Add(new ItemDefinition { Id = "heavy_armor", DisplayName = "Heavy Armor", Category = ItemCategory.Equipment, Price = 400, Description = "Heavy plate armor.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Tactical, BonusMaxHp = 50, BonusCombatSkill = 3 });
		Add(new ItemDefinition { Id = "full_armor", DisplayName = "Full Armor", Category = ItemCategory.Equipment, Price = 500, Description = "Complete plate armor.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Tactical, BonusMaxHp = 60, BonusCombatSkill = 4 });
		Add(new ItemDefinition { Id = "workwear_set", DisplayName = "Workwear Set", Category = ItemCategory.Equipment, Price = 150, Description = "A complete ranch workwear set.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Workwear });
		Add(new ItemDefinition { Id = "overall_set", DisplayName = "Overall Set", Category = ItemCategory.Equipment, Price = 160, Description = "A complete overall set.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Workwear });
		Add(new ItemDefinition { Id = "miko_set", DisplayName = "Miko Set", Category = ItemCategory.Equipment, Price = 200, Description = "A complete shrine maiden outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Exorcist });
		Add(new ItemDefinition { Id = "sister_set", DisplayName = "Sister Set", Category = ItemCategory.Equipment, Price = 190, Description = "A complete sister outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Exorcist });
		Add(new ItemDefinition { Id = "cowgirl_outfit_set", DisplayName = "Cowgirl Outfit Set", Category = ItemCategory.Equipment, Price = 170, Description = "A complete cowgirl outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "micro_cowgirl_set", DisplayName = "Micro Cowgirl Set", Category = ItemCategory.Equipment, Price = 180, Description = "A minimal cowgirl outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "hole_cowgirl_set", DisplayName = "Holey Cowgirl Set", Category = ItemCategory.Equipment, Price = 185, Description = "A cowgirl outfit with strategic holes.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "maid_set_full", DisplayName = "Maid Set", Category = ItemCategory.Equipment, Price = 160, Description = "A complete maid outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Maid });
		Add(new ItemDefinition { Id = "nursing_maid_set", DisplayName = "Nursing Maid Set", Category = ItemCategory.Equipment, Price = 170, Description = "A maid outfit with nursing access.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Maid });
		Add(new ItemDefinition { Id = "bunny_suit_full", DisplayName = "Bunny Suit Full", Category = ItemCategory.Equipment, Price = 150, Description = "A complete bunny suit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Bunny });
		Add(new ItemDefinition { Id = "cowgirl_set_full", DisplayName = "Cowgirl Set", Category = ItemCategory.Equipment, Price = 160, Description = "A complete cowgirl outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "micro_cowgirl_full", DisplayName = "Micro Cowgirl Full", Category = ItemCategory.Equipment, Price = 170, Description = "A minimal cowgirl outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "hole_cowgirl_full", DisplayName = "Holey Cowgirl Full", Category = ItemCategory.Equipment, Price = 175, Description = "A cowgirl outfit with holes.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });

		// === Clothing: Lingerie (1600-1699) ===
		Add(new ItemDefinition { Id = "cat_bra_lg", DisplayName = "Cat-Ear Bra", Category = ItemCategory.Equipment, Price = 40, Description = "A bra with cat-ear details.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "sports_bra_lg", DisplayName = "Sports Bra", Category = ItemCategory.Equipment, Price = 45, Description = "A supportive bra for physical activity.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Default });
		Add(new ItemDefinition { Id = "striped_panties_lg", DisplayName = "Striped Panties", Category = ItemCategory.Equipment, Price = 30, Description = "Striped-pattern panties.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "t_back_lg", DisplayName = "T-Back Panties", Category = ItemCategory.Equipment, Price = 35, Description = "A T-back style panty.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "c_string_lg", DisplayName = "C-String Panties", Category = ItemCategory.Equipment, Price = 40, Description = "A minimal C-string design.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "camisole_lg", DisplayName = "Camisole", Category = ItemCategory.Equipment, Price = 35, Description = "A light camisole for summer.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "leotard_lg", DisplayName = "Leotard", Category = ItemCategory.Equipment, Price = 50, Description = "A form-fitting leotard for dance.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Swimsuit });
		Add(new ItemDefinition { Id = "slingshot_lg", DisplayName = "Slingshot Swimsuit", Category = ItemCategory.Equipment, Price = 55, Description = "A minimal slingshot-style swimsuit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Swimsuit });
		Add(new ItemDefinition { Id = "bunny_ears_lg", DisplayName = "Bunny Ears", Category = ItemCategory.Equipment, Price = 30, Description = "Bunny-ear headband.", Slot = EquipmentSlot.Ears, ClothingStyleValue = ClothingStyle.Bunny, BonusMorale = 2 });
		Add(new ItemDefinition { Id = "bunny_tail_lg", DisplayName = "Bunny Tail", Category = ItemCategory.Equipment, Price = 25, Description = "A fluffy bunny tail.", Slot = EquipmentSlot.Coat, ClothingStyleValue = ClothingStyle.Bunny, BonusMorale = 1 });
		Add(new ItemDefinition { Id = "tentacle_clothes", DisplayName = "Tentacle Outfit", Category = ItemCategory.Equipment, Price = 300, Description = "A living outfit woven from tentacles. Highly adaptive.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Default, BonusMaxHp = 10, BonusMaxEnergy = 10 });

		// === Potions/Drugs (200-399) ===
		Add(new ItemDefinition { Id = "milk_boost_potion", DisplayName = "Milk Secretion Potion", Category = ItemCategory.Consumable, Price = 150, Description = "Increases milk production capacity. (Orig: 母乳分泌促進薬)", EffectType = ItemEffectType.MilkCapacityIncrease, EffectValue = 20 });
		Add(new ItemDefinition { Id = "magic_milk_potion", DisplayName = "Magic Milk Potion", Category = ItemCategory.Consumable, Price = 300, Description = "Transforms milk into magical milk. (Orig: 魔力母乳分泌促進薬)", EffectType = ItemEffectType.MagicMilkConstitution, EffectValue = 1 });
		Add(new ItemDefinition { Id = "hair_color_potion", DisplayName = "Hair Color Potion", Category = ItemCategory.Consumable, Price = 10000, Description = "Permanently changes hair color. (Orig: 10000g hair color)", EffectType = ItemEffectType.HairColorChange });
		Add(new ItemDefinition { Id = "contact_lens", DisplayName = "Colored Contact Lenses", Category = ItemCategory.Consumable, Price = 500, Description = "Changes eye color temporarily. (Orig: カラーコンタクト)" });
		Add(new ItemDefinition { Id = "aphrodisiac", DisplayName = "Aphrodisiac", Category = ItemCategory.Consumable, Price = 200, Description = "Increases sensitivity and arousal. (Orig: 媚薬)", EffectType = ItemEffectType.SensitivityIncrease, EffectValue = 10 });
		Add(new ItemDefinition { Id = "energy_tonic", DisplayName = "Energy Tonic", Category = ItemCategory.Consumable, Price = 100, Description = "A powerful energy supplement. (Orig: 精力剤)", EffectType = ItemEffectType.EnergyRestore, EffectValue = 30 });
		Add(new ItemDefinition { Id = "milk_body_potion", DisplayName = "Milk Body Potion", Category = ItemCategory.Consumable, Price = 500, Description = "Transforms body constitution to produce milk. (Orig: 母乳体質化薬)", EffectType = ItemEffectType.MilkConstitution, EffectValue = 1 });
		Add(new ItemDefinition { Id = "magic_milk_body", DisplayName = "Magic Milk Body Potion", Category = ItemCategory.Consumable, Price = 800, Description = "Transforms body for magical milk production. (Orig: 魔力母乳体質化薬)", EffectType = ItemEffectType.MagicMilkConstitution, EffectValue = 1 });
		Add(new ItemDefinition { Id = "breast_growth_potion", DisplayName = "Breast Growth Potion", Category = ItemCategory.Consumable, Price = 600, Description = "Increases breast size. (Orig: 膨乳薬)", EffectType = ItemEffectType.BreastSizeIncrease, EffectValue = 1 });
		Add(new ItemDefinition { Id = "milk_thicken_potion", DisplayName = "Milk Thicken Potion", Category = ItemCategory.Consumable, Price = 400, Description = "Thickens milk concentration. (Orig: 母乳濃厚化薬)", EffectType = ItemEffectType.ConcentrationThicken, EffectValue = 1 });
		Add(new ItemDefinition { Id = "penetration_aphrodisiac", DisplayName = "Penetration Aphrodisiac", Category = ItemCategory.Consumable, Price = 350, Description = "A potent aphrodisiac with deeper effects. (Orig: 浸透媚薬)", EffectType = ItemEffectType.SensitivityIncrease, EffectValue = 15 });
		Add(new ItemDefinition { Id = "super_breast_potion", DisplayName = "Super Breast Potion", Category = ItemCategory.Consumable, Price = 900, Description = "Dramatically increases breast size. (Orig: 超乳薬)", EffectType = ItemEffectType.BreastSizeIncrease, EffectValue = 2 });
		Add(new ItemDefinition { Id = "sensitivity_potion", DisplayName = "Sensitivity Potion", Category = ItemCategory.Consumable, Price = 450, Description = "Sharpens all sensory receptors. (Orig: 受容器鋭敏化薬)", EffectType = ItemEffectType.SensitivityIncrease, EffectValue = 12 });
		Add(new ItemDefinition { Id = "multi_organ_potion", DisplayName = "Multi-Organ Activation Potion", Category = ItemCategory.Consumable, Price = 700, Description = "Activates multiple organs simultaneously. (Orig: 多臓器活性薬)", EffectType = ItemEffectType.Transformation, EffectValue = 1 });

		// === Restraint/Training Tools (500-699) ===
		Add(new ItemDefinition { Id = "vibrator", DisplayName = "Vibrator", Category = ItemCategory.Tool, Price = 150, Description = "A vibrating device for pleasure. (Orig: バイブ)" });
		Add(new ItemDefinition { Id = "anal_vibrator", DisplayName = "Anal Vibrator", Category = ItemCategory.Tool, Price = 160, Description = "A vibrator designed for anal use. (Orig: アナルバイブ)" });
		Add(new ItemDefinition { Id = "nipple_rotor", DisplayName = "Nipple Rotor", Category = ItemCategory.Tool, Price = 130, Description = "A rotor device for nipple stimulation. (Orig: 乳首ローター)" });
		Add(new ItemDefinition { Id = "clit_rotor", DisplayName = "Clit Rotor", Category = ItemCategory.Tool, Price = 130, Description = "A rotor for clitoral stimulation. (Orig: クリローター)" });
		Add(new ItemDefinition { Id = "nipple_suction", DisplayName = "Nipple Suction Device", Category = ItemCategory.Tool, Price = 140, Description = "A device that applies suction to nipples. (Orig: 乳首吸引器)" });
		Add(new ItemDefinition { Id = "clit_suction", DisplayName = "Clit Suction Device", Category = ItemCategory.Tool, Price = 140, Description = "A device that applies suction to the clitoris. (Orig: クリ吸引器)" });
		Add(new ItemDefinition { Id = "blindfold", DisplayName = "Blindfold", Category = ItemCategory.Tool, Price = 60, Description = "A soft blindfold for sensory deprivation. (Orig: アイマスク)" });
		Add(new ItemDefinition { Id = "mouth_gag", DisplayName = "Mouth Gag", Category = ItemCategory.Tool, Price = 70, Description = "A gag to silence the mouth. (Orig: 口枷)" });
		Add(new ItemDefinition { Id = "ball_gag", DisplayName = "Ball Gag", Category = ItemCategory.Tool, Price = 80, Description = "A ball-type gag. (Orig: ボールギャグ)" });
		Add(new ItemDefinition { Id = "forced_mouth", DisplayName = "Forced Mouth Opener", Category = ItemCategory.Tool, Price = 90, Description = "A device that forces the mouth open. (Orig: 強制口開け)" });
		Add(new ItemDefinition { Id = "rough_rope", DisplayName = "Rough SM Rope", Category = ItemCategory.Tool, Price = 100, Description = "Thick rope for restraint. (Orig: ＳＭ用荒縄)" });
		Add(new ItemDefinition { Id = "nipple_tags", DisplayName = "Nipple Tags", Category = ItemCategory.Tool, Price = 50, Description = "Tags attached to nipples. (Orig: 乳首札)" });
		Add(new ItemDefinition { Id = "nipple_lock", DisplayName = "Nipple Lock", Category = ItemCategory.Tool, Price = 65, Description = "A device that locks nipples in place. (Orig: 乳首固定具)" });
		Add(new ItemDefinition { Id = "hand_cuffs", DisplayName = "Hand Cuffs", Category = ItemCategory.Tool, Price = 80, Description = "Metal cuffs for restraining hands. (Orig: 手枷)" });
		Add(new ItemDefinition { Id = "suspension_chain", DisplayName = "Suspension Chain", Category = ItemCategory.Tool, Price = 120, Description = "A chain for suspension. (Orig: 吊るし鎖)" });
		Add(new ItemDefinition { Id = "milking_stand", DisplayName = "Training Milking Stand", Category = ItemCategory.Tool, Price = 250, Description = "A stand designed for milking training. (Orig: 調教用搾乳台)" });
		Add(new ItemDefinition { Id = "cross_restraint", DisplayName = "Cross Restraint Table", Category = ItemCategory.Tool, Price = 300, Description = "A cross-shaped restraint table. (Orig: 十字架拘束台)" });
		Add(new ItemDefinition { Id = "x_restraint", DisplayName = "X-Restraint Table", Category = ItemCategory.Tool, Price = 300, Description = "An X-shaped restraint table. (Orig: Ｘ字拘束台)" });
		Add(new ItemDefinition { Id = "restraint_bed", DisplayName = "Restraint Bed", Category = ItemCategory.Tool, Price = 350, Description = "A padded bed with restraints. (Orig: 拘束ベッド)" });
		Add(new ItemDefinition { Id = "suspension_harness", DisplayName = "Suspension Harness", Category = ItemCategory.Tool, Price = 180, Description = "A harness for suspension play. (Orig: 吊るしハーネス)" });
		Add(new ItemDefinition { Id = "wall_milk_restraint", DisplayName = "Wall Milk Restraint", Category = ItemCategory.Tool, Price = 280, Description = "A wall-mounted milk restraint device. (Orig: 壁乳拘束台)" });
		Add(new ItemDefinition { Id = "pommel_horse", DisplayName = "Pommel Horse", Category = ItemCategory.Tool, Price = 200, Description = "A triangular pommel device. (Orig: 三角木馬)" });
		Add(new ItemDefinition { Id = "magic_scissors", DisplayName = "Magical Shears", Category = ItemCategory.Tool, Price = 150, Description = "Shears enchanted for cutting magic barriers. (Orig: マジカル裁ちバサミ)" });

		// === Magic Items (600-699) ===
		Add(new ItemDefinition { Id = "teleport", DisplayName = "Teleport", Category = ItemCategory.Tool, Price = 500, Description = "A one-way teleportation gate. (Orig: 個人用転移門)" });
		Add(new ItemDefinition { Id = "teleport_scroll", DisplayName = "Teleport Scroll", Category = ItemCategory.Tool, Price = 200, Description = "A scroll that enables teleportation. (Orig: テレポート)" });
		Add(new ItemDefinition { Id = "energy_drain_device", DisplayName = "Energy Drain Device", Category = ItemCategory.Tool, Price = 250, Description = "Extracts energy from targets. (Orig: 搾乳エナジードレイン)" });
		Add(new ItemDefinition { Id = "milk_drain_device", DisplayName = "Milk Drain Device", Category = ItemCategory.Tool, Price = 280, Description = "Extracts concentrated milk. (Orig: 濃厚ミルクドレイン)" });
		Add(new ItemDefinition { Id = "magic_injection", DisplayName = "Magic Injection", Category = ItemCategory.Tool, Price = 300, Description = "Injects magical energy. (Orig: 魔力注入)" });
		Add(new ItemDefinition { Id = "hypnosis_device", DisplayName = "Hypnosis Device", Category = ItemCategory.Tool, Price = 350, Description = "A device for simple hypnosis. (Orig: 簡易催眠)" });
		Add(new ItemDefinition { Id = "tentacle_transform", DisplayName = "Tentacle Transformation", Category = ItemCategory.Material, Price = 1000, Description = "Transforms the body to produce tentacles. (Orig: 触手変化)" });
		Add(new ItemDefinition { Id = "brush_tentacle", DisplayName = "Brush Tentacle", Category = ItemCategory.Material, Price = 200, Description = "A soft brush-type tentacle attachment. (Orig: ブラシ触手)" });
		Add(new ItemDefinition { Id = "penis_tentacle", DisplayName = "Penis Tentacle", Category = ItemCategory.Material, Price = 250, Description = "A tentacle resembling a penis. (Orig: ペニス触手)" });
		Add(new ItemDefinition { Id = "suction_tentacle", DisplayName = "Suction Tentacle", Category = ItemCategory.Material, Price = 220, Description = "A tentacle with suction cups. (Orig: 吸引触手)" });
		Add(new ItemDefinition { Id = "massage_tentacle", DisplayName = "Massage Tentacle", Category = ItemCategory.Material, Price = 200, Description = "A tentacle designed for massage. (Orig: 揉み触手)" });
		Add(new ItemDefinition { Id = "split_tentacle", DisplayName = "Split Tentacle", Category = ItemCategory.Material, Price = 230, Description = "A tentacle that splits at the tip. (Orig: 先割れ触手)" });
		Add(new ItemDefinition { Id = "transparent_tentacle", DisplayName = "Transparent Tentacle", Category = ItemCategory.Material, Price = 240, Description = "A nearly invisible tentacle. (Orig: 半透明触手)" });
		Add(new ItemDefinition { Id = "mouth_tentacle", DisplayName = "Mouth Tentacle", Category = ItemCategory.Material, Price = 250, Description = "A tentacle with a mouth at the tip. (Orig: 口型触手)" });
		Add(new ItemDefinition { Id = "injection_tentacle", DisplayName = "Injection Tentacle", Category = ItemCategory.Material, Price = 260, Description = "A tentacle that injects substances. (Orig: 注入触手)" });
		Add(new ItemDefinition { Id = "thin_tentacle", DisplayName = "Thin Tentacle", Category = ItemCategory.Material, Price = 210, Description = "A very thin, flexible tentacle. (Orig: 極細触手)" });
		Add(new ItemDefinition { Id = "poison_venom", DisplayName = "Aphrodisiac Venom", Category = ItemCategory.Material, Price = 300, Description = "Venom with aphrodisiac properties. (Orig: 媚毒生成)" });
		Add(new ItemDefinition { Id = "milk_body_extract", DisplayName = "Milk Body Extract", Category = ItemCategory.Material, Price = 400, Description = "An extract that induces milk body constitution. (Orig: 母乳体質化エキス)" });
		Add(new ItemDefinition { Id = "breast_reform_extract", DisplayName = "Breast Reform Extract", Category = ItemCategory.Material, Price = 500, Description = "An extract that reforms breast tissue. (Orig: 膨乳改造エキス)" });
		Add(new ItemDefinition { Id = "sensitive_mucus", DisplayName = "Sensitivity Mucus", Category = ItemCategory.Material, Price = 350, Description = "A mucus that increases sensitivity. (Orig: 感度上昇粘液)" });
		Add(new ItemDefinition { Id = "tentacle_ejaculation", DisplayName = "Tentacle Ejaculation", Category = ItemCategory.Material, Price = 300, Description = "Tentacle-based fertilization fluid. (Orig: 触手射精)" });
		Add(new ItemDefinition { Id = "tentacle_fertilization", DisplayName = "Tentacle Fertilization", Category = ItemCategory.Material, Price = 500, Description = "Tentacle-based conception fluid. (Orig: 触手受胎)" });
		Add(new ItemDefinition { Id = "tentacle_equipment", DisplayName = "Tentacle Equipment Kit", Category = ItemCategory.Material, Price = 400, Description = "Materials for crafting tentacle equipment. (Orig: 触手装備作成)" });
		Add(new ItemDefinition { Id = "secretion_booster", DisplayName = "Secretion Booster", Category = ItemCategory.Material, Price = 350, Description = "Boosts secretion volume. (Orig: 触手分泌液増量)" });
		Add(new ItemDefinition { Id = "淫_mark", DisplayName = "Mark of Lust", Category = ItemCategory.Material, Price = 600, Description = "A magical mark that enhances pleasure sensitivity. (Orig: 淫紋付与)" });
		Add(new ItemDefinition { Id = "orgasm_healing_mark", DisplayName = "Orgasm Healing Mark", Category = ItemCategory.Material, Price = 500, Description = "A mark that heals at orgasm. (Orig: 絶頂体力回復淫紋)" });
		Add(new ItemDefinition { Id = "orgasm_magic_mark", DisplayName = "Orgasm Magic Mark", Category = ItemCategory.Material, Price = 500, Description = "A mark that restores magic at orgasm. (Orig: 絶頂魔力回復淫紋)" });
		Add(new ItemDefinition { Id = "pain_pleasure_convert", DisplayName = "Pain-Pleasure Converter", Category = ItemCategory.Material, Price = 700, Description = "Converts pain into pleasure. (Orig: 苦痛快楽変換)" });
		Add(new ItemDefinition { Id = "penis_transform", DisplayName = "Penis Transformation", Category = ItemCategory.Material, Price = 800, Description = "Transforms the body to produce a penis. (Orig: ペニス変化)" });
		Add(new ItemDefinition { Id = "time_compress", DisplayName = "Time Compression", Category = ItemCategory.Material, Price = 1000, Description = "Compresses time for accelerated processes. (Orig: 時間圧縮)" });
		Add(new ItemDefinition { Id = "brainwash", DisplayName = "Brainwashing", Category = ItemCategory.Material, Price = 2000, Description = "A powerful mental alteration technique. (Orig: 洗脳)" });
		Add(new ItemDefinition { Id = "体内凌辱", DisplayName = "Internal Humiliation", Category = ItemCategory.Material, Price = 900, Description = "Internal stimulation technique. (Orig: 体内凌辱)" });
		Add(new ItemDefinition { Id = "volume_increase", DisplayName = "Body Volume Increase", Category = ItemCategory.Material, Price = 600, Description = "Increases overall body capacity. (Orig: 体内容量増加)" });
		Add(new ItemDefinition { Id = "permanent_time_compress", DisplayName = "Permanent Time Compression", Category = ItemCategory.Material, Price = 1500, Description = "Permanently compresses time. (Orig: 時間圧縮永続化)" });

		// === Farming/Breeding Items (800-899) ===
		Add(new ItemDefinition { Id = "fertility_boost", DisplayName = "Fertility Boost", Category = ItemCategory.Material, Price = 400, Description = "Boosts fertility and breeding capacity. (Orig: 豊穣)" });
		Add(new ItemDefinition { Id = "rich_milk_massage", DisplayName = "Rich Milk Massage", Category = ItemCategory.Material, Price = 300, Description = "A massage technique that boosts milk production. (Orig: 濃厚母乳マッサージ)" });
		Add(new ItemDefinition { Id = "milk_tank_massage", DisplayName = "Milk Tank Massage", Category = ItemCategory.Material, Price = 350, Description = "A deep massage for maximum milk output. (Orig: ミルクタンクマッサージ)" });
		Add(new ItemDefinition { Id = "endurance_potion", DisplayName = "Endurance Potion", Category = ItemCategory.Consumable, Price = 250, Description = "Increases physical endurance. (Orig: 絶倫)" });
		Add(new ItemDefinition { Id = "hermaphrodite_potion", DisplayName = "Hermaphrodite Potion", Category = ItemCategory.Consumable, Price = 1500, Description = "Transforms the body to produce both sexes. (Orig: ふたなりちんぽ)" });
		Add(new ItemDefinition { Id = "inter_species_potion", DisplayName = "Inter-Species Breeding Potion", Category = ItemCategory.Consumable, Price = 2000, Description = "Enables breeding across species. (Orig: 異種族孕ませ)" });

		// === Milking Devices (900-999) ===
		Add(new ItemDefinition { Id = "livestock_milker", DisplayName = "Livestock Milking Machine", Category = ItemCategory.Tool, Price = 200, Description = "An automated milking device for livestock. (Orig: 家畜用搾乳器)" });
		Add(new ItemDefinition { Id = "magic_milker", DisplayName = "Magic Milking Device", Category = ItemCategory.Tool, Price = 300, Description = "A magical device that enhances milking. (Orig: 魔動快楽搾乳器)" });
		Add(new ItemDefinition { Id = "tentacle_milker", DisplayName = "Tentacle Milking Device", Category = ItemCategory.Tool, Price = 350, Description = "A device using tentacles for milking. (Orig: 触手快楽搾乳器)" });

		// === Pet Adoption Tickets (100-102) ===
		Add(new ItemDefinition { Id = "pegasus_ticket", DisplayName = "Fallen Pegasus Ticket", Category = ItemCategory.Keepsake, Price = 500, Description = "A ticket to adopt a fallen pegasus. (Orig: 堕天馬)", EffectType = ItemEffectType.PetAdopt, EffectTarget = "fallen_pegasus" });
		Add(new ItemDefinition { Id = "orthrus_ticket", DisplayName = "Orthrus Ticket", Category = ItemCategory.Keepsake, Price = 400, Description = "A ticket to adopt Orthrus. (Orig: オルトロス)", EffectType = ItemEffectType.PetAdopt, EffectTarget = "orthrus" });
		Add(new ItemDefinition { Id = "demon_hamster_ticket", DisplayName = "Demon Hamster Ticket", Category = ItemCategory.Keepsake, Price = 200, Description = "A ticket to adopt a demon hamster. (Orig: 魔界ハムスター)", EffectType = ItemEffectType.PetAdopt, EffectTarget = "demon_hamster" });

		// === New Buildings (120-129) ===
		Add(new ItemDefinition { Id = "family_bath", DisplayName = "Family Bath", Category = ItemCategory.Tool, Price = 800, Description = "A private bath for the whole ranch. (Orig: 家族風呂)" });
		Add(new ItemDefinition { Id = "public_bath", DisplayName = "Public Bathhouse", Category = ItemCategory.Tool, Price = 1200, Description = "A large bathhouse for ranch guests. (Orig: 大浴場)" });
		Add(new ItemDefinition { Id = "hot_spring", DisplayName = "Natural Hot Spring", Category = ItemCategory.Tool, Price = 2000, Description = "A natural hot spring on ranch grounds. (Orig: 天然温泉)" });
		Add(new ItemDefinition { Id = "office_expansion", DisplayName = "Office Expansion", Category = ItemCategory.Tool, Price = 600, Description = "Expands the office space. (Orig: 事務所増築)" });
		Add(new ItemDefinition { Id = "slave_dorm", DisplayName = "Slave Dormitory", Category = ItemCategory.Tool, Price = 700, Description = "A dormitory for enslaved workers. (Orig: 奴隷寮)" });
		Add(new ItemDefinition { Id = "slave_dorm_expansion", DisplayName = "Slave Dormitory Expansion", Category = ItemCategory.Tool, Price = 500, Description = "Expands the slave dormitory. (Orig: 奴隷寮増築)" });
		Add(new ItemDefinition { Id = "kitchen_system", DisplayName = "System Kitchen", Category = ItemCategory.Tool, Price = 1000, Description = "A fully equipped modern kitchen. (Orig: システムキッチン)" });
		Add(new ItemDefinition { Id = "pet_kennel", DisplayName = "Pet Kennel", Category = ItemCategory.Tool, Price = 400, Description = "A kennel for ranch pets. (Orig: ペット小屋)" });
		Add(new ItemDefinition { Id = "cow_barn", DisplayName = "Cow Barn", Category = ItemCategory.Tool, Price = 600, Description = "A barn for dairy cows. (Orig: 牛舎)" });

		// === Special Items ===
		Add(new ItemDefinition { Id = "necronomicon", DisplayName = "Necronomicon", Category = ItemCategory.Keepsake, Price = 5000, Description = "A forbidden tome of dark knowledge. (Orig: ネクロノミコン)" });
		Add(new ItemDefinition { Id = "tentacle_encyclopedia", DisplayName = "Tentacle Ecology Encyclopedia", Category = ItemCategory.Keepsake, Price = 3000, Description = "A comprehensive guide to tentacle biology. (Orig: 触手生物生態図鑑)" });
		Add(new ItemDefinition { Id = "succubus_novel", DisplayName = "Succubus Pamphlet", Category = ItemCategory.Keepsake, Price = 2000, Description = "A pamphlet distributed by succubi. (Orig: サキュバス頒布の薄い本)" });
		Add(new ItemDefinition { Id = "dragon_egg", DisplayName = "Dragon Egg", Category = ItemCategory.Keepsake, Price = 10000, Description = "A mysterious egg that may hatch something. (Orig: 龍のタマ)" });
		Add(new ItemDefinition { Id = "alchemy_table", DisplayName = "Advanced Alchemy Table", Category = ItemCategory.Tool, Price = 1500, Description = "A high-grade alchemy workstation. (Orig: 高度薬学台)" });
		Add(new ItemDefinition { Id = "magic_storage_small", DisplayName = "Personal Magic Storage", Category = ItemCategory.Tool, Price = 800, Description = "A small device for storing magic power. (Orig: 家庭用魔力貯蔵器)" });
		Add(new ItemDefinition { Id = "magic_storage_large", DisplayName = "Large Magic Storage", Category = ItemCategory.Tool, Price = 1500, Description = "A large-scale magic storage device. (Orig: 業務用魔力貯蔵器)" });
		Add(new ItemDefinition { Id = "magic_storage_huge", DisplayName = "Huge Magic Storage", Category = ItemCategory.Tool, Price = 3000, Description = "A massive magic storage facility. (Orig: 大容量魔力貯蔵器)" });
		Add(new ItemDefinition { Id = "magic_storage_mod", DisplayName = "Modified Magic Storage", Category = ItemCategory.Tool, Price = 5000, Description = "A heavily modified magic storage device. (Orig: 魔改造魔力貯蔵器)" });
		Add(new ItemDefinition { Id = "magic_supply_device", DisplayName = "Magic Supply Device", Category = ItemCategory.Tool, Price = 2000, Description = "A device that supplies magic power. (Orig: 魔力補給装置)" });
		Add(new ItemDefinition { Id = "magic_cuffs", DisplayName = "Magic Cuffs", Category = ItemCategory.Tool, Price = 600, Description = "Cuffs that suppress magical abilities. (Orig: 魔力枷)" });
		Add(new ItemDefinition { Id = "spirit_extractor", DisplayName = "Spirit Extraction Device", Category = ItemCategory.Tool, Price = 400, Description = "Extracts spiritual energy. (Orig: 霊力抽出装置)" });
		Add(new ItemDefinition { Id = "small_spirit_extractor", DisplayName = "Small Spirit Extractor", Category = ItemCategory.Tool, Price = 250, Description = "A compact spirit extraction device. (Orig: 小型霊力抽出装置)" });
		Add(new ItemDefinition { Id = "energy_drain", DisplayName = "Energy Drain", Category = ItemCategory.Material, Price = 150, Description = "Extracts energy from targets. (Orig: エナジードレイン効率)" });
		Add(new ItemDefinition { Id = "magic_absorb", DisplayName = "Magic Absorption", Category = ItemCategory.Material, Price = 200, Description = "Absorbs magic from the environment. (Orig: 魔力吸収効率)" });
		Add(new ItemDefinition { Id = "cow_bed_mat", DisplayName = "Cow Bed Mat", Category = ItemCategory.Material, Price = 50, Description = "A comfortable mat for livestock. (Orig: 牛床マット)" });
		Add(new ItemDefinition { Id = "water_filter", DisplayName = "Water Filter", Category = ItemCategory.Material, Price = 40, Description = "A filter for clean water. (Orig: 給水器フィルター)" });
		Add(new ItemDefinition { Id = "maid_collar", DisplayName = "Maid Collar", Category = ItemCategory.Equipment, Price = 70, Description = "A decorative maid collar.", Slot = EquipmentSlot.Necklace, ClothingStyleValue = ClothingStyle.Maid });
		Add(new ItemDefinition { Id = "cow_bell", DisplayName = "Cowbell", Category = ItemCategory.Equipment, Price = 30, Description = "A bell worn around the neck.", Slot = EquipmentSlot.Necklace, ClothingStyleValue = ClothingStyle.CowGirl, BonusMorale = 1 });
		Add(new ItemDefinition { Id = "cow_girl_headband", DisplayName = "Cowgirl Headband", Category = ItemCategory.Equipment, Price = 35, Description = "A headband with cow ears.", Slot = EquipmentSlot.Head, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "cow_bikini", DisplayName = "Cow Bikini", Category = ItemCategory.Equipment, Price = 80, Description = "A spotted bikini.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "cow_micro_bikini", DisplayName = "Cow Micro Bikini", Category = ItemCategory.Equipment, Price = 90, Description = "A minimal spotted bikini.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "cow_holey_bikini", DisplayName = "Cow Holey Bikini", Category = ItemCategory.Equipment, Price = 95, Description = "A spotted bikini with holes.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "cow_socks", DisplayName = "Cow Socks", Category = ItemCategory.Equipment, Price = 40, Description = "Spotted socks.", Slot = EquipmentSlot.Legs, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "cow_gloves", DisplayName = "Cow Gloves", Category = ItemCategory.Equipment, Price = 45, Description = "Spotted gloves.", Slot = EquipmentSlot.Arms, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "cow_sling", DisplayName = "Cow Slingshot", Category = ItemCategory.Equipment, Price = 60, Description = "A slingshot bikini.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "nipple_expose_bra", DisplayName = "Nipple-Expose Bra", Category = ItemCategory.Equipment, Price = 50, Description = "A bra with nipple openings.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "nippleless", DisplayName = "Nippleless", Category = ItemCategory.Equipment, Price = 40, Description = "A bra without nipple coverage.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "eyepatch_bra", DisplayName = "Eyepatch Bra", Category = ItemCategory.Equipment, Price = 55, Description = "A bra with an eyepatch design.", Slot = EquipmentSlot.UnderwearTop, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "exposed_panties", DisplayName = "Exposed Panties", Category = ItemCategory.Equipment, Price = 45, Description = "Panties with strategic openings.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "front_patch", DisplayName = "Front Patch", Category = ItemCategory.Equipment, Price = 30, Description = "A patch for the front.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "crotch_tag", DisplayName = "Crotch Tag", Category = ItemCategory.Equipment, Price = 35, Description = "A tag attached to the crotch.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "micro_bikini", DisplayName = "Micro Bikini", Category = ItemCategory.Equipment, Price = 60, Description = "A minimal bikini.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Swimsuit });
		Add(new ItemDefinition { Id = "garter_belt", DisplayName = "Garter Belt", Category = ItemCategory.Equipment, Price = 50, Description = "A garter belt for stockings.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "string", DisplayName = "String", Category = ItemCategory.Equipment, Price = 20, Description = "A simple string.", Slot = EquipmentSlot.UnderwearBottom, ClothingStyleValue = ClothingStyle.Lingerie });
		Add(new ItemDefinition { Id = "leather_armor_exp", DisplayName = "Leather Armor", Category = ItemCategory.Equipment, Price = 200, Description = "Tough leather armor.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Tactical, BonusMaxHp = 25, BonusCombatSkill = 2 });
		Add(new ItemDefinition { Id = "light_armor", DisplayName = "Light Armor", Category = ItemCategory.Equipment, Price = 300, Description = "Lightweight but protective armor.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Tactical, BonusMaxHp = 30, BonusMaxEnergy = 10 });
		Add(new ItemDefinition { Id = "heavy_armor", DisplayName = "Heavy Armor", Category = ItemCategory.Equipment, Price = 400, Description = "Heavy plate armor.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Tactical, BonusMaxHp = 50, BonusCombatSkill = 3 });
		Add(new ItemDefinition { Id = "full_armor", DisplayName = "Full Armor", Category = ItemCategory.Equipment, Price = 500, Description = "Complete plate armor.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Tactical, BonusMaxHp = 60, BonusCombatSkill = 4 });
		Add(new ItemDefinition { Id = "workwear_set", DisplayName = "Workwear Set", Category = ItemCategory.Equipment, Price = 150, Description = "A complete ranch workwear set.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Workwear });
		Add(new ItemDefinition { Id = "overall_set", DisplayName = "Overall Set", Category = ItemCategory.Equipment, Price = 160, Description = "A complete overall set.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Workwear });
		Add(new ItemDefinition { Id = "miko_set", DisplayName = "Miko Set", Category = ItemCategory.Equipment, Price = 200, Description = "A complete shrine maiden outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Exorcist });
		Add(new ItemDefinition { Id = "sister_set", DisplayName = "Sister Set", Category = ItemCategory.Equipment, Price = 190, Description = "A complete sister outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Exorcist });
		Add(new ItemDefinition { Id = "cowgirl_outfit_set", DisplayName = "Cowgirl Outfit Set", Category = ItemCategory.Equipment, Price = 170, Description = "A complete cowgirl outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "micro_cowgirl_set", DisplayName = "Micro Cowgirl Set", Category = ItemCategory.Equipment, Price = 180, Description = "A minimal cowgirl outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "hole_cowgirl_set", DisplayName = "Holey Cowgirl Set", Category = ItemCategory.Equipment, Price = 185, Description = "A cowgirl outfit with strategic holes.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "maid_set_full", DisplayName = "Maid Set", Category = ItemCategory.Equipment, Price = 160, Description = "A complete maid outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Maid });
		Add(new ItemDefinition { Id = "nursing_maid_set", DisplayName = "Nursing Maid Set", Category = ItemCategory.Equipment, Price = 170, Description = "A maid outfit with nursing access.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Maid });
		Add(new ItemDefinition { Id = "bunny_suit_full", DisplayName = "Bunny Suit Full", Category = ItemCategory.Equipment, Price = 150, Description = "A complete bunny suit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.Bunny });
		Add(new ItemDefinition { Id = "cowgirl_set_full", DisplayName = "Cowgirl Set", Category = ItemCategory.Equipment, Price = 160, Description = "A complete cowgirl outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "micro_cowgirl_full", DisplayName = "Micro Cowgirl Full", Category = ItemCategory.Equipment, Price = 170, Description = "A minimal cowgirl outfit.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
		Add(new ItemDefinition { Id = "hole_cowgirl_full", DisplayName = "Holey Cowgirl Full", Category = ItemCategory.Equipment, Price = 175, Description = "A cowgirl outfit with holes.", Slot = EquipmentSlot.Armor, ClothingStyleValue = ClothingStyle.CowGirl });
	}

	private void SeedFacilities()
	{
		Add(new FacilityDefinition { Id = "office", DisplayName = "Office", BuildCost = 0, UpkeepGold = 0, Capacity = 1 });
		Add(new FacilityDefinition { Id = "private_room", DisplayName = "Private Room", BuildCost = 0, UpkeepGold = 0, Capacity = 1 });
		Add(new FacilityDefinition { Id = "barn", DisplayName = "Barn", BuildCost = 0, UpkeepGold = 0, Capacity = 3 });
		Add(new FacilityDefinition { Id = "guest_room", DisplayName = "Guest Rooms", BuildCost = 120, UpkeepGold = 8, OutputResourceId = "comfort", OutputBonus = 1, Capacity = 2 });
		Add(new FacilityDefinition { Id = "dormitory", DisplayName = "Dormitory", BuildCost = 0, UpkeepGold = 0, Capacity = 4 });
		Add(new FacilityDefinition { Id = "pasture", DisplayName = "Pasture", BuildCost = 180, UpkeepGold = 20, OutputResourceId = "farm_goods", OutputBonus = 3 });
		Add(new FacilityDefinition { Id = "kitchen", DisplayName = "Kitchen", BuildCost = 140, UpkeepGold = 12, OutputResourceId = "meals", OutputBonus = 1 });
		Add(new FacilityDefinition { Id = "workshop", DisplayName = "Workshop", BuildCost = 170, UpkeepGold = 16, OutputResourceId = "supplies", OutputBonus = 1 });
		Add(new FacilityDefinition { Id = "well", DisplayName = "Well", BuildCost = 160, UpkeepGold = 10, OutputResourceId = "farm_goods", OutputBonus = 2 });
		Add(new FacilityDefinition { Id = "storage", DisplayName = "Storage Shed", BuildCost = 130, UpkeepGold = 6, OutputResourceId = "supplies", OutputBonus = 1 });
		Add(new FacilityDefinition { Id = "dairy_barn", DisplayName = "Dairy Barn", BuildCost = 250, UpkeepGold = 25, OutputResourceId = "farm_goods", OutputBonus = 5 });
		Add(new FacilityDefinition { Id = "pharmacy_lab", DisplayName = "Pharmacy Lab", BuildCost = 300, UpkeepGold = 20, OutputResourceId = "supplies", OutputBonus = 3 });
		Add(new FacilityDefinition { Id = "bathhouse", DisplayName = "Bathhouse", BuildCost = 200, UpkeepGold = 15, OutputResourceId = "comfort", OutputBonus = 2 });

		// === NSFW Facilities (from original CSV) ===
		Add(new FacilityDefinition { Id = "family_bath", DisplayName = "Family Bath (家族風呂)", BuildCost = 500, UpkeepGold = 30, OutputResourceId = "comfort", OutputBonus = 5, Capacity = 4 });
		Add(new FacilityDefinition { Id = "large_bath", DisplayName = "Large Bath (大浴場)", BuildCost = 800, UpkeepGold = 50, OutputResourceId = "comfort", OutputBonus = 8, Capacity = 8 });
		Add(new FacilityDefinition { Id = "natural_hot_spring", DisplayName = "Natural Hot Spring (天然温泉)", BuildCost = 1500, UpkeepGold = 80, OutputResourceId = "comfort", OutputBonus = 12, Capacity = 10 });
		Add(new FacilityDefinition { Id = "office_extension", DisplayName = "Office Extension (事務所増築)", BuildCost = 400, UpkeepGold = 20, OutputResourceId = "office_bonus", OutputBonus = 3, Capacity = 2 });
		Add(new FacilityDefinition { Id = "system_kitchen", DisplayName = "System Kitchen (システムキッチン)", BuildCost = 600, UpkeepGold = 40, OutputResourceId = "meals", OutputBonus = 5, Capacity = 4 });
		Add(new FacilityDefinition { Id = "slave_dormitory", DisplayName = "Slave Dormitory (奴隷寮)", BuildCost = 1000, UpkeepGold = 60, OutputResourceId = "comfort", OutputBonus = 3, Capacity = 10 });
		Add(new FacilityDefinition { Id = "magic_workshop", DisplayName = "Magic Workshop (魔改造工房)", BuildCost = 1200, UpkeepGold = 70, OutputResourceId = "magic_supplies", OutputBonus = 8, Capacity = 6 });
		Add(new FacilityDefinition { Id = "tentacle_room", DisplayName = "Tentacle Room (触手部屋)", BuildCost = 2000, UpkeepGold = 100, OutputResourceId = "comfort", OutputBonus = 15, Capacity = 6 });
		Add(new FacilityDefinition { Id = "nursing_room", DisplayName = "Nursing Room (授乳ルーム)", BuildCost = 800, UpkeepGold = 50, OutputResourceId = "milk_bonus", OutputBonus = 10, Capacity = 4 });
		Add(new FacilityDefinition { Id = "restraint_room", DisplayName = "Restraint Room (拘束室)", BuildCost = 1500, UpkeepGold = 80, OutputResourceId = "comfort", OutputBonus = 12, Capacity = 4 });
		Add(new FacilityDefinition { Id = "laboratory", DisplayName = "Laboratory (実験室)", BuildCost = 2500, UpkeepGold = 120, OutputResourceId = "magic_supplies", OutputBonus = 15, Capacity = 8 });
		Add(new FacilityDefinition { Id = "magic_storage", DisplayName = "Magic Storage (魔力貯蔵器)", BuildCost = 3000, UpkeepGold = 150, OutputResourceId = "magic_storage", OutputBonus = 20, Capacity = 20 });
		Add(new FacilityDefinition { Id = "magic_storage_2", DisplayName = "Magic Storage Mk.II (魔力貯蔵器改)", BuildCost = 5000, UpkeepGold = 250, OutputResourceId = "magic_storage", OutputBonus = 30, Capacity = 30 });
		Add(new FacilityDefinition { Id = "magic_storage_3", DisplayName = "Magic Storage Mk.III (魔力貯蔵器弐)", BuildCost = 8000, UpkeepGold = 400, OutputResourceId = "magic_storage", OutputBonus = 50, Capacity = 50 });
		Add(new FacilityDefinition { Id = "spirit_extractor", DisplayName = "Spirit Extractor (霊力抽出装置)", BuildCost = 4000, UpkeepGold = 200, OutputResourceId = "spirit_power", OutputBonus = 25, Capacity = 15 });
		Add(new FacilityDefinition { Id = "spirit_extractor_2", DisplayName = "Spirit Extractor Pro (霊力抽出装置改)", BuildCost = 7000, UpkeepGold = 350, OutputResourceId = "spirit_power", OutputBonus = 40, Capacity = 25 });
		Add(new FacilityDefinition { Id = "training_equipment", DisplayName = "Training Equipment (調教設備)", BuildCost = 1800, UpkeepGold = 90, OutputResourceId = "training_bonus", OutputBonus = 12, Capacity = 6 });
		Add(new FacilityDefinition { Id = "home_type_extractor", DisplayName = "Home Spirit Extractor (家庭用霊力抽出装置)", BuildCost = 2000, UpkeepGold = 100, OutputResourceId = "spirit_power", OutputBonus = 15, Capacity = 8 });
		Add(new FacilityDefinition { Id = "commercial_extractor", DisplayName = "Commercial Spirit Extractor (業務用霊力抽出装置)", BuildCost = 5000, UpkeepGold = 250, OutputResourceId = "spirit_power", OutputBonus = 35, Capacity = 20 });
		Add(new FacilityDefinition { Id = "large_capacity_extractor", DisplayName = "Large Capacity Extractor (大容量霊力抽出装置)", BuildCost = 8000, UpkeepGold = 400, OutputResourceId = "spirit_power", OutputBonus = 55, Capacity = 35 });
		Add(new FacilityDefinition { Id = "magic_reform_extractor", DisplayName = "Magic Reform Extractor (魔改造霊力抽出装置)", BuildCost = 12000, UpkeepGold = 600, OutputResourceId = "spirit_power", OutputBonus = 80, Capacity = 50 });
		Add(new FacilityDefinition { Id = "teleport_gate", DisplayName = "Teleport Gate (転移門)", BuildCost = 3000, UpkeepGold = 150, OutputResourceId = "teleport", OutputBonus = 10, Capacity = 1 });
		Add(new FacilityDefinition { Id = "teleport_gate_2", DisplayName = "Teleport Gate Mk.II (転移門改)", BuildCost = 6000, UpkeepGold = 300, OutputResourceId = "teleport", OutputBonus = 20, Capacity = 1 });
		Add(new FacilityDefinition { Id = "tentacle_gear", DisplayName = "Tentacle Gear (触手装備)", BuildCost = 2500, UpkeepGold = 120, OutputResourceId = "tentacle_bonus", OutputBonus = 18, Capacity = 5 });
		Add(new FacilityDefinition { Id = "brand_port", DisplayName = "Brand Port (淫紋接続ポート)", BuildCost = 3500, UpkeepGold = 180, OutputResourceId = "brand_control", OutputBonus = 25, Capacity = 8 });
		Add(new FacilityDefinition { Id = "milk_tank", DisplayName = "Milk Tank (乳搾りタンク)", BuildCost = 1500, UpkeepGold = 70, OutputResourceId = "milk_bonus", OutputBonus = 15, Capacity = 10 });
		Add(new FacilityDefinition { Id = "milk_tank_2", DisplayName = "Milk Tank Pro (乳搾りタンク改)", BuildCost = 3000, UpkeepGold = 150, OutputResourceId = "milk_bonus", OutputBonus = 25, Capacity = 20 });
		Add(new FacilityDefinition { Id = "milk_tank_3", DisplayName = "Milk Tank Industrial (乳搾りタンク業務用)", BuildCost = 5000, UpkeepGold = 250, OutputResourceId = "milk_bonus", OutputBonus = 40, Capacity = 35 });
		Add(new FacilityDefinition { Id = "training_room", DisplayName = "Training Room (訓練室)", BuildCost = 1000, UpkeepGold = 50, OutputResourceId = "training_bonus", OutputBonus = 8, Capacity = 4 });
		Add(new FacilityDefinition { Id = "magic_training_room", DisplayName = "Magic Training Room (魔力訓練室)", BuildCost = 2000, UpkeepGold = 100, OutputResourceId = "magic_bonus", OutputBonus = 15, Capacity = 6 });

		// === Magic Spells (28 from original) ===
		Spells.Add("energy_drain", new SpellDefinition { Id = "energy_drain", DisplayName = "Energy Drain (エナジードレイン)", Description = "Drains target's energy and converts to mana.", Type = SpellType.Drain, ManaCost = 15, SpiritEnergyCost = 5, CooldownDays = 1, RequiredMagicPower = 10, EffectTarget = "energy", EffectValue = 20, EffectDescription = "Drains 20 energy.", RequiresTarget = true });
		Spells.Add("spirit_inject", new SpellDefinition { Id = "spirit_inject", DisplayName = "Spirit Injection (霊力注入)", Description = "Injects spirit energy into target.", Type = SpellType.Empower, ManaCost = 10, SpiritEnergyCost = 15, CooldownDays = 1, RequiredMagicPower = 8, EffectTarget = "spirit", EffectValue = 30, EffectDescription = "Restores 30 spirit energy.", RequiresTarget = true });
		Spells.Add("brainwash", new SpellDefinition { Id = "brainwash", DisplayName = "Brainwash (洗脳)", Description = "Attempts to alter target's mental state.", Type = SpellType.Curse, ManaCost = 50, SpiritEnergyCost = 30, CooldownDays = 3, RequiredMagicPower = 30, EffectTarget = "mental", EffectValue = 100, EffectDescription = "Reduces mental resistance.", RequiresTarget = true });
		Spells.Add("internal_humiliation", new SpellDefinition { Id = "internal_humiliation", DisplayName = "Internal Humiliation (体内凌辱)", Description = "Internal transformation spell.", Type = SpellType.Transform, ManaCost = 40, SpiritEnergyCost = 25, CooldownDays = 2, RequiredMagicPower = 25, EffectTarget = "body", EffectValue = 15, EffectDescription = "Body undergoes internal transformation.", RequiresTarget = true });
		Spells.Add("time_compress", new SpellDefinition { Id = "time_compress", DisplayName = "Time Compression (時間圧縮)", Description = "Compresses time for accelerated actions.", Type = SpellType.Empower, ManaCost = 60, SpiritEnergyCost = 40, CooldownDays = 5, RequiredMagicPower = 35, EffectTarget = "time", EffectValue = 3, EffectDescription = "Triples action speed for 1 day.", RequiresTarget = false });
		Spells.Add("brand_grant", new SpellDefinition { Id = "brand_grant", DisplayName = "Brand Grant (淫紋付与)", Description = "Grants a magical brand mark.", Type = SpellType.Enchant, ManaCost = 45, SpiritEnergyCost = 30, CooldownDays = 3, RequiredMagicPower = 28, EffectTarget = "brand", EffectValue = 1, EffectDescription = "Adds transformation mark.", RequiresTarget = true });
		Spells.Add("milk_blessing", new SpellDefinition { Id = "milk_blessing", DisplayName = "Milk Blessing (母乳祝福)", Description = "Blesses target to produce magical milk.", Type = SpellType.Bless, ManaCost = 30, SpiritEnergyCost = 20, CooldownDays = 2, RequiredMagicPower = 20, EffectTarget = "milk", EffectValue = 10, EffectDescription = "Increases milk capacity.", RequiresTarget = true });
		Spells.Add("tentacle_summon", new SpellDefinition { Id = "tentacle_summon", DisplayName = "Tentacle Summon (触手召喚)", Description = "Summons tentacles for combat or training.", Type = SpellType.Summon, ManaCost = 35, SpiritEnergyCost = 25, CooldownDays = 2, RequiredMagicPower = 22, EffectTarget = "tentacle", EffectValue = 3, EffectDescription = "Summons 3 tentacles.", RequiresTarget = false });
		Spells.Add("mana_convert", new SpellDefinition { Id = "mana_convert", DisplayName = "Mana Convert (魔力転換)", Description = "Converts spirit energy to mana.", Type = SpellType.Empower, ManaCost = 0, SpiritEnergyCost = 30, CooldownDays = 1, RequiredMagicPower = 15, EffectTarget = "mana", EffectValue = 20, EffectDescription = "Converts 30 SP to 20 MP.", RequiresTarget = false });
		Spells.Add("corruption_field", new SpellDefinition { Id = "corruption_field", DisplayName = "Corruption Field (腐敗の輪)", Description = "Creates a field of corruption around caster.", Type = SpellType.Curse, ManaCost = 40, SpiritEnergyCost = 35, CooldownDays = 3, RequiredMagicPower = 28, EffectTarget = "area", EffectValue = 50, EffectDescription = "Reduces resistance in area.", RequiresTarget = false });
		Spells.Add("purification", new SpellDefinition { Id = "purification", DisplayName = "Purification (浄化)", Description = "Purifies corruption and restores mental state.", Type = SpellType.Bless, ManaCost = 25, SpiritEnergyCost = 15, CooldownDays = 1, RequiredMagicPower = 18, EffectTarget = "mental", EffectValue = 30, EffectDescription = "Restores mental resistance.", RequiresTarget = true });
		Spells.Add("sensitivity_enhance", new SpellDefinition { Id = "sensitivity_enhance", DisplayName = "Sensitivity Enhancement (感度上昇)", Description = "Enhances target's sensitivity.", Type = SpellType.Empower, ManaCost = 20, SpiritEnergyCost = 10, CooldownDays = 1, RequiredMagicPower = 12, EffectTarget = "sensitivity", EffectValue = 15, EffectDescription = "Increases sensitivity.", RequiresTarget = true });
		Spells.Add("breast_growth", new SpellDefinition { Id = "breast_growth", DisplayName = "Breast Growth (胸成長)", Description = "Accelerates breast tissue growth.", Type = SpellType.Transform, ManaCost = 35, SpiritEnergyCost = 20, CooldownDays = 2, RequiredMagicPower = 20, EffectTarget = "breast", EffectValue = 5, EffectDescription = "Increases bust size.", RequiresTarget = true });
		Spells.Add("hair_color_change", new SpellDefinition { Id = "hair_color_change", DisplayName = "Hair Color Change (髪色変換)", Description = "Changes hair color permanently.", Type = SpellType.Transform, ManaCost = 10, SpiritEnergyCost = 5, CooldownDays = 5, RequiredMagicPower = 8, EffectTarget = "hair", EffectValue = 1, EffectDescription = "Hair color transformed.", RequiresTarget = true });
		Spells.Add("body_soften", new SpellDefinition { Id = "body_soften", DisplayName = "Body Soften (身体軟化)", Description = "Softens body structure for flexibility.", Type = SpellType.Transform, ManaCost = 30, SpiritEnergyCost = 20, CooldownDays = 2, RequiredMagicPower = 18, EffectTarget = "body", EffectValue = 10, EffectDescription = "Increases body flexibility.", RequiresTarget = true });
		Spells.Add("orgasm_induce", new SpellDefinition { Id = "orgasm_induce", DisplayName = "Orgasm Induce (絶頂誘発)", Description = "Induces intense orgasmic response.", Type = SpellType.Curse, ManaCost = 25, SpiritEnergyCost = 20, CooldownDays = 1, RequiredMagicPower = 15, EffectTarget = "pleasure", EffectValue = 50, EffectDescription = "Induces orgasm.", RequiresTarget = true });
		Spells.Add("mind_break", new SpellDefinition { Id = "mind_break", DisplayName = "Mind Break (精神崩壊)", Description = "Breaks target's mental resistance.", Type = SpellType.Curse, ManaCost = 80, SpiritEnergyCost = 50, CooldownDays = 7, RequiredMagicPower = 45, EffectTarget = "mental", EffectValue = 200, EffectDescription = "Reduces dignity significantly.", RequiresTarget = true });
		Spells.Add("healing", new SpellDefinition { Id = "healing", DisplayName = "Healing (治癒)", Description = "Restores target's health.", Type = SpellType.Bless, ManaCost = 15, SpiritEnergyCost = 10, CooldownDays = 1, RequiredMagicPower = 10, EffectTarget = "hp", EffectValue = 50, EffectDescription = "Restores 50 HP.", RequiresTarget = true });
		Spells.Add("fatigue_heal", new SpellDefinition { Id = "fatigue_heal", DisplayName = "Fatigue Heal (疲労回復)", Description = "Reduces target's fatigue.", Type = SpellType.Bless, ManaCost = 10, SpiritEnergyCost = 10, CooldownDays = 1, RequiredMagicPower = 8, EffectTarget = "fatigue", EffectValue = 30, EffectDescription = "Reduces fatigue by 30.", RequiresTarget = true });
		Spells.Add("teleport", new SpellDefinition { Id = "teleport", DisplayName = "Teleport (転移)", Description = "Instantly teleports target to location.", Type = SpellType.Teleport, ManaCost = 20, SpiritEnergyCost = 15, CooldownDays = 2, RequiredMagicPower = 12, EffectTarget = "position", EffectValue = 1, EffectDescription = "Teleports target.", RequiresTarget = true });
		Spells.Add("magic_barrier", new SpellDefinition { Id = "magic_barrier", DisplayName = "Magic Barrier (魔力障壁)", Description = "Creates a protective magic barrier.", Type = SpellType.Bless, ManaCost = 25, SpiritEnergyCost = 20, CooldownDays = 1, RequiredMagicPower = 15, EffectTarget = "defense", EffectValue = 40, EffectDescription = "Adds defensive barrier.", RequiresTarget = false });
		Spells.Add("addiction_induce", new SpellDefinition { Id = "addiction_induce", DisplayName = "Addiction Induce (中毒誘発)", Description = "Induces magical addiction in target.", Type = SpellType.Curse, ManaCost = 50, SpiritEnergyCost = 35, CooldownDays = 5, RequiredMagicPower = 32, EffectTarget = "addiction", EffectValue = 1, EffectDescription = "Creates magical dependency.", RequiresTarget = true });
		Spells.Add("pet_adoption", new SpellDefinition { Id = "pet_adoption", DisplayName = "Pet Adoption (ペット採用)", Description = "Adopts a magical creature as a pet.", Type = SpellType.Summon, ManaCost = 50, SpiritEnergyCost = 30, CooldownDays = 10, RequiredMagicPower = 30, EffectTarget = "pet", EffectValue = 1, EffectDescription = "Adopts a magical pet.", RequiresTarget = false });
		Spells.Add("skill_boost", new SpellDefinition { Id = "skill_boost", DisplayName = "Skill Boost (スキル上昇)", Description = "Temporarily boosts a skill.", Type = SpellType.Empower, ManaCost = 15, SpiritEnergyCost = 10, CooldownDays = 1, RequiredMagicPower = 10, EffectTarget = "skill", EffectValue = 5, EffectDescription = "Boosts skill by 5 for 1 day.", RequiresTarget = true });
		Spells.Add("bond_link", new SpellDefinition { Id = "bond_link", DisplayName = "Bond Link (絆リンク)", Description = "Creates a magical bond link between characters.", Type = SpellType.Bless, ManaCost = 30, SpiritEnergyCost = 20, CooldownDays = 3, RequiredMagicPower = 20, EffectTarget = "bond", EffectValue = 20, EffectDescription = "Increases bond between characters.", RequiresTarget = true });
		Spells.Add("milk_concentration", new SpellDefinition { Id = "milk_concentration", DisplayName = "Milk Concentration (母乳濃縮)", Description = "Concentrates milk quality.", Type = SpellType.Transform, ManaCost = 20, SpiritEnergyCost = 15, CooldownDays = 1, RequiredMagicPower = 12, EffectTarget = "milk_quality", EffectValue = 10, EffectDescription = "Increases milk quality.", RequiresTarget = true });
		Spells.Add("spirit_drain", new SpellDefinition { Id = "spirit_drain", DisplayName = "Spirit Drain (霊力吸収)", Description = "Drains spirit energy from target.", Type = SpellType.Drain, ManaCost = 10, SpiritEnergyCost = 0, CooldownDays = 1, RequiredMagicPower = 8, EffectTarget = "spirit", EffectValue = 25, EffectDescription = "Drains 25 spirit energy.", RequiresTarget = true });
		Spells.Add("magic_resonance", new SpellDefinition { Id = "magic_resonance", DisplayName = "Magic Resonance (魔力共鳴)", Description = "Resonates magic with environment.", Type = SpellType.Empower, ManaCost = 45, SpiritEnergyCost = 30, CooldownDays = 3, RequiredMagicPower = 30, EffectTarget = "magic", EffectValue = 20, EffectDescription = "Boosts all magic power.", RequiresTarget = false });
	}

	// === Spell Registry ===
	public Dictionary<string, SpellDefinition> Spells { get; } = new();

	private void SeedMissions()
	{
		Add(new MissionDefinition { Id = "road_patrol", DisplayName = "Road Patrol", Tier = MissionTier.Local, Difficulty = 10, RewardGold = 80, RewardItemId = "feed_bundle", EnemyGroupId = "group_wild_local" });
		Add(new MissionDefinition { Id = "field_clear", DisplayName = "Field Clear", Tier = MissionTier.Local, Difficulty = 8, RewardGold = 60, RewardItemId = "feed_bundle", EnemyGroupId = "group_wild_local" });
		Add(new MissionDefinition { Id = "beast_hunt", DisplayName = "Beast Hunt", Tier = MissionTier.Local, Difficulty = 12, RewardGold = 90, RewardItemId = "pet_jerky", EnemyGroupId = "group_beast_local" });
		Add(new MissionDefinition { Id = "forest_survey", DisplayName = "Forest Survey", Tier = MissionTier.Regional, Difficulty = 16, RewardGold = 130, RewardItemId = "tool_kit", EnemyGroupId = "group_forest_regional" });
		Add(new MissionDefinition { Id = "trade_escort", DisplayName = "Trade Escort", Tier = MissionTier.Regional, Difficulty = 18, RewardGold = 150, RewardItemId = "camping_gear", EnemyGroupId = "group_bandit_regional" });
		Add(new MissionDefinition { Id = "ruin_delve", DisplayName = "Ruin Delve", Tier = MissionTier.Regional, Difficulty = 22, RewardGold = 200, RewardItemId = "travel_gear", EnemyGroupId = "group_ruin_regional" });
		Add(new MissionDefinition { Id = "dragon_outcrop", DisplayName = "Dragon Outcrop", Tier = MissionTier.Dangerous, Difficulty = 30, RewardGold = 350, RewardItemId = "keepsake", EnemyGroupId = "group_dragon_dangerous" });
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
		Add(new SkillDefinition { Id = "dairy_science", DisplayName = "Dairy Science", Description = "Increases dairy output and milk quality.", CostResourceId = "supplies", CostAmount = 4 });
		Add(new SkillDefinition { Id = "culinary_arts", DisplayName = "Culinary Arts", Description = "Improves meal quality and cooking output.", CostResourceId = "meals", CostAmount = 4 });
		Add(new SkillDefinition { Id = "herbalism", DisplayName = "Herbalism", Description = "Improves pharmacy and potion crafting.", CostResourceId = "supplies", CostAmount = 3 });
		Add(new SkillDefinition { Id = "hospitality", DisplayName = "Hospitality", Description = "Improves guest comfort and bond gains.", CostResourceId = "comfort", CostAmount = 3 });
		Add(new SkillDefinition { Id = "craftsmanship", DisplayName = "Craftsmanship", Description = "Improves workshop output and item quality.", CostResourceId = "supplies", CostAmount = 4 });
		Add(new SkillDefinition { Id = "logistics", DisplayName = "Logistics", Description = "Reduces facility upkeep costs.", CostResourceId = "supplies", CostAmount = 5 });
		Add(new SkillDefinition { Id = "tactical_training", DisplayName = "Tactical Training", Description = "All party members gain bonus combat skill in missions.", CostResourceId = "meals", CostAmount = 5 });
		Add(new SkillDefinition { Id = "arcane_studies", DisplayName = "Arcane Studies", Description = "Improves MagicPower training efficiency and energy recovery.", CostResourceId = "supplies", CostAmount = 4 });
	}

	private void SeedPets()
	{
		Add(new PetDefinition { Id = "stable_cat", DisplayName = "Stable Cat", CareCost = 15 });
		Add(new PetDefinition { Id = "yard_hound", DisplayName = "Yard Hound", CareCost = 20 });
		Add(new PetDefinition { Id = "fallen_pegasus", DisplayName = "Fallen Angel Horse", CareCost = 60, IsMountable = true });
		Add(new PetDefinition { Id = "orthrus", DisplayName = "Orthrus", CareCost = 50, IsMountable = true });
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
		Add(new BondEventDefinition { Id = "rancher_morning_rounds", CharacterId = "rancher", Title = "Morning Rounds", Description = "Walk the fence line together at dawn. The ranch is most alive in the quiet hours.", RequiredBond = 0, BondReward = 7, MoraleReward = 4, StockpileRewardId = "farm_goods", StockpileRewardAmount = 2 });
		Add(new BondEventDefinition { Id = "rancher_stable_talk", CharacterId = "rancher", Title = "Stable Talk", Description = "Brush down the horses and share stories of past ranches. Steady hands build steady bonds.", RequiredBond = 12, BondReward = 8, MoraleReward = 5, StockpileRewardId = "supplies", StockpileRewardAmount = 1 });
		Add(new BondEventDefinition { Id = "rancher_fire_watch", CharacterId = "rancher", Title = "Fire Watch", Description = "Sit by the outdoor fire pit long after sunset. The embers glow as trust takes root.", RequiredBond = 22, BondReward = 10, MoraleReward = 6, StockpileRewardId = "comfort", StockpileRewardAmount = 2 });
	}

	private void SeedTalents()
	{
		Add(new TalentDefinition { Id = "fast_learner", DisplayName = "Fast Learner", Description = "Gains skills 25% faster.", GrowthMultiplier = 1.25f });
		Add(new TalentDefinition { Id = "klutz", DisplayName = "Klutz", Description = "Clumsy hands reduce job output by 20%.", JobOutputMultiplier = 0.8f });
		Add(new TalentDefinition { Id = "hospitality_clumsy", DisplayName = "Hospitality Clumsy", Description = "Awkward service reduces output by 15%.", JobOutputMultiplier = 0.85f });
		Add(new TalentDefinition { Id = "cowardly", DisplayName = "Cowardly", Description = "-1 Combat Skill.", BonusCombatSkill = -1 });
		Add(new TalentDefinition { Id = "shy", DisplayName = "Shy", Description = "Training is 10% less effective.", TrainingEfficiency = 0.9f });
		Add(new TalentDefinition { Id = "obedient", DisplayName = "Obedient", Description = "Training is 20% more effective.", TrainingEfficiency = 1.2f });
		Add(new TalentDefinition { Id = "devoted", DisplayName = "Devoted", Description = "+10 max Morale.", MoraleCapBonus = 10 });
		Add(new TalentDefinition { Id = "proud", DisplayName = "Proud", Description = "+1 Combat Skill.", BonusCombatSkill = 1 });
		Add(new TalentDefinition { Id = "faith", DisplayName = "Faith", Description = "+2 Combat Skill, +5 max Morale.", BonusCombatSkill = 2, MoraleCapBonus = 5 });
		Add(new TalentDefinition { Id = "justice", DisplayName = "Justice", Description = "+2 Combat Skill.", BonusCombatSkill = 2 });
		Add(new TalentDefinition { Id = "steadfast", DisplayName = "Steadfast", Description = "Resists 2 fatigue per day.", FatigueResistance = 2 });
		Add(new TalentDefinition { Id = "honest_to_pleasure", DisplayName = "Honest to Pleasure", Description = "Training is 15% more effective.", TrainingEfficiency = 1.15f });
		Add(new TalentDefinition { Id = "shameless", DisplayName = "Shameless", Description = "Training is 20% more effective.", TrainingEfficiency = 1.2f });
		Add(new TalentDefinition { Id = "weak_to_pain", DisplayName = "Weak to Pain", Description = "Training is 15% less effective.", TrainingEfficiency = 0.85f });
		Add(new TalentDefinition { Id = "moody", DisplayName = "Moody", Description = "Gains 1 more fatigue per day.", FatigueResistance = -1 });
		Add(new TalentDefinition { Id = "easily_charmed", DisplayName = "Easily Charmed", Description = "Training is 10% more effective.", TrainingEfficiency = 1.1f });
		Add(new TalentDefinition { Id = "pharmacy_knowledge", DisplayName = "Pharmacy Knowledge", Description = "+1 Craft Skill, better item healing.", BonusCraftSkill = 1 });
		Add(new TalentDefinition { Id = "docile", DisplayName = "Docile", Description = "+5 max Morale.", MoraleCapBonus = 5 });
		Add(new TalentDefinition { Id = "maternal_instinct", DisplayName = "Maternal Instinct", Description = "+5 max Energy, +1 Ranch Skill.", BonusMaxEnergy = 5, BonusRanchSkill = 1 });
		Add(new TalentDefinition { Id = "self_control", DisplayName = "Self Control", Description = "Resists 1 fatigue per day.", FatigueResistance = 1 });
		Add(new TalentDefinition { Id = "conservative", DisplayName = "Conservative", Description = "+1 Craft Skill.", BonusCraftSkill = 1 });
		Add(new TalentDefinition { Id = "dignity", DisplayName = "Dignity", Description = "+1 max Morale, resists 1 fatigue.", MoraleCapBonus = 1, FatigueResistance = 1 });
		Add(new TalentDefinition { Id = "rebellious", DisplayName = "Rebellious", Description = "Training is 10% less effective.", TrainingEfficiency = 0.9f });
		Add(new TalentDefinition { Id = "horns", DisplayName = "Horns" });
		Add(new TalentDefinition { Id = "male", DisplayName = "Male" });
		Add(new TalentDefinition { Id = "owner", DisplayName = "Ranch Owner" });
		Add(new TalentDefinition { Id = "makai_race", DisplayName = "Makai Race" });
		Add(new TalentDefinition { Id = "mouth_paradise", DisplayName = "Mouth Paradise" });
		Add(new TalentDefinition { Id = "virginity_barrier", DisplayName = "Virginity Barrier" });
		Add(new TalentDefinition { Id = "chastity", DisplayName = "Chastity" });
		Add(new TalentDefinition { Id = "virgin", DisplayName = "Virgin" });
		Add(new TalentDefinition { Id = "a_virgin", DisplayName = "Anal Virgin" });
		Add(new TalentDefinition { Id = "m_virgin", DisplayName = "Mouth Virgin" });
		Add(new TalentDefinition { Id = "pure", DisplayName = "Pure" });
		Add(new TalentDefinition { Id = "tsundere", DisplayName = "Tsundere" });
		Add(new TalentDefinition { Id = "doesnt_cross_line", DisplayName = "Doesn't Cross Line" });
		Add(new TalentDefinition { Id = "maiden_heart", DisplayName = "Maiden Heart" });
		Add(new TalentDefinition { Id = "denies_pleasure", DisplayName = "Denies Pleasure" });
		Add(new TalentDefinition { Id = "jk", DisplayName = "JK" });
		Add(new TalentDefinition { Id = "baby_face", DisplayName = "Baby Face" });
		Add(new TalentDefinition { Id = "extreme_milk_pressure", DisplayName = "Extreme Milk Pressure" });
		Add(new TalentDefinition { Id = "breast_abuse_hatred", DisplayName = "Breast Abuse Hatred" });
		Add(new TalentDefinition { Id = "breast_proud", DisplayName = "Breast Proud" });
		Add(new TalentDefinition { Id = "cleaning_clumsy", DisplayName = "Cleaning Clumsy" });
		Add(new TalentDefinition { Id = "indifferent", DisplayName = "Indifferent" });
		Add(new TalentDefinition { Id = "animal_ears", DisplayName = "Animal Ears" });
		Add(new TalentDefinition { Id = "instigator", DisplayName = "Instigator" });
	}

	private void SeedTrainingActions()
	{
		// Hand category (10 actions)
		Add(new TrainingActionDefinition { Id = "hand_01", DisplayName = "Breast Massage", Category = TrainingCategory.Hand, ActionId = 10, FatigueDelta = 8, MoraleDelta = 3, XpTypes = new List<string> { "ranch_skill" }, Description = "Gentle breast massage" });
		Add(new TrainingActionDefinition { Id = "hand_02", DisplayName = "Breast Milking", Category = TrainingCategory.Hand, ActionId = 11, FatigueDelta = 10, MoraleDelta = 2, XpTypes = new List<string> { "ranch_skill" }, Description = "Breast milking" });
		Add(new TrainingActionDefinition { Id = "hand_03", DisplayName = "Nipple Pinch", Category = TrainingCategory.Hand, ActionId = 12, FatigueDelta = 7, MoraleDelta = 1, XpTypes = new List<string> { "craft_skill" }, Description = "Nipple pinch" });
		Add(new TrainingActionDefinition { Id = "hand_04", DisplayName = "Clit Stimulation", Category = TrainingCategory.Hand, ActionId = 13, FatigueDelta = 9, MoraleDelta = 2, XpTypes = new List<string> { "craft_skill" }, Description = "Clit stimulation" });
		Add(new TrainingActionDefinition { Id = "hand_05", DisplayName = "V Finger Insertion", Category = TrainingCategory.Hand, ActionId = 14, FatigueDelta = 10, MoraleDelta = 1, XpTypes = new List<string> { "ranch_skill" }, Description = "V finger insertion" });
		Add(new TrainingActionDefinition { Id = "hand_06", DisplayName = "A Finger Insertion", Category = TrainingCategory.Hand, ActionId = 15, FatigueDelta = 10, MoraleDelta = 1, XpTypes = new List<string> { "ranch_skill" }, Description = "A finger insertion" });
		Add(new TrainingActionDefinition { Id = "hand_07", DisplayName = "Butt Caress", Category = TrainingCategory.Hand, ActionId = 16, FatigueDelta = 5, MoraleDelta = 2, XpTypes = new List<string> { "ranch_skill" }, Description = "Butt caress" });
		Add(new TrainingActionDefinition { Id = "hand_08", DisplayName = "Breast Caress", Category = TrainingCategory.Hand, ActionId = 17, FatigueDelta = 6, MoraleDelta = 3, XpTypes = new List<string> { "craft_skill" }, Description = "Breast caress" });
		Add(new TrainingActionDefinition { Id = "hand_09", DisplayName = "Tickling", Category = TrainingCategory.Hand, ActionId = 18, FatigueDelta = 4, MoraleDelta = 5, XpTypes = new List<string> { "ranch_skill" }, Description = "Tickling" });
		Add(new TrainingActionDefinition { Id = "hand_10", DisplayName = "Belly Caress", Category = TrainingCategory.Hand, ActionId = 19, FatigueDelta = 5, MoraleDelta = 3, XpTypes = new List<string> { "craft_skill" }, Description = "Belly caress" });

		// Mouth category (10 actions)
		Add(new TrainingActionDefinition { Id = "mouth_01", DisplayName = "Breast Sucking", Category = TrainingCategory.Mouth, ActionId = 20, FatigueDelta = 8, MoraleDelta = 3, XpTypes = new List<string> { "craft_skill" }, Description = "Breast sucking" });
		Add(new TrainingActionDefinition { Id = "mouth_02", DisplayName = "Nipple Sucking", Category = TrainingCategory.Mouth, ActionId = 21, FatigueDelta = 7, MoraleDelta = 4, XpTypes = new List<string> { "craft_skill" }, Description = "Nipple sucking" });
		Add(new TrainingActionDefinition { Id = "mouth_03", DisplayName = "Kiss", Category = TrainingCategory.Mouth, ActionId = 22, FatigueDelta = 4, MoraleDelta = 5, XpTypes = new List<string> { "ranch_skill" }, Description = "Kiss" });
		Add(new TrainingActionDefinition { Id = "mouth_04", DisplayName = "Clit Licking", Category = TrainingCategory.Mouth, ActionId = 23, FatigueDelta = 9, MoraleDelta = 2, XpTypes = new List<string> { "craft_skill" }, Description = "Clit licking" });
		Add(new TrainingActionDefinition { Id = "mouth_05", DisplayName = "Vaginal Licking", Category = TrainingCategory.Mouth, ActionId = 24, FatigueDelta = 10, MoraleDelta = 2, XpTypes = new List<string> { "craft_skill" }, Description = "Vaginal licking" });
		Add(new TrainingActionDefinition { Id = "mouth_06", DisplayName = "Anal Licking", Category = TrainingCategory.Mouth, ActionId = 25, FatigueDelta = 10, MoraleDelta = 1, XpTypes = new List<string> { "craft_skill" }, Description = "Anal licking" });
		Add(new TrainingActionDefinition { Id = "mouth_07", DisplayName = "Breast Licking", Category = TrainingCategory.Mouth, ActionId = 26, FatigueDelta = 6, MoraleDelta = 3, XpTypes = new List<string> { "craft_skill" }, Description = "Breast licking" });
		Add(new TrainingActionDefinition { Id = "mouth_08", DisplayName = "Ear Licking", Category = TrainingCategory.Mouth, ActionId = 27, FatigueDelta = 5, MoraleDelta = 4, XpTypes = new List<string> { "craft_skill" }, Description = "Ear licking" });
		Add(new TrainingActionDefinition { Id = "mouth_09", DisplayName = "Armpit Licking", Category = TrainingCategory.Mouth, ActionId = 28, FatigueDelta = 5, MoraleDelta = 3, XpTypes = new List<string> { "craft_skill" }, Description = "Armpit licking" });
		Add(new TrainingActionDefinition { Id = "mouth_10", DisplayName = "Cheek Licking", Category = TrainingCategory.Mouth, ActionId = 29, FatigueDelta = 3, MoraleDelta = 4, XpTypes = new List<string> { "ranch_skill" }, Description = "Cheek licking" });

		// V Insertion (8 actions)
		Add(new TrainingActionDefinition { Id = "v_01", DisplayName = "Missionary Position", Category = TrainingCategory.VInsertion, ActionId = 30, FatigueDelta = 12, MoraleDelta = 2, XpTypes = new List<string> { "combat_skill" }, Description = "Missionary position" });
		Add(new TrainingActionDefinition { Id = "v_02", DisplayName = "Doggy Style", Category = TrainingCategory.VInsertion, ActionId = 31, FatigueDelta = 12, MoraleDelta = 1, XpTypes = new List<string> { "combat_skill" }, Description = "Doggy style" });
		Add(new TrainingActionDefinition { Id = "v_03", DisplayName = "Face-to-Face Sitting", Category = TrainingCategory.VInsertion, ActionId = 32, FatigueDelta = 11, MoraleDelta = 3, XpTypes = new List<string> { "ranch_skill" }, Description = "Face-to-face sitting" });
		Add(new TrainingActionDefinition { Id = "v_04", DisplayName = "Back-to-Back Sitting", Category = TrainingCategory.VInsertion, ActionId = 33, FatigueDelta = 11, MoraleDelta = 2, XpTypes = new List<string> { "ranch_skill" }, Description = "Back-to-back sitting" });
		Add(new TrainingActionDefinition { Id = "v_05", DisplayName = "Cowgirl Position", Category = TrainingCategory.VInsertion, ActionId = 34, FatigueDelta = 13, MoraleDelta = 2, XpTypes = new List<string> { "combat_skill" }, Description = "Cowgirl position" });
		Add(new TrainingActionDefinition { Id = "v_06", DisplayName = "Face-to-Face Standing", Category = TrainingCategory.VInsertion, ActionId = 35, FatigueDelta = 14, MoraleDelta = 1, XpTypes = new List<string> { "combat_skill" }, Description = "Face-to-face standing" });
		Add(new TrainingActionDefinition { Id = "v_07", DisplayName = "Back-to-Back Standing", Category = TrainingCategory.VInsertion, ActionId = 36, FatigueDelta = 14, MoraleDelta = 1, XpTypes = new List<string> { "combat_skill" }, Description = "Back-to-back standing" });
		Add(new TrainingActionDefinition { Id = "v_08", DisplayName = "Forced Cowgirl", Category = TrainingCategory.VInsertion, ActionId = 37, FatigueDelta = 15, MoraleDelta = -2, XpTypes = new List<string> { "combat_skill" }, Description = "Forced cowgirl" });

		// A Insertion (8 actions)
		Add(new TrainingActionDefinition { Id = "a_01", DisplayName = "A Missionary", Category = TrainingCategory.AInsertion, ActionId = 40, FatigueDelta = 12, MoraleDelta = 1, XpTypes = new List<string> { "combat_skill" }, Description = "A missionary" });
		Add(new TrainingActionDefinition { Id = "a_02", DisplayName = "A Doggy Style", Category = TrainingCategory.AInsertion, ActionId = 41, FatigueDelta = 13, MoraleDelta = 0, XpTypes = new List<string> { "combat_skill" }, Description = "A doggy style" });
		Add(new TrainingActionDefinition { Id = "a_03", DisplayName = "A Face-to-Face Sitting", Category = TrainingCategory.AInsertion, ActionId = 42, FatigueDelta = 11, MoraleDelta = 1, XpTypes = new List<string> { "ranch_skill" }, Description = "A face-to-face sitting" });
		Add(new TrainingActionDefinition { Id = "a_04", DisplayName = "A Back-to-Back Sitting", Category = TrainingCategory.AInsertion, ActionId = 43, FatigueDelta = 11, MoraleDelta = 0, XpTypes = new List<string> { "ranch_skill" }, Description = "A back-to-back sitting" });
		Add(new TrainingActionDefinition { Id = "a_05", DisplayName = "A Cowgirl", Category = TrainingCategory.AInsertion, ActionId = 44, FatigueDelta = 13, MoraleDelta = 1, XpTypes = new List<string> { "combat_skill" }, Description = "A cowgirl" });
		Add(new TrainingActionDefinition { Id = "a_06", DisplayName = "A Face-to-Face Standing", Category = TrainingCategory.AInsertion, ActionId = 45, FatigueDelta = 14, MoraleDelta = 0, XpTypes = new List<string> { "combat_skill" }, Description = "A face-to-face standing" });
		Add(new TrainingActionDefinition { Id = "a_07", DisplayName = "A Back-to-Back Standing", Category = TrainingCategory.AInsertion, ActionId = 46, FatigueDelta = 14, MoraleDelta = 0, XpTypes = new List<string> { "combat_skill" }, Description = "A back-to-back standing" });
		Add(new TrainingActionDefinition { Id = "a_08", DisplayName = "Forced A Cowgirl", Category = TrainingCategory.AInsertion, ActionId = 47, FatigueDelta = 15, MoraleDelta = -3, XpTypes = new List<string> { "combat_skill" }, Description = "Forced A cowgirl" });

		// Penis Actions (9 actions)
		Add(new TrainingActionDefinition { Id = "penis_01", DisplayName = "Frottage", Category = TrainingCategory.PenisAction, ActionId = 50, FatigueDelta = 8, MoraleDelta = 3, XpTypes = new List<string> { "combat_skill" }, Description = "Frottage" });
		Add(new TrainingActionDefinition { Id = "penis_02", DisplayName = "Fellatio", Category = TrainingCategory.PenisAction, ActionId = 51, FatigueDelta = 7, MoraleDelta = 4, XpTypes = new List<string> { "craft_skill" }, Description = "Fellatio" });
		Add(new TrainingActionDefinition { Id = "penis_03", DisplayName = "Iratachio", Category = TrainingCategory.PenisAction, ActionId = 52, FatigueDelta = 10, MoraleDelta = 1, XpTypes = new List<string> { "craft_skill" }, Description = "Iratachio" });
		Add(new TrainingActionDefinition { Id = "penis_04", DisplayName = "Paizuri", Category = TrainingCategory.PenisAction, ActionId = 53, FatigueDelta = 9, MoraleDelta = 3, XpTypes = new List<string> { "craft_skill" }, Description = "Paizuri" });
		Add(new TrainingActionDefinition { Id = "penis_05", DisplayName = "Forced Paizuri", Category = TrainingCategory.PenisAction, ActionId = 54, FatigueDelta = 11, MoraleDelta = 0, XpTypes = new List<string> { "craft_skill" }, Description = "Forced paizuri" });
		Add(new TrainingActionDefinition { Id = "penis_06", DisplayName = "Vertical Paizuri", Category = TrainingCategory.PenisAction, ActionId = 55, FatigueDelta = 10, MoraleDelta = 2, XpTypes = new List<string> { "craft_skill" }, Description = "Vertical paizuri" });
		Add(new TrainingActionDefinition { Id = "penis_07", DisplayName = "Forced Vertical Paizuri", Category = TrainingCategory.PenisAction, ActionId = 56, FatigueDelta = 12, MoraleDelta = -1, XpTypes = new List<string> { "craft_skill" }, Description = "Forced vertical paizuri" });
		Add(new TrainingActionDefinition { Id = "penis_08", DisplayName = "Handjob", Category = TrainingCategory.PenisAction, ActionId = 57, FatigueDelta = 6, MoraleDelta = 4, XpTypes = new List<string> { "craft_skill" }, Description = "Handjob" });
		Add(new TrainingActionDefinition { Id = "penis_09", DisplayName = "Breastfeeding Handjob", Category = TrainingCategory.PenisAction, ActionId = 58, FatigueDelta = 7, MoraleDelta = 3, XpTypes = new List<string> { "craft_skill" }, Description = "Breastfeeding handjob" });

		// Tools (4 actions)
		Add(new TrainingActionDefinition { Id = "tool_01", DisplayName = "Livestock Milking Machine", Category = TrainingCategory.Tool, ActionId = 60, FatigueDelta = 5, MoraleDelta = 2, XpTypes = new List<string> { "ranch_skill" }, Description = "Livestock milking machine" });
		Add(new TrainingActionDefinition { Id = "tool_02", DisplayName = "Magic Milking Device", Category = TrainingCategory.Tool, ActionId = 61, FatigueDelta = 6, MoraleDelta = 3, XpTypes = new List<string> { "ranch_skill" }, Description = "Magic milking device" });
		Add(new TrainingActionDefinition { Id = "tool_03", DisplayName = "Tentacle Milking Device", Category = TrainingCategory.Tool, ActionId = 62, FatigueDelta = 7, MoraleDelta = 1, XpTypes = new List<string> { "ranch_skill" }, Description = "Tentacle milking device" });
		Add(new TrainingActionDefinition { Id = "tool_04", DisplayName = "Small Spirit Extraction Device", Category = TrainingCategory.Tool, ActionId = 63, FatigueDelta = 8, MoraleDelta = 0, XpTypes = new List<string> { "ranch_skill" }, Description = "Small spirit extraction device" });

		// Pain (1 action)
		Add(new TrainingActionDefinition { Id = "pain_01", DisplayName = "Spanking", Category = TrainingCategory.Pain, ActionId = 70, FatigueDelta = 10, MoraleDelta = -3, XpTypes = new List<string> { "combat_skill" }, Description = "Spanking" });

		// Tentacle (17 actions)
		Add(new TrainingActionDefinition { Id = "tentacle_01", DisplayName = "Tentacle Breast Massage", Category = TrainingCategory.Tentacle, ActionId = 100, FatigueDelta = 7, MoraleDelta = 2, XpTypes = new List<string> { "ranch_skill" }, Description = "Tentacle breast massage" });
		Add(new TrainingActionDefinition { Id = "tentacle_02", DisplayName = "Tentacle Breast Milking", Category = TrainingCategory.Tentacle, ActionId = 101, FatigueDelta = 8, MoraleDelta = 1, XpTypes = new List<string> { "ranch_skill" }, Description = "Tentacle breast milking" });
		Add(new TrainingActionDefinition { Id = "tentacle_03", DisplayName = "Petal Milking Tentacle", Category = TrainingCategory.Tentacle, ActionId = 102, FatigueDelta = 9, MoraleDelta = 0, XpTypes = new List<string> { "ranch_skill" }, Description = "Petal milking tentacle" });
		Add(new TrainingActionDefinition { Id = "tentacle_04", DisplayName = "Transparent Cup Tentacle", Category = TrainingCategory.Tentacle, ActionId = 103, FatigueDelta = 8, MoraleDelta = 1, XpTypes = new List<string> { "ranch_skill" }, Description = "Transparent cup tentacle" });
		Add(new TrainingActionDefinition { Id = "tentacle_05", DisplayName = "Milking Worm Tentacle", Category = TrainingCategory.Tentacle, ActionId = 104, FatigueDelta = 9, MoraleDelta = 0, XpTypes = new List<string> { "ranch_skill" }, Description = "Milking worm tentacle" });
		Add(new TrainingActionDefinition { Id = "tentacle_06", DisplayName = "Tentacle V Insertion", Category = TrainingCategory.Tentacle, ActionId = 105, FatigueDelta = 12, MoraleDelta = 0, XpTypes = new List<string> { "combat_skill" }, Description = "Tentacle V insertion" });
		Add(new TrainingActionDefinition { Id = "tentacle_07", DisplayName = "Tentacle A Insertion", Category = TrainingCategory.Tentacle, ActionId = 106, FatigueDelta = 12, MoraleDelta = 0, XpTypes = new List<string> { "combat_skill" }, Description = "Tentacle A insertion" });
		Add(new TrainingActionDefinition { Id = "tentacle_08", DisplayName = "Tentacle Forced Paizuri", Category = TrainingCategory.Tentacle, ActionId = 107, FatigueDelta = 11, MoraleDelta = -1, XpTypes = new List<string> { "craft_skill" }, Description = "Tentacle forced paizuri" });
		Add(new TrainingActionDefinition { Id = "tentacle_09", DisplayName = "Tentacle Fellatio", Category = TrainingCategory.Tentacle, ActionId = 108, FatigueDelta = 8, MoraleDelta = 1, XpTypes = new List<string> { "craft_skill" }, Description = "Tentacle fellatio" });
		Add(new TrainingActionDefinition { Id = "tentacle_10", DisplayName = "Tentacle Iratachio", Category = TrainingCategory.Tentacle, ActionId = 109, FatigueDelta = 10, MoraleDelta = -1, XpTypes = new List<string> { "craft_skill" }, Description = "Tentacle iratachio" });
		Add(new TrainingActionDefinition { Id = "tentacle_11", DisplayName = "Brush Tentacle", Category = TrainingCategory.Tentacle, ActionId = 110, FatigueDelta = 5, MoraleDelta = 3, XpTypes = new List<string> { "ranch_skill" }, Description = "Brush tentacle" });
		Add(new TrainingActionDefinition { Id = "tentacle_12", DisplayName = "Tentacle Blindfold", Category = TrainingCategory.Tentacle, ActionId = 112, FatigueDelta = 4, MoraleDelta = 2, XpTypes = new List<string> { "ranch_skill" }, Description = "Tentacle blindfold" });
		Add(new TrainingActionDefinition { Id = "tentacle_13", DisplayName = "Tentacle Clit Sucking", Category = TrainingCategory.Tentacle, ActionId = 113, FatigueDelta = 8, MoraleDelta = 2, XpTypes = new List<string> { "craft_skill" }, Description = "Tentacle clit sucking" });
		Add(new TrainingActionDefinition { Id = "tentacle_14", DisplayName = "Tentacle Ear Rape", Category = TrainingCategory.Tentacle, ActionId = 114, FatigueDelta = 9, MoraleDelta = 0, XpTypes = new List<string> { "craft_skill" }, Description = "Tentacle ear rape" });
		Add(new TrainingActionDefinition { Id = "tentacle_15", DisplayName = "Tentacle Forced Handjob", Category = TrainingCategory.Tentacle, ActionId = 115, FatigueDelta = 10, MoraleDelta = -1, XpTypes = new List<string> { "craft_skill" }, Description = "Tentacle forced handjob" });
		Add(new TrainingActionDefinition { Id = "tentacle_16", DisplayName = "Suction Milking Tentacle", Category = TrainingCategory.Tentacle, ActionId = 116, FatigueDelta = 7, MoraleDelta = 1, XpTypes = new List<string> { "ranch_skill" }, Description = "Suction milking tentacle" });
		Add(new TrainingActionDefinition { Id = "tentacle_17", DisplayName = "Mammary Gland Invasion Tentacle", Category = TrainingCategory.Tentacle, ActionId = 117, FatigueDelta = 10, MoraleDelta = -1, XpTypes = new List<string> { "ranch_skill" }, Description = "Mammary gland invasion tentacle" });

		// Massage (3 actions)
		Add(new TrainingActionDefinition { Id = "massage_01", DisplayName = "Breast Growth Massage", Category = TrainingCategory.Massage, ActionId = 150, FatigueDelta = 6, MoraleDelta = 3, XpTypes = new List<string> { "ranch_skill" }, Description = "Breast growth massage" });
		Add(new TrainingActionDefinition { Id = "massage_02", DisplayName = "Rich Milk Massage", Category = TrainingCategory.Massage, ActionId = 151, FatigueDelta = 7, MoraleDelta = 2, XpTypes = new List<string> { "ranch_skill" }, Description = "Rich milk massage" });
		Add(new TrainingActionDefinition { Id = "massage_03", DisplayName = "Milk Tank Massage", Category = TrainingCategory.Massage, ActionId = 152, FatigueDelta = 8, MoraleDelta = 1, XpTypes = new List<string> { "ranch_skill" }, Description = "Milk tank massage" });

		// Items (17 actions)
		Add(new TrainingActionDefinition { Id = "item_01", DisplayName = "Vibrator", Category = TrainingCategory.Item, ActionId = 201, FatigueDelta = 6, MoraleDelta = 3, XpTypes = new List<string> { "craft_skill" }, Description = "Vibrator" });
		Add(new TrainingActionDefinition { Id = "item_02", DisplayName = "Anal Vibrator", Category = TrainingCategory.Item, ActionId = 202, FatigueDelta = 7, MoraleDelta = 2, XpTypes = new List<string> { "craft_skill" }, Description = "Anal vibrator" });
		Add(new TrainingActionDefinition { Id = "item_03", DisplayName = "Nipple Rotor", Category = TrainingCategory.Item, ActionId = 203, FatigueDelta = 5, MoraleDelta = 3, XpTypes = new List<string> { "craft_skill" }, Description = "Nipple rotor" });
		Add(new TrainingActionDefinition { Id = "item_04", DisplayName = "Clit Rotor", Category = TrainingCategory.Item, ActionId = 204, FatigueDelta = 5, MoraleDelta = 3, XpTypes = new List<string> { "craft_skill" }, Description = "Clit rotor" });
		Add(new TrainingActionDefinition { Id = "item_05", DisplayName = "Nipple Suction Device", Category = TrainingCategory.Item, ActionId = 205, FatigueDelta = 6, MoraleDelta = 2, XpTypes = new List<string> { "ranch_skill" }, Description = "Nipple suction device" });
		Add(new TrainingActionDefinition { Id = "item_06", DisplayName = "Clit Suction Device", Category = TrainingCategory.Item, ActionId = 206, FatigueDelta = 5, MoraleDelta = 3, XpTypes = new List<string> { "craft_skill" }, Description = "Clit suction device" });
		Add(new TrainingActionDefinition { Id = "item_07", DisplayName = "Eye Mask", Category = TrainingCategory.Item, ActionId = 207, FatigueDelta = 3, MoraleDelta = 2, XpTypes = new List<string> { "ranch_skill" }, Description = "Eye mask" });
		Add(new TrainingActionDefinition { Id = "item_08", DisplayName = "Mouth Gag", Category = TrainingCategory.Item, ActionId = 208, FatigueDelta = 4, MoraleDelta = 1, XpTypes = new List<string> { "ranch_skill" }, Description = "Mouth gag" });
		Add(new TrainingActionDefinition { Id = "item_09", DisplayName = "Ball Gag", Category = TrainingCategory.Item, ActionId = 209, FatigueDelta = 3, MoraleDelta = 1, XpTypes = new List<string> { "ranch_skill" }, Description = "Ball gag" });
		Add(new TrainingActionDefinition { Id = "item_10", DisplayName = "Forced Mouth Opener", Category = TrainingCategory.Item, ActionId = 212, FatigueDelta = 5, MoraleDelta = -2, XpTypes = new List<string> { "ranch_skill" }, Description = "Forced mouth opener" });
		Add(new TrainingActionDefinition { Id = "item_11", DisplayName = "Lotion", Category = TrainingCategory.Item, ActionId = 250, FatigueDelta = 2, MoraleDelta = 1, XpTypes = new List<string> { "craft_skill" }, Description = "Lotion" });
		Add(new TrainingActionDefinition { Id = "item_12", DisplayName = "Aphrodisiac", Category = TrainingCategory.Item, ActionId = 251, FatigueDelta = 3, MoraleDelta = 2, XpTypes = new List<string> { "craft_skill" }, Description = "Aphrodisiac" });
		Add(new TrainingActionDefinition { Id = "item_13", DisplayName = "Condom", Category = TrainingCategory.Item, ActionId = 252, FatigueDelta = 1, MoraleDelta = 0, XpTypes = new List<string> { "craft_skill" }, Description = "Condom" });
		Add(new TrainingActionDefinition { Id = "item_14", DisplayName = "Energy Drink", Category = TrainingCategory.Item, ActionId = 253, FatigueDelta = -5, MoraleDelta = 1, XpTypes = new List<string> { "ranch_skill" }, Description = "Energy drink" });
		Add(new TrainingActionDefinition { Id = "item_15", DisplayName = "V Lotion", Category = TrainingCategory.Item, ActionId = 290, FatigueDelta = 2, MoraleDelta = 1, XpTypes = new List<string> { "craft_skill" }, Description = "V lotion" });
		Add(new TrainingActionDefinition { Id = "item_16", DisplayName = "A Lotion", Category = TrainingCategory.Item, ActionId = 291, FatigueDelta = 2, MoraleDelta = 1, XpTypes = new List<string> { "craft_skill" }, Description = "A lotion" });
		Add(new TrainingActionDefinition { Id = "item_17", DisplayName = "B Lotion", Category = TrainingCategory.Item, ActionId = 292, FatigueDelta = 2, MoraleDelta = 1, XpTypes = new List<string> { "craft_skill" }, Description = "B lotion" });

		// Body Mod (4 actions)
		Add(new TrainingActionDefinition { Id = "bodymod_01", DisplayName = "Mark Infusion", Category = TrainingCategory.BodyMod, ActionId = 502, FatigueDelta = 10, MoraleDelta = -5, XpTypes = new List<string> { "combat_skill" }, Description = "Mark infusion" });
		Add(new TrainingActionDefinition { Id = "bodymod_02", DisplayName = "Pleasure-Pain Conversion", Category = TrainingCategory.BodyMod, ActionId = 503, FatigueDelta = 8, MoraleDelta = -3, XpTypes = new List<string> { "combat_skill" }, Description = "Pleasure-pain conversion" });
		Add(new TrainingActionDefinition { Id = "bodymod_03", DisplayName = "Penis Change", Category = TrainingCategory.BodyMod, ActionId = 504, FatigueDelta = 15, MoraleDelta = -10, XpTypes = new List<string> { "combat_skill" }, Description = "Penis change" });
		Add(new TrainingActionDefinition { Id = "bodymod_04", DisplayName = "Time Compression", Category = TrainingCategory.BodyMod, ActionId = 505, FatigueDelta = 12, MoraleDelta = -5, XpTypes = new List<string> { "combat_skill" }, Description = "Time compression" });

		// Magic (19 actions)
		Add(new TrainingActionDefinition { Id = "magic_01", DisplayName = "Aphrodisiac Slime", Category = TrainingCategory.ForbiddenMagic, ActionId = 550, FatigueDelta = 5, MoraleDelta = 2, XpTypes = new List<string> { "craft_skill" }, Description = "Aphrodisiac slime" });
		Add(new TrainingActionDefinition { Id = "magic_02", DisplayName = "Brainwashing Tentacle", Category = TrainingCategory.ForbiddenMagic, ActionId = 580, FatigueDelta = 15, MoraleDelta = -10, XpTypes = new List<string> { "combat_skill" }, Description = "Brainwashing tentacle" });
		Add(new TrainingActionDefinition { Id = "magic_03", DisplayName = "Orgasm HP Recovery Mark", Category = TrainingCategory.ForbiddenMagic, ActionId = 581, FatigueDelta = -10, MoraleDelta = 5, XpTypes = new List<string> { "combat_skill" }, Description = "Orgasm HP recovery mark" });
		Add(new TrainingActionDefinition { Id = "magic_04", DisplayName = "Orgasm Mana Recovery Mark", Category = TrainingCategory.ForbiddenMagic, ActionId = 582, FatigueDelta = -8, MoraleDelta = 3, XpTypes = new List<string> { "combat_skill" }, Description = "Orgasm mana recovery mark" });
		Add(new TrainingActionDefinition { Id = "magic_05", DisplayName = "Body Modification", Category = TrainingCategory.ForbiddenMagic, ActionId = 800, FatigueDelta = 12, MoraleDelta = -8, XpTypes = new List<string> { "combat_skill" }, Description = "Body modification" });
		Add(new TrainingActionDefinition { Id = "magic_06", DisplayName = "Mark Infusion (Magic)", Category = TrainingCategory.ForbiddenMagic, ActionId = 801, FatigueDelta = 10, MoraleDelta = -5, XpTypes = new List<string> { "combat_skill" }, Description = "Mark infusion magic" });
		Add(new TrainingActionDefinition { Id = "magic_07", DisplayName = "Penis Removal", Category = TrainingCategory.ForbiddenMagic, ActionId = 890, FatigueDelta = 10, MoraleDelta = -3, XpTypes = new List<string> { "combat_skill" }, Description = "Penis removal" });
		Add(new TrainingActionDefinition { Id = "magic_08", DisplayName = "Milking Machine Removal", Category = TrainingCategory.ForbiddenMagic, ActionId = 891, FatigueDelta = 5, MoraleDelta = 2, XpTypes = new List<string> { "ranch_skill" }, Description = "Milking machine removal" });
		Add(new TrainingActionDefinition { Id = "magic_09", DisplayName = "Milking Tentacle Removal", Category = TrainingCategory.ForbiddenMagic, ActionId = 892, FatigueDelta = 6, MoraleDelta = 1, XpTypes = new List<string> { "ranch_skill" }, Description = "Milking tentacle removal" });
		Add(new TrainingActionDefinition { Id = "magic_10", DisplayName = "Breast Cleaning", Category = TrainingCategory.ForbiddenMagic, ActionId = 893, FatigueDelta = 3, MoraleDelta = 2, XpTypes = new List<string> { "ranch_skill" }, Description = "Breast cleaning" });
		Add(new TrainingActionDefinition { Id = "magic_11", DisplayName = "Condom Removal", Category = TrainingCategory.ForbiddenMagic, ActionId = 895, FatigueDelta = 2, MoraleDelta = 1, XpTypes = new List<string> { "ranch_skill" }, Description = "Condom removal" });
		Add(new TrainingActionDefinition { Id = "magic_12", DisplayName = "Piston", Category = TrainingCategory.ForbiddenMagic, ActionId = 900, FatigueDelta = 14, MoraleDelta = 0, XpTypes = new List<string> { "combat_skill" }, Description = "Piston" });
		Add(new TrainingActionDefinition { Id = "magic_13", DisplayName = "Breast Sucking Piston", Category = TrainingCategory.ForbiddenMagic, ActionId = 901, FatigueDelta = 12, MoraleDelta = 1, XpTypes = new List<string> { "ranch_skill" }, Description = "Breast sucking piston" });
		Add(new TrainingActionDefinition { Id = "magic_14", DisplayName = "Milk Sucking", Category = TrainingCategory.ForbiddenMagic, ActionId = 902, FatigueDelta = 10, MoraleDelta = 2, XpTypes = new List<string> { "ranch_skill" }, Description = "Milk sucking" });
		Add(new TrainingActionDefinition { Id = "magic_15", DisplayName = "Tentacle Milk Sucking", Category = TrainingCategory.ForbiddenMagic, ActionId = 903, FatigueDelta = 11, MoraleDelta = 1, XpTypes = new List<string> { "ranch_skill" }, Description = "Tentacle milk sucking" });
		Add(new TrainingActionDefinition { Id = "magic_16", DisplayName = "Penis Training", Category = TrainingCategory.ForbiddenMagic, ActionId = 904, FatigueDelta = 8, MoraleDelta = -2, XpTypes = new List<string> { "combat_skill" }, Description = "Penis training" });
		Add(new TrainingActionDefinition { Id = "magic_17", DisplayName = "Spirit Power Injection", Category = TrainingCategory.ForbiddenMagic, ActionId = 905, FatigueDelta = -8, MoraleDelta = 3, XpTypes = new List<string> { "combat_skill" }, Description = "Spirit power injection" });
		Add(new TrainingActionDefinition { Id = "magic_18", DisplayName = "Mana Injection", Category = TrainingCategory.ForbiddenMagic, ActionId = 906, FatigueDelta = -6, MoraleDelta = 2, XpTypes = new List<string> { "combat_skill" }, Description = "Mana injection" });
		Add(new TrainingActionDefinition { Id = "magic_19", DisplayName = "Force Service", Category = TrainingCategory.ForbiddenMagic, ActionId = 907, FatigueDelta = 12, MoraleDelta = -5, XpTypes = new List<string> { "craft_skill" }, Description = "Force service" });
		Add(new TrainingActionDefinition { Id = "magic_20", DisplayName = "Mana Sucking", Category = TrainingCategory.ForbiddenMagic, ActionId = 910, FatigueDelta = 10, MoraleDelta = -3, XpTypes = new List<string> { "combat_skill" }, Description = "Mana sucking" });
	}

	private void Add(CharacterDefinition definition)
	{
		definition.ResourceName = definition.Id;
		Characters[definition.Id] = definition;
	}
	private void Add(JobDefinition definition)
	{
		definition.ResourceName = definition.Id;
		Jobs[definition.Id] = definition;
	}
	private void Add(ItemDefinition definition)
	{
		definition.ResourceName = definition.Id;
		Items[definition.Id] = definition;
	}
	private void Add(FacilityDefinition definition)
	{
		definition.ResourceName = definition.Id;
		Facilities[definition.Id] = definition;
	}
	private void Add(MissionDefinition definition)
	{
		definition.ResourceName = definition.Id;
		Missions[definition.Id] = definition;
	}
	private void Add(MilestoneDefinition definition)
	{
		definition.ResourceName = definition.Id;
		Milestones[definition.Id] = definition;
	}
	private void Add(SkillDefinition definition)
	{
		definition.ResourceName = definition.Id;
		Skills[definition.Id] = definition;
	}
	private void Add(PetDefinition definition)
	{
		definition.ResourceName = definition.Id;
		Pets[definition.Id] = definition;
	}
	private void Add(EnemyDefinition definition)
	{
		definition.ResourceName = definition.Id;
		Enemies[definition.Id] = definition;
	}
	private void Add(BondEventDefinition definition)
	{
		definition.ResourceName = definition.Id;
		BondEvents[definition.Id] = definition;
	}
	private void Add(TalentDefinition definition)
	{
		definition.ResourceName = definition.Id;
		Talents[definition.Id] = definition;
	}
	private void Add(TrainingActionDefinition definition)
	{
		definition.ResourceName = definition.Id;
		TrainingActions[definition.Id] = definition;
	}
}
