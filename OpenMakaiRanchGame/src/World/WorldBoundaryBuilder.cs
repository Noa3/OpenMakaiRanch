using System;
using System.Collections.Generic;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.World;

/// <summary>
/// Finite-world contract: permanent collision boundaries prevent falling/escaping the authored map,
/// while quality-scaled seasonal trees/rocks make that boundary visually legible. Travel gates stay
/// as the intentional way out.
/// </summary>
public partial class WorldBoundaryBuilder : Node3D
{
    [Export] public Vector2 HalfExtents { get; set; } = new(19.25f, 14.25f);
    [Export] public float WallHeight { get; set; } = 4.0f;
    [Export] public float SouthGateHalfWidth { get; set; } = 3.0f;
    [Export] public string AreaStyle { get; set; } = "ranch";

    private Node3D? _collisionRoot;
    private Node3D? _dressingRoot;
    private Season _lastSeason = (Season)(-1);
    private float _lastDensity = -1f;

    public int DressingNodeCount => _dressingRoot?.GetChildCount() ?? 0;
    public bool HasCollisionBoundary => _collisionRoot?.GetChildCount() >= 4;

    public override void _Ready()
    {
        BuildCollisionOnce();
        RebuildDressing();

        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged += Refresh;
        }
    }

    public override void _ExitTree()
    {
        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        var game = GameRoot.Instance;
        if (game is null)
        {
            return;
        }

        var density = game.RuntimeSettings.DecorationDensity;
        var season = game.State.Calendar.Season;
        if (season != _lastSeason || Math.Abs(density - _lastDensity) > 0.01f)
        {
            RebuildDressing();
        }
    }

    private void BuildCollisionOnce()
    {
        if (_collisionRoot is not null)
        {
            return;
        }

        _collisionRoot = new Node3D { Name = "BoundaryCollision" };
        AddChild(_collisionRoot);

        // North/east/west are continuous. South is split around the authored gate so the visual
        // portal can be approached, while collision still closes the world outside the gate.
        AddWall("NorthBoundary", new Vector3(0, WallHeight * 0.5f, -HalfExtents.Y), new Vector3(HalfExtents.X * 2f, WallHeight, 0.8f));
        AddWall("WestBoundary", new Vector3(-HalfExtents.X, WallHeight * 0.5f, 0), new Vector3(0.8f, WallHeight, HalfExtents.Y * 2f));
        AddWall("EastBoundary", new Vector3(HalfExtents.X, WallHeight * 0.5f, 0), new Vector3(0.8f, WallHeight, HalfExtents.Y * 2f));

        var sideWidth = Math.Max(0.5f, HalfExtents.X - SouthGateHalfWidth);
        AddWall("SouthBoundaryLeft",
            new Vector3(-(SouthGateHalfWidth + sideWidth * 0.5f), WallHeight * 0.5f, HalfExtents.Y),
            new Vector3(sideWidth, WallHeight, 0.8f));
        AddWall("SouthBoundaryRight",
            new Vector3(SouthGateHalfWidth + sideWidth * 0.5f, WallHeight * 0.5f, HalfExtents.Y),
            new Vector3(sideWidth, WallHeight, 0.8f));
    }

    private void AddWall(string name, Vector3 position, Vector3 size)
    {
        var body = new StaticBody3D { Name = name, Position = position };
        var shape = new CollisionShape3D { Shape = new BoxShape3D { Size = size } };
        body.AddChild(shape);
        _collisionRoot!.AddChild(body);
    }

    private void RebuildDressing()
    {
        if (_dressingRoot is not null)
        {
            RemoveChild(_dressingRoot);
            _dressingRoot.QueueFree();
        }

        _dressingRoot = new Node3D { Name = "BoundaryDressing" };
        AddChild(_dressingRoot);

        var game = GameRoot.Instance;
        var season = game?.State.Calendar.Season ?? Season.Spring;
        var density = Mathf.Clamp(game?.RuntimeSettings.DecorationDensity ?? 0.7f, 0.30f, 1.30f);
        var spacing = Mathf.Lerp(5.0f, 2.1f, Mathf.Clamp((density - 0.30f) / 1.0f, 0f, 1f));

        AddEdgeDressing(new Vector3(-HalfExtents.X + 1f, 0, -HalfExtents.Y + 0.6f),
            new Vector3(HalfExtents.X - 1f, 0, -HalfExtents.Y + 0.6f), spacing, season, "North");
        AddEdgeDressing(new Vector3(-HalfExtents.X + 0.6f, 0, -HalfExtents.Y + 1f),
            new Vector3(-HalfExtents.X + 0.6f, 0, HalfExtents.Y - 1f), spacing, season, "West");
        AddEdgeDressing(new Vector3(HalfExtents.X - 0.6f, 0, -HalfExtents.Y + 1f),
            new Vector3(HalfExtents.X - 0.6f, 0, HalfExtents.Y - 1f), spacing, season, "East");

        // South leaves a readable gate opening.
        AddEdgeDressing(new Vector3(-HalfExtents.X + 1f, 0, HalfExtents.Y - 0.6f),
            new Vector3(-SouthGateHalfWidth - 1f, 0, HalfExtents.Y - 0.6f), spacing, season, "SouthL");
        AddEdgeDressing(new Vector3(SouthGateHalfWidth + 1f, 0, HalfExtents.Y - 0.6f),
            new Vector3(HalfExtents.X - 1f, 0, HalfExtents.Y - 0.6f), spacing, season, "SouthR");

        _lastSeason = season;
        _lastDensity = density;
    }

    private void AddEdgeDressing(Vector3 from, Vector3 to, float spacing, Season season, string prefix)
    {
        var distance = from.DistanceTo(to);
        if (distance < 0.1f)
        {
            return;
        }

        var steps = Math.Max(1, Mathf.FloorToInt(distance / spacing));
        for (var i = 0; i <= steps; i++)
        {
            var t = steps == 0 ? 0f : (float)i / steps;
            var pos = from.Lerp(to, t);
            var hash = StableHash(prefix, i);
            var jitter = new Vector3(((hash % 7) - 3) * 0.12f, 0, (((hash / 7) % 7) - 3) * 0.10f);
            pos += jitter;

            // Alternate tree/rock/fence clusters. Collisions are intentionally NOT attached to this
            // presentation layer; the stable boundary wall above is the gameplay contract.
            if (hash % 5 == 0)
            {
                AddRock($"{prefix}_Rock_{i}", pos, 0.55f + (hash % 4) * 0.08f, season);
            }
            else
            {
                AddTree($"{prefix}_Tree_{i}", pos, 0.85f + (hash % 5) * 0.05f, season);
            }

            if (AreaStyle == "ranch" && i % 2 == 0)
            {
                AddFencePost($"{prefix}_Fence_{i}", pos);
            }
        }
    }

    private void AddTree(string name, Vector3 pos, float scale, Season season)
    {
        var trunk = new MeshInstance3D
        {
            Name = $"{name}_Trunk",
            Position = pos + new Vector3(0, 0.85f * scale, 0),
            Mesh = new CylinderMesh
            {
                TopRadius = 0.12f * scale,
                BottomRadius = 0.18f * scale,
                Height = 1.7f * scale,
                RadialSegments = 8
            },
            MaterialOverride = Material(new Color(0.34f, 0.22f, 0.14f))
        };
        _dressingRoot!.AddChild(trunk);

        var crownColor = season switch
        {
            Season.Spring => new Color(0.44f, 0.72f, 0.40f),
            Season.Summer => new Color(0.25f, 0.58f, 0.30f),
            Season.Autumn => new Color(0.82f, 0.40f, 0.14f),
            Season.Winter => new Color(0.48f, 0.52f, 0.50f),
            _ => new Color(0.36f, 0.64f, 0.34f)
        };

        var crown = new MeshInstance3D
        {
            Name = $"{name}_Crown",
            Position = pos + new Vector3(0, 2.05f * scale, 0),
            Mesh = new SphereMesh
            {
                Radius = (season == Season.Winter ? 0.58f : 0.78f) * scale,
                Height = (season == Season.Winter ? 1.05f : 1.55f) * scale,
                RadialSegments = 10,
                Rings = 6
            },
            MaterialOverride = Material(crownColor)
        };
        _dressingRoot.AddChild(crown);
    }

    private void AddRock(string name, Vector3 pos, float scale, Season season)
    {
        var color = season == Season.Winter
            ? new Color(0.62f, 0.66f, 0.68f)
            : new Color(0.38f, 0.40f, 0.39f);
        var rock = new MeshInstance3D
        {
            Name = name,
            Position = pos + new Vector3(0, 0.35f * scale, 0),
            Rotation = new Vector3(0.08f, scale * 0.7f, -0.05f),
            Mesh = new SphereMesh
            {
                Radius = 0.58f * scale,
                Height = 0.75f * scale,
                RadialSegments = 8,
                Rings = 5
            },
            MaterialOverride = Material(color)
        };
        _dressingRoot!.AddChild(rock);
    }

    private void AddFencePost(string name, Vector3 pos)
    {
        var post = new MeshInstance3D
        {
            Name = name,
            Position = pos + new Vector3(0, 0.55f, 0),
            Mesh = new BoxMesh { Size = new Vector3(0.12f, 1.1f, 0.12f) },
            MaterialOverride = Material(new Color(0.34f, 0.24f, 0.16f))
        };
        _dressingRoot!.AddChild(post);
    }

    private static StandardMaterial3D Material(Color color) => new()
    {
        AlbedoColor = color,
        Roughness = 0.92f,
        Metallic = 0f
    };

    private static int StableHash(string prefix, int index)
    {
        unchecked
        {
            var hash = 17;
            foreach (var c in prefix)
            {
                hash = hash * 31 + c;
            }
            return Math.Abs(hash * 397 ^ index * 7919);
        }
    }
}
