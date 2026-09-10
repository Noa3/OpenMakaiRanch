using System;
using System.Collections.Generic;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Character;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.World;

public enum WorldSurfaceReaction
{
    None,
    WetStep,
    SnowTrack,
    LeafRustle
}

/// <summary>
/// Bounded presentation-only surface reactions for the active world.
/// It tracks only the player plus a small number of nearby roster avatars and reuses a strict
/// mark budget, so snow/rain feedback cannot grow without bound on long play sessions.
/// </summary>
public partial class WorldSurfaceInteractionController : Node3D
{
    [Export] public NodePath PlayerPath { get; set; } = "../Player";
    [Export] public NodePath RosterPath { get; set; } = "../RosterRig";
    [Export] public float StepDistance { get; set; } = 0.72f;
    [Export] public int MaxMarks { get; set; } = 64;
    [Export] public int MaxTrackedActors { get; set; } = 8;
    [Export] public int MaxPuddles { get; set; } = 10;

    private readonly Dictionary<ulong, Vector3> _lastPositions = new();
    private readonly Queue<MeshInstance3D> _marks = new();
    private ThirdPersonPlayerController? _player;
    private Node3D? _rosterRoot;
    private Node3D? _puddleRoot;
    private Weather _lastWeather = (Weather)(-1);
    private Season _lastSeason = (Season)(-1);
    private string _lastQuality = string.Empty;

    public int SurfaceMarkCount => _marks.Count;
    public int PuddleCount => _puddleRoot?.GetChildCount() ?? 0;
    public WorldSurfaceReaction CurrentReaction { get; private set; }

    public override void _Ready()
    {
        _player = GetNodeOrNull<ThirdPersonPlayerController>(PlayerPath);
        _rosterRoot = GetNodeOrNull<Node3D>(RosterPath);
        _puddleRoot = new Node3D { Name = "Puddles" };
        AddChild(_puddleRoot);

        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged += RefreshFromGame;
        }

        RefreshFromGame();
    }

    public override void _ExitTree()
    {
        if (GameRoot.Instance is { } game && GodotObject.IsInstanceValid(game))
        {
            game.StateChanged -= RefreshFromGame;
        }
    }

    public override void _Process(double delta)
    {
        var game = GameRoot.Instance;
        if (game is null)
        {
            return;
        }

        var calendar = game.State.Calendar;
        var quality = game.State.Settings.GraphicsQuality;
        if (calendar.CurrentWeather != _lastWeather || calendar.Season != _lastSeason
            || !string.Equals(quality, _lastQuality, StringComparison.Ordinal))
        {
            RefreshFromGame();
        }

        if (!game.RuntimeSettings.EffectiveWorldParticlesEnabled || CurrentReaction == WorldSurfaceReaction.None)
        {
            return;
        }

        var tracked = 0;
        if (_player is not null && GodotObject.IsInstanceValid(_player))
        {
            TrackActor(_player);
            tracked++;
        }

        if (_rosterRoot is null || !GodotObject.IsInstanceValid(_rosterRoot))
        {
            _rosterRoot = GetNodeOrNull<Node3D>(RosterPath);
        }

        if (_rosterRoot is null)
        {
            return;
        }

        foreach (var child in _rosterRoot.GetChildren())
        {
            if (tracked >= MaxTrackedActors)
            {
                break;
            }

            if (child is CharacterAvatar3D avatar && GodotObject.IsInstanceValid(avatar))
            {
                TrackActor(avatar);
                tracked++;
            }
        }
    }

    public void RefreshFromGame()
    {
        var game = GameRoot.Instance;
        if (game is null)
        {
            return;
        }

        var calendar = game.State.Calendar;
        var newReaction = ReactionFor(calendar.Season, calendar.CurrentWeather);
        if (newReaction != CurrentReaction)
        {
            ClearMarks();
            _lastPositions.Clear();
        }

        CurrentReaction = newReaction;
        _lastWeather = calendar.CurrentWeather;
        _lastSeason = calendar.Season;
        _lastQuality = game.State.Settings.GraphicsQuality;
        RebuildPuddles(calendar.CurrentWeather, game.RuntimeSettings.EffectiveWorldParticlesEnabled);
    }

    public static WorldSurfaceReaction ReactionFor(Season season, Weather weather)
    {
        if (OriginalCalendarRules.IsSnow(weather))
        {
            return WorldSurfaceReaction.SnowTrack;
        }

        if (OriginalCalendarRules.IsRain(weather))
        {
            return WorldSurfaceReaction.WetStep;
        }

        return season == Season.Autumn && !OriginalCalendarRules.IsSevere(weather)
            ? WorldSurfaceReaction.LeafRustle
            : WorldSurfaceReaction.None;
    }

    private void TrackActor(Node3D actor)
    {
        var current = actor.GlobalPosition;
        var id = actor.GetInstanceId();
        if (!_lastPositions.TryGetValue(id, out var previous))
        {
            _lastPositions[id] = current;
            return;
        }

        var planar = current - previous;
        planar.Y = 0f;
        if (planar.Length() < StepDistance)
        {
            return;
        }

        _lastPositions[id] = current;
        if (WorldShelterVolume.IsPointSheltered(GetTree(), current))
        {
            return;
        }

        AddSurfaceMark(actor, current);
    }

    private void AddSurfaceMark(Node3D actor, Vector3 position)
    {
        var budget = EffectiveMarkBudget();
        while (_marks.Count >= budget && _marks.Count > 0)
        {
            var old = _marks.Dequeue();
            if (GodotObject.IsInstanceValid(old))
            {
                old.QueueFree();
            }
        }

        var (mesh, color) = CurrentReaction switch
        {
            WorldSurfaceReaction.SnowTrack => ((PrimitiveMesh)new BoxMesh { Size = new Vector3(0.18f, 0.012f, 0.32f) }, new Color(0.66f, 0.74f, 0.82f, 0.58f)),
            WorldSurfaceReaction.WetStep => (new CylinderMesh { TopRadius = 0.22f, BottomRadius = 0.22f, Height = 0.010f, RadialSegments = 16 }, new Color(0.32f, 0.52f, 0.72f, 0.34f)),
            WorldSurfaceReaction.LeafRustle => ((PrimitiveMesh)new BoxMesh { Size = new Vector3(0.20f, 0.008f, 0.16f) }, new Color(0.84f, 0.39f, 0.10f, 0.58f)),
            _ => ((PrimitiveMesh)new BoxMesh { Size = Vector3.Zero }, Colors.Transparent)
        };

        var mark = new MeshInstance3D
        {
            Name = $"SurfaceMark_{CurrentReaction}",
            Mesh = mesh,
            MaterialOverride = SurfaceMaterial(color)
        };
        AddChild(mark);
        mark.GlobalPosition = new Vector3(position.X, 0.035f, position.Z);
        mark.GlobalRotation = new Vector3(0f, actor.GlobalRotation.Y, 0f);
        _marks.Enqueue(mark);
    }

    private void RebuildPuddles(Weather weather, bool enabled)
    {
        if (_puddleRoot is null)
        {
            return;
        }

        foreach (var child in _puddleRoot.GetChildren())
        {
            child.QueueFree();
        }

        if (!enabled || !OriginalCalendarRules.IsRain(weather))
        {
            return;
        }

        var count = EffectivePuddleBudget();
        var positions = new[]
        {
            new Vector3(-7.2f, 0.025f, -4.6f), new Vector3(5.8f, 0.025f, -6.2f),
            new Vector3(-2.8f, 0.025f, 5.4f), new Vector3(8.4f, 0.025f, 3.8f),
            new Vector3(-10.2f, 0.025f, 7.1f), new Vector3(2.1f, 0.025f, -9.0f),
            new Vector3(11.0f, 0.025f, -1.7f), new Vector3(-5.0f, 0.025f, 10.0f),
            new Vector3(6.7f, 0.025f, 9.2f), new Vector3(-11.2f, 0.025f, -8.0f)
        };

        for (var i = 0; i < Math.Min(count, positions.Length); i++)
        {
            var worldPoint = ToGlobal(positions[i]);
            if (WorldShelterVolume.IsPointSheltered(GetTree(), worldPoint))
            {
                continue;
            }

            var puddle = new MeshInstance3D
            {
                Name = $"Puddle_{i:00}",
                Position = positions[i],
                Scale = new Vector3(1f + (i % 3) * 0.22f, 1f, 0.72f + (i % 2) * 0.18f),
                Mesh = new CylinderMesh
                {
                    TopRadius = 0.48f,
                    BottomRadius = 0.48f,
                    Height = 0.012f,
                    RadialSegments = 24
                },
                MaterialOverride = SurfaceMaterial(new Color(0.18f, 0.34f, 0.52f, 0.34f), wet: true)
            };
            _puddleRoot.AddChild(puddle);
        }
    }

    private int EffectiveMarkBudget()
    {
        var profile = GraphicsQualityProfile.Resolve(GameRoot.Instance?.State.Settings.GraphicsQuality);
        return Math.Max(1, Math.Min(MaxMarks, profile.SurfaceMarkBudget));
    }

    private int EffectivePuddleBudget()
    {
        var profile = GraphicsQualityProfile.Resolve(GameRoot.Instance?.State.Settings.GraphicsQuality);
        return Math.Max(0, Math.Min(MaxPuddles, profile.PuddleBudget));
    }

    private void ClearMarks()
    {
        while (_marks.Count > 0)
        {
            var mark = _marks.Dequeue();
            if (GodotObject.IsInstanceValid(mark))
            {
                mark.QueueFree();
            }
        }
    }

    private static StandardMaterial3D SurfaceMaterial(Color color, bool wet = false)
    {
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            Roughness = wet ? 0.18f : 0.78f,
            Metallic = wet ? 0.08f : 0f,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
        };
    }
}
