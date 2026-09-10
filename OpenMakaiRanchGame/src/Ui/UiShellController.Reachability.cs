using Godot;

namespace OpenMakaiRanch.Ui;

/// <summary>
/// Keeps every ordinary management feature reachable when responsive layout hides the desktop
/// navigation and switches to the horizontally scrolling compact navigation.
/// </summary>
public partial class UiShellController
{
    private bool _reachabilityCompactRoutesBound;

    public override void _Process(double delta)
    {
        if (_reachabilityCompactRoutesBound || !_shellReady || !GodotObject.IsInstanceValid(_compactNavigation))
        {
            return;
        }

        AddCompactReachabilityRoute("clothing_list", "Clothes");
        AddCompactReachabilityRoute("visit", "Visit");
        AddCompactReachabilityRoute("room_assign", "Rooms");
        AddCompactReachabilityRoute("magic_basic", "Magic");
        AddCompactReachabilityRoute("ability", "Abilities");
        AddCompactReachabilityRoute("pharmacy_list", "Pharmacy");
        AddCompactReachabilityRoute("options", "Options");

        _reachabilityCompactRoutesBound = true;
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
