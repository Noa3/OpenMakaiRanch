using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Ui;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

/// <summary>
/// Opt-in engine-routed input and rendered layout checks. Never runs in the ordinary boot path.
/// The post-creation Day-2 fixture is synthetic; this is not an earned-progression playthrough.
/// </summary>
public partial class UiLayoutAcceptance : Node
{
    private sealed record CheckResult(bool Passed, string Message);
    private readonly List<CheckResult> _checks = new();
    private readonly List<string> _captures = new();
    private string _evidence = string.Empty;
    private bool _rendered;

    public override async void _Ready()
    {
        try
        {
            ValidateIsolation();
            _rendered = DisplayServer.GetName() != "headless";
            if (OS.GetCmdlineUserArgs().Contains("--require-ui-captures") && !_rendered)
                throw new InvalidOperationException("A renderer is required; headless is not visual evidence.");
            ProcessMode = ProcessModeEnum.Always;
            await Frames(3);
            // Keep this opt-in observer alive while the real menu changes the current scene.
            GetTree().CurrentScene = null;
            Check(GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn") == Error.Ok, "real main-menu scene opens");
            await Frames(12);
            var menu = (MainMenuController)GetTree().CurrentScene;
            var newGame = menu.GetNode<Button>(menu.NewGameButtonPath);
            Check(GetViewport().GuiGetFocusOwner() == newGame, "fresh main menu focuses New Game without a mouse");
            await CheckStartupLayouts(menu);
            await Click(newGame);
            await Frames(15);
            var world = (WorldGameController)GetTree().CurrentScene;
            var shell = world.Shell!;
            Check(world.IsManagementVisible && shell.CurrentScreen == "character_creation", "engine-routed New Game click opens real character creation");
            var name = Descendants(shell).OfType<LineEdit>().First(control => control.Name == "NameInput");
            name.GrabFocus();
            var beforeName = name.Text;
            name.CaretColumn = name.Text.Length;
            await Stroke(Key.T, 't');
            Check(name.Text == beforeName + "t" && GameRoot.Instance.State.Player.Name == name.Text,
                "typing changes the canonical player name without losing the character-creation input");
            await Stroke(Key.Tab);
            Check(GetViewport().GuiGetFocusOwner() != name && shell.CurrentScreen == "character_creation",
                "Tab moves focus within creation instead of toggling management");
            await CheckCreationLayouts(shell);

            // Explicitly synthetic ordinary-session fixture, after proving the real menu route.
            var game = GameRoot.Instance;
            game.State.Calendar.Day = 2;
            game.State.Calendar.Phase = DayPhase.Morning;
            game.State.Story.FirstDayCompleted = true;
            game.State.Story.FirstDayStage = FirstDayFlowController.StageCompleted;
            game.State.Settings.ReducedMotion = true;
            world.FirstDayFlow?.RefreshFromCurrentState();
            shell.ShowScreen("ranch");
            world.ActivateStoryArea("ranch", reposition: false, firstArrival: false);
            world.Transition?.CompleteImmediately();
            world.CloseManagement();
            game.NotifyStateChanged();
            await Frames(8);
            var gold = game.Economy.Gold;
            var stamina = game.State.Player.Stamina;
            foreach (var size in new[] { new Vector2I(1280, 720), new Vector2I(960, 540),
                new Vector2I(640, 480), new Vector2I(480, 800), new Vector2I(1920, 720) })
            {
                await Resize(size);
                var tag = $"{size.X}x{size.Y}";
                Check(world.OpenManagementScreen("options"), $"{tag}: options opens in the live world");
                await Frames(10);
                var viewport = GetViewport().GetVisibleRect();
                var panel = shell.GetNode<Control>(shell.RootPanelPath);
                var scroll = shell.GetNode<ScrollContainer>(shell.ScrollPath);
                var content = shell.GetNode<Control>(shell.ContentPath);
                var back = Descendants(shell).OfType<Button>().Single(button => button.Name == "ReturnToWorldButton");
                GD.Print($"UI METRICS {tag}: viewport={viewport.Size}, panel={panel.GetGlobalRect()}, scroll={scroll.GetGlobalRect()}");
                Check(Encloses(viewport, panel.GetGlobalRect()), $"{tag}: management panel stays within the logical viewport");
                Check(content.Size.X <= scroll.Size.X + 1, $"{tag}: options do not require horizontal content scrolling");
                Check(VisibleTarget(back), $"{tag}: Return to World is visible, unclipped and at least 24 physical pixels high");
                Check(VisibleTarget(shell.GetNode<Button>(shell.EndDayButtonPath)), $"{tag}: time button is visible, unclipped and at least 24 physical pixels high");
                shell.GetNode<Button>(shell.MenuButtonPath).GrabFocus();
                scroll.ScrollVertical = 0;
                await Frames(3);
                await Capture("options-top-" + tag);
                var binding = Descendants(content).OfType<Button>().First(button => button.Name == "Keyboard_interact");
                binding.GrabFocus();
                // FollowFocus owns this scroll; a second same-frame request sees stale geometry.
                await Frames(4);
                Check(VisibleTarget(binding), $"{tag}: keyboard focus scrolls the interaction binding fully into view");
                var oldBinding = InputBindingService.GetKeyboardLabel("interact");
                await Click(binding);
                await ToSignal(GetTree().CreateTimer(0.22), SceneTreeTimer.SignalName.Timeout);
                await Stroke(Key.Escape);
                Check(world.IsManagementVisible && shell.CurrentScreen == "options"
                    && oldBinding == InputBindingService.GetKeyboardLabel("interact"),
                    $"{tag}: Escape cancels input capture without closing the menu or changing the binding");
                await Capture("options-controls-" + tag);
                await Click(back);
                Check(!world.IsManagementVisible && world.Ranch?.InputGate.WorldInputEnabled == true,
                    $"{tag}: actual Return-to-World hit target releases management input ownership");
                // Keep collecting layout evidence even when a click was clipped in the baseline.
                world.CloseManagement();
            }
            await CheckGamepadMenus(world);
            await CheckUiScale(world);
            await CheckWorldLayouts(world);
            Check(game.Economy.Gold == gold && game.State.Player.Stamina == stamina && game.State.Calendar.Day == 2,
                "layout/input inspection does not pay rewards, spend daily stamina or settle the day");
        }
        catch (Exception exception)
        {
            Check(false, "acceptance stopped: " + exception);
        }
        finally
        {
            GetTree().Paused = false;
            var passed = _checks.Count > 0 && _checks.All(check => check.Passed);
            if (!string.IsNullOrEmpty(_evidence))
                File.WriteAllText(Path.Combine(_evidence, "results.json"), JsonSerializer.Serialize(new
                {
                    passed, rendered = _rendered, engine = Engine.GetVersionInfo()["string"].AsString(),
                    checks = _checks, captures = _captures,
                    limits = "Synthetic engine events and Day-2 state; not physical-device, Forward+ performance or complete playthrough acceptance."
                }, new JsonSerializerOptions { WriteIndented = true }));
            GD.Print(passed ? "UI ACCEPTANCE PASS" : "UI ACCEPTANCE FAIL");
            GetTree().Quit(passed ? 0 : 1);
        }
    }

    private void ValidateIsolation()
    {
        if (!OS.IsDebugBuild() || !OS.GetCmdlineUserArgs().Contains("--ui-layout-acceptance"))
            throw new InvalidOperationException("Use Tools/Godot/ui_acceptance.py in a debug build.");
        var root = OS.GetEnvironment("OMR_EXPECTED_USER_ROOT");
        var evidence = OS.GetEnvironment("OMR_UI_EVIDENCE_DIR");
        if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(evidence))
            throw new InvalidOperationException("Missing isolated UI profile.");
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var user = Path.GetFullPath(OS.GetUserDataDir());
        var artifacts = Path.GetFullPath(Path.Combine(ProjectSettings.GlobalizePath("res://"), "..", ".artifacts", "godot"))
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullRoot.StartsWith(artifacts, comparison) || !user.StartsWith(fullRoot, comparison)
            || !string.Equals(Path.GetFullPath(evidence) + Path.DirectorySeparatorChar, fullRoot, comparison)
            || !File.Exists(Path.Combine(root, "ui-acceptance.marker")))
            throw new InvalidOperationException("Unsafe profile/evidence path; no UI actions will be run.");
        _evidence = Path.GetFullPath(evidence);
    }

    private static IEnumerable<Node> Descendants(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            yield return child;
            foreach (var nested in Descendants(child)) yield return nested;
        }
    }

    private static bool Encloses(Rect2 outer, Rect2 inner) => inner.Size.X > 0 && inner.Size.Y > 0
        && outer.Grow(1).Encloses(inner);

    private bool VisibleTarget(Control control)
    {
        if (!control.IsVisibleInTree()) return false;
        var rect = control.GetGlobalRect();
        if (!Encloses(GetViewport().GetVisibleRect(), rect)) return false;
        for (var parent = control.GetParent(); parent is not null; parent = parent.GetParent())
            if (parent is Control { ClipContents: true } clip && !Encloses(clip.GetGlobalRect(), rect)) return false;
        var physical = GetViewport().GetFinalTransform() * rect;
        return physical.Size.Y >= 23.5f && physical.Size.X >= 23.5f;
    }

    private async Task Click(Control control)
    {
        var point = control.GetGlobalRect().GetCenter();
        GetViewport().PushInput(new InputEventMouseMotion { Position = point, GlobalPosition = point }, true);
        GetViewport().PushInput(new InputEventMouseButton { Position = point, GlobalPosition = point,
            ButtonIndex = MouseButton.Left, Pressed = true }, true);
        await Frames(1);
        GetViewport().PushInput(new InputEventMouseButton { Position = point, GlobalPosition = point,
            ButtonIndex = MouseButton.Left, Pressed = false }, true);
        await Frames(3);
    }

    private async Task Stroke(Key key, char unicode = '\0')
    {
        GetViewport().PushInput(new InputEventKey { Keycode = key, PhysicalKeycode = key,
            Unicode = unicode, Pressed = true }, true);
        await Frames(1);
        GetViewport().PushInput(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = false }, true);
        await Frames(3);
    }

    private async Task Frames(int count)
    {
        for (var i = 0; i < count; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    private async Task Capture(string name)
    {
        if (!_rendered) return;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        using var image = GetViewport().GetTexture().GetImage();
        if (image is null || image.IsEmpty() || image.SavePng(Path.Combine(_evidence, name + ".png")) != Error.Ok)
            throw new IOException("Could not capture " + name);
        _captures.Add(name + ".png");
    }

    private void Check(bool passed, string message)
    {
        _checks.Add(new CheckResult(passed, message));
        GD.Print($"UI {(passed ? "OK" : "FAIL")} {message}");
    }
}
