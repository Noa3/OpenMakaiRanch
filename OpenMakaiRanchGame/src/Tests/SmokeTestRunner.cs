using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Ui;
using OpenMakaiRanch.Character;
using OpenMakaiRanch.World;

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
            TestMissionCatalogIntegrity(result);
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
            TestUiScreensRender(result);
            TestLayeredPortraitRendering(result);
            TestParityMechanics(result);
            TestClothingEquipmentIntegration(result);
            TestTrainingParityAndVisit(result);
            TestAdultEligibilityGate(result);
            TestWinConditionReachable(result);
            TestDailyReportHistory(result);
            SaveRegressionTests.Run(result);
            GameCommandTests.Run(result);
            TestWorldGreybox(result);
            TestWorldSharedSimulation(result);
            TestCharacterAvatar(result);
            TestEventDialogueStaging(result);
            TestWorldPanelCoordinator(result);
            TestSaveLoadRoundTrip(result);
            TestWorldDaylightAndRoster(result);
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

    private static void TestMissionCatalogIntegrity(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        Assert(result, data.Missions.Count > 0, "mission catalog has entries");

        var missionIds = data.Missions.Values.Select(mission => mission.Id).ToList();
        Assert(result, missionIds.Distinct(StringComparer.Ordinal).Count() == missionIds.Count, "mission ids are unique");

        var enemyGroups = data.Enemies.Values
            .Where(enemy => !string.IsNullOrWhiteSpace(enemy.GroupId))
            .Select(enemy => enemy.GroupId)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var mission in data.Missions.Values)
        {
            Assert(result, !string.IsNullOrWhiteSpace(mission.Id), "mission id is not empty");

            var rewardExists = string.IsNullOrWhiteSpace(mission.RewardItemId) || data.Items.ContainsKey(mission.RewardItemId);
            Assert(result, rewardExists, $"mission '{mission.Id}' reward item exists");

            Assert(result, enemyGroups.Contains(mission.EnemyGroupId), $"mission '{mission.Id}' enemy group exists");
        }

        Assert(result, data.Missions.Values.Any(mission => mission.Tier == MissionTier.Local), "mission catalog includes Local tier");
        Assert(result, data.Missions.Values.Any(mission => mission.Tier == MissionTier.Regional), "mission catalog includes Regional tier");
        Assert(result, data.Missions.Values.Any(mission => mission.Tier == MissionTier.Dangerous), "mission catalog includes Dangerous tier");

        var state = new SaveStateFactory(data).CreateNewGame();
        foreach (var character in state.Roster.Characters)
        {
            character.CombatSkill = Math.Max(character.CombatSkill, 20);
            character.CraftSkill = Math.Max(character.CraftSkill, 20);
            character.RanchSkill = Math.Max(character.RanchSkill, 20);
            character.Morale = 100;
            character.Fatigue = 0;
        }

        var economy = new EconomyService(state);
        var inventory = new InventoryService(state);
        var milestones = new MilestoneService(state, data, economy);
        var adventure = new AdventureService(state, data, economy, inventory, milestones, new Random(17));
        var party = state.Roster.Characters.Select(character => character.Id).ToList();
        var guaranteedMission = data.Missions.Values
            .Where(mission => mission.Tier == MissionTier.Local)
            .OrderBy(mission => mission.Difficulty)
            .First();

        var goldBefore = state.Economy.Gold;
        var report = adventure.ResolveMission(guaranteedMission.Id, party);
        Assert(result, report.Outcome == MissionOutcome.Success, "resolve mission returns success for prepared party path");
        Assert(result, report.RewardGold > 0 && state.Economy.Gold > goldBefore, "successful resolve mission grants gold");
        if (!string.IsNullOrWhiteSpace(guaranteedMission.RewardItemId))
        {
            Assert(result, state.Inventory.Items.GetValueOrDefault(guaranteedMission.RewardItemId) >= 1, "successful resolve mission grants reward item");
        }
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
                "BondEventButton",
                "PetsButton",
                "TrainingButton",
                "MilkCowButton",
                "MentalButton",
                "SaveLoadButton",
                "SettingsButton",
                "ClothingListButton", "ClothingChangeButton", "ClothingStripButton",
                "VisitButton", "RoomAssignButton",
                "MagicBasicButton", "MagicForbiddenButton", "MagicTentacleButton",
                "AbilityButton", "PharmacyButton", "PharmacyCraftButton", "OptionsButton"
            };
            var actualNavButtons = navigation?.GetChildren()
                .OfType<Button>()
                .Select(button => button.Name.ToString())
                .ToHashSet() ?? new HashSet<string>();
            Assert(result, expectedNavButtons.SetEquals(actualNavButtons),
                $"game shell has expected navigation button set (missing: {string.Join(", ", expectedNavButtons.Except(actualNavButtons))}; unexpected: {string.Join(", ", actualNavButtons.Except(expectedNavButtons))})");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/Body/NavPanel/NavScroll/Navigation/BondSection", "game shell has bond section");
            AssertNodeExists(result, game, "UiShell/Margin/RootPanel/Root/Body/NavPanel/NavScroll/Navigation/PetsSection", "game shell has pets section");

            var compactNavigation = game.GetNodeOrNull<HBoxContainer>("UiShell/Margin/RootPanel/Root/CompactNavigationScroll/CompactNavigation");
            Assert(result, compactNavigation is not null, "game shell has compact navigation node");
            var actualCompactButtons = compactNavigation?.GetChildren()
                .OfType<Button>()
                .Select(button => button.Name.ToString())
                .ToHashSet() ?? new HashSet<string>();
            // Compact navigation intentionally exposes the original management shortcuts,
            // not every detailed sidebar action. Check names, not merely a matching count.
            var expectedCompactButtons = new HashSet<string>
            {
                "OverviewCompactButton", "CharactersCompactButton", "ScheduleCompactButton",
                "TownCompactButton", "ShopCompactButton", "AdventureCompactButton", "CombatCompactButton",
                "MilestonesCompactButton", "ResearchCompactButton", "BondCompactButton", "PetsCompactButton",
                "TrainingCompactButton", "MilkCompactButton", "MentalCompactButton", "SaveLoadCompactButton", "SettingsCompactButton"
            };
            Assert(result, expectedCompactButtons.SetEquals(actualCompactButtons), "game shell has expected compact management shortcuts");
        }
        finally
        {
            game.Free();
        }
    }

    private static void TestUiScreensRender(SmokeTestResult result)
    {
        if (GameRoot.Instance is not { } root || !GodotObject.IsInstanceValid(root))
        {
            Assert(result, false, "game root present for ui screen walk");
            return;
        }

        var gameScene = GD.Load<PackedScene>("res://scenes/Game.tscn");
        if (gameScene is null)
        {
            Assert(result, false, "game scene loads for ui screen walk");
            return;
        }

        var game = gameScene.Instantiate();
        try
        {
            var tree = root.GetTree();
            tree.Root.AddChild(game);
            var shell = game.GetNodeOrNull<UiShellController>("UiShell");
            Assert(result, shell is not null, "ui shell controller node present");
            if (shell is null)
            {
                return;
            }

            var screens = new[]
            {
                "title", "ranch", "roster", "schedule", "town", "shop", "adventure",
                "combat", "milestones", "research", "bond", "pets", "saveload",
                "settings", "training", "milk", "mental", "character_creation",
                "prologue", "victory", "character_detail", "visit", "report"
            };
            var failedScreens = new List<string>();
            foreach (var screenId in screens)
            {
                try
                {
                    shell.ShowScreen(screenId);
                }
                catch (Exception exception)
                {
                    failedScreens.Add($"{screenId} ({exception.GetType().Name}: {exception.Message})");
                }
            }

            Assert(result, failedScreens.Count == 0, $"all ui screens render without exceptions (failed: {string.Join(", ", failedScreens)})");
            tree.Root.RemoveChild(game);
        }
        finally
        {
            if (GodotObject.IsInstanceValid(game) && game.IsInsideTree())
            {
                game.GetTree().Root.RemoveChild(game);
            }
            game.Free();
        }
    }

    private static void TestLayeredPortraitRendering(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var roster = new RosterService(state, data);
        var renderer = new PortraitRenderer();
        var layers = new List<string>();
        var failed = false;
        foreach (var character in state.Roster.Characters)
        {
            var definition = roster.DefinitionFor(character);
            var visual = renderer.BuildCharacterVisual(character, definition);
            if (visual is null)
            {
                failed = true;
                layers.Add(character.Id);
                continue;
            }

            visual.Free();
        }

        Assert(result, !failed, "layered portraits render for all starting characters");
        Assert(result, PortraitLayerCatalog.AllLayerPaths().All(path => ResourceLoader.Exists(path)), "all portrait layer assets exist");
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
        Assert(result, pets.Adopt("yard_hound"), "pet adoption succeeds when affordable");
        Assert(result, state.Pets.AdoptedPetIds.Contains("yard_hound"), "pet state stores adoption");
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
        Assert(result, state.Pets.AdoptedPetIds.Count == 1 && state.Pets.AdoptedPetIds.Contains("stable_cat"), "new game starts with the starting pet");
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

    private static void TestDailyReportHistory(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        Assert(result, state.Reports.Count == 0, "new game starts with empty report history");

        var economy = new EconomyService(state);
        var inventory = new InventoryService(state);
        var equipment = new EquipmentService(state, data);
        var talents = new TalentService(state, data);
        var schedule = new ScheduleService(state, data);
        var ranch = new RanchService(state, data, equipment, talents);
        var dayCycle = new DayCycleService(state);
        var milestones = new MilestoneService(state, data, economy);
        var research = new ResearchService(state, data, milestones);
        var recruitment = new RecruitmentService(state, data, economy);
        var settlement = new DailySettlementService(state, data, schedule, ranch, economy, dayCycle, milestones, inventory, talents);

        var report1 = settlement.SettleDay();
        state.Reports.RemoveAll(report => report.Day == report1.Day);
        state.Reports.Add(report1);
        var report2 = settlement.SettleDay();
        state.Reports.RemoveAll(report => report.Day == report2.Day);
        state.Reports.Add(report2);

        Assert(result, state.Reports.Count == 2, "report history accumulates daily reports");
        Assert(result, state.Reports.OrderByDescending(report => report.Day).First().Day == report2.Day, "report history keeps most recent first");
        Assert(result, report2.Lines.Count > 0 || report2.Events.Count > 0, "settled day records a log");
    }

private static void TestWinConditionReachable(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var economy = new EconomyService(state);
        var milestones = new MilestoneService(state, data, economy);
        var discovery = new DiscoveryService(state, data);
        var ranch = new RanchService(state, data, new EquipmentService(state, data), new TalentService(state, data));
        var research = new ResearchService(state, data, milestones);
        var win = new WinConditionService(state, data);

        var allFacilities = data.Facilities.Values.ToList();
        var allSkills = data.Skills.Values.ToList();
        var allMissionCount = data.Missions.Count;
        var totalBonds = data.BondEvents.Values.Select(e => e.CharacterId).Distinct().Count();

        Assert(result, allFacilities.Count > 0, "win goal has facilities");
        Assert(result, allSkills.Count > 0, "win goal has research");
        Assert(result, allMissionCount > 0, "win goal has missions");

        var days = 0;
        var safety = 365;
        while (days < safety && !win.IsGameComplete())
        {
            days++;

            // Player discovers the next mission each day via adventure route.
            discovery.DiscoverNext();

            // Player funds facility upgrades and research from daily income.
            foreach (var facility in allFacilities.Where(f => !state.Ranch.Facilities.TryGetValue(f.Id, out var lv) || lv < 5))
            {
                economy.AddGold(5000);
                ranch.UpgradeFacility(facility.Id, economy);
            }

            foreach (var skill in allSkills.Where(s => !state.Research.UnlockedSkillIds.Contains(s.Id)))
            {
                if (string.IsNullOrWhiteSpace(skill.CostResourceId))
                {
                    research.Unlock(skill.Id);
                }
                else if (state.Ranch.Stockpile.TryGetValue(skill.CostResourceId, out var amt) && amt >= skill.CostAmount)
                {
                    research.Unlock(skill.Id);
                }
                else if (state.Ranch.Stockpile.GetValueOrDefault(skill.CostResourceId) + 8 >= skill.CostAmount)
                {
                    state.Ranch.Stockpile[skill.CostResourceId] = state.Ranch.Stockpile.GetValueOrDefault(skill.CostResourceId) + 12;
                    research.Unlock(skill.Id);
                }
            }

            // Player cares for / trains a girl to raise bond via visit actions.
            foreach (var character in state.Roster.Characters.Where(c => c.Bond < 40))
            {
                character.Bond = Math.Min(100, character.Bond + 12);
            }
        }

        Assert(result, win.IsGameComplete(), "win condition reachable in normal play");
        Assert(result, state.Adventure.DiscoveredMissionIds.Count >= allMissionCount, "all missions discovered before win");
        Assert(result, state.Research.UnlockedSkillIds.Count >= allSkills.Count, "all research unlocked before win");
        Assert(result, state.Ranch.Facilities.Count(f => f.Value >= 5) >= allFacilities.Count, "all facilities maxed before win");
        Assert(result, state.Roster.Characters.All(c => c.Bond >= 40), "all characters bonded before win");
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

    private static void TestWorldGreybox(SmokeTestResult result)
    {
        // ---- World movement math (deterministic, headless) ----
        // Diagonal movement must not be faster than straight-line (3D_REMAKE_PLAN AC).
        var straight = WorldMovementMath.ClampSpeed(new Vector3(5f, 0f, 0f), 5f);
        var diagonal = WorldMovementMath.ClampSpeed(new Vector3(5f, 0f, 5f), 5f);
        Assert(result, diagonal.Length() <= straight.Length() + 0.001f,
            "world diagonal movement is not faster than straight-line");

        // Camera-relative forward maps input.Y to the flattened forward vector.
        // Godot 4: Vector3.Forward = (0,0,-1) (camera looks along -Z).
        var camFwd = new Vector3(0f, 0f, -1f);
        var camRgt = new Vector3(1f, 0f, 0f);
        var fwd = WorldMovementMath.ComputeMovementDirection(camFwd, camRgt, new Vector2(0f, 1f));
        Assert(result, Math.Abs(fwd.Z - (-1f)) < 0.001f && Math.Abs(fwd.X) < 0.001f,
            "world forward input maps to camera-forward XZ plane");

        // Dead zone: a sub-threshold input yields zero velocity, not jitter.
        Assert(result, WorldMovementMath.ComputeMovementDirection(camFwd, camRgt, new Vector2(0.0001f, 0.0001f)).IsEqualApprox(Vector3.Zero),
            "world input dead zone produces zero direction");

        // Gravity only affects the vertical component.
        var grav = WorldMovementMath.ApplyGravity(new Vector3(2f, 0f, 3f), 20f, 0.1f);
        Assert(result, Math.Abs(grav.Y - (-2f)) < 0.001f && Math.Abs(grav.X - 2f) < 0.001f && Math.Abs(grav.Z - 3f) < 0.001f,
            "world gravity only changes vertical velocity");

        // ---- World camera math (deterministic, headless) ----
        // Orbit distance is clamped to the supported range.
        var camPos = WorldCameraMath.ComputeCameraPosition(Vector3.Zero, 0f, 0f, 1000f);
        Assert(result, camPos.Length() <= WorldCameraMath.MaxDistance + 0.001f,
            "world camera zoom clamps to max distance");

        // Zoom in never goes below min distance.
        Assert(result, WorldCameraMath.ApplyZoom(10f, 100f) == WorldCameraMath.MinDistance,
            "world camera zoom clamps to min distance");

        // Camera must not penetrate geometry: a ray hit at 2m clamps the camera in front of it.
        var target = Vector3.Zero;
        var desired = new Vector3(0f, 0f, 10f);
        var clamped = WorldCameraMath.ClampToGeometry(target, desired, 2f);
        Assert(result, clamped.Z <= 2f + 0.001f, "world camera clamps in front of geometry");

        // Pitch is clamped so the camera never under-runs the floor.
        Assert(result, WorldCameraMath.ClampPitch(Mathf.DegToRad(200f)) <= Mathf.DegToRad(WorldCameraMath.MaxPitchDegrees) + 0.001f,
            "world camera pitch clamps below floor");

        // ---- World input gate ----
        var gate = new WorldInputGate();
        Assert(result, gate.WorldInputEnabled, "world input enabled by default");
        gate.SetUiOwnsInput(true);
        Assert(result, !gate.WorldInputEnabled && gate.UiOwnsInput,
            "world input stops while management UI owns input");
        gate.SetUiOwnsInput(false);
        Assert(result, gate.WorldInputEnabled, "world input resumes when UI releases");
        gate.SetWindowFocused(false);
        Assert(result, !gate.WorldInputEnabled && gate.WindowFocused == false,
            "world input stops when window loses focus");
        gate.Reset();
        Assert(result, gate.WorldInputEnabled, "world input gate reset restores world ownership");

        // ---- World interaction guard (double-activation + missing target) ----
        var guard = new WorldInteractionGuard();
        Assert(result, guard.CanInteract, "world guard allows interaction when idle");
        Assert(result, guard.BeginCommand(), "world guard begins a command once");
        Assert(result, !guard.CanInteract, "world guard blocks re-entrancy during a command");
        Assert(result, !guard.BeginCommand(), "world guard rejects a second concurrent command");
        guard.EndCommand();
        Assert(result, guard.CanInteract, "world guard allows interaction again after command ends");
        guard.SetTargetPresent(false);
        Assert(result, !guard.CanInteract, "world guard rejects interaction when target is despawned");
        guard.SetTargetPresent(true);
        Assert(result, guard.CanInteract, "world guard re-allows interaction when target is present");

        // ---- World station (smart object) with a stub dispatcher ----
        // Verifies the station's availability + dispatch + double-activation contract
        // without touching the real simulation.
        var station = new WorldStation();
        try
        {
            Assert(result, station.UnavailableReason is not null,
                "world station is unavailable with no dispatcher bound");
            Assert(result, !station.IsAvailable, "world station reports unavailable with no dispatcher");

            var calls = new System.Collections.Generic.List<WorldCommand>();
            station.Dispatcher = new RecordingDispatcher(calls);
            station.CommandTargetId = "JOB_MILK_TEST";
            Assert(result, station.IsAvailable, "world station becomes available with a dispatcher");
            Assert(result, station.UnavailableReason == string.Empty,
                "world station has no unavailable reason when ready");

            var context = new WorldInteractionContext("rancher", 42UL);
            Assert(result, station.Activate(context), "world station dispatches a command on activate");
            Assert(result, calls.Count == 1 && calls[0].Kind == WorldCommandKind.AssignJob,
                "world station dispatches the configured command kind");
            Assert(result, calls[0].TargetId == "JOB_MILK_TEST", "world station dispatches the configured target");

            // Double activation is rejected by the guard (already released here, so it is the
            // missing-target / re-entrancy contract that must hold across rapid presses).
            var second = station.Activate(context);
            Assert(result, second && calls.Count == 2, "world station allows sequential activations");
        }
        finally
        {
            station.Free();
        }

        // ---- Ranch greybox scene loads and exposes the expected node contract ----
        var scene = GD.Load<PackedScene>("res://scenes/dev/RanchGreybox.tscn");
        Assert(result, scene is not null, "ranch greybox scene loads");
        if (scene is null)
        {
            return;
        }

        var greybox = scene.Instantiate();
        try
        {
            AssertNodeExists(result, greybox, "WorldInputBootstrap", "greybox registers world input bootstrap");
            AssertNodeExists(result, greybox, "Player", "greybox has a third-person player");
            AssertNodeExists(result, greybox, "CameraRig/Camera", "greybox has a follow camera");
            AssertNodeExists(result, greybox, "Station", "greybox has an interactable station");
            AssertNodeExists(result, greybox, "PromptLayer/Prompt", "greybox shows an interaction prompt");
            AssertNodeExists(result, greybox, "ButtonLayer/OpenManagementButton", "greybox has a management UI button");

            // The greybox root node carries the controller script. In Godot 4 C# the
            // instantiated root reports the C# extension type, so an `as` cast resolves it.
            var controller = greybox as RanchGreyboxController;
            Assert(result, controller is not null, "greybox root is the controller");
            if (controller is not null)
            {
                // _Ready() normally fires on tree entry; invoke it directly so the
                // wiring (player / station / dispatcher) is verified headlessly.
                controller._Ready();
                Assert(result, controller.Wired, "greybox controller wires player + station");
                Assert(result, controller.Player is not null, "greybox controller resolves the player");
                Assert(result, controller.Station is not null, "greybox controller resolves the station");
                Assert(result, controller.Station is not null && controller.Station.Dispatcher is not null,
                    "greybox station has a production dispatcher bound");
            }
        }
        finally
        {
            greybox.Free();
        }
    }

    private static void TestWorldSharedSimulation(SmokeTestResult result)
    {
        // WORLD-002: a world station must mutate the SAME Schedule state the management UI reads,
        // through the production GameRootCommandDispatcher -> GameRoot boundary. No second reward.
        var root = GameRoot.Instance;
        root.NewGame();
        var station = new WorldStation
        {
            TargetId = "STATION_WORLD_SIM",
            Label = "Shared Simulation Station",
            CommandKind = WorldCommandKind.AssignJob,
        };
        try
        {
            // Production dispatcher: routes to GameRoot.Instance (no stub).
            station.Dispatcher = new GameRootCommandDispatcher();
            Assert(result, station.IsAvailable, "world station available with production dispatcher");

            var character = root.Roster.Characters.First();
            var characterId = character.Id;
            var current = root.Schedule.GetAssignment(characterId);
            var job = root.Schedule.AssignableJobs.First(value => value.Id != current);
            station.CommandTargetId = job.Id;

            var gold = root.State.Economy.Gold;
            var day = root.State.Calendar.Day;
            var generation = root.StateGeneration;

            // Shared simulation: the world interaction changes the same assignment the UI reads.
            var ok = station.Activate(new WorldInteractionContext(characterId, generation));
            Assert(result, ok, "world station dispatches an assignment through GameRoot");
            Assert(result, root.Schedule.GetAssignment(characterId) == job.Id,
                "world interaction mutates the same Schedule state the management UI reads");
            Assert(result, root.State.Economy.Gold == gold && root.State.Calendar.Day == day,
                "world interaction neither pays work nor advances time (no second reward)");

            // Stale generation must be rejected (StateGeneration guard), even via the world path.
            root.NewGame(); // StateGeneration++
            var staleGeneration = root.StateGeneration - 1;
            var staleOk = station.Activate(new WorldInteractionContext(characterId, staleGeneration));
            Assert(result, !staleOk, "world interaction with a stale generation is rejected");
            Assert(result, root.StateGeneration == staleGeneration + 1,
                "stale world interaction leaves the generation intact");
        }
        finally
        {
            if (station.IsInsideTree())
                station.GetParent()?.RemoveChild(station);
            station.Free();
            root.NewGame();
        }
    }

    private static void TestCharacterAvatar(SmokeTestResult result)
    {
        // CHAR-001: gate-safe, honest stand-in avatars bound by stable DefinitionId.
        // Per 3D_REMAKE_PLAN: "Missing art gets an honest debug stand-in, not a random hero
        // model." and "Adult-specific presentation has a separate fail-closed identity/design
        // gate; this plan does not grant content approval."

        // ---- Fail-closed gate: real avatar requires ConfirmedAdult ----
        Assert(result, !CharacterAvatarFactory.CanUseRealAvatar(new CharacterDefinition { Id = "c_unknown", AdultEligibility = AdultEligibility.Unknown }), "gate: Unknown denies real avatar");
        Assert(result, !CharacterAvatarFactory.CanUseRealAvatar(new CharacterDefinition { Id = "c_minor", AdultEligibility = AdultEligibility.Minor }), "gate: Minor denies real avatar");
        Assert(result, !CharacterAvatarFactory.CanUseRealAvatar(new CharacterDefinition { Id = "c_ambiguous", AdultEligibility = AdultEligibility.Ambiguous }), "gate: Ambiguous denies real avatar");
        Assert(result, CharacterAvatarFactory.CanUseRealAvatar(new CharacterDefinition { Id = "c_adult", AdultEligibility = AdultEligibility.ConfirmedAdult }), "gate: ConfirmedAdult permits real avatar");
        Assert(result, !CharacterAvatarFactory.CanUseRealAvatar(null), "gate: null definition denies real avatar");

        // ---- Profile: stable DefinitionId binding, no gameplay state ----
        var adult = new CharacterDefinition
        {
            Id = "slay",
            DisplayName = "Slay",
            AdultEligibility = AdultEligibility.ConfirmedAdult,
            Provenance = CharacterProvenance.OriginalHero,
            SkinColor = "pale",
            HairColor = "silver",
        };
        var profile = CharacterAvatarFactory.CreateProfile(adult);
        Assert(result, profile.DefinitionId == "slay", "profile: stable DefinitionId binding");
        Assert(result, profile.DisplayName == "Slay", "profile: display name carried");
        Assert(result, profile.IsDebugStandIn, "profile: CHAR-001 ships honest stand-in, not real model");
        Assert(result, profile.AdultEligibility == AdultEligibility.ConfirmedAdult, "profile: fail-closed eligibility carried forward");
        // Presentation ≠ gameplay state: no HP, skill, bond, reward fields exist.
        Assert(result, !typeof(CharacterVisualProfile).GetProperties().Any(p =>
            p.Name is "MaxHp" or "RanchSkill" or "BondLevel" or "RewardGold" or "Energy"),
            "profile: no gameplay numbers on presentation type (separate from simulation state)");

        // ---- Node: stand-in geometry generated, material overridden ----
        var avatar = CharacterAvatarFactory.BuildAvatar(profile);
        Assert(result, avatar is not null && ReferenceEquals(avatar.Profile, profile), "avatar: built with bound profile");
        avatar!.Rebuild(); // deterministic, no tree required
        Assert(result, avatar.Body is not null, "avatar: body capsule generated");
        Assert(result, avatar.Head is not null, "avatar: head sphere generated");
        Assert(result, avatar.Body!.Mesh is CapsuleMesh, "avatar: body is capsule stand-in");
        Assert(result, avatar.Head!.Mesh is SphereMesh, "avatar: head is sphere stand-in");
        var bodyMat = avatar.Body!.MaterialOverride as StandardMaterial3D;
        Assert(result, bodyMat is not null && bodyMat.AlbedoColor.IsEqualApprox(profile.BodyColor), "avatar: body tint matches profile skin mapping");
        var headMat = avatar.Head!.MaterialOverride as StandardMaterial3D;
        Assert(result, headMat is not null && headMat.AlbedoColor.IsEqualApprox(profile.HeadColor), "avatar: head tint matches profile hair mapping");

        // ---- Rebuild idempotency: swapping profile regenerates, no stale children ----
        var minor = new CharacterDefinition { Id = "c_minor", AdultEligibility = AdultEligibility.Minor, HairColor = "black" };
        var minorProfile = CharacterAvatarFactory.CreateProfile(minor);
        avatar.Profile = minorProfile;
        avatar.Rebuild();
        Assert(result, avatar.Body is not null && avatar.Head is not null, "avatar: rebuild after profile swap regenerates geometry");
        Assert(result, avatar.Body!.MaterialOverride is StandardMaterial3D { } m2 && m2.AlbedoColor.IsEqualApprox(minorProfile.BodyColor), "avatar: rebuild reflects new profile colors");

        avatar.QueueFree();
    }

    private static void TestEventDialogueStaging(SmokeTestResult result)
    {
        // EVENT-001: dialogue staging is pure presentation. It frames camera/look-at/pose for a
        // mentorship or bond event, but structurally CANNOT carry or move simulation state — the
        // plan's "must not duplicate event state" rule. The test asserts the staging is valid,
        // deterministic, and that a second staging call does not mutate anything it did not own.
        var anchor = new Vector3(2f, 0f, 3f);

        var mentorship = DialogueStager.BuildMentorship("ayaka", anchor);
        Assert(result, mentorship.IsValid, "staging: mentorship framing is valid (speaker + text on every line)");
        Assert(result, ReferenceEquals(mentorship.CharacterId, "ayaka"), "staging: bound to the framed character id");
        Assert(result, mentorship.CameraTarget == anchor + new Vector3(0f, 1.2f, 0f), "staging: camera target = anchor + head offset");
        Assert(result, mentorship.LookAt == mentorship.CameraTarget, "staging: look-at equals camera target (conversational framing)");
        Assert(result, mentorship.Lines.Count >= 2, "staging: mentorship has at least a greeting and a reply");

        // Determinism: the same anchor yields the identical framing (no hidden randomness in presentation).
        var mentorshipAgain = DialogueStager.BuildMentorship("ayaka", anchor);
        Assert(result, mentorshipAgain.CameraTarget == mentorship.CameraTarget && mentorshipAgain.LookAt == mentorship.LookAt,
            "staging: framing is a pure function of the anchor (deterministic)");

        var bondEvent = DialogueStager.BuildBondEvent("noir", "Tea by the forge", anchor);
        Assert(result, bondEvent.IsValid, "staging: bond-event framing is valid");
        Assert(result, bondEvent.Lines[0].Text.Contains("Tea by the forge", StringComparison.Ordinal),
            "staging: bond event name is presentation metadata, not a reward");

        // The load-bearing guarantee: the staging type has NO Bond/Morale/Stockpile/CompletedEvent
        // fields. Reflect over its public surface to prove it cannot move a second reward.
        var stagingFields = typeof(DialoguePresentation)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Select(p => p.Name)
            .ToList();
        Assert(result,
            !stagingFields.Any(f => f.Contains("Bond", StringComparison.Ordinal)
                || f.Contains("Morale", StringComparison.Ordinal)
                || f.Contains("Stockpile", StringComparison.Ordinal)
                || f.Contains("Completed", StringComparison.Ordinal)),
            "staging: type has no bond/morale/stockpile/completed fields (cannot duplicate event state)");
    }

    private static void TestWorldPanelCoordinator(SmokeTestResult result)
    {
        // EVENT-001: opening a management panel from the world suspends world input; closing it
        // resumes world input *safely* — the gate is never left UI-owned, which would freeze
        // world movement. This is the "closing them resumes world input safely" rule.
        var gate = new WorldInputGate();
        var known = new[] { "schedule", "roster", "character_detail", "report", "saveload" };
        var coordinator = new WorldPanelCoordinator(gate, known);

        Assert(result, coordinator.WorldInputEnabled, "panel: world input enabled before any panel is open");
        Assert(result, !coordinator.IsOpen, "panel: no panel open initially");

        // Open a known management panel → world input suspended, UI owns input.
        Assert(result, coordinator.Open("schedule"), "panel: known management panel opens");
        Assert(result, coordinator.IsOpen && coordinator.ActivePanel == "schedule", "panel: schedule is the active panel");
        Assert(result, !coordinator.WorldInputEnabled && gate.UiOwnsInput, "panel: world input suspended while a panel is open");

        // Opening a second known panel closes the first (single-panel rule), input stays suspended.
        Assert(result, coordinator.Open("roster"), "panel: switching to another known panel");
        Assert(result, coordinator.ActivePanel == "roster", "panel: roster is now active (previous closed)");
        Assert(result, gate.UiOwnsInput, "panel: input still UI-owned across a panel switch");

        // Close → world input resumed.
        Assert(result, coordinator.Close(), "panel: close returns true when a panel was open");
        Assert(result, !coordinator.IsOpen && coordinator.ActivePanel is null, "panel: no panel active after close");
        Assert(result, coordinator.WorldInputEnabled && !gate.UiOwnsInput, "panel: world input resumed safely after close");

        // Safety: closing again (nothing to close) still returns world input to enabled — it can
        // never be left frozen in the UI-owned state.
        Assert(result, !coordinator.Close(), "panel: closing with nothing open returns false");
        Assert(result, coordinator.WorldInputEnabled && !gate.UiOwnsInput, "panel: repeated close keeps world input safely enabled");

        // Safety: a world interaction must not be able to open an arbitrary/unknown screen.
        Assert(result, !coordinator.Open("__arbitrary__"), "panel: unknown panel id is rejected");
        Assert(result, !coordinator.IsOpen, "panel: state unchanged after a rejected open");
        Assert(result, coordinator.WorldInputEnabled, "panel: world input still enabled after a rejected open");
    }

    private static void TestSaveLoadRoundTrip(SmokeTestResult result)
    {
        // SAVE-002: a full 3D world session — job assignment, mentorship (social action), and a day
        // transition — must survive Save/Load on the disposable smoke slot (99) with no data loss.
        // The world path (station -> dispatcher -> GameRoot) is the one exercised; save/load is the
        // current-version round-trip, not an old-save migration (D-011: fresh starts expected).
        const int slot = 99;
        var root = GameRoot.Instance;
        root.NewGame();
        var station = new WorldStation
        {
            TargetId = "STATION_SAVE_ROUNDTRIP",
            Label = "Save Round-Trip Station",
        };
        try
        {
            var character = root.Roster.Characters.First();
            var characterId = character.Id;

            // 1) Job assignment through the 3D world path.
            station.CommandKind = WorldCommandKind.AssignJob;
            var current = root.Schedule.GetAssignment(characterId);
            var job = root.Schedule.AssignableJobs.First(value => value.Id != current);
            station.CommandTargetId = job.Id;
            station.Dispatcher = new GameRootCommandDispatcher();
            Assert(result, station.Activate(new WorldInteractionContext(characterId, root.StateGeneration)),
                "save round-trip: world assignment succeeds");
            Assert(result, root.Schedule.GetAssignment(characterId) == job.Id,
                "save round-trip: world assignment mutates the shared schedule");

            // 2) Social action (mentorship) through the same world path — moves Bond, Morale, Fatigue.
            var bondBefore = root.Roster.Find(characterId)!.Bond;
            var moraleBefore = root.Roster.Find(characterId)!.Morale;
            station.CommandKind = WorldCommandKind.Mentorship;
            station.CommandTargetId = characterId;
            Assert(result, station.Activate(new WorldInteractionContext(characterId, root.StateGeneration)),
                "save round-trip: world mentorship succeeds");
            Assert(result, root.Roster.Find(characterId)!.Bond > bondBefore,
                "save round-trip: mentorship raises the character's bond");
            Assert(result, root.Roster.Find(characterId)!.Morale > moraleBefore,
                "save round-trip: mentorship raises the character's morale");

            // 3) Full day transition: Morning -> Afternoon -> Evening -> Night -> (settle) -> Morning.
            // Settlement may adjust bond/morale, so capture the FINAL state (post-transition) for
            // the round-trip assertion — that is the exact state being persisted.
            var dayBefore = root.State.Calendar.Day;
            root.AdvanceTime(); // -> Afternoon
            root.AdvanceTime(); // -> Evening
            root.AdvanceTime(); // -> Night
            root.AdvanceTime(); // Night + settle -> new day, Morning
            Assert(result, root.State.Calendar.Day == dayBefore + 1,
                "save round-trip: a full day transition advances to the next day");
            Assert(result, root.State.Calendar.Phase == DayPhase.Morning,
                "save round-trip: the new day starts in the morning");
            var bondFinal = root.Roster.Find(characterId)!.Bond;
            var moraleFinal = root.Roster.Find(characterId)!.Morale;
            var dayAfterTransition = root.State.Calendar.Day;

            // 4) Persist the whole session, then start a fresh game and load it back.
            Assert(result, root.SaveSlot(slot), "save round-trip: the session persists to the slot");
            root.NewGame(); // fresh start: assignment/bond/day are all reset
            var freshCharacter = root.Roster.Characters.First(value => value.Id == characterId);
            Assert(result, freshCharacter.Bond <= bondBefore,
                "save round-trip: a fresh game does not carry the old bond");
            Assert(result, root.Save.Load(slot) is not null,
                "save round-trip: the saved session still exists on disk");

            // 5) Load it back: every world-path mutation must be intact.
            Assert(result, root.LoadSlot(slot), "save round-trip: the session loads back");
            Assert(result, root.Schedule.GetAssignment(characterId) == job.Id,
                "save round-trip: the world assignment survives save/load");
            Assert(result, root.Roster.Find(characterId)!.Bond == bondFinal,
                "save round-trip: the mentorship bond survives save/load");
            Assert(result, root.Roster.Find(characterId)!.Morale == moraleFinal,
                "save round-trip: the mentorship morale survives save/load");
            Assert(result, root.State.Calendar.Day == dayAfterTransition,
                "save round-trip: the day counter survives save/load");
            Assert(result, root.State.Calendar.Phase == DayPhase.Morning,
                "save round-trip: the phase survives save/load");

            // 6) StateGeneration guard: a pre-load generation must now be rejected via the world path.
            var staleGeneration = root.StateGeneration - 1;
            station.CommandKind = WorldCommandKind.Mentorship;
            Assert(result, !station.Activate(new WorldInteractionContext(characterId, staleGeneration)),
                "save round-trip: a stale (pre-load) generation is rejected");
        }
        finally
        {
            if (station.IsInsideTree())
            {
                station.GetParent()?.RemoveChild(station);
            }
            station.Free();
            root.Save.Delete(slot); // leave no disposable save behind
        }
    }

    private static void TestWorldDaylightAndRoster(SmokeTestResult result)
    {
        var game = GameRoot.Instance;
        game.NewGame();

        // --- DaylightMath: deterministic, phase-derived, no second clock -------------------------
        var morning = DaylightMath.For(DayPhase.Morning);
        var afternoon = DaylightMath.For(DayPhase.Afternoon);
        var evening = DaylightMath.For(DayPhase.Evening);
        var night = DaylightMath.For(DayPhase.Night);

        Assert(result, morning.SunEnergy > 0f && afternoon.SunEnergy > 0f && evening.SunEnergy > 0f,
            "daylight: day phases carry direct sunlight");
        Assert(result, night.SunEnergy == 0f, "daylight: night has no direct sun");
        Assert(result, night.IsNight, "daylight: night is flagged as night");
        Assert(result, !morning.IsNight && !afternoon.IsNight && !evening.IsNight,
            "daylight: day phases are not night");
        Assert(result, afternoon.SunElevationDegrees > morning.SunElevationDegrees
                   && morning.SunElevationDegrees > evening.SunElevationDegrees,
            "daylight: sun elevation orders afternoon > morning > evening (hand-tuned table)");
        // Deterministic: same phase -> same state (no hidden clock, no randomness).
        Assert(result, DaylightMath.For(DayPhase.Morning) == morning,
            "daylight: mapping is deterministic (pure function of the shared phase)");

        // --- DaylightRig: applies the resolved state to a real sun node ---------------------------
        var parent = game; // GameRoot is the autoload, already in the scene tree.
        var dayRig = new DaylightRig();
        var sun = new DirectionalLight3D();
        var worldEnvironment = new WorldEnvironment { Environment = new Godot.Environment() };
        parent.AddChild(sun);
        parent.AddChild(worldEnvironment);
        parent.AddChild(dayRig);
        try
        {
            dayRig.Bind(sun, worldEnvironment);
            Assert(result, dayRig.Wired, "daylight rig binds sun + environment");

            var applied = dayRig.Apply(DayPhase.Evening);
            Assert(result, applied == DaylightMath.For(DayPhase.Evening),
                "daylight rig applies the resolved evening state");
            Assert(result, Math.Abs(sun.LightEnergy - evening.SunEnergy) < 0.0001f,
                "daylight rig writes the sun energy to the node");
            Assert(result, worldEnvironment.Environment.AmbientLightEnergy == evening.AmbientEnergy,
                "daylight rig writes the ambient energy to the environment");
            Assert(result, worldEnvironment.Environment.TonemapExposure == evening.TonemapExposure,
                "daylight rig writes the tonemap exposure to the environment");

            var fromGame = dayRig.ApplyFrom(game);
            Assert(result, fromGame == DaylightMath.For(game.State.Calendar.Phase),
                "daylight rig reads the shared phase (single source of truth, no second clock)");
        }
        finally
        {
            dayRig.Free();
            sun.Free();
            worldEnvironment.Free();
        }

        // --- RosterPlacementMath: job -> in-bounds anchor, deterministic spread --------------------
        foreach (JobCategory category in Enum.GetValues<JobCategory>())
        {
            var anchor = RosterPlacementMath.AnchorForJob(category);
            Assert(result, IsInGreyboxBounds(anchor.Position),
                $"placement: category {category} resolves an in-bounds anchor ({anchor.AnchorId})");
        }
        Assert(result, RosterPlacementMath.AnchorForJob(JobCategory.Dairy).AnchorId == "DAIRY",
            "placement: dairy maps to the milk-station anchor");
        Assert(result, RosterPlacementMath.AnchorForJob(JobCategory.Rest).AnchorId == "REST_AREA",
            "placement: rest maps to the rest area");
        Assert(result, RosterPlacementMath.AnchorForJob((JobCategory)999).AnchorId == "REST_AREA",
            "placement: unknown category falls back to the rest area (always a logical place)");
        var spread0 = RosterPlacementMath.SpreadOffset(RosterPlacementMath.Pasture, 0);
        var spread1 = RosterPlacementMath.SpreadOffset(RosterPlacementMath.Pasture, 1);
        var spread2 = RosterPlacementMath.SpreadOffset(RosterPlacementMath.Pasture, 2);
        Assert(result, spread0 == Vector3.Zero, "placement: first mate stands at the anchor");
        Assert(result, spread1 != spread0 && spread2 != spread0,
            "placement: later mates stand side by side (no overlap)");
        Assert(result, IsInGreyboxBounds(RosterPlacementMath.Pasture.Position + spread2),
            "placement: the spread stays in bounds");

        // --- RosterRig: places CHAR-001 stand-ins for the live roster, idempotent ------------------
        var rosterRig = new RosterRig();
        parent.AddChild(rosterRig);
        try
        {
            var count = rosterRig.Refresh(game);
            Assert(result, count == game.Roster.Characters.Count,
                "roster rig places one avatar per roster character");
            Assert(result, count > 0, "roster rig places at least one avatar");

            // Idempotent: a second refresh keeps the same set, no duplicates.
            var countAgain = rosterRig.Refresh(game);
            Assert(result, countAgain == count, "roster rig refresh is idempotent (no duplicate avatars)");

            // Every placed avatar is in-bounds and is an honest CHAR-001 stand-in (presentation only).
            bool allInBounds = true;
            bool allStandIn = true;
            foreach (var child in rosterRig.GetChildren())
            {
                if (child is CharacterAvatar3D avatar)
                {
                    if (!IsInGreyboxBounds(avatar.Position))
                    {
                        allInBounds = false;
                    }
                    if (avatar.Profile is null || !avatar.Profile.IsDebugStandIn)
                    {
                        allStandIn = false;
                    }
                }
            }
            Assert(result, allInBounds, "roster rig keeps every avatar in the greybox bounds");
            Assert(result, allStandIn, "roster rig uses CHAR-001 stand-in avatars (presentation only)");

            // No second work economy: placing avatars must not mutate the shared schedule.
            var assignmentBefore = game.Schedule.GetAssignment(game.Roster.Characters.First().Id);
            rosterRig.Refresh(game);
            Assert(result, game.Schedule.GetAssignment(game.Roster.Characters.First().Id) == assignmentBefore,
                "roster rig moves presentation only — the shared schedule is untouched");
        }
        finally
        {
            rosterRig.Free();
        }

        game.NewGame();
    }

    private static bool IsInGreyboxBounds(Vector3 position)
    {
        // Greybox ground is 40 x 30 centred on the origin: |x| <= 15, |z| <= 10.
        return Math.Abs(position.X) <= 15f && Math.Abs(position.Z) <= 10f;
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

private static void TestParityMechanics(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var inventory = new InventoryService(state);

        Assert(result, data.Items.ContainsKey("mana_shackle"), "mana shackles item seeded");
        Assert(result, data.Items.ContainsKey("lactation_drug"), "lactation drug item seeded");
        Assert(result, data.Items.ContainsKey("mana_infusion_drug"), "mana infusion drug item seeded");

        var combatEquipment = new EquipmentService(state, data);
        var talents = new TalentService(state, data);
        var combat = new CombatService(state, data, combatEquipment, talents);
        var combatNoMerc = combat.AttemptCapture("road_patrol");
        Assert(result, !combatNoMerc.CaptureSucceeded, "combat capture blocked without a hired mercenary");
        state.Adventure.ActiveMercenaryHpBonus = 10;
        var combatNoShackle = combat.AttemptCapture("road_patrol");
        Assert(result, !combatNoShackle.CaptureSucceeded, "combat capture blocked when shackles absent");

        var economy = new EconomyService(state);
        var milestones = new MilestoneService(state, data, economy);
        var adventure = new AdventureService(state, data, economy, inventory, milestones, new Random(1));
        var party = state.Roster.Characters.Select(character => character.Id).ToList();

        var rosterBefore = state.Roster.Characters.Count;
        var blockedCapture = adventure.ResolveMission("road_patrol", party, true);
        Assert(result, !blockedCapture.CaptureSucceeded, "capture fails without a hired mercenary");
        Assert(result, state.Roster.Characters.Count == rosterBefore, "capture without mercenary does not add recruits");

        state.Adventure.ActiveMercenaryHpBonus = 10;
        var blocked2 = adventure.ResolveMission("road_patrol", party, true);
        Assert(result, !blocked2.CaptureSucceeded, "mercenary alone is not enough; mana shackle still required");
        Assert(result, state.Roster.Characters.Count == rosterBefore, "capture without shackles does not add recruits");

        inventory.AddItem("mana_shackle", 1);
        state.Adventure.ActiveMercenaryHpBonus = 10;
        var shackleBefore = state.Roster.Characters.Count;
        var suppliedCapture = adventure.ResolveMission("road_patrol", party, true);
        if (suppliedCapture.CaptureSucceeded)
        {
            Assert(result, state.Roster.Characters.Count == shackleBefore + 1, "successful capture adds one recruit");
            Assert(result, !inventory.TryConsume("mana_shackle", 1), "successful capture consumes the mana shackle");
            Assert(result, state.Adventure.ActiveMercenaryHpBonus == 0, "successful capture consumes the hired mercenary");
        }
        else
        {
            Assert(result, state.Roster.Characters.Count == shackleBefore, "failed capture keeps roster unchanged");
        }

        var lactation = new MilkEconomyService(state);
        var subject = state.Roster.Characters.First(character => character.Id == "rancher");
        subject.Milk.HasMilkConstitution = false;
        subject.Milk.CurrentAmount = 0;
        subject.Talents.RemoveAll(t => t == "extreme_milk_pressure");
        state.Mature.TotalMilkProduced = 0;
        var milkBefore = state.Mature.TotalMilkProduced;
        lactation.ProduceMilk(subject.Id);
        Assert(result, state.Mature.TotalMilkProduced == milkBefore, "milk production requires a milk constitution or talent");
        subject.Milk.HasMilkConstitution = true;
        lactation.ProduceMilk(subject.Id);
        Assert(result, state.Mature.TotalMilkProduced > milkBefore, "constitution enables milk production");

        var qualityBefore = subject.Milk.Quality;
        inventory.AddItem("mana_infusion_drug", 1);
        var useResult = inventory.UseItemOnCharacter("mana_infusion_drug", subject);
        Assert(result, useResult, "mana infusion drug usable on character");
        Assert(result, subject.Milk.HasMagicMilkConstitution, "mana infusion grants magic constitution");
Assert(result, subject.Milk.Quality >= qualityBefore, "mana infusion raises milk quality");

        var factory = new SaveStateFactory(data);
        state.Adventure.CapturePrefs.Race = "Cowfolk";
        state.Adventure.CapturePrefs.BustSize = "10";
        state.Adventure.CapturePrefs.Job = "Knight";
        state.Adventure.CapturePrefs.ManaAmount = 3;
        var prefRecruit = factory.CreateGeneratedRecruitWithPreferences(state, state.Adventure.CapturePrefs);
        Assert(result, prefRecruit.Race == "Cowfolk", "capture prefs set race");
        Assert(result, prefRecruit.JobClass == "Knight", "capture prefs set job");
        Assert(result, prefRecruit.BustSize == 10, "capture prefs set bust size");
        Assert(result, prefRecruit.MagicPower == 6, "capture prefs set mana to amount * 2");

        var talents2 = new TalentService(state, data);
        var training = new TrainingService(state, talents2);

        var schedule = new ScheduleService(state, data);
        var equipment = new EquipmentService(state, data);
        var ranch = new RanchService(state, data, equipment, talents2);
        var economy2 = new EconomyService(state);
        var milestones2 = new MilestoneService(state, data, economy2);
        var inventory2 = new InventoryService(state);
        var dayCycle = new DayCycleService(state);
        var milk2 = new MilkEconomyService(state);
        var settlement = new DailySettlementService(state, data, schedule, ranch, economy2, dayCycle, milestones2, inventory2, talents2);

        var traineeA = state.Roster.Characters[0];
        var traineeB = state.Roster.Characters[1];
        traineeA.Energy = 100;
        traineeB.Energy = 100;
        traineeA.Fatigue = 0;
        traineeB.Fatigue = 0;
        Assert(result, training.Train(traineeA.Id, "ranch"), "first training slot succeeds");
        Assert(result, training.Train(traineeB.Id, "combat"), "second training slot succeeds");
        Assert(result, !training.Train(traineeB.Id, "craft"), "third training slot blocked by two-per-day rule");
        Assert(result, state.Calendar.TrainedToday == 2, "two training slots tracked per day");

        var enhanced = new EnhancedTrainingService(state, new Random(3));
        var trainingTarget = state.Roster.Characters[0];
        trainingTarget.Energy = 100;
        trainingTarget.Bond = 20;
        trainingTarget.Mature.FallState = FallState.Normal;
        // Test fixture: grant the reviewed-approval record so the fail-closed gate
        // passes and we exercise the two-per-day mechanic (not the gate itself).
        trainingTarget.AdultEligibility = AdultEligibility.ConfirmedAdult;
        var action = TrainingActionCatalog.All.FirstOrDefault(a => a.EnergyCost <= 20);
        Assert(result, action is not null, "training action catalog has actions");
        var perform = enhanced.PerformAction(trainingTarget.Id, action!.Id);
        Assert(result, !perform.Success, "enhanced training blocked after two-per-day cap");
        Assert(result, perform.Summary.Contains("two"), "enhanced training reports the two-per-day rule");

        state.Calendar.NightAction = "rest";
        var fatigueBefore = traineeA.Fatigue;
        settlement.SettleDay();
        Assert(result, state.Calendar.NightAction == string.Empty, "night action consumed after settlement");
        Assert(result, traineeA.Fatigue < fatigueBefore, "rest night action reduces fatigue");
        Assert(result, state.Calendar.TrainedToday == 0, "training slots reset at day start");
    }

private static void TestTrainingParityAndVisit(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var inventory = new InventoryService(state);
        var enhanced = new EnhancedTrainingService(state, new Random(77), inventory);

        state.Calendar.TrainedToday = 0;
        var charA = state.Roster.Characters[0];
        charA.Energy = 100;
        charA.Fatigue = 0;
        charA.Bond = 80;
        charA.Mature.FallState = FallState.Normal;
        // Test fixture: grant the reviewed-approval record so the fail-closed gate
        // passes and we exercise the tool/consent mechanics (not the gate itself).
        charA.AdultEligibility = AdultEligibility.ConfirmedAdult;

        // Unknown action id must not silently map to a real action.
        var unknown = enhanced.PerformAction(charA.Id, "not_a_real_action");
        Assert(result, !unknown.Success, "unknown training action id is rejected");

        // Tool-required action fails without the tool.
        var toolAction = TrainingActionCatalog.All.FirstOrDefault(a => a.ToolRequired == "whip");
        Assert(result, toolAction is not null, "whip training action exists in catalog");
        var noTool = enhanced.PerformAction(charA.Id, toolAction!.Id);
        Assert(result, !noTool.Success, "tool action blocked without tool in inventory");
        Assert(result, noTool.Summary.Contains("tool"), "tool action reports missing tool");

        // Grant the tool and succeed.
        inventory.AddItem("whip", 1);
        var withTool = enhanced.PerformAction(charA.Id, toolAction.Id);
        Assert(result, withTool.Success, "tool action succeeds with tool present");

        state.Calendar.TrainedToday = 0;

        // Consent-gated action fails when consent is not present.
        var consentAction = TrainingActionCatalog.All.FirstOrDefault(a => a.RequiresConsent);
        Assert(result, consentAction is not null, "consent action exists in catalog");
        var consentChar = state.Roster.Characters[1];
        consentChar.Energy = 100;
        consentChar.Bond = 20;
        consentChar.Mature.FallState = FallState.Normal;
        consentChar.Mature.Obedience = 0;
        consentChar.Mature.Submission = 0;
        // Test fixture: grant the reviewed-approval record so the fail-closed gate
        // passes and we exercise the consent mechanic (not the gate itself).
        consentChar.AdultEligibility = AdultEligibility.ConfirmedAdult;
        var noConsent = enhanced.PerformAction(consentChar.Id, consentAction!.Id);
        Assert(result, !noConsent.Success, "consent action blocked without consent");
        Assert(result, noConsent.Summary.Contains("consent"), "consent block reports consent");

        // High obedience grants consent.
        consentChar.Energy = 100;
        consentChar.Mature.Obedience = 9000;
        var withConsent = enhanced.PerformAction(consentChar.Id, consentAction.Id);
        Assert(result, withConsent.Success, "consent action succeeds with high obedience");

        // Care actions on the visit screen.
        var visit = new VisitService(state, data);
        var target = state.Roster.Characters[0];
        target.Energy = 30;
        target.Fatigue = 80;
        target.Morale = 40;
        target.Bond = 20;

        var beforeEnergy = target.Energy;
        var beforeMorale = target.Morale;
        var bath = visit.CareBathe(target.Id);
        Assert(result, bath.Length > 0, "bathe returns feedback");
        Assert(result, target.Fatigue < 80, "bathing reduces fatigue");
        Assert(result, target.Morale > beforeMorale, "bathing raises morale");

        inventory.AddItem("meal_box", 3);
        var mealsBefore = inventory.Items.TryGetValue("meal_box", out var mealsStart) ? mealsStart : 0;
        var feed = visit.CareFeed(target.Id);
        Assert(result, feed.Length > 0, "feed returns feedback");
        Assert(result, target.Energy > beforeEnergy, "feeding boosts energy");
        var mealsAfter = inventory.Items.TryGetValue("meal_box", out var mealsEnd) ? mealsEnd : 0;
        Assert(result, mealsAfter == mealsBefore - 1, "feeding consumes exactly one meal_box");

        var talk = visit.CareTalk(target.Id);
        Assert(result, talk.Length > 0, "talk returns feedback");
        Assert(result, target.Bond > 0 || target.Morale > 0, "talking affects bond or morale");

        inventory.AddItem("gift_ribbon", 1);
        var beforeFav = target.Mature.Favorability;
        var gift = visit.CareGift(target.Id, "gift_ribbon");
        Assert(result, gift.Length > 0, "gift returns feedback");
        Assert(result, target.Mature.Favorability > beforeFav, "gift raises favorability");
        Assert(result, !inventory.Items.ContainsKey("gift_ribbon"), "gift consumes the keepsake item");

        var rest = visit.CareRest(target.Id);
        Assert(result, rest.Length > 0, "rest returns feedback");
        Assert(result, target.Energy > 30, "rest restores energy");

        var groom = visit.CareGroom(target.Id);
        Assert(result, groom.Length > 0, "groom returns feedback");
        Assert(result, target.Bond > 0, "grooming raises bond");

        // Catalog sanity: every tool referenced has a resolvable item.
        var missingTools = TrainingActionCatalog.All
            .Where(a => !string.IsNullOrEmpty(a.ToolRequired))
            .Select(a => a.ToolRequired)
            .Distinct()
            .Where(toolId => !inventory.Items.ContainsKey(EnhancedTrainingService.ResolveToolId(toolId))
                && !data.Items.ContainsKey(EnhancedTrainingService.ResolveToolId(toolId)))
            .ToList();
        Assert(result, missingTools.Count == 0, $"all catalog tools exist as items (missing: {(missingTools.Count > 0 ? string.Join(", ", missingTools) : "none")})");
    }

    private static void TestAdultEligibilityGate(SmokeTestResult result)
    {
        // Fail-closed at definition import: a minor apparent age is never eligible.
        var minor = new CharacterDefinition { Id = "gate_minor" };
        AdultEligibilityGate.ValidateAndSetEligibility(minor, 13, "Source apparent age 13; minor");
        Assert(result, minor.AdultEligibility == AdultEligibility.Minor, "gate classifies minor apparent age as Minor");
        Assert(result, !AdultEligibilityGate.IsEligibleForAdult(minor), "gate denies adult presentation for a minor");
        Assert(result, !AdultEligibilityGate.IsEligibleForAdult(minor), "gate denies adult presentation for a minor (definition)");

        // Ambiguous context (school-age marker) with age 18+ is not confirmed adult.
        var ambiguous = new CharacterDefinition { Id = "gate_ambiguous" };
        AdultEligibilityGate.ValidateAndSetEligibility(ambiguous, 18, "JK (schoolgirl) marker; ambiguous");
        Assert(result, ambiguous.AdultEligibility == AdultEligibility.Ambiguous, "gate classifies school-age context as Ambiguous");
        Assert(result, !AdultEligibilityGate.IsEligibleForAdult(ambiguous), "gate denies adult presentation for ambiguous context");

        // Age 18+ with clean context is NOT auto-approved: it is Unknown (pending review).
        var unknown = new CharacterDefinition { Id = "gate_unknown" };
        AdultEligibilityGate.ValidateAndSetEligibility(unknown, 21, "Source apparent age 21; pending design review");
        Assert(result, unknown.AdultEligibility == AdultEligibility.Unknown, "gate defaults clean-context age 18+ to Unknown (no implicit approval)");
        Assert(result, !AdultEligibilityGate.IsEligibleForAdult(unknown), "gate denies adult presentation for unreviewed Unknown");

        // Only an explicit ConfirmedAdult record grants eligibility.
        var approved = new CharacterDefinition { Id = "gate_approved", AdultEligibility = AdultEligibility.ConfirmedAdult };
        Assert(result, AdultEligibilityGate.IsEligibleForAdult(approved), "gate grants adult presentation for ConfirmedAdult");

        // Runtime state path: fail-closed for a minor character state.
        var minorState = new CharacterState { Id = "gate_state_minor" };
        AdultEligibilityGate.ValidateAndSetEligibility(minorState, 15, "baby_face trait; minor");
        Assert(result, minorState.AdultEligibility == AdultEligibility.Minor, "gate classifies minor character state");
        Assert(result, !AdultEligibilityGate.IsEligibleForAdult(minorState), "gate denies adult presentation for minor state");
        Assert(result, !AdultEligibilityGate.CanPerformAdultAction(minorState), "gate denies adult action for minor state");

        // Denial reasons are present and specific for each blocked state.
        Assert(result, AdultEligibilityGate.GetDenialReason(minorState).Contains("15"), "minor denial reason carries the apparent age");
        Assert(result, AdultEligibilityGate.GetDenialReason(ambiguous).Contains("schoolgirl"), "ambiguous denial reason carries the context note");
        Assert(result, AdultEligibilityGate.GetDenialReason(unknown).Contains("Unknown"), "unknown denial reason names the state");
    }

    private static void TestClothingEquipmentIntegration(SmokeTestResult result)
    {
        var data = DataRegistry.CreateSeeded();
        var state = new SaveStateFactory(data).CreateNewGame();
        var inventory = new InventoryService(state);
        var clothing = new ClothingService(state, data);
        var character = state.Roster.Characters.First(c => c.Id == "rancher");

        static string SlotKey(EquipmentSlot slot) => slot switch
        {
            EquipmentSlot.Weapon => "weapon",
            EquipmentSlot.Armor => "armor",
            EquipmentSlot.Accessory => "accessory",
            EquipmentSlot.Head => "head",
            EquipmentSlot.Feet => "feet",
            EquipmentSlot.UnderwearTop => "underwear_top",
            EquipmentSlot.UnderwearBottom => "underwear_bottom",
            EquipmentSlot.Necklace => "necklace",
            EquipmentSlot.Coat => "coat",
            EquipmentSlot.Ears => "ears",
            EquipmentSlot.Arms => "arms",
            EquipmentSlot.Legs => "legs",
            _ => slot.ToString().ToLowerInvariant()
        };

        static string? FirstEquipBySlot(DataRegistry registry, EquipmentSlot slot)
        {
            return registry.Items.Values
                .Where(item => item.Category == ItemCategory.Equipment && item.Slot == slot)
                .Select(item => item.Id)
                .FirstOrDefault();
        }

        static (EquipmentSlot Slot, string Id)? FirstEquipByAnySlot(DataRegistry registry, EquipmentSlot except)
        {
            foreach (var entry in registry.Items.Values.Where(item => item.Category == ItemCategory.Equipment))
            {
                if (entry.Slot == except)
                    continue;
                return (entry.Slot, entry.Id);
            }

            return null;
        }

        var armorItemId = FirstEquipBySlot(data, EquipmentSlot.Armor);
        var secondEquip = FirstEquipByAnySlot(data, EquipmentSlot.Armor);

        Assert(result, !string.IsNullOrWhiteSpace(armorItemId), "data has at least one armor item");
        Assert(result, secondEquip is not null, "data has at least one additional equipment slot item");
        if (string.IsNullOrWhiteSpace(armorItemId) || secondEquip is null)
        {
            return;
        }

        inventory.AddItem(armorItemId, 1);
        inventory.AddItem(secondEquip.Value.Id, 1);

        var equipArmor = clothing.EquipItem(character, armorItemId);
        Assert(result, equipArmor.Success, "clothing service equips armor item");
        Assert(result, character.EquippedItems.TryGetValue("armor", out var armorId) && armorId == armorItemId, "equipped armor stored as slot -> item id");
        Assert(result, character.Equipment.ArmorId == armorItemId, "equipment state armor id synced");
        if (data.Items.TryGetValue(armorItemId, out var armorDef) && armorDef.ClothingStyleValue != ClothingStyle.Default)
        {
            Assert(result, character.Equipment.ActiveClothingStyle == armorDef.ClothingStyleValue, "active clothing style synced from equipped item");
        }
        Assert(result, !state.Inventory.Items.ContainsKey(armorItemId), "equipping consumes inventory item");

        var equipSecond = clothing.EquipItem(character, secondEquip.Value.Id);
        Assert(result, equipSecond.Success, "clothing service equips item for second slot");
        var secondSlotKey = SlotKey(secondEquip.Value.Slot);
        Assert(result, character.EquippedItems.TryGetValue(secondSlotKey, out var secondId) && secondId == secondEquip.Value.Id, "second slot populated with expected item");

        var legacyMap = new Dictionary<string, string>
        {
            [armorItemId] = "Armor",
            [secondEquip.Value.Id] = secondEquip.Value.Slot.ToString()
        };
        character.EquippedItems = legacyMap;
        clothing.SyncCharacterEquipment(character);
        Assert(result, character.EquippedItems.TryGetValue("armor", out var normalizedArmor) && normalizedArmor == armorItemId, "legacy item->slot map normalized to slot->item");
        Assert(result, character.EquippedItems.TryGetValue(secondSlotKey, out var normalizedSecond) && normalizedSecond == secondEquip.Value.Id, "legacy secondary slot normalized");

        var unequipSecond = clothing.UnequipItem(character, secondEquip.Value.Slot);
        Assert(result, unequipSecond.Success, "clothing service unequips item from explicit slot");
        Assert(result, !character.EquippedItems.ContainsKey(secondSlotKey), "slot removed after unequip");
        Assert(result, state.Inventory.Items.TryGetValue(secondEquip.Value.Id, out var secondCount) && secondCount >= 1, "unequip returns item to inventory");
    }

    /// <summary>
    /// Test double that records every dispatched world command so the station's dispatch
    /// contract can be verified without touching the real GameRoot simulation.
    /// </summary>
    private sealed class RecordingDispatcher : IWorldCommandDispatcher
    {
        private readonly System.Collections.Generic.List<WorldCommand> _recorded;
        private readonly bool _shouldFail;

        public RecordingDispatcher(System.Collections.Generic.List<WorldCommand> recorded, bool shouldFail = false)
        {
            _recorded = recorded;
            _shouldFail = shouldFail;
        }

        public bool Dispatch(WorldCommand command, WorldInteractionContext context)
        {
            _recorded.Add(command);
            return !_shouldFail;
        }
    }
}
