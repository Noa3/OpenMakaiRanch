using Godot;

namespace OpenMakaiRanch.Visuals;

/// <summary>
/// Explicit per-surface opt-in. Never mutates an imported Mesh/Material resource and never
/// overrides a whole-mesh MaterialOverride. Releasing ownership cannot undo another system's edit.
/// </summary>
[GlobalClass]
public partial class AnimeSurfaceBinding3D : Node
{
    [Export] public NodePath TargetMeshPath { get; set; } = new("..");
    [Export] public int SurfaceIndex { get; set; }
    [Export] public AnimeSurfaceProfile? Profile { get; set; }
    [Export] public string Quality { get; set; } = "High";
    private MeshInstance3D? _target;
    private Mesh? _mesh;
    private int _surface;
    private Material? _previous;
    private ShaderMaterial? _owned;

    public override void _Ready()
    {
        if (Profile is not null) TryApply(GetNodeOrNull<MeshInstance3D>(TargetMeshPath), SurfaceIndex, Profile, Quality);
    }

    public bool TryApply(MeshInstance3D? target, int surface, AnimeSurfaceProfile profile, string quality = "High")
    {
        if (target is null || !GodotObject.IsInstanceValid(target) || target.Mesh is null
            || surface < 0 || surface >= target.Mesh.GetSurfaceCount() || target.MaterialOverride is not null)
            return false;
        var active = target.GetActiveMaterial(surface);
        if (active is BaseMaterial3D authored && authored.Transparency != BaseMaterial3D.TransparencyEnum.Disabled)
            return false; // This pipeline is opaque; never turn hair cards/glass into solid polygons.
        if (active is ShaderMaterial && active != _owned)
            return false; // Unknown shader/depth semantics require a separate authored conversion.
        // Prepare before releasing the old binding, so an invalid profile does not partially apply.
        var replacement = AnimeMaterialFactory.Create(profile, quality);
        Restore();
        _target = target;
        _mesh = target.Mesh;
        _surface = surface;
        _previous = target.GetSurfaceOverrideMaterial(surface);
        _owned = replacement;
        target.SetSurfaceOverrideMaterial(surface, replacement);
        return true;
    }

    public void Restore()
    {
        if (_target is not null && GodotObject.IsInstanceValid(_target) && _target.Mesh == _mesh
            && _mesh is not null && _surface < _mesh.GetSurfaceCount()
            && _target.GetSurfaceOverrideMaterial(_surface) == _owned)
            _target.SetSurfaceOverrideMaterial(_surface, _previous);
        _target = null;
        _mesh = null;
        _previous = null;
        _owned = null;
    }

    public override void _ExitTree() => Restore();
}
