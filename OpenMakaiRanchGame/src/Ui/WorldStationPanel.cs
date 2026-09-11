using System;
using System.Linq;
using Godot;
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
        _title = Text(""); _title.AddThemeFontSizeOverride("font_size", 25); layout.AddChild(_title);
        _scroll = new ScrollContainer { Name = "StationScroll", SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, FollowFocus = true };
        layout.AddChild(_scroll);
        _content = new VBoxContainer { Name = "StationContent", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _content.AddThemeConstantOverride("separation", 10); _scroll.AddChild(_content);
        _status = Text(""); _status.MaxLinesVisible = 2; layout.AddChild(_status);
        _close = new Button { Name = "StationClose", Text = "Back to the world", CustomMinimumSize = new Vector2(0, 40) };
        _close.Pressed += Close; layout.AddChild(_close);
        Visible = false;
    }

    public void Bind(WorldGameController world)
    {
        _world = world; _game = GameRoot.Instance;
        _game.StateChanged += OnStateChanged;
    }

    public override void _ExitTree()
    {
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

    private void Render()
    {
        _revision++;
        foreach (var node in _content.GetChildren()) { _content.RemoveChild(node); node.QueueFree(); }
        if (_kind == "guide") { RenderGuide(); return; }
        if (_kind == "resident") { RenderResident(); return; }
        var station = _world.ResolveStation(_id);
        if (station is null) { Close(); return; }
        _title.Text = station.Label;
        if (_id == "ranch_house") { RenderHouse(); return; }
        if (_id == "pet_care")
        {
            _content.AddChild(Text("Check the needs of your adopted pets here."));
            Service("pets", "Care for pets"); return;
        }
        var alerts = WorldAlertEvaluator.Evaluate(_game).Where(a => WorldDestinationCatalog.ForAlert(a).TargetId == _id).ToArray();
        foreach (var alert in alerts) _content.AddChild(Text($"{(alert.Severity == WorldAlertSeverity.Info ? "Info" : "Attention")}: {alert.Detail}"));
        if (_game.Data.Jobs.TryGetValue(station.CommandTargetId, out var job))
        {
            _content.AddChild(Text($"{job.DisplayName} • output is settled at the end of the day, not on assignment."));
            _content.AddChild(Text($"Base output: {job.GoldIncome} G / {job.ResourceAmount} {job.ResourceId}. Fatigue change: {job.FatigueDelta:+0;-0;0}. Facility and resident modifiers still apply."));
            var available = station.IsAvailable && _game.Schedule.AssignableJobs.Any(j => j.Id == job.Id);
            if (!available) _content.AddChild(Text(station.UnavailableReason ?? "This job is not available yet."));
            foreach (var character in _game.Roster.Characters)
            {
                var id = character.Id;
                var name = string.IsNullOrWhiteSpace(character.DisplayNameOverride) ? _game.Roster.DefinitionFor(character).DisplayName : character.DisplayNameOverride;
                var currentJob = _game.Schedule.GetAssignment(id);
                var currentName = _game.Data.Jobs.TryGetValue(currentJob, out var current) ? current.DisplayName : currentJob;
                _content.AddChild(Text($"{name} • {currentName} • Energy {character.Energy} • Fatigue {character.Fatigue}"));
                Action($"Assign_{id}", currentJob == job.Id ? $"{name} works here" : $"Assign {name} here", () =>
                {
                    var live = _world.ResolveStation(_id);
                    if (live?.IsAvailable != true || !_game.Schedule.AssignableJobs.Any(j => j.Id == job.Id)) return "This station is unavailable.";
                    var ok = live.Activate(new WorldInteractionContext(id, _generation));
                    if (ok)
                    {
                        if (_world.IsGuidedOpening) Close();
                        _world.Ranch?.NotifyStationAssignment(id, job.Id);
                    }
                    return ok ? $"{name} assigned. Production remains part of the daily settlement." : "Assignment unchanged.";
                }, !available || currentJob == job.Id);
                if (currentJob == job.Id)
                    Action($"Rest_{id}", $"Send {name} to rest", () => _game.TryAssignJob(id, "rest", _generation) ? "Rest assigned." : "Assignment unchanged.");
            }
        }
        AddFacility(station.RequiredFacilityId);
        if (_id == "office")
        {
            Service("inventory", "Storage and equipment");
            Service("milk", "Shipments");
            Service("report", "Last daily report");
        }
        if (_id == "workshop") Service("research", "Workshop research");
        if (_id == "pharmacy_lab") Service("pharmacy_list", "Pharmacy recipes");
    }

    private void AddFacility(string id)
    {
        if (!_game.Data.Facilities.TryGetValue(id, out var facility) || facility.BuildCost <= 0) return;
        var level = _game.Ranch.Facilities.TryGetValue(id, out var value) ? value : 0;
        var cost = _game.Ranch.FacilityUpgradeCost(facility, level);
        _content.AddChild(Text($"Facility level {level} • upkeep {facility.UpkeepGold} G/day. Wallet: {_game.Economy.Gold} G."));
        Action("FacilityUpgrade", $"{(level == 0 ? "Build" : "Upgrade")} this facility — {cost} G", () =>
        {
            if (!_game.Ranch.UpgradeFacility(id, _game.Economy)) return "The upgrade requirements are not met.";
            _game.NotifyStateChanged(); return "Facility upgraded.";
        }, _game.Economy.Gold < cost);
    }

    private void RenderResident()
    {
        var character = _game.Roster.Find(_id);
        if (character is null) { Close(); return; }
        var name = string.IsNullOrWhiteSpace(character.DisplayNameOverride) ? _game.Roster.DefinitionFor(character).DisplayName : character.DisplayNameOverride;
        _title.Text = name;
        _content.AddChild(Text($"Energy {character.Energy} • Fatigue {character.Fatigue} • Morale {character.Morale} • Bond {character.Bond}"));
        _content.AddChild(Text("A conversation here stays with this resident. Work is assigned at the relevant station."));
        var care = _game.PlayerStaminaCost(PlayerActivityKind.VisitCare);
        var feed = _game.PlayerStaminaCost(PlayerActivityKind.VisitFeed);
        Action("ResidentTalk", $"Talk — {care} stamina", () => _game.TryVisitCare(_id, "talk"), !_game.CanSpendPlayerStamina(PlayerActivityKind.VisitCare));
        Action("ResidentFeed", $"Offer a meal box — {feed} stamina", () => _game.TryVisitCare(_id, "feed"),
            !_game.CanSpendPlayerStamina(PlayerActivityKind.VisitFeed) || !_game.State.Inventory.Items.TryGetValue("meal_box", out var meals) || meals < 1);
        Action("ResidentRest", "Give the day off", () => _game.TryAssignJob(_id, "rest", _generation) ? "Rest assigned. No production was paid early." : "Already resting.",
            _game.Schedule.GetAssignment(_id) == "rest");
    }

    private void RenderHouse()
    {
        _content.AddChild(Text($"Day {_day} • {_phase} • Stamina {_game.State.Player.Stamina}/{_game.State.Player.MaxStamina + _game.State.Player.DailyStaminaBonus}"));
        _content.AddChild(Text("Night choices are revisable until you sleep. A prepared hot bath grants its extra energy tomorrow, separately from tonight's plan."));
        if (_phase is DayPhase.Evening or DayPhase.Night)
            Action("HouseBath", _game.State.Ranch.BathtubClean ? $"Hot bath — tomorrow +{PlayerStaminaService.HotBathNextDayBonus} stamina" : "Quick shower — no next-day bonus",
                () => _game.UsePlayerBath().Message, _game.State.Player.BathedToday);
        if (_phase == DayPhase.Night)
        {
            foreach (var (id, text) in new[] { ("rest", "Rest — recover resident energy"), ("train", "Training — one extra growth pass"), ("admin", "Admin — reduce workload") })
                Action("HousePlan_" + id, text, () => _game.TrySelectNightAction(id, _generation, _day) ? "Night plan selected." : "Plan unchanged.", _game.State.Calendar.NightAction == id);
            Action("HouseSleep", "Sleep and settle the day", () =>
            {
                var generation = _generation;
                if (!_game.TryAdvanceTime(generation, _day, _phase)) return "Select a night plan first.";
                // The state notification closes this stale-day panel. Open only the matching report.
                if (_game.StateGeneration == generation) _world.OpenDedicatedService("report");
                return "A new day begins.";
            }, !_game.HasNightPlan);
        }
        else _content.AddChild(Text("Return at night to choose Rest, Training or Admin and settle the day."));
        foreach (var character in _game.Roster.Characters.Where(c => _game.Schedule.GetAssignment(c.Id) != "rest"))
        {
            var id = character.Id;
            Action("HouseRest_" + id, $"Give {_game.Roster.DefinitionFor(character).DisplayName} the day off",
                () => _game.TryAssignJob(id, "rest", _generation) ? "Rest assigned." : "Assignment unchanged.");
        }
    }

    private void RenderGuide()
    {
        _title.Text = "Places";
        _content.AddChild(Text("Choose one destination to mark. Follow its arrow, then interact there. This guide never assigns work, spends resources or teleports you."));
        foreach (var station in _world.Ranch!.Stations.Where(s => s.RequiresWorker))
            Destination(new WorldDestination("ranch", station.TargetId, station.Label));
        foreach (var service in _world.Town!.Services)
            Destination(new WorldDestination("town", service.ServiceId, service.Label));
        Destination(new WorldDestination("ranch", RanchLeisureController.BoardId, "Community Board"));
        Destination(new WorldDestination("ranch", RanchLeisureController.CornerId, "Quiet corner"));
        Action("ClearDestination", "Clear the destination", () => { _world.NavigationGuide?.ClearTarget(); return "Destination cleared."; });
    }

    private void Destination(WorldDestination destination) => Action("Place_" + destination.TargetId,
        $"{destination.Label} • {(destination.AreaId == "town" ? "Town" : "Ranch")}", () =>
        { _world.NavigationGuide?.Track(destination); Close(); return ""; });

    private void Service(string screen, string label) => Action("Service_" + screen, label, () =>
    {
        Close(); return _world.OpenDedicatedService(screen) ? "" : "Service unavailable.";
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
