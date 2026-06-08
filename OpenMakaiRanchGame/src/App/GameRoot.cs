using System;
using System.Linq;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Data;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.Locale;
using OpenMakaiRanch.Tests;
using OpenMakaiRanch.Ui;

namespace OpenMakaiRanch.App;

public partial class GameRoot : Node
{
    public static GameRoot Instance { get; private set; } = null!;

    public event Action? StateChanged;
    public event Action<DailyReport>? DaySettled;
    public event Action<CombatReport>? CombatResolved;
    public event Action? GameComplete;

    public DataRegistry Data { get; private set; } = null!;
    public SaveState State { get; private set; } = null!;
    public FeedbackService Feedback { get; private set; } = null!;
    public SettingsStorage SettingsStorage { get; private set; } = new();
    public SaveService Save { get; private set; } = new();
    public RosterService Roster { get; private set; } = null!;
    public ScheduleService Schedule { get; private set; } = null!;
    public RanchService Ranch { get; private set; } = null!;
    public EconomyService Economy { get; private set; } = null!;
    public InventoryService Inventory { get; private set; } = null!;
    public ShopService Shop { get; private set; } = null!;
    public AdventureService Adventure { get; private set; } = null!;
    public MilestoneService Milestones { get; private set; } = null!;
    public BondService Bond { get; private set; } = null!;
    public TownService Town { get; private set; } = new();
    public RecruitmentService Recruitment { get; private set; } = null!;
    public ResearchService Research { get; private set; } = null!;
    public PetService Pets { get; private set; } = null!;
    public TrainingService Training { get; private set; } = null!;
    public IMatureContentHooks MatureContentHooks { get; private set; } = new MatureContentHooks();
    public MentalStateService MentalState { get; private set; } = null!;
    public EnhancedTrainingService EnhancedTraining { get; private set; } = null!;
    public MilkEconomyService MilkEconomy { get; private set; } = null!;
    public AddictionService Addiction { get; private set; } = null!;
    public CombatService Combat { get; private set; } = null!;
    public DiscoveryService Discovery { get; private set; } = null!;
    public MercenaryService Mercenary { get; private set; } = null!;
    public WinConditionService WinCondition { get; private set; } = null!;
    public EquipmentService Equipment { get; private set; } = null!;
    public TalentService Talents { get; private set; } = null!;
    public DailyReport? LastDailyReport { get; set; }
    public CombatReport? LastCombatReport { get; set; }
    public CombatPhase CurrentCombatPhase { get; set; } = CombatPhase.PreBattle;
    public int CurrentCombatRound { get; set; }

    public override void _Ready()
    {
        Instance = this;
        Feedback = new FeedbackService();
        AddChild(Feedback);
        Data = DataRegistry.CreateSeeded();
        State = new SaveStateFactory(Data).CreateNewGame();
        State.Settings = SettingsStorage.Load();
        SyncFeedbackSettings();
        BuildServices();
        GD.Print($"OpenMakaiRanch ready: {Data.Characters.Count} characters, {Data.Jobs.Count} jobs, {Data.Items.Count} items.");
        if (SmokeTestRunner.ShouldRun())
        {
            var result = SmokeTestRunner.Run();
            foreach (var line in result.Lines)
            {
                GD.Print(line);
            }

            GetTree().Quit(result.Passed ? 0 : 1);
        }
    }

    public void NewGame()
    {
        var persistedSettings = State.Settings.Clone();
        State = new SaveStateFactory(Data).CreateNewGame();
        State.Settings = persistedSettings;
        LastDailyReport = null;
        LastCombatReport = null;
        SyncFeedbackSettings();
        BuildServices();
        StateChanged?.Invoke();
    }

    public void StartNewGamePlus()
    {
        var persistedSettings = State.Settings.Clone();
        State = new SaveStateFactory(Data).CreateNewGame();
        State.Settings = persistedSettings;
        State.NgPlusActive = true;
        State.Economy.Gold = 5000;
        LastDailyReport = null;
        LastCombatReport = null;
        SyncFeedbackSettings();
        BuildServices();
        StateChanged?.Invoke();
    }

    public void RerollGeneratedRecruits()
    {
        new SaveStateFactory(Data).RerollGeneratedRecruits(State);
        StateChanged?.Invoke();
    }

    public void SetRecruitName(string characterId, string name)
    {
        var character = State.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null || string.IsNullOrWhiteSpace(name)) return;
        character.DisplayNameOverride = name.Trim();
        StateChanged?.Invoke();
    }

    public void SetRecruitRace(string characterId, string race)
    {
        var character = State.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null || string.IsNullOrWhiteSpace(race)) return;
        character.Race = race;
        StateChanged?.Invoke();
    }

    public void SetRecruitPersonality(string characterId, string personality)
    {
        var character = State.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null || string.IsNullOrWhiteSpace(personality)) return;
        character.Personality = personality;
        character.TraitOverride = personality;
        StateChanged?.Invoke();
    }

    public void RerollSingleRecruit(string characterId)
    {
        var character = State.Roster.Characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null) return;
        var factory = new SaveStateFactory(Data);
        var existingIds = State.Roster.Characters.Select(c => c.Id).ToHashSet();
        existingIds.Remove(characterId);
        var replacement = factory.CreateGeneratedRecruit(State, existingIds);
        replacement.IsStartingRecruit = character.IsStartingRecruit;
        var index = State.Roster.Characters.FindIndex(c => c.Id == characterId);
        if (index >= 0)
        {
            State.Roster.Characters[index] = replacement;
            State.Schedule.AssignedJobs.Remove(characterId);
            State.Schedule.AssignedJobs[replacement.Id] = "rest";
            State.Adventure.SelectedPartyIds.Remove(characterId);
        }

        StateChanged?.Invoke();
    }

    public void SetPlayerName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
            State.Player.Name = name.Trim();
        StateChanged?.Invoke();
    }

    public void SetPlayerRace(string race)
    {
        if (!string.IsNullOrWhiteSpace(race))
            State.Player.Race = race;
        StateChanged?.Invoke();
    }

    public void SetRanchName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
            State.Player.RanchName = name.Trim();
        StateChanged?.Invoke();
    }

    public void SetPlayerGender(string gender)
    {
        if (!string.IsNullOrWhiteSpace(gender))
            State.Player.Gender = gender;
        StateChanged?.Invoke();
    }

    public void ModifyPlayer(Action<PlayerState> modify)
    {
        modify(State.Player);
        StateChanged?.Invoke();
    }

    public bool SaveSlot(int slot)
    {
        var saved = Save.Save(State, slot);
        StateChanged?.Invoke();
        return saved;
    }

    public bool LoadSlot(int slot)
    {
        var loaded = Save.Load(slot);
        if (loaded is null)
        {
            return false;
        }

        State = loaded;
        State.Settings = SettingsStorage.Load();
        LastDailyReport = null;
        LastCombatReport = null;
        SyncFeedbackSettings();
        BuildServices();
        StateChanged?.Invoke();
        return true;
    }

    public void ToggleAudioFeedback()
    {
        State.Settings.AudioEnabled = !State.Settings.AudioEnabled;
        PersistAndSyncFeedbackSettings();
        StateChanged?.Invoke();
    }

    public void ToggleHapticsFeedback()
    {
        State.Settings.HapticsEnabled = !State.Settings.HapticsEnabled;
        PersistAndSyncFeedbackSettings();
        StateChanged?.Invoke();
    }

    public UiThemePalette Theme => UiThemeCatalog.Resolve(State.Settings.ThemeId);

    public bool SetTheme(string themeId)
    {
        var resolved = UiThemeCatalog.Resolve(themeId);
        if (string.Equals(State.Settings.ThemeId, resolved.Id, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        State.Settings.ThemeId = resolved.Id;
        PersistAndSyncFeedbackSettings();
        StateChanged?.Invoke();
        return true;
    }

    public bool SetUiScale(float value)
    {
        var clamped = Mathf.Clamp(value, 0.85f, 1.35f);
        if (Mathf.IsEqualApprox(State.Settings.UiScale, clamped))
        {
            return false;
        }

        State.Settings.UiScale = clamped;
        PersistAndSyncFeedbackSettings();
        StateChanged?.Invoke();
        return true;
    }

    public bool SetLocale(string locale)
    {
        var normalizedLocale = LocaleCatalog.NormalizeLocale(locale);
        if (string.IsNullOrWhiteSpace(normalizedLocale))
        {
            return false;
        }

        if (string.Equals(State.Settings.Locale, normalizedLocale, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        State.Settings.Locale = normalizedLocale;
        ApplyLocale();
        PersistAndSyncFeedbackSettings();
        StateChanged?.Invoke();
        return true;
    }

    public bool TrainCharacter(string characterId, string focus)
    {
        if (!Training.Train(characterId, focus))
        {
            return false;
        }

        StateChanged?.Invoke();
        return true;
    }

    public bool AdvanceTime()
    {
        var dayCycle = new DayCycleService(State);
        if (dayCycle.AdvancePhase())
        {
            StateChanged?.Invoke();
            return true;
        }

        EndDay();
        return true;
    }

    public DailyReport EndDay()
    {
        var dayCycle = new DayCycleService(State);
        var settlement = new DailySettlementService(State, Data, Schedule, Ranch, Economy, dayCycle, Milestones, Inventory, Talents);
        LastDailyReport = settlement.SettleDay();
        DaySettled?.Invoke(LastDailyReport);
        StateChanged?.Invoke();
        if (WinCondition.IsGameComplete() && !State.VictoryDay.HasValue)
        {
            State.VictoryDay = State.Calendar.Day;
            GameComplete?.Invoke();
        }
        return LastDailyReport;
    }

    public CombatReport RunMission(string missionId)
    {
        return RunMission(missionId, false);
    }

    public CombatReport RunMission(string missionId, bool attemptCapture)
    {
        var party = State.Adventure.SelectedPartyIds.Count > 0 ? State.Adventure.SelectedPartyIds : State.Roster.Characters.ConvertAll(character => character.Id);
        LastCombatReport = Adventure.ResolveMission(missionId, party, attemptCapture);
        CombatResolved?.Invoke(LastCombatReport);
        StateChanged?.Invoke();
        return LastCombatReport;
    }

    public CombatReport RunRoundBasedMission(string missionId, bool autoResolve)
    {
        LastCombatReport = Combat.ResolveMissionRounds(missionId, autoResolve);
        CurrentCombatPhase = CombatPhase.BattleResults;
        CurrentCombatRound = LastCombatReport.Rounds.Count;
        CombatResolved?.Invoke(LastCombatReport);
        StateChanged?.Invoke();
        return LastCombatReport;
    }

    public CombatReport RunRoundBasedCapture(string missionId)
    {
        LastCombatReport = Combat.AttemptCapture(missionId);
        CurrentCombatPhase = CombatPhase.BattleResults;
        CurrentCombatRound = LastCombatReport.Rounds.Count;
        CombatResolved?.Invoke(LastCombatReport);
        StateChanged?.Invoke();
        return LastCombatReport;
    }

    public void StartNewCombat()
    {
        CurrentCombatPhase = CombatPhase.PreBattle;
        CurrentCombatRound = 0;
        LastCombatReport = null;
        NotifyStateChanged();
    }

    public void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    public bool HasSaveSlot(int slot) => Save.HasSave(slot);

    public void TogglePartyMember(string characterId)
    {
        if (State.Adventure.SelectedPartyIds.Contains(characterId))
        {
            if (State.Adventure.SelectedPartyIds.Count > 1)
            {
                State.Adventure.SelectedPartyIds.Remove(characterId);
            }
        }
        else
        {
            State.Adventure.SelectedPartyIds.Add(characterId);
        }

        StateChanged?.Invoke();
    }

    public bool UseItemOnCharacter(string itemId, string characterId)
    {
        var character = Roster.Find(characterId);
        if (character is null || !Inventory.UseItemOnCharacter(itemId, character))
        {
            return false;
        }

        StateChanged?.Invoke();
        return true;
    }

    public TrainingReport PerformTraining(string characterId, string actionId)
    {
        var report = EnhancedTraining.PerformAction(characterId, actionId);
        if (report.Success)
        {
            Addiction.ApplyAddictionDelta(characterId, report.Action?.Category.ToString() ?? "", report.Action?.BasePleasure ?? 0);
            StateChanged?.Invoke();
        }
        return report;
    }

    public int ShipMilk(string characterId)
    {
        var revenue = MilkEconomy.ShipMilk(characterId);
        if (revenue > 0)
        {
            StateChanged?.Invoke();
        }
        return revenue;
    }

    public void ProduceAllMilk()
    {
        foreach (var character in State.Roster.Characters)
        {
            MilkEconomy.ProduceMilk(character.Id);
        }
    }

    private void BuildServices()
    {
        Roster = new RosterService(State, Data);
        Schedule = new ScheduleService(State, Data);
        Equipment = new EquipmentService(State, Data);
        Talents = new TalentService(State, Data);
        Ranch = new RanchService(State, Data, Equipment, Talents);
        Economy = new EconomyService(State);
        Inventory = new InventoryService(State);
        Milestones = new MilestoneService(State, Data, Economy);
        Shop = new ShopService(Data, Economy, Inventory);
        var adventureSeed = State.Calendar.Day * 1009 + State.Roster.Characters.Count * 37 + State.Economy.Gold;
        Adventure = new AdventureService(State, Data, Economy, Inventory, Milestones, new Random(adventureSeed));
        Bond = new BondService(State, Data, Milestones);
        Recruitment = new RecruitmentService(State, Data, Economy);
        Recruitment.EnsureOffer();
        Research = new ResearchService(State, Data, Milestones);
        Pets = new PetService(State, Data, Economy);
        Training = new TrainingService(State, Talents);
        MentalState = new MentalStateService();
        EnhancedTraining = new EnhancedTrainingService(State);
        MilkEconomy = new MilkEconomyService(State);
        Addiction = new AddictionService(State);
        Combat = new CombatService(State, Data, Equipment, Talents);
        Discovery = new DiscoveryService(State, Data);
        Mercenary = new MercenaryService(State, Economy);
        WinCondition = new WinConditionService(State, Data);
        CurrentCombatPhase = CombatPhase.PreBattle;
        CurrentCombatRound = 0;
    }

    private void SyncFeedbackSettings()
    {
        Feedback.ApplySettings(State.Settings);
        ApplyLocale();
    }

    private void PersistAndSyncFeedbackSettings()
    {
        SettingsStorage.Save(State.Settings);
        SyncFeedbackSettings();
    }

    private void ApplyLocale()
    {
        var locale = LocaleCatalog.NormalizeLocale(State.Settings.Locale);
        State.Settings.Locale = locale;
        TranslationServer.SetLocale(locale);
        LocaleCatalog.LoadLocale(locale);
    }
}