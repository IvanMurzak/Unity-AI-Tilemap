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
        public const string SetOrientationToolId = "tilemap-set-orientation";

        [AiTool
        (
            SetOrientationToolId,
            Title = "Tilemap / Set Anchor + Orientation",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Set the tile anchor (sub-cell pivot, default (0.5,0.5,0)) and/or the layout orientation " +
            "(XY/XZ/YX/YZ/ZX/ZY/Custom) of a Tilemap.")]
        [AiSkillBody("Configure how a `Tilemap` positions and orients its tiles.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Tilemap` (required).\n" +
            "- `anchor` — optional sub-cell anchor (tile pivot within a cell), default `(0.5,0.5,0)`.\n" +
            "- `orientation` — optional layout plane: `XY` (default 2D), `XZ`, `YX`, `YZ`, `ZX`, `ZY`, `Custom`.\n\n" +
            "## Behavior\n\n" +
            "Sets `Tilemap.tileAnchor` and/or `Tilemap.orientation`, marks the scene dirty, and repaints. Runs on " +
            "the Unity main thread.")]
        [Description("Sets the tile anchor and/or layout orientation of a Tilemap.")]
        public SetOrientationResponse SetOrientation
        (
            [Description("Reference to the GameObject containing the Tilemap component.")]
            GameObjectRef gameObjectRef,
            [Description("Optional sub-cell tile anchor (default (0.5,0.5,0)).")]
            Vector3? anchor = null,
            [Description("Optional layout orientation plane.")]
            TilemapOrientation? orientation = null
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var tilemap = ResolveTilemap(gameObjectRef, nameof(gameObjectRef));

                bool anchorApplied = false;
                bool orientationApplied = false;

                if (anchor.HasValue)
                {
                    tilemap.tileAnchor = anchor.Value;
                    anchorApplied = true;
                }

                if (orientation.HasValue)
                {
                    tilemap.orientation = orientation.Value switch
                    {
                        TilemapOrientation.XY => Tilemap.Orientation.XY,
                        TilemapOrientation.XZ => Tilemap.Orientation.XZ,
                        TilemapOrientation.YX => Tilemap.Orientation.YX,
                        TilemapOrientation.YZ => Tilemap.Orientation.YZ,
                        TilemapOrientation.ZX => Tilemap.Orientation.ZX,
                        TilemapOrientation.ZY => Tilemap.Orientation.ZY,
                        TilemapOrientation.Custom => Tilemap.Orientation.Custom,
                        _ => Tilemap.Orientation.XY
                    };
                    orientationApplied = true;
                }

                MarkDirtyAndRepaint(tilemap, tilemap.gameObject.scene);

                return new SetOrientationResponse
                {
                    gameObjectRef = new GameObjectRef(tilemap.gameObject),
                    tilemapRef = new ComponentRef(tilemap),
                    tileAnchor = tilemap.tileAnchor,
                    orientation = tilemap.orientation.ToString(),
                    anchorApplied = anchorApplied,
                    orientationApplied = orientationApplied,
                    success = true
                };
            });
        }

        public class SetOrientationResponse
        {
            [Description("Reference to the Tilemap GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Tilemap component.")]
            public ComponentRef? tilemapRef;

            [Description("Resulting tile anchor.")]
            public Vector3 tileAnchor;

            [Description("Resulting orientation.")]
            public string orientation = string.Empty;

            [Description("True when the anchor was changed.")]
            public bool anchorApplied;

            [Description("True when the orientation was changed.")]
            public bool orientationApplied;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
