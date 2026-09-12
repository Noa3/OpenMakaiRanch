using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

/// <summary>Real source/import contracts; not a substitute for physics traversal.</summary>
public static class CoastalRegionRegressionTests
{
    public static void Run(SmokeTestResult result)
    {
        void Check(bool pass, string message)
        { result.Passed &= pass; result.Lines.Add($"SMOKE {(pass ? "OK" : "FAIL")} coastal surface: {message}"); }
        var source = Godot.FileAccess.GetFileAsBytes(OrganicWorldLayout.SourcePath);
        var fields = Godot.FileAccess.GetFileAsString(CoastalRegionTerrain.HeightfieldPath);
        var terrain = CoastalRegionTerrain.Parse(source, fields);
        Check(terrain.Areas.Values.Sum(a => a.Modules.Length) == 11, "all eleven authored terrain/deck modules are accepted");
        foreach (var area in terrain.Areas.Values)
        {
            Check(!area.TryWalkingHeight(new Vector2(float.NaN, 0), out _), area.Id + ": nonfinite query rejected");
            foreach (var deck in area.Decks)
            {
                var point = (deck.Start + deck.End) / 2;
                Check(area.TryWalkingHeight(new Vector2(point.X, point.Z), out var height)
                    && Mathf.Abs(height - deck.Start.Y) < 0.015f,
                    area.Id + "/" + deck.Id + ": fallback selects the real dry deck elevation");
                Check(deck.Contains(new Vector2(deck.Start.X, deck.Start.Z))
                    && deck.Contains(new Vector2(deck.End.X, deck.End.Z)),
                    area.Id + "/" + deck.Id + ": both deck endpoints included");
            }
        }
        var ranch = terrain.Areas["ranch"];
        var valley = ranch.Decks.Single(d => d.Id == "valley");
        var middle = (valley.Start + valley.End) / 2;
        Check(ranch.TryHeight(new Vector2(middle.X, middle.Z), out var bed) && bed < valley.Start.Y - 0.5f,
            "raw terrain retains the carved stream beneath the separate walking deck");
        var modified = source.Concat(new byte[] { 32 }).ToArray();
        var refused = false;
        try { CoastalRegionTerrain.Parse(modified, fields); }
        catch (InvalidOperationException) { refused = true; }
        Check(refused, "stale source bytes are rejected rather than silently rebinding old geometry");
        var bridge = GD.Load<PackedScene>("res://assets/3d/coastal_region/ranch_valley_bridge.glb").Instantiate<Node3D>();
        Check(bridge.Scale == Vector3.One && Descendants(bridge).OfType<CollisionShape3D>().Count(s => s.Shape is not null && !s.Disabled) == 7,
            "real Godot bridge import retains unit scale and seven collision shapes");
        bridge.Free();
    }

    private static IEnumerable<Node> Descendants(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }
}
