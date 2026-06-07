using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenMakaiRanch.Gameplay;

public static class CharacterGenerationPools
{
    // === Name Pools (from Str.csv sections 10000+) ===
    // Japanese names (10000-10999)
    public static readonly string[] JapaneseNames =
    {
        "Aoi", "Yuki", "Hinata", "Sakura", "Mei", "Rin", "Hana", "Miyu", "Erika", "Nana",
        "Koharu", "Akari", "Misaki", "Saki", "Risa", "Miyabi", "Kana", "Yuna", "Mao", "Riko",
        "Sora", "Airi", "Miki", "Natsumi", "Noa", "Moe", "Chiharu", "Asuka", "Runa",
        "Suzu", "Fuyu", "Kiri", "Toki", "Mizu", "Kaze", "Tori", "Umi", "Nagi", "Shizu",
        "Aika", "Aiko", "Akiha", "Asako", "Asano", "Asami", "Azami", "Azusa", "Azumi",
        "Atsuko", "Amane", "Aya", "Ayaka", "Ayane", "Ayame", "Ayu", "Ayumi", "Arisa",
        "Anzu", "Io", "Iori", "Isuzu", "Izumi", "Itsuki", "Ito", "Inori", "Ibuki", "Iroha",
        "Uzuki", "Urara", "Eri", "Eriko", "Otome", "Kaede", "Kaori", "Kasumi", "Kana",
        "Kanae", "Kanade", "Kaya", "Kanna", "Kikyo", "Kyoka", "Kyoko", "Kirika", "Kiriko",
        "Kirin", "Kumi", "Kumiko", "Kurumi", "Koko", "Kokoa", "Kokoro", "Konatsu", "Komugi",
        "Koyuki", "Saeko", "Saori", "Saki", "Satsuki", "Satomi", "Sana", "Sanae", "Saya",
        "Sayaka", "Sayuri", "Sayo", "Sayori", "Sara", "Sawa", "Sango", "Shiori", "Shion",
        "Shigure", "Shizuka", "Shizuru", "Shino", "Shinobu", "Shiho", "Shuko", "Junko",
        "Shiranui", "Suiren", "Suzuka", "Suzuna", "Suzune", "Suzume", "Suzuran", "Subaru",
        "Sumire", "Seiko", "Setsuna", "Serina", "Sen", "Sora", "Tae", "Taeko", "Takumi",
        "Tamao", "Tamaki", "Tamako", "Tamayo", "Chiaki", "Chieri", "Chika", "Chikage",
        "Chise", "Chizuru", "Chitose", "Chihaya", "Chiyo", "Tsukiko", "Tsugumi", "Tsubaki",
        "Tsumugi", "Tsuyu", "Toka", "Tomo", "Tomoe", "Tomoka", "Nao", "Nagi", "Nazuna",
        "Natsumi", "Natsume", "Nadeshiko", "Nana", "Nanaka", "Nanako", "Nanase", "Nanami",
        "Nami", "Nene", "Neneka", "Nozomi", "Hazuki", "Hatsune", "Hanako", "Hanayo", "Haruka"
    };

    public static readonly string[] EnglishNames =
    {
        "Alice", "Lily", "Rose", "Daisy", "Ivy", "Violet", "Hazel", "Jasmine", "Holly", "Willow",
        "Emma", "Olivia", "Sophia", "Charlotte", "Amelia", "Ella", "Grace", "Chloe", "Victoria", "Audrey",
        "Claire", "Ruby", "Pearl", "Sapphire", "Crystal", "Amber", "Scarlet", "Jade", "Opal", "Iris",
        "Fiona", "Gwen", "Morgan", "Brianna", "Catherine", "Eleanor", "Margaret", "Elizabeth", "Anna", "Laura",
        "Emily", "Abigail", "Harriet", "Beatrix", "Cecilia", "Diana", "Florence", "Georgia", "Henrietta", "Isabel",
        "Julia", "Katherine", "Lydia", "Matilda", "Nora", "Penelope", "Rosalind", "Sylvia", "Tabitha", "Ursula",
        "Vivian", "Wendy", "Xenia", "Yvonne", "Zara", "Adelaide", "Beatrice", "Constance", "Dorothy", "Edith"
    };

    public static readonly string[] FrenchNames =
    {
        "Camille", "Marie", "Julie", "Morgane", "Amelie", "Chloe", "Elodie", "Manon", "Sarah", "Lea",
        "Lucie", "Pauline", "Helene", "Sophie", "Isabelle", "Celine", "Nathalie", "Valerie", "Dominique", "Brigitte",
        "Adrienne", "Bernadette", "Colette", "Danielle", "Francoise", "Genevieve", "Henriette", "Josette", "Lucienne", "Marguerite",
        "Nicole", "Odette", "Paulette", "Renee", "Simone", "Therese", "Vivienne", "Yvette", "Adele", "Celeste"
    };

    public static readonly string[] GermanNames =
    {
        "Gretchen", "Heidi", "Ingrid", "Klara", "Liesel", "Brunhilde", "Adelheid", "Petra", "Ursula", "Sabine",
        "Anneliese", "Frieda", "Gisela", "Hildegard", "Lotte", "Marta", "Regina", "Therese", "Wilma", "Erika",
        "Astrid", "Brigitta", "Dietlinde", "Elfriede", "Gertrud", "Hannelore", "Irmgard", "Jutta", "Kunigunde", "Lorelei"
    };

    public static readonly string[] ItalianNames =
    {
        "Alessandra", "Beatrice", "Carlotta", "Domenica", "Eleonora", "Fiorella", "Gabriella", "Isa", "Lorenza", "Mariella",
        "Natalia", "Ottavia", "Paola", "Raffaella", "Serafina", "Teresa", "Valentina", "Zia", "Aria", "Bella",
        "Angelina", "Bianca", "Caterina", "Donatella", "Elena", "Francesca", "Gianna", "Helena", "Isabella", "Lucia"
    };

    public static readonly string[] RussianNames =
    {
        "Anastasia", "Dasha", "Ekaterina", "Galina", "Irina", "Katya", "Larisa", "Masha", "Natasha",
        "Olga", "Raisa", "Svetlana", "Tatiana", "Ulyana", "Vera", "Yulia", "Zoya", "Nikol", "Sasha",
        "Alina", "Boris", "Dina", "Elina", "Faina", "Kira", "Lena", "Marina", "Nina", "Polina", "Tamara"
    };

    public static readonly string[] FantasyNames =
    {
        "Aelindra", "Briar", "Cinder", "Dusk", "Elara", "Fable", "Glimmer", "Hollow", "Ivy", "Jade",
        "Kestrel", "Lark", "Meadow", "Nyx", "Orchid", "Petal", "Quill", "Raven", "Sage", "Thorn",
        "Umbra", "Vale", "Wisp", "Xanthe", "Yarrow", "Zephyr", "Aster", "Bramble", "Clover", "Dew"
    };

    // === Racial Pools (from Str.csv sections 1000+) ===
    public static readonly string[] Races =
    {
        "Human", "Elf", "Halfling", "Spirit", "Half-Elf", "Dark Elf", "Winged",
        "Dogfolk", "Catfolk", "Rabbitfolk", "Foxfolk", "Tanukifolk",
        "Deerfolk", "Cowfolk", "Sheepfolk",
        "Sage", "Dragonkin", "Angel", "Celestial", "Youkai", "Fox Spirit",
        "Vampire", "Dhampir", "Homunculus", "Goddess"
    };

    public static readonly string[] RemovedRaces =
    {
        "Demon", "Succubus", "Oni", "Slime", "Harpy", "Lamias", "Goblin", "Orc",
        "Troll", "Fairy", "Djinn", "Golem", "Phantom", "Shapeshifter", "Lizardfolk"
    };

    public static readonly string[] RaceCategories =
    {
        "Humanoid", "Elf", "Halfling", "Spirit", "Half-Elf", "DarkElf", "Winged",
        "Beast", "Beast", "Beast", "Beast", "Beast",
        "Beast", "Beast", "Beast",
        "Sage", "Dragon", "Divine", "Divine", "Spirit", "Spirit",
        "Undead", "Undead", "Artificial", "Divine"
    };

    // === Job/Class Pools (from Str.csv sections 4000+) ===
    // 4000-4009: Standard jobs
    public static readonly string[] StandardJobs =
    {
        "Traveler", "Swordsman", "Warrior", "Knight", "Archer", "Gladiator",
        "Hunter", "Artillerist", "Priest", "Thief"
    };

    // 4100-4106: Magic-related
    public static readonly string[] MagicJobs =
    {
        "White Mage", "Black Mage", "Cursemancer", "Witch", "Necromancer",
        "Alchemist", "Apothecary"
    };

    // 4200-4207: Adventurer / combat
    public static readonly string[] AdventurerJobs =
    {
        "Soldier", "Adventurer", "Martial Artist", "Sniper", "Magic Knight",
        "Dark Knight", "Heavy Armor", "Hero (Self-Proclaimed)"
    };

    // 4300-4307: Elite / special
    public static readonly string[] EliteJobs =
    {
        "Assassin", "Kunoichi", "Samurai", "Battle Sister", "Battle Maid",
        "Exorcist Priestess", "Magical Girl", "Onmyoji", "Paladin"
    };

    // 4400-4408: Civilian
    public static readonly string[] CivilianJobs =
    {
        "Maid", "Sister", "Scholar", "Merchant", "Blacksmith",
        "Shrine Maiden", "Chef", "Shepherd", "Cowherd"
    };

    // 4500-4513: High-end / advanced
    public static readonly string[] AdvancedJobs =
    {
        "Paladin", "Bishop", "Valkyrie", "Black Knight", "Sage",
        "Cardinal", "Time Mage", "Hero", "Warlord", "Archmage",
        "Summoner", "Beast Tamer", "Strategist", "Saint"
    };

    // 4600-4607: Auction house (noble background)
    public static readonly string[] NobleJobs =
    {
        "Priest", "White Mage", "Black Mage", "Paladin", "Scholar",
        "Holy Knight", "Bishop", "Cardinal"
    };

    // 4700-4702: Non-random special jobs
    public static readonly string[] SpecialJobs =
    {
        "Princess", "Shrine Princess", "Palace Knight"
    };

    public static string[] AllJobs => StandardJobs.Concat(MagicJobs).Concat(AdventurerJobs)
        .Concat(EliteJobs).Concat(CivilianJobs).Concat(AdvancedJobs).Concat(NobleJobs).ToArray();

    // === Body Type Pools ===
    public static readonly string[] BodyTypes =
    {
        "Standard", "Slender", "Athletic", "Curvy", "Voluptuous", "Petite", "Statuesque",
        "Amazonian", "Delicate", "Stout", "Lanky", "Busty", "Slim", "Thick",
        "Glamorous", "Plump", "Chubby", "Chubby", "Muscular", "Skinny"
    };

    // Str.csv 0-4: Height descriptors
    public static readonly string[] HeightDescriptors =
    {
        "Standard", "Petite", "Short", "Tall", "Childlike"
    };

    // Str.csv 50-55: Body shape
    public static readonly string[] BodyShapes =
    {
        "Standard", "Glamorous", "Voluptuous", "Plump", "Skinny", "Slender"
    };

    // Str.csv 100-105: Skin colors
    public static readonly string[] SkinColors =
    {
        "Fair", "Pale", "Porcelain", "Albino", "Tan", "Wheat",
        "Standard", "White", "Light", "Olive", "Brown", "Dark", "Ebony",
        "Peach", "Golden", "Caramel", "Chocolate"
    };

    // Str.csv 150-161: Eye colors
    public static readonly string[] EyeColors =
    {
        "Black", "Gold", "Silver", "Red", "Pink", "Blue", "Green", "Brown",
        "Purple", "White", "Orange", "Sky Blue", "Hazel", "Amber", "Gray",
        "Crimson", "Violet", "Teal", "Heterochromatic", "Glowing"
    };

    // Str.csv 200-211: Hair colors
    public static readonly string[] HairColors =
    {
        "Black", "Blonde", "Chestnut", "Red", "Pink", "Blue", "Green", "Silver",
        "Purple", "White", "Orange", "Sky Blue", "Brown", "Auburn", "Platinum",
        "Gray", "Crimson", "Rose", "Azure", "Cyan", "Teal", "Mint", "Rainbow", "Two-Tone"
    };

    // Str.csv 300-311: Hair features
    public static readonly string[] HairFeatures =
    {
        "Straight Bangs", "One Eye Hidden", "Blindfold Hair", "Hime Cut", "Intake",
        "Center Part", "Forehead Out", "Straight", "Natural", "Side Flip",
        "Wave", "Airy", "Wavy", "Curly", "Frizzy", "Silky", "Voluminous",
        "Thin", "Thick", "Shiny", "Dull", "Fluffy", "Sleek", "Messy", "Braided"
    };

    // Str.csv 500-523: Hair styles
    public static readonly string[] HairStyles =
    {
        "Long", "Semi-Long", "Short", "Ponytail", "Twin Tails", "Side Tail",
        "Bob Cut", "Two-Side Up", "Half-Up", "Braid", "Very Long",
        "Pigtails", "Single Knot", "Double Knots", "Curled", "Back Bun",
        "Side Bun", "Twin Rings", "Pompadour", "One Length", "Fishbone",
        "One-Side Up", "Crown Braid", "Tri-Tail",
        "Pixie Cut", "Undercut", "Sidecut", "Mohawk", "Buzz Cut", "Bald",
        "Topknot", "Man Bun", "Chignon", "Updo", "French Braid", "Dutch Braid",
        "Cornrows", "Dreadlocks", "Afro", "Permed", "Straight Bangs", "Side Bangs", "Blunt Bangs"
    };

    // Str.csv 800-802: Eye shapes
    public static readonly string[] EyeShapes =
    {
        "Star Eyes", "Heart Eyes", "Cross Eyes"
    };

    // Str.csv 2000-2003: Eye characteristics
    public static readonly string[] EyeCharacteristics =
    {
        "Droopy Eyes", "Sharp Eyes", "Thread Eyes", "Half-Closed Eyes"
    };

    // Str.csv 1000-1014: Normal races
    // Str.csv 1200-1209: Special races

    // === Personality Pools (from Str.csv sections 3000-3011) ===
    public static readonly string[] Personalities =
    {
        "Bold", "Timid", "Cheeky", "Earnest", "Quiet", "Carefree", "Naive",
        "Noble", "Pure", "Lustful", "Gentle", "Reserved",
        "Serious", "Cheerful", "Proud", "Humble", "Mischievous", "Kind",
        "Cold", "Warm", "Lazy", "Diligent", "Playful", "Stoic",
        "Passionate", "Outgoing", "Shy", "Confident", "Insecure",
        "Optimistic", "Pessimistic", "Calm", "Anxious", "Brave", "Cowardly",
        "Honest", "Deceptive", "Loyal", "Fickle", "Patient", "Impulsive",
        "Creative", "Practical", "Idealistic", "Cynical", "Naive", "Worldly",
        "Innocent", "Mysterious", "Charming", "Blunt", "Diplomatic",
        "Rebellious", "Dutiful", "Free-spirited", "Disciplined", "Relaxed"
    };

    // Str.csv 3200-3202: Social status
    public static readonly string[] SocialStatuses =
    {
        "Commoner", "Noble", "Royal"
    };

    // === Talent/Trait Pools (Talent.csv sections 10+, 50+, 100+, 200+) ===
    public static readonly string[] PositiveTraits =
    {
        "Brave", "Diligent", "Kind", "Patient", "Creative", "Loyal", "Honest", "Optimistic",
        "Calm", "Confident", "Diplomatic", "Disciplined", "Dutiful", "Gentle", "Humble",
        "Passionate", "Playful", "Quick Learner", "Resilient", "Perceptive", "Adaptable",
        "Charismatic", "Empathetic", "Generous", "Graceful", "Intuitive", "Resourceful",
        "Pain Resistant", "Gets Wet Easily", "Fast Recovery", "Big Eater", "Quick Study",
        "Liked by Animals", "Strong Nose"
    };

    public static readonly string[] NegativeTraits =
    {
        "Cowardly", "Lazy", "Cruel", "Impulsive", "Deceptive", "Fickle", "Cynical",
        "Anxious", "Insecure", "Pessimistic", "Rebellious", "Stubborn", "Greedy",
        "Jealous", "Vain", "Arrogant", "Wasteful", "Clumsy", "Naive", "Blunt",
        "Melancholic", "Suspicious", "Possessive", "Vindictive", "Indecisive",
        "Pain Weakness", "Gets Dry Easily", "Slow Recovery", "Small Eater", "Slow Study",
        "Disliked by Animals", "Weak Nose"
    };

    // Talent.csv 50-57: Racial physical traits
    public static readonly string[] RacialBodyTraits =
    {
        "Demon Realm Race", "Heavenly Race", "Divine Being",
        "Animal Ears", "Tail", "Two Horns", "Elf Ears", "Halfling Ears"
    };

    // Talent.csv 70-77: Individual physical traits
    public static readonly string[] BodyTraits =
    {
        "Demon Realm Adaptation", "Drug Resistance", "Sunlight Weakness", "Cat Tongue",
        "Serrated Teeth", "Inverted Nipples", "Puffy Nipples", "Baby Face",
        "Animal Ears", "Tail", "Horns", "Fangs", "Elf Ears", "Pointed Ears", "Wings",
        "Scales", "Fur", "Feathers", "Glowing Eyes", "Forked Tongue", "Claws", "Hooves",
        "Slit Pupils", "Elongated Limbs", "Translucent Skin", "Bioluminescent Markings"
    };

    // Talent.csv 200-214: Skills / expertise
    public static readonly string[] SkillTraits =
    {
        "Teaching", "Sex Knowledge", "Alchemy", "Engineering", "Adult Toy Knowledge",
        "Medical Knowledge", "Plant Knowledge", "Pharmacology", "Cooking", "Cleaning",
        "Crafting", "Herbalism", "Smithing", "Tailoring", "Carpentry", "Brewing",
        "Animal Handling", "Horticulture", "Fishing", "Hunting", "Tracking", "Stealth",
        "Persuasion", "Intimidation", "Leadership", "Strategy", "Tactics", "Diplomacy"
    };

    // Talent.csv 30-34: Innate sexual traits
    public static readonly string[] InnateSexualTraits =
    {
        "Famous Make", "Anal Famous Make", "Innate Milk Constitution",
        "Oral Maiden", "Supreme Breast Pressure"
    };

    public static readonly string[] SexualTraits =
    {
        "Virginity Barrier", "Chastity Belt", "Innate Milk Constitution", "Magic Milk Constitution",
        "Oral Maiden", "Supreme Breast Pressure", "Fertile", "Barren", "Sensitive Nipples",
        "Sensitive Clit", "Tight Vagina", "Tight Anus", "Lactating", "Highly Fertile",
        "Early Bloomer", "Late Bloomer", "Leaky", "Dry", "Nymphomaniac", "Frigid"
    };

    public static readonly string[] PreferenceTraits =
    {
        "Likes Animals", "Likes Cooking", "Likes Cleaning", "Likes Reading", "Likes Plants",
        "Likes Machines", "Likes Experiments", "Likes Fashion", "Likes Baths",
        "Likes Sweets", "Likes Spicy", "Likes Sour", "Likes Tea", "Likes Wine",
        "Likes Meat", "Likes Fish", "Likes Vegetables", "Likes Milk"
    };

    public static readonly string[] TraumaTraits =
    {
        "Phobia: Demons", "Phobia: Tentacles", "Phobia: Goblins", "Phobia: Orcs",
        "Phobia: Slimes", "Phobia: Worms", "Phobia: Needles", "Phobia: Darkness",
        "Phobia: Enclosed Spaces", "Phobia: Heights", "Phobia: Water", "Phobia: Fire"
    };

    // === Breast Size Pool (from Talent.csv sections 10+) ===
    public static readonly string[] BreastSizeLabels =
    {
        "Flat", "Small", "Medium", "Large", "Busty", "Voluptuous", "Massive", "Hyper"
    };

    public static readonly int[] BreastSizeValues = { 0, 2, 4, 6, 8, 10, 12, 15 };

    // === Height Pool (from Str.csv 6500+) ===
    public static readonly (string Label, int Min, int Max)[] HeightRanges =
    {
        ("Very Short", 1300, 1450),
        ("Short", 1451, 1540),
        ("Average", 1541, 1650),
        ("Tall", 1651, 1750),
        ("Very Tall", 1751, 1900),
        ("Imposing", 1901, 2100)
    };

    // === Apparent Age Pool ===
    public static readonly (string Label, int Age)[] ApparentAges =
    {
        ("Childlike", 12), ("Young Teen", 14), ("Teen", 16), ("Young Adult", 18),
        ("Adult", 22), ("Mature", 28), ("Elderly", 50)
    };

    // === Name Pool Selector ===
    public static readonly string[] NamePoolCategories = { "Japanese", "English", "French", "German", "Italian", "Russian", "Fantasy" };

    public static string[] GetNamePool(string category) => category switch
    {
        "Japanese" => JapaneseNames,
        "English" => EnglishNames,
        "French" => FrenchNames,
        "German" => GermanNames,
        "Italian" => ItalianNames,
        "Russian" => RussianNames,
        "Fantasy" => FantasyNames,
        _ => EnglishNames
    };

    public static (string GivenName, string FamilyName) GenerateName(Random random, string? poolCategory = null)
    {
        poolCategory ??= NamePoolCategories[random.Next(NamePoolCategories.Length)];
        var pool = GetNamePool(poolCategory);
        var givenName = pool[random.Next(pool.Length)];

        var familyPools = new[] { JapaneseNames, EnglishNames, FrenchNames, GermanNames };
        var familyPool = familyPools[random.Next(familyPools.Length)];
        var familyName = familyPool[random.Next(familyPool.Length)];

        return (givenName, familyName);
    }

    public static int GenerateHeight(Random random)
    {
        var range = HeightRanges[random.Next(HeightRanges.Length)];
        return random.Next(range.Min, range.Max + 1);
    }

    public static int GenerateApparentAge(Random random)
    {
        var ages = ApparentAges;
        // Weight toward 16-22
        var weights = new[] { 5, 8, 15, 25, 20, 12, 5 }; // percentages
        var total = weights.Sum();
        var roll = random.Next(total);
        var cumulative = 0;
        for (var i = 0; i < ages.Length; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative)
                return ages[i].Age;
        }
        return 18;
    }

    public static int PickBreastSize(Random random)
    {
        // Weight toward medium/large
        var weights = new[] { 5, 10, 25, 25, 18, 10, 5, 2 };
        var total = weights.Sum();
        var roll = random.Next(total);
        var cumulative = 0;
        for (var i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative)
                return BreastSizeValues[i];
        }
        return 4;
    }

    public static string PickRace(Random random)
    {
        // Weight toward human/humanoid
        var racialWeights = new Dictionary<string, int>
        {
            ["Human"] = 30,
            ["Elf"] = 10,
            ["Half-Elf"] = 8,
            ["Dark Elf"] = 5,
            ["Halfling"] = 5,
            ["Spirit"] = 3,
            ["Winged"] = 3,
            ["Catfolk"] = 5,
            ["Dogfolk"] = 3,
            ["Foxfolk"] = 3,
            ["Rabbitfolk"] = 3,
            ["Tanukifolk"] = 2,
            ["Cowfolk"] = 2,
            ["Dragonkin"] = 3,
            ["Vampire"] = 3,
            ["Dhampir"] = 2,
            ["Angel"] = 2,
            ["Youkai"] = 2,
            ["Fox Spirit"] = 2,
            ["Goddess"] = 1,
        };
        var total = racialWeights.Values.Sum();
        var roll = random.Next(total);
        var cumulative = 0;
        foreach (var pair in racialWeights)
        {
            cumulative += pair.Value;
            if (roll < cumulative)
                return pair.Key;
        }
        return "Human";
    }

    public static List<string> GenerateTalents(Random random, int count = 3)
    {
        var talents = new List<string>();
        var allPools = new[] { PositiveTraits, NegativeTraits, SkillTraits, PreferenceTraits, BodyTraits };
        while (talents.Count < count)
        {
            var pool = allPools[random.Next(allPools.Length)];
            var talent = pool[random.Next(pool.Length)];
            if (!talents.Contains(talent))
                talents.Add(talent);
        }
        return talents;
    }

    public static string PickJob(Random random)
    {
        var all = AllJobs;
        return all[random.Next(all.Length)];
    }
}
