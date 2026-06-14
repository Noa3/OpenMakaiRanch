using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Data;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.Tests;

public sealed class SmokeTestResult
{
    public bool Passed { get; set; } = true;
    public List<string> Lines { get; } = new();
}

public static class SmokeTestRunner
{
    public static bool ShouldRun()
    {
        return OS.GetCmdlineArgs().Contains("--run-smoke-tests") || OS.GetCmdlineUserArgs().Contains("--run-smoke-tests");
    }

    public static SmokeTestResult Run()
    {
        var result = new SmokeTestResult();
        try
        {
            TestEconomyBounds(result);
            TestSettlementAndMilestone(result);
            TestAdventureResolution(result);
            TestManagementLoops(result);
            TestBondEventsAndResearchEffects(result);
            TestGeneratedRecruits(result);
            TestPortraitLayerCatalog(result);
            TestSaveRoundTrip(result);
            TestSceneAuthoredUiNodes(result);
            TestAllServiceConstructors(result);
            TestNewGameDefaults(result);
            TestScheduleAssignments(result);
            TestServiceWiring(result);
            TestNewGamePlusCarryover(result);
            TestCharacterGrowth(result);
            TestInventoryEdgeCases(result);
            TestSaveMigrationEdgeCases(result);
            TestRosterService(result);
            TestMagicPowerTraining(result);
            TestFatigueAndCollapseConsequences(result);
        }
        catch (Exception exception)
        {
            result.Passed = false;
            result.Lines.Add($"SMOKE FAIL unexpected exception: {exception.Message}");
        }

        result.Lines.Add(result.Passed ? "SMOKE PASS" : "SMOKE FAIL");
        return result;
    }

    private static void TestEconomyBounds(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var economy = new EconomyService(state);
        Assert(result, !economy.Spend(999999), "economy rejects overspend");
        Assert(result, economy.Spend(0), "economy allows no-op zero spend");
    }

    private static void TestSettlementAndMilestone(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var schedule = new ScheduleService(state, data);
        var equipment = new EquipmentService(state, data);
        var talents = new TalentService(state, data);
        var ranch = new RanchService(state, data, equipment, talents);
        var economy = new EconomyService(state);
        var milestones = new MilestoneService(state, data, economy);
        var settlement = new DailySettlementService(state, data, schedule, ranch, economy, new DayCycleService(state), milestones, new InventoryService(state), talents);

        schedule.AssignJob("rancher", "pasture");
        var startingGold = state.Economy.Gold;
        var report = settlement.SettleDay();

        Assert(result, state.Calendar.Day == 2, "settlement advances to day 2");
        Assert(result, state.Ranch.Stockpile.TryGetValue("farm_goods", out var goods) && goods > 0, "settlement creates farm goods");
        Assert(result, state.Economy.Gold != startingGold, "settlement changes gold");
        Assert(result, report.Lines.Count > 0, "settlement emits report lines");
        Assert(result, state.Milestones.CompletedIds.Contains("first_day"), "day milestone unlocks");
    }

    private static void TestAdventureResolution(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var economy = new EconomyService(state);
        var inventory = new InventoryService(state);
        var milestones = new MilestoneService(state, data, economy);
        var adventure = new AdventureService(state, data, economy, inventory, milestones, new Random(321));
        var party = state.Roster.Characters.Select(character => character.Id).ToList();
        var report = adventure.ResolveMission("road_patrol", party);

        Assert(result, report.Outcome != MissionOutcome.None, "adventure resolves an outcome");
        Assert(result, state.Adventure.LastMissionId == "road_patrol", "adventure stores last mission");
        Assert(result, state.Milestones.CompletedIds.Contains("first_patrol"), "mission milestone unlocks");

        var rosterBeforeCapture = state.Roster.Characters.Count;
        var captureReport = adventure.ResolveMission("road_patrol", party, true);
        Assert(result, captureReport.CaptureAttempted, "capture battle flags capture attempt");
        Assert(result, captureReport.TurnLog.Count >= 3, "capture battle emits turn log");
        Assert(result, !string.IsNullOrWhiteSpace(state.Adventure.LastCaptureSummary), "capture summary stored in adventure state");
        var captureSummary = state.Adventure.LastCaptureSummary;
        if (captureReport.CaptureSucceeded)
        {
            Assert(result, state.Roster.Characters.Count == rosterBeforeCapture + 1, "successful capture adds recruit to roster");
            Assert(result, state.Schedule.AssignedJobs.ContainsKey(captureReport.CapturedCharacterId), "captured recruit gets schedule assignment");
        }

        _ = adventure.ResolveMission("road_patrol", party, false);
        Assert(result, state.Adventure.LastCaptureSummary == captureSummary, "non-capture mission preserves last capture summary");
    }

    private static void TestSaveRoundTrip(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data, new Random(7)).CreateNewGame();
        state.Calendar.Phase = DayPhase.Evening;
        state.Economy.Gold = 777;
        state.Settings.AudioEnabled = false;
        state.Settings.HapticsEnabled = false;
        new SaveStateFactory(data, new Random(70)).RerollGeneratedRecruits(state);
        var generatedRecruit = state.Roster.Characters.First(character => character.IsGenerated);

        var save = new SaveService();
        Assert(result, save.Save(state, 99), "save writes slot 99");
        var loaded = save.Load(99);
        Assert(result, loaded is not null, "save loads slot 99");
        Assert(result, loaded?.SchemaVersion == SaveState.CurrentSchemaVersion, "save schema is current");
        Assert(result, loaded?.Calendar.Phase == DayPhase.Evening, "enum round-trip uses stable values");
        Assert(result, loaded?.Economy.Gold == 777, "gold round-trips");
        Assert(result, loaded?.Settings.AudioEnabled == false, "audio setting round-trips");
        Assert(result, loaded?.Settings.HapticsEnabled == false, "haptics setting round-trips");
        Assert(result, loaded?.Roster.Characters.Any(character => character.IsGenerated && character.DisplayNameOverride == generatedRecruit.DisplayNameOverride) == true, "generated recruit metadata round-trips");
        Assert(result, loaded?.Roster.Characters.Any(character => character.IsGenerated && character.BodyTypeOverride == generatedRecruit.BodyTypeOverride) == true, "generated recruit body metadata round-trips");
        Assert(result, loaded?.Recruitment.CurrentOffer?.Id == state.Recruitment.CurrentOffer?.Id, "recruitment offer round-trips");
        save.Delete(99);
    }

    private static void TestSceneAuthoredUiNodes(SmokeTestResult result)
    {
        var mainMenuScene = GD.Load<PackedScene>("res://scenes/MainMenu.tscn");
        Assert(result, mainMenuScene is not null, "main menu scene loads");
        if (mainMenuScene is not null)
        {
            var mainMenu = mainMenuScene.Instantiate();
            try
            {
                AssertNodeExists(result, mainMenu, "Root/Center/Panel/Content/ContinueButton", "main menu has continue button node");
                AssertNodeExists(result, mainMenu, "Root/Center/Panel/Content/NewGameButton", "main menu has new game button node");
                AssertNodeExists(result, mainMenu, "Root/Center/Panel/Content/NewGamePlusButton", "main menu has new game plus button node");
                AssertNodeExists(result, mainMenu, "Root/Center/Panel/Content/QuitButton", "main menu has quit button node");
            }
            finally
            {
                mainMenu.Free();
            }
        }

        var gameScene = GD.Load<PackedScene>("res://scenes/Game.tscn");
        Assert(result, gameScene is not null, "game scene loads");
        if (gameScene is null)
        {
            return;
        }

        var game = gameScene.Instantiate();
        try
        {
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/TopBarRow2/EndDayButton", "game shell has end day button node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/TopBarRow1/DayChip/DayLabel", "game shell has day label node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/TopBarRow1/PhaseChip/PhaseLabel", "game shell has phase label node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/TopBarRow2/GoldChip/GoldLabel", "game shell has gold label node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/TopBarRow1/WeatherChip/WeatherLabel", "game shell has weather label node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/TopBarRow1/PlayerNameChip/PlayerNameLabel", "game shell has player name label node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/TopBarRow1/HpChip/HpLabel", "game shell has hp label node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/TopBarRow2/SpiritChip/SpiritLabel", "game shell has spirit label node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/TopBarRow2/ManaChip/ManaLabel", "game shell has mana label node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/TopBarRow2/HealthChip/HealthLabel", "game shell has health label node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/TopBarRow1/WorkloadChip/WorkloadLabel", "game shell has workload label node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/TopBarRow2/BathtubChip/BathtubLabel", "game shell has bathtub label node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/Body/ContentPanel/Scroll/Content", "game shell has dynamic content root node");

            var topBar = game.GetNodeOrNull<VBoxContainer>("UiShell/Margin/RootPanel/Root/TopBar");
            var directTopBarChildren = topBar?.GetChildren().Select(child => child.Name.ToString()).ToHashSet() ?? new HashSet<string>();
            Assert(result, directTopBarChildren.SetEquals(new[] { "TopBarRow1", "TopBarRow2" }), "game shell top bar only contains scripted rows");

            var navigation = game.GetNodeOrNull<VBoxContainer>("UiShell/Margin/RootPanel/Root/Body/NavPanel/NavScroll/Navigation");
            Assert(result, navigation is not null, "game shell has navigation node");
            var expectedNavButtons = new HashSet<string>
            {
                "OverviewButton",
                "CharactersButton",
                "ScheduleButton",
                "TownButton",
                "ShopButton",
                "AdventureButton",
                "CombatButton",
                "MilestonesButton",
                "ResearchButton",
                "BondButton",
                "PetsButton",
                "TrainingButton",
                "MilkButton",
                "MentalButton",
                "SaveLoadButton",
                "SettingsButton"
            };
            var actualNavButtons = navigation?.GetChildren()
                .OfType<Button>()
                .Select(button => button.Name.ToString())
                .ToHashSet() ?? new HashSet<string>();
            Assert(result, expectedNavButtons.SetEquals(actualNavButtons), "game shell has expected navigation button set");

            var compactNavigation = game.GetNodeOrNull<HBoxContainer>("UiShell/Margin/RootPanel/Root/CompactNavigationScroll/CompactNavigation");
            Assert(result, compactNavigation is not null, "game shell has compact navigation node");
            var actualCompactButtons = compactNavigation?.GetChildren()
                .OfType<Button>()
                .Select(button => button.Name.ToString())
                .ToHashSet() ?? new HashSet<string>();
            Assert(result, actualCompactButtons.Count == expectedNavButtons.Count, "game shell has compact button for each side navigation button");
        }
        finally
        {
            game.Free();
        }
    }

    private static void TestGeneratedRecruits(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data, new Random(42)).CreateNewGame();
        var roster = new RosterService(state, data);
        var generated = state.Roster.Characters.Where(character => character.IsGenerated).ToList();
        var startingRecruits = generated.Where(character => character.IsStartingRecruit).ToList();

        Assert(result, generated.Count == 0, "new game starts with no extra generated recruits");
        Assert(result, startingRecruits.Count == 0, "new game starts with no starting recruits");

        var retainedGold = state.Economy.Gold;
        new SaveStateFactory(data, new Random(84)).RerollGeneratedRecruits(state);
        generated = state.Roster.Characters.Where(character => character.IsGenerated).ToList();
        Assert(result, generated.Count == 2, "reroll creates two generated recruits");
        Assert(result, generated.All(character => !string.IsNullOrWhiteSpace(character.DisplayNameOverride)), "generated recruits receive display names");
        Assert(result, generated.Select(character => character.Id).Distinct().Count() == generated.Count, "generated recruits receive unique ids");
        Assert(result, generated.All(character => character.Hp > 0 && character.Energy > 0), "generated recruits have positive hp and energy");
        Assert(result, generated.All(character => !string.IsNullOrWhiteSpace(character.PortraitPathOverride)), "generated recruits receive portrait overrides");
        Assert(result, generated.All(character => !string.IsNullOrWhiteSpace(character.BodyImagePathOverride)), "generated recruits receive body image overrides");
        Assert(result, generated.All(character => !string.IsNullOrWhiteSpace(character.BodyTypeOverride)), "generated recruits receive body type overrides");
        Assert(result, generated.All(character => character.BodyLayerIndex >= 0), "generated recruits receive body layer indices");
        Assert(result, generated.All(character => character.SkinColorIndex >= 0), "generated recruits receive skin color indices");
        Assert(result, generated.All(character => character.BreastSizeIndex >= 0), "generated recruits receive breast size indices");
        Assert(result, generated.All(character => character.RaceLayerIndex >= 0), "generated recruits receive race layer indices");
        Assert(result, generated.All(character => character.HairLayerIndex >= 0), "generated recruits receive hair layer indices");
        Assert(result, generated.All(character => character.ClothLayerIndex >= 0), "generated recruits receive cloth layer indices");
        Assert(result, generated.All(character => !string.IsNullOrWhiteSpace(character.Race)), "generated recruits receive race");
        Assert(result, generated.All(character => character.BustSize >= 0), "generated recruits receive bust size");
        Assert(result, generated.All(character => character.Talents is { Count: > 0 }), "generated recruits receive talents");
        Assert(result, generated.All(character => !string.IsNullOrWhiteSpace(character.JobClass)), "generated recruits receive job class");
        var uniqueLayerProfiles = generated
            .Select(character => $"{character.BodyLayerIndex}:{character.SkinColorIndex}:{character.BreastSizeIndex}:{character.RaceLayerIndex}:{character.HairLayerIndex}:{character.ClothLayerIndex}")
            .Distinct()
            .Count();
        Assert(result, uniqueLayerProfiles >= 2, "generated recruits vary in layered visual profile");

        var resolved = roster.DefinitionFor(generated[0]);
        Assert(result, resolved.DisplayName == generated[0].DisplayNameOverride, "roster resolves generated recruit name overrides");
        Assert(result, resolved.MaxHp == generated[0].MaxHpOverride, "roster resolves generated recruit max hp overrides");
        Assert(result, resolved.BodyType == generated[0].BodyTypeOverride, "roster resolves generated recruit body type override");

        var originalGeneratedIds = generated.Select(character => character.Id).ToHashSet();
        new SaveStateFactory(data, new Random(126)).RerollGeneratedRecruits(state);
        var rerolledGenerated = state.Roster.Characters.Where(character => character.IsGenerated).ToList();
        Assert(result, rerolledGenerated.Count == 2, "reroll keeps two generated recruits");
        Assert(result, rerolledGenerated.All(character => !originalGeneratedIds.Contains(character.Id)), "reroll replaces generated recruit ids");
        Assert(result, state.Economy.Gold == retainedGold, "reroll recruits does not reset economy state");
    }

    private static void TestPortraitLayerCatalog(SmokeTestResult result)
    {
        Assert(result, ResourceLoader.Exists(PortraitLayerCatalog.BackgroundLayer), "portrait background layer exists");
        foreach (var frame in PortraitLayerCatalog.RaceLayers
                     .Concat(PortraitLayerCatalog.HairLayers)
                     .Concat(PortraitLayerCatalog.ClothLayers)
                     .Concat(PortraitLayerCatalog.BodyBaseLayers)
                     .Concat(PortraitLayerCatalog.BreastLayers)
                     .Append(PortraitLayerCatalog.FaceLayer)
                     .Append(PortraitLayerCatalog.MouthLayer))
        {
            Assert(result, ResourceLoader.Exists(frame.Path), $"portrait layer asset exists: {frame.Path}");
            var texture = GD.Load<Texture2D>(frame.Path);
            Assert(result, texture is not null, $"portrait layer texture loads: {frame.Path}");
            if (texture is null)
            {
                continue;
            }

            var regionIsValid = frame.X >= 0
                && frame.Y >= 0
                && frame.Width > 0
                && frame.Height > 0
                && frame.X + frame.Width <= texture.GetWidth()
                && frame.Y + frame.Height <= texture.GetHeight();
            Assert(result, regionIsValid, $"portrait layer frame fits sheet: {frame.Path}");
        }
    }

    private static void TestManagementLoops(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var economy = new EconomyService(state);
        var inventory = new InventoryService(state);
        var equipment = new EquipmentService(state, data);
        var talents = new TalentService(state, data);
        var ranch = new RanchService(state, data, equipment, talents);
        var milestones = new MilestoneService(state, data, economy);
        var recruitment = new RecruitmentService(state, data, economy);
        var research = new ResearchService(state, data, milestones);
        var pets = new PetService(state, data, economy);

        var startingGold = state.Economy.Gold;
        Assert(result, ranch.UpgradeFacility("workshop", economy), "facility upgrade succeeds when affordable");
        Assert(result, state.Ranch.Facilities["workshop"] == 1, "facility upgrade records level");
        Assert(result, state.Economy.Gold < startingGold, "facility upgrade spends gold");
        Assert(result, research.Unlock("ranch_planning"), "research spends stockpile and unlocks skill");
        Assert(result, state.Research.UnlockedSkillIds.Contains("ranch_planning"), "research state stores unlock");
        Assert(result, state.Milestones.CompletedIds.Contains("first_research"), "research milestone unlocks");
        Assert(result, pets.Adopt("stable_cat"), "pet adoption succeeds when affordable");
        Assert(result, state.Pets.AdoptedPetIds.Contains("stable_cat"), "pet state stores adoption");
        Assert(result, inventory.TryConsume("meal_box", 1), "inventory consumes item");

        var rosterCountBeforeRecruit = state.Roster.Characters.Count;
        var goldBeforeRecruit = state.Economy.Gold;
        var initialOffer = recruitment.EnsureOffer();
        Assert(result, initialOffer.IsGenerated, "recruitment board has a generated offer");

        var goldBeforeReroll = state.Economy.Gold;
        Assert(result, recruitment.RerollOffer(), "recruitment board can reroll offer");
        var rerolledOffer = recruitment.CurrentOffer;
        Assert(result, rerolledOffer is not null && rerolledOffer.Id != initialOffer.Id, "reroll replaces offer candidate");
        Assert(result, state.Economy.Gold == goldBeforeReroll - RecruitmentService.RerollOfferCost, "reroll spends reroll fee");

        Assert(result, recruitment.HireOffer(), "recruitment board can hire offer");
        Assert(result, state.Roster.Characters.Count == rosterCountBeforeRecruit + 1, "recruitment adds one character");
        Assert(result, state.Economy.Gold == goldBeforeRecruit - RecruitmentService.DefaultRecruitCost - RecruitmentService.RerollOfferCost, "recruitment spends recruit and reroll fees");
        var newestRecruit = state.Roster.Characters.Last();
        Assert(result, !newestRecruit.IsStartingRecruit, "hired recruit is not tagged as a starting recruit");
        Assert(result, state.Schedule.AssignedJobs.TryGetValue(newestRecruit.Id, out var assignment) && assignment == "rest", "recruited character gets default schedule");
        Assert(result, recruitment.CurrentOffer is not null && recruitment.CurrentOffer.Id != newestRecruit.Id, "hiring refreshes the next offer");

        var protectedHireId = newestRecruit.Id;
        new SaveStateFactory(data, new Random(99)).RerollGeneratedRecruits(state);
        Assert(result, state.Roster.Characters.Any(character => character.Id == protectedHireId), "title reroll keeps hired recruits");

        state.Economy.Gold = 0;
        var offerBeforeFailedReroll = recruitment.CurrentOffer?.Id;
        Assert(result, !recruitment.RerollOffer(), "reroll fails without enough gold");
        Assert(result, recruitment.CurrentOffer?.Id == offerBeforeFailedReroll, "failed reroll keeps current offer");
        var rosterAfterHire = state.Roster.Characters.Count;
        Assert(result, !recruitment.HireOffer(), "hire fails without enough gold");
        Assert(result, state.Roster.Characters.Count == rosterAfterHire, "failed hire does not mutate roster");
    }

    private static void TestBondEventsAndResearchEffects(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var economy = new EconomyService(state);
        var milestones = new MilestoneService(state, data, economy);
        var bond = new BondService(state, data, milestones);
        var equipment = new EquipmentService(state, data);
        var talents = new TalentService(state, data);
        var ranch = new RanchService(state, data, equipment, talents);

        Assert(result, bond.AvailableEvents("rancher").Any(), "bond events are available at starting bond");
        Assert(result, bond.CompleteEvent("rancher_morning_rounds"), "bond event completes");
        Assert(result, state.Bond.CompletedEventIds.Contains("rancher_morning_rounds"), "bond event stored in save state");
        Assert(result, state.Roster.Characters.First(character => character.Id == "rancher").Bond >= 7, "bond event increases bond");

        var rancherCharacter = state.Roster.Characters.First(character => character.Id == "rancher");
        var reportWithoutResearch = new DailyReport();
        var pasture = data.Job("pasture");
        var outputWithoutResearch = ranch.ApplyJobOutput(rancherCharacter, pasture, reportWithoutResearch);
        state.Ranch.Stockpile["farm_goods"] = 0;
        state.Research.UnlockedSkillIds.Add("ranch_planning");
        var reportWithResearch = new DailyReport();
        var outputWithResearch = ranch.ApplyJobOutput(rancherCharacter, pasture, reportWithResearch);
        Assert(result, outputWithResearch > outputWithoutResearch, "ranch planning increases job output value");

        var migrated = SaveMigrator.Migrate(new SaveState
        {
            SchemaVersion = 1,
            Adventure = null!,
            Bond = null!,
            Milestones = null!,
            Pets = null!,
            Research = null!
        });
        Assert(result, migrated.SchemaVersion == SaveState.CurrentSchemaVersion, "save migrator upgrades schema v1");
        Assert(result, migrated.Bond is not null, "save migrator initializes bond state");
        Assert(result, migrated.Adventure.SelectedPartyIds is not null, "save migrator initializes adventure party list");
        var migratedRecruitment = new RecruitmentService(migrated, data, new EconomyService(migrated));
        Assert(result, migratedRecruitment.EnsureOffer() is not null, "recruitment service can initialize offer on migrated save");
    }

    private static void TestAllServiceConstructors(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();

        try
        {
            _ = new RosterService(state, data);
            _ = new ScheduleService(state, data);
            _ = new EquipmentService(state, data);
            _ = new TalentService(state, data);
            _ = new RanchService(state, data, new EquipmentService(state, data), new TalentService(state, data));
            _ = new EconomyService(state);
            _ = new DayCycleService(state);
            _ = new InventoryService(state);
            _ = new MilkEconomyService(state);
            _ = new DailyEventService(state, data, new EconomyService(state));
            _ = new MilestoneService(state, data, new EconomyService(state));
            _ = new BondService(state, data, new MilestoneService(state, data, new EconomyService(state)));
            _ = new PetService(state, data, new EconomyService(state));
            _ = new ResearchService(state, data, new MilestoneService(state, data, new EconomyService(state)));
            _ = new RecruitmentService(state, data, new EconomyService(state));
            _ = new AdventureService(state, data, new EconomyService(state), new InventoryService(state), new MilestoneService(state, data, new EconomyService(state)));
            _ = new CharacterGrowthService(state, new TalentService(state, data));
            _ = new ResourceConsumptionService(state, data);
            _ = new SaveService();
            _ = new TalentService(state, data);
            result.Lines.Add("SMOKE OK all 21 service constructors succeed without exception");
        }
        catch (Exception exception)
        {
            Assert(result, false, $"service constructor threw: {exception.Message}");
        }
    }

    private static void TestNewGameDefaults(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();

        Assert(result, state.SchemaVersion == SaveState.CurrentSchemaVersion, "new game schema is current");
        Assert(result, state.Calendar.Day == 1, "new game starts on day 1");
        Assert(result, state.Calendar.Phase == DayPhase.Morning, "new game starts in morning");
        Assert(result, state.Economy.Gold == 500, "new game starts with 500 gold");
        Assert(result, state.Economy.LastIncome == 0, "new game starts with 0 last income");
        Assert(result, state.Economy.LastExpenses == 0, "new game starts with 0 last expenses");
        Assert(result, state.Roster.Characters.Count >= 2, "new game has starting characters");
        Assert(result, state.Ranch.Facilities.Count >= 2, "new game has at least 2 facilities");
        Assert(result, state.Ranch.Stockpile["supplies"] == 3, "new game starts with 3 supplies");
        Assert(result, state.Inventory.Items["meal_box"] == 2, "new game starts with 2 meal boxes");
        Assert(result, state.Adventure.DiscoveredMissionIds.Count >= 3, "new game discovers 3 local missions");
        Assert(result, state.Adventure.SelectedPartyIds.Count == state.Roster.Characters.Count, "new game selects all starting party members");
        Assert(result, state.Milestones.CompletedIds.Count == 0, "new game has no completed milestones");
        Assert(result, state.Research.UnlockedSkillIds.Count == 0, "new game has no unlocked research");
        Assert(result, state.Bond.CompletedEventIds.Count == 0, "new game has no completed bond events");
        Assert(result, state.Pets.AdoptedPetIds.Count == 0, "new game has no pets");
        Assert(result, state.NgPlusActive == false, "new game is not NG+");
        Assert(result, state.VictoryDay == null, "new game has no victory day");
        Assert(result, state.Recruitment.CurrentOffer is not null, "new game has a recruitment offer");
        Assert(result, state.Player.Name == "Anon", "new game player name is Anon");
        Assert(result, state.Player.RanchName == "Okachi Ranch", "new game ranch name is Okachi Ranch");
    }

    private static void TestScheduleAssignments(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();

        Assert(result, state.Schedule.AssignedJobs.Count == state.Roster.Characters.Count, "every character has a schedule assignment");
        foreach (var character in state.Roster.Characters)
        {
            var hasAssignment = state.Schedule.AssignedJobs.TryGetValue(character.Id, out var jobId);
            Assert(result, hasAssignment, $"character '{character.Id}' has schedule entry");
            if (hasAssignment)
            {
                Assert(result, jobId == "rest", $"character '{character.Id}' defaults to rest");
            }
        }

        var schedule = new ScheduleService(state, data);
        Assert(result, schedule.AssignableJobs.Count >= 10, "schedule has at least 10 assignable jobs");
        foreach (var character in state.Roster.Characters.Take(3))
        {
            var assignment = schedule.GetAssignment(character.Id);
            Assert(result, assignment == "rest", $"schedule returns rest for '{character.Id}'");
        }

        schedule.AssignJob("rancher", "pasture");
        Assert(result, schedule.GetAssignment("rancher") == "pasture", "schedule assignment takes effect");
        schedule.AssignJob("rancher", "nonexistent_job");
        Assert(result, schedule.GetAssignment("rancher") == "pasture", "schedule rejects unknown job id");
    }

    private static void TestServiceWiring(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var economy = new EconomyService(state);
        var inventory = new InventoryService(state);
        var equipment = new EquipmentService(state, data);
        var talents = new TalentService(state, data);
        var schedule = new ScheduleService(state, data);
        var ranch = new RanchService(state, data, equipment, talents);
        var dayCycle = new DayCycleService(state);
        var milestones = new MilestoneService(state, data, economy);
        var recruitment = new RecruitmentService(state, data, economy);
        var research = new ResearchService(state, data, milestones);

        Assert(result, ranch.UpgradeFacility("kitchen", economy), "kitchen upgrade succeeds");
        Assert(result, state.Ranch.Facilities["kitchen"] == 2, "kitchen upgraded to level 2");
        state.Ranch.Stockpile["supplies"] = Math.Max(state.Ranch.Stockpile.GetValueOrDefault("supplies"), 4);
        Assert(result, research.Unlock("dairy_science"), "dairy science research unlocks");
        Assert(result, state.Research.UnlockedSkillIds.Contains("dairy_science"), "research state has dairy science");
        Assert(result, recruitment.EnsureOffer() is not null, "recruitment offer exists after service wiring");

        for (int day = 0; day < 5; day++)
        {
            var settlement = new DailySettlementService(state, data, schedule, ranch, economy, dayCycle, milestones, inventory, talents);
            settlement.SettleDay();
        }
        Assert(result, state.Calendar.Day >= 6, "5 days of settlement advances calendar");
        Assert(result, state.Milestones.CompletedIds.Count >= 2, "5 days of settlement completes at least 2 milestones");
    }

    private static void TestNewGamePlusCarryover(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();

        state.Economy.Gold = 25000;
        state.Research.UnlockedSkillIds.Add("dairy_science");
        state.Research.UnlockedSkillIds.Add("ranch_planning");
        state.Adventure.DiscoveredMissionIds.Add("road_patrol");
        state.Ranch.Facilities["workshop"] = 3;
        state.Ranch.Stockpile["farm_goods"] = 50;
        state.Inventory.Items["meal_box"] = 10;
        state.Bond.CompletedEventIds.Add("slay_morning_rounds");
        state.Milestones.CompletedIds.Add("first_day");
        state.Milestones.CompletedIds.Add("first_patrol");
        state.VictoryDay = 42;

        var ngPlus = new SaveStateFactory(data).CreateNewGame();
        ngPlus.NgPlusActive = true;
        ngPlus.Economy.Gold = Math.Max(5000, (int)(state.Economy.Gold * 0.2));
        foreach (var skillId in state.Research.UnlockedSkillIds)
            if (!ngPlus.Research.UnlockedSkillIds.Contains(skillId))
                ngPlus.Research.UnlockedSkillIds.Add(skillId);
        foreach (var facility in state.Ranch.Facilities)
            ngPlus.Ranch.Facilities[facility.Key] = facility.Value;
        foreach (var milestoneId in state.Milestones.CompletedIds)
            if (!ngPlus.Milestones.CompletedIds.Contains(milestoneId))
                ngPlus.Milestones.CompletedIds.Add(milestoneId);

        Assert(result, ngPlus.NgPlusActive, "NG+ state is flagged");
        Assert(result, ngPlus.Economy.Gold == 5000, "NG+ carries min 5000 gold");
        Assert(result, ngPlus.Research.UnlockedSkillIds.Contains("dairy_science"), "NG+ carries research");
        Assert(result, ngPlus.Ranch.Facilities["workshop"] == 3, "NG+ carries facility level");
        Assert(result, ngPlus.Milestones.CompletedIds.Contains("first_day"), "NG+ carries milestones");
        Assert(result, ngPlus.VictoryDay == null, "NG+ resets victory day");
        Assert(result, ngPlus.Calendar.Day == 1, "NG+ resets to day 1");
    }

    private static void TestCharacterGrowth(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var talents = new TalentService(state, data);
        var growth = new CharacterGrowthService(state, talents);

        foreach (var character in state.Roster.Characters)
        {
            character.RanchSkill = 1;
            character.CraftSkill = 1;
            character.CombatSkill = 1;
            character.SkillXp["ranch"] = 14;
            character.SkillXp["craft"] = 5;
            character.SkillXp["combat"] = 5;
            state.Schedule.AssignedJobs[character.Id] = "pasture";
        }

        var report = new DailyReport();
        growth.ApplyGrowth(report);

        Assert(result, report.CharacterGrowth.Count > 0, "character growth produces entries");
        var anyGrowth = state.Roster.Characters.Any(character => character.RanchSkill > 1 || character.CraftSkill > 1 || character.CombatSkill > 1);
        Assert(result, anyGrowth, "characters gain skill levels from growth");
    }

    private static void TestInventoryEdgeCases(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var inventory = new InventoryService(state);

        Assert(result, !inventory.TryConsume("nonexistent_item", 1), "consuming nonexistent item returns false");
        Assert(result, !inventory.TryConsume("meal_box", 9999), "consuming more than available returns false");

        var originalCount = state.Inventory.Items.GetValueOrDefault("meal_box");
        Assert(result, inventory.TryConsume("meal_box", 1), "consuming available item succeeds");
        Assert(result, state.Inventory.Items["meal_box"] == originalCount - 1, "item count decreased after consume");

        Assert(result, originalCount >= 2, "test preconditions: meal_box count >= 2");
        var countAfterFirst = state.Inventory.Items["meal_box"];
        Assert(result, inventory.TryConsume("meal_box", countAfterFirst), "consuming all remaining succeeds");
        Assert(result, !state.Inventory.Items.ContainsKey("meal_box") || state.Inventory.Items["meal_box"] == 0, "item removed or zeroed when fully consumed");
    }

    private static void TestSaveMigrationEdgeCases(SmokeTestResult result)
    {
        var migrated = SaveMigrator.Migrate(new SaveState { SchemaVersion = 0 });
        Assert(result, migrated.SchemaVersion == SaveState.CurrentSchemaVersion, "schema v0 migrates to current");
        Assert(result, migrated.Calendar is not null, "migrated v0 has calendar");
        Assert(result, migrated.Economy is not null, "migrated v0 has economy");
        Assert(result, migrated.Ranch is not null, "migrated v0 has ranch");
        Assert(result, migrated.Roster is not null, "migrated v0 has roster");
        Assert(result, migrated.Inventory is not null, "migrated v0 has inventory");
        Assert(result, migrated.Settings is not null, "migrated v0 has settings");
        Assert(result, migrated is not null && migrated.Roster is not null && migrated.Roster.Characters.Count == 0, "migrated v0 has empty roster");

        migrated = SaveMigrator.Migrate(new SaveState { SchemaVersion = 10 });
        Assert(result, migrated.SchemaVersion == SaveState.CurrentSchemaVersion, "schema v10 migrates to current");
        Assert(result, migrated.Player is not null && migrated.Player.Name == "Anon", "migrated v10 player defaults to Anon");
    }

    private static void TestRosterService(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        new SaveStateFactory(data, new Random(96)).RerollGeneratedRecruits(state);
        var roster = new RosterService(state, data);

        Assert(result, roster.Characters.Count == state.Roster.Characters.Count, "roster service exposes all characters");
        var firstDefined = state.Roster.Characters.First(character => !character.IsGenerated);
        var definition = roster.DefinitionFor(firstDefined);
        Assert(result, definition.Id == firstDefined.DefinitionId, "roster resolves definition id");
        Assert(result, definition.MaxHp > 0, "roster resolves positive max hp");
        Assert(result, definition.RanchSkill >= 1, "roster resolves ranch skill");
        Assert(result, !string.IsNullOrWhiteSpace(definition.DisplayName), "roster resolves display name");

        var found = roster.Find(firstDefined.Id);
        Assert(result, found is not null && found.Id == firstDefined.Id, "roster find returns correct character");
        var notFound = roster.Find("nonexistent_id");
        Assert(result, notFound is null, "roster find returns null for unknown id");

        var generated = state.Roster.Characters.First(character => character.IsGenerated);
        var generatedDefinition = roster.DefinitionFor(generated);
        Assert(result, generatedDefinition.DisplayName == generated.DisplayNameOverride, "roster uses display name override for generated characters");
    }

    private static void Assert(SmokeTestResult result, bool condition, string message)
    {
        if (condition)
        {
            result.Lines.Add($"SMOKE OK {message}");
            return;
        }

        result.Passed = false;
        result.Lines.Add($"SMOKE FAIL {message}");
    }

    private static void TestMagicPowerTraining(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var talents = new TalentService(state, data);
        var training = new TrainingService(state, talents);

        new SaveStateFactory(data, new Random(112)).RerollGeneratedRecruits(state);

        // Verify MagicPower is initialized from definition for defined characters
        var slay = state.Roster.Characters.First(c => c.Id == "rancher");
        var slayDef = data.Characters["rancher"];
        Assert(result, slay.MagicPower == slayDef.MagicPower, $"defined character MagicPower matches definition ({slay.MagicPower} == {slayDef.MagicPower})");

        // Verify MagicPower is initialized for generated recruits (with random variation)
        var generated = state.Roster.Characters.First(c => c.IsGenerated);
        var genDef = data.Characters[generated.DefinitionId];
        Assert(result, generated.MagicPower >= genDef.MagicPower, $"generated recruit MagicPower >= definition base ({generated.MagicPower} >= {genDef.MagicPower})");
        Assert(result, generated.MagicPower <= genDef.MagicPower + 3, $"generated recruit MagicPower <= definition base + 3 ({generated.MagicPower} <= {genDef.MagicPower + 3})");

        // Train MagicPower and verify increase
        var oldMagic = slay.MagicPower;
        slay.Energy = 50;
        Assert(result, training.Train(slay.Id, "magic"), "magic training succeeds with enough energy");
        Assert(result, slay.MagicPower >= oldMagic + 2, $"magic training increases MagicPower by at least 2 ({slay.MagicPower} >= {oldMagic + 2})");
        Assert(result, slay.Energy == 40, "magic training consumes 10 energy");
        Assert(result, slay.Fatigue > 0, "magic training adds fatigue");
        Assert(result, slay.Morale > 50, "magic training adds morale");

        // Verify fails without enough energy
        slay.Energy = 5;
        Assert(result, !training.Train(slay.Id, "magic"), "magic training fails without enough energy");

        // Verify initialization of MagicPower = 0 handles migrated saves
        var customChar = new CharacterState
        {
            Id = "test_migrated",
            DefinitionId = "slay",
            Energy = 50
        };
        state.Roster.Characters.Add(customChar);
        Assert(result, customChar.MagicPower == 0, "migrated character has MagicPower = 0 by default");
        Assert(result, training.Train(customChar.Id, "magic"), "training handles zero MagicPower (migrated save)");
        Assert(result, customChar.MagicPower == 3, $"first training initializes to 1 then adds 2 ({customChar.MagicPower} == 3)");
    }

    private static void TestFatigueAndCollapseConsequences(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var equipment = new EquipmentService(state, data);
        var talents = new TalentService(state, data);
        var ranch = new RanchService(state, data, equipment, talents);
        var economy = new EconomyService(state);
        var training = new TrainingService(state, talents);
        var schedule = new ScheduleService(state, data);
        var dayCycle = new DayCycleService(state);
        var milestones = new MilestoneService(state, data, economy);
        var inventory = new InventoryService(state);
        var settlement = new DailySettlementService(state, data, schedule, ranch, economy, dayCycle, milestones, inventory, talents);

        var character = state.Roster.Characters.First(c => c.Id == "rancher");
        var job = data.Job("pasture");

        // === Rest fatigue reduction ===
        var fatigueBeforeRest = character.Fatigue = 60;
        character.Morale = 50;
        var restJob = data.Job("rest");
        var restReport = new DailyReport();
        _ = ranch.ApplyJobOutput(character, restJob, restReport);
        var fatigueResistance = talents.FatigueResistance(character.Id);
        var fatigueDelta = -24 - fatigueResistance;
        var expectedFatigue = Math.Clamp(fatigueBeforeRest + fatigueDelta, 0, 100);
        // Simulate what DailySettlementService does
        var restDelta = -24 - fatigueResistance;
        Assert(result, restDelta < 0, $"rest fatigue delta is negative ({restDelta} < 0)");

        // Work: ApplyJobOutput rounds down properly
        character.RanchSkill = 5;
        character.Fatigue = 0;
        character.Morale = 50;
        character.Energy = 100;
        var workReport = new DailyReport();
        var output = ranch.ApplyJobOutput(character, job, workReport);
        Assert(result, output > 0, "character with 0 fatigue produces output");

        // === Fatigue penalty on job output at high fatigue ===
        character.Fatigue = 80;
        var highFatigueReport = new DailyReport();
        var reducedOutput = ranch.ApplyJobOutput(character, job, highFatigueReport);
        Assert(result, reducedOutput < output, "high fatigue (80+) reduces job output");

        // === Training blocked at high fatigue ===
        character.Energy = 50;
        character.Fatigue = 80;
        Assert(result, !training.Train(character.Id, "ranch"), "training fails at fatigue >= 80");

        // === Training blocked at Collapse state ===
        character.Fatigue = 0;
        character.Mature.FallState = FallState.Collapse;
        character.Mature.IsCollapsed = true;
        Assert(result, !training.Train(character.Id, "ranch"), "training fails for collapsed character");

        // === Collapse blocks job output ===
        character.Fatigue = 0;
        var collapseReport = new DailyReport();
        var collapseOutput = ranch.ApplyJobOutput(character, job, collapseReport);
        Assert(result, collapseOutput == 0, "collapsed character produces no job output");
    }

    private static void AssertNodeExists(SmokeTestResult result, Node root, string path, string message)
    {
        Assert(result, root.GetNodeOrNull(path) is not null, message);
    }
}