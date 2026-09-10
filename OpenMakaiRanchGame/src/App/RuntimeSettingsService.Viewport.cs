using Godot;

namespace OpenMakaiRanch.App;

public partial class RuntimeSettingsService
{
    private Window? _canvasWindow;
    private bool _updatingCanvas;

    public override void _EnterTree()
    {
        _canvasWindow = GetTree().Root;
        _canvasWindow.SizeChanged += RefreshCanvasBase;
        RefreshCanvasBase();
    }

    public override void _ExitTree()
    {
        if (GodotObject.IsInstanceValid(_canvasWindow))
            _canvasWindow!.SizeChanged -= RefreshCanvasBase;
        _canvasWindow = null;
    }

    /// <summary>
    /// Never shrink a 1920-wide desktop interface onto a small window. Below the authored
    /// baseline use actual pixels so existing compact layouts can reflow; above it retain
    /// proportional HiDPI scaling and expanded ultrawide space. User UI scale remains separate.
    /// </summary>
    public static Vector2I CanvasBaseFor(Vector2I physicalSize)
    {
        var width = Mathf.Max(1, physicalSize.X);
        var height = Mathf.Max(1, physicalSize.Y);
        var scale = Mathf.Max(1f, Mathf.Min(width / 1920f, height / 1080f));
        return new Vector2I(Mathf.Max(1, Mathf.RoundToInt(width / scale)),
            Mathf.Max(1, Mathf.RoundToInt(height / scale)));
    }

    private void RefreshCanvasBase()
    {
        if (_updatingCanvas || !GodotObject.IsInstanceValid(_canvasWindow)) return;
        var desired = CanvasBaseFor(_canvasWindow!.Size);
        if (_canvasWindow.ContentScaleSize == desired) return;
        _updatingCanvas = true;
        try { _canvasWindow.ContentScaleSize = desired; }
        finally { _updatingCanvas = false; }
    }
}
