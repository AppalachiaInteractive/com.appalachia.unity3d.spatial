#if UNITY_EDITOR
using Appalachia.Core.Extensions;
using Appalachia.Spatial.Visualizers;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace Appalachia.Spatial.Terrains
{
    public sealed class
        TerrainTestVisualizer : InstancedIndirectHeightmapMapVisualization<TerrainTestVisualizer>
    {
        #region Fields and Autoproperties

        [OnValueChanged(nameof(SetupTerrainTexture))]
        public Terrain terrain;

        #endregion

        /// <inheritdoc />
        protected override bool CanGenerate => terrain != null;

        /// <inheritdoc />
        protected override bool ShouldRegenerate => false;

        /// <inheritdoc />
        protected override void GetVisualizationInfo(
            Vector3 position,
            out Quaternion rotation,
            out Vector3 scale)
        {
            rotation = quaternion.identity;
            scale = Vector3.one * visualizationSize;
        }

        /// <inheritdoc />
        protected override void PrepareInitialGeneration()
        {
            if (terrain == null)
            {
                terrain = Terrain.activeTerrain;
            }
        }

        /// <inheritdoc />
        protected override void PrepareSubsequentGenerations()
        {
            if (terrain == null)
            {
                terrain = Terrain.activeTerrain;
            }
        }

        private void SetupTerrainTexture()
        {
            if (terrain == null)
            {
                terrain = Terrain.activeTerrain;
            }

            texture = terrain.terrainData.heightmapTexture.ToTexture2D();
            Transform.position = terrain.transform.position;
        }
    }
}

#endif
