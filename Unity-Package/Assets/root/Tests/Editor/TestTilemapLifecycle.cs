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
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

namespace com.IvanMurzak.Unity.MCP.Tilemap.Editor.Tests
{
    public class TestTilemapLifecycle : BaseTest
    {
        const string TestTileAssetPath = "Assets/Temp_TilemapTests/TestTile.asset";

        static Tile CreateTileAssetOnDisk()
        {
            var dir = "Assets/Temp_TilemapTests";
            if (!AssetDatabase.IsValidFolder(dir))
                AssetDatabase.CreateFolder("Assets", "Temp_TilemapTests");
            var tile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(tile, TestTileAssetPath);
            AssetDatabase.SaveAssets();
            return tile;
        }

        [TearDown]
        public void Cleanup()
        {
            if (AssetDatabase.IsValidFolder("Assets/Temp_TilemapTests"))
                AssetDatabase.DeleteAsset("Assets/Temp_TilemapTests");
        }

        [UnityTest]
        public IEnumerator Create_BuildsGridAndTilemap()
        {
            var tool = new Tool_Tilemap();
            var result = tool.Create(gridName: GO_GridName, tilemapName: GO_TilemapName);

            Assert.IsTrue(result.success, "Create should succeed");
            Assert.IsNotNull(result.tilemapRef, "Tilemap reference should be returned");
            Assert.IsNotNull(result.rendererRef, "Renderer reference should be returned");
            Assert.AreEqual(GO_GridName, result.gridName, "Grid name should match");

            var tilemapGo = GameObject.Find(GO_TilemapName);
            Assert.IsNotNull(tilemapGo, "Tilemap GameObject should exist in the scene");
            Assert.IsNotNull(tilemapGo!.GetComponent<Tilemap>(), "Tilemap component should be present");
            Assert.IsNotNull(tilemapGo.GetComponent<TilemapRenderer>(), "TilemapRenderer component should be present");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetTile_And_GetTile_RoundTrip()
        {
            var (_, tilemapGo, _) = CreateGridWithTilemap();
            CreateTileAssetOnDisk();

            var tool = new Tool_Tilemap();
            var setResult = tool.SetTile(
                gameObjectRef: new GameObjectRef(tilemapGo.GetInstanceID()),
                x: 2, y: 3, z: 0,
                tileAssetPath: TestTileAssetPath);
            Assert.IsTrue(setResult.success, "SetTile should succeed");
            Assert.IsFalse(setResult.erased, "A tile was painted, not erased");

            var getResult = tool.GetTile(new GameObjectRef(tilemapGo.GetInstanceID()), x: 2, y: 3, z: 0);
            Assert.IsTrue(getResult.hasTile, "Cell (2,3) should now have a tile");
            Assert.AreEqual(TestTileAssetPath, getResult.tileAssetPath, "Tile asset path should round-trip");

            yield return null;
        }

        [UnityTest]
        public IEnumerator BoxFill_FillsRegion()
        {
            var (_, tilemapGo, tilemap) = CreateGridWithTilemap();
            CreateTileAssetOnDisk();

            var tool = new Tool_Tilemap();
            var result = tool.BoxFill(
                gameObjectRef: new GameObjectRef(tilemapGo.GetInstanceID()),
                tileAssetPath: TestTileAssetPath,
                minX: 0, minY: 0, maxX: 2, maxY: 1, z: 0);

            Assert.IsTrue(result.success, "BoxFill should succeed");
            Assert.AreEqual(6, result.filledCount, "3x2 region should report 6 cells");
            Assert.IsTrue(tilemap.HasTile(new Vector3Int(2, 1, 0)), "Corner cell should be filled");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Clear_RemovesAllTiles()
        {
            var (_, tilemapGo, tilemap) = CreateGridWithTilemap();
            CreateTileAssetOnDisk();

            var tool = new Tool_Tilemap();
            tool.BoxFill(new GameObjectRef(tilemapGo.GetInstanceID()), TestTileAssetPath, 0, 0, 3, 3, 0);
            Assert.Greater(tilemap.GetUsedTilesCount(), 0, "Tilemap should have tiles before clear");

            var clearResult = tool.Clear(new GameObjectRef(tilemapGo.GetInstanceID()), clearAll: true);
            Assert.IsTrue(clearResult.success, "Clear should succeed");
            Assert.AreEqual(0, tilemap.GetUsedTilesCount(), "Tilemap should be empty after ClearAll");

            yield return null;
        }

        [UnityTest]
        public IEnumerator List_FindsTilemap()
        {
            CreateGridWithTilemap(GO_GridName, GO_TilemapName);

            var tool = new Tool_Tilemap();
            var result = tool.List(includeInactive: true);

            Assert.GreaterOrEqual(result.count, 1, "At least one tilemap should be listed");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetOrientation_And_SetTileFlags()
        {
            var (_, tilemapGo, tilemap) = CreateGridWithTilemap();
            CreateTileAssetOnDisk();

            var tool = new Tool_Tilemap();

            var orientResult = tool.SetOrientation(
                new GameObjectRef(tilemapGo.GetInstanceID()),
                anchor: new Vector3(0f, 0f, 0f),
                orientation: Tool_Tilemap.TilemapOrientation.XY);
            Assert.IsTrue(orientResult.success, "SetOrientation should succeed");
            Assert.AreEqual(new Vector3(0f, 0f, 0f), tilemap.tileAnchor, "Anchor should be applied");

            tool.SetTile(new GameObjectRef(tilemapGo.GetInstanceID()), 0, 0, 0, TestTileAssetPath);
            var flagsResult = tool.SetTileFlags(
                new GameObjectRef(tilemapGo.GetInstanceID()),
                x: 0, y: 0, z: 0,
                color: Color.red);
            Assert.IsTrue(flagsResult.success, "SetTileFlags should succeed");
            Assert.IsTrue(flagsResult.colorApplied, "Color override should be applied");
            Assert.AreEqual(Color.red, tilemap.GetColor(new Vector3Int(0, 0, 0)), "Cell color should be red");

            yield return null;
        }

        [UnityTest]
        public IEnumerator CreateTileAsset_And_CreateRuleTile()
        {
            var tool = new Tool_Tilemap();

            var tileResult = tool.CreateTileAsset("Assets/Temp_TilemapTests/MadeTile.asset");
            Assert.IsTrue(tileResult.success, "CreateTileAsset should succeed");
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<Tile>(tileResult.assetPath), "Tile asset should exist on disk");

            var ruleResult = tool.CreateRuleTile("Assets/Temp_TilemapTests/MadeRuleTile.asset");
            Assert.IsTrue(ruleResult.success, "CreateRuleTile should succeed");
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<RuleTile>(ruleResult.assetPath), "RuleTile asset should exist on disk");

            yield return null;
        }
    }
}
