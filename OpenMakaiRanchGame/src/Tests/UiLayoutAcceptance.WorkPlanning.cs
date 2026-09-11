using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

public partial class UiLayoutAcceptance
{
    private async Task CheckWorkPlanningJourney(WorldGameController world)
    {
        var game = GameRoot.Instance;
        var locale = game.State.Settings.Locale;
        var panel = world.StationPanel!;
        Button ButtonNamed(string name) => Descendants(panel).OfType<Button>().Single(b => b.Name == name);
        async Task OpenKitchen()
        {
            world.CloseManagement(); world.Transition?.CompleteImmediately();
            if (world.ActiveAreaId != "ranch") world.TravelTo("ranch");
            world.Transition?.CompleteImmediately(); await Frames(6);
            var station = world.ResolveStation("kitchen")!;
            // Explicit proximity fixture; the separate doorway scenario still tests physical walking.
            world.ActivePlayer!.GlobalPosition = station.GlobalPosition + new Vector3(0, 0.3f, 0);
            world.ActivePlayer.Velocity = Vector3.Zero; await Frames(6);
            Check(world.OpenStation(station), "work planning: the nearby kitchen opens its own interface");
            await Frames(8);
        }
        if (game.HasSaveSlot(99)) throw new InvalidOperationException("Refusing an occupied work-planning fixture slot.");
        try
        {
            await OpenKitchen();
            Check(panel.StationPage == "overview" && !Descendants(panel).OfType<Button>().Any(b =>
                b.Name == "FacilityUpgrade" || b.Name.ToString().StartsWith("Assign_", StringComparison.Ordinal)),
                "work planning: initial overview does not dump purchase and roster controls together");
            var snapshot = JsonSerializer.Serialize(game.State);
            game.SetLocale("de");
            await Frames(8);
            foreach (var size in new[] { new Vector2I(640, 480), new Vector2I(480, 800) })
            {
                await Resize(size); await Frames(8);
                CheckLocalizedPanelGeometry(panel, $"kitchen overview German {size.X}x{size.Y}");
                Check(ButtonNamed("StationTab_team").Text == "Team" && ButtonNamed("StationTab_upgrade").Text == "Ausstattung",
                    $"work planning: actual German station sections appear at {size.X}x{size.Y}");
                await Capture($"kitchen-overview-german-{size.X}x{size.Y}");
            }
            await ClickStationButton(ButtonNamed("StationTab_upgrade"));
            Check(!Descendants(panel).OfType<Button>().Any(b => b.Name.ToString().StartsWith("Assign_", StringComparison.Ordinal)),
                "work planning: equipment review is separate from changing staff");
            await ClickStationButton(ButtonNamed("StationCancelUpgrade"));
            game.SetLocale(locale); await Frames(8);
            Check(JsonSerializer.Serialize(game.State) == snapshot && panel.StationPage == "overview",
                "work planning: translated review and Back without buying preserve the complete gameplay state");
            await ClickStationButton(ButtonNamed("StationTab_upgrade"));
            var quote = game.Ranch.InspectFacilityUpgrade("kitchen");
            Check(quote.CanUpgrade, "work planning: inherited fixture funds can afford the displayed kitchen upgrade without a grant");
            if (!quote.CanUpgrade) throw new InvalidOperationException("Work-planning fixture cannot afford the canonical upgrade.");
            game.SetLocale("de"); await Frames(8);
            Check(panel.StationPage == "upgrade" && panel.ContextId == "kitchen", "work planning: changing language retains the reviewed equipment page");
            await Resize(new Vector2I(640, 480)); await Frames(8);
            CheckLocalizedPanelGeometry(panel, "kitchen equipment German 640x480");
            await Capture("kitchen-equipment-german-640x480");
            var gold = game.Economy.Gold; var stamina = game.State.Player.Stamina;
            var day = game.State.Calendar.Day; var phase = game.State.Calendar.Phase; var generation = game.StateGeneration;
            var stock = JsonSerializer.Serialize(game.State.Ranch.Stockpile);
            await ClickStationButton(ButtonNamed("FacilityUpgrade"));
            Check(game.State.Ranch.Facilities["kitchen"] == quote.NextLevel && game.Economy.Gold == gold - quote.Cost
                && game.Ranch.FacilityUpkeep() == quote.RanchUpkeepAfter,
                "work planning: actual Confirm spends exactly the quoted price and produces the quoted daily facility bill");
            Check(game.State.Calendar.Day == day && game.State.Calendar.Phase == phase && game.State.Player.Stamina == stamina
                && JsonSerializer.Serialize(game.State.Ranch.Stockpile) == stock,
                "work planning: equipment purchase does not produce stock, spend stamina or settle time");
            var outcome = Descendants(panel).OfType<Label>().Single(l => l.Name == "StationOutcome");
            var scroll = Descendants(panel).OfType<ScrollContainer>().Single(c => c.Name == "StationScroll");
            Check(outcome.Text.Contains(quote.Cost.ToString(), StringComparison.Ordinal)
                && outcome.GetGlobalRect().Position.Y >= scroll.GetGlobalRect().Position.Y - 1
                && outcome.GetGlobalRect().End.Y <= scroll.GetGlobalRect().End.Y + 1,
                "work planning: purchase outcome and charged amount are visible after the old button is replaced");
            await Capture("kitchen-purchased-german-640x480");
            snapshot = JsonSerializer.Serialize(game.State);
            Check(!world.TryUpgradeAtStation("kitchen", quote, generation, day, phase).Success
                && snapshot == JsonSerializer.Serialize(game.State), "work planning: an old successful price quote cannot buy another level");
            var livingQuote = game.Ranch.InspectFacilityUpgrade("kitchen");
            var position = world.ActivePlayer!.GlobalPosition;
            world.ActivePlayer.GlobalPosition = new Vector3(0, 0.8f, 10);
            Check(!world.TryUpgradeAtStation("kitchen", livingQuote, generation, day, phase).Success
                && snapshot == JsonSerializer.Serialize(game.State), "work planning: a remote call cannot buy while an old panel is still visible");
            world.ActivePlayer.GlobalPosition = position;
            var retired = ButtonNamed("FacilityUpgrade");
            world.CloseManagement(); retired.EmitSignal(BaseButton.SignalName.Pressed);
            Check(snapshot == JsonSerializer.Serialize(game.State), "work planning: a closed purchase control cannot charge anything");
            await OpenKitchen(); await ClickStationButton(ButtonNamed("StationTab_team"));
            var job = world.ResolveStation("kitchen")!.CommandTargetId;
            var worker = game.Roster.Characters.First(c => game.Schedule.GetAssignment(c.Id) != job);
            var workerId = worker.Id;
            gold = game.Economy.Gold;
            await ClickStationButton(ButtonNamed("Assign_" + workerId));
            Check(game.Schedule.GetAssignment(workerId) == job && game.Economy.Gold == gold
                && stock == JsonSerializer.Serialize(game.State.Ranch.Stockpile), "work planning: actual Team assignment changes the job but does not pay output early");
            await ClickStationButton(ButtonNamed("Rest_" + workerId));
            Check(game.Schedule.GetAssignment(workerId) == "rest" && game.Economy.Gold == gold,
                "work planning: a current kitchen worker can be sent to rest from that same team page");
            await ClickStationButton(ButtonNamed("Assign_" + workerId));
            Check(game.SaveSlot(99) && game.LoadSlot(99) && game.Schedule.GetAssignment(workerId) == job
                && game.State.Ranch.Facilities["kitchen"] == quote.NextLevel && game.Ranch.FacilityUpkeep() == quote.RanchUpkeepAfter,
                "work planning: current-schema save/load restores the purchased level, staff and matching facility upkeep together");
            world.Transition?.CompleteImmediately(); world.CloseManagement(); await Frames(8);
            // Follow the real world phase buttons; no direct phase injection for this day completion.
            Check(game.State.Calendar.Phase == DayPhase.Morning, "work planning: the inherited resident journey left a real next Morning");
            var advance = world.Ranch!.GetNode<Button>("WorldHud/AdvanceTimeButton");
            for (var i = 0; i < 4 && game.State.Calendar.Phase != DayPhase.Night; i++)
            {
                Check(!advance.Disabled && VisibleTarget(advance), "work planning: the physical phase button is usable");
                await Click(advance); await Frames(6);
            }
            Check(game.State.Calendar.Phase == DayPhase.Night && game.State.Calendar.Day == day,
                "work planning: world time reaches Night without silently settling work");
            var house = world.ResolveStation("ranch_house")!;
            world.ActivePlayer!.GlobalPosition = house.GlobalPosition + new Vector3(0, 0.3f, 0);
            world.ActivePlayer.Velocity = Vector3.Zero; await Frames(6);
            Check(world.OpenStation(house), "work planning: the physical house handles day completion");
            await Frames(8);
            await ClickStationButton(ButtonNamed("HousePlan_rest"));
            var beforeGold = game.Economy.Gold; var countReports = game.State.Reports.Count;
            await ClickStationButton(ButtonNamed("HouseSleep"));
            var report = game.LastDailyReport;
            Check(game.State.Calendar.Day == day + 1 && game.State.Calendar.Phase == DayPhase.Morning
                && game.State.Reports.Count == countReports + 1 && report is not null
                && report.NetGold == (long)game.Economy.Gold - beforeGold,
                "work planning: actual Sleep produces one reconciled next-morning report after the equipment and staffing decisions");
            Check(report is not null && report.Expenses >= quote.RanchUpkeepAfter
                && game.Ranch.FacilityUpkeep() == quote.RanchUpkeepAfter && game.Schedule.GetAssignment(workerId) == job,
                "work planning: the upgraded recurring bill reaches real settlement, with the chosen work plan retained");
            Check(game.SaveSlot(99) && game.LoadSlot(99) && game.State.Calendar.Day == day + 1
                && game.State.Ranch.Facilities["kitchen"] == quote.NextLevel,
                "work planning: the completed next morning and purchased equipment survive another load");
        }
        finally
        {
            game.Save.Delete(99); game.SetLocale(locale);
            world.Transition?.CompleteImmediately(); world.CloseManagement();
        }
    }
}
