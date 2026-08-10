using System;
using System.Collections.Generic;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;

namespace OpenMakaiRanch.Gameplay;

/// <summary>
/// Renders layered character portraits from PortraitLayerCatalog.
/// Handles texture caching, layer compositing, and fallback states.
/// </summary>
public sealed class PortraitRenderer
{
    private const float PortraitDisplaySize = 112f;
    private const float PortraitBodyOriginX = 32f;

    // Z-order: lower = drawn first (behind), higher = drawn last (front)
    public enum LayerOrder
    {
        Background = 0,
        BodyBase = 10,
        Race = 20,
        Breast = 30,
        Face = 40,
        Mouth = 50,
        Hair = 60,
        Clothing = 70
    }

    // Cached textures keyed by resource path
    private readonly Dictionary<string, Texture2D> _textureCache = new(StringComparer.Ordinal);

    public PortraitRenderer()
    {
        // Preload all layer paths from catalog to warm cache
        foreach (var path in PortraitLayerCatalog.AllLayerPaths())
        {
            _textureCache[path] = ResourceLoader.Load<Texture2D>(path);
        }
    }

    /// <summary>
    /// Build a layered portrait Control for a character.
    /// Returns null if critical layers are missing or textures fail to load.
    /// </summary>
    public Control? BuildLayeredPortrait(CharacterState character, CharacterDefinition definition)
    {
        // Validate catalog data
        if (PortraitLayerCatalog.RaceLayers.Length == 0
            || PortraitLayerCatalog.HairLayers.Length == 0
            || PortraitLayerCatalog.BodyBaseLayers.Length == 0
            || PortraitLayerCatalog.BreastLayers.Length == 0
            || PortraitLayerCatalog.ClothLayers.Length == 0)
        {
            return null;
        }

        // Validate background
        var bgTexture = GetTexture(PortraitLayerCatalog.BackgroundLayer);
        if (bgTexture is null)
        {
            return null;
        }

        // Clamp indices to valid ranges
        var bodyShape = PortraitLayerCatalog.ClampIndex(character.BodyLayerIndex, PortraitLayerCatalog.BodyTypeCount);
        var skinColor = PortraitLayerCatalog.ClampIndex(character.SkinColorIndex, PortraitLayerCatalog.SkinColorCount);
        var breastSize = PortraitLayerCatalog.ClampIndex(character.BreastSizeIndex, PortraitLayerCatalog.BreastSizeCount);
        var raceIndex = PortraitLayerCatalog.ClampIndex(character.RaceLayerIndex, PortraitLayerCatalog.RaceLayers.Length);
        var hairIndex = PortraitLayerCatalog.ClampIndex(character.HairLayerIndex, PortraitLayerCatalog.HairLayers.Length);
        var clothIndex = PortraitLayerCatalog.ClampIndex(character.ClothLayerIndex, PortraitLayerCatalog.ClothLayers.Length);

        // Resolve all layers
        var bodyBase = ResolveLayer(PortraitLayerCatalog.BodyBaseLayers[PortraitLayerCatalog.BodyBaseIndex(bodyShape, skinColor)]);
        var breast = ResolveLayer(PortraitLayerCatalog.BreastLayers[PortraitLayerCatalog.BreastIndex(bodyShape, breastSize, skinColor)]);
        var race = ResolveLayer(PortraitLayerCatalog.RaceLayers[raceIndex]);
        var face = ResolveLayer(PortraitLayerCatalog.FaceLayer);
        var mouth = ResolveLayer(PortraitLayerCatalog.MouthLayer);
        var hair = ResolveLayer(PortraitLayerCatalog.HairLayers[hairIndex]);
        var cloth = ResolveLayer(PortraitLayerCatalog.ClothLayers[clothIndex]);

        if (bodyBase is null || breast is null || race is null || face is null || mouth is null || hair is null || cloth is null)
        {
            return null;
        }

        // Build composition stack
        var stack = new Control { CustomMinimumSize = new Vector2(PortraitDisplaySize, PortraitDisplaySize) };

        // Background (centered in portrait)
        stack.AddChild(CreateTextureRect(bgTexture, new Vector2(24, 0), new Vector2(64, 112)));

        // Z-ordered layers: body → race → breast → face → mouth → hair → clothing
        // Godot draws CanvasItem children in sibling order, so add order == draw order.
        AddTextureChild(stack, bodyBase);
        AddTextureChild(stack, race);
        AddTextureChild(stack, breast);
        AddTextureChild(stack, face);
        AddTextureChild(stack, mouth);
        AddTextureChild(stack, hair);
        AddTextureChild(stack, cloth);

        return stack;
    }

    /// <summary>
    /// Build a fallback portrait when layered rendering fails.
    /// Uses definition.PortraitPath or definition.BodyImagePath.
    /// </summary>
    public Control? BuildFallbackPortrait(CharacterDefinition definition)
    {
        var path = string.IsNullOrWhiteSpace(definition.PortraitPath)
            ? string.Empty
            : definition.PortraitPath;

        if (!string.IsNullOrEmpty(path))
        {
            var texture = GetTexture(path);
            if (texture is not null)
            {
                return CreateTextureRect(texture, new Vector2(PortraitDisplaySize, PortraitDisplaySize));
            }
        }

        // Ultimate fallback: dark placeholder
        return new ColorRect
        {
            Color = new Color("24364f"),
            CustomMinimumSize = new Vector2(PortraitDisplaySize, PortraitDisplaySize)
        };
    }

    /// <summary>
    /// Build a visual wrapper that tries layered rendering first, then falls back.
    /// </summary>
    public Control BuildCharacterVisual(CharacterState character, CharacterDefinition definition)
    {
        var wrap = new VBoxContainer { CustomMinimumSize = new Vector2(PortraitDisplaySize, PortraitDisplaySize) };
        wrap.AddThemeConstantOverride("separation", 6);

        var layered = BuildLayeredPortrait(character, definition);
        if (layered is not null)
        {
            wrap.AddChild(layered);
        }
        else
        {
            wrap.AddChild(BuildFallbackPortrait(definition));
        }

        return wrap;
    }

    /// <summary>
    /// Clear the texture cache. Call when assets change or memory pressure is high.
    /// </summary>
    public void ClearCache()
    {
        _textureCache.Clear();
    }

    /// <summary>
    /// Get a cached texture, loading from disk if not cached.
    /// </summary>
    private Texture2D? GetTexture(string path)
    {
        if (_textureCache.TryGetValue(path, out var cached))
        {
            return cached;
        }

        var texture = ResourceLoader.Load<Texture2D>(path);
        if (texture is not null)
        {
            _textureCache[path] = texture;
        }

        return texture;
    }

    /// <summary>
    /// Resolve a PortraitLayerFrame to a TextureRect, or null if texture fails.
    /// </summary>
    private TextureRect? ResolveLayer(PortraitLayerFrame frame)
    {
        var texture = GetTexture(frame.Path);
        if (texture is null)
        {
            return null;
        }

        var atlas = new AtlasTexture
        {
            Atlas = texture,
            Region = new Rect2(frame.X, frame.Y, frame.Width, frame.Height)
        };

        var scaledW = (int)(frame.Width * frame.Scale);
        var scaledH = (int)(frame.Height * frame.Scale);

        return CreateTextureRect(atlas, new Vector2(PortraitBodyOriginX + frame.OffsetX, frame.OffsetY), new Vector2(scaledW, scaledH));
    }

    /// <summary>
    /// Create a TextureRect with standard portrait settings.
    /// </summary>
    private static TextureRect CreateTextureRect(Texture2D texture, Vector2 position, Vector2 size)
    {
        return new TextureRect
        {
            Texture = texture,
            Position = position,
            Size = size,
            CustomMinimumSize = size,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
    }

    private static TextureRect? CreateTextureRect(Texture2D texture, Vector2 size)
    {
        return new TextureRect
        {
            Texture = texture,
            CustomMinimumSize = size,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
    }

    /// <summary>
    /// Append a texture layer to the stack. Godot draws siblings in node order,
    /// so later additions render on top of earlier ones.
    /// </summary>
    private static void AddTextureChild(Control stack, TextureRect? layer)
    {
        if (layer is not null)
        {
            stack.AddChild(layer);
        }
    }
}
