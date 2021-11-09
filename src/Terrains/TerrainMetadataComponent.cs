#region

using System;
using System.Collections.Generic;
using Appalachia.CI.Integration.Assets;
using Appalachia.Core.Debugging;
using Appalachia.Editing.Debugging;
using Appalachia.Editing.Debugging.Handle;
using Appalachia.Spatial.Terrains.Utilities;
using Appalachia.Utility.Logging;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

namespace Appalachia.Spatial.Terrains
{
    [ExecuteAlways]
    [RequireComponent(typeof(Terrain))]
    public class TerrainMetadataComponent : MonoBehaviour
    {
        public Terrain terrain;

        [FormerlySerializedAs("terrainThreadsafeData")]
        public TerrainMetadata terrainMetadata;

        [FoldoutGroup("Gizmos")] public bool gizmosEnabled;
        [FoldoutGroup("Gizmos")] public bool selectedOnly;

        [FoldoutGroup("Gizmos")] public Vector3 center;

        [FoldoutGroup("Gizmos")] public float radius;

        [FoldoutGroup("Gizmos")] public float size;

        [FoldoutGroup("Gizmos")] public Color color;

        [FoldoutGroup("Gizmos")] public Color interpolated;

        private void OnEnable()
        {
            if (terrain == null)
            {
                terrain = GetComponent<Terrain>();
            }

            if (terrainMetadata == null)
            {
                TerrainMetadataManager.Initialize();
            }
        }

        private void OnDisable()
        {
            if (terrain == null)
            {
                terrain = GetComponent<Terrain>();
            }

            TerrainMetadataManager.Remove(terrain);
        }

        public void OnDrawGizmos()
        {
            if (!gizmosEnabled || selectedOnly)
            {
                return;
            }

            if (!GizmoCameraChecker.ShouldRenderGizmos())
            {
                return;
            }

            DrawGizmos();
        }

        public void OnDrawGizmosSelected()
        {
            if (!gizmosEnabled || !selectedOnly)
            {
                return;
            }

            if (!GizmoCameraChecker.ShouldRenderGizmos())
            {
                return;
            }

            DrawGizmos();
        }

        public void SetGizmoParameters(
            Vector3 center,
            float radius,
            float size,
            Color color,
            Color interpolated)
        {
            this.center = center;
            this.radius = radius;
            this.size = size;
            this.color = color;
            this.interpolated = interpolated;
        }

        public void DrawGizmos()
        {
            var centerTS = center - terrainMetadata.terrainPosition;
            centerTS.x /= terrainMetadata.scale.x;
            centerTS.y /= terrainMetadata.scale.y;
            centerTS.z /= terrainMetadata.scale.z;

            var startIndexX = (int) centerTS.x;
            var startIndexY = (int) centerTS.z;

            startIndexX -= (int) ((radius * .5) / terrainMetadata.scale.x);
            startIndexY -= (int) ((radius * .5) / terrainMetadata.scale.z);

            var xSteps = (int) (radius / terrainMetadata.scale.x);
            var ySteps = (int) (radius / terrainMetadata.scale.z);

            for (var xIndex = startIndexX; xIndex < (startIndexX + xSteps); xIndex++)
            {
                for (var yIndex = startIndexY; yIndex < (startIndexY + ySteps); yIndex++)
                {
                    var index = (yIndex * terrainMetadata.resolution) + xIndex;

                    var height = terrainMetadata.heights[index];
                    var scaledHeight = height * terrainMetadata.scale.y;

                    var realworldHeight = terrainMetadata.terrainPosition.y + scaledHeight;

                    var realworldX = terrainMetadata.terrainPosition.x +
                                     (xIndex * terrainMetadata.scale.x);
                    var realworldZ = terrainMetadata.terrainPosition.z +
                                     (yIndex * terrainMetadata.scale.z);

                    var pos = new Vector3(realworldX, realworldHeight, realworldZ);

                    SmartHandles.DrawWireDisc(pos, Vector3.up, size, color);

                    var interpolatedHeight = terrainMetadata.GetWorldSpaceHeight(pos);

                    pos.y = interpolatedHeight;

                    SmartHandles.DrawWireDisc(pos, Vector3.up, size * .8f, interpolated);
                }
            }
        }

        public static List<TerrainMetadata> GetFromAllTerrains()
        {
            var results = new List<TerrainMetadata>();

            var terrains = FindObjectsOfType<Terrain>();

            for (var i = 0; i < terrains.Length; i++)
            {
                try
                {
                    var terrain = terrains[i];

                    var comp = terrain.GetComponent<TerrainMetadataComponent>();

                    if (comp == null)
                    {
                        throw new NotSupportedException(
                            $"Missing {nameof(TerrainMetadataComponent)}!"
                        );
                    }

                    var terrainThreadsafeData = comp.terrainMetadata;

                    if (terrainThreadsafeData == null)
                    {
                        throw new NotSupportedException($"Missing {nameof(TerrainMetadata)}!");
                    }

                    terrainThreadsafeData.Initialize(terrain, Allocator.Persistent);

                    comp.terrainMetadata = terrainThreadsafeData;

                    results.Add(terrainThreadsafeData);
                }
                catch (Exception ex)
                {
                    AppaLog.Error($"Failed to create terrain job data: {ex}");
                }
            }

            return results;
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Tools.Base + "Terrains/Add TerrainMetadata To All")]
        public static void AddToAllTerrains_Menu()
        {
            AddToAllTerrains();
        }

        [Button]
        public static List<TerrainMetadata> AddToAllTerrains()
        {
            var results = new List<TerrainMetadata>();

            var terrains = FindObjectsOfType<Terrain>();

            for (var i = 0; i < terrains.Length; i++)
            {
                try
                {
                    var terrain = terrains[i];

                    var comp = terrain.GetComponent<TerrainMetadataComponent>();

                    if (comp == null)
                    {
                        comp = terrain.gameObject.AddComponent<TerrainMetadataComponent>();
                    }

                    AssetDatabaseManager.TryGetGUIDAndLocalFileIdentifier(
                        terrain.terrainData,
                        out var guid,
                        out long _
                    );

                    var terrainName = $"{terrain.terrainData.name}_{guid}";
                    terrain.name = terrainName;

                    var terrainThreadsafeData = comp.terrainMetadata;

                    if (terrainThreadsafeData == null)
                    {
                        terrainThreadsafeData = TerrainMetadata.LoadOrCreateNew(terrainName);
                    }

                    terrainThreadsafeData.profileName = terrainName;
                    terrainThreadsafeData.UpdateName();

                    terrainThreadsafeData.Initialize(terrain, Allocator.Persistent);

                    comp.terrainMetadata = terrainThreadsafeData;

                    results.Add(terrainThreadsafeData);
                }
                catch (Exception ex)
                {
                    AppaLog.Error($"Failed to create terrain job data: {ex}");
                }
            }

            return results;
        }
#endif
    }
}
