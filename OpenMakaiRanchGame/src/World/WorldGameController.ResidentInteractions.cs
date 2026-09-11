using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.World;

public partial class WorldGameController
{
    public bool OpenResident(string id)
    {
        if (_stationPanel is null || !WorldActionsAvailable || !CanVisitResidentHere(id)) return false;
        _stationPanel.OpenResident(id);
        ActiveEnterManagement(); RefreshHudOwnership();
        return true;
    }

    internal void SetResidentConversationFocus(string id)
    {
        if (_ranch?.Roster is { } ranchRoster && Godot.GodotObject.IsInstanceValid(ranchRoster)) ranchRoster.ConversationFocusId = _activeAreaId == "ranch" ? id : "";
        if (_town?.Companion is { } townRoster && Godot.GodotObject.IsInstanceValid(townRoster)) townRoster.ConversationFocusId = _activeAreaId == "town" ? id : "";
    }

    public ResidentActionResult TryResidentAction(string id, ResidentAction action, ulong generation,
        int day, DayPhase phase, string? itemId = null)
    {
        if (!IsStationPanelOpen || _stationPanel?.ContextKind != "resident" || _stationPanel.ContextId != id
            || !CanVisitResidentHere(id))
            return new(false, false, T("world.resident.reason.nearby", "Talk to this resident nearby before choosing an interaction."));
        return GameRoot.Instance.TryResidentAction(id, action, generation, day, phase, itemId);
    }
}
