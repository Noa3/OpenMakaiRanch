using Godot;

namespace OpenMakaiRanch.World;

/// <summary>One line of spoken text in a world dialogue.</summary>
public sealed record DialogueLine(string Speaker, string Text);

/// <summary>
/// Pure, presentational framing for a world dialogue (a mentorship or bond event staged in 3D).
///
/// This type carries a character id *for framing only* — it has **no** Bond, Morale, Stockpile,
/// or CompletedEvent fields. That is deliberate and load-bearing: per the 3D_REMAKE_PLAN,
/// "Dialogue staging may control camera/look-at/animation but must not duplicate event state."
/// Because the record cannot express the numbers, the staging layer cannot accidentally move a
/// second bond/morale/stockpile — the simulation state changes only through the GameRoot command
/// boundary (proven by WORLD-002).
///
/// Node-free and deterministic so it can be verified headlessly.
/// </summary>
public sealed record DialoguePresentation(
    string CharacterId,
    Vector3 CameraTarget,
    Vector3 LookAt,
    string Pose,
    IReadOnlyList<DialogueLine> Lines)
{
    /// <summary>True when the staging is structurally complete (speaker + text on every line).</summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(CharacterId)
        && Lines is { Count: > 0 }
        && Lines.All(line => !string.IsNullOrWhiteSpace(line.Speaker) && !string.IsNullOrWhiteSpace(line.Text));
}

/// <summary>
/// Builds <see cref="DialoguePresentation"/> records for a mentorship or a bond event.
/// Pure function of its inputs; never reads or writes simulation state.
/// </summary>
public static class DialogueStager
{
    /// <summary>Head height above the anchor (roughly the NPC's eye line for a 3/4 framing).</summary>
    private static readonly Vector3 HeadOffset = new(0f, 1.2f, 0f);

    /// <summary>Camera offset for a comfortable three-quarter conversation angle.</summary>
    private static readonly Vector3 ConversationCameraOffset = new(0.8f, 0.4f, 1.6f);

    /// <summary>
    /// Stages a mentorship conversation. Framing is a fixed function of the anchor — it does not
    /// depend on the character's bond/morale, so the same anchor always yields the same framing.
    /// </summary>
    public static DialoguePresentation BuildMentorship(string characterId, Vector3 anchor)
    {
        var target = anchor + HeadOffset;
        return new DialoguePresentation(
            characterId,
            target,
            target,
            "FacePlayer",
            new[]
            {
                new DialogueLine(characterId, "Thank you for taking the time to train me."),
                new DialogueLine("You", "Consider it an investment in the ranch."),
            });
    }

    /// <summary>
    /// Stages a bond-event beat. The event name is presentation metadata (what happened), not a
    /// reward — the reward is applied by the simulation, not by this type.
    /// </summary>
    public static DialoguePresentation BuildBondEvent(string characterId, string eventName, Vector3 anchor)
    {
        var target = anchor + HeadOffset;
        return new DialoguePresentation(
            characterId,
            target,
            target,
            "Warm",
            new[]
            {
                new DialogueLine(characterId, string.IsNullOrWhiteSpace(eventName)
                    ? "I feel closer to you now."
                    : $"{eventName}"),
                new DialogueLine(characterId, "I can feel our bond growing stronger."),
            });
    }
}
