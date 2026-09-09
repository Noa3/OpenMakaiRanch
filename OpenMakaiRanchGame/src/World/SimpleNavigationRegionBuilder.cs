using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// Authors a simple rectangular NavigationRegion3D at runtime for the current open greybox.
///
/// This is deliberately conservative: it gives NavigationAgent3D a real map today while the current
/// placeholder landmarks remain collision-free. When final buildings/fences receive collision, this
/// region can be replaced by an editor-baked navmesh without changing RosterRig.
/// </summary>
public partial class SimpleNavigationRegionBuilder : NavigationRegion3D
{
    [Export] public Vector2 Size { get; set; } = new(36f, 26f);
    [Export] public float Y { get; set; } = 0.02f;

    public override void _Ready()
    {
        if (NavigationMesh is not null && NavigationMesh.GetPolygonCount() > 0)
        {
            return;
        }

        var halfX = Mathf.Max(1f, Size.X * 0.5f);
        var halfZ = Mathf.Max(1f, Size.Y * 0.5f);

        var mesh = new NavigationMesh
        {
            Vertices = new PackedVector3Array(new[]
            {
                new Vector3(-halfX, Y, -halfZ),
                new Vector3( halfX, Y, -halfZ),
                new Vector3( halfX, Y,  halfZ),
                new Vector3(-halfX, Y,  halfZ)
            })
        };
        mesh.AddPolygon(new PackedInt32Array(new[] { 0, 1, 2, 3 }));
        NavigationMesh = mesh;
    }
}
