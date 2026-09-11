using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static OpenMakaiRanch.Locale.LocaleCatalog;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>Bounded warning markers and one opt-in destination, derived from the shared simulation.</summary>
public partial class WorldNavigationController : Node
{
    public WorldGameController World { get; set; } = null!;
    public WorldDestination? Target { get; private set; }
    private readonly Dictionary<string, Label3D> _markers = new(StringComparer.Ordinal);
    private Label _waypoint = null!;
    private CanvasLayer _layer = null!;
    private ulong _generation;
    private string? _trackedAlertId;
    private double _timer;
    public int MarkerCount => _markers.Count;

    public override void _Ready()
    {
        _generation = GameRoot.Instance.StateGeneration;
        _layer = new CanvasLayer { Name = "WayfindingLayer", Layer = 6 };
        AddChild(_layer);
        _waypoint = new Label { Name = "TrackedDestination", Visible = false, MouseFilter = Control.MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new Vector2(180, 48) };
        _waypoint.AddThemeColorOverride("font_color", new Color("fff3c7"));
        _waypoint.AddThemeStyleboxOverride("normal", new StyleBoxFlat { BgColor = new Color(0.03f, 0.06f, 0.09f, 0.92f),
            CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8, CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
            ContentMarginLeft = 10, ContentMarginRight = 10, ContentMarginTop = 6, ContentMarginBottom = 6 });
        _layer.AddChild(_waypoint);
        GameRoot.Instance.StateChanged += Refresh;
        Refresh();
    }

    public override void _ExitTree()
    {
        if (GodotObject.IsInstanceValid(GameRoot.Instance)) GameRoot.Instance.StateChanged -= Refresh;
        foreach (var marker in _markers.Values) if (GodotObject.IsInstanceValid(marker)) marker.QueueFree();
        _markers.Clear();
    }

    public void Track(WorldDestination destination) { _trackedAlertId = null; Target = destination; UpdatePositions(); }
    public void TrackAlert(WorldAlert alert) { Track(WorldDestinationCatalog.ForAlert(alert)); _trackedAlertId = alert.Id; }
    public void ClearTarget() { _trackedAlertId = null; Target = null; _waypoint.Visible = false; }

    public override void _Process(double delta)
    {
        _timer += delta;
        if (_timer < 0.08) return;
        _timer = 0;
        UpdatePositions();
    }

    public string Direction(WorldDestination destination)
    {
        var point = World.ResolveDestination(destination, out var viaGate);
        if (point is null || !point.IsInsideTree() || World.ActivePlayer is not { } player) return "—";
        var camera = GetViewport().GetCamera3D();
        var arrow = camera is null ? "" : WorldDestinationCatalog.DirectionArrow(player.GlobalPosition, point.GlobalPosition, camera.GlobalBasis);
        var distance = new Vector2(point.GlobalPosition.X - player.GlobalPosition.X, point.GlobalPosition.Z - player.GlobalPosition.Z).Length();
        return viaGate ? T("world.direction.gate", "{0} {1:0} m · gate", arrow, distance) : T("world.direction.distance", "{0} {1:0} m", arrow, distance);
    }

    public void Refresh()
    {
        if (!GodotObject.IsInstanceValid(World) || World.Ranch is null) return;
        var game = GameRoot.Instance;
        if (_generation != game.StateGeneration) { _generation = game.StateGeneration; ClearTarget(); }
        var alerts = WorldAlertEvaluator.Evaluate(game);
        if (_trackedAlertId is not null && !alerts.Any(a => a.Id == _trackedAlertId)) ClearTarget();
        var groups = alerts.GroupBy(alert => WorldDestinationCatalog.ForAlert(alert).TargetId).ToArray();
        var active = new HashSet<string>(StringComparer.Ordinal);
        foreach (var group in groups.Take(12))
        {
            var alert = group.OrderByDescending(a => a.Severity).First();
            var destination = WorldDestinationCatalog.ForAlert(alert);
            Node3D? node = destination.AreaId == "ranch" ? World.ResolveStation(destination.TargetId)
                : World.Town?.Services.FirstOrDefault(s => s.ServiceId == destination.TargetId);
            if (node is null) continue;
            active.Add(group.Key);
            if (!_markers.TryGetValue(group.Key, out var marker) || !GodotObject.IsInstanceValid(marker))
            {
                marker = new Label3D { Name = "StationWarning", Position = new Vector3(0, 4.2f, 0),
                    Billboard = BaseMaterial3D.BillboardModeEnum.Enabled, FontSize = 64, OutlineSize = 10,
                    PixelSize = 0.012f, NoDepthTest = false };
                node.AddChild(marker); _markers[group.Key] = marker;
            }
            marker.Text = (alert.Severity == WorldAlertSeverity.Info ? "i" : "!") + (group.Count() > 1 ? $" {group.Count()}" : "");
            marker.Modulate = alert.Severity switch { WorldAlertSeverity.Critical => new Color("ff9191"),
                WorldAlertSeverity.Warning => new Color("ffd881"), _ => new Color("bfdfff") };
        }
        foreach (var id in _markers.Keys.Where(id => !active.Contains(id)).ToArray())
        { _markers[id].QueueFree(); _markers.Remove(id); }
        UpdatePositions();
    }

    private void UpdatePositions()
    {
        var show = World.ActiveAreaId is "ranch" or "town" && !World.IsManagementVisible
            && !World.FlowLocksUi && World.PauseMenu?.IsOpen != true && World.FirstDayFlow?.BlocksWorldInput != true;
        foreach (var marker in _markers.Values) if (GodotObject.IsInstanceValid(marker)) marker.Visible = show;
        _waypoint.Visible = false;
        if (!show || Target is null) return;
        var point = World.ResolveDestination(Target, out var viaGate);
        var camera = GetViewport().GetCamera3D();
        if (point is null || !point.IsInsideTree() || camera is null) return;
        var viewport = GetViewport().GetVisibleRect().Size;
        var width = Mathf.Min(240, viewport.X - 32);
        var position = point.GlobalPosition + new Vector3(0, 2.8f, 0);
        var projected = camera.UnprojectPosition(position);
        // A behind-camera projection is not a visible target. Put a directional label on a safe edge.
        if (camera.IsPositionBehind(position))
        {
            var local = camera.GlobalTransform.AffineInverse() * position;
            projected = new Vector2(local.X < 0 ? 20 : viewport.X - width - 20, viewport.Y - 138);
        }
        else projected -= new Vector2(width / 2, 24);
        if (!projected.IsFinite()) return;
        _waypoint.Position = new Vector2(Mathf.Clamp(projected.X, 16, Mathf.Max(16, viewport.X - width - 16)),
            Mathf.Clamp(projected.Y, 156, Mathf.Max(156, viewport.Y - 138)));
        _waypoint.Size = new Vector2(width, 52);
        _waypoint.Text = viaGate ? T("world.direction.other_area", "{0} · {1}\nFollow the gate to the other area", Direction(Target), Target.DisplayName) : T("world.direction.target", "{0} · {1}", Direction(Target), Target.DisplayName);
        _waypoint.Visible = true;
    }
}
