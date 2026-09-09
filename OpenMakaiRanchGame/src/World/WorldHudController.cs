using System;
using System.Linq;
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
    private Label? _guidanceLabel;
    private Label? _promptLabel;
    private Label? _statusLabel;
    private Button? _advanceTimeButton;

    private double _refreshRemaining;
    private double _statusRemaining;
    private string _selectedCharacterId = string.Empty;
    private string _selectedCharacterName = string.Empty;

    public string SelectedCharacterId => _selectedCharacterId;

    public override void _Ready()
    {
        _dayLabel = GetNodeOrNull<Label>("TopBar/DayLabel");
        _economyLabel = GetNodeOrNull<Label>("TopBar/EconomyLabel");
        _rosterLabel = GetNodeOrNull<Label>("TopBar/RosterLabel");
        _workerLabel = GetNodeOrNull<Label>("WorkerPanel/WorkerLabel");
        _assignmentLabel = GetNodeOrNull<Label>("WorkerPanel/AssignmentLabel");
        _guidanceLabel = GetNodeOrNull<Label>("GuidancePanel/GuidanceLabel");
        _promptLabel = GetNodeOrNull<Label>("Prompt");
        _statusLabel = GetNodeOrNull<Label>("StatusLabel");
        _advanceTimeButton = GetNodeOrNull<Button>("AdvanceTimeButton");

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

        if (_advanceTimeButton is not null)
        {
            if (calendar.Phase == OpenMakaiRanch.Core.Models.DayPhase.Night)
            {
                var hasNightPlan = calendar.NightAction is "rest" or "train" or "admin";
                _advanceTimeButton.Text = hasNightPlan ? "End Day" : "Plan Night";
                _advanceTimeButton.TooltipText = hasNightPlan
                    ? "Settle the current day and show the daily report"
                    : "Open management and choose tonight's work before ending the day";
            }
            else
            {
                _advanceTimeButton.Text = "Advance Phase";
                _advanceTimeButton.TooltipText = "Advance the shared ranch clock to the next phase";
            }
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
        RefreshGuidance(game);
    }

    private void RefreshGuidance(GameRoot game)
    {
        if (_guidanceLabel is null)
        {
            return;
        }

        var calendar = game.State.Calendar;
        if (calendar.Phase == OpenMakaiRanch.Core.Models.DayPhase.Night)
        {
            if (calendar.NightAction is not ("rest" or "train" or "admin"))
            {
                _guidanceLabel.Text = "Next: choose tonight's plan in Management [M].";
                _guidanceLabel.TooltipText = "Night settlement requires a night plan. Open Management and choose Rest, Train or Admin.";
                return;
            }

            _guidanceLabel.Text = "Next: End Day when you are finished.";
            _guidanceLabel.TooltipText = "The night plan is ready. End Day to run the existing settlement and open the Daily Report.";
            return;
        }

        var restingWorkers = game.Roster.Characters.Count(character =>
            string.Equals(game.Schedule.GetAssignment(character.Id), "rest", StringComparison.OrdinalIgnoreCase));

        if (restingWorkers > 0)
        {
            _guidanceLabel.Text = restingWorkers == 1
                ? "Next: 1 worker is resting — assign work or keep them resting."
                : $"Next: {restingWorkers} workers are resting — assign work or keep them resting.";
            _guidanceLabel.TooltipText = "Use Tab + a nearby work station, or open Management → Schedule. Rest is also a valid deliberate choice.";
            return;
        }

        _guidanceLabel.Text = $"Next: review the ranch, then advance from {calendar.Phase} when ready.";
        _guidanceLabel.TooltipText = "You can inspect residents, change schedules, build facilities, explore management, or advance the shared day phase.";
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
            _selectedCharacterName = string.Empty;
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
            _selectedCharacterName = string.Empty;
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
        _selectedCharacterName = definition.DisplayName;
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
            _promptLabel.Text = "WASD / arrows: move   •   Shift: sprint   •   Hold RMB: look   •   Wheel: zoom   •   Tab: worker   •   M: management   •   F1: help";
            _promptLabel.TooltipText = "Basic world controls. Press F1 for the full help and gameplay loop.";
            return;
        }

        if (distance <= interactionRange)
        {
            _promptLabel.Text = target.IsAvailable
                ? (string.IsNullOrWhiteSpace(_selectedCharacterName)
                    ? $"[F] Use {target.Label}"
                    : $"[F] Assign {_selectedCharacterName} → {target.Label}")
                : $"{target.Label} — {target.UnavailableReason}";
            _promptLabel.TooltipText = target.IsAvailable
                ? "Press F to apply this spatial interaction through the same shared simulation used by Management."
                : $"Unavailable: {target.UnavailableReason}";
            return;
        }

        _promptLabel.Text = $"{target.Label}  {distance:0.0} m   •   move closer to interact";
        _promptLabel.TooltipText = $"Move within {interactionRange:0.0} m to interact with {target.Label}.";
    }

    public void SetCharacterInteractionTarget(string characterName, float distance, float interactionRange)
    {
        if (_promptLabel is null)
        {
            return;
        }

        if (distance <= interactionRange)
        {
            _promptLabel.Text = $"[F] Talk to {characterName}";
            _promptLabel.TooltipText = $"Open {characterName}'s existing character detail screen. This world interaction does not apply hidden rewards or relationship changes.";
        }
        else
        {
            _promptLabel.Text = $"{characterName}  {distance:0.0} m   •   move closer to talk";
            _promptLabel.TooltipText = $"Move within {interactionRange:0.0} m to interact with {characterName}.";
        }
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
