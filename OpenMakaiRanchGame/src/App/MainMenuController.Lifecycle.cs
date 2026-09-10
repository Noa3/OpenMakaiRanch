using Godot;

namespace OpenMakaiRanch.App;

public partial class MainMenuController
{
    public override void _ExitTree()
    {
        // GameRoot is the persistent autoload; NewGame/Load replace its state, not the node.
        // A scene-local menu must not remain in this C# event after ChangeSceneToFile frees it.
        if (GodotObject.IsInstanceValid(GameRoot.Instance))
            GameRoot.Instance.StateChanged -= RefreshState;
    }
}
