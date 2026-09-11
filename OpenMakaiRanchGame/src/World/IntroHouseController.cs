using System;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Small first-day bedroom world. It exists only for the guided opening and shares the same player
/// movement/camera implementation as Ranch/Town.
/// </summary>
public partial class IntroHouseController : Node3D
{
    [Export] public float DoorInteractionRange { get; set; } = 2.2f;

    public WorldInputGate InputGate { get; } = new();

    private ThirdPersonPlayerController? _player;
    private WorldCameraRig? _cameraRig;
    private Node3D? _doorPoint;
    private Node3D? _wakeVisual;
    private Label? _prompt;
    private bool _doorEnabled;

    public ThirdPersonPlayerController? Player => _player;
    public WorldCameraRig? CameraRig => _cameraRig;
    public bool DoorEnabled => _doorEnabled;

    public event Action? ExitRequested;

    public override void _Ready()
    {
        _player = GetNodeOrNull<ThirdPersonPlayerController>("Player");
        _cameraRig = GetNodeOrNull<WorldCameraRig>("CameraRig");
        _doorPoint = GetNodeOrNull<Node3D>("ExitDoorPoint");
        _wakeVisual = GetNodeOrNull<Node3D>("WakeVisual");
        _prompt = GetNodeOrNull<Label>("IntroHud/Prompt");

        if (_player is not null)
        {
            _player.InputGate = InputGate;
            var target = _player.EnsureCameraTarget();
            if (_cameraRig is not null)
            {
                _cameraRig.InputGate = InputGate;
                _cameraRig.Target = target;
            }
        }

        var alreadyAwake = GameRoot.Instance?.State.Story.FirstDayStage >= FirstDayFlowController.StageLeaveBedroom;
        SetWakePresentation(!alreadyAwake);
        SetDoorEnabled(alreadyAwake);
    }

    public void FinishWakeUp()
    {
        SetWakePresentation(false);
        if (_player is not null)
        {
            _player.GlobalPosition = new Vector3(-1.0f, 0.8f, -0.2f);
        }
        SetDoorEnabled(true);
    }

    public void SynchronizeWakeState(bool awake)
    {
        SetWakePresentation(!awake);
        SetDoorEnabled(awake);
    }

    private void SetWakePresentation(bool sleeping)
    {
        if (_wakeVisual is not null)
        {
            _wakeVisual.Visible = sleeping;
        }

        if (_player is not null)
        {
            _player.Visible = !sleeping;
            _player.ProcessMode = sleeping ? ProcessModeEnum.Disabled : ProcessModeEnum.Inherit;
        }
    }

    public override void _Process(double delta)
    {
        if (!_doorEnabled || _player is null || _doorPoint is null)
        {
            return;
        }

        var distance = _player.GlobalPosition.DistanceTo(_doorPoint.GlobalPosition);
        if (_prompt is not null)
        {
            _prompt.Text = distance <= DoorInteractionRange
                ? $"[{InputBindingService.GetCombinedLabel("interact")}] Follow your childhood friend outside"
                : $"Bedroom door  {distance:0.0} m";
        }

        if (Input.IsActionJustPressed("interact")) TryInteract();
    }

    public bool TryInteract()
    {
        if (!IsVisibleInTree() || !CanProcess() || !_doorEnabled || !InputGate.WorldInputEnabled
            || _player is null || _doorPoint is null
            || _player.GlobalPosition.DistanceTo(_doorPoint.GlobalPosition) > DoorInteractionRange) return false;
        ExitRequested?.Invoke();
        return true;
    }

    public void SetDoorEnabled(bool enabled)
    {
        _doorEnabled = enabled;
        if (_prompt is not null)
        {
            _prompt.Visible = enabled;
            if (enabled)
            {
                _prompt.Text = $"Walk to the bedroom door and press {InputBindingService.GetCombinedLabel("interact")}.";
            }
        }
    }

    public void EnterStoryUi() => InputGate.SetUiOwnsInput(true);
    public void LeaveStoryUi() => InputGate.SetUiOwnsInput(false);
}
