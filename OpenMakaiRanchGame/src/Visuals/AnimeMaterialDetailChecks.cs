using System;
using Godot;

namespace OpenMakaiRanch.Visuals;

/// <summary>Checks authored texture isolation, UV validation and original calibration mesh contracts.</summary>
internal static class AnimeMaterialDetailChecks
{
    public static void Run(Action<string, bool> check)
    {
        var (color, mask) = AnimeCalibrationTextures.Eye();
        var profile = AnimeSurfaceProfile.Create(AnimeSurfaceKind.Eye, new Color(0.75f, 0.45f, 0.1f));
        profile.AlbedoTexture = color; profile.TintMaskTexture = mask;
        profile.UvScale = new Vector2(2f, 0.5f); profile.UvOffset = new Vector2(0.1f, 0.2f);
        var material = AnimeMaterialFactory.Create(profile, "High");
        check("eye art does not enable the spherical procedural iris", !(bool)material.GetShaderParameter("procedural_iris"));
        check("selective tint preserves the authored mask reference", material.GetShaderParameter("tint_mask_texture").AsGodotObject() == mask);
        check("selective tint is enabled explicitly", (bool)material.GetShaderParameter("use_tint_mask"));
        check("authored UV scale transfers", (Vector2)material.GetShaderParameter("uv_scale") == profile.UvScale);
        check("authored UV offset transfers", (Vector2)material.GetShaderParameter("uv_offset") == profile.UvOffset);
        var data = mask.GetImage();
        check("eye whites are excluded from tint", data.GetPixel(12, 64).R < 0.01f);
        check("eye pupil is excluded from tint", data.GetPixel(128, 63).R < 0.01f);
        check("iris remains tintable", data.GetPixel(154, 64).R > 0.9f);
        check("small original eye maps have mipmaps", color.GetImage().HasMipmaps() && data.HasMipmaps());
        profile.UvScale = new Vector2(float.NaN, 0f); profile.UvOffset = new Vector2(float.PositiveInfinity, 1000f);
        material = AnimeMaterialFactory.Create(profile, "Low");
        check("invalid UV scales have finite defaults", (Vector2)material.GetShaderParameter("uv_scale") == Vector2.One);
        check("invalid UV offsets are bounded", (Vector2)material.GetShaderParameter("uv_offset") == new Vector2(0f, 64f));
        check("UV validation does not mutate caller resources", float.IsNaN(profile.UvScale.X) && float.IsPositiveInfinity(profile.UvOffset.X));
        profile.TintMaskTexture = null;
        check("missing tint mask keeps full palette behavior", !(bool)AnimeMaterialFactory.Create(profile).GetShaderParameter("use_tint_mask"));

        foreach (var (name, mesh) in new[]
        {
            ("Head", AnimeCalibrationGeometry.Head()), ("Hair", AnimeCalibrationGeometry.Hair()),
            ("Eye", AnimeCalibrationGeometry.Eye(1f)), ("Lines", AnimeCalibrationGeometry.FacialLines()),
            ("Mouth", AnimeCalibrationGeometry.Mouth())
        })
        {
            var arrays = mesh.SurfaceGetArrays(0);
            var vertices = arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
            var normals = arrays[(int)Mesh.ArrayType.Normal].AsVector3Array();
            var tangents = arrays[(int)Mesh.ArrayType.Tangent].AsFloat32Array();
            var uvs = arrays[(int)Mesh.ArrayType.TexUV].AsVector2Array();
            var indices = arrays[(int)Mesh.ArrayType.Index].AsInt32Array();
            check(name + " geometry is bounded", vertices.Length is > 32 and < 20000 && indices.Length < 100000);
            var good = normals.Length == vertices.Length && uvs.Length == vertices.Length && tangents.Length == vertices.Length * 4;
            for (var i = 0; i < vertices.Length && good; i++)
                good &= vertices[i].IsFinite() && normals[i].IsFinite() && uvs[i].IsFinite()
                    && Mathf.Abs(normals[i].Length() - 1f) < 0.01f
                    && float.IsFinite(tangents[i * 4]) && float.IsFinite(tangents[i * 4 + 1]) && float.IsFinite(tangents[i * 4 + 2])
                    && Mathf.Abs(Mathf.Abs(tangents[i * 4 + 3]) - 1f) < 0.01f;
            foreach (var index in indices) good &= index >= 0 && index < vertices.Length;
            check(name + " geometry has finite normals UVs tangents and indices", good);
        }
    }
}
