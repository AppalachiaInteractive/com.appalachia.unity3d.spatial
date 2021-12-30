#region

using System;
using System.Collections.Generic;
using Appalachia.CI.Integration.Assets;
using Appalachia.Core.Attributes;
using Appalachia.Core.Debugging;
using Appalachia.Core.Objects.Root;
using Appalachia.Editing.Debugging.Handle;
using Appalachia.Spatial.Terrains.Utilities;
using Appalachia.Utility.Async;
using Appalachia.Utility.Strings;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

namespace Appalachia.Spatial.Terrains
{
    [ExecuteAlways]
    [RequireComponent(typeof(Terrain))]
    [CallStaticConstructorInEditor]
    public sealed class TerrainMetadataComponent : AppalachiaBehaviour<TerrainMetadataComponent>
    {
        // [CallStaticConstructorInEditor] should be added to the class (initsingletonattribute)
        static TerrainMetadataComponent()
        {
            TerrainMetadataManager.InstanceAvailable += i => _terrainMetadataManager = i;
        }

        #region Static Fields and Autoproperties

        private static TerrainMetadataManager _terrainMetadataManager;

        #endregion

        #region Fields and Autoproperties

        [FoldoutGroup("Gizmos")] public bool gizmosEnabled;
        [FoldoutGroup("Gizmos")] public bool selectedOnly;

        [FoldoutGroup("Gizmos")] public Color color;

        [FoldoutGroup("Gizmos")] public Color interpolated;

        [FoldoutGroup("Gizmos")] public float radius;

        [FoldoutGroup("Gizmos")] public float size;
        public Terrain terrain;

        [FormerlySerializedAs("terrainThreadsafeData")]
        public TerrainMetadata terrainMetadata;

        [FoldoutGroup("Gizmos")] public Vector3 center;

        #endregion

        public static List<TerrainMetadata> GetFromAllTerrains()
        {
            using (_PRF_GetFromAllTerrains.Auto())
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
                                ZString.Format("Missing {0}!", nameof(TerrainMetadataComponent))
                            );
                        }

                        var terrainThreadsafeData = comp.terrainMetadata;

                        if (terrainThreadsafeData == null)
                        {
                            throw new NotSupportedException(
                                ZString.Format("Missing {0}!", nameof(TerrainMetadata))
                            );
                        }

                        terrainThreadsafeData.InitializeTerrain(terrain, Allocator.Persistent);

                        comp.terrainMetadata = terrainThreadsafeData;

                        results.Add(terrainThreadsafeData);
                    }
                    catch (Exception ex)
                    {
                        StaticContext.Log.Error(ZString.Format("Failed to create terrain job data: {0}", ex));
                    }
                }

                return results;
            }
        }

        protected override async AppaTask WhenDisabled()

        {
            using (_PRF_OnDisable.Auto())
            {
                await base.WhenDisabled();

                if (terrain == null)
                {
                    terrain = GetComponent<Terrain>();
                }

                _terrainMetadataManager.Remove(terrain);
            }
        }

        protected override async AppaTask WhenEnabled()
        {
            using (_PRF_OnEnable.Auto())
            {
                await base.WhenEnabled();

                if (terrain == null)
                {
                    terrain = GetComponent<Terrain>();
                }

#if UNITY_EDITOR
                if (terrainMetadata == null)
                {
                    terrainMetadata = AddMetadataToTerrain(terrain);
                }
#endif
            }
        }

        #region Profiling

        private const string _PRF_PFX = nameof(TerrainMetadataComponent) + ".";

        private static readonly ProfilerMarker _PRF_GetFromAllTerrains =
            new ProfilerMarker(_PRF_PFX + nameof(GetFromAllTerrains));

        private static readonly ProfilerMarker
            _PRF_OnEnable = new ProfilerMarker(_PRF_PFX + nameof(OnEnable));

        private static readonly ProfilerMarker _PRF_OnDisable =
            new ProfilerMarker(_PRF_PFX + nameof(OnDisable));

        #endregion

#if UNITY_EDITOR
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

        private static readonly ProfilerMarker _PRF_DrawGizmos =
            new ProfilerMarker(_PRF_PFX + nameof(DrawGizmos));

        public void DrawGizmos()
        {
            using (_PRF_DrawGizmos.Auto())
            {
                var centerTS = center - terrainMetadata.terrainPosition;
                centerTS.x /= terrainMetadata.scale.x;
                centerTS.y /= terrainMetadata.scale.y;
                centerTS.z /= terrainMetadata.scale.z;

                var startIndexX = (int)centerTS.x;
                var startIndexY = (int)centerTS.z;

                startIndexX -= (int)((radius * .5) / terrainMetadata.scale.x);
                startIndexY -= (int)((radius * .5) / terrainMetadata.scale.z);

                var xSteps = (int)(radius / terrainMetadata.scale.x);
                var ySteps = (int)(radius / terrainMetadata.scale.z);

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
        }

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Tools.Base + "Terrains/Add TerrainMetadata To All")]
        public static void AddToAllTerrains_Menu()
        {
            AddToAllTerrains();
        }

        private static readonly ProfilerMarker _PRF_AddToAllTerrains =
            new ProfilerMarker(_PRF_PFX + nameof(AddToAllTerrains));

        [Button]
        public static List<TerrainMetadata> AddToAllTerrains()
        {
            using (_PRF_AddToAllTerrains.Auto())
            {
                var results = new List<TerrainMetadata>();

                var terrains = FindObjectsOfType<Terrain>();

                for (var i = 0; i < terrains.Length; i++)
                {
                    var t = terrains[i];

                    var terrainThreadsafeData = AddMetadataToTerrain(t);
                    results.Add(terrainThreadsafeData);
                }

                return results;
            }
        }

        private static TerrainMetadata AddMetadataToTerrain(Terrain t)
        {
            TerrainMetadata terrainThreadsafeData = null;

            try
            {
                var comp = t.GetComponent<TerrainMetadataComponent>();

                if (comp == null)
                {
                    comp = t.gameObject.AddComponent<TerrainMetadataComponent>();
                }

                AssetDatabaseManager.TryGetGUIDAndLocalFileIdentifier(t.terrainData, out var guid, out var _);

                var terrainName = ZString.Format("{0}_{1}", t.terrainData.name, guid);
                t.name = terrainName;

                terrainThreadsafeData = comp.terrainMetadata;

                if (terrainThreadsafeData == null)
                {
                    terrainThreadsafeData = TerrainMetadata.LoadOrCreateNew(terrainName);
                }

                terrainThreadsafeData.profileName = terrainName;
                terrainThreadsafeData.UpdateName();

                terrainThreadsafeData.InitializeTerrain(t, Allocator.Persistent);

                comp.terrainMetadata = terrainThreadsafeData;
            }
            catch (Exception ex)
            {
                StaticContext.Log.Error(ZString.Format("Failed to create terrain job data: {0}", ex));
            }

            return terrainThreadsafeData;
        }

#endif
    }
}
