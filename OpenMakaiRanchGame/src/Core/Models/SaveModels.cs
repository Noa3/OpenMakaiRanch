using System.Collections.Generic;

namespace OpenMakaiRanch.Core.Models;

public enum DayPhase
{
    Morning,
    Afternoon,
    Evening,
    Night
}

public enum MissionOutcome
{
    None,
    Failure,
    PartialSuccess,
    Success
}

public enum ContentMode
{
    Sfw,
    MatureSkeleton
}

public sealed class SaveState
{
    public const int CurrentSchemaVersion = 7;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
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
}

public sealed class CalendarState
{
    public int Day { get; set; } = 1;
    public DayPhase Phase { get; set; } = DayPhase.Morning;
}

public sealed class EconomyState
{
    public int Gold { get; set; } = 500;
    public int LastIncome { get; set; }
    public int LastExpenses { get; set; }
}

public sealed class RanchState
{
    public Dictionary<string, int> Stockpile { get; set; } = new();
    public Dictionary<string, int> Facilities { get; set; } = new();
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
    public ContentMode ContentMode { get; set; } = ContentMode.Sfw;
    public bool MatureContentAgeConfirmed { get; set; }
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
            ContentMode = ContentMode,
            MatureContentAgeConfirmed = MatureContentAgeConfirmed,
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
    public List<string> Lines { get; set; } = new();
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
}