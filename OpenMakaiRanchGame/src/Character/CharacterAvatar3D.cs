using Godot;

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
/// </summary>
[GlobalClass]
public partial class CharacterAvatar3D : Node3D
{
    /// <summary>The presentation profile this avatar renders. Null → no geometry.</summary>
    [Export] public CharacterVisualProfile? Profile { get; set; }

    /// <summary>Body mesh (capsule stand-in). Null when no profile or before Rebuild.</summary>
    public MeshInstance3D? Body { get; private set; }

    /// <summary>Head mesh (sphere stand-in). Null when no profile or before Rebuild.</summary>
    public MeshInstance3D? Head { get; private set; }

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
        Body = new MeshInstance3D
        {
            Mesh = new CapsuleMesh { Radius = 0.3f, Height = 1.4f },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = Profile.BodyColor },
            Position = new Vector3(0f, 0.9f, 0f),
        };
        Head = new MeshInstance3D
        {
            Mesh = new SphereMesh { Radius = 0.22f },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = Profile.HeadColor },
            Position = new Vector3(0f, 1.85f, 0f),
        };
        AddChild(Body);
        AddChild(Head);
    }
}
