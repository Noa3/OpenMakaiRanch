using System;
using System.Linq;
using Godot;

namespace OpenMakaiRanch.Character;

/// <summary>
/// Presentation-only 3D placeholder bound to a stable CharacterVisualProfile.
///
/// The preferred placeholder is a project-authored generic debug mannequin. It deliberately remains marked
/// IsDebugStandIn=true and does not claim identity parity with the source character. If the external
/// resource cannot load, the old capsule/sphere geometry stays as a visible fallback.
///
/// Rebuild only replaces nodes owned by this component; navigation/nameplates/other authored children
/// are preserved.
/// </summary>
[GlobalClass]
public partial class CharacterAvatar3D : Node3D
{
    [Export] public CharacterVisualProfile? Profile { get; set; }

    /// <summary>
    /// Recovered merge contract used by capture/dev tooling. For primitive fallbacks it applies the
    /// project's soft-anime material; external authored placeholder models retain their own materials.
    /// </summary>
    [Export] public bool UseSoftShading { get; set; }

    /// <summary>Optional code-side tuning for the soft fallback material.</summary>
    public SoftShadingMath.SoftParameters? SoftParams { get; set; }

    public MeshInstance3D? Body { get; private set; }
    public MeshInstance3D? Head { get; private set; }
    public Node3D? PlaceholderModel { get; private set; }
    public bool UsesExternalPlaceholder => PlaceholderModel is not null;

    private Node3D? _ownedVisualRoot;
    private AnimationPlayer? _animationPlayer;
    private string _idleAnimation = string.Empty;
    private string _walkAnimation = string.Empty;
    private string _runAnimation = string.Empty;
    private string _currentAnimation = string.Empty;

    public override void _Ready()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        if (_ownedVisualRoot is not null && GodotObject.IsInstanceValid(_ownedVisualRoot))
        {
            RemoveChild(_ownedVisualRoot);
            _ownedVisualRoot.Free();
        }

        _ownedVisualRoot = null;
        PlaceholderModel = null;
        _animationPlayer = null;
        _idleAnimation = string.Empty;
        _walkAnimation = string.Empty;
        _runAnimation = string.Empty;
        _currentAnimation = string.Empty;
        Body = null;
        Head = null;

        if (Profile is null)
            return;

        _ownedVisualRoot = new Node3D { Name = "AvatarVisual" };
        AddChild(_ownedVisualRoot);

        var loadedExternal = TryBuildExternalPlaceholder();
        BuildPrimitiveFallback(visible: !loadedExternal);

        CacheAnimations();
        PlayLocomotion(0f, false);
    }

    /// <summary>Presentation-only locomotion cue. Does not modify CharacterState or gameplay.</summary>
    public void PlayLocomotion(float horizontalSpeed, bool sprinting)
    {
        if (_animationPlayer is null)
            return;

        var desired = horizontalSpeed <= 0.05f
            ? _idleAnimation
            : sprinting && !string.IsNullOrWhiteSpace(_runAnimation)
                ? _runAnimation
                : _walkAnimation;

        if (string.IsNullOrWhiteSpace(desired))
            desired = _idleAnimation;

        if (string.IsNullOrWhiteSpace(desired) || string.Equals(_currentAnimation, desired, StringComparison.Ordinal))
            return;

        _animationPlayer.Play(desired, customBlend: 0.15);
        _currentAnimation = desired;
    }

    private bool TryBuildExternalPlaceholder()
    {
        if (Profile is null
            || string.IsNullOrWhiteSpace(Profile.PlaceholderModelPath)
            || !ResourceLoader.Exists(Profile.PlaceholderModelPath))
        {
            return false;
        }

        var packed = GD.Load<PackedScene>(Profile.PlaceholderModelPath);
        if (packed is null)
            return false;

        var instance = packed.Instantiate<Node3D>();
        if (instance is null)
            return false;

        instance.Name = "ExternalPlaceholder";
        // This authored stand-in's bodily bounds are 0.02..2.01m; its debug
        // marker is not part of physical height. Other imports keep their legacy contract.
        var calibrated = Profile.PlaceholderModelPath == "res://scenes/dev/GenericCharacterPlaceholder.tscn";
        var sourceHeight = calibrated ? 1.99f : 1.8f;
        var scale = calibrated ? Profile.Height / sourceHeight
            : Mathf.Clamp(Profile.Height / sourceHeight, 0.78f, 1.25f);
        instance.Scale = Vector3.One * scale;
        if (calibrated) instance.Position = new Vector3(0, -0.02f * scale, 0);
        _ownedVisualRoot!.AddChild(instance);
        PlaceholderModel = instance;
        return true;
    }

    private void BuildPrimitiveFallback(bool visible)
    {
        if (Profile is null || _ownedVisualRoot is null)
            return;

        Material bodyMaterial = UseSoftShading
            ? SoftMaterialFactory.Create(Profile.BodyColor, SoftParams)
            : new StandardMaterial3D { AlbedoColor = Profile.BodyColor };
        Material headMaterial = UseSoftShading
            ? SoftMaterialFactory.Create(Profile.HeadColor, SoftParams)
            : new StandardMaterial3D { AlbedoColor = Profile.HeadColor };

        // Explicit head diameter makes fallback bounds 0.20..2.07m before calibration.
        var fallbackScale = Profile.Height / 1.87f;
        Body = new MeshInstance3D
        {
            Name = "FallbackBody",
            Mesh = new CapsuleMesh { Radius = 0.3f * fallbackScale, Height = 1.4f * fallbackScale },
            MaterialOverride = bodyMaterial,
            Position = new Vector3(0f, 0.7f * fallbackScale, 0f),
            Visible = visible
        };

        Head = new MeshInstance3D
        {
            Name = "FallbackHead",
            Mesh = new SphereMesh { Radius = 0.22f * fallbackScale, Height = 0.44f * fallbackScale },
            MaterialOverride = headMaterial,
            Position = new Vector3(0f, 1.65f * fallbackScale, 0f),
            Visible = visible
        };

        _ownedVisualRoot.AddChild(Body);
        _ownedVisualRoot.AddChild(Head);
    }

    private void CacheAnimations()
    {
        if (PlaceholderModel is null)
            return;

        _animationPlayer = FindAnimationPlayer(PlaceholderModel);
        if (_animationPlayer is null)
            return;

        var names = _animationPlayer.GetAnimationList().Select(name => name.ToString()).ToArray();
        _idleAnimation = FindAnimation(names, "idle");
        _walkAnimation = FindAnimation(names, "walk");
        _runAnimation = FindAnimation(names, "run", "jog");

        if (string.IsNullOrWhiteSpace(_idleAnimation))
            _idleAnimation = names.FirstOrDefault(name => !name.Equals("RESET", StringComparison.OrdinalIgnoreCase)) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(_walkAnimation))
            _walkAnimation = _runAnimation;
    }

    private static AnimationPlayer? FindAnimationPlayer(Node node)
    {
        if (node is AnimationPlayer player)
            return player;

        foreach (var child in node.GetChildren())
        {
            if (FindAnimationPlayer(child) is { } nested)
                return nested;
        }

        return null;
    }

    private static string FindAnimation(string[] names, params string[] fragments)
    {
        return names.FirstOrDefault(name =>
            fragments.Any(fragment => name.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
            ?? string.Empty;
    }
}