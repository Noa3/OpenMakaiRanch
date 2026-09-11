using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

public partial class UiLayoutAcceptance
{
    private async Task CheckScaleButtons(WorldGameController world)
    {
        var game = GameRoot.Instance;
        var previous = game.State.Settings.Clone();
        try
        {
            game.State.Settings.WindowWidth = 960;
            game.State.Settings.WindowHeight = 540;
            game.SetUiScale(1f);
            await Resize(new Vector2I(960, 540));
            world.OpenManagementScreen("options");
            await Frames(8);
            var shell = world.Shell!;
            var panel = shell.GetNode<Control>(shell.RootPanelPath);
            foreach (var direction in new[] { "Scale Up", "Scale Down" })
            {
                for (var i = 0; i < 6; i++)
                {
                    var button = Descendants(shell).OfType<Button>().First(control => control.Text == direction);
                    button.GrabFocus();
                    await Frames(3);
                    var expected = Mathf.Clamp(game.State.Settings.UiScale + (direction == "Scale Up" ? 0.1f : -0.1f), 0.85f, 1.35f);
                    await Click(button);
                    await Frames(3);
                    Check(Mathf.IsEqualApprox(game.State.Settings.UiScale, expected)
                        && Mathf.IsEqualApprox(GetTree().Root.ContentScaleFactor, expected)
                        && panel.Scale.IsEqualApprox(Vector2.One),
                        $"{direction} click {i + 1}: canonical scale applies once, including its existing limit");
                    Check(Encloses(GetViewport().GetVisibleRect(), panel.GetGlobalRect()),
                        $"{direction} click {i + 1}: management stays within the resized logical viewport");
                }
                var caption = $"UI Scale: {(game.State.Settings.UiScale * 100):F0}%";
                Check(Descendants(shell).OfType<Label>().Any(label => label.Text == caption),
                    $"{direction}: displayed percentage matches the saved preference");
                await Capture(direction == "Scale Up" ? "options-button-scale-upper" : "options-button-scale-lower");
            }
            var back = Descendants(shell).OfType<Button>().Single(button => button.Name == "ReturnToWorldButton");
            Check(VisibleTarget(back), "scale button sequence retains a usable Return to World target");
            await Click(back);
            Check(!world.IsManagementVisible && world.WorldActionsAvailable,
                "return after actual scale-button clicks restores world input");
        }
        finally
        {
            game.State.Settings.WindowWidth = previous.WindowWidth;
            game.State.Settings.WindowHeight = previous.WindowHeight;
            game.SetUiScale(previous.UiScale);
            game.RuntimeSettings.Apply(game.State.Settings);
            world.CloseManagement();
        }
    }
}
