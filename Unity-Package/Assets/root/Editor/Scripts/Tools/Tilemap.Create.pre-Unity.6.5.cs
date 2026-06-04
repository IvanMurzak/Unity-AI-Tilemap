/*
┌─────────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)                        │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-AI-Tilemap)        │
└─────────────────────────────────────────────────────────────────────────────┘
*/

#nullable enable
#if !UNITY_6000_5_OR_NEWER
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
        public const string CreateToolId = "tilemap-create";

        [AiTool
        (
            CreateToolId,
            Title = "Tilemap / Create Grid + Tilemap",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Create a Grid GameObject with a child Tilemap + TilemapRenderer in the active scene. " +
            "Optionally parent the Grid under an existing GameObject and name the tilemap. Returns the new tilemap " +
            "GameObject reference and its instanceId.")]
        [AiSkillBody("Create the standard 2D tilemap hierarchy: a `Grid` GameObject hosting a child GameObject with a " +
            "`Tilemap` and `TilemapRenderer`. This is the minimal structure required before painting tiles.\n\n" +
            "## Inputs\n\n" +
            "- `gridName` — optional name for the Grid GameObject (default `Grid`).\n" +
            "- `tilemapName` — optional name for the Tilemap child GameObject (default `Tilemap`).\n" +
            "- `parentRef` — optional GameObject to parent the new Grid under.\n" +
            "- `cellSize` — optional Grid cell size (default `(1,1,0)`).\n\n" +
            "## Behavior\n\n" +
            "Creates the Grid + child Tilemap/TilemapRenderer, sets the cell size, parents under `parentRef` when " +
            "supplied, marks the scene dirty, repaints, and returns references + the tilemap GameObject instanceId. " +
            "Runs on the Unity main thread.")]
        [Description("Creates a Grid GameObject with a child Tilemap + TilemapRenderer in the active scene.")]
        public CreateResponse Create
        (
            [Description("Name of the Grid GameObject.")]
            string? gridName = null,
            [Description("Name of the child Tilemap GameObject.")]
            string? tilemapName = null,
            [Description("Optional GameObject to parent the new Grid under.")]
            GameObjectRef? parentRef = null,
            [Description("Grid cell size. Defaults to (1,1,0).")]
            Vector3? cellSize = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var gridGo = new GameObject(string.IsNullOrEmpty(gridName) ? "Grid" : gridName);
                var grid = gridGo.AddComponent<Grid>();
                grid.cellSize = cellSize ?? new Vector3(1, 1, 0);

                var tilemapGo = new GameObject(string.IsNullOrEmpty(tilemapName) ? "Tilemap" : tilemapName);
                tilemapGo.transform.SetParent(gridGo.transform, false);
                var tilemap = tilemapGo.AddComponent<Tilemap>();
                var renderer = tilemapGo.AddComponent<TilemapRenderer>();

                if (parentRef != null && parentRef.IsValid(out _))
                {
                    var parent = ResolveGameObject(parentRef, nameof(parentRef));
                    gridGo.transform.SetParent(parent.transform, false);
                }

                EditorUtility.SetDirty(tilemapGo);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(tilemapGo.scene);
                com.IvanMurzak.Unity.MCP.Editor.Utils.EditorUtils.RepaintAllEditorWindows();

                return new CreateResponse
                {
                    gridRef = new GameObjectRef(gridGo),
                    tilemapGameObjectRef = new GameObjectRef(tilemapGo),
                    tilemapRef = new ComponentRef(tilemap),
                    rendererRef = new ComponentRef(renderer),
                    instanceId = tilemapGo.GetInstanceID(),
                    gridName = gridGo.name,
                    tilemapName = tilemapGo.name,
                    success = true
                };
            });
        }

        public class CreateResponse
        {
            [Description("Reference to the created Grid GameObject.")]
            public GameObjectRef? gridRef;

            [Description("Reference to the created Tilemap GameObject.")]
            public GameObjectRef? tilemapGameObjectRef;

            [Description("Reference to the created Tilemap component.")]
            public ComponentRef? tilemapRef;

            [Description("Reference to the created TilemapRenderer component.")]
            public ComponentRef? rendererRef;

            [Description("Instance id of the created Tilemap GameObject.")]
            public int instanceId;

            [Description("Name of the created Grid GameObject.")]
            public string gridName = string.Empty;

            [Description("Name of the created Tilemap GameObject.")]
            public string tilemapName = string.Empty;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
#endif
