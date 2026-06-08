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
        public const string ClearToolId = "tilemap-clear";

        [AiTool
        (
            ClearToolId,
            Title = "Tilemap / Clear Tiles",
            ReadOnlyHint = false,
            DestructiveHint = true,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Clear tiles from a Tilemap: either every tile (ClearAllTiles) or a rectangular region " +
            "(by erasing each cell in the inclusive min..max range). Destructive.")]
        [AiSkillBody("Erase tiles from a `Tilemap`. By default clears the whole map; pass a region to clear only a " +
            "rectangular block.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Tilemap` (required).\n" +
            "- `clearAll` (bool, default true) — when true, calls `Tilemap.ClearAllTiles()`; when false, erases the " +
            "region defined by `minX/minY/maxX/maxY`.\n" +
            "- `minX`, `minY`, `maxX`, `maxY`, `z` — the inclusive region (used only when `clearAll` is false).\n\n" +
            "## Behavior\n\n" +
            "Either clears all tiles or sets each cell in the region to null, marks the scene dirty, and repaints. " +
            "Runs on the Unity main thread.")]
        [Description("Clears all tiles or a rectangular region from a Tilemap. Destructive.")]
        public ClearResponse Clear
        (
            [Description("Reference to the GameObject containing the Tilemap component.")]
            GameObjectRef gameObjectRef,
            [Description("If true (default) clear the entire Tilemap; if false, clear the min..max region only.")]
            bool clearAll = true,
            [Description("Inclusive minimum cell X (region mode).")]
            int minX = 0,
            [Description("Inclusive minimum cell Y (region mode).")]
            int minY = 0,
            [Description("Inclusive maximum cell X (region mode).")]
            int maxX = 0,
            [Description("Inclusive maximum cell Y (region mode).")]
            int maxY = 0,
            [Description("Cell Z coordinate (region mode, default 0).")]
            int z = 0
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var tilemap = ResolveTilemap(gameObjectRef, nameof(gameObjectRef));

                int cleared;
                if (clearAll)
                {
                    tilemap.ClearAllTiles();
                    cleared = -1;
                }
                else
                {
                    var loX = Mathf.Min(minX, maxX);
                    var loY = Mathf.Min(minY, maxY);
                    var hiX = Mathf.Max(minX, maxX);
                    var hiY = Mathf.Max(minY, maxY);
                    cleared = 0;
                    for (int cx = loX; cx <= hiX; cx++)
                        for (int cy = loY; cy <= hiY; cy++)
                        {
                            tilemap.SetTile(new Vector3Int(cx, cy, z), null);
                            cleared++;
                        }
                }

                MarkDirtyAndRepaint(tilemap, tilemap.gameObject.scene);

                return new ClearResponse
                {
                    gameObjectRef = new GameObjectRef(tilemap.gameObject),
                    tilemapRef = new ComponentRef(tilemap),
                    clearedAll = clearAll,
                    clearedCount = cleared,
                    success = true
                };
            });
        }

        public class ClearResponse
        {
            [Description("Reference to the Tilemap GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Tilemap component.")]
            public ComponentRef? tilemapRef;

            [Description("True when the whole map was cleared.")]
            public bool clearedAll;

            [Description("Number of cells cleared in region mode; -1 when clearedAll.")]
            public int clearedCount;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
