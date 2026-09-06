using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Tests;

public static class SaveRegressionTests
{
    public static void Run(SmokeTestResult result)
    {
        TestFlagRoundTrip(result);
        TestNullSaveSections(result);
        TestRejectedLoadsPreserveLiveState(result);
    }

    private static void TestFlagRoundTrip(SmokeTestResult result)
    {
        var game = GameRoot.Instance;
        game.NewGame();
        try
        {
            var characterId = game.State.Roster.Characters.First().Id;
            var flags = game.Flags;
            flags.SetGlobalFlag(1, true);
            flags.SetGlobalIntFlag(2, -7);
            flags.SetTempFlag(3, true);
            flags.SetTempIntFlag(4, 23);
            flags.SetCharFlag(characterId, 5, true);
            flags.SetCharIntFlag(characterId, 6, 42);
            Check(result, game.SaveSlot(99), "root flag save succeeds");
            flags.SetGlobalFlag(1, false);
            flags.SetGlobalIntFlag(2, 0);
            flags.ClearTempFlags();
            flags.ClearCharFlags(characterId);
            Check(result, game.LoadSlot(99), "root flag load succeeds");
            Check(result, !ReferenceEquals(flags, game.Flags), "root flag service rebuilt on load");
            Check(result, game.Flags.GetGlobalFlag(1), "root global bool flag round-trips");
            Check(result, game.Flags.GetGlobalIntFlag(2) == -7, "root global int flag round-trips");
            Check(result, game.Flags.GetTempFlag(3), "root temporary bool flag round-trips");
            Check(result, game.Flags.GetTempIntFlag(4) == 23, "root temporary int flag round-trips");
            Check(result, game.Flags.GetCharFlag(characterId, 5), "root character bool flag round-trips");
            Check(result, game.Flags.GetCharIntFlag(characterId, 6) == 42, "root character int flag round-trips");
        }
        finally
        {
            game.Save.Delete(99);
            game.NewGame();
        }
    }

    private static void TestNullSaveSections(SmokeTestResult result)
    {
        var game = GameRoot.Instance;
        var path = ProjectSettings.GlobalizePath("user://saves/slot99.json");
        var sections = new[]
        {
            "\"Roster\":null",
            "\"Roster\":{\"Characters\":null}",
            "\"Reports\":null,\"Flags\":null",
            "\"Flags\":{\"GlobalBoolFlags\":null,\"GlobalIntFlags\":null,\"TempBoolFlags\":null,\"TempIntFlags\":null,\"CharBoolFlags\":null,\"CharIntFlags\":null}",
            "\"Flags\":{\"GlobalIntFlags\":{\"2\":19},\"CharBoolFlags\":{\"empty\":null,\"kept\":{\"5\":true}},\"CharIntFlags\":{\"empty\":null,\"kept\":{\"6\":42}}}"
        };
        try
        {
            foreach (var version in new[] { 13, SaveState.CurrentSchemaVersion })
            {
                for (var index = 0; index < sections.Length; index++)
                {
                    game.NewGame();
                    var label = $"root null fixture v{version}/{index}";
                    var json = $"{{\"SchemaVersion\":{version},\"Economy\":{{\"Gold\":777}},{sections[index]}}}";
                    File.WriteAllText(path, json);
                    try
                    {
                        var loaded = game.LoadSlot(99);
                        Check(result, loaded, $"{label} loads");
                        Check(result, File.ReadAllText(path) == json, $"{label} source file unchanged");
                        if (!loaded) continue;
                        Check(result, game.State.SchemaVersion == SaveState.CurrentSchemaVersion && game.State.Economy.Gold == 777,
                            $"{label} migrates without losing valid values");
                        Check(result, game.State.Roster.Characters is not null && game.State.Reports is not null,
                            $"{label} collections normalized");
                        if (index == 4)
                        {
                            Check(result, game.Flags.GetGlobalIntFlag(2) == 19 && game.Flags.GetCharFlag("kept", 5)
                                && game.Flags.GetCharIntFlag("kept", 6) == 42, $"{label} valid neighboring flags preserved");
                        }
                        game.Flags.SetGlobalFlag(1, true);
                        Check(result, game.SaveSlot(99) && game.LoadSlot(99) && game.Flags.GetGlobalFlag(1),
                            $"{label} remains saveable and loadable");
                    }
                    catch (Exception exception)
                    {
                        Check(result, false, $"{label} threw {exception.GetType().Name}");
                    }
                }
            }
        }
        finally
        {
            game.Save.Delete(99);
            game.NewGame();
        }
    }

    private static void TestRejectedLoadsPreserveLiveState(SmokeTestResult result)
    {
        var game = GameRoot.Instance;
        var path = ProjectSettings.GlobalizePath("user://saves/slot99.json");
        var fixtures = new[]
        {
            "{",
            "null",
            new JsonObject { ["SchemaVersion"] = SaveState.CurrentSchemaVersion + 1 }.ToJsonString(),
            "{\"Roster\":{\"Characters\":[null]}}",
            "{\"Flags\":{\"GlobalBoolFlags\":{\"1\":{}}}}"
        };
        game.NewGame();
        var liveState = game.State;
        var liveFlags = game.Flags;
        var liveSchedule = game.Schedule;
        var liveBond = game.Bond;
        game.Flags.SetGlobalIntFlag(2, 47);
        var changes = 0;
        void OnChanged() => changes++;
        game.StateChanged += OnChanged;
        try
        {
            foreach (var json in fixtures)
            {
                File.WriteAllText(path, json);
                Check(result, !game.LoadSlot(99), "root rejects invalid save");
                Check(result, ReferenceEquals(liveState, game.State) && ReferenceEquals(liveFlags, game.Flags)
                    && ReferenceEquals(liveSchedule, game.Schedule) && ReferenceEquals(liveBond, game.Bond),
                    "root rejected load preserves state and service references");
                Check(result, changes == 0 && game.Flags.GetGlobalIntFlag(2) == 47,
                    "root rejected load leaves live flags and notifications unchanged");
                Check(result, File.ReadAllText(path) == json, "root rejected load preserves source bytes");
            }
        }
        finally
        {
            game.StateChanged -= OnChanged;
            game.Save.Delete(99);
            game.NewGame();
        }
    }

    private static void Check(SmokeTestResult result, bool condition, string description)
    {
        result.Passed &= condition;
        result.Lines.Add($"SMOKE {(condition ? "OK" : "FAIL")} {description}");
    }
}
