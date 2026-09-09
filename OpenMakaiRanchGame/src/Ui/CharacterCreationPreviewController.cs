using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Character;

namespace OpenMakaiRanch.Ui;

/// <summary>
/// Controller for the mixed 2D/3D character-creation screen. The existing controls remain ordinary
/// Godot 2D UI; a SubViewport renders the same safe player stand-in used by the world.
///
/// This controller owns presentation only. All edits still go through GameRoot/UiShellController.
/// </summary>
public partial class CharacterCreationPreviewController : VBoxContainer
{
    [Export] public NodePath AvatarPath { get; set; } = "CreationBody/PreviewCard/PreviewInner/PreviewFrame/PreviewViewport/PreviewWorld/Avatar";
    [Export] public NodePath CameraPath { get; set; } = "CreationBody/PreviewCard/PreviewInner/PreviewFrame/PreviewViewport/PreviewWorld/Camera";
    [Export] public NodePath SummaryPath { get; set; } = "CreationBody/PreviewCard/PreviewInner/PreviewSummary";
    [Export] public NodePath RotateLeftPath { get; set; } = "CreationBody/PreviewCard/PreviewInner/PreviewActions/RotateLeftButton";
    [Export] public NodePath ResetPath { get; set; } = "CreationBody/PreviewCard/PreviewInner/PreviewActions/ResetButton";
    [Export] public NodePath RotateRightPath { get; set; } = "CreationBody/PreviewCard/PreviewInner/PreviewActions/RotateRightButton";

    private PlayerAvatar3D? _avatar;
    private Camera3D? _camera;
    private Label? _summary;
    private Button? _rotateLeft;
    private Button? _reset;
    private Button? _rotateRight;

    public bool PreviewReady => _avatar is not null && _camera is not null;
    public PlayerAvatar3D? Avatar => _avatar;

    public override void _Ready()
    {
        _avatar = GetNodeOrNull<PlayerAvatar3D>(AvatarPath);
        _camera = GetNodeOrNull<Camera3D>(CameraPath);
        _summary = GetNodeOrNull<Label>(SummaryPath);
        _rotateLeft = GetNodeOrNull<Button>(RotateLeftPath);
        _reset = GetNodeOrNull<Button>(ResetPath);
        _rotateRight = GetNodeOrNull<Button>(RotateRightPath);

        if (_avatar is null || _camera is null)
        {
            GD.PushError("CharacterCreationPreviewController could not bind its 3D preview nodes.");
            return;
        }

        if (_rotateLeft is not null)
        {
            _rotateLeft.Pressed += RotateLeft;
            _rotateLeft.TooltipText = "Rotate the 3D preview left without changing your saved appearance.";
        }
        if (_reset is not null)
        {
            _reset.Pressed += ResetRotation;
            _reset.TooltipText = "Return the 3D preview to its default front-facing rotation.";
        }
        if (_rotateRight is not null)
        {
            _rotateRight.Pressed += RotateRight;
            _rotateRight.TooltipText = "Rotate the 3D preview right without changing your saved appearance.";
        }

        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged += RefreshSummary;
            _avatar.RefreshFrom(game.State.Player);
            RefreshPreviewCamera(game.State.Player);
            RefreshSummary();
        }
    }

    public override void _ExitTree()
    {
        if (_rotateLeft is not null && GodotObject.IsInstanceValid(_rotateLeft))
        {
            _rotateLeft.Pressed -= RotateLeft;
        }
        if (_reset is not null && GodotObject.IsInstanceValid(_reset))
        {
            _reset.Pressed -= ResetRotation;
        }
        if (_rotateRight is not null && GodotObject.IsInstanceValid(_rotateRight))
        {
            _rotateRight.Pressed -= RotateRight;
        }

        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged -= RefreshSummary;
        }
    }

    public void RotateLeft()
    {
        _avatar?.RotateY(Mathf.DegToRad(-22.5f));
    }

    public void RotateRight()
    {
        _avatar?.RotateY(Mathf.DegToRad(22.5f));
    }

    public void ResetRotation()
    {
        if (_avatar is not null)
        {
            _avatar.Rotation = Vector3.Zero;
        }
    }

    private void RefreshPreviewCamera(OpenMakaiRanch.Core.Models.PlayerState player)
    {
        if (_camera is null)
        {
            return;
        }

        var heightMeters = Mathf.Clamp(player.Height / 1000f, 1.45f, 2.25f);
        var focusHeight = heightMeters * 0.52f;
        var distance = Mathf.Lerp(3.6f, 4.9f, Mathf.InverseLerp(1.45f, 2.25f, heightMeters));
        _camera.Position = new Vector3(0f, focusHeight + 0.12f, distance);
        _camera.LookAt(new Vector3(0f, focusHeight, 0f), Vector3.Up);
    }

    private void RefreshSummary()
    {
        if (_summary is null || GameRoot.Instance is not { } game || !GodotObject.IsInstanceValid(game))
        {
            return;
        }

        var player = game.State.Player;
        RefreshPreviewCamera(player);
        _summary.Text = $"{player.Name}  •  {player.Race}  •  {player.Height / 10f:0} cm\n3D preview uses a neutral placeholder model until final player assets exist.";
    }
}
