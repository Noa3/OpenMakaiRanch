using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Tools;

/// <summary>
/// Development-time tool that exports the hardcoded DataRegistry seed data
/// to JSON files under res://data/. Run via --dump-data command-line argument.
///
/// After export, DataRegistry can load the JSON files instead of running
/// the seed methods, allowing data to be edited without recompiling C#.
/// </summary>
public static class DataExporter
{
    public static bool ShouldRun()
    {
        return OS.GetCmdlineArgs().Contains("--dump-data") || OS.GetCmdlineUserArgs().Contains("--dump-data");
    }

    public static void Run()
    {
        var registry = DataRegistry.CreateSeeded();
        var dataDir = ProjectSettings.GlobalizePath("res://data");
        if (!Directory.Exists(dataDir))
            Directory.CreateDirectory(dataDir);

        var opts = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new ResourceJsonConverter() }
        };

        WriteJson($"{dataDir}/characters.json", registry.Characters.Values.OrderBy(d => d.Id), opts);
        WriteJson($"{dataDir}/jobs.json", registry.Jobs.Values.OrderBy(d => d.Id), opts);
        WriteJson($"{dataDir}/items.json", registry.Items.Values.OrderBy(d => d.Id), opts);
        WriteJson($"{dataDir}/facilities.json", registry.Facilities.Values.OrderBy(d => d.Id), opts);
        WriteJson($"{dataDir}/missions.json", registry.Missions.Values.OrderBy(d => d.Id), opts);
        WriteJson($"{dataDir}/enemies.json", registry.Enemies.Values.OrderBy(d => d.Id), opts);
        WriteJson($"{dataDir}/milestones.json", registry.Milestones.Values.OrderBy(d => d.Id), opts);
        WriteJson($"{dataDir}/skills.json", registry.Skills.Values.OrderBy(d => d.Id), opts);
        WriteJson($"{dataDir}/pets.json", registry.Pets.Values.OrderBy(d => d.Id), opts);
        WriteJson($"{dataDir}/bond_events.json", registry.BondEvents.Values.OrderBy(d => d.Id), opts);
        WriteJson($"{dataDir}/talents.json", registry.Talents.Values.OrderBy(d => d.Id), opts);

        GD.Print($"DataExporter: wrote 11 JSON files to res://data/");
    }

    private static void WriteJson<T>(string path, IEnumerable<T> items, JsonSerializerOptions opts)
    {
        var list = items.ToList();
        var json = JsonSerializer.Serialize(list, opts);
        File.WriteAllText(path, json);
        GD.Print($"  Wrote {path} ({list.Count} entries)");
    }
}

/// <summary>
/// JSON converter that serializes Godot Resource-derived classes by reading
/// only the C#-declared properties and ignoring inherited Godot properties
/// (NativeInstance etc.) that System.Text.Json cannot serialize.
/// </summary>
public class ResourceJsonConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Resource).IsAssignableFrom(typeToConvert);
    }

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        var instance = Activator.CreateInstance(typeToConvert)!;
        var properties = typeToConvert.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return instance;

            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;

            var propName = reader.GetString();
            reader.Read();

            var prop = Array.Find(properties, p => p.Name == propName);
            if (prop is not null && prop.CanWrite)
            {
                var value = JsonSerializer.Deserialize(ref reader, prop.PropertyType, options);
                prop.SetValue(instance, value);
            }
            else
            {
                reader.Skip();
            }
        }

        return instance;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        var type = value.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (var prop in properties)
        {
            if (!prop.CanRead)
                continue;

            if (prop.Name == "NativeInstance" || prop.Name == "ResourceName")
                continue;

            var propValue = prop.GetValue(value);
            writer.WritePropertyName(prop.Name);
            JsonSerializer.Serialize(writer, propValue, prop.PropertyType, options);
        }

        // Write the Id as resource_name so the importing code can key by it
        var idProp = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        if (idProp is not null && idProp.CanRead)
        {
            var idValue = idProp.GetValue(value) as string;
            if (!string.IsNullOrWhiteSpace(idValue))
            {
                writer.WriteString("_id", idValue);
            }
        }

        writer.WriteEndObject();
    }
}
