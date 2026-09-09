using System;
using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// Deterministic full-screen location reveal used after prologue, continue and area travel.
/// Presentation only; it never advances time or alters saves.
/// </summary>
public partial class WorldTransitionController : Control
{
    private ColorRect? _blackout;
    private Label? _locationLabel;
    private Label? _subtitleLabel;
    private double _elapsed;
    private double _holdSeconds;
    private double _fadeSeconds;
    private bool _running;

    public bool IsTransitioning => _running;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _blackout = GetNodeOrNull<ColorRect>("Blackout");
        _locationLabel = GetNodeOrNull<Label>("Center/LocationLabel");
        _subtitleLabel = GetNodeOrNull<Label>("Center/SubtitleLabel");
    }

    public override void _Process(double delta)
    {
        if (!_running || _blackout is null)
        {
            return;
        }

        _elapsed += delta;
        if (_elapsed <= _holdSeconds)
        {
            SetAlpha(1f);
            return;
        }

        var progress = _fadeSeconds <= 0.01
            ? 1f
            : Mathf.Clamp((float)((_elapsed - _holdSeconds) / _fadeSeconds), 0f, 1f);
        SetAlpha(1f - progress);

        if (progress >= 1f)
        {
            _running = false;
            Visible = false;
        }
    }

    public void HideImmediately()
    {
        _running = false;
        Visible = false;
        SetAlpha(0f);
    }

    public void CoverInstant()
    {
        _running = false;
        Visible = true;
        SetAlpha(1f);
    }

    public void Reveal(string location, string subtitle, double holdSeconds = 0.75, double fadeSeconds = 0.85)
    {
        _elapsed = 0;
        _holdSeconds = Math.Max(0, holdSeconds);
        _fadeSeconds = Math.Max(0.05, fadeSeconds);
        _running = true;
        Visible = true;

        if (_locationLabel is not null) _locationLabel.Text = location;
        if (_subtitleLabel is not null) _subtitleLabel.Text = subtitle;
        SetAlpha(1f);
    }

    private void SetAlpha(float alpha)
    {
        alpha = Mathf.Clamp(alpha, 0f, 1f);
        if (_blackout is not null)
        {
            var color = _blackout.Color;
            color.A = alpha;
            _blackout.Color = color;
        }

        if (_locationLabel is not null)
        {
            var modulate = _locationLabel.Modulate;
            modulate.A = alpha;
            _locationLabel.Modulate = modulate;
        }

        if (_subtitleLabel is not null)
        {
            var modulate = _subtitleLabel.Modulate;
            modulate.A = alpha;
            _subtitleLabel.Modulate = modulate;
        }
    }
}
