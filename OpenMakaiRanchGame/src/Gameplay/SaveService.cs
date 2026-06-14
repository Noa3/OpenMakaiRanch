using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Gameplay;

public sealed class SaveService
{
    private const long MaxSaveBytes = 4 * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        MaxDepth = 32,
        Converters = { new JsonStringEnumConverter() }
    };

    public bool Save(SaveState state, int slot)
    {
        var path = SavePath(slot);
        EnsureSaveDirectory();
        state.SavedAt = DateTime.UtcNow;
        var json = JsonSerializer.Serialize(state, JsonOptions);
        if (Encoding.UTF8.GetByteCount(json) > MaxSaveBytes)
        {
            GD.PushError($"Save slot {slot} exceeds the maximum supported size.");
            return false;
        }

        var absolutePath = ProjectSettings.GlobalizePath(path);
        var temporaryPath = absolutePath + ".tmp";
        try
        {
            File.WriteAllText(temporaryPath, json, Encoding.UTF8);
            File.Move(temporaryPath, absolutePath, true);
            return true;
        }
        catch (Exception exception)
        {
            GD.PushError($"Could not write save slot {slot}: {exception.Message}");
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            return false;
        }
    }

    public bool HasSave(int slot) => Godot.FileAccess.FileExists(SavePath(slot));

    public SaveSlotMetadata? LoadMetadata(int slot)
    {
        var path = SavePath(slot);
        if (!Godot.FileAccess.FileExists(path))
        {
            return null;
        }

        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
        if (file is null)
        {
            GD.PushError($"Could not open save slot {slot} for metadata.");
            return null;
        }

        if ((long)file.GetLength() > MaxSaveBytes)
        {
            GD.PushError($"Save slot {slot} exceeds the maximum supported size.");
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(file.GetAsText());
            var root = document.RootElement;
            var day = TryGetNestedInt(root, "Calendar", "Day") ?? 1;
            var gold = TryGetNestedInt(root, "Economy", "Gold") ?? 0;
            var characters = TryGetNestedArrayLength(root, "Roster", "Characters") ?? 0;
            var savedAt = TryGetDateTime(root, "SavedAt");
            var victoryDay = TryGetInt(root, "VictoryDay");
            return new SaveSlotMetadata(day, gold, characters, savedAt, victoryDay);
        }
        catch (Exception exception)
        {
            GD.PushError($"Save slot {slot} metadata could not be parsed: {exception.Message}");
            return null;
        }
    }

    public SaveState? Load(int slot)
    {
        var path = SavePath(slot);
        if (!Godot.FileAccess.FileExists(path))
        {
            return null;
        }

        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
        if (file is null)
        {
            GD.PushError($"Could not open save slot {slot} for reading.");
            return null;
        }

        if ((long)file.GetLength() > MaxSaveBytes)
        {
            GD.PushError($"Save slot {slot} exceeds the maximum supported size.");
            return null;
        }

        try
        {
            var state = JsonSerializer.Deserialize<SaveState>(file.GetAsText(), JsonOptions);
            if (state is null)
            {
                GD.PushError($"Save slot {slot} was empty or invalid.");
                return null;
            }

            state = SaveMigrator.Migrate(state);
            if (state.SchemaVersion != SaveState.CurrentSchemaVersion)
            {
                GD.PushError($"Save slot {slot} schema {state.SchemaVersion} is not supported by schema {SaveState.CurrentSchemaVersion}.");
                return null;
            }

            return state;
        }
        catch (Exception exception)
        {
            GD.PushError($"Save slot {slot} could not be parsed: {exception.Message}");
            return null;
        }
    }

    public void Delete(int slot)
    {
        var path = SavePath(slot);
        if (!Godot.FileAccess.FileExists(path))
        {
            return;
        }

        DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(path));
    }

    private static string SavePath(int slot) => $"user://saves/slot{slot}.json";

    private static int? TryGetNestedInt(JsonElement root, string objectName, string propertyName)
    {
        if (!root.TryGetProperty(objectName, out var parent))
        {
            return null;
        }

        return TryGetInt(parent, propertyName);
    }

    private static int? TryGetNestedArrayLength(JsonElement root, string objectName, string propertyName)
    {
        if (!root.TryGetProperty(objectName, out var parent) || !parent.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        return value.GetArrayLength();
    }

    private static int? TryGetInt(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        return value.TryGetInt32(out var number) ? number : null;
    }

    private static DateTime? TryGetDateTime(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return value.TryGetDateTime(out var dateTime) ? dateTime : null;
    }

    private static void EnsureSaveDirectory()
    {
        var absolutePath = ProjectSettings.GlobalizePath("user://saves");
        Directory.CreateDirectory(absolutePath);
    }
}

public sealed record SaveSlotMetadata(int Day, int Gold, int CharacterCount, DateTime? SavedAt, int? VictoryDay);
