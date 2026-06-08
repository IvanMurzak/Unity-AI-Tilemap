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
using UnityEngine;
using UnityEngine.Tilemaps;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Tilemap
    {
        public const string SetTileToolId = "tilemap-set-tile";

        [AiTool
        (
            SetTileToolId,
            Title = "Tilemap / Set Tile",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Paint a single tile (a TileBase asset such as a Tile or RuleTile) into a Tilemap at a " +
            "given cell coordinate. Pass a null/empty tileAssetPath to erase the cell.")]
        [AiSkillBody("Set the tile at a single cell of a `Tilemap`. The tile is loaded from an `Assets/`-rooted path " +
            "to any `TileBase` asset (a `Tile`, `RuleTile`, etc.).\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Tilemap` (required).\n" +
            "- `x`, `y`, `z` — the integer cell coordinate (z defaults to 0).\n" +
            "- `tileAssetPath` — `Assets/`-rooted path to the `TileBase` asset; null/empty erases the cell.\n\n" +
            "## Behavior\n\n" +
            "Loads the tile asset (when supplied), calls `Tilemap.SetTile(cell, tile)`, marks the scene dirty, and " +
            "repaints. Runs on the Unity main thread.")]
        [Description("Paints a single TileBase asset into a Tilemap at a cell coordinate. Null path erases the cell.")]
        public SetTileResponse SetTile
        (
            [Description("Reference to the GameObject containing the Tilemap component.")]
            GameObjectRef gameObjectRef,
            [Description("Cell X coordinate.")]
            int x,
            [Description("Cell Y coordinate.")]
            int y,
            [Description("Cell Z coordinate (default 0).")]
            int z = 0,
            [Description("Assets/-rooted path to the TileBase asset to paint. Null/empty erases the cell.")]
            string? tileAssetPath = null
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var tilemap = ResolveTilemap(gameObjectRef, nameof(gameObjectRef));
                var cell = new Vector3Int(x, y, z);

                TileBase? tile = null;
                if (!string.IsNullOrEmpty(tileAssetPath))
                    tile = ResolveTileAsset(tileAssetPath!);

                tilemap.SetTile(cell, tile);
                MarkDirtyAndRepaint(tilemap, tilemap.gameObject.scene);

                return new SetTileResponse
                {
                    gameObjectRef = new GameObjectRef(tilemap.gameObject),
                    tilemapRef = new ComponentRef(tilemap),
                    cellX = x,
                    cellY = y,
                    cellZ = z,
                    tileAssetPath = tileAssetPath,
                    erased = tile == null,
                    success = true
                };
            });
        }

        public class SetTileResponse
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

            [Description("Path of the painted tile asset, or null.")]
            public string? tileAssetPath;

            [Description("True when the cell was erased (no tile painted).")]
            public bool erased;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
