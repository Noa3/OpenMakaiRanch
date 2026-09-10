using System;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Two personal interaction points in the existing ranch. The area's existing nearest-target,
/// InputMap, touch assistance and station guard own interaction. This node never polls input,
/// pays rewards or changes the simulation; it opens the established pause-owned surfaces.
/// </summary>
public partial class RanchLeisureController : Node3D, IWorldCommandDispatcher
{
    public const string CornerId = "POINT_QUIET_CORNER";
    public const string BoardId = "POINT_COMMUNITY_BOARD";
    public static readonly Vector3 CornerApproach = new(-6.5f, 0.6f, 9.0f);
    public static readonly Vector3 BoardApproach = new(-2.25f, 0.6f, 3.7f);

    private RanchGreyboxController? _ranch;
    private WorldGameController? _world;
    private Node3D? _broken;
    private Node3D? _bench;
    private Label3D? _cornerLabel;
    private GameRoot? _game;
    public WorldStation? CornerStation { get; private set; }
    public WorldStation? BoardStation { get; private set; }
    public bool ShowsRestoredCorner => _bench?.Visible == true;

    public override void _Ready()
    {
        _ranch = GetParent() as RanchGreyboxController;
        _world = _ranch?.GetParent() as WorldGameController;
        if (_ranch is null || _world is null) return;

        // This child becomes ready before its parent ranch collects stations. No second target list.
        CornerStation = AddPoint(CornerId, "Quiet corner", CornerApproach);
        BoardStation = AddPoint(BoardId, "Community Board", BoardApproach);
        _cornerLabel = AddSign("Quiet corner", new Vector3(-6.5f, 1.8f, 8.0f));
        AddSign("Community Board", new Vector3(-2.25f, 2.25f, 2.9f));
        BuildCornerStandIn();
        _game = GameRoot.Instance;
        if (_game is not null && GodotObject.IsInstanceValid(_game))
            _game.StateChanged += RefreshFromGame;
        RefreshFromGame();
    }

    public override void _ExitTree()
    {
        if (_game is not null && GodotObject.IsInstanceValid(_game))
            _game.StateChanged -= RefreshFromGame;
    }

    private WorldStation AddPoint(string id, string label, Vector3 position)
    {
        var point = new WorldStation
        {
            Name = id, TargetId = id, CommandTargetId = id, Label = label, Position = position,
            RequiresWorker = false, SuccessFeedback = $"{label} opened.", Dispatcher = this,
            AvailabilityResolver = Availability
        };
        AddChild(point);
        return point;
    }

    private (bool Available, string Reason) Availability()
    {
        var game = GameRoot.Instance;
        if (game is null || !GodotObject.IsInstanceValid(game) || _world is null)
            return (false, "The ranch session is unavailable.");
        if (game.State.Calendar.Day < 2 || !game.State.Story.FirstDayCompleted)
            return (false, "Finish or skip the first day before using optional ranch activities.");
        if (game.CombatWorldTimeLocked || game.ActiveCombatSession is not null)
            return (false, "Finish the encounter first.");
        return (true, string.Empty);
    }

    public bool Dispatch(WorldCommand command, WorldInteractionContext context)
    {
        if (_world is null || _ranch is null || !Availability().Available
            || _world.IsManagementVisible || _world.FlowLocksUi || _world.PauseMenu?.IsOpen == true
            || _world.FirstDayFlow?.BlocksWorldInput == true || _world.Transition?.IsTransitioning == true
            || !_ranch.InputGate.WorldInputEnabled || GameRoot.Instance.StateGeneration != context.ExpectedGeneration)
            return false;
        var point = command.TargetId == CornerId ? CornerStation : command.TargetId == BoardId ? BoardStation : null;
        if (point is null || !IsNear(point)) return false;
        if (command.TargetId == BoardId)
            return _world.PauseMenu?.OpenCommunityBoardFromWorld("ranch") == true;
        return _world.PauseMenu?.OpenRanchCornerFromWorld(() => IsNear(CornerStation)) == true;
    }

    public bool IsNear(WorldStation? point)
    {
        if (_world is null || !GodotObject.IsInstanceValid(_world) || _world.ActiveAreaId != "ranch"
            || _ranch is null || !GodotObject.IsInstanceValid(_ranch) || !_ranch.IsVisibleInTree()
            || point is null || !GodotObject.IsInstanceValid(point) || !point.IsInsideTree()
            || _ranch.Player is not { } player || !GodotObject.IsInstanceValid(player) || !player.IsInsideTree())
            return false;
        return player.GlobalPosition.DistanceTo(point.GlobalPosition) <= _ranch.InteractionRange;
    }

    public void RefreshFromGame()
    {
        if (_game is null || !GodotObject.IsInstanceValid(_game) || _bench is null || _broken is null) return;
        var restored = _game.IsRanchCornerRestored;
        _bench.Visible = restored;
        _broken.Visible = !restored;
        if (_cornerLabel is not null)
            _cornerLabel.Text = restored ? "Quiet corner\nTake a break" : "Quiet corner\nA small restoration project";
    }

    private Label3D AddSign(string text, Vector3 position)
    {
        var label = new Label3D
        {
            Text = text, Position = position, FontSize = 28, OutlineSize = 5,
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled
        };
        AddChild(label);
        return label;
    }

    private void BuildCornerStandIn()
    {
        // Honest greybox presentation, not a new admitted production asset or navigation obstacle.
        // Both states are built once. Loads/repeated refreshes only switch visibility.
        _broken = new Node3D { Name = "UnrestoredCorner", Position = new Vector3(-6.5f, 0f, 8f) };
        _bench = new Node3D { Name = "RestoredCorner", Position = _broken.Position, Visible = false };
        AddChild(_broken);
        AddChild(_bench);
        var wood = new StandardMaterial3D { AlbedoColor = new Color("986a47"), Roughness = 0.9f };
        var oldWood = new StandardMaterial3D { AlbedoColor = new Color("71645a"), Roughness = 1f };
        for (var i = 0; i < 3; i++)
            Box(_broken, new Vector3(0f, 0.12f + i * 0.09f, i * 0.20f), new Vector3(1.8f, 0.12f, 0.18f), oldWood);
        Box(_bench, new Vector3(0f, 0.58f, 0f), new Vector3(2.2f, 0.15f, 0.65f), wood);
        Box(_bench, new Vector3(0f, 1f, -0.25f), new Vector3(2.2f, 0.55f, 0.12f), wood);
        Box(_bench, new Vector3(-0.82f, 0.26f, 0f), new Vector3(0.18f, 0.52f, 0.5f), wood);
        Box(_bench, new Vector3(0.82f, 0.26f, 0f), new Vector3(0.18f, 0.52f, 0.5f), wood);
    }

    private static void Box(Node parent, Vector3 position, Vector3 size, Material material) =>
        parent.AddChild(new MeshInstance3D { Position = position, Mesh = new BoxMesh { Size = size }, MaterialOverride = material });
}
