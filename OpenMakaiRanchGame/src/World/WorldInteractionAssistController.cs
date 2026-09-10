using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Unified ranch/town interaction affordance. Displays the current target and remapped input,
/// exposes a mouse/touch action button, and highlights only the active in-range target.
/// </summary>
public partial class WorldInteractionAssistController : Node
{
    [Export] public float NearbyPromptMultiplier { get; set; } = 2f;

    private WorldGameController? _world;
    private CanvasLayer? _canvas;
    private PanelContainer? _panel;
    private Label? _label;
    private Button? _actionButton;
    private MeshInstance3D? _highlight;
    private double _pulseTime;

    public override void _Ready()
    {
        _world = GetParent() as WorldGameController;
        InputBindingService.EnsureApplied();
        BuildHud();
        BuildHighlight();
        HideLegacyPrompts();
    }

    public override void _Process(double delta)
    {
        if (_world is null || !GodotObject.IsInstanceValid(_world))
        {
            SetVisible(false);
            return;
        }

        var blocked = _world.IsManagementVisible
            || _world.PauseMenu?.IsOpen == true
            || _world.Transition?.IsTransitioning == true
            || _world.FirstDayFlow?.BlocksWorldInput == true
            || _world.FlowLocksUi;
        if (blocked || _world.ActiveAreaId is not ("ranch" or "town"))
        {
            SetVisible(false);
            return;
        }

        var presentation = _world.ActiveAreaId == "town"
            ? _world.Town?.GetInteractionPresentation() ?? WorldInteractionPresentation.None(2.5f)
            : _world.Ranch?.GetInteractionPresentation() ?? WorldInteractionPresentation.None(2.5f);

        RefreshHud(presentation);
        RefreshHighlight(presentation, delta);
    }

    private void BuildHud()
    {
        _canvas = new CanvasLayer { Name = "InteractionAssistCanvas", Layer = 45 };
        AddChild(_canvas);

        var root = new Control { Name = "Root", MouseFilter = Control.MouseFilterEnum.Ignore };
        root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _canvas.AddChild(root);

        _panel = new PanelContainer { Name = "InteractionPanel", MouseFilter = Control.MouseFilterEnum.Pass };
        _panel.AnchorLeft = _panel.AnchorRight = .5f;
        _panel.AnchorTop = _panel.AnchorBottom = 1f;
        _panel.OffsetLeft = -390; _panel.OffsetRight = 390; _panel.OffsetTop = -104; _panel.OffsetBottom = -36;
        _panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(.025f, .04f, .065f, .92f),
            BorderColor = new Color(.34f, .76f, .72f, .72f),
            BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10, CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10,
            ContentMarginLeft = 12, ContentMarginRight = 12, ContentMarginTop = 8, ContentMarginBottom = 8
        });
        root.AddChild(_panel);

        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 12);
        _panel.AddChild(row);

        _label = new Label
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        row.AddChild(_label);

        _actionButton = new Button { CustomMinimumSize = new Vector2(190, 44), FocusMode = Control.FocusModeEnum.None };
        _actionButton.Pressed += ActivateCurrentTarget;
        row.AddChild(_actionButton);
    }

    private void BuildHighlight()
    {
        _highlight = new MeshInstance3D
        {
            Name = "InteractionHighlight",
            Mesh = new CylinderMesh { TopRadius = .72f, BottomRadius = .72f, Height = .025f, RadialSegments = 40 },
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = new Color(.30f, .95f, .82f, .42f),
                EmissionEnabled = true,
                Emission = new Color(.18f, .72f, .62f),
                EmissionEnergyMultiplier = 1.35f,
                Roughness = .35f,
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
            },
            Visible = false,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        AddChild(_highlight);
    }

    private void HideLegacyPrompts()
    {
        var ranchPrompt = _world?.Ranch?.GetNodeOrNull<Label>("WorldHud/Prompt");
        if (ranchPrompt is not null) ranchPrompt.Visible = false;
        var townPrompt = _world?.Town?.GetNodeOrNull<Label>("TownHud/Prompt");
        if (townPrompt is not null) townPrompt.Visible = false;
    }

    private void RefreshHud(WorldInteractionPresentation presentation)
    {
        if (_panel is null || _label is null || _actionButton is null) return;
        if (_canvas is not null) _canvas.Visible = true;
        _panel.Visible = true;

        if (!presentation.HasTarget || presentation.Distance > presentation.InteractionRange * Mathf.Max(1f, NearbyPromptMultiplier))
        {
            _label.Text = $"Interact {InputBindingService.GetCombinedLabel("interact")}  •  Management {InputBindingService.GetCombinedLabel("toggle_management")}  •  Pause {InputBindingService.GetCombinedLabel("pause_menu")}";
            _actionButton.Visible = false;
            return;
        }

        if (!presentation.InRange)
        {
            _label.Text = $"{presentation.Label}  •  {presentation.Distance:0.0} m  •  move closer";
            _actionButton.Visible = false;
            return;
        }

        if (!presentation.Available)
        {
            _label.Text = string.IsNullOrWhiteSpace(presentation.UnavailableReason)
                ? $"{presentation.Label} is currently unavailable."
                : $"{presentation.Label}  •  {presentation.UnavailableReason}";
            _actionButton.Visible = false;
            return;
        }

        var binding = InputBindingService.GetCombinedLabel("interact");
        _label.Text = presentation.Label;
        _actionButton.Text = $"[{binding}]  Interact";
        _actionButton.TooltipText = $"Interact using {binding}, or click/tap this button.";
        _actionButton.Visible = true;
    }

    private void RefreshHighlight(WorldInteractionPresentation presentation, double delta)
    {
        if (_highlight is null) return;
        var show = InputBindingService.InteractionHighlightEnabled && presentation.HasTarget && presentation.InRange;
        _highlight.Visible = show;
        if (!show || presentation.TargetNode is null) return;

        _pulseTime += delta;
        var pulse = 1f + Mathf.Sin((float)_pulseTime * 4.2f) * .08f;
        _highlight.Scale = new Vector3(pulse, 1, pulse);
        var p = presentation.TargetNode.GlobalPosition;
        _highlight.GlobalPosition = new Vector3(p.X, p.Y + .04f, p.Z);
    }

    private void ActivateCurrentTarget()
    {
        if (_world is null || _world.IsManagementVisible || _world.PauseMenu?.IsOpen == true) return;
        if (_world.ActiveAreaId == "town") _world.Town?.TryInteract();
        else if (_world.ActiveAreaId == "ranch") _world.Ranch?.TryInteractWithNearestWorldTarget();
    }

    private void SetVisible(bool visible)
    {
        if (_canvas is not null) _canvas.Visible = visible;
        if (_highlight is not null) _highlight.Visible = false;
    }
}
