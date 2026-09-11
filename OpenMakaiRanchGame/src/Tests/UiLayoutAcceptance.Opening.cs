using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Ui;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

public partial class UiLayoutAcceptance
{
    private async Task CheckOpeningJourney(WorldGameController world)
    {
        var game = GameRoot.Instance;
        var shell = world.Shell!;
        var probe = new TypewriterLabel { Text = "A quiet morning at the ranch." };
        AddChild(probe);
        await Frames(2);
        probe.Finish();
        Check(probe.Text == "A quiet morning at the ranch." && probe.IsComplete,
            "opening: finishing a label initialized through Text retains its complete line instead of erasing it");
        probe.Begin(string.Empty);
        Check(probe.IsComplete, "opening: an empty line is already complete and never consumes an extra Continue");
        probe.QueueFree();

        var start = Descendants(shell).OfType<Button>().Single(button => button.Name == "StartButton");
        start.GrabFocus();
        await Frames(5);
        await Click(start);
        await Frames(6);
        Check(shell.CurrentScreen == "prologue", "opening: the actual character-creation Start enters the prologue");
        var lines = Descendants(shell).OfType<TypewriterLabel>().ToArray();
        Check(lines.Length > 0, "opening: the prologue contains authored narrative lines");
        var next = Descendants(shell).OfType<Button>().Single(button => button.Text == "Continue");
        next.GrabFocus();
        await Frames(4);
        await Click(next);
        Check(Descendants(shell).OfType<TypewriterLabel>().Any(label => !string.IsNullOrWhiteSpace(label.Text)),
            "opening: the first Continue never turns the narrative into an empty page");
        await Capture("opening-prologue");
        var skip = Descendants(shell).OfType<Button>().Single(button => button.Text == "Skip");
        skip.GrabFocus();
        await Frames(4);
        await Click(skip);
        world.Transition?.CompleteImmediately();
        await Frames(10);
        Check(world.ActiveAreaId == "intro" && game.State.Story.FirstDayStage == FirstDayFlowController.StageWakeUp,
            "opening: skipping the text prologue still starts the playable morning in the bedroom");
        var wake = Descendants(world.FirstDayFlow!).OfType<Button>().Single(button => button.Text == "Wake up");
        await Click(wake);
        world.Transition?.CompleteImmediately();
        await Frames(8);
        var position = world.IntroHouse!.Player!.GlobalPosition;
        var generation = game.StateGeneration;
        var day = game.State.Calendar.Day;
        var gold = game.Economy.Gold;
        Check(game.State.Story.FirstDayStage == FirstDayFlowController.StageLeaveBedroom,
            "opening: the actual Wake up choice reaches the bedroom exit objective");
        Check(world.OpenManagementScreen("options"), "opening: utility options remain available during the guided morning");
        await Frames(8);
        Check(world.CloseManagement(), "opening: utility options can close during the guided morning");
        await Frames(8);
        Check(world.ActiveAreaId == "intro" && world.IntroHouse.Player.GlobalPosition.DistanceTo(position) < 0.1f,
            "opening: closing utility options returns to the same bedroom and position, not the stored ordinary ranch area");
        Check(game.StateGeneration == generation && game.State.Calendar.Day == day && game.Economy.Gold == gold
            && game.State.Story.FirstDayStage == FirstDayFlowController.StageLeaveBedroom,
            "opening: utility navigation preserves the same session, objective, day and wallet");
        await Capture("opening-bedroom-return");
    }
}
