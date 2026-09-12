using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>Opt-in WorldGame host. No economy, clock, movement tuning, or production activation.</summary>
public partial class CoastalRegionController : WorldGameController
{
    public CoastalRegionTerrain? Terrain { get; private set; }
    public bool RegionReady { get; private set; }
    public string RegionError { get; private set; } = "";
    public int WalkingTransitionCount { get; private set; }
    public string LastTransitionBlocker { get; private set; } = "";
    private double _cooldown;
    private readonly Dictionary<string, Node3D> _roots = new();

    public override void _EnterTree()
    {
        try
        {
            Terrain = CoastalRegionTerrain.Load();
            foreach (var id in new[] { "ranch", "town" })
            {
                var root = GetNode<Node3D>(id == "ranch" ? RanchPath : TownPath);
                var area = Terrain.Areas[id];
                _roots[id] = root;
                root.Transform = area.Transform;
                foreach (var name in new[] { "Ground", "Wall1", "Wall2", "InteriorWall" })
                {
                    if (root.GetNodeOrNull<Node>(name) is not { } obsolete) continue;
                    obsolete.ProcessMode = ProcessModeEnum.Disabled;
                    if (obsolete is Node3D visual) visual.Visible = false;
                    if (obsolete is CollisionObject3D body) { body.CollisionLayer = 0; body.CollisionMask = 0; }
                    foreach (var shape in Descendants(obsolete).OfType<CollisionShape3D>()) shape.Disabled = true;
                }
                var boundary = root.GetNode<WorldBoundaryBuilder>("WorldBoundary");
                boundary.RegionalBoundary = area.Boundary;
                boundary.RegionalHeightRange = area.HeightRange;
                root.GetNode<SimpleNavigationRegionBuilder>("NavigationRegion").RegionalBakeBounds = area.NavigationBounds;
                root.GetNode<RosterRig>(id == "ranch" ? "RosterRig" : "CompanionRig").BindCoastalSurface(this, id);
                if (!area.TryHeight(new Vector2(area.Spawn.X, area.Spawn.Z), out var spawnGround))
                    throw new InvalidOperationException(id + ": spawn outside heightfield");
                root.GetNode<Node3D>("Player").Position = area.Spawn + Vector3.Up * spawnGround;
                if (!area.TryHeight(new Vector2(area.QuickPortal.X, area.QuickPortal.Z), out var portalGround))
                    throw new InvalidOperationException(id + ": quick portal outside heightfield");
                root.GetNode<Node3D>(id == "ranch" ? "TravelToTown" : "TravelToRanch").Position = area.QuickPortal + Vector3.Up * portalGround;
                var authored = root.GetNode<Node3D>("CoastalModules");
                foreach (var module in area.Modules)
                {
                    var instance = authored.GetChildren().OfType<Node3D>().SingleOrDefault(n => n.SceneFilePath == module);
                    if (instance is null) throw new InvalidOperationException("Missing authored coastal module instance: " + module);
                    if ((module.EndsWith("_terrain.glb") || module.EndsWith("_safety.glb") || module.EndsWith("_bridge.glb") || module.EndsWith("_pier.glb"))
                        && !Descendants(instance).OfType<CollisionShape3D>().Any(s => s.Shape is not null && !s.Disabled))
                        throw new InvalidOperationException("Coastal module missing imported -col/-colonly collision: " + module);
                }
            }
        }
        catch (Exception error)
        {
            RegionError = error.Message;
            GD.PushError("COASTAL REGION BLOCKED: " + RegionError);
            ProcessMode = ProcessModeEnum.Disabled;
        }
    }

    public override void _Ready()
    {
        if (RegionError.Length > 0) return;
        base._Ready();
        // Buildings and service anchors are created by existing presentation controllers before this host.
        foreach (var (id, root) in _roots)
        {
            // Existing player yaw is world-relative. TopLevel isolates that unchanged controller
            // from the town's rotated regional frame without changing movement tuning.
            foreach (var collision in Descendants(root).OfType<CollisionObject3D>())
                collision.DisableMode = CollisionObject3D.DisableModeEnum.Remove;
            var player = root.GetNode<ThirdPersonPlayerController>("Player");
            var playerPose = player.GlobalTransform;
            player.TopLevel = true;
            player.GlobalTransform = playerPose;
            foreach (var node in Descendants(root).OfType<Node3D>().Where(n => n is WorldStation or TownServicePoint or WalkInBuilding))
            {
                var local = root.ToLocal(node.GlobalPosition);
                if (!Terrain!.Areas[id].TryHeight(new Vector2(local.X, local.Z), out var ground))
                    throw new InvalidOperationException($"{id}: anchor outside generated terrain: {node.GetPath()}");
                local.Y += ground;
                node.GlobalPosition = root.ToGlobal(local);
            }
        }
        RegionReady = true;
    }

    public Node3D GetAreaRoot(string areaId) => _roots[areaId];
    public Vector3 GetSeamWorld(string areaId) => _roots[areaId].ToGlobal(Terrain!.Areas[areaId].Seam);
    public Vector3 GetSpawnWorld(string areaId)
    {
        var area = Terrain!.Areas[areaId];
        if (!area.TryHeight(new Vector2(area.Spawn.X, area.Spawn.Z), out var height))
            throw new InvalidOperationException(areaId + ": spawn outside heightfield");
        return _roots[areaId].ToGlobal(area.Spawn + Vector3.Up * height);
    }
    public bool NavigationReady => RegionReady && _roots.Values.All(r => r.GetNode<SimpleNavigationRegionBuilder>("NavigationRegion").CollisionBakeComplete);

    /// <summary>Ground height in regional world space; exported triangle surface, with dry physical decks taking precedence.</summary>
    public Vector3 GroundWorldPosition(string areaId, Vector3 worldPosition, bool usePhysics = true)
    {
        var root = _roots[areaId];
        var local = root.ToLocal(worldPosition);
        if (!Terrain!.Areas[areaId].TryWalkingHeight(new Vector2(local.X, local.Z), out var height))
            throw new ArgumentOutOfRangeException(nameof(worldPosition), $"Outside {areaId} coastal heightfield");
        var ground = root.ToGlobal(new Vector3(local.X, height, local.Z));
        if (usePhysics && RegionReady && root.IsInsideTree() && root.CanProcess())
        {
            var query = PhysicsRayQueryParameters3D.Create(
                new Vector3(worldPosition.X, Math.Max(worldPosition.Y, ground.Y) + 1.5f, worldPosition.Z),
                new Vector3(worldPosition.X, ground.Y - 0.5f, worldPosition.Z), 1);
            var excluded = new Godot.Collections.Array<Rid>();
            foreach (var areaRoot in _roots.Values)
                excluded.Add(areaRoot.GetNode<CollisionObject3D>("Player").GetRid());
            query.Exclude = excluded;
            var hit = root.GetWorld3D().DirectSpaceState.IntersectRay(query);
            if (hit.Count > 0 && hit["normal"].AsVector3().Y > 0.5f) ground = hit["position"].AsVector3();
        }
        return ground;
    }

    public override void _PhysicsProcess(double delta)
    {
        _cooldown = Math.Max(0, _cooldown - delta);
        if (!RegionReady || !NavigationReady || !WorldActionsAvailable || _cooldown > 0 || ActiveAreaId is not ("ranch" or "town")) return;
        var player = GetActivePlayer();
        if (player is null) return;
        var area = Terrain!.Areas[ActiveAreaId];
        var direction = _roots[ActiveAreaId].GlobalBasis * area.Departure;
        var offset = player.GlobalPosition - GetSeamWorld(ActiveAreaId);
        if (new Vector2(offset.X, offset.Z).Length() <= 4 && offset.Dot(direction) >= 0
            && player.Velocity.Dot(direction) > 0.1f) TryWalkingTransition();
    }

    /// <summary>Seam-only handoff. Rejects remote calls; never uses quick-travel spawn or moves a follower to catch up.</summary>
    public bool TryWalkingTransition()
    {
        LastTransitionBlocker = "";
        if (!RegionReady || !NavigationReady || !WorldActionsAvailable || FirstDayFlow?.IsActive == true || _cooldown > 0
            || ActiveAreaId is not ("ranch" or "town")) return false;
        var from = ActiveAreaId; var to = from == "ranch" ? "town" : "ranch";
        var player = GetActivePlayer();
        if (player is null || player.GlobalPosition.DistanceTo(GetSeamWorld(from)) > 4.5f)
        { LastTransitionBlocker = "Player has not reached the coincident walking seam."; return false; }
        var sourceRoster = from == "ranch" ? Ranch?.Roster : Town?.Companion;
        var targetRoster = to == "ranch" ? Ranch?.Roster : Town?.Companion;
        var companionId = sourceRoster?.ActiveCompanionId ?? "";
        Transform3D? companionPose = null;
        if (companionId.Length > 0)
        {
            if (sourceRoster is null || !sourceRoster.TryGetAvatar(companionId, out var avatar) || avatar is null
                || avatar.GlobalPosition.DistanceTo(GetSeamWorld(from)) > 4.5f)
            { LastTransitionBlocker = "Wait for the companion at the walking seam."; return false; }
            companionPose = avatar.GlobalTransform;
        }
        var pose = player.GlobalTransform; var velocity = player.Velocity;
        var camera = from == "ranch" ? Ranch?.CameraRig : Town?.CameraRig;
        var yaw = camera?.Yaw ?? 0; var pitch = camera?.Pitch ?? 0; var distance = camera?.Distance ?? 7;
        if (!ActivateCoastalArea(to)) return false;
        var replacement = GetActivePlayer()!;
        replacement.GlobalTransform = pose; replacement.Velocity = velocity;
        var nextCamera = to == "ranch" ? Ranch?.CameraRig : Town?.CameraRig;
        if (nextCamera is not null) { nextCamera.Yaw = yaw; nextCamera.Pitch = pitch; nextCamera.Distance = distance; }
        if (companionPose is { } saved && targetRoster is not null && targetRoster.TryGetAvatar(companionId, out var companion) && companion is not null)
            companion.GlobalTransform = saved;
        _cooldown = 1.0;
        WalkingTransitionCount++;
        return true;
    }

    private static IEnumerable<Node> Descendants(Node node)
    {
        foreach (var child in node.GetChildren())
        {
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }
}
