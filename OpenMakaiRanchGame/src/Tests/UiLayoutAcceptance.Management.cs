using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Ui;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

public partial class UiLayoutAcceptance
{
    private async Task CheckManagementJourney(WorldGameController world)
    {
        var game = GameRoot.Instance;
        var shell = world.Shell!;
        world.Transition?.CompleteImmediately();
        var day = game.State.Calendar.Day;
        var phase = game.State.Calendar.Phase;
        var stamina = game.State.Player.Stamina;
        Check(world.OpenManagementScreen("shop"), "management: the store is reachable after returning from combat");
        await Resize(new Vector2I(640, 480));
        await Frames(8);
        CheckAdventureCardGeometry(shell, "store 640x480");
        await Capture("store-640x480");
        var item = game.Data.ShopItems().Single(definition => definition.Id == "meal_box");
        var quantity = game.State.Inventory.Items.GetValueOrDefault(item.Id);
        var gold = game.Economy.Gold;
        Check(gold >= item.Price * 2, "management: previously earned funds afford two ordinary meals without injected gold");
        if (gold < item.Price * 2) throw new InvalidOperationException("The resource-backed store fixture cannot afford two meals.");
        await ClickAdventureButton(ShopBuyButton(shell, item.DisplayName), "buy one meal from its own store row");
        Check(game.State.Inventory.Items.GetValueOrDefault(item.Id) == quantity + 1 && game.Economy.Gold == gold - item.Price,
            "management: one actual Buy click charges the listed price once and adds exactly one meal");

        Check(world.OpenManagementScreen("schedule"), "management: the working schedule opens after shopping");
        await Frames(8);
        CheckAdventureCardGeometry(shell, "schedule 640x480");
        var worker = game.Roster.Characters.First();
        var workerName = game.Roster.DefinitionFor(worker).DisplayName;
        var currentJob = game.Schedule.GetAssignment(worker.Id);
        var nextJob = game.Schedule.AssignableJobs.First(job => job.Id != currentJob);
        var workerCard = Descendants(shell.GetNode<Control>(shell.ContentPath)).OfType<PanelContainer>()
            .Single(card => Descendants(card).OfType<Label>().Any(label => label.Text.StartsWith(workerName + " — ", StringComparison.Ordinal)));
        var assignment = Descendants(workerCard).OfType<Button>().Single(button => button.Text == nextJob.DisplayName);
        var beforePlanningGold = game.Economy.Gold;
        await ClickAdventureButton(assignment, "assign a different available job to the selected worker");
        Check(game.Schedule.GetAssignment(worker.Id) == nextJob.Id && game.Economy.Gold == beforePlanningGold,
            "management: a real job button changes the canonical schedule without paying work output early");
        await Capture("schedule-640x480");
        Check(world.OpenManagementScreen("milestones"), "management: milestone progress is reachable");
        await Frames(8);
        CheckAdventureCardGeometry(shell, "milestones 640x480");
        await Capture("milestones-640x480");
        Check(game.State.Calendar.Day == day && game.State.Calendar.Phase == phase && game.State.Player.Stamina == stamina,
            "management: shopping, planning and reading milestones do not consume adventure stamina or advance the day");

        // ValidateIsolation already confined this process to a fresh .artifacts profile.
        // Slot 3 is needed to exercise the real authored Save/Load buttons (which expose 1-3).
        // Refuse an occupied slot even in that disposable profile; clean up only our own save.
        const int slot = 3;
        if (game.HasSaveSlot(slot)) throw new InvalidOperationException("Refusing to overwrite an existing UI fixture slot.");
        var wroteSlot = false;
        try
        {
            Check(world.OpenManagementScreen("saveload"), "management: the real save/load menu opens");
            await Frames(8);
            var savedGold = game.Economy.Gold;
            var savedQuantity = game.State.Inventory.Items.GetValueOrDefault(item.Id);
            await ClickAdventureButton(SlotButton(shell, slot, "Save"), "save through the visible Slot 3 button");
            wroteSlot = game.HasSaveSlot(slot);
            Check(wroteSlot, "management: the actual Save hit target writes its selected slot");
            if (!wroteSlot) throw new InvalidOperationException("The actual Save click did not create Slot 3.");
            await Capture("save-load-640x480");
            Check(world.OpenManagementScreen("shop"), "management: the saved session remains playable");
            await Frames(8);
            await ClickAdventureButton(ShopBuyButton(shell, item.DisplayName), "make one unsaved purchase");
            Check(game.Economy.Gold == savedGold - item.Price
                && game.State.Inventory.Items.GetValueOrDefault(item.Id) == savedQuantity + 1,
                "management: an unsaved purchase creates a real state difference for the load check");
            Check(world.OpenManagementScreen("saveload"), "management: return to the save/load menu");
            await Frames(8);
            var generation = game.StateGeneration;
            await ClickAdventureButton(SlotButton(shell, slot, "Load"), "load through the visible Slot 3 button");
            Check(game.StateGeneration != generation && game.Economy.Gold == savedGold
                && game.State.Inventory.Items.GetValueOrDefault(item.Id) == savedQuantity
                && game.Schedule.GetAssignment(worker.Id) == nextJob.Id,
                "management: the actual Load click rebinds the session and restores wallet, inventory and chosen job together");
            world.Transition?.CompleteImmediately();
            await Frames(8);
            Check(!game.CombatWorldTimeLocked && game.State.Calendar.Day == day
                && game.State.Calendar.Phase == phase && game.State.Player.Stamina == stamina,
                "management: a UI save/load roundtrip retains day/resources without a phantom combat lock");
        }
        finally { if (wroteSlot) game.Save.Delete(slot); }
    }

    private static Button ShopBuyButton(UiShellController shell, string displayName)
    {
        var row = Descendants(shell.GetNode<Control>(shell.ContentPath)).OfType<HFlowContainer>().Single(flow =>
            flow.GetChildren().OfType<Label>().Any(label => label.Text.StartsWith(displayName + " - ", StringComparison.Ordinal)));
        return row.GetChildren().OfType<Button>().Single(button => button.Text == "Buy");
    }

    private static Button SlotButton(UiShellController shell, int slot, string text)
    {
        var card = Descendants(shell.GetNode<Control>(shell.ContentPath)).OfType<PanelContainer>().Single(panel =>
            Descendants(panel).OfType<Label>().Any(label => label.Text == $"Slot {slot}"));
        return Descendants(card).OfType<Button>().Single(button => button.Text == text);
    }
}
