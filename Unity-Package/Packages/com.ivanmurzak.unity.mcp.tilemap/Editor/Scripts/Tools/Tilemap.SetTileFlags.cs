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
        public const string SetTileFlagsToolId = "tilemap-set-tile-flags";

        [AiTool
        (
            SetTileFlagsToolId,
            Title = "Tilemap / Set Tile Color + Transform",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Set the per-cell tint color and/or transform (flip X/Y, Z rotation, scale) of a tile " +
            "already painted into a Tilemap. Unlocks the relevant TileFlags so the overrides take effect.")]
        [AiSkillBody("Override the per-cell color and transform of a single tile in a `Tilemap`.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Tilemap` (required).\n" +
            "- `x`, `y`, `z` — the cell coordinate (z defaults to 0).\n" +
            "- `color` — optional per-cell tint color; unlocks `TileFlags.LockColor` when applied.\n" +
            "- `flipX`, `flipY` — optional booleans to mirror the tile.\n" +
            "- `rotationZ` — optional Z rotation in degrees.\n" +
            "- `scale` — optional uniform/again per-axis scale (default identity).\n\n" +
            "## Behavior\n\n" +
            "When a color is given, removes `TileFlags.LockColor` then calls `SetColor`. When any transform argument " +
            "is given, removes `TileFlags.LockTransform` then builds a TRS matrix and calls `SetTransformMatrix`. " +
            "Marks the scene dirty and repaints. Runs on the Unity main thread.")]
        [Description("Sets per-cell color and/or transform (flip/rotate/scale) of a painted tile, unlocking the needed TileFlags.")]
        public SetTileFlagsResponse SetTileFlags
        (
            [Description("Reference to the GameObject containing the Tilemap component.")]
            GameObjectRef gameObjectRef,
            [Description("Cell X coordinate.")]
            int x,
            [Description("Cell Y coordinate.")]
            int y,
            [Description("Cell Z coordinate (default 0).")]
            int z = 0,
            [Description("Optional per-cell tint color.")]
            Color? color = null,
            [Description("Optional: mirror the tile horizontally.")]
            bool? flipX = null,
            [Description("Optional: mirror the tile vertically.")]
            bool? flipY = null,
            [Description("Optional: Z rotation in degrees.")]
            float? rotationZ = null,
            [Description("Optional per-axis scale (default (1,1,1)).")]
            Vector3? scale = null
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var tilemap = ResolveTilemap(gameObjectRef, nameof(gameObjectRef));
                var cell = new Vector3Int(x, y, z);

                bool colorApplied = false;
                bool transformApplied = false;

                if (color.HasValue)
                {
                    var flags = tilemap.GetTileFlags(cell);
                    tilemap.SetTileFlags(cell, flags & ~TileFlags.LockColor);
                    tilemap.SetColor(cell, color.Value);
                    colorApplied = true;
                }

                if (flipX.HasValue || flipY.HasValue || rotationZ.HasValue || scale.HasValue)
                {
                    var flags = tilemap.GetTileFlags(cell);
                    tilemap.SetTileFlags(cell, flags & ~TileFlags.LockTransform);

                    var s = scale ?? Vector3.one;
                    if (flipX == true) s.x = -Mathf.Abs(s.x);
                    if (flipY == true) s.y = -Mathf.Abs(s.y);
                    var rot = Quaternion.Euler(0, 0, rotationZ ?? 0f);
                    var matrix = Matrix4x4.TRS(Vector3.zero, rot, s);

                    tilemap.SetTransformMatrix(cell, matrix);
                    transformApplied = true;
                }

                MarkDirtyAndRepaint(tilemap, tilemap.gameObject.scene);

                return new SetTileFlagsResponse
                {
                    gameObjectRef = new GameObjectRef(tilemap.gameObject),
                    tilemapRef = new ComponentRef(tilemap),
                    cellX = x,
                    cellY = y,
                    cellZ = z,
                    colorApplied = colorApplied,
                    transformApplied = transformApplied,
                    success = true
                };
            });
        }

        public class SetTileFlagsResponse
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

            [Description("True when a color override was applied.")]
            public bool colorApplied;

            [Description("True when a transform override was applied.")]
            public bool transformApplied;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
