using System;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Data;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.Tests;
using OpenMakaiRanch.Ui;

namespace OpenMakaiRanch.App;

public partial class GameRoot : Node
{
    public static GameRoot Instance { get; private set; } = null!;

    public event Action? StateChanged;
    public event Action<DailyReport>? DaySettled;
    public event Action<CombatReport>? CombatResolved;

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
    public IMatureContentHooks MatureContentHooks { get; private set; } = new NullMatureContentHooks();
    public DailyReport? LastDailyReport { get; private set; }
    public CombatReport? LastCombatReport { get; private set; }

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

    public void RerollGeneratedRecruits()
    {
        new SaveStateFactory(Data).RerollGeneratedRecruits(State);
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
        if (string.IsNullOrWhiteSpace(locale))
        {
            return false;
        }

        if (string.Equals(State.Settings.Locale, locale, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        State.Settings.Locale = locale;
        ApplyLocale();
        PersistAndSyncFeedbackSettings();
        StateChanged?.Invoke();
        return true;
    }

    public bool TrySetContentMode(ContentMode mode, bool ageConfirmed)
    {
        if (mode == ContentMode.MatureSkeleton && !ageConfirmed)
        {
            return false;
        }

        if (mode == ContentMode.MatureSkeleton)
        {
            State.Settings.MatureContentAgeConfirmed = true;
        }

        if (State.Settings.ContentMode == mode)
        {
            return false;
        }

        State.Settings.ContentMode = mode;
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
        var settlement = new DailySettlementService(State, Data, Schedule, Ranch, Economy, dayCycle, Milestones);
        LastDailyReport = settlement.SettleDay();
        DaySettled?.Invoke(LastDailyReport);
        StateChanged?.Invoke();
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

    private void BuildServices()
    {
        Roster = new RosterService(State, Data);
        Schedule = new ScheduleService(State, Data);
        Ranch = new RanchService(State, Data);
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
        Training = new TrainingService(State);
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
        if (!string.IsNullOrWhiteSpace(State.Settings.Locale))
        {
            TranslationServer.SetLocale(State.Settings.Locale);
        }
    }
}