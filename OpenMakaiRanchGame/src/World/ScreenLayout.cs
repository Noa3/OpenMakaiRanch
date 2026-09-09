using Godot;

namespace OpenMakaiRanch.World;

public enum ScreenAspectClass
{
    Compact,
    Standard,
    Wide,
    UltraWide
}

public readonly record struct ScreenLayoutMetrics(
    Vector2 ViewportSize,
    float AspectRatio,
    ScreenAspectClass AspectClass,
    float SafeLeft,
    float SafeTop,
    float SafeRight,
    float SafeBottom,
    float ContentLeft,
    float ContentRight,
    float ContentWidth)
{
    public float HorizontalCenter => (ContentLeft + ContentRight) * 0.5f;
}

/// <summary>
/// Shared responsive layout math for 16:9, 16:10, 21:9, 32:9 and mobile safe areas.
/// The 3D world is free to expand into extra width; important HUD controls stay inside a centered
/// readable region close to the 1920x1080 design width.
/// </summary>
public static class ScreenLayout
{
    public const float DesignWidth = 1920f;
    public const float DesignHeight = 1080f;
    public const float MaxHudContentWidth = 1920f;

    public static ScreenLayoutMetrics Calculate(Viewport viewport)
    {
        var viewportSize = viewport.GetVisibleRect().Size;
        var width = Mathf.Max(1f, viewportSize.X);
        var height = Mathf.Max(1f, viewportSize.Y);
        var aspect = width / height;

        var aspectClass = aspect switch
        {
            >= 2.60f => ScreenAspectClass.UltraWide, // 32:9 and similar
            >= 1.90f => ScreenAspectClass.Wide,      // 21:9 and similar
            >= 1.45f => ScreenAspectClass.Standard,  // 16:10, 16:9
            _ => ScreenAspectClass.Compact
        };

        var safe = ResolveSafeMargins(viewportSize);
        var readableWidth = Mathf.Min(width - safe.X - safe.Z, MaxHudContentWidth);
        readableWidth = Mathf.Max(320f, readableWidth);

        var extra = Mathf.Max(0f, width - safe.X - safe.Z - readableWidth);
        var contentLeft = safe.X + extra * 0.5f;
        var contentRight = width - safe.Z - extra * 0.5f;

        return new ScreenLayoutMetrics(
            viewportSize,
            aspect,
            aspectClass,
            safe.X,
            safe.Y,
            safe.Z,
            safe.W,
            contentLeft,
            contentRight,
            readableWidth);
    }

    /// <summary>
    /// Returns logical viewport-space safe margins. Godot exposes physical safe-area pixels on
    /// Android/iOS; desktop uses zero because an ordinary window already excludes taskbars/notches.
    /// </summary>
    private static Vector4 ResolveSafeMargins(Vector2 viewportSize)
    {
        if (!(OS.HasFeature("android") || OS.HasFeature("ios") || OS.HasFeature("mobile")))
        {
            return Vector4.Zero;
        }

        var safeRect = DisplayServer.GetDisplaySafeArea();
        var screenSize = DisplayServer.ScreenGetSize();
        if (screenSize.X <= 0 || screenSize.Y <= 0 || safeRect.Size.X <= 0 || safeRect.Size.Y <= 0)
        {
            return Vector4.Zero;
        }

        var scaleX = viewportSize.X / screenSize.X;
        var scaleY = viewportSize.Y / screenSize.Y;

        var left = Mathf.Max(0f, safeRect.Position.X * scaleX);
        var top = Mathf.Max(0f, safeRect.Position.Y * scaleY);
        var rightPixels = Mathf.Max(0, screenSize.X - (safeRect.Position.X + safeRect.Size.X));
        var bottomPixels = Mathf.Max(0, screenSize.Y - (safeRect.Position.Y + safeRect.Size.Y));
        var right = rightPixels * scaleX;
        var bottom = bottomPixels * scaleY;

        return new Vector4(left, top, right, bottom);
    }
}
