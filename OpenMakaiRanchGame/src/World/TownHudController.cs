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
    private double _statusRemaining;

    public override void _Ready()
    {
        _locationLabel = GetNodeOrNull<Label>("TopBar/LocationLabel");
        _economyLabel = GetNodeOrNull<Label>("TopBar/EconomyLabel");
        _promptLabel = GetNodeOrNull<Label>("Prompt");
        _statusLabel = GetNodeOrNull<Label>("StatusLabel");
        _guidanceLabel = GetNodeOrNull<Label>("GuidancePanel/GuidanceLabel");
        Refresh(GameRoot.Instance);
    }

    public override void _Process(double delta)
    {
        if (_statusRemaining > 0)
        {
            _statusRemaining -= delta;
            if (_statusRemaining <= 0 && _statusLabel is not null)
            {
                _statusLabel.Text = string.Empty;
            }
        }
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
            _economyLabel.Text = $"{economy.Gold:N0} G   Mana {economy.ManaReservoir:N0}";
        }

        if (_guidanceLabel is not null)
        {
            _guidanceLabel.Text = calendar.Phase == OpenMakaiRanch.Core.Models.DayPhase.Night
                ? "Town services remain available; return to the ranch when you are ready to plan/end the night."
                : "Explore services, shop, prepare missions, research, recruit, or return to the ranch.";
            _guidanceLabel.TooltipText = "Town interactions open the existing management screens. Purchases and progression still use the shared GameRoot services.";
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
