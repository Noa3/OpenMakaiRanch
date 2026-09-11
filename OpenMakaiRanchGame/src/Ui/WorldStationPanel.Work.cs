using System;
using System.Linq;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.World;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Ui;

public partial class WorldStationPanel
{
    private string _stationPage = "overview";
    private string _stationFeedback = "";
    public string StationPage => _stationPage;

    private void RenderWorkStation(WorldStation station)
    {
        var hasFacility = _game.Data.Facilities.TryGetValue(station.RequiredFacilityId, out _);
        // The tutorial is already a guided one-task flow: retain its visible Build -> Assign sequence.
        if (!_world.IsGuidedOpening)
        {
            var tabs = new HFlowContainer { Name = "StationTabs", SizeFlagsHorizontal = SizeFlags.ExpandFill };
            tabs.AddThemeConstantOverride("h_separation", 8);
            tabs.AddThemeConstantOverride("v_separation", 6);
            _content.AddChild(tabs);
            AddTab("overview", T("world.station.tab.overview", "Overview"));
            AddTab("team", T("world.station.tab.team", "Team"));
            if (hasFacility) AddTab("upgrade", T("world.station.tab.upgrade", "Equipment"));
            void AddTab(string page, string title)
            {
                var button = Action("StationTab_" + page, title, () => ShowStationPage(page), _stationPage == page);
                button.SizeFlagsHorizontal = SizeFlags.Fill;
                button.CustomMinimumSize = new Vector2(110, 40);
                _content.RemoveChild(button); tabs.AddChild(button);
            }
        }
        if (_stationFeedback.Length > 0)
        {
            var outcome = Text(_stationFeedback); outcome.Name = "StationOutcome";
            outcome.AddThemeColorOverride("font_color", new Color("e8d6a5"));
            _content.AddChild(outcome);
        }
        if (_world.IsGuidedOpening)
        {
            if (!station.IsAvailable) AddStationUpgrade(station);
            if (_game.Data.Jobs.TryGetValue(station.CommandTargetId, out var guidedJob)) RenderStationTeam(station, guidedJob);
            return;
        }
        if (_stationPage == "upgrade") { AddStationUpgrade(station); return; }
        if (!_game.Data.Jobs.TryGetValue(station.CommandTargetId, out var job))
        { _content.AddChild(Text(T("world.work.locked", "This job is not available yet."))); return; }
        if (_stationPage == "team") { RenderStationTeam(station, job); return; }

        var workers = _game.Roster.Characters.Count(c => _game.Schedule.GetAssignment(c.Id) == job.Id);
        _content.AddChild(Text(T("world.station.overview.staff", "Planned team: {0} residents • {1}", workers, JobName(job.Id, job.DisplayName))));
        if (hasFacility)
        {
            var offer = _game.Ranch.InspectFacilityUpgrade(station.RequiredFacilityId);
            _content.AddChild(Text(T("world.station.overview.level", "Equipment level: {0} • facility upkeep before research: {1} G/day", offer.Level, offer.BaseUpkeepBefore)));
        }
        if (!station.IsAvailable)
        {
            _content.AddChild(Text(station.UnavailableReason ?? T("world.station.build_first", "Build this facility before assigning its team.")));
            if (hasFacility) Action("StationBuildDetails", T("world.station.review_build", "Review construction and running costs"), () => ShowStationPage("upgrade"));
        }
        else Action("StationManageTeam", T("world.station.manage_team", "Choose who works here"), () => ShowStationPage("team"));
        _content.AddChild(Text(T("world.station.settlement", "Assignments are plans. Work output is added once when you sleep, not when you press Assign.")));
        if (!string.IsNullOrWhiteSpace(job.ResourceId))
        {
            var stock = _game.State.Ranch.Stockpile.TryGetValue(job.ResourceId, out var count) ? count : 0;
            _content.AddChild(Text(T("world.station.stock", "Ranch stock: {0} {1}", stock, ResourceName(job.ResourceId))));
            // These are base data, not an inaccurate promise of tonight's exact result.
            _content.AddChild(Text(T("world.station.base_output", "Base job data per resident: {0} G and {1} {2}. Skills, condition and research change the settled result.",
                job.GoldIncome, job.ResourceAmount, ResourceName(job.ResourceId))));
        }
        var alerts = WorldAlertEvaluator.Evaluate(_game).Where(a => WorldDestinationCatalog.ForAlert(a).TargetId == station.TargetId).ToArray();
        foreach (var alert in alerts)
            _content.AddChild(Text(T("world.alert.detail", "{0}: {1}", alert.Severity == WorldAlertSeverity.Info ? T("world.info", "Info") : T("world.attention", "Attention"), alert.Detail)));
        if (_id == "office")
        {
            Service("inventory", T("world.service.storage", "Storage"));
            Service("milestones", T("world.service.records", "Ranch records"));
            Service("milk", T("world.service.shipments", "Shipments"));
            Service("report", T("world.service.report", "Last daily report"));
        }
        if (_id == "workshop") AddWorkService(station, "research", T("world.service.research", "Workshop research"));
        if (_id == "pharmacy_lab") AddWorkService(station, "pharmacy_list", T("world.service.recipes", "Pharmacy recipes"));
    }

    private void AddWorkService(WorldStation station, string screen, string title)
    {
        if (station.IsAvailable) Service(screen, title);
        else
        {
            var button = Action("Service_" + screen, title, () => "", disabled: true);
            button.TooltipText = T("world.station.service_locked", "Build this workplace before using its service.");
            _content.AddChild(Text(button.TooltipText));
        }
    }

    private void RenderStationTeam(WorldStation station, JobDefinition job)
    {
        _content.AddChild(Text(T("world.station.team.help", "Choose a resident or send a current worker to rest. Changing plans costs no stamina and pays no production.")));
        var available = station.IsAvailable && job.Assignable;
        if (!available) _content.AddChild(Text(station.UnavailableReason ?? T("world.work.locked", "This job is not available yet.")));
        var residents = _game.Roster.Characters.OrderByDescending(c => _game.Schedule.GetAssignment(c.Id) == job.Id).ToArray();
        if (residents.Length == 0) _content.AddChild(Text(T("world.station.team.empty", "There are no residents to assign yet.")));
        foreach (var character in residents)
        {
            var residentId = character.Id;
            var name = string.IsNullOrWhiteSpace(character.DisplayNameOverride) ? _game.Roster.DefinitionFor(character).DisplayName : character.DisplayNameOverride;
            var previousJob = _game.Schedule.GetAssignment(residentId);
            var previousName = _game.Data.Jobs.TryGetValue(previousJob, out var current) ? JobName(previousJob, current.DisplayName) : previousJob;
            _content.AddChild(Text(T("world.work.resident", "{0} • {1} • Energy {2} • Fatigue {3}", name, previousName, character.Energy, character.Fatigue)));
            if (character.Mature.FallState == FallState.Collapse)
                _content.AddChild(Text(T("world.station.team.collapsed", "This resident is collapsed and cannot produce work. Consider rest and care.")));
            else if (character.Fatigue >= 60)
                _content.AddChild(Text(T("world.station.team.tired", "High fatigue reduces work output. Rest is an alternative.")));
            var stationId = station.TargetId; var generation = _generation; var day = _day; var phase = _phase;
            Action("Assign_" + residentId, previousJob == job.Id ? T("world.work.assigned", "{0} works here", name)
                : T("world.work.assign", "Assign {0} here", name), () => StationResult(
                    _world.TryAssignAtStation(stationId, residentId, previousJob, false, generation, day, phase)), !available || previousJob == job.Id);
            if (previousJob == job.Id)
                Action("Rest_" + residentId, T("world.work.rest", "Send {0} to rest", name), () => StationResult(
                    _world.TryAssignAtStation(stationId, residentId, previousJob, true, generation, day, phase)));
        }
    }

    private void AddStationUpgrade(WorldStation station)
    {
        var offer = _game.Ranch.InspectFacilityUpgrade(station.RequiredFacilityId);
        if (!_game.Data.Facilities.ContainsKey(station.RequiredFacilityId))
        { _content.AddChild(Text(offer.Reason)); return; }
        _content.AddChild(Text(T("world.station.upgrade.level", "Equipment level {0} → {1}", offer.Level, offer.NextLevel)));
        _content.AddChild(Text(T("world.station.upgrade.wallet", "One-time price: {0} G • Your wallet: {1} G", offer.Cost, _game.Economy.Gold)));
        _content.AddChild(Text(T("world.station.upgrade.local_upkeep", "This facility before research: {0} → {1} G/day", offer.BaseUpkeepBefore, offer.BaseUpkeepAfter)));
        _content.AddChild(Text(T("world.station.upgrade.total_upkeep", "Whole-ranch facility upkeep: {0} → {1} G/day, including your research discount.", offer.RanchUpkeepBefore, offer.RanchUpkeepAfter)));
        var stationId = station.TargetId; var generation = _generation; var day = _day; var phase = _phase;
        var button = Action("FacilityUpgrade", offer.Level == 0 ? T("world.facility.build", "Build this facility — {0} G", offer.Cost)
            : T("world.station.upgrade.confirm", "Confirm level {0} — {1} G", offer.NextLevel, offer.Cost),
            () => StationResult(_world.TryUpgradeAtStation(stationId, offer, generation, day, phase), returnToOverview: true), !offer.CanUpgrade);
        button.TooltipText = offer.Reason;
        if (!offer.CanUpgrade) _content.AddChild(Text(offer.Reason));
        if (!_world.IsGuidedOpening)
            Action("StationCancelUpgrade", T("world.station.upgrade.back", "Back without buying"), () => ShowStationPage("overview"));
        // The price and recurring bill precede the decision; longer explanations follow it.
        _content.AddChild(Text(T("world.station.upgrade.expenses", "Pet care and any unstaffed-dairy penalty are separate daily costs.")));
        _content.AddChild(Text(T("world.facility.envelope", "Equipment upgrades use the reserved footprint. Higher levels do not enlarge the building or block its entrance.")));
        _content.AddChild(Text(T("world.station.upgrade.output", "An equipment level is not a multiplier on daily job output. Workers and research still determine production; existing automation and project rules remain in effect.")));
    }

    private string ShowStationPage(string page)
    {
        _stationPage = page; _stationFeedback = ""; _status.Text = ""; _status.TooltipText = "";
        _close.GrabFocus(); _scroll.ScrollVertical = 0; Render();
        return "";
    }

    private string StationResult(StationActionResult result, bool returnToOverview = false)
    {
        if (Visible && ContextMatches())
        {
            if (result.Success && returnToOverview) _stationPage = "overview";
            _stationFeedback = result.Message;
            _close.GrabFocus(); _scroll.ScrollVertical = 0;
        }
        return ""; // Full outcome remains readable in scrollable content, not a clipped footer.
    }
}
