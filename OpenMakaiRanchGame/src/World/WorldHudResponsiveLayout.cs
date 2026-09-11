using System.Linq;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>Presentation-only compact projections; full instructions and alerts remain in Help.</summary>
public static class WorldHudResponsiveLayout
{
    public static void Apply(CanvasLayer hud, ScreenLayoutMetrics m, bool ranch)
    {
        var compact = m.ContentWidth < 1100 || m.ViewportSize.Y < 620;
        var width = Mathf.Max(1, m.ContentWidth - 36);
        var left = m.ContentLeft + 18;
        var top = m.SafeTop;
        if (hud.GetNodeOrNull<Control>("GuidancePanel") is { } guidance) guidance.Visible = !compact;
        if (hud.GetNodeOrNull<Label>("TopBar/RosterLabel") is { } roster) roster.Visible = !compact;
        hud.GetNodeOrNull<WorldAlertPanelController>("AlertPanel")?.SetCompact(compact);
        var bar = hud.GetNode<Control>("TopBar");
        bar.OffsetBottom = top + (compact ? 104 : 56);
        foreach (var path in new[] { ranch ? "TopBar/DayLabel" : "TopBar/LocationLabel", "TopBar/EconomyLabel", "StatusLabel" })
        {
            var label = hud.GetNode<Label>(path);
            label.AutowrapMode = TextServer.AutowrapMode.Off;
            label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        }
        var hint = hud.GetNode<Control>("TutorialOverlay/HintCard");
        if (ranch)
        {
            hint.GetNode<Control>("Inner/BodyLabel").Visible = !compact;
            hint.GetNode<Control>("Inner/KeyLabel").Visible = !compact;
            var title = hint.GetNode<Label>("Inner/TitleLabel");
            title.AutowrapMode = compact ? TextServer.AutowrapMode.Off : TextServer.AutowrapMode.WordSmart;
            title.TextOverrunBehavior = compact ? TextServer.OverrunBehavior.TrimEllipsis : TextServer.OverrunBehavior.NoTrimming;
        }
        else
        {
            var label = hint.GetNode<Label>("Inner/HintLabel");
            label.MaxLinesVisible = compact ? 2 : -1;
        }
        if (compact)
        {
            SetRect(hud.GetNode<Control>(ranch ? "TopBar/DayLabel" : "TopBar/LocationLabel"), left, top + 8, width, 22);
            SetRect(hud.GetNode<Control>("TopBar/EconomyLabel"), left, top + 32, width, 24);
            if (ranch)
            {
                SetRect(hud.GetNode<Control>("ManagementButton"), m.ContentRight - 154, top + 64, 136, 36);
                SetRect(hud.GetNode<Control>("AdvanceTimeButton"), m.ContentRight - 304, top + 64, 138, 36);
            }
            else SetRect(hud.GetNode<Control>("ReturnRanchButton"), m.ContentRight - 170, top + 64, 152, 36);
            var alertWidth = Mathf.Min(280, width * 0.44f);
            SetRect(hud.GetNode<Control>("AlertPanel"), m.ContentRight - 18 - alertWidth, top + 110, alertWidth, 96);
            var otherWidth = width - alertWidth - 12;
            if (ranch)
            {
                var worker = hud.GetNode<Control>("WorkerPanel");
                SetRect(worker, left, top + 110, otherWidth, 64);
                foreach (var name in new[] { "WorkerLabel", "AssignmentLabel" })
                {
                    var label = worker.GetNode<Label>(name);
                    label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
                    label.AutowrapMode = TextServer.AutowrapMode.Off;
                    SetRect(label, 8, name == "WorkerLabel" ? 6 : 32, otherWidth - 16, 24);
                }
                SetRect(hint, left, top + 214, Mathf.Min(424, width), 100);
            }
            else SetRect(hint, left, top + 110, otherWidth, 96);
        }
        else if (ranch)
        {
            var worker = hud.GetNode<Control>("WorkerPanel");
            SetRect(worker.GetNode<Control>("WorkerLabel"), 12, 5, worker.Size.X - 24, 24);
            SetRect(worker.GetNode<Control>("AssignmentLabel"), 12, 31, worker.Size.X - 24, 24);
        }
        var help = hud.GetNode<ScrollContainer>("TutorialOverlay/HelpPanel");
        var helpWidth = Mathf.Min(720, m.ContentWidth - 32);
        var helpHeight = Mathf.Min(620, m.ViewportSize.Y - m.SafeTop - m.SafeBottom - 32);
        SetRect(help, m.HorizontalCenter - helpWidth / 2, top + (m.ViewportSize.Y - top - m.SafeBottom - helpHeight) / 2,
            helpWidth, helpHeight);
        help.FollowFocus = true;
        help.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        help.FocusMode = Control.FocusModeEnum.All;
        help.GetNode<Control>("Inner").SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        help.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = GameRoot.Instance.Theme.RootPanelFill,
            BorderColor = GameRoot.Instance.Theme.RootPanelBorder,
            BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
            ContentMarginLeft = 12, ContentMarginTop = 12, ContentMarginRight = 12, ContentMarginBottom = 12
        });
    }

    public static void PrepareHelp(ScrollContainer help, string currentHint)
    {
        var inner = help.GetNode<Control>("Inner");
        var details = inner.GetNodeOrNull<Label>("ContextDetails");
        if (details is null)
        {
            details = new Label { Name = "ContextDetails", AutowrapMode = TextServer.AutowrapMode.WordSmart,
                TextOverrunBehavior = TextServer.OverrunBehavior.NoTrimming };
            inner.AddChild(details);
            inner.MoveChild(details, 1);
        }
        var game = GameRoot.Instance;
        var player = game.State.Player;
        var resources = $"Stamina {player.Stamina}/{player.MaxStamina + player.DailyStaminaBonus} | MP {player.Mana}/{player.MaxMana} | Stored MP {game.State.Economy.ManaReservoir}";
        var alerts = WorldAlertEvaluator.Evaluate(game);
        details.Text = resources + (string.IsNullOrWhiteSpace(currentHint) ? "" : "\n\n" + currentHint)
            + (alerts.Count == 0 ? "" : "\n\nRanch status\n" + string.Join("\n\n", alerts.Select(alert =>
                $"{alert.Title}\n{alert.Detail}\nManagement: {alert.SuggestedScreen}")));
        help.ScrollVertical = 0;
    }

    private static void SetRect(Control control, float x, float y, float width, float height)
    {
        control.Position = new Vector2(x, y);
        control.Size = new Vector2(Mathf.Max(1, width), Mathf.Max(1, height));
    }
}
