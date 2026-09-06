using System;
using System.Collections.Generic;

namespace OpenMakaiRanch.World;

/// <summary>
/// Coordinates which management panel is open over the 3D world and — critically — that world
/// input is suspended while a panel owns it and safely resumed when the panel closes.
///
/// Per the 3D_REMAKE_PLAN: "Open existing inventory and management panels from the world; closing
/// them resumes world input safely." The rule that makes it *safe* is that exactly one panel may
/// own input at a time, and the gate is always returned to "world owns input" on close — never
/// left dangling in the UI-owned state, which would silently freeze world movement.
///
/// This type only *drives* the existing <see cref="WorldInputGate"/>; it never computes rewards,
/// bond, economy or job outcomes. Node-free and deterministic so it can be verified headlessly.
/// </summary>
public sealed class WorldPanelCoordinator
{
    private readonly WorldInputGate _inputGate;
    private readonly HashSet<string> _knownPanels;

    /// <summary>The panel currently open, or null when the world has no panel over it.</summary>
    public string? ActivePanel { get; private set; }

    /// <summary>True while any management panel owns input over the world.</summary>
    public bool IsOpen => ActivePanel is not null;

    public WorldPanelCoordinator(WorldInputGate inputGate, IEnumerable<string> knownPanels)
    {
        _inputGate = inputGate ?? throw new ArgumentNullException(nameof(inputGate));
        _knownPanels = new HashSet<string>(knownPanels ?? Array.Empty<string>(), StringComparer.Ordinal);
    }

    /// <summary>
    /// Opens <paramref name="panelId"/>. If a different panel is already open it is closed first
    /// (only one panel may own input at a time). The input gate flips to UI-owned on open.
    /// Returns false (no state change) when the panel id is not a known management panel — a world
    /// interaction must not be able to open an arbitrary screen.
    /// </summary>
    public bool Open(string panelId)
    {
        if (string.IsNullOrWhiteSpace(panelId) || !_knownPanels.Contains(panelId))
        {
            return false;
        }

        if (ActivePanel != panelId)
        {
            // Close any currently-open panel before opening the new one (single-panel rule).
            if (ActivePanel is not null)
            {
                ActivePanel = null;
            }
            ActivePanel = panelId;
            _inputGate.SetUiOwnsInput(true);
        }
        return true;
    }

    /// <summary>
    /// Closes the active panel (if any). The input gate is always returned to "world owns input"
    /// on close, even if the active panel was null — this is the "resumes world input safely"
    /// guarantee: world movement can never be left frozen because a panel close was missed.
    /// Returns true when a panel was actually closed, false when there was nothing to close.
    /// </summary>
    public bool Close()
    {
        var hadPanel = ActivePanel is not null;
        ActivePanel = null;
        _inputGate.SetUiOwnsInput(false);
        return hadPanel;
    }

    /// <summary>
    /// True when the world can accept movement/interaction right now — no panel over it and the
    /// window focused. This is the single source of truth the player controller reads.
    /// </summary>
    public bool WorldInputEnabled => _inputGate.WorldInputEnabled;
}
