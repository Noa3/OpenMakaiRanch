using System;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Ui;

namespace OpenMakaiRanch.World;

public partial class PauseMenuController
{
    private RanchCornerPanel? _ranchCorner;
    public bool IsRanchCornerOpen => Visible && _ranchCorner?.Visible == true;

    public bool OpenCommunityBoardFromWorld(string areaId)
    {
        if (Visible || _communityBoard is null || _menuCenter is null) return false;
        Open(areaId);
        OpenCommunityBoard();
        return IsCommunityBoardOpen;
    }

    public bool OpenRanchCornerFromWorld(Func<bool> stillNear)
    {
        if (Visible || _menuCenter is null || stillNear is null || !stillNear()
            || GameRoot.Instance is not { } game || !GodotObject.IsInstanceValid(game)) return false;
        if (_ranchCorner is null)
        {
            _ranchCorner = new RanchCornerPanel { Name = "RanchCorner" };
            AddChild(_ranchCorner);
            _ranchCorner.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            _ranchCorner.OffsetLeft = 24;
            _ranchCorner.OffsetTop = 24;
            _ranchCorner.OffsetRight = -24;
            _ranchCorner.OffsetBottom = -24;
            _ranchCorner.BackRequested += Close;
            _ranchCorner.PlanningRequested += OpenManagement;
        }
        Open("ranch");
        _menuCenter.Visible = false;
        _ranchCorner.Open(game, stillNear);
        return IsRanchCornerOpen;
    }
}
