using System;
using System.Linq;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Compact HUD projection of WorldAlertEvaluator. Presentation only; no alert mutates gameplay.
/// </summary>
public partial class WorldAlertPanelController : PanelContainer
{
    private Label? _header;
    private Label? _body;

    public int AlertCount { get; private set; }
    public WorldAlertSeverity? HighestSeverity { get; private set; }

    public override void _Ready()
    {
        _header = GetNodeOrNull<Label>("Inner/Header");
        _body = GetNodeOrNull<Label>("Inner/Body");

        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged += Refresh;
        }

        Refresh();
    }

    public override void _ExitTree()
    {
        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        var alerts = WorldAlertEvaluator.Evaluate(GameRoot.Instance);
        AlertCount = alerts.Count;
        HighestSeverity = alerts.Count == 0 ? null : alerts.Max(alert => alert.Severity);

        Visible = alerts.Count > 0;
        if (!Visible)
        {
            return;
        }

        var top = alerts.Take(3).ToArray();
        if (_header is not null)
        {
            var prefix = HighestSeverity == WorldAlertSeverity.Critical ? "ATTENTION" : "RANCH CHECK";
            _header.Text = alerts.Count > 3 ? $"{prefix}  •  {alerts.Count} issues" : $"{prefix}  •  {alerts.Count}";
            _header.TooltipText = "These warnings are derived from existing ranch, resident, pet and economy state.";
            _header.AddThemeColorOverride("font_color", HighestSeverity switch
            {
                WorldAlertSeverity.Critical => new Color("ff8a8a"),
                WorldAlertSeverity.Warning => new Color("ffd27a"),
                _ => new Color("b9d8ff")
            });
        }

        if (_body is not null)
        {
            _body.Text = string.Join("\n", top.Select(alert =>
                $"{SeverityPrefix(alert.Severity)} {alert.Title}: {alert.Detail}"))
                + (alerts.Count > top.Length ? $"\n+{alerts.Count - top.Length} more — open Management [M]" : string.Empty);

            _body.TooltipText = string.Join("\n\n", alerts.Select(alert =>
                $"{alert.Title}\n{alert.Detail}\nSuggested: {alert.SuggestedScreen}"));
        }
    }

    private static string SeverityPrefix(WorldAlertSeverity severity) => severity switch
    {
        WorldAlertSeverity.Critical => "[!]",
        WorldAlertSeverity.Warning => "[~]",
        _ => "[i]"
    };
}
