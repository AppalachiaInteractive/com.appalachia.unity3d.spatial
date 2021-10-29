#region

using System.Collections.Generic;
using Appalachia.Core.Attributes;
using Appalachia.Core.Collections.Native;
using Appalachia.Spatial.Terrains.Utilities;
using Unity.Collections;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Terrains
{
    [AlwaysInitializeOnLoad]
    
    public static class TerrainMetadataManager
    {
        private const string _PRF_PFX = nameof(TerrainMetadataManager) + ".";
        private static bool _initialized;

        private static NativeHashMap<int, TerrainJobData> _nativeData;
        private static NativeKeyArray2D<int, float> _nativeHeights;

        private static readonly ProfilerMarker _PRF_TerrainMetadataManager =
            new(_PRF_PFX + nameof(TerrainMetadataManager));

        private static readonly ProfilerMarker _PRF_Initialize = new(_PRF_PFX + nameof(Initialize));

        private static readonly ProfilerMarker _PRF_ReInitialize =
            new(_PRF_PFX + nameof(ReInitialize));

        private static readonly ProfilerMarker _PRF_GetTerrain = new(_PRF_PFX + nameof(GetTerrain));

        private static readonly ProfilerMarker _PRF_GetNativeMetadata =
            new(_PRF_PFX + nameof(GetNativeMetadata));

        private static readonly ProfilerMarker _PRF_GetNativeHeights =
            new(_PRF_PFX + nameof(GetNativeHeights));

        private static readonly ProfilerMarker _PRF_GetTerrainHashCodeAt =
            new(_PRF_PFX + nameof(GetTerrainHashCodeAt));

        private static readonly ProfilerMarker _PRF_GetTerrainAt =
            new(_PRF_PFX + nameof(GetTerrainAt));

        private static readonly ProfilerMarker _PRF_Remove = new(_PRF_PFX + nameof(Remove));

        private static readonly ProfilerMarker _PRF_DisposeNativeCollections =
            new(_PRF_PFX + nameof(DisposeNativeCollections));

        static TerrainMetadataManager()
        {
            using (_PRF_TerrainMetadataManager.Auto())
            {
                EditorApplication.delayCall += Initialize;
            }
        }

        public static bool Initialized => _initialized;

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            using (_PRF_Initialize.Auto())
            {
                var terrainLookup = TerrainMetadataDictionary.instance;
                terrainLookup.Lookup.Clear();

                List<TerrainMetadata> results;
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    results = TerrainMetadataComponent.GetFromAllTerrains();
                }
                else
                {
                    results = TerrainMetadataComponent.AddToAllTerrains();
                }

#else
                results = TerrainMetadataComponent.GetFromAllTerrains();
#endif

                for (var i = 0; i < results.Count; i++)
                {
                    var threadsafeData = results[i];
                    var terrain = threadsafeData.GetTerrain();

                    var hashCode = terrain.GetHashCode();

                    if (!terrainLookup.Lookup.ContainsKey(hashCode))
                    {
                        terrainLookup.Lookup.AddOrUpdate(hashCode, threadsafeData);
                    }
                }
            }
        }

        public static void ReInitialize()
        {
            using (_PRF_ReInitialize.Auto())
            {
                _initialized = false;

                Initialize();
            }
        }

        public static TerrainMetadata GetTerrain(int hashCode)
        {
            using (_PRF_GetTerrain.Auto())
            {
                var terrainLookup = TerrainMetadataDictionary.instance;
                var data = terrainLookup.Lookup.Get(hashCode);

                return data;
            }
        }

        public static TerrainMetadata GetTerrain(Terrain t)
        {
            using (_PRF_GetTerrain.Auto())
            {
                var terrainLookup = TerrainMetadataDictionary.instance;
                return terrainLookup.Lookup.Get(t.GetHashCode());
            }
        }

        public static NativeHashMap<int, TerrainJobData> GetNativeMetadata()
        {
            using (_PRF_GetNativeMetadata.Auto())
            {
                if (_nativeData.ShouldAllocate())
                {
                    _nativeData = new NativeHashMap<int, TerrainJobData>(4, Allocator.Persistent);
                }

                _nativeData.Clear();

                var terrainLookup = TerrainMetadataDictionary.instance;

                for (var i = 0; i < terrainLookup.Lookup.Count; i++)
                {
                    var key = terrainLookup.Lookup.GetKeyByIndex(i);
                    var adj = terrainLookup.Lookup[key];

                    if (!_nativeData.ContainsKey(key))
                    {
                        _nativeData.Add(key, adj.JobData);
                    }
                    else
                    {
                        _nativeData[key] = adj.JobData;
                    }
                }

                return _nativeData;
            }
        }

        [ExecuteOnDisable]
        private static void OnDisable()
        {
            _nativeData.SafeDispose();
        }

        public static NativeKeyArray2D<int, float> GetNativeHeights()
        {
            using (_PRF_GetNativeHeights.Auto())
            {
                if (_nativeHeights.ShouldAllocate())
                {
                    _nativeHeights = TerrainJobHelper.InitializeJobHeights(Terrain.activeTerrains);
                }
                /*else
                {
                    TerrainJobHelper.LoadHeightData(Terrain.activeTerrains, _nativeHeights);
                }
                */

                return _nativeHeights;
            }
        }

        public static int GetTerrainHashCodeAt(Vector3 position)
        {
            using (_PRF_GetTerrainHashCodeAt.Auto())
            {
                var terrain = Terrain.activeTerrains.GetTerrainAtPosition(position);
                return terrain.GetHashCode();
            }
        }

        public static TerrainMetadata GetTerrainAt(Vector3 position)
        {
            using (_PRF_GetTerrainAt.Auto())
            {
                var terrainLookup = TerrainMetadataDictionary.instance;
                var terrain = Terrain.activeTerrains.GetTerrainAtPosition(position);

                return terrainLookup.Lookup.Get(terrain.GetHashCode());
            }
        }

        public static void Remove(Terrain t)
        {
            using (_PRF_Remove.Auto())
            {
                var hashCode = t.GetHashCode();

                var terrainLookup = TerrainMetadataDictionary.instance;

                var x = terrainLookup.Lookup.RemoveByKey(hashCode);

                x?.heights.SafeDispose();
            }
        }

        [ExecuteOnDisable]
        public static void DisposeNativeCollections()
        {
            using (_PRF_DisposeNativeCollections.Auto())
            {
                //Debug.Log("Disposing native collections.");

                var terrainLookup = TerrainMetadataDictionary.instance;

                for (var i = 0; i < terrainLookup.Lookup.Count; i++)
                {
                    var data = terrainLookup.Lookup.GetByIndex(i);

                    if (data.heights.IsCreated)
                    {
                        data.heights.SafeDispose();
                    }
                }

                _nativeData.SafeDispose();
            }
        }
    }
}
