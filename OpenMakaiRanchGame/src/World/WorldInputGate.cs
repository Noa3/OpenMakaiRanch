using System;

namespace OpenMakaiRanch.World;

/// <summary>
/// Tracks whether the world or the management UI currently owns player input.
/// The rule (from the 3D_REMAKE_PLAN) is that world movement must stop while the
/// management UI owns input, and capture must be released on focus loss.
///
/// Kept as a pure state machine so it can be verified headlessly without a scene.
/// The <c>WorldInputGate</c> node owns one instance and drives the controller from it.
/// </summary>
public sealed class WorldInputGate
{
    private bool _uiOwnsInput;
    private bool _windowFocused = true;

    public bool UiOwnsInput => _uiOwnsInput;
    public bool WindowFocused => _windowFocused;

    /// <summary>True only when the world should accept movement/interaction input.</summary>
    public bool WorldInputEnabled => !_uiOwnsInput && _windowFocused;

    public event Action? InputStateDidChange;

    public void SetUiOwnsInput(bool owns)
    {
        if (_uiOwnsInput == owns)
        {
            return;
        }

        _uiOwnsInput = owns;
        InputStateDidChange?.Invoke();
    }

    public void SetWindowFocused(bool focused)
    {
        if (_windowFocused == focused)
        {
            return;
        }

        _windowFocused = focused;
        InputStateDidChange?.Invoke();
    }

    public void Reset()
    {
        _uiOwnsInput = false;
        _windowFocused = true;
    }
}
