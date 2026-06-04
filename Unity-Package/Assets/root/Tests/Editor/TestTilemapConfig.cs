/*
┌─────────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak/Unity-AI-Tilemap)       │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-AI-Tilemap)        │
└─────────────────────────────────────────────────────────────────────────────┘
*/

#nullable enable
using System.Collections;
using AIGD;
using com.IvanMurzak.Unity.MCP.Editor.API;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Tilemap.Editor.Tests
{
    public class TestTilemapConfig : BaseTest
    {
        [UnityTest]
        public IEnumerator CreateJson_Dispatch()
        {
            var json = @"{
                ""gridName"": ""DispatchedGrid"",
                ""tilemapName"": ""DispatchedTilemap""
            }";

            var result = RunToolAllowWarnings(Tool_Tilemap.CreateToolId, json);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsNotNull(GameObject.Find("DispatchedTilemap"), "The dispatched create should produce a Tilemap GameObject");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetOrientationJson_Dispatch()
        {
            var (_, tilemapGo, _) = CreateGridWithTilemap();

            var json = $@"{{
                ""gameObjectRef"": {{ ""instanceID"": {tilemapGo.GetInstanceID()} }},
                ""orientation"": ""XY""
            }}";

            var result = RunToolAllowWarnings(Tool_Tilemap.SetOrientationToolId, json);
            Assert.IsNotNull(result, "Result should not be null");

            yield return null;
        }
    }
}
