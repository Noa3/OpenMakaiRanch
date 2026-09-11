using System;
using System.Collections.Generic;
using Godot;

namespace OpenMakaiRanch.Visuals;

/// <summary>
/// Original, non-rigged calibration geometry, not a production character or a copy of reference art.
/// Smooth sampled surfaces supply explicit normals, UVs and tangent handedness. No external assets.
/// </summary>
public static class AnimeCalibrationGeometry
{
    public static readonly Vector3 HeadOrigin = new(0f, 1.63f, 0f);

    public static ArrayMesh Head()
    {
        var builder = new Builder();
        builder.Patch(80, 48, (u, v) =>
        {
            var angle = (u * 2f - 1f) * Mathf.Pi;
            var latitude = Mathf.Lerp(-Mathf.Pi * 0.5f + 0.002f, Mathf.Pi * 0.5f - 0.002f, v);
            var y = 0.24f * Mathf.Sin(latitude);
            var ring = Mathf.Cos(latitude);
            var x = Width(y) * ring * Mathf.Sin(angle);
            var c = Mathf.Cos(angle);
            var z = 0.18f * ring * MathF.CopySign(Mathf.Pow(Mathf.Abs(c), 0.55f), c);
            if (c > 0f) z += Nose(x, y) * Mathf.SmoothStep(0f, 0.4f, c);
            return new Vector3(x, y, z);
        });
        return builder.Build();
    }

    private static float Width(float y) => 0.205f * Mathf.Lerp(0.76f, 1f, Mathf.SmoothStep(-0.18f, 0.02f, y));
    private static float Nose(float x, float y)
        => 0.029f * Mathf.Exp(-x * x / 0.00019f - (y + 0.022f) * (y + 0.022f) / 0.0011f);

    public static float Front(float x, float y)
    {
        var ring = Mathf.Sqrt(Mathf.Max(0.001f, 1f - y * y / (0.24f * 0.24f)));
        var ratio = Mathf.Clamp(x / (Width(y) * ring), -0.995f, 0.995f);
        return 0.18f * ring * Mathf.Pow(Mathf.Sqrt(1f - ratio * ratio), 0.55f) + Nose(x, y);
    }

    public static Vector3 EyePoint(float side, float u, float v)
    {
        var x = side * 0.078f + u * 0.060f;
        var curve = Mathf.Cos(u * Mathf.Pi * 0.5f);
        var y = 0.037f + curve * v * 0.023f + side * u * 0.005f;
        var z = Front(x, y) + 0.003f + 0.011f * curve * Mathf.Max(0f, 1f - v * v);
        return new Vector3(x, y, z);
    }

    public static ArrayMesh Eye(float side)
    {
        var builder = new Builder();
        builder.Patch(40, 20, (u, v) => EyePoint(side, u * 1.998f - 0.999f, v * 2f - 1f));
        return builder.Build();
    }

    public static ArrayMesh FacialLines()
    {
        var builder = new Builder();
        foreach (var side in new[] { -1f, 1f })
        {
            // Eyelids are local graphic accents, not a global silhouette/outlining effect.
            foreach (var upper in new[] { false, true })
            {
                builder.Patch(40, 2, (u, v) =>
                {
                    var p = EyePoint(side, u * 1.996f - 0.998f, upper ? 1f : -1f);
                    var taper = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(u * Mathf.Pi)), 0.45f);
                    p.Y += (v - 0.5f) * (upper ? 0.006f : 0.0025f) * taper;
                    p.Z += 0.002f;
                    return p;
                });
            }
            builder.Patch(32, 2, (u, v) =>
            {
                var x = side * 0.078f + (u - 0.5f) * 0.102f;
                var y = 0.092f + 0.009f * Mathf.Sin(u * Mathf.Pi) + side * (u - 0.5f) * 0.008f
                    + (v - 0.5f) * 0.006f * Mathf.Pow(Mathf.Max(0f, Mathf.Sin(u * Mathf.Pi)), 0.4f);
                return new Vector3(x, y, Front(x, y) + 0.003f);
            });
        }
        return builder.Build();
    }

    public static ArrayMesh Mouth()
    {
        var builder = new Builder();
        builder.Patch(32, 3, (u, v) =>
        {
            var x = (u - 0.5f) * 0.060f;
            var y = -0.091f + 0.004f * Mathf.Pow(u * 2f - 1f, 2f)
                + (v - 0.5f) * 0.0035f * Mathf.Sin(u * Mathf.Pi);
            return new Vector3(x, y, Front(x, y) + 0.002f);
        });
        return builder.Build();
    }

    public static ArrayMesh Hair()
    {
        var builder = new Builder();
        // Scalp shell ends above the face but below the ears at the back.
        builder.Patch(80, 32, (u, v) =>
        {
            var theta = (u * 2f - 1f) * Mathf.Pi;
            var front = Mathf.SmoothStep(-0.15f, 0.85f, Mathf.Cos(theta));
            var latitude = Mathf.Lerp(Mathf.Lerp(-1.05f, 0.57f, front), 1.567f, v);
            return new Vector3(0.226f * Mathf.Cos(latitude) * Mathf.Sin(theta),
                0.274f * Mathf.Sin(latitude) + 0.008f,
                0.212f * Mathf.Cos(latitude) * Mathf.Cos(theta) - 0.025f);
        });
        // Curved, volumetric locks: continuous V runs from root to tip for anisotropic flow.
        Strand(builder, new(-0.095f, 0.233f, 0.080f), new(-0.16f, 0.19f, 0.18f), new(-0.16f, 0.11f, 0.20f), new(-0.14f, 0.060f, 0.178f), 0.047f);
        Strand(builder, new(-0.050f, 0.25f, 0.07f), new(-0.045f, 0.21f, 0.20f), new(-0.10f, 0.13f, 0.215f), new(-0.075f, 0.102f, 0.186f), 0.048f);
        Strand(builder, new(0.010f, 0.257f, 0.064f), new(0.020f, 0.22f, 0.205f), new(0.020f, 0.14f, 0.219f), new(0.061f, 0.093f, 0.198f), 0.056f);
        Strand(builder, new(0.075f, 0.241f, 0.060f), new(0.145f, 0.19f, 0.16f), new(0.155f, 0.115f, 0.18f), new(0.137f, 0.061f, 0.180f), 0.053f);
        foreach (var side in new[] { -1f, 1f })
        {
            Strand(builder, new(side * 0.145f, 0.204f, 0.048f), new(side * 0.235f, 0.16f, 0.077f), new(side * 0.224f, -0.10f, 0.12f), new(side * 0.178f, -0.19f, 0.13f), 0.055f);
            Strand(builder, new(side * 0.16f, 0.18f, -0.09f), new(side * 0.253f, 0.10f, -0.03f), new(side * 0.256f, -0.15f, 0.04f), new(side * 0.213f, -0.23f, 0.025f), 0.052f);
            Strand(builder, new(side * 0.12f, 0.175f, -0.16f), new(side * 0.20f, 0.02f, -0.20f), new(side * 0.20f, -0.19f, -0.12f), new(side * 0.23f, -0.24f, -0.08f), 0.055f);
        }
        return builder.Build();
    }

    private static void Strand(Builder builder, Vector3 a, Vector3 b, Vector3 c, Vector3 d, float width)
    {
        builder.Patch(16, 28, (u, v) =>
        {
            var t = 0.002f + v * 0.996f;
            var center = a * Mathf.Pow(1f - t, 3f) + b * (3f * (1f - t) * (1f - t) * t)
                + c * (3f * (1f - t) * t * t) + d * (t * t * t);
            var radius = Mathf.Pow(Mathf.Sin(t * Mathf.Pi), 0.65f);
            var theta = u * Mathf.Tau;
            return center + new Vector3(width * radius * Mathf.Cos(theta), 0f,
                0.018f * radius * Mathf.Sin(theta));
        });
    }

    private sealed class Builder
    {
        private readonly List<Vector3> _vertices = new(), _normals = new();
        private readonly List<Vector2> _uvs = new();
        private readonly List<float> _tangents = new();
        private readonly List<int> _indices = new();

        public void Patch(int columns, int rows, Func<float, float, Vector3> point)
        {
            var start = _vertices.Count;
            for (var row = 0; row <= rows; row++)
            for (var column = 0; column <= columns; column++)
            {
                var u = (float)column / columns;
                var v = (float)row / rows;
                var du = point(Mathf.Min(1f, u + 0.0003f), v) - point(Mathf.Max(0f, u - 0.0003f), v);
                var dv = point(u, Mathf.Min(1f, v + 0.0003f)) - point(u, Mathf.Max(0f, v - 0.0003f));
                var normal = du.Cross(dv).Normalized();
                var tangent = du.Normalized();
                if (normal.LengthSquared() < 0.5f) normal = Vector3.Back;
                if (tangent.LengthSquared() < 0.5f) tangent = Vector3.Right;
                _vertices.Add(point(u, v)); _normals.Add(normal); _uvs.Add(new Vector2(u, v));
                _tangents.AddRange(new[] { tangent.X, tangent.Y, tangent.Z,
                    normal.Cross(tangent).Dot(dv) < 0f ? -1f : 1f });
            }
            for (var row = 0; row < rows; row++)
            for (var column = 0; column < columns; column++)
            {
                var a = start + row * (columns + 1) + column;
                var b = a + columns + 1;
                // Godot front faces use clockwise winding when viewed from outside.
                _indices.AddRange(new[] { a, b, a + 1, a + 1, b, b + 1 });
            }
        }

        public ArrayMesh Build()
        {
            var arrays = new Godot.Collections.Array();
            arrays.Resize((int)Mesh.ArrayType.Max);
            arrays[(int)Mesh.ArrayType.Vertex] = _vertices.ToArray();
            arrays[(int)Mesh.ArrayType.Normal] = _normals.ToArray();
            arrays[(int)Mesh.ArrayType.Tangent] = _tangents.ToArray();
            arrays[(int)Mesh.ArrayType.TexUV] = _uvs.ToArray();
            arrays[(int)Mesh.ArrayType.Index] = _indices.ToArray();
            var mesh = new ArrayMesh();
            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
            return mesh;
        }
    }
}
