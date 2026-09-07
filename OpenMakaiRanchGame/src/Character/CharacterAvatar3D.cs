using Godot;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Character;

/// <summary>
/// A 3D character placeholder bound to a stable <see cref="CharacterVisualProfile"/>.
/// CHAR-001 renders honest debug stand-in geometry (neutral capsule body + sphere head) so
/// scale, collision, and travel can be tested before real models exist.
///
/// Per the 3D_REMAKE_PLAN: "Missing art gets an honest debug stand-in, not a random hero
/// model." and "Do not mark a rough mesh GAMEPLAY_APPROVED."
///
/// This node owns no simulation state. It reads only the profile (presentation parameters)
/// and emits no reward/economy/bond signals. All gameplay effects flow through GameRoot.
///
/// NPC interaction (AC #10): the avatar implements <see cref="IWorldInteractable"/> exactly
/// like <see cref="WorldStation"/>. When <see cref="CharacterId"/> and <see cref="Dispatcher"/>
/// are set, <see cref="IWorldInteractable.Activate"/> dispatches a
/// <see cref="WorldCommandKind.Mentorship"/> through the shared command boundary — the same
/// path as the station. The avatar computes no rewards, bond, or economy itself.
/// </summary>
[GlobalClass]
public partial class CharacterAvatar3D : Node3D, IWorldInteractable
{
    /// <summary>The presentation profile this avatar renders. Null → no geometry.</summary>
    [Export] public CharacterVisualProfile? Profile { get; set; }

    /// <summary>Body mesh (capsule stand-in). Null when no profile or before Rebuild.</summary>
    public MeshInstance3D? Body { get; private set; }

    /// <summary>Head mesh (sphere stand-in). Null when no profile or before Rebuild.</summary>
    public MeshInstance3D? Head { get; private set; }

    /// <summary>
    /// When true, the stand-in geometry renders with the soft-anime shader
    /// (<see cref="SoftMaterialFactory"/>) — a smooth, natural lighting model
    /// (soft diffuse ramp + hemisphere ambient + gentle rim), NOT a hard toon/cel
    /// band. Off by default: the CHAR-001 stand-in stays an honest debug material,
    /// and the soft path is opt-in so the existing StandardMaterial3D smoke
    /// assertion is unaffected. Enabling it is the CHAR-002/ART-002 presentation
    /// upgrade.
    /// </summary>
    [Export] public bool UseSoftShading { get; set; } = false;

    /// <summary>Soft-shading parameters used when <see cref="UseSoftShading"/> is on.
    /// Plain property (not <c>[Export]</c>): a C# struct is not a Godot-serializable
    /// Variant, so it stays a code-only knob; the defaults are deterministic.</summary>
    public SoftShadingMath.SoftParameters? SoftParams { get; set; }

    // ── NPC interaction (AC #10) — IWorldInteractable ─────────────────────

    /// <summary>Roster character id this avatar represents. Null/empty → not interactable.</summary>
    public string? CharacterId { get; set; }

    /// <summary>Display name shown on the interaction prompt.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The dispatcher routing the command to GameRoot. The scene injects the production binding;
    /// headless tests inject a stub. Null → the avatar is not interactable.
    /// </summary>
    public IWorldCommandDispatcher? Dispatcher { get; set; }

    /// <summary>Per-avatar guard preventing double activation.</summary>
    public WorldInteractionGuard InteractionGuard { get; } = new();

    // IWorldInteractable members.
    string IWorldInteractable.TargetId => CharacterId ?? string.Empty;
    string IWorldInteractable.Label => DisplayName;
    bool IWorldInteractable.IsAvailable => !string.IsNullOrEmpty(CharacterId) && Dispatcher is not null && InteractionGuard.CanInteract;
    string? IWorldInteractable.UnavailableReason => Dispatcher is null ? "no command dispatcher bound" : string.Empty;

    /// <summary>Convenience: true when the avatar can accept an interaction right now.</summary>
    public bool IsInteractable =>
        !string.IsNullOrEmpty(CharacterId) && Dispatcher is not null && InteractionGuard.CanInteract;

    /// <summary>
    /// Dispatch the mentorship interaction through the guard + command boundary. Returns the
    /// command result (true = success). Rejects double activation and a missing dispatcher.
    /// Mirrors <see cref="WorldStation.Activate"/>.
    /// </summary>
    public bool Activate(WorldInteractionContext context)
    {
        if (!IsInteractable || Dispatcher is null)
        {
            return false;
        }

        if (!InteractionGuard.BeginCommand())
        {
            return false;
        }

        try
        {
            var command = new WorldCommand(WorldCommandKind.Mentorship, CharacterId);
            return Dispatcher.Dispatch(command, context);
        }
        finally
        {
            InteractionGuard.EndCommand();
        }
    }

    public override void _Ready()
    {
        Rebuild();
    }

    /// <summary>
    /// (Re)generate the stand-in geometry from the current <see cref="Profile"/>. Safe to call
    /// after the node is in the tree and after the profile is swapped.
    /// </summary>
    public void Rebuild()
    {
        // Clear existing children (idempotent).
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.Free();
        }
        Body = null;
        Head = null;

        if (Profile is null)
        {
            return;
        }

        // Honest debug stand-in: neutral capsule body + sphere head.
        // No adult geometry, no clothing, no morphs, no skeleton.
        // This is a placeholder for scale/collision/travel — not a character model.
        Material bodyMaterial;
        Material headMaterial;
        if (UseSoftShading)
        {
            // Soft-anime path: smooth diffuse + hemisphere ambient + gentle rim,
            // rendered by the Godot spatial shader (SoftShaderSource). No hard
            // cel band, no hard corners — the user's explicit direction.
            bodyMaterial = SoftMaterialFactory.Create(Profile.BodyColor, SoftParams);
            headMaterial = SoftMaterialFactory.Create(Profile.HeadColor, SoftParams);
        }
        else
        {
            bodyMaterial = new StandardMaterial3D { AlbedoColor = Profile.BodyColor };
            headMaterial = new StandardMaterial3D { AlbedoColor = Profile.HeadColor };
        }

        Body = new MeshInstance3D
        {
            Mesh = new CapsuleMesh { Radius = 0.3f, Height = 1.4f },
            MaterialOverride = bodyMaterial,
            Position = new Vector3(0f, 0.9f, 0f),
        };
        Head = new MeshInstance3D
        {
            Mesh = new SphereMesh { Radius = 0.22f },
            MaterialOverride = headMaterial,
            Position = new Vector3(0f, 1.85f, 0f),
        };
        AddChild(Body);
        AddChild(Head);
    }
}
