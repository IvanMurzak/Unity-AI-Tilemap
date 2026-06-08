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
        public const string BoxFillToolId = "tilemap-box-fill";

        [AiTool
        (
            BoxFillToolId,
            Title = "Tilemap / Box Fill",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Fill a rectangular region of a Tilemap with a single TileBase asset using Tilemap.BoxFill. " +
            "The region is defined by an inclusive min and max cell coordinate.")]
        [AiSkillBody("Fill a rectangular block of cells of a `Tilemap` with one tile via `Tilemap.BoxFill`.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Tilemap` (required).\n" +
            "- `minX`, `minY` — inclusive minimum cell coordinate.\n" +
            "- `maxX`, `maxY` — inclusive maximum cell coordinate.\n" +
            "- `z` — cell Z coordinate (default 0).\n" +
            "- `tileAssetPath` — `Assets/`-rooted path to the `TileBase` asset to fill with (required).\n\n" +
            "## Behavior\n\n" +
            "Loads the tile, calls `Tilemap.BoxFill(start, tile, minX, minY, maxX, maxY)`, marks the scene dirty, and " +
            "repaints. Returns the filled cell count. Runs on the Unity main thread.")]
        [Description("Fills a rectangular region of a Tilemap with a TileBase asset via Tilemap.BoxFill.")]
        public BoxFillResponse BoxFill
        (
            [Description("Reference to the GameObject containing the Tilemap component.")]
            GameObjectRef gameObjectRef,
            [Description("Assets/-rooted path to the TileBase asset to fill the region with.")]
            string tileAssetPath,
            [Description("Inclusive minimum cell X coordinate.")]
            int minX,
            [Description("Inclusive minimum cell Y coordinate.")]
            int minY,
            [Description("Inclusive maximum cell X coordinate.")]
            int maxX,
            [Description("Inclusive maximum cell Y coordinate.")]
            int maxY,
            [Description("Cell Z coordinate (default 0).")]
            int z = 0
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var tilemap = ResolveTilemap(gameObjectRef, nameof(gameObjectRef));
                var tile = ResolveTileAsset(tileAssetPath);

                var loX = Mathf.Min(minX, maxX);
                var loY = Mathf.Min(minY, maxY);
                var hiX = Mathf.Max(minX, maxX);
                var hiY = Mathf.Max(minY, maxY);

                // Use Tilemap.BoxFill to seed the region, then deterministically set each cell
                // (inclusive of both corners) so the result is identical across Unity versions —
                // Tilemap.BoxFill's brush-pivot semantics vary and can leave edge cells unset.
                var start = new Vector3Int(loX, loY, z);
                tilemap.BoxFill(start, tile, loX, loY, hiX, hiY);
                for (int cx = loX; cx <= hiX; cx++)
                    for (int cy = loY; cy <= hiY; cy++)
                        tilemap.SetTile(new Vector3Int(cx, cy, z), tile);

                MarkDirtyAndRepaint(tilemap, tilemap.gameObject.scene);

                var filled = (hiX - loX + 1) * (hiY - loY + 1);

                return new BoxFillResponse
                {
                    gameObjectRef = new GameObjectRef(tilemap.gameObject),
                    tilemapRef = new ComponentRef(tilemap),
                    minX = loX,
                    minY = loY,
                    maxX = hiX,
                    maxY = hiY,
                    cellZ = z,
                    filledCount = filled,
                    tileAssetPath = tileAssetPath,
                    success = true
                };
            });
        }

        public class BoxFillResponse
        {
            [Description("Reference to the Tilemap GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Tilemap component.")]
            public ComponentRef? tilemapRef;

            [Description("Resolved minimum X.")]
            public int minX;

            [Description("Resolved minimum Y.")]
            public int minY;

            [Description("Resolved maximum X.")]
            public int maxX;

            [Description("Resolved maximum Y.")]
            public int maxY;

            [Description("Cell Z coordinate.")]
            public int cellZ;

            [Description("Number of cells filled.")]
            public int filledCount;

            [Description("Path of the tile asset used.")]
            public string tileAssetPath = string.Empty;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
