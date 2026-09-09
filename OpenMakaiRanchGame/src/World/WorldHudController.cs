using System;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Lightweight world-facing HUD for the 3D ranch. It reads the same <see cref="GameRoot"/>
/// simulation used by the management UI and never owns gameplay state.
///
/// The HUD intentionally exposes only ordinary ranch/session information: day/phase/weather,
/// economy resources, roster size, selected worker/job, interaction prompts and transient
/// feedback. It is safe to keep active while other presentation systems evolve.
/// </summary>
public partial class WorldHudController : CanvasLayer
{
    [Export] public double RefreshIntervalSeconds { get; set; } = 0.20;
    [Export] public double DefaultStatusSeconds { get; set; } = 2.5;

    private Label? _dayLabel;
    private Label? _economyLabel;
    private Label? _rosterLabel;
    private Label? _workerLabel;
    private Label? _assignmentLabel;
    private Label? _promptLabel;
    private Label? _statusLabel;

    private double _refreshRemaining;
    private double _statusRemaining;
    private string _selectedCharacterId = string.Empty;

    public string SelectedCharacterId => _selectedCharacterId;

    public override void _Ready()
    {
        _dayLabel = GetNodeOrNull<Label>("TopBar/DayLabel");
        _economyLabel = GetNodeOrNull<Label>("TopBar/EconomyLabel");
        _rosterLabel = GetNodeOrNull<Label>("TopBar/RosterLabel");
        _workerLabel = GetNodeOrNull<Label>("WorkerPanel/WorkerLabel");
        _assignmentLabel = GetNodeOrNull<Label>("WorkerPanel/AssignmentLabel");
        _promptLabel = GetNodeOrNull<Label>("Prompt");
        _statusLabel = GetNodeOrNull<Label>("StatusLabel");

        RefreshSimulation(GameRoot.Instance);
    }

    public override void _Process(double delta)
    {
        _refreshRemaining -= delta;
        if (_refreshRemaining <= 0.0)
        {
            _refreshRemaining = Math.Max(0.05, RefreshIntervalSeconds);
            RefreshSimulation(GameRoot.Instance);
        }

        if (_statusRemaining > 0.0)
        {
            _statusRemaining -= delta;
            if (_statusRemaining <= 0.0 && _statusLabel is not null)
            {
                _statusLabel.Text = string.Empty;
            }
        }
    }

    /// <summary>Refresh normal ranch/session values from the shared simulation.</summary>
    public void RefreshSimulation(GameRoot? game)
    {
        if (game is null || !GodotObject.IsInstanceValid(game))
        {
            return;
        }

        var state = game.State;
        var calendar = state.Calendar;
        var economy = state.Economy;

        if (_dayLabel is not null)
        {
            _dayLabel.Text = $"Day {calendar.Day}  •  {calendar.Season}  •  {calendar.Phase}  •  {calendar.CurrentWeather}";
        }

        if (_economyLabel is not null)
        {
            _economyLabel.Text = $"{economy.Gold:N0} G   Spirit {economy.SpiritEnergy:N0}   Mana {economy.ManaReservoir:N0}";
        }

        if (_rosterLabel is not null)
        {
            var count = game.Roster.Characters.Count;
            _rosterLabel.Text = count == 1 ? "1 resident" : $"{count} residents";
        }

        RefreshSelectedCharacter(game);
    }

    /// <summary>
    /// Sets the worker used by spatial job stations. The controller owns selection; the HUD only
    /// presents it. Passing an empty id explicitly renders the no-worker state.
    /// </summary>
    public void SetSelectedCharacter(GameRoot? game, string? characterId)
    {
        _selectedCharacterId = characterId ?? string.Empty;
        RefreshSelectedCharacter(game);
    }

    private void RefreshSelectedCharacter(GameRoot? game)
    {
        if (_workerLabel is null && _assignmentLabel is null)
        {
            return;
        }

        if (game is null || !GodotObject.IsInstanceValid(game) || string.IsNullOrWhiteSpace(_selectedCharacterId))
        {
            if (_workerLabel is not null)
            {
                _workerLabel.Text = "Worker: none";
            }
            if (_assignmentLabel is not null)
            {
                _assignmentLabel.Text = "Tab: switch worker";
            }
            return;
        }

        var character = game.Roster.Find(_selectedCharacterId);
        if (character is null)
        {
            if (_workerLabel is not null)
            {
                _workerLabel.Text = "Worker: unavailable";
            }
            if (_assignmentLabel is not null)
            {
                _assignmentLabel.Text = "Tab: switch worker";
            }
            return;
        }

        var definition = game.Roster.DefinitionFor(character);
        var jobId = game.Schedule.GetAssignment(character.Id);
        var jobLabel = game.Data.Jobs.TryGetValue(jobId, out var job)
            ? job.DisplayName
            : jobId;

        if (_workerLabel is not null)
        {
            _workerLabel.Text = $"Worker: {definition.DisplayName}";
        }
        if (_assignmentLabel is not null)
        {
            _assignmentLabel.Text = $"Current job: {jobLabel}   •   Tab: switch";
        }
    }

    /// <summary>Show the closest station and whether it can currently be activated.</summary>
    public void SetInteractionTarget(IWorldInteractable? target, float distance, float interactionRange)
    {
        if (_promptLabel is null)
        {
            return;
        }

        if (target is null)
        {
            _promptLabel.Text = "WASD / arrows: move   •   Hold RMB: look   •   Mouse wheel: zoom   •   Tab: worker";
            return;
        }

        if (distance <= interactionRange)
        {
            _promptLabel.Text = target.IsAvailable
                ? $"[F] {target.Label}"
                : $"{target.Label} — {target.UnavailableReason}";
            return;
        }

        _promptLabel.Text = $"{target.Label}  {distance:0.0} m   •   move closer to interact";
    }

    public void SetStatus(string message, double? seconds = null)
    {
        if (_statusLabel is null)
        {
            return;
        }

        _statusLabel.Text = message ?? string.Empty;
        _statusRemaining = string.IsNullOrWhiteSpace(message)
            ? 0.0
            : Math.Max(0.1, seconds ?? DefaultStatusSeconds);
    }
}
