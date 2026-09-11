using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static OpenMakaiRanch.Locale.LocaleCatalog;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>At most three short alerts, with camera-relative directions. Clicking only marks a place.</summary>
public partial class WorldAlertPanelController : PanelContainer
{
    private Label? _header;
    private VBoxContainer? _rows;
    private bool _compact;
    private double _elapsed;
    private readonly List<(Button Button, WorldAlert Alert)> _buttons = new();
    public int AlertCount { get; private set; }
    public WorldAlertSeverity? HighestSeverity { get; private set; }

    private WorldGameController? Host()
    {
        for (Node? node = this; node is not null; node = node.GetParent())
            if (node is WorldGameController world) return world;
        return null;
    }

    public void SetCompact(bool compact)
    {
        if (_compact == compact) return;
        _compact = compact; Refresh();
    }

    public override void _Ready()
    {
        _header = GetNodeOrNull<Label>("Inner/Header");
        if (GetNodeOrNull<Label>("Inner/Body") is { } body) body.Visible = false;
        _rows = new VBoxContainer { Name = "AlertDirections" };
        _rows.AddThemeConstantOverride("separation", 3);
        GetNode("Inner").AddChild(_rows);
        GameRoot.Instance.StateChanged += Refresh;
        Refresh();
    }

    public override void _ExitTree()
    {
        if (GodotObject.IsInstanceValid(GameRoot.Instance)) GameRoot.Instance.StateChanged -= Refresh;
    }

    public override void _Process(double delta)
    {
        _elapsed += delta;
        if (_elapsed < 0.1 || !IsVisibleInTree()) return;
        _elapsed = 0;
        var world = Host();
        foreach (var (button, alert) in _buttons)
        {
            var destination = WorldDestinationCatalog.ForAlert(alert);
            button.Text = $"{world?.NavigationGuide?.Direction(destination) ?? "→"}  {alert.Title}";
            button.Disabled = world?.WorldActionsAvailable != true || world.IsGuidedOpening;
        }
    }

    public void Refresh()
    {
        var alerts = WorldAlertEvaluator.Evaluate(GameRoot.Instance);
        AlertCount = alerts.Count;
        HighestSeverity = alerts.Count == 0 ? null : alerts.Max(alert => alert.Severity);
        Visible = alerts.Count > 0;
        if (_rows is null) return;
        foreach (var node in _rows.GetChildren()) { _rows.RemoveChild(node); node.QueueFree(); }
        _buttons.Clear();
        if (_header is not null)
        {
            _header.Text = HighestSeverity == WorldAlertSeverity.Critical ? T("world.alert.header.critical", "ATTENTION • {0}", alerts.Count) : T("world.alert.header", "RANCH CHECK • {0}", alerts.Count);
            _header.TooltipText = T("world.alert.help", "Select an issue to mark its station. Places [{0}] lists destinations. Arrows show direction, not an obstacle-free route.", InputBindingService.GetKeyboardLabel("toggle_management"));
            _header.AddThemeColorOverride("font_color", HighestSeverity switch
            { WorldAlertSeverity.Critical => new Color("ff8a8a"), WorldAlertSeverity.Warning => new Color("ffd27a"), _ => new Color("b9d8ff") });
        }
        var generation = GameRoot.Instance.StateGeneration;
        foreach (var alert in alerts.Take(_compact ? 1 : 3))
        {
            var destination = WorldDestinationCatalog.ForAlert(alert);
            var button = new Button { Name = "RanchAlert_" + alert.Id, Text = $"→ {alert.Title}",
                Alignment = HorizontalAlignment.Left, ClipText = true,
                TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
                CustomMinimumSize = new Vector2(0, 32),
                TooltipText = T("world.alert.tip", "{0}\n{1}\nMark: {2} ({3}). This does not perform an action remotely.", alert.Title, alert.Detail, destination.DisplayName, WorldName(destination.AreaId, destination.AreaId)) };
            _rows.AddChild(button); _buttons.Add((button, alert));
            button.Pressed += () =>
            {
                var world = Host();
                if (!IsVisibleInTree() || button.Disabled || !_rows.IsAncestorOf(button)
                    || world?.WorldActionsAvailable != true || world.IsGuidedOpening
                    || GameRoot.Instance.StateGeneration != generation
                    || !WorldAlertEvaluator.Evaluate(GameRoot.Instance).Any(a => a.Id == alert.Id)) return;
                world.NavigationGuide?.TrackAlert(alert);
            };
        }
    }
}
