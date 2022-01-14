#region

using System;
using Appalachia.Core.Collections.Native;
using Appalachia.Core.Objects.Initialization;
using Appalachia.Core.Objects.Scriptables;
using Appalachia.Spatial.Terrains.Utilities;
using Appalachia.Utility.Async;
using Appalachia.Utility.Extensions;
using Appalachia.Utility.Strings;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Terrains
{
    [Serializable]
    public class TerrainMetadata : AutonamedIdentifiableAppalachiaObject<TerrainMetadata>
    {
        #region Fields and Autoproperties

        [NonSerialized] public int[] alphamapTextureFoleyIndex;
        [SerializeField] private Allocator _allocator = Allocator.Persistent;
        [NonSerialized] private float[,,] _alphamaps;
        [NonSerialized] private int _alphamapHeight;
        [NonSerialized] private int _alphamapTextureCount;
        [NonSerialized] private int _alphamapWidth;
        [NonSerialized] private NativeArray<float> _heights;

        [NonSerialized] private Terrain _terrain;

        [SerializeField] private TerrainData _terrainData;

        [NonSerialized] private TerrainJobData jobData;
        [NonSerialized] private Vector3 _origin;

        #endregion

        public int resolution => jobData.resolution;

        public NativeArray<float> heights
        {
            get
            {
                CheckHeights();

                return _heights;
            }
        }

        public TerrainData Data => _terrainData;

        public TerrainJobData JobData
        {
            get
            {
                CheckHeights();
                return jobData;
            }
        }

        public Vector3 scale => jobData.scale;

        public Vector3 terrainPosition => jobData.terrainPosition;

        public void CheckHeights()
        {
            using (_PRF_CheckHeights.Auto())
            {
                if (_heights.ShouldAllocate())
                {
                    _heights.SafeDispose();

                    _heights = TerrainJobHelper.LoadHeightData(_terrain, jobData.allocator);
                }
            }
        }

        public int GetFoleyIndexAtPosition(Vector3 position)
        {
            using (_PRF_GetFoleyIndexAtPosition.Auto())
            {
                if (alphamapTextureFoleyIndex == null)
                {
                    return -1;
                }

                var splatIndex = GetSplatIndexAtPosition(position);

                if ((splatIndex < 0) || (splatIndex >= alphamapTextureFoleyIndex.Length))
                {
                    return -1;
                }

                return alphamapTextureFoleyIndex[splatIndex];
            }
        }

        public int GetSplatIndexAtPosition(Vector3 position)
        {
            using (_PRF_GetSplatIndexAtPosition.Auto())
            {
                if ((_terrainData == null) || (_alphamaps == null))
                {
                    return -1;
                }

                var x = Mathf.FloorToInt(((position.x - _origin.x) / _terrainData.size.x) * _alphamapWidth);
                var z = Mathf.FloorToInt(((position.z - _origin.z) / _terrainData.size.z) * _alphamapHeight);
                var primarySplatIndex = 0;
                var maximumSplatWeight = 0f;

                for (int textureIndex = 0, n = _alphamapTextureCount; textureIndex < n; ++textureIndex)
                {
                    var splatWeight = _alphamaps[z, x, textureIndex];

                    if (maximumSplatWeight < splatWeight)
                    {
                        primarySplatIndex = textureIndex;
                        maximumSplatWeight = splatWeight;
                    }
                }

                return primarySplatIndex;
            }
        }

        public Terrain GetTerrain()
        {
            return _terrain;
        }

        public void InitializeFoley(string[] names)
        {
            using (_PRF_InitializeFoley.Auto())
            {
                if ((names == null) || (_terrainData == null))
                {
                    Context.Log.Warn("Failed to initialize foley map");
                    return;
                }

                alphamapTextureFoleyIndex = new int[_alphamapTextureCount];

                for (int i = 0, n = _alphamapTextureCount; i < n; ++i)
                {
                    var texture = _terrainData.terrainLayers[i].diffuseTexture;

                    var found = false;

                    for (int j = 0, m = names.Length; j < m; ++j)
                    {
                        if (string.IsNullOrEmpty(names[j]))
                        {
                            continue;
                        }

                        if (texture.name.IndexOf(names[j], StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            continue;
                        }

                        // Context.Log.Info("map: " + names[j] + " -- " + texture.name);
                        alphamapTextureFoleyIndex[i] = j;
                        found = true;
                        break;
                    }

                    if (!found)
                    {
                        Context.Log.Warn(
                            ZString.Format(
                                "Failed to bind footstep foley to terrain texture '{0}'",
                                texture.name
                            )
                        );
                    }
                }
            }
        }

        public void InitializeTerrain(Terrain terrain, Allocator allocator)
        {
            using (_PRF_InitializeTerrain.Auto())
            {
                _terrain = terrain;
                _allocator = allocator;

                if (_heights.IsCreated)
                {
                    _heights.Dispose();
                }

                _heights = TerrainJobHelper.LoadHeightData(terrain, allocator);

                jobData = new TerrainJobData(terrain, allocator);

                if ((terrain != null) && (_terrainData == null))
                {
                    _terrainData = terrain.terrainData;
                }

                if ((_alphamaps == null) && (_terrainData != null))
                {
                    _alphamapWidth = _terrainData.alphamapWidth;
                    _alphamapHeight = _terrainData.alphamapHeight;
                    _alphamaps = _terrainData.GetAlphamaps(0, 0, _alphamapWidth, _alphamapHeight);
                    _origin = terrain.transform.position;
                    _alphamapTextureCount = _alphamaps.Length / (_alphamapWidth * _alphamapHeight);
                }
            }
        }

        protected override async AppaTask Initialize(Initializer initializer)
        {
            await base.Initialize(initializer);

            using (_PRF_Initialize.Auto())
            {
                if (_terrainData == null)
                {
                    return;
                }

                var terrains = Terrain.activeTerrains;

                if ((terrains == null) || (terrains.Length == 0))
                {
                    return;
                }

                if (_terrain == null)
                {
                    _terrain = terrains.First_NoAlloc(t => t.terrainData == _terrainData);
                }

                InitializeTerrain(
                    _terrain,
                    _allocator == Allocator.Invalid ? Allocator.Persistent : _allocator
                );
            }
        }

        #region Profiling


        private static readonly ProfilerMarker _PRF_InitializeTerrain =
            new ProfilerMarker(_PRF_PFX + nameof(InitializeTerrain));

        private static readonly ProfilerMarker _PRF_OnEnable = new(_PRF_PFX + nameof(OnEnable));
        private static readonly ProfilerMarker _PRF_InitializeFoley = new(_PRF_PFX + nameof(InitializeFoley));
        private static readonly ProfilerMarker _PRF_CheckHeights = new(_PRF_PFX + nameof(CheckHeights));

        private static readonly ProfilerMarker _PRF_GetSplatIndexAtPosition =
            new(_PRF_PFX + nameof(GetSplatIndexAtPosition));

        private static readonly ProfilerMarker _PRF_GetFoleyIndexAtPosition =
            new(_PRF_PFX + nameof(GetFoleyIndexAtPosition));

        #endregion
    }
}
