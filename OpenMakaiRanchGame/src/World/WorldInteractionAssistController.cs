using System;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Unified world interaction affordance for ranch and town. It mirrors the target selected by the
/// area controllers, exposes a mouse/touch action button, displays the current remapped binding,
/// and adds one lightweight highlight marker to the target currently in interaction range.
/// Gameplay remains owned by the existing area controllers.
/// </summary>
public partial class WorldInteractionAssistController : Node
{
    [Export] public float NearbyPromptMultiplier { get; set; } = 2.0f;

    private WorldGameController? _world;
    private CanvasLayer? _canvas;
    private PanelContainer? _panel;
    private Label? _label;
    private Button? _actionButton;
    private MeshInstance3D? _highlight;
    private StandardMaterial3D? _highlightMaterial;
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

        // The overlay never competes with management, pause, transition or mandatory story UI.
        var blocked = _world.IsManagementVisible
            || _world.PauseMenu?.IsOpen == true
            || _world.Transition?.IsTransitioning == true
            || _world.FirstDayFlow?.BlocksWorldInput == true
            || _world.FlowLocksUi || _world.WorldHelpVisible;
        if (blocked || _world.ActiveAreaId is not ("ranch" or "town"))
        {
            SetVisible(false);
            return;
        }

        var presentation = _world.ActiveAreaId == "town"
            ? _world.Town?.GetInteractionPresentation() ?? WorldInteractionPresentation.None(2.5f)
            : _world.Ranch?.GetInteractionPresentation() ?? WorldInteractionPresentation.None(2.5f);

        RefreshHud(presentation);
        RefreshHudLayout();
        RefreshHighlight(presentation, delta);
    }

    private void BuildHud()
    {
        _canvas = new CanvasLayer { Name = "InteractionAssistCanvas", Layer = 45 };
        AddChild(_canvas);

        var root = new Control
        {
            Name = "Root",
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _canvas.AddChild(root);

        _panel = new PanelContainer
        {
            Name = "InteractionPanel",
            MouseFilter = Control.MouseFilterEnum.Pass
        };
        _panel.AnchorLeft = 0.5f;
        _panel.AnchorRight = 0.5f;
        _panel.AnchorTop = 1f;
        _panel.AnchorBottom = 1f;
        _panel.OffsetLeft = -390f;
        _panel.OffsetRight = 390f;
        _panel.OffsetTop = -104f;
        _panel.OffsetBottom = -36f;
        _panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.025f, 0.04f, 0.065f, 0.92f),
            BorderColor = new Color(0.34f, 0.76f, 0.72f, 0.72f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 10,
            CornerRadiusTopRight = 10,
            CornerRadiusBottomLeft = 10,
            CornerRadiusBottomRight = 10,
            ContentMarginLeft = 12,
            ContentMarginRight = 12,
            ContentMarginTop = 8,
            ContentMarginBottom = 8
        });
        root.AddChild(_panel);

        var row = new BoxContainer { Name = "Row" };
        row.AddThemeConstantOverride("separation", 12);
        _panel.AddChild(row);

        _label = new Label
        {
            Name = "PromptLabel",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        row.AddChild(_label);

        _actionButton = new Button
        {
            Name = "ActionButton",
            CustomMinimumSize = new Vector2(190f, 44f),
            FocusMode = Control.FocusModeEnum.None
        };
        _actionButton.Pressed += ActivateCurrentTarget;
        row.AddChild(_actionButton);
    }

    private void BuildHighlight()
    {
        _highlightMaterial = new StandardMaterial3D
        {
            AlbedoColor = new Color(0.30f, 0.95f, 0.82f, 0.42f),
            EmissionEnabled = true,
            Emission = new Color(0.18f, 0.72f, 0.62f),
            EmissionEnergyMultiplier = 1.35f,
            Roughness = 0.35f,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            NoDepthTest = false
        };

        _highlight = new MeshInstance3D
        {
            Name = "InteractionHighlight",
            Mesh = new CylinderMesh
            {
                TopRadius = 0.72f,
                BottomRadius = 0.72f,
                Height = 0.025f,
                RadialSegments = 40
            },
            MaterialOverride = _highlightMaterial,
            Visible = false,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        AddChild(_highlight);
    }

    private void HideLegacyPrompts()
    {
        var ranchPrompt = _world?.Ranch?.GetNodeOrNull<Label>("WorldHud/Prompt");
        if (ranchPrompt is not null)
        {
            ranchPrompt.Visible = false;
        }

        var townPrompt = _world?.Town?.GetNodeOrNull<Label>("TownHud/Prompt");
        if (townPrompt is not null)
        {
            townPrompt.Visible = false;
        }
    }

    private void RefreshHud(WorldInteractionPresentation presentation)
    {
        if (_panel is null || _label is null || _actionButton is null)
        {
            return;
        }

        if (_canvas is not null)
        {
            _canvas.Visible = true;
        }
        _panel.Visible = true;
        var interactBinding = InputBindingService.GetCombinedLabel("interact");

        if (!presentation.HasTarget
            || presentation.Distance > presentation.InteractionRange * Mathf.Max(1f, NearbyPromptMultiplier))
        {
            _label.Text = BuildGeneralControlHint();
            _label.TooltipText = "Controls use your current keyboard/controller bindings.";
            _actionButton.Visible = false;
            return;
        }

        if (!presentation.InRange)
        {
            _label.Text = $"{presentation.Label}  •  {presentation.Distance:0.0} m  •  move closer";
            _label.TooltipText = $"Move within {presentation.InteractionRange:0.0} m to interact.";
            _actionButton.Visible = false;
            return;
        }

        if (!presentation.Available)
        {
            _label.Text = string.IsNullOrWhiteSpace(presentation.UnavailableReason)
                ? $"{presentation.Label} is currently unavailable."
                : $"{presentation.Label}  •  {presentation.UnavailableReason}";
            _label.TooltipText = presentation.UnavailableReason;
            _actionButton.Visible = false;
            return;
        }

        _label.Text = presentation.Label;
        _label.TooltipText = $"Use {presentation.Label}.";
        _actionButton.Text = $"[{interactBinding}]  Interact";
        _actionButton.TooltipText = $"Interact using {interactBinding}, or click/tap this button.";
        _actionButton.Visible = true;
    }

    private string BuildGeneralControlHint()
    {
        var interact = InputBindingService.GetCombinedLabel("interact");
        var management = InputBindingService.GetCombinedLabel("toggle_management");
        var pause = InputBindingService.GetCombinedLabel("pause_menu");
        var help = InputBindingService.GetCombinedLabel("open_help");
        if (GetViewport().GetVisibleRect().Size.X < 900) return $"Interact {interact}   •   Help {help}";
        return $"Interact {interact}   •   Management {management}   •   Pause {pause}   •   Help {help}";
    }

    private void RefreshHighlight(WorldInteractionPresentation presentation, double delta)
    {
        if (_highlight is null)
        {
            return;
        }

        var show = InputBindingService.InteractionHighlightEnabled
            && presentation.HasTarget
            && presentation.InRange;
        _highlight.Visible = show;
        if (!show || presentation.TargetNode is null)
        {
            return;
        }

        _pulseTime += delta;
        var pulse = 1f + Mathf.Sin((float)_pulseTime * 4.2f) * 0.08f;
        _highlight.Scale = new Vector3(pulse, 1f, pulse);
        var targetPosition = presentation.TargetNode.GlobalPosition;
        _highlight.GlobalPosition = new Vector3(targetPosition.X, targetPosition.Y + 0.04f, targetPosition.Z);
    }

    private void ActivateCurrentTarget()
    {
        if (_world is null || !_world.WorldActionsAvailable)
        {
            return;
        }

        if (_world.ActiveAreaId == "town")
        {
            _world.Town?.TryInteract();
        }
        else if (_world.ActiveAreaId == "ranch")
        {
            _world.Ranch?.TryInteractWithNearestWorldTarget();
        }
    }

    private void SetVisible(bool visible)
    {
        if (_canvas is not null)
        {
            _canvas.Visible = visible;
        }
        if (_highlight is not null)
        {
            _highlight.Visible = false;
        }
    }
}
