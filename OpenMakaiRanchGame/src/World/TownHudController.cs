using System;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Lightweight HUD for Okachi Town. It presents shared economy/location state and contextual prompts
/// but owns no shop/research/adventure logic.
/// </summary>
public partial class TownHudController : CanvasLayer
{
    private Label? _locationLabel;
    private Label? _economyLabel;
    private Label? _promptLabel;
    private Label? _statusLabel;
    private Label? _guidanceLabel;
    private Button? _returnRanchButton;
    private Control? _guidancePanel;
    private Control? _alertPanel;
    private Control? _tutorialHintCard;
    private Control? _tutorialHelpButton;
    private Vector2 _lastViewportSize = Vector2.Zero;
    private double _statusRemaining;

    public override void _Ready()
    {
        _locationLabel = GetNodeOrNull<Label>("TopBar/LocationLabel");
        _economyLabel = GetNodeOrNull<Label>("TopBar/EconomyLabel");
        _promptLabel = GetNodeOrNull<Label>("Prompt");
        _statusLabel = GetNodeOrNull<Label>("StatusLabel");
        _guidanceLabel = GetNodeOrNull<Label>("GuidancePanel/GuidanceLabel");
        _returnRanchButton = GetNodeOrNull<Button>("ReturnRanchButton");
        _guidancePanel = GetNodeOrNull<Control>("GuidancePanel");
        _alertPanel = GetNodeOrNull<Control>("AlertPanel");
        _tutorialHintCard = GetNodeOrNull<Control>("TutorialOverlay/HintCard");
        _tutorialHelpButton = GetNodeOrNull<Control>("TutorialOverlay/HelpButton");
        ApplyResponsiveLayout(force: true);
        Refresh(GameRoot.Instance);
    }

    public override void _Process(double delta)
    {
        ApplyResponsiveLayout();
        if (_statusRemaining > 0)
        {
            _statusRemaining -= delta;
            if (_statusRemaining <= 0 && _statusLabel is not null)
            {
                _statusLabel.Text = string.Empty;
            }
        }
    }

    private void ApplyResponsiveLayout(bool force = false)
    {
        var viewport = GetViewport();
        if (viewport is null)
        {
            return;
        }

        var metrics = ScreenLayout.Calculate(viewport);
        if (!force && metrics.ViewportSize.IsEqualApprox(_lastViewportSize))
        {
            return;
        }
        _lastViewportSize = metrics.ViewportSize;

        var left = metrics.ContentLeft + 18f;
        var rightInset = metrics.ViewportSize.X - metrics.ContentRight + 18f;
        var top = metrics.SafeTop;

        if (_locationLabel is not null)
        {
            _locationLabel.OffsetLeft = left;
            _locationLabel.OffsetRight = left + Mathf.Min(760f, metrics.ContentWidth * 0.62f);
            _locationLabel.OffsetTop = top + 8f;
            _locationLabel.OffsetBottom = top + 30f;
        }
        if (_economyLabel is not null)
        {
            _economyLabel.OffsetLeft = left;
            _economyLabel.OffsetRight = left + Mathf.Min(760f, metrics.ContentWidth * 0.62f);
            _economyLabel.OffsetTop = top + 31f;
            _economyLabel.OffsetBottom = top + 53f;
        }

        if (_returnRanchButton is not null)
        {
            _returnRanchButton.OffsetRight = -rightInset;
            _returnRanchButton.OffsetLeft = -rightInset - 152f;
            _returnRanchButton.OffsetTop = top + 14f;
            _returnRanchButton.OffsetBottom = top + 46f;
        }

        var leftPanelWidth = Mathf.Min(456f, Mathf.Max(320f, metrics.ContentWidth * 0.44f));
        if (_guidancePanel is not null)
        {
            _guidancePanel.OffsetLeft = left;
            _guidancePanel.OffsetRight = left + leftPanelWidth;
            _guidancePanel.OffsetTop = top + 72f;
            _guidancePanel.OffsetBottom = top + 124f;
        }
        if (_tutorialHintCard is not null)
        {
            _tutorialHintCard.OffsetLeft = left;
            _tutorialHintCard.OffsetRight = left + leftPanelWidth;
            _tutorialHintCard.OffsetTop = top + 138f;
            _tutorialHintCard.OffsetBottom = top + 252f;
        }

        var alertWidth = Mathf.Min(452f, Mathf.Max(330f, metrics.ContentWidth * 0.40f));
        if (_alertPanel is not null)
        {
            _alertPanel.OffsetRight = -rightInset;
            _alertPanel.OffsetLeft = -rightInset - alertWidth;
            _alertPanel.OffsetTop = top + 62f;
            _alertPanel.OffsetBottom = top + 196f;
        }

        if (_promptLabel is not null)
        {
            var promptHalf = Mathf.Min(540f, metrics.ContentWidth * 0.42f);
            var shift = metrics.HorizontalCenter - metrics.ViewportSize.X * 0.5f;
            _promptLabel.OffsetLeft = shift - promptHalf;
            _promptLabel.OffsetRight = shift + promptHalf;
            _promptLabel.OffsetTop = -68f - metrics.SafeBottom;
            _promptLabel.OffsetBottom = -34f - metrics.SafeBottom;
        }
        if (_statusLabel is not null)
        {
            _statusLabel.OffsetLeft = left;
            _statusLabel.OffsetRight = Mathf.Min(metrics.ContentRight - 18f, left + 820f);
            _statusLabel.OffsetTop = -38f - metrics.SafeBottom;
            _statusLabel.OffsetBottom = -12f - metrics.SafeBottom;
        }
        if (_tutorialHelpButton is not null)
        {
            _tutorialHelpButton.OffsetRight = -rightInset;
            _tutorialHelpButton.OffsetLeft = -rightInset - 56f;
            _tutorialHelpButton.OffsetTop = -72f - metrics.SafeBottom;
            _tutorialHelpButton.OffsetBottom = -18f - metrics.SafeBottom;
        }
        WorldHudResponsiveLayout.Apply(this, metrics, ranch: false);
    }

    public void Refresh(GameRoot? game)
    {
        if (game is null || !GodotObject.IsInstanceValid(game))
        {
            return;
        }

        var calendar = game.State.Calendar;
        var economy = game.State.Economy;

        if (_locationLabel is not null)
        {
            _locationLabel.Text = $"Okachi Town  •  Day {calendar.Day}  •  {calendar.Phase}";
        }

        if (_economyLabel is not null)
        {
            var player = game.State.Player;
            var staminaCapacity = player.MaxStamina + player.DailyStaminaBonus;
            var rested = player.DailyStaminaBonus > 0 ? $"(+{player.DailyStaminaBonus} Rested)" : string.Empty;
            _economyLabel.Text = $"{economy.Gold:N0} G   STA {player.Stamina}/{staminaCapacity}{rested}   MP {player.Mana:N0}/{player.MaxMana:N0}   Stored {economy.ManaReservoir:N0}";
        }

        if (_guidanceLabel is not null)
        {
            var supplies = game.State.Ranch.Stockpile.TryGetValue("supplies", out var supplyCount) ? supplyCount : 0;
            var workshopBuilt = game.Ranch.Facilities.TryGetValue("workshop", out var workshopLevel) && workshopLevel > 0;

            if (calendar.Phase == OpenMakaiRanch.Core.Models.DayPhase.Night)
            {
                _guidanceLabel.Text = "Town is still usable, but return to the ranch when you are ready to plan or end the night.";
            }
            else if (supplies <= 1)
            {
                _guidanceLabel.Text = "Suggested errand: supplies are low — visit the General Store.";
            }
            else if (!workshopBuilt)
            {
                _guidanceLabel.Text = "Suggested errand: use Construction & Planning to build the Workshop and unlock Research Office access.";
            }
            else if (!game.Discovery.AllDiscovered)
            {
                _guidanceLabel.Text = "Suggested errand: the Adventure Guild can scout for undiscovered missions.";
            }
            else
            {
                _guidanceLabel.Text = "Explore services, shop, prepare missions, research, recruit, review milestones, or return to the ranch.";
            }

            _guidanceLabel.TooltipText = "Suggestions are optional. Town actions still execute through the shared GameRoot services.";
        }
    }

    public void SetServicePrompt(TownServicePoint? service, float distance, float range)
    {
        if (_promptLabel is null)
        {
            return;
        }

        if (service is null)
        {
            _promptLabel.Text = "WASD / arrows: move   •   Shift: sprint   •   RMB: look   •   F: enter service   •   F1: help";
            _promptLabel.TooltipText = "Walk up to a town building and press F. Return to the ranch through the south gate.";
            return;
        }

        if (distance <= range)
        {
            _promptLabel.Text = service.IsAvailable
                ? $"[F] Enter {service.Label}"
                : $"{service.Label} — {service.UnavailableReason}";
            _promptLabel.TooltipText = service.IsAvailable
                ? service.Description
                : service.UnavailableReason;
        }
        else
        {
            _promptLabel.Text = $"{service.Label}  {distance:0.0} m   •   move closer";
            _promptLabel.TooltipText = service.Description;
        }
    }

    public void SetCompanionPrompt(string displayName, float distance, float range)
    {
        if (_promptLabel is null)
        {
            return;
        }

        _promptLabel.Text = distance <= range
            ? $"[F] Talk with {displayName}"
            : $"{displayName}  {distance:0.0} m";
        _promptLabel.TooltipText = "Open this companion's Personal Time view for relationship details and shared activities.";
    }

    public void SetTravelPrompt(WorldTravelPortal? portal, float distance, float range)
    {
        if (_promptLabel is null)
        {
            return;
        }

        if (portal is null)
        {
            return;
        }

        _promptLabel.Text = distance <= range
            ? $"[F] {portal.Prompt}"
            : $"{portal.Label}  {distance:0.0} m   •   move closer";
        _promptLabel.TooltipText = "Travel between the ranch and town. Current travel has no added time or gold cost.";
    }

    public void SetStatus(string message, double seconds = 2.8)
    {
        if (_statusLabel is null)
        {
            return;
        }

        _statusLabel.Text = message ?? string.Empty;
        _statusRemaining = string.IsNullOrWhiteSpace(message) ? 0 : Math.Max(0.1, seconds);
    }
}
