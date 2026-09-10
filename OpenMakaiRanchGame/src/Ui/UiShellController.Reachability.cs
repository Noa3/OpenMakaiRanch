using Godot;

namespace OpenMakaiRanch.Ui;

/// <summary>
/// Keeps ordinary management features reachable in compact layouts, keeps canonical player HP visible,
/// and attaches controller/input options without duplicating the authored options screen.
/// </summary>
public partial class UiShellController
{
    private bool _reachabilityCompactRoutesBound;

    public override void _Process(double delta)
    {
        if (!_shellReady) return;

        if (!_reachabilityCompactRoutesBound && GodotObject.IsInstanceValid(_compactNavigation))
        {
            AddCompactReachabilityRoute("clothing_list", "Clothes");
            AddCompactReachabilityRoute("clothing_change", "Change Clothes");
            AddCompactReachabilityRoute("clothing_strip", "Undress");
            AddCompactReachabilityRoute("visit", "Visit");
            AddCompactReachabilityRoute("room_assign", "Rooms");
            AddCompactReachabilityRoute("magic_basic", "Magic");
            AddCompactReachabilityRoute("magic_forbidden", "Advanced Magic");
            AddCompactReachabilityRoute("magic_tentacle", "Special Magic");
            AddCompactReachabilityRoute("ability", "Abilities");
            AddCompactReachabilityRoute("pharmacy_list", "Potions");
            AddCompactReachabilityRoute("pharmacy_craft", "Craft Potions");
            AddCompactReachabilityRoute("options", "Options");
            _reachabilityCompactRoutesBound = true;
        }

        RefreshCanonicalPlayerVitals();
        EnsureInputOptionsExtension();
        EnsureControllerFocus();
    }

    private void AddCompactReachabilityRoute(string screenId, string fallbackLabel)
    {
        if (_compactNavButtons.ContainsKey(screenId)) return;
        var title = ScreenTitle(screenId);
        var button = new Button
        {
            Name = $"Reachability_{screenId}",
            Text = string.IsNullOrWhiteSpace(title) ? fallbackLabel : title,
            TooltipText = string.IsNullOrWhiteSpace(title) ? fallbackLabel : title,
            CustomMinimumSize = new Vector2(84, 34),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
        };
        ApplySecondaryButtonStyle(button);
        button.Pressed += () => { _game.Feedback.PlayNavigate(); ShowScreen(screenId); };
        _compactNavigation.AddChild(button);
        _compactNavButtons[screenId] = button;
    }

    private void RefreshCanonicalPlayerVitals()
    {
        if (!GodotObject.IsInstanceValid(_hpLabel) || !GodotObject.IsInstanceValid(_hpBar)) return;
        var player = _game.State.Player;
        var maxHp = System.Math.Max(1, player.MaxHp);
        var hp = System.Math.Clamp(player.Hp, 0, maxHp);
        _hpLabel.Text = $"HP {hp}/{maxHp}";
        _hpBar.MinValue = 0;
        _hpBar.MaxValue = maxHp;
        _hpBar.Value = hp;
    }
}
