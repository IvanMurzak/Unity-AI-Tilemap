/*
┌─────────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)                        │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-AI-Tilemap)        │
└─────────────────────────────────────────────────────────────────────────────┘
*/

#nullable enable
using System.ComponentModel;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Tilemap
    {
        /// <summary>The orientation plane a Tilemap lays tiles out on (mirrors UnityEngine.Tilemaps.Tilemap.Orientation).</summary>
        public enum TilemapOrientation
        {
            [Description("XY plane (default 2D layout).")]
            XY,
            [Description("XZ plane.")]
            XZ,
            [Description("YX plane.")]
            YX,
            [Description("YZ plane.")]
            YZ,
            [Description("ZX plane.")]
            ZX,
            [Description("ZY plane.")]
            ZY,
            [Description("Custom orientation matrix.")]
            Custom
        }

        /// <summary>The collider mode generated for a tile (mirrors UnityEngine.Tilemaps.Tile.ColliderType).</summary>
        public enum TileColliderTypeArg
        {
            [Description("No collider is generated for the tile.")]
            None,
            [Description("A collider that matches the sprite outline of the tile.")]
            Sprite,
            [Description("A collider that matches the grid cell of the tile.")]
            Grid
        }
    }
}
