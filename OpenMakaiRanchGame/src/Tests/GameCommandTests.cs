using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Ui;

namespace OpenMakaiRanch.Tests;

public static class GameCommandTests
{
    public static void Run(SmokeTestResult result)
    {
        TestUiNotifications(result);
        TestStaleUiAfterStateReplacement(result);
        TestCommandBoundaries(result);
    }

    private static void TestUiNotifications(SmokeTestResult result)
    {
        var root = GameRoot.Instance;
        root.NewGame();
        var scene = GD.Load<PackedScene>("res://scenes/Game.tscn").Instantiate();
        var notifications = 0;
        var secondObserverNotifications = 0;
        void OnChanged() => notifications++;
        void OnSecondObserverChanged() => secondObserverNotifications++;
        try
        {
            root.GetTree().Root.AddChild(scene);
            var shell = scene.GetNode<UiShellController>("UiShell");
            var character = root.Roster.Characters.First();
            var job = root.Schedule.AssignableJobs.First(value => value.Id != root.Schedule.GetAssignment(character.Id));
            var gold = root.State.Economy.Gold;
            var day = root.State.Calendar.Day;
            root.StateChanged += OnChanged;
            root.StateChanged += OnSecondObserverChanged;
            shell.ShowScreen("schedule");
            Buttons(shell).First(button => button.Text == job.DisplayName).EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, root.Schedule.GetAssignment(character.Id) == job.Id, "command UI job assignment reaches simulation");
            Check(result, notifications == 1 && secondObserverNotifications == 1, "command UI job notifies both observers exactly once");
            Check(result, root.State.Economy.Gold == gold && root.State.Calendar.Day == day,
                "command UI assignment neither pays work nor advances time");

            notifications = secondObserverNotifications = 0;
            character.Bond = 0;
            character.Morale = 50;
            character.Fatigue = 0;
            shell.ShowScreen("bond");
            Buttons(shell).First(button => button.Text.StartsWith("Mentorship")).EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, character.Bond == 5 && character.Morale == 54 && character.Fatigue == 4,
                "command UI mentorship keeps existing service effects");
            Check(result, notifications == 1 && secondObserverNotifications == 1,
                "command UI mentorship notifies both observers exactly once");

            notifications = secondObserverNotifications = 0;
            character.Bond = 100;
            var eventId = root.Bond.AvailableEvents(character.Id).First().Id;
            shell.ShowScreen("bond");
            var completeButton = Buttons(shell).First(button => button.Text == "Complete Event");
            completeButton.EmitSignal(BaseButton.SignalName.Pressed);
            // First click finishes typewriting; only the next click may complete the event.
            if (!root.State.Bond.CompletedEventIds.Contains(eventId))
                completeButton.EmitSignal(BaseButton.SignalName.Pressed);
            Check(result, root.State.Bond.CompletedEventIds.Contains(eventId), "command UI completes existing bond event");
            Check(result, notifications == 1 && secondObserverNotifications == 1,
                "command UI bond event notifies both observers exactly once");
        }
        finally
        {
            root.StateChanged -= OnChanged;
            root.StateChanged -= OnSecondObserverChanged;
            if (scene.IsInsideTree()) root.GetTree().Root.RemoveChild(scene);
            scene.Free();
            root.NewGame();
        }
    }

    private static void TestStaleUiAfterStateReplacement(SmokeTestResult result)
    {
        var root = GameRoot.Instance;
        root.NewGame();
        var scene = GD.Load<PackedScene>("res://scenes/Game.tscn").Instantiate();
        var notifications = 0;
        void OnChanged() => notifications++;
        try
        {
            root.GetTree().Root.AddChild(scene);
            var shell = scene.GetNode<UiShellController>("UiShell");
            root.StateChanged += OnChanged;
            foreach (var load in new[] { false, true })
            {
                root.NewGame();
                Check(result, root.SaveSlot(99), "command replacement fixture saved");
                var oldState = root.State;
                var characterId = root.Roster.Characters.First().Id;
                var assignment = root.Schedule.GetAssignment(characterId);
                var job = root.Schedule.AssignableJobs.First(value => value.Id != assignment);
                shell.ShowScreen("schedule");
                var staleButton = Buttons(shell).First(button => button.Text == job.DisplayName);
                if (load) Check(result, root.LoadSlot(99), "command replacement fixture loaded");
                else root.NewGame();
                notifications = 0;
                // QueueFree has not run yet: a queued signal can still reach the old callback.
                staleButton.EmitSignal(BaseButton.SignalName.Pressed);
                Check(result, notifications == 0 && root.Schedule.GetAssignment(characterId) == assignment,
                    $"command stale UI rejected after {(load ? "load" : "new game")}");
                Check(result, oldState.Schedule.AssignedJobs[characterId] == assignment,
                    "command stale UI does not mutate detached state");
                Buttons(shell).First(button => button.Text == job.DisplayName).EmitSignal(BaseButton.SignalName.Pressed);
                Check(result, notifications == 1 && root.Schedule.GetAssignment(characterId) == job.Id,
                    "command rebound UI mutates current state exactly once");
            }
        }
        finally
        {
            root.StateChanged -= OnChanged;
            if (scene.IsInsideTree()) root.GetTree().Root.RemoveChild(scene);
            scene.Free();
            root.Save.Delete(99);
            root.NewGame();
        }
    }

    private static void TestCommandBoundaries(SmokeTestResult result)
    {
        var root = GameRoot.Instance;
        root.NewGame();
        var notifications = 0;
        var observedState = root.State;
        var observedGeneration = root.StateGeneration;
        void OnChanged()
        {
            notifications++;
            observedState = root.State;
            observedGeneration = root.StateGeneration;
        }
        root.StateChanged += OnChanged;
        try
        {
            var character = root.Roster.Characters.First();
            var id = character.Id;
            var generation = root.StateGeneration;
            var assignment = root.Schedule.GetAssignment(id);
            foreach (var invalid in new string?[] { null, "", "MISSING_ID" })
            {
                Check(result, !root.TryAssignJob(invalid, "rest", generation)
                    && !root.TryAssignJob(id, invalid, generation)
                    && !root.TryConductMentorship(invalid, generation)
                    && !root.TryCompleteBondEvent(invalid, generation), "command rejects invalid identifiers");
            }
            Check(result, !root.TryAssignJob(id, assignment, generation) && notifications == 0,
                "command invalid and no-op requests emit no change");
            var bondEvent = root.Data.BondEvents.Values.First(value => value.CharacterId == id && value.RequiredBond > 0);
            character.Bond = bondEvent.RequiredBond - 1;
            Check(result, !root.TryCompleteBondEvent(bondEvent.Id, generation) && notifications == 0,
                "command locked event emits no change");
            character.Bond = 100;
            Check(result, root.TryCompleteBondEvent(bondEvent.Id, generation) && notifications == 1,
                "command valid event emits one change");
            var stockpile = root.State.Ranch.Stockpile.ToDictionary(pair => pair.Key, pair => pair.Value);
            Check(result, !root.TryCompleteBondEvent(bondEvent.Id, generation) && notifications == 1
                && stockpile.Count == root.State.Ranch.Stockpile.Count
                && stockpile.All(pair => root.State.Ranch.Stockpile[pair.Key] == pair.Value),
                "command duplicate event neither notifies nor rewards twice");

            Check(result, root.SaveSlot(99), "command lifecycle fixture saved");
            foreach (var replacement in new[] { "new", "load", "ngplus" })
            {
                var oldGeneration = root.StateGeneration;
                var oldState = root.State;
                var oldSchedule = root.Schedule;
                var oldBond = root.Bond;
                notifications = 0;
                if (replacement == "new") root.NewGame();
                else if (replacement == "load") Check(result, root.LoadSlot(99), "command lifecycle fixture loads");
                else root.StartNewGamePlus();
                Check(result, root.StateGeneration == oldGeneration + 1 && observedGeneration == root.StateGeneration
                    && ReferenceEquals(observedState, root.State) && !ReferenceEquals(oldState, root.State)
                    && !ReferenceEquals(oldSchedule, root.Schedule) && !ReferenceEquals(oldBond, root.Bond)
                    && notifications == 1, $"command {replacement} exposes rebound services and generation once");
                notifications = 0;
                Check(result, !root.TryAssignJob(id, "pasture", oldGeneration)
                    && !root.TryConductMentorship(id, oldGeneration)
                    && !root.TryCompleteBondEvent(bondEvent.Id, oldGeneration) && notifications == 0,
                    $"command {replacement} rejects all stale command types");
                Check(result, root.TryConductMentorship(id, root.StateGeneration) && notifications == 1,
                    $"command {replacement} accepts current generation");
            }
            root.Save.Delete(99);
            generation = root.StateGeneration;
            notifications = 0;
            Check(result, !root.LoadSlot(99) && root.StateGeneration == generation && notifications == 0,
                "command failed load keeps generation and observers unchanged");
        }
        finally
        {
            root.StateChanged -= OnChanged;
            root.Save.Delete(99);
            root.NewGame();
        }
    }

    private static IEnumerable<Button> Buttons(Node node)
    {
        if (node is Button button) yield return button;
        foreach (var child in node.GetChildren())
            foreach (var descendant in Buttons(child))
                yield return descendant;
    }

    private static void Check(SmokeTestResult result, bool condition, string description)
    {
        result.Passed &= condition;
        result.Lines.Add($"SMOKE {(condition ? "OK" : "FAIL")} {description}");
    }
}
