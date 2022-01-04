#region

using System.Collections.Generic;
using Appalachia.Core.Attributes;
using Appalachia.Core.Attributes.Editing;
using Appalachia.Core.Collections.Native;
using Appalachia.Core.Objects.Initialization;
using Appalachia.Core.Objects.Root;
using Appalachia.Spatial.Terrains.Utilities;
using Appalachia.Utility.Async;
using Appalachia.Utility.Execution;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Terrains
{
    [CallStaticConstructorInEditor]
    [SmartLabelChildren]
    public class TerrainMetadataManager : SingletonAppalachiaBehaviour<TerrainMetadataManager>
    {
        static TerrainMetadataManager()
        {
            RegisterDependency<MainTerrainMetadataDictionary>(i => _mainTerrainMetadataDictionary = i);
        }

        #region Static Fields and Autoproperties

        [InlineEditor, ShowInInspector]
        private static MainTerrainMetadataDictionary _mainTerrainMetadataDictionary;

        #endregion

        #region Fields and Autoproperties

        private NativeHashMap<int, TerrainJobData> _nativeData;
        private NativeKeyArray2D<int, float> _nativeHeights;

        #endregion

        public NativeKeyArray2D<int, float> GetNativeHeights()
        {
            using (_PRF_GetNativeHeights.Auto())
            {
                if (_nativeHeights.ShouldAllocate())
                {
                    _nativeHeights = TerrainJobHelper.InitializeJobHeights(Terrain.activeTerrains);
                }

                return _nativeHeights;
            }
        }

        public NativeHashMap<int, TerrainJobData> GetNativeMetadata()
        {
            using (_PRF_GetNativeMetadata.Auto())
            {
                if (_nativeData.ShouldAllocate())
                {
                    _nativeData = new NativeHashMap<int, TerrainJobData>(4, Allocator.Persistent);
                }

                _nativeData.Clear();

                for (var i = 0; i < _mainTerrainMetadataDictionary.Lookup.Count; i++)
                {
                    var key = _mainTerrainMetadataDictionary.Lookup.Items.GetKeyByIndex(i);
                    var adj = _mainTerrainMetadataDictionary.Lookup[key];

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

        public TerrainMetadata GetTerrain(int hashCode)
        {
            using (_PRF_GetTerrain.Auto())
            {
                var data = _mainTerrainMetadataDictionary.Lookup.Items.Get(hashCode);

                return data;
            }
        }

        public TerrainMetadata GetTerrain(Terrain t)
        {
            using (_PRF_GetTerrain.Auto())
            {
                return _mainTerrainMetadataDictionary.Lookup.Items.Get(t.GetHashCode());
            }
        }

        public TerrainMetadata GetTerrainAt(Vector3 position)
        {
            using (_PRF_GetTerrainAt.Auto())
            {
                var terrain = Terrain.activeTerrains.GetTerrainAtPosition(position);

                return _mainTerrainMetadataDictionary.Lookup.Items.Get(terrain.GetHashCode());
            }
        }

        public int GetTerrainHashCodeAt(Vector3 position)
        {
            using (_PRF_GetTerrainHashCodeAt.Auto())
            {
                var terrain = Terrain.activeTerrains.GetTerrainAtPosition(position);
                return terrain.GetHashCode();
            }
        }

        public void Remove(Terrain t)
        {
            using (_PRF_Remove.Auto())
            {
                var hashCode = t.terrainData.GetHashCode();

                var x = _mainTerrainMetadataDictionary.Lookup.RemoveByKey(hashCode);

                x?.heights.SafeDispose();
            }
        }

        protected override async AppaTask Initialize(Initializer initializer)
        {
            using (_PRF_Initialize.Auto())
            {
                await base.Initialize(initializer);

                _mainTerrainMetadataDictionary.Lookup.Clear();

                List<TerrainMetadata> results;
#if UNITY_EDITOR
                if (AppalachiaApplication.IsPlaying)
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

                    if (!_mainTerrainMetadataDictionary.Lookup.Items.ContainsKey(hashCode))
                    {
                        _mainTerrainMetadataDictionary.Lookup.Items.AddOrUpdate(hashCode, threadsafeData);
                    }
                }
            }
        }

        protected override async AppaTask WhenDisabled()

        {
            using (_PRF_WhenDisabled.Auto())
            {
                await base.WhenDisabled();

                for (var i = 0; i < _mainTerrainMetadataDictionary.Lookup.Count; i++)
                {
                    var data = _mainTerrainMetadataDictionary.Lookup.Items.GetByIndex(i);

                    if (data.heights.IsCreated)
                    {
                        data.heights.SafeDispose();
                    }
                }

                _nativeData.SafeDispose();
                _nativeHeights.SafeDispose();
            }
        }

        #region Profiling

        private const string _PRF_PFX = nameof(TerrainMetadataManager) + ".";

        private static readonly ProfilerMarker _PRF_Initialize =
            new ProfilerMarker(_PRF_PFX + nameof(Initialize));

        private static readonly ProfilerMarker _PRF_WhenDisabled =
            new ProfilerMarker(_PRF_PFX + nameof(OnDisable));

        private static readonly ProfilerMarker _PRF_TerrainMetadataManager =
            new(_PRF_PFX + nameof(TerrainMetadataManager));

        private static readonly ProfilerMarker _PRF_GetTerrain = new(_PRF_PFX + nameof(GetTerrain));

        private static readonly ProfilerMarker _PRF_GetNativeMetadata =
            new(_PRF_PFX + nameof(GetNativeMetadata));

        private static readonly ProfilerMarker _PRF_GetNativeHeights =
            new(_PRF_PFX + nameof(GetNativeHeights));

        private static readonly ProfilerMarker _PRF_GetTerrainHashCodeAt =
            new(_PRF_PFX + nameof(GetTerrainHashCodeAt));

        private static readonly ProfilerMarker _PRF_GetTerrainAt = new(_PRF_PFX + nameof(GetTerrainAt));

        private static readonly ProfilerMarker _PRF_Remove = new(_PRF_PFX + nameof(Remove));

        #endregion
    }
}
