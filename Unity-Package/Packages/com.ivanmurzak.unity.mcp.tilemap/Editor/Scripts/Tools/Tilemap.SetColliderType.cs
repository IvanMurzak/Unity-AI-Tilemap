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
        public const string SetColliderTypeToolId = "tilemap-set-collider-type";

        [AiTool
        (
            SetColliderTypeToolId,
            Title = "Tilemap / Set Tile Collider Type",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Set the per-cell Tile.ColliderType (None / Sprite / Grid) of a tile painted into a " +
            "Tilemap. Pair with a TilemapCollider2D to generate physics colliders from the tiles.")]
        [AiSkillBody("Override the collider type of a single tile cell in a `Tilemap`.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Tilemap` (required).\n" +
            "- `x`, `y`, `z` — the cell coordinate (z defaults to 0).\n" +
            "- `colliderType` — `None`, `Sprite`, or `Grid`.\n\n" +
            "## Behavior\n\n" +
            "Maps the argument to `UnityEngine.Tilemaps.Tile.ColliderType` and calls `Tilemap.SetColliderType`, marks " +
            "the scene dirty, and repaints. Runs on the Unity main thread.")]
        [Description("Sets the per-cell collider type (None/Sprite/Grid) of a tile in a Tilemap.")]
        public SetColliderTypeResponse SetColliderType
        (
            [Description("Reference to the GameObject containing the Tilemap component.")]
            GameObjectRef gameObjectRef,
            [Description("Cell X coordinate.")]
            int x,
            [Description("Cell Y coordinate.")]
            int y,
            [Description("The collider type to set: None, Sprite, or Grid.")]
            TileColliderTypeArg colliderType,
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

                var native = colliderType switch
                {
                    TileColliderTypeArg.None => Tile.ColliderType.None,
                    TileColliderTypeArg.Sprite => Tile.ColliderType.Sprite,
                    TileColliderTypeArg.Grid => Tile.ColliderType.Grid,
                    _ => Tile.ColliderType.None
                };

                tilemap.SetColliderType(cell, native);
                MarkDirtyAndRepaint(tilemap, tilemap.gameObject.scene);

                return new SetColliderTypeResponse
                {
                    gameObjectRef = new GameObjectRef(tilemap.gameObject),
                    tilemapRef = new ComponentRef(tilemap),
                    cellX = x,
                    cellY = y,
                    cellZ = z,
                    colliderType = native.ToString(),
                    success = true
                };
            });
        }

        public class SetColliderTypeResponse
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

            [Description("Resulting collider type.")]
            public string colliderType = string.Empty;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
