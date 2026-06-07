using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;

namespace OpenMakaiRanch.Gameplay;

public static class PortraitLayerCatalog
{
    // Persisted layer indices depend on array order. Keep existing entries stable and append new ones.
    public static readonly PortraitLayerFrame[] RaceLayers =
    {
        new("res://assets/portrait_layers/race_00.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_01.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_02.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_03.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_04.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_05.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_06.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_07.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_08.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_09.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_10.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_11.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_20.png", 0, 0, 48, 48, 0, -16),
        new("res://assets/portrait_layers/race_21.png", 0, 0, 48, 48, 0, -16)
    };

    public static readonly PortraitLayerFrame[] HairLayers =
    {
        new("res://assets/portrait_layers/hair_00.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_01.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_02.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_03.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_04.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_05.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_06.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_07.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_08.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_09.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_10.png", 0, 32, 48, 112, 0, -2),
        new("res://assets/portrait_layers/hair_11.png", 0, 32, 48, 112, 0, -2)
    };

    // First six cloth entries preserve the historical order used by existing save data.
    public static readonly PortraitLayerFrame[] ClothLayers =
    {
        new("res://assets/portrait_layers/cloth/cloth_320_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_321.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_322_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_324_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_329_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_364_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/accessory_01.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/accessory_02.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/accessory_02_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/accessory_03.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_1462.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_1462_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_302.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_302_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_320.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_322.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_324.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_329.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_363.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_364.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_380.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_381.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_382.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_383.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_401.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_401_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_402.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_402_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_403.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_403_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_404.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_405.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_406.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_407.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_407_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_408.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_408_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_409.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_409_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_410.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_410_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_411.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_411_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_412.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_412_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_413.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_413_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_414.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_414_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_415.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_415_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_437.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_437_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_439.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_439_00.png", 0, 0, 64, 112, -8, 0),
        new("res://assets/portrait_layers/cloth/cloth_460.png", 0, 0, 64, 112, -8, 0)
    };

    public static readonly PortraitLayerFrame[] BodyLayers =
    {
        new("res://assets/portrait_layers/body.png", 0, 0, 48, 112, 0, 0),
        new("res://assets/portrait_layers/body2.png", 0, 0, 80, 112, -25, 0)
    };

    public static readonly PortraitLayerFrame FaceLayer = new("res://assets/portrait_layers/face.png", 0, 0, 8, 16, 14, 14);
    // Legacy mouth frames use the same draw offset; their opaque pixels sit lower inside the transparent 16x16 crop.
    public static readonly PortraitLayerFrame MouthLayer = new("res://assets/portrait_layers/face.png", 0, 384, 16, 16, 14, 14);
    public const string BackgroundLayer = "res://assets/portrait_layers/bg.png";

    public static IEnumerable<string> AllLayerPaths()
    {
        return RaceLayers.Concat(HairLayers)
            .Concat(ClothLayers)
            .Concat(BodyLayers)
            .Append(FaceLayer)
            .Append(MouthLayer)
            .Select(frame => frame.Path)
            .Append(BackgroundLayer)
            .Distinct(StringComparer.Ordinal);
    }

    public static int ClampIndex(int index, int size)
    {
        if (size <= 0)
        {
            return 0;
        }

        return ((index % size) + size) % size;
    }
}

public readonly record struct PortraitLayerFrame(string Path, int X, int Y, int Width, int Height, int OffsetX, int OffsetY);