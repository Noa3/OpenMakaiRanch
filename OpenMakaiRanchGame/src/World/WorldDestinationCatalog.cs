using System;
using Godot;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.World;

/// <summary>Stable presentation destinations. These are directions, never remote gameplay commands.</summary>
public sealed record WorldDestination(string AreaId, string TargetId, string Label)
{
    public string DisplayName => WorldName(TargetId, Label);
}

public static class WorldDestinationCatalog
{
    public static readonly WorldDestination House = new("ranch", "ranch_house", "Ranch house");
    public static readonly WorldDestination Office = new("ranch", "office", "Office");
    public static readonly WorldDestination Store = new("town", "general_store", "General Store");

    public static WorldDestination ForAlert(WorldAlert alert) => alert.Id switch
    {
        "dairy_unstaffed" => new("ranch", "dairy_barn", "Dairy Barn"),
        "pasture_unstaffed" or "cattle_health_low" or "cattle_health_critical" => new("ranch", "pasture", "Pasture"),
        "meals_empty" => new("ranch", "kitchen", "Kitchen"),
        "supplies_empty" or "supplies_low" => Office,
        "meal_box_missing" => Store,
        "bath_dirty" or "night_plan_missing" or "roster_exhausted" or "roster_hp_critical" or "roster_morale_low" => House,
        "pets_hungry" or "pets_getting_hungry" => new("ranch", "pet_care", "Pet care"),
        _ => Office
    };

    public static string DirectionArrow(Vector3 player, Vector3 target, Basis camera)
    {
        var delta = target - player;
        delta.Y = 0;
        if (!delta.IsFinite() || delta.LengthSquared() < 0.25f) return "•";
        var right = camera.X; right.Y = 0;
        var forward = -camera.Z; forward.Y = 0;
        right = right.Normalized(); forward = forward.Normalized();
        var angle = Mathf.Atan2(delta.Dot(right), delta.Dot(forward));
        var sector = ((int)MathF.Round(angle / (Mathf.Pi / 4f)) + 8) % 8;
        return new[] { "↑", "↗", "→", "↘", "↓", "↙", "←", "↖" }[sector];
    }
}
