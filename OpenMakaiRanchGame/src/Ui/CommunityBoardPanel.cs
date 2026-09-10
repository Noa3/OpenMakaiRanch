using System;
using System.Collections.Generic;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.Ui;

/// <summary>Pause-owned presentation. Only GameRoot commands may deliver or pay rewards.</summary>
public partial class CommunityBoardPanel : PanelContainer
{
    private sealed class RequestRow
    {
        public string RequestId = string.Empty;
        public Label Title = null!;
        public Label Detail = null!;
        public Label Progress = null!;
        public Label Hint = null!;
        public Button Deliver = null!;
    }

    private readonly List<RequestRow> _rows = new();
    private Label? _summary;
    private Label? _status;
    private Button? _back;
    private Button? _plan;
    private GameRoot? _game;
    private ulong _generation;
    private int _day;
    private bool _built;

    public event Action? BackRequested;
    public event Action? PlanningRequested;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        MouseFilter = MouseFilterEnum.Stop;
        BuildUi();
        Visible = false;
    }

    public void Open(GameRoot game)
    {
        BuildUi();
        _game = game;
        if (_status is not null) _status.Text = string.Empty;
        Visible = true;
        Refresh();
        CallDeferred(nameof(FocusFirstAction));
    }

    public void Close()
    {
        Visible = false;
        _game = null;
    }

    private void BuildUi()
    {
        if (_built) return;
        _built = true;
        var margin = new MarginContainer { Name = "Margin" };
        foreach (var side in new[] { "left", "right", "top", "bottom" })
            margin.AddThemeConstantOverride($"margin_{side}", 16);
        AddChild(margin);
        var layout = new VBoxContainer { Name = "Layout" };
        layout.AddThemeConstantOverride("separation", 12);
        margin.AddChild(layout);

        var title = WrappedLabel("Community Board");
        title.AddThemeFontSizeOverride("font_size", 24);
        layout.AddChild(title);
        layout.AddChild(WrappedLabel("Optional courier orders: choose at most ONE delivery per day. No streaks, failure penalties or required trips. Orders rotate; skipping a day costs nothing."));
        _summary = WrappedLabel(string.Empty);
        layout.AddChild(_summary);
        _status = WrappedLabel(string.Empty);
        layout.AddChild(_status);

        var scroll = new ScrollContainer
        {
            Name = "OrdersScroll",
            SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            FollowFocus = true
        };
        layout.AddChild(scroll);
        var cards = new VBoxContainer { Name = "Orders", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        cards.AddThemeConstantOverride("separation", 16);
        scroll.AddChild(cards);
        for (var index = 0; index < 3; index++)
        {
            var card = new VBoxContainer { Name = $"Order{index}", SizeFlagsHorizontal = SizeFlags.ExpandFill };
            card.AddThemeConstantOverride("separation", 6);
            cards.AddChild(card);
            var row = new RequestRow
            {
                Title = WrappedLabel(string.Empty), Detail = WrappedLabel(string.Empty),
                Progress = WrappedLabel(string.Empty), Hint = WrappedLabel(string.Empty),
                Deliver = new Button { Name = "DeliverButton", Text = "Deliver", CustomMinimumSize = new Vector2(0, 40), FocusMode = FocusModeEnum.All }
            };
            row.Title.AddThemeFontSizeOverride("font_size", 19);
            card.AddChild(row.Title);
            card.AddChild(row.Detail);
            card.AddChild(row.Progress);
            card.AddChild(row.Hint);
            card.AddChild(row.Deliver);
            card.AddChild(new HSeparator());
            row.Deliver.Pressed += () => Deliver(row.RequestId);
            _rows.Add(row);
        }

        var footer = new HBoxContainer { Name = "Footer" };
        footer.AddThemeConstantOverride("separation", 12);
        layout.AddChild(footer);
        _plan = new Button { Name = "PlanButton", Text = "Plan ranch work", SizeFlagsHorizontal = SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(0, 40) };
        _back = new Button { Name = "BackButton", Text = "Back to pause", SizeFlagsHorizontal = SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(0, 40) };
        footer.AddChild(_plan);
        footer.AddChild(_back);
        _plan.Pressed += () => PlanningRequested?.Invoke();
        _back.Pressed += () => BackRequested?.Invoke();
    }

    private void Refresh()
    {
        var game = _game;
        if (game is null || !GodotObject.IsInstanceValid(game)) return;
        _generation = game.StateGeneration;
        _day = game.State.Calendar.Day;
        var offers = game.GetCommunityRequests();
        if (_summary is not null)
            _summary.Text = $"Day {_day}  |  {game.Economy.Gold:N0} G  |  Deliveries completed: {game.CompletedCommunityDeliveries}";
        for (var index = 0; index < _rows.Count; index++)
        {
            var row = _rows[index];
            if (index >= offers.Count) { row.Deliver.Disabled = true; continue; }
            var offer = offers[index];
            row.RequestId = offer.Id;
            row.Title.Text = offer.Title;
            row.Detail.Text = offer.Description;
            row.Progress.Text = $"{offer.ResourceName}: {offer.AvailableAmount}/{offer.RequiredAmount} required  |  Reward: {offer.RewardGold} G";
            row.Hint.Text = offer.CanDeliver
                ? $"Ready. Delivery consumes {offer.RequiredAmount}; {offer.AvailableAmount - offer.RequiredAmount} will remain in the ranch stockpile."
                : offer.UnavailableReason;
            row.Deliver.Text = $"Deliver for {offer.RewardGold} G";
            row.Deliver.Disabled = !offer.CanDeliver || game.CombatWorldTimeLocked || game.ActiveCombatSession is not null;
            row.Deliver.TooltipText = offer.ProductionHint;
            if (game.CombatWorldTimeLocked || game.ActiveCombatSession is not null)
                row.Hint.Text = "Finish the encounter before arranging a delivery.";
        }
        LinkFocus();
    }

    private void Deliver(string requestId)
    {
        var game = _game;
        if (game is null || !GodotObject.IsInstanceValid(game)) return;
        var delivered = game.TryDeliverCommunityRequest(requestId, _day, _generation, out var message);
        if (_status is not null) _status.Text = message;
        if (delivered) game.Feedback.PlayConfirm();
        Refresh();
        CallDeferred(nameof(FocusFirstAction));
    }

    private void LinkFocus()
    {
        var actions = new List<Button>();
        foreach (var row in _rows)
            if (!row.Deliver.Disabled) actions.Add(row.Deliver);
        if (_plan is not null) actions.Add(_plan);
        if (_back is not null) actions.Add(_back);
        for (var index = 0; index < actions.Count; index++)
        {
            var button = actions[index];
            var previous = actions[(index + actions.Count - 1) % actions.Count];
            var next = actions[(index + 1) % actions.Count];
            button.FocusNeighborTop = button.GetPathTo(previous);
            button.FocusNeighborBottom = button.GetPathTo(next);
            button.FocusPrevious = button.GetPathTo(previous);
            button.FocusNext = button.GetPathTo(next);
        }
    }

    private void FocusFirstAction()
    {
        if (!IsVisibleInTree()) return;
        foreach (var row in _rows)
        {
            if (row.Deliver.Disabled) continue;
            row.Deliver.GrabFocus();
            return;
        }
        _back?.GrabFocus();
    }

    private static Label WrappedLabel(string text) => new()
    {
        Text = text,
        AutowrapMode = TextServer.AutowrapMode.WordSmart,
        CustomMinimumSize = new Vector2(0, 24),
        SizeFlagsHorizontal = SizeFlags.ExpandFill,
        MouseFilter = MouseFilterEnum.Ignore
    };
}
