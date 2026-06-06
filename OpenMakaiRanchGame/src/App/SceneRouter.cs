using System;
using Godot;
using OpenMakaiRanch.Ui;

namespace OpenMakaiRanch.App;

public partial class SceneRouter : Node
{
    public static SceneRouter Instance { get; private set; } = null!;

    private UiShellController? _shell;

    public override void _Ready()
    {
        if (Instance is not null && !ReferenceEquals(Instance, this) && GodotObject.IsInstanceValid(Instance))
        {
            GD.PushWarning("SceneRouter duplicate instance detected; keeping the first active instance.");
            QueueFree();
            return;
        }

        Instance = this;
    }

    public override void _ExitTree()
    {
        if (ReferenceEquals(Instance, this))
        {
            Instance = null!;
        }
    }

    public void RegisterShell(UiShellController shell)
    {
        _shell = shell;
    }

    public void UnregisterShell(UiShellController shell)
    {
        if (ReferenceEquals(_shell, shell))
        {
            _shell = null;
        }
    }

    public void Show(string screenId)
    {
        if (_shell is null)
        {
            GD.PushWarning($"SceneRouter.Show('{screenId}') ignored because no UI shell is registered.");
            return;
        }

        _shell.ShowScreen(screenId);
    }
}