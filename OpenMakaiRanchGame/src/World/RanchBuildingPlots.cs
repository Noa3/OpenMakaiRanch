using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace OpenMakaiRanch.World;

public sealed record RanchBuildingPlot(string Id, Vector2 Center, Vector2 Footprint, float Yaw)
{
    // Conservatively includes roof overhang and clearance. Every visual grade keeps this envelope.
    public Rect2 ReservedBounds
    {
        get
        {
            var size = Footprint + Vector2.One;
            var c = Mathf.Abs(Mathf.Cos(Yaw)); var s = Mathf.Abs(Mathf.Sin(Yaw));
            var extent = new Vector2(c * size.X + s * size.Y, s * size.X + c * size.Y);
            return new Rect2(Center - extent / 2, extent);
        }
    }
    public Rect2 EntranceBounds
    {
        get
        {
            var forward = new Vector2(Mathf.Sin(Yaw), Mathf.Cos(Yaw));
            var c = Mathf.Abs(Mathf.Cos(Yaw)); var s = Mathf.Abs(Mathf.Sin(Yaw));
            var size = new Vector2(c * 2.2f + s * 2.6f, s * 2.2f + c * 2.6f);
            var center = Center + forward * (Footprint.Y / 2 + 1.3f);
            return new Rect2(center - size / 2, size);
        }
    }
}

/// <summary>Metre-scale authored plots, not arbitrary mesh scaling. No simulation state is changed here.</summary>
public static class RanchBuildingPlots
{
    public static readonly Rect2 WorldBounds = new(-60, -50, 120, 100);
    public static readonly Rect2 GateApproach = new(-1.6f, 7, 3.2f, 12.4f);
    public static IReadOnlyList<RanchBuildingPlot> All { get; } = OrganicWorldLayout.LoadRanchPlots();
    public static RanchBuildingPlot? Find(string id) => All.FirstOrDefault(plot => plot.Id == id);
    public static int VisualGrade(int simulationLevel) => Math.Clamp(simulationLevel, 0, 3);

    public static IReadOnlyList<string> Validate(IEnumerable<RanchBuildingPlot> source)
    {
        var plots = source.ToArray(); var errors = new List<string>();
        foreach (var plot in plots)
        {
            if (!plot.Center.IsFinite() || !plot.Footprint.IsFinite() || !float.IsFinite(plot.Yaw)
                || plot.Footprint.X < 4.6f || plot.Footprint.Y < 4.4f)
            { errors.Add(plot.Id + ": invalid dimensions"); continue; }
            if (plots.Count(other => other.Id == plot.Id) != 1) errors.Add(plot.Id + ": duplicate plot");
            if (!WorldBounds.Encloses(plot.ReservedBounds) || !WorldBounds.Encloses(plot.EntranceBounds))
                errors.Add(plot.Id + ": outside ranch bounds");
            if (plot.ReservedBounds.Intersects(GateApproach)) errors.Add(plot.Id + ": blocks town gate approach");
            foreach (var other in plots.Where(other => other.Id != plot.Id))
            {
                if (plot.ReservedBounds.Intersects(other.ReservedBounds)) errors.Add(plot.Id + ": overlaps " + other.Id);
                if (plot.ReservedBounds.Intersects(other.EntranceBounds)) errors.Add(plot.Id + ": blocks entrance of " + other.Id);
            }
        }
        return errors.AsReadOnly();
    }
}
