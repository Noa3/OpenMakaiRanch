using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Applies persistent keyboard/controller bindings before world input consumers begin processing.
/// InputBindingService owns defaults, remaps, controller navigation and persistence.
/// </summary>
public partial class WorldInputBootstrap : Node
{
    public override void _Ready()
    {
        InputBindingService.EnsureApplied();
    }
}
