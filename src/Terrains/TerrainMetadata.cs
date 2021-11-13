#region

using System;
using Appalachia.Core.Collections.Native;
using Appalachia.Core.Extensions;
using Appalachia.Core.Scriptables;
using Appalachia.Spatial.Terrains.Utilities;
using Appalachia.Utility.Extensions;
using Appalachia.Utility.Logging;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Terrains
{
    [Serializable]
    public class TerrainMetadata : AutonamedIdentifiableAppalachiaObject<TerrainMetadata>
    {
        private const string _PRF_PFX = nameof(TerrainMetadata) + ".";

        private static readonly ProfilerMarker _PRF_OnEnable = new(_PRF_PFX + nameof(OnEnable));

        private static readonly ProfilerMarker _PRF_Initialize = new(_PRF_PFX + nameof(Initialize));

        private static readonly ProfilerMarker _PRF_InitializeFoley =
            new(_PRF_PFX + nameof(InitializeFoley));

        private static readonly ProfilerMarker _PRF_CheckHeights =
            new(_PRF_PFX + nameof(CheckHeights));

        private static readonly ProfilerMarker _PRF_GetSplatIndexAtPosition =
            new(_PRF_PFX + nameof(GetSplatIndexAtPosition));

        private static readonly ProfilerMarker _PRF_GetFoleyIndexAtPosition =
            new(_PRF_PFX + nameof(GetFoleyIndexAtPosition));

        [SerializeField] private TerrainData _terrainData;
        [SerializeField] private Allocator _allocator = Allocator.Persistent;
        [NonSerialized] private int _alphamapHeight;
        [NonSerialized] private float[,,] _alphamaps;
        [NonSerialized] private int _alphamapTextureCount;
        [NonSerialized] private int _alphamapWidth;
        [NonSerialized] private NativeArray<float> _heights;
        [NonSerialized] private Vector3 _origin;

        [NonSerialized] private Terrain _terrain;
        [NonSerialized] public int[] alphamapTextureFoleyIndex;

        [NonSerialized] private TerrainJobData jobData;

        public Vector3 terrainPosition => jobData.terrainPosition;
        public int resolution => jobData.resolution;
        public Vector3 scale => jobData.scale;

        public NativeArray<float> heights
        {
            get
            {
                CheckHeights();

                return _heights;
            }
        }

        public TerrainJobData JobData
        {
            get
            {
                CheckHeights();
                return jobData;
            }
        }

        protected override void OnEnable()
        {
            using (_PRF_OnEnable.Auto())
            {
                base.OnEnable();

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

                Initialize(
                    _terrain,
                    _allocator == Allocator.Invalid ? Allocator.Persistent : _allocator
                );
            }
        }

        public void Initialize(Terrain terrain, Allocator allocator)
        {
            using (_PRF_Initialize.Auto())
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

        public void InitializeFoley(string[] names)
        {
            using (_PRF_InitializeFoley.Auto())
            {
                if ((names == null) || (_terrainData == null))
                {
                   AppaLog.Warn("Failed to initialize foley map");
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

                        // AppaLog.Info("map: " + names[j] + " -- " + texture.name);
                        alphamapTextureFoleyIndex[i] = j;
                        found = true;
                        break;
                    }

                    if (!found)
                    {
                        AppaLog.Warn($"Failed to bind footstep foley to terrain texture '{texture.name}'");
                    }
                }
            }
        }

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

        public Terrain GetTerrain()
        {
            return _terrain;
        }

        public int GetSplatIndexAtPosition(Vector3 position)
        {
            using (_PRF_GetSplatIndexAtPosition.Auto())
            {
                if ((_terrainData == null) || (_alphamaps == null))
                {
                    return -1;
                }

                var x = Mathf.FloorToInt(
                    ((position.x - _origin.x) / _terrainData.size.x) * _alphamapWidth
                );
                var z = Mathf.FloorToInt(
                    ((position.z - _origin.z) / _terrainData.size.z) * _alphamapHeight
                );
                var primarySplatIndex = 0;
                var maximumSplatWeight = 0f;

                for (int textureIndex = 0, n = _alphamapTextureCount;
                    textureIndex < n;
                    ++textureIndex)
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
    }
}
