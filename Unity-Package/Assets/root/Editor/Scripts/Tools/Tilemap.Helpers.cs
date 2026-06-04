/*
┌─────────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)                        │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-AI-Tilemap)        │
│  Copyright (c) 2025 Ivan Murzak                                             │
│  Licensed under the MIT License.                                            │
└─────────────────────────────────────────────────────────────────────────────┘
*/

#nullable enable
using System;
using AIGD;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Tilemap
    {
        /// <summary>Resolve a required GameObjectRef to its GameObject (throws on failure).</summary>
        static GameObject ResolveGameObject(GameObjectRef? gameObjectRef, string paramName)
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(paramName);
            if (!gameObjectRef.IsValid(out var validationError))
                throw new ArgumentException(validationError, paramName);

            var go = gameObjectRef.FindGameObject(out var error);
            if (error != null)
                throw new Exception(error);
            if (go == null)
                throw new Exception(Error.GameObjectNotFound());

            return go;
        }

        /// <summary>Resolve a required GameObjectRef to a Tilemap component (throws on failure).</summary>
        static Tilemap ResolveTilemap(GameObjectRef? gameObjectRef, string paramName)
        {
            var go = ResolveGameObject(gameObjectRef, paramName);
            var tilemap = go.GetComponent<Tilemap>();
            if (tilemap == null)
                throw new Exception(Error.TilemapNotFound());
            return tilemap;
        }

        /// <summary>Load a TileBase asset from an Assets/-rooted path (throws on failure).</summary>
        static TileBase ResolveTileAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || !assetPath.Replace('\\', '/').StartsWith("Assets/", StringComparison.Ordinal))
                throw new Exception(Error.InvalidAssetPath(assetPath));

            var tile = AssetDatabase.LoadAssetAtPath<TileBase>(assetPath);
            if (tile == null)
                throw new Exception(Error.TileAssetNotFound(assetPath));
            return tile;
        }

        /// <summary>Mark a scene object dirty and repaint the editor after a mutation.</summary>
        static void MarkDirtyAndRepaint(UnityEngine.Object target, UnityEngine.SceneManagement.Scene scene)
        {
            EditorUtility.SetDirty(target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            com.IvanMurzak.Unity.MCP.Editor.Utils.EditorUtils.RepaintAllEditorWindows();
        }
    }
}
