using System.Collections.Generic;
using Godot;

namespace OpenMakaiRanch.App;

/// <summary>
/// Canonical top-level scene routes for the playable game shell.
/// Keeping these paths in one place prevents Bootstrap, menus and tests from silently
/// drifting onto different compositions after branch merges.
/// </summary>
public static class GameRouteCatalog
{
    public const string Bootstrap = "res://scenes/Bootstrap.tscn";
    public const string MainMenu = "res://scenes/MainMenu.tscn";
    public const string WorldGame = "res://scenes/WorldGame.tscn";
    public const string Management = "res://scenes/Game.tscn";
    public const string Ranch = "res://scenes/dev/RanchGreybox.tscn";
    public const string Town = "res://scenes/dev/TownGreybox.tscn";

    public static IReadOnlyList<string> CriticalScenePaths { get; } = new[]
    {
        MainMenu,
        WorldGame,
        Management,
        Ranch,
        Town,
    };

    public static IReadOnlyList<string> MissingCriticalScenes()
    {
        var missing = new List<string>();
        foreach (var path in CriticalScenePaths)
        {
            if (!ResourceLoader.Exists(path))
            {
                missing.Add(path);
            }
        }

        return missing;
    }
}
