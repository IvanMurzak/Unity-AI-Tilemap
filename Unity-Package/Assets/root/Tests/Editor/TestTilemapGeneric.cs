/*
┌─────────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak/Unity-AI-Tilemap)       │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-AI-Tilemap)        │
└─────────────────────────────────────────────────────────────────────────────┘
*/

#nullable enable
using System;
using System.Collections;
using com.IvanMurzak.ReflectorNet.Model;
using AIGD;
using com.IvanMurzak.Unity.MCP.Editor.API;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

namespace com.IvanMurzak.Unity.MCP.Tilemap.Editor.Tests
{
    public class TestTilemapGeneric : BaseTest
    {
        [UnityTest]
        public IEnumerator Get_SerializesTilemapComponent()
        {
            var (_, tilemapGo, tilemap) = CreateGridWithTilemap();

            var tool = new Tool_Tilemap();
            var result = tool.GetComponentData(
                gameObjectRef: new GameObjectRef(tilemapGo.GetInstanceID()),
                componentRef: new ComponentRef(tilemap.GetInstanceID()));

            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsNotNull(result.data, "Serialized data should not be null");
            StringAssert.Contains("Tilemap", result.componentType, "Component type should be reported");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Get_FirstTilemapComponent_WhenNoComponentRef()
        {
            var (_, tilemapGo, _) = CreateGridWithTilemap();

            var tool = new Tool_Tilemap();
            var result = tool.GetComponentData(new GameObjectRef(tilemapGo.GetInstanceID()));

            Assert.IsNotNull(result.data, "Should serialize the first Tilemap-related component");

            yield return null;
        }

        // Drives the ReflectorNet **fields** channel: RuleTile.m_DefaultColliderType is a public
        // *field*, so the diff must be supplied through AddField (the 'fields' channel). ReflectorNet
        // resolves 'props' as PropertyInfo only and 'fields' as FieldInfo only — no cross-fallback.
        [UnityTest]
        public IEnumerator Modify_RuleTileDefaultColliderType_ViaFieldsChannel()
        {
            var reflector = UnityMcpPluginEditor.Instance.Reflector ?? throw new Exception("Reflector not available.");

            var ruleTile = ScriptableObject.CreateInstance<RuleTile>();
            ruleTile.m_DefaultColliderType = Tile.ColliderType.None;

            var diff = SerializedMember.FromValue(
                    reflector: reflector,
                    name: ruleTile.GetType().Name,
                    type: typeof(RuleTile),
                    value: null)
                .AddField(SerializedMember.FromValue(
                    reflector: reflector,
                    name: nameof(ruleTile.m_DefaultColliderType),
                    value: Tile.ColliderType.Grid));

            object? boxed = ruleTile;
            var ok = reflector.TryModify(ref boxed, diff);

            Assert.IsTrue(ok, "Field modification should succeed through the fields channel");
            Assert.AreEqual(Tile.ColliderType.Grid, ruleTile.m_DefaultColliderType,
                "m_DefaultColliderType (a public field) should be modified via the fields channel");

            UnityEngine.Object.DestroyImmediate(ruleTile);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Modify_GridCellSize_ViaModifyTool()
        {
            var (gridGo, _, _) = CreateGridWithTilemap();
            var grid = gridGo.GetComponent<Grid>();
            var reflector = UnityMcpPluginEditor.Instance.Reflector ?? throw new Exception("Reflector not available.");

            // cellSize is a property -> props channel.
            var diff = SerializedMember.FromValue(
                    reflector: reflector,
                    name: grid.GetType().Name,
                    type: typeof(Grid),
                    value: null)
                .AddProperty(SerializedMember.FromValue(
                    reflector: reflector,
                    name: nameof(grid.cellSize),
                    value: new Vector3(2f, 2f, 0f)));

            var tool = new Tool_Tilemap();
            var result = tool.ModifyComponent(
                gameObjectRef: new GameObjectRef(gridGo.GetInstanceID()),
                data: diff,
                componentRef: new ComponentRef(grid.GetInstanceID()));

            Assert.IsTrue(result.success, "Modification should succeed");
            Assert.AreEqual(new Vector3(2f, 2f, 0f), grid.cellSize, "cellSize should be modified via the props channel");

            yield return null;
        }
    }
}
