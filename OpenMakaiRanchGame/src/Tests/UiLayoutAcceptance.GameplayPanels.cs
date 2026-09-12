using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Ui;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

/// <summary>Connected panel journeys on the existing isolated fixture; staged proximity is not a navigation test.</summary>
public partial class UiLayoutAcceptance
{
    private async Task CheckGameplayPanels(WorldGameController world)
    {
        var game = GameRoot.Instance;
        var panel = world.StationPanel!;
        var shell = world.Shell!;
        var locale = game.State.Settings.Locale;
        if (game.HasSaveSlot(99)) throw new InvalidOperationException("Panel journey refuses an occupied isolated slot 99.");
        var wrote = false;
        Button Local(string name) => Descendants(panel).OfType<Button>().Single(b => b.Name == name);
        Button Service(string name) => Descendants(shell).OfType<Button>().Single(b => b.Name == name);
        string Snapshot()
        {
            var storage = new FlagStorage(); game.Flags.SyncToStorage(storage);
            return JsonSerializer.Serialize(new { game.State, Flags = storage });
        }
        async Task OpenStation(string id)
        {
            world.Transition?.CompleteImmediately(); world.CloseManagement(); await Frames(3);
            var station = world.ResolveStation(id) ?? throw new InvalidOperationException("Missing station: " + id);
            world.ActivePlayer!.GlobalPosition = station.GlobalPosition + new Vector3(0, 0.3f, 0);
            world.ActivePlayer.Velocity = Vector3.Zero; await Frames(3);
            Check(world.OpenStation(station), "panels: opens physical " + id + " in range");
            await Frames(4);
        }
        void CheckServiceLayout(string label)
        {
            var content = shell.GetNode<Control>(shell.ContentPath);
            var scroll = shell.GetNode<ScrollContainer>(shell.ScrollPath);
            Check(!shell.GetNode<Control>(shell.NavigationPath).IsVisibleInTree()
                && !shell.GetNode<Button>(shell.EndDayButtonPath).IsVisibleInTree(),
                "panels: " + label + " stays dedicated, without global management or a time command");
            Check(content.Size.X <= scroll.Size.X + 1 && scroll.Size.Y > 80,
                "panels: " + label + " fits the viewport width and retains a usable scroll area");
            foreach (var card in content.GetChildren().OfType<PanelContainer>())
                Check(card.GetChildCount() == 1 && card.GetChild(0) is VBoxContainer,
                    "panels: " + label + " card " + card.Name + " has one vertical layout owner");
        }
        try
        {
            world.Transition?.CompleteImmediately(); world.CloseManagement();
            if (world.ActiveAreaId != "ranch") world.TravelTo("ranch");
            world.Transition?.CompleteImmediately(); await Frames(6);
            game.SetLocale("de"); await Resize(new Vector2I(640, 480)); await Frames(6);
            var resident = game.Roster.Characters.First(c => c.Id != "anon");
            var avatar = world.ResolveResidentNode(resident.Id)!;
            avatar.GlobalPosition = new Vector3(0.8f, 0, 2);
            world.ActivePlayer!.GlobalPosition = new Vector3(0, 0.8f, 2);
            world.ActivePlayer.Velocity = Vector3.Zero;
            Check(world.OpenResident(resident.Id), "panels: a nearby resident opens without a global roster screen");
            await Frames(6);
            var before = Snapshot(); var generation = game.StateGeneration;
            await ClickStationButton(Local("ResidentDevelopmentPage"));
            Check(panel.ResidentPage == "development" && Local("StationBack").IsVisibleInTree(),
                "panels: Development opens a local page with a permanent Back action");
            CheckLocalizedPanelGeometry(panel, "development German 640x480");
            await Capture("panels-development-german-640x480");
            await ClickStationButton(Local("DevelopmentHistory"));
            Check(panel.ResidentPage == "development_history", "panels: the development journal opens from the resident");
            await Stroke(Key.Escape); await Frames(4);
            Check(panel.ResidentPage == "development" && world.IsStationPanelOpen,
                "panels: Escape from history returns one level, not out of the conversation");
            await ClickStationButton(Local("DevelopmentResources"));
            Check(Descendants(panel).OfType<Label>().Any(l => l.Text.Contains("EP", StringComparison.Ordinal))
                && Descendants(panel).OfType<Label>().Any(l => l.Text.Contains("MP", StringComparison.Ordinal)),
                "panels: condition shows spirit and mana as separate resources");
            await Resize(new Vector2I(480, 800)); await Frames(5);
            CheckLocalizedPanelGeometry(panel, "condition German 480x800");
            await Capture("panels-condition-german-480x800");
            Check(Snapshot() == before && game.StateGeneration == generation,
                "panels: reading development, history and protection grants no progress and changes no resources");
            await Stroke(Key.Escape); await Frames(3);
            await Stroke(Key.Escape); await Frames(3);
            Check(panel.ResidentPage == "overview" && world.IsStationPanelOpen,
                "panels: successive Back actions return to the same resident's overview");
            await Stroke(Key.Escape); await Frames(4);
            Check(!world.IsManagementVisible && world.Ranch!.InputGate.WorldInputEnabled,
                "panels: Escape from the resident root restores world control");

            await Resize(new Vector2I(640, 480));
            await OpenStation("kitchen"); before = Snapshot();
            var lunch = game.InspectNextLunch();
            await ClickStationButton(Local("StationProvisions"));
            Check(panel.StationPage == "provisions" && Descendants(panel).OfType<Label>().Any(l =>
                l.Text == OpenMakaiRanch.Locale.LocaleCatalog.T("panel.provisions.split", "", lunch.PantryMeals, lunch.MealBoxes, lunch.MissingMeals)),
                "panels: kitchen food view displays the canonical pantry/box/shortfall split");
            CheckLocalizedPanelGeometry(panel, "provisions German 640x480");
            await Capture("panels-provisions-german-640x480");
            Check(before == Snapshot(), "panels: inspecting meal supply does not cook, buy, allocate jobs or settle the day");
            var position = world.ActivePlayer!.GlobalPosition;
            game.SetLocale("en"); await Frames(5);
            Check(panel.StationPage == "provisions" && game.StateGeneration == generation
                && world.ActivePlayer.GlobalPosition.DistanceTo(position) < 0.1f,
                "panels: changing language keeps the local food page, session and position");
            game.SetLocale("de"); await Frames(4);
            await Stroke(Key.Escape); await Frames(4);
            Check(panel.StationPage == "overview" && world.IsStationPanelOpen,
                "panels: the provisions page returns to its own kitchen overview");
            world.CloseManagement(); world.OpenWorldGuide(); await Frames(4); before = Snapshot();
            await ClickStationButton(Local("OpenAchievements"));
            Check(panel.ContextId == "achievements" && Descendants(panel).OfType<PanelContainer>()
                .Count(c => c.Name.ToString().StartsWith("Achievement_", StringComparison.Ordinal)) == game.GetRanchAchievements().Count,
                "panels: Places exposes all five canonical optional records without awarding them");
            CheckLocalizedPanelGeometry(panel, "achievements German 640x480");
            await Capture("panels-achievements-german-640x480");
            Check(before == Snapshot(), "panels: displaying completion progress is read-only");
            await Stroke(Key.Escape); await Frames(3);
            Check(panel.ContextId == "places", "panels: achievements return to Places, not an unrelated station");
            world.CloseManagement();

            Check(world.TravelTo("town"), "panels: travel to the town for the real store entry");
            world.Transition?.CompleteImmediately(); await Frames(6);
            var shop = world.Town!.Services.First(service => service.ScreenId == "shop");
            world.ActivePlayer!.GlobalPosition = shop.GlobalPosition + new Vector3(0, 0.3f, 0);
            world.ActivePlayer.Velocity = Vector3.Zero; await Frames(6);
            Check(world.Town.TryInteract() && shell.CurrentScreen == "shop" && shell.IsDedicatedService,
                "panels: interacting with the actual town store opens its dedicated shop");
            await Frames(6); CheckServiceLayout("store German 640x480");
            var offer = game.InspectPurchase("meal_box", 1)!;
            Check(offer.CanBuy, "panels: the inherited fixture can afford a real meal-box purchase");
            var buy = Service("Purchase_meal_box");
            await ClickStationButton(buy);
            Check(game.Economy.Gold == offer.Gold - offer.TotalPrice && game.Inventory.Items.GetValueOrDefault("meal_box") == offer.Owned + 1,
                "panels: the visible Buy action pays the displayed price and adds one item");
            before = Snapshot();
            var armedShop = Service("Purchase_meal_box");
            shell.ShowScreen("shop"); // Retire this enabled node without advancing a frame/freeing it yet.
            armedShop.EmitSignal(BaseButton.SignalName.Pressed);
            Check(before == Snapshot(), "panels: a retired enabled shop button cannot purchase after a view rebuild");
            await Frames(4);
            Service("Purchase_meal_box").GrabFocus(); await Frames(4);
            await Capture("panels-store-german-640x480");
            await ClickStationButton(Service("ShopInventory"));
            Check(shell.CurrentScreen == "inventory" && Descendants(shell).Any(n => n.Name == "StorageBag")
                && Descendants(shell).Any(n => n.Name == "StorageStock"),
                "panels: bag and ranch stock remain explicitly separate in the local store child page");
            CheckServiceLayout("store inventory German 640x480");
            await Stroke(Key.Escape); await Frames(5);
            Check(shell.CurrentScreen == "shop" && world.IsManagementVisible,
                "panels: Escape from the shop inventory returns to the same store");
            position = world.ActivePlayer.GlobalPosition;
            world.ActivePlayer.GlobalPosition += new Vector3(3, 0, 0); before = Snapshot();
            Service("Purchase_meal_box").EmitSignal(BaseButton.SignalName.Pressed);
            Check(before == Snapshot(), "panels: a still-open shop cannot purchase after its player leaves the opening position");
            world.ActivePlayer.GlobalPosition = position;
            world.CloseManagement();
            Check(world.ActiveAreaId == "town" && world.Town.InputGate.WorldInputEnabled,
                "panels: closing the store restores town control without travel");
            Check(world.TravelTo("ranch"), "panels: return to the ranch for workshop research");
            world.Transition?.CompleteImmediately(); await Frames(6);

            // Only these research prerequisites are staged; currency/skill rewards are still real service effects.
            game.State.Ranch.Facilities["workshop"] = Math.Max(1, game.Ranch.Facilities.GetValueOrDefault("workshop"));
            var skill = game.Data.Skills.Values.First(s => !game.State.Research.UnlockedSkillIds.Contains(s.Id)
                && s.CostAmount > 0 && !string.IsNullOrWhiteSpace(s.CostResourceId));
            game.State.Ranch.Stockpile[skill.CostResourceId] = Math.Max(skill.CostAmount, game.Ranch.Stockpile.GetValueOrDefault(skill.CostResourceId));
            game.NotifyStateChanged(); await OpenStation("workshop");
            await ClickStationButton(Local("Service_research"));
            Check(shell.CurrentScreen == "research" && shell.IsDedicatedService,
                "panels: the physical workshop opens the stock-funded research surface");
            CheckServiceLayout("research German 640x480");
            var research = game.InspectResearch(skill.Id)!;
            var unlock = Service("Unlock_" + skill.Id);
            await ClickStationButton(unlock);
            Check(game.State.Research.UnlockedSkillIds.Contains(skill.Id)
                && game.Ranch.Stockpile[skill.CostResourceId] == research.Available - research.Cost,
                "panels: actual research spends its listed ranch resource once");
            before = Snapshot();
            Check(!game.TryResearchOffer(research, generation, game.State.Calendar.Day, game.State.Calendar.Phase).Success
                && before == Snapshot(), "panels: a completed research quote cannot repeat unlocking or payment");
            await ClickStationButton(Service("ResearchCompleted"));
            Check(Descendants(shell).Any(n => n.Name == "ResearchItem_" + skill.Id),
                "panels: completed research moves into the unlocked section");
            await Capture("panels-research-german-640x480");
            await ClickStationButton(Service("ResearchAvailable"));
            var oldSession = Service("ResearchCompleted");
            wrote = game.SaveSlot(99);
            // Save can redraw synchronously; capture the live navigation node after that notification.
            oldSession = Service("ResearchCompleted");
            var loaded = game.LoadSlot(99);
            before = Snapshot(); var loadedScreen = shell.CurrentScreen;
            oldSession.EmitSignal(BaseButton.SignalName.Pressed);
            Check(wrote && loaded && game.State.Research.UnlockedSkillIds.Contains(skill.Id),
                "panels: the unlocked research survives a real current-schema save/load");
            Check(before == Snapshot() && shell.CurrentScreen == loadedScreen,
                "panels: old-session service navigation cannot act on the loaded game");
            world.Transition?.CompleteImmediately(); world.CloseManagement(); await Frames(6);

            // Explicit synthetic main-completion fixture. No claim of an organically earned ending.
            game.State.VictoryDay = null;
            game.State.Adventure.DiscoveredMissionIds.Clear(); game.State.Adventure.DiscoveredMissionIds.AddRange(game.Data.Missions.Keys);
            foreach (var c in game.State.Roster.Characters) c.Bond = Math.Max(40, c.Bond);
            foreach (var id in game.Data.Facilities.Keys) game.State.Ranch.Facilities[id] = Math.Max(5, game.Ranch.Facilities.GetValueOrDefault(id));
            foreach (var id in game.Data.Skills.Keys)
                if (!game.State.Research.UnlockedSkillIds.Contains(id)) game.State.Research.UnlockedSkillIds.Add(id);
            game.State.Calendar.Phase = DayPhase.Night; game.State.Calendar.NightAction = "rest";
            game.NotifyStateChanged(); await OpenStation("ranch_house");
            var beforeDay = game.State.Calendar.Day; generation = game.StateGeneration;
            position = world.ActivePlayer!.GlobalPosition;
            await ClickStationButton(Local("HouseSleep")); await Frames(6);
            Check(game.State.VictoryDay.HasValue && game.State.Calendar.Day == beforeDay + 1
                && shell.CurrentScreen == "victory" && world.FlowLocksUi,
                "panels: actual house Sleep records victory and is not overwritten by the subsequent report request");
            Check(!Service("ContinueRanching").Disabled, "panels: a genuine completion has a usable continuation action");
            Service("ContinueRanching").GrabFocus(); await Frames(4);
            await Capture("panels-completion-german-640x480");
            before = Snapshot();
            await ClickStationButton(Service("CompletionReport"));
            Check(shell.CurrentScreen == "report" && !world.FlowLocksUi && game.StateGeneration == generation,
                "panels: completion can open its daily report without starting a new game");
            Check(game.State.Reports.Count > 1, "panels: report-history fixture contains more than one actually settled day");
            var latestReportDay = game.LastDailyReport!.Day;
            await ClickStationButton(Service("ReportOlder"));
            Check(!Service("ReportNewer").Disabled && Snapshot() == before,
                "panels: browsing an older saved day changes no gameplay or settlement state");
            await ClickStationButton(Service("ReportNewer"));
            Check(Service("ReportNewer").Disabled && Descendants(shell).OfType<Label>().Any(l =>
                l.Text == OpenMakaiRanch.Locale.LocaleCatalog.T("panel.report.day", "", latestReportDay)),
                "panels: report history returns to the latest actual day");
            await ClickStationButton(Service("ReportDetails"));
            Check(Descendants(shell).Any(n => n.Name == "ReportLines"), "panels: the report expands recorded details on demand");
            CheckServiceLayout("daily report German 640x480");
            Service("ReportReturn").GrabFocus(); await Frames(4);
            await Capture("panels-report-german-640x480");
            await ClickStationButton(Service("ReportReturn"));
            Check(!world.IsManagementVisible && world.ActiveAreaId == "ranch"
                && new Vector2(world.ActivePlayer!.GlobalPosition.X, world.ActivePlayer.GlobalPosition.Z)
                    .DistanceTo(new Vector2(position.X, position.Z)) < 0.1f && Snapshot() == before,
                "panels: reading/closing the completion report changes no state and returns to the same ranch position");
            Check(world.TravelTo("town"), "panels: the completed ranch remains playable and can still visit town");
            world.Transition?.CompleteImmediately(); await Frames(6);
            position = world.ActivePlayer!.GlobalPosition; before = Snapshot();
            world.PresentGameCompletion(); await Frames(6);
            await ClickStationButton(Service("ContinueRanching"));
            Check(!world.IsManagementVisible && !world.FlowLocksUi && world.ActiveAreaId == "town"
                && new Vector2(world.ActivePlayer.GlobalPosition.X, world.ActivePlayer.GlobalPosition.Z)
                    .DistanceTo(new Vector2(position.X, position.Z)) < 0.1f && world.Town!.InputGate.WorldInputEnabled
                && Snapshot() == before && game.StateGeneration == generation && !game.State.NgPlusActive,
                "panels: Continue Ranching from town preserves area, position, session, resources and victory instead of forcing a new run or ranch travel");
        }
        finally
        {
            world.Transition?.CompleteImmediately(); world.CloseManagement();
            if (wrote) game.Save.Delete(99);
            game.SetLocale(locale);
        }
    }
}
