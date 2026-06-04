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
using System.Linq;
using com.IvanMurzak.McpPlugin;
using Microsoft.Extensions.Logging;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Utils;
using AIGD;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Tilemap
    {
        public const string ModifyToolId = "tilemap-modify";

        [AiTool
        (
            ModifyToolId,
            Title = "Tilemap / Modify Component",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Generic write: apply a `SerializedMember` diff to a Tilemap-related `Component` on a " +
            "GameObject via ReflectorNet `TryModify`. Fields go through the `fields` channel, properties through " +
            "`props`. Use 'tilemap-get' first to inspect the structure.")]
        [AiSkillBody("Modify a Tilemap-related component on a GameObject by applying a `SerializedMember` diff via " +
            "ReflectorNet. This is the generic escape hatch for members not covered by the dedicated tools.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the component (required).\n" +
            "- `data` — the `SerializedMember` diff to apply. Route C# **fields** through the `fields` channel and " +
            "**properties** through `props` — ReflectorNet resolves each channel strictly with no cross-fallback.\n" +
            "- `componentRef` — optional. Resolves a specific component when the GameObject has more than one.\n\n" +
            "## Behavior\n\n" +
            "Finds the target component, applies the diff via `Reflector.TryModify`, and on success marks the " +
            "component + scene dirty and repaints. The applied logs are returned. Runs on the Unity main thread.")]
        [Description("Generic: apply a SerializedMember diff to a Tilemap-related Component via ReflectorNet TryModify.")]
        public TilemapModifyResponse ModifyComponent
        (
            [Description("Reference to the GameObject containing the Tilemap-related component.")]
            GameObjectRef gameObjectRef,
            [Description("The SerializedMember diff to apply. Only include members you want to change.")]
            SerializedMember data,
            [Description("Optional reference to a specific component if the GameObject has multiple.")]
            ComponentRef? componentRef = null
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));
            if (!gameObjectRef.IsValid(out var validationError))
                throw new ArgumentException(validationError, nameof(gameObjectRef));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return MainThread.Instance.Run(() =>
            {
                var go = ResolveGameObject(gameObjectRef, nameof(gameObjectRef));
                var (component, index) = FindTilemapComponent(go, componentRef);
                if (component == null)
                    throw new Exception("[Error] No Tilemap-related component found on the specified GameObject.");

                var reflector = UnityMcpPluginEditor.Instance.Reflector ?? throw new Exception(Error.ReflectorNotAvailable());
                var logger = UnityLoggerFactory.LoggerFactory.CreateLogger<Tool_Tilemap>();

                var response = new TilemapModifyResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    componentRef = new ComponentRef(component),
                    componentIndex = index,
                    componentType = component.GetType().FullName ?? component.GetType().Name
                };

                var logs = new List<string>();
                var modifyLogs = new Logs();
                object? boxed = component;
                if (reflector.TryModify(ref boxed, data, logs: modifyLogs, logger: logger))
                {
                    response.success = true;
                    logs.Add("Component modified successfully.");
                    UnityEditor.EditorUtility.SetDirty(component);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
                    com.IvanMurzak.Unity.MCP.Editor.Utils.EditorUtils.RepaintAllEditorWindows();
                }
                else
                {
                    logs.Add("No modifications were made.");
                }
                logs.AddRange(modifyLogs.Select(l => l.ToString()));

                response.logs = logs.ToArray();
                return response;
            });
        }

        public class TilemapModifyResponse
        {
            [Description("Whether the modification was successful.")]
            public bool success;

            [Description("Reference to the GameObject containing the component.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the modified component.")]
            public ComponentRef? componentRef;

            [Description("Index of the component in the GameObject's component list.")]
            public int componentIndex = -1;

            [Description("Full type name of the modified component.")]
            public string componentType = string.Empty;

            [Description("Log of modifications and any warnings/errors.")]
            public string[]? logs;
        }
    }
}
