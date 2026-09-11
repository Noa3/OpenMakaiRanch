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
        if (_serviceRoot is null || screen == _serviceRoot || screen == _currentScreen) return true;
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
            "inventory" => screen is "equipment" or "clothing_list" or "clothing_change",
            "clothing_list" => screen == "clothing_change",
            "magic" => screen is "mana_conversion" or "incubation" or "production",
            "pharmacy_list" => screen == "pharmacy_craft",
            "roster" => screen is "character_detail" or "equipment",
            _ => false
        };
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
