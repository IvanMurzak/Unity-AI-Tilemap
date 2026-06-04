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

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Tilemap
    {
        public const string CreateRuleTileToolId = "tilemap-create-rule-tile";

        [AiTool
        (
            CreateRuleTileToolId,
            Title = "Tilemap / Create Rule Tile",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Create a UnityEngine.RuleTile asset (from the 2D Tilemap Extras package) at an " +
            "Assets/-rooted path, with an optional default Sprite. RuleTiles auto-pick sprites based on neighbour " +
            "rules — paint them like any tile and add rules afterward via the Inspector or 'tilemap-modify'.")]
        [AiSkillBody("Create a `RuleTile` ScriptableObject asset (provided by `com.unity.2d.tilemap.extras`).\n\n" +
            "## Inputs\n\n" +
            "- `assetPath` — `Assets/`-rooted path ending in `.asset` for the new RuleTile.\n" +
            "- `defaultSpriteAssetPath` — optional `Assets/`-rooted path to a `Sprite` used as the RuleTile's default " +
            "sprite (shown when no rule matches).\n\n" +
            "## Behavior\n\n" +
            "Creates the intermediate folders, instantiates a `RuleTile`, assigns the default sprite when supplied, " +
            "writes the asset via `AssetDatabase.CreateAsset`, saves + refreshes, and returns the asset path. Neighbour " +
            "rules can then be authored via `tilemap-modify` or the Inspector. Runs on the Unity main thread.")]
        [Description("Creates a RuleTile asset (2D Tilemap Extras) with an optional default Sprite at an Assets/ path.")]
        public CreateRuleTileResponse CreateRuleTile
        (
            [Description("Assets/-rooted path ending in '.asset' for the new RuleTile asset.")]
            string assetPath,
            [Description("Optional Assets/-rooted path to a Sprite used as the RuleTile default sprite.")]
            string? defaultSpriteAssetPath = null
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

                var ruleTile = ScriptableObject.CreateInstance<RuleTile>();

                string? resolvedSprite = null;
                if (!string.IsNullOrEmpty(defaultSpriteAssetPath))
                {
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(defaultSpriteAssetPath);
                    if (sprite == null)
                        throw new Exception(Error.SpriteNotFound(defaultSpriteAssetPath!));
                    ruleTile.m_DefaultSprite = sprite;
                    resolvedSprite = defaultSpriteAssetPath;
                }

                AssetDatabase.CreateAsset(ruleTile, normalized);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return new CreateRuleTileResponse
                {
                    assetPath = normalized,
                    defaultSpriteAssetPath = resolvedSprite,
                    ruleCount = ruleTile.m_TilingRules?.Count ?? 0,
                    success = true
                };
            });
        }

        public class CreateRuleTileResponse
        {
            [Description("Path of the created RuleTile asset.")]
            public string assetPath = string.Empty;

            [Description("Path of the assigned default Sprite, or null.")]
            public string? defaultSpriteAssetPath;

            [Description("Number of tiling rules (0 for a fresh RuleTile).")]
            public int ruleCount;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
