using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;

namespace OpenMakaiRanch.World;

/// <summary>
/// Shared spatial authoring input for runtime anchors and offline Blender scenery.
/// It owns no save state, service, schedule or reward. Invalid authoring fails before placement.
/// </summary>
public static class OrganicWorldLayout
{
    public const string SourcePath = "res://data/world/organic_layout.json";
    private static readonly Lazy<string> Source = new(() =>
    {
        if (!Godot.FileAccess.FileExists(SourcePath))
            throw new InvalidOperationException("Missing organic world layout: " + SourcePath);
        return Godot.FileAccess.GetFileAsString(SourcePath);
    });
    private static readonly Lazy<IReadOnlyList<RanchBuildingPlot>> Town = new(() => ParsePlots(Source.Value, "town"));
    public static IReadOnlyList<RanchBuildingPlot> TownPlots => Town.Value;
    public static IReadOnlyList<RanchBuildingPlot> LoadRanchPlots() => ParsePlots(Source.Value, "ranch");

    // Reuse the existing footprint/entrance geometry record; its name is retained for existing callers.
    public static IReadOnlyList<RanchBuildingPlot> ParsePlots(string json, string area)
    {
        if (area is not ("ranch" or "town")) throw new ArgumentException("Unknown world area", nameof(area));
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (root.GetProperty("version").GetInt32() != 1)
            throw new InvalidOperationException("Unsupported organic world layout version");
        var definition = root.GetProperty(area);
        var half = ReadVector(definition.GetProperty("half_extents"));
        if (half.X <= 0 || half.Y <= 0) throw new InvalidOperationException("Invalid world bounds");
        var bounds = new Rect2(-half, half * 2);
        var result = new List<RanchBuildingPlot>();
        foreach (var item in definition.GetProperty("plots").EnumerateArray())
        {
            var id = item.GetProperty("id").GetString();
            var center = ReadVector(item.GetProperty("center"));
            var footprint = ReadVector(item.GetProperty("footprint"));
            var yaw = item.GetProperty("yaw").GetSingle();
            if (string.IsNullOrWhiteSpace(id) || result.Any(p => p.Id == id)
                || footprint.X <= 0 || footprint.Y <= 0 || !float.IsFinite(yaw))
                throw new InvalidOperationException("Invalid or duplicate world plot: " + id);
            var plot = new RanchBuildingPlot(id, center, footprint, yaw);
            if (!bounds.Encloses(plot.ReservedBounds) || !bounds.Encloses(plot.EntranceBounds))
                throw new InvalidOperationException("World plot leaves its finite area: " + id);
            result.Add(plot);
        }
        var expected = area == "ranch"
            ? new[] { "dairy_barn", "pasture", "workshop", "pharmacy_lab", "ranch_house", "pet_care" }
            : new[] { "general_store", "adventure_guild", "research_office", "tavern", "bathhouse", "town_hall" };
        if (!expected.Order().SequenceEqual(result.Select(p => p.Id).Order()))
            throw new InvalidOperationException("World layout must preserve all existing " + area + " physical plot identifiers");
        foreach (var plot in result)
            foreach (var other in result.Where(p => p != plot))
                if (plot.ReservedBounds.Intersects(other.ReservedBounds)
                    || plot.ReservedBounds.Intersects(other.EntranceBounds))
                    throw new InvalidOperationException($"{area}: {plot.Id} blocks {other.Id}");
        return result.AsReadOnly();
    }

    public static Vector3 Entrance(RanchBuildingPlot plot, float height = 0.7f) =>
        new Vector3(plot.Center.X, height, plot.Center.Y)
        + new Vector3(Mathf.Sin(plot.Yaw), 0, Mathf.Cos(plot.Yaw)) * (plot.Footprint.Y / 2 + 1.3f);

    private static Vector2 ReadVector(JsonElement element)
    {
        if (element.GetArrayLength() != 2) throw new InvalidOperationException("Expected a two-component world position");
        var value = new Vector2(element[0].GetSingle(), element[1].GetSingle());
        if (!value.IsFinite()) throw new InvalidOperationException("Nonfinite world position");
        return value;
    }
}
