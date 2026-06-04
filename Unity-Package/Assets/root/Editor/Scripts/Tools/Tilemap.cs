/*
┌─────────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)                        │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-AI-Tilemap)        │
│  Copyright (c) 2025 Ivan Murzak                                             │
│  Licensed under the MIT License.                                            │
│  See the LICENSE file in the project root for more information.             │
└─────────────────────────────────────────────────────────────────────────────┘
*/

#nullable enable
using com.IvanMurzak.McpPlugin;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    [AiToolType]
    public partial class Tool_Tilemap
    {
        public static class Error
        {
            public static string GameObjectNotFound()
                => "[Error] GameObject not found. Provide a valid reference to an existing GameObject.";

            public static string TilemapNotFound()
                => "[Error] Tilemap component not found on the target GameObject. " +
                   "Make sure the GameObject has a Tilemap component attached (create one with 'tilemap-create').";

            public static string TilemapRendererNotFound()
                => "[Error] TilemapRenderer component not found on the target GameObject.";

            public static string TileAssetNotFound(string path)
                => $"[Error] Tile asset not found at '{path}'. Provide a valid 'Assets/'-rooted path to a TileBase asset.";

            public static string SpriteNotFound(string path)
                => $"[Error] Sprite not found at '{path}'. Provide a valid 'Assets/'-rooted path to a Sprite asset.";

            public static string InvalidAssetPath(string path)
                => $"[Error] Invalid asset path '{path}'. The path must start with 'Assets/' and end with the expected extension.";

            public static string TypeNotFound(string typeName)
                => $"[Error] Type '{typeName}' could not be resolved. Provide a full type name (e.g. 'UnityEngine.Tilemaps.Tilemap').";

            public static string ReflectorNotAvailable()
                => "[Error] ReflectorNet reflector is not available.";
        }
    }
}
