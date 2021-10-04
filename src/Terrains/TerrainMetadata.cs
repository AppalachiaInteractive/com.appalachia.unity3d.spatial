#region

using System;
using Appalachia.Base.Scriptables;
using Appalachia.Core.Collections.Native;
using Appalachia.Core.Extensions;
using Appalachia.Spatial.Terrains.Utilities;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Terrains
{
    [Serializable]
    public class TerrainMetadata : SelfNamingSavingAndIdentifyingScriptableObject<TerrainMetadata>
    {
        private const string _PRF_PFX = nameof(TerrainMetadata) + ".";

        [SerializeField] private TerrainData _terrainData;
        [SerializeField] private Allocator _allocator = Allocator.Persistent;
        
        [NonSerialized] private Terrain _terrain;
        
        [NonSerialized] private TerrainJobData jobData;
        [NonSerialized] private NativeArray<float> _heights;
        [NonSerialized] private float[,,] _alphamaps;
        [NonSerialized] private Vector3 _origin;
        [NonSerialized] private int _alphamapWidth;
        [NonSerialized] private int _alphamapHeight;
        [NonSerialized] private int _alphamapTextureCount;
        [NonSerialized] public int[] alphamapTextureFoleyIndex;

        private static readonly ProfilerMarker _PRF_OnEnable = new ProfilerMarker(_PRF_PFX + nameof(OnEnable));
        
        
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
                
                if (terrains == null || terrains.Length == 0)
                {
                    return;
                }
                
                if (_terrain == null)
                {
                    _terrain = terrains.First_NoAlloc(t => t.terrainData == _terrainData);
                }
            
                Initialize(_terrain, _allocator == Allocator.Invalid ? Allocator.Persistent : _allocator);
            }
        }

        private static readonly ProfilerMarker _PRF_Initialize = new ProfilerMarker(_PRF_PFX + nameof(Initialize));
        private static readonly ProfilerMarker _PRF_InitializeFoley = new ProfilerMarker(_PRF_PFX + nameof(InitializeFoley));
        
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

                if (terrain != null && _terrainData == null)
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
                else
                {
                    //Debug.LogWarning("Failed to initialize splat map");
                }
            }
        }
        
        public void InitializeFoley(string[] names)
        {
            using (_PRF_InitializeFoley.Auto())
            {
                if ((names == null) || (_terrainData == null))
                {
                    Debug.LogWarning("Failed to initialize foley map");
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

                        // Debug.Log("map: " + names[j] + " -- " + texture.name);
                        alphamapTextureFoleyIndex[i] = j;
                        found = true;
                        break;
                    }

                    if (!found)
                    {
                        Debug.LogWarningFormat("Failed to bind footstep foley to terrain texture '{0}'", texture.name);
                    }
                }
            }
        }
        
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

        private static readonly ProfilerMarker _PRF_CheckHeights = new ProfilerMarker(_PRF_PFX + nameof(CheckHeights));
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

        private static readonly ProfilerMarker _PRF_GetSplatIndexAtPosition = new ProfilerMarker(_PRF_PFX + nameof(GetSplatIndexAtPosition));
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

        private static readonly ProfilerMarker _PRF_GetFoleyIndexAtPosition = new ProfilerMarker(_PRF_PFX + nameof(GetFoleyIndexAtPosition));
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
