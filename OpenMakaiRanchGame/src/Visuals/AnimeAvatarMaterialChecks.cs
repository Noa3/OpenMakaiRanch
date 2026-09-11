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
        root.Free();
    }
}
