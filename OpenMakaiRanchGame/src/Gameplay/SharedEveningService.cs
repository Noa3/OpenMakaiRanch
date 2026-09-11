using System;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Data;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Gameplay;

public sealed record SharedEveningStatus(bool CanPlan, bool Planned, string PartnerId, string Reason);
internal sealed record SharedEveningSnapshot(int Day, string PartnerId);

/// <summary>Optional non-explicit overnight companionship. No conception, new clock, or alternate rest rewards.</summary>
public sealed class SharedEveningService
{
    public const int PlannedDayFlag = 1_230_200; // per resident; at most one is positive
    public const int LastSharedDayFlag = 1_230_201; // per resident, bounded receipt
    public const int LastSettledDayFlag = 1_230_202; // global, duplicate settlement protection
    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private readonly FlagService _flags;
    private readonly DatingService _dating;

    public SharedEveningService(SaveState state, DataRegistry data, FlagService flags, DatingService dating)
    { _state = state; _data = data; _flags = flags; _dating = dating; }

    public SharedEveningStatus Inspect()
    {
        var id = _state.Dating.ActivePartnerId;
        var planned = !string.IsNullOrEmpty(id) && _flags.GetCharIntFlag(id, PlannedDayFlag) == _state.Calendar.Day;
        string reason;
        if (!_state.Story.FirstDayCompleted || _state.Calendar.Day < 2 || _state.Calendar.Phase != DayPhase.Night)
            reason = T("evening.reason.night", "Available at night after the introduction.");
        else if (_state.WorldAreaId != "ranch") reason = T("evening.reason.home", "Return to the ranch house first.");
        else if (_state.Calendar.NightAction != "rest") reason = T("evening.reason.rest", "Choose Rest for tonight first. Training and Admin remain alternatives.");
        else if (_flags.GetGlobalIntFlag(LastSettledDayFlag) >= _state.Calendar.Day)
            reason = T("evening.reason.used", "This night has already been completed.");
        else if (!IsReviewedAdult("anon") || !IsReviewedAdult(id) || id == "anon")
            reason = T("evening.reason.adults", "This activity is available only for reviewed adult participants.");
        else if (_state.Dating.ActiveApproach != DateInviteApproach.Respectful)
            reason = T("evening.reason.voluntary", "A voluntary invitation is required. Pressure does not unlock closeness.");
        else if (_dating.StageFor(id) < RelationshipStage.Romantic)
            reason = T("evening.reason.trust", "Spend voluntary time together and build a romantic bond first.");
        else
        {
            var partner = _state.Roster.Characters.Single(c => c.Id == id);
            reason = partner.Morale < 50 || partner.Fatigue >= 80 || partner.Hp <= 0 || partner.Energy <= 0
                || partner.Mature.IsCollapsed || partner.Mature.FallState == FallState.Collapse
                ? T("evening.reason.wellbeing", "Your companion needs time to recover. There is no penalty for resting separately.") : string.Empty;
        }
        return new(reason.Length == 0, planned, id, reason);
    }

    private bool IsReviewedAdult(string id)
    {
        var candidates = _state.Roster.Characters.Where(c => c.Id == id).ToArray();
        if (candidates.Length != 1) return false;
        var character = candidates[0];
        return AdultEligibilityGate.IsEligibleForAdult(character) && character.ApparentAge >= 18
            && _data.Characters.TryGetValue(character.DefinitionId, out var definition)
            && AdultEligibilityGate.IsEligibleForAdult(definition) && definition.ApparentAge >= 18;
    }

    public bool TryPlan(out string message)
    {
        var status = Inspect();
        if (!status.CanPlan || status.Planned)
        { message = status.Reason.Length > 0 ? status.Reason : T("evening.already", "Tonight is already planned."); return false; }
        Cancel();
        _flags.SetCharIntFlag(status.PartnerId, PlannedDayFlag, _state.Calendar.Day);
        message = T("evening.planned", "A quiet night together is planned. Nothing happens until you sleep; you can still cancel.");
        return true;
    }

    public void Cancel()
    {
        foreach (var character in _state.Roster.Characters)
            if (_flags.GetCharIntFlag(character.Id, PlannedDayFlag) != 0)
                _flags.SetCharIntFlag(character.Id, PlannedDayFlag, 0);
    }

    internal SharedEveningSnapshot? CaptureForSettlement()
    {
        var status = Inspect();
        return status.CanPlan && status.Planned ? new(_state.Calendar.Day, status.PartnerId) : null;
    }

    internal bool CompleteAfterSettlement(SharedEveningSnapshot? snapshot, DailyReport report)
    {
        Cancel(); // An invalidated invitation also expires quietly, without penalties.
        if (snapshot is null || snapshot.Day != report.Day || (long)_state.Calendar.Day != (long)report.Day + 1
            || _flags.GetGlobalIntFlag(LastSettledDayFlag) >= report.Day || !IsReviewedAdult(snapshot.PartnerId) || !IsReviewedAdult("anon")) return false;
        var partner = _state.Roster.Characters.Single(c => c.Id == snapshot.PartnerId);
        _flags.SetGlobalIntFlag(LastSettledDayFlag, report.Day);
        _flags.SetCharIntFlag(partner.Id, LastSharedDayFlag, report.Day);
        // Modest relationship acknowledgement only. Normal Rest/bath recovery has already run once.
        partner.Bond = (int)Math.Clamp((long)partner.Bond + 1, 0, 100);
        partner.Morale = (int)Math.Clamp((long)partner.Morale + 2, 0, 100);
        report.Lines.Add(T("evening.report", "You spent a quiet night together. A new morning begins."));
        return true;
    }
}
