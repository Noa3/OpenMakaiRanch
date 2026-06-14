using System.Collections.Generic;
using System.Linq;
using Godot;

namespace OpenMakaiRanch.Gameplay;

public static class PortraitLayerCatalog
{
	public const int SkinColorCount = 6;
	public const int BodyTypeCount = 2;
	public const int BreastSizeCount = 7;

	private static readonly string[] SkinColorNames =
	{
		"Standard", "Pale", "Porcelain",
		"Albino", "Tan", "Wheat"
	};

	public static int BodyBaseIndex(int bodyType, int skinColor)
	{
		return bodyType * SkinColorCount + skinColor;
	}

	public static int BreastIndex(int bodyType, int breastSize, int skinColor)
	{
		return (bodyType * BreastSizeCount + breastSize) * SkinColorCount + skinColor;
	}

	public static int MapSkinColorToIndex(string skinColor)
	{
		if (string.IsNullOrWhiteSpace(skinColor))
			return 0;
		var lower = skinColor.Trim().ToLowerInvariant();
		return lower switch
		{
			"standard" or "fair" or "peach" or "light" => 0,
			"pale" or "white" or "porcelain" => 1,
			"albino" => 3,
			"tan" or "olive" or "brown" or "golden" or "caramel" => 4,
			"wheat" or "dark" or "ebony" or "chocolate" => 5,
			_ => 0
		};
	}

	public static int MapBustSizeToBreastIndex(int bustSize)
	{
		return bustSize switch
		{
			<= 1 => 0,
			<= 3 => 1,
			<= 5 => 2,
			<= 7 => 3,
			<= 9 => 4,
			<= 11 => 5,
			_ => 6
		};
	}

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

	private static readonly string[] BodyBaseSkinNames =
	{
		"fair", "light", "pale", "deepbrown", "rosylight", "tan"
	};

	public static readonly PortraitLayerFrame[] BodyBaseLayers = GenerateBodyBaseLayers();
	public static readonly PortraitLayerFrame[] BreastLayers = GenerateBreastLayers();

	public static readonly PortraitLayerFrame FaceLayer = new("res://assets/portrait_layers/face.png", 0, 0, 8, 16, 0, 14, 6f);
	public static readonly PortraitLayerFrame MouthLayer = new("res://assets/portrait_layers/face.png", 0, 384, 16, 16, 0, 54, 5f);
	public const string BackgroundLayer = "res://assets/portrait_layers/bg.png";

	private static PortraitLayerFrame[] GenerateBodyBaseLayers()
	{
		var layers = new PortraitLayerFrame[BodyTypeCount * SkinColorCount];
		for (int bodyType = 0; bodyType < BodyTypeCount; bodyType++)
		{
			var prefix = bodyType == 0 ? "big" : "small";
			for (int skin = 0; skin < SkinColorCount; skin++)
			{
				var path = $"res://assets/portrait_layers/body_{prefix}_{BodyBaseSkinNames[skin]}.png";
				layers[BodyBaseIndex(bodyType, skin)] = new PortraitLayerFrame(path, 0, 0, 48, 128, 0, 0);
			}
		}
		return layers;
	}

	private static PortraitLayerFrame[] GenerateBreastLayers()
	{
		var layers = new PortraitLayerFrame[BodyTypeCount * BreastSizeCount * SkinColorCount];
		for (int bodyType = 0; bodyType < BodyTypeCount; bodyType++)
		{
			string prefix = bodyType == 0 ? "big" : "small";
			for (int size = 0; size < BreastSizeCount; size++)
			{
				int num = size + 1;
				int w = num == 7 ? 80 : 48;
				int h = num == 7 ? 112 : 32;
				int offsetX = num == 7 ? -25 : (size == 5 ? (bodyType == 0 ? -4 : -1) : 0);
				int offsetY = num == 7 ? 0 : (bodyType == 0 ? 27 : 36);

				for (int skin = 0; skin < SkinColorCount; skin++)
				{
					var path = $"res://assets/portrait_layers/breast_{prefix}_{BodyBaseSkinNames[skin]}_{num}.png";
					layers[BreastIndex(bodyType, size, skin)] = new PortraitLayerFrame(path, 0, 0, w, h, offsetX, offsetY);
				}
			}
		}
		return layers;
	}

	public static IEnumerable<string> AllLayerPaths()
	{
		return RaceLayers.Concat(HairLayers)
			.Concat(ClothLayers)
			.Concat(BodyBaseLayers)
			.Concat(BreastLayers)
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

public readonly record struct PortraitLayerFrame(string Path, int X, int Y, int Width, int Height, int OffsetX, int OffsetY, float Scale = 1f);
