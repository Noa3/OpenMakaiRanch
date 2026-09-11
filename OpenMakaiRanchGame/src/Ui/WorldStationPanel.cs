using System;
using System.Linq;
using Godot;
using static OpenMakaiRanch.Locale.LocaleCatalog;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Ui;

/// <summary>A physical station/resident view. Every mutation revalidates location and the captured session.</summary>
public partial class WorldStationPanel : Control
{
    private WorldGameController _world = null!;
    private GameRoot _game = null!;
    private PanelContainer _panel = null!;
    private VBoxContainer _content = null!;
    private ScrollContainer _scroll = null!;
    private Label _title = null!;
    private Label _status = null!;
    private Button _close = null!;
    private string _kind = string.Empty;
    private string _id = string.Empty;
    private ulong _generation;
    private int _day;
    private DayPhase _phase;
    private ulong _revision;
    private bool _busy;
    private bool _refreshPending;
    public string ContextId => _id;
    public string ContextKind => _kind;
    public event Action? Closed;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        var shade = new ColorRect { Color = new Color(0.015f, 0.025f, 0.04f, 0.45f), MouseFilter = MouseFilterEnum.Ignore };
        AddChild(shade); shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _panel = new PanelContainer { Name = "StationCard" };
        _panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color("17232e"), BorderColor = new Color("bfaa76"),
            BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2, BorderWidthBottom = 2,
            CornerRadiusTopLeft = 12, CornerRadiusTopRight = 12, CornerRadiusBottomLeft = 12, CornerRadiusBottomRight = 12,
            ContentMarginLeft = 20, ContentMarginRight = 20, ContentMarginTop = 16, ContentMarginBottom = 16
        });
        AddChild(_panel);
        var layout = new VBoxContainer(); layout.AddThemeConstantOverride("separation", 12); _panel.AddChild(layout);
        _title = Text(""); _title.Name = "StationTitle"; _title.AddThemeFontSizeOverride("font_size", 25); layout.AddChild(_title);
        _scroll = new ScrollContainer { Name = "StationScroll", SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, FollowFocus = true };
        layout.AddChild(_scroll);
        _content = new VBoxContainer { Name = "StationContent", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _content.AddThemeConstantOverride("separation", 10); _scroll.AddChild(_content);
        _status = Text(""); _status.MaxLinesVisible = 2; layout.AddChild(_status);
        _close = new Button { Name = "StationClose", Text = T("world.back", "Back to the world"), CustomMinimumSize = new Vector2(0, 40) };
        _close.Pressed += Close; layout.AddChild(_close);
        Visible = false;
    }

    public void Bind(WorldGameController world)
    {
        if (GodotObject.IsInstanceValid(_game)) _game.StateChanged -= OnStateChanged;
        _world = world; _game = GameRoot.Instance;
        _game.StateChanged += OnStateChanged;
    }

    public override void _ExitTree()
    {
        if (GodotObject.IsInstanceValid(_world)) _world.SetResidentConversationFocus("");
        if (GodotObject.IsInstanceValid(_game)) _game.StateChanged -= OnStateChanged;
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;
        var metrics = ScreenLayout.Calculate(GetViewport());
        var width = Mathf.Min(700, metrics.ContentWidth - 24);
        var height = Mathf.Min(690, metrics.ViewportSize.Y - metrics.SafeTop - metrics.SafeBottom - 24);
        _panel.Position = new Vector2(metrics.HorizontalCenter - width / 2, metrics.SafeTop + 12);
        _panel.Size = new Vector2(width, height);
        if (!ContextMatches()) Close();
        else if (_refreshPending && !_busy) { _refreshPending = false; Render(); }
    }

    public void OpenStation(string id) => Open("station", id);
    public void OpenResident(string id) => Open("resident", id);
    public void OpenGuide() => Open("guide", "places");

    private void Open(string kind, string id)
    {
        _residentPage = "overview"; _residentFeedback = "";
        _world.SetResidentConversationFocus(kind == "resident" ? id : "");
        _kind = kind; _id = id; _generation = _game.StateGeneration;
        _day = _game.State.Calendar.Day; _phase = _game.State.Calendar.Phase;
        _refreshPending = false; _status.Text = ""; _scroll.ScrollVertical = 0; Visible = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        Render();
        _close.GrabFocus();
    }

    public void Close()
    {
        if (!Visible) return;
        Visible = false; _revision++; _kind = ""; _id = "";
        _world.SetResidentConversationFocus("");
        if (GetViewport().GuiGetFocusOwner() is { } owner && IsAncestorOf(owner)) owner.ReleaseFocus();
        Closed?.Invoke();
    }

    private bool ContextMatches() => IsInsideTree() && GodotObject.IsInstanceValid(_world)
        && _generation == _game.StateGeneration && _day == _game.State.Calendar.Day && _phase == _game.State.Calendar.Phase
        && !_world.FlowLocksUi && !_game.CombatWorldTimeLocked
        && (_kind == "guide" ? _world.ActiveAreaId is "ranch" or "town"
            : _kind == "station" ? _world.CanUseStationHere(_world.ResolveStation(_id))
            : _kind == "resident" && _world.CanVisitResidentHere(_id));

    private void OnStateChanged()
    {
        if (!Visible) return;
        if (!ContextMatches()) { Close(); return; }
        _refreshPending = true;
    }

    private string _renderedLocale = "";

    private void Render()
    {
        var oldFocus = GetViewport().GuiGetFocusOwner();
        var focusName = oldFocus is not null && _content.IsAncestorOf(oldFocus) ? oldFocus.Name.ToString() : null;
        var scroll = _scroll.ScrollVertical;
        var revision = ++_revision;
        if (_renderedLocale != CurrentLocale) { _status.Text = ""; _status.TooltipText = ""; _residentFeedback = ""; _renderedLocale = CurrentLocale; }
        _close.Text = T("world.back", "Back to the world");
        foreach (var node in _content.GetChildren()) { _content.RemoveChild(node); node.QueueFree(); }
        BuildContent();
        var tree = GetTree();
        void Restore()
        {
            tree.ProcessFrame -= Restore;
            if (!GodotObject.IsInstanceValid(this) || !IsInsideTree() || !Visible
                || _revision != revision || !ContextMatches()) return;
            if (focusName is not null)
            {
                var target = _content.GetChildren().OfType<Button>().FirstOrDefault(b => b.Name == focusName && !b.Disabled);
                (target ?? _close).GrabFocus();
                _scroll.ScrollVertical = scroll;
                if (target is not null) _scroll.EnsureControlVisible(target);
            }
            else _scroll.ScrollVertical = scroll;
        }
        tree.ProcessFrame += Restore;
    }

    private void BuildContent()
    {
        if (_kind == "guide") { RenderGuide(); return; }
        if (_kind == "resident") { RenderResident(); return; }
        var station = _world.ResolveStation(_id);
        if (station is null) { Close(); return; }
        _title.Text = WorldName(station.TargetId, station.Label);
        if (_id == "ranch_house") { RenderHouse(); return; }
        if (_id == "pet_care")
        {
            _content.AddChild(Text(T("world.pets.help", "Check the needs of your adopted pets here.")));
            Service("pets", T("world.pets.open", "Care for pets")); return;
        }
        var alerts = WorldAlertEvaluator.Evaluate(_game).Where(a => WorldDestinationCatalog.ForAlert(a).TargetId == _id).ToArray();
        foreach (var alert in alerts) _content.AddChild(Text(T("world.alert.detail", "{0}: {1}", alert.Severity == WorldAlertSeverity.Info ? T("world.info", "Info") : T("world.attention", "Attention"), alert.Detail)));
        if (_game.Data.Jobs.TryGetValue(station.CommandTargetId, out var job))
        {
            _content.AddChild(Text(T("world.work.settlement", "{0} • output is settled at the end of the day, not on assignment.", JobName(job.Id, job.DisplayName))));
            _content.AddChild(Text(T("world.work.output", "Base output: {0} G / {1} {2}. Fatigue change: {3:+0;-0;0}. Facility and resident modifiers still apply.", job.GoldIncome, job.ResourceAmount, ResourceName(job.ResourceId), job.FatigueDelta)));
            var available = station.IsAvailable && _game.Schedule.AssignableJobs.Any(j => j.Id == job.Id);
            if (!available) _content.AddChild(Text(station.UnavailableReason ?? T("world.work.locked", "This job is not available yet.")));
            foreach (var character in _game.Roster.Characters)
            {
                var id = character.Id;
                var name = string.IsNullOrWhiteSpace(character.DisplayNameOverride) ? _game.Roster.DefinitionFor(character).DisplayName : character.DisplayNameOverride;
                var currentJob = _game.Schedule.GetAssignment(id);
                var currentName = _game.Data.Jobs.TryGetValue(currentJob, out var current) ? JobName(currentJob, current.DisplayName) : currentJob;
                _content.AddChild(Text(T("world.work.resident", "{0} • {1} • Energy {2} • Fatigue {3}", name, currentName, character.Energy, character.Fatigue)));
                Action($"Assign_{id}", currentJob == job.Id ? T("world.work.assigned", "{0} works here", name) : T("world.work.assign", "Assign {0} here", name), () =>
                {
                    var live = _world.ResolveStation(_id);
                    if (live?.IsAvailable != true || !_game.Schedule.AssignableJobs.Any(j => j.Id == job.Id)) return T("world.work.unavailable", "This station is unavailable.");
                    var ok = live.Activate(new WorldInteractionContext(id, _generation));
                    if (ok)
                    {
                        if (_world.IsGuidedOpening) Close();
                        _world.Ranch?.NotifyStationAssignment(id, job.Id);
                    }
                    return ok ? T("world.work.success", "{0} assigned. Production remains part of the daily settlement.", name) : T("world.work.unchanged", "Assignment unchanged.");
                }, !available || currentJob == job.Id);
                if (currentJob == job.Id)
                    Action($"Rest_{id}", T("world.work.rest", "Send {0} to rest", name), () => _game.TryAssignJob(id, "rest", _generation) ? T("world.work.rest_success", "Rest assigned.") : T("world.work.unchanged", "Assignment unchanged."));
            }
        }
        AddFacility(station.RequiredFacilityId);
        if (_id == "office")
        {
            Service("inventory", T("world.service.storage", "Storage"));
            Service("milestones", T("world.service.records", "Ranch records"));
            Service("milk", T("world.service.shipments", "Shipments"));
            Service("report", T("world.service.report", "Last daily report"));
        }
        if (_id == "workshop") Service("research", T("world.service.research", "Workshop research"));
        if (_id == "pharmacy_lab") Service("pharmacy_list", T("world.service.recipes", "Pharmacy recipes"));
    }

    private void AddFacility(string id)
    {
        if (!_game.Data.Facilities.TryGetValue(id, out var facility) || facility.BuildCost <= 0) return;
        var level = _game.Ranch.Facilities.TryGetValue(id, out var value) ? value : 0;
        var cost = _game.Ranch.FacilityUpgradeCost(facility, level);
        _content.AddChild(Text(T("world.facility.envelope", "Equipment upgrades use the reserved footprint. Higher levels do not enlarge the building or block its entrance.")));
        _content.AddChild(Text(T("world.facility.summary", "Facility level {0} • upkeep {1} G/day. Wallet: {2} G.", level, facility.UpkeepGold, _game.Economy.Gold)));
        Action("FacilityUpgrade", level == 0 ? T("world.facility.build", "Build this facility — {0} G", cost) : T("world.facility.upgrade", "Upgrade this facility — {0} G", cost), () =>
        {
            if (!_game.Ranch.UpgradeFacility(id, _game.Economy)) return T("world.facility.requirements", "The upgrade requirements are not met.");
            _game.NotifyStateChanged(); return T("world.facility.success", "Facility upgraded.");
        }, _game.Economy.Gold < cost);
    }

    private void RenderHouse()
    {
        _content.AddChild(Text(T("world.house.summary", "Day {0} • {1} • Stamina {2}/{3}", _day, EnumDisplayName(_phase), _game.State.Player.Stamina, _game.State.Player.MaxStamina + _game.State.Player.DailyStaminaBonus)));
        _content.AddChild(Text(T("world.house.help", "Night choices are revisable until you sleep. A prepared hot bath grants its extra energy tomorrow, separately from tonight's plan.")));
        if (_phase is DayPhase.Evening or DayPhase.Night)
            Action("HouseBath", _game.State.Ranch.BathtubClean ? T("world.house.bath", "Hot bath — tomorrow +{0} stamina", PlayerStaminaService.HotBathNextDayBonus) : T("world.house.shower", "Quick shower — no next-day bonus"),
                () => _game.UsePlayerBath().Message, _game.State.Player.BathedToday);
        if (_phase == DayPhase.Night)
        {
            foreach (var (id, text) in new[] { ("rest", T("world.house.plan.rest", "Rest — recover resident energy")), ("train", T("world.house.plan.train", "Training — one extra growth pass")), ("admin", T("world.house.plan.admin", "Admin — reduce workload")) })
                Action("HousePlan_" + id, text, () => _game.TrySelectNightAction(id, _generation, _day) ? T("world.house.selected", "Night plan selected.") : T("world.house.unchanged", "Plan unchanged."), _game.State.Calendar.NightAction == id);
            RenderSharedEvening();
            Action("HouseSleep", T("world.house.sleep", "Sleep and settle the day"), () =>
            {
                var generation = _generation;
                if (!_game.TryAdvanceTime(generation, _day, _phase)) return T("world.house.select_first", "Select a night plan first.");
                // The state notification closes this stale-day panel. Open only the matching report.
                if (_game.StateGeneration == generation)
                {
                    _world.OpenDedicatedService("report");
                    _world.RevealSharedEvening(generation, _day);
                }
                return T("world.house.new_day", "A new day begins.");
            }, !_game.HasNightPlan);
        }
        else _content.AddChild(Text(T("world.house.return", "Return at night to choose Rest, Training or Admin and settle the day.")));
        Service("clothing_list", T("world.service.wardrobe", "Wardrobe"));
        Service("room_assign", T("world.service.rooms", "Assign rooms"));
        Service("training", T("world.service.training", "Personal training"));
        foreach (var character in _game.Roster.Characters.Where(c => _game.Schedule.GetAssignment(c.Id) != "rest"))
        {
            var id = character.Id;
            Action("HouseRest_" + id, T("world.house.day_off", "Give {0} the day off", _game.Roster.DefinitionFor(character).DisplayName),
                () => _game.TryAssignJob(id, "rest", _generation) ? T("world.work.rest_success", "Rest assigned.") : T("world.work.unchanged", "Assignment unchanged."));
        }
    }

    private void RenderGuide()
    {
        if (_id == "projects") { RenderProjects(); return; }
        _title.Text = T("world.guide.title", "Places");
        Action("OpenProjects", T("project.journal.open", "Ranch projects — what could I do next?"), () => { Open("guide", "projects"); return ""; });
        _content.AddChild(Text(T("world.guide.help", "Choose one destination to mark. Follow its arrow, then interact there. This guide never assigns work, spends resources or teleports you.")));
        foreach (var station in _world.Ranch!.Stations.Where(s => s.RequiresWorker || s.TargetId is "ranch_house" or "pet_care"))
            Destination(new WorldDestination("ranch", station.TargetId, station.Label));
        foreach (var service in _world.Town!.Services)
            Destination(new WorldDestination("town", service.ServiceId, service.Label));
        Destination(new WorldDestination("ranch", RanchLeisureController.BoardId, "Community Board"));
        Destination(new WorldDestination("ranch", RanchLeisureController.CornerId, "Quiet corner"));
        Action("ClearDestination", T("world.guide.clear", "Clear the destination"), () => { _world.NavigationGuide?.ClearTarget(); return T("world.guide.cleared", "Destination cleared."); });
    }

    private void Destination(WorldDestination destination) => Action("Place_" + destination.TargetId,
        T("world.guide.place", "{0} • {1}", WorldName(destination.TargetId, destination.Label), WorldName(destination.AreaId, destination.AreaId == "town" ? "Town" : "Ranch")), () =>
        { _world.NavigationGuide?.Track(destination); Close(); return ""; });

    private void Service(string screen, string label) => Action("Service_" + screen, label, () =>
    {
        Close(); return _world.OpenDedicatedService(screen) ? "" : T("world.service.unavailable", "Service unavailable.");
    });

    private void Action(string name, string text, Func<string> command, bool disabled = false)
    {
        var revision = _revision;
        var button = new Button { Name = name, Text = text, Disabled = disabled, CustomMinimumSize = new Vector2(0, 40),
            AutowrapMode = TextServer.AutowrapMode.WordSmart, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _content.AddChild(button);
        button.Pressed += () =>
        {
            if (_busy || !IsVisibleInTree() || !ContextMatches() || _revision != revision
                || !GodotObject.IsInstanceValid(button) || button.Disabled || !_content.IsAncestorOf(button)) return;
            _busy = true;
            try
            {
                var message = command();
                if (Visible && ContextMatches()) { _status.Text = message; _status.TooltipText = message; _refreshPending = true; }
            }
            finally { _busy = false; }
        };
    }

    private static Label Text(string text) => new() { Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart,
        SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
}
