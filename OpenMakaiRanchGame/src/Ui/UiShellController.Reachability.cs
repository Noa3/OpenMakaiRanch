using Godot;

namespace OpenMakaiRanch.Ui;

/// <summary>
/// Keeps every ordinary management feature reachable when responsive layout hides the desktop
/// navigation and switches to the horizontally scrolling compact navigation. Also keeps the
/// top-bar player vitality display bound to PlayerState rather than the first ranch resident.
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
            AddCompactReachabilityRoute("clothing_list", "Clothes");
            AddCompactReachabilityRoute("visit", "Visit");
            AddCompactReachabilityRoute("room_assign", "Rooms");
            AddCompactReachabilityRoute("magic_basic", "Magic");
            AddCompactReachabilityRoute("ability", "Abilities");
            AddCompactReachabilityRoute("pharmacy_list", "Pharmacy");
            AddCompactReachabilityRoute("options", "Options");
            _reachabilityCompactRoutesBound = true;
        }

        RefreshCanonicalPlayerVitals();
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

    private void RefreshCanonicalPlayerVitals()
    {
        if (!GodotObject.IsInstanceValid(_hpLabel) || !GodotObject.IsInstanceValid(_hpBar))
        {
            return;
        }

        var player = _game.State.Player;
        var maxHp = System.Math.Max(1, player.MaxHp);
        var hp = System.Math.Clamp(player.Hp, 0, maxHp);
        _hpLabel.Text = $"HP {hp}/{maxHp}";
        _hpBar.MinValue = 0;
        _hpBar.MaxValue = maxHp;
        _hpBar.Value = hp;
    }
}
