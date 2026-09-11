using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

public partial class UiLayoutAcceptance
{
    private async Task CheckResidentJourney(WorldGameController world)
    {
        var game = GameRoot.Instance;
        world.CloseManagement(); world.Transition?.CompleteImmediately();
        if (world.ActiveAreaId != "ranch") world.TravelTo("ranch");
        world.Transition?.CompleteImmediately(); await Frames(8);
        if (game.HasSaveSlot(99)) throw new InvalidOperationException("Refusing an occupied resident fixture slot.");
        var locale = game.State.Settings.Locale;
        // Inherits the existing isolated Day-2-derived fixture. Ordinary non-explicit actions do
        // not need adult approvals. Set bounded tiredness/skill values, not production content IDs.
        var resident = game.Roster.Characters.First(c => c.Id != "anon");
        var id = resident.Id;
        resident.Hp = 80; resident.Energy = 80; resident.Fatigue = 20; resident.Morale = 50; resident.CraftSkill = 2;
        game.State.Calendar.Phase = DayPhase.Morning; game.State.Calendar.TrainedToday = 0;
        game.State.Player.Stamina = 100;
        game.NotifyStateChanged(); await Frames(8);
        var day = game.State.Calendar.Day; var generation = game.StateGeneration;
        var assignment = game.Schedule.GetAssignment(id);
        var panel = world.StationPanel!;
        Button ButtonNamed(string name) => Descendants(panel).OfType<Button>().Single(b => b.Name == name);
        async Task OpenNearby()
        {
            world.Transition?.CompleteImmediately(); world.CloseManagement(); await Frames(6);
            var avatar = world.ResolveResidentNode(id)!;
            // Staged proximity; actual held-key doorway traversal remains covered separately.
            avatar.GlobalPosition = new Vector3(0.8f, 0, 2);
            world.ActivePlayer!.GlobalPosition = new Vector3(0, 0.8f, 2);
            world.ActivePlayer.Velocity = Vector3.Zero;
            Check(world.OpenResident(id), "resident: a nearby resident opens their own interaction surface");
            await Frames(8);
        }
        try
        {
            await OpenNearby(); await Resize(new Vector2I(640, 480)); await Frames(8);
            var avatar = world.ResolveResidentNode(id)!; var position = avatar.GlobalPosition;
            await Frames(40);
            Check(avatar.GlobalPosition.DistanceTo(position) < 0.01f && world.Ranch!.Roster!.ConversationFocusId == id,
                "resident: the addressed NPC waits during the conversation instead of walking out of range");
            Check(!Descendants(panel).OfType<Button>().Any(b => b.Name == "ResidentPractice_craft"),
                "resident: practice and gift controls are not dumped into the initial conversation view");
            var snapshot = JsonSerializer.Serialize(game.State);
            await ClickStationButton(ButtonNamed("ResidentTalk"));
            Check(JsonSerializer.Serialize(game.State) == snapshot && Descendants(panel).OfType<Label>().Any(l => l.Name == "ResidentFeedback" && l.Text.Length > 0),
                "resident: an actual free conversation shows readable feedback without changing gameplay state");
            await ClickStationButton(ButtonNamed("ResidentPracticePage"));
            CheckLocalizedPanelGeometry(panel, "resident practice 640x480");
            var skill = resident.CraftSkill; var stamina = game.State.Player.Stamina; var energy = resident.Energy;
            await ClickStationButton(ButtonNamed("ResidentPractice_craft"));
            Check(resident.CraftSkill == skill + 1 && resident.Energy == energy - 10
                && game.State.Player.Stamina == stamina - game.Residents.Cost(ResidentAction.CraftPractice)
                && game.State.Calendar.TrainedToday == 1 && game.Schedule.GetAssignment(id) == assignment,
                "resident: the real practice button changes the shared skill/energy/slot once without early work output");
            Check(ButtonNamed("ResidentPractice_magic").Disabled,
                "resident: another focus cannot buy a second lesson for the same resident today");
            game.SetLocale("de"); await Frames(8);
            CheckLocalizedPanelGeometry(panel, "resident practice German 640x480");
            Check(ButtonNamed("ResidentPractice_craft").Text.Contains("Handwerk", StringComparison.Ordinal)
                && panel.ContextId == id && game.Schedule.GetAssignment(id) == assignment,
                "resident: language refresh keeps the practice page, target and work assignment");
            await Capture("resident-practice-german-640x480");
            await Resize(new Vector2I(480, 800)); await Frames(8);
            CheckLocalizedPanelGeometry(panel, "resident practice German 480x800");
            await Capture("resident-practice-german-480x800");
            game.SetLocale("en"); await Frames(8);
            await ClickStationButton(ButtonNamed("ResidentSectionBack"));
            await ClickStationButton(ButtonNamed("ResidentCarePage"));
            var nestedAccepted = false;
            void AttemptReentry()
            {
                nestedAccepted |= game.TryResidentAction(id, ResidentAction.Recovery, generation, day, DayPhase.Morning).Success;
            }
            game.StateChanged += AttemptReentry;
            stamina = game.State.Player.Stamina;
            try { await ClickStationButton(ButtonNamed("ResidentEncourage")); }
            finally { game.StateChanged -= AttemptReentry; }
            Check(!nestedAccepted && game.State.Player.Stamina == stamina - game.Residents.Cost(ResidentAction.Encourage),
                "resident: a synchronous state observer cannot purchase a different interaction reentrantly");
            var mealCount = game.State.Inventory.Items.TryGetValue("meal_box", out var n) ? n : 0;
            Check(game.TryBuyItem("meal_box", 1, game.StateGeneration), "resident: meal fixture purchases one item through the canonical shop transaction");
            await Frames(8); await ClickStationButton(ButtonNamed("ResidentFeed"));
            Check((game.State.Inventory.Items.TryGetValue("meal_box", out n) ? n : 0) == mealCount && ButtonNamed("ResidentFeed").Disabled,
                "resident: the actual meal consumes exactly one box and disables its daily repeat");
            Check(game.TryBuyItem("gift_journal", 1, game.StateGeneration), "resident: gift fixture buys an ordinary journal with actual funds");
            await Frames(8); await ClickStationButton(ButtonNamed("ResidentGifts"));
            var gifts = game.State.Inventory.Items["gift_journal"];
            await ClickStationButton(ButtonNamed("ResidentGift_gift_journal"));
            Check((game.State.Inventory.Items.TryGetValue("gift_journal", out n) ? n : 0) == gifts - 1,
                "resident: the actual selected gift consumes its own item, not a quest keepsake");
            await Capture("resident-gifts-480x800");
            var retired = ButtonNamed("ResidentSectionBack");
            world.CloseManagement(); retired.EmitSignal(BaseButton.SignalName.Pressed);
            Check(!panel.Visible && world.Ranch!.Roster!.ConversationFocusId == "" && world.Ranch.InputGate.WorldInputEnabled,
                "resident: a hidden callback cannot reopen the panel; closing releases NPC and player input");
            var stateBeforeRemote = JsonSerializer.Serialize(game.State);
            world.ActivePlayer!.GlobalPosition = new Vector3(0, 0.8f, 10);
            Check(!world.OpenResident(id) && !world.TryResidentAction(id, ResidentAction.Recovery, generation, day, DayPhase.Morning).Success
                && JsonSerializer.Serialize(game.State) == stateBeforeRemote, "resident: remote/closed interactions cannot spend resources or change the resident");
            Check(game.SaveSlot(99) && game.LoadSlot(99), "resident: completed care and practice save/load through the actual root");
            world.Transition?.CompleteImmediately(); await Frames(8);
            Check(!game.Residents.Inspect(id, ResidentAction.CraftPractice).Available
                && !game.TryResidentAction(id, ResidentAction.Recovery, generation, day, DayPhase.Morning).Success,
                "resident: daily receipts survive load and the old session cannot execute a command");
            var reports = game.State.Reports.Count;
            while (game.State.Calendar.Phase != DayPhase.Night)
                if (!game.AdvanceTime()) throw new InvalidOperationException("Resident journey could not reach the ordinary Night.");
            world.Transition?.CompleteImmediately(); await Frames(8);
            var house = world.ResolveStation("ranch_house")!;
            world.ActivePlayer!.GlobalPosition = house.GlobalPosition + new Vector3(0, 0.3f, 0);
            world.ActivePlayer.Velocity = Vector3.Zero; await Frames(6);
            Check(world.OpenStation(house), "resident: the physical house remains available after interactions and load");
            await Frames(8);
            if (game.State.Calendar.NightAction != "rest") await ClickStationButton(ButtonNamed("HousePlan_rest"));
            await ClickStationButton(ButtonNamed("HouseSleep"));
            world.Transition?.CompleteImmediately(); world.CloseManagement(); await Frames(8);
            Check(game.State.Calendar.Day == day + 1 && game.State.Reports.Count == reports + 1
                && game.State.Calendar.TrainedToday == 0 && game.State.Player.Stamina == game.PlayerStamina.CurrentCapacity
                && game.Residents.Inspect(id, ResidentAction.CraftPractice).Available,
                "resident: actual Sleep settles once, refills the daily budget and unlocks tomorrow's lesson");
            Check(game.SaveSlot(99) && game.LoadSlot(99) && game.State.Calendar.Day == day + 1
                && game.Residents.Inspect(id, ResidentAction.CraftPractice).Available,
                "resident: next-morning availability persists without a second reset or phantom interaction");
        }
        finally
        {
            game.Save.Delete(99); game.SetLocale(locale);
            world.Transition?.CompleteImmediately(); world.CloseManagement();
        }
    }
}
