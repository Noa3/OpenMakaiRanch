using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

/// <summary>Deterministic control/lifecycle checks. OS cursor capture and physical pads still need a rendered playtest.</summary>
public static class PlayabilityRegressionTests
{
    public static void Run(SmokeTestResult result)
    {
        var game = GameRoot.Instance;
        var settings = game.State.Settings.Clone();
        try
        {
            TestAnalogMovement(result, game);
            TestCameraLifecycle(result, game);
            TestFirstDaySkip(result, game);
        }
        finally
        {
            game.GetTree().Paused = false;
            game.State.Settings = settings;
            GameRoot.PendingInitialScreen = null;
            game.NewGame();
        }
    }

    private static void TestAnalogMovement(SmokeTestResult result, GameRoot game)
    {
        InputBindingService.EnsureApplied();
        var gate = new WorldInputGate();
        var changes = 0;
        gate.InputStateDidChange += () => changes++;
        gate.SetUiOwnsInput(true);
        gate.Reset();
        Check(result, changes == 2 && gate.WorldInputEnabled, "gate reset publishes restored input once");
        gate.Reset();
        Check(result, changes == 2, "unchanged gate reset does not send redundant notifications");

        var player = new ThirdPersonPlayerController();
        var firstCamera = new Camera3D { Rotation = new Vector3(0f, 0.6f, 0f) };
        var secondCamera = new Camera3D { Rotation = new Vector3(0f, -0.8f, 0f) };
        var preview = new SubViewport { Size = new Vector2I(64, 64), OwnWorld3D = true };
        var priorCamera = game.GetViewport().GetCamera3D();
        var fixture = new Node3D();
        fixture.AddChild(player);
        fixture.AddChild(firstCamera);
        fixture.AddChild(secondCamera);
        fixture.AddChild(preview);
        preview.AddChild(new Camera3D { Current = true });
        game.AddChild(fixture);
        try
        {
            player.InputGate = gate;
            firstCamera.MakeCurrent();
            player.RefreshCameraBasis();
            Check(result, player.CameraBasisForward.IsEqualApprox(-firstCamera.GlobalTransform.Basis.Z),
                "movement uses the main viewport camera, not a preview SubViewport");
            secondCamera.MakeCurrent();
            player.RefreshCameraBasis();
            Check(result, player.CameraBasisForward.IsEqualApprox(-secondCamera.GlobalTransform.Basis.Z),
                "camera changes refresh movement orientation without a stale cache");

            Input.ActionPress("move_forward", 0.65f);
            var softInput = player.ReadMovementInput();
            var softVelocity = player.ComputeDesiredVelocity(softInput, false);
            Input.ActionPress("move_forward", 1f);
            var fullVelocity = player.ComputeDesiredVelocity(player.ReadMovementInput(), false);
            Check(result, softInput.Y > 0f && softInput.Length() < 1f && softVelocity.Length() < fullVelocity.Length(),
                "partial mapped action strength gives slower movement");
            Check(result, Math.Abs(fullVelocity.Length() - player.MaxWalkSpeed) < 0.001f,
                "full keyboard input retains the normal walk speed");
            Input.ActionPress("move_right", 1f);
            Check(result, player.ComputeDesiredVelocity(player.ReadMovementInput(), false).Length() <= player.MaxWalkSpeed + 0.001f,
                "keyboard diagonal movement cannot exceed straight-line speed");
            Input.ActionRelease("move_forward");
            Input.ActionRelease("move_right");
            player.MobileMovementInput = new Vector2(0f, 0.25f);
            Check(result, Math.Abs(player.ComputeDesiredVelocity(player.ReadMovementInput(), false).Length() - player.MaxWalkSpeed * 0.25f) < 0.001f,
                "quarter-strength touch input remains quarter-speed after direction normalization");

            player.Velocity = new Vector3(3f, -2f, 4f);
            player.MobileSprintHeld = true;
            gate.SetUiOwnsInput(true);
            Check(result, player.Velocity == new Vector3(0f, -2f, 0f)
                && player.MobileMovementInput == Vector2.Zero && !player.MobileSprintHeld,
                "opening UI immediately clears horizontal drift and held touch input, preserving gravity");
            Check(result, player.ReadMovementInput() == Vector2.Zero, "UI blocks mapped and touch movement");
            gate.SetUiOwnsInput(false);
            Input.ActionPress("move_forward", 1f);
            player._Notification((int)Node.NotificationApplicationFocusOut);
            gate.Reset();
            Check(result, player.ReadMovementInput() == Vector2.Zero,
                "an area gate reset cannot reenable movement while application focus is still lost");
            player._Notification((int)Node.NotificationApplicationFocusIn);
            Check(result, player.ReadMovementInput().Y > 0f, "focus return restores mapped movement");
        }
        finally
        {
            Input.ActionRelease("move_forward");
            Input.ActionRelease("move_right");
            fixture.Free();
            if (priorCamera is not null && GodotObject.IsInstanceValid(priorCamera) && priorCamera.IsInsideTree()) priorCamera.MakeCurrent();
        }
        gate.SetUiOwnsInput(true);
        gate.Reset();
        Check(result, gate.WorldInputEnabled, "a retained gate can change after its player was freed without a disposed-node callback");
    }

    private static void TestCameraLifecycle(SmokeTestResult result, GameRoot game)
    {
        game.NewGame();
        game.State.Calendar.Day = 2; // Explicit presentation fixture; not a gameplay progression test.
        game.State.Story.FirstDayCompleted = true;
        game.State.Story.FirstDayStage = FirstDayFlowController.StageCompleted;
        game.State.Settings.InvertCameraY = false;
        game.State.Settings.ReducedMotion = false;
        var world = CreateWorld(game);
        var gate = world.Ranch!.InputGate;
        try
        {
            var rig = world.Ranch.CameraRig!;
            var player = world.Ranch.Player!;
            rig.SetFirstPerson(true);
            Check(result, rig.OwnsMouseCapture, "active first-person camera claims mouse ownership");
            world.PauseMenu!.Open("ranch");
            Check(result, !rig.OwnsMouseCapture, "pause releases camera ownership before processing stops");
            world.PauseMenu.Close();
            rig.RefreshMouseCapture();
            Check(result, rig.OwnsMouseCapture, "resume reacquires first-person ownership without a stale captured latch");
            world.OpenManagementScreen("schedule");
            Check(result, !rig.OwnsMouseCapture, "management releases the cursor synchronously through the shared input gate");
            world.CloseManagement();
            rig.RefreshMouseCapture();
            Check(result, rig.OwnsMouseCapture, "closing management restores active camera ownership");

            rig._Notification((int)Node.NotificationApplicationFocusOut);
            gate.Reset();
            rig.RefreshMouseCapture();
            Check(result, !rig.OwnsMouseCapture, "gate reset cannot capture the mouse in an unfocused application");
            rig._Notification((int)Node.NotificationApplicationFocusIn);
            rig.RefreshMouseCapture();
            Check(result, rig.OwnsMouseCapture, "focus return permits active first-person capture again");

            var target = rig.Target;
            var before = rig.Camera!.GlobalTransform;
            var detached = new Node3D { Position = new Vector3(100f, 20f, 30f) };
            rig.Target = detached;
            rig._Process(0.016);
            Check(result, !rig.OwnsMouseCapture && rig.Camera.GlobalTransform.IsEqualApprox(before),
                "detached targets cannot write local coordinates into a live world camera");
            detached.Free();
            rig._Process(0.016);
            rig.Target = target;
            rig.RefreshMouseCapture();
            Check(result, rig.OwnsMouseCapture, "camera can recover after a detached or freed target is replaced");

            rig.SetOrbit(0f, 0f, 5f);
            rig.ApplyLookDelta(new Vector2(0f, -20f));
            Check(result, WorldCameraMath.ComputeViewDirection(rig.Yaw, rig.Pitch).Y > 0f,
                "non-inverted mouse-up looks up with the orbit coordinate convention");
            game.State.Settings.InvertCameraY = true;
            rig.ApplyUserSettings();
            rig.SetOrbit(0f, 0f, 5f);
            rig.ApplyLookDelta(new Vector2(0f, -20f));
            Check(result, WorldCameraMath.ComputeViewDirection(rig.Yaw, rig.Pitch).Y < 0f,
                "Invert Y reverses mouse look explicitly");
            game.State.Settings.InvertCameraY = false;
            rig.ApplyUserSettings();
            rig.SetOrbit(0f, 0f, 5f);
            rig.ApplyStickLook(new Vector2(0.25f, 0f), 0.1f);
            var quarterTurn = Math.Abs(rig.Yaw);
            rig.SetOrbit(0f, 0f, 5f);
            rig.ApplyStickLook(Vector2.Right, 0.1f);
            Check(result, quarterTurn > 0f && Math.Abs(Math.Abs(rig.Yaw) - quarterTurn * 4f) < 0.0001f,
                "analog camera turn rate scales continuously with stick strength");

            rig.SetFirstPerson(false);
            player.Rotation = new Vector3(0f, 0.73f, 0f);
            rig.SetOrbit(-2f, -0.5f, 5f);
            rig.RequestRecenter();
            for (var i = 0; i < 180; i++) rig._Process(1.0 / 60.0);
            var expectedYaw = Mathf.Atan2(player.GlobalTransform.Basis.Z.Z, player.GlobalTransform.Basis.Z.X);
            Check(result, !rig.IsRecentering && Math.Abs(Mathf.Wrap(rig.Yaw - expectedYaw, -Mathf.Pi, Mathf.Pi)) < 0.005f,
                "one recenter request completes behind the player's facing direction");
            Check(result, Math.Abs(rig.Pitch - Mathf.DegToRad(30f)) < 0.005f && Math.Abs(rig.Distance - 5f) < 0.001f,
                "recenter restores pitch but preserves player-selected zoom");
            rig.RequestRecenter();
            rig.ApplyLookDelta(new Vector2(15f, 0f));
            Check(result, !rig.IsRecentering, "manual look cancels an in-progress recenter");
            game.State.Settings.ReducedMotion = true;
            rig.ApplyUserSettings();
            rig.SetOrbit(-2f, -0.5f, 5f);
            rig.RequestRecenter();
            Check(result, !rig.IsRecentering && Math.Abs(rig.Yaw - expectedYaw) < 0.001f,
                "reduced motion recenters immediately rather than animating the camera");

            rig.SetFirstPerson(true);
            Check(result, world.TravelTo("town"), "camera lifecycle fixture can travel to town");
            world.Transition?.CompleteImmediately();
            var townRig = world.Town!.CameraRig!;
            townRig.SetFirstPerson(true);
            rig.RefreshMouseCapture();
            Check(result, townRig.OwnsMouseCapture && !rig.OwnsMouseCapture,
                "inactive ranch camera cannot recapture or release the town camera's mouse");
            townRig.ProcessMode = Node.ProcessModeEnum.Disabled;
            Check(result, !townRig.OwnsMouseCapture, "disabling a camera releases its mouse without waiting for another frame");
            townRig.ProcessMode = Node.ProcessModeEnum.Inherit;
            townRig.RefreshMouseCapture();
            Check(result, townRig.OwnsMouseCapture, "reenabled active camera can reacquire ownership");
        }
        finally
        {
            game.GetTree().Paused = false;
            world.Free();
        }
        gate.SetUiOwnsInput(true);
        gate.Reset();
        Check(result, gate.WorldInputEnabled, "camera gate callbacks are detached when the world is freed");
    }

    private static void TestFirstDaySkip(SmokeTestResult result, GameRoot game)
    {
        game.NewGame();
        game.State.Settings.ReducedMotion = false;
        var world = CreateWorld(game);
        try
        {
            var flow = world.FirstDayFlow!;
            flow.RefreshFromCurrentState();
            world.Transition?.CompleteImmediately();
            flow._Process(0.016);
            var gold = game.Economy.Gold;
            var stamina = game.State.Player.Stamina;
            Press(flow, "Skip First Day → Night");
            Press(flow, "Skip to Night");
            world.Transition?.CompleteImmediately();
            Check(result, game.State.Calendar.Day == 1 && game.State.Calendar.Phase == DayPhase.Night
                && flow.CurrentStage == FirstDayFlowController.StageNightRoutine,
                "skip reaches the first Night without settling or incrementing the day");
            Check(result, game.Economy.Gold == gold && game.State.Player.Stamina == stamina,
                "skipping the tutorial grants no gold and does not drain daily stamina");
            Check(result, flow.BlocksWorldInput && world.Ranch!.InputGate.UiOwnsInput,
                "finishing the area fade must not unlock movement behind the night-choice dialogue");
            Press(flow, "Take a hot bath, then sleep");
            Press(flow, "End Day");
            Check(result, game.State.Calendar.Day == 2 && game.State.Story.FirstDayCompleted
                && world.Shell!.CurrentScreen == "report" && world.IsManagementVisible,
                "skip, bath and End Day reach the normal Day 2 report through existing services");
            Check(result, game.State.Player.DailyStaminaBonus == PlayerStaminaService.HotBathNextDayBonus,
                "the first-night bath supplies the next-day bonus rather than an immediate refill");
        }
        finally { world.Free(); }
    }

    internal static WorldGameController CreateWorld(GameRoot game)
    {
        GameRoot.PendingInitialScreen = "ranch";
        var world = GD.Load<PackedScene>("res://scenes/WorldGame.tscn").Instantiate<WorldGameController>();
        game.AddChild(world);
        world.Transition?.CompleteImmediately();
        return world;
    }

    internal static IEnumerable<Button> Buttons(Node node)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is Button button) yield return button;
            foreach (var nested in Buttons(child)) yield return nested;
        }
    }

    internal static void Press(Node root, string text)
    {
        var button = Buttons(root).FirstOrDefault(value => value.Text == text && !value.Disabled && value.IsVisibleInTree());
        if (button is null) throw new InvalidOperationException($"Playable button unavailable: {text}");
        button.EmitSignal(BaseButton.SignalName.Pressed);
    }

    internal static void Check(SmokeTestResult result, bool passed, string message)
    {
        result.Passed &= passed;
        result.Lines.Add($"SMOKE {(passed ? "OK" : "FAIL")} playability: {message}");
    }
}
