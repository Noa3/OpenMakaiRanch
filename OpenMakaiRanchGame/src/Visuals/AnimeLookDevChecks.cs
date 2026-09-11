using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Godot;

namespace OpenMakaiRanch.Visuals;

/// <summary>Rendered acceptance entry point for the disposable, autoload-free validation host only.</summary>
public partial class AnimeLookDevChecks : Node
{
    private readonly List<object> _checks = new();
    private readonly List<object> _captures = new();
    private int _failures;
    private AnimeLookDev? _study;
    private string _output = string.Empty;

    public override async void _Ready()
    {
        var canWrite = false;
        try
        {
            _output = OS.GetEnvironment("OMR_LOOKDEV_OUTPUT");
            var runId = OS.GetEnvironment("OMR_LOOKDEV_RUN_ID");
            if (string.IsNullOrWhiteSpace(_output) || string.IsNullOrWhiteSpace(runId)
                || !Path.IsPathFullyQualified(_output)
                || !File.Exists(Path.Combine(_output, "owner.txt"))
                || File.ReadAllText(Path.Combine(_output, "owner.txt")).Trim() != runId)
                throw new InvalidOperationException("Use Tools/Godot/anime_lookdev.py; missing disposable-output ownership marker.");
            canWrite = true;
            if (DisplayServer.GetName() == "headless")
                throw new InvalidOperationException("Rendered validation requires a display; dummy/headless rendering is not evidence.");
            Check("requested renderer is actually active", RenderingServer.GetCurrentRenderingMethod().ToString() == OS.GetEnvironment("OMR_LOOKDEV_RENDERER"));
            Contracts();
            _study = GD.Load<PackedScene>("res://scenes/dev/AnimeLookDev.tscn").Instantiate<AnimeLookDev>();
            AddChild(_study);
            await Frames(4);
            Check("study owns its 3D world", _study.StudyViewport.OwnWorld3D);
            Check("sample budget is bounded", _study.SampleCount is >= 16 and <= 32);
            Check("automatic exposure remains disabled", _study.StudyCamera.Attributes is CameraAttributesPractical { AutoExposureEnabled: false });
            var count = _study.SampleCount;
            var viewport = _study.StudyViewport;
            var environment = _study.StudyEnvironment;
            var root = _study.SpecimenRoot;
            var camera = _study.StudyCamera.Transform;
            var baseline = await Capture("neutral-high");
            _study.SetMatte(true);
            var matte = await Capture("neutral-matte");
            Check("A/B retains the camera", camera == _study.StudyCamera.Transform);
            Check("A/B retains world, geometry and environment instances", root == _study.SpecimenRoot && environment == _study.StudyEnvironment && viewport == _study.StudyViewport);
            Check("A/B changes rendered pixels", Difference(baseline, matte) > 0.0005);
            _study.SetMatte(false);
            for (var index = 1; index < AnimeLookDev.LightNames.Length; index++)
            {
                _study.SetLighting(index);
                await Capture(AnimeLookDev.LightNames[index].ToLowerInvariant() + "-high");
            }
            _study.SetLighting(0);
            foreach (var tier in new[] { "Low", "Medium", "Ultra" })
            {
                _study.SetQuality(tier);
                await Capture("neutral-" + tier.ToLowerInvariant());
            }
            _study.SetQuality("High");
            _study.SetDepthOfField(true);
            Check("gameplay framing never enables DOF", _study.StudyCamera.Attributes is CameraAttributesPractical { DofBlurFarEnabled: false });
            _study.SetPortrait(true);
            var expectedDof = RenderingServer.GetCurrentRenderingMethod().ToString() == "forward_plus";
            Check("portrait DOF follows renderer capability", ((CameraAttributesPractical)_study.StudyCamera.Attributes).DofBlurFarEnabled == expectedDof);
            await Capture("portrait-high");
            _study.SpecimenRoot.RotationDegrees = new Vector3(0f, 30f, 0f);
            await Capture("portrait-rotated-high");
            Check("quality and lighting changes do not add samples", _study.SampleCount == count);
            Check("sample viewport stays at native render scale", Mathf.IsEqualApprox(_study.StudyViewport.Scaling3DScale, 1f));
        }
        catch (Exception exception)
        {
            Check("acceptance exception: " + exception.Message, false);
            GD.PushError(exception.ToString());
        }
        if (canWrite)
        {
            File.WriteAllText(Path.Combine(_output, "results.json"), JsonSerializer.Serialize(new
            {
                schema = 1, source_commit = OS.GetEnvironment("OMR_LOOKDEV_SOURCE_COMMIT"),
                run_id = OS.GetEnvironment("OMR_LOOKDEV_RUN_ID"), engine = Engine.GetVersionInfo()["string"].ToString(),
                renderer = RenderingServer.GetCurrentRenderingMethod().ToString(), adapter = RenderingServer.GetVideoAdapterName(),
                scope = "Original material/viewport contract study; not production character art or target-hardware performance certification.",
                passed = _failures == 0, failures = _failures, checks = _checks, captures = _captures
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        GD.Print(_failures == 0 ? "ANIME LOOKDEV PASS" : "ANIME LOOKDEV FAIL");
        // Retire scene references before native .NET shutdown; never used by normal gameplay.
        if (_study is not null) { _study.QueueFree(); _study = null; await Frames(2); }
        GetTree().Quit(_failures == 0 ? 0 : 1);
    }

    private void Check(string name, bool passed)
    {
        _checks.Add(new { name, passed });
        if (!passed) _failures++;
        GD.Print($"LOOKDEV {(passed ? "OK" : "FAIL")}: {name}");
    }

    private void Contracts()
    {
        AnimeAvatarMaterialChecks.Run(Check);
        foreach (var kind in Enum.GetValues<AnimeSurfaceKind>())
        {
            var path = AnimeMaterialFactory.ResolveShader(kind, "High", "forward_plus");
            Check($"{kind} shader exists", ResourceLoader.Exists(path));
            var code = Regex.Replace(Godot.FileAccess.GetFileAsString(path), @"//[^\n]*", string.Empty);
            Check($"{kind} stays opaque", !Regex.IsMatch(code, @"\bALPHA\s*="));
        }
        Check("High Forward+ skin uses SSS", AnimeMaterialFactory.ResolveShader(AnimeSurfaceKind.Skin, "High", "forward_plus").EndsWith("anime_skin.gdshader", StringComparison.Ordinal));
        foreach (var tier in new[] { "Low", "Medium", "Custom", "unexpected" })
            Check($"{tier} skin has no SSS path", AnimeMaterialFactory.ResolveShader(AnimeSurfaceKind.Skin, tier, "forward_plus").EndsWith("anime_basic.gdshader", StringComparison.Ordinal));
        foreach (var renderer in new[] { "mobile", "gl_compatibility", "unknown" })
            Check($"{renderer} skin falls back", AnimeMaterialFactory.ResolveShader(AnimeSurfaceKind.Skin, "Ultra", renderer).EndsWith("anime_basic.gdshader", StringComparison.Ordinal));
        Check("Low hair has no anisotropy path", AnimeMaterialFactory.ResolveShader(AnimeSurfaceKind.Hair, "Low", "forward_plus").EndsWith("anime_basic.gdshader", StringComparison.Ordinal));
        var profile = AnimeSurfaceProfile.Create(AnimeSurfaceKind.Cloth, new Color(0.4f, 0.5f, 0.6f));
        profile.Roughness = float.NaN;
        profile.Specular = 10f;
        profile.Rim = -2f;
        var a = AnimeMaterialFactory.Create(profile);
        var b = AnimeMaterialFactory.Create(profile);
        Check("NaN roughness has finite default", Mathf.IsEqualApprox((float)a.GetShaderParameter("roughness"), 0.46f));
        Check("specular is bounded", Mathf.IsEqualApprox((float)a.GetShaderParameter("specular_strength"), 1f));
        Check("negative rim is bounded", Mathf.IsZeroApprox((float)a.GetShaderParameter("rim_strength")));
        Check("factory does not mutate caller profile", float.IsNaN(profile.Roughness) && profile.Specular == 10f && profile.Rim == -2f);
        Check("materials have independent instances", a != b);
        Check("shader resources are reused", a.Shader == b.Shader);
        a.SetShaderParameter("base_color", Colors.Red);
        Check("tint cannot leak between instances", (Color)b.GetShaderParameter("base_color") == profile.BaseColor);
        Check("missing albedo texture is explicitly disabled", !(bool)b.GetShaderParameter("use_albedo"));
        var image = Image.CreateEmpty(2, 2, false, Image.Format.Rgba8);
        image.Fill(Colors.White);
        profile.AlbedoTexture = ImageTexture.CreateFromImage(image);
        var textured = AnimeMaterialFactory.Create(profile);
        Check("authored texture reference is preserved", textured.GetShaderParameter("albedo_texture").AsGodotObject() == profile.AlbedoTexture);
        Check("authored texture is enabled", (bool)textured.GetShaderParameter("use_albedo"));
        var binding = new AnimeSurfaceBinding3D();
        var original = new StandardMaterial3D();
        var mesh = new SphereMesh { Material = original };
        var target = new MeshInstance3D { Mesh = mesh };
        AddChild(target);
        AddChild(binding);
        Check("out-of-range surface is refused", !binding.TryApply(target, 1, profile));
        target.MaterialOverride = original;
        Check("whole-mesh override is preserved", !binding.TryApply(target, 0, profile) && target.MaterialOverride == original);
        target.MaterialOverride = null;
        original.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        Check("transparent source is not made opaque", !binding.TryApply(target, 0, profile));
        original.Transparency = BaseMaterial3D.TransparencyEnum.Disabled;
        target.SetSurfaceOverrideMaterial(0, b);
        Check("unknown source shader is not replaced", !binding.TryApply(target, 0, profile));
        target.SetSurfaceOverrideMaterial(0, null);
        Check("explicit surface binding applies", binding.TryApply(target, 0, profile));
        Check("imported mesh material is unchanged", mesh.Material == original);
        var bound = target.GetSurfaceOverrideMaterial(0);
        Check("rebinding succeeds without stacking", binding.TryApply(target, 0, profile) && target.GetSurfaceOverrideMaterial(0) != bound);
        binding.Restore();
        Check("restore returns original null override", target.GetSurfaceOverrideMaterial(0) is null);
        binding.TryApply(target, 0, profile);
        target.SetSurfaceOverrideMaterial(0, original);
        binding.Restore();
        Check("restore cannot undo another system's edit", target.GetSurfaceOverrideMaterial(0) == original);
        target.QueueFree();
        binding.QueueFree();
    }

    private async Task Frames(int count)
    {
        for (var i = 0; i < count; i++)
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
    }

    private async Task<Image> Capture(string name)
    {
        await Frames(3);
        var image = _study!.StudyViewport.GetTexture().GetImage();
        if (image.IsEmpty()) throw new InvalidOperationException("Empty rendered viewport.");
        var file = name + ".png";
        if (image.SavePng(Path.Combine(_output, file)) != Error.Ok) throw new IOException("Could not write " + file);
        float min = 1f, max = 0f;
        for (var y = 0; y < image.GetHeight(); y += 16)
        for (var x = 0; x < image.GetWidth(); x += 16)
        {
            var color = image.GetPixel(x, y);
            var luminance = color.R * 0.2126f + color.G * 0.7152f + color.B * 0.0722f;
            min = Math.Min(min, luminance); max = Math.Max(max, luminance);
        }
        Check(name + " has rendered tonal variation", max - min > 0.08f);
        _captures.Add(new { file, width = image.GetWidth(), height = image.GetHeight(), luminance_min = min, luminance_max = max });
        return image;
    }

    private static double Difference(Image a, Image b)
    {
        if (a.GetSize() != b.GetSize()) return 0;
        double total = 0; var count = 0;
        for (var y = 0; y < a.GetHeight(); y += 8)
        for (var x = 0; x < a.GetWidth(); x += 8)
        {
            var left = a.GetPixel(x, y); var right = b.GetPixel(x, y);
            total += Math.Abs(left.R - right.R) + Math.Abs(left.G - right.G) + Math.Abs(left.B - right.B);
            count++;
        }
        return total / Math.Max(1, count * 3);
    }
}
