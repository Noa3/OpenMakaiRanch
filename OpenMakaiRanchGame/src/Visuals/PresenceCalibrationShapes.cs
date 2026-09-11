using System;
using Godot;

namespace OpenMakaiRanch.Visuals;

/// <summary>Original lab-only shape targets. Not a replacement for an authored facial rig or topology.</summary>
public static class PresenceCalibrationShapes
{
    public static ArrayMesh Add(ArrayMesh source, string family)
    {
        var names = family switch
        {
            "eye" => new[] { "blink" }, "lines" => new[] { "blink", "brow_raise", "brow_pinch" },
            "mouth" => new[] { "smile", "jaw_open" }, _ => Array.Empty<string>()
        };
        if (names.Length == 0) return source;
        var original = source.SurfaceGetArrays(0);
        var positions = original[(int)Mesh.ArrayType.Vertex].AsVector3Array();
        var uv = original[(int)Mesh.ArrayType.TexUV].AsVector2Array();
        var mesh = new ArrayMesh { BlendShapeMode = Mesh.BlendShapeMode.Normalized };
        var shapes = new Godot.Collections.Array<Godot.Collections.Array>();
        foreach (var name in names)
        {
            mesh.AddBlendShape(name);
            var target = new Vector3[positions.Length];
            for (var i = 0; i < target.Length; i++)
            {
                var p = positions[i]; var oldY = p.Y;
                if (name == "blink" && (family == "eye" || p.Y < .074f))
                {
                    var side = p.X < 0 ? -1f : 1f;
                    var u = (p.X - side * .078f) / .06f;
                    p.Y = .037f + side * u * .005f + (family == "eye" ? (p.Y - .037f) * .025f : 0);
                }
                if (name == "brow_raise" && p.Y > .08f) p.Y += .017f;
                if (name == "brow_pinch" && p.Y > .08f) p.Y -= .013f * Math.Clamp(1 - Math.Abs(p.X) / .15f, 0, 1);
                if (name == "smile") p.Y += .014f * Mathf.Pow(Math.Abs(p.X) / .03f, 2f);
                if (name == "jaw_open") p.Y -= .025f * uv[i].Y * Mathf.Sin(uv[i].X * Mathf.Pi);
                // Keep the altered graphic accent in front of the curved face rather than inside it.
                p.Z += AnimeCalibrationGeometry.Front(p.X, p.Y) - AnimeCalibrationGeometry.Front(p.X, oldY);
                target[i] = p;
            }
            var arrays = new Godot.Collections.Array(); arrays.Resize((int)Mesh.ArrayType.Max);
            arrays[(int)Mesh.ArrayType.Vertex] = target;
            arrays[(int)Mesh.ArrayType.Normal] = original[(int)Mesh.ArrayType.Normal];
            arrays[(int)Mesh.ArrayType.Tangent] = original[(int)Mesh.ArrayType.Tangent];
            shapes.Add(arrays);
        }
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, original, shapes);
        return mesh;
    }
}
