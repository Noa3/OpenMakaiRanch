using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Gameplay;

public sealed class MatureContentHooks : IMatureContentHooks
{
    public string BondScenePlaceholder(string characterId)
    {
        return $"Mature scene content for {characterId} will be shown here.";
    }
}

public enum SensationType
{
    PleasureA, PleasureB, PleasureC, PleasureV, PleasureN,
    Pain, Fear, Disgust, Antipathy, Despair, Lust
}

public sealed class TrainingActionDefinition
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TrainingCategory Category { get; set; }
    public int BasePleasure { get; set; }
    public int BasePain { get; set; }
    public int MentalEffect { get; set; }
    public int FatigueCost { get; set; }
    public int EnergyCost { get; set; }
    public int MinBond { get; set; }
    public bool RequiresConsent { get; set; }
    public bool IsMatureOnly { get; set; } = true;
    public string ToolRequired { get; set; } = string.Empty;
    public List<SensationType> SensationTypes { get; set; } = new();
}

public static class TrainingActionCatalog
{
    private static List<TrainingActionDefinition>? _actions;

    public static IReadOnlyList<TrainingActionDefinition> All =>
        _actions ??= BuildCatalog();

    public static TrainingActionDefinition Get(string id) =>
        All.FirstOrDefault(a => a.Id == id) ?? All[0];

    public static IReadOnlyList<TrainingActionDefinition> ByCategory(TrainingCategory category) =>
        All.Where(a => a.Category == category).ToList();

    private static List<TrainingActionDefinition> BuildCatalog()
    {
        return new List<TrainingActionDefinition>
        {
            // === Hand actions (Train.csv 10-19) ===
            new() { Id = "breast_massage", DisplayName = "Breast Massage", Category = TrainingCategory.Hand, BasePleasure = 8, BasePain = 0, MentalEffect = -2, FatigueCost = 6, EnergyCost = 5, SensationTypes = { SensationType.PleasureB } },
            new() { Id = "breast_milk", DisplayName = "Breast Milking", Category = TrainingCategory.Hand, BasePleasure = 7, BasePain = 3, MentalEffect = -3, FatigueCost = 8, EnergyCost = 6, MinBond = 5, SensationTypes = { SensationType.PleasureB, SensationType.Pain } },
            new() { Id = "nipple_pinch", DisplayName = "Nipple Pinch", Category = TrainingCategory.Hand, BasePleasure = 6, BasePain = 4, MentalEffect = -3, FatigueCost = 4, EnergyCost = 3, SensationTypes = { SensationType.PleasureN, SensationType.Pain } },
            new() { Id = "clit_play", DisplayName = "Clit Play", Category = TrainingCategory.Hand, BasePleasure = 12, BasePain = 2, MentalEffect = -4, FatigueCost = 8, EnergyCost = 6, MinBond = 10, SensationTypes = { SensationType.PleasureC } },
            new() { Id = "vaginal_finger", DisplayName = "Vaginal Fingering", Category = TrainingCategory.Hand, BasePleasure = 14, BasePain = 3, MentalEffect = -5, FatigueCost = 10, EnergyCost = 8, MinBond = 15, SensationTypes = { SensationType.PleasureV } },
            new() { Id = "anal_finger", DisplayName = "Anal Fingering", Category = TrainingCategory.Hand, BasePleasure = 10, BasePain = 6, MentalEffect = -6, FatigueCost = 8, EnergyCost = 6, MinBond = 20, SensationTypes = { SensationType.Pain, SensationType.PleasureA } },
            new() { Id = "butt_caress", DisplayName = "Butt Caress", Category = TrainingCategory.Hand, BasePleasure = 6, BasePain = 0, MentalEffect = -1, FatigueCost = 4, EnergyCost = 3, SensationTypes = { SensationType.PleasureA } },
            new() { Id = "breast_caress", DisplayName = "Breast Caress", Category = TrainingCategory.Hand, BasePleasure = 5, BasePain = 0, MentalEffect = 2, FatigueCost = 3, EnergyCost = 2, SensationTypes = { SensationType.PleasureB } },
            new() { Id = "tickle", DisplayName = "Tickle", Category = TrainingCategory.Hand, BasePleasure = 3, BasePain = 0, MentalEffect = 4, FatigueCost = 5, EnergyCost = 4, SensationTypes = { } },
            new() { Id = "belly_rub", DisplayName = "Belly Rub", Category = TrainingCategory.Hand, BasePleasure = 4, BasePain = 0, MentalEffect = 3, FatigueCost = 2, EnergyCost = 2, SensationTypes = { } },
            new() { Id = "cheek_caress", DisplayName = "Cheek Caress", Category = TrainingCategory.Hand, BasePleasure = 3, BasePain = 0, MentalEffect = 4, FatigueCost = 2, EnergyCost = 1, SensationTypes = { } },
            new() { Id = "head_pat", DisplayName = "Head Pat", Category = TrainingCategory.Hand, BasePleasure = 2, BasePain = 0, MentalEffect = 5, FatigueCost = 1, EnergyCost = 1, SensationTypes = { } },
            new() { Id = "back_rub", DisplayName = "Back Rub", Category = TrainingCategory.Hand, BasePleasure = 4, BasePain = -2, MentalEffect = 4, FatigueCost = 3, EnergyCost = 3, SensationTypes = { } },
            new() { Id = "thigh_caress", DisplayName = "Thigh Caress", Category = TrainingCategory.Hand, BasePleasure = 5, BasePain = 0, MentalEffect = 1, FatigueCost = 3, EnergyCost = 2, MinBond = 5, SensationTypes = { } },
            new() { Id = "inner_thigh", DisplayName = "Inner Thigh Caress", Category = TrainingCategory.Hand, BasePleasure = 7, BasePain = 0, MentalEffect = -1, FatigueCost = 4, EnergyCost = 3, MinBond = 10, SensationTypes = { } },
            new() { Id = "hair_stroke", DisplayName = "Hair Stroking", Category = TrainingCategory.Hand, BasePleasure = 2, BasePain = 0, MentalEffect = 5, FatigueCost = 1, EnergyCost = 1, SensationTypes = { } },
            new() { Id = "neck_kiss_hand", DisplayName = "Neck Kiss (Hand)", Category = TrainingCategory.Hand, BasePleasure = 6, BasePain = 0, MentalEffect = 3, FatigueCost = 3, EnergyCost = 2, MinBond = 8, SensationTypes = { } },

            // === Mouth actions (Train.csv 20-29) ===
            new() { Id = "breast_suck", DisplayName = "Breast Sucking", Category = TrainingCategory.Mouth, BasePleasure = 10, BasePain = 0, MentalEffect = -3, FatigueCost = 6, EnergyCost = 5, MinBond = 10, SensationTypes = { SensationType.PleasureB } },
            new() { Id = "nipple_suck", DisplayName = "Nipple Sucking", Category = TrainingCategory.Mouth, BasePleasure = 8, BasePain = 2, MentalEffect = -4, FatigueCost = 5, EnergyCost = 4, MinBond = 12, SensationTypes = { SensationType.PleasureN } },
            new() { Id = "kiss", DisplayName = "Kiss", Category = TrainingCategory.Mouth, BasePleasure = 6, BasePain = 0, MentalEffect = 5, FatigueCost = 3, EnergyCost = 2, MinBond = 5, SensationTypes = { } },
            new() { Id = "cunnilingus", DisplayName = "Cunnilingus", Category = TrainingCategory.Mouth, BasePleasure = 16, BasePain = 0, MentalEffect = -5, FatigueCost = 10, EnergyCost = 8, MinBond = 20, SensationTypes = { SensationType.PleasureC, SensationType.PleasureV } },
            new() { Id = "vaginal_lick", DisplayName = "Vaginal Licking", Category = TrainingCategory.Mouth, BasePleasure = 14, BasePain = 0, MentalEffect = -4, FatigueCost = 8, EnergyCost = 6, MinBond = 18, SensationTypes = { SensationType.PleasureV } },
            new() { Id = "analingus", DisplayName = "Analingus", Category = TrainingCategory.Mouth, BasePleasure = 10, BasePain = 2, MentalEffect = -6, FatigueCost = 8, EnergyCost = 6, MinBond = 25, SensationTypes = { SensationType.PleasureA } },
            new() { Id = "breast_lick", DisplayName = "Breast Licking", Category = TrainingCategory.Mouth, BasePleasure = 7, BasePain = 0, MentalEffect = -1, FatigueCost = 4, EnergyCost = 3, SensationTypes = { SensationType.PleasureB } },
            new() { Id = "ear_lick", DisplayName = "Ear Licking", Category = TrainingCategory.Mouth, BasePleasure = 7, BasePain = 0, MentalEffect = 2, FatigueCost = 3, EnergyCost = 2, SensationTypes = { } },
            new() { Id = "armpit_lick", DisplayName = "Armpit Licking", Category = TrainingCategory.Mouth, BasePleasure = 5, BasePain = 0, MentalEffect = -2, FatigueCost = 4, EnergyCost = 3, SensationTypes = { } },
            new() { Id = "cheek_lick", DisplayName = "Cheek Licking", Category = TrainingCategory.Mouth, BasePleasure = 4, BasePain = 0, MentalEffect = 2, FatigueCost = 2, EnergyCost = 1, SensationTypes = { } },
            new() { Id = "neck_nuzzle", DisplayName = "Neck Nuzzling", Category = TrainingCategory.Mouth, BasePleasure = 6, BasePain = 0, MentalEffect = 4, FatigueCost = 3, EnergyCost = 2, MinBond = 8, SensationTypes = { } },
            new() { Id = "belly_kiss", DisplayName = "Belly Kisses", Category = TrainingCategory.Mouth, BasePleasure = 5, BasePain = 0, MentalEffect = 3, FatigueCost = 2, EnergyCost = 2, SensationTypes = { } },
            new() { Id = "spine_lick", DisplayName = "Spine Licking", Category = TrainingCategory.Mouth, BasePleasure = 8, BasePain = 0, MentalEffect = 1, FatigueCost = 4, EnergyCost = 3, MinBond = 15, SensationTypes = { } },
            new() { Id = "inner_thigh_lick", DisplayName = "Inner Thigh Licking", Category = TrainingCategory.Mouth, BasePleasure = 9, BasePain = 0, MentalEffect = -2, FatigueCost = 5, EnergyCost = 4, MinBond = 15, SensationTypes = { } },

            // === Penis actions (Train.csv 50-59) ===
            new() { Id = "sumata", DisplayName = "Sumata", Category = TrainingCategory.PenisAction, BasePleasure = 8, BasePain = 0, MentalEffect = -3, FatigueCost = 8, EnergyCost = 6, MinBond = 15, SensationTypes = { SensationType.PleasureV } },
            new() { Id = "fellatio", DisplayName = "Fellatio", Category = TrainingCategory.PenisAction, BasePleasure = 10, BasePain = 2, MentalEffect = -5, FatigueCost = 10, EnergyCost = 8, MinBond = 20, SensationTypes = { } },
            new() { Id = "irrumatio", DisplayName = "Irrumatio", Category = TrainingCategory.PenisAction, BasePleasure = 8, BasePain = 6, MentalEffect = -7, FatigueCost = 12, EnergyCost = 10, MinBond = 30, SensationTypes = { SensationType.Pain } },
            new() { Id = "paizuri", DisplayName = "Paizuri", Category = TrainingCategory.PenisAction, BasePleasure = 9, BasePain = 0, MentalEffect = -3, FatigueCost = 8, EnergyCost = 6, MinBond = 15, RequiresConsent = true, SensationTypes = { SensationType.PleasureB } },
            new() { Id = "forced_paizuri", DisplayName = "Forced Paizuri", Category = TrainingCategory.PenisAction, BasePleasure = 6, BasePain = 4, MentalEffect = -8, FatigueCost = 12, EnergyCost = 10, MinBond = 40, SensationTypes = { SensationType.PleasureB, SensationType.Pain } },
            new() { Id = "vertical_paizuri", DisplayName = "Vertical Paizuri", Category = TrainingCategory.PenisAction, BasePleasure = 10, BasePain = 0, MentalEffect = -3, FatigueCost = 8, EnergyCost = 6, MinBond = 20, SensationTypes = { SensationType.PleasureB } },
            new() { Id = "forced_vertical_paizuri", DisplayName = "Forced Vertical Paizuri", Category = TrainingCategory.PenisAction, BasePleasure = 7, BasePain = 5, MentalEffect = -8, FatigueCost = 12, EnergyCost = 10, MinBond = 45, SensationTypes = { SensationType.PleasureB, SensationType.Pain } },
            new() { Id = "handjob", DisplayName = "Handjob", Category = TrainingCategory.PenisAction, BasePleasure = 7, BasePain = 0, MentalEffect = -2, FatigueCost = 6, EnergyCost = 5, MinBond = 10, SensationTypes = { } },
            new() { Id = "milking_handjob", DisplayName = "Milking Handjob", Category = TrainingCategory.PenisAction, BasePleasure = 10, BasePain = 0, MentalEffect = -3, FatigueCost = 8, EnergyCost = 6, MinBond = 25, SensationTypes = { } },
            new() { Id = "footjob", DisplayName = "Footjob", Category = TrainingCategory.PenisAction, BasePleasure = 6, BasePain = 0, MentalEffect = -2, FatigueCost = 5, EnergyCost = 4, MinBond = 10, SensationTypes = { } },
            new() { Id = "thighjob", DisplayName = "Thighjob", Category = TrainingCategory.PenisAction, BasePleasure = 5, BasePain = 0, MentalEffect = -1, FatigueCost = 4, EnergyCost = 3, SensationTypes = { } },
            new() { Id = "armpit_fuck", DisplayName = "Armpit Fuck", Category = TrainingCategory.PenisAction, BasePleasure = 7, BasePain = 2, MentalEffect = -4, FatigueCost = 8, EnergyCost = 6, MinBond = 20, SensationTypes = { } },
            new() { Id = "face_sitting", DisplayName = "Face Sitting", Category = TrainingCategory.PenisAction, BasePleasure = 12, BasePain = 0, MentalEffect = -3, FatigueCost = 10, EnergyCost = 8, MinBond = 25, RequiresConsent = true, SensationTypes = { SensationType.PleasureC, SensationType.PleasureV } },

            // === V Insertion (Train.csv 30-37) ===
            new() { Id = "missionary", DisplayName = "Missionary", Category = TrainingCategory.VInsertion, BasePleasure = 18, BasePain = 4, MentalEffect = -6, FatigueCost = 14, EnergyCost = 12, MinBond = 25, SensationTypes = { SensationType.PleasureV, SensationType.Pain } },
            new() { Id = "doggy", DisplayName = "Doggy Style", Category = TrainingCategory.VInsertion, BasePleasure = 20, BasePain = 3, MentalEffect = -6, FatigueCost = 16, EnergyCost = 14, MinBond = 25, SensationTypes = { SensationType.PleasureV, SensationType.Pain } },
            new() { Id = "facing_seated", DisplayName = "Facing Seated", Category = TrainingCategory.VInsertion, BasePleasure = 16, BasePain = 2, MentalEffect = -4, FatigueCost = 12, EnergyCost = 10, MinBond = 30, SensationTypes = { SensationType.PleasureV } },
            new() { Id = "back_seated", DisplayName = "Back Seated", Category = TrainingCategory.VInsertion, BasePleasure = 14, BasePain = 3, MentalEffect = -5, FatigueCost = 12, EnergyCost = 10, MinBond = 30, SensationTypes = { SensationType.PleasureV, SensationType.Pain } },
            new() { Id = "cowgirl", DisplayName = "Cowgirl", Category = TrainingCategory.VInsertion, BasePleasure = 22, BasePain = 2, MentalEffect = -4, FatigueCost = 18, EnergyCost = 16, MinBond = 35, RequiresConsent = true, SensationTypes = { SensationType.PleasureV } },
            new() { Id = "standing_face", DisplayName = "Standing Face", Category = TrainingCategory.VInsertion, BasePleasure = 14, BasePain = 3, MentalEffect = -5, FatigueCost = 10, EnergyCost = 8, MinBond = 25, SensationTypes = { SensationType.PleasureV } },
            new() { Id = "standing_back", DisplayName = "Standing Back", Category = TrainingCategory.VInsertion, BasePleasure = 12, BasePain = 4, MentalEffect = -5, FatigueCost = 10, EnergyCost = 8, MinBond = 25, SensationTypes = { SensationType.PleasureV, SensationType.Pain } },
            new() { Id = "forced_cowgirl", DisplayName = "Forced Cowgirl", Category = TrainingCategory.VInsertion, BasePleasure = 14, BasePain = 6, MentalEffect = -9, FatigueCost = 20, EnergyCost = 18, MinBond = 50, SensationTypes = { SensationType.PleasureV, SensationType.Pain, SensationType.Fear } },
            new() { Id = "side_ways", DisplayName = "Sideways", Category = TrainingCategory.VInsertion, BasePleasure = 15, BasePain = 2, MentalEffect = -4, FatigueCost = 12, EnergyCost = 10, MinBond = 25, SensationTypes = { SensationType.PleasureV } },
            new() { Id = "scissoring", DisplayName = "Scissoring", Category = TrainingCategory.VInsertion, BasePleasure = 13, BasePain = 1, MentalEffect = -3, FatigueCost = 10, EnergyCost = 8, MinBond = 20, SensationTypes = { SensationType.PleasureC, SensationType.PleasureV } },
            new() { Id = "edge_of_bed", DisplayName = "Edge of Bed", Category = TrainingCategory.VInsertion, BasePleasure = 17, BasePain = 3, MentalEffect = -5, FatigueCost = 14, EnergyCost = 12, MinBond = 25, SensationTypes = { SensationType.PleasureV } },

            // === A Insertion (Train.csv 40-47) ===
            new() { Id = "anal_missionary", DisplayName = "Anal Missionary", Category = TrainingCategory.AInsertion, BasePleasure = 14, BasePain = 8, MentalEffect = -7, FatigueCost = 14, EnergyCost = 12, MinBond = 35, SensationTypes = { SensationType.PleasureA, SensationType.Pain } },
            new() { Id = "anal_doggy", DisplayName = "Anal Doggy", Category = TrainingCategory.AInsertion, BasePleasure = 16, BasePain = 7, MentalEffect = -7, FatigueCost = 16, EnergyCost = 14, MinBond = 35, SensationTypes = { SensationType.PleasureA, SensationType.Pain } },
            new() { Id = "anal_facing_seated", DisplayName = "Anal Facing Seated", Category = TrainingCategory.AInsertion, BasePleasure = 12, BasePain = 6, MentalEffect = -6, FatigueCost = 12, EnergyCost = 10, MinBond = 35, SensationTypes = { SensationType.PleasureA, SensationType.Pain } },
            new() { Id = "anal_back_seated", DisplayName = "Anal Back Seated", Category = TrainingCategory.AInsertion, BasePleasure = 10, BasePain = 8, MentalEffect = -7, FatigueCost = 12, EnergyCost = 10, MinBond = 35, SensationTypes = { SensationType.PleasureA, SensationType.Pain } },
            new() { Id = "anal_cowgirl", DisplayName = "Anal Cowgirl", Category = TrainingCategory.AInsertion, BasePleasure = 18, BasePain = 5, MentalEffect = -5, FatigueCost = 18, EnergyCost = 16, MinBond = 45, RequiresConsent = true, SensationTypes = { SensationType.PleasureA } },
            new() { Id = "anal_standing_face", DisplayName = "Anal Standing Face", Category = TrainingCategory.AInsertion, BasePleasure = 10, BasePain = 7, MentalEffect = -6, FatigueCost = 10, EnergyCost = 8, MinBond = 35, SensationTypes = { SensationType.PleasureA, SensationType.Pain } },
            new() { Id = "anal_standing_back", DisplayName = "Anal Standing Back", Category = TrainingCategory.AInsertion, BasePleasure = 8, BasePain = 9, MentalEffect = -7, FatigueCost = 10, EnergyCost = 8, MinBond = 35, SensationTypes = { SensationType.PleasureA, SensationType.Pain } },
            new() { Id = "forced_anal_cowgirl", DisplayName = "Forced Anal Cowgirl", Category = TrainingCategory.AInsertion, BasePleasure = 10, BasePain = 10, MentalEffect = -10, FatigueCost = 22, EnergyCost = 20, MinBond = 60, SensationTypes = { SensationType.PleasureA, SensationType.Pain, SensationType.Fear } },
            new() { Id = "anal_side", DisplayName = "Anal Side Position", Category = TrainingCategory.AInsertion, BasePleasure = 12, BasePain = 6, MentalEffect = -6, FatigueCost = 12, EnergyCost = 10, MinBond = 35, SensationTypes = { SensationType.PleasureA, SensationType.Pain } },

            // === Tool actions (Train.csv 60-63, 200-252, 290-292) ===
            new() { Id = "livestock_milker", DisplayName = "Livestock Milker", Category = TrainingCategory.Tool, BasePleasure = 10, BasePain = 4, MentalEffect = -5, FatigueCost = 8, EnergyCost = 4, ToolRequired = "livestock_milker", SensationTypes = { SensationType.PleasureB, SensationType.Pain } },
            new() { Id = "magic_pleasure_milker", DisplayName = "Magic Pleasure Milker", Category = TrainingCategory.Tool, BasePleasure = 16, BasePain = 2, MentalEffect = -6, FatigueCost = 10, EnergyCost = 6, ToolRequired = "magic_milker", MinBond = 15, SensationTypes = { SensationType.PleasureB } },
            new() { Id = "tentacle_milker", DisplayName = "Tentacle Pleasure Milker", Category = TrainingCategory.Tool, BasePleasure = 18, BasePain = 6, MentalEffect = -8, FatigueCost = 14, EnergyCost = 8, ToolRequired = "tentacle_milker", MinBond = 30, SensationTypes = { SensationType.PleasureB, SensationType.Pain, SensationType.Fear } },
            new() { Id = "spirit_extractor", DisplayName = "Small Spirit Extractor", Category = TrainingCategory.Tool, BasePleasure = 6, BasePain = 8, MentalEffect = -10, FatigueCost = 16, EnergyCost = 12, ToolRequired = "spirit_extractor", MinBond = 60, SensationTypes = { SensationType.Pain, SensationType.Fear, SensationType.Despair } },
            new() { Id = "vibrator", DisplayName = "Vibrator", Category = TrainingCategory.Tool, BasePleasure = 12, BasePain = 0, MentalEffect = -4, FatigueCost = 6, EnergyCost = 4, ToolRequired = "vibrator", SensationTypes = { SensationType.PleasureC, SensationType.PleasureV } },
            new() { Id = "anal_vibrator", DisplayName = "Anal Vibrator", Category = TrainingCategory.Tool, BasePleasure = 10, BasePain = 4, MentalEffect = -5, FatigueCost = 8, EnergyCost = 5, ToolRequired = "anal_vibrator", MinBond = 15, SensationTypes = { SensationType.PleasureA, SensationType.Pain } },
            new() { Id = "nipple_rotor", DisplayName = "Nipple Rotor", Category = TrainingCategory.Tool, BasePleasure = 8, BasePain = 2, MentalEffect = -3, FatigueCost = 4, EnergyCost = 3, ToolRequired = "nipple_rotor", SensationTypes = { SensationType.PleasureN } },
            new() { Id = "clit_rotor", DisplayName = "Clit Rotor", Category = TrainingCategory.Tool, BasePleasure = 14, BasePain = 0, MentalEffect = -4, FatigueCost = 6, EnergyCost = 4, ToolRequired = "clit_rotor", MinBond = 10, SensationTypes = { SensationType.PleasureC } },
            new() { Id = "nipple_sucker", DisplayName = "Nipple Sucker", Category = TrainingCategory.Tool, BasePleasure = 7, BasePain = 3, MentalEffect = -3, FatigueCost = 4, EnergyCost = 3, ToolRequired = "nipple_sucker", SensationTypes = { SensationType.PleasureN, SensationType.Pain } },
            new() { Id = "clit_sucker", DisplayName = "Clit Sucker", Category = TrainingCategory.Tool, BasePleasure = 15, BasePain = 0, MentalEffect = -5, FatigueCost = 6, EnergyCost = 4, ToolRequired = "clit_sucker", MinBond = 15, SensationTypes = { SensationType.PleasureC } },
            new() { Id = "blindfold", DisplayName = "Blindfold", Category = TrainingCategory.Tool, BasePleasure = 2, BasePain = 0, MentalEffect = -2, FatigueCost = 2, EnergyCost = 1, ToolRequired = "blindfold", SensationTypes = { SensationType.Fear } },
            new() { Id = "ball_gag", DisplayName = "Ball Gag", Category = TrainingCategory.Tool, BasePleasure = 1, BasePain = 2, MentalEffect = -3, FatigueCost = 3, EnergyCost = 2, ToolRequired = "ball_gag", SensationTypes = { SensationType.Pain } },
            new() { Id = "mouth_gag", DisplayName = "Mouth Gag", Category = TrainingCategory.Tool, BasePleasure = 0, BasePain = 3, MentalEffect = -4, FatigueCost = 3, EnergyCost = 2, ToolRequired = "mouth_gag", SensationTypes = { SensationType.Pain, SensationType.Fear } },
            new() { Id = "spreader_bar", DisplayName = "Spreader Bar", Category = TrainingCategory.Tool, BasePleasure = 0, BasePain = 4, MentalEffect = -5, FatigueCost = 4, EnergyCost = 2, ToolRequired = "spreader_bar", SensationTypes = { SensationType.Pain, SensationType.Fear, SensationType.Antipathy } },
            new() { Id = "lotion", DisplayName = "Lotion", Category = TrainingCategory.Tool, BasePleasure = 4, BasePain = 0, MentalEffect = 1, FatigueCost = 2, EnergyCost = 2, ToolRequired = "lotion", SensationTypes = { } },
            new() { Id = "aphrodisiac", DisplayName = "Aphrodisiac", Category = TrainingCategory.Tool, BasePleasure = 8, BasePain = 2, MentalEffect = -6, FatigueCost = 6, EnergyCost = 4, ToolRequired = "aphrodisiac", MinBond = 5, SensationTypes = { SensationType.Lust } },
            new() { Id = "condom", DisplayName = "Condom (Apply)", Category = TrainingCategory.Tool, BasePleasure = 0, BasePain = 0, MentalEffect = 0, FatigueCost = 1, EnergyCost = 1, ToolRequired = "condom", SensationTypes = { } },
            new() { Id = "energy_drink", DisplayName = "Energy Drink", Category = TrainingCategory.Tool, BasePleasure = 0, BasePain = 0, MentalEffect = 2, FatigueCost = 0, EnergyCost = -10, ToolRequired = "energy_drink", SensationTypes = { } },
            new() { Id = "vaginal_lube", DisplayName = "Vaginal Lube", Category = TrainingCategory.Tool, BasePleasure = 6, BasePain = -2, MentalEffect = 0, FatigueCost = 2, EnergyCost = 2, ToolRequired = "vaginal_lube", SensationTypes = { SensationType.PleasureV } },
            new() { Id = "anal_lube", DisplayName = "Anal Lube", Category = TrainingCategory.Tool, BasePleasure = 4, BasePain = -3, MentalEffect = 0, FatigueCost = 2, EnergyCost = 2, ToolRequired = "anal_lube", SensationTypes = { SensationType.PleasureA } },
            new() { Id = "breast_lube", DisplayName = "Breast Lube", Category = TrainingCategory.Tool, BasePleasure = 5, BasePain = -1, MentalEffect = 0, FatigueCost = 2, EnergyCost = 2, ToolRequired = "breast_lube", SensationTypes = { SensationType.PleasureB } },

            // === Pain actions (Train.csv 70) ===
            new() { Id = "spanking", DisplayName = "Spanking", Category = TrainingCategory.Pain, BasePleasure = 2, BasePain = 8, MentalEffect = -5, FatigueCost = 6, EnergyCost = 4, SensationTypes = { SensationType.Pain, SensationType.Fear, SensationType.Antipathy } },
            new() { Id = "crop", DisplayName = "Riding Crop", Category = TrainingCategory.Pain, BasePleasure = 1, BasePain = 10, MentalEffect = -6, FatigueCost = 6, EnergyCost = 4, ToolRequired = "crop", SensationTypes = { SensationType.Pain, SensationType.Fear } },
            new() { Id = "whip", DisplayName = "Whip", Category = TrainingCategory.Pain, BasePleasure = 0, BasePain = 14, MentalEffect = -8, FatigueCost = 8, EnergyCost = 5, ToolRequired = "whip", SensationTypes = { SensationType.Pain, SensationType.Fear, SensationType.Despair } },
            new() { Id = "clamps", DisplayName = "Clamps", Category = TrainingCategory.Pain, BasePleasure = 3, BasePain = 8, MentalEffect = -5, FatigueCost = 6, EnergyCost = 3, ToolRequired = "clamps", SensationTypes = { SensationType.Pain, SensationType.PleasureN } },
            new() { Id = "nipple_clamps", DisplayName = "Nipple Clamps", Category = TrainingCategory.Pain, BasePleasure = 2, BasePain = 10, MentalEffect = -6, FatigueCost = 6, EnergyCost = 4, ToolRequired = "nipple_clamps", SensationTypes = { SensationType.Pain, SensationType.PleasureN } },
            new() { Id = "candle_wax", DisplayName = "Candle Wax", Category = TrainingCategory.Pain, BasePleasure = 1, BasePain = 8, MentalEffect = -5, FatigueCost = 4, EnergyCost = 3, SensationTypes = { SensationType.Pain, SensationType.Fear } },
            new() { Id = "paddle", DisplayName = "Paddle", Category = TrainingCategory.Pain, BasePleasure = 1, BasePain = 10, MentalEffect = -6, FatigueCost = 6, EnergyCost = 4, ToolRequired = "paddle", SensationTypes = { SensationType.Pain, SensationType.Fear } },
            new() { Id = "caning", DisplayName = "Caning", Category = TrainingCategory.Pain, BasePleasure = 0, BasePain = 12, MentalEffect = -7, FatigueCost = 8, EnergyCost = 5, ToolRequired = "cane", SensationTypes = { SensationType.Pain, SensationType.Fear, SensationType.Antipathy } },
            new() { Id = "electro", DisplayName = "Electro Stim", Category = TrainingCategory.Pain, BasePleasure = 4, BasePain = 12, MentalEffect = -8, FatigueCost = 10, EnergyCost = 6, ToolRequired = "electro", MinBond = 20, SensationTypes = { SensationType.Pain, SensationType.Fear, SensationType.PleasureV } },
            new() { Id = "hot_wax", DisplayName = "Hot Wax Play", Category = TrainingCategory.Pain, BasePleasure = 2, BasePain = 6, MentalEffect = -4, FatigueCost = 4, EnergyCost = 3, SensationTypes = { SensationType.Pain, SensationType.Fear } },
            new() { Id = "ice_play", DisplayName = "Ice Play", Category = TrainingCategory.Pain, BasePleasure = 3, BasePain = 4, MentalEffect = -3, FatigueCost = 3, EnergyCost = 2, SensationTypes = { SensationType.Pain, SensationType.PleasureN } },
            new() { Id = "needle", DisplayName = "Needle Play", Category = TrainingCategory.Pain, BasePleasure = 4, BasePain = 14, MentalEffect = -9, FatigueCost = 12, EnergyCost = 8, ToolRequired = "needle", MinBond = 50, SensationTypes = { SensationType.Pain, SensationType.Fear, SensationType.Despair } },

            // === Massage (Train.csv 150-152) ===
            new() { Id = "breast_growth_massage", DisplayName = "Breast Growth Massage", Category = TrainingCategory.Massage, BasePleasure = 6, BasePain = 2, MentalEffect = 2, FatigueCost = 8, EnergyCost = 10, MinBond = 10, SensationTypes = { SensationType.PleasureB } },
            new() { Id = "rich_milk_massage", DisplayName = "Rich Milk Massage", Category = TrainingCategory.Massage, BasePleasure = 8, BasePain = 2, MentalEffect = 1, FatigueCost = 10, EnergyCost = 12, MinBond = 15, SensationTypes = { SensationType.PleasureB } },
            new() { Id = "milk_tank_massage", DisplayName = "Milk Tank Massage", Category = TrainingCategory.Massage, BasePleasure = 10, BasePain = 3, MentalEffect = 0, FatigueCost = 12, EnergyCost = 14, MinBond = 20, SensationTypes = { SensationType.PleasureB } },
            new() { Id = "full_body_massage", DisplayName = "Full Body Massage", Category = TrainingCategory.Massage, BasePleasure = 8, BasePain = -4, MentalEffect = 8, FatigueCost = 10, EnergyCost = 12, MinBond = 5, SensationTypes = { } },
            new() { Id = "shoulder_massage", DisplayName = "Shoulder Massage", Category = TrainingCategory.Massage, BasePleasure = 3, BasePain = -3, MentalEffect = 6, FatigueCost = 4, EnergyCost = 5, SensationTypes = { } },
            new() { Id = "foot_reflexology", DisplayName = "Foot Reflexology", Category = TrainingCategory.Massage, BasePleasure = 4, BasePain = -2, MentalEffect = 5, FatigueCost = 4, EnergyCost = 5, SensationTypes = { } },
            new() { Id = "scalp_massage", DisplayName = "Scalp Massage", Category = TrainingCategory.Massage, BasePleasure = 5, BasePain = -1, MentalEffect = 7, FatigueCost = 3, EnergyCost = 4, SensationTypes = { } },
            new() { Id = "oil_massage", DisplayName = "Oil Massage", Category = TrainingCategory.Massage, BasePleasure = 10, BasePain = -2, MentalEffect = 6, FatigueCost = 10, EnergyCost = 12, MinBond = 10, ToolRequired = "massage_oil", SensationTypes = { } },
            new() { Id = "sensual_massage", DisplayName = "Sensual Massage", Category = TrainingCategory.Massage, BasePleasure = 14, BasePain = 0, MentalEffect = 2, FatigueCost = 12, EnergyCost = 14, MinBond = 20, ToolRequired = "massage_oil", SensationTypes = { SensationType.PleasureB, SensationType.PleasureV } },
            new() { Id = "perineum_massage", DisplayName = "Perineum Massage", Category = TrainingCategory.Massage, BasePleasure = 12, BasePain = 2, MentalEffect = -3, FatigueCost = 8, EnergyCost = 10, MinBond = 25, SensationTypes = { SensationType.PleasureV, SensationType.PleasureA } },

            // === Tentacle actions (Train.csv 100-117) ===
            new() { Id = "tentacle_breast_massage", DisplayName = "Tentacle Breast Massage", Category = TrainingCategory.Tentacle, BasePleasure = 12, BasePain = 6, MentalEffect = -8, FatigueCost = 12, EnergyCost = 8, ToolRequired = "tentacle", MinBond = 40, SensationTypes = { SensationType.PleasureB, SensationType.Pain, SensationType.Fear } },
            new() { Id = "tentacle_breast_milk", DisplayName = "Tentacle Breast Milking", Category = TrainingCategory.Tentacle, BasePleasure = 14, BasePain = 8, MentalEffect = -10, FatigueCost = 14, EnergyCost = 10, ToolRequired = "tentacle", MinBond = 50, SensationTypes = { SensationType.PleasureB, SensationType.Pain, SensationType.Fear } },
            new() { Id = "tentacle_flower_milker", DisplayName = "Tentacle Flower Petal Milker", Category = TrainingCategory.Tentacle, BasePleasure = 16, BasePain = 8, MentalEffect = -9, FatigueCost = 16, EnergyCost = 10, ToolRequired = "tentacle_milker", MinBond = 60, SensationTypes = { SensationType.PleasureB, SensationType.Pain, SensationType.Fear } },
            new() { Id = "tentacle_cup_milker", DisplayName = "Tentacle Clear Cup Milker", Category = TrainingCategory.Tentacle, BasePleasure = 12, BasePain = 6, MentalEffect = -8, FatigueCost = 14, EnergyCost = 8, ToolRequired = "tentacle_milker", MinBond = 45, SensationTypes = { SensationType.PleasureB, SensationType.Pain } },
            new() { Id = "tentacle_worm_milker", DisplayName = "Tentacle Milking Worm", Category = TrainingCategory.Tentacle, BasePleasure = 14, BasePain = 10, MentalEffect = -10, FatigueCost = 16, EnergyCost = 10, ToolRequired = "tentacle_milker", MinBond = 55, SensationTypes = { SensationType.PleasureB, SensationType.Pain, SensationType.Disgust } },
            new() { Id = "tentacle_vaginal", DisplayName = "Tentacle Vaginal", Category = TrainingCategory.Tentacle, BasePleasure = 18, BasePain = 8, MentalEffect = -10, FatigueCost = 16, EnergyCost = 10, ToolRequired = "tentacle", MinBond = 50, SensationTypes = { SensationType.PleasureV, SensationType.Pain, SensationType.Fear } },
            new() { Id = "tentacle_anal", DisplayName = "Tentacle Anal", Category = TrainingCategory.Tentacle, BasePleasure = 14, BasePain = 10, MentalEffect = -10, FatigueCost = 16, EnergyCost = 10, ToolRequired = "tentacle", MinBond = 50, SensationTypes = { SensationType.PleasureA, SensationType.Pain, SensationType.Fear } },
            new() { Id = "tentacle_paizuri", DisplayName = "Tentacle Paizuri", Category = TrainingCategory.Tentacle, BasePleasure = 10, BasePain = 4, MentalEffect = -7, FatigueCost = 10, EnergyCost = 6, ToolRequired = "tentacle", MinBond = 45, SensationTypes = { SensationType.PleasureB, SensationType.Pain } },
            new() { Id = "tentacle_fellatio", DisplayName = "Tentacle Fellatio", Category = TrainingCategory.Tentacle, BasePleasure = 12, BasePain = 6, MentalEffect = -8, FatigueCost = 12, EnergyCost = 8, ToolRequired = "tentacle", MinBond = 45, SensationTypes = { } },
            new() { Id = "tentacle_irrumatio", DisplayName = "Tentacle Irrumatio", Category = TrainingCategory.Tentacle, BasePleasure = 10, BasePain = 10, MentalEffect = -10, FatigueCost = 14, EnergyCost = 10, ToolRequired = "tentacle", MinBond = 55, SensationTypes = { SensationType.Pain, SensationType.Fear } },
            new() { Id = "tentacle_brush", DisplayName = "Tentacle Brush", Category = TrainingCategory.Tentacle, BasePleasure = 8, BasePain = 2, MentalEffect = -5, FatigueCost = 8, EnergyCost = 5, ToolRequired = "tentacle", SensationTypes = { SensationType.PleasureA } },
            new() { Id = "tentacle_restraint", DisplayName = "Tentacle Restraint", Category = TrainingCategory.Tentacle, BasePleasure = 2, BasePain = 2, MentalEffect = -5, FatigueCost = 3, EnergyCost = 2, ToolRequired = "tentacle", SensationTypes = { SensationType.Fear } },
            new() { Id = "tentacle_blindfold", DisplayName = "Tentacle Blindfold", Category = TrainingCategory.Tentacle, BasePleasure = 4, BasePain = 0, MentalEffect = -4, FatigueCost = 4, EnergyCost = 3, ToolRequired = "tentacle", SensationTypes = { SensationType.Fear } },
            new() { Id = "tentacle_clit", DisplayName = "Tentacle Clit Sucking", Category = TrainingCategory.Tentacle, BasePleasure = 16, BasePain = 2, MentalEffect = -7, FatigueCost = 10, EnergyCost = 6, ToolRequired = "tentacle", MinBond = 40, SensationTypes = { SensationType.PleasureC, SensationType.Fear } },
            new() { Id = "tentacle_ear", DisplayName = "Tentacle Ear", Category = TrainingCategory.Tentacle, BasePleasure = 8, BasePain = 2, MentalEffect = -6, FatigueCost = 8, EnergyCost = 5, ToolRequired = "tentacle", MinBond = 35, SensationTypes = { SensationType.Fear, SensationType.Disgust } },
            new() { Id = "tentacle_handjob", DisplayName = "Tentacle Forced Handjob", Category = TrainingCategory.Tentacle, BasePleasure = 10, BasePain = 2, MentalEffect = -5, FatigueCost = 8, EnergyCost = 5, ToolRequired = "tentacle", MinBond = 35, SensationTypes = { SensationType.Pain } },
            new() { Id = "tentacle_suction_milker", DisplayName = "Tentacle Suction Milker", Category = TrainingCategory.Tentacle, BasePleasure = 14, BasePain = 6, MentalEffect = -8, FatigueCost = 14, EnergyCost = 8, ToolRequired = "tentacle_milker", MinBond = 50, SensationTypes = { SensationType.PleasureB, SensationType.Pain } },
            new() { Id = "tentacle_mammary", DisplayName = "Tentacle Mammary Penetration", Category = TrainingCategory.Tentacle, BasePleasure = 12, BasePain = 12, MentalEffect = -12, FatigueCost = 18, EnergyCost = 12, ToolRequired = "tentacle", MinBond = 70, SensationTypes = { SensationType.PleasureB, SensationType.Pain, SensationType.Fear, SensationType.Despair } },
            new() { Id = "tentacle_breast_absorb", DisplayName = "Tentacle Breast Absorption", Category = TrainingCategory.Tentacle, BasePleasure = 10, BasePain = 10, MentalEffect = -10, FatigueCost = 16, EnergyCost = 10, ToolRequired = "tentacle", MinBond = 60, SensationTypes = { SensationType.PleasureB, SensationType.Pain, SensationType.Fear } },

            // === Modification / Special actions (Train.csv 502-582, 800-910) ===
            new() { Id = "brand_of_pleasure", DisplayName = "Brand of Pleasure", Category = TrainingCategory.ForbiddenMagic, BasePleasure = 8, BasePain = 10, MentalEffect = -8, FatigueCost = 20, EnergyCost = 20, MinBond = 60, ToolRequired = "magic_brand", SensationTypes = { SensationType.Pain, SensationType.PleasureV, SensationType.Fear } },
            new() { Id = "brand_of_pain_to_pleasure", DisplayName = "Pain-to-Pleasure Conversion", Category = TrainingCategory.ForbiddenMagic, BasePleasure = 5, BasePain = 8, MentalEffect = -6, FatigueCost = 15, EnergyCost = 15, MinBond = 50, ToolRequired = "magic_brand", SensationTypes = { SensationType.Pain, SensationType.Fear } },
            new() { Id = "penis_transformation", DisplayName = "Penis Transformation", Category = TrainingCategory.BodyMod, BasePleasure = 4, BasePain = 12, MentalEffect = -10, FatigueCost = 18, EnergyCost = 16, MinBond = 70, ToolRequired = "magic_reagent", SensationTypes = { SensationType.Pain, SensationType.Fear, SensationType.Despair } },
            new() { Id = "time_compression", DisplayName = "Time Compression Training", Category = TrainingCategory.ForbiddenMagic, BasePleasure = 0, BasePain = 4, MentalEffect = -4, FatigueCost = 25, EnergyCost = 30, MinBond = 40, ToolRequired = "magic_circle", SensationTypes = { SensationType.Fear } },
            new() { Id = "aphrodisiac_slime", DisplayName = "Aphrodisiac Slime", Category = TrainingCategory.Item, BasePleasure = 14, BasePain = 4, MentalEffect = -8, FatigueCost = 10, EnergyCost = 8, ToolRequired = "aphrodisiac_slime", MinBond = 20, SensationTypes = { SensationType.Lust, SensationType.PleasureV } },
            new() { Id = "brainwashing_tentacle", DisplayName = "Brainwashing Tentacle", Category = TrainingCategory.Interrogation, BasePleasure = 6, BasePain = 6, MentalEffect = -15, FatigueCost = 20, EnergyCost = 15, ToolRequired = "tentacle", MinBond = 80, SensationTypes = { SensationType.Fear, SensationType.Despair, SensationType.Disgust } },
            new() { Id = "brand_of_recovery", DisplayName = "Brand of Climax Recovery", Category = TrainingCategory.ForbiddenMagic, BasePleasure = 8, BasePain = 8, MentalEffect = -6, FatigueCost = 15, EnergyCost = 12, MinBond = 50, ToolRequired = "magic_brand", SensationTypes = { SensationType.Pain, SensationType.PleasureV, SensationType.Fear } },
            new() { Id = "brand_of_mana_recovery", DisplayName = "Brand of Mana Recovery", Category = TrainingCategory.ForbiddenMagic, BasePleasure = 6, BasePain = 8, MentalEffect = -6, FatigueCost = 15, EnergyCost = 12, MinBond = 50, ToolRequired = "magic_brand", SensationTypes = { SensationType.Pain, SensationType.PleasureV, SensationType.Fear } },
            new() { Id = "body_modification", DisplayName = "Body Modification", Category = TrainingCategory.BodyMod, BasePleasure = 2, BasePain = 14, MentalEffect = -10, FatigueCost = 20, EnergyCost = 18, MinBond = 75, ToolRequired = "magic_reagent", SensationTypes = { SensationType.Pain, SensationType.Fear, SensationType.Despair } },
            new() { Id = "spirit_infusion", DisplayName = "Spirit Infusion", Category = TrainingCategory.ForbiddenMagic, BasePleasure = 6, BasePain = 6, MentalEffect = -4, FatigueCost = 14, EnergyCost = 20, MinBond = 30, ToolRequired = "magic_circle", SensationTypes = { SensationType.Pain, SensationType.PleasureV } },
            new() { Id = "mana_infusion", DisplayName = "Mana Infusion", Category = TrainingCategory.ForbiddenMagic, BasePleasure = 6, BasePain = 6, MentalEffect = -4, FatigueCost = 14, EnergyCost = 20, MinBond = 30, ToolRequired = "magic_circle", SensationTypes = { SensationType.Pain, SensationType.PleasureV } },
            new() { Id = "service_training", DisplayName = "Service Training", Category = TrainingCategory.Service, BasePleasure = 2, BasePain = 2, MentalEffect = -3, FatigueCost = 8, EnergyCost = 8, MinBond = 20, SensationTypes = { SensationType.Antipathy } },
            new() { Id = "mana_drain", DisplayName = "Mana Drain", Category = TrainingCategory.ForbiddenMagic, BasePleasure = 4, BasePain = 10, MentalEffect = -8, FatigueCost = 12, EnergyCost = 10, MinBond = 50, ToolRequired = "magic_circle", SensationTypes = { SensationType.Pain, SensationType.Fear, SensationType.Despair } },
            new() { Id = "penis_extraction", DisplayName = "Penis Extraction", Category = TrainingCategory.Service, BasePleasure = 0, BasePain = 2, MentalEffect = 0, FatigueCost = 2, EnergyCost = 1, SensationTypes = { } },
            new() { Id = "milker_removal", DisplayName = "Milker Removal", Category = TrainingCategory.Service, BasePleasure = 0, BasePain = 0, MentalEffect = 1, FatigueCost = 2, EnergyCost = 1, SensationTypes = { } },
            new() { Id = "tentacle_removal", DisplayName = "Tentacle Removal", Category = TrainingCategory.Service, BasePleasure = 0, BasePain = 2, MentalEffect = 2, FatigueCost = 2, EnergyCost = 1, SensationTypes = { SensationType.Pain } },
            new() { Id = "chest_wipe", DisplayName = "Chest Wipe Clean", Category = TrainingCategory.Service, BasePleasure = 1, BasePain = 0, MentalEffect = 1, FatigueCost = 1, EnergyCost = 1, SensationTypes = { } },
            new() { Id = "piston", DisplayName = "Piston Action", Category = TrainingCategory.VInsertion, BasePleasure = 20, BasePain = 6, MentalEffect = -8, FatigueCost = 18, EnergyCost = 16, MinBond = 30, SensationTypes = { SensationType.PleasureV, SensationType.Pain } },
            new() { Id = "breast_suck_mix", DisplayName = "Breast Massage & Suck", Category = TrainingCategory.Massage, BasePleasure = 12, BasePain = 2, MentalEffect = -4, FatigueCost = 10, EnergyCost = 8, MinBond = 15, SensationTypes = { SensationType.PleasureB } },
            new() { Id = "milk_drain", DisplayName = "Milk Drain Complete", Category = TrainingCategory.Massage, BasePleasure = 14, BasePain = 6, MentalEffect = -5, FatigueCost = 14, EnergyCost = 10, MinBond = 25, SensationTypes = { SensationType.PleasureB, SensationType.Pain } },
            new() { Id = "tentacle_milk_combo", DisplayName = "Tentacle Milk Combo", Category = TrainingCategory.Tentacle, BasePleasure = 18, BasePain = 10, MentalEffect = -10, FatigueCost = 20, EnergyCost = 14, ToolRequired = "tentacle_milker", MinBond = 60, SensationTypes = { SensationType.PleasureB, SensationType.Pain, SensationType.Fear } },
            new() { Id = "penis_training", DisplayName = "Penis Name Training", Category = TrainingCategory.Interrogation, BasePleasure = 2, BasePain = 0, MentalEffect = -2, FatigueCost = 4, EnergyCost = 3, SensationTypes = { } },

            // === Service / Bonding actions ===
            new() { Id = "serve_meal", DisplayName = "Serve Meal", Category = TrainingCategory.Service, BasePleasure = 2, BasePain = 0, MentalEffect = 3, FatigueCost = 4, EnergyCost = 3, SensationTypes = { } },
            new() { Id = "bathe", DisplayName = "Assisted Bathing", Category = TrainingCategory.Service, BasePleasure = 4, BasePain = 0, MentalEffect = 4, FatigueCost = 4, EnergyCost = 3, SensationTypes = { } },
            new() { Id = "massage_feet", DisplayName = "Foot Massage", Category = TrainingCategory.Service, BasePleasure = 3, BasePain = 0, MentalEffect = 3, FatigueCost = 3, EnergyCost = 2, SensationTypes = { } },
            new() { Id = "dress_up", DisplayName = "Dress Up", Category = TrainingCategory.Service, BasePleasure = 1, BasePain = 0, MentalEffect = 2, FatigueCost = 2, EnergyCost = 1, SensationTypes = { } },
            new() { Id = "groom", DisplayName = "Grooming", Category = TrainingCategory.Service, BasePleasure = 2, BasePain = 0, MentalEffect = 4, FatigueCost = 3, EnergyCost = 2, SensationTypes = { } },
            new() { Id = "read_story", DisplayName = "Read Story", Category = TrainingCategory.Service, BasePleasure = 1, BasePain = 0, MentalEffect = 5, FatigueCost = 2, EnergyCost = 1, SensationTypes = { } },
            new() { Id = "tea_time", DisplayName = "Tea Time", Category = TrainingCategory.Service, BasePleasure = 2, BasePain = 0, MentalEffect = 4, FatigueCost = 3, EnergyCost = 2, SensationTypes = { } },
            new() { Id = "walk_together", DisplayName = "Walk Together", Category = TrainingCategory.Service, BasePleasure = 2, BasePain = 0, MentalEffect = 5, FatigueCost = 4, EnergyCost = 3, SensationTypes = { } },
            new() { Id = "cuddle", DisplayName = "Cuddling", Category = TrainingCategory.Service, BasePleasure = 4, BasePain = 0, MentalEffect = 6, FatigueCost = 2, EnergyCost = 2, MinBond = 5, SensationTypes = { } },
            new() { Id = "lap_pillow", DisplayName = "Lap Pillow", Category = TrainingCategory.Service, BasePleasure = 3, BasePain = 0, MentalEffect = 7, FatigueCost = 3, EnergyCost = 2, MinBond = 8, SensationTypes = { } },
            new() { Id = "dance", DisplayName = "Dance Together", Category = TrainingCategory.Service, BasePleasure = 3, BasePain = 0, MentalEffect = 4, FatigueCost = 6, EnergyCost = 5, MinBond = 10, SensationTypes = { } },
            new() { Id = "music", DisplayName = "Play Music", Category = TrainingCategory.Service, BasePleasure = 2, BasePain = 0, MentalEffect = 4, FatigueCost = 3, EnergyCost = 2, SensationTypes = { } },
            new() { Id = "gardening", DisplayName = "Gardening Together", Category = TrainingCategory.Service, BasePleasure = 1, BasePain = 0, MentalEffect = 3, FatigueCost = 5, EnergyCost = 4, SensationTypes = { } },
            new() { Id = "teach", DisplayName = "Teaching", Category = TrainingCategory.Service, BasePleasure = 1, BasePain = 0, MentalEffect = 3, FatigueCost = 5, EnergyCost = 4, SensationTypes = { } },
            new() { Id = "star_gaze", DisplayName = "Stargazing", Category = TrainingCategory.Service, BasePleasure = 2, BasePain = 0, MentalEffect = 6, FatigueCost = 2, EnergyCost = 2, MinBond = 5, SensationTypes = { } },
            new() { Id = "hair_brush", DisplayName = "Hair Brushing", Category = TrainingCategory.Service, BasePleasure = 3, BasePain = 0, MentalEffect = 5, FatigueCost = 2, EnergyCost = 2, SensationTypes = { } },
            new() { Id = "sleep_next", DisplayName = "Sleep Beside", Category = TrainingCategory.Service, BasePleasure = 3, BasePain = 0, MentalEffect = 6, FatigueCost = 1, EnergyCost = 1, MinBond = 10, SensationTypes = { } },
        };
    }
}

public sealed class MentalStateEffects
{
    public int ResistanceDelta { get; set; }
    public int DignityDelta { get; set; }
    public int AversionDelta { get; set; }
    public int ReasonDelta { get; set; }
    public int MentalStrengthDelta { get; set; }
    public int FavorabilityDelta { get; set; }
    public int ObedienceDelta { get; set; }
    public int LustDelta { get; set; }
    public int SubmissionDelta { get; set; }
    public int MilkCowDelta { get; set; }
    public int PainDelta { get; set; }
    public int FearDelta { get; set; }
    public int DisgustDelta { get; set; }
    public int AntipathyDelta { get; set; }
    public int DespairDelta { get; set; }
}

public sealed class MentalStateService
{
    public MentalStateEffects ResolveEffects(TrainingActionDefinition action, CharacterState character)
    {
        var effects = new MentalStateEffects();
        var baseMental = action.MentalEffect;

        // Pain reduces mental stats
        effects.ResistanceDelta = -baseMental * 2;
        effects.DignityDelta = -baseMental * 2;
        effects.AversionDelta = baseMental;
        effects.ReasonDelta = -baseMental;
        effects.MentalStrengthDelta = -baseMental * (action.BasePain > 0 ? 2 : 1);

        // Pleasure increases favorability and lust
        var pleasure = action.BasePleasure;
        effects.FavorabilityDelta = pleasure / 3;
        effects.LustDelta = pleasure / 2;
        effects.ObedienceDelta = (pleasure - action.BasePain) / 4;
        effects.SubmissionDelta = action.BasePain / 3;
        effects.MilkCowDelta = action.BasePain / 4;

        // Pain effects
        effects.PainDelta = action.BasePain;
        effects.FearDelta = action.SensationTypes.Contains(SensationType.Fear) ? action.BasePain / 2 : 0;
        effects.DisgustDelta = action.SensationTypes.Contains(SensationType.Disgust) ? action.BasePain / 3 : 0;
        effects.AntipathyDelta = action.SensationTypes.Contains(SensationType.Antipathy) ? action.BasePain / 2 : 0;
        effects.DespairDelta = action.SensationTypes.Contains(SensationType.Despair) ? action.BasePain : 0;

        return effects;
    }

    public void ApplyEffects(CharacterState character, MentalStateEffects effects)
    {
        var m = character.Mature;
        m.Resistance = Clamp(m.Resistance + effects.ResistanceDelta);
        m.Dignity = Clamp(m.Dignity + effects.DignityDelta);
        m.Aversion = Clamp(m.Aversion + effects.AversionDelta);
        m.Reason = Clamp(m.Reason + effects.ReasonDelta);
        m.MentalStrength = Clamp(m.MentalStrength + effects.MentalStrengthDelta);
        m.Favorability = Clamp(m.Favorability + effects.FavorabilityDelta, 0, 20000);
        m.Lust = Clamp(m.Lust + effects.LustDelta, 0, 20000);
        m.Obedience = Clamp(m.Obedience + effects.ObedienceDelta, 0, 20000);
        m.Submission = Clamp(m.Submission + effects.SubmissionDelta, 0, 20000);
        m.MilkCow = Clamp(m.MilkCow + effects.MilkCowDelta, 0, 20000);
        m.Pain = Clamp(m.Pain + effects.PainDelta, 0, 10000);
        m.Fear = Clamp(m.Fear + effects.FearDelta, 0, 10000);
        m.Disgust = Clamp(m.Disgust + effects.DisgustDelta, 0, 10000);
        m.Antipathy = Clamp(m.Antipathy + effects.AntipathyDelta, 0, 10000);
        m.Despair = Clamp(m.Despair + effects.DespairDelta, 0, 10000);

        RecalculateFallState(character);
    }

    public void RecalculateFallState(CharacterState character)
    {
        var m = character.Mature;

        // Fall state determination
        if (m.MentalStrength <= 0 || m.Despair >= 8000)
        {
            m.FallState = FallState.Collapse;
            m.IsCollapsed = true;
        }
        else if (m.MilkCow >= 10000)
        {
            m.FallState = FallState.MilkCow;
        }
        else if (m.Obedience >= 15000 && m.Submission >= 12000)
        {
            m.FallState = FallState.Slave;
        }
        else if (m.Favorability >= 10000 || m.Lust >= 12000)
        {
            m.FallState = FallState.Devotion;
        }
        else if (m.Favorability >= 5000 || m.Lust >= 6000)
        {
            m.FallState = FallState.Love;
        }
        else
        {
            m.FallState = FallState.Normal;
        }
    }

    public string FallStateLabel(FallState state) => state switch
    {
        FallState.Normal => "Normal",
        FallState.Love => "In Love",
        FallState.Devotion => "Devoted",
        FallState.Collapse => "Collapsed",
        FallState.MilkCow => "Milk Cow",
        FallState.Slave => "Slave",
        _ => "Unknown"
    };

    private static int Clamp(int value, int min = 0, int max = 10000) =>
        value < min ? min : value > max ? max : value;
}

public sealed class EnhancedTrainingService
{
    private readonly SaveState _state;
    private readonly MentalStateService _mental;
    private readonly Random _random;

    public EnhancedTrainingService(SaveState state, Random? random = null)
    {
        _state = state;
        _mental = new MentalStateService();
        _random = random ?? Random.Shared;
    }

    public TrainingReport PerformAction(string characterId, string actionId)
    {
        var character = _state.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null)
            return new TrainingReport { Success = false, Summary = "Character not found." };

        var action = TrainingActionCatalog.Get(actionId);
        if (action is null || action.Id == "breast_massage" && actionId != "breast_massage")
            return new TrainingReport { Success = false, Summary = "Training action not found." };

        if (character.Energy < action.EnergyCost)
            return new TrainingReport { Success = false, Summary = "Not enough energy." };

        if (character.Bond < action.MinBond)
            return new TrainingReport { Success = false, Summary = $"Requires bond {action.MinBond} (current: {character.Bond})." };

        // Apply effects
        character.Energy -= action.EnergyCost;
        character.Fatigue = Clamp100(character.Fatigue + action.FatigueCost);

        var effects = _mental.ResolveEffects(action, character);
        _mental.ApplyEffects(character, effects);

        // Record training
        _state.Mature.TrainingHistory.Add(new TrainingRecord
        {
            ActionId = action.Id,
            CharacterId = characterId,
            Day = _state.Calendar.Day,
            Phase = (int)_state.Calendar.Phase,
            PleasureGained = action.BasePleasure,
            PainGained = action.BasePain,
            MentalEffect = action.MentalEffect,
            Summary = $"{action.DisplayName}: +{action.BasePleasure} pleasure, {action.BasePain} pain"
        });
        _state.Mature.TotalTrainingSessions++;

        return new TrainingReport
        {
            Success = true,
            Summary = $"{action.DisplayName} performed on {characterId}.",
            Action = action,
            Effects = effects,
            NewFallState = character.Mature.FallState
        };
    }

    private static int Clamp100(int value) => value < 0 ? 0 : value > 100 ? 100 : value;
}

public sealed class TrainingReport
{
    public bool Success { get; set; }
    public string Summary { get; set; } = string.Empty;
    public TrainingActionDefinition? Action { get; set; }
    public MentalStateEffects? Effects { get; set; }
    public FallState NewFallState { get; set; }
    public int PleasureGained { get; set; }
    public int PainGained { get; set; }
}

public sealed class MilkEconomyService
{
    private readonly SaveState _state;

    public MilkEconomyService(SaveState state)
    {
        _state = state;
    }

    public void ProduceMilk(string characterId)
    {
        var character = _state.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null) return;

        var milk = character.Milk;
        var produced = milk.Production + milk.BaseOutput;

        // Quality bonus from concentration
        var qualityMultiplier = milk.Quality / 100.0;
        produced = (int)(produced * qualityMultiplier);

        // Magic milk bonus
        if (milk.HasMagicMilkConstitution)
            produced += produced / 2;

        milk.CurrentAmount += produced;
        milk.TotalProduced += produced;
        _state.Mature.TotalMilkProduced += produced;
    }

    public int ShipMilk(string characterId)
    {
        var character = _state.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null) return 0;

        var milk = character.Milk;
        var amount = milk.CurrentAmount;
        if (amount <= 0) return 0;

        // Price per unit: base 3g, quality bonus
        var basePrice = 3;
        var qualityBonus = milk.Quality / 50;
        var concentrationBonus = milk.Concentration switch
        {
            "rich" => 2,
            "superior" => 4,
            "premium" => 6,
            "supreme" => 10,
            _ => 0
        };
        var pricePerUnit = basePrice + qualityBonus + concentrationBonus;

        var revenue = amount * pricePerUnit;
        milk.CurrentAmount = 0;
        milk.TotalShipped += amount;
        milk.TotalRevenue += revenue;
        _state.Mature.TotalMilkRevenue += revenue;
        _state.Economy.Gold += revenue;
        _state.Economy.LastIncome += revenue;

        return revenue;
    }

    public void SetMilkQuality(string characterId, int quality)
    {
        var character = _state.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null) return;

        character.Milk.Quality = Clamp100(quality);
    }

    public void SetMilkConcentration(string characterId, string concentration)
    {
        var character = _state.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null) return;

        character.Milk.Concentration = concentration;
    }

    private static int Clamp100(int value) => value < 0 ? 0 : value > 100 ? 100 : value;
}

public sealed class AddictionService
{
    private readonly SaveState _state;

    public AddictionService(SaveState state)
    {
        _state = state;
    }

    public void ApplyAddictionDelta(string characterId, string actionCategory, int intensity)
    {
        var character = _state.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null) return;

        var a = character.Addictions;
        switch (actionCategory)
        {
            case "VInsertion": a.VaginalEjaculation = Clamp(a.VaginalEjaculation + intensity); break;
            case "AInsertion": a.AnalEjaculation = Clamp(a.AnalEjaculation + intensity); break;
            case "PenisAction": a.BreastEjaculation = Clamp(a.BreastEjaculation + intensity); break;
            case "Mouth": a.SemenDrinking = Clamp(a.SemenDrinking + intensity / 2); break;
            case "Tool": a.Milking = Clamp(a.Milking + intensity); break;
            case "Tentacle": a.Tentacle = Clamp(a.Tentacle + intensity); break;
            case "Pain": a.Masochism = Clamp(a.Masochism + intensity); break;
            case "Service": a.ServiceSpirit = Clamp(a.ServiceSpirit + intensity); break;
        }
    }

    public string DescribeAddictions(CharacterState character)
    {
        var a = character.Addictions;
        var parts = new System.Collections.Generic.List<string>();
        if (a.VaginalEjaculation > 50) parts.Add("V-addicted");
        if (a.AnalEjaculation > 50) parts.Add("A-addicted");
        if (a.Masochism > 50) parts.Add("masochistic");
        if (a.Milking > 50) parts.Add("milking-addicted");
        if (a.Tentacle > 50) parts.Add("tentacle-addicted");
        if (a.ServiceSpirit > 50) parts.Add("service-oriented");
        return parts.Count > 0 ? string.Join(", ", parts) : "No notable addictions";
    }

    private static int Clamp(int value, int min = 0, int max = 100) =>
        value < min ? min : value > max ? max : value;
}
