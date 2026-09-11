using System;
using Godot;

namespace OpenMakaiRanch.Visuals;

/// <summary>
/// Optional original mana-veil art prototype. No sky/environment replacement, lights, collisions,
/// camera changes, weather rolls or rewards. The caller supplies canonical phase/shelter/settings.
/// A static veil remains on Low; motes scale independently. This is proposed lore, not original canon.
/// </summary>
public partial class MakaiPhenomenon3D : Node3D
{
    public const int MaximumMotes = 96;
    private MultiMesh? _motes;
    private ShaderMaterial? _veil;
    private bool _reducedMotion;
    private double _seconds;
    public int ActiveMotes => _motes?.VisibleInstanceCount ?? 0;
    public double VisualSeconds => _seconds;
    public bool EnabledForView { get; private set; }
    private const string ShaderCode = """
        shader_type spatial;
        render_mode unshaded, cull_disabled, blend_add, depth_draw_never, shadows_disabled;
        uniform float phase = 0.0;
        uniform float strength = 0.35;
        void fragment() {
            float edge = pow(max(0.0, sin(UV.y * 3.14159265)), 1.7);
            float ends = smoothstep(0.0, 0.12, UV.x) * smoothstep(0.0, 0.12, 1.0 - UV.x);
            float folds = 0.65 + 0.35 * sin(UV.x * 29.0 + phase * 0.22 + UV.y * 3.0);
            vec3 tint = mix(vec3(0.15, 0.65, 0.58), vec3(0.44, 0.22, 0.70), UV.y);
            ALBEDO = tint;
            ALPHA = edge * ends * folds * strength;
        }
        """;

    public override void _Ready()
    {
        _veil = new ShaderMaterial { Shader = new Shader { Code = ShaderCode } };
        AddChild(new MeshInstance3D { Name = "ManaVeil", Mesh = Ribbon(), MaterialOverride = _veil,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off });
        _motes = new MultiMesh
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            Mesh = new SphereMesh { Radius = .025f, Height = .05f, RadialSegments = 8, Rings = 4 },
            InstanceCount = MaximumMotes, VisibleInstanceCount = 0
        };
        AddChild(new MultiMeshInstance3D
        {
            Name = "ManaMotes", Multimesh = _motes, CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            MaterialOverride = new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                AlbedoColor = new Color(.40f, .73f, .62f), EmissionEnabled = true,
                Emission = new Color(.12f, .26f, .20f)
            }
        });
        Configure(false, PresencePhase.Night, "Low", false, false);
    }

    public void Configure(bool enabled, PresencePhase phase, string? quality, bool sheltered, bool reducedMotion)
    {
        EnabledForView = enabled && !sheltered && (phase is PresencePhase.Evening or PresencePhase.Night);
        Visible = EnabledForView; _reducedMotion = reducedMotion || quality is not ("Medium" or "High" or "Ultra");
        if (_motes is null || _veil is null) return;
        _motes.VisibleInstanceCount = !EnabledForView || reducedMotion ? 0 : quality switch
        { "Ultra" => 96, "High" => 64, "Medium" => 24, _ => 0 };
        _veil.SetShaderParameter("strength", phase == PresencePhase.Night ? .45f : .24f);
        DrawAt(_seconds);
    }

    public void Advance(double delta, bool paused)
    {
        if (!EnabledForView || paused || _reducedMotion || !double.IsFinite(delta) || delta <= 0) return;
        _seconds = (_seconds + Math.Min(delta, .1)) % 86400;
        DrawAt(_seconds);
    }

    private void DrawAt(double seconds)
    {
        if (_motes is null || _veil is null) return;
        _veil.SetShaderParameter("phase", (float)seconds);
        for (var i = 0; i < _motes.VisibleInstanceCount; i++)
        {
            var a = i * 2.399963f;
            var radius = 1.8f + (i % 7) * .45f;
            var position = new Vector3(Mathf.Sin(a) * radius,
                .35f + (float)((i * .137 + seconds * .11) % 2.8), -3.5f + Mathf.Cos(a) * 1.2f);
            _motes.SetInstanceTransform(i, new Transform3D(Basis.Identity, position));
        }
    }

    private static ArrayMesh Ribbon()
    {
        const int columns = 80;
        var positions = new Vector3[(columns + 1) * 2];
        var uv = new Vector2[positions.Length]; var indices = new int[columns * 6];
        for (var i = 0; i <= columns; i++)
        {
            var u = i / (float)columns;
            for (var row = 0; row < 2; row++)
            {
                var index = i * 2 + row;
                positions[index] = new Vector3((u - .5f) * 18f,
                    2.4f + .65f * Mathf.Sin(u * 6.0f) + row * .9f, -9f + Mathf.Sin(u * 8f));
                uv[index] = new Vector2(u, row);
            }
            if (i == columns) continue;
            var n = i * 2; var j = i * 6;
            indices[j] = n; indices[j + 1] = n + 1; indices[j + 2] = n + 2;
            indices[j + 3] = n + 2; indices[j + 4] = n + 1; indices[j + 5] = n + 3;
        }
        var arrays = new Godot.Collections.Array(); arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = positions; arrays[(int)Mesh.ArrayType.TexUV] = uv;
        arrays[(int)Mesh.ArrayType.Index] = indices;
        var mesh = new ArrayMesh(); mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        return mesh;
    }
}
