/*
┌─────────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak/Unity-AI-Tilemap)       │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-AI-Tilemap)        │
└─────────────────────────────────────────────────────────────────────────────┘
*/

#nullable enable
using System;
using System.ComponentModel;
using System.IO;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using AIGD;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Tilemap
    {
        public const string CreateTileAssetToolId = "tilemap-create-tile-asset";

        [AiTool
        (
            CreateTileAssetToolId,
            Title = "Tilemap / Create Tile Asset",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Create a UnityEngine.Tilemaps.Tile asset at an Assets/-rooted path and assign a Sprite " +
            "(loaded from a sprite asset path) plus an optional color and collider type. Returns the created asset path.")]
        [AiSkillBody("Create a `Tile` ScriptableObject asset and wire its sprite so it can be painted into a Tilemap.\n\n" +
            "## Inputs\n\n" +
            "- `assetPath` — `Assets/`-rooted path ending in `.asset` for the new Tile.\n" +
            "- `spriteAssetPath` — optional `Assets/`-rooted path to a `Sprite` asset to assign.\n" +
            "- `color` — optional tile color (default white).\n" +
            "- `colliderType` — optional `None` / `Sprite` / `Grid` (default Sprite).\n\n" +
            "## Behavior\n\n" +
            "Creates the intermediate folders, instantiates a `Tile`, assigns sprite/color/collider type, writes the " +
            "asset via `AssetDatabase.CreateAsset`, saves + refreshes, and returns the asset path. Runs on the Unity " +
            "main thread.")]
        [Description("Creates a Tile asset, assigns a Sprite + color + collider type, and saves it at an Assets/ path.")]
        public CreateTileAssetResponse CreateTileAsset
        (
            [Description("Assets/-rooted path ending in '.asset' for the new Tile asset.")]
            string assetPath,
            [Description("Optional Assets/-rooted path to a Sprite asset to assign to the tile.")]
            string? spriteAssetPath = null,
            [Description("Optional tile color (default white).")]
            Color? color = null,
            [Description("Optional collider type (default Sprite).")]
            TileColliderTypeArg colliderType = TileColliderTypeArg.Sprite
        )
        {
            if (string.IsNullOrEmpty(assetPath))
                throw new ArgumentNullException(nameof(assetPath));

            return MainThread.Instance.Run(() =>
            {
                var normalized = assetPath.Replace('\\', '/');
                if (!normalized.StartsWith("Assets/", StringComparison.Ordinal) ||
                    !normalized.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                    throw new Exception(Error.InvalidAssetPath(assetPath));

                var dir = Path.GetDirectoryName(normalized)!.Replace('\\', '/');
                EnsureAssetFolder(dir);

                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.color = color ?? Color.white;
                tile.colliderType = colliderType switch
                {
                    TileColliderTypeArg.None => Tile.ColliderType.None,
                    TileColliderTypeArg.Sprite => Tile.ColliderType.Sprite,
                    TileColliderTypeArg.Grid => Tile.ColliderType.Grid,
                    _ => Tile.ColliderType.Sprite
                };

                string? resolvedSprite = null;
                if (!string.IsNullOrEmpty(spriteAssetPath))
                {
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteAssetPath);
                    if (sprite == null)
                        throw new Exception(Error.SpriteNotFound(spriteAssetPath!));
                    tile.sprite = sprite;
                    resolvedSprite = spriteAssetPath;
                }

                AssetDatabase.CreateAsset(tile, normalized);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return new CreateTileAssetResponse
                {
                    assetPath = normalized,
                    spriteAssetPath = resolvedSprite,
                    color = tile.color,
                    colliderType = tile.colliderType.ToString(),
                    success = true
                };
            });
        }

        /// <summary>Recursively create an Assets/-rooted folder chain if missing.</summary>
        static void EnsureAssetFolder(string folder)
        {
            folder = folder.Replace('\\', '/');
            if (string.IsNullOrEmpty(folder) || folder == "Assets" || AssetDatabase.IsValidFolder(folder))
                return;

            var parent = Path.GetDirectoryName(folder)!.Replace('\\', '/');
            EnsureAssetFolder(parent);
            var name = Path.GetFileName(folder);
            AssetDatabase.CreateFolder(parent, name);
        }

        public class CreateTileAssetResponse
        {
            [Description("Path of the created Tile asset.")]
            public string assetPath = string.Empty;

            [Description("Path of the assigned Sprite asset, or null.")]
            public string? spriteAssetPath;

            [Description("Tile color.")]
            public Color color;

            [Description("Tile collider type.")]
            public string colliderType = string.Empty;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
