using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.Locale;
using OpenMakaiRanch.Tests;
using OpenMakaiRanch.Tools;
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
	// Presentation callbacks capture this value, never a state-bound service reference.
	public ulong StateGeneration { get; private set; }
	public FeedbackService Feedback { get; private set; } = null!;
	public RuntimeSettingsService RuntimeSettings { get; private set; } = null!;
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

	public VisitService Visit { get; private set; } = null!;
	public MilkEconomyService MilkEconomy { get; private set; } = null!;
	public AddictionService Addiction { get; private set; } = null!;
	public CombatService Combat { get; private set; } = null!;
	public MagicService Magic { get; private set; } = null!;
	public PlayerStaminaService PlayerStamina { get; private set; } = null!;
	public DiscoveryService Discovery { get; private set; } = null!;
	public MercenaryService Mercenary { get; private set; } = null!;
	public WinConditionService WinCondition { get; private set; } = null!;
	public EquipmentService Equipment { get; private set; } = null!;
	public ClothingService Clothing { get; private set; } = null!;
	public TalentService Talents { get; private set; } = null!;
	public FlagService Flags { get; private set; } = null!;
	public DailyReport? LastDailyReport { get; set; }
	public CombatReport? LastCombatReport { get; set; }
	public CombatPhase CurrentCombatPhase { get; set; } = CombatPhase.PreBattle;
	public static string? PendingInitialScreen { get; set; }
	public int CurrentCombatRound { get; set; }
	private bool _combatWorldTimeLocked;
	public bool CombatWorldTimeLocked => _combatWorldTimeLocked;

	public override void _Ready()
	{
		Instance = this;
		RuntimeSettings = new RuntimeSettingsService();
		AddChild(RuntimeSettings);
		Feedback = new FeedbackService();
		AddChild(Feedback);
		Data = DataRegistry.CreateSeeded();
		State = new SaveStateFactory(Data).CreateNewGame();
		State.Settings = SettingsStorage.Load();
		SyncFeedbackSettings();
		BuildServices();
		GD.Print($"OpenMakaiRanch ready: {Data.Characters.Count} characters, {Data.Jobs.Count} jobs, {Data.Items.Count} items.");
		if (DataExporter.ShouldRun())
		{
			DataExporter.Run();
			GetTree().Quit(0);
			return;
		}

		if (SmokeTestRunner.ShouldRun())
		{
			// Defer so the scene tree finishes setup; UI walk tests need a quiescent root.
			CallDeferred(nameof(RunSmokeTestsAndExit));
		}
	}

	public override void _Notification(int what)
	{
		if (what == NotificationApplicationPaused)
		{
			TryAutosave("application paused");
		}
	}

	private void RunSmokeTestsAndExit()
	{
		var result = SmokeTestRunner.Run();
		foreach (var line in result.Lines)
		{
			GD.Print(line);
		}

		GetTree().Quit(result.Passed ? 0 : 1);
	}

	private void ResetTransientRuntimeState()
	{
		LastDailyReport = null;
		LastCombatReport = null;
		CurrentCombatPhase = CombatPhase.PreBattle;
		CurrentCombatRound = 0;
		_combatWorldTimeLocked = false;
		PendingInitialScreen = null;
	}
	public void NewGame()
	{
		var persistedSettings = State.Settings.Clone();
		State = new SaveStateFactory(Data).CreateNewGame();
		State.Settings = persistedSettings;
		ResetTransientRuntimeState();
		SyncFeedbackSettings();
		BuildServices();
		EnsureCharacterMagicPowerInitialized();
		StateChanged?.Invoke();
	}

	public void StartNewGamePlus()
	{
		var oldState = State;
		var persistedSettings = oldState.Settings.Clone();
		State = new SaveStateFactory(Data).CreateNewGame();
		State.Settings = persistedSettings;
		State.NgPlusActive = true;

		// Carry over a percentage of gold from the winning run
		var carryGold = Math.Max(5000, (int)(oldState.Economy.Gold * 0.2));
		State.Economy.Gold = carryGold;

		// Carry over research progress
		foreach (var skillId in oldState.Research.UnlockedSkillIds)
		{
			if (!State.Research.UnlockedSkillIds.Contains(skillId))
				State.Research.UnlockedSkillIds.Add(skillId);
		}

		// Carry over discovered missions
		foreach (var missionId in oldState.Adventure.DiscoveredMissionIds)
		{
			if (!State.Adventure.DiscoveredMissionIds.Contains(missionId))
				State.Adventure.DiscoveredMissionIds.Add(missionId);
		}

		// Carry over facility levels (preserve upgrades)
		foreach (var (facilityId, level) in oldState.Ranch.Facilities)
		{
			State.Ranch.Facilities[facilityId] = level;
		}

		// Carry over a portion of stockpile materials (rounded up)
		foreach (var (materialId, amount) in oldState.Ranch.Stockpile)
		{
			var carryAmount = Math.Max(1, (int)Math.Ceiling(amount * 0.3));
			State.Ranch.Stockpile[materialId] = carryAmount;
		}

		// Carry over a portion of inventory items (rounded up)
		foreach (var (itemId, count) in oldState.Inventory.Items)
		{
			var carryCount = Math.Max(1, (int)Math.Ceiling(count * 0.3));
			State.Inventory.Items[itemId] = carryCount;
		}

		// Carry over completed bond events (story progress)
		foreach (var eventId in oldState.Bond.CompletedEventIds)
		{
			if (!State.Bond.CompletedEventIds.Contains(eventId))
				State.Bond.CompletedEventIds.Add(eventId);
		}

		// Carry over completed milestones
		foreach (var milestoneId in oldState.Milestones.CompletedIds)
		{
			if (!State.Milestones.CompletedIds.Contains(milestoneId))
				State.Milestones.CompletedIds.Add(milestoneId);
		}

		// Carry over adopted pets
		foreach (var petId in oldState.Pets.AdoptedPetIds)
		{
			if (!State.Pets.AdoptedPetIds.Contains(petId))
			{
				State.Pets.AdoptedPetIds.Add(petId);
				if (oldState.Pets.Entries.TryGetValue(petId, out var petEntry))
				{
					State.Pets.Entries[petId] = petEntry;
				}
			}
		}

		// Carry over player customization
		State.Player = oldState.Player;

		ResetTransientRuntimeState();
		SyncFeedbackSettings();
		BuildServices();
		EnsureCharacterMagicPowerInitialized();
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
		EnsurePlayerEligibility();
		StateChanged?.Invoke();
	}

	/// <summary>
	/// Fail-closed player eligibility (audit ADULT_CHARACTER_VALIDATION.md line 95:
	/// "Players are not exempt"). Clamps a below-18 apparent age to 18 and records
	/// the denial. A legacy/custom mutation that tries to set the player to a minor
	/// age cannot acquire an adult-coded design.
	/// </summary>
	private void EnsurePlayerEligibility()
	{
		const int minAge = 18;
		if (State.Player.ApparentAge < minAge)
		{
			GD.PrintErr($"[EligibilityGate] Player apparent age {State.Player.ApparentAge} below {minAge}; clamped to {minAge} (fail-closed).");
			State.Player.ApparentAge = minAge;
		}
	}

	/// <summary>
	/// Fail-closed save-load guard (audit ADULT_CHARACTER_VALIDATION.md line 96):
	/// legacy or missing metadata must not acquire approval through defaults.
	/// A minor-apparent-age character can never be ConfirmedAdult on load;
	/// any such corrupt/legacy record is downgraded to Minor (denied).
	/// </summary>
	private void EnsureLoadedEligibility()
	{
		EnsurePlayerEligibility();
		foreach (var character in State.Roster.Characters)
		{
			if (character is null) continue;
			if (character.ApparentAge < 18 && character.AdultEligibility == AdultEligibility.ConfirmedAdult)
			{
				GD.PrintErr($"[EligibilityGate] Character {character.Id} has ConfirmedAdult with apparent age {character.ApparentAge}; downgraded to Minor (fail-closed).");
				character.AdultEligibility = AdultEligibility.Minor;
			}
		}
	}

	public bool SaveSlot(int slot)
	{
		Flags.SyncToStorage(State.Flags);
		var saved = Save.Save(State, slot);
		StateChanged?.Invoke();
		return saved;
	}

	public bool TryAssignJob(string? characterId, string? jobId, ulong expectedGeneration)
	{
		if (expectedGeneration != StateGeneration || string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(jobId)
			|| Roster.Find(characterId) is null || !Data.Jobs.ContainsKey(jobId)
			|| Schedule.GetAssignment(characterId) == jobId)
		{
			return false;
		}

		Schedule.AssignJob(characterId, jobId);
		StateChanged?.Invoke();
		return true;
	}

	public bool TryConductMentorship(string? characterId, ulong expectedGeneration)
	{
		if (expectedGeneration != StateGeneration || string.IsNullOrWhiteSpace(characterId) || Roster.Find(characterId) is null
			|| !PlayerStamina.CanSpend(PlayerActivityKind.Mentorship))
		{
			return false;
		}

		Bond.ConductMentorship(characterId);
		PlayerStamina.Spend(PlayerActivityKind.Mentorship);
		StateChanged?.Invoke();
		return true;
	}

	public bool TryCompleteBondEvent(string? eventId, ulong expectedGeneration)
	{
		if (expectedGeneration != StateGeneration || string.IsNullOrWhiteSpace(eventId)
			|| !PlayerStamina.CanSpend(PlayerActivityKind.BondEvent)
			|| !Bond.CompleteEvent(eventId))
		{
			return false;
		}

		PlayerStamina.Spend(PlayerActivityKind.BondEvent);
		StateChanged?.Invoke();
		return true;
	}

	public int PlayerStaminaCost(PlayerActivityKind kind) => PlayerStamina.Cost(kind);

	public bool CanSpendPlayerStamina(PlayerActivityKind kind) => PlayerStamina.CanSpend(kind);

	public string TryPlayWithPet(string petId)
	{
		if (!PlayerStamina.CanSpend(PlayerActivityKind.PetPlay))
			return "Not enough player stamina. Rest, bathe in the evening, or continue tomorrow.";

		var result = Pets.Play(petId);
		if (!result.StartsWith("Played successfully", StringComparison.Ordinal))
			return result;

		PlayerStamina.Spend(PlayerActivityKind.PetPlay);
		StateChanged?.Invoke();
		return result;
	}

	public string TryTrainPet(string petId)
	{
		if (!PlayerStamina.CanSpend(PlayerActivityKind.PetTraining))
			return "Not enough player stamina. Rest, bathe in the evening, or continue tomorrow.";

		var result = Pets.Train(petId);
		if (!result.StartsWith("Trained successfully", StringComparison.Ordinal))
			return result;

		PlayerStamina.Spend(PlayerActivityKind.PetTraining);
		StateChanged?.Invoke();
		return result;
	}

	public string TryVisitCare(string characterId, string action, string? itemId = null)
	{
		var kind = action switch
		{
			"feed" => PlayerActivityKind.VisitFeed,
			"gift" => PlayerActivityKind.VisitGift,
			_ => PlayerActivityKind.VisitCare
		};
		if (!PlayerStamina.CanSpend(kind))
			return "Not enough player stamina. Exploration and ordinary world interaction remain available.";

		var result = action switch
		{
			"feed" => Visit.CareFeed(characterId),
			"bathe" => Visit.CareBathe(characterId),
			"talk" => Visit.CareTalk(characterId),
			"groom" => Visit.CareGroom(characterId),
			"rest" => Visit.CareRest(characterId),
			"gift" when !string.IsNullOrWhiteSpace(itemId) => Visit.CareGift(characterId, itemId),
			_ => "Unknown care action."
		};

		if (result is "Character not found." or "No meal_box available. Buy one at the General Store." or "That is not a gift item."
			|| result.StartsWith("No ", StringComparison.Ordinal) || result == "Unknown care action.")
		{
			return result;
		}

		PlayerStamina.Spend(kind);
		StateChanged?.Invoke();
		return result;
	}

	public bool LoadSlot(int slot)
	{
		var loaded = Save.Load(slot);
		if (loaded is null)
		{
			return false;
		}

		State = loaded;
		State.Story ??= new StoryProgressState();
		EnsureLoadedEligibility();
		if (State.WorldAreaId is not ("ranch" or "town"))
		{
			State.WorldAreaId = "ranch";
		}
		State.Settings = SettingsStorage.Load();
		ResetTransientRuntimeState();
		SyncFeedbackSettings();
		BuildServices();
		EnsureCharacterMagicPowerInitialized();
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

	public bool SetMasterVolume(float value) => SetVolumeSetting(value, () => State.Settings.MasterVolume, v => State.Settings.MasterVolume = v);
	public bool SetMusicVolume(float value) => SetVolumeSetting(value, () => State.Settings.MusicVolume, v => State.Settings.MusicVolume = v);
	public bool SetSfxVolume(float value) => SetVolumeSetting(value, () => State.Settings.SfxVolume, v => State.Settings.SfxVolume = v);
	public bool SetUiVolume(float value) => SetVolumeSetting(value, () => State.Settings.UiVolume, v => State.Settings.UiVolume = v);

	private bool SetVolumeSetting(float value, Func<float> get, Action<float> set)
	{
		var clamped = Mathf.Clamp(value, 0f, 1f);
		if (Mathf.IsEqualApprox(get(), clamped))
		{
			return false;
		}

		set(clamped);
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetMuteWhenUnfocused(bool enabled)
	{
		if (State.Settings.MuteWhenUnfocused == enabled) return false;
		State.Settings.MuteWhenUnfocused = enabled;
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetGraphicsQuality(string quality)
	{
		RuntimeSettingsService.ApplyQualityPreset(State.Settings, quality);
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetRenderScale(float value)
	{
		var clamped = Mathf.Clamp(value, 0.50f, 1.00f);
		if (Mathf.IsEqualApprox(State.Settings.RenderScale, clamped)) return false;
		State.Settings.RenderScale = clamped;
		State.Settings.GraphicsQuality = "Custom";
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetAtmosphereEffectsEnabled(bool enabled)
	{
		if (State.Settings.AtmosphereEffectsEnabled == enabled) return false;
		State.Settings.AtmosphereEffectsEnabled = enabled;
		State.Settings.GraphicsQuality = "Custom";
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetWeatherEffectsEnabled(bool enabled)
	{
		if (State.Settings.WeatherEffectsEnabled == enabled) return false;
		State.Settings.WeatherEffectsEnabled = enabled;
		State.Settings.GraphicsQuality = "Custom";
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetShadowsEnabled(bool enabled)
	{
		if (State.Settings.ShadowsEnabled == enabled) return false;
		State.Settings.ShadowsEnabled = enabled;
		State.Settings.GraphicsQuality = "Custom";
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetAdvancedLightingEnabled(bool enabled)
	{
		if (State.Settings.AdvancedLightingEnabled == enabled) return false;
		State.Settings.AdvancedLightingEnabled = enabled;
		State.Settings.GraphicsQuality = "Custom";
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetWorldParticlesEnabled(bool enabled)
	{
		if (State.Settings.WorldParticlesEnabled == enabled) return false;
		State.Settings.WorldParticlesEnabled = enabled;
		State.Settings.GraphicsQuality = "Custom";
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetWorldDetailScale(float value)
	{
		var clamped = Mathf.Clamp(value, 0.35f, 1.25f);
		if (Mathf.IsEqualApprox(State.Settings.WorldDetailScale, clamped)) return false;
		State.Settings.WorldDetailScale = clamped;
		State.Settings.GraphicsQuality = "Custom";
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetFrameRateLimit(int value)
	{
		var normalized = value <= 0 ? 0 : value switch
		{
			<= 30 => 30,
			<= 45 => 45,
			<= 60 => 60,
			<= 90 => 90,
			<= 120 => 120,
			_ => 144
		};
		if (State.Settings.FrameRateLimit == normalized) return false;
		State.Settings.FrameRateLimit = normalized;
		State.Settings.GraphicsQuality = "Custom";
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetVSyncEnabled(bool enabled)
	{
		if (State.Settings.VSyncEnabled == enabled) return false;
		State.Settings.VSyncEnabled = enabled;
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetWindowSize(int width, int height)
	{
		var w = Mathf.Clamp(width, 960, 7680);
		var h = Mathf.Clamp(height, 540, 4320);
		if (State.Settings.WindowWidth == w && State.Settings.WindowHeight == h)
		{
			return false;
		}

		State.Settings.WindowWidth = w;
		State.Settings.WindowHeight = h;
		State.Settings.Fullscreen = false;
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetFullscreen(bool enabled)
	{
		if (State.Settings.Fullscreen == enabled) return false;
		State.Settings.Fullscreen = enabled;
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetCameraSensitivity(float value)
	{
		var clamped = Mathf.Clamp(value, 0.35f, 2.50f);
		if (Mathf.IsEqualApprox(State.Settings.CameraSensitivity, clamped)) return false;
		State.Settings.CameraSensitivity = clamped;
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetCameraFov(float value)
	{
		var clamped = Mathf.Clamp(value, 55f, 95f);
		if (Mathf.IsEqualApprox(State.Settings.CameraFov, clamped)) return false;
		State.Settings.CameraFov = clamped;
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetInvertCameraY(bool enabled)
	{
		if (State.Settings.InvertCameraY == enabled) return false;
		State.Settings.InvertCameraY = enabled;
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetTouchControlsEnabled(bool enabled)
	{
		if (State.Settings.TouchControlsEnabled == enabled) return false;
		State.Settings.TouchControlsEnabled = enabled;
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetTouchControlScale(float value)
	{
		var clamped = Mathf.Clamp(value, 0.75f, 1.50f);
		if (Mathf.IsEqualApprox(State.Settings.TouchControlScale, clamped)) return false;
		State.Settings.TouchControlScale = clamped;
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetAutosaveEnabled(bool enabled)
	{
		if (State.Settings.AutosaveEnabled == enabled) return false;
		State.Settings.AutosaveEnabled = enabled;
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public void ApplyRecommendedSettings()
	{
		RuntimeSettingsService.ApplyQualityPreset(State.Settings, RuntimeSettings.IsMobilePlatform ? "Low" : "Medium");
		State.Settings.UiScale = RuntimeSettings.IsMobilePlatform ? 1.15f : 1.0f;
		State.Settings.TouchControlsEnabled = RuntimeSettings.IsMobilePlatform;
		State.Settings.TouchControlScale = 1.0f;
		State.Settings.VSyncEnabled = true;
		State.Settings.CameraSensitivity = 1.0f;
		State.Settings.CameraFov = RuntimeSettings.IsMobilePlatform ? 65f : 70f;
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
	}

	public bool SetWorldArea(string areaId)
	{
		if (areaId is not ("ranch" or "town") || State.WorldAreaId == areaId)
		{
			return false;
		}

		State.WorldAreaId = areaId;
		StateChanged?.Invoke();
		return true;
	}

	public bool SetReducedMotion(bool enabled)
	{
		if (State.Settings.ReducedMotion == enabled) return false;
		State.Settings.ReducedMotion = enabled;
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool SetTutorialHintsEnabled(bool enabled)
	{
		if (State.Settings.TutorialHintsEnabled == enabled)
		{
			return false;
		}

		State.Settings.TutorialHintsEnabled = enabled;
		PersistAndSyncFeedbackSettings();
		StateChanged?.Invoke();
		return true;
	}

	public bool HasSeenTutorial(string tutorialId)
	{
		return !string.IsNullOrWhiteSpace(tutorialId)
			&& State.Settings.SeenTutorialIds?.Contains(tutorialId) == true;
	}

	public bool MarkTutorialSeen(string tutorialId)
	{
		if (string.IsNullOrWhiteSpace(tutorialId))
		{
			return false;
		}

		State.Settings.SeenTutorialIds ??= new HashSet<string>(StringComparer.Ordinal);
		if (!State.Settings.SeenTutorialIds.Add(tutorialId))
		{
			return false;
		}

		SettingsStorage.Save(State.Settings);
		return true;
	}

	public bool ResetTutorialProgress()
	{
		State.Settings.SeenTutorialIds ??= new HashSet<string>(StringComparer.Ordinal);
		if (State.Settings.SeenTutorialIds.Count == 0)
		{
			return false;
		}

		State.Settings.SeenTutorialIds.Clear();
		SettingsStorage.Save(State.Settings);
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
		if (_combatWorldTimeLocked)
		{
			return false;
		}

		var dayCycle = new DayCycleService(State);
		if (dayCycle.AdvancePhase())
		{
			StateChanged?.Invoke();
			return true;
		}

		EndDay();
		return true;
	}

	public PlayerRecoveryResult UsePlayerBath()
	{
		var recovery = PlayerStamina.TryBathRecovery(State.Ranch.BathtubClean, State.Calendar.Phase);
		if (!recovery.Used)
		{
			return recovery;
		}

		if (recovery.UsedCleanBath)
		{
			State.Ranch.BathtubClean = false;
		}

		if (State.Calendar.Phase == DayPhase.Night)
		{
			State.Calendar.NightAction = "rest";
		}

		State.Story.PlayerBathedOnFirstNight = State.Calendar.Day == 1 || State.Story.PlayerBathedOnFirstNight;
		StateChanged?.Invoke();
		return recovery;
	}

	public bool UsePlayerBathForNight()
	{
		if (State.Calendar.Phase != DayPhase.Night)
		{
			return false;
		}

		return UsePlayerBath().Used;
	}

	public bool AutosaveCheckpoint(string reason)
	{
		return TryAutosave(reason);
	}

	public void SetNightAction(string action)
	{
		if (action is not ("rest" or "train" or "admin")) return;
		State.Calendar.NightAction = action;
		StateChanged?.Invoke();
	}

	public DailyReport EndDay()
	{
		var dayCycle = new DayCycleService(State);
		var settlement = new DailySettlementService(State, Data, Schedule, Ranch, Economy, dayCycle, Milestones, Inventory, Talents);
		LastDailyReport = settlement.SettleDay();
		State.Reports.RemoveAll(report => report.Day == LastDailyReport.Day);
		State.Reports.Add(LastDailyReport);
		DaySettled?.Invoke(LastDailyReport);
		StateChanged?.Invoke();
		TryAutosave("day settled");
		if (WinCondition.IsGameComplete() && !State.VictoryDay.HasValue)
		{
			State.VictoryDay = State.Calendar.Day;
			GameComplete?.Invoke();
		}
		return LastDailyReport;
	}

	public int RechargePlayerManaFromStorage(int requestedAmount = int.MaxValue)
	{
		var transferred = Magic.RechargePlayerManaFromStorage(requestedAmount);
		if (transferred > 0)
		{
			StateChanged?.Invoke();
		}
		return transferred;
	}

	public int IncreasePlayerManaCapacity(int amount, bool fillNewCapacity = false)
	{
		var gained = Magic.IncreasePlayerManaCapacity(amount, fillNewCapacity);
		if (gained > 0)
		{
			StateChanged?.Invoke();
		}
		return gained;
	}

	public bool CastPlayerSpell(string spellId, int manaCost)
	{
		if (!Magic.CastSpell(spellId, manaCost, State.Roster.Characters.FirstOrDefault()?.Id ?? string.Empty))
		{
			return false;
		}
		StateChanged?.Invoke();
		return true;
	}

	public CombatReport RunMission(string missionId)
	{
		return RunMission(missionId, false);
	}

	public int AdventureStaminaCost(string missionId) =>
		string.Equals(missionId, "tutorial_ranch_intruder", StringComparison.OrdinalIgnoreCase)
			? 0
			: PlayerStamina.Cost(PlayerActivityKind.Adventure);

	public bool CanStartAdventure(string missionId)
	{
		if (!Data.Missions.ContainsKey(missionId))
			return false;

		var hasParty = State.Roster.Characters.Any(character =>
			State.Adventure.SelectedPartyIds.Count == 0 || State.Adventure.SelectedPartyIds.Contains(character.Id));
		return hasParty && PlayerStamina.CanSpend(AdventureStaminaCost(missionId));
	}

	public CombatReport RunMission(string missionId, bool attemptCapture)
	{
		if (!CanStartAdventure(missionId))
			return BlockedCombatReport(missionId, "Adventure blocked: not enough stamina, no valid party, or mission unavailable.");

		var party = State.Adventure.SelectedPartyIds.Count > 0 ? State.Adventure.SelectedPartyIds : State.Roster.Characters.ConvertAll(character => character.Id);
		LastCombatReport = Adventure.ResolveMission(missionId, party, attemptCapture);
		if (LastCombatReport.Outcome != MissionOutcome.None)
			PlayerStamina.Spend(AdventureStaminaCost(missionId));
		CombatResolved?.Invoke(LastCombatReport);
		StateChanged?.Invoke();
		return LastCombatReport;
	}

	public CombatReport RunRoundBasedMission(string missionId, bool autoResolve)
	{
		if (!CanStartAdventure(missionId))
			return BlockedCombatReport(missionId, "Adventure blocked: not enough stamina, no valid party, or mission unavailable.");

		LastCombatReport = Combat.ResolveMissionRounds(missionId, autoResolve);
		if (LastCombatReport.Outcome != MissionOutcome.None)
			PlayerStamina.Spend(AdventureStaminaCost(missionId));
		CurrentCombatPhase = CombatPhase.BattleResults;
		CurrentCombatRound = LastCombatReport.Rounds.Count;
		CombatResolved?.Invoke(LastCombatReport);
		StateChanged?.Invoke();
		return LastCombatReport;
	}

	public CombatReport RunRoundBasedCapture(string missionId)
	{
		if (!CanStartAdventure(missionId))
			return BlockedCombatReport(missionId, "Capture battle blocked: not enough stamina, no valid party, or mission unavailable.");

		LastCombatReport = Combat.AttemptCapture(missionId);
		if (LastCombatReport.Rounds.Count > 0)
			PlayerStamina.Spend(AdventureStaminaCost(missionId));
		CurrentCombatPhase = CombatPhase.BattleResults;
		CurrentCombatRound = LastCombatReport.Rounds.Count;
		CombatResolved?.Invoke(LastCombatReport);
		StateChanged?.Invoke();
		return LastCombatReport;
	}

	private static CombatReport BlockedCombatReport(string missionId, string summary) => new()
	{
		MissionId = missionId,
		Outcome = MissionOutcome.None,
		Summary = summary,
		IsRoundBased = true,
		TurnLog = new List<string> { summary }
	};

	public void StartNewCombat()
	{
		BeginCombatSession();
	}

	public void BeginCombatSession()
	{
		_combatWorldTimeLocked = true;
		CurrentCombatPhase = CombatPhase.PreBattle;
		CurrentCombatRound = 0;
		LastCombatReport = null;
		NotifyStateChanged();
	}

	public void EndCombatSession()
	{
		if (!_combatWorldTimeLocked && CurrentCombatPhase == CombatPhase.PreBattle && LastCombatReport is null)
		{
			return;
		}

		_combatWorldTimeLocked = false;
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

	/// <summary>
	/// Return the newest usable save among autosave slot 0 and manual slots 1-3. MainMenu and
	/// New Game+ use this instead of silently ignoring manual slots 2/3.
	/// </summary>
	public int? MostRecentSaveSlot(bool requireVictory = false)
	{
		int? selectedSlot = null;
		DateTime selectedTime = DateTime.MinValue;

		for (var slot = 0; slot <= 3; slot++)
		{
			var metadata = Save.LoadMetadata(slot);
			if (metadata is null || (requireVictory && !metadata.VictoryDay.HasValue))
			{
				continue;
			}

			var savedAt = metadata.SavedAt ?? DateTime.MinValue;
			if (!selectedSlot.HasValue || savedAt >= selectedTime)
			{
				selectedSlot = slot;
				selectedTime = savedAt;
			}
		}

		return selectedSlot;
	}

	public bool HasVictorySave() => MostRecentSaveSlot(requireVictory: true).HasValue;

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

	public (bool Success, string Error) EquipCharacterItem(string characterId, string itemId)
	{
		var character = Roster.Find(characterId);
		if (character is null)
		{
			return (false, "Character not found.");
		}

		var result = Clothing.EquipItem(character, itemId);
		if (!result.Success)
		{
			return result;
		}

		StateChanged?.Invoke();
		return result;
	}

	public (bool Success, string Error) UnequipCharacterItem(string characterId, EquipmentSlot slot)
	{
		var character = Roster.Find(characterId);
		if (character is null)
		{
			return (false, "Character not found.");
		}

		var result = Clothing.UnequipItem(character, slot);
		if (!result.Success)
		{
			return result;
		}

		StateChanged?.Invoke();
		return result;
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

	private bool TryAutosave(string reason)
	{
		if (State is null || !State.Settings.AutosaveEnabled || SmokeTestRunner.ShouldRun())
		{
			return false;
		}

		var saved = SaveSlot(0);
		if (saved)
		{
			GD.Print($"Autosave completed ({reason}).");
		}
		return saved;
	}

	private void BuildServices()
	{
		Roster = new RosterService(State, Data);
		Schedule = new ScheduleService(State, Data);
		Equipment = new EquipmentService(State, Data);
		Clothing = new ClothingService(State, Data);
		Talents = new TalentService(State, Data);
		Ranch = new RanchService(State, Data, Equipment, Talents);
		Economy = new EconomyService(State);
		Magic = new MagicService(State, Data);
		PlayerStamina = new PlayerStaminaService(State);
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
		Visit = new VisitService(State, Data);
		MilkEconomy = new MilkEconomyService(State);
		Addiction = new AddictionService(State);
		Combat = new CombatService(State, Data, Equipment, Talents, Magic);
		Discovery = new DiscoveryService(State, Data);
		Mercenary = new MercenaryService(State, Economy);
		WinCondition = new WinConditionService(State, Data);
		Flags = new FlagService();
		Flags.SyncFromStorage(State.Flags);
		CurrentCombatPhase = CombatPhase.PreBattle;
		CurrentCombatRound = 0;
		StateGeneration++;
	}

	private void SyncFeedbackSettings()
	{
		Feedback.ApplySettings(State.Settings);
		RuntimeSettings.Apply(State.Settings);
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

	private void EnsureCharacterMagicPowerInitialized()
	{
		foreach (var character in State.Roster.Characters)
		{
			if (character.MagicPower == 0 && Data.Characters.TryGetValue(character.DefinitionId, out var def))
			{
				character.MagicPower = def.MagicPower;
			}
		}
	}
}
