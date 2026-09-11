using System;
using Godot;

namespace OpenMakaiRanch.Visuals;

/// <summary>Small, deterministic original maps for the lab. No reference images or downloaded assets.</summary>
public static class AnimeCalibrationTextures
{
    public static (ImageTexture Color, ImageTexture Tint) Eye()
    {
        const int width = 256, height = 128;
        var albedo = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        var mask = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var u = (x + 0.5f) / width; var v = (y + 0.5f) / height;
            var p = new Vector2((u - 0.5f) / 0.215f, (v - 0.49f) / 0.48f);
            var radius = p.Length();
            var iris = 1f - Mathf.SmoothStep(0.95f, 1.02f, radius);
            var pupil = 1f - Mathf.SmoothStep(0.26f, 0.32f, radius);
            var ring = Mathf.SmoothStep(0.75f, 0.98f, radius);
            var fibers = Mathf.Sin(Mathf.Atan2(p.Y, p.X) * 46f + radius * 13f) * 0.035f;
            var value = Mathf.Clamp(0.91f - v * 0.27f + fibers - ring * 0.50f, 0.1f, 1f);
            var color = new Color(0.91f, 0.935f, 0.93f).Lerp(new Color(value, value, value), iris);
            color = color.Lerp(new Color(0.014f, 0.018f, 0.02f), iris * pupil);
            // A small painted catchlight is an art input, not emission or screen-space refraction.
            var catchlight = 1f - Mathf.SmoothStep(0.022f, 0.032f,
                new Vector2(u - 0.455f, (v - 0.71f) * 0.48f).Length());
            color = color.Lerp(new Color(0.99f, 0.99f, 0.99f), catchlight * iris);
            albedo.SetPixel(x, y, color);
            var tint = iris * (1f - pupil) * (1f - catchlight);
            mask.SetPixel(x, y, new Color(tint, tint, tint));
        }
        return (Finish(albedo), Finish(mask));
    }

    public static ImageTexture Skin()
    {
        var image = Image.CreateEmpty(256, 128, false, Image.Format.Rgba8);
        for (var y = 0; y < 128; y++)
        for (var x = 0; x < 256; x++)
        {
            var u = (x + 0.5f) / 256; var v = (y + 0.5f) / 128;
            float blush = 0f;
            foreach (var center in new[] { 0.42f, 0.58f })
                blush += Mathf.Exp(-Mathf.Pow((u - center) / 0.038f, 2f) - Mathf.Pow((v - 0.48f) / 0.056f, 2f));
            image.SetPixel(x, y, Colors.White.Lerp(new Color(1f, 0.74f, 0.76f), blush * 0.34f));
        }
        return Finish(image);
    }

    public static (ImageTexture Color, ImageTexture Surface) Hair()
    {
        var image = Image.CreateEmpty(128, 256, false, Image.Format.Rgba8);
        var surface = Image.CreateEmpty(128, 256, false, Image.Format.Rgba8);
        for (var y = 0; y < 256; y++)
        for (var x = 0; x < 128; x++)
        {
            var u = (x + 0.5f) / 128; var v = (y + 0.5f) / 256;
            var strands = 0.035f * Mathf.Cos(u * Mathf.Tau * 19f) + 0.018f * Mathf.Cos(u * Mathf.Tau * 37f);
            var value = 0.81f + 0.14f * Mathf.Sin(v * Mathf.Pi) + strands;
            image.SetPixel(x, y, new Color(value, value, value));
            // Linear material data, not glTF ORM. Preserve broad, moving specular response.
            surface.SetPixel(x, y, new Color(1f, 0.94f + strands, 0.85f + 0.12f * Mathf.Sin(v * Mathf.Pi), 1f));
        }
        return (Finish(image), Finish(surface));
    }

    private static ImageTexture Finish(Image image)
    {
        image.GenerateMipmaps();
        return ImageTexture.CreateFromImage(image);
    }
}
