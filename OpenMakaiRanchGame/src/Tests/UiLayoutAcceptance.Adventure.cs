using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Ui;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

public partial class UiLayoutAcceptance
{
    private async Task CheckAdventureJourney(WorldGameController world)
    {
        CheckSequentialCardContract();
        var game = GameRoot.Instance;
        var shell = world.Shell!;
        // Continue the explicitly synthetic Day-2/night fixture after its real settlement
        // and save/load. Do not grant gold, items, health, combat stats or stamina here.
        world.Transition?.CompleteImmediately();
        Check(world.OpenManagementScreen("adventure"), "adventure: the guild is reachable after the completed night was loaded");
        await Frames(10);
        foreach (var size in new[] { new Vector2I(640, 480), new Vector2I(960, 540) })
        {
            await Resize(size);
            await Frames(6);
            CheckAdventureCardGeometry(shell, $"guild {size.X}x{size.Y}");
            await Capture($"adventure-guild-{size.X}x{size.Y}");
        }

        const string missionId = "road_patrol";
        var missionName = game.Data.Missions[missionId].DisplayName;
        var missionCard = Descendants(shell).OfType<PanelContainer>().Single(card =>
            Descendants(card).OfType<Label>().Any(label => label.Text == missionName)
            && Descendants(card).OfType<Button>().Count(button => button.Text == "Fight") == 1);
        var fight = Descendants(missionCard).OfType<Button>().Single(button => button.Text == "Fight");
        var day = game.State.Calendar.Day;
        var phase = game.State.Calendar.Phase;
        var stamina = game.State.Player.Stamina;
        var gold = game.Economy.Gold;
        await ClickAdventureButton(fight, "select the discovered Road Patrol mission");
        Check(shell.CurrentScreen == "combat" && game.CurrentCombatPhase == CombatPhase.PreBattle
            && game.LastCombatReport?.MissionId == missionId && game.CombatWorldTimeLocked,
            "adventure: the actual mission button enters its own combat preparation and locks world time");
        Check(game.Economy.Gold == gold && game.State.Player.Stamina == stamina,
            "adventure: inspecting a mission costs neither gold nor stamina");
        await Resize(new Vector2I(640, 480));
        await Frames(6);
        CheckAdventureCardGeometry(shell, "combat preparation 640x480");
        await Capture("combat-preparation-640x480");
        var tactical = Descendants(shell).OfType<Button>().Single(button => button.Text.StartsWith("Tactical Battle", StringComparison.Ordinal));
        await ClickAdventureButton(tactical, "start Tactical Battle");
        Check(game.ActiveCombatSession is not null && game.CurrentCombatPhase == CombatPhase.PlayerTurn,
            "adventure: an unboosted party reaches an actual player turn through Tactical Battle");
        if (game.ActiveCombatSession is not { IsFinished: false })
            throw new InvalidOperationException("The ordinary Road Patrol fixture did not reach a player turn.");
        Check(game.State.Player.Stamina == stamina - game.AdventureStaminaCost(missionId),
            "adventure: mission entry commits its daily stamina exactly once");
        Check(shell.GetNode<Button>(shell.EndDayButtonPath).Disabled && !game.AdvanceTime()
            && game.State.Calendar.Day == day && game.State.Calendar.Phase == phase,
            "adventure: both the visible time control and shared clock remain blocked during battle");
        CheckAdventureCardGeometry(shell, "player turn 640x480");
        await Capture("combat-player-turn-640x480");
        var playerName = game.ActiveCombatSession!.PlayerState!.DisplayName;
        var defend = Descendants(shell).OfType<Button>().Single(button => button.Text == "Defend");
        await ClickAdventureButton(defend, "Defend in the actual player turn");
        var report = game.LastCombatReport!;
        Check(report.Rounds.SelectMany(round => round.Actions).Any(action => action.ActorName == playerName && action.ActionType == "Defend"),
            "adventure: the real Defend hit target adds a defensive action to the combat log");
        if (game.ActiveCombatSession is { IsFinished: false })
        {
            var attack = Descendants(shell).OfType<Button>().First(button => button.Text.StartsWith("Attack ", StringComparison.Ordinal));
            await ClickAdventureButton(attack, "Attack a living enemy");
            Check(game.LastCombatReport!.Rounds.SelectMany(round => round.Actions).Any(action => action.ActorName == playerName && action.ActionType == "Attack"),
                "adventure: the real Attack hit target resolves an attack through the shared combat service");
        }
        if (game.ActiveCombatSession is { IsFinished: false })
            await ClickAdventureButton(Descendants(shell).OfType<Button>().Single(button => button.Text == "Auto Finish"), "finish the remaining turns");
        report = game.LastCombatReport!;
        Check(game.CurrentCombatPhase == CombatPhase.BattleResults && report.Outcome != MissionOutcome.None,
            "adventure: player commands and Auto Finish reach a concrete mission result");
        Check(game.State.Player.Stamina == stamina - game.AdventureStaminaCost(missionId)
            && game.State.Calendar.Day == day && game.State.Calendar.Phase == phase,
            "adventure: turns/results do not charge entry stamina again or silently advance the day");
        var resultBack = Descendants(shell.GetNode<Control>(shell.ContentPath)).OfType<Button>()
            .Single(button => button.Text == "Back");
        Check(ReferenceEquals(GetViewport().GuiGetFocusOwner(), resultBack) && VisibleTarget(resultBack),
            "adventure: replacing battle commands with the results Back button restores a visible focus target");
        var resolvedGold = game.Economy.Gold;
        var resolvedStamina = game.State.Player.Stamina;
        shell.ShowScreen("combat");
        shell.ShowScreen("combat");
        await Frames(8);
        Check(game.Economy.Gold == resolvedGold && game.State.Player.Stamina == resolvedStamina
            && ReferenceEquals(report, game.LastCombatReport), "adventure: redrawing results cannot repeat mission rewards or costs");
        CheckAdventureCardGeometry(shell, "combat results 640x480");
        await Capture("combat-results-640x480");
        await ClickAdventureButton(Descendants(shell.GetNode<Control>(shell.ContentPath)).OfType<Button>()
            .Single(button => button.Text == "Back"), "return from results to the guild");
        Check(shell.CurrentScreen == "adventure" && !game.CombatWorldTimeLocked && game.ActiveCombatSession is null,
            "adventure: the actual results Back button releases the encounter and world-time lock");
        var back = Descendants(shell).OfType<Button>().Single(button => button.Name == "ReturnToWorldButton");
        await ClickAdventureButton(back, "return from the guild to the ranch world");
        Check(!world.IsManagementVisible && world.Ranch?.InputGate.WorldInputEnabled == true,
            "adventure: Return to World restores movement after the mission");
        var health = game.Roster.Characters.Select(character => (character.Id, character.Hp, character.Energy)).ToArray();
        Check(game.SaveSlot(99), "adventure: the completed mission saves through the root in disposable slot 99");
        Check(game.LoadSlot(99), "adventure: the mission save loads through the root");
        Check(game.Economy.Gold == resolvedGold && game.State.Player.Stamina == resolvedStamina
            && game.State.Adventure.LastMissionId == missionId
            && health.SequenceEqual(game.Roster.Characters.Select(character => (character.Id, character.Hp, character.Energy)))
            && !game.CombatWorldTimeLocked && game.ActiveCombatSession is null,
            "adventure: mission identity, gold, stamina and battle wear persist without restoring a phantom encounter");
        game.Save.Delete(99);
    }

    private void CheckSequentialCardContract()
    {
        var panel = new PanelContainer();
        var title = new Label { Text = "Title" };
        var first = new Button { Text = "First" };
        var second = new Button { Text = "Second" };
        var notifications = 0;
        first.Pressed += () => notifications++;
        panel.AddChild(title);
        panel.AddChild(first);
        panel.AddChild(second);
        try
        {
            UiShellController.StackSequentialCards(panel);
            var content = panel.GetChildOrNull<VBoxContainer>(0);
            Check(panel.GetChildCount() == 1 && content is not null
                && content.GetChildren().SequenceEqual(new Node[] { title, first, second }),
                "card composition: sequential content retains its original nodes and order in one VBox");
            UiShellController.StackSequentialCards(panel);
            Check(ReferenceEquals(content, panel.GetChild(0)) && content!.GetChildCount() == 3,
                "card composition: repeated finalization leaves an already composed card unchanged");
            first.EmitSignal(BaseButton.SignalName.Pressed);
            Check(notifications == 1, "card composition: reparenting preserves each original button subscription exactly once");
        }
        finally { panel.Free(); }
    }

    private void CheckAdventureCardGeometry(UiShellController shell, string context)
    {
        var content = shell.GetNode<Control>(shell.ContentPath);
        var cards = Descendants(content).OfType<PanelContainer>().ToArray();
        Check(cards.Length > 0 && cards.All(card => card.GetChildren().OfType<Control>().Count() == 1),
            $"{context}: every panel has one layout owner rather than overlapping direct children");
        var stacks = Descendants(content).OfType<VBoxContainer>().Where(stack => stack.Name == "SequentialCardContent");
        Check(stacks.All(stack =>
        {
            var children = stack.GetChildren().OfType<Control>().Where(child => child.Visible).ToArray();
            return children.Zip(children.Skip(1), (a, b) => a.GetGlobalRect().End.Y <= b.GlobalPosition.Y + 1).All(value => value);
        }), $"{context}: labels, nested cards and command rows have non-overlapping vertical rectangles");
        Check(content.Size.X <= shell.GetNode<ScrollContainer>(shell.ScrollPath).Size.X + 1,
            $"{context}: the content remains usable without horizontal scrolling");
    }

    private async Task ClickAdventureButton(Button button, string context)
    {
        button.GrabFocus();
        await Frames(6);
        Check(!button.Disabled && VisibleTarget(button), $"adventure click: {context} is enabled and physically visible");
        await Click(button);
        await Frames(6);
    }
}
