using Godot;

namespace OpenMakaiRanch.Ui;

/// <summary>
/// Keeps every ordinary management feature reachable when responsive layout hides the desktop
/// navigation and switches to the horizontally scrolling compact navigation. Also keeps the
/// top-bar player vitality display bound to PlayerState rather than the first ranch resident.
/// Existing gameplay/eligibility gates remain the authority for whether a screen action is allowed.
/// </summary>
public partial class UiShellController
{
    private bool _reachabilityCompactRoutesBound;

    public override void _Process(double delta)
    {
        if (!_shellReady)
        {
            return;
        }

        if (!_reachabilityCompactRoutesBound && GodotObject.IsInstanceValid(_compactNavigation))
        {
            // Desktop navigation exposes these as separate destinations, but the authored compact
            // bar omitted them. Mirror the actual desktop routes instead of hiding gameplay on
            // narrow windows/mobile.
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


        EnsureInputOptionsExtension();
        ApplyOpeningUtilityLayout();
        ApplyDedicatedServiceLayout();
        EnsureControllerFocus();
    }

    private void AddCompactReachabilityRoute(string screenId, string fallbackLabel)
    {
        if (_compactNavButtons.ContainsKey(screenId))
        {
            return;
        }

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
        button.Pressed += () =>
        {
            _game.Feedback.PlayNavigate();
            ShowScreen(screenId);
        };

        _compactNavigation.AddChild(button);
        _compactNavButtons[screenId] = button;
    }

}
