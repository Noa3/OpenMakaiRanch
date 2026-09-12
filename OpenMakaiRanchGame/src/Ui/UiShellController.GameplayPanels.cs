using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Ui;

/// <summary>Dedicated service views on the existing shell/theme; no second economy or global hub.</summary>
public partial class UiShellController
{
    private bool _reportDetails;
    private bool _researchCompleted;
    private int? _localReportDay;

    private bool TryRenderGameplayPanel(string screen)
    {
        if (screen == "victory") { RenderCompletionPanel(); return true; }
        if (!IsDedicatedService) return false;
        switch (screen)
        {
            case "shop": RenderLocalShop(); return true;
            case "research": RenderLocalResearch(); return true;
            case "inventory": RenderLocalStorage(); return true;
            case "report": RenderLocalReport(); return true;
            default: return false;
        }
    }

    private VBoxContainer GameplayCard(string id, string title)
    {
        var card = CardContainer(); card.Name = id; _content.AddChild(card);
        var body = CardContent(); card.AddChild(body);
        body.AddChild(SubtitleLabel(title));
        return body;
    }

    private Button GameplayButton(Node parent, string id, string text, Func<bool> command, bool disabled = false)
    {
        var revision = _viewRevision; var generation = _game.StateGeneration;
        var day = _game.State.Calendar.Day; var phase = _game.State.Calendar.Phase;
        var screen = _currentScreen; var root = _serviceRoot;
        var button = SecondaryButton(text); button.Name = id; button.Disabled = disabled;
        button.CustomMinimumSize = new Vector2(0, 40);
        parent.AddChild(button);
        button.Pressed += () =>
        {
            if (!IsInsideTree() || !IsVisibleInTree() || !GodotObject.IsInstanceValid(button)
                || button.Disabled || !_content.IsAncestorOf(button) || revision != _viewRevision
                || generation != _game.StateGeneration || day != _game.State.Calendar.Day
                || phase != _game.State.Calendar.Phase || screen != _currentScreen || root != _serviceRoot
                || GetTree().Paused) return;
            command();
        };
        return button;
    }

    public bool TryBackDedicatedService()
    {
        if (!IsDedicatedService || _serviceGeneration != _game.StateGeneration || _serviceRoot == _currentScreen
            || !IsVisibleInTree() || _game.ActiveCombatSession is { IsFinished: false }) return false;
        var target = _serviceRoot!;
        ShowScreen(target);
        return _currentScreen == target;
    }

    private void AddDedicatedNavigation()
    {
        if (!IsDedicatedService || _fullScreenMode || _content.GetNodeOrNull<Control>("ServiceNavigation") is not null) return;
        var row = new VBoxContainer { Name = "ServiceNavigation", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _content.AddChild(row); _content.MoveChild(row, 0);
        row.AddChild(MutedLabel(T("panel.service.context", "At this service: {0}", ScreenTitle(_serviceRoot!))));
        if (_currentScreen != _serviceRoot)
            GameplayButton(row, "ServiceBack", T("panel.service.back", "Back to {0}", ScreenTitle(_serviceRoot!)), TryBackDedicatedService);
        // All service roots retain the existing fixed ReturnToWorld button in the shell header.
    }

    private bool CommitService(Func<StationActionResult> command)
    {
        if (!CanExecuteDedicatedCommand()) return false;
        var generation = _game.StateGeneration;
        var result = command();
        if (generation == _game.StateGeneration && IsVisibleInTree()) SetStatus(result.Message, !result.Success);
        return result.Success;
    }

    private void RenderLocalShop()
    {
        AddTitle(T("panel.shop.title", "General store"));
        _content.AddChild(MutedLabel(T("panel.shop.wallet", "Available gold: {0} G. Purchases go into your bag and do not advance time.", _game.Economy.Gold)));
        GameplayButton(_content, "ShopInventory", T("panel.storage.bag", "Your bag"), () => { ShowScreen("inventory"); return _currentScreen == "inventory"; });
        foreach (var item in _game.Data.Items.Values.Where(item => item.Price > 0).OrderBy(item => item.Id, StringComparer.Ordinal))
        {
            var offer = _game.InspectPurchase(item.Id, 1);
            if (offer is null) continue;
            var body = GameplayCard("ShopItem_" + item.Id, T("item." + item.Id + ".name", item.DisplayName));
            body.AddChild(MutedLabel(T("panel.shop.item", "Price: {0} G • In your bag: {1}", offer.UnitPrice, offer.Owned)));
            var generation = _game.StateGeneration; var day = _game.State.Calendar.Day; var phase = _game.State.Calendar.Phase;
            var buy = GameplayButton(body, "Purchase_" + item.Id, T("panel.shop.buy", "Buy one — {0} G", offer.TotalPrice),
                () => CommitService(() => _game.TryPurchaseOffer(offer, generation, day, phase)), !offer.CanBuy);
            if (!offer.CanBuy) buy.TooltipText = T("panel.shop.unavailable", "Not enough gold or no room in this inventory stack. Nothing is charged.");
        }
    }

    private void RenderLocalResearch()
    {
        AddTitle(T("panel.research.title", "Workshop research"));
        _content.AddChild(MutedLabel(T("panel.research.help", "Research spends the listed ranch resource, not gold. Existing effects apply immediately; there is no additional waiting period.")));
        var tabs = FlowRow(8); _content.AddChild(tabs);
        GameplayButton(tabs, "ResearchAvailable", T("panel.research.available", "To research"), () => { _researchCompleted = false; ShowScreen("research"); return true; }, !_researchCompleted);
        GameplayButton(tabs, "ResearchCompleted", T("panel.research.completed", "Unlocked"), () => { _researchCompleted = true; ShowScreen("research"); return true; }, _researchCompleted);
        var skills = _game.Data.Skills.Values.Where(skill => _game.State.Research.UnlockedSkillIds.Contains(skill.Id) == _researchCompleted).ToArray();
        if (skills.Length == 0) _content.AddChild(MutedLabel(T("panel.research.empty", "No research in this section.")));
        foreach (var skill in skills)
        {
            var offer = _game.InspectResearch(skill.Id);
            if (offer is null) continue;
            var body = GameplayCard("ResearchItem_" + skill.Id, T("skill." + skill.Id + ".name", skill.DisplayName));
            body.AddChild(MutedLabel(T("skill." + skill.Id + ".description", skill.Description)));
            if (offer.Unlocked) { body.AddChild(MutedLabel(T("panel.research.completed", "Unlocked"))); continue; }
            body.AddChild(MutedLabel(T("panel.research.price", "Required: {0} {1} • Available: {2}", offer.Cost, ResourceName(offer.ResourceId), offer.Available)));
            var generation = _game.StateGeneration; var day = _game.State.Calendar.Day; var phase = _game.State.Calendar.Phase;
            var unlock = GameplayButton(body, "Unlock_" + skill.Id, T("panel.research.unlock", "Unlock research"),
                () => CommitService(() => _game.TryResearchOffer(offer, generation, day, phase)), !offer.CanUnlock);
            unlock.TooltipText = offer.CanUnlock ? T("panel.research.applies", "Spend exactly the displayed resources once.")
                : T("panel.research.shortage", "The required ranch resources are not available. Nothing is spent.");
        }
    }

    private void RenderLocalStorage()
    {
        AddTitle(T("panel.storage.title", "Inventory and ranch stock"));
        _content.AddChild(MutedLabel(T("panel.storage.help", "These are separate stocks. Daily lunch can use ranch meals; personal gifts and care use items from your bag. This page does not convert or consume items.")));
        var bag = GameplayCard("StorageBag", T("panel.storage.bag", "Your bag"));
        foreach (var item in _game.Inventory.Items.Where(item => item.Value > 0).OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var name = _game.Data.Items.TryGetValue(item.Key, out var definition) ? definition.DisplayName : item.Key;
            bag.AddChild(MutedLabel(T("panel.storage.amount", "{0}: {1}", T("item." + item.Key + ".name", name), item.Value)));
        }
        if (!_game.Inventory.Items.Any(item => item.Value > 0)) bag.AddChild(MutedLabel(T("panel.storage.empty", "Empty")));
        var stock = GameplayCard("StorageStock", T("panel.storage.stock", "Ranch production stock"));
        foreach (var item in _game.Ranch.Stockpile.OrderBy(item => item.Key, StringComparer.Ordinal))
            stock.AddChild(MutedLabel(T("panel.storage.amount", "{0}: {1}", ResourceName(item.Key), item.Value)));
        var plan = _game.InspectNextLunch();
        stock.AddChild(MutedLabel(T("panel.provisions.split", "From ranch meals: {0} • From meal boxes: {1} • Missing servings: {2}", plan.PantryMeals, plan.MealBoxes, plan.MissingMeals)));
    }

    private void RenderLocalReport()
    {
        AddTitle(T("panel.report.title", "Daily report"));
        GameplayButton(_content, "ReportReturn", T("world.back", "Back to the world"), () => WorldHost()?.CloseManagement() == true);
        var history = _game.State.Reports.OrderByDescending(entry => entry.Day).ToArray();
        var report = history.FirstOrDefault(entry => entry.Day == _localReportDay)
            ?? _game.LastDailyReport ?? history.FirstOrDefault();
        if (report is null) { _content.AddChild(MutedLabel(T("panel.report.empty", "No day has been settled yet."))); return; }
        _localReportDay = report.Day;
        if (history.Length > 1)
        {
            var index = Array.FindIndex(history, entry => entry.Day == report.Day);
            var older = index >= 0 && index + 1 < history.Length ? history[index + 1].Day : (int?)null;
            var newer = index > 0 ? history[index - 1].Day : (int?)null;
            var navigation = FlowRow(8); _content.AddChild(navigation);
            GameplayButton(navigation, "ReportOlder", T("panel.report.older", "Older day"),
                () => { _localReportDay = older; ShowScreen("report"); return true; }, older is null);
            GameplayButton(navigation, "ReportNewer", T("panel.report.newer", "Newer day"),
                () => { _localReportDay = newer; ShowScreen("report"); return true; }, newer is null);
        }
        var summary = GameplayCard("ReportSummary", T("panel.report.day", "Day {0}", report.Day));
        summary.AddChild(MutedLabel(T("panel.report.balance", "Income {0} G • Expenses {1} G • Net {2} G", report.Income, report.Expenses, report.NetGold)));
        summary.AddChild(MutedLabel(T("panel.report.scope", "This is the recorded settlement, including its events. Purchases made earlier in the day are separate transactions.")));
        GameplayButton(_content, "ReportDetails", _reportDetails ? T("panel.report.less", "Hide details") : T("panel.report.more", "Show events and details"),
            () => { _reportDetails = !_reportDetails; ShowScreen("report"); return true; });
        if (!_reportDetails) return;
        foreach (var entry in report.Events)
        {
            var card = GameplayCard("ReportEvent_" + report.Events.IndexOf(entry), entry.Title);
            card.AddChild(MutedLabel(entry.Description));
        }
        if (report.CharacterGrowth.Count > 0)
        {
            var growth = GameplayCard("ReportGrowth", T("panel.report.growth", "Resident skill changes"));
            foreach (var change in report.CharacterGrowth)
                growth.AddChild(MutedLabel(T("panel.report.gain", "{0}: {1} +{2}", change.DisplayName, change.SkillGained, change.Amount)));
        }
        var details = GameplayCard("ReportLines", T("panel.report.details", "Settlement details"));
        foreach (var line in report.Lines) details.AddChild(MutedLabel(line));
        details.AddChild(MutedLabel(T("panel.report.language", "Stored report entries retain the language used when they were recorded.")));
    }

    private void RenderCompletionPanel()
    {
        AddTitle(T("panel.completion.title", "Main objectives completed"));
        var generation = _game.StateGeneration;
        var day = _game.State.VictoryDay;
        if (day is null)
        {
            _content.AddChild(MutedLabel(T("panel.completion.unavailable", "There is no recorded completion in this game.")));
            return;
        }
        _content.AddChild(MutedLabel(T("panel.campaign.finished", "Completion recorded on day {0}. You can keep playing this ranch.", day.Value)));
        GameplayButton(_content, "ContinueRanching", T("victory.continue", "Continue Ranching"), () =>
        {
            if (WorldHost() is { } host) return host.LeaveCompletion(generation, day.Value, false);
            ClearServiceContext(); ShowScreen("ranch"); return _currentScreen == "ranch";
        });
        GameplayButton(_content, "CompletionReport", T("panel.completion.report", "Read the daily report"), () =>
        {
            if (WorldHost() is { } host) return host.LeaveCompletion(generation, day.Value, true);
            ClearServiceContext(); ShowScreen("report"); return _currentScreen == "report";
        });
        var body = GameplayCard("CompletionNext", T("panel.completion.next", "What comes next?"));
        body.AddChild(MutedLabel(T("panel.completion.keep", "Your residents, resources and remaining projects stay in this game. Optional challenges do not move the finish line.")));
        body.AddChild(MutedLabel(T("panel.completion.ngplus", "New Game+ is a separate decision from the main menu. Continuing here does not start a new run.")));
    }
}
