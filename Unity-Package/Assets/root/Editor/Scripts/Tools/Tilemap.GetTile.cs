/*
┌─────────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)                        │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-AI-Tilemap)        │
└─────────────────────────────────────────────────────────────────────────────┘
*/

#nullable enable
using System;
using System.ComponentModel;
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
        public const string GetTileToolId = "tilemap-get-tile";

        [AiTool
        (
            GetTileToolId,
            Title = "Tilemap / Get Tile",
            ReadOnlyHint = true,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Read the tile occupying a single cell of a Tilemap: returns whether the cell has a tile, " +
            "the tile asset name + path, the tile color, and the collider type at that cell. Read-only.")]
        [AiSkillBody("Inspect a single cell of a `Tilemap`.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Tilemap` (required).\n" +
            "- `x`, `y`, `z` — the integer cell coordinate (z defaults to 0).\n\n" +
            "## Behavior\n\n" +
            "Reads `Tilemap.GetTile`, `GetColor`, and `GetColliderType` at the cell and returns them. When the cell " +
            "is empty, `hasTile` is false. Read-only. Runs on the Unity main thread.")]
        [Description("Reads the tile at a single Tilemap cell (presence, asset name/path, color, collider type). Read-only.")]
        public GetTileResponse GetTile
        (
            [Description("Reference to the GameObject containing the Tilemap component.")]
            GameObjectRef gameObjectRef,
            [Description("Cell X coordinate.")]
            int x,
            [Description("Cell Y coordinate.")]
            int y,
            [Description("Cell Z coordinate (default 0).")]
            int z = 0
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var tilemap = ResolveTilemap(gameObjectRef, nameof(gameObjectRef));
                var cell = new Vector3Int(x, y, z);

                var tile = tilemap.GetTile(cell);
                var hasTile = tile != null;
                string? assetPath = hasTile ? AssetDatabase.GetAssetPath(tile) : null;

                return new GetTileResponse
                {
                    gameObjectRef = new GameObjectRef(tilemap.gameObject),
                    tilemapRef = new ComponentRef(tilemap),
                    cellX = x,
                    cellY = y,
                    cellZ = z,
                    hasTile = hasTile,
                    tileName = hasTile ? tile!.name : null,
                    tileAssetPath = string.IsNullOrEmpty(assetPath) ? null : assetPath,
                    color = tilemap.GetColor(cell),
                    colliderType = tilemap.GetColliderType(cell).ToString()
                };
            });
        }

        public class GetTileResponse
        {
            [Description("Reference to the Tilemap GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Tilemap component.")]
            public ComponentRef? tilemapRef;

            [Description("Cell X coordinate.")]
            public int cellX;

            [Description("Cell Y coordinate.")]
            public int cellY;

            [Description("Cell Z coordinate.")]
            public int cellZ;

            [Description("Whether a tile occupies the cell.")]
            public bool hasTile;

            [Description("Name of the tile asset, or null when empty.")]
            public string? tileName;

            [Description("Asset path of the tile, or null when empty/unsaved.")]
            public string? tileAssetPath;

            [Description("Per-cell tint color.")]
            public Color color;

            [Description("Per-cell collider type (None/Sprite/Grid).")]
            public string colliderType = string.Empty;
        }
    }
}
