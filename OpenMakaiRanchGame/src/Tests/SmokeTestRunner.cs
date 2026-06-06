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
            TestSaveRoundTrip(result);
            TestSceneAuthoredUiNodes(result);
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
        var ranch = new RanchService(state, data);
        var economy = new EconomyService(state);
        var milestones = new MilestoneService(state, data, economy);
        var settlement = new DailySettlementService(state, data, schedule, ranch, economy, new DayCycleService(state), milestones);

        schedule.AssignJob("slay", "pasture");
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
                AssertNodeExists(result, mainMenu, "Root/Center/Panel/Content/QuitButton", "main menu has quit button node");
                AssertNodeExists(result, mainMenu, "Root/Center/Panel/Content/StatusLabel", "main menu has status label node");
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
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/EndDayButton", "game shell has end day button node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/DayChip/DayLabel", "game shell has day label node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/PhaseChip/PhaseLabel", "game shell has phase label node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/TopBar/GoldChip/GoldLabel", "game shell has gold label node");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/Body/ContentPanel/Scroll/Content", "game shell has dynamic content root node");

            var navigation = game.GetNodeOrNull<VBoxContainer>("UiShell/Margin/RootPanel/Root/Body/NavPanel/Navigation");
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
                "SaveLoadButton",
                "SettingsButton"
            };
            var actualNavButtons = navigation?.GetChildren()
                .OfType<Button>()
                .Select(button => button.Name.ToString())
                .ToHashSet() ?? new HashSet<string>();
            Assert(result, expectedNavButtons.SetEquals(actualNavButtons), "game shell has expected navigation button set");
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

        Assert(result, generated.Count == 2, "new game creates two generated recruits");
        Assert(result, startingRecruits.Count == 2, "new game generated recruits are marked as starting recruits");
        Assert(result, generated.All(character => !string.IsNullOrWhiteSpace(character.DisplayNameOverride)), "generated recruits receive display names");
        Assert(result, generated.Select(character => character.Id).Distinct().Count() == generated.Count, "generated recruits receive unique ids");
        Assert(result, generated.All(character => character.Hp > 0 && character.Energy > 0), "generated recruits have positive hp and energy");
        Assert(result, generated.All(character => !string.IsNullOrWhiteSpace(character.PortraitPathOverride)), "generated recruits receive portrait overrides");
        Assert(result, generated.All(character => !string.IsNullOrWhiteSpace(character.BodyImagePathOverride)), "generated recruits receive body image overrides");
        Assert(result, generated.All(character => !string.IsNullOrWhiteSpace(character.BodyTypeOverride)), "generated recruits receive body type overrides");
        Assert(result, generated.All(character => character.RaceLayerIndex >= 0), "generated recruits receive race layer indices");
        Assert(result, generated.All(character => character.HairLayerIndex >= 0), "generated recruits receive hair layer indices");
        Assert(result, generated.All(character => character.ClothLayerIndex >= 0), "generated recruits receive cloth layer indices");
        var uniqueLayerProfiles = generated
            .Select(character => $"{character.BodyLayerIndex}:{character.RaceLayerIndex}:{character.HairLayerIndex}:{character.ClothLayerIndex}")
            .Distinct()
            .Count();
        Assert(result, uniqueLayerProfiles >= 2, "generated recruits vary in layered visual profile");

        var resolved = roster.DefinitionFor(generated[0]);
        Assert(result, resolved.DisplayName == generated[0].DisplayNameOverride, "roster resolves generated recruit name overrides");
        Assert(result, resolved.MaxHp == generated[0].MaxHpOverride, "roster resolves generated recruit max hp overrides");
        Assert(result, resolved.BodyType == generated[0].BodyTypeOverride, "roster resolves generated recruit body type override");

        var originalGeneratedIds = generated.Select(character => character.Id).ToHashSet();
        var retainedGold = state.Economy.Gold;
        new SaveStateFactory(data, new Random(84)).RerollGeneratedRecruits(state);
        var rerolledGenerated = state.Roster.Characters.Where(character => character.IsGenerated).ToList();
        Assert(result, rerolledGenerated.Count == 2, "reroll keeps two generated recruits");
        Assert(result, rerolledGenerated.All(character => !originalGeneratedIds.Contains(character.Id)), "reroll replaces generated recruit ids");
        Assert(result, state.Economy.Gold == retainedGold, "reroll recruits does not reset economy state");
    }

    private static void TestManagementLoops(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var economy = new EconomyService(state);
        var inventory = new InventoryService(state);
        var ranch = new RanchService(state, data);
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
        var ranch = new RanchService(state, data);

        Assert(result, bond.AvailableEvents("slay").Any(), "bond events are available at starting bond");
        Assert(result, bond.CompleteEvent("slay_morning_rounds"), "bond event completes");
        Assert(result, state.Bond.CompletedEventIds.Contains("slay_morning_rounds"), "bond event stored in save state");
        Assert(result, state.Roster.Characters.First(character => character.Id == "slay").Bond >= 7, "bond event increases bond");

        var slay = state.Roster.Characters.First(character => character.Id == "slay");
        var reportWithoutResearch = new DailyReport();
        var pasture = data.Job("pasture");
        var outputWithoutResearch = ranch.ApplyJobOutput(slay, pasture, reportWithoutResearch);
        state.Ranch.Stockpile["farm_goods"] = 0;
        state.Research.UnlockedSkillIds.Add("ranch_planning");
        var reportWithResearch = new DailyReport();
        var outputWithResearch = ranch.ApplyJobOutput(slay, pasture, reportWithResearch);
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

    private static void AssertNodeExists(SmokeTestResult result, Node root, string path, string message)
    {
        Assert(result, root.GetNodeOrNull(path) is not null, message);
    }
}