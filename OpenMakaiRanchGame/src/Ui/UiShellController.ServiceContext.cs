using System;
using Godot;

namespace OpenMakaiRanch.Ui;

/// <summary>Restricts the retained service renderers to the location that opened them.</summary>
public partial class UiShellController
{
    private string? _serviceRoot;
    public bool IsDedicatedService => _serviceRoot is not null;
    private ulong _serviceGeneration;
    public void SetServiceContext(string screen)
    {
        _serviceRoot = screen;
        _serviceGeneration = _game.StateGeneration;
    }
    public void ClearServiceContext()
    {
        _serviceRoot = null;
        if (_shellReady) { _endDayButton.Visible = true; _menuButton.Visible = true; _topBarRow2.Visible = true; }
    }

    private bool ServiceAllowsScreen(string screen)
    {
        // A load/new game is a new navigation context, not a child page of the old service.
        if (_serviceRoot is not null && _serviceGeneration != _game.StateGeneration) ClearServiceContext();
        if (_serviceRoot is null) return true;
        if (!IsKnownService(screen)) return false;
        if (screen == _serviceRoot || screen == _currentScreen) return true;
        if (screen == "ranch")
        {
            // Legacy Back buttons used to return to the global hub. In a physical service
            // they instead close the surface and leave the player at the same location.
            WorldHost()?.CloseManagement();
            return false;
        }
        return _serviceRoot switch
        {
            "adventure" => screen == "combat",
            "shop" => screen == "inventory",
            "inventory" => false,
            "clothing_list" => screen is "clothing_change" or "clothing_strip",
            "magic_basic" => screen is "magic_forbidden" or "magic_tentacle",
            "pharmacy_list" => screen == "pharmacy_craft",
            "roster" => screen is "character_detail" or "ability" or "room_assign",
            _ => false
        };
    }

    internal static bool IsKnownService(string screen) => screen is "ranch" or "report" or "roster"
        or "shop" or "inventory" or "adventure" or "combat" or "milestones" or "research"
        or "bond" or "pets" or "saveload" or "options" or "settings" or "training" or "visit"
        or "milk" or "mental" or "character_detail" or "clothing_list" or "clothing_change"
        or "clothing_strip" or "room_assign" or "ability" or "pharmacy_list" or "pharmacy_craft"
        or "magic_basic" or "magic_forbidden" or "magic_tentacle";

    private void RenderStationStorage()
    {
        AddTitle("Ranch storage");
        _content.AddChild(MutedLabel("Stored items and production share the same ranch inventory. Trade at the General Store; offer meals by speaking to a resident."));
        foreach (var item in _game.Inventory.Items)
            _content.AddChild(MutedLabel($"{_game.Data.Item(item.Key).DisplayName}: {item.Value}"));
        _content.AddChild(SubtitleLabel("Production stockpile"));
        foreach (var item in _game.State.Ranch.Stockpile)
            _content.AddChild(MutedLabel($"{item.Key}: {item.Value}"));
    }

    private void ApplyDedicatedServiceLayout()
    {
        if (_serviceRoot is null || !_shellReady || _fullScreenMode) return;
        _navPanel.Visible = false;
        _compactNavigationScroll.Visible = false;
        _endDayButton.Visible = false;
        _menuButton.Visible = false;
        _topBarRow2.Visible = false;
    }

    private void RenderFacilityPlanning()
    {
        AddTitle("Facility planning");
        _content.AddChild(MutedLabel("Plan construction here. Assign daily work at each building's own workstation."));
        AddFacilityTiles("Construction and upgrades");
    }
}
