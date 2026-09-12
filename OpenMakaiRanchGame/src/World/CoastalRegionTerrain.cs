using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Godot;

namespace OpenMakaiRanch.World;

/// <summary>Validated, immutable regional authoring data. Heights use the exported mesh diagonal.</summary>
public sealed class CoastalRegionTerrain
{
    public const string HeightfieldPath = "res://data/world/coastal_heightfields.json";
    public sealed record Deck(string Id, Vector3 Start, Vector3 End, float Width)
    {
        public bool Contains(Vector2 point)
        {
            var axis = new Vector2(End.X - Start.X, End.Z - Start.Z);
            var length = axis.Length();
            var direction = axis / length;
            var offset = point - new Vector2(Start.X, Start.Z);
            var along = offset.Dot(direction);
            const float precision = 0.0001f;
            return point.IsFinite() && along >= -precision && along <= length + precision
                && Mathf.Abs(offset.Cross(direction)) <= Width / 2 + precision;
        }
    }
    public sealed record Area(string Id, Vector3 Origin, float Yaw, Rect2 Bounds, Vector2 HeightRange,
        Vector2[] Boundary, Vector3 Spawn, Vector3 QuickPortal, Vector3 Seam, Vector3 Departure,
        float Step, int Columns, int Rows, float[] Heights, string[] Modules)
    {
        public IReadOnlyList<Deck> Decks { get; init; } = Array.Empty<Deck>();
        public Transform3D Transform => new(new Basis(Vector3.Up, Yaw), Origin);
        public Aabb NavigationBounds => new(new Vector3(Bounds.Position.X, HeightRange.X, Bounds.Position.Y),
            new Vector3(Bounds.Size.X, HeightRange.Y - HeightRange.X, Bounds.Size.Y));

        public bool TryWalkingHeight(Vector2 point, out float height)
        {
            if (!TryHeight(point, out height)) return false;
            foreach (var deck in Decks)
                if (deck.Contains(point)) height = Mathf.Max(height, deck.Start.Y);
            return true;
        }

        // Keep raw terrain queries available: a bridge must not fill the carved stream bed.
        public bool TryHeight(Vector2 point, out float height)
        {
            height = 0;
            var grid = (point - Bounds.Position) / Step;
            if (!point.IsFinite() || grid.X < 0 || grid.Y < 0 || grid.X > Columns - 1 || grid.Y > Rows - 1)
                return false;
            var x = Math.Min((int)grid.X, Columns - 2);
            var z = Math.Min((int)grid.Y, Rows - 2);
            var u = grid.X - x; var v = grid.Y - z;
            var a = Heights[z * Columns + x]; var b = Heights[z * Columns + x + 1];
            var c = Heights[(z + 1) * Columns + x]; var d = Heights[(z + 1) * Columns + x + 1];
            height = u >= v ? a + u * (b - a) + v * (d - b) : a + v * (c - a) + u * (d - c);
            return true;
        }
    }

    public IReadOnlyDictionary<string, Area> Areas { get; }
    public string SourceSha256 { get; }
    private CoastalRegionTerrain(Dictionary<string, Area> areas, string hash) { Areas = areas; SourceSha256 = hash; }

    public static CoastalRegionTerrain Load()
    {
        if (!Godot.FileAccess.FileExists(HeightfieldPath))
            throw new InvalidOperationException("Coastal region requires generated heightfields: " + HeightfieldPath);
        var source = Godot.FileAccess.GetFileAsBytes(OrganicWorldLayout.SourcePath);
        return Parse(source, Godot.FileAccess.GetFileAsString(HeightfieldPath));
    }

    public static CoastalRegionTerrain Parse(byte[] source, string heightfields)
    {
        using var layout = JsonDocument.Parse(source);
        using var fields = JsonDocument.Parse(heightfields);
        var hash = Convert.ToHexString(SHA256.HashData(source)).ToLowerInvariant();
        var fieldRoot = fields.RootElement;
        if (fieldRoot.GetProperty("version").GetInt32() != 1 || fieldRoot.GetProperty("source_sha256").GetString() != hash)
            throw new InvalidOperationException("Coastal heightfield version/source SHA-256 mismatch; regenerate assets.");
        var definitions = layout.RootElement.GetProperty("region").GetProperty("areas");
        var areas = new Dictionary<string, Area>();
        foreach (var id in new[] { "ranch", "town" })
        {
            var def = definitions.GetProperty(id); var field = fieldRoot.GetProperty("areas").GetProperty(id);
            var bounds = ReadBounds(def.GetProperty("terrain_bounds"));
            if (ReadBounds(field.GetProperty("bounds")) != bounds) throw new InvalidOperationException(id + ": heightfield bounds mismatch");
            var step = field.GetProperty("step").GetSingle();
            var columns = field.GetProperty("columns").GetInt32(); var rows = field.GetProperty("rows").GetInt32();
            var heights = field.GetProperty("heights").EnumerateArray().Select(e => e.GetSingle()).ToArray();
            var range = V2(def.GetProperty("navigation_height_range"));
            if (step != 1f || columns < 2 || rows < 2 || heights.LongLength != (long)columns * rows
                || !Mathf.IsEqualApprox((columns - 1) * step, bounds.Size.X) || !Mathf.IsEqualApprox((rows - 1) * step, bounds.Size.Y)
                || heights.Any(h => !float.IsFinite(h) || h < range.X || h > range.Y) || range.Y <= range.X)
                throw new InvalidOperationException(id + ": invalid heightfield grid or navigation range");
            var modules = field.GetProperty("modules").EnumerateArray().Select(e => e.GetString() ?? "").ToArray();
            var kinds = id == "ranch"
                ? new[] { "terrain", "paths", "water", "safety", "bridge", "valley_bridge" }
                : new[] { "terrain", "paths", "water", "safety", "pier" };
            var required = kinds
                .Select(kind => $"res://assets/3d/coastal_region/{id}_{kind}.glb").ToArray();
            if (!modules.Order().SequenceEqual(required.Order())) throw new InvalidOperationException(id + ": invalid module manifest");
            var yaw = def.GetProperty("yaw").GetSingle();
            if (!float.IsFinite(yaw)) throw new InvalidOperationException(id + ": invalid yaw");
            var boundary = def.GetProperty("walkable_boundary").EnumerateArray().Select(V2).ToArray();
            if (boundary.Length < 3) throw new InvalidOperationException(id + ": missing walkable boundary");
            var area = new Area(id, V3(def.GetProperty("origin")), yaw, bounds, range, boundary,
                V3(def.GetProperty("spawn")), V3(def.GetProperty("quick_portal")), V3(def.GetProperty("seam")),
                V3(def.GetProperty("seam_departure_direction")).Normalized(), step, columns, rows, heights, modules);
            var region = layout.RootElement.GetProperty("region");
            if (id == "ranch")
            {
                var bridge = region.GetProperty("connection").GetProperty("bridge");
                if (bridge.GetProperty("module").GetString() != "ranch_valley_bridge.glb")
                    throw new InvalidOperationException("Unknown valley bridge module");
                var inverse = area.Transform.AffineInverse();
                var meadow = region.GetProperty("ranch_side_route").GetProperty("bridge");
                area = area with { Decks = new[]
                {
                    ReadDeck("valley", inverse * V3(bridge.GetProperty("start")),
                        inverse * V3(bridge.GetProperty("end")), bridge.GetProperty("clear_width").GetSingle()),
                    RectangleDeck("meadow", ReadBounds(meadow.GetProperty("deck_bounds")),
                        meadow.GetProperty("deck_elevation").GetSingle() - area.Origin.Y)
                } };
            }
            else
            {
                var pier = region.GetProperty("coast").GetProperty("pier");
                var rectangle = ReadBounds(pier.GetProperty("deck_bounds"));
                area = area with { Decks = new[]
                {
                    RectangleDeck("pier", rectangle, pier.GetProperty("deck_elevation").GetSingle()),
                    ReadDeck("pier_landing", V3(pier.GetProperty("shore_ramp_start")),
                        V3(pier.GetProperty("seaward_stop")), rectangle.Size.Y)
                } };
            }
            areas.Add(id, area);
        }
        if ((areas["ranch"].Transform * areas["ranch"].Seam).DistanceTo(areas["town"].Transform * areas["town"].Seam) > 0.01f)
            throw new InvalidOperationException("Coastal walking seam anchors are not coincident.");
        return new CoastalRegionTerrain(areas, hash);
    }

    private static Deck RectangleDeck(string id, Rect2 rectangle, float height) => ReadDeck(id,
        new Vector3(rectangle.Position.X, height, rectangle.GetCenter().Y),
        new Vector3(rectangle.End.X, height, rectangle.GetCenter().Y), rectangle.Size.Y);

    private static Deck ReadDeck(string id, Vector3 start, Vector3 end, float width)
    {
        if (!start.IsFinite() || !end.IsFinite() || !float.IsFinite(width) || width <= 0
            || new Vector2(end.X - start.X, end.Z - start.Z).Length() <= 0.01f
            || Mathf.Abs(end.Y - start.Y) > 0.001f)
            throw new InvalidOperationException("Invalid level coastal deck: " + id);
        return new Deck(id, start, end, width);
    }

    private static Rect2 ReadBounds(JsonElement e)
    {
        if (e.GetArrayLength() != 4) throw new InvalidOperationException("Expected regional bounds");
        var result = new Rect2(e[0].GetSingle(), e[1].GetSingle(), e[2].GetSingle(), e[3].GetSingle());
        if (!result.Position.IsFinite() || !result.Size.IsFinite() || result.Size.X <= 0 || result.Size.Y <= 0)
            throw new InvalidOperationException("Invalid regional bounds");
        return result;
    }
    private static Vector2 V2(JsonElement e)
    {
        if (e.GetArrayLength() != 2) throw new InvalidOperationException("Expected regional Vector2");
        var v = new Vector2(e[0].GetSingle(), e[1].GetSingle());
        if (!v.IsFinite()) throw new InvalidOperationException("Nonfinite regional Vector2");
        return v;
    }
    private static Vector3 V3(JsonElement e)
    {
        if (e.GetArrayLength() != 3) throw new InvalidOperationException("Expected regional Vector3");
        var v = new Vector3(e[0].GetSingle(), e[1].GetSingle(), e[2].GetSingle());
        if (!v.IsFinite()) throw new InvalidOperationException("Nonfinite regional Vector3");
        return v;
    }
}
