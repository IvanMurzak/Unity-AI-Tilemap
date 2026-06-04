/*
┌─────────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak/Unity-AI-Tilemap)       │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-AI-Tilemap)        │
└─────────────────────────────────────────────────────────────────────────────┘
*/

#nullable enable
using System;
using System.Collections.Generic;
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
        public const string ListToolId = "tilemap-list";

        [AiTool
        (
            ListToolId,
            Title = "Tilemap / List Tilemaps",
            ReadOnlyHint = true,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("List every Tilemap in the active scene with its name, painted-tile count, cell bounds, " +
            "and orientation. Read-only.")]
        [AiSkillBody("Enumerate all `Tilemap` components in the active scene.\n\n" +
            "## Inputs\n\n" +
            "- `includeInactive` (bool, default true) — include Tilemaps on inactive GameObjects.\n\n" +
            "## Behavior\n\n" +
            "Finds all `Tilemap` instances, reads each one's `GetUsedTilesCount`, `cellBounds`, and `orientation`, and " +
            "returns them. Read-only. Runs on the Unity main thread.")]
        [Description("Lists all Tilemaps in the active scene with name, tile count, bounds and orientation. Read-only.")]
        public ListResponse List
        (
            [Description("If true (default), include Tilemaps on inactive GameObjects.")]
            bool includeInactive = true
        )
        {
            return MainThread.Instance.Run(() =>
            {
#if UNITY_2023_1_OR_NEWER
                var tilemaps = UnityEngine.Object.FindObjectsByType<Tilemap>(
                    includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
                var tilemaps = UnityEngine.Object.FindObjectsOfType<Tilemap>(includeInactive);
#endif
                var items = new List<ListItem>(tilemaps.Length);
                foreach (var tilemap in tilemaps)
                {
                    if (tilemap == null)
                        continue;
                    var b = tilemap.cellBounds;
                    items.Add(new ListItem
                    {
                        gameObjectRef = new GameObjectRef(tilemap.gameObject),
                        tilemapRef = new ComponentRef(tilemap),
                        name = tilemap.name,
                        tileCount = tilemap.GetUsedTilesCount(),
                        boundsMin = new Vector3Int(b.xMin, b.yMin, b.zMin),
                        boundsMax = new Vector3Int(b.xMax, b.yMax, b.zMax),
                        orientation = tilemap.orientation.ToString()
                    });
                }

                return new ListResponse
                {
                    count = items.Count,
                    tilemaps = items.ToArray()
                };
            });
        }

        public class ListResponse
        {
            [Description("Number of Tilemaps found.")]
            public int count;

            [Description("The Tilemaps in the active scene.")]
            public ListItem[] tilemaps = Array.Empty<ListItem>();
        }

        public class ListItem
        {
            [Description("Reference to the Tilemap GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Tilemap component.")]
            public ComponentRef? tilemapRef;

            [Description("Name of the Tilemap GameObject.")]
            public string name = string.Empty;

            [Description("Number of painted (used) tiles.")]
            public int tileCount;

            [Description("Inclusive minimum cell bound.")]
            public Vector3Int boundsMin;

            [Description("Exclusive maximum cell bound.")]
            public Vector3Int boundsMax;

            [Description("Layout orientation.")]
            public string orientation = string.Empty;
        }
    }
}
