using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

public partial class UiLayoutAcceptance
{
    private async Task CheckNightLoop(WorldGameController world)
    {
        var game = GameRoot.Instance;
        var shell = world.Shell!;
        // Explicit Day-2 UI fixture following the unchanged layout/no-mutation checks.
        // Work output and settlement use existing services; no payment is injected.
        var day = game.State.Calendar.Day;
        game.State.Calendar.Phase = DayPhase.Night;
        game.State.Calendar.NightAction = string.Empty;
        game.NotifyStateChanged();
        await Resize(new Vector2I(640, 480));
        world.OpenManagementScreen("ranch");
        await Frames(8);
        var gold = game.Economy.Gold;
        var stamina = game.State.Player.Stamina;
        var stats = game.Roster.Characters.Select(c => (c.Energy, c.Fatigue, c.Bond, c.Morale)).ToArray();
        var capacity = game.State.Player.DailyStaminaBonus;
        var time = Descendants(shell).OfType<Button>().Single(b => b.Name == "OverviewAdvanceTime");
        Check(time.Text == "Plan Night", "overview time action explicitly requests an unplanned night");
        await FocusClick(time);
        Check(game.State.Calendar.Day == day && gold == game.Economy.Gold && shell.CurrentScreen == "ranch",
            "clicking Plan Night opens planning without settling or paying");
        Check(shell.GetNode<ScrollContainer>(shell.ScrollPath).Size.Y >= 96,
            "long planning status leaves a usable content viewport instead of expanding the whole header");
        var status = shell.GetNode<Label>(shell.StatusLabelPath);
        Check(!string.IsNullOrEmpty(status.Text) && status.TooltipText == status.Text,
            "bounded header retains the complete status message in its tooltip");
        var initialChoices = Descendants(shell).OfType<Button>().Where(b => b.Name.ToString().StartsWith("NightChoice_")).ToArray();
        Check(initialChoices.Length == 3 && initialChoices[0].GetGlobalRect().End.Y <= initialChoices[1].GlobalPosition.Y
            && initialChoices[1].GetGlobalRect().End.Y <= initialChoices[2].GlobalPosition.Y,
            "nightly choices have separate non-overlapping hit targets, not stacked panel children");
        foreach (var choice in new[] { "rest", "train", "admin", "rest" })
        {
            var button = Descendants(shell).OfType<Button>().Single(b => b.Name == "NightChoice_" + choice);
            await FocusClick(button);
            Check(game.State.Calendar.NightAction == choice,
                $"actual night choice click selects {choice}, including revising a previous selection");
            var choices = Descendants(shell).OfType<Button>().Where(b => b.Name.ToString().StartsWith("NightChoice_")).ToArray();
            Check(choices.Length == 3 && choices.Count(b => b.Disabled) == 1
                && choices.Single(b => b.Disabled).Name == "NightChoice_" + choice,
                $"{choice}: alternatives remain available and only the currently selected option is disabled");
        }
        Check(gold == game.Economy.Gold && stamina == game.State.Player.Stamina
            && capacity == game.State.Player.DailyStaminaBonus
            && stats.SequenceEqual(game.Roster.Characters.Select(c => (c.Energy, c.Fatigue, c.Bond, c.Morale))),
            "revising the plan does not immediately recover, train, charge stamina or consume the bath bonus");
        await Capture("night-planning-640x480");

        // Queued-signal checks supplement, rather than replace, viewport clicks.
        var retired = Descendants(shell).OfType<Button>().Single(b => b.Name == "NightChoice_train");
        shell.ShowScreen("schedule");
        retired.EmitSignal(BaseButton.SignalName.Pressed);
        Check(game.State.Calendar.NightAction == "rest" && shell.CurrentScreen == "schedule",
            "a retired night's button cannot change the plan or reopen its old menu");
        await Frames(6);
        world.OpenManagementScreen("ranch");
        await Frames(6);
        await FocusClick(Descendants(shell).OfType<Button>().Single(b => b.Name == "NightChoice_train"));
        var bathButton = Descendants(shell).OfType<Button>().Single(b => b.Name == "PlayerBathAction");
        await FocusClick(bathButton);
        Check(game.State.Player.BathedToday
            && game.State.Player.NextDayStaminaBonus == PlayerStaminaService.HotBathNextDayBonus
            && game.State.Calendar.NightAction == "train",
            "a prepared bath schedules tomorrow's bonus without overwriting the chosen Training plan");
        await Frames(6);
        var ending = Descendants(shell).OfType<Button>().Single(b => b.Name == "OverviewAdvanceTime");
        Check(ending.Text == "End Day", "a valid choice changes the overview action to End Day");
        var beforeGold = game.Economy.Gold;
        var beforeReports = game.State.Reports.Count;
        await FocusClick(ending);
        var report = game.LastDailyReport;
        Check(game.State.Calendar.Day == day + 1 && game.State.Calendar.Phase == DayPhase.Morning
            && game.State.Calendar.NightAction == string.Empty && game.State.Reports.Count == beforeReports + 1
            && shell.CurrentScreen == "report" && report is not null, "the actual End Day button reaches one next-morning settlement report");
        if (report is null) throw new System.InvalidOperationException("The actual End Day click produced no daily report.");
        Check(report.NetGold == (long)game.Economy.Gold - beforeGold
            && (long)report.Income - report.Expenses == report.NetGold
            && game.State.Economy.LastIncome == report.Income && game.State.Economy.LastExpenses == report.Expenses,
            "rendered report totals reconcile with the real wallet and overview, including events and rewards");
        Check(game.State.Player.DailyStaminaBonus == PlayerStaminaService.HotBathNextDayBonus,
            "the separately prepared bath still supplies tomorrow's stamina bonus");
        await Capture("day-report-640x480");
        await Resize(new Vector2I(960, 540));
        await Frames(6);
        await Capture("day-report-960x540");
        Check(game.SaveSlot(99), "completed night/report writes the verified disposable save slot");
        var savedGold = game.Economy.Gold;
        game.NewGame();
        Check(game.LoadSlot(99), "saved completed night loads through the real root boundary");
        Check(game.State.Calendar.Day == day + 1 && game.Economy.Gold == savedGold
            && game.State.Reports.Any(r => r.Day == day && r.NetGold == report.NetGold && r.Income == report.Income && r.Expenses == report.Expenses),
            "day, wallet and complete report totals survive current-schema saving together");
        game.Save.Delete(99);
        await CheckAdventureJourney(world);
        await CheckManagementJourney(world);
        await CheckStationJourney(world);
        await CheckProjectsJourney(world);
        await CheckLocalizationJourney(world);
        await CheckSharedEveningJourney(world);
        await CheckResidentJourney(world);
        await CheckWorkPlanningJourney(world);
        await CheckGameplayPanels(world);
    }

    private async Task FocusClick(Button button)
    {
        button.GrabFocus();
        await Frames(5);
        Check(!button.Disabled && VisibleTarget(button), $"night flow: {button.Name} is an enabled visible physical hit target");
        await Click(button);
        await Frames(5);
    }
}
