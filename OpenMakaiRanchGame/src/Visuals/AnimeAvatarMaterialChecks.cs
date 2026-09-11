using System;
using Godot;

namespace OpenMakaiRanch.Visuals;

/// <summary>Generated-player preview boundaries; no GameRoot or player saves are involved.</summary>
internal static class AnimeAvatarMaterialChecks
{
    public static void Run(Action<string, bool> check)
    {
        var root = new Node3D();
        var original = new StandardMaterial3D { AlbedoColor = new Color(0.25f, 0.55f, 0.4f) };
        var skin = new MeshInstance3D { Name = "Head", Mesh = new SphereMesh(), MaterialOverride = original };
        var eye = new MeshInstance3D { Name = "EyeLeft", Mesh = new SphereMesh(), MaterialOverride = original };
        var unknown = new MeshInstance3D { Name = "Glasses", Mesh = new SphereMesh(), MaterialOverride = original };
        var nestedRoot = new Node3D();
        var nested = new MeshInstance3D { Name = "Head", Mesh = new SphereMesh(), MaterialOverride = original };
        var alpha = new StandardMaterial3D { Transparency = BaseMaterial3D.TransparencyEnum.Alpha };
        var hair = new MeshInstance3D { Name = "Hair", Mesh = new SphereMesh(), MaterialOverride = alpha };
        root.AddChild(skin); root.AddChild(eye); root.AddChild(unknown); root.AddChild(hair);
        root.AddChild(nestedRoot); nestedRoot.AddChild(nested);
        check("player preview off preserves all originals", AnimeAvatarMaterials.ApplyPlayer(root, "High", false) == 0 && skin.MaterialOverride == original);
        check("player preview only binds recognized opaque generated parts", AnimeAvatarMaterials.ApplyPlayer(root, "High", true) == 2);
        check("player preview preserves skin tint", skin.MaterialOverride is ShaderMaterial s && (Color)s.GetShaderParameter("base_color") == original.AlbedoColor);
        check("player preview preserves selected iris color", eye.MaterialOverride is ShaderMaterial e && (Color)e.GetShaderParameter("iris_color") == original.AlbedoColor);
        check("player preview leaves shared source materials unchanged", original.AlbedoColor == new Color(0.25f, 0.55f, 0.4f));
        check("player preview does not alter accessories", unknown.MaterialOverride == original);
        check("player preview does not traverse nested/imported models", nested.MaterialOverride == original);
        check("player preview preserves transparency", hair.MaterialOverride == alpha);
        check("player preview is idempotent for an existing generation", AnimeAvatarMaterials.ApplyPlayer(root, "High", true) == 0);
        var textureImage = Image.CreateEmpty(4, 4, false, Image.Format.Rgba8);
        textureImage.Fill(Colors.White);
        var textured = new StandardMaterial3D
        {
            AlbedoColor = new Color(0.8f, 0.9f, 0.7f), AlbedoTexture = ImageTexture.CreateFromImage(textureImage),
            Uv1Scale = new Vector3(2f, 0.5f, 1f), Uv1Offset = new Vector3(0.15f, 0.25f, 0f)
        };
        eye.MaterialOverride = textured;
        check("textured generated eye is eligible for explicit preview", AnimeAvatarMaterials.ApplyPlayer(root, "High", true) == 1);
        var converted = (ShaderMaterial)eye.MaterialOverride;
        check("textured eye is never painted over with procedural iris", !(bool)converted.GetShaderParameter("procedural_iris"));
        check("textured eye retains source albedo and tint", converted.GetShaderParameter("albedo_texture").AsGodotObject() == textured.AlbedoTexture
            && (Color)converted.GetShaderParameter("base_color") == textured.AlbedoColor);
        check("generated preview retains UV scale and offset", (Vector2)converted.GetShaderParameter("uv_scale") == new Vector2(2f, 0.5f)
            && (Vector2)converted.GetShaderParameter("uv_offset") == new Vector2(0.15f, 0.25f));
        root.Free();
    }
}
