using System;
using System.Collections.Generic;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Ui;

/// <summary>Pause-owned, scrollable view of the existing session. Only root commands mutate state.</summary>
public partial class RanchCornerPanel : PanelContainer
{
    private GameRoot? _game;
    private Func<bool>? _stillNear;
    private ulong _generation;
    private int _day;
    private DayPhase _phase;
    private string _partner = string.Empty;
    private bool _built;
    private Label _summary = null!;
    private Label _status = null!;
    private Label _restoreHint = null!;
    private Label _restHint = null!;
    private Label _shareHint = null!;
    private Button _restore = null!;
    private Button _rest = null!;
    private Button _share = null!;
    private Button _back = null!;
    private readonly List<Button> _buttons = new();

    public event Action? BackRequested;
    public event Action<string>? PlanningRequested;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        MouseFilter = MouseFilterEnum.Stop;
        BuildUi();
        Visible = false;
    }

    public void Open(GameRoot game, Func<bool> stillNear)
    {
        Close();
        BuildUi();
        _game = game;
        _stillNear = stillNear;
        _generation = game.StateGeneration;
        _day = game.State.Calendar.Day;
        _phase = game.State.Calendar.Phase;
        _partner = game.Dating.ActivePartnerId;
        _status.Text = string.Empty;
        game.StateChanged += OnStateChanged;
        Visible = true;
        Refresh();
        CallDeferred(nameof(FocusFirstAction));
    }

    public void Close()
    {
        if (_game is not null && GodotObject.IsInstanceValid(_game))
            _game.StateChanged -= OnStateChanged;
        _game = null;
        _stillNear = null;
        Visible = false;
    }

    public override void _ExitTree() => Close();

    private bool ContextMatches() => _game is not null && GodotObject.IsInstanceValid(_game)
        && _game.StateGeneration == _generation && _game.State.Calendar.Day == _day
        && _game.State.Calendar.Phase == _phase && _game.Dating.ActivePartnerId == _partner
        && !_game.CombatWorldTimeLocked && _game.ActiveCombatSession is null && _stillNear?.Invoke() == true;

    private void OnStateChanged()
    {
        if (!Visible) return;
        if (!ContextMatches())
        {
            BackRequested?.Invoke();
            Close();
            return;
        }
        Refresh();
    }

    private void Execute(string action)
    {
        if (!IsVisibleInTree() || !ContextMatches()) return;
        var game = _game!;
        string message;
        if (action == "restore") game.TryRestoreRanchCorner(_day, _phase, _generation, out message);
        else if (action == "rest") game.TryRestAtRanchCorner(_day, _phase, _generation, out message);
        else if (action == "share") game.TryShareRanchCorner(_partner, _day, _phase, _generation, out message);
        else return;
        // Notifications may have loaded a different session and closed this panel synchronously.
        if (!Visible || _game is null) return;
        _status.Text = message;
        Refresh();
    }

    public void Refresh()
    {
        if (_game is null || !_built) return;
        var status = _game.GetRanchCornerStatus();
        var weather = OriginalCalendarRules.IsRain(_game.State.Calendar.CurrentWeather)
            ? "Rain patters over the ranch."
            : OriginalCalendarRules.IsSnow(_game.State.Calendar.CurrentWeather)
                ? "Snow softens the familiar paths."
                : _phase == DayPhase.Night ? "The ranch has settled into the evening quiet." : "There is room for a quiet pause between the day's plans.";
        _summary.Text = $"{weather}\nDay {_day} · {_phase} · {status.Gold} G · {status.Supplies} supplies\n"
            + (status.Restored ? "Your restored bench is here to stay. No upkeep or daily obligation." : "A few weathered boards could become a small rest area. Restoring it is optional.");
        _restore.Text = $"Restore permanently — {RanchLeisureService.RestoreGoldCost} G + {RanchLeisureService.RestoreSupplyCost} supplies";
        _restore.Disabled = !status.CanRestore;
        _restoreHint.Text = status.CanRestore ? "Pays the displayed cost once. No stamina cost and no time advance." : status.RestoreReason;
        _rest.Text = $"Take a break — restore {status.RecoveryAmount} stamina";
        _rest.Disabled = !status.CanRest;
        _restHint.Text = status.CanRest ? "At most 10 stamina once per day, capped at today's capacity. A partial refill uses today's break. A full bar does not consume it." : status.RestReason;
        _share.Disabled = !_game.CanShareRanchCorner(out var shareReason);
        _share.Text = $"Share a quiet moment — {_game.Dating.ActivityCost(DateActivityKind.QuietRest)} stamina";
        var companion = _game.Roster.Find(_partner);
        var name = companion is null ? "Your companion" : _game.Roster.DefinitionFor(companion).DisplayName;
        _shareHint.Text = _share.Disabled ? shareReason
            : $"With {name}. Uses the existing Quiet Rest activity and its one-activity-per-phase limit. It does not also grant the solo recovery.";
        for (var i = 0; i < _buttons.Count; i++)
        {
            var previous = i;
            var next = i;
            do { previous = (previous + _buttons.Count - 1) % _buttons.Count; } while (_buttons[previous].Disabled);
            do { next = (next + 1) % _buttons.Count; } while (_buttons[next].Disabled);
            _buttons[i].FocusNeighborTop = _buttons[i].GetPathTo(_buttons[previous]);
            _buttons[i].FocusNeighborBottom = _buttons[i].GetPathTo(_buttons[next]);
            _buttons[i].FocusPrevious = _buttons[i].FocusNeighborTop;
            _buttons[i].FocusNext = _buttons[i].FocusNeighborBottom;
        }
        if (GetViewport()?.GuiGetFocusOwner() is Button { Disabled: true }) _back.GrabFocus();
    }

    private void BuildUi()
    {
        if (_built) return;
        _built = true;
        var margin = new MarginContainer { Name = "Margin" };
        foreach (var side in new[] { "left", "right", "top", "bottom" }) margin.AddThemeConstantOverride($"margin_{side}", 16);
        AddChild(margin);
        var layout = new VBoxContainer { Name = "Layout" };
        layout.AddThemeConstantOverride("separation", 12);
        margin.AddChild(layout);
        var title = Text("Quiet corner");
        title.AddThemeFontSizeOverride("font_size", 24);
        layout.AddChild(title);
        var scroll = new ScrollContainer
        {
            Name = "Scroll", SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, FollowFocus = true
        };
        layout.AddChild(scroll);
        var content = new VBoxContainer { Name = "Content", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        content.AddThemeConstantOverride("separation", 10);
        scroll.AddChild(content);
        _summary = Text(string.Empty);
        _status = Text(string.Empty);
        content.AddChild(_summary);
        content.AddChild(_status);
        _restore = Action(content, "Restore", () => Execute("restore"));
        _restoreHint = Text(string.Empty); content.AddChild(_restoreHint);
        _rest = Action(content, "Rest", () => Execute("rest"));
        _restHint = Text(string.Empty); content.AddChild(_restHint);
        _share = Action(content, "Share", () => Execute("share"));
        _shareHint = Text(string.Empty); content.AddChild(_shareHint);
        content.AddChild(Text("Solo recovery and companionship are separate choices. Walking is free. The prepared hot bath still gives its normal bonus on the next day. Skipping this corner has no penalty."));
        Action(content, "PlanSupplies", () => Plan("schedule")).Text = T("world.corner.find_supplies", "Find supply production");
        Action(content, "Residents", () => Plan("ranch")).Text = T("world.corner.find_places", "Find a place on the ranch");
        _back = Action(layout, "Back", () => { if (IsVisibleInTree()) BackRequested?.Invoke(); });
        _back.Text = "Back to the world";
    }

    private void Plan(string screen)
    {
        if (IsVisibleInTree() && ContextMatches()) PlanningRequested?.Invoke(screen);
    }

    private Button Action(Node parent, string name, Action callback)
    {
        var button = new Button
        {
            Name = name, CustomMinimumSize = new Vector2(0, 42), SizeFlagsHorizontal = SizeFlags.ExpandFill,
            FocusMode = FocusModeEnum.All, AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        parent.AddChild(button);
        button.Pressed += callback;
        _buttons.Add(button);
        return button;
    }

    private static Label Text(string text) => new()
    {
        Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart,
        SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore
    };

    private void FocusFirstAction()
    {
        if (!IsVisibleInTree()) return;
        foreach (var button in _buttons)
            if (!button.Disabled) { button.GrabFocus(); return; }
    }
}
