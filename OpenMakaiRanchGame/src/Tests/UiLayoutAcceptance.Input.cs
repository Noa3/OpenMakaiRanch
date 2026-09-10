using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Ui;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

public partial class UiLayoutAcceptance
{
    private async Task Resize(Vector2I size)
    {
        GetTree().Root.Size = size;
        await Frames(8);
        Check(GetTree().Root.Size == size, $"requested physical window {size} was actually applied");
    }

    private async Task CheckStartupLayouts(MainMenuController menu)
    {
        Check(RuntimeSettingsService.CanvasBaseFor(new Vector2I(640, 480)) == new Vector2I(640, 480),
            "small canvas retains 1:1 base UI pixels rather than shrinking desktop controls");
        Check(RuntimeSettingsService.CanvasBaseFor(new Vector2I(3840, 2160)) == new Vector2I(1920, 1080),
            "4K retains proportional scaling from the existing desktop baseline");
        Check(RuntimeSettingsService.CanvasBaseFor(new Vector2I(3840, 1080)) == new Vector2I(3840, 1080),
            "ultrawide canvas retains expanded world space");
        foreach (var size in new[] { new Vector2I(480, 800), new Vector2I(1280, 720) })
        {
            await Resize(size);
            Check(Encloses(GetViewport().GetVisibleRect(), menu.GetNode<Control>(menu.PanelPath).GetGlobalRect()),
                $"{size}: main menu fits the viewport");
            Check(VisibleTarget(menu.GetNode<Button>(menu.NewGameButtonPath)), $"{size}: New Game is a usable physical hit target");
            await Capture($"main-menu-{size.X}x{size.Y}");
        }
    }

    private async Task CheckCreationLayouts(UiShellController shell)
    {
        var content = shell.GetNode<Control>(shell.ContentPath);
        var scroll = shell.GetNode<ScrollContainer>(shell.ScrollPath);
        foreach (var size in new[] { new Vector2I(640, 480), new Vector2I(480, 800), new Vector2I(1280, 720) })
        {
            await Resize(size);
            Check(content.Size.X <= scroll.Size.X + 1, $"{size}: character creation reflows without horizontal clipping");
            Check(Descendants(content).OfType<Label>().Where(label => label.Name == "BasicTitle" || label.Name == "BodyTitle")
                .All(label => label.Size.Y >= label.GetThemeFontSize("font_size")),
                $"{size}: section headings retain a readable line height");
            var input = Descendants(content).OfType<LineEdit>().First(node => node.Name == "NameInput");
            input.GrabFocus();
            await Frames(4);
            Check(VisibleTarget(input), $"{size}: name entry can be reached by focus scrolling");
            await Capture($"creation-name-{size.X}x{size.Y}");
            var start = Descendants(content).OfType<Button>().First(node => node.Name == "StartButton");
            start.GrabFocus();
            await Frames(4);
            Check(VisibleTarget(start), $"{size}: Start Game remains reachable below the creation form");
        }
    }

    private async Task CheckGamepadMenus(WorldGameController world)
    {
        // These are synthetic standardized joypad events through Godot, not hardware certification.
        await Resize(new Vector2I(640, 480));
        await JoyStroke((JoyButton)4);
        Check(world.IsManagementVisible, "mapped controller Management opens the overlay");
        var shell = world.Shell!;
        world.OpenManagementScreen("options");
        await Frames(8);
        var nav = shell.GetNode<Control>(shell.CompactNavigationPath);
        var navScroll = shell.GetNode<ScrollContainer>(shell.CompactNavigationScrollPath);
        var schedule = Descendants(nav).OfType<Button>().First(button => button.Name.ToString().Contains("Schedule"));
        schedule.GrabFocus();
        await Frames(3);
        Check(VisibleTarget(schedule), "compact navigation scrolls its focused route into view");
        var focus = GetViewport().GuiGetFocusOwner();
        await JoyStroke(JoyButton.DpadRight);
        Check(GetViewport().GuiGetFocusOwner() is { } next && next != focus && next.IsVisibleInTree(),
            "D-pad moves focus between compact navigation controls");
        schedule.GrabFocus();
        await JoyStroke(JoyButton.A);
        Check(shell.CurrentScreen == "schedule", "controller Accept activates the focused Schedule route");
        await JoyStroke(JoyButton.B);
        Check(!world.IsManagementVisible && world.PauseMenu?.IsOpen != true,
            "controller Back returns to the world without opening pause above it");
        world.CloseManagement();

        world.OpenManagementScreen("options");
        await Frames(8);
        var binding = Descendants(shell).OfType<Button>().First(button => button.Name == "Controller_interact");
        binding.GrabFocus();
        await Frames(3);
        var original = InputBindingService.GetGamepadLabel("interact");
        await JoyStroke(JoyButton.A);
        await Stroke(Key.Escape);
        Check(world.IsManagementVisible && binding.Text == original,
            "keyboard Escape cancels controller capture as well as keyboard capture");
        if (!world.IsManagementVisible) world.OpenManagementScreen("options");
        await Frames(6);
        binding = Descendants(shell).OfType<Button>().First(button => button.Name == "Controller_interact");
        binding.GrabFocus();
        await JoyStroke(JoyButton.A);
        await JoyStroke(JoyButton.B);
        Check(world.IsManagementVisible && binding.Text == original
            && InputBindingService.GetGamepadLabel("interact") == original,
            "controller Back cancels capture without rebinding or closing management");
        world.CloseManagement();
        await JoyStroke(JoyButton.Start);
        Check(world.PauseMenu?.IsOpen == true && GetTree().Paused, "controller Start opens the real pause menu");
        await JoyStroke(JoyButton.B);
        Check(world.PauseMenu?.IsOpen != true && !GetTree().Paused, "controller Back resumes from pause exactly once");
    }

    private async Task CheckUiScale(WorldGameController world)
    {
        var game = GameRoot.Instance;
        var original = game.State.Settings.UiScale;
        foreach (var scale in new[] { 0.8f, 1.5f })
        {
            game.State.Settings.UiScale = scale;
            game.RuntimeSettings.Apply(game.State.Settings);
            await Resize(new Vector2I(960, 540));
            world.OpenManagementScreen("options");
            await Frames(8);
            var shell = world.Shell!;
            Check(Mathf.IsEqualApprox(GetTree().Root.ContentScaleFactor, scale), $"UI scale {scale}: saved preference remains applied");
            Check(Encloses(GetViewport().GetVisibleRect(), shell.GetNode<Control>(shell.RootPanelPath).GetGlobalRect()),
                $"UI scale {scale}: management still fits after user scaling");
            await Capture($"options-scale-{scale:0.0}");
            world.CloseManagement();
        }
        game.State.Settings.UiScale = original;
        game.RuntimeSettings.Apply(game.State.Settings);
    }

    private async Task JoyStroke(JoyButton button)
    {
        GetViewport().PushInput(new InputEventJoypadButton { Device = 0, ButtonIndex = button, Pressed = true }, true);
        await Frames(1);
        GetViewport().PushInput(new InputEventJoypadButton { Device = 0, ButtonIndex = button, Pressed = false }, true);
        await Frames(3);
    }
}
