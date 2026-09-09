using System;
using System.Linq;
using Godot;

namespace OpenMakaiRanch.Character;

/// <summary>
/// Presentation-only 3D placeholder bound to a stable CharacterVisualProfile.
///
/// The preferred placeholder is a generic CC0 rigged character. It deliberately remains marked
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
        {
            return;
        }

        _ownedVisualRoot = new Node3D { Name = "AvatarVisual" };
        AddChild(_ownedVisualRoot);

        var loadedExternal = TryBuildExternalPlaceholder();
        BuildPrimitiveFallback(visible: !loadedExternal);

        CacheAnimations();
        PlayLocomotion(0f, false);
    }

    /// <summary>
    /// Presentation-only locomotion cue. Does not modify CharacterState or gameplay.
    /// </summary>
    public void PlayLocomotion(float horizontalSpeed, bool sprinting)
    {
        if (_animationPlayer is null)
        {
            return;
        }

        var desired = horizontalSpeed <= 0.05f
            ? _idleAnimation
            : sprinting && !string.IsNullOrWhiteSpace(_runAnimation)
                ? _runAnimation
                : _walkAnimation;

        if (string.IsNullOrWhiteSpace(desired))
        {
            desired = _idleAnimation;
        }

        if (string.IsNullOrWhiteSpace(desired) || string.Equals(_currentAnimation, desired, StringComparison.Ordinal))
        {
            return;
        }

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
        {
            return false;
        }

        var instance = packed.Instantiate<Node3D>();
        if (instance is null)
        {
            return false;
        }

        instance.Name = "ExternalPlaceholder";
        var sourceHeight = 1.8f;
        var scale = Mathf.Clamp(Profile.Height / sourceHeight, 0.78f, 1.25f);
        instance.Scale = Vector3.One * scale;
        _ownedVisualRoot!.AddChild(instance);
        PlaceholderModel = instance;
        return true;
    }

    private void BuildPrimitiveFallback(bool visible)
    {
        if (Profile is null || _ownedVisualRoot is null)
        {
            return;
        }

        Body = new MeshInstance3D
        {
            Name = "FallbackBody",
            Mesh = new CapsuleMesh { Radius = 0.3f, Height = 1.4f },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = Profile.BodyColor },
            Position = new Vector3(0f, 0.9f, 0f),
            Visible = visible
        };

        Head = new MeshInstance3D
        {
            Name = "FallbackHead",
            Mesh = new SphereMesh { Radius = 0.22f },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = Profile.HeadColor },
            Position = new Vector3(0f, 1.85f, 0f),
            Visible = visible
        };

        _ownedVisualRoot.AddChild(Body);
        _ownedVisualRoot.AddChild(Head);
    }

    private void CacheAnimations()
    {
        if (PlaceholderModel is null)
        {
            return;
        }

        _animationPlayer = FindAnimationPlayer(PlaceholderModel);
        if (_animationPlayer is null)
        {
            return;
        }

        var names = _animationPlayer.GetAnimationList().Select(name => name.ToString()).ToArray();
        _idleAnimation = FindAnimation(names, "idle");
        _walkAnimation = FindAnimation(names, "walk");
        _runAnimation = FindAnimation(names, "run", "jog");

        if (string.IsNullOrWhiteSpace(_idleAnimation))
        {
            _idleAnimation = names.FirstOrDefault(name => !name.Equals("RESET", StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(_walkAnimation))
        {
            _walkAnimation = _runAnimation;
        }
    }

    private static AnimationPlayer? FindAnimationPlayer(Node node)
    {
        if (node is AnimationPlayer player)
        {
            return player;
        }

        foreach (var child in node.GetChildren())
        {
            if (FindAnimationPlayer(child) is { } nested)
            {
                return nested;
            }
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
