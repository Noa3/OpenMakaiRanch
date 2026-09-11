using System;
using Godot;

namespace OpenMakaiRanch.Ui;

/// <summary>Restricts the retained service renderers to the location that opened them.</summary>
public partial class UiShellController
{
    private string? _serviceRoot;
    public bool IsDedicatedService => _serviceRoot is not null;
    public void SetServiceContext(string screen) => _serviceRoot = screen;
    public void ClearServiceContext()
    {
        _serviceRoot = null;
        if (_shellReady) { _endDayButton.Visible = true; _menuButton.Visible = true; _topBarRow2.Visible = true; }
    }

    private bool ServiceAllowsScreen(string screen)
    {
        if (_serviceRoot is null || screen == _serviceRoot || screen == _currentScreen) return true;
        return _serviceRoot switch
        {
            "adventure" => screen == "combat",
            "shop" => screen == "inventory",
            "inventory" => screen is "equipment" or "clothing_list" or "clothing_change",
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
