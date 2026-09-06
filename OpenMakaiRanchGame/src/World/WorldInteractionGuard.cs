namespace OpenMakaiRanch.World;

/// <summary>
/// Pure state machine guarding a single interaction target against double activation and
/// against activating a missing/despawned target. From the 3D_REMAKE_PLAN:
/// "Reject missing/despawned targets and double activation while a command is running."
///
/// Node-free so it can be verified headlessly. The <c>WorldStation</c> node owns one guard
/// and consults it before dispatching any command through the command boundary.
/// </summary>
public sealed class WorldInteractionGuard
{
    private bool _commandRunning;
    private bool _targetPresent = true;

    public bool CommandRunning => _commandRunning;
    public bool TargetPresent => _targetPresent;

    /// <summary>True when an interaction may be dispatched right now.</summary>
    public bool CanInteract => _targetPresent && !_commandRunning;

    /// <summary>
    /// Begin a command. Returns true if it was accepted, false if it was rejected
    /// (target missing or a command already running). Callers must call <see cref="EndCommand"/>
    /// exactly once for every accepted command.
    /// </summary>
    public bool BeginCommand()
    {
        if (!_targetPresent || _commandRunning)
        {
            return false;
        }

        _commandRunning = true;
        return true;
    }

    /// <summary>Release the guard after the command finishes (success or failure).</summary>
    public void EndCommand()
    {
        _commandRunning = false;
    }

    /// <summary>
    /// Mark the target as missing/despawned (scene exit, load/new game, error). While missing,
    /// no interaction may be dispatched.
    /// </summary>
    public void SetTargetPresent(bool present)
    {
        _targetPresent = present;
        if (!present)
        {
            _commandRunning = false;
        }
    }
}
